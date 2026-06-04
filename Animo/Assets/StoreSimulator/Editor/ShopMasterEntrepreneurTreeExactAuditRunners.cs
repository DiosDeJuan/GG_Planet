using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator.Editor
{
    public static class ShopMasterEntrepreneurTreeDesignAuditRunner
    {
        public static void Run() => ShopMasterEntrepreneurTreeExactAuditCore.Run("design");
    }

    public static class ShopMasterEntrepreneurTreeNodeStateAuditRunner
    {
        public static void Run() => ShopMasterEntrepreneurTreeExactAuditCore.Run("state");
    }

    public static class ShopMasterEntrepreneurTreeUnlockRulesAuditRunner
    {
        public static void Run() => ShopMasterEntrepreneurTreeExactAuditCore.Run("rules");
    }

    public static class ShopMasterComputerUIConsistencyAuditRunner
    {
        public static void Run() => ShopMasterEntrepreneurTreeExactAuditCore.Run("consistency");
    }

    [InitializeOnLoad]
    internal static class ShopMasterEntrepreneurTreeExactAuditCore
    {
        private const string ModeKey = "ShopMaster.TreeExactAudit.Mode";
        private const string StateKey = "ShopMaster.TreeExactAudit.State";
        private const string StartKey = "ShopMaster.TreeExactAudit.Start";
        private const string FailureKey = "ShopMaster.TreeExactAudit.Failures";
        private const string ScenePath = "Assets/StoreSimulator/Scenes/Game.unity";

        static ShopMasterEntrepreneurTreeExactAuditCore()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            Application.logMessageReceived -= OnConsole;
            Application.logMessageReceived += OnConsole;
        }

        public static void Run(string mode)
        {
            File.WriteAllText(GetLogPath(mode), "# ShopMaster Entrepreneur Tree exact audit - " + mode + "\n");
            SessionState.SetString(ModeKey, mode);
            SessionState.SetString(StateKey, "enter");
            SessionState.SetFloat(StartKey, 0f);
            SessionState.SetInt(FailureKey, 0);
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
                    string mode = SessionState.GetString(ModeKey, "design");
                    AuditContext ctx = OpenTree();
                    if (mode == "design")
                        RunDesignAudit(ctx);
                    else if (mode == "state")
                        RunNodeStateAudit(ctx);
                    else if (mode == "rules")
                        RunUnlockRulesAudit(ctx);
                    else if (mode == "consistency")
                        RunConsistencyAudit(ctx);
                }
                catch (Exception ex)
                {
                    Fail("Unhandled audit exception: " + ex);
                }

                SessionState.SetString(StateKey, "leave");
                EditorApplication.ExitPlaymode();
                return;
            }

            if (state == "leave" && !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                int failures = SessionState.GetInt(FailureKey, 0);
                Log("Audit finished. Failures=" + failures);
                SessionState.EraseString(StateKey);
                SessionState.EraseString(ModeKey);
                SessionState.EraseFloat(StartKey);
                SessionState.EraseInt(FailureKey);
                EditorApplication.Exit(failures == 0 ? 0 : 1);
            }
        }

        private static AuditContext OpenTree()
        {
            UIShopDesktop desktop = UnityEngine.Object.FindAnyObjectByType<UIShopDesktop>();
            PassIf(desktop != null, "Computer interactable exists.");
            PassIf(desktop != null && desktop.Interact("LeftClick"), "Computer opens in Play Mode.");

            UpgradesUIController upgrades = UnityEngine.Object.FindAnyObjectByType<UpgradesUIController>(FindObjectsInactive.Include);
            PassIf(upgrades != null, "UpgradesUIController exists.");
            InvokeTab("EXPANDIR", "Expandir");

            Button openTree = upgrades != null ? upgrades.transform.Find("OpenEntrepreneurTreeButton")?.GetComponent<Button>() : null;
            PassIf(openTree != null, "Open Entrepreneur Tree button exists.");
            openTree?.onClick.Invoke();
            Canvas.ForceUpdateCanvases();

            return new AuditContext { upgrades = upgrades };
        }

        private static void RunDesignAudit(AuditContext ctx)
        {
            Transform root = ctx.upgrades != null ? ctx.upgrades.transform.Find("EntrepreneurTreeRoot") : null;
            PassIf(root != null && root.gameObject.activeSelf, "Tree root is visible.");
            PassIf(root != null && root.Find("Header/Title")?.GetComponent<TMP_Text>() != null, "Header title exists.");
            PassIf(ctx.upgrades != null && ctx.upgrades.pointsLabel != null && ctx.upgrades.pointsLabel.text.Contains("Puntos disponibles"), "Points counter exists.");
            PassIf(ctx.upgrades != null && ctx.upgrades.infoPanel != null, "Detail panel exists.");
            ScrollRect scroll = root != null ? root.Find("TreeScrollView")?.GetComponent<ScrollRect>() : null;
            PassIf(scroll != null && scroll.content != null && scroll.viewport != null, "ScrollRect exists with viewport and content.");

            NodeUI[] nodes = UnityEngine.Object.FindObjectsByType<NodeUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            PassIf(nodes.Length > 0, "Rendered nodes exist.");
            for (int i = 0; i < nodes.Length; i++)
            {
                RectTransform rt = nodes[i].GetComponent<RectTransform>();
                TMP_Text label = nodes[i].titleLabel;
                PassIf(label != null && !string.IsNullOrWhiteSpace(label.text), "Node has non-empty text: " + NodeId(nodes[i]));
                PassIf(rt != null && rt.rect.width >= 180f && rt.rect.height >= 90f, "Node minimum size valid: " + NodeId(nodes[i]));
            }
        }

        private static void RunNodeStateAudit(AuditContext ctx)
        {
            EntrepreneurTreeManager tree = EntrepreneurTreeManager.Instance;
            PassIf(tree != null && tree.treeData != null, "Tree manager and data exist.");
            tree.LoadFromJSON(null);
            EntrepreneurTreeManager.SetPoints(0);

            PassIf(EntrepreneurTreeManager.IsNodeUnlocked("product_basic_1"), "Productos Basicos 1 starts unlocked.");
            PassIf(!EntrepreneurTreeManager.CanUnlockNode("employee_1"), "Node with missing requirement stays blocked.");
            EntrepreneurTreeManager.SetPoints(1);
            PassIf(EntrepreneurTreeManager.CanUnlockNode("product_basic_2"), "Node with met requirement and point is available.");
            EntrepreneurTreeManager.TryUnlockNodeDetailed("product_basic_2");
            PassIf(EntrepreneurTreeManager.IsNodeUnlocked("product_basic_2"), "Unlocked node remains unlocked.");

            HashSet<string> ids = new HashSet<string>();
            bool duplicates = false;
            foreach (NodeData node in tree.treeData.nodes)
                if (node != null && !ids.Add(node.id))
                    duplicates = true;
            PassIf(!duplicates, "No duplicate node IDs.");
        }

        private static void RunUnlockRulesAudit(AuditContext ctx)
        {
            TreeData data = EntrepreneurTreeManager.Instance != null ? EntrepreneurTreeManager.Instance.treeData : null;
            PassIf(data != null, "Tree data exists.");
            PassRequirement(data, "security_1", "employee_7");
            PassRequirement(data, "security_2", "employee_8");
            PassRequirement(data, "security_3", "employee_14");
            PassRequirement(data, "upgrade_caffeine", "product_fresh_2");
            PassRequirement(data, "upgrade_charismatic", "employee_15");

            bool costsOk = true;
            foreach (NodeData node in data.nodes)
            {
                if (node == null) continue;
                if (node.id == EntrepreneurTreeDefinition.DefaultUnlockedNodeId)
                    costsOk &= node.cost == 0;
                else
                    costsOk &= node.cost == 1;
            }
            PassIf(costsOk, "Starter node costs 0 and all other nodes cost 1.");
        }

        private static void RunConsistencyAudit(AuditContext ctx)
        {
            PassIf(FindContentPanel("Compra") != null, "Compra panel exists.");
            PassIf(FindContentPanel("Empleados") != null, "Empleados panel exists.");
            PassIf(FindContentPanel("Expandir") != null, "Expandir panel exists.");
            PassIf(ctx.upgrades != null && ctx.upgrades.transform.Find("EntrepreneurTreeRoot") != null, "Tree panel exists.");

            Transform root = ctx.upgrades != null ? ctx.upgrades.transform.Find("EntrepreneurTreeRoot") : null;
            bool whiteDefault = false;
            if (root != null)
            {
                Image[] images = root.GetComponentsInChildren<Image>(true);
                for (int i = 0; i < images.Length; i++)
                    if (images[i] != null && images[i].GetComponent<Button>() != null && images[i].color == Color.white)
                        whiteDefault = true;

                TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
                for (int i = 0; i < texts.Length; i++)
                    PassIf(!string.IsNullOrWhiteSpace(texts[i].text), "Tree text is not empty: " + texts[i].name);
            }
            PassIf(!whiteDefault, "No default white buttons inside tree.");

            Transform legacy = ctx.upgrades != null ? ctx.upgrades.transform.Find("Scroll View") : null;
            PassIf(legacy == null || !legacy.gameObject.activeSelf, "Legacy expansion panel is hidden while tree is active.");
        }

        private static void PassRequirement(TreeData data, string nodeId, string requiredId)
        {
            NodeData node = data != null ? data.GetNodeById(nodeId) : null;
            PassIf(node != null && node.requiredNodeIds != null && node.requiredNodeIds.Contains(requiredId),
                nodeId + " requires " + requiredId + ".");
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
                    PassIf(FindContentPanel(panelName)?.gameObject.activeSelf == true, "Computer tab opens panel: " + panelName);
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

        private static string NodeId(NodeUI node)
        {
            return node != null && node.data != null ? node.data.id : "unknown";
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
            File.AppendAllText(GetLogPath(SessionState.GetString(ModeKey, "design")), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + "\n");
        }

        private static void OnConsole(string condition, string stackTrace, LogType type)
        {
            string state = SessionState.GetString(StateKey, string.Empty);
            if (string.IsNullOrEmpty(state) || state == "leave")
                return;
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                Fail("Console " + type + ": " + condition + "\n" + stackTrace);
        }

        private static string GetLogPath(string mode)
        {
            switch (mode)
            {
                case "state":
                    return "Documentos/TreeNodeStateAudit_Report.txt";
                case "rules":
                    return "Documentos/TreeUnlockRulesAudit_Report.txt";
                case "consistency":
                    return "Documentos/ComputerUIConsistencyAudit_Report.txt";
                default:
                    return "Documentos/TreeDesignAudit_Report.txt";
            }
        }

        private struct AuditContext
        {
            public UpgradesUIController upgrades;
        }
    }
}
