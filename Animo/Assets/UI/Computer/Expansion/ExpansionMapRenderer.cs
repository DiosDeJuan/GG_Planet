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

            if (zones.Count == 0)
                return;

            // ── Compute bounds of all zone definitions ────────────────────────
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            for (int i = 0; i < zones.Count; i++)
            {
                ExpansionZoneData z = zones[i];
                if (z == null) continue;
                if (z.mapPosition.x < minX) minX = z.mapPosition.x;
                if (z.mapPosition.y < minY) minY = z.mapPosition.y;
                float rx = z.mapPosition.x + z.mapSize.x;
                float ry = z.mapPosition.y + z.mapSize.y;
                if (rx > maxX) maxX = rx;
                if (ry > maxY) maxY = ry;
            }

            float contentW = maxX - minX;
            float contentH = maxY - minY;
            if (contentW <= 0f || contentH <= 0f)
                return;

            // ── Read available panel size; force layout so size is current ────
            Canvas.ForceUpdateCanvases();
            Vector2 panelSize = mapRoot.rect.size;

            // If canvas hasn't been laid out yet, fall back to a safe default.
            if (panelSize.x <= 0f || panelSize.y <= 0f)
                panelSize = new Vector2(400f, 320f);

            const float Padding = 16f;
            float availW = panelSize.x - Padding * 2f;
            float availH = panelSize.y - Padding * 2f;

            // Uniform scale to fit content inside available area while keeping aspect ratio.
            float scaleX = availW / contentW;
            float scaleY = availH / contentH;
            float scale  = Mathf.Min(scaleX, scaleY);

            // Centre the scaled content inside the panel.
            float offsetX = Padding + (availW - contentW * scale) * 0.5f;
            float offsetY = Padding + (availH - contentH * scale) * 0.5f;

            // ── Background ────────────────────────────────────────────────────
            DrawGridBackground(zones);

            // ── Zone buttons ──────────────────────────────────────────────────
            for (int i = 0; i < zones.Count; i++)
            {
                ExpansionZoneData zone = zones[i];
                if (zone == null || string.IsNullOrEmpty(zone.id))
                    continue;

                GameObject zoneObj = new GameObject(zone.id,
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
                    typeof(ExpansionZoneButtonUI));
                zoneObj.transform.SetParent(mapRoot, false);

                // Convert zone's reference-space position to scaled panel space.
                float px = offsetX + (zone.mapPosition.x - minX) * scale;
                float py = offsetY + (zone.mapPosition.y - minY) * scale;
                float sw = zone.mapSize.x * scale;
                float sh = zone.mapSize.y * scale;

                RectTransform rt = zoneObj.GetComponent<RectTransform>();
                rt.anchorMin        = new Vector2(0f, 0f);
                rt.anchorMax        = new Vector2(0f, 0f);
                rt.pivot            = new Vector2(0f, 0f);
                rt.anchoredPosition = new Vector2(px, py);
                rt.sizeDelta        = new Vector2(sw, sh);

                ExpansionZoneButtonUI btn = zoneObj.GetComponent<ExpansionZoneButtonUI>();
                btn.Initialize(zone, OnZoneClicked);
                buttonsByZone[zone.id] = btn;
            }

            Debug.Log(LogPrefix + "Map rebuilt: " + buttonsByZone.Count
                + " zones. Scale=" + scale.ToString("F2")
                + " Panel=" + panelSize.x.ToString("F0") + "×" + panelSize.y.ToString("F0")
                + " Content=" + contentW.ToString("F0") + "×" + contentH.ToString("F0"));
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
