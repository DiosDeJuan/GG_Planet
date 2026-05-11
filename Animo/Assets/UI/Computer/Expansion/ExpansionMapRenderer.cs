using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    public class ExpansionMapRenderer : MonoBehaviour
    {
        private const string LogPrefix = "[ExpansionApp] ";
        private readonly Dictionary<string, ExpansionZoneButtonUI> buttonsByZone = new Dictionary<string, ExpansionZoneButtonUI>();

        [SerializeField] private RectTransform mapRoot;
        [SerializeField] private ExpansionAppUIController appController;

        public void Initialize(RectTransform root, ExpansionAppUIController controller)
        {
            mapRoot = root;
            appController = controller;
        }

        public void Rebuild(IReadOnlyList<ExpansionZoneData> zones)
        {
            if (mapRoot == null || zones == null)
                return;

            for (int i = mapRoot.childCount - 1; i >= 0; i--)
                Destroy(mapRoot.GetChild(i).gameObject);

            buttonsByZone.Clear();
            for (int i = 0; i < zones.Count; i++)
            {
                ExpansionZoneData zone = zones[i];
                if (zone == null)
                    continue;

                GameObject zoneObj = new GameObject(zone.id, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ExpansionZoneButtonUI), typeof(Button));
                zoneObj.transform.SetParent(mapRoot, false);
                RectTransform rt = zoneObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 0f);
                rt.pivot = new Vector2(0f, 0f);
                rt.anchoredPosition = zone.mapPosition;
                rt.sizeDelta = zone.mapSize;

                ExpansionZoneButtonUI zoneButton = zoneObj.GetComponent<ExpansionZoneButtonUI>();
                zoneButton.Initialize(zone, OnZoneSelected);
                buttonsByZone[zone.id] = zoneButton;
            }

            Debug.Log(LogPrefix + "Map rebuild complete.");
        }

        public void Refresh(IReadOnlyList<ExpansionZoneData> zones)
        {
            if (zones == null)
                return;

            for (int i = 0; i < zones.Count; i++)
            {
                ExpansionZoneData zone = zones[i];
                if (zone == null)
                    continue;
                if (buttonsByZone.TryGetValue(zone.id, out ExpansionZoneButtonUI button))
                    button.Refresh(zone);
            }

            Debug.Log(LogPrefix + "Map state refresh complete.");
        }

        private void OnZoneSelected(string zoneId)
        {
            appController?.SelectZone(zoneId);
        }
    }
}
