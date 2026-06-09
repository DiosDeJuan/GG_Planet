//Adaptado por POMPIC 20100333
using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.AI;

namespace FLOBUK.StoreSimulator
{
    public class EmployeeManager : MonoBehaviour
    {
        public const long DefaultDailySalary = 6000;

        public static event Action onEmployeesChanged;
        public static EmployeeManager Instance { get; private set; }

        private readonly Dictionary<string, EmployeeState> states = new Dictionary<string, EmployeeState>();
        private readonly Dictionary<string, EmployeeRuntimeAgent> agents = new Dictionary<string, EmployeeRuntimeAgent>();
        private int lastSalaryChargedDay;

        public static IReadOnlyList<EntrepreneurTreeNodeDefinition> EmployeeDefinitions =>
            EntrepreneurTreeDefinitions.Nodes.Where(node => node.Type == EntrepreneurTreeNodeType.Employee).ToList();

        public static EmployeeManager EnsureInstance()
        {
            if (Instance != null)
                return Instance;

            EmployeeManager existing = FindFirstObjectByType<EmployeeManager>();
            if (existing != null)
                return existing;

            GameObject obj = new GameObject("EmployeeManager");
            return obj.AddComponent<EmployeeManager>();
        }

        public static void ResetRuntimeState()
        {
            if (Instance == null)
                return;

            Instance.states.Clear();
            Instance.ClearAgents();
            Instance.lastSalaryChargedDay = 0;
            onEmployeesChanged?.Invoke();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DayCycleSystem.onDayFinished += ChargeDailySalaries;
        }

        public EmployeeState GetState(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
                return null;

            if (!states.TryGetValue(employeeId, out EmployeeState state))
            {
                state = new EmployeeState(employeeId);
                states.Add(employeeId, state);
            }

            return state;
        }

        public bool HasAnyUnlockedEmployee()
        {
            return EmployeeDefinitions.Any(node => EntrepreneurProgress.IsUnlocked(node.Id));
        }

        public bool TryHire(string employeeId, out string message)
        {
            EntrepreneurTreeNodeDefinition definition = EntrepreneurTreeDefinitions.Get(employeeId);
            EmployeeState state = GetState(employeeId);

            if (definition == null || definition.Type != EntrepreneurTreeNodeType.Employee)
            {
                message = "Empleado bloqueado. Falta desbloquear: nodo de empleado.";
                return false;
            }

            if (!EntrepreneurProgress.IsUnlocked(employeeId))
            {
                List<string> missing = EntrepreneurProgress.GetMissingPrerequisites(definition);
                string required = missing.Count > 0 ? string.Join(", ", missing) : definition.Title;
                message = "Empleado bloqueado. Falta desbloquear: " + required + ".";
                return false;
            }

            if (state.isHired)
            {
                message = "Este empleado ya esta contratado.";
                return false;
            }

            state.isHired = true;
            state.role = EmployeeRole.None;
            state.assignedWorkstationId = string.Empty;
            SpawnEmployee(state);
            message = "Empleado contratado correctamente.";
            onEmployeesChanged?.Invoke();
            return true;
        }

        public bool TryAssignRole(string employeeId, EmployeeRole role, out string message)
        {
            EmployeeState state = GetState(employeeId);
            if (state == null || !state.isHired)
            {
                message = "Empleado bloqueado. Falta contratarlo desde la app Empleados.";
                return false;
            }

            ReleaseCurrentWorkstation(state);
            state.role = role;
            state.assignedWorkstationId = string.Empty;

            if (role == EmployeeRole.Cashier && !TryAssignCashDesk(state, out message))
            {
                state.role = EmployeeRole.None;
                onEmployeesChanged?.Invoke();
                return false;
            }

            if (role == EmployeeRole.Restocker)
                message = "Rol Surtidor asignado.";
            else if (role == EmployeeRole.Cashier)
                message = "Rol Cajero asignado.";
            else
                message = "Rol actualizado.";

            SpawnEmployee(state);
            onEmployeesChanged?.Invoke();
            return true;
        }

        public JSONNode SaveToJSON()
        {
            JSONNode data = new JSONObject();
            data["lastSalaryChargedDay"] = lastSalaryChargedDay;

            JSONArray employees = new JSONArray();
            int index = 0;
            foreach (EmployeeState state in states.Values.OrderBy(state => state.employeeId))
            {
                if (!state.isHired)
                    continue;

                JSONNode item = new JSONObject();
                item["employeeId"] = state.employeeId;
                item["isHired"] = state.isHired;
                item["role"] = state.role.ToString();
                item["assignedWorkstationId"] = state.assignedWorkstationId ?? string.Empty;
                employees[index++] = item;
            }

            data["employees"] = employees;
            return data;
        }

        public void LoadFromJSON(JSONNode data)
        {
            states.Clear();
            ClearAgents();
            lastSalaryChargedDay = 0;

            if (data != null && data.Count > 0)
            {
                lastSalaryChargedDay = data["lastSalaryChargedDay"].AsInt;
                JSONArray employees = data["employees"].AsArray;
                for (int i = 0; i < employees.Count; i++)
                {
                    string employeeId = employees[i]["employeeId"].Value;
                    if (string.IsNullOrEmpty(employeeId))
                        continue;

                    EmployeeState state = GetState(employeeId);
                    state.isHired = employees[i]["isHired"].AsBool;
                    if (!Enum.TryParse(employees[i]["role"].Value, out EmployeeRole role))
                        role = EmployeeRole.None;

                    state.role = role;
                    state.assignedWorkstationId = employees[i]["assignedWorkstationId"].Value;
                }
            }

            foreach (EmployeeState state in states.Values.Where(state => state.isHired))
            {
                if (state.role == EmployeeRole.Cashier && !TryRestoreCashDesk(state))
                    state.assignedWorkstationId = string.Empty;

                SpawnEmployee(state);
            }

            onEmployeesChanged?.Invoke();
        }

        public bool TryRestockOne(out string message, out Vector3 targetPosition)
        {
            targetPosition = Vector3.zero;
            PlacementObject[] placements = FindObjectsByType<PlacementObject>(FindObjectsSortMode.None);
            PackageObject[] packages = FindObjectsByType<PackageObject>(FindObjectsSortMode.None);

            bool foundAssignedPlacement = false;
            bool foundPackageStock = false;
            foreach (PlacementObject placement in placements)
            {
                if (placement == null || placement.product == null)
                    continue;

                foundAssignedPlacement = true;
                ProductScriptableObject product = placement.product;
                if (!placement.IsPlaceable(product))
                    continue;

                PackageObject package = packages.FirstOrDefault(candidate =>
                    candidate != null &&
                    candidate.purchasable == product &&
                    candidate.count > 0 &&
                    candidate.purchasable is ProductScriptableObject packageProduct &&
                    packageProduct.storageType == placement.storageType);

                if (package == null)
                    continue;

                foundPackageStock = true;
                Vector3 localPosition = placement.Add(product);
                Quaternion targetRotation = Quaternion.Euler(0, placement.orientation, 0);
                Transform item = package.Remove();
                targetPosition = placement.container.TransformPoint(localPosition);
                InteractionSystem.MoveToTargetArc(item, placement.container, localPosition, targetRotation);
                message = "Rol Surtidor asignado.";
                return true;
            }

            if (!foundAssignedPlacement)
                message = "Este mueble no tiene producto asignado.";
            else if (!foundPackageStock)
                message = "No hay productos disponibles en almacen para surtir.";
            else
                message = "No se encontro un puesto valido para este empleado.";

            return false;
        }

        public EmployeeRole GetRole(string employeeId)
        {
            EmployeeState state = GetState(employeeId);
            return state != null ? state.role : EmployeeRole.None;
        }

        public Transform GetAssignedWorkstationTransform(string employeeId)
        {
            EmployeeState state = GetState(employeeId);
            if (state == null || string.IsNullOrEmpty(state.assignedWorkstationId))
                return null;

            CashDesk desk = FindObjectsByType<CashDesk>(FindObjectsSortMode.None)
                .FirstOrDefault(candidate => candidate.GetWorkstationId() == state.assignedWorkstationId);
            return desk != null ? desk.transform : null;
        }

        private bool TryAssignCashDesk(EmployeeState state, out string message)
        {
            CashDesk[] desks = FindObjectsByType<CashDesk>(FindObjectsSortMode.None);
            if (desks.Length == 0)
            {
                message = "No hay cajas registradoras disponibles.";
                return false;
            }

            foreach (CashDesk desk in desks)
            {
                if (desk.HasAutomaticCashier() && desk.assignedEmployeeId != state.employeeId)
                    continue;

                if (desk.AssignAutomaticCashier(state.employeeId))
                {
                    state.assignedWorkstationId = desk.GetWorkstationId();
                    message = "Rol Cajero asignado.";
                    return true;
                }
            }

            message = "No hay cajas registradoras disponibles.";
            return false;
        }

        private bool TryRestoreCashDesk(EmployeeState state)
        {
            CashDesk[] desks = FindObjectsByType<CashDesk>(FindObjectsSortMode.None);
            CashDesk preferred = desks.FirstOrDefault(desk => desk.GetWorkstationId() == state.assignedWorkstationId);
            if (preferred != null && preferred.AssignAutomaticCashier(state.employeeId))
                return true;

            foreach (CashDesk desk in desks)
            {
                if (desk.AssignAutomaticCashier(state.employeeId))
                {
                    state.assignedWorkstationId = desk.GetWorkstationId();
                    return true;
                }
            }

            return false;
        }

        private void ReleaseCurrentWorkstation(EmployeeState state)
        {
            if (state == null || state.role != EmployeeRole.Cashier)
                return;

            foreach (CashDesk desk in FindObjectsByType<CashDesk>(FindObjectsSortMode.None))
                desk.ReleaseAutomaticCashier(state.employeeId);
        }

        private void SpawnEmployee(EmployeeState state)
        {
            if (state == null || !state.isHired)
                return;

            if (agents.TryGetValue(state.employeeId, out EmployeeRuntimeAgent existing) && existing != null)
            {
                existing.Configure(state.employeeId);
                return;
            }

            GameObject prefab = GetEmployeeVisualPrefab(state.employeeId);
            Vector3 position = GetSpawnPosition(state.employeeId);
            GameObject obj = prefab != null ? Instantiate(prefab, position, Quaternion.identity) : new GameObject(state.employeeId);
            obj.name = "Employee NPC - " + EntrepreneurTreeDefinitions.GetTitle(state.employeeId);
            StripCustomerBehaviour(obj);
            EmployeeRuntimeAgent agent = obj.GetComponent<EmployeeRuntimeAgent>();
            if (agent == null)
                agent = obj.AddComponent<EmployeeRuntimeAgent>();

            agent.Configure(state.employeeId);
            agents[state.employeeId] = agent;
        }

        private GameObject GetEmployeeVisualPrefab(string employeeId)
        {
            if (CustomerSystem.Instance == null || CustomerSystem.Instance.customerPrefabs == null || CustomerSystem.Instance.customerPrefabs.Length == 0)
                return null;

            int index = Mathf.Abs(employeeId.GetHashCode()) % CustomerSystem.Instance.customerPrefabs.Length;
            return CustomerSystem.Instance.customerPrefabs[index];
        }

        private Vector3 GetSpawnPosition(string employeeId)
        {
            Transform workstation = GetAssignedWorkstationTransform(employeeId);
            if (workstation != null)
                return workstation.position + workstation.right * 1.5f;

            if (CustomerSystem.Instance != null && CustomerSystem.Instance.spawnLocations != null && CustomerSystem.Instance.spawnLocations.Length > 0)
                return CustomerSystem.Instance.spawnLocations[0].position;

            if (StoreDatabase.Instance != null && StoreDatabase.Instance.storeEntry != null)
                return StoreDatabase.Instance.storeEntry.position + Vector3.right * 2f;

            return Vector3.zero;
        }

        private static void StripCustomerBehaviour(GameObject obj)
        {
            Customer customer = obj.GetComponent<Customer>();
            if (customer != null)
                customer.enabled = false;

            CustomerCart cart = obj.GetComponent<CustomerCart>();
            if (cart != null)
                cart.enabled = false;

            CustomerAgent agent = obj.GetComponent<CustomerAgent>();
            if (agent != null)
                agent.enabled = false;

            NavMeshAgent navAgent = obj.GetComponent<NavMeshAgent>();
            if (navAgent != null)
                navAgent.enabled = true;
        }

        private void ClearAgents()
        {
            foreach (EmployeeRuntimeAgent agent in agents.Values)
            {
                if (agent != null)
                    Destroy(agent.gameObject);
            }

            agents.Clear();
        }

        private void ChargeDailySalaries()
        {
            int salaryDay = Mathf.Max(1, DayCycleSystem.Instance.currentDay - 1);
            if (lastSalaryChargedDay == salaryDay)
                return;

            int hiredCount = states.Values.Count(state => state.isHired);
            if (hiredCount <= 0)
                return;

            long total = hiredCount * DefaultDailySalary;
            lastSalaryChargedDay = salaryDay;
            if (StatsDatabase.Instance != null)
                StatsDatabase.Instance.RegisterEmployeeSalary(total);

            StoreDatabase.AddRemoveMoney(-total);
            UIGame.AddNotification("Salarios de empleados: " + StoreDatabase.FromLongToStringMoney(total) + ".");
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            DayCycleSystem.onDayFinished -= ChargeDailySalaries;
        }
    }
}
