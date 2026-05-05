using System.Collections;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Connects hired restockers to real in-store replenishment (packages -> placements).
    /// </summary>
    public class EmployeeRestockCoordinator : MonoBehaviour
    {
        private const string LogPrefix = "[EntrepreneurTree] ";

        [Header("Cycle")]
        [Min(1f)] public float baseCycleSeconds = 12f;
        [Min(0.5f)] public float minCycleSeconds = 1.5f;
        [Range(0.1f, 0.95f)] public float lowStockThreshold = 0.40f;
        [Min(1)] public int maxTasksPerCycle = 6;
        public bool notifyWhenMissingSource = false;

        private Coroutine cycleRoutine;
        private float lastMissingSourceWarnTime;

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
                    ExecuteSingleTask();
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

            if (!target.IsPlaceable(sourceProduct))
                return;

            Transform item = sourcePackage.Remove();
            if (item == null)
                item = Instantiate(sourceProduct.prefab).transform;

            Vector3 targetPosition = target.Add(sourceProduct);
            Quaternion targetRotation = Quaternion.Euler(0, target.orientation, 0);
            InteractionSystem.MoveToTargetArc(item, target.container, targetPosition, targetRotation, GetPlacementLerpSpeed());
        }


        private PlacementObject FindRestockTarget(out PackageObject sourcePackage, out ProductScriptableObject sourceProduct)
        {
            sourcePackage = null;
            sourceProduct = null;

            PlacementObject[] placements = Object.FindObjectsByType<PlacementObject>(FindObjectsSortMode.None);
            PlacementObject bestTarget = null;
            float bestFillRatio = 1f;

            for (int i = 0; i < placements.Length; i++)
            {
                PlacementObject placement = placements[i];
                if (placement == null)
                    continue;

                PackageObject packageCandidate;
                ProductScriptableObject productCandidate;

                if (placement.product != null)
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
                if (productCandidate.storageType != placement.storageType)
                    continue;
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

            return bestTarget;
        }


        private static PackageObject FindPackageWithProduct(ProductScriptableObject product)
        {
            if (product == null)
                return null;

            PackageObject[] packages = Object.FindObjectsByType<PackageObject>(FindObjectsSortMode.None);
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
            PackageObject[] packages = Object.FindObjectsByType<PackageObject>(FindObjectsSortMode.None);
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


        void OnDestroy()
        {
            StopCycle();
            DayCycleSystem.onDayStarted -= OnDayStarted;
            DayCycleSystem.onDayOver -= OnDayOver;
            SaveGameSystem.dataLoadEvent -= OnDataLoaded;
        }
    }
}
