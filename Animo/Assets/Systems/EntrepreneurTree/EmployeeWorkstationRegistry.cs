// Árbol del Emprendedor — EmployeeWorkstationRegistry
// Central registry of all EmployeeWorkstation instances in the scene.
// Auto-populated by EmployeeWorkstation.Awake/OnDestroy.
// Also auto-creates fallback CashierStations next to any CashDesk found in the scene.

using System.Collections.Generic;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Singleton registry for all <see cref="EmployeeWorkstation"/> instances.
    /// Provides query, assign and release operations and persists assignments through
    /// <see cref="EntrepreneurTreeSaveIntegration"/>.
    /// </summary>
    public class EmployeeWorkstationRegistry : MonoBehaviour
    {
        private const string LogPrefix = "[WorkstationReg] ";

        // Distance threshold for checking whether a cashier station already covers a cash desk.
        private const float CashierCoverageRadius = 3f;
        // Local position offset for fallback cashier station (behind the cash desk).
        private const float CashierStationBackOffset = -0.8f;
        // World-space offset for the fallback restocker station when none is defined in the scene.
        private static readonly Vector3 RestockerFallbackOffset = new Vector3(2f, 0f, 2f);

        public static EmployeeWorkstationRegistry Instance { get; private set; }

        // All workstations discovered in the current scene, keyed by stable workstationId.
        private static readonly Dictionary<string, EmployeeWorkstation> _all
            = new Dictionary<string, EmployeeWorkstation>();

        // employeeId → workstationId
        private readonly Dictionary<int, string> _assignmentByEmployee
            = new Dictionary<int, string>();

        // ── Registration (called by EmployeeWorkstation) ──────────────────────────

        public static void Register(EmployeeWorkstation ws)
        {
            if (ws == null || string.IsNullOrEmpty(ws.workstationId))
                return;

            _all[ws.workstationId] = ws;
        }

        public static void Unregister(EmployeeWorkstation ws)
        {
            if (ws == null || string.IsNullOrEmpty(ws.workstationId))
                return;

            _all.Remove(ws.workstationId);
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            SaveGameSystem.dataLoadEvent += OnDataLoaded;
            EntrepreneurEmployeeSystem.onEmployeeRoleChanged += OnEmployeeRoleChanged;
        }

        void Start()
        {
            EnsureFallbackCashierStations();
        }

        void OnDestroy()
        {
            SaveGameSystem.dataLoadEvent -= OnDataLoaded;
            EntrepreneurEmployeeSystem.onEmployeeRoleChanged -= OnEmployeeRoleChanged;

            if (Instance == this)
                Instance = null;
        }

        // ── Public query API ──────────────────────────────────────────────────────

        /// <summary>Returns all registered workstations.</summary>
        public IReadOnlyCollection<EmployeeWorkstation> GetAll()
            => _all.Values;

        /// <summary>Returns all workstations of the given type.</summary>
        public List<EmployeeWorkstation> GetByType(EmployeeWorkstationType type)
        {
            List<EmployeeWorkstation> result = new List<EmployeeWorkstation>();
            foreach (EmployeeWorkstation ws in _all.Values)
            {
                if (ws != null && ws.workstationType == type)
                    result.Add(ws);
            }
            return result;
        }

        /// <summary>Returns the workstation currently assigned to <paramref name="employeeId"/>, or null.</summary>
        public EmployeeWorkstation GetAssignedStation(int employeeId)
        {
            string wsId;
            if (!_assignmentByEmployee.TryGetValue(employeeId, out wsId) || string.IsNullOrEmpty(wsId))
                return null;

            EmployeeWorkstation ws;
            return _all.TryGetValue(wsId, out ws) ? ws : null;
        }

        /// <summary>Returns the workstationId for <paramref name="employeeId"/>, or empty string.</summary>
        public string GetAssignedId(int employeeId)
        {
            string wsId;
            return _assignmentByEmployee.TryGetValue(employeeId, out wsId) ? wsId : string.Empty;
        }

        /// <summary>
        /// Attempts to assign the first free station of the correct type to <paramref name="employeeId"/>.
        /// Returns the assigned workstation, or null on failure.
        /// </summary>
        public EmployeeWorkstation TryAutoAssign(int employeeId, EmployeeWorkstationType type,
            out string reason)
        {
            reason = string.Empty;

            // Release previous station if any.
            ReleaseEmployee(employeeId);

            List<EmployeeWorkstation> candidates = GetByType(type);
            if (candidates.Count == 0)
            {
                reason = "No hay puestos de tipo " + type + " disponibles.";
                return null;
            }

            foreach (EmployeeWorkstation ws in candidates)
            {
                if (ws != null && !ws.IsOccupied)
                {
                    if (ws.TryAssign(employeeId))
                    {
                        _assignmentByEmployee[employeeId] = ws.workstationId;
                        Debug.Log(LogPrefix + "Auto-assigned employee #" + employeeId
                            + " to station '" + ws.workstationId + "'.");
                        return ws;
                    }
                }
            }

            reason = "Todos los puestos de " + type + " están ocupados.";
            return null;
        }

        /// <summary>
        /// Assigns a specific workstation by ID to <paramref name="employeeId"/>.
        /// Returns false if the station does not exist or is occupied by another employee.
        /// </summary>
        public bool TryAssignById(int employeeId, string workstationId, out string reason)
        {
            reason = string.Empty;

            EmployeeWorkstation ws;
            if (!_all.TryGetValue(workstationId, out ws) || ws == null)
            {
                reason = "Puesto '" + workstationId + "' no encontrado.";
                return false;
            }

            // Release previous station first.
            ReleaseEmployee(employeeId);

            if (!ws.TryAssign(employeeId))
            {
                reason = "El puesto '" + workstationId + "' ya está ocupado.";
                return false;
            }

            _assignmentByEmployee[employeeId] = workstationId;
            Debug.Log(LogPrefix + "Employee #" + employeeId + " assigned to station '" + workstationId + "'.");
            return true;
        }

        /// <summary>Releases the workstation currently assigned to <paramref name="employeeId"/>.</summary>
        public void ReleaseEmployee(int employeeId)
        {
            string wsId;
            if (!_assignmentByEmployee.TryGetValue(employeeId, out wsId) || string.IsNullOrEmpty(wsId))
                return;

            EmployeeWorkstation ws;
            if (_all.TryGetValue(wsId, out ws) && ws != null)
                ws.Release();

            _assignmentByEmployee.Remove(employeeId);
        }

        // ── Save / Load ───────────────────────────────────────────────────────────

        public SimpleJSON.JSONNode SaveToJSON()
        {
            SimpleJSON.JSONObject data = new SimpleJSON.JSONObject();
            SimpleJSON.JSONArray arr   = new SimpleJSON.JSONArray();
            foreach (KeyValuePair<int, string> pair in _assignmentByEmployee)
            {
                SimpleJSON.JSONObject row = new SimpleJSON.JSONObject();
                row["employeeId"]   = pair.Key;
                row["workstationId"] = pair.Value;
                arr.Add(row);
            }
            data["assignments"] = arr;
            return data;
        }

        public void LoadFromJSON(SimpleJSON.JSONNode data)
        {
            // Clear current assignments without releasing scene objects
            // (scene objects will be reset below).
            foreach (EmployeeWorkstation ws in _all.Values)
                if (ws != null) ws.Release();

            _assignmentByEmployee.Clear();

            if (data == null || data.Count == 0)
                return;

            SimpleJSON.JSONArray arr = data["assignments"].AsArray;
            for (int i = 0; i < arr.Count; i++)
            {
                int empId    = arr[i]["employeeId"].AsInt;
                string wsId  = arr[i]["workstationId"].Value;
                if (empId <= 0 || string.IsNullOrEmpty(wsId))
                    continue;

                EmployeeWorkstation ws;
                if (_all.TryGetValue(wsId, out ws) && ws != null)
                {
                    ws.RestoreOccupant(empId);
                    _assignmentByEmployee[empId] = wsId;
                }
            }

            Debug.Log(LogPrefix + "Workstation assignments loaded. count=" + _assignmentByEmployee.Count);
        }

        // ── Auto-fallback creation ────────────────────────────────────────────────

        /// <summary>
        /// Discovers CashDesk instances in the scene and creates a CashierStation
        /// near each one that doesn't already have one nearby.
        /// This allows employees to be assigned to cash desks even when the scene
        /// designer hasn't manually placed EmployeeWorkstation components.
        /// </summary>
        public void EnsureFallbackCashierStations()
        {
#if UNITY_2022_2_OR_NEWER
            CashDesk[] desks = Object.FindObjectsByType<CashDesk>(FindObjectsSortMode.None);
#else
            CashDesk[] desks = Object.FindObjectsOfType<CashDesk>();
#endif

            int created = 0;
            for (int i = 0; i < desks.Length; i++)
            {
                CashDesk desk = desks[i];
                if (desk == null)
                    continue;

                string fallbackId = "cashier_station_" + i;
                if (_all.ContainsKey(fallbackId))
                    continue;

                // Check whether a manually placed station already covers this desk.
                bool hasCashierNearby = false;
                foreach (EmployeeWorkstation existing in _all.Values)
                {
                    if (existing != null
                        && existing.workstationType == EmployeeWorkstationType.Cashier
                        && Vector3.Distance(existing.StandPosition, desk.transform.position) < CashierCoverageRadius)
                    {
                        hasCashierNearby = true;
                        break;
                    }
                }

                if (hasCashierNearby)
                    continue;

                // Create a child GameObject on the CashDesk.
                GameObject stationGo = new GameObject("CashierStation_" + i);
                stationGo.transform.SetParent(desk.transform, false);
                // Stand slightly behind the desk, facing forward.
                stationGo.transform.localPosition = new Vector3(0f, 0f, CashierStationBackOffset);
                stationGo.transform.localRotation = Quaternion.identity;

                EmployeeWorkstation ws = stationGo.AddComponent<EmployeeWorkstation>();
                ws.workstationId   = fallbackId;
                ws.workstationType = EmployeeWorkstationType.Cashier;
                // lookTarget left null — NPC will use the workstation's forward direction.

                created++;
            }

            // Also ensure at least one Restocker station near the origin if none exist.
            if (GetByType(EmployeeWorkstationType.Restocker).Count == 0)
            {
                GameObject restockGo = new GameObject("RestockerStation_0");
                restockGo.transform.SetParent(transform, false);
                restockGo.transform.position = transform.position + RestockerFallbackOffset;

                EmployeeWorkstation ws = restockGo.AddComponent<EmployeeWorkstation>();
                ws.workstationId   = "restocker_station_0";
                ws.workstationType = EmployeeWorkstationType.Restocker;
                created++;
            }

            if (created > 0)
                Debug.Log(LogPrefix + "Created " + created + " fallback workstation(s).");
        }

        // ── Event handlers ────────────────────────────────────────────────────────

        private void OnDataLoaded()
        {
            // Workstation save data is loaded by EntrepreneurTreeSaveIntegration calling LoadFromJSON.
        }

        private void OnEmployeeRoleChanged(int employeeId, EmployeeRole role)
        {
            // When role changes, release old station and try auto-assign to new type.
            EmployeeWorkstation current = GetAssignedStation(employeeId);
            if (current != null)
            {
                EmployeeWorkstationType newType = RoleToStationType(role);
                if (current.workstationType != newType)
                {
                    ReleaseEmployee(employeeId);
                    string reason;
                    TryAutoAssign(employeeId, newType, out reason);
                }
            }
            else
            {
                // No station yet — try to auto-assign.
                EmployeeWorkstationType type = RoleToStationType(role);
                string reason;
                TryAutoAssign(employeeId, type, out reason);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static EmployeeWorkstationType RoleToStationType(EmployeeRole role)
        {
            switch (role)
            {
                case EmployeeRole.Restocker: return EmployeeWorkstationType.Restocker;
                default:                    return EmployeeWorkstationType.Cashier;
            }
        }
    }
}
