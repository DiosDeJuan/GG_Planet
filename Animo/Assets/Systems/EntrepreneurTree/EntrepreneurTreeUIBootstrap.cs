using System.Collections.Generic;
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

                Debug.Log(LogPrefix + "Created runtime EntrepreneurTree systems GameObject.");
            }

            if (manager.treeData == null)
            {
                manager.treeData = CreateFallbackTreeData();
                Debug.LogWarning(LogPrefix + "TreeData not assigned in Inspector. Using runtime fallback tree data.");
            }
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

            List<NodeData> nodes = new List<NodeData>();
            nodes.Add(CreateNode(
                "productos_basicos_1",
                "Productos Básicos 1",
                "Desbloquea la primera capa de productos esenciales para tu supermercado.",
                TreeNodeType.Product,
                0,
                new string[0],
                new Vector2(-920f, 0f)));

            nodes.Add(CreateNode(
                "productos_basicos_2",
                "Productos Básicos 2",
                "Amplía el surtido básico para mejorar ventas iniciales.",
                TreeNodeType.Product,
                1,
                new[] { "productos_basicos_1" },
                new Vector2(-560f, 0f)));

            nodes.Add(CreateNode(
                "productos_frescos_1",
                "Frescos 1",
                "Añade productos frescos para atraer más clientes.",
                TreeNodeType.Product,
                2,
                new[] { "productos_basicos_2" },
                new Vector2(-220f, 0f)));

            nodes.Add(CreateNode(
                "empleado_cajero_1",
                "Cajero 1",
                "Permite contratar un primer cajero para acelerar cobros.",
                TreeNodeType.Employee,
                2,
                new[] { "productos_basicos_2" },
                new Vector2(-560f, -260f)));

            nodes.Add(CreateNode(
                "seguridad_camaras_1",
                "Cámaras 1",
                "Instala cámaras para reducir pérdidas y mejorar control.",
                TreeNodeType.Security,
                2,
                new[] { "productos_basicos_2" },
                new Vector2(-560f, 260f)));

            nodes.Add(CreateNode(
                "mejora_checkout_1",
                "Checkout Ágil",
                "Optimiza el flujo de caja para atender más clientes por hora.",
                TreeNodeType.Improvement,
                3,
                new[] { "empleado_cajero_1", "productos_frescos_1" },
                new Vector2(180f, -120f)));

            runtimeFallbackTree.nodes = nodes;
            return runtimeFallbackTree;
        }


        private static NodeData CreateNode(
            string id,
            string title,
            string description,
            TreeNodeType type,
            int cost,
            string[] requiredIds,
            Vector2 position)
        {
            NodeData node = ScriptableObject.CreateInstance<NodeData>();
            node.name = id;
            node.id = id;
            node.title = title;
            node.description = description;
            node.nodeType = type;
            node.cost = cost;
            node.uiPosition = position;
            node.requiredNodeIds = new List<string>(requiredIds);
            return node;
        }
    }
}
