//Adaptado por POMPIC 20100333
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    public class EntrepreneurTreeUI : MonoBehaviour
    {
        private readonly Dictionary<string, TMP_Text> stateLabels = new Dictionary<string, TMP_Text>();
        private readonly Dictionary<string, Button> nodeButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, GameObject> nodeCards = new Dictionary<string, GameObject>();
        private readonly Dictionary<EntrepreneurTreeNodeCategory?, Button> filterButtons = new Dictionary<EntrepreneurTreeNodeCategory?, Button>();

        private TMP_Text pointsLabel;
        private TMP_Text detailsLabel;
        private TMP_Text selectedStateLabel;
        private TMP_Text filterLabel;
        private Button unlockButton;
        private EntrepreneurTreeNodeDefinition selectedNode;
        private EntrepreneurTreeNodeCategory? currentFilter;
        private bool built;

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
            if (built)
                return;

            built = true;
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image background = gameObject.AddComponent<Image>();
            background.color = new Color(0.05f, 0.06f, 0.07f, 0.94f);

            VerticalLayoutGroup rootLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(18, 18, 16, 16);
            rootLayout.spacing = 10;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;

            BuildHeader(transform);
            BuildFilters(transform);
            BuildBody(transform);

            SelectNode(EntrepreneurTreeDefinitions.Get(EntrepreneurTreeDefinitions.DefaultUnlockedNodeId));
            ApplyFilter(null);
            Refresh();
        }

        private void BuildHeader(Transform parent)
        {
            GameObject header = CreatePanel("Header", parent, new Color(0.09f, 0.11f, 0.13f, 0.95f));
            LayoutElement headerLayout = header.AddComponent<LayoutElement>();
            headerLayout.preferredHeight = 110;

            HorizontalLayoutGroup headerGroup = header.AddComponent<HorizontalLayoutGroup>();
            headerGroup.padding = new RectOffset(14, 14, 10, 10);
            headerGroup.spacing = 16;
            headerGroup.childControlWidth = true;
            headerGroup.childControlHeight = true;
            headerGroup.childForceExpandWidth = true;
            headerGroup.childForceExpandHeight = false;

            GameObject textBox = CreateLayoutBox("Text", header.transform);
            VerticalLayoutGroup textLayout = textBox.AddComponent<VerticalLayoutGroup>();
            textLayout.spacing = 4;
            textLayout.childControlWidth = true;
            textLayout.childControlHeight = true;

            TMP_Text title = CreateText("Title", textBox.transform, "Árbol del Emprendedor", 28, FontStyles.Bold, TextAlignmentOptions.Left);
            title.color = Color.white;

            TMP_Text subtitle = CreateText(
                "Subtitle",
                textBox.transform,
                "Desbloquea productos, empleados, seguridad y mejoras para hacer crecer tu supermercado.",
                14,
                FontStyles.Normal,
                TextAlignmentOptions.Left);
            subtitle.color = new Color(0.84f, 0.87f, 0.89f);

            filterLabel = CreateText("FilterLabel", textBox.transform, "Vista: Todo", 13, FontStyles.Italic, TextAlignmentOptions.Left);
            filterLabel.color = new Color(0.67f, 0.86f, 1f, 1f);

            pointsLabel = CreateText("Points", header.transform, "Puntos: 0", 22, FontStyles.Bold, TextAlignmentOptions.Right);
            pointsLabel.color = new Color(0.55f, 0.9f, 0.55f);
            pointsLabel.GetComponent<LayoutElement>().preferredWidth = 230;
        }

        private void BuildFilters(Transform parent)
        {
            GameObject bar = CreatePanel("Filters", parent, new Color(0.08f, 0.1f, 0.12f, 0.95f));
            LayoutElement barLayout = bar.AddComponent<LayoutElement>();
            barLayout.preferredHeight = 56;

            HorizontalLayoutGroup barGroup = bar.AddComponent<HorizontalLayoutGroup>();
            barGroup.padding = new RectOffset(8, 8, 8, 8);
            barGroup.spacing = 8;
            barGroup.childControlWidth = true;
            barGroup.childControlHeight = true;
            barGroup.childForceExpandWidth = false;
            barGroup.childForceExpandHeight = true;

            CreateFilterButton(bar.transform, "Todo", null);
            CreateFilterButton(bar.transform, "Productos", EntrepreneurTreeNodeCategory.Productos);
            CreateFilterButton(bar.transform, "Empleados", EntrepreneurTreeNodeCategory.Empleados);
            CreateFilterButton(bar.transform, "Seguridad", EntrepreneurTreeNodeCategory.Seguridad);
            CreateFilterButton(bar.transform, "Mejoras", EntrepreneurTreeNodeCategory.Mejoras);
        }

        private void CreateFilterButton(Transform parent, string label, EntrepreneurTreeNodeCategory? category)
        {
            Button button = CreateActionButton("Filter_" + label, parent, label);
            LayoutElement layout = button.GetComponent<LayoutElement>();
            layout.preferredHeight = 38;
            layout.preferredWidth = 130;
            button.onClick.AddListener(() => ApplyFilter(category));
            filterButtons[category] = button;
        }

        private void BuildBody(Transform parent)
        {
            GameObject body = CreateLayoutBox("Body", parent);
            LayoutElement bodyLayout = body.AddComponent<LayoutElement>();
            bodyLayout.flexibleHeight = 1;
            bodyLayout.minHeight = 450;

            HorizontalLayoutGroup bodyGroup = body.AddComponent<HorizontalLayoutGroup>();
            bodyGroup.spacing = 12;
            bodyGroup.childControlWidth = true;
            bodyGroup.childControlHeight = true;
            bodyGroup.childForceExpandWidth = true;
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
            columns.spacing = 10;
            columns.childControlWidth = true;
            columns.childControlHeight = true;
            columns.childForceExpandWidth = false;
            columns.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            int minColumn = EntrepreneurTreeDefinitions.Nodes.Min(node => node.VisualColumn);
            int maxColumn = EntrepreneurTreeDefinitions.Nodes.Max(node => node.VisualColumn);
            for (int columnIndex = minColumn; columnIndex <= maxColumn; columnIndex++)
            {
                BuildColumn(content, columnIndex);
            }
        }

        private void BuildColumn(Transform parent, int columnIndex)
        {
            List<EntrepreneurTreeNodeDefinition> nodesInColumn = EntrepreneurTreeDefinitions.Nodes
                .Where(node => node.VisualColumn == columnIndex)
                .OrderBy(node => node.VisualRow)
                .ThenBy(node => node.SortOrder)
                .ToList();

            if (nodesInColumn.Count == 0)
                return;

            GameObject column = CreatePanel("Column_" + columnIndex, parent, new Color(0.09f, 0.11f, 0.13f, 0.95f));
            LayoutElement columnLayout = column.AddComponent<LayoutElement>();
            columnLayout.preferredWidth = 235;
            columnLayout.minWidth = 235;

            VerticalLayoutGroup columnGroup = column.AddComponent<VerticalLayoutGroup>();
            columnGroup.padding = new RectOffset(8, 8, 8, 8);
            columnGroup.spacing = 8;
            columnGroup.childControlWidth = true;
            columnGroup.childControlHeight = true;
            columnGroup.childForceExpandWidth = true;
            columnGroup.childForceExpandHeight = false;

            int previousRow = 0;
            for (int i = 0; i < nodesInColumn.Count; i++)
            {
                EntrepreneurTreeNodeDefinition node = nodesInColumn[i];
                int rowGap = Mathf.Max(0, node.VisualRow - previousRow - 1);
                for (int gapIndex = 0; gapIndex < rowGap; gapIndex++)
                    CreateSpacer(column.transform, 20);

                CreateNodeButton(column.transform, node);
                previousRow = node.VisualRow;
            }
        }

        private void CreateNodeButton(Transform parent, EntrepreneurTreeNodeDefinition node)
        {
            GameObject nodeObject = CreatePanel(node.Id, parent, new Color(0.16f, 0.19f, 0.22f, 1f));
            LayoutElement layout = nodeObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 108;

            Button button = nodeObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = nodeObject.GetComponent<Image>();
            button.onClick.AddListener(() => SelectNode(node));

            VerticalLayoutGroup group = nodeObject.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(8, 8, 8, 8);
            group.spacing = 2;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;

            TMP_Text title = CreateText("Title", nodeObject.transform, node.DisplayName, 14, FontStyles.Bold, TextAlignmentOptions.Left);
            title.color = Color.white;

            TMP_Text categoryText = CreateText("Category", nodeObject.transform, GetCategoryLabel(node.Category), 12, FontStyles.Normal, TextAlignmentOptions.Left);
            categoryText.color = new Color(0.72f, 0.8f, 0.86f);

            TMP_Text stateText = CreateText("State", nodeObject.transform, string.Empty, 12, FontStyles.Normal, TextAlignmentOptions.Left);
            stateText.color = new Color(0.9f, 0.92f, 0.94f);
            stateText.textWrappingMode = TextWrappingModes.Normal;

            nodeButtons[node.Id] = button;
            stateLabels[node.Id] = stateText;
            nodeCards[node.Id] = nodeObject;
        }

        private void BuildDetailsPanel(Transform parent)
        {
            GameObject panel = CreatePanel("Details", parent, new Color(0.09f, 0.1f, 0.12f, 0.95f));
            LayoutElement layout = panel.AddComponent<LayoutElement>();
            layout.preferredWidth = 340;
            layout.minWidth = 340;

            VerticalLayoutGroup group = panel.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(14, 14, 14, 14);
            group.spacing = 10;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;

            TMP_Text title = CreateText("Title", panel.transform, "Detalle de nodo", 22, FontStyles.Bold, TextAlignmentOptions.Left);
            title.color = Color.white;
            title.GetComponent<LayoutElement>().preferredHeight = 32;

            selectedStateLabel = CreateText("State", panel.transform, "Estado: -", 13, FontStyles.Bold, TextAlignmentOptions.Left);
            selectedStateLabel.color = new Color(0.93f, 0.95f, 0.97f);

            detailsLabel = CreateText("Details", panel.transform, string.Empty, 14, FontStyles.Normal, TextAlignmentOptions.Left);
            detailsLabel.color = new Color(0.88f, 0.9f, 0.92f);
            detailsLabel.textWrappingMode = TextWrappingModes.Normal;
            LayoutElement detailsLayout = detailsLabel.GetComponent<LayoutElement>();
            detailsLayout.preferredHeight = 320;
            detailsLayout.flexibleHeight = 1;

            unlockButton = CreateActionButton("Unlock Button", panel.transform, "Desbloquear");
            unlockButton.onClick.AddListener(UnlockSelectedNode);
        }

        private void ApplyFilter(EntrepreneurTreeNodeCategory? category)
        {
            currentFilter = category;
            string filterText = category.HasValue ? GetCategoryLabel(category.Value) : "Todo";
            if (filterLabel != null)
                filterLabel.text = "Vista: " + filterText;

            foreach (EntrepreneurTreeNodeDefinition node in EntrepreneurTreeDefinitions.Nodes)
            {
                bool visible = !category.HasValue || node.Category == category.Value;
                if (nodeCards.TryGetValue(node.Id, out GameObject card))
                    card.SetActive(visible);
            }

            UpdateFilterVisuals();
            RefreshDetails();
        }

        private void UpdateFilterVisuals()
        {
            foreach (KeyValuePair<EntrepreneurTreeNodeCategory?, Button> entry in filterButtons)
            {
                Image image = entry.Value.targetGraphic as Image;
                if (image == null)
                    continue;

                bool selected = entry.Key == currentFilter;
                image.color = selected ? new Color(0.18f, 0.45f, 0.77f, 1f) : new Color(0.14f, 0.24f, 0.35f, 1f);
            }
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

                if (!nodeButtons.TryGetValue(node.Id, out Button button))
                    continue;

                Image image = button.targetGraphic as Image;
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

            bool selectedVisible = !currentFilter.HasValue || selectedNode.Category == currentFilter.Value;
            if (!selectedVisible)
                selectedNode = EntrepreneurTreeDefinitions.Get(EntrepreneurTreeDefinitions.DefaultUnlockedNodeId);

            if (selectedNode == null)
                return;

            string requirements = selectedNode.RequiredNodeIds.Length == 0
                ? "Ninguno"
                : string.Join(", ", Array.ConvertAll(selectedNode.RequiredNodeIds, EntrepreneurTreeDefinitions.GetDisplayName));

            string products = selectedNode.ProductNames.Length == 0
                ? "No aplica"
                : string.Join(", ", selectedNode.ProductNames);

            string stateDescription = EntrepreneurProgress.GetStateDescription(selectedNode).Replace("\n", " - ");
            string blockedReason = EntrepreneurProgress.GetNodeBlockReason(selectedNode);

            if (selectedStateLabel != null)
                selectedStateLabel.text = "Estado: " + stateDescription;

            detailsLabel.text =
                "Nombre: " + selectedNode.DisplayName + "\n" +
                "Categoría: " + GetCategoryLabel(selectedNode.Category) + "\n" +
                "Costo: " + selectedNode.Cost + " punto(s)\n" +
                "Requisitos: " + requirements + "\n" +
                "Razón actual: " + blockedReason + "\n\n" +
                "Descripción:\n" + selectedNode.Description + "\n\n" +
                "Beneficio:\n" + selectedNode.Benefit + "\n\n" +
                "Productos incluidos:\n" + products;

            EntrepreneurTreeNodeState state = EntrepreneurProgress.GetState(selectedNode);
            unlockButton.interactable = state == EntrepreneurTreeNodeState.Available;
            unlockButton.GetComponentInChildren<TMP_Text>().text = state == EntrepreneurTreeNodeState.Unlocked ? "Desbloqueado" : "Desbloquear";
        }

        private static void CreateSpacer(Transform parent, float height)
        {
            GameObject spacer = CreateLayoutBox("Spacer", parent);
            LayoutElement layout = spacer.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
        }

        private static string GetCategoryLabel(EntrepreneurTreeNodeCategory category)
        {
            switch (category)
            {
                case EntrepreneurTreeNodeCategory.Productos: return "Productos";
                case EntrepreneurTreeNodeCategory.Empleados: return "Empleados";
                case EntrepreneurTreeNodeCategory.Seguridad: return "Seguridad";
                default: return "Mejoras";
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
            GameObject obj = CreatePanel(name, parent, new Color(0.14f, 0.24f, 0.35f, 1f));
            LayoutElement layout = obj.AddComponent<LayoutElement>();
            layout.preferredHeight = 48;
            Button button = obj.AddComponent<Button>();
            button.targetGraphic = obj.GetComponent<Image>();
            TMP_Text text = CreateText("Text", obj.transform, label, 16, FontStyles.Bold, TextAlignmentOptions.Center);
            text.color = Color.white;
            return button;
        }

        private static GameObject CreateScrollView(string name, Transform parent, out Transform content)
        {
            GameObject scroll = CreatePanel(name, parent, new Color(0.07f, 0.08f, 0.09f, 0.95f));
            ScrollRect scrollRect = scroll.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 25f;

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
            contentRect.sizeDelta = new Vector2(1600, 1400);

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            content = contentObject.transform;
            return scroll;
        }
    }
}
