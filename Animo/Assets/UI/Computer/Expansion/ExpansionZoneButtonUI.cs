using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Represents one zone cell on the expansion map.
    /// Handles click, visual state, and selection highlight.
    /// </summary>
    public class ExpansionZoneButtonUI : MonoBehaviour, IPointerClickHandler
    {
        public string zoneId { get; private set; }

        private Image background;
        private TMP_Text label;
        private Image selectedBorder;
        private Action<string> onSelected;
        private ExpansionZoneState lastState;
        private ExpansionZoneType lastType;
        private bool isSelected;

        // ── Colours per type + state ──────────────────────────────────────────

        // Sales
        private static readonly Color SalesPurchased   = new Color(0.20f, 0.65f, 0.32f, 0.95f); // green
        private static readonly Color SalesAvailable   = new Color(0.88f, 0.25f, 0.55f, 0.92f); // pink
        private static readonly Color SalesBlocked     = new Color(0.32f, 0.30f, 0.34f, 0.92f); // dark grey

        // Storage
        private static readonly Color StoragePurchased = new Color(0.18f, 0.58f, 0.72f, 0.95f); // teal-blue
        private static readonly Color StorageAvailable = new Color(0.24f, 0.46f, 0.82f, 0.92f); // blue
        private static readonly Color StorageBlocked   = new Color(0.28f, 0.30f, 0.38f, 0.92f); // blue-grey

        // Office
        private static readonly Color OfficePurchased  = new Color(0.52f, 0.48f, 0.42f, 0.95f); // warm grey
        private static readonly Color OfficeOther      = new Color(0.32f, 0.32f, 0.32f, 0.92f);

        // Selection border
        private static readonly Color BorderSelected   = new Color(1f, 0.92f, 0.28f, 1f);  // bright yellow

        // ── Lifecycle ─────────────────────────────────────────────────────────

        public void Initialize(ExpansionZoneData zone, Action<string> onSelect)
        {
            if (zone == null)
                return;

            zoneId    = zone.id;
            onSelected = onSelect;

            background = GetComponent<Image>();
            if (background == null)
                background = gameObject.AddComponent<Image>();

            EnsureLabel();
            EnsureBorder();

            Refresh(zone);
        }

        public void Refresh(ExpansionZoneData zone)
        {
            if (zone == null || background == null)
                return;

            lastState = zone.state;
            lastType  = zone.type;
            background.color = GetBaseColor(zone.type, zone.state);
            UpdateLabel(zone);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (selectedBorder != null)
                selectedBorder.enabled = selected;
            if (label != null)
                label.color = selected ? new Color(1f, 0.95f, 0.25f) : Color.white;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onSelected?.Invoke(zoneId);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void EnsureLabel()
        {
            label = GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                return;

            GameObject labelObj = new GameObject("Label",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObj.transform.SetParent(transform, false);
            RectTransform rt = labelObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(3f, 3f);
            rt.offsetMax = new Vector2(-3f, -3f);

            label = labelObj.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
#if TMP_VERSION_3_0_0_OR_NEWER || TMP_VERSION_4_0_0_OR_NEWER
            label.textWrappingMode = TextWrappingModes.Normal;
#else
            label.enableWordWrapping = true;
#endif
            label.fontSize  = 11f;
            label.color     = Color.white;
        }

        private void EnsureBorder()
        {
            Transform existing = transform.Find("SelectionBorder");
            if (existing != null)
            {
                selectedBorder = existing.GetComponent<Image>();
                return;
            }

            GameObject borderObj = new GameObject("SelectionBorder",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            borderObj.transform.SetParent(transform, false);
            borderObj.transform.SetAsFirstSibling();

            RectTransform rt = borderObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-3f, -3f);
            rt.offsetMax = new Vector2(3f, 3f);

            selectedBorder = borderObj.GetComponent<Image>();
            selectedBorder.color = new Color(1f, 0.92f, 0.28f, 0.55f);
            selectedBorder.enabled = false;
        }

        private void UpdateLabel(ExpansionZoneData zone)
        {
            if (label == null)
                return;

            string typeAbbr = zone.type == ExpansionZoneType.Sales    ? "V"
                            : zone.type == ExpansionZoneType.Storage   ? "A"
                            : "O";

            string stateLine = zone.state == ExpansionZoneState.Purchased ? "✓"
                             : zone.state == ExpansionZoneState.Available  ? "+"
                             : "✗";

            label.text = typeAbbr + "\n" + stateLine;
        }

        private static Color GetBaseColor(ExpansionZoneType type, ExpansionZoneState state)
        {
            switch (type)
            {
                case ExpansionZoneType.Sales:
                    return state == ExpansionZoneState.Purchased ? SalesPurchased
                         : state == ExpansionZoneState.Available  ? SalesAvailable
                         : SalesBlocked;

                case ExpansionZoneType.Storage:
                    return state == ExpansionZoneState.Purchased ? StoragePurchased
                         : state == ExpansionZoneState.Available  ? StorageAvailable
                         : StorageBlocked;

                default: // Office
                    return state == ExpansionZoneState.Purchased ? OfficePurchased : OfficeOther;
            }
        }
    }
}
