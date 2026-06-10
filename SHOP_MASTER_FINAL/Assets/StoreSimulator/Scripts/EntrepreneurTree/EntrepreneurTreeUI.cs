//Adaptado por POMPIC 20100333
using System.Collections.Generic;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_INCLUDE_TESTS
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    public class EntrepreneurTreeUI : MonoBehaviour
    {
        private static readonly Color DesktopPink = new Color(1f, 0f, 0.392f, 1f);
        private static readonly Color PanelBackground = new Color(0.035f, 0.04f, 0.07f, 0.99f);
        private static readonly Color CardBackground = new Color(0.075f, 0.085f, 0.13f, 0.98f);
        private static readonly Color CardBackgroundAlt = new Color(0.105f, 0.12f, 0.18f, 0.98f);
        private static readonly Color TextDark = new Color(0.94f, 0.95f, 0.98f, 1f);
        private static readonly Color TextMuted = new Color(0.68f, 0.72f, 0.82f, 1f);
        private static readonly Color LineLocked = new Color(0.22f, 0.24f, 0.31f, 0.68f);
        private static readonly Color LineAvailable = new Color(0.1f, 0.88f, 1f, 0.98f);
        private static readonly Color LineNoPoints = new Color(1f, 0.72f, 0.2f, 0.95f);
        private static readonly Color LineUnlocked = new Color(0.18f, 1f, 0.52f, 0.98f);

        public static Color VisualPanelBackground => PanelBackground;
        public static Color VisualCardBackground => CardBackground;

        private readonly Dictionary<string, EntrepreneurTreeNodeView> nodeViews = new Dictionary<string, EntrepreneurTreeNodeView>();
        private readonly List<string> fallbackNodeIds = new List<string>();

        private TMP_Text pointsLabel;
        private TMP_Text progressLabel;
        private TMP_Text achievementsLabel;
        private TMP_Text detailsTitle;
        private TMP_Text detailsLabel;
        private TMP_Text detailsMessageLabel;
        private Button toggleAchievementsButton;
        private Button unlockButton;
        private EntrepreneurTreeConnectionGraphic connectionGraphic;
        private ScrollRect graphScroll;
        private GameObject achievementListObject;
        private Transform achievementListContent;
        private EntrepreneurTreeNodeDefinition selectedNode;
        private bool built;
        private bool showAchievements;
#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_INCLUDE_TESTS
        private GameObject qaOverlayObject;
        private TMP_Text qaStatusLabel;
        private bool qaOverlayVisible;
#endif

        void OnEnable()
        {
            EntrepreneurProgress.onProgressChanged += Refresh;
            EntrepreneurAchievementManager.onAchievementsChanged += Refresh;
            EntrepreneurAchievementManager.EvaluateAll();
            Refresh();
        }

        void OnDisable()
        {
            EntrepreneurProgress.onProgressChanged -= Refresh;
            EntrepreneurAchievementManager.onAchievementsChanged -= Refresh;
        }

        public void Build()
        {
            if (built)
                return;

            built = true;
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image background = gameObject.AddComponent<Image>();
            background.color = PanelBackground;

            VerticalLayoutGroup rootLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(12, 12, 10, 10);
            rootLayout.spacing = 7;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;

            BuildHeader(transform);
            BuildBody(transform);
#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_INCLUDE_TESTS
            BuildQaOverlay(transform);
#endif
            SelectNode(EntrepreneurTreeDefinitions.Get(EntrepreneurTreeDefinitions.DefaultUnlockedNodeId), false);
            CenterOnDefaultNode();
            Refresh();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_INCLUDE_TESTS
        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame)
                SetQaOverlayVisibleForQA(!qaOverlayVisible);
        }
#endif

        private void BuildHeader(Transform parent)
        {
            GameObject header = CreatePanel("Header", parent, CardBackground);
            LayoutElement headerLayout = header.AddComponent<LayoutElement>();
            headerLayout.preferredHeight = 88;

            HorizontalLayoutGroup headerGroup = header.AddComponent<HorizontalLayoutGroup>();
            headerGroup.padding = new RectOffset(14, 14, 7, 7);
            headerGroup.spacing = 12;
            headerGroup.childControlWidth = true;
            headerGroup.childControlHeight = true;
            headerGroup.childForceExpandWidth = true;
            headerGroup.childForceExpandHeight = true;

            GameObject textBox = CreateLayoutBox("Text", header.transform);
            VerticalLayoutGroup textLayout = textBox.AddComponent<VerticalLayoutGroup>();
            textLayout.spacing = 2;
            textLayout.childControlWidth = true;
            textLayout.childControlHeight = true;

            TMP_Text title = CreateText("Title", textBox.transform, "ARBOL DEL EMPRENDEDOR", 21, FontStyles.Bold, TextAlignmentOptions.Left);
            title.color = TextDark;
            TMP_Text subtitle = CreateText("Subtitle", textBox.transform, "Ramas RPG por productos, empleados, seguridad y mejoras. Los puntos vienen de logros cobrados una sola vez.", 12, FontStyles.Normal, TextAlignmentOptions.Left);
            subtitle.color = TextMuted;

            GameObject pointsBox = CreateLayoutBox("Points Box", header.transform);
            LayoutElement pointsBoxLayout = pointsBox.AddComponent<LayoutElement>();
            pointsBoxLayout.preferredWidth = 285;
            VerticalLayoutGroup pointsLayout = pointsBox.AddComponent<VerticalLayoutGroup>();
            pointsLayout.spacing = 4;
            pointsLayout.childControlWidth = true;
            pointsLayout.childControlHeight = true;

            pointsLabel = CreateText("Points", pointsBox.transform, string.Empty, 18, FontStyles.Bold, TextAlignmentOptions.Right);
            pointsLabel.color = new Color(0.1f, 0.88f, 1f, 1f);
            progressLabel = CreateText("Progress", pointsBox.transform, string.Empty, 12, FontStyles.Bold, TextAlignmentOptions.Right);
            progressLabel.color = TextMuted;
            achievementsLabel = CreateText("Achievements", pointsBox.transform, string.Empty, 12, FontStyles.Bold, TextAlignmentOptions.Right);
            achievementsLabel.color = TextMuted;
            toggleAchievementsButton = CreateActionButton("Toggle Achievements", pointsBox.transform, "Ver logros");
            toggleAchievementsButton.onClick.AddListener(ToggleAchievements);
            toggleAchievementsButton.GetComponent<LayoutElement>().preferredHeight = 26;
            TMP_Text buttonText = toggleAchievementsButton.GetComponentInChildren<TMP_Text>();
            buttonText.fontSizeMax = 13;
            buttonText.fontSize = 13;
        }

        private void BuildBody(Transform parent)
        {
            GameObject body = CreateLayoutBox("Body", parent);
            LayoutElement bodyLayout = body.AddComponent<LayoutElement>();
            bodyLayout.flexibleHeight = 1;
            bodyLayout.minHeight = 350;

            HorizontalLayoutGroup bodyGroup = body.AddComponent<HorizontalLayoutGroup>();
            bodyGroup.spacing = 10;
            bodyGroup.childControlWidth = true;
            bodyGroup.childControlHeight = true;
            bodyGroup.childForceExpandWidth = false;
            bodyGroup.childForceExpandHeight = true;

            BuildGraphPanel(body.transform);
            BuildDetailsPanel(body.transform);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_INCLUDE_TESTS
        private void BuildQaOverlay(Transform parent)
        {
            qaOverlayObject = CreatePanel("QA Overlay", parent, new Color(0.075f, 0.055f, 0.11f, 0.98f));
            LayoutElement overlayLayout = qaOverlayObject.AddComponent<LayoutElement>();
            overlayLayout.preferredHeight = 48;

            HorizontalLayoutGroup group = qaOverlayObject.AddComponent<HorizontalLayoutGroup>();
            group.padding = new RectOffset(8, 8, 5, 5);
            group.spacing = 6;
            group.childControlWidth = false;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = true;

            qaStatusLabel = CreateText("QA Status", qaOverlayObject.transform, "QA F8: puntos, prerequisitos, perfil fase12.", 11, FontStyles.Bold, TextAlignmentOptions.Left);
            qaStatusLabel.color = TextMuted;
            qaStatusLabel.GetComponent<LayoutElement>().preferredWidth = 300;

            CreateQaButton("+1", AddOneQaPoint);
            CreateQaButton("+5", AddFiveQaPoints);
            CreateQaButton("Prereqs", UnlockSelectedPrerequisitesForQA);
            CreateQaButton("Reset", ResetTreeForQA);
            CreateQaButton("Guardar QA", SaveQaProfileForQA);
            CreateQaButton("Cargar QA", LoadQaProfileForQA);

            qaOverlayObject.SetActive(false);
        }

        private Button CreateQaButton(string label, UnityEngine.Events.UnityAction action)
        {
            Button button = CreateActionButton("QA " + label, qaOverlayObject.transform, label);
            LayoutElement layout = button.GetComponent<LayoutElement>();
            layout.preferredWidth = label.Length > 5 ? 104 : 66;
            layout.minWidth = 58;
            layout.preferredHeight = 32;
            TMP_Text text = button.GetComponentInChildren<TMP_Text>();
            text.fontSize = 12;
            text.fontSizeMax = 12;
            text.fontSizeMin = 8;
            button.onClick.AddListener(action);
            return button;
        }

        public void SetQaOverlayVisibleForQA(bool visible)
        {
            qaOverlayVisible = visible;
            if (qaOverlayObject != null)
                qaOverlayObject.SetActive(visible);

            UpdateQaStatus(visible ? "QA activo: usa los botones o F8 para ocultar." : "QA oculto.");
            RefreshDetails();
        }

        public bool SelectNodeForQA(string nodeId)
        {
            EntrepreneurTreeNodeDefinition node = EntrepreneurTreeDefinitions.Get(nodeId);
            if (node == null)
                return false;

            SelectNode(node, true);
            return true;
        }

        public void ShowAchievementsForQA(bool visible)
        {
            showAchievements = visible;
            Refresh();
        }

        public bool TryUnlockSelectedForQA(out string message)
        {
            if (selectedNode == null)
            {
                message = "Nodo QA no seleccionado.";
                return false;
            }

            bool unlocked = EntrepreneurProgress.TryUnlock(selectedNode.Id, out message);
            UpdateQaStatus(message);
            Refresh();
            return unlocked;
        }

        private void AddOneQaPoint()
        {
            AddQaPoints(1);
        }

        private void AddFiveQaPoints()
        {
            AddQaPoints(5);
        }

        private void AddQaPoints(int amount)
        {
            EntrepreneurProgress.AddPointsForInternalTesting(amount);
            UpdateQaStatus("QA +" + amount + " punto(s). Disponibles: " + EntrepreneurProgress.AvailablePoints + ".");
            Refresh();
        }

        private void UnlockSelectedPrerequisitesForQA()
        {
            if (selectedNode == null)
            {
                UpdateQaStatus("QA sin nodo seleccionado.");
                return;
            }

            int unlocked = 0;
            HashSet<string> visited = new HashSet<string>();
            for (int i = 0; i < selectedNode.Prerequisites.Length; i++)
                unlocked += UnlockPrerequisiteChainForQA(selectedNode.Prerequisites[i], visited);

            UpdateQaStatus("QA prerequisitos desbloqueados: " + unlocked + ".");
            Refresh();
        }

        private static int UnlockPrerequisiteChainForQA(string nodeId, HashSet<string> visited)
        {
            if (!visited.Add(nodeId))
                return 0;

            EntrepreneurTreeNodeDefinition node = EntrepreneurTreeDefinitions.Get(nodeId);
            if (node == null)
                return 0;

            int unlocked = 0;
            for (int i = 0; i < node.Prerequisites.Length; i++)
                unlocked += UnlockPrerequisiteChainForQA(node.Prerequisites[i], visited);

            if (EntrepreneurProgress.IsUnlocked(node.Id))
                return unlocked;

            int missingPoints = Mathf.Max(0, node.Cost - EntrepreneurProgress.AvailablePoints);
            if (missingPoints > 0)
                EntrepreneurProgress.AddPointsForInternalTesting(missingPoints);

            if (EntrepreneurProgress.TryUnlock(node.Id, out _))
                unlocked++;

            return unlocked;
        }

        private void ResetTreeForQA()
        {
            EntrepreneurProgress.ResetToDefaults();
            UpdateQaStatus("QA estado del arbol reiniciado.");
            SelectNode(EntrepreneurTreeDefinitions.Get(EntrepreneurTreeDefinitions.DefaultUnlockedNodeId), true);
        }

        private void SaveQaProfileForQA()
        {
            if (SaveGameSystem.Instance == null)
            {
                UpdateQaStatus("SaveGameSystem no esta disponible en esta escena.");
                return;
            }

            try
            {
                SaveGameSystem.Save("fase12_tree_qa");
                UpdateQaStatus("Perfil QA guardado: fase12_tree_qa.");
            }
            catch (System.Exception exception)
            {
                UpdateQaStatus("No se pudo guardar perfil QA: " + exception.GetType().Name + ".");
            }
        }

        private void LoadQaProfileForQA()
        {
            if (SaveGameSystem.Instance == null)
            {
                UpdateQaStatus("SaveGameSystem no esta disponible en esta escena.");
                return;
            }

            try
            {
                SaveGameSystem.Load("fase12_tree_qa");
                UpdateQaStatus("Carga QA solicitada: fase12_tree_qa.");
                Refresh();
            }
            catch (System.Exception exception)
            {
                UpdateQaStatus("No se pudo cargar perfil QA: " + exception.GetType().Name + ".");
            }
        }

        private void UpdateQaStatus(string message)
        {
            if (qaStatusLabel != null)
                qaStatusLabel.text = message;
        }
#endif

        private void BuildGraphPanel(Transform parent)
        {
            GameObject graphPanel = CreatePanel("Graph Panel", parent, CardBackground);
            LayoutElement graphLayout = graphPanel.AddComponent<LayoutElement>();
            graphLayout.flexibleWidth = 1;

            VerticalLayoutGroup graphGroup = graphPanel.AddComponent<VerticalLayoutGroup>();
            graphGroup.padding = new RectOffset(8, 8, 8, 8);
            graphGroup.spacing = 7;
            graphGroup.childControlWidth = true;
            graphGroup.childControlHeight = true;
            graphGroup.childForceExpandWidth = true;
            graphGroup.childForceExpandHeight = false;

            BuildLegend(graphPanel.transform);
            BuildGraphScroll(graphPanel.transform);
        }

        private void BuildLegend(Transform parent)
        {
            GameObject legend = CreatePanel("Legend", parent, CardBackgroundAlt);
            LayoutElement layout = legend.AddComponent<LayoutElement>();
            layout.preferredHeight = 30;

            TMP_Text label = CreateText("Legend Text", legend.transform, "Bloqueado | Disponible | Desbloqueado     P Producto  E Empleado  S Seguridad  M Mejora", 11, FontStyles.Bold, TextAlignmentOptions.Center);
            label.color = TextMuted;
        }

        private void BuildGraphScroll(Transform parent)
        {
            GameObject scroll = CreatePanel("Graph Scroll", parent, new Color(0.025f, 0.03f, 0.055f, 0.96f));
            LayoutElement scrollLayout = scroll.AddComponent<LayoutElement>();
            scrollLayout.flexibleHeight = 1;

            graphScroll = scroll.AddComponent<ScrollRect>();
            graphScroll.horizontal = true;
            graphScroll.vertical = true;
            graphScroll.movementType = ScrollRect.MovementType.Clamped;
            graphScroll.scrollSensitivity = 30f;

            GameObject viewport = CreateLayoutBox("Viewport", scroll.transform);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            Stretch(viewportRect, new Vector2(8, 8), new Vector2(-8, -8));
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = Color.clear;
            viewport.AddComponent<RectMask2D>();

            GameObject content = CreateLayoutBox("Graph Content", viewport.transform);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(0, 1);
            contentRect.pivot = new Vector2(0, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = EntrepreneurTreeVisualLayout.ContentSize;

            GameObject connections = CreateLayoutBox("Connections", content.transform);
            RectTransform connectionRect = connections.GetComponent<RectTransform>();
            connectionRect.anchorMin = new Vector2(0, 1);
            connectionRect.anchorMax = new Vector2(0, 1);
            connectionRect.pivot = new Vector2(0, 1);
            connectionRect.anchoredPosition = Vector2.zero;
            connectionRect.sizeDelta = EntrepreneurTreeVisualLayout.ContentSize;
            connectionGraphic = connections.AddComponent<EntrepreneurTreeConnectionGraphic>();
            connectionGraphic.raycastTarget = false;

            CreateGraphNodes(content.transform);
            graphScroll.viewport = viewportRect;
            graphScroll.content = contentRect;
        }

        private void CreateGraphNodes(Transform content)
        {
            int fallbackIndex = 0;
            foreach (EntrepreneurTreeNodeDefinition node in EntrepreneurTreeDefinitions.Nodes)
            {
                Vector2 position = EntrepreneurTreeVisualLayout.GetAnchoredPosition(node.Id, fallbackIndex, out bool usedFallback);
                if (usedFallback)
                {
                    fallbackNodeIds.Add(node.Id);
                    fallbackIndex++;
                }

                GameObject nodeObject = new GameObject(node.Id, typeof(RectTransform));
                nodeObject.transform.SetParent(content, false);
                RectTransform rect = nodeObject.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = EntrepreneurTreeVisualLayout.NodeSize;
                rect.anchoredPosition = position;

                EntrepreneurTreeNodeView view = nodeObject.AddComponent<EntrepreneurTreeNodeView>();
                view.Initialize(node, SelectNode);
                nodeViews[node.Id] = view;
            }
        }

        private void BuildDetailsPanel(Transform parent)
        {
            GameObject panel = CreatePanel("Details", parent, CardBackground);
            LayoutElement layout = panel.AddComponent<LayoutElement>();
            layout.preferredWidth = 318;
            layout.minWidth = 318;

            VerticalLayoutGroup group = panel.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(12, 12, 12, 12);
            group.spacing = 10;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;

            detailsTitle = CreateText("Title", panel.transform, "Detalle del nodo", 19, FontStyles.Bold, TextAlignmentOptions.Left);
            detailsTitle.color = TextDark;
            detailsTitle.GetComponent<LayoutElement>().preferredHeight = 30;

            detailsLabel = CreateText("Details Text", panel.transform, string.Empty, 13, FontStyles.Normal, TextAlignmentOptions.Left);
            detailsLabel.color = TextDark;
            detailsLabel.textWrappingMode = TextWrappingModes.Normal;
            LayoutElement detailsLayout = detailsLabel.GetComponent<LayoutElement>();
            detailsLayout.preferredHeight = 260;
            detailsLayout.flexibleHeight = 1;

            detailsMessageLabel = CreateText("Message", panel.transform, string.Empty, 13, FontStyles.Bold, TextAlignmentOptions.Left);
            detailsMessageLabel.color = TextMuted;
            detailsMessageLabel.GetComponent<LayoutElement>().preferredHeight = 52;

            unlockButton = CreateActionButton("Unlock Button", panel.transform, "Desbloquear");
            unlockButton.onClick.AddListener(UnlockSelectedNode);
            BuildAchievementList(panel.transform);
        }

        private void BuildAchievementList(Transform parent)
        {
            achievementListObject = CreatePanel("Achievement List", parent, CardBackgroundAlt);
            LayoutElement layout = achievementListObject.AddComponent<LayoutElement>();
            layout.flexibleHeight = 1;
            layout.minHeight = 270;

            ScrollRect scroll = achievementListObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;

            GameObject viewport = CreateLayoutBox("Viewport", achievementListObject.transform);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            Stretch(viewportRect, new Vector2(6, 6), new Vector2(-6, -6));
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = Color.clear;
            viewport.AddComponent<RectMask2D>();

            GameObject content = CreateLayoutBox("Content", viewport.transform);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup contentGroup = content.AddComponent<VerticalLayoutGroup>();
            contentGroup.padding = new RectOffset(4, 4, 4, 4);
            contentGroup.spacing = 6;
            contentGroup.childControlWidth = true;
            contentGroup.childControlHeight = true;
            contentGroup.childForceExpandWidth = true;
            contentGroup.childForceExpandHeight = false;
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            achievementListContent = content.transform;
            achievementListObject.SetActive(false);
        }

        private void SelectNode(EntrepreneurTreeNodeDefinition node)
        {
            SelectNode(node, true);
        }

        private void SelectNode(EntrepreneurTreeNodeDefinition node, bool refresh)
        {
            selectedNode = node;
            if (refresh)
                Refresh();
            else
                RefreshDetails();
        }

        private void UnlockSelectedNode()
        {
            if (selectedNode == null)
                return;

            bool unlocked = EntrepreneurProgress.TryUnlock(selectedNode.Id, out string message);
            if (detailsMessageLabel != null)
                detailsMessageLabel.text = message;

            if (UIGame.Instance != null)
                UIGame.AddNotification(message, otherColor: unlocked ? Color.green : new Color(1f, 0.78f, 0.25f), otherDuration: 4f);

            Refresh();
        }

        private void Refresh()
        {
            if (pointsLabel != null)
                pointsLabel.text = "Puntos: " + EntrepreneurProgress.AvailablePoints;
            if (progressLabel != null)
            {
                int unlocked = EntrepreneurProgress.GetUnlockedNodeCount();
                int total = EntrepreneurProgress.GetTotalNodeCount();
                int percent = Mathf.RoundToInt(EntrepreneurProgress.GetTreeCompletionPercent() * 100f);
                progressLabel.text = "Progreso: " + unlocked + "/" + total + " nodos (" + percent + "%)";
            }
            if (achievementsLabel != null)
                achievementsLabel.text = "Logros: " + EntrepreneurAchievementManager.CompletedCount + "/" + EntrepreneurAchievementManager.TotalAchievementCount;
            if (toggleAchievementsButton != null)
                toggleAchievementsButton.GetComponentInChildren<TMP_Text>().text = showAchievements ? "Ver nodo" : "Ver logros";

            foreach (EntrepreneurTreeNodeView view in nodeViews.Values)
                view.Refresh(view.Node == selectedNode);

            RefreshConnections();
            RefreshDetails();
        }

        private void RefreshConnections()
        {
            if (connectionGraphic == null)
                return;

            List<EntrepreneurTreeConnectionGraphic.ConnectionLine> lines = new List<EntrepreneurTreeConnectionGraphic.ConnectionLine>();
            foreach (EntrepreneurTreeNodeDefinition childNode in EntrepreneurTreeDefinitions.Nodes)
            {
                if (!nodeViews.TryGetValue(childNode.Id, out EntrepreneurTreeNodeView childView))
                    continue;

                for (int i = 0; i < childNode.Prerequisites.Length; i++)
                {
                    string parentId = childNode.Prerequisites[i];
                    EntrepreneurTreeNodeDefinition parentNode = EntrepreneurTreeDefinitions.Get(parentId);
                    if (parentNode == null || !nodeViews.TryGetValue(parentId, out EntrepreneurTreeNodeView parentView))
                        continue;

                    Vector2 start = GetConnectionEdge(parentView.Rect.anchoredPosition, childView.Rect.anchoredPosition);
                    Vector2 end = GetConnectionEdge(childView.Rect.anchoredPosition, parentView.Rect.anchoredPosition);
                    Color color = GetConnectionColor(parentNode, childNode);
                    lines.Add(new EntrepreneurTreeConnectionGraphic.ConnectionLine(start, end, color, 8f));
                }
            }

            connectionGraphic.SetConnections(lines);
        }

        private void RefreshDetails()
        {
            if (selectedNode == null || detailsLabel == null)
                return;

            if (showAchievements)
            {
                detailsTitle.text = "Logros";
                detailsLabel.gameObject.SetActive(false);
                detailsMessageLabel.gameObject.SetActive(false);
                unlockButton.gameObject.SetActive(false);
                achievementListObject.SetActive(true);
                RefreshAchievementList();
                return;
            }

            detailsTitle.text = "Detalle del nodo";
            detailsLabel.gameObject.SetActive(true);
            detailsMessageLabel.gameObject.SetActive(true);
            unlockButton.gameObject.SetActive(true);
            achievementListObject.SetActive(false);

            string requirements = selectedNode.Prerequisites.Length == 0 ? "Ninguno" : string.Join(", ", System.Array.ConvertAll(selectedNode.Prerequisites, EntrepreneurTreeDefinitions.GetTitle));
            string productSummary = selectedNode.Type == EntrepreneurTreeNodeType.Product ? "\n\nProductos:\n" + DocumentedProductCatalog.GetUnlockSummaryForNode(selectedNode.Id) : string.Empty;
            string qaId = string.Empty;
#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_INCLUDE_TESTS
            qaId = qaOverlayVisible ? "\nID QA: " + selectedNode.Id + "\n" : string.Empty;
#endif
            detailsLabel.text =
                selectedNode.Title + "\n\n" +
                "Tipo: " + GetTypeLabel(selectedNode.Type) + "\n" +
                "Estado: " + EntrepreneurProgress.GetStateDescription(selectedNode).Replace("\n", " - ") + "\n" +
                "Costo: " + selectedNode.Cost + " punto(s)\n" +
                "Requiere: " + requirements + "\n" +
                qaId +
                "Arbol: " + EntrepreneurProgress.GetUnlockedNodeCount() + "/" + EntrepreneurProgress.GetTotalNodeCount() + " nodos\n\n" +
                "Beneficio:\n" + selectedNode.Benefit + productSummary;

            EntrepreneurTreeNodeState state = EntrepreneurProgress.GetState(selectedNode);
            unlockButton.interactable = state == EntrepreneurTreeNodeState.Available;
            unlockButton.GetComponentInChildren<TMP_Text>().text = state == EntrepreneurTreeNodeState.Unlocked ? "Desbloqueado" : state == EntrepreneurTreeNodeState.Available ? "Desbloquear" : "Bloqueado";

            if (detailsMessageLabel != null)
                detailsMessageLabel.text = GetContextMessage(selectedNode, state);
        }

        private string GetContextMessage(EntrepreneurTreeNodeDefinition node, EntrepreneurTreeNodeState state)
        {
            if (state == EntrepreneurTreeNodeState.Unlocked)
                return node.Title + " ya esta desbloqueado.";

            List<string> missing = EntrepreneurProgress.GetMissingPrerequisites(node);
            if (missing.Count > 0)
                return "Falta desbloquear: " + string.Join(", ", missing);

            if (EntrepreneurProgress.ProgressPoints < node.Cost)
                return "No tienes puntos de progreso suficientes.";

            return "Disponible para desbloquear.";
        }

        private void ToggleAchievements()
        {
            showAchievements = !showAchievements;
            Refresh();
        }

        private void RefreshAchievementList()
        {
            if (achievementListContent == null)
                return;

            for (int i = achievementListContent.childCount - 1; i >= 0; i--)
                Destroy(achievementListContent.GetChild(i).gameObject);

            TMP_Text help = CreateText("Help", achievementListContent, "Completa logros para ganar puntos. Cada logro reclamado otorga +1 punto salvo Arbol Completo, que es simbolico.", 12, FontStyles.Bold, TextAlignmentOptions.Left);
            help.color = TextMuted;
            help.GetComponent<LayoutElement>().preferredHeight = 44;

            foreach (EntrepreneurAchievementDefinition achievement in EntrepreneurAchievementManager.Definitions)
            {
                string reward = achievement.RewardPoints > 0 ? "+" + achievement.RewardPoints + " punto" : "sin punto";
                string status = EntrepreneurAchievementManager.GetStatusText(achievement);
                string suffix = achievement.IsHook && !string.IsNullOrEmpty(achievement.HookReason) ? "\nPendiente: " + achievement.HookReason : string.Empty;
                TMP_Text row = CreateText("Achievement - " + achievement.Id, achievementListContent, achievement.Title + "\n" + achievement.Description + "\nEstado: " + status + " | Recompensa: " + reward + suffix, 11, FontStyles.Normal, TextAlignmentOptions.Left);
                row.color = achievement.IsHook && !EntrepreneurAchievementManager.IsCompleted(achievement.Id) ? TextMuted : TextDark;
                row.GetComponent<LayoutElement>().preferredHeight = achievement.IsHook ? 72 : 58;
            }
        }

        private static Color GetConnectionColor(EntrepreneurTreeNodeDefinition parentNode, EntrepreneurTreeNodeDefinition childNode)
        {
            if (EntrepreneurProgress.IsUnlocked(childNode.Id))
                return LineUnlocked;

            if (!EntrepreneurProgress.IsUnlocked(parentNode.Id))
                return LineLocked;

            if (EntrepreneurProgress.GetState(childNode) == EntrepreneurTreeNodeState.Available)
                return LineAvailable;

            if (EntrepreneurProgress.ArePrerequisitesUnlocked(childNode))
                return LineNoPoints;

            return LineLocked;
        }

        private static Vector2 GetConnectionEdge(Vector2 from, Vector2 to)
        {
            Vector2 delta = to - from;
            if (delta.sqrMagnitude < 0.1f)
                return from;

            Vector2 direction = delta.normalized;
            Vector2 half = EntrepreneurTreeVisualLayout.NodeSize * 0.5f;
            float scale = Mathf.Abs(direction.x) > Mathf.Abs(direction.y) ? half.x : half.y;
            return from + direction * scale;
        }

        private void CenterOnDefaultNode()
        {
            if (graphScroll == null)
                return;

            if (graphScroll.content != null)
                graphScroll.content.anchoredPosition = Vector2.zero;

            graphScroll.normalizedPosition = new Vector2(0f, 1f);
        }

        private static string GetTypeLabel(EntrepreneurTreeNodeType type)
        {
            switch (type)
            {
                case EntrepreneurTreeNodeType.Product: return "Producto";
                case EntrepreneurTreeNodeType.Employee: return "Empleado";
                case EntrepreneurTreeNodeType.Security: return "Seguridad";
                default: return "Mejora";
            }
        }

        private static GameObject CreateLayoutBox(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = CreateLayoutBox(name, parent);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static TMP_Text CreateText(string name, Transform parent, string text, int size, FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject obj = CreateLayoutBox(name, parent);
            Stretch(obj.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            TextMeshProUGUI label = obj.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.enableAutoSizing = true;
            label.fontSizeMin = Mathf.Max(9, size - 5);
            label.fontSizeMax = size;
            label.raycastTarget = false;
            obj.AddComponent<LayoutElement>();
            return label;
        }

        private static Button CreateActionButton(string name, Transform parent, string label)
        {
            GameObject obj = CreatePanel(name, parent, DesktopPink);
            LayoutElement layout = obj.AddComponent<LayoutElement>();
            layout.preferredHeight = 44;
            Button button = obj.AddComponent<Button>();
            button.targetGraphic = obj.GetComponent<Image>();
            TMP_Text text = CreateText("Text", obj.transform, label, 18, FontStyles.Bold, TextAlignmentOptions.Center);
            text.color = Color.white;
            return button;
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
