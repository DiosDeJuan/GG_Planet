using UnityEngine;
using UnityEngine.SceneManagement;

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
        }


        private static EntrepreneurTreeManager FindExistingTreeManager()
        {
            if (cachedManager != null)
                return cachedManager;

            EntrepreneurTreeManager[] managers = Object.FindObjectsOfType<EntrepreneurTreeManager>(true);
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
    }
}
