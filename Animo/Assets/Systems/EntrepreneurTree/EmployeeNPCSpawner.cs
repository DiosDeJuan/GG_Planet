using System.Collections.Generic;
using UnityEngine;

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
        }

        void Start()
        {
            // Re-spawn any employees that were hired before this component started
            // (e.g. loaded from save or admin mode unlock-all).
            RespawnAll();
        }

        void OnDestroy()
        {
            EntrepreneurEmployeeSystem.onEmployeeHired       -= OnEmployeeHired;
            EntrepreneurEmployeeSystem.onEmployeeRoleChanged -= OnRoleChanged;
            SaveGameSystem.dataLoadEvent                     -= OnDataLoaded;

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

        // ── Private helpers ───────────────────────────────────────────────────────

        private void OnEmployeeHired(int employeeId)
        {
            SpawnOrRefresh(employeeId);
        }

        private void OnRoleChanged(int employeeId, EmployeeRole role)
        {
            ApplyRoleTint(employeeId, role);
            MoveNPCToWorkstation(employeeId);
        }

        private void OnDataLoaded()
        {
            // Destroy all existing NPCs (they were from the previous session's state).
            DespawnAll();
            RespawnAll();
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
                : transform.position;

            // If a workstation is assigned, spawn the NPC there instead of the generic spawn point.
            Vector3 position = basePos + spawnOffset * (spawnedNPCs.Count);
            Quaternion rotation = Quaternion.identity;

            if (EmployeeWorkstationRegistry.Instance != null)
            {
                EmployeeWorkstation ws = EmployeeWorkstationRegistry.Instance.GetAssignedStation(employeeId);
                if (ws != null)
                {
                    position = ws.StandPosition;
                    rotation = ws.StandRotation;
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

            Debug.Log(LogPrefix + "Spawned NPC for employee #" + employeeId
                + " using prefab '" + prefab.name + "' at " + position + ".");
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

            npc.transform.position = ws.StandPosition;
            npc.transform.rotation = ws.StandRotation;
            Debug.Log(LogPrefix + "Moved NPC for employee #" + employeeId
                + " to workstation '" + ws.workstationId + "' at " + ws.StandPosition + ".");
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
