using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    public class ExpansionAppUIController : MonoBehaviour
    {
        private const string LogPrefix = "[ExpansionApp] ";

        private UIShopCategoryHelper categoryHelper;
        private RectTransform panelRoot;
        private RectTransform mapRoot;
        private ExpansionMapRenderer mapRenderer;
        private TMP_Text detailsText;
        private TMP_Text messageText;
        private Button buyButton;
        private string selectedZoneId;
        private bool initialized;

        public void Initialize(UIShopCategoryHelper helper, RectTransform root)
        {
            categoryHelper = helper;
            panelRoot = root;
            if (initialized)
                return;

            BuildUI();
            initialized = true;
            Refresh();
            Debug.Log(LogPrefix + "Loaded " + SupermarketExpansionSystem.Instance?.Zones.Count + " zones.");
        }

        void OnEnable()
        {
            Refresh();
        }

        public void SelectZone(string zoneId)
        {
            selectedZoneId = zoneId;
            ExpansionZoneData zone = SupermarketExpansionSystem.Instance != null ? SupermarketExpansionSystem.Instance.GetZone(zoneId) : null;
            if (zone == null || detailsText == null)
                return;

            detailsText.text =
                "Nombre: " + zone.displayName + "\n" +
                "Tipo: " + GetTypeLabel(zone.type) + "\n" +
                "Tamaño: " + zone.sizeSquareMeters + " m²\n" +
                "Precio: " + StoreDatabase.FromLongToStringMoney(zone.price) + "\n" +
                "Estado: " + GetStateLabel(zone.state) + "\n" +
                "Descripción: " + zone.description;

            if (zone.state == ExpansionZoneState.Blocked && !string.IsNullOrEmpty(zone.blockedReason))
                detailsText.text += "\nMotivo: " + zone.blockedReason;

            buyButton.interactable = zone.state == ExpansionZoneState.Available;
            messageText.text = string.Empty;
            Debug.Log(LogPrefix + "Selected zone: " + zone.id + ".");
        }

        private void BuildUI()
        {
            if (panelRoot == null)
                panelRoot = transform as RectTransform;
            if (panelRoot == null)
                return;

            mapRoot = CreatePanel("MapPanel", panelRoot, new Vector2(0f, 0f), new Vector2(0.66f, 1f), new Color(0.12f, 0.14f, 0.18f, 0.96f));
            RectTransform detailsPanel = CreatePanel("DetailsPanel", panelRoot, new Vector2(0.66f, 0f), new Vector2(1f, 1f), new Color(0.08f, 0.10f, 0.14f, 0.96f));

            CreateLabel("Title", detailsPanel, "EXPANDIR", 30, TextAlignmentOptions.Center, new Vector2(0f, 0.86f), new Vector2(1f, 1f));
            detailsText = CreateLabel("Details", detailsPanel, "Selecciona una zona.", 19, TextAlignmentOptions.TopLeft, new Vector2(0f, 0.28f), new Vector2(1f, 0.85f));
            messageText = CreateLabel("Message", detailsPanel, string.Empty, 18, TextAlignmentOptions.BottomLeft, new Vector2(0f, 0.16f), new Vector2(1f, 0.28f));

            GameObject buyObj = new GameObject("BuyButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buyObj.transform.SetParent(detailsPanel, false);
            RectTransform buyRT = buyObj.GetComponent<RectTransform>();
            buyRT.anchorMin = new Vector2(0.08f, 0.04f);
            buyRT.anchorMax = new Vector2(0.92f, 0.13f);
            buyRT.offsetMin = Vector2.zero;
            buyRT.offsetMax = Vector2.zero;
            Image buyImage = buyObj.GetComponent<Image>();
            buyImage.color = new Color(0.87f, 0.26f, 0.56f, 1f);
            buyButton = buyObj.GetComponent<Button>();
            buyButton.targetGraphic = buyImage;
            buyButton.onClick.AddListener(OnBuySelectedZone);
            CreateLabel("BuyText", buyRT, "Comprar", 20, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);

            mapRenderer = panelRoot.GetComponent<ExpansionMapRenderer>();
            if (mapRenderer == null)
                mapRenderer = panelRoot.gameObject.AddComponent<ExpansionMapRenderer>();
            mapRenderer.Initialize(mapRoot, this);
        }

        private void Refresh()
        {
            if (!initialized || SupermarketExpansionSystem.Instance == null)
                return;

            mapRenderer.Rebuild(SupermarketExpansionSystem.Instance.Zones);
            buyButton.interactable = false;
            if (string.IsNullOrEmpty(selectedZoneId))
                detailsText.text = "Selecciona una zona.";
            else
                SelectZone(selectedZoneId);
        }

        private void OnBuySelectedZone()
        {
            if (string.IsNullOrEmpty(selectedZoneId) || SupermarketExpansionSystem.Instance == null)
                return;

            if (SupermarketExpansionSystem.Instance.TryPurchaseZone(selectedZoneId, out long missingFunds, out string reason))
            {
                messageText.text = "Compra realizada.";
                mapRenderer.Refresh(SupermarketExpansionSystem.Instance.Zones);
                SelectZone(selectedZoneId);
                return;
            }

            if (missingFunds > 0)
                messageText.text = "Fondos insuficientes. Faltan " + StoreDatabase.FromLongToStringMoney(missingFunds) + ".";
            else
                messageText.text = reason;
        }

        private static RectTransform CreatePanel(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = new Vector2(8f, 8f);
            rt.offsetMax = new Vector2(-8f, -8f);
            go.GetComponent<Image>().color = color;
            return rt;
        }

        private static TMP_Text CreateLabel(string name, Transform parent, string text, float size, TextAlignmentOptions align, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = new Vector2(12f, 12f);
            rt.offsetMax = new Vector2(-12f, -12f);

            TMP_Text label = go.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.alignment = align;
            label.enableWordWrapping = true;
            label.color = Color.white;
            return label;
        }

        private static string GetTypeLabel(ExpansionZoneType type)
        {
            switch (type)
            {
                case ExpansionZoneType.Sales: return "Venta";
                case ExpansionZoneType.Storage: return "Almacenamiento";
                default: return "Oficina";
            }
        }

        private static string GetStateLabel(ExpansionZoneState state)
        {
            switch (state)
            {
                case ExpansionZoneState.Purchased: return "Comprada";
                case ExpansionZoneState.Available: return "Disponible";
                default: return "Bloqueada";
            }
        }
    }
}
