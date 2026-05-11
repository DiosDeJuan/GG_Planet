using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    public class ExpansionZoneButtonUI : MonoBehaviour, IPointerClickHandler
    {
        public string zoneId { get; private set; }
        public TMP_Text label { get; private set; }
        public Image background { get; private set; }

        private Action<string> onSelected;

        public void Initialize(ExpansionZoneData zone, Action<string> onSelect)
        {
            if (zone == null)
                return;

            zoneId = zone.id;
            onSelected = onSelect;
            background = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            label = GetComponentInChildren<TMP_Text>(true);
            if (label == null)
            {
                GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                labelObj.transform.SetParent(transform, false);
                RectTransform labelRT = labelObj.GetComponent<RectTransform>();
                labelRT.anchorMin = Vector2.zero;
                labelRT.anchorMax = Vector2.one;
                labelRT.offsetMin = new Vector2(6f, 6f);
                labelRT.offsetMax = new Vector2(-6f, -6f);
                label = labelObj.GetComponent<TextMeshProUGUI>();
                label.alignment = TextAlignmentOptions.Center;
#if TMP_VERSION_3_0_0_OR_NEWER || TMP_VERSION_4_0_0_OR_NEWER
                label.textWrappingMode = TextWrappingModes.Normal;
#else
                label.enableWordWrapping = true;
#endif
                label.fontSize = 16f;
                label.color = Color.white;
            }

            label.text = zone.displayName;
            background.color = GetColor(zone);
        }

        public void Refresh(ExpansionZoneData zone)
        {
            if (zone == null || background == null)
                return;

            background.color = GetColor(zone);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onSelected?.Invoke(zoneId);
        }

        private static Color GetColor(ExpansionZoneData zone)
        {
            switch (zone.state)
            {
                case ExpansionZoneState.Purchased:
                    return new Color(0.21f, 0.67f, 0.34f, 0.92f);
                case ExpansionZoneState.Available:
                    return new Color(0.88f, 0.25f, 0.55f, 0.9f);
                default:
                    return new Color(0.35f, 0.35f, 0.39f, 0.9f);
            }
        }
    }
}
