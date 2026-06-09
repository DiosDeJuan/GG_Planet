//Adaptado por POMPIC 20100333
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    public class EntrepreneurTreeUI : MonoBehaviour
    {
        private static readonly Color DesktopPink = new Color(1f, 0f, 0.392f, 1f);
        private static readonly Color PanelBackground = new Color(0.94f, 0.94f, 0.95f, 0.98f);
        private static readonly Color CardBackground = new Color(1f, 1f, 1f, 0.96f);
        private static readonly Color TextDark = new Color(0.12f, 0.12f, 0.13f, 1f);
        private static readonly Color TextMuted = new Color(0.32f, 0.33f, 0.35f, 1f);

        private readonly Dictionary<string, TMP_Text> stateLabels = new Dictionary<string, TMP_Text>();
        private readonly Dictionary<string, Button> nodeButtons = new Dictionary<string, Button>();

        private TMP_Text pointsLabel;
        private TMP_Text detailsLabel;
        private Button unlockButton;
        private EntrepreneurTreeNodeDefinition selectedNode;

        void OnEnable()
        {
            EntrepreneurProgress.onProgressChanged += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            EntrepreneurProgress.onProgressChanged -= Refresh;
        }

        public void Build()
        {
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image background = gameObject.AddComponent<Image>();
            background.color = PanelBackground;

            VerticalLayoutGroup rootLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(14, 14, 12, 12);
            rootLayout.spacing = 8;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;

            BuildHeader(transform);
            BuildBody(transform);
            SelectNode(EntrepreneurTreeDefinitions.Get(EntrepreneurTreeDefinitions.DefaultUnlockedNodeId));
            Refresh();
        }

        private void BuildHeader(Transform parent)
        {
            GameObject header = CreatePanel("Header", parent, CardBackground);
            LayoutElement headerLayout = header.AddComponent<LayoutElement>();
            headerLayout.preferredHeight = 78;

            HorizontalLayoutGroup headerGroup = header.AddComponent<HorizontalLayoutGroup>();
            headerGroup.padding = new RectOffset(14, 14, 8, 8);
            headerGroup.spacing = 14;
            headerGroup.childControlWidth = true;
            headerGroup.childControlHeight = true;
            headerGroup.childForceExpandWidth = true;
            headerGroup.childForceExpandHeight = true;

            GameObject textBox = CreateLayoutBox("Text", header.transform);
            VerticalLayoutGroup textLayout = textBox.AddComponent<VerticalLayoutGroup>();
            textLayout.spacing = 2;
            textLayout.childControlWidth = true;
            textLayout.childControlHeight = true;

            TMP_Text title = CreateText("Title", textBox.transform, "Arbol del Emprendedor", 24, FontStyles.Bold, TextAlignmentOptions.Left);
            title.color = TextDark;
            TMP_Text subtitle = CreateText("Subtitle", textBox.transform, "Desbloquea productos, empleados, seguridad y mejoras para expandir tu supermercado.", 13, FontStyles.Normal, TextAlignmentOptions.Left);
            subtitle.color = TextMuted;

            pointsLabel = CreateText("Points", header.transform, string.Empty, 19, FontStyles.Bold, TextAlignmentOptions.Right);
            pointsLabel.color = DesktopPink;
            pointsLabel.GetComponent<LayoutElement>().preferredWidth = 165;
        }

        private void BuildBody(Transform parent)
        {
            GameObject body = CreateLayoutBox("Body", parent);
            LayoutElement bodyLayout = body.AddComponent<LayoutElement>();
            bodyLayout.flexibleHeight = 1;
            bodyLayout.minHeight = 315;

            HorizontalLayoutGroup bodyGroup = body.AddComponent<HorizontalLayoutGroup>();
            bodyGroup.spacing = 10;
            bodyGroup.childControlWidth = true;
            bodyGroup.childControlHeight = true;
            bodyGroup.childForceExpandWidth = false;
            bodyGroup.childForceExpandHeight = true;

            BuildNodeArea(body.transform);
            BuildDetailsPanel(body.transform);
        }

        private void BuildNodeArea(Transform parent)
        {
            GameObject scrollObject = CreateScrollView("Tree Scroll", parent, out Transform content);
            LayoutElement scrollLayout = scrollObject.AddComponent<LayoutElement>();
            scrollLayout.flexibleWidth = 1;

            HorizontalLayoutGroup columns = content.gameObject.AddComponent<HorizontalLayoutGroup>();
            columns.padding = new RectOffset(10, 10, 10, 10);
            columns.spacing = 12;
            columns.childControlWidth = true;
            columns.childControlHeight = true;
            columns.childForceExpandWidth = false;
            columns.childForceExpandHeight = false;
            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            BuildColumn(content, "Productos", EntrepreneurTreeNodeType.Product);
            BuildColumn(content, "Empleados", EntrepreneurTreeNodeType.Employee);
            BuildColumn(content, "Seguridad", EntrepreneurTreeNodeType.Security);
            BuildColumn(content, "Mejoras", EntrepreneurTreeNodeType.Upgrade);
        }

        private void BuildColumn(Transform parent, string title, EntrepreneurTreeNodeType type)
        {
            GameObject column = CreatePanel(title, parent, CardBackground);
            LayoutElement columnLayout = column.AddComponent<LayoutElement>();
            columnLayout.preferredWidth = GetColumnWidth(type);
            columnLayout.minWidth = columnLayout.preferredWidth;

            VerticalLayoutGroup columnGroup = column.AddComponent<VerticalLayoutGroup>();
            columnGroup.padding = new RectOffset(8, 8, 8, 8);
            columnGroup.spacing = 6;
            columnGroup.childControlWidth = true;
            columnGroup.childControlHeight = true;
            columnGroup.childForceExpandWidth = true;
            columnGroup.childForceExpandHeight = false;

            TMP_Text columnTitle = CreateText("Title", column.transform, title, 18, FontStyles.Bold, TextAlignmentOptions.Center);
            columnTitle.color = TextDark;
            columnTitle.GetComponent<LayoutElement>().preferredHeight = 28;

            foreach (EntrepreneurTreeNodeDefinition node in EntrepreneurTreeDefinitions.Nodes)
            {
                if (node.Type != type)
                    continue;

                CreateNodeButton(column.transform, node);
            }
        }

        private void CreateNodeButton(Transform parent, EntrepreneurTreeNodeDefinition node)
        {
            GameObject nodeObject = CreatePanel(node.Id, parent, new Color(0.88f, 0.88f, 0.89f, 1f));
            LayoutElement layout = nodeObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 68;

            Button button = nodeObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = nodeObject.GetComponent<Image>();
            button.onClick.AddListener(() => SelectNode(node));

            VerticalLayoutGroup group = nodeObject.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(8, 8, 5, 5);
            group.spacing = 2;
            group.childControlWidth = true;
            group.childControlHeight = true;

            TMP_Text title = CreateText("Title", nodeObject.transform, node.Title, 14, FontStyles.Bold, TextAlignmentOptions.Left);
            title.color = TextDark;
            title.textWrappingMode = TextWrappingModes.NoWrap;
            TMP_Text state = CreateText("State", nodeObject.transform, string.Empty, 12, FontStyles.Normal, TextAlignmentOptions.Left);
            state.color = TextMuted;
            state.textWrappingMode = TextWrappingModes.Normal;

            nodeButtons[node.Id] = button;
            stateLabels[node.Id] = state;
        }

        private void BuildDetailsPanel(Transform parent)
        {
            GameObject panel = CreatePanel("Details", parent, CardBackground);
            LayoutElement layout = panel.AddComponent<LayoutElement>();
            layout.preferredWidth = 285;
            layout.minWidth = 285;

            VerticalLayoutGroup group = panel.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(12, 12, 12, 12);
            group.spacing = 10;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;

            TMP_Text title = CreateText("Title", panel.transform, "Detalle", 22, FontStyles.Bold, TextAlignmentOptions.Left);
            title.color = TextDark;
            title.GetComponent<LayoutElement>().preferredHeight = 30;

            detailsLabel = CreateText("Details Text", panel.transform, string.Empty, 15, FontStyles.Normal, TextAlignmentOptions.Left);
            detailsLabel.color = TextDark;
            detailsLabel.textWrappingMode = TextWrappingModes.Normal;
            LayoutElement detailsLayout = detailsLabel.GetComponent<LayoutElement>();
            detailsLayout.preferredHeight = 250;
            detailsLayout.flexibleHeight = 1;

            unlockButton = CreateActionButton("Unlock Button", panel.transform, "Desbloquear");
            unlockButton.onClick.AddListener(UnlockSelectedNode);
        }

        private void SelectNode(EntrepreneurTreeNodeDefinition node)
        {
            selectedNode = node;
            RefreshDetails();
        }

        private void UnlockSelectedNode()
        {
            if (selectedNode == null)
                return;

            bool unlocked = EntrepreneurProgress.TryUnlock(selectedNode.Id, out string message);
            if (UIGame.Instance != null)
                UIGame.AddNotification(message, otherColor: unlocked ? Color.green : new Color(1f, 0.78f, 0.25f), otherDuration: 4f);

            Refresh();
        }

        private void Refresh()
        {
            if (pointsLabel != null)
                pointsLabel.text = "Puntos: " + EntrepreneurProgress.ProgressPoints;

            foreach (EntrepreneurTreeNodeDefinition node in EntrepreneurTreeDefinitions.Nodes)
            {
                if (!stateLabels.TryGetValue(node.Id, out TMP_Text stateLabel))
                    continue;

                EntrepreneurTreeNodeState state = EntrepreneurProgress.GetState(node);
                stateLabel.text = EntrepreneurProgress.GetStateDescription(node);

                Image image = nodeButtons[node.Id].targetGraphic as Image;
                if (image == null)
                    continue;

                switch (state)
                {
                    case EntrepreneurTreeNodeState.Unlocked:
                        image.color = new Color(0.75f, 0.92f, 0.78f, 1f);
                        break;
                    case EntrepreneurTreeNodeState.Available:
                        image.color = new Color(0.80f, 0.88f, 1f, 1f);
                        break;
                    default:
                        image.color = new Color(0.88f, 0.88f, 0.89f, 1f);
                        break;
                }
            }

            RefreshDetails();
        }

        private void RefreshDetails()
        {
            if (selectedNode == null || detailsLabel == null)
                return;

            string requirements = selectedNode.Prerequisites.Length == 0 ? "Ninguno" : string.Join(", ", System.Array.ConvertAll(selectedNode.Prerequisites, EntrepreneurTreeDefinitions.GetTitle));
            detailsLabel.text =
                selectedNode.Title + "\n\n" +
                "Tipo: " + GetTypeLabel(selectedNode.Type) + "\n" +
                "Costo: " + selectedNode.Cost + " punto(s)\n" +
                "Estado: " + EntrepreneurProgress.GetStateDescription(selectedNode).Replace("\n", " - ") + "\n" +
                "Requisitos: " + requirements + "\n\n" +
                "Beneficio:\n" + selectedNode.Benefit;

            EntrepreneurTreeNodeState state = EntrepreneurProgress.GetState(selectedNode);
            unlockButton.interactable = state == EntrepreneurTreeNodeState.Available;
            unlockButton.GetComponentInChildren<TMP_Text>().text = state == EntrepreneurTreeNodeState.Unlocked ? "Desbloqueado" : "Desbloquear";
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
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            TextMeshProUGUI label = obj.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.enableAutoSizing = true;
            label.fontSizeMin = Mathf.Max(10, size - 5);
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

        private static GameObject CreateScrollView(string name, Transform parent, out Transform content)
        {
            GameObject scroll = CreatePanel(name, parent, new Color(1f, 1f, 1f, 0.72f));
            ScrollRect scrollRect = scroll.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = true;

            GameObject viewport = CreateLayoutBox("Viewport", scroll.transform);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(8, 8);
            viewportRect.offsetMax = new Vector2(-8, -8);
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = Color.clear;
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject contentObject = CreateLayoutBox("Content", viewport.transform);
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(0, 1);
            contentRect.pivot = new Vector2(0, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(835, 720);

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            content = contentObject.transform;
            return scroll;
        }

        private static float GetColumnWidth(EntrepreneurTreeNodeType type)
        {
            switch (type)
            {
                case EntrepreneurTreeNodeType.Product:
                    return 220;
                case EntrepreneurTreeNodeType.Employee:
                    return 185;
                default:
                    return 200;
            }
        }
    }
}
