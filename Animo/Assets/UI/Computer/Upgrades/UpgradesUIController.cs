// Árbol del Emprendedor — UpgradesUIController
// Builds the scrollable node graph and manages the info/detail panel.
// Attach this to the root panel of the UPGRADES tab inside the UIShopDesktop canvas.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Controller for the UPGRADES tab of the in-game computer (UIShopDesktop).
    ///
    /// What it does:
    ///   • On activation (OnEnable) it reads the TreeData from EntrepreneurTreeManager
    ///     and spawns one NodeUI prefab per node and one ConnectionLineUI prefab per dependency edge.
    ///   • Provides ShowNodeInfo / HideNodeInfo called by NodeUI on hover.
    ///   • Keeps the points label up to date.
    ///
    /// ─────────────────────────────────────────────────────────────────────────────
    /// PREFAB / SCENE SETUP (one-time, in the Unity Editor):
    ///
    ///   1.  Inside the UIShopDesktop prefab, find the panel shown for the "UPGRADES" tab.
    ///       Add a child GameObject called "EntrepreneurTreePanel" and attach this script.
    ///
    ///   2.  Inside "EntrepreneurTreePanel" build the following hierarchy:
    ///
    ///         EntrepreneurTreePanel
    ///           ├─ PointsLabel          (TMP_Text)          → assign to "pointsLabel"
    ///           ├─ ScrollView           (ScrollRect + Mask)
    ///           │    └─ Viewport
    ///           │         └─ Content    (RectTransform)     → assign to "treeScrollContent"
    ///           └─ InfoPanel            (Panel, starts off)  → assign to "infoPanel"
    ///                ├─ InfoTitle       (TMP_Text)          → assign to "infoTitle"
    ///                ├─ InfoDescription (TMP_Text)          → assign to "infoDescription"
    ///                ├─ InfoCost        (TMP_Text)          → assign to "infoCost"
    ///                ├─ InfoRequirements(TMP_Text)          → assign to "infoRequirements"
    ///                └─ UnlockButton    (Button)            → assign to "infoUnlockButton"
    ///
    ///   3.  Create the NodePrefab:
    ///         • UI > Image (name it "NodePrefab"), size e.g. 100×100 px.
    ///         • Add child Image for icon (name "Icon").
    ///         • Add child TMP_Text for label (name "Label").
    ///         • Attach NodeUI script; assign background, iconImage and titleLabel references.
    ///         • Set the RectTransform anchor to (0.5, 0.5) so anchoredPosition places it correctly.
    ///       Assign the prefab to "nodePrefab" on this controller.
    ///
    ///   4.  Create the LinePrefab:
    ///         • UI > Image (name it "LinePrefab"), size e.g. 100×3 px, colour white.
    ///         • Attach ConnectionLineUI script.
    ///         • Set the RectTransform anchor to (0.5, 0.5).
    ///       Assign the prefab to "linePrefab" on this controller.
    ///
    ///   5.  Create the TreeData ScriptableObject (Create > EntrepreneurTree > TreeData),
    ///       populate it with NodeData assets, and assign it to EntrepreneurTreeManager.treeData.
    /// ─────────────────────────────────────────────────────────────────────────────
    /// </summary>
    public class UpgradesUIController : MonoBehaviour
    {
        // ── Inspector fields ──────────────────────────────────────────────────────

        [Header("Tree Canvas")]
        /// <summary>Content RectTransform of the ScrollRect that holds the node graph.</summary>
        public RectTransform treeScrollContent;

        /// <summary>Prefab with a NodeUI component representing one tree node.</summary>
        public GameObject nodePrefab;

        /// <summary>Prefab with a ConnectionLineUI + Image component for edges between nodes.</summary>
        public GameObject linePrefab;

        [Header("Info Panel")]
        /// <summary>Root object of the info panel; toggled on hover.</summary>
        public GameObject infoPanel;

        /// <summary>Displays the hovered node's title.</summary>
        public TMP_Text infoTitle;

        /// <summary>Displays the hovered node's description.</summary>
        public TMP_Text infoDescription;

        /// <summary>Displays the point cost of the hovered node.</summary>
        public TMP_Text infoCost;

        /// <summary>Lists the prerequisite nodes and their completion status.</summary>
        public TMP_Text infoRequirements;

        /// <summary>Button in the info panel that attempts to unlock the selected node.</summary>
        public Button infoUnlockButton;

        [Header("Points")]
        /// <summary>Label showing the player's current progress-point total.</summary>
        public TMP_Text pointsLabel;


        // ── Runtime ───────────────────────────────────────────────────────────────

        // Node ID → NodeUI instance for O(1) edge lookup.
        private Dictionary<string, NodeUI> nodeUIMap = new Dictionary<string, NodeUI>();

        // The node whose info panel is currently open (hover).
        private NodeData currentInfoNode;

        // Guard so BuildTree only runs once per activation when already populated.
        private bool treeBuilt = false;


        // ── Unity callbacks ───────────────────────────────────────────────────────

        void Awake()
        {
            if (infoPanel) infoPanel.SetActive(false);
            if (infoUnlockButton) infoUnlockButton.onClick.AddListener(OnUnlockButtonClicked);

            EntrepreneurTreeManager.onPointsChanged += OnPointsChanged;
            EntrepreneurTreeManager.onNodeUnlocked  += OnNodeUnlocked;
        }


        // Rebuild the tree every time the Upgrades tab becomes visible.
        // This makes the first activation seamless regardless of the load order.
        void OnEnable()
        {
            if (!treeBuilt) BuildTree();
            RefreshPointsLabel();
        }


        // ── Tree construction ─────────────────────────────────────────────────────

        /// <summary>
        /// Instantiates all NodeUI and ConnectionLineUI objects from the TreeData.
        /// Destroys any previously created children first (safe to call multiple times).
        /// </summary>
        public void BuildTree()
        {
            if (EntrepreneurTreeManager.Instance == null ||
                EntrepreneurTreeManager.Instance.treeData == null)
                return;

            // Tear down previous graph.
            if (treeScrollContent != null)
                foreach (Transform child in treeScrollContent)
                    Destroy(child.gameObject);

            nodeUIMap.Clear();
            treeBuilt = true;

            TreeData tree = EntrepreneurTreeManager.Instance.treeData;

            // ── Pass 1: spawn nodes ───────────────────────────────────────────────
            foreach (NodeData nodeData in tree.nodes)
            {
                if (nodePrefab == null) continue;

                GameObject nodeObj = Instantiate(nodePrefab, treeScrollContent, false);
                NodeUI     nodeUI  = nodeObj.GetComponent<NodeUI>();
                if (nodeUI == null) continue;

                // Position the node in the scroll canvas.
                RectTransform rt = nodeObj.GetComponent<RectTransform>();
                rt.anchoredPosition = nodeData.uiPosition;

                nodeUI.Initialize(nodeData, this);
                nodeUIMap[nodeData.id] = nodeUI;
            }

            // ── Pass 2: spawn edges ───────────────────────────────────────────────
            foreach (NodeData nodeData in tree.nodes)
            {
                foreach (string reqId in nodeData.requiredNodeIds)
                {
                    if (!nodeUIMap.ContainsKey(reqId) || !nodeUIMap.ContainsKey(nodeData.id))
                        continue;
                    if (linePrefab == null) continue;

                    GameObject lineObj = Instantiate(linePrefab, treeScrollContent, false);
                    // Render behind nodes.
                    lineObj.transform.SetAsFirstSibling();

                    ConnectionLineUI line = lineObj.GetComponent<ConnectionLineUI>();
                    line?.Initialize(nodeUIMap[reqId], nodeUIMap[nodeData.id]);
                }
            }
        }


        // ── Info panel ────────────────────────────────────────────────────────────

        /// <summary>
        /// Opens the info panel and populates it with data from the given node.
        /// Called by NodeUI.OnPointerEnter.
        /// </summary>
        public void ShowNodeInfo(NodeData node)
        {
            if (node == null || infoPanel == null) return;
            currentInfoNode = node;

            if (infoTitle)       infoTitle.text       = node.title;
            if (infoDescription) infoDescription.text = node.description;
            if (infoCost)        infoCost.text         = "Cost: " + node.cost +
                                                         " point" + (node.cost != 1 ? "s" : "");

            // Build prerequisite list with status ticks.
            if (infoRequirements)
            {
                if (node.requiredNodeIds.Count == 0)
                {
                    infoRequirements.text = "No prerequisites";
                }
                else
                {
                    var lines = new System.Text.StringBuilder("Requires:\n");
                    foreach (string reqId in node.requiredNodeIds)
                    {
                        NodeData req    = EntrepreneurTreeManager.Instance?.treeData.GetNodeById(reqId);
                        string   name   = req != null ? req.title : reqId;
                        string   status = (req != null && req.isUnlocked) ? " ✓" : " ✗";
                        lines.AppendLine(name + status);
                    }
                    infoRequirements.text = lines.ToString().TrimEnd();
                }
            }

            // Show unlock button only when the node is still locked.
            if (infoUnlockButton)
            {
                infoUnlockButton.gameObject.SetActive(!node.isUnlocked);
                infoUnlockButton.interactable = EntrepreneurTreeManager.CanUnlockNode(node.id);
            }

            infoPanel.SetActive(true);
        }


        /// <summary>
        /// Hides the info panel.
        /// Called by NodeUI.OnPointerExit.
        /// </summary>
        public void HideNodeInfo()
        {
            if (infoPanel) infoPanel.SetActive(false);
            currentInfoNode = null;
        }


        // ── Event handlers ────────────────────────────────────────────────────────

        // Unlock button pressed inside the info panel.
        private void OnUnlockButtonClicked()
        {
            if (currentInfoNode == null) return;
            EntrepreneurTreeManager.TryUnlockNode(currentInfoNode.id);
            // Refresh the panel so the button hides if the unlock succeeded.
            ShowNodeInfo(currentInfoNode);
        }


        // Points changed — refresh label and unlock-button interactability.
        private void OnPointsChanged(int total, int change)
        {
            if (pointsLabel) pointsLabel.text = "Points: " + total;

            if (infoPanel && infoPanel.activeSelf && currentInfoNode != null && infoUnlockButton)
                infoUnlockButton.interactable = EntrepreneurTreeManager.CanUnlockNode(currentInfoNode.id);
        }


        // Any node was unlocked — refresh info panel if it is open.
        private void OnNodeUnlocked(NodeData _)
        {
            if (infoPanel && infoPanel.activeSelf && currentInfoNode != null)
                ShowNodeInfo(currentInfoNode);
        }


        // ── Helpers ───────────────────────────────────────────────────────────────

        private void RefreshPointsLabel()
        {
            if (pointsLabel && EntrepreneurTreeManager.Instance != null)
                pointsLabel.text = "Points: " + EntrepreneurTreeManager.Instance.currentPoints;
        }


        void OnDestroy()
        {
            EntrepreneurTreeManager.onPointsChanged -= OnPointsChanged;
            EntrepreneurTreeManager.onNodeUnlocked  -= OnNodeUnlocked;
        }
    }
}
