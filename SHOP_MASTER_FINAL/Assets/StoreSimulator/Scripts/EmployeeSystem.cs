//Adaptado por POMPIC 20100333
using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.AI;

namespace FLOBUK.StoreSimulator
{
    public enum EmployeeRole
    {
        None,
        Cashier,
        Restocker
    }

    [Serializable]
    public class EmployeeData
    {
        public int employeeNumber;
        public string displayName;
        public bool hired;
        public EmployeeRole role;
        public int assignedWorkstationId;
        public string assignedWorkstationType;
        public string assignedShelfId;
        public string assignedProductId;
        public int npcInstanceId;
        public long salaryPerDay;
        public float movementSpeedMultiplier;
        public float cashierSalesMultiplier;
        public string status;

        public EmployeeData(int number)
        {
            employeeNumber = number;
            displayName = "Empleado " + number;
            salaryPerDay = EmployeeSystem.BaseSalaryPerDay;
            movementSpeedMultiplier = 1f;
            cashierSalesMultiplier = 1f;
            status = "Sin puesto";
            assignedWorkstationType = string.Empty;
            assignedShelfId = string.Empty;
            assignedProductId = string.Empty;
        }
    }

    public class EmployeeNPCMarker : MonoBehaviour
    {
        public int employeeNumber;
    }

    public class EmployeeSystem : MonoBehaviour
    {
        public const int MaxEmployees = 18;
        public const long BaseSalaryPerDay = 6000;

        public static EmployeeSystem Instance { get; private set; }

        public static event Action onEmployeesChanged;

        private readonly Dictionary<int, EmployeeData> employees = new Dictionary<int, EmployeeData>();
        private readonly Dictionary<int, GameObject> employeeNpcs = new Dictionary<int, GameObject>();
        private float cashierTick;
        private float restockerTick;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureEmployeeRecords();
            DayCycleSystem.onDayFinished += ApplyDailySalary;
            EntrepreneurProgress.onProgressChanged += NotifyChanged;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            DayCycleSystem.onDayFinished -= ApplyDailySalary;
            EntrepreneurProgress.onProgressChanged -= NotifyChanged;
        }

        void Update()
        {
            if (DayCycleSystem.GetStoreOpenState() != StoreOpenState.Open)
                return;

            float speedMultiplier = Mathf.Max(0.1f, EntrepreneurProgress.GetEmployeeSpeedMultiplier());
            cashierTick += Time.deltaTime * speedMultiplier;
            restockerTick += Time.deltaTime * speedMultiplier;

            if (cashierTick >= 0.5f)
            {
                cashierTick = 0f;
                ProcessCashiers();
            }

            if (restockerTick >= 2f)
            {
                restockerTick = 0f;
                ProcessRestockers();
            }
        }

        public IReadOnlyList<EmployeeData> GetAllEmployees()
        {
            EnsureEmployeeRecords();
            return employees.Values.OrderBy(e => e.employeeNumber).ToList();
        }

        public bool HasUnlockedEmployees()
        {
            return EntrepreneurProgress.GetUnlockedEmployees().Count > 0;
        }

        public int GetUnlockedCount()
        {
            return EntrepreneurProgress.GetUnlockedEmployees().Count;
        }

        public int GetHiredCount()
        {
            return employees.Values.Count(e => e.hired);
        }

        public long GetDailyEmployeeSalaryCost()
        {
            return employees.Values.Where(e => e.hired).Sum(e => e.salaryPerDay);
        }

        public bool TryHireEmployee(int employeeNumber, out string message)
        {
            EmployeeData employee = GetEmployee(employeeNumber);
            if (employee == null)
            {
                message = "Empleado no encontrado.";
                return false;
            }

            if (!EntrepreneurProgress.IsEmployeeUnlocked(employeeNumber))
            {
                message = GetLockedEmployeeMessage(employeeNumber);
                return false;
            }

            if (employee.hired)
            {
                message = "Empleado ya contratado.";
                return false;
            }

            if (GetHiredCount() >= MaxEmployees)
            {
                message = "Límite de empleados alcanzado (18).";
                return false;
            }

            employee.hired = true;
            employee.role = EmployeeRole.None;
            employee.status = "Sin puesto";
            employee.salaryPerDay = BaseSalaryPerDay;
            employee.movementSpeedMultiplier = EntrepreneurProgress.GetEmployeeSpeedMultiplier();
            employee.cashierSalesMultiplier = EntrepreneurProgress.GetCashierSalesMultiplier();
            employee.assignedWorkstationId = 0;
            employee.assignedWorkstationType = string.Empty;
            employee.assignedShelfId = string.Empty;
            employee.assignedProductId = string.Empty;

            SpawnOrRestoreNpc(employee, true);
            NotifyChanged();
            message = "Empleado contratado, pero falta puesto disponible.";
            return true;
        }

        public bool TryAssignRole(int employeeNumber, EmployeeRole role, out string message)
        {
            EmployeeData employee = GetEmployee(employeeNumber);
            if (employee == null)
            {
                message = "Empleado no encontrado.";
                return false;
            }

            if (!employee.hired)
            {
                message = "Primero debes contratar al empleado.";
                return false;
            }

            employee.role = role;
            employee.movementSpeedMultiplier = EntrepreneurProgress.GetEmployeeSpeedMultiplier();
            employee.cashierSalesMultiplier = EntrepreneurProgress.GetCashierSalesMultiplier();

            switch (role)
            {
                case EmployeeRole.Cashier:
                    AssignCashDesk(employee);
                    if (employee.assignedWorkstationId == 0)
                    {
                        employee.status = "Esperando puesto disponible";
                        message = "Empleado contratado, pero falta puesto disponible.";
                    }
                    else
                    {
                        employee.status = "Caja asignada";
                        message = "Rol asignado: Cajero.";
                    }
                    break;
                case EmployeeRole.Restocker:
                    PrepareRestockerAssignment(employee);
                    message = employee.status == "Surtido asignado"
                        ? "Rol asignado: Surtidor."
                        : "Empleado contratado, pero falta puesto disponible.";
                    break;
                default:
                    employee.assignedWorkstationId = 0;
                    employee.assignedWorkstationType = string.Empty;
                    employee.assignedShelfId = string.Empty;
                    employee.assignedProductId = string.Empty;
                    employee.status = "Sin puesto";
                    message = "Rol quitado.";
                    break;
            }

            SpawnOrRestoreNpc(employee, true);
            NotifyChanged();
            return true;
        }

        public JSONNode SaveToJSON()
        {
            EnsureEmployeeRecords();

            JSONNode data = new JSONObject();
            JSONArray employeeArray = new JSONArray();

            foreach (EmployeeData employee in employees.Values.OrderBy(e => e.employeeNumber))
            {
                JSONNode element = new JSONObject();
                element["employeeNumber"] = employee.employeeNumber;
                element["displayName"] = employee.displayName;
                element["hired"] = employee.hired;
                element["role"] = (int)employee.role;
                element["assignedWorkstationId"] = employee.assignedWorkstationId;
                element["assignedWorkstationType"] = employee.assignedWorkstationType;
                element["assignedShelfId"] = employee.assignedShelfId;
                element["assignedProductId"] = employee.assignedProductId;
                element["npcInstanceId"] = employee.npcInstanceId;
                element["salaryPerDay"] = employee.salaryPerDay;
                element["movementSpeedMultiplier"] = employee.movementSpeedMultiplier;
                element["cashierSalesMultiplier"] = employee.cashierSalesMultiplier;
                element["status"] = employee.status;
                employeeArray.Add(element);
            }

            data["employees"] = employeeArray;
            data["dailySalaryCost"] = GetDailyEmployeeSalaryCost();
            return data;
        }

        public void LoadFromJSON(JSONNode data)
        {
            EnsureEmployeeRecords();
            RestoreExistingNpcMap();

            foreach (EmployeeData employee in employees.Values)
            {
                employee.hired = false;
                employee.role = EmployeeRole.None;
                employee.assignedWorkstationId = 0;
                employee.assignedWorkstationType = string.Empty;
                employee.assignedShelfId = string.Empty;
                employee.assignedProductId = string.Empty;
                employee.npcInstanceId = 0;
                employee.salaryPerDay = BaseSalaryPerDay;
                employee.movementSpeedMultiplier = EntrepreneurProgress.GetEmployeeSpeedMultiplier();
                employee.cashierSalesMultiplier = EntrepreneurProgress.GetCashierSalesMultiplier();
                employee.status = "Sin puesto";
            }

            if (data != null && data.Count > 0)
            {
                JSONArray employeeArray = data["employees"].AsArray;
                if (employeeArray != null)
                {
                    for (int i = 0; i < employeeArray.Count; i++)
                    {
                        JSONNode element = employeeArray[i];
                        int employeeNumber = element["employeeNumber"].AsInt;
                        EmployeeData employee = GetEmployee(employeeNumber);
                        if (employee == null)
                            continue;

                        employee.displayName = string.IsNullOrWhiteSpace(element["displayName"].Value) ? employee.displayName : element["displayName"].Value;
                        employee.hired = element["hired"].AsBool;
                        employee.role = (EmployeeRole)element["role"].AsInt;
                        employee.assignedWorkstationId = element["assignedWorkstationId"].AsInt;
                        employee.assignedWorkstationType = element["assignedWorkstationType"].Value;
                        employee.assignedShelfId = element["assignedShelfId"].Value;
                        employee.assignedProductId = element["assignedProductId"].Value;
                        employee.npcInstanceId = element["npcInstanceId"].AsInt;
                        employee.salaryPerDay = element["salaryPerDay"].AsLong > 0 ? element["salaryPerDay"].AsLong : BaseSalaryPerDay;
                        employee.movementSpeedMultiplier = element["movementSpeedMultiplier"].AsFloat > 0f ? element["movementSpeedMultiplier"].AsFloat : EntrepreneurProgress.GetEmployeeSpeedMultiplier();
                        employee.cashierSalesMultiplier = element["cashierSalesMultiplier"].AsFloat > 0f ? element["cashierSalesMultiplier"].AsFloat : EntrepreneurProgress.GetCashierSalesMultiplier();
                        employee.status = string.IsNullOrWhiteSpace(element["status"].Value) ? "Sin puesto" : element["status"].Value;
                    }
                }
            }

            foreach (EmployeeData employee in employees.Values.Where(e => e.hired))
            {
                SpawnOrRestoreNpc(employee, false);

                if (employee.role == EmployeeRole.Cashier)
                    AssignCashDesk(employee);
                else if (employee.role == EmployeeRole.Restocker)
                    PrepareRestockerAssignment(employee);
            }

            CleanupOrphanEmployeeNpcs();

            NotifyChanged();
        }

        public string GetLockedEmployeeMessage(int employeeNumber)
        {
            string nodeId = "empleado_" + employeeNumber;
            EntrepreneurTreeNodeDefinition node = EntrepreneurTreeDefinitions.Get(nodeId);
            if (node == null)
                return "Empleado bloqueado. Desbloquea el nodo requerido en el Árbol.";

            return "Empleado bloqueado. Desbloquea " + node.DisplayName + " en el Árbol.";
        }

        private void ProcessCashiers()
        {
            foreach (EmployeeData employee in employees.Values)
            {
                if (!employee.hired || employee.role != EmployeeRole.Cashier)
                    continue;

                AssignCashDesk(employee);
                MoveEmployeeToWorkstation(employee);

                if (employee.assignedWorkstationId == 0)
                {
                    employee.status = "Esperando puesto disponible";
                    continue;
                }

                CashDesk cashDesk = FindObjectByInstanceId<CashDesk>(employee.assignedWorkstationId);
                if (cashDesk == null)
                {
                    employee.assignedWorkstationId = 0;
                    employee.status = "Esperando puesto disponible";
                    continue;
                }

                bool processed = cashDesk.AutoProcessCurrentCustomer(EntrepreneurProgress.GetCashierSalesMultiplier());
                employee.status = processed ? "Caja asignada" : "Caja asignada";
            }
        }

        private void ProcessRestockers()
        {
            foreach (EmployeeData employee in employees.Values)
            {
                if (!employee.hired || employee.role != EmployeeRole.Restocker)
                    continue;

                if (TryRestockOne(employee))
                {
                    MoveEmployeeToWorkstation(employee);
                    continue;
                }

                if (string.IsNullOrEmpty(employee.assignedShelfId))
                    employee.status = "Esperando puesto disponible";
                else
                    employee.status = "Esperando puesto disponible";
            }
        }

        private bool TryRestockOne(EmployeeData employee)
        {
            PackageObject[] packages = FindObjectsByType<PackageObject>(FindObjectsSortMode.None);
            if (packages == null || packages.Length == 0)
            {
                employee.status = "Esperando puesto disponible";
                return false;
            }

            PlacementObject[] placements = FindObjectsByType<PlacementObject>(FindObjectsSortMode.None);
            if (placements == null || placements.Length == 0)
            {
                employee.status = "Esperando puesto disponible";
                return false;
            }

            foreach (PackageObject package in packages)
            {
                ProductScriptableObject product = package.purchasable as ProductScriptableObject;
                if (product == null || package.count <= 0)
                    continue;

                PlacementObject targetPlacement = placements
                    .Where(p => p != null && p.storageType == product.storageType)
                    .OrderBy(p => p.product == product ? 0 : 1)
                    .ThenBy(p => p.count)
                    .FirstOrDefault(p =>
                        (p.product == product || p.IsEmpty()) &&
                        p.IsPlaceable(product));

                if (targetPlacement == null)
                    continue;

                Vector3 targetPosition = targetPlacement.Add(product);
                Transform packageItem = package.Remove();
                if (packageItem != null)
                {
                    packageItem.SetParent(targetPlacement.container, true);
                    packageItem.localPosition = targetPosition;
                    packageItem.localRotation = Quaternion.Euler(0, targetPlacement.orientation, 0);
                }

                employee.assignedShelfId = targetPlacement.GetInstanceID().ToString();
                employee.assignedProductId = product.id;
                employee.status = "Surtido asignado";
                return true;
            }

            employee.status = "Esperando puesto disponible";
            return false;
        }

        private void AssignCashDesk(EmployeeData employee)
        {
            CashDesk assigned = FindObjectByInstanceId<CashDesk>(employee.assignedWorkstationId);
            if (assigned != null)
                return;

            HashSet<int> usedDeskIds = new HashSet<int>(
                employees.Values
                    .Where(e => e.hired && e.employeeNumber != employee.employeeNumber && e.role == EmployeeRole.Cashier && e.assignedWorkstationId != 0)
                    .Select(e => e.assignedWorkstationId));

            CashDesk[] cashDesks = FindObjectsByType<CashDesk>(FindObjectsSortMode.None);
            CashDesk freeDesk = cashDesks.FirstOrDefault(d => !usedDeskIds.Contains(d.GetInstanceID()));
            if (freeDesk == null)
            {
                employee.assignedWorkstationId = 0;
                employee.assignedWorkstationType = "CashDesk";
                return;
            }

            employee.assignedWorkstationId = freeDesk.GetInstanceID();
            employee.assignedWorkstationType = "CashDesk";
        }

        private void PrepareRestockerAssignment(EmployeeData employee)
        {
            PlacementObject assignedShelf = FindShelfById(employee.assignedShelfId);
            if (assignedShelf == null)
            {
                PlacementObject[] placements = FindObjectsByType<PlacementObject>(FindObjectsSortMode.None);
                assignedShelf = placements.FirstOrDefault();
            }

            if (assignedShelf == null)
            {
                employee.assignedShelfId = string.Empty;
                employee.assignedProductId = string.Empty;
                employee.assignedWorkstationId = 0;
                employee.assignedWorkstationType = "Restock";
                employee.status = "Esperando puesto disponible";
                return;
            }

            employee.assignedShelfId = assignedShelf.GetInstanceID().ToString();
            employee.assignedProductId = assignedShelf.product != null ? assignedShelf.product.id : employee.assignedProductId;
            employee.assignedWorkstationId = assignedShelf.GetInstanceID();
            employee.assignedWorkstationType = "Restock";
            employee.status = assignedShelf.product != null ? "Surtido asignado" : "Esperando puesto disponible";
        }

        private void SpawnOrRestoreNpc(EmployeeData employee, bool forceRefresh)
        {
            if (!employee.hired)
                return;

            if (employeeNpcs.TryGetValue(employee.employeeNumber, out GameObject existing) && existing != null)
            {
                employee.npcInstanceId = existing.GetInstanceID();
                if (forceRefresh)
                    MoveEmployeeToWorkstation(employee);
                return;
            }

            EmployeeNPCMarker[] existingMarkers = FindObjectsByType<EmployeeNPCMarker>(FindObjectsSortMode.None);
            EmployeeNPCMarker marker = existingMarkers.FirstOrDefault(m => m.employeeNumber == employee.employeeNumber);
            if (marker != null)
            {
                employeeNpcs[employee.employeeNumber] = marker.gameObject;
                employee.npcInstanceId = marker.gameObject.GetInstanceID();
                if (forceRefresh)
                    MoveEmployeeToWorkstation(employee);
                return;
            }

            GameObject prefab = ResolveEmployeePrefab();
            Vector3 spawnPosition = ResolveSafeSpawn(employee.employeeNumber);
            GameObject npc = prefab != null
                ? Instantiate(prefab, spawnPosition, Quaternion.identity)
                : GameObject.CreatePrimitive(PrimitiveType.Capsule);

            if (prefab == null)
                npc.transform.position = spawnPosition;

            npc.name = "EmployeeNPC_" + employee.employeeNumber;
            DisableCustomerBehaviour(npc);

            marker = npc.GetComponent<EmployeeNPCMarker>();
            if (marker == null)
                marker = npc.AddComponent<EmployeeNPCMarker>();
            marker.employeeNumber = employee.employeeNumber;

            employeeNpcs[employee.employeeNumber] = npc;
            employee.npcInstanceId = npc.GetInstanceID();
            MoveEmployeeToWorkstation(employee);
        }

        private void MoveEmployeeToWorkstation(EmployeeData employee)
        {
            if (!employeeNpcs.TryGetValue(employee.employeeNumber, out GameObject npc) || npc == null)
                return;

            Vector3 targetPosition = npc.transform.position;

            if (employee.role == EmployeeRole.Cashier)
            {
                CashDesk cashDesk = FindObjectByInstanceId<CashDesk>(employee.assignedWorkstationId);
                if (cashDesk != null)
                    targetPosition = cashDesk.transform.position;
            }
            else if (employee.role == EmployeeRole.Restocker)
            {
                PlacementObject shelf = FindShelfById(employee.assignedShelfId);
                if (shelf != null)
                    targetPosition = shelf.transform.position;
            }

            NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.speed = 2.5f * EntrepreneurProgress.GetEmployeeSpeedMultiplier();
                agent.SetDestination(targetPosition);
            }
            else
            {
                npc.transform.position = targetPosition;
            }
        }

        private void DisableCustomerBehaviour(GameObject npc)
        {
            Customer customer = npc.GetComponent<Customer>();
            if (customer != null)
                customer.enabled = false;

            CustomerCart customerCart = npc.GetComponent<CustomerCart>();
            if (customerCart != null)
                customerCart.enabled = false;

            CustomerAgent customerAgent = npc.GetComponent<CustomerAgent>();
            if (customerAgent != null)
                customerAgent.enabled = false;
        }

        private GameObject ResolveEmployeePrefab()
        {
            if (CustomerSystem.Instance != null && CustomerSystem.Instance.customerPrefabs != null)
            {
                for (int i = 0; i < CustomerSystem.Instance.customerPrefabs.Length; i++)
                {
                    if (CustomerSystem.Instance.customerPrefabs[i] != null)
                        return CustomerSystem.Instance.customerPrefabs[i];
                }
            }

            return null;
        }

        private Vector3 ResolveSafeSpawn(int employeeNumber)
        {
            Vector3 basePosition = StoreDatabase.Instance != null && StoreDatabase.Instance.storeEntry != null
                ? StoreDatabase.Instance.storeEntry.position
                : Vector3.zero;

            Vector3 offset = new Vector3((employeeNumber % 3) * 1.2f, 0f, -2f - ((employeeNumber / 3) * 1.2f));
            Vector3 target = basePosition + offset;

            if (NavMesh.SamplePosition(target, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                return hit.position;

            return target;
        }

        private void ApplyDailySalary()
        {
            long salaryCost = GetDailyEmployeeSalaryCost();
            if (salaryCost <= 0)
                return;

            StoreDatabase.AddRemoveMoney(-salaryCost);
        }

        private void RestoreExistingNpcMap()
        {
            employeeNpcs.Clear();
            EmployeeNPCMarker[] markers = FindObjectsByType<EmployeeNPCMarker>(FindObjectsSortMode.None);
            for (int i = 0; i < markers.Length; i++)
            {
                if (markers[i] == null)
                    continue;

                employeeNpcs[markers[i].employeeNumber] = markers[i].gameObject;
            }
        }

        private void CleanupOrphanEmployeeNpcs()
        {
            List<int> toRemove = new List<int>();
            foreach (KeyValuePair<int, GameObject> pair in employeeNpcs)
            {
                EmployeeData employee = GetEmployee(pair.Key);
                if (employee != null && employee.hired)
                    continue;

                if (pair.Value != null)
                    Destroy(pair.Value);

                toRemove.Add(pair.Key);
            }

            for (int i = 0; i < toRemove.Count; i++)
                employeeNpcs.Remove(toRemove[i]);
        }

        private EmployeeData GetEmployee(int number)
        {
            EnsureEmployeeRecords();
            employees.TryGetValue(number, out EmployeeData employee);
            return employee;
        }

        private void EnsureEmployeeRecords()
        {
            for (int i = 1; i <= MaxEmployees; i++)
            {
                if (!employees.ContainsKey(i))
                    employees[i] = new EmployeeData(i);
            }
        }

        private PlacementObject FindShelfById(string shelfId)
        {
            if (string.IsNullOrWhiteSpace(shelfId))
                return null;

            if (!int.TryParse(shelfId, out int instanceId))
                return null;

            return FindObjectByInstanceId<PlacementObject>(instanceId);
        }

        private static T FindObjectByInstanceId<T>(int instanceId) where T : UnityEngine.Object
        {
            if (instanceId == 0)
                return null;

            T[] objects = UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None);
            return objects.FirstOrDefault(obj => obj != null && obj.GetInstanceID() == instanceId);
        }

        private void NotifyChanged()
        {
            onEmployeesChanged?.Invoke();
        }
    }

    public static class EmployeeSystemBootstrap
    {
        public static void EnsureInScene()
        {
            if (EmployeeSystem.Instance != null)
                return;

            EmployeeSystem existing = UnityEngine.Object.FindFirstObjectByType<EmployeeSystem>();
            if (existing != null)
                return;

            GameObject host = GameObject.Find("GameSystems");
            if (host == null)
                host = new GameObject("GameSystems");

            host.AddComponent<EmployeeSystem>();
        }
    }
}
