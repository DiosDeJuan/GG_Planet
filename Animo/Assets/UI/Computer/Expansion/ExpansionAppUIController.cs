using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Manages the "Expandir / Mapa del Terreno" app panel inside the in-game computer.
    /// Layout:
    ///   Top 10% — header strip: title + money + area summaries
    ///   Left 62% of remaining 90% — map panel with zone grid
    ///   Right 38% of remaining 90% — detail panel with zone info + buy button
    /// </summary>
    public class ExpansionAppUIController : MonoBehaviour
    {
        private const string LogPrefix = "[Expansion] ";

        // ── References ────────────────────────────────────────────────────────
        private UIShopCategoryHelper categoryHelper;
        private RectTransform panelRoot;
        private RectTransform mapRoot;
        private ExpansionMapRenderer mapRenderer;

        // Header
        private TMP_Text moneyLabel;
        private TMP_Text salesAreaLabel;
        private TMP_Text storageAreaLabel;

        // Details
        private TMP_Text zoneNameText;
        private TMP_Text zoneTypeText;
        private TMP_Text zonePriceText;
        private TMP_Text zoneStateText;
        private TMP_Text zoneBenefitText;
        private TMP_Text messageText;
        private Button buyButton;
        private TMP_Text buyButtonLabel;

        // ── State ─────────────────────────────────────────────────────────────
        private string selectedZoneId;
        private bool initialized;
        private bool mapBuilt;
        private bool zonesSubscribed;
        private bool moneySubscribed;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        public void Initialize(UIShopCategoryHelper helper, RectTransform root)
        {
            categoryHelper = helper;
            panelRoot      = root;

            if (initialized)
            {
                RefreshSummary();
                return;
            }

            BuildUI();
            SubscribeAll();
            initialized = true;
            FullRefresh();
            Debug.Log(LogPrefix + "Expansion app initialized. Zones: "
                + (SupermarketExpansionSystem.Instance?.Zones.Count ?? 0));
        }

        void OnEnable()
        {
            if (!initialized)
                return;

            SubscribeAll();
            FullRefresh();
        }

        void OnDisable()
        {
            UnsubscribeAll();
        }

        void OnDestroy()
        {
            UnsubscribeAll();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Called by ExpansionMapRenderer when the player clicks a zone cell.</summary>
        public void SelectZone(string zoneId)
        {
            selectedZoneId = zoneId;
            mapRenderer?.SetSelectedZone(zoneId);

            ExpansionZoneData zone = SupermarketExpansionSystem.Instance != null
                ? SupermarketExpansionSystem.Instance.GetZone(zoneId)
                : null;

            if (zone == null)
            {
                ShowEmptyDetail();
                return;
            }

            ShowZoneDetail(zone);
            Debug.Log(LogPrefix + "Selected zone: " + zone.id + ".");
        }

        // ── UI Construction ───────────────────────────────────────────────────

        private void BuildUI()
        {
            if (panelRoot == null)
                panelRoot = transform as RectTransform;
            if (panelRoot == null)
                return;

            // ── Header strip (top 10%) ──────────────────────────────────────
            RectTransform header = CreatePanel("HeaderStrip", panelRoot,
                new Vector2(0f, 0.90f), new Vector2(1f, 1f),
                new Color(0.08f, 0.09f, 0.12f, 0.98f), 4f, 2f);

            CreateLabel("TitleLabel", header, "[MAPA]  Mapa del Terreno", 20,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 0f), new Vector2(0.30f, 1f));

            moneyLabel = CreateLabel("MoneyLabel", header, "Dinero: —", 17,
                TextAlignmentOptions.Midline,
                new Vector2(0.30f, 0f), new Vector2(0.55f, 1f));

            salesAreaLabel = CreateLabel("SalesAreaLabel", header, "Venta: —", 17,
                TextAlignmentOptions.Midline,
                new Vector2(0.55f, 0f), new Vector2(0.77f, 1f));

            storageAreaLabel = CreateLabel("StorageAreaLabel", header, "Almacén: —", 17,
                TextAlignmentOptions.Midline,
                new Vector2(0.77f, 0f), new Vector2(1f, 1f));

            // ── Map panel (left 62%, rows 0–90%) ────────────────────────────
            mapRoot = CreatePanel("MapPanel", panelRoot,
                new Vector2(0f, 0f), new Vector2(0.62f, 0.89f),
                new Color(0.11f, 0.13f, 0.17f, 0.97f), 8f, 8f);

            // Map legend (tiny strip at top of map)
            RectTransform mapLegend = CreatePanel("MapLegend", mapRoot,
                new Vector2(0f, 0.94f), new Vector2(1f, 1f),
                new Color(0.07f, 0.08f, 0.11f, 0.96f), 2f, 2f);
            CreateLabel("LegendText", mapLegend,
                "V=Venta   A=Almacén   O=Oficina   [OK]Comprado  +Disponible  [NO]Bloqueado",
                10, TextAlignmentOptions.MidlineLeft,
                Vector2.zero, Vector2.one);

            // Actual map content area (below legend)
            RectTransform mapContent = CreatePanel("MapContent", mapRoot,
                new Vector2(0f, 0f), new Vector2(1f, 0.93f),
                new Color(0.10f, 0.12f, 0.16f, 0.94f), 4f, 4f);

            // ── Details panel (right 38%, rows 0–90%) ───────────────────────
            RectTransform details = CreatePanel("DetailsPanel", panelRoot,
                new Vector2(0.62f, 0f), new Vector2(1f, 0.89f),
                new Color(0.07f, 0.08f, 0.12f, 0.97f), 8f, 8f);

            CreateLabel("DetailsTitle", details, "DETALLE DE ZONA", 18,
                TextAlignmentOptions.Center,
                new Vector2(0f, 0.88f), new Vector2(1f, 1f));

            zoneNameText = CreateLabel("ZoneName", details, "Selecciona una zona del mapa.", 17,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 0.74f), new Vector2(1f, 0.87f));

            zoneTypeText = CreateLabel("ZoneType", details, string.Empty, 15,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 0.62f), new Vector2(1f, 0.73f));

            zonePriceText = CreateLabel("ZonePrice", details, string.Empty, 15,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 0.52f), new Vector2(1f, 0.61f));

            zoneStateText = CreateLabel("ZoneState", details, string.Empty, 15,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 0.38f), new Vector2(1f, 0.51f));

            zoneBenefitText = CreateLabel("ZoneBenefit", details, string.Empty, 14,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 0.24f), new Vector2(1f, 0.37f));

            messageText = CreateLabel("MessageText", details, string.Empty, 14,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 0.13f), new Vector2(1f, 0.23f));
            messageText.color = new Color(1f, 0.85f, 0.25f);

            // Buy button
            buyButton = CreateButton("BuyButton", details,
                new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.12f),
                "Comprar Expansión", new Color(0.87f, 0.26f, 0.56f, 1f));

            // Attach map renderer to the content area (not the whole mapRoot so legend is unaffected)
            mapRenderer = mapContent.GetComponent<ExpansionMapRenderer>();
            if (mapRenderer == null)
                mapRenderer = mapContent.gameObject.AddComponent<ExpansionMapRenderer>();
            mapRenderer.Initialize(mapContent, this);

            ShowEmptyDetail();
        }

        // ── Data / Refresh ────────────────────────────────────────────────────

        private void FullRefresh()
        {
            RefreshSummary();
            if (!mapBuilt)
            {
                if (SupermarketExpansionSystem.Instance != null)
                    mapRenderer?.Rebuild(SupermarketExpansionSystem.Instance.Zones);
                mapBuilt = true;
            }
            else
            {
                if (SupermarketExpansionSystem.Instance != null)
                    mapRenderer?.Refresh(SupermarketExpansionSystem.Instance.Zones);
            }

            if (!string.IsNullOrEmpty(selectedZoneId))
            {
                mapRenderer?.SetSelectedZone(selectedZoneId);
                SelectZone(selectedZoneId);
            }
            else
            {
                ShowEmptyDetail();
            }
        }

        private void RefreshSummary()
        {
            if (moneyLabel != null)
            {
                string money = StoreDatabase.Instance != null
                    ? StoreDatabase.FromLongToStringMoney(StoreDatabase.Instance.currentMoney)
                    : "—";
                moneyLabel.text = "Dinero: " + money;
            }

            if (salesAreaLabel != null && SupermarketExpansionSystem.Instance != null)
            {
                float mult = SupermarketExpansionSystem.Instance.GetCustomerCapacityMultiplier();
                string bonusStr = FormatBonusPct(mult, "% clientes");
                salesAreaLabel.text = "Venta: "
                    + SupermarketExpansionSystem.Instance.GetPurchasedSalesAreaM2() + " m²"
                    + bonusStr;
            }

            if (storageAreaLabel != null && SupermarketExpansionSystem.Instance != null)
            {
                float mult = SupermarketExpansionSystem.Instance.GetStorageCapacityMultiplier();
                string bonusStr = FormatBonusPct(mult, "% cap.");
                storageAreaLabel.text = "Almacén: "
                    + SupermarketExpansionSystem.Instance.GetPurchasedStorageAreaM2() + " m²"
                    + bonusStr;
            }
        }

        private static string FormatBonusPct(float multiplier, string suffix)
        {
            int bonusPct = Mathf.RoundToInt((multiplier - 1f) * 100f);
            return bonusPct > 0 ? " +" + bonusPct + suffix : string.Empty;
        }

        private void ShowEmptyDetail()
        {
            SetDetailText("Selecciona una zona del mapa.",
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
            if (buyButton != null)
                buyButton.interactable = false;
        }

        private void ShowZoneDetail(ExpansionZoneData zone)
        {
            string typeLine  = "Tipo: " + GetTypeLabel(zone.type)
                             + "  |  Tamaño: " + zone.sizeSquareMeters + " m²";
            string priceLine = zone.price > 0
                ? "Precio: " + StoreDatabase.FromLongToStringMoney(zone.price)
                : "Precio: Gratis (zona inicial)";
            string stateLine = "Estado: " + GetStateLabel(zone.state);
            string benefit   = !string.IsNullOrEmpty(zone.benefit) ? zone.benefit : zone.description;

            string msg = BuildStateMessage(zone);

            SetDetailText(zone.displayName, typeLine, priceLine, stateLine, benefit, msg);

            bool canBuy = zone.state == ExpansionZoneState.Available;
            if (buyButton != null)
            {
                buyButton.interactable = canBuy;
                if (buyButtonLabel != null)
                    buyButtonLabel.text = canBuy ? "Comprar Expansión" : "No disponible";
            }
        }

        private string BuildStateMessage(ExpansionZoneData zone)
        {
            if (zone.state == ExpansionZoneState.Purchased)
                return "Esta zona ya fue comprada.";

            if (zone.state == ExpansionZoneState.Blocked)
            {
                string reason = !string.IsNullOrEmpty(zone.blockedReason)
                    ? zone.blockedReason
                    : "Compra primero una zona vecina.";
                return "Bloqueada: " + reason;
            }

            // Available – check funds
            if (StoreDatabase.Instance != null && !StoreDatabase.CanPurchase(zone.price))
            {
                long missing = zone.price - StoreDatabase.Instance.currentMoney;
                return "Fondos insuficientes. Faltan "
                    + StoreDatabase.FromLongToStringMoney(missing) + ".";
            }

            // Available and affordable – show confirmation prompt
            return "¿Comprar " + zone.displayName + " por "
                + StoreDatabase.FromLongToStringMoney(zone.price) + "?\n"
                + "Pulsa 'Comprar' para confirmar.";
        }

        private void SetDetailText(string name, string type, string price,
            string state, string benefit, string message)
        {
            if (zoneNameText    != null) zoneNameText.text    = name;
            if (zoneTypeText    != null) zoneTypeText.text    = type;
            if (zonePriceText   != null) zonePriceText.text   = price;
            if (zoneStateText   != null) zoneStateText.text   = state;
            if (zoneBenefitText != null) zoneBenefitText.text = benefit;
            if (messageText     != null) messageText.text     = message;
        }

        // ── Buy button ────────────────────────────────────────────────────────

        private void OnBuySelectedZone()
        {
            if (string.IsNullOrEmpty(selectedZoneId)
                || SupermarketExpansionSystem.Instance == null)
                return;

            long missingFunds;
            string reason;
            bool success = SupermarketExpansionSystem.Instance
                .TryPurchaseZone(selectedZoneId, out missingFunds, out reason);

            if (success)
            {
                if (messageText != null)
                    messageText.text = "¡Expansión comprada exitosamente!";

                // Refresh map incrementally (zone states may have changed due to adjacency).
                mapRenderer?.Refresh(SupermarketExpansionSystem.Instance.Zones);
                RefreshSummary();

                // Re-show the detail for the purchased zone.
                SelectZone(selectedZoneId);
                return;
            }

            string errorMsg = missingFunds > 0
                ? "Fondos insuficientes. Faltan "
                    + StoreDatabase.FromLongToStringMoney(missingFunds) + "."
                : reason;

            if (messageText != null)
                messageText.text = errorMsg;
        }

        // ── Event subscriptions ───────────────────────────────────────────────

        private void SubscribeAll()
        {
            if (!zonesSubscribed)
            {
                SupermarketExpansionSystem.onZonesChanged += OnZonesChanged;
                SupermarketExpansionSystem.onZonesReset   += OnZonesReset;
                zonesSubscribed = true;
            }

            if (!moneySubscribed)
            {
                StoreDatabase.onMoneyUpdate += OnMoneyUpdate;
                moneySubscribed = true;
            }
        }

        private void UnsubscribeAll()
        {
            if (zonesSubscribed)
            {
                SupermarketExpansionSystem.onZonesChanged -= OnZonesChanged;
                SupermarketExpansionSystem.onZonesReset   -= OnZonesReset;
                zonesSubscribed = false;
            }

            if (moneySubscribed)
            {
                StoreDatabase.onMoneyUpdate -= OnMoneyUpdate;
                moneySubscribed = false;
            }
        }

        private void OnZonesChanged()
        {
            if (this == null) return;
            if (!isActiveAndEnabled)
                return;

            RefreshSummary();
            if (SupermarketExpansionSystem.Instance != null)
                mapRenderer?.Refresh(SupermarketExpansionSystem.Instance.Zones);
            if (!string.IsNullOrEmpty(selectedZoneId))
                SelectZone(selectedZoneId);
        }

        private void OnZonesReset()
        {
            if (this == null) return;
            // Full data reset (load from file): force map rebuild on next show.
            mapBuilt = false;
            selectedZoneId = null;
            if (isActiveAndEnabled)
                FullRefresh();
        }

        private void OnMoneyUpdate(string current, string change)
        {
            if (this == null) return;
            if (!isActiveAndEnabled)
                return;

            RefreshSummary();
            // Refresh buy-button affordability if a zone is selected.
            if (!string.IsNullOrEmpty(selectedZoneId)
                && SupermarketExpansionSystem.Instance != null)
            {
                ExpansionZoneData zone = SupermarketExpansionSystem.Instance.GetZone(selectedZoneId);
                if (zone != null && zone.state == ExpansionZoneState.Available)
                    ShowZoneDetail(zone);
            }
        }

        // ── UI factories ──────────────────────────────────────────────────────

        private static RectTransform CreatePanel(string name, RectTransform parent,
            Vector2 anchorMin, Vector2 anchorMax, Color color,
            float padX = 6f, float padY = 6f)
        {
            GameObject go = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = new Vector2(padX, padY);
            rt.offsetMax = new Vector2(-padX, -padY);
            go.GetComponent<Image>().color = color;
            return rt;
        }

        private static TMP_Text CreateLabel(string name, Transform parent, string text,
            float size, TextAlignmentOptions align,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = new Vector2(10f, 6f);
            rt.offsetMax = new Vector2(-10f, -6f);

            TMP_Text label = go.GetComponent<TextMeshProUGUI>();
            label.text      = text;
            label.fontSize  = size;
            label.alignment = align;
            label.color     = Color.white;
            label.textWrappingMode = TextWrappingModes.Normal;
            return label;
        }

        private Button CreateButton(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax,
            string labelText, Color color)
        {
            GameObject go = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = go.GetComponent<Image>();
            img.color  = color;

            Button btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(OnBuySelectedZone);

            buyButtonLabel = CreateLabel("BuyLabel", go.transform, labelText, 17,
                TextAlignmentOptions.Center, Vector2.zero, Vector2.one);

            return btn;
        }

        // ── String helpers ────────────────────────────────────────────────────

        private static string GetTypeLabel(ExpansionZoneType type)
        {
            switch (type)
            {
                case ExpansionZoneType.Sales:   return "Venta";
                case ExpansionZoneType.Storage: return "Almacenamiento";
                default:                        return "Oficina";
            }
        }

        private static string GetStateLabel(ExpansionZoneState state)
        {
            switch (state)
            {
                case ExpansionZoneState.Purchased: return "Comprada [OK]";
                case ExpansionZoneState.Available: return "Disponible";
                default:                           return "Bloqueada [NO]";
            }
        }
    }
}
