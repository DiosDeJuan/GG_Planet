using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Spawns and manages visual employee NPC GameObjects.
    ///
    /// How it works:
    ///   • Reuses Customer_A–E prefabs from the asset (Inspector slot: employeePrefabs).
    ///   • When an employee is hired, a prefab variant is instantiated near the spawn
    ///     point (Inspector slot: employeeSpawnPoint).
    ///   • Customer, CustomerAgent and CustomerCart components are immediately disabled
    ///     so the NPC stops acting as a shopper.
    ///   • NavMeshAgent and Animator remain active — future patrol/task AI can drive them.
    ///   • Each NPC is tagged with a tint material override to visually distinguish
    ///     Cashiers (blue tint) from Restockers (green tint). Requires the material to
    ///     be readable and the renderer to be writable; if not, the tint is skipped.
    ///
    /// SCENE SETUP:
    ///   Attach this component to the same persistent root as EntrepreneurEmployeeSystem.
    ///   Assign at least one prefab from Assets/StoreSimulator/Prefabs/Customers/ to
    ///   'employeePrefabs'.
    ///   Assign 'employeeSpawnPoint' to any Transform inside the store (e.g. the cash
    ///   desk area or a dedicated spawn marker).
    ///   If left unconfigured, spawning is skipped with a warning — gameplay logic still
    ///   works (EmployeeCashierCoordinator / EmployeeRestockCoordinator continue).
    /// </summary>
    [DisallowMultipleComponent]
    public class EmployeeNPCSpawner : MonoBehaviour
    {
        private const string LogPrefix = "[EmployeeNPC] ";

        // ── Inspector ────────────────────────────────────────────────────────────

        [Header("Prefabs (assign Customer_A–E variants)")]
        [Tooltip("Pool of customer NPC prefabs to pick from for each employee.")]
        public GameObject[] employeePrefabs = new GameObject[0];

        [Header("Spawn / placement")]
        [Tooltip("Where newly hired employees appear. If null, spawns at this transform's position.")]
        public Transform employeeSpawnPoint;

        [Tooltip("Offset added to spawn position for each successive hired employee, to avoid stacking.")]
        public Vector3 spawnOffset = new Vector3(1.5f, 0f, 0f);

        [Header("Tints (optional visual distinction)")]
        public Color cashierTint    = new Color(0.55f, 0.70f, 1.00f, 1f);
        public Color restockerTint  = new Color(0.55f, 0.90f, 0.55f, 1f);

        // ── Internal ─────────────────────────────────────────────────────────────

        // employeeId → spawned NPC GameObject
        private readonly Dictionary<int, GameObject> spawnedNPCs = new Dictionary<int, GameObject>();

        private MaterialPropertyBlock _mpb;

        public static EmployeeNPCSpawner Instance { get; private set; }

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            EntrepreneurEmployeeSystem.onEmployeeHired       += OnEmployeeHired;
            EntrepreneurEmployeeSystem.onEmployeeRoleChanged += OnRoleChanged;
            SaveGameSystem.dataLoadEvent                     += OnDataLoaded;
            SceneManager.sceneLoaded                         += OnSceneLoaded;
        }

        void Start()
        {
            // Auto-discover Customer prefabs from the scene if none are configured in the Inspector.
            // This is needed because this component is added at runtime via AddComponent and
            // cannot be configured in the Inspector.
            if (employeePrefabs == null || employeePrefabs.Length == 0)
                employeePrefabs = AutoDiscoverCustomerPrefabs();

            // Re-spawn any employees that were hired before this component started
            // (e.g. loaded from save or admin mode unlock-all).
            RespawnAll();
        }

        void OnDestroy()
        {
            EntrepreneurEmployeeSystem.onEmployeeHired       -= OnEmployeeHired;
            EntrepreneurEmployeeSystem.onEmployeeRoleChanged -= OnRoleChanged;
            SaveGameSystem.dataLoadEvent                     -= OnDataLoaded;
            SceneManager.sceneLoaded                         -= OnSceneLoaded;

            if (Instance == this)
                Instance = null;
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>Returns the spawned NPC for the given employee, or null if not spawned.</summary>
        public GameObject GetNPC(int employeeId)
        {
            GameObject npc;
            return spawnedNPCs.TryGetValue(employeeId, out npc) ? npc : null;
        }

        /// <summary>
        /// Moves the NPC for <paramref name="employeeId"/> to its currently assigned workstation.
        /// Safe to call even when no workstation is assigned (no-op).
        /// Called by EmployeeAppUIController after assigning a workstation from the UI.
        /// </summary>
        public void RefreshNPCPosition(int employeeId)
        {
            MoveNPCToWorkstation(employeeId);
        }

        // ── Auto-discovery ────────────────────────────────────────────────────────

        /// <summary>
        /// Finds distinct Customer GameObjects in the scene and returns their root
        /// GameObjects as prefab candidates.  This is a runtime fallback for when
        /// the component is created programmatically (no Inspector assignment).
        /// Each found customer root is used as a visual template — it is NOT
        /// re-used directly; we pass it to Instantiate(), same as a prefab.
        /// </summary>
        private static GameObject[] AutoDiscoverCustomerPrefabs()
        {
            if (CustomerSystem.Instance != null &&
                CustomerSystem.Instance.customerPrefabs != null &&
                CustomerSystem.Instance.customerPrefabs.Length > 0)
            {
                Debug.Log(LogPrefix + "Using " + CustomerSystem.Instance.customerPrefabs.Length
                    + " customer prefab(s) from CustomerSystem for employee NPC spawning.");
                return CustomerSystem.Instance.customerPrefabs;
            }

            Customer[] customers = FindCustomers();
            if (customers == null || customers.Length == 0)
            {
                Debug.LogWarning(LogPrefix + "Auto-discover: no Customer components found in scene. "
                    + "Employee visual NPCs will not spawn. Assign prefabs manually or ensure "
                    + "Customer_A–E prefabs are in the scene.");
                return new GameObject[0];
            }

            // Collect distinct root GameObjects (use root so we capture the full character prefab).
            System.Collections.Generic.HashSet<GameObject> seen =
                new System.Collections.Generic.HashSet<GameObject>();
            System.Collections.Generic.List<GameObject> result =
                new System.Collections.Generic.List<GameObject>();

            for (int i = 0; i < customers.Length; i++)
            {
                if (customers[i] == null)
                    continue;

                // Walk up to the first root or to the first parent without a Customer component
                // so we grab the whole character, not just the component's GO.
                GameObject root = customers[i].gameObject;
                Transform parent = root.transform.parent;
                while (parent != null && parent.GetComponent<Customer>() != null)
                {
                    root = parent.gameObject;
                    parent = root.transform.parent;
                }

                if (seen.Add(root))
                    result.Add(root);
            }

            if (result.Count == 0)
            {
                Debug.LogWarning(LogPrefix + "Auto-discover: customers found but no roots resolved.");
                return new GameObject[0];
            }

            System.Text.StringBuilder nameList = new System.Text.StringBuilder();
            for (int i = 0; i < result.Count; i++)
            {
                if (i > 0) nameList.Append(", ");
                nameList.Append(result[i].name);
            }
            Debug.Log(LogPrefix + "Auto-discovered " + result.Count
                + " customer prefab(s) for employee NPC spawning: "
                + nameList);
            return result.ToArray();
        }

        private static Customer[] FindCustomers()
        {
#if UNITY_2022_2_OR_NEWER
            return Object.FindObjectsByType<Customer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            return Object.FindObjectsOfType<Customer>(true);
#endif
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        private void OnEmployeeHired(int employeeId)
        {
            EnsureEmployeePrefabs();
            SpawnOrRefresh(employeeId);
        }

        private void OnRoleChanged(int employeeId, EmployeeRole role)
        {
            ApplyRoleTint(employeeId, role);
            MoveNPCToWorkstation(employeeId);
        }

        private void OnDataLoaded()
        {
            // Ensure we have prefabs discovered even after a scene reload.
            EnsureEmployeePrefabs();

            // Destroy all existing NPCs (they were from the previous session's state).
            DespawnAll();
            RespawnAll();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureEmployeePrefabs();
            RespawnAll();
        }

        private void EnsureEmployeePrefabs()
        {
            if (employeePrefabs == null || employeePrefabs.Length == 0)
                employeePrefabs = AutoDiscoverCustomerPrefabs();
        }

        private void RespawnAll()
        {
            if (EntrepreneurEmployeeSystem.Instance == null)
                return;

            foreach (EmployeeAssignment a in EntrepreneurEmployeeSystem.Instance.GetAssignments())
            {
                if (a != null && a.isHired)
                    SpawnOrRefresh(a.employeeId);
            }
        }

        private void DespawnAll()
        {
            foreach (KeyValuePair<int, GameObject> pair in spawnedNPCs)
            {
                if (pair.Value != null)
                    Destroy(pair.Value);
            }

            spawnedNPCs.Clear();
        }

        private void SpawnOrRefresh(int employeeId)
        {
            if (spawnedNPCs.ContainsKey(employeeId))
            {
                // Already spawned — just refresh tint.
                EmployeeRole role = EntrepreneurEmployeeSystem.Instance != null
                    ? EntrepreneurEmployeeSystem.Instance.GetEmployeeRole(employeeId)
                    : EmployeeRole.None;
                ApplyRoleTint(employeeId, role);
                return;
            }

            if (employeePrefabs == null || employeePrefabs.Length == 0)
            {
                EnsureEmployeePrefabs();
            }

            if (employeePrefabs == null || employeePrefabs.Length == 0)
            {
                Debug.LogWarning(LogPrefix + "No employee prefabs configured — visual NPC will not spawn for employee #"
                    + employeeId + ". Assign Customer_A–E prefabs to EmployeeNPCSpawner.employeePrefabs.");
                return;
            }

            // Pick a prefab deterministically so the same employee always uses the same visual.
            int prefabIndex = (employeeId - 1) % employeePrefabs.Length;
            GameObject prefab = employeePrefabs[prefabIndex];
            if (prefab == null)
            {
                Debug.LogWarning(LogPrefix + "Employee prefab slot " + prefabIndex + " is null.");
                return;
            }

            Vector3 basePos = employeeSpawnPoint != null
                ? employeeSpawnPoint.position
                : GetFallbackSpawnPosition();

            // If a workstation is assigned, spawn the NPC there instead of the generic spawn point.
            Vector3 position = basePos + spawnOffset * (spawnedNPCs.Count);
            Quaternion rotation = Quaternion.identity;

            string workstationId = string.Empty;
            if (EmployeeWorkstationRegistry.Instance != null)
            {
                EmployeeWorkstation ws = EmployeeWorkstationRegistry.Instance.GetAssignedStation(employeeId);
                if (ws != null)
                {
                    position = ws.StandPosition;
                    rotation = ws.StandRotation;
                    workstationId = ws.workstationId;
                }
            }

            GameObject npc = Instantiate(prefab, position, rotation);
            npc.name = "Employee_" + employeeId + "_NPC";

            // Disable shopping AI — keep visual/locomotion alive.
            DisableShoppingComponents(npc);

            // Apply tint for role.
            EmployeeRole startRole = EntrepreneurEmployeeSystem.Instance != null
                ? EntrepreneurEmployeeSystem.Instance.GetEmployeeRole(employeeId)
                : EmployeeRole.None;
            ApplyTintToNPC(npc, startRole);

            spawnedNPCs[employeeId] = npc;

            LogSpawnDiagnostics(employeeId, prefab.name, position, npc, startRole, workstationId);
        }

        /// <summary>
        /// Logs comprehensive visual diagnostics for a newly spawned employee NPC.
        /// Output covers: employeeId, role, prefab name, spawn position, final position,
        /// world-space scale, active renderer count, parent-active chain, NavMeshAgent status,
        /// workstation ID and approximate distance to the main camera.
        /// Emits [WARN] lines for any condition that would make the NPC invisible or non-functional.
        /// Satisfies Tarea 6 visual proof requirements.
        /// </summary>
        /// <param name="employeeId">Numeric ID of the hired employee.</param>
        /// <param name="prefabName">Name of the prefab used for instantiation.</param>
        /// <param name="spawnPosition">World-space position requested at instantiation time.</param>
        /// <param name="npc">The instantiated NPC GameObject.</param>
        /// <param name="role">Role assigned at spawn time (Cashier, Restocker or None).</param>
        /// <param name="workstationId">ID of the workstation the NPC was sent to, or empty if none.</param>
        private static void LogSpawnDiagnostics(int employeeId, string prefabName,
            Vector3 spawnPosition, GameObject npc, EmployeeRole role, string workstationId)
        {
            Vector3 finalPos  = npc.transform.position;
            Vector3 finalScale = npc.transform.lossyScale;

            // Renderer status
            Renderer[] renderers = npc.GetComponentsInChildren<Renderer>(includeInactive: true);
            int activeRenderers   = 0;
            int totalRenderers    = renderers.Length;
            foreach (Renderer r in renderers)
                if (r != null && r.enabled && r.gameObject.activeInHierarchy)
                    activeRenderers++;

            // NavMeshAgent status
            NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
            bool hasAgent  = agent != null;
            bool agentEnabled = hasAgent && agent.enabled;
            bool agentOnMesh  = hasAgent && agent.isOnNavMesh;
            string pathStatus = hasAgent ? agent.pathStatus.ToString() : "N/A";

            // Camera / player proximity
            Camera mainCam = Camera.main;
            float camDist  = mainCam != null
                ? Vector3.Distance(finalPos, mainCam.transform.position)
                : -1f;

            // Parent active chain
            bool parentActive = true;
            Transform t = npc.transform.parent;
            while (t != null)
            {
                if (!t.gameObject.activeSelf) { parentActive = false; break; }
                t = t.parent;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine(LogPrefix + "=== NPC Spawn Diagnostics ===");
            sb.AppendLine("  employeeId   : " + employeeId);
            sb.AppendLine("  role         : " + role);
            sb.AppendLine("  prefab       : " + prefabName);
            sb.AppendLine("  spawnPos     : " + spawnPosition);
            sb.AppendLine("  finalPos     : " + finalPos);
            sb.AppendLine("  scale        : " + finalScale);
            sb.AppendLine("  renderers    : " + activeRenderers + "/" + totalRenderers + " active");
            sb.AppendLine("  parentActive : " + parentActive);
            sb.AppendLine("  npcActive    : " + npc.activeSelf + " (hierarchy=" + npc.activeInHierarchy + ")");
            sb.AppendLine("  NavMeshAgent : present=" + hasAgent
                + " enabled=" + agentEnabled + " onMesh=" + agentOnMesh
                + " pathStatus=" + pathStatus);
            sb.AppendLine("  workstationId: " + (string.IsNullOrEmpty(workstationId) ? "<none>" : workstationId));
            sb.AppendLine("  camDist      : " + (camDist < 0f ? "no main camera" : camDist.ToString("0.0") + "m"));

            if (activeRenderers == 0)
                sb.AppendLine("  [WARN] No active renderers — NPC will be invisible to player!");
            if (!agentOnMesh)
                sb.AppendLine("  [WARN] NavMeshAgent is NOT on NavMesh — NPC may be stuck or teleported!");
            if (!parentActive)
                sb.AppendLine("  [WARN] A parent object is inactive — NPC will not appear in scene!");
            if (finalScale.x < 0.01f || finalScale.y < 0.01f || finalScale.z < 0.01f)
                sb.AppendLine("  [WARN] Scale is near zero — NPC will not be visible!");

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Dumps a full visual diagnostic for all currently spawned NPCs.
        /// Call this from the console or a runner to verify NPC visibility at runtime.
        /// </summary>
        public void DumpAllNPCDiagnostics()
        {
            if (spawnedNPCs.Count == 0)
            {
                Debug.LogWarning(LogPrefix + "DumpAllNPCDiagnostics: no NPCs currently spawned.");
                return;
            }

            foreach (KeyValuePair<int, GameObject> pair in spawnedNPCs)
            {
                int eid = pair.Key;
                GameObject npc = pair.Value;
                if (npc == null)
                {
                    Debug.LogWarning(LogPrefix + "Employee #" + eid + " NPC has been destroyed.");
                    continue;
                }

                EmployeeRole role = EntrepreneurEmployeeSystem.Instance != null
                    ? EntrepreneurEmployeeSystem.Instance.GetEmployeeRole(eid)
                    : EmployeeRole.None;

                string wsId = EmployeeWorkstationRegistry.Instance != null
                    ? EmployeeWorkstationRegistry.Instance.GetAssignedId(eid)
                    : string.Empty;

                LogSpawnDiagnostics(eid, npc.name, npc.transform.position, npc, role, wsId);
            }
        }

        private Vector3 GetFallbackSpawnPosition()
        {
            if (CustomerSystem.Instance != null &&
                CustomerSystem.Instance.spawnLocations != null &&
                CustomerSystem.Instance.spawnLocations.Length > 0 &&
                CustomerSystem.Instance.spawnLocations[0] != null)
            {
                return CustomerSystem.Instance.spawnLocations[0].position;
            }

            return transform.position;
        }

        private static void DisableShoppingComponents(GameObject npc)
        {
            // Disable Customer AI so NPC doesn't start shopping.
            Customer customer = npc.GetComponent<Customer>();
            if (customer != null)
                customer.enabled = false;

            // Disable cart/bag holder logic.
            CustomerCart cart = npc.GetComponent<CustomerCart>();
            if (cart != null)
                cart.enabled = false;

            // Disable customer-movement AI (CustomerAgent routes to store locations).
            CustomerAgent customerAgent = npc.GetComponent<CustomerAgent>();
            if (customerAgent != null)
                customerAgent.enabled = false;

            // Keep NavMeshAgent active (future patrol route AI can use it).
            // Keep Animator active (idle/walk animations play).
        }

        private void ApplyRoleTint(int employeeId, EmployeeRole role)
        {
            if (!spawnedNPCs.TryGetValue(employeeId, out GameObject npc) || npc == null)
                return;

            ApplyTintToNPC(npc, role);
        }

        /// <summary>
        /// Moves an already-spawned NPC to the position/rotation of its current workstation.
        /// Safe to call even if no workstation is assigned (NPC stays where it is).
        /// </summary>
        private void MoveNPCToWorkstation(int employeeId)
        {
            if (!spawnedNPCs.TryGetValue(employeeId, out GameObject npc) || npc == null)
                return;

            if (EmployeeWorkstationRegistry.Instance == null)
                return;

            EmployeeWorkstation ws = EmployeeWorkstationRegistry.Instance.GetAssignedStation(employeeId);
            if (ws == null)
                return;

            NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
            NavMeshHit hit;
            if (agent != null && agent.enabled && agent.isOnNavMesh &&
                NavMesh.SamplePosition(ws.StandPosition, out hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                npc.transform.rotation = ws.StandRotation;
                Debug.Log(LogPrefix + "Employee #" + employeeId
                    + " walking to workstation '" + ws.workstationId + "' at " + hit.position + ".");
                return;
            }

            npc.transform.position = ws.StandPosition;
            npc.transform.rotation = ws.StandRotation;
            Debug.LogWarning(LogPrefix + "Employee #" + employeeId
                + " moved directly to workstation '" + ws.workstationId
                + "' because no valid NavMesh route was available.");
        }

        private void ApplyTintToNPC(GameObject npc, EmployeeRole role)
        {
            Color tint = role == EmployeeRole.Cashier   ? cashierTint
                       : role == EmployeeRole.Restocker ? restockerTint
                       : Color.white;

            // Apply tint via MaterialPropertyBlock (avoids material duplication / leaks).
            Renderer[] renderers = npc.GetComponentsInChildren<Renderer>(includeInactive: true);

            foreach (Renderer r in renderers)
            {
                if (r == null)
                    continue;

                r.GetPropertyBlock(_mpb);
                _mpb.SetColor("_Color", tint);
                r.SetPropertyBlock(_mpb);
            }
        }
    }
}
