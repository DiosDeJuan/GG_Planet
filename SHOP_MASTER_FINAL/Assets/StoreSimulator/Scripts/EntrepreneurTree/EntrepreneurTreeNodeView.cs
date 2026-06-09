//Adaptado por POMPIC 20100333
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    public class EntrepreneurTreeNodeView : MonoBehaviour
    {
        private static readonly Color LockedBackground = new Color(0.82f, 0.82f, 0.84f, 1f);
        private static readonly Color AvailableBackground = new Color(1f, 0.91f, 0.96f, 1f);
        private static readonly Color UnlockedBackground = new Color(0.74f, 0.91f, 0.77f, 1f);
        private static readonly Color DesktopPink = new Color(1f, 0f, 0.392f, 1f);
        private static readonly Color TextDark = new Color(0.12f, 0.12f, 0.13f, 1f);
        private static readonly Color TextMuted = new Color(0.32f, 0.33f, 0.35f, 1f);

        private Image background;
        private Outline outline;
        private TMP_Text titleLabel;
        private TMP_Text metaLabel;

        public EntrepreneurTreeNodeDefinition Node { get; private set; }
        public RectTransform Rect { get; private set; }

        public void Initialize(EntrepreneurTreeNodeDefinition node, UnityAction<EntrepreneurTreeNodeDefinition> onSelected)
        {
            Node = node;
            Rect = GetComponent<RectTransform>();

            background = gameObject.AddComponent<Image>();
            background.color = LockedBackground;

            Button button = gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.ColorTint;
            button.onClick.AddListener(() => onSelected?.Invoke(Node));

            outline = gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(2f, -2f);
            outline.enabled = false;

            VerticalLayoutGroup group = gameObject.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(8, 8, 6, 6);
            group.spacing = 2;
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;

            titleLabel = CreateText("Title", transform, 15, FontStyles.Bold);
            titleLabel.color = TextDark;
            LayoutElement titleLayout = titleLabel.GetComponent<LayoutElement>();
            titleLayout.preferredHeight = 34;

            metaLabel = CreateText("Meta", transform, 11, FontStyles.Bold);
            metaLabel.color = TextMuted;
            LayoutElement metaLayout = metaLabel.GetComponent<LayoutElement>();
            metaLayout.preferredHeight = 18;

            Refresh(false);
        }

        public void Refresh(bool selected)
        {
            EntrepreneurTreeNodeState state = EntrepreneurProgress.GetState(Node);
            background.color = GetBackgroundColor(state);

            titleLabel.text = GetTypeTag(Node.Type) + " " + Node.Title;
            metaLabel.text = GetStateText(state) + " | " + Node.Cost + "P";

            outline.enabled = selected || state == EntrepreneurTreeNodeState.Available;
            outline.effectColor = selected ? Color.black : DesktopPink;
            outline.effectDistance = selected ? new Vector2(3f, -3f) : new Vector2(2f, -2f);
        }

        private static TMP_Text CreateText(string name, Transform parent, int fontSize, FontStyles style)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);

            TextMeshProUGUI label = obj.AddComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.enableAutoSizing = true;
            label.fontSizeMin = 8;
            label.fontSizeMax = fontSize;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.raycastTarget = false;

            obj.AddComponent<LayoutElement>();
            return label;
        }

        private static Color GetBackgroundColor(EntrepreneurTreeNodeState state)
        {
            switch (state)
            {
                case EntrepreneurTreeNodeState.Unlocked:
                    return UnlockedBackground;
                case EntrepreneurTreeNodeState.Available:
                    return AvailableBackground;
                default:
                    return LockedBackground;
            }
        }

        private static string GetStateText(EntrepreneurTreeNodeState state)
        {
            switch (state)
            {
                case EntrepreneurTreeNodeState.Unlocked:
                    return "Desbloqueado";
                case EntrepreneurTreeNodeState.Available:
                    return "Disponible";
                default:
                    return "Bloqueado";
            }
        }

        private static string GetTypeTag(EntrepreneurTreeNodeType type)
        {
            switch (type)
            {
                case EntrepreneurTreeNodeType.Product:
                    return "P";
                case EntrepreneurTreeNodeType.Employee:
                    return "E";
                case EntrepreneurTreeNodeType.Security:
                    return "S";
                default:
                    return "M";
            }
        }
    }
}
