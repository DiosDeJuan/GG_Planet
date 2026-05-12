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
    }
}
