using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Runtime-only integration layer that wires the Entrepreneur Tree UI into the existing
    /// UPGRADES tab (Expansions panel) without modifying base asset scripts.
    /// </summary>
    public static class EntrepreneurTreeUIBootstrap
    {
        private const string LogPrefix = "[EntrepreneurTree] ";
        private const string ResourcesTreePath = "EntrepreneurTree/EntrepreneurTreeData";
#if UNITY_EDITOR
        private const string EditorTreePath = "Assets/Data/EntrepreneurTree/EntrepreneurTreeData.asset";
#endif
        /// <summary>Preferred pixel width for each custom tab button we inject.</summary>
        private const float TabButtonPreferredWidth = 120f;
        /// <summary>Minimum pixel width a custom tab button may shrink to.</summary>
        private const float TabButtonMinWidth = 80f;

        private static bool initialized;
        private static TreeData runtimeFallbackTree;
        private static EntrepreneurTreeManager cachedManager;
        private static bool sceneHandlerRegistered;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (initialized)
                return;

            initialized = true;
            if (!sceneHandlerRegistered)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                sceneHandlerRegistered = true;
            }
            IntegrateScene(SceneManager.GetActiveScene());
        }


        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            IntegrateScene(scene);
        }


        private static void IntegrateScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return;

            EnsureTreeSystems();

            // Apply admin-mode configuration the first time the Game scene loads.
            if (AdminSessionConfig.isActive)
                RegisterAdminApply();

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                UIShopCategoryHelper[] helpers = roots[i].GetComponentsInChildren<UIShopCategoryHelper>(true);
                for (int h = 0; h < helpers.Length; h++)
                {
                    UIShopCategoryHelper helper = helpers[h];
                    if (helper == null)
                        continue;

                    Transform contentArea = helper.transform;
                    Transform upgradesPanel = contentArea.Find("Expansions");
                    if (upgradesPanel == null)
                        continue;
                    Debug.Log(LogPrefix + "Bootstrap found Expansions panel.");

                    UpgradesUIController controller = upgradesPanel.GetComponent<UpgradesUIController>();
                    if (controller == null)
                    {
                        controller = upgradesPanel.gameObject.AddComponent<UpgradesUIController>();
                        Debug.Log(LogPrefix + "Attached UpgradesUIController to ContentArea/Expansions.");
                    }

                    Transform oldScroll = upgradesPanel.Find("Scroll View");
                    if (oldScroll != null)
                        controller.SetLegacyContent(oldScroll.gameObject);

                    controller.TryAutoConfigureFromHierarchy();
                    controller.BuildTree();

                    EnsureExpansionTab(helper, contentArea);
                    EnsureEmployeeTab(helper, contentArea);
                    EnsureAchievementsTab(helper, contentArea);
                    EnsureOrdersTab(helper, contentArea);
                    EnsurePricingTab(helper, contentArea);
                    EnsureInventoryTab(helper, contentArea);
                }
            }
        }


        private static void EnsureTreeSystems()
        {
            EntrepreneurTreeManager manager = FindExistingTreeManager();
            if (manager == null)
            {
                GameObject systems = new GameObject("EntrepreneurTreeSystems");
                Object.DontDestroyOnLoad(systems);

                manager = systems.AddComponent<EntrepreneurTreeManager>();
                cachedManager = manager;
                systems.AddComponent<AchievementSystem>();
                systems.AddComponent<EntrepreneurTreeSaveIntegration>();
                systems.AddComponent<EntrepreneurTreeProductUnlockAdapter>();
                systems.AddComponent<EntrepreneurTreeEmployeeUnlockAdapter>();
                systems.AddComponent<EntrepreneurTreeSecurityAdapter>();
                systems.AddComponent<EntrepreneurTreeUpgradeAdapter>();
                systems.AddComponent<EntrepreneurTreeGameplayBridge>();
                systems.AddComponent<EntrepreneurEmployeeSystem>();
                systems.AddComponent<EmployeeRestockCoordinator>();
                systems.AddComponent<ShoplifterSystem>();
                systems.AddComponent<SupermarketExpansionSystem>();
                systems.AddComponent<ExpansionCustomerDemandAdapter>();
                systems.AddComponent<ExpansionStorageCapacityAdapter>();
                systems.AddComponent<ProductInventorySystem>();

                Debug.Log(LogPrefix + "Created runtime EntrepreneurTree systems GameObject.");
            }
            else
            {
                EnsureGameplayAdapters(manager.gameObject);
            }

            if (manager.treeData == null)
            {
                manager.treeData = TryLoadProjectTreeData();
                if (manager.treeData != null)
                {
                    Debug.Log(LogPrefix + "TreeData loaded successfully.");
                }
                else
                {
                    manager.treeData = CreateFallbackTreeData();
                    Debug.LogWarning(LogPrefix + "TreeData not assigned in Inspector. Using runtime fallback tree data.");
                }
            }

            EntrepreneurTreeDefinition.SynchronizeTreeData(manager.treeData);
        }


        private static void EnsureGameplayAdapters(GameObject systems)
        {
            if (systems.GetComponent<AchievementSystem>() == null)
                systems.AddComponent<AchievementSystem>();
            if (systems.GetComponent<EntrepreneurTreeSaveIntegration>() == null)
                systems.AddComponent<EntrepreneurTreeSaveIntegration>();
            if (systems.GetComponent<EntrepreneurTreeProductUnlockAdapter>() == null)
                systems.AddComponent<EntrepreneurTreeProductUnlockAdapter>();
            if (systems.GetComponent<EntrepreneurTreeEmployeeUnlockAdapter>() == null)
                systems.AddComponent<EntrepreneurTreeEmployeeUnlockAdapter>();
            if (systems.GetComponent<EntrepreneurTreeSecurityAdapter>() == null)
                systems.AddComponent<EntrepreneurTreeSecurityAdapter>();
            if (systems.GetComponent<EntrepreneurTreeUpgradeAdapter>() == null)
                systems.AddComponent<EntrepreneurTreeUpgradeAdapter>();
            if (systems.GetComponent<EntrepreneurTreeGameplayBridge>() == null)
                systems.AddComponent<EntrepreneurTreeGameplayBridge>();
            if (systems.GetComponent<EntrepreneurEmployeeSystem>() == null)
                systems.AddComponent<EntrepreneurEmployeeSystem>();
            if (systems.GetComponent<EmployeeRestockCoordinator>() == null)
                systems.AddComponent<EmployeeRestockCoordinator>();
            if (systems.GetComponent<ShoplifterSystem>() == null)
                systems.AddComponent<ShoplifterSystem>();
            if (systems.GetComponent<SupermarketExpansionSystem>() == null)
                systems.AddComponent<SupermarketExpansionSystem>();
            if (systems.GetComponent<ExpansionCustomerDemandAdapter>() == null)
                systems.AddComponent<ExpansionCustomerDemandAdapter>();
            if (systems.GetComponent<ExpansionStorageCapacityAdapter>() == null)
                systems.AddComponent<ExpansionStorageCapacityAdapter>();
            if (systems.GetComponent<ProductInventorySystem>() == null)
                systems.AddComponent<ProductInventorySystem>();
        }


        private static EntrepreneurTreeManager FindExistingTreeManager()
        {
            if (cachedManager != null)
                return cachedManager;

            EntrepreneurTreeManager[] managers = FindTreeManagers();
            if (managers != null && managers.Length > 0)
            {
                cachedManager = managers[0];
                return cachedManager;
            }

            return null;
        }


        private static TreeData CreateFallbackTreeData()
        {
            if (runtimeFallbackTree != null)
                return runtimeFallbackTree;

            runtimeFallbackTree = ScriptableObject.CreateInstance<TreeData>();
            runtimeFallbackTree.name = "EntrepreneurTreeData_Runtime";
            runtimeFallbackTree.nodes = EntrepreneurTreeDefinition.CreateRuntimeNodes();
            return runtimeFallbackTree;
        }


        private static TreeData TryLoadProjectTreeData()
        {
            TreeData fromResources = Resources.Load<TreeData>(ResourcesTreePath);
            if (fromResources != null)
                return fromResources;

            TreeData[] loaded = Resources.FindObjectsOfTypeAll<TreeData>();
            if (loaded != null && loaded.Length > 0)
            {
                Debug.Log(LogPrefix + "TreeData loaded from loaded objects fallback: " + loaded[0].name);
                return loaded[0];
            }

#if UNITY_EDITOR
            TreeData fromEditorPath = UnityEditor.AssetDatabase.LoadAssetAtPath<TreeData>(EditorTreePath);
            if (fromEditorPath != null)
            {
                Debug.Log(LogPrefix + "TreeData loaded from editor path fallback: " + EditorTreePath);
                return fromEditorPath;
            }
#endif

            return null;
        }

        private static void EnsureExpansionTab(UIShopCategoryHelper helper, Transform contentArea)
        {
            if (helper == null || contentArea == null)
                return;

            Transform expandirPanel = contentArea.Find("Expandir");
            if (expandirPanel == null)
            {
                GameObject panelObject = new GameObject("Expandir", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                panelObject.transform.SetParent(contentArea, false);
                RectTransform panelRT = panelObject.GetComponent<RectTransform>();
                panelRT.anchorMin = Vector2.zero;
                panelRT.anchorMax = Vector2.one;
                panelRT.offsetMin = Vector2.zero;
                panelRT.offsetMax = Vector2.zero;
                Image panelImage = panelObject.GetComponent<Image>();
                panelImage.color = new Color(0.09f, 0.10f, 0.13f, 0.96f);

                panelObject.SetActive(false);
                expandirPanel = panelObject.transform;
                Debug.Log("[ExpansionApp] Expansion panel created.");
            }

            ExpansionAppUIController app = expandirPanel.GetComponent<ExpansionAppUIController>();
            if (app == null)
                app = expandirPanel.gameObject.AddComponent<ExpansionAppUIController>();

            app.Initialize(helper, expandirPanel as RectTransform);
            EnsureExpansionButton(helper, expandirPanel.gameObject);
        }

        private static void EnsureExpansionButton(UIShopCategoryHelper helper, GameObject panel)
        {
            if (helper == null || panel == null)
                return;

            Transform root = helper.transform.parent != null ? helper.transform.parent : helper.transform;
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            Button template = null;
            Button existing = null;
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null)
                    continue;

                TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                string text = label != null ? label.text.Trim().ToUpperInvariant() : string.Empty;
                if (text == "EXPANDIR")
                {
                    existing = button;
                    break;
                }

                if ((text == "CUSTOMIZATION" || text == "BOOSTERS") && template == null)
                    template = button;
            }

            if (existing != null)
            {
                ConfigureExpansionButton(existing, helper, panel);
                return;
            }

            if (template == null)
                return;

            Button newButton = Object.Instantiate(template, template.transform.parent, false);
            newButton.name = "ExpandirButton";
            TMP_Text newLabel = newButton.GetComponentInChildren<TMP_Text>(true);
            if (newLabel != null)
            {
                newLabel.text = "EXPANDIR";
                newLabel.color = Color.white;
            }

            Image buttonImage = newButton.GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.color = new Color(0.89f, 0.23f, 0.56f, 0.95f);

            newButton.onClick.RemoveAllListeners();
            ConfigureExpansionButton(newButton, helper, panel);
            ApplyTabButtonCompact(newButton);
            newButton.transform.SetAsLastSibling();
            Debug.Log("[ExpansionApp] Expansion tab created.");
        }

        private static void ConfigureExpansionButton(Button button, UIShopCategoryHelper helper, GameObject panel)
        {
            if (button == null)
                return;

            ExpansionTabButtonLink link = button.GetComponent<ExpansionTabButtonLink>();
            if (link == null)
                link = button.gameObject.AddComponent<ExpansionTabButtonLink>();
            link.Configure(helper, panel);
        }

        // ── Employee tab (Empleados) ──────────────────────────────────────────

        /// <summary>
        /// Creates (or reuses) a dedicated "Empleados" content panel inside contentArea,
        /// attaches EmployeeAppUIController to it, and wires a tab button — mirrors EnsureExpansionTab.
        /// </summary>
        private static void EnsureEmployeeTab(UIShopCategoryHelper helper, Transform contentArea)
        {
            if (helper == null || contentArea == null)
                return;

            Transform empleadosPanel = contentArea.Find("Empleados");
            if (empleadosPanel == null)
            {
                GameObject panelObject = new GameObject("Empleados", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                panelObject.transform.SetParent(contentArea, false);
                RectTransform panelRT = panelObject.GetComponent<RectTransform>();
                panelRT.anchorMin = Vector2.zero;
                panelRT.anchorMax = Vector2.one;
                panelRT.offsetMin = Vector2.zero;
                panelRT.offsetMax = Vector2.zero;
                Image panelImage = panelObject.GetComponent<Image>();
                panelImage.color = new Color(0.07f, 0.09f, 0.12f, 0.96f);

                panelObject.SetActive(false);
                empleadosPanel = panelObject.transform;
                Debug.Log("[Employees] Employee panel created.");
            }

            EmployeeAppUIController app = empleadosPanel.GetComponent<EmployeeAppUIController>();
            if (app == null)
            {
                app = empleadosPanel.gameObject.AddComponent<EmployeeAppUIController>();
                Debug.Log("[Employees] EmployeeAppUIController attached to Empleados panel.");
            }

            if (helper != null)
                EnsureEmployeeButton(helper, empleadosPanel.gameObject);
        }

        private static void EnsureEmployeeButton(UIShopCategoryHelper helper, GameObject panel)
        {
            if (helper == null || panel == null)
                return;

            Transform root = helper.transform.parent != null ? helper.transform.parent : helper.transform;
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            Button template = null;
            Button existing = null;
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null)
                    continue;

                TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                string text = label != null ? label.text.Trim().ToUpperInvariant() : string.Empty;
                if (text == "EMPLEADOS")
                {
                    existing = button;
                    break;
                }

                if ((text == "CUSTOMIZATION" || text == "BOOSTERS" || text == "EXPANDIR") && template == null)
                    template = button;
            }

            if (existing != null)
            {
                ConfigureEmployeeButton(existing, helper, panel);
                return;
            }

            if (template == null)
                return;

            Button newButton = Object.Instantiate(template, template.transform.parent, false);
            newButton.name = "EmpleadosButton";
            TMP_Text newLabel = newButton.GetComponentInChildren<TMP_Text>(true);
            if (newLabel != null)
            {
                newLabel.text = "EMPLEADOS";
                newLabel.color = Color.white;
            }

            Image buttonImage = newButton.GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.color = new Color(0.18f, 0.46f, 0.28f, 0.95f);

            newButton.onClick.RemoveAllListeners();
            ConfigureEmployeeButton(newButton, helper, panel);
            ApplyTabButtonCompact(newButton);
            newButton.transform.SetAsLastSibling();
            Debug.Log("[Employees] Employee tab button created.");
        }

        private static void ConfigureEmployeeButton(Button button, UIShopCategoryHelper helper, GameObject panel)
        {
            if (button == null)
                return;

            ExpansionTabButtonLink link = button.GetComponent<ExpansionTabButtonLink>();
            if (link == null)
                link = button.gameObject.AddComponent<ExpansionTabButtonLink>();
            link.Configure(helper, panel);
        }

        // ── Achievements tab (Logros) ──────────────────────────────────────────

        /// <summary>
        /// Creates (or reuses) a dedicated "Logros" content panel inside contentArea,
        /// attaches AchievementsAppUIController to it, and wires a tab button.
        /// Mirrors EnsureEmployeeTab.
        /// </summary>
        private static void EnsureAchievementsTab(UIShopCategoryHelper helper, Transform contentArea)
        {
            if (helper == null || contentArea == null)
                return;

            Transform logrosPanel = contentArea.Find("Logros");
            if (logrosPanel == null)
            {
                GameObject panelObject = new GameObject("Logros", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                panelObject.transform.SetParent(contentArea, false);
                RectTransform panelRT = panelObject.GetComponent<RectTransform>();
                panelRT.anchorMin = Vector2.zero;
                panelRT.anchorMax = Vector2.one;
                panelRT.offsetMin = Vector2.zero;
                panelRT.offsetMax = Vector2.zero;
                Image panelImage = panelObject.GetComponent<Image>();
                panelImage.color = new Color(0.07f, 0.08f, 0.11f, 0.96f);

                panelObject.SetActive(false);
                logrosPanel = panelObject.transform;
                Debug.Log("[Achievements] Achievements panel created.");
            }

            AchievementsAppUIController app = logrosPanel.GetComponent<AchievementsAppUIController>();
            if (app == null)
            {
                app = logrosPanel.gameObject.AddComponent<AchievementsAppUIController>();
                Debug.Log("[Achievements] AchievementsAppUIController attached to Logros panel.");
            }

            if (helper != null)
                EnsureAchievementsButton(helper, logrosPanel.gameObject);
        }

        private static void EnsureAchievementsButton(UIShopCategoryHelper helper, GameObject panel)
        {
            if (helper == null || panel == null)
                return;

            Transform root = helper.transform.parent != null ? helper.transform.parent : helper.transform;
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            Button template = null;
            Button existing = null;
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null)
                    continue;

                TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                string text = label != null ? label.text.Trim().ToUpperInvariant() : string.Empty;
                if (text == "LOGROS")
                {
                    existing = button;
                    break;
                }

                if ((text == "CUSTOMIZATION" || text == "BOOSTERS" || text == "EXPANDIR" || text == "EMPLEADOS") && template == null)
                    template = button;
            }

            if (existing != null)
            {
                ConfigureAchievementsButton(existing, helper, panel);
                return;
            }

            if (template == null)
                return;

            Button newButton = Object.Instantiate(template, template.transform.parent, false);
            newButton.name = "LogrosButton";
            TMP_Text newLabel = newButton.GetComponentInChildren<TMP_Text>(true);
            if (newLabel != null)
            {
                newLabel.text  = "LOGROS";
                newLabel.color = Color.white;
            }

            Image buttonImage = newButton.GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.color = new Color(0.72f, 0.45f, 0.10f, 0.95f);

            newButton.onClick.RemoveAllListeners();
            ConfigureAchievementsButton(newButton, helper, panel);
            ApplyTabButtonCompact(newButton);
            newButton.transform.SetAsLastSibling();
            Debug.Log("[Achievements] Achievements tab button created.");
        }

        private static void ConfigureAchievementsButton(Button button, UIShopCategoryHelper helper, GameObject panel)
        {
            if (button == null)
                return;

            ExpansionTabButtonLink link = button.GetComponent<ExpansionTabButtonLink>();
            if (link == null)
                link = button.gameObject.AddComponent<ExpansionTabButtonLink>();
            link.Configure(helper, panel);
        }

        private static EntrepreneurTreeManager[] FindTreeManagers()
        {
#if UNITY_2022_2_OR_NEWER
            return Object.FindObjectsByType<EntrepreneurTreeManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            return Object.FindObjectsOfType<EntrepreneurTreeManager>(true);
#endif
        }


        // ── Orders tab (Compra / Pedidos) ──────────────────────────────────────

        /// <summary>
        /// Creates (or reuses) a dedicated "Compra" content panel inside contentArea,
        /// attaches OrdersAppUIController to it, and wires a tab button — mirrors EnsureAchievementsTab.
        /// </summary>
        private static void EnsureOrdersTab(UIShopCategoryHelper helper, Transform contentArea)
        {
            if (helper == null || contentArea == null)
                return;

            Transform ordersPanel = contentArea.Find("Compra");
            if (ordersPanel == null)
            {
                GameObject panelObject = new GameObject("Compra", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                panelObject.transform.SetParent(contentArea, false);
                RectTransform panelRT = panelObject.GetComponent<RectTransform>();
                panelRT.anchorMin = Vector2.zero;
                panelRT.anchorMax = Vector2.one;
                panelRT.offsetMin = Vector2.zero;
                panelRT.offsetMax = Vector2.zero;
                Image panelImage = panelObject.GetComponent<Image>();
                panelImage.color = new Color(0.07f, 0.08f, 0.11f, 0.96f);

                panelObject.SetActive(false);
                ordersPanel = panelObject.transform;
                Debug.Log("[Orders] Orders panel created.");
            }

            OrdersAppUIController app = ordersPanel.GetComponent<OrdersAppUIController>();
            if (app == null)
            {
                app = ordersPanel.gameObject.AddComponent<OrdersAppUIController>();
                Debug.Log("[Orders] OrdersAppUIController attached to Compra panel.");
            }

            if (helper != null)
                EnsureOrdersButton(helper, ordersPanel.gameObject);
        }

        private static void EnsureOrdersButton(UIShopCategoryHelper helper, GameObject panel)
        {
            if (helper == null || panel == null)
                return;

            Transform root = helper.transform.parent != null ? helper.transform.parent : helper.transform;
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            Button template = null;
            Button existing = null;
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null)
                    continue;

                TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                string text = label != null ? label.text.Trim().ToUpperInvariant() : string.Empty;
                if (text == "COMPRA")
                {
                    existing = button;
                    break;
                }

                if ((text == "CUSTOMIZATION" || text == "BOOSTERS" || text == "EXPANDIR" || text == "EMPLEADOS" || text == "LOGROS") && template == null)
                    template = button;
            }

            if (existing != null)
            {
                ConfigureOrdersButton(existing, helper, panel);
                return;
            }

            if (template == null)
                return;

            Button newButton = Object.Instantiate(template, template.transform.parent, false);
            newButton.name = "CompraButton";
            TMP_Text newLabel = newButton.GetComponentInChildren<TMP_Text>(true);
            if (newLabel != null)
            {
                newLabel.text  = "COMPRA";
                newLabel.color = Color.white;
            }

            Image buttonImage = newButton.GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.color = new Color(0.15f, 0.55f, 0.30f, 0.95f);  // green tint

            newButton.onClick.RemoveAllListeners();
            ConfigureOrdersButton(newButton, helper, panel);
            ApplyTabButtonCompact(newButton);
            newButton.transform.SetAsLastSibling();
            Debug.Log("[Orders] Orders tab button created.");
        }

        private static void ConfigureOrdersButton(Button button, UIShopCategoryHelper helper, GameObject panel)
        {
            if (button == null)
                return;

            ExpansionTabButtonLink link = button.GetComponent<ExpansionTabButtonLink>();
            if (link == null)
                link = button.gameObject.AddComponent<ExpansionTabButtonLink>();
            link.Configure(helper, panel);
        }

        // ── Pricing tab ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates the "Precios" panel in ContentArea, attaches PricingAppUIController,
        /// and wires a tab button — mirrors EnsureOrdersTab.
        /// </summary>
        private static void EnsurePricingTab(UIShopCategoryHelper helper, Transform contentArea)
        {
            if (helper == null || contentArea == null)
                return;

            Transform pricingPanel = contentArea.Find("Precios");
            if (pricingPanel == null)
            {
                GameObject panelObject = new GameObject("Precios", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                panelObject.transform.SetParent(contentArea, false);
                RectTransform panelRT = panelObject.GetComponent<RectTransform>();
                panelRT.anchorMin = Vector2.zero;
                panelRT.anchorMax = Vector2.one;
                panelRT.offsetMin = Vector2.zero;
                panelRT.offsetMax = Vector2.zero;
                Image panelImage = panelObject.GetComponent<Image>();
                panelImage.color = new Color(0.07f, 0.08f, 0.11f, 0.96f);

                panelObject.SetActive(false);
                pricingPanel = panelObject.transform;
                Debug.Log("[Pricing] Pricing panel created.");
            }

            PricingAppUIController app = pricingPanel.GetComponent<PricingAppUIController>();
            if (app == null)
            {
                app = pricingPanel.gameObject.AddComponent<PricingAppUIController>();
                Debug.Log("[Pricing] PricingAppUIController attached to Precios panel.");
            }

            if (helper != null)
                EnsurePricingButton(helper, pricingPanel.gameObject);
        }

        private static void EnsurePricingButton(UIShopCategoryHelper helper, GameObject panel)
        {
            if (helper == null || panel == null)
                return;

            Transform root = helper.transform.parent != null ? helper.transform.parent : helper.transform;
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            Button template = null;
            Button existing = null;
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null)
                    continue;

                TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                string text = label != null ? label.text.Trim().ToUpperInvariant() : string.Empty;
                if (text == "PRECIOS")
                {
                    existing = button;
                    break;
                }

                if ((text == "CUSTOMIZATION" || text == "BOOSTERS" || text == "EXPANDIR" ||
                     text == "EMPLEADOS"     || text == "LOGROS"    || text == "COMPRA") && template == null)
                    template = button;
            }

            if (existing != null)
            {
                ConfigurePricingButton(existing, helper, panel);
                return;
            }

            if (template == null)
                return;

            Button newButton = Object.Instantiate(template, template.transform.parent, false);
            newButton.name = "PreciosButton";
            TMP_Text newLabel = newButton.GetComponentInChildren<TMP_Text>(true);
            if (newLabel != null)
            {
                newLabel.text  = "PRECIOS";
                newLabel.color = Color.white;
            }

            Image buttonImage = newButton.GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.color = new Color(0.60f, 0.35f, 0.10f, 0.95f);  // amber tint

            newButton.onClick.RemoveAllListeners();
            ConfigurePricingButton(newButton, helper, panel);
            ApplyTabButtonCompact(newButton);
            newButton.transform.SetAsLastSibling();
            Debug.Log("[Pricing] Pricing tab button created.");
        }

        private static void ConfigurePricingButton(Button button, UIShopCategoryHelper helper, GameObject panel)
        {
            if (button == null)
                return;

            ExpansionTabButtonLink link = button.GetComponent<ExpansionTabButtonLink>();
            if (link == null)
                link = button.gameObject.AddComponent<ExpansionTabButtonLink>();
            link.Configure(helper, panel);
        }

        // ── INVENTARIO tab ─────────────────────────────────────────────────────

        /// <summary>
        /// Creates the "INVENTARIO" panel + tab button and attaches
        /// <see cref="InventoryAppUIController"/> — mirrors EnsurePricingTab.
        /// </summary>
        private static void EnsureInventoryTab(UIShopCategoryHelper helper, Transform contentArea)
        {
            if (helper == null || contentArea == null)
                return;

            Transform panel = contentArea.Find("Inventario");
            if (panel == null)
            {
                GameObject panelObject = new GameObject("Inventario",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                panelObject.transform.SetParent(contentArea, false);
                RectTransform panelRT = panelObject.GetComponent<RectTransform>();
                panelRT.anchorMin = Vector2.zero;
                panelRT.anchorMax = Vector2.one;
                panelRT.offsetMin = Vector2.zero;
                panelRT.offsetMax = Vector2.zero;
                panelObject.GetComponent<Image>().color = new Color(0.07f, 0.08f, 0.11f, 0.96f);
                panelObject.SetActive(false);
                panel = panelObject.transform;
                Debug.Log("[Inventory] Inventory panel created.");
            }

            InventoryAppUIController app = panel.GetComponent<InventoryAppUIController>();
            if (app == null)
            {
                app = panel.gameObject.AddComponent<InventoryAppUIController>();
                Debug.Log("[Inventory] InventoryAppUIController attached to Inventario panel.");
            }

            if (helper != null)
                EnsureInventoryButton(helper, panel.gameObject);
        }

        private static void EnsureInventoryButton(UIShopCategoryHelper helper, GameObject panel)
        {
            if (helper == null || panel == null)
                return;

            Transform root = helper.transform.parent != null
                ? helper.transform.parent
                : helper.transform;
            Button[] buttons  = root.GetComponentsInChildren<Button>(true);
            Button template   = null;
            Button existing   = null;

            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null)
                    continue;

                TMP_Text label  = button.GetComponentInChildren<TMP_Text>(true);
                string   text   = label != null
                    ? label.text.Trim().ToUpperInvariant()
                    : string.Empty;

                if (text == "INVENTARIO")
                {
                    existing = button;
                    break;
                }

                if ((text == "CUSTOMIZATION" || text == "BOOSTERS" || text == "EXPANDIR" ||
                     text == "EMPLEADOS"     || text == "LOGROS"    || text == "COMPRA"  ||
                     text == "PRECIOS") && template == null)
                    template = button;
            }

            if (existing != null)
            {
                ConfigureInventoryButton(existing, helper, panel);
                return;
            }

            if (template == null)
                return;

            Button newButton = Object.Instantiate(template, template.transform.parent, false);
            newButton.name   = "InventarioButton";

            TMP_Text newLabel = newButton.GetComponentInChildren<TMP_Text>(true);
            if (newLabel != null)
            {
                newLabel.text  = "INVENTARIO";
                newLabel.color = Color.white;
            }

            Image buttonImage = newButton.GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.color = new Color(0.10f, 0.45f, 0.60f, 0.95f);  // teal tint

            newButton.onClick.RemoveAllListeners();
            ConfigureInventoryButton(newButton, helper, panel);
            ApplyTabButtonCompact(newButton);
            newButton.transform.SetAsLastSibling();
            Debug.Log("[Inventory] Inventory tab button created.");
        }

        private static void ConfigureInventoryButton(Button button,
            UIShopCategoryHelper helper, GameObject panel)
        {
            if (button == null)
                return;

            ExpansionTabButtonLink link = button.GetComponent<ExpansionTabButtonLink>();
            if (link == null)
                link = button.gameObject.AddComponent<ExpansionTabButtonLink>();
            link.Configure(helper, panel);
        }

        // ── Tab-bar compact styling ───────────────────────────────────────────

        /// <summary>
        /// Sets auto-sizing on the label of a newly created tab button so that all our
        /// extra tabs stay within a reasonable width even on smaller screens.
        /// Also constrains the button's RectTransform preferred width.
        /// </summary>
        private static void ApplyTabButtonCompact(Button btn)
        {
            if (btn == null) return;

            TMP_Text lbl = btn.GetComponentInChildren<TMP_Text>(true);
            if (lbl != null)
            {
                lbl.enableAutoSizing   = true;
                lbl.fontSizeMin        = 9f;
                lbl.fontSizeMax        = 13f;
                lbl.fontStyle          = TMPro.FontStyles.Bold;
            }

            // Narrow the button so the row fits without scrolling on common resolutions.
            RectTransform rt = btn.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector2 sd = rt.sizeDelta;
                if (sd.x > TabButtonPreferredWidth)
                    rt.sizeDelta = new Vector2(TabButtonPreferredWidth, sd.y);
            }

            LayoutElement le = btn.GetComponent<LayoutElement>();
            if (le == null)
                le = btn.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = TabButtonPreferredWidth;
            le.minWidth       = TabButtonMinWidth;
        }

        // ── Admin Mode application ────────────────────────────────────────────

        private static bool adminApplyRegistered;

        private static void RegisterAdminApply()
        {
            if (adminApplyRegistered)
                return;

            adminApplyRegistered = true;
            SaveGameSystem.dataLoadEvent += ApplyAdminConfig;
            Debug.Log("[AdminMode] Admin config will be applied after data load.");
        }


        private static void ApplyAdminConfig()
        {
            SaveGameSystem.dataLoadEvent -= ApplyAdminConfig;
            adminApplyRegistered = false;

            if (!AdminSessionConfig.isActive)
                return;

            Debug.Log("[AdminMode] Applying admin session configuration...");

            // ── Money ─────────────────────────────────────────────────────────
            if (AdminSessionConfig.startMoney > 0 && StoreDatabase.Instance != null)
            {
                long current = StoreDatabase.Instance.currentMoney;
                long delta   = AdminSessionConfig.startMoney - current;
                if (delta != 0)
                    StoreDatabase.AddRemoveMoney(delta);
                Debug.Log("[AdminMode] Money set to: " + StoreDatabase.GetMoneyString());
            }

            // ── Tree points ───────────────────────────────────────────────────
            if (AdminSessionConfig.treePoints > 0)
            {
                EntrepreneurTreeManager.SetPoints(AdminSessionConfig.treePoints);
                Debug.Log("[AdminMode] Tree points set to: " + AdminSessionConfig.treePoints);
            }

            // ── Node unlocks ──────────────────────────────────────────────────
            if (AdminSessionConfig.unlockEntireTree)
            {
                EntrepreneurTreeManager.AdminUnlockAll();
                Debug.Log("[AdminMode] All tree nodes force-unlocked.");
            }
            else
            {
                if (AdminSessionConfig.unlockAllProducts)
                {
                    EntrepreneurTreeManager.AdminUnlockByType(TreeNodeType.Product);
                    Debug.Log("[AdminMode] All product nodes force-unlocked.");
                }
                if (AdminSessionConfig.unlockAllEmployees)
                {
                    EntrepreneurTreeManager.AdminUnlockByType(TreeNodeType.Employee);
                    Debug.Log("[AdminMode] All employee nodes force-unlocked.");
                }
            }

            if (UIGame.Instance != null)
                UIGame.AddNotification("[AdminMode] Sesión de prueba iniciada.", otherColor: new Color(1f, 0.7f, 0.1f));
            AdminSessionConfig.Reset();
        }
    }
}
