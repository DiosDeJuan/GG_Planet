// Árbol del Emprendedor — UpgradesUIController
// Builds and manages the Entrepreneur Tree UI inside the computer's UPGRADES tab.

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FLOBUK.StoreSimulator
{
    [DisallowMultipleComponent]
    public class UpgradesUIController : MonoBehaviour
    {
        private const string LogPrefix = "[EntrepreneurTree] ";

        [Header("Tree Canvas")]
        public RectTransform treeScrollContent;
        public GameObject nodePrefab;
        public GameObject linePrefab;

        [Header("Info Panel")]
        public GameObject infoPanel;
        public TMP_Text infoTitle;
        public TMP_Text infoDescription;
        public TMP_Text infoCost;
        public TMP_Text infoRequirements;
        public Button infoUnlockButton;

        [Header("Points")]
        public TMP_Text pointsLabel;

        [Header("Optional Legacy Content")]
        [SerializeField] private GameObject legacyUpgradesContent;

        private readonly Dictionary<string, NodeUI> nodeUIMap = new Dictionary<string, NodeUI>();

        private NodeData currentInfoNode;
        private RectTransform linesContainer;
        private RectTransform nodesContainer;
        private GameObject treeRootObject;
        private ScrollRect treeScrollRect;
        private Button openTreeButton;
        private Button backToLegacyButton;

        private bool treeBuilt;
        private bool treeEventsBound;
        private bool unlockButtonListenerBound;
        private bool openTreeButtonListenerBound;
        private bool backTreeButtonListenerBound;
        private bool autoConfigured;
        private int lastFocusFrame = -1;


        void Awake()
        {
            TryAutoConfigureFromHierarchy();
            BindListeners();

            if (infoPanel) infoPanel.SetActive(false);
            RefreshPointsLabel();
        }


        void OnEnable()
        {
            TryAutoConfigureFromHierarchy();

            if (!treeBuilt)
                BuildTree();
            else
                RefreshNodeStates();

            RefreshPointsLabel();
        }


        public void SetLegacyContent(GameObject oldContent)
        {
            legacyUpgradesContent = oldContent;
        }


        public void TryAutoConfigureFromHierarchy()
        {
            if (autoConfigured && treeScrollContent != null && linesContainer != null && nodesContainer != null)
                return;

            EnsureTreeRootHierarchy();
            EnsureContainers();

            autoConfigured = treeScrollContent != null && linesContainer != null && nodesContainer != null;

            if (legacyUpgradesContent == null)
            {
                Transform oldScroll = transform.Find("Scroll View");
                if (oldScroll != null && oldScroll.gameObject != treeScrollContent?.gameObject)
                    legacyUpgradesContent = oldScroll.gameObject;
            }
        }


        public void BuildTree()
        {
            if (treeBuilt && nodeUIMap.Count > 0)
            {
                ShowTreeView();
                RefreshNodeStates();
                FocusDefaultNode();
                Debug.Log(LogPrefix + "BuildTree skipped. Existing tree reused.");
                return;
            }

            if (!ValidateTreePrerequisites())
            {
                ToggleLegacyContent(true);
                return;
            }

            ClearChildren(linesContainer);
            ClearChildren(nodesContainer);
            nodeUIMap.Clear();

            TreeData tree = EntrepreneurTreeManager.Instance.treeData;
            int createdNodes = 0;
            int createdLines = 0;
            Debug.Log(LogPrefix + "Tree data loaded: " + (tree != null && tree.nodes != null ? tree.nodes.Count : 0) + " nodes.");

            for (int i = 0; i < tree.nodes.Count; i++)
            {
                NodeData nodeData = tree.nodes[i];
                if (nodeData == null || string.IsNullOrEmpty(nodeData.id))
                    continue;

                GameObject nodeObj = CreateNodeObject();
                if (nodeObj == null)
                    continue;

                RectTransform rt = nodeObj.GetComponent<RectTransform>();
                rt.anchoredPosition = nodeData.uiPosition;
                Debug.Log(LogPrefix + "Rendering node: " + nodeData.id + " at " + nodeData.uiPosition.x + "/" + nodeData.uiPosition.y + ".");

                NodeUI nodeUI = nodeObj.GetComponent<NodeUI>();
                if (nodeUI == null)
                {
                    Debug.LogWarning(LogPrefix + "Node prefab missing, using runtime fallback.");
                    nodeUI = EnsureFallbackNodeVisuals(nodeObj);
                    if (nodeUI == null)
                        continue;
                }

                nodeUI.Initialize(nodeData, this);
                nodeUIMap[nodeData.id] = nodeUI;
                createdNodes++;
            }

            for (int i = 0; i < tree.nodes.Count; i++)
            {
                NodeData nodeData = tree.nodes[i];
                if (nodeData == null || nodeData.requiredNodeIds == null)
                    continue;

                for (int r = 0; r < nodeData.requiredNodeIds.Count; r++)
                {
                    string reqId = nodeData.requiredNodeIds[r];
                    if (!nodeUIMap.ContainsKey(reqId) || !nodeUIMap.ContainsKey(nodeData.id))
                        continue;

                    GameObject lineObj = CreateLineObject();
                    if (lineObj == null)
                        continue;

                    ConnectionLineUI line = lineObj.GetComponent<ConnectionLineUI>();
                    if (line == null)
                        continue;

                    line.Initialize(nodeUIMap[reqId], nodeUIMap[nodeData.id]);
                    createdLines++;
                }
            }

            treeBuilt = true;
            if (legacyUpgradesContent != null)
                ShowLegacyView();
            else
                ShowTreeView();
            FocusDefaultNode();
            Debug.Log(LogPrefix + "Tree render complete: " + createdNodes + " nodes, " + createdLines + " connections.");
        }


        public void ShowNodeInfo(NodeData node)
        {
            if (node == null || infoPanel == null)
                return;

            currentInfoNode = node;

            if (infoTitle) infoTitle.text = node.title;
            if (infoDescription)
            {
                string typeLabel = "Tipo: " + GetNodeTypeLabel(node.nodeType);
                if (!string.IsNullOrEmpty(node.description))
                    infoDescription.text = typeLabel + "\n" + node.description;
                else
                    infoDescription.text = typeLabel;
            }
            if (infoCost) infoCost.text = node.isUnlocked
                ? "Estado: Desbloqueado"
                : "Costo: " + node.cost + " punto" + (node.cost != 1 ? "s" : "");

            if (infoRequirements)
            {
                if (node.requiredNodeIds == null || node.requiredNodeIds.Count == 0)
                {
                    infoRequirements.text = "Sin requisitos";
                }
                else
                {
                    StringBuilder sb = new StringBuilder("Requisitos:\n");
                    for (int i = 0; i < node.requiredNodeIds.Count; i++)
                    {
                        string reqId = node.requiredNodeIds[i];
                        NodeData reqNode = EntrepreneurTreeManager.Instance != null && EntrepreneurTreeManager.Instance.treeData != null
                            ? EntrepreneurTreeManager.Instance.treeData.GetNodeById(reqId)
                            : null;

                        string title = reqNode != null ? reqNode.title : reqId;
                        bool unlocked = reqNode != null && reqNode.isUnlocked;
                        sb.Append(title).Append(unlocked ? " [OK]" : " [NO]");
                        if (i < node.requiredNodeIds.Count - 1)
                            sb.AppendLine();
                    }
                    infoRequirements.text = sb.ToString();
                }
            }

            if (infoUnlockButton)
            {
                infoUnlockButton.gameObject.SetActive(!node.isUnlocked);
                bool canUnlock = EntrepreneurTreeManager.CanUnlockNode(node.id);
                infoUnlockButton.interactable = canUnlock;

                Image buttonImage = infoUnlockButton.targetGraphic as Image;
                if (buttonImage != null)
                    buttonImage.color = canUnlock ? new Color(0.15f, 0.45f, 0.2f, 1f) : new Color(0.25f, 0.25f, 0.25f, 1f);
            }

            infoPanel.SetActive(true);
        }


        public void HideNodeInfo()
        {
            if (infoPanel) infoPanel.SetActive(false);
            currentInfoNode = null;
        }


        private void OnUnlockButtonClicked()
        {
            if (currentInfoNode == null)
                return;

            EntrepreneurTreeManager.TryUnlockNode(currentInfoNode.id);
            ShowNodeInfo(currentInfoNode);
        }


        private void OnPointsChanged(int total, int change)
        {
            if (pointsLabel)
                pointsLabel.text = "Puntos disponibles: " + total;

            if (infoPanel && infoPanel.activeSelf && currentInfoNode != null && infoUnlockButton)
                infoUnlockButton.interactable = EntrepreneurTreeManager.CanUnlockNode(currentInfoNode.id);
        }


        private void OnNodeUnlocked(NodeData _)
        {
            RefreshNodeStates();

            if (infoPanel && infoPanel.activeSelf && currentInfoNode != null)
                ShowNodeInfo(currentInfoNode);
        }


        private void RefreshPointsLabel()
        {
            if (pointsLabel && EntrepreneurTreeManager.Instance != null)
                pointsLabel.text = "Puntos disponibles: " + EntrepreneurTreeManager.Instance.currentPoints;
        }


        private void RefreshNodeStates()
        {
            foreach (NodeUI node in nodeUIMap.Values)
            {
                if (node != null)
                    node.Refresh();
            }
        }


        private void BindListeners()
        {
            if (!treeEventsBound)
            {
                EntrepreneurTreeManager.onPointsChanged += OnPointsChanged;
                EntrepreneurTreeManager.onNodeUnlocked += OnNodeUnlocked;
                treeEventsBound = true;
            }

            if (infoUnlockButton && !unlockButtonListenerBound)
            {
                infoUnlockButton.onClick.AddListener(OnUnlockButtonClicked);
                unlockButtonListenerBound = true;
            }

            if (openTreeButton != null && !openTreeButtonListenerBound)
            {
                openTreeButton.onClick.AddListener(ShowTreeView);
                openTreeButtonListenerBound = true;
            }

            if (backToLegacyButton != null && !backTreeButtonListenerBound)
            {
                backToLegacyButton.onClick.AddListener(ShowLegacyView);
                backTreeButtonListenerBound = true;
            }
        }


        private bool ValidateTreePrerequisites()
        {
            if (EntrepreneurTreeManager.Instance == null)
            {
                Debug.LogWarning(LogPrefix + "EntrepreneurTreeManager not found. Keeping legacy UPGRADES content visible.");
                return false;
            }

            if (EntrepreneurTreeManager.Instance.treeData == null)
            {
                Debug.LogWarning(LogPrefix + "TreeData is not assigned. Keeping legacy UPGRADES content visible.");
                return false;
            }

            if (treeScrollContent == null || linesContainer == null || nodesContainer == null)
            {
                Debug.LogWarning(LogPrefix + "Tree UI references missing. Auto-configuration failed.");
                return false;
            }

            return true;
        }


        private void EnsureTreeRootHierarchy()
        {
            Transform rootTransform = transform.Find("EntrepreneurTreeRoot");
            if (rootTransform == null)
            {
                rootTransform = CreateTreeRoot();
                Debug.Log(LogPrefix + "EntrepreneurTreeRoot created.");
            }
            else
            {
                Debug.Log(LogPrefix + "Existing EntrepreneurTreeRoot reused.");
            }

            int rootCount = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child == null || child.name != "EntrepreneurTreeRoot")
                    continue;

                rootCount++;
                if (rootCount > 1)
                {
                    child.gameObject.SetActive(false);
                    Debug.LogWarning(LogPrefix + "Skipping duplicate tree root.");
                }
            }

            if (rootTransform == null)
                return;

            pointsLabel = pointsLabel != null ? pointsLabel : FindText(rootTransform, "PointsText");

            Transform info = rootTransform.Find("InfoPanel");
            if (info != null)
            {
                infoPanel = infoPanel != null ? infoPanel : info.gameObject;
                infoTitle = infoTitle != null ? infoTitle : FindText(info, "NodeTitleText");
                infoDescription = infoDescription != null ? infoDescription : FindText(info, "NodeDescriptionText");
                infoCost = infoCost != null ? infoCost : FindText(info, "NodeCostText");
                infoRequirements = infoRequirements != null ? infoRequirements : FindText(info, "RequirementsText");

                if (infoUnlockButton == null)
                {
                    Transform unlockButton = info.Find("UnlockButton");
                    if (unlockButton != null)
                        infoUnlockButton = unlockButton.GetComponent<Button>();
                }
            }

            if (treeScrollContent == null)
            {
                Transform content = rootTransform.Find("TreeScrollView/Viewport/Content");
                if (content != null)
                    treeScrollContent = content as RectTransform;
            }
            if (treeScrollRect == null)
            {
                Transform scroll = rootTransform.Find("TreeScrollView");
                if (scroll != null)
                    treeScrollRect = scroll.GetComponent<ScrollRect>();
            }

            treeRootObject = rootTransform.gameObject;
            Debug.Log(LogPrefix + "Tree root resolved: " + rootTransform.name + ".");
            Debug.Log(LogPrefix + "Content parent resolved: " + (treeScrollContent != null ? treeScrollContent.name : "null") + ".");
            EnsureTreeOpenButton();
            BindListeners();
        }


        private void EnsureContainers()
        {
            if (treeScrollContent == null)
                return;

            Transform lines = treeScrollContent.Find("ConnectionLinesContainer");
            if (lines == null)
            {
                GameObject linesObj = CreateUIObject("ConnectionLinesContainer", treeScrollContent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
                lines = linesObj.transform;
            }

            Transform nodes = treeScrollContent.Find("NodesContainer");
            if (nodes == null)
            {
                GameObject nodesObj = CreateUIObject("NodesContainer", treeScrollContent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
                nodes = nodesObj.transform;
            }

            linesContainer = lines as RectTransform;
            nodesContainer = nodes as RectTransform;
        }


        private Transform CreateTreeRoot()
        {
            GameObject rootObj = CreateUIObject("EntrepreneurTreeRoot", transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            Image rootBg = rootObj.AddComponent<Image>();
            rootBg.color = new Color(0.05f, 0.07f, 0.1f, 0.92f);

            RectTransform rootRT = rootObj.GetComponent<RectTransform>();
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            GameObject header = CreateUIObject("Header", rootObj.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
            RectTransform headerRT = header.GetComponent<RectTransform>();
            headerRT.sizeDelta = new Vector2(0f, 72f);
            Image headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0.1f, 0.12f, 0.16f, 0.96f);

            CreateTextObject("Title", header.transform, "Árbol del Emprendedor", 30, TextAlignmentOptions.Left,
                new Vector2(0f, 0f), new Vector2(0.7f, 1f), new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(-20f, 0f));

            pointsLabel = CreateTextObject("PointsText", header.transform, "Puntos disponibles: 0", 26, TextAlignmentOptions.Right,
                new Vector2(0.7f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(-20f, 0f));

            GameObject backButton = CreateUIObject("BackToExpansionsButton", header.transform, new Vector2(0.7f, 0f), new Vector2(0.85f, 1f), new Vector2(0.5f, 0.5f));
            Image backImage = backButton.AddComponent<Image>();
            backImage.color = new Color(0.2f, 0.28f, 0.38f, 1f);
            backToLegacyButton = backButton.AddComponent<Button>();
            backToLegacyButton.targetGraphic = backImage;
            CreateTextObject("Text", backButton.transform, "Volver", 18, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            GameObject info = CreateUIObject("InfoPanel", rootObj.transform, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f));
            RectTransform infoRT = info.GetComponent<RectTransform>();
            infoRT.sizeDelta = new Vector2(360f, -96f);
            infoRT.anchoredPosition = new Vector2(-10f, -36f);
            Image infoBg = info.AddComponent<Image>();
            infoBg.color = new Color(0.08f, 0.1f, 0.14f, 0.98f);

            VerticalLayoutGroup infoLayout = info.AddComponent<VerticalLayoutGroup>();
            infoLayout.padding = new RectOffset(20, 20, 20, 20);
            infoLayout.spacing = 12;
            infoLayout.childControlHeight = false;
            infoLayout.childControlWidth = true;
            infoLayout.childForceExpandHeight = false;
            infoLayout.childForceExpandWidth = true;

            ContentSizeFitter infoFitter = info.AddComponent<ContentSizeFitter>();
            infoFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            infoTitle = CreateTextObject("NodeTitleText", info.transform, "", 24, TextAlignmentOptions.Left,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
            infoDescription = CreateTextObject("NodeDescriptionText", info.transform, "", 20, TextAlignmentOptions.TopLeft,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
            infoCost = CreateTextObject("NodeCostText", info.transform, "", 20, TextAlignmentOptions.Left,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
            infoRequirements = CreateTextObject("RequirementsText", info.transform, "", 18, TextAlignmentOptions.TopLeft,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);

            GameObject unlock = CreateUIObject("UnlockButton", info.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f));
            RectTransform unlockRT = unlock.GetComponent<RectTransform>();
            unlockRT.sizeDelta = new Vector2(0f, 52f);
            Image unlockBg = unlock.AddComponent<Image>();
            unlockBg.color = new Color(0.15f, 0.45f, 0.2f, 1f);
            infoUnlockButton = unlock.AddComponent<Button>();
            infoUnlockButton.targetGraphic = unlockBg;

            CreateTextObject("Text", unlock.transform, "Desbloquear", 22, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            GameObject treeScroll = CreateUIObject("TreeScrollView", rootObj.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f));
            RectTransform scrollRT = treeScroll.GetComponent<RectTransform>();
            scrollRT.offsetMin = new Vector2(18f, 18f);
            scrollRT.offsetMax = new Vector2(-380f, -90f);

            Image scrollBg = treeScroll.AddComponent<Image>();
            scrollBg.color = new Color(0.11f, 0.13f, 0.17f, 0.9f);
            ScrollRect scrollRect = treeScroll.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 24f;
            treeScrollRect = scrollRect;

            GameObject viewport = CreateUIObject("Viewport", treeScroll.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.offsetMin = new Vector2(6f, 6f);
            viewportRT.offsetMax = new Vector2(-6f, -6f);
            Image viewportBg = viewport.AddComponent<Image>();
            viewportBg.color = new Color(0.06f, 0.08f, 0.11f, 0.88f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            GameObject content = CreateUIObject("Content", viewport.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            treeScrollContent = content.GetComponent<RectTransform>();
            treeScrollContent.sizeDelta = new Vector2(2600f, 1400f);
            treeScrollContent.anchoredPosition = new Vector2(140f, -140f);

            scrollRect.viewport = viewportRT;
            scrollRect.content = treeScrollContent;

            if (infoPanel == null)
                infoPanel = info;

            info.SetActive(false);
            return rootObj.transform;
        }


        private void FocusDefaultNode()
        {
            if (treeScrollRect == null || treeScrollContent == null || EntrepreneurTreeManager.Instance == null)
                return;
            if (lastFocusFrame == Time.frameCount)
                return;

            if (!nodeUIMap.TryGetValue(EntrepreneurTreeDefinition.DefaultUnlockedNodeId, out NodeUI defaultNode) || defaultNode == null)
                return;

            RectTransform viewport = treeScrollRect.viewport;
            RectTransform nodeRT = defaultNode.GetComponent<RectTransform>();
            if (viewport == null || nodeRT == null)
                return;

            Canvas.ForceUpdateCanvases();

            float contentWidth = Mathf.Max(treeScrollContent.rect.width, treeScrollContent.sizeDelta.x);
            float contentHeight = Mathf.Max(treeScrollContent.rect.height, treeScrollContent.sizeDelta.y);
            float viewportWidth = viewport.rect.width;
            float viewportHeight = viewport.rect.height;
            if (contentWidth <= 0f || contentHeight <= 0f || viewportWidth <= 0f || viewportHeight <= 0f)
                return;

            Vector2 nodePos = nodeRT.anchoredPosition;
            float nodeXFromLeft = nodePos.x + (contentWidth * 0.5f);
            float nodeYFromBottom = nodePos.y + (contentHeight * 0.5f);
            float horizontalRange = Mathf.Max(1f, contentWidth - viewportWidth);
            float verticalRange = Mathf.Max(1f, contentHeight - viewportHeight);
            float hNormalized = contentWidth <= viewportWidth
                ? 0.5f
                : Mathf.Clamp01((nodeXFromLeft - (viewportWidth * 0.5f)) / horizontalRange);
            float vNormalized = contentHeight <= viewportHeight
                ? 0.5f
                : Mathf.Clamp01((nodeYFromBottom - (viewportHeight * 0.5f)) / verticalRange);

            treeScrollRect.horizontalNormalizedPosition = hNormalized;
            treeScrollRect.verticalNormalizedPosition = vNormalized;
            lastFocusFrame = Time.frameCount;
            Debug.Log(LogPrefix + "Focused default node: " + EntrepreneurTreeDefinition.DefaultUnlockedNodeId + ".");
        }


        private GameObject CreateNodeObject()
        {
            if (nodePrefab != null)
                return Instantiate(nodePrefab, nodesContainer, false);

            GameObject nodeObj = CreateUIObject("Node", nodesContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            RectTransform rt = nodeObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(180f, 88f);

            Image background = nodeObj.AddComponent<Image>();
            background.color = new Color(0.35f, 0.35f, 0.35f, 1f);
            background.raycastTarget = true;

            NodeUI nodeUI = nodeObj.AddComponent<NodeUI>();

            GameObject iconObj = CreateUIObject("Icon", nodeObj.transform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f));
            RectTransform iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.sizeDelta = new Vector2(46f, 46f);
            iconRT.anchoredPosition = new Vector2(30f, 0f);
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.raycastTarget = false;

            TMP_Text label = CreateTextObject("Label", nodeObj.transform, "", 18, TextAlignmentOptions.Left,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(64f, 0f), new Vector2(-12f, 0f));

            nodeUI.background = background;
            nodeUI.iconImage = iconImage;
            nodeUI.titleLabel = label;

            return nodeObj;
        }

        private NodeUI EnsureFallbackNodeVisuals(GameObject nodeObj)
        {
            if (nodeObj == null)
                return null;

            Image background = nodeObj.GetComponent<Image>();
            if (background == null)
                background = nodeObj.AddComponent<Image>();
            background.color = new Color(0.35f, 0.35f, 0.35f, 1f);
            background.raycastTarget = true;

            NodeUI nodeUI = nodeObj.GetComponent<NodeUI>();
            if (nodeUI == null)
                nodeUI = nodeObj.AddComponent<NodeUI>();

            RectTransform rt = nodeObj.GetComponent<RectTransform>();
            if (rt != null && rt.sizeDelta.sqrMagnitude <= 0.01f)
                rt.sizeDelta = new Vector2(180f, 88f);

            Transform iconTransform = nodeObj.transform.Find("Icon");
            Image iconImage = iconTransform != null ? iconTransform.GetComponent<Image>() : null;
            if (iconImage == null)
            {
                GameObject iconObj = CreateUIObject("Icon", nodeObj.transform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f));
                RectTransform iconRT = iconObj.GetComponent<RectTransform>();
                iconRT.sizeDelta = new Vector2(46f, 46f);
                iconRT.anchoredPosition = new Vector2(30f, 0f);
                iconImage = iconObj.AddComponent<Image>();
                iconImage.raycastTarget = false;
            }

            TMP_Text label = FindText(nodeObj.transform, "Label");
            if (label == null)
            {
                label = CreateTextObject("Label", nodeObj.transform, "", 18, TextAlignmentOptions.Left,
                    new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(64f, 0f), new Vector2(-12f, 0f));
            }

            nodeUI.background = background;
            nodeUI.iconImage = iconImage;
            nodeUI.titleLabel = label;
            return nodeUI;
        }


        private GameObject CreateLineObject()
        {
            if (linePrefab != null)
            {
                GameObject line = Instantiate(linePrefab, linesContainer, false);
                if (line.GetComponent<ConnectionLineUI>() == null)
                    line.AddComponent<ConnectionLineUI>();
                line.transform.SetAsFirstSibling();
                return line;
            }

            GameObject lineObj = CreateUIObject("Line", linesContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            Image img = lineObj.AddComponent<Image>();
            img.raycastTarget = false;
            img.color = Color.clear; // Prevent white-flash before ConnectionLineUI.UpdateColor() runs
            lineObj.AddComponent<ConnectionLineUI>();
            lineObj.transform.SetAsFirstSibling();
            return lineObj;
        }


        private static void ClearChildren(RectTransform parent)
        {
            if (parent == null)
                return;

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                    Destroy(parent.GetChild(i).gameObject);
                else
                    DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }


        private void ToggleLegacyContent(bool showLegacy)
        {
            if (legacyUpgradesContent != null && legacyUpgradesContent.activeSelf != showLegacy)
            {
                legacyUpgradesContent.SetActive(showLegacy);
                Debug.Log(LogPrefix + (showLegacy ? "Legacy Scroll View shown." : "Legacy Scroll View hidden."));
            }

            if (treeRootObject != null)
                treeRootObject.SetActive(!showLegacy);

            if (openTreeButton != null)
                openTreeButton.gameObject.SetActive(showLegacy);
        }

        private void ShowTreeView()
        {
            ToggleLegacyContent(false);
            FocusDefaultNode();
        }

        private void ShowLegacyView()
        {
            ToggleLegacyContent(true);
        }

        private void EnsureTreeOpenButton()
        {
            Transform existing = transform.Find("OpenEntrepreneurTreeButton");
            if (existing != null)
            {
                openTreeButton = existing.GetComponent<Button>();
                return;
            }

            GameObject buttonObject = CreateUIObject("OpenEntrepreneurTreeButton", transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            RectTransform buttonRT = buttonObject.GetComponent<RectTransform>();
            buttonRT.sizeDelta = new Vector2(280f, 46f);
            buttonRT.anchoredPosition = new Vector2(-20f, -16f);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.15f, 0.32f, 0.24f, 0.95f);
            openTreeButton = buttonObject.AddComponent<Button>();
            openTreeButton.targetGraphic = image;
            CreateTextObject("Text", buttonObject.transform, "Abrir Árbol del Emprendedor", 18, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        }


        private static GameObject CreateUIObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            return go;
        }


        private static TMP_Text CreateTextObject(
            string name,
            Transform parent,
            string text,
            float fontSize,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.color = Color.white;

            return label;
        }


        private static TMP_Text FindText(Transform root, string childName)
        {
            Transform t = root.Find(childName);
            if (t == null)
                return null;

            return t.GetComponent<TMP_Text>();
        }

        private static string GetNodeTypeLabel(TreeNodeType type)
        {
            switch (type)
            {
                case TreeNodeType.Product:
                    return "Producto";
                case TreeNodeType.Employee:
                    return "Empleado";
                case TreeNodeType.Security:
                    return "Seguridad";
                case TreeNodeType.Improvement:
                    return "Mejora";
                default:
                    return type.ToString();
            }
        }


        void OnDestroy()
        {
            if (treeEventsBound)
            {
                EntrepreneurTreeManager.onPointsChanged -= OnPointsChanged;
                EntrepreneurTreeManager.onNodeUnlocked -= OnNodeUnlocked;
                treeEventsBound = false;
            }

            if (infoUnlockButton && unlockButtonListenerBound)
            {
                infoUnlockButton.onClick.RemoveListener(OnUnlockButtonClicked);
                unlockButtonListenerBound = false;
            }

            if (openTreeButton != null && openTreeButtonListenerBound)
            {
                openTreeButton.onClick.RemoveListener(ShowTreeView);
                openTreeButtonListenerBound = false;
            }

            if (backToLegacyButton != null && backTreeButtonListenerBound)
            {
                backToLegacyButton.onClick.RemoveListener(ShowLegacyView);
                backTreeButtonListenerBound = false;
            }
        }
    }
}
