using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FLOBUK.StoreSimulator;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator.Editor
{
    [InitializeOnLoad]
    public static class ShopMasterEntrepreneurTreeGreenAuditRunner
    {
        private const string StateKey = "ShopMaster.TreeGreenAudit.State";
        private const string StartKey = "ShopMaster.TreeGreenAudit.Start";
        private const string FailureKey = "ShopMaster.TreeGreenAudit.Failures";
        private const string LogPath = "Documentos/Unity_PlayMode_Auditoria_Arbol_Emprendedor_Fase2.log";
        private const string ScenePath = "Assets/StoreSimulator/Scenes/Game.unity";

        private static CashDesk cashierDesk;
        private static long cashierMoneyBefore;
        private static long cashierExpectedIncome;

        static ShopMasterEntrepreneurTreeGreenAuditRunner()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            Application.logMessageReceived -= OnConsole;
            Application.logMessageReceived += OnConsole;
        }

        public static void Run()
        {
            File.WriteAllText(LogPath, "# ShopMaster Entrepreneur Tree Green Audit - Phase 2\n");
            SessionState.SetInt(FailureKey, 0);
            SessionState.SetString(StateKey, "enter");
            SessionState.SetFloat(StartKey, 0f);
            Log("Opening scene: " + ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }

        private static void OnUpdate()
        {
            string state = SessionState.GetString(StateKey, string.Empty);
            if (string.IsNullOrEmpty(state))
                return;

            if (state == "enter" && EditorApplication.isPlaying)
            {
                if (!Elapsed(4f))
                    return;
                try
                {
                    RunImmediate();
                    SetupCashierScenario();
                    SessionState.SetString(StateKey, "cashier");
                    SessionState.SetFloat(StartKey, (float)EditorApplication.timeSinceStartup);
                }
                catch (Exception ex)
                {
                    Fail("Unhandled phase-2 setup exception: " + ex);
                    LeavePlayMode();
                }
                return;
            }

            if (state == "cashier" && EditorApplication.isPlaying)
            {
                if (!Elapsed(8f))
                    return;
                try
                {
                    VerifyCashierScenario();
                    RunRestockScenario();
                    RunRobberyScenarios();
                    RunDiskPersistenceScenario();
                }
                catch (Exception ex)
                {
                    Fail("Unhandled phase-2 scenario exception: " + ex);
                }
                LeavePlayMode();
                return;
            }

            if (state == "leave" && !EditorApplication.isPlaying &&
                !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                int failures = SessionState.GetInt(FailureKey, 0);
                Log("Audit finished. Failures=" + failures);
                SessionState.EraseString(StateKey);
                SessionState.EraseInt(FailureKey);
                SessionState.EraseFloat(StartKey);
                EditorApplication.Exit(failures == 0 ? 0 : 1);
            }
        }

        private static bool Elapsed(float seconds)
        {
            float start = SessionState.GetFloat(StartKey, 0f);
            if (start <= 0f)
            {
                SessionState.SetFloat(StartKey, (float)EditorApplication.timeSinceStartup);
                return false;
            }
            return EditorApplication.timeSinceStartup - start >= seconds;
        }

        private static void RunImmediate()
        {
            Log("Running Phase 2 integrated Play Mode audit.");
            UIShopDesktop desktop = UnityEngine.Object.FindAnyObjectByType<UIShopDesktop>();
            PassIf(desktop != null, "RQF3 computer interactable exists in Game.unity.");
            PassIf(desktop != null && desktop.Interact("LeftClick"), "RQF3 player flow opens the real laptop interactable.");

            EntrepreneurTreeManager tree = EntrepreneurTreeManager.Instance;
            PassIf(tree != null, "EntrepreneurTreeManager exists.");
            tree.LoadFromJSON(null);
            EntrepreneurTreeManager.SetPoints(100);

            UpgradesUIController upgrades = UnityEngine.Object.FindAnyObjectByType<UpgradesUIController>(FindObjectsInactive.Include);
            PassIf(upgrades != null, "RQF3 Tree UI controller exists.");
            InvokeTab("EXPANDIR", "Expandir");
            Button openTree = upgrades != null ? upgrades.transform.Find("OpenEntrepreneurTreeButton")?.GetComponent<Button>() : null;
            PassIf(openTree != null, "RQF3 real Open Entrepreneur Tree button exists.");
            openTree?.onClick.Invoke();
            PassIf(upgrades != null && upgrades.transform.Find("EntrepreneurTreeRoot")?.gameObject.activeSelf == true,
                "RQF3 real tree button activates EntrepreneurTreeRoot.");

            NodeUI employeeNode = FindNode("employee_1");
            NodeUI basic2 = FindNode("product_basic_2");
            PassIf(employeeNode != null && employeeNode.titleLabel.text.Contains("BLOQUEADO"),
                "RQF4 locked node renders blocked visual state.");
            int points = EntrepreneurTreeManager.GetAvailablePoints();
            employeeNode?.OnPointerClick(null);
            PassIf(!EntrepreneurTreeManager.IsNodeUnlocked("employee_1") &&
                   EntrepreneurTreeManager.GetAvailablePoints() == points,
                "RQF36 blocked node click cannot skip prerequisite or spend points.");
            PassIf(basic2 != null && basic2.titleLabel.text.Contains("DISPONIBLE"),
                "RQF4 eligible node renders available visual state.");
            UnlockNodeThroughDetail(upgrades, basic2);
            PassIf(EntrepreneurTreeManager.IsNodeUnlocked("product_basic_2") &&
                   EntrepreneurTreeManager.GetAvailablePoints() == points - 1 &&
                   basic2.titleLabel.text.Contains("DESBLOQUEADO"),
                "RQF4 real node detail button unlocks, spends one point and renders unlocked state.");
            UnlockNodeThroughDetail(upgrades, FindNode("product_spices_1"));
            UnlockNodeThroughDetail(upgrades, FindNode("employee_1"));

            RunOrdersUI();
            RunEmployeesUI();
            RunInstructionsUI();
            RunSecurityVisuals();
        }

        private static void RunOrdersUI()
        {
            InvokeTab("COMPRA", "Compra");
            Transform panel = FindContentPanel("Compra");
            PassIf(panel != null && panel.gameObject.activeSelf, "RQNF18 COMPRA tab button activates real Orders panel.");
            OrdersAppUIController orders = panel != null ? panel.GetComponent<OrdersAppUIController>() : null;
            PassIf(orders != null, "Orders UI controller is attached to active panel.");

            ProductScriptableObject starter = GetProduct("0");
            Button buy = panel != null ? panel.Find("Scroll/Viewport/Content/Row_0/BtnBuy")?.GetComponent<Button>() : null;
            StoreDatabase.AddRemoveMoney(1000000L);
            PassIf(buy != null && buy.interactable, "RQNF7 starter product has an interactable real UI buy button.");

            long current = StoreDatabase.Instance.currentMoney;
            StoreDatabase.AddRemoveMoney(-current);
            buy?.onClick.Invoke();
            TMP_Text status = panel != null ? panel.Find("Summary/Status")?.GetComponent<TMP_Text>() : null;
            PassIf(status != null && status.text.Contains("Fondos insuficientes"),
                "RQNF7 insufficient funds click shows a clear UI message.");

            StoreDatabase.AddRemoveMoney(1000000L);
            int packages = UnityEngine.Object.FindObjectsByType<PackageObject>(FindObjectsSortMode.None).Length;
            long money = StoreDatabase.Instance.currentMoney;
            buy?.onClick.Invoke();
            PassIf(UnityEngine.Object.FindObjectsByType<PackageObject>(FindObjectsSortMode.None).Length == packages + 1,
                "RQNF9 real UI purchase creates a delivery package.");
            PassIf(StoreDatabase.Instance.currentMoney == money - starter.buyPrice * starter.packageCount,
                "RQNF7 real UI purchase deducts exact cost.");
        }

        private static void RunEmployeesUI()
        {
            InvokeTab("EMPLEADOS", "Empleados");
            Transform panel = FindContentPanel("Empleados");
            PassIf(panel != null && panel.gameObject.activeSelf, "RQF25 EMPLEADOS tab activates real panel.");
            Button open = panel != null ? panel.Find("OpenEmployeesAppButton")?.GetComponent<Button>() : null;
            open?.onClick.Invoke();
            Transform app = panel != null ? panel.Find("EmployeesAppRoot") : null;
            PassIf(open != null && app != null && app.gameObject.activeSelf, "RQF25 real employee app open button works.");
            app?.Find("ListPanel/Viewport/Content/Employee_1")?.GetComponent<Button>()?.onClick.Invoke();
            Button hire = app != null ? app.Find("DetailPanel/HireButton")?.GetComponent<Button>() : null;
            long before = StoreDatabase.Instance.currentMoney;
            long cost = EntrepreneurEmployeeSystem.Instance.GetAssignment(1).hireCost;
            PassIf(hire != null && hire.interactable, "RQF25 unlocked employee is hireable in real UI.");
            hire?.onClick.Invoke();
            PassIf(EntrepreneurEmployeeSystem.Instance.IsEmployeeHired(1) &&
                   StoreDatabase.Instance.currentMoney == before - cost,
                "RQF25 real hire button updates roster and deducts cost.");
            GameObject npc = EmployeeNPCSpawner.Instance.GetNPC(1);
            PassIf(npc != null && npc.activeInHierarchy && npc.GetComponent<UnityEngine.AI.NavMeshAgent>() != null,
                "RQF8 hired employee creates visible NavMesh NPC.");
            PassIf(EmployeeWorkstationRegistry.Instance.GetAssignedStation(1) != null,
                "RQF8 hired employee has resolvable stable cashier workstation.");
        }

        private static void RunInstructionsUI()
        {
            InvokeTab("AYUDA", "Instrucciones");
            Transform panel = FindContentPanel("Instrucciones");
            InstructionsAppUIController instructions = panel != null ? panel.GetComponent<InstructionsAppUIController>() : null;
            TMP_Text text = instructions != null ? instructions.GetComponentInChildren<TMP_Text>(true) : null;
            PassIf(panel != null && panel.gameObject.activeSelf && text != null &&
                   InstructionsAppUIController.GuideText.Contains("Arbol del Emprendedor") &&
                   InstructionsAppUIController.GuideText.Contains("ladrones"),
                "RQNF20 AYUDA tab exposes in-game instructions for tree and connected gameplay.");
        }

        private static void RunSecurityVisuals()
        {
            EntrepreneurTreeManager.AdminUnlockByType(TreeNodeType.Employee);
            EntrepreneurTreeManager.AdminUnlockByType(TreeNodeType.Security);
            EntrepreneurTreeSecurityAdapter security = EntrepreneurTreeSecurityAdapter.Instance;
            security.RefreshFromTree();
            PassIf(security.GetCurrentSecurityLevel() == 3 && Mathf.Approximately(security.GetAutomaticArrestChance(), 0.99f),
                "RQF18 security level 3 applies 99% automatic arrest chance.");
            PassIf(security.securityLevel1Visual != null && security.securityLevel1Visual.activeSelf &&
                   security.securityLevel2Visual != null && security.securityLevel2Visual.activeSelf &&
                   security.securityLevel3Visual != null && security.securityLevel3Visual.activeSelf,
                "RQF11 cameras, guards and gate visuals activate by security level.");
            PassIf(security.securityLevel1Visual.GetComponentsInChildren<Collider>(true).Length == 0 &&
                   security.securityLevel2Visual.GetComponentsInChildren<Collider>(true).Length == 0 &&
                   security.securityLevel3Visual.GetComponentsInChildren<Collider>(true).Length == 0,
                "RQF11 security visuals do not block NavMesh gameplay.");
        }

        private static void SetupCashierScenario()
        {
            EntrepreneurTreeManager.AdminUnlockByType(TreeNodeType.Improvement);
            string reason;
            EntrepreneurEmployeeSystem.Instance.TryAssignRole(1, EmployeeRole.Restocker, out reason);
            EntrepreneurEmployeeSystem.Instance.TryAssignRole(1, EmployeeRole.Cashier, out reason);
            cashierDesk = UnityEngine.Object.FindAnyObjectByType<CashDesk>();
            PassIf(cashierDesk != null, "RQF26 real CashDesk exists.");
            ProductScriptableObject product = GetProduct("0");
            Customer customer = CreateControlledCustomer(product, false);
            cashierDesk.AddCustomerToQueue(customer);
            cashierDesk.PlaceBagContents(customer.GetComponent<CustomerCart>());
            cashierMoneyBefore = StoreDatabase.Instance.currentMoney;
            cashierExpectedIncome = EntrepreneurTreeUpgradeAdapter.Instance.ApplySalesBonus(product.storePrice);
            PassIf(EntrepreneurTreeUpgradeAdapter.Instance.GetSalesMultiplier() == 1.05f,
                "RQF28 Charismatic multiplier is exactly 1.05 before cashier sale.");
            PassIf(cashierDesk.TryStartAutomatedCheckout(1f), "RQF26 real cashier checkout starts on real desk and customer cart.");
        }

        private static void VerifyCashierScenario()
        {
            PassIf(cashierDesk != null && !cashierDesk.isAutomatedCheckoutInProgress,
                "RQF26 cashier completes automated checkout within expected card timing window.");
            PassIf(StoreDatabase.Instance.currentMoney == cashierMoneyBefore + cashierExpectedIncome,
                "RQF26 cashier sale credits money with one Charismatic 1.05 application.");
            PassIf(EntrepreneurTreeUpgradeAdapter.Instance.GetEmployeeSpeedMultiplier() == 1.10f &&
                   EntrepreneurTreeUpgradeAdapter.Instance.GetSalesMultiplier() == 1.05f,
                "RQF28 Caffeine and Charismatic remain 1.10 and 1.05 without stacking.");
        }

        private static void RunRestockScenario()
        {
            string reason;
            EntrepreneurEmployeeSystem.Instance.TryAssignRole(1, EmployeeRole.Restocker, out reason);
            ProductScriptableObject product = GetProduct("0");
            PlacementObject placement = FindEmptyPlacement(product);
            PassIf(placement != null, "RQF27 compatible empty shelf placement exists.");
            PassIf(ShelfProductSlotSystem.Instance.AssignProduct(placement, product, out reason),
                "RQF27 product assignment to real shelf slot succeeds.");
            DeliverySystem.Purchase(product);
            PackageObject package = FindPackage(product);
            int shelfBefore = placement.count;
            int packageBefore = package != null ? package.count : -1;
            InvokePrivate(UnityEngine.Object.FindAnyObjectByType<EmployeeRestockCoordinator>(), "ExecuteSingleTask");
            PassIf(package != null && placement.count == shelfBefore + 1 && package.count == packageBefore - 1,
                "RQF27 restocker task moves one real package item into assigned shelf.");
        }

        private static void RunRobberyScenarios()
        {
            StatsDatabase.Instance.LoadFromJSON(null);
            EntrepreneurTreeManager.Instance.LoadFromJSON(null);
            EntrepreneurTreeSecurityAdapter.Instance.RefreshFromTree();
            ProductScriptableObject product = GetProduct("0");

            PlacementObject manualShelf = FindEmptyPlacement(product);
            Customer manual = CreateCollectedCustomer(product, manualShelf);
            ShoplifterSystem.AdminForceNextSpawn(ShoplifterType.Common);
            ShoplifterSystem.Instance.RegisterCustomer(manual);
            PassIf(ShoplifterSystem.Instance.TryBeginTheft(manual, manual.GetComponent<CustomerCart>()),
                "RQF13 common real customer begins theft with real cart item.");
            ShoplifterAgent manualAgent = manual.GetComponent<ShoplifterAgent>();
            int points = EntrepreneurTreeManager.GetAvailablePoints();
            int recovered = StatsDatabase.Instance.recoveredProducts;
            PassIf(manualAgent != null && manualAgent.TryManualCapture() &&
                   EntrepreneurTreeManager.GetAvailablePoints() > points &&
                   StatsDatabase.Instance.recoveredProducts == recovered + 1,
                "RQF17 manual capture restores real product and grants progress points.");

            PlacementObject escapeShelf = FindEmptyPlacement(product);
            Customer escape = CreateCollectedCustomer(product, escapeShelf);
            ShoplifterSystem.AdminForceNextSpawn(ShoplifterType.Common);
            ShoplifterSystem.Instance.RegisterCustomer(escape);
            long money = StoreDatabase.Instance.currentMoney;
            ShoplifterSystem.Instance.TryBeginTheft(escape, escape.GetComponent<CustomerCart>());
            ShoplifterAgent escapeAgent = escape.GetComponent<ShoplifterAgent>();
            long stolen = escapeAgent.stolenValue;
            escapeAgent.ResolveAsEscaped();
            PassIf(stolen > 0 && StoreDatabase.Instance.currentMoney == money - stolen && escapeShelf.count == 0,
                "RQF14 escaped thief loses exact value of reserved real product.");

            EntrepreneurTreeManager.AdminUnlockByType(TreeNodeType.Employee);
            EntrepreneurTreeManager.AdminUnlockByType(TreeNodeType.Security);
            EntrepreneurTreeSecurityAdapter.Instance.RefreshFromTree();
            UnityEngine.Random.InitState(1234);
            PlacementObject autoShelf = FindEmptyPlacement(product);
            Customer auto = CreateCollectedCustomer(product, autoShelf);
            ShoplifterSystem.AdminForceNextSpawn(ShoplifterType.Common);
            ShoplifterSystem.Instance.RegisterCustomer(auto);
            recovered = StatsDatabase.Instance.recoveredProducts;
            ShoplifterSystem.Instance.TryBeginTheft(auto, auto.GetComponent<CustomerCart>());
            ShoplifterAgent autoAgent = auto.GetComponent<ShoplifterAgent>();
            PassIf(autoAgent != null && autoAgent.isResolved && !autoAgent.isEscaping &&
                   StatsDatabase.Instance.recoveredProducts == recovered + 1,
                "RQF18 security level 3 automatically arrests and restores real product.");
            string summary = StatsDatabase.GetDailyRobberySummary();
            PassIf(summary.Contains("Detenidos manualmente: 1") && summary.Contains("Escaparon: 1") &&
                   summary.Contains("Arrestos automáticos: 1"),
                "RQF18 daily report records manual capture, escape and automatic arrest.");
        }

        private static void RunDiskPersistenceScenario()
        {
            string root = Application.persistentDataPath;
            string phaseSave = Path.Combine(root, "phase2_green_audit.dat");
            string[] auxiliary = { "entrepreneurTree.dat", "shelfSlots.dat", "expansionApp.dat" };
            Dictionary<string, byte[]> backups = new Dictionary<string, byte[]>();
            try
            {
                for (int i = 0; i < auxiliary.Length; i++)
                {
                    string path = Path.Combine(root, auxiliary[i]);
                    backups[path] = File.Exists(path) ? File.ReadAllBytes(path) : null;
                }

                EntrepreneurTreeManager.SetPoints(77);
                EntrepreneurTreeManager.AdminUnlockByType(TreeNodeType.Improvement);
                SaveGameSystem.Save("phase2_green_audit");
                PassIf(File.Exists(phaseSave), "RQF21 temporary base save is written to disk.");
                PassIf(File.Exists(Path.Combine(root, "entrepreneurTree.dat")),
                    "RQF21 entrepreneur tree auxiliary save is written to disk.");

                EntrepreneurTreeManager.Instance.LoadFromJSON(null);
                EntrepreneurEmployeeSystem.Instance.LoadFromJSON(null);
                InvokePrivate(UnityEngine.Object.FindAnyObjectByType<EntrepreneurTreeSaveIntegration>(), "OnLoad");
                PassIf(EntrepreneurTreeManager.GetAvailablePoints() == 77 &&
                       EntrepreneurEmployeeSystem.Instance.IsEmployeeHired(1) &&
                       EntrepreneurEmployeeSystem.Instance.GetEmployeeRole(1) == EmployeeRole.Restocker,
                    "RQNF3 disk reload restores tree points, hired employee and role.");
                EntrepreneurTreeUpgradeAdapter.Instance.SendMessage("OnDataLoaded", SendMessageOptions.DontRequireReceiver);
                EntrepreneurTreeSecurityAdapter.Instance.RefreshFromTree();
                PassIf(EntrepreneurTreeUpgradeAdapter.Instance.GetEmployeeSpeedMultiplier() == 1.10f &&
                       EntrepreneurTreeUpgradeAdapter.Instance.GetSalesMultiplier() == 1.05f &&
                       EntrepreneurTreeSecurityAdapter.Instance.GetCurrentSecurityLevel() == 3,
                    "RQNF3 disk reload preserves upgrades without stacking and restores security.");
            }
            finally
            {
                if (File.Exists(phaseSave))
                    File.Delete(phaseSave);
                foreach (KeyValuePair<string, byte[]> pair in backups)
                {
                    if (pair.Value == null)
                    {
                        if (File.Exists(pair.Key))
                            File.Delete(pair.Key);
                    }
                    else
                    {
                        File.WriteAllBytes(pair.Key, pair.Value);
                    }
                }
            }
        }

        private static Customer CreateControlledCustomer(ProductScriptableObject product, bool collectFromShelf)
        {
            GameObject prefab = CustomerSystem.Instance.customerPrefabs[0];
            GameObject go = UnityEngine.Object.Instantiate(prefab, CustomerSystem.Instance.spawnLocations[0].position, Quaternion.identity);
            Customer customer = go.GetComponent<Customer>();
            customer.enabled = false;
            CustomerCart cart = go.GetComponent<CustomerCart>();
            if (!collectFromShelf)
                cart.items.Add(new CustomerBagItem { product = product, fixedPrice = product.storePrice, count = 1 });
            return customer;
        }

        private static Customer CreateCollectedCustomer(ProductScriptableObject product, PlacementObject shelf)
        {
            SeedShelf(shelf, product);
            Customer customer = CreateControlledCustomer(product, true);
            CustomerCart cart = customer.GetComponent<CustomerCart>();
            cart.wishlist.Add((product, shelf));
            cart.Add();
            return customer;
        }

        private static void SeedShelf(PlacementObject shelf, ProductScriptableObject product)
        {
            Vector3 local = shelf.Add(product);
            UnityEngine.Object.Instantiate(product.prefab, shelf.container.TransformPoint(local),
                shelf.transform.rotation * Quaternion.Euler(0f, shelf.orientation, 0f), shelf.container);
        }

        private static PlacementObject FindEmptyPlacement(ProductScriptableObject product)
        {
            PlacementObject[] placements = UnityEngine.Object.FindObjectsByType<PlacementObject>(FindObjectsSortMode.None);
            for (int i = 0; i < placements.Length; i++)
                if (placements[i] != null && placements[i].IsEmpty() && placements[i].IsPlaceable(product))
                    return placements[i];
            return null;
        }

        private static PackageObject FindPackage(ProductScriptableObject product)
        {
            PackageObject[] packages = UnityEngine.Object.FindObjectsByType<PackageObject>(FindObjectsSortMode.None);
            for (int i = 0; i < packages.Length; i++)
                if (packages[i] != null && packages[i].purchasable == product && packages[i].count > 0)
                    return packages[i];
            return null;
        }

        private static ProductScriptableObject GetProduct(string id)
        {
            return ItemDatabase.GetById(typeof(ProductScriptableObject), id) as ProductScriptableObject;
        }

        private static NodeUI FindNode(string id)
        {
            NodeUI[] nodes = UnityEngine.Object.FindObjectsByType<NodeUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < nodes.Length; i++)
                if (nodes[i] != null && nodes[i].gameObject.activeInHierarchy && nodes[i].data != null && nodes[i].data.id == id)
                    return nodes[i];

            for (int i = 0; i < nodes.Length; i++)
                if (nodes[i] != null && nodes[i].data != null && nodes[i].data.id == id)
                    return nodes[i];
            return null;
        }

        private static void UnlockNodeThroughDetail(UpgradesUIController upgrades, NodeUI node)
        {
            if (upgrades == null || node == null)
                return;

            node.OnPointerClick(null);
            upgrades.infoUnlockButton?.onClick.Invoke();
        }

        private static void InvokeTab(string label, string panelName)
        {
            Button[] buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < buttons.Length; i++)
            {
                TMP_Text text = buttons[i] != null ? buttons[i].GetComponentInChildren<TMP_Text>(true) : null;
                if (text != null && text.text.Trim().ToUpperInvariant() == label)
                {
                    buttons[i].onClick.Invoke();
                    PassIf(FindContentPanel(panelName)?.gameObject.activeSelf == true,
                        "Real computer tab button '" + label + "' activates panel '" + panelName + "'.");
                    return;
                }
            }
            Fail("Computer tab button not found: " + label);
        }

        private static Transform FindContentPanel(string name)
        {
            UIShopCategoryHelper helper = UnityEngine.Object.FindAnyObjectByType<UIShopCategoryHelper>(FindObjectsInactive.Include);
            return helper != null ? helper.transform.Find(name) : null;
        }

        private static void InvokePrivate(object instance, string method)
        {
            MethodInfo info = instance?.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic);
            if (info == null)
                throw new MissingMethodException(instance?.GetType().Name, method);
            info.Invoke(instance, null);
        }

        private static void LeavePlayMode()
        {
            SessionState.SetString(StateKey, "leave");
            EditorApplication.ExitPlaymode();
        }

        private static void OnConsole(string condition, string stackTrace, LogType type)
        {
            string state = SessionState.GetString(StateKey, string.Empty);
            if (string.IsNullOrEmpty(state) || state == "leave")
                return;
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                Fail("Console " + type + ": " + condition + "\n" + stackTrace);
        }

        private static void PassIf(bool condition, string message)
        {
            if (condition) Log("PASS: " + message);
            else Fail(message);
        }

        private static void Fail(string message)
        {
            SessionState.SetInt(FailureKey, SessionState.GetInt(FailureKey, 0) + 1);
            Log("FAIL: " + message);
        }

        private static void Log(string message)
        {
            File.AppendAllText(LogPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + "\n");
        }
    }
}
