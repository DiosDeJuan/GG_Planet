using System;
using System.IO;
using FLOBUK.StoreSimulator;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace FLOBUK.StoreSimulator.Editor
{
    [InitializeOnLoad]
    public static class ShopMasterEntrepreneurTreeAuditRunner
    {
        private const string StateKey = "ShopMaster.TreeAudit.State";
        private const string StartTimeKey = "ShopMaster.TreeAudit.StartTime";
        private const string FailureCountKey = "ShopMaster.TreeAudit.Failures";
        private const string LogPath = "Documentos/Unity_PlayMode_Auditoria_Arbol_Emprendedor.log";
        private const string GameScene = "Assets/StoreSimulator/Scenes/Game.unity";

        static ShopMasterEntrepreneurTreeAuditRunner()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            Application.logMessageReceived -= OnLogMessage;
            Application.logMessageReceived += OnLogMessage;
        }

        public static void Run()
        {
            File.WriteAllText(LogPath, "# ShopMaster Entrepreneur Tree Play Mode Audit\n");
            SessionState.SetInt(FailureCountKey, 0);
            SessionState.SetString(StateKey, "enter-play");
            SessionState.SetFloat(StartTimeKey, 0f);
            Log("Opening scene: " + GameScene);
            EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }

        private static void OnEditorUpdate()
        {
            string state = SessionState.GetString(StateKey, string.Empty);
            if (string.IsNullOrEmpty(state))
                return;

            if (state == "enter-play" && EditorApplication.isPlaying)
            {
                float start = SessionState.GetFloat(StartTimeKey, 0f);
                if (start <= 0f)
                {
                    SessionState.SetFloat(StartTimeKey, (float)EditorApplication.timeSinceStartup);
                    return;
                }

                if (EditorApplication.timeSinceStartup - start < 3d)
                    return;

                SessionState.SetString(StateKey, "leave-play");
                try
                {
                    RunSmokeAudit();
                }
                catch (Exception ex)
                {
                    Fail("Unhandled audit exception: " + ex);
                }

                EditorApplication.ExitPlaymode();
                return;
            }

            if (state == "leave-play" && !EditorApplication.isPlaying &&
                !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                int failures = SessionState.GetInt(FailureCountKey, 0);
                Log("Audit finished. Failures=" + failures);
                SessionState.EraseString(StateKey);
                SessionState.EraseInt(FailureCountKey);
                SessionState.EraseFloat(StartTimeKey);
                EditorApplication.Exit(failures == 0 ? 0 : 1);
            }
        }

        private static void RunSmokeAudit()
        {
            Log("Running integrated Play Mode smoke audit.");

            PassIf(UnityEngine.Object.FindAnyObjectByType<UIShopDesktop>() != null,
                "Computer UI exists in Game scene.");
            PassIf(UnityEngine.Object.FindAnyObjectByType<UpgradesUIController>(FindObjectsInactive.Include) != null,
                "Entrepreneur Tree UI controller is connected to the computer.");
            PassIf(UnityEngine.Object.FindAnyObjectByType<OrdersAppUIController>(FindObjectsInactive.Include) != null,
                "Orders app is connected to the computer.");
            PassIf(UnityEngine.Object.FindAnyObjectByType<EmployeeAppUIController>(FindObjectsInactive.Include) != null,
                "Employees app is connected to the computer.");
            PassIf(UnityEngine.Object.FindObjectsByType<NodeUI>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= 36,
                "Entrepreneur Tree rendered its runtime nodes.");

            EntrepreneurTreeManager tree = EntrepreneurTreeManager.Instance;
            EntrepreneurEmployeeSystem employees = EntrepreneurEmployeeSystem.Instance;
            EmployeeWorkstationRegistry stations = EmployeeWorkstationRegistry.Instance;
            EmployeeNPCSpawner spawner = EmployeeNPCSpawner.Instance;
            PassIf(tree != null, "EntrepreneurTreeManager exists.");
            PassIf(employees != null, "EntrepreneurEmployeeSystem exists.");
            PassIf(stations != null, "EmployeeWorkstationRegistry exists.");
            PassIf(spawner != null, "EmployeeNPCSpawner exists.");
            PassIf(ItemDatabase.Instance != null, "ItemDatabase exists.");
            PassIf(DeliverySystem.Instance != null, "DeliverySystem exists.");
            PassIf(StoreDatabase.Instance != null, "StoreDatabase exists.");

            if (tree == null || employees == null || stations == null || spawner == null ||
                ItemDatabase.Instance == null || DeliverySystem.Instance == null || StoreDatabase.Instance == null)
            {
                return;
            }

            PassIf(NavMesh.CalculateTriangulation().vertices.Length > 0,
                "Game scene has baked NavMesh data.");

            tree.LoadFromJSON(null);
            EntrepreneurTreeManager.SetPoints(100);
            PassIf(EntrepreneurTreeManager.IsNodeUnlocked("product_basic_1"),
                "Starter product node is unlocked by default.");
            int pointsBeforeRejectedUnlock = EntrepreneurTreeManager.GetAvailablePoints();
            PassIf(!EntrepreneurTreeManager.TryUnlockNode("employee_1"),
                "Tree rejects an employee node when prerequisite is missing.");
            PassIf(EntrepreneurTreeManager.GetAvailablePoints() == pointsBeforeRejectedUnlock,
                "Rejected unlock does not spend progress points.");
            PassIf(EntrepreneurTreeManager.TryUnlockNode("product_basic_2"),
                "Tree unlocks Productos Basicos 2.");
            PassIf(EntrepreneurTreeManager.TryUnlockNode("product_spices_1"),
                "Tree unlocks Especias 1.");
            PassIf(EntrepreneurTreeManager.TryUnlockNode("employee_1"),
                "Tree unlocks Empleado 1 after prerequisite.");

            var products = ItemDatabase.GetByType(typeof(ProductScriptableObject));
            PassIf(products.Count == 46, "Real catalog exposes 46 products.");
            bool validCatalog = true;
            for (int i = 0; i < products.Count; i++)
            {
                ProductScriptableObject product = products[i] as ProductScriptableObject;
                validCatalog &= product != null && !string.IsNullOrEmpty(product.id) &&
                                product.prefab != null && product.packageCount > 0 && product.buyPrice >= 0;
            }
            PassIf(validCatalog, "Every catalog product has ID, prefab, package count and non-negative buy price.");

            ProductScriptableObject starter = ItemDatabase.GetById(typeof(ProductScriptableObject), "0") as ProductScriptableObject;
            PassIf(EntrepreneurTreeGameplayBridge.Instance != null &&
                   EntrepreneurTreeGameplayBridge.Instance.IsProductUnlocked(starter),
                "Gameplay bridge exposes starter product as unlocked.");
            StoreDatabase.AddRemoveMoney(1000000L);
            int packagesBefore = UnityEngine.Object.FindObjectsByType<PackageObject>(FindObjectsSortMode.None).Length;
            long moneyBefore = StoreDatabase.Instance.currentMoney;
            bool purchased = DeliverySystem.Purchase(starter);
            int packagesAfter = UnityEngine.Object.FindObjectsByType<PackageObject>(FindObjectsSortMode.None).Length;
            PassIf(purchased, "Unlocked starter product purchase succeeds through DeliverySystem.");
            PassIf(packagesAfter == packagesBefore + 1, "Successful purchase creates one real delivery package.");
            PassIf(StoreDatabase.Instance.currentMoney == moneyBefore - starter.buyPrice * starter.packageCount,
                "Successful purchase deducts the exact package cost.");

            ShoplifterSystem shoplifters = ShoplifterSystem.Instance;
            PassIf(shoplifters != null, "ShoplifterSystem exists.");
            if (shoplifters != null)
            {
                float originalSpecialChance = shoplifters.specialBaseChance;
                float originalExpertChance = shoplifters.expertChance;
                float originalFastChance = shoplifters.fastChance;
                float originalSuspiciousChance = shoplifters.suspiciousChance;
                shoplifters.specialBaseChance = 0f;
                shoplifters.expertChance = 1f;
                shoplifters.fastChance = 0f;
                shoplifters.suspiciousChance = 0f;
                PassIf(InvokeChooseThiefType(shoplifters) == ShoplifterType.Common,
                    "Premium thief types stay disabled before premium product unlocks.");

                EntrepreneurTreeManager.AdminUnlockByType(TreeNodeType.Product);
                shoplifters.specialBaseChance = 1f;
                PassIf(InvokeChooseThiefType(shoplifters) == ShoplifterType.Special,
                    "Special thief type becomes available after premium product unlocks.");
                shoplifters.specialBaseChance = originalSpecialChance;
                shoplifters.expertChance = originalExpertChance;
                shoplifters.fastChance = originalFastChance;
                shoplifters.suspiciousChance = originalSuspiciousChance;
            }

            EntrepreneurTreeManager.AdminUnlockByType(TreeNodeType.Product);
            StoreDatabase.AddRemoveMoney(100000000L);
            bool allProductsPurchasable = true;
            for (int i = 0; i < products.Count; i++)
            {
                ProductScriptableObject product = products[i] as ProductScriptableObject;
                allProductsPurchasable &= product != null && DeliverySystem.Purchase(product);
            }
            PassIf(allProductsPurchasable,
                "Every real catalog product can be delivered after product tree unlocks.");

            string hireReason;
            PassIf(employees.TryHireEmployee(1, out hireReason), "Unlocked employee can be hired from employee backend.");
            PassIf(employees.IsEmployeeHired(1), "Hired employee is stored in roster.");
            GameObject employeeNpc = spawner.GetNPC(1);
            PassIf(employeeNpc != null && employeeNpc.activeInHierarchy, "Hired employee spawns a visible NPC.");
            PassIf(employeeNpc != null && employeeNpc.GetComponent<NavMeshAgent>() != null,
                "Employee NPC has NavMeshAgent.");
            PassIf(stations.GetAssignedStation(1) != null, "Hired cashier receives a resolvable workstation.");
            PassIf(!string.IsNullOrEmpty(employees.GetAssignment(1).workstationId),
                "Employee roster stores stable workstation ID.");

            string roleReason;
            PassIf(employees.TryAssignRole(1, EmployeeRole.Restocker, out roleReason),
                "Employee role changes to Restocker.");
            PassIf(stations.GetAssignedStation(1) != null &&
                   stations.GetAssignedStation(1).workstationType == EmployeeWorkstationType.Restocker,
                "Restocker receives a resolvable restocker workstation.");

            EntrepreneurTreeManager.AdminUnlockByType(TreeNodeType.Employee);
            EntrepreneurTreeManager.AdminUnlockByType(TreeNodeType.Security);
            EntrepreneurTreeSecurityAdapter.Instance?.RefreshFromTree();
            PassIf(EntrepreneurTreeSecurityAdapter.Instance != null &&
                   EntrepreneurTreeSecurityAdapter.Instance.GetCurrentSecurityLevel() == 3 &&
                   Mathf.Approximately(EntrepreneurTreeSecurityAdapter.Instance.GetAutomaticArrestChance(), 0.99f),
                "Security tree unlock activates level 3 and 99% automatic arrest chance.");

            EntrepreneurTreeManager.AdminUnlockByType(TreeNodeType.Improvement);
            PassIf(EntrepreneurTreeUpgradeAdapter.Instance != null &&
                   Mathf.Approximately(EntrepreneurTreeUpgradeAdapter.Instance.GetEmployeeSpeedMultiplier(), 1.10f),
                "Caffeine applies 10% employee speed multiplier.");
            PassIf(EntrepreneurTreeUpgradeAdapter.Instance != null &&
                   Mathf.Approximately(EntrepreneurTreeUpgradeAdapter.Instance.GetSalesMultiplier(), 1.05f),
                "Charismatic exposes 5% cashier sales multiplier.");

            var treeSnapshot = tree.SaveToJSON();
            var employeeSnapshot = employees.SaveToJSON();
            var stationSnapshot = stations.SaveToJSON();
            int savedPoints = EntrepreneurTreeManager.GetAvailablePoints();
            string savedStation = employees.GetAssignment(1).workstationId;
            tree.LoadFromJSON(null);
            employees.LoadFromJSON(null);
            stations.LoadFromJSON(null);
            tree.LoadFromJSON(treeSnapshot);
            employees.LoadFromJSON(employeeSnapshot);
            stations.LoadFromJSON(stationSnapshot);
            PassIf(EntrepreneurTreeManager.GetAvailablePoints() == savedPoints &&
                   EntrepreneurTreeManager.IsNodeUnlocked("upgrade_caffeine"),
                "Tree points and unlocked nodes survive serialization round-trip.");
            PassIf(employees.IsEmployeeHired(1) &&
                   employees.GetEmployeeRole(1) == EmployeeRole.Restocker &&
                   employees.GetAssignment(1).workstationId == savedStation,
                "Employee hire, role and workstation survive serialization round-trip.");

            Log("Integrated Play Mode smoke audit completed.");
        }

        private static ShoplifterType InvokeChooseThiefType(ShoplifterSystem shoplifters)
        {
            var method = typeof(ShoplifterSystem).GetMethod("ChooseThiefType",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return method != null ? (ShoplifterType)method.Invoke(shoplifters, null) : ShoplifterType.Special;
        }

        private static void OnLogMessage(string condition, string stackTrace, LogType type)
        {
            if (SessionState.GetString(StateKey, string.Empty) != "enter-play")
                return;

            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                Fail("Console " + type + ": " + condition + "\n" + stackTrace);
        }

        private static void PassIf(bool condition, string message)
        {
            if (condition)
                Log("PASS: " + message);
            else
                Fail(message);
        }

        private static void Fail(string message)
        {
            SessionState.SetInt(FailureCountKey, SessionState.GetInt(FailureCountKey, 0) + 1);
            Log("FAIL: " + message);
        }

        private static void Log(string message)
        {
            File.AppendAllText(LogPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + "\n");
        }
    }
}
