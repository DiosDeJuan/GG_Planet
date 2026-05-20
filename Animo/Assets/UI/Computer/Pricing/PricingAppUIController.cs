using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// "Precios" tab inside the in-game computer.
    /// Lists every product with its current sell price, ideal price, and purchase
    /// probability.  Price adjustments are routed through ProductPricingSystem which
    /// calls ItemDatabase.UpdateStorePrice() — the same API the asset scanner uses.
    ///
    /// SCENE SETUP:
    ///   Attached at runtime by EntrepreneurTreeUIBootstrap.EnsurePricingTab().
    ///   No Inspector wiring required.
    /// </summary>
    public class PricingAppUIController : MonoBehaviour
    {
        private const string LogPrefix = "[Pricing] ";

        // ── Layout constants ──────────────────────────────────────────────────────

        private const float HeaderHeightFraction = 0.12f;
        private const float SummaryHeightFraction = 0.08f;
        private const float RowHeight = 72f;
        private const float RowSpacing = 4f;

        // ── Color palette (consistent with Orders/Achievements panels) ────────────

        private static readonly Color ColorBackground   = new Color(0.08f, 0.09f, 0.11f, 1.00f);
        private static readonly Color ColorHeader       = new Color(0.11f, 0.14f, 0.18f, 1.00f);
        private static readonly Color ColorSummary      = new Color(0.10f, 0.12f, 0.16f, 1.00f);
        private static readonly Color ColorRowUnlocked  = new Color(0.13f, 0.15f, 0.20f, 0.95f);
        private static readonly Color ColorRowLocked    = new Color(0.10f, 0.11f, 0.13f, 0.80f);
        private static readonly Color ColorBtnNeutral   = new Color(0.20f, 0.25f, 0.35f, 1.00f);
        private static readonly Color ColorBtnReset     = new Color(0.25f, 0.50f, 0.70f, 1.00f);
        private static readonly Color ColorAccent       = new Color(0.30f, 0.80f, 0.55f, 1.00f);
        private static readonly Color ColorLocked       = new Color(0.50f, 0.50f, 0.55f, 1.00f);
        private static readonly Color ColorWarning      = new Color(0.90f, 0.55f, 0.10f, 1.00f);
        private static readonly Color ColorDanger       = new Color(0.75f, 0.15f, 0.10f, 1.00f);
        private static readonly Color ColorGood         = new Color(0.25f, 0.75f, 0.30f, 1.00f);

        // Step sizes in cents ($0.10 = 10 cents, $0.50 = 50 cents)
        private const long StepSmall       = 10L;
        private const long StepLarge       = 50L;
        private const long DeltaReset      = long.MinValue; // sentinel: means "reset to ideal"

        // ── Internal state ────────────────────────────────────────────────────────

        private TMP_Text moneyLabel;
        private TMP_Text statusLabel;

        private readonly List<PricingRow> rows = new List<PricingRow>();
        private bool uiBuilt;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        void Awake()
        {
            ItemDatabase.onStorePriceUpdate      += OnStorePriceUpdate;
            EntrepreneurTreeManager.onProductNodeUnlocked += OnProductNodeUnlocked;
            StoreDatabase.onMoneyUpdate          += OnMoneyUpdate;
            SaveGameSystem.dataLoadEvent         += OnDataLoaded;
        }

        void OnEnable()
        {
            if (!uiBuilt)
                BuildUI();
            RefreshAll();
        }

        void OnDestroy()
        {
            ItemDatabase.onStorePriceUpdate      -= OnStorePriceUpdate;
            EntrepreneurTreeManager.onProductNodeUnlocked -= OnProductNodeUnlocked;
            StoreDatabase.onMoneyUpdate          -= OnMoneyUpdate;
            SaveGameSystem.dataLoadEvent         -= OnDataLoaded;
        }

        // ── UI Construction ───────────────────────────────────────────────────────

        private void BuildUI()
        {
            uiBuilt = true;

            // Panel background
            Image bg = gameObject.AddComponent<Image>();
            bg.color = ColorBackground;

            RectTransform rt = GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // ── Header ─────────────────────────────────────────────────────────
            GameObject headerGO = CreateChild("Header", gameObject);
            SetAnchors(headerGO.GetComponent<RectTransform>(), 0, 1 - HeaderHeightFraction, 1, 1, 0, 0, 0, 0);
            headerGO.AddComponent<Image>().color = ColorHeader;

            AddText(headerGO, "Title", "Precios", 18, FontStyles.Bold, Color.white,
                    0, 0.50f, 1, 1, 8, 0, -4, 0);
            AddText(headerGO, "Subtitle", "Ajusta precios y observa cómo cambia la probabilidad de compra.",
                    10, FontStyles.Normal, new Color(0.70f, 0.75f, 0.80f),
                    0, 0, 1, 0.52f, 8, 2, -4, 0);

            // ── Summary strip ───────────────────────────────────────────────────
            float sTop = 1 - HeaderHeightFraction;
            float sBot = sTop - SummaryHeightFraction;
            GameObject summaryGO = CreateChild("Summary", gameObject);
            SetAnchors(summaryGO.GetComponent<RectTransform>(), 0, sBot, 1, sTop, 0, 0, 0, 0);
            summaryGO.AddComponent<Image>().color = ColorSummary;

            moneyLabel  = AddText(summaryGO, "Money",  "Dinero: --",
                                  11, FontStyles.Normal, ColorAccent,
                                  0, 0.5f, 0.5f, 1, 8, 0, 0, 0);
            statusLabel = AddText(summaryGO, "Status", "",
                                  10, FontStyles.Normal, ColorGood,
                                  0.5f, 0, 1, 1, 4, 2, -4, -2);

            // Legend
            AddText(summaryGO, "Legend",
                    "💚 ideal   🟡 caro   🔴 muy caro   🔵 barato",
                    9, FontStyles.Normal, new Color(0.55f, 0.60f, 0.65f),
                    0, 0, 0.5f, 0.5f, 8, 2, -4, 0);

            // ── Scrollable product list ─────────────────────────────────────────
            GameObject scrollGO = CreateChild("Scroll", gameObject);
            SetAnchors(scrollGO.GetComponent<RectTransform>(), 0, 0, 1, sBot, 0, 0, 0, 0);
            ScrollRect scroll = scrollGO.AddComponent<ScrollRect>();
            scroll.horizontal = false;

            GameObject vpGO = CreateChild("Viewport", scrollGO);
            RectTransform vpRt = vpGO.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero; vpRt.offsetMax = Vector2.zero;
            vpGO.AddComponent<Image>().color = Color.clear;
            vpGO.AddComponent<Mask>().showMaskGraphic = false;

            GameObject contentGO = CreateChild("Content", vpGO);
            RectTransform contentRt = contentGO.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot     = new Vector2(0.5f, 1);
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = RowSpacing;
            vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.childControlWidth  = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vpRt;
            scroll.content  = contentRt;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;

            // ── Build rows ──────────────────────────────────────────────────────
            rows.Clear();
            if (ItemDatabase.Instance != null)
            {
                var raw = ItemDatabase.GetByType(typeof(ProductScriptableObject));
                foreach (var p in raw)
                {
                    ProductScriptableObject product = p as ProductScriptableObject;
                    if (product == null) continue;
                    rows.Add(BuildRow(contentGO, product));
                }
            }
            else
            {
                Debug.LogWarning(LogPrefix + "ItemDatabase not available during UI build.");
            }
        }

        // ── Row construction ──────────────────────────────────────────────────────

        private PricingRow BuildRow(GameObject parent, ProductScriptableObject product)
        {
            PricingRow row = new PricingRow();
            row.product = product;

            GameObject rowGO = CreateChild("Row_" + product.id, parent);
            LayoutElement le = rowGO.AddComponent<LayoutElement>();
            le.preferredHeight = RowHeight;
            row.background = rowGO.AddComponent<Image>();

            // Name (top-left)
            row.nameLabel = AddText(rowGO, "Name", product.title,
                                    12, FontStyles.Bold, Color.white,
                                    0, 0.65f, 0.50f, 1, 8, 0, -4, 0);
            row.nameLabel.alignment = TextAlignmentOptions.BottomLeft;

            // Lock reason / status (bottom-left)
            row.statusLabel = AddText(rowGO, "Status", "",
                                      9, FontStyles.Normal, new Color(0.55f, 0.65f, 0.75f),
                                      0, 0.40f, 0.50f, 0.65f, 8, 2, -4, 0);
            row.statusLabel.alignment = TextAlignmentOptions.TopLeft;

            // Probability (bottom-left-lower)
            row.probLabel = AddText(rowGO, "Prob", "",
                                    9, FontStyles.Normal, ColorGood,
                                    0, 0.10f, 0.50f, 0.40f, 8, 2, -4, 0);
            row.probLabel.alignment = TextAlignmentOptions.TopLeft;

            // Price info (top-right)
            row.priceLabel = AddText(rowGO, "Price", "",
                                     11, FontStyles.Normal, new Color(0.85f, 0.85f, 0.55f),
                                     0.50f, 0.60f, 1, 1, 4, 0, -4, 0);
            row.priceLabel.alignment = TextAlignmentOptions.BottomRight;

            row.pendingPrice = product.storePrice;

            // Adjust buttons row (bottom-right)
            // [-$0.50][-$0.10][+$0.10][+$0.50][$0][OK][IDEAL]
            string[] labels = { "-$0.50", "-$0.10", "+$0.10", "+$0.50", "$0", "OK", "IDEAL" };
            long[]   deltas = { -StepLarge, -StepSmall, StepSmall, StepLarge, 0L, 0L, DeltaReset };
            Color[]  colors = { ColorDanger, ColorWarning, ColorGood, ColorGood, ColorBtnNeutral, ColorAccent, ColorBtnReset };

            float btnW = 0.07f;
            float btnStart = 0.50f;
            for (int b = 0; b < labels.Length; b++)
            {
                int captured = b;
                long capturedDelta = deltas[b];

                GameObject btnGO = CreateChild("Btn_" + b, rowGO);
                RectTransform btnRt = btnGO.GetComponent<RectTransform>();
                float minX = btnStart + b * btnW;
                float maxX = minX + btnW - 0.005f;
                SetAnchors(btnRt, minX, 0.08f, maxX, 0.58f, 2, 2, -2, -2);
                Image btnImg = btnGO.AddComponent<Image>();
                btnImg.color = colors[b];
                Button btn = btnGO.AddComponent<Button>();
                btn.targetGraphic = btnImg;

                ProductScriptableObject capturedProduct = product;
                btn.onClick.AddListener(() =>
                {
                    if (captured == 4)
                        ApplySetZero(row);
                    else if (captured == 5)
                        ConfirmPrice(row);
                    else if (capturedDelta == DeltaReset)
                        ApplyResetPreview(row);
                    else
                        ApplyDelta(row, capturedDelta);
                });

                AddText(btnGO, "L", labels[b], 8, FontStyles.Bold, Color.white);

                if (captured < row.adjButtons.Length)
                    row.adjButtons[captured] = btn;
            }

            return row;
        }

        // ── Button handlers ───────────────────────────────────────────────────────

        private void ApplyDelta(PricingRow row, long deltaCents)
        {
            if (row == null || row.product == null || ProductPricingSystem.Instance == null) return;
            row.pendingPrice += deltaCents;
            if (statusLabel != null)
                statusLabel.text = "Pendiente: " + row.product.title + " -> " + StoreDatabase.FromLongToStringMoney(row.pendingPrice);
            RefreshRow(row);
        }

        private void ApplySetZero(PricingRow row)
        {
            if (row == null || row.product == null) return;
            row.pendingPrice = 0L;
            if (statusLabel != null)
                statusLabel.text = "Precio $0.00: no genera ingresos, pero puede aumentar compra extra.";
            RefreshRow(row);
        }

        private void ApplyResetPreview(PricingRow row)
        {
            if (row == null || row.product == null || ProductPricingSystem.Instance == null) return;
            row.pendingPrice = ProductPricingSystem.Instance.GetIdealPrice(row.product);
            if (statusLabel != null)
                statusLabel.text = "Pendiente: " + row.product.title + " -> precio ideal.";
            RefreshRow(row);
        }

        private void ConfirmPrice(PricingRow row)
        {
            if (row == null || row.product == null || ProductPricingSystem.Instance == null) return;

            if (!ProductPricingSystem.Instance.TrySetCurrentPrice(row.product, row.pendingPrice, out string reason))
            {
                if (statusLabel != null)
                    statusLabel.text = reason;
                RefreshRow(row);
                return;
            }

            if (statusLabel != null)
                statusLabel.text = row.product.title + ": " + StoreDatabase.FromLongToStringMoney(row.pendingPrice) + " confirmado.";
            RefreshRow(row);
        }

        private void ApplyReset(ProductScriptableObject product)
        {
            if (product == null || ProductPricingSystem.Instance == null) return;
            ProductPricingSystem.Instance.ResetProductPrice(product);
            if (statusLabel != null)
                statusLabel.text = product.title + " → precio ideal restaurado.";
        }

        // ── Refresh ───────────────────────────────────────────────────────────────

        private void RefreshAll()
        {
            if (!uiBuilt) return;
            if (moneyLabel != null)
                moneyLabel.text = "Dinero: " + StoreDatabase.GetMoneyString();
            foreach (PricingRow r in rows)
                RefreshRow(r);
        }

        private void RefreshRow(PricingRow row)
        {
            if (row == null || row.product == null) return;

            bool unlocked = IsUnlocked(row.product);

            row.background.color = unlocked ? ColorRowUnlocked : ColorRowLocked;
            row.nameLabel.color  = unlocked ? Color.white : ColorLocked;

            // Buttons
            for (int i = 0; i < row.adjButtons.Length; i++)
            {
                if (row.adjButtons[i] != null)
                    row.adjButtons[i].interactable = unlocked && ProductPricingSystem.Instance != null;
            }

            if (!unlocked)
            {
                row.statusLabel.text  = "🔒 Bloqueado en el Árbol del Emprendedor";
                row.statusLabel.color = ColorLocked;
                row.priceLabel.text   = "--";
                row.probLabel.text    = "";
                return;
            }

            if (ProductPricingSystem.Instance == null) return;

            long ideal   = ProductPricingSystem.Instance.GetIdealPrice(row.product);
            long current = row.product.storePrice;
            long preview = row.pendingPrice;
            long maxP    = ProductPricingSystem.Instance.GetMaxAllowedPrice(row.product);
            float prob   = ProductPricingSystem.Instance.GetPurchaseProbabilityForPrice(row.product, preview);
            float extra  = ProductPricingSystem.Instance.GetExtraPurchaseProbabilityForPrice(row.product, preview);

            // Price label
            row.priceLabel.text = "Actual: " + StoreDatabase.FromLongToStringMoney(current) +
                                  "  Nuevo: " + StoreDatabase.FromLongToStringMoney(preview) +
                                  "  ideal: " + StoreDatabase.FromLongToStringMoney(ideal);

            // Status
            string stateIcon;
            Color stateColor;
            if (preview < 0 || preview > maxP)
            {
                row.statusLabel.text = "Fuera de rango. Permitido: $0.00 a " + StoreDatabase.FromLongToStringMoney(maxP);
                row.statusLabel.color = ColorDanger;
                row.probLabel.text = "Corrige el precio y confirma.";
                row.probLabel.color = ColorDanger;
                return;
            }

            if (preview == 0)
            {
                stateIcon  = "🔵 Gratis";
                stateColor = new Color(0.40f, 0.70f, 1.00f);
            }
            else if (preview < ideal)
            {
                stateIcon  = "🔵 Barato";
                stateColor = new Color(0.40f, 0.70f, 1.00f);
            }
            else if (preview == ideal)
            {
                stateIcon  = "💚 Ideal";
                stateColor = ColorGood;
            }
            else if (preview <= ideal * 2)
            {
                stateIcon  = "🟡 Caro";
                stateColor = ColorWarning;
            }
            else
            {
                stateIcon  = "🔴 Muy caro";
                stateColor = ColorDanger;
            }

            row.statusLabel.text  = stateIcon + "   máx: " + StoreDatabase.FromLongToStringMoney(maxP);
            row.statusLabel.color = stateColor;

            // Probability
            int probPct  = Mathf.RoundToInt(prob  * 100f);
            int extraPct = Mathf.RoundToInt(extra * 100f);
            row.probLabel.text = "Compra: " + probPct + "%   Extra: " + extraPct + "%";
            row.probLabel.color = prob >= 1f ? ColorGood : (prob >= 0.5f ? ColorWarning : ColorDanger);
        }

        // ── Event handlers ────────────────────────────────────────────────────────

        private void OnStorePriceUpdate(ProductScriptableObject product, string _)
        {
            if (!isActiveAndEnabled) return;
            PricingRow row = FindRow(product);
            if (row != null)
            {
                row.pendingPrice = product.storePrice;
                RefreshRow(row);
            }
        }

        private void OnProductNodeUnlocked(NodeData _)
        {
            if (isActiveAndEnabled) RefreshAll();
        }

        private void OnMoneyUpdate(string _, string __)
        {
            if (isActiveAndEnabled && moneyLabel != null)
                moneyLabel.text = "Dinero: " + StoreDatabase.GetMoneyString();
        }

        private void OnDataLoaded()
        {
            if (isActiveAndEnabled) RefreshAll();
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static bool IsUnlocked(ProductScriptableObject product)
        {
            if (EntrepreneurTreeProductUnlockAdapter.Instance != null)
                return EntrepreneurTreeProductUnlockAdapter.Instance.IsProductUnlocked(product);
            return true;
        }

        private PricingRow FindRow(ProductScriptableObject product)
        {
            if (product == null) return null;
            for (int i = 0; i < rows.Count; i++)
                if (rows[i].product == product) return rows[i];
            return null;
        }

        // ── Unity UI helpers ──────────────────────────────────────────────────────

        private static GameObject CreateChild(string childName, GameObject parent)
        {
            GameObject go = new GameObject(childName, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static void SetAnchors(RectTransform rt,
                                        float anchorMinX, float anchorMinY,
                                        float anchorMaxX, float anchorMaxY,
                                        float offsetMinX, float offsetMinY,
                                        float offsetMaxX, float offsetMaxY)
        {
            rt.anchorMin = new Vector2(anchorMinX, anchorMinY);
            rt.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
            rt.offsetMin = new Vector2(offsetMinX, offsetMinY);
            rt.offsetMax = new Vector2(offsetMaxX, offsetMaxY);
        }

        private static TMP_Text AddText(GameObject parent, string childName, string text,
                                         float fontSize, FontStyles style, Color color,
                                         float anchorMinX, float anchorMinY,
                                         float anchorMaxX, float anchorMaxY,
                                         float offsetMinX, float offsetMinY,
                                         float offsetMaxX, float offsetMaxY)
        {
            GameObject go = CreateChild(childName, parent);
            SetAnchors(go.GetComponent<RectTransform>(),
                       anchorMinX, anchorMinY, anchorMaxX, anchorMaxY,
                       offsetMinX, offsetMinY, offsetMaxX, offsetMaxY);
            TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = fontSize; tmp.fontStyle = style;
            tmp.color = color; tmp.overflowMode = TextOverflowModes.Ellipsis;
            return tmp;
        }

        private static TMP_Text AddText(GameObject parent, string childName, string text,
                                         float fontSize, FontStyles style, Color color)
        {
            return AddText(parent, childName, text, fontSize, style, color,
                           0, 0, 1, 1, 4, 4, -4, -4);
        }

        // ── Inner types ───────────────────────────────────────────────────────────

        private class PricingRow
        {
            public ProductScriptableObject product;
            public Image    background;
            public TMP_Text nameLabel;
            public TMP_Text statusLabel;
            public TMP_Text probLabel;
            public TMP_Text priceLabel;
            public long pendingPrice;
            public Button[] adjButtons = new Button[7];
        }
    }
}
