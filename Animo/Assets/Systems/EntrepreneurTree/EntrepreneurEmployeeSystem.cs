using System.Collections.Generic;
using System;
using SimpleJSON;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    public enum EmployeeRole
    {
        None = 0,
        Cashier = 1,
        Restocker = 2
    }

    [Serializable]
    public class EmployeeAssignment
    {
        public int employeeId;
        public bool isHired;
        public EmployeeRole role;
        public long hireCost;
    }

    /// <summary>
    /// Runtime employee roster gated by Entrepreneur Tree unlocks.
    /// Unlocking tree employees enables hiring here but does not auto-hire.
    /// </summary>
    public class EntrepreneurEmployeeSystem : MonoBehaviour
    {
        private const string LogPrefix = "[EmployeeApp] ";
        public const int MaxEmployees = 18;

        public static EntrepreneurEmployeeSystem Instance { get; private set; }
        public static event Action<int> onEmployeeHired;
        public static event Action<int, EmployeeRole> onEmployeeRoleChanged;

        [Header("Hiring")]
        [Tooltip("When true, hiring checks and deducts money from StoreDatabase.")]
        public bool requireMoneyForHiring = true;

        [Tooltip("Default cost in cents used when no explicit per-employee cost is configured.")]
        public long defaultHireCost = 25000;

        [Tooltip("Optional per-employee hiring costs in cents (index 0 == employee_1).")]
        public List<long> employeeHireCosts = new List<long>();

        private readonly Dictionary<int, EmployeeAssignment> assignments = new Dictionary<int, EmployeeAssignment>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            EnsureRosterInitialized();
            SaveGameSystem.dataLoadEvent += OnDataLoaded;
        }


        public IReadOnlyCollection<EmployeeAssignment> GetAssignments()
        {
            return assignments.Values;
        }


        public EmployeeAssignment GetAssignment(int employeeId)
        {
            EmployeeAssignment assignment;
            return assignments.TryGetValue(employeeId, out assignment) ? assignment : null;
        }


        public bool IsEmployeeUnlocked(int employeeId)
        {
            if (employeeId < 1 || employeeId > MaxEmployees)
                return false;

            if (EntrepreneurTreeEmployeeUnlockAdapter.Instance != null)
                return EntrepreneurTreeEmployeeUnlockAdapter.Instance.IsEmployeeUnlocked(employeeId);

            return EntrepreneurTreeGameplayBridge.Instance != null &&
                   EntrepreneurTreeGameplayBridge.Instance.IsEmployeeUnlocked(employeeId);
        }


        public bool IsEmployeeHired(int employeeId)
        {
            EmployeeAssignment assignment = GetAssignment(employeeId);
            return assignment != null && assignment.isHired;
        }


        public EmployeeRole GetEmployeeRole(int employeeId)
        {
            EmployeeAssignment assignment = GetAssignment(employeeId);
            return assignment != null ? assignment.role : EmployeeRole.None;
        }


        public int GetHiredCount()
        {
            int count = 0;
            foreach (EmployeeAssignment assignment in assignments.Values)
            {
                if (assignment.isHired)
                    count++;
            }
            return count;
        }


        public int GetAssignedCount()
        {
            int count = 0;
            foreach (EmployeeAssignment assignment in assignments.Values)
            {
                if (assignment.isHired && assignment.role != EmployeeRole.None)
                    count++;
            }
            return count;
        }


        public int GetRoleCount(EmployeeRole role)
        {
            int count = 0;
            foreach (EmployeeAssignment assignment in assignments.Values)
            {
                if (assignment.isHired && assignment.role == role)
                    count++;
            }
            return count;
        }


        public bool CanHireEmployee(int employeeId, out string reason)
        {
            reason = string.Empty;
            EmployeeAssignment assignment = GetAssignment(employeeId);
            if (assignment == null)
            {
                reason = "Empleado desconocido.";
                return false;
            }

            if (!IsEmployeeUnlocked(employeeId))
            {
                reason = "Desbloquea este empleado en el Árbol del Emprendedor.";
                return false;
            }

            if (assignment.isHired)
            {
                reason = "Este empleado ya está contratado.";
                return false;
            }

            if (requireMoneyForHiring && !StoreDatabase.CanPurchase(assignment.hireCost))
            {
                reason = "No tienes dinero suficiente para contratarlo.";
                return false;
            }

            return true;
        }


        public bool TryHireEmployee(int employeeId, out string reason)
        {
            if (!CanHireEmployee(employeeId, out reason))
                return false;

            EmployeeAssignment assignment = GetAssignment(employeeId);
            if (assignment == null)
            {
                reason = "Empleado desconocido.";
                return false;
            }

            if (requireMoneyForHiring && assignment.hireCost > 0)
                StoreDatabase.AddRemoveMoney(-assignment.hireCost);

            assignment.isHired = true;
            if (assignment.role == EmployeeRole.None)
                assignment.role = EmployeeRole.Cashier;

            Debug.Log(LogPrefix + "Employee hired: #" + employeeId
                + " cost=" + StoreDatabase.FromLongToStringMoney(assignment.hireCost)
                + " role=" + assignment.role + ".");
            onEmployeeHired?.Invoke(employeeId);
            onEmployeeRoleChanged?.Invoke(employeeId, assignment.role);
            AchievementSystem.RegisterEmployeeHired(GetHiredCount(), GetAssignedCount(), MaxEmployees);
            UIGame.AddNotification("Empleado contratado: #" + employeeId, otherColor: new Color(0.20f, 0.75f, 0.35f));
            return true;
        }


        public bool TryAssignRole(int employeeId, EmployeeRole role, out string reason)
        {
            reason = string.Empty;
            EmployeeAssignment assignment = GetAssignment(employeeId);
            if (assignment == null)
            {
                reason = "Empleado desconocido.";
                return false;
            }

            if (!IsEmployeeUnlocked(employeeId))
            {
                reason = "Desbloquea este empleado en el Árbol del Emprendedor antes de asignar rol.";
                return false;
            }

            if (!assignment.isHired)
            {
                reason = "Debes contratar al empleado antes de asignar rol.";
                return false;
            }

            if (role == EmployeeRole.None)
            {
                reason = "Selecciona un rol válido.";
                return false;
            }

            if (assignment.role == role)
            {
                reason = "El empleado ya tiene ese rol.";
                return false;
            }

            assignment.role = role;
            Debug.Log(LogPrefix + "Employee role changed: #" + employeeId + " -> " + role + ".");
            onEmployeeRoleChanged?.Invoke(employeeId, role);
            AchievementSystem.RegisterAllEmployeesAssigned(GetHiredCount(), GetAssignedCount(), MaxEmployees);
            return true;
        }


        public float GetCashierSpeedMultiplier()
        {
            int cashiers = GetRoleCount(EmployeeRole.Cashier);
            return 1f + Mathf.Clamp(cashiers * 0.03f, 0f, 0.45f);
        }


        public float GetRestockerSpeedMultiplier()
        {
            int restockers = GetRoleCount(EmployeeRole.Restocker);
            return 1f + Mathf.Clamp(restockers * 0.03f, 0f, 0.45f);
        }


        public JSONNode SaveToJSON()
        {
            JSONNode data = new JSONObject();
            JSONArray array = new JSONArray();
            for (int employeeId = 1; employeeId <= MaxEmployees; employeeId++)
            {
                EmployeeAssignment assignment = GetAssignment(employeeId);
                if (assignment == null)
                    continue;

                JSONNode row = new JSONObject();
                row["employeeId"] = assignment.employeeId;
                row["isHired"] = assignment.isHired;
                row["role"] = (int)assignment.role;
                row["hireCost"] = assignment.hireCost;
                array.Add(row);
            }

            data["employees"] = array;
            return data;
        }


        public void LoadFromJSON(JSONNode data)
        {
            EnsureRosterInitialized();
            for (int employeeId = 1; employeeId <= MaxEmployees; employeeId++)
            {
                EmployeeAssignment assignment = GetAssignment(employeeId);
                if (assignment == null)
                    continue;

                assignment.isHired = false;
                assignment.role = EmployeeRole.None;
                assignment.hireCost = ResolveHireCost(employeeId);
            }

            if (data == null || data.Count == 0)
                return;

            JSONArray array = data["employees"].AsArray;
            for (int i = 0; i < array.Count; i++)
            {
                JSONNode row = array[i];
                int employeeId = row["employeeId"].AsInt;
                EmployeeAssignment assignment = GetAssignment(employeeId);
                if (assignment == null)
                    continue;

                assignment.isHired = row["isHired"].AsBool;
                assignment.role = SanitizeRole(row["role"].AsInt);
                assignment.hireCost = Math.Max(0L, row["hireCost"].AsLong);
            }

            Debug.Log(LogPrefix + "Employee roster loaded. hired=" + GetHiredCount()
                + ", cashiers=" + GetRoleCount(EmployeeRole.Cashier)
                + ", restockers=" + GetRoleCount(EmployeeRole.Restocker) + ".");
        }


        private void OnDataLoaded()
        {
            EnsureRosterInitialized();
        }


        private void EnsureRosterInitialized()
        {
            while (employeeHireCosts.Count < MaxEmployees)
                employeeHireCosts.Add(defaultHireCost);

            for (int employeeId = 1; employeeId <= MaxEmployees; employeeId++)
            {
                if (assignments.ContainsKey(employeeId))
                    continue;

                assignments.Add(employeeId, new EmployeeAssignment
                {
                    employeeId = employeeId,
                    isHired = false,
                    role = EmployeeRole.None,
                    hireCost = ResolveHireCost(employeeId),
                });
            }
        }


        private long ResolveHireCost(int employeeId)
        {
            if (employeeId < 1 || employeeId > MaxEmployees)
                return defaultHireCost;

            int index = GetHireCostIndex(employeeId);
            if (employeeHireCosts == null || employeeHireCosts.Count <= index)
                return defaultHireCost;

            return Math.Max(0L, employeeHireCosts[index]);
        }


        private static int GetHireCostIndex(int employeeId)
        {
            return employeeId - 1;
        }


        private static EmployeeRole SanitizeRole(int roleValue)
        {
            if (roleValue == (int)EmployeeRole.Cashier)
                return EmployeeRole.Cashier;
            if (roleValue == (int)EmployeeRole.Restocker)
                return EmployeeRole.Restocker;
            return EmployeeRole.None;
        }


        void OnDestroy()
        {
            SaveGameSystem.dataLoadEvent -= OnDataLoaded;
            if (Instance == this)
                Instance = null;
        }
    }
}
