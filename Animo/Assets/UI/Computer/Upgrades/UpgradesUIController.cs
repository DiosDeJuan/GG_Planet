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
        private const float NodeWidth = 210f;
        private const float NodeHeight = 112f;
        private const float ColumnSpacing = 260f;
        private const float RowSpacing = 142f;
        private const float ColumnTop = -110f;
        private const float HeaderY = -40f;

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
        private string lastDetailMessage = "Selecciona un nodo para ver sus detalles.";

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
            Dictionary<TreeNodeType, int> columnRows = new Dictionary<TreeNodeType, int>
            {
                { TreeNodeType.Product, 0 },
                { TreeNodeType.Employee, 0 },
                { TreeNodeType.Security, 0 },
                { TreeNodeType.Improvement, 0 }
            };
            CreateColumnHeaders();
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
                int row = columnRows.ContainsKey(nodeData.nodeType) ? columnRows[nodeData.nodeType] : 0;
                rt.anchoredPosition = GetColumnNodePosition(nodeData.nodeType, row);
                columnRows[nodeData.nodeType] = row + 1;
                Debug.Log(LogPrefix + "Rendering node: " + nodeData.id + " at " + rt.anchoredPosition.x + "/" + rt.anchoredPosition.y + ".");

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

        private void CreateColumnHeaders()
        {
            CreateColumnHeader(TreeNodeType.Product, "Productos");
            CreateColumnHeader(TreeNodeType.Employee, "Empleados");
            CreateColumnHeader(TreeNodeType.Security, "Seguridad");
            CreateColumnHeader(TreeNodeType.Improvement, "Mejoras");
        }

        private void CreateColumnHeader(TreeNodeType type, string label)
        {
            GameObject header = CreateUIObject("ColumnHeader_" + label, nodesContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            RectTransform rt = header.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(NodeWidth, 34f);
            rt.anchoredPosition = new Vector2(GetColumnX(type), HeaderY);
            Image bg = header.AddComponent<Image>();
            bg.color = ComputerUITheme.GetNodeAccent(type);
            TMP_Text text = CreateTextObject("Text", header.transform, label.ToUpperInvariant(), ComputerUITheme.FontSmall, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            text.color = ComputerUITheme.TextPrimary;
        }

        private static Vector2 GetColumnNodePosition(TreeNodeType type, int row)
        {
            return new Vector2(GetColumnX(type), ColumnTop - (row * RowSpacing));
        }

        private static float GetColumnX(TreeNodeType type)
        {
            switch (type)
            {
                case TreeNodeType.Employee:
                    return ColumnSpacing;
                case TreeNodeType.Security:
                    return ColumnSpacing * 2f;
                case TreeNodeType.Improvement:
                    return ColumnSpacing * 3f;
                default:
                    return 0f;
            }
        }


        public void ShowNodeInfo(NodeData node)
        {
            if (node == null || infoPanel == null)
                return;

            if (currentInfoNode == null || currentInfoNode.id != node.id)
                lastDetailMessage = EntrepreneurTreeManager.EvaluateUnlock(node.id).message;
            currentInfoNode = node;

            if (infoTitle)
            {
                infoTitle.text  = node.title;
                infoTitle.color = ComputerUITheme.GetNodeAccent(node.nodeType);
            }

            if (infoDescription)
            {
                string employeeStatus = BuildEmployeeStatusLine(node);
                string body =
                    "Tipo: " + GetNodeTypeLabel(node.nodeType) + "\n" +
                    "Estado: " + GetNodeStateText(node) + "\n" +
                    "Costo: " + node.cost + " punto" + (node.cost == 1 ? "" : "s") + "\n" +
                    "Puntos disponibles: " + EntrepreneurTreeManager.GetAvailablePoints() + " puntos\n" +
                    "Requisito: " + BuildRequirementText(node) + "\n" +
                    "Beneficio: " + (string.IsNullOrEmpty(node.description) ? "Sin descripcion." : node.description) + "\n" +
                    "Sistema afectado: " + BuildSystemAffectedText(node);
                if (!string.IsNullOrEmpty(employeeStatus))
                    body += "\n" + employeeStatus;

                infoDescription.text = body;
            }

            if (infoCost)
            {
                infoCost.text = "Mensaje: " + BuildDetailMessage(node);
                infoCost.color = node.isUnlocked ? ComputerUITheme.TextSuccess : ComputerUITheme.TextWarning;
            }

            if (infoRequirements)
            {
                infoRequirements.text = BuildRequirementStatusBlock(node);
                infoRequirements.color = ComputerUITheme.TextSecondary;
            }

            if (infoUnlockButton)
            {
                infoUnlockButton.gameObject.SetActive(true);
                EntrepreneurTreeUnlockResponse unlockState = EntrepreneurTreeManager.EvaluateUnlock(node.id);
                bool canUnlock = unlockState.success;
                infoUnlockButton.interactable = canUnlock;

                Image buttonImage = infoUnlockButton.targetGraphic as Image;
                if (buttonImage != null)
                    buttonImage.color = node.isUnlocked
                        ? ComputerUITheme.NodeUnlockedBg
                        : canUnlock ? ComputerUITheme.ButtonPositive : ComputerUITheme.ButtonDisabled;

                TMP_Text buttonText = infoUnlockButton.GetComponentInChildren<TMP_Text>(true);
                if (buttonText != null)
                {
                    buttonText.text = node.isUnlocked ? "YA DESBLOQUEADO" : canUnlock ? "DESBLOQUEAR" : "BLOQUEADO";
                    buttonText.enableAutoSizing = true;
                    buttonText.fontSizeMin = 10f;
                    buttonText.fontSizeMax = ComputerUITheme.FontBody;
                }
            }

            infoPanel.SetActive(true);
        }


        /// <summary>
        /// For Employee-type nodes, returns a short status line showing whether the
        /// employee linked to this node is hired, has a role, and has a workstation.
        /// Returns null or empty for non-employee nodes.
        /// </summary>
        private static string BuildEmployeeStatusLine(NodeData node)
        {
            if (node == null || node.nodeType != TreeNodeType.Employee)
                return string.Empty;

            // Extract employee index from node id: "employee_3" → 3
            int empId = -1;
            if (node.id != null && node.id.StartsWith("employee_"))
            {
                int.TryParse(node.id.Substring("employee_".Length), out empId);
            }

            if (empId < 1 || EntrepreneurEmployeeSystem.Instance == null)
                return string.Empty;

            EmployeeAssignment assignment = EntrepreneurEmployeeSystem.Instance.GetAssignment(empId);
            bool isUnlocked = EntrepreneurEmployeeSystem.Instance.IsEmployeeUnlocked(empId);

            if (!isUnlocked)
                return "Empleado: " + ComputerUITheme.LabelBlocked;

            if (assignment == null || !assignment.isHired)
                return "Empleado: " + ComputerUITheme.LabelReady + " (disponible para contratar)";

            string roleText = assignment.role == EmployeeRole.Cashier   ? "Cajero"
                            : assignment.role == EmployeeRole.Restocker ? "Surtidor"
                            : "Sin rol";

            string wsText = !string.IsNullOrEmpty(assignment.workstationId)
                ? assignment.workstationId
                : ComputerUITheme.LabelNoStation;

            return "Empleado: " + ComputerUITheme.LabelOk
                + " Contratado  |  Rol: " + roleText
                + "  |  Puesto: " + wsText;
        }

        private static string GetNodeStateText(NodeData node)
        {
            if (node == null)
                return "Bloqueado";
            if (node.isUnlocked)
                return "Desbloqueado";
            return EntrepreneurTreeManager.CanUnlockNode(node.id) ? "Disponible" : "Bloqueado";
        }

        private static string BuildRequirementText(NodeData node)
        {
            if (node == null || node.requiredNodeIds == null || node.requiredNodeIds.Count == 0)
                return "Ninguno";

            return "Requiere: " + string.Join(", ", GetRequirementTitles(node));
        }

        private static string BuildRequirementStatusBlock(NodeData node)
        {
            if (node == null || node.requiredNodeIds == null || node.requiredNodeIds.Count == 0)
                return "Requisito: Ninguno";

            StringBuilder sb = new StringBuilder("Requisitos:\n");
            for (int i = 0; i < node.requiredNodeIds.Count; i++)
            {
                string reqId = node.requiredNodeIds[i];
                NodeData req = EntrepreneurTreeManager.Instance != null && EntrepreneurTreeManager.Instance.treeData != null
                    ? EntrepreneurTreeManager.Instance.treeData.GetNodeById(reqId)
                    : null;
                sb.Append(req != null && req.isUnlocked ? "[OK] " : "[NO] ")
                  .Append(req != null ? req.title : reqId);
                if (i < node.requiredNodeIds.Count - 1)
                    sb.AppendLine();
            }

            return sb.ToString();
        }

        private static List<string> GetRequirementTitles(NodeData node)
        {
            List<string> titles = new List<string>();
            if (node == null || node.requiredNodeIds == null)
                return titles;

            for (int i = 0; i < node.requiredNodeIds.Count; i++)
            {
                string reqId = node.requiredNodeIds[i];
                NodeData req = EntrepreneurTreeManager.Instance != null && EntrepreneurTreeManager.Instance.treeData != null
                    ? EntrepreneurTreeManager.Instance.treeData.GetNodeById(reqId)
                    : null;
                titles.Add(req != null ? req.title : reqId);
            }

            return titles;
        }

        private static string BuildSystemAffectedText(NodeData node)
        {
            if (node == null)
                return "Mejoras";
            switch (node.nodeType)
            {
                case TreeNodeType.Product:
                    return "Compra/Productos";
                case TreeNodeType.Employee:
                    return "Empleados";
                case TreeNodeType.Security:
                    return "Seguridad";
                case TreeNodeType.Improvement:
                    return "Mejoras";
                default:
                    return "Mejoras";
            }
        }

        private string BuildDetailMessage(NodeData node)
        {
            if (!string.IsNullOrEmpty(lastDetailMessage) && currentInfoNode == node)
                return lastDetailMessage;

            EntrepreneurTreeUnlockResponse state = EntrepreneurTreeManager.EvaluateUnlock(node != null ? node.id : string.Empty);
            return state.message;
        }

        private static string BuildActivationMessage(NodeData node)
        {
            if (node == null)
                return string.Empty;
            switch (node.nodeType)
            {
                case TreeNodeType.Product:
                    return "Producto disponible en Compra.";
                case TreeNodeType.Employee:
                    return "Empleado disponible en la app Empleados.";
                case TreeNodeType.Security:
                    return "Nivel de seguridad activado.";
                case TreeNodeType.Improvement:
                    return "Mejora aplicada correctamente.";
                default:
                    return string.Empty;
            }
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

            EntrepreneurTreeUnlockResponse response = EntrepreneurTreeManager.TryUnlockNodeDetailed(currentInfoNode.id);
            lastDetailMessage = response.message;
            if (response.success)
            {
                string activation = BuildActivationMessage(currentInfoNode);
                if (!string.IsNullOrEmpty(activation))
                    lastDetailMessage += "\n" + activation;
            }
            ShowNodeInfo(currentInfoNode);
        }


        private void OnPointsChanged(int total, int change)
        {
            if (pointsLabel)
                pointsLabel.text = "Puntos disponibles: " + total;

            if (infoPanel && infoPanel.activeSelf && currentInfoNode != null && infoUnlockButton)
                ShowNodeInfo(currentInfoNode);
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
            rootBg.color = ComputerUITheme.RootBg;

            RectTransform rootRT = rootObj.GetComponent<RectTransform>();
            rootRT.offsetMin = new Vector2(20f, 20f);
            rootRT.offsetMax = new Vector2(-20f, -20f);

            GameObject header = CreateUIObject("Header", rootObj.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
            RectTransform headerRT = header.GetComponent<RectTransform>();
            headerRT.sizeDelta = new Vector2(0f, ComputerUITheme.HeaderHeight);
            Image headerBg = header.AddComponent<Image>();
            headerBg.color = ComputerUITheme.HeaderBg;

            TMP_Text titleTmp = CreateTextObject("Title", header.transform, "ARBOL DEL EMPRENDEDOR", ComputerUITheme.FontTitle, TextAlignmentOptions.Left,
                new Vector2(0f, 0f), new Vector2(0.6f, 1f), new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(-20f, 0f));
            titleTmp.color = ComputerUITheme.TextPrimary;

            pointsLabel = CreateTextObject("PointsText", header.transform, "Puntos disponibles: 0", ComputerUITheme.FontBody, TextAlignmentOptions.Right,
                new Vector2(0.6f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(-20f, 0f));
            pointsLabel.color = ComputerUITheme.TextWarning;

            GameObject backButton = CreateUIObject("BackToExpansionsButton", header.transform, new Vector2(0.45f, 0.1f), new Vector2(0.60f, 0.9f), new Vector2(0.5f, 0.5f));
            Image backImage = backButton.AddComponent<Image>();
            backImage.color = ComputerUITheme.ButtonSecondary;
            backToLegacyButton = backButton.AddComponent<Button>();
            backToLegacyButton.targetGraphic = backImage;
            TMP_Text backTxt = CreateTextObject("Text", backButton.transform, "Volver", ComputerUITheme.FontSmall, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            backTxt.color = ComputerUITheme.TextPrimary;

            GameObject info = CreateUIObject("InfoPanel", rootObj.transform, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f));
            RectTransform infoRT = info.GetComponent<RectTransform>();
            infoRT.sizeDelta = new Vector2(380f, -(ComputerUITheme.HeaderHeight + 28f));
            infoRT.anchoredPosition = new Vector2(-10f, -(ComputerUITheme.HeaderHeight * 0.5f + 4f));
            Image infoBg = info.AddComponent<Image>();
            infoBg.color = ComputerUITheme.PanelDarkBg;

            VerticalLayoutGroup infoLayout = info.AddComponent<VerticalLayoutGroup>();
            infoLayout.padding = new RectOffset(20, 20, 20, 20);
            infoLayout.spacing = 10;
            infoLayout.childControlHeight = false;
            infoLayout.childControlWidth = true;
            infoLayout.childForceExpandHeight = false;
            infoLayout.childForceExpandWidth = true;

            ContentSizeFitter infoFitter = info.AddComponent<ContentSizeFitter>();
            infoFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            infoTitle = CreateTextObject("NodeTitleText", info.transform, "Selecciona un nodo", ComputerUITheme.FontHeader, TextAlignmentOptions.Left,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
            infoTitle.color = ComputerUITheme.TextPrimary;

            infoDescription = CreateTextObject("NodeDescriptionText", info.transform, "Tipo: -\nEstado: -\nCosto: -\nPuntos disponibles: -\nRequisito: -\nBeneficio: -\nSistema afectado: -", ComputerUITheme.FontBody, TextAlignmentOptions.TopLeft,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
            infoDescription.color = ComputerUITheme.TextSecondary;

            infoCost = CreateTextObject("NodeCostText", info.transform, "Mensaje: Selecciona un nodo para ver sus detalles.", ComputerUITheme.FontBody, TextAlignmentOptions.Left,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
            infoCost.color = ComputerUITheme.TextPrimary;

            infoRequirements = CreateTextObject("RequirementsText", info.transform, "Requisito: Ninguno", ComputerUITheme.FontSmall, TextAlignmentOptions.TopLeft,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
            infoRequirements.color = ComputerUITheme.TextSecondary;

            GameObject unlock = CreateUIObject("UnlockButton", info.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f));
            RectTransform unlockRT = unlock.GetComponent<RectTransform>();
            unlockRT.sizeDelta = new Vector2(0f, ComputerUITheme.ButtonHeight);
            Image unlockBg = unlock.AddComponent<Image>();
            unlockBg.color = ComputerUITheme.ButtonPositive;
            infoUnlockButton = unlock.AddComponent<Button>();
            infoUnlockButton.targetGraphic = unlockBg;

            TMP_Text unlockTxt = CreateTextObject("Text", unlock.transform, "Desbloquear", ComputerUITheme.FontBody, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            unlockTxt.color = ComputerUITheme.TextPrimary;

            GameObject treeScroll = CreateUIObject("TreeScrollView", rootObj.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f));
            RectTransform scrollRT = treeScroll.GetComponent<RectTransform>();
            scrollRT.offsetMin = new Vector2(20f, 20f);
            scrollRT.offsetMax = new Vector2(-400f, -(ComputerUITheme.HeaderHeight + 20f));

            Image scrollBg = treeScroll.AddComponent<Image>();
            scrollBg.color = ComputerUITheme.CardBg;
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
            viewportBg.color = ComputerUITheme.RootBg;
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            GameObject content = CreateUIObject("Content", viewport.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            treeScrollContent = content.GetComponent<RectTransform>();
            treeScrollContent.sizeDelta = new Vector2(1040f, 2700f);
            treeScrollContent.anchoredPosition = new Vector2(20f, -20f);

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
            rt.sizeDelta = new Vector2(NodeWidth, NodeHeight);

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

            TMP_Text label = CreateTextObject("Label", nodeObj.transform, "", 15, TextAlignmentOptions.Left,
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
            if (rt != null)
                rt.sizeDelta = new Vector2(Mathf.Max(rt.sizeDelta.x, NodeWidth), Mathf.Max(rt.sizeDelta.y, NodeHeight));

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
                label = CreateTextObject("Label", nodeObj.transform, "", 15, TextAlignmentOptions.Left,
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
