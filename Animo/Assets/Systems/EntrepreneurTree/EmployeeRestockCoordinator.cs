using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Connects hired restockers to real in-store replenishment (packages -> placements).
    /// </summary>
    public class EmployeeRestockCoordinator : MonoBehaviour
    {
        private const string LogPrefix = "[Restock] ";

        public enum RestockTaskState
        {
            Idle,
            GoingToStorage,
            PickingStock,
            GoingToShelf,
            PlacingProduct,
            ReturningOrIdle
        }

        [Header("Cycle")]
        [Min(1f)] public float baseCycleSeconds = 12f;
        [Min(0.5f)] public float minCycleSeconds = 1.5f;
        [Range(0.1f, 0.95f)] public float lowStockThreshold = 0.40f;
        [Min(1)] public int maxTasksPerCycle = 6;
        public bool notifyWhenMissingSource = false;
        [Min(1f)] public float navigationTimeoutSeconds = 20f;
        [Min(1f)] public float navigationTimeoutMarginSeconds = 8f;
        [Min(1f)] public float navigationTimeoutMultiplier = 1.75f;
        [Min(0.05f)] public float pickingSeconds = 0.35f;
        [Min(0.05f)] public float placingSeconds = 0.35f;
        [Min(0.1f)] public float arrivalTolerance = 0.8f;
        [Min(1f)] public float navMeshSampleRadius = 8f;

        private Coroutine cycleRoutine;
        private Coroutine visualTaskRoutine;
        private float lastMissingSourceWarnTime;

        public RestockTaskState CurrentTaskState { get; private set; } = RestockTaskState.Idle;
        public bool IsVisualTaskInProgress => visualTaskRoutine != null;
        public bool LastTaskCompletedVisualRoute { get; private set; }

        void Awake()
        {
            DayCycleSystem.onDayStarted += OnDayStarted;
            DayCycleSystem.onDayOver += OnDayOver;
            SaveGameSystem.dataLoadEvent += OnDataLoaded;
        }


        void Start()
        {
            if (DayCycleSystem.GetStoreOpenState() == StoreOpenState.Open)
                StartCycle();
        }


        private void OnDayStarted()
        {
            StartCycle();
        }


        private void OnDayOver()
        {
            StopCycle();
        }


        private void OnDataLoaded()
        {
            StopCycle();
            if (DayCycleSystem.GetStoreOpenState() == StoreOpenState.Open)
                StartCycle();
        }


        private void StartCycle()
        {
            if (cycleRoutine != null)
                return;

            cycleRoutine = StartCoroutine(RestockRoutine());
        }


        private void StopCycle()
        {
            if (cycleRoutine == null)
                return;

            StopCoroutine(cycleRoutine);
            cycleRoutine = null;
        }


        private IEnumerator RestockRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(GetCurrentCycleDelay());

                if (DayCycleSystem.GetStoreOpenState() != StoreOpenState.Open)
                    continue;
                if (EntrepreneurEmployeeSystem.Instance == null)
                    continue;

                int restockers = EntrepreneurEmployeeSystem.Instance.GetRoleCount(EmployeeRole.Restocker);
                if (restockers <= 0)
                    continue;

                int tasks = Mathf.Clamp(restockers, 1, maxTasksPerCycle);
                for (int i = 0; i < tasks; i++)
                    if (TryStartVisualTask())
                        break;
            }
        }


        private void ExecuteSingleTask()
        {
            PackageObject sourcePackage;
            ProductScriptableObject sourceProduct;
            PlacementObject target = FindRestockTarget(out sourcePackage, out sourceProduct);
            if (target == null || sourcePackage == null || sourceProduct == null)
            {
                MaybeNotifyMissingSource();
                return;
            }

            if (sourcePackage.count <= 0)
                return;

            if (target.container == null)
            {
                Debug.LogWarning(LogPrefix + "Restock target '" + target.name + "' has no container assigned.");
                return;
            }
            if (!target.IsPlaceable(sourceProduct))
            {
                if (UIGame.Instance != null)
                    UIGame.AddNotification("No hay espacio valido para " + sourceProduct.title + " en " + target.name + ".", otherColor: new Color(1f, 0.65f, 0.18f));
                Debug.LogWarning(LogPrefix + "Restock target rejected '" + sourceProduct.title + "' for '" + target.name + "'.");
                return;
            }

            Transform item = sourcePackage.Remove();
            if (item == null)
                item = Instantiate(sourceProduct.prefab).transform;

            Vector3 targetPosition = target.Add(sourceProduct);
            Quaternion targetRotation = Quaternion.Euler(0, target.orientation, 0);
            InteractionSystem.MoveToTargetArc(item, target.container, targetPosition, targetRotation, GetPlacementLerpSpeed());
        }


        private bool TryStartVisualTask()
        {
            if (visualTaskRoutine != null)
                return false;

            PackageObject sourcePackage;
            ProductScriptableObject sourceProduct;
            PlacementObject target = FindRestockTarget(out sourcePackage, out sourceProduct);
            if (target == null || sourcePackage == null || sourceProduct == null)
            {
                MaybeNotifyMissingSource();
                return false;
            }

            int employeeId = FindAvailableRestockerEmployeeId();
            GameObject npc = employeeId > 0 ? EmployeeNPCSpawner.Instance?.GetNPC(employeeId) : null;
            NavMeshAgent agent = npc != null ? npc.GetComponent<NavMeshAgent>() : null;
            if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            {
                Debug.LogWarning(LogPrefix + "Visual route unavailable; using documented emergency logical placement.");
                ExecuteSingleTask();
                return false;
            }

            LastTaskCompletedVisualRoute = false;
            visualTaskRoutine = StartCoroutine(RunVisualTask(employeeId, npc, agent, sourcePackage, sourceProduct, target));
            return true;
        }


        private IEnumerator RunVisualTask(int employeeId, GameObject npc, NavMeshAgent agent,
                                          PackageObject sourcePackage, ProductScriptableObject sourceProduct,
                                          PlacementObject target)
        {
            Transform carriedItem = null;
            try
            {
                SetTaskState(RestockTaskState.GoingToStorage, employeeId, sourceProduct, target);
                if (!TrySetDestination(agent, sourcePackage.transform.position))
                    yield break;
                yield return WaitForArrival(agent, GetNavigationTimeout(agent));
                if (!HasArrived(agent))
                    yield break;

                SetTaskState(RestockTaskState.PickingStock, employeeId, sourceProduct, target);
                yield return new WaitForSeconds(pickingSeconds);
                carriedItem = Instantiate(sourceProduct.prefab).transform;
                carriedItem.SetParent(npc.transform, true);

                Vector3 shelfPosition = target.container != null
                    ? target.container.position
                    : target.transform.position;
                SetTaskState(RestockTaskState.GoingToShelf, employeeId, sourceProduct, target);
                if (!TrySetDestination(agent, shelfPosition))
                    yield break;
                yield return WaitForArrival(agent, GetNavigationTimeout(agent));
                if (!HasArrived(agent))
                    yield break;

                SetTaskState(RestockTaskState.PlacingProduct, employeeId, sourceProduct, target);
                yield return new WaitForSeconds(placingSeconds);
                if (target.container == null || !target.IsPlaceable(sourceProduct))
                    yield break;

                Destroy(carriedItem.gameObject);
                carriedItem = sourcePackage.Remove();
                if (carriedItem == null)
                    yield break;
                Vector3 targetPosition = target.Add(sourceProduct);
                Quaternion targetRotation = Quaternion.Euler(0, target.orientation, 0);
                carriedItem.SetParent(null, true);
                InteractionSystem.MoveToTargetArc(carriedItem, target.container, targetPosition, targetRotation, GetPlacementLerpSpeed());
                carriedItem = null;
                LastTaskCompletedVisualRoute = true;

                SetTaskState(RestockTaskState.ReturningOrIdle, employeeId, sourceProduct, target);
                EmployeeNPCSpawner.Instance?.RefreshNPCPosition(employeeId);
            }
            finally
            {
                if (carriedItem != null)
                    Destroy(carriedItem.gameObject);
                CurrentTaskState = RestockTaskState.Idle;
                visualTaskRoutine = null;
                Debug.Log(LogPrefix + "State Idle.");
            }
        }


        private static int FindAvailableRestockerEmployeeId()
        {
            if (EntrepreneurEmployeeSystem.Instance == null)
                return -1;

            foreach (EmployeeAssignment assignment in EntrepreneurEmployeeSystem.Instance.GetAssignments())
                if (assignment != null && assignment.isHired && assignment.role == EmployeeRole.Restocker)
                    return assignment.employeeId;

            return -1;
        }


        private bool TrySetDestination(NavMeshAgent agent, Vector3 target)
        {
            if (agent == null || !agent.enabled || !agent.isOnNavMesh)
                return false;

            if (!TryResolveReachablePoint(agent, target, out Vector3 destination))
            {
                Debug.LogWarning(LogPrefix + "No reachable NavMesh point near " + target + ".");
                return false;
            }

            agent.isStopped = false;
            agent.updatePosition = true;
            agent.updateRotation = true;
            agent.speed = Mathf.Max(2.5f, agent.speed);
            agent.acceleration = Mathf.Max(8f, agent.acceleration);
            agent.SetDestination(destination);
            Debug.Log(LogPrefix + "Navigation target " + destination + " resolved near " + target + ".");
            return true;
        }


        private bool TryResolveReachablePoint(NavMeshAgent agent, Vector3 target, out Vector3 destination)
        {
            destination = Vector3.zero;
            float bestDistance = float.MaxValue;
            NavMeshPath path = new NavMeshPath();

            for (float radius = 0f; radius <= navMeshSampleRadius; radius += 2f)
            {
                int samples = radius <= 0f ? 1 : 12;
                for (int i = 0; i < samples; i++)
                {
                    float angle = samples == 1 ? 0f : i * Mathf.PI * 2f / samples;
                    Vector3 candidate = target + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                    if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2.5f, NavMesh.AllAreas))
                        continue;
                    if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
                        continue;

                    float distance = Vector3.Distance(hit.position, target);
                    if (distance >= bestDistance)
                        continue;

                    bestDistance = distance;
                    destination = hit.position;
                }
            }

            return bestDistance < float.MaxValue;
        }


        private IEnumerator WaitForArrival(NavMeshAgent agent, float timeout)
        {
            float started = Time.realtimeSinceStartup;
            Vector3 startPosition = agent != null ? agent.transform.position : Vector3.zero;
            while (agent != null && agent.enabled && agent.isOnNavMesh && agent.pathPending)
            {
                if (Time.realtimeSinceStartup - started >= navigationTimeoutSeconds)
                {
                    Debug.LogWarning(LogPrefix + "Navigation path calculation timed out in state "
                        + CurrentTaskState + ".");
                    yield break;
                }
                yield return null;
            }

            timeout = Mathf.Max(timeout, GetNavigationTimeout(agent));
            while (agent != null && agent.enabled && agent.isOnNavMesh && !HasArrived(agent))
            {
                UpdateEmployeeAnimator(agent);
                if (Time.realtimeSinceStartup - started >= timeout)
                {
                    Debug.LogWarning(LogPrefix + "Navigation timeout in state " + CurrentTaskState
                        + ". start=" + startPosition + ", current=" + agent.transform.position
                        + ", moved=" + Vector3.Distance(startPosition, agent.transform.position).ToString("0.00")
                        + ", remaining=" + agent.remainingDistance.ToString("0.00")
                        + ", velocity=" + agent.velocity.magnitude.ToString("0.00") + ".");
                    yield break;
                }
                yield return null;
            }
            UpdateEmployeeAnimator(agent);
            if (HasArrived(agent))
                Debug.Log(LogPrefix + "Arrived in state " + CurrentTaskState
                    + ". moved=" + Vector3.Distance(startPosition, agent.transform.position).ToString("0.00")
                    + ", elapsed=" + (Time.realtimeSinceStartup - started).ToString("0.00") + "s.");
        }


        private float GetNavigationTimeout(NavMeshAgent agent)
        {
            if (agent == null || !agent.hasPath || agent.path == null)
                return navigationTimeoutSeconds;

            Vector3[] corners = agent.path.corners;
            float distance = 0f;
            for (int i = 1; i < corners.Length; i++)
                distance += Vector3.Distance(corners[i - 1], corners[i]);

            float expectedSeconds = distance / Mathf.Max(0.1f, agent.speed);
            return Mathf.Max(navigationTimeoutSeconds,
                expectedSeconds * navigationTimeoutMultiplier + navigationTimeoutMarginSeconds);
        }


        private static void UpdateEmployeeAnimator(NavMeshAgent agent)
        {
            if (agent == null)
                return;

            Animator animator = agent.GetComponent<Animator>();
            if (animator == null || !animator.enabled)
                return;

            Vector3 localVelocity = Quaternion.Inverse(agent.transform.rotation) * agent.desiredVelocity;
            animator.SetFloat("Speed", agent.velocity.magnitude);
            animator.SetFloat("Direction", Mathf.Atan2(localVelocity.x, localVelocity.z) * Mathf.Rad2Deg);
        }


        private bool HasArrived(NavMeshAgent agent)
        {
            return agent != null && !agent.pathPending &&
                   agent.remainingDistance <= Mathf.Max(arrivalTolerance, agent.stoppingDistance);
        }


        private void SetTaskState(RestockTaskState state, int employeeId,
                                  ProductScriptableObject product, PlacementObject target)
        {
            CurrentTaskState = state;
            Debug.Log(LogPrefix + "Employee #" + employeeId + " state " + state
                + " product='" + product.title + "' target='" + target.name + "'.");
        }


        private PlacementObject FindRestockTarget(out PackageObject sourcePackage, out ProductScriptableObject sourceProduct)
        {
            sourcePackage = null;
            sourceProduct = null;

            PlacementObject[] placements = FindAllPlacements();
            PlacementObject bestTarget = null;
            float bestFillRatio = 1f;
            bool anySlotAssigned = ShelfProductSlotSystem.Instance != null
                && ShelfProductSlotSystem.Instance.HasAnySlotAssigned();

            for (int i = 0; i < placements.Length; i++)
            {
                PlacementObject placement = placements[i];
                if (placement == null)
                    continue;

                PackageObject packageCandidate;
                ProductScriptableObject productCandidate;

                // If ShelfProductSlotSystem has an assignment for this placement, respect it.
                if (ShelfProductSlotSystem.Instance != null)
                {
                    ProductScriptableObject slotProduct =
                        ShelfProductSlotSystem.Instance.GetAssignedProduct(placement);
                    if (slotProduct != null)
                    {
                        productCandidate = slotProduct;
                        packageCandidate = FindPackageWithProduct(productCandidate);
                        if (packageCandidate == null)
                        {
                            Debug.Log(LogPrefix + "Restock: assigned product '"
                                + productCandidate.title + "' for '" + placement.name
                                + "' — no stock available.");
                            if (UIGame.Instance != null)
                                UIGame.AddNotification(
                                    "Sin stock de " + productCandidate.title + " para surtir " + placement.name + ".",
                                    otherColor: new Color(1f, 0.65f, 0.18f));
                            continue;
                        }
                    }
                    else if (placement.product != null)
                    {
                        productCandidate = placement.product;
                        packageCandidate = FindPackageWithProduct(productCandidate);
                    }
                    else
                    {
                        packageCandidate = FindPackageForStorageType(placement.storageType, out productCandidate);
                    }
                }
                else if (placement.product != null)
                {
                    productCandidate = placement.product;
                    packageCandidate = FindPackageWithProduct(productCandidate);
                }
                else
                {
                    packageCandidate = FindPackageForStorageType(placement.storageType, out productCandidate);
                }

                if (packageCandidate == null || productCandidate == null)
                    continue;
                string compatibilityReason;
                if (!ShelfProductSlotSystem.CanPlaceProductOnFurniture(productCandidate, placement, out compatibilityReason))
                {
                    Debug.LogWarning(LogPrefix + compatibilityReason);
                    if (UIGame.Instance != null)
                        UIGame.AddNotification(
                            "El producto " + productCandidate.title + " no puede colocarse en este mueble.",
                            otherColor: new Color(1f, 0.30f, 0.30f));
                    continue;
                }
                if (!placement.IsPlaceable(productCandidate))
                    continue;

                int maxSlots = placement.GetPlacementPositions(productCandidate).Length;
                if (maxSlots <= 0)
                    continue;

                float fillRatio = placement.count / (float)maxSlots;
                if (fillRatio > lowStockThreshold)
                    continue;
                if (fillRatio >= bestFillRatio)
                    continue;

                bestFillRatio = fillRatio;
                bestTarget = placement;
                sourcePackage = packageCandidate;
                sourceProduct = productCandidate;
            }

            if (bestTarget != null)
            {
                Debug.Log(LogPrefix + "Restock: found missing product '"
                    + (sourceProduct != null ? sourceProduct.title : "?")
                    + "' → '" + bestTarget.name + "'. fill=" + bestFillRatio.ToString("F2")
                    + (anySlotAssigned ? " [slot-assigned]" : " [unassigned]"));
            }
            else if (!anySlotAssigned && ShelfProductSlotSystem.Instance != null)
            {
                Debug.Log(LogPrefix + "Restock: no assigned shelf slots — restocking by storage type only.");
            }

            return bestTarget;
        }


        private static PackageObject FindPackageWithProduct(ProductScriptableObject product)
        {
            if (product == null)
                return null;

            PackageObject[] packages = FindAllPackages();
            for (int i = 0; i < packages.Length; i++)
            {
                PackageObject package = packages[i];
                if (package == null || package.count <= 0)
                    continue;
                if (package.purchasable is not ProductScriptableObject packageProduct)
                    continue;
                if (packageProduct != product)
                    continue;

                return package;
            }

            return null;
        }


        private static PackageObject FindPackageForStorageType(StorageType storageType, out ProductScriptableObject product)
        {
            product = null;
            PackageObject[] packages = FindAllPackages();
            for (int i = 0; i < packages.Length; i++)
            {
                PackageObject package = packages[i];
                if (package == null || package.count <= 0)
                    continue;
                if (package.purchasable is not ProductScriptableObject packageProduct)
                    continue;
                if (packageProduct.storageType != storageType)
                    continue;

                product = packageProduct;
                return package;
            }

            return null;
        }


        private void MaybeNotifyMissingSource()
        {
            if (!notifyWhenMissingSource)
                return;
            if (Time.time - lastMissingSourceWarnTime < 8f)
                return;

            lastMissingSourceWarnTime = Time.time;
            UIGame.AddNotification("Surtidores sin productos disponibles para reabasto.", otherColor: new Color(1f, 0.65f, 0.18f));
            Debug.LogWarning(LogPrefix + "Restocker cycle skipped due to missing package source or target placement.");
        }


        private float GetCurrentCycleDelay()
        {
            float speed = 1f;
            if (EntrepreneurEmployeeSystem.Instance != null)
                speed *= EntrepreneurEmployeeSystem.Instance.GetRestockerSpeedMultiplier();
            if (EntrepreneurTreeGameplayBridge.Instance != null)
                speed *= EntrepreneurTreeGameplayBridge.Instance.GetEmployeeSpeedMultiplier();

            return Mathf.Max(minCycleSeconds, baseCycleSeconds / Mathf.Max(0.1f, speed));
        }


        private float GetPlacementLerpSpeed()
        {
            float speed = 1.2f;
            if (InteractionSystem.Instance != null)
                speed = InteractionSystem.Instance.lerpSpeed;

            if (EntrepreneurEmployeeSystem.Instance != null)
                speed *= EntrepreneurEmployeeSystem.Instance.GetRestockerSpeedMultiplier();
            if (EntrepreneurTreeGameplayBridge.Instance != null)
                speed *= EntrepreneurTreeGameplayBridge.Instance.GetEmployeeSpeedMultiplier();

            return speed;
        }

        private static PlacementObject[] FindAllPlacements()
        {
#if UNITY_2022_2_OR_NEWER
            return Object.FindObjectsByType<PlacementObject>(FindObjectsSortMode.None);
#else
            return Object.FindObjectsOfType<PlacementObject>();
#endif
        }

        private static PackageObject[] FindAllPackages()
        {
#if UNITY_2022_2_OR_NEWER
            return Object.FindObjectsByType<PackageObject>(FindObjectsSortMode.None);
#else
            return Object.FindObjectsOfType<PackageObject>();
#endif
        }


        void OnDestroy()
        {
            StopCycle();
            if (visualTaskRoutine != null)
                StopCoroutine(visualTaskRoutine);
            DayCycleSystem.onDayStarted -= OnDayStarted;
            DayCycleSystem.onDayOver -= OnDayOver;
            SaveGameSystem.dataLoadEvent -= OnDataLoaded;
        }
    }
}
