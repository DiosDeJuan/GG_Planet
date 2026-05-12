using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Renders the expansion zone grid inside the map panel.
    /// Call Rebuild() to create all zone buttons from scratch.
    /// Call Refresh() to update colours/labels of existing buttons.
    /// Call SetSelectedZone() to highlight the currently selected zone.
    /// </summary>
    public class ExpansionMapRenderer : MonoBehaviour
    {
        private const string LogPrefix = "[Expansion] ";

        private readonly Dictionary<string, ExpansionZoneButtonUI> buttonsByZone =
            new Dictionary<string, ExpansionZoneButtonUI>();

        private RectTransform mapRoot;
        private ExpansionAppUIController appController;
        private string currentSelectedId;

        public void Initialize(RectTransform root, ExpansionAppUIController controller)
        {
            mapRoot       = root;
            appController = controller;
        }

        /// <summary>Destroys all existing zone buttons and recreates them from the zone list.</summary>
        public void Rebuild(IReadOnlyList<ExpansionZoneData> zones)
        {
            if (mapRoot == null || zones == null)
                return;

            for (int i = mapRoot.childCount - 1; i >= 0; i--)
                Destroy(mapRoot.GetChild(i).gameObject);

            buttonsByZone.Clear();
            currentSelectedId = null;

            // Map background grid (subtle dark tint already comes from the panel Image).
            // Draw a slightly lighter grid pattern as a single child image.
            DrawGridBackground(zones);

            for (int i = 0; i < zones.Count; i++)
            {
                ExpansionZoneData zone = zones[i];
                if (zone == null || string.IsNullOrEmpty(zone.id))
                    continue;

                GameObject zoneObj = new GameObject(zone.id,
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
                    typeof(ExpansionZoneButtonUI));
                zoneObj.transform.SetParent(mapRoot, false);

                RectTransform rt = zoneObj.GetComponent<RectTransform>();
                rt.anchorMin      = new Vector2(0f, 0f);
                rt.anchorMax      = new Vector2(0f, 0f);
                rt.pivot          = new Vector2(0f, 0f);
                rt.anchoredPosition = zone.mapPosition;
                rt.sizeDelta        = zone.mapSize;

                ExpansionZoneButtonUI btn = zoneObj.GetComponent<ExpansionZoneButtonUI>();
                btn.Initialize(zone, OnZoneClicked);
                buttonsByZone[zone.id] = btn;
            }

            Debug.Log(LogPrefix + "Map rebuilt: " + buttonsByZone.Count + " zones.");
        }

        /// <summary>Updates colours/labels of existing zone buttons without recreating them.</summary>
        public void Refresh(IReadOnlyList<ExpansionZoneData> zones)
        {
            if (zones == null)
                return;

            for (int i = 0; i < zones.Count; i++)
            {
                ExpansionZoneData zone = zones[i];
                if (zone == null)
                    continue;

                ExpansionZoneButtonUI btn;
                if (buttonsByZone.TryGetValue(zone.id, out btn) && btn != null)
                    btn.Refresh(zone);
            }
        }

        /// <summary>Highlights the given zone and clears any previous highlight.</summary>
        public void SetSelectedZone(string zoneId)
        {
            // Deselect previous
            if (!string.IsNullOrEmpty(currentSelectedId))
            {
                ExpansionZoneButtonUI prev;
                if (buttonsByZone.TryGetValue(currentSelectedId, out prev) && prev != null)
                    prev.SetSelected(false);
            }

            currentSelectedId = zoneId;

            if (!string.IsNullOrEmpty(zoneId))
            {
                ExpansionZoneButtonUI next;
                if (buttonsByZone.TryGetValue(zoneId, out next) && next != null)
                    next.SetSelected(true);
            }
        }

        // ── Internals ─────────────────────────────────────────────────────────

        private void OnZoneClicked(string zoneId)
        {
            appController?.SelectZone(zoneId);
        }

        private void DrawGridBackground(IReadOnlyList<ExpansionZoneData> zones)
        {
            // We render just a subtle background rectangle behind the zone cells.
            // Actual grid lines would need a custom mesh; a dark backdrop is sufficient.
            GameObject bg = new GameObject("MapBackground",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bg.transform.SetParent(mapRoot, false);

            RectTransform rt = bg.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            bg.GetComponent<Image>().color = new Color(0.10f, 0.12f, 0.16f, 0.6f);
            bg.transform.SetAsFirstSibling();
        }
    }
}
