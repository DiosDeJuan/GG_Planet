//Adaptado por POMPIC 20100333
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    public class EntrepreneurTreeUI : MonoBehaviour
    {
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
            background.color = new Color(0.05f, 0.06f, 0.07f, 0.94f);

            VerticalLayoutGroup rootLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(22, 22, 18, 18);
            rootLayout.spacing = 12;
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
            GameObject header = CreatePanel("Header", parent, new Color(0.09f, 0.11f, 0.13f, 0.95f));
            LayoutElement headerLayout = header.AddComponent<LayoutElement>();
            headerLayout.preferredHeight = 92;

            HorizontalLayoutGroup headerGroup = header.AddComponent<HorizontalLayoutGroup>();
            headerGroup.padding = new RectOffset(16, 16, 10, 10);
            headerGroup.spacing = 20;
            headerGroup.childControlWidth = true;
            headerGroup.childControlHeight = true;
            headerGroup.childForceExpandWidth = true;
            headerGroup.childForceExpandHeight = true;

            GameObject textBox = CreateLayoutBox("Text", header.transform);
            VerticalLayoutGroup textLayout = textBox.AddComponent<VerticalLayoutGroup>();
            textLayout.spacing = 2;
            textLayout.childControlWidth = true;
            textLayout.childControlHeight = true;

            TMP_Text title = CreateText("Title", textBox.transform, "Arbol del Emprendedor", 28, FontStyles.Bold, TextAlignmentOptions.Left);
            title.color = Color.white;
            TMP_Text subtitle = CreateText("Subtitle", textBox.transform, "Desbloquea productos, empleados, seguridad y mejoras para expandir tu supermercado.", 15, FontStyles.Normal, TextAlignmentOptions.Left);
            subtitle.color = new Color(0.83f, 0.86f, 0.88f);

            pointsLabel = CreateText("Points", header.transform, string.Empty, 22, FontStyles.Bold, TextAlignmentOptions.Right);
            pointsLabel.color = new Color(0.55f, 0.9f, 0.55f);
            pointsLabel.GetComponent<LayoutElement>().preferredWidth = 210;
        }

        private void BuildBody(Transform parent)
        {
            GameObject body = CreateLayoutBox("Body", parent);
            LayoutElement bodyLayout = body.AddComponent<LayoutElement>();
            bodyLayout.flexibleHeight = 1;
            bodyLayout.minHeight = 430;

            HorizontalLayoutGroup bodyGroup = body.AddComponent<HorizontalLayoutGroup>();
            bodyGroup.spacing = 14;
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
            GameObject column = CreatePanel(title, parent, new Color(0.1f, 0.12f, 0.14f, 0.95f));
            LayoutElement columnLayout = column.AddComponent<LayoutElement>();
            columnLayout.preferredWidth = type == EntrepreneurTreeNodeType.Employee ? 210 : 230;
            columnLayout.minWidth = columnLayout.preferredWidth;

            VerticalLayoutGroup columnGroup = column.AddComponent<VerticalLayoutGroup>();
            columnGroup.padding = new RectOffset(8, 8, 8, 8);
            columnGroup.spacing = 7;
            columnGroup.childControlWidth = true;
            columnGroup.childControlHeight = true;
            columnGroup.childForceExpandWidth = true;
            columnGroup.childForceExpandHeight = false;

            TMP_Text columnTitle = CreateText("Title", column.transform, title, 18, FontStyles.Bold, TextAlignmentOptions.Center);
            columnTitle.color = Color.white;
            columnTitle.GetComponent<LayoutElement>().preferredHeight = 30;

            foreach (EntrepreneurTreeNodeDefinition node in EntrepreneurTreeDefinitions.Nodes)
            {
                if (node.Type != type)
                    continue;

                CreateNodeButton(column.transform, node);
            }
        }

        private void CreateNodeButton(Transform parent, EntrepreneurTreeNodeDefinition node)
        {
            GameObject nodeObject = CreatePanel(node.Id, parent, new Color(0.17f, 0.19f, 0.21f, 1f));
            LayoutElement layout = nodeObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 76;

            Button button = nodeObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = nodeObject.GetComponent<Image>();
            button.onClick.AddListener(() => SelectNode(node));

            VerticalLayoutGroup group = nodeObject.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(8, 8, 6, 6);
            group.spacing = 2;
            group.childControlWidth = true;
            group.childControlHeight = true;

            TMP_Text title = CreateText("Title", nodeObject.transform, node.Title, 14, FontStyles.Bold, TextAlignmentOptions.Left);
            title.color = Color.white;
            TMP_Text state = CreateText("State", nodeObject.transform, string.Empty, 12, FontStyles.Normal, TextAlignmentOptions.Left);
            state.color = new Color(0.86f, 0.88f, 0.9f);
            state.textWrappingMode = TextWrappingModes.Normal;

            nodeButtons[node.Id] = button;
            stateLabels[node.Id] = state;
        }

        private void BuildDetailsPanel(Transform parent)
        {
            GameObject panel = CreatePanel("Details", parent, new Color(0.09f, 0.1f, 0.12f, 0.95f));
            LayoutElement layout = panel.AddComponent<LayoutElement>();
            layout.preferredWidth = 310;
            layout.minWidth = 310;

            VerticalLayoutGroup group = panel.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(14, 14, 14, 14);
            group.spacing = 12;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;

            TMP_Text title = CreateText("Title", panel.transform, "Detalle", 22, FontStyles.Bold, TextAlignmentOptions.Left);
            title.color = Color.white;
            title.GetComponent<LayoutElement>().preferredHeight = 32;

            detailsLabel = CreateText("Details Text", panel.transform, string.Empty, 15, FontStyles.Normal, TextAlignmentOptions.Left);
            detailsLabel.color = new Color(0.88f, 0.9f, 0.92f);
            detailsLabel.textWrappingMode = TextWrappingModes.Normal;
            LayoutElement detailsLayout = detailsLabel.GetComponent<LayoutElement>();
            detailsLayout.preferredHeight = 290;
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
                        image.color = new Color(0.17f, 0.35f, 0.2f, 1f);
                        break;
                    case EntrepreneurTreeNodeState.Available:
                        image.color = new Color(0.18f, 0.28f, 0.42f, 1f);
                        break;
                    default:
                        image.color = new Color(0.18f, 0.18f, 0.19f, 1f);
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
            label.raycastTarget = false;
            obj.AddComponent<LayoutElement>();
            return label;
        }

        private static Button CreateActionButton(string name, Transform parent, string label)
        {
            GameObject obj = CreatePanel(name, parent, new Color(0.22f, 0.45f, 0.75f, 1f));
            LayoutElement layout = obj.AddComponent<LayoutElement>();
            layout.preferredHeight = 48;
            Button button = obj.AddComponent<Button>();
            button.targetGraphic = obj.GetComponent<Image>();
            TMP_Text text = CreateText("Text", obj.transform, label, 18, FontStyles.Bold, TextAlignmentOptions.Center);
            text.color = Color.white;
            return button;
        }

        private static GameObject CreateScrollView(string name, Transform parent, out Transform content)
        {
            GameObject scroll = CreatePanel(name, parent, new Color(0.07f, 0.08f, 0.09f, 0.95f));
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
            contentRect.sizeDelta = new Vector2(900, 900);

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            content = contentObject.transform;
            return scroll;
        }
    }
}
