using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// "Compra / Pedidos" panel inside the in-game computer.
    /// Lists all ProductScriptableObject assets, shows lock status based on the
    /// Entrepreneur Tree, validates money before each purchase and delegates the
    /// actual delivery to the existing DeliverySystem.Purchase() path.
    ///
    /// SCENE SETUP:
    ///   This component is attached to the "Compra" panel at runtime by
    ///   EntrepreneurTreeUIBootstrap.EnsureOrdersTab().  No Inspector wiring needed.
    /// </summary>
    public class OrdersAppUIController : MonoBehaviour
    {
        private const string LogPrefix = "[Orders] ";

        // ── Layout constants ──────────────────────────────────────────────────────

        private const float HeaderHeightFraction = 0.12f;
        private const float SummaryHeightFraction = 0.10f;
        private const float RowHeight = 60f;
        private const float RowSpacing = 4f;

        // ── Color palette (mirrors AchievementsAppUIController style) ─────────────

        private static readonly Color ColorBackground   = new Color(0.08f, 0.09f, 0.11f, 1.00f);
        private static readonly Color ColorHeader       = new Color(0.11f, 0.14f, 0.18f, 1.00f);
        private static readonly Color ColorSummary      = new Color(0.10f, 0.12f, 0.16f, 1.00f);
        private static readonly Color ColorRowUnlocked  = new Color(0.13f, 0.15f, 0.20f, 0.95f);
        private static readonly Color ColorRowLocked    = new Color(0.10f, 0.11f, 0.13f, 0.80f);
        private static readonly Color ColorButtonBuy    = new Color(0.15f, 0.65f, 0.30f, 1.00f);
        private static readonly Color ColorButtonNoMoney= new Color(0.65f, 0.20f, 0.15f, 1.00f);
        private static readonly Color ColorButtonLocked = new Color(0.30f, 0.30f, 0.35f, 1.00f);
        private static readonly Color ColorAccent       = new Color(0.30f, 0.80f, 0.55f, 1.00f);
        private static readonly Color ColorLocked       = new Color(0.50f, 0.50f, 0.55f, 1.00f);

        // ── Internal state ────────────────────────────────────────────────────────

        private TMP_Text moneyLabel;
        private TMP_Text unlockedLabel;
        private TMP_Text statusLabel;

        private readonly List<ProductRow> rows = new List<ProductRow>();
        private bool uiBuilt;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        void Awake()
        {
            StoreDatabase.onMoneyUpdate += OnMoneyUpdate;
            EntrepreneurTreeManager.onProductNodeUnlocked += OnProductNodeUnlocked;
            DeliverySystem.onProductPurchase += OnProductPurchased;
            SaveGameSystem.dataLoadEvent += OnDataLoaded;
        }

        void OnEnable()
        {
            if (!uiBuilt)
                BuildUI();

            RefreshAll();
        }

        void OnDestroy()
        {
            StoreDatabase.onMoneyUpdate -= OnMoneyUpdate;
            EntrepreneurTreeManager.onProductNodeUnlocked -= OnProductNodeUnlocked;
            DeliverySystem.onProductPurchase -= OnProductPurchased;
            SaveGameSystem.dataLoadEvent -= OnDataLoaded;
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
            RectTransform headerRt = headerGO.GetComponent<RectTransform>();
            SetAnchors(headerRt, 0f, 1f - HeaderHeightFraction, 1f, 1f, 0f, 0f, 0f, 0f);
            headerGO.AddComponent<Image>().color = ColorHeader;

            AddText(headerGO, "Title", "Compra / Pedidos", 18, FontStyles.Bold, Color.white,
                    0f, 0.50f, 1f, 1f, 8f, 0f, -4f, 0f);

            AddText(headerGO, "Subtitle",
                    "Compra paquetes de productos desbloqueados para surtir tu supermercado.",
                    10, FontStyles.Normal, new Color(0.70f, 0.75f, 0.80f),
                    0f, 0f, 1f, 0.52f, 8f, 2f, -4f, 0f);

            // ── Summary strip ───────────────────────────────────────────────────
            float summaryTop = 1f - HeaderHeightFraction;
            float summaryBot = summaryTop - SummaryHeightFraction;
            GameObject summaryGO = CreateChild("Summary", gameObject);
            RectTransform summaryRt = summaryGO.GetComponent<RectTransform>();
            SetAnchors(summaryRt, 0f, summaryBot, 1f, summaryTop, 0f, 0f, 0f, 0f);
            summaryGO.AddComponent<Image>().color = ColorSummary;

            moneyLabel = AddText(summaryGO, "Money", "Dinero: --",
                                 11, FontStyles.Normal, ColorAccent,
                                 0f, 0.5f, 0.5f, 1f, 8f, 0f, 0f, 0f);

            unlockedLabel = AddText(summaryGO, "Unlocked", "Desbloqueados: --",
                                    11, FontStyles.Normal, new Color(0.70f, 0.75f, 0.80f),
                                    0f, 0f, 0.5f, 0.5f, 8f, 2f, 0f, 0f);

            statusLabel = AddText(summaryGO, "Status", "",
                                  11, FontStyles.Normal, new Color(0.70f, 0.85f, 0.55f),
                                  0.5f, 0.5f, 1f, 1f, 4f, 0f, -4f, 0f);

            AddText(summaryGO, "StatusBottom", "→ Los productos desbloqueados se pueden comprar directamente.",
                    9, FontStyles.Normal, new Color(0.55f, 0.60f, 0.65f),
                    0.5f, 0f, 1f, 0.5f, 4f, 2f, -4f, 0f);

            // ── Scrollable product list ─────────────────────────────────────────
            float listTop = summaryBot;
            GameObject scrollGO = CreateChild("Scroll", gameObject);
            RectTransform scrollRt = scrollGO.GetComponent<RectTransform>();
            SetAnchors(scrollRt, 0f, 0f, 1f, listTop, 0f, 0f, 0f, 0f);

            ScrollRect scroll = scrollGO.AddComponent<ScrollRect>();
            scroll.horizontal = false;

            // Viewport
            GameObject vpGO = CreateChild("Viewport", scrollGO);
            RectTransform vpRt = vpGO.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero;
            vpRt.offsetMax = Vector2.zero;
            vpGO.AddComponent<Image>().color = Color.clear;
            Mask mask = vpGO.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Content
            GameObject contentGO = CreateChild("Content", vpGO);
            RectTransform contentRt = contentGO.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot     = new Vector2(0.5f, 1f);
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

            // ── Build rows for every product ────────────────────────────────────
            rows.Clear();
            if (ItemDatabase.Instance != null)
            {
                List<PurchasableScriptableObject> raw = ItemDatabase.GetByType(typeof(ProductScriptableObject));
                foreach (PurchasableScriptableObject p in raw)
                {
                    ProductScriptableObject product = p as ProductScriptableObject;
                    if (product == null) continue;
                    ProductRow row = BuildProductRow(contentGO, product);
                    rows.Add(row);
                }
            }
            else
            {
                Debug.LogWarning(LogPrefix + "ItemDatabase not available during UI build. Rows will be empty.");
            }
        }

        // ── Row construction ──────────────────────────────────────────────────────

        private ProductRow BuildProductRow(GameObject parent, ProductScriptableObject product)
        {
            ProductRow row = new ProductRow();
            row.product = product;

            GameObject rowGO = CreateChild("Row_" + product.id, parent);
            LayoutElement le = rowGO.AddComponent<LayoutElement>();
            le.preferredHeight = RowHeight;
            row.background = rowGO.AddComponent<Image>();

            // ── Left section: name + category + lock reason ───────────────────
            row.nameLabel = AddText(rowGO, "Name", product.title, 13, FontStyles.Bold, Color.white,
                                    0f, 0.55f, 0.55f, 1f, 8f, 0f, -4f, 0f);
            row.nameLabel.alignment = TextAlignmentOptions.BottomLeft;

            // Category or lock reason
            string categoryText = GetCategoryText(product);
            row.categoryLabel = AddText(rowGO, "Category", categoryText, 9, FontStyles.Normal,
                                        new Color(0.55f, 0.65f, 0.75f),
                                        0f, 0.15f, 0.55f, 0.55f, 8f, 2f, -4f, 2f);
            row.categoryLabel.alignment = TextAlignmentOptions.TopLeft;

            // ── Right section: price + button ─────────────────────────────────
            long totalCost = product.buyPrice * product.packageCount;
            row.priceLabel = AddText(rowGO, "Price",
                                     StoreDatabase.FromLongToStringMoney(totalCost) + " / " + product.packageCount + " uds.",
                                     11, FontStyles.Normal, new Color(0.85f, 0.85f, 0.55f),
                                     0.55f, 0.55f, 0.85f, 1f, 4f, 0f, -4f, 0f);
            row.priceLabel.alignment = TextAlignmentOptions.BottomRight;

            // Button
            GameObject btnGO = CreateChild("BtnBuy", rowGO);
            RectTransform btnRt = btnGO.GetComponent<RectTransform>();
            SetAnchors(btnRt, 0.85f, 0.15f, 1f, 0.85f, 4f, 4f, -4f, -4f);
            row.buttonImage = btnGO.AddComponent<Image>();
            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = row.buttonImage;

            row.buttonLabel = AddText(btnGO, "BtnText", "Comprar", 11, FontStyles.Bold, Color.white,
                                      0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);
            row.buttonLabel.alignment = TextAlignmentOptions.Center;

            // Capture local copy for lambda
            ProductScriptableObject capturedProduct = product;
            btn.onClick.AddListener(() => OnBuyClicked(capturedProduct));
            row.button = btn;

            return row;
        }

        // ── Refresh ───────────────────────────────────────────────────────────────

        private void RefreshAll()
        {
            if (!uiBuilt) return;

            // Summary
            if (moneyLabel != null)
                moneyLabel.text = "Dinero: " + StoreDatabase.GetMoneyString();

            int total = rows.Count;
            int unlocked = 0;
            foreach (ProductRow r in rows)
                if (IsUnlocked(r.product)) unlocked++;

            if (unlockedLabel != null)
                unlockedLabel.text = "Desbloqueados: " + unlocked + "/" + total;

            // Rows
            foreach (ProductRow r in rows)
                RefreshRow(r);
        }

        private void RefreshRow(ProductRow row)
        {
            if (row == null || row.product == null) return;

            bool unlocked = IsUnlocked(row.product);
            long totalCost = row.product.buyPrice * row.product.packageCount;
            bool canAfford = StoreDatabase.CanPurchase(totalCost);

            // Background
            row.background.color = unlocked ? ColorRowUnlocked : ColorRowLocked;

            // Name color
            row.nameLabel.color = unlocked ? Color.white : ColorLocked;

            // Category / lock label
            row.categoryLabel.text = unlocked
                ? GetCategoryText(row.product)
                : "🔒 " + GetLockReason(row.product);
            row.categoryLabel.color = unlocked
                ? new Color(0.55f, 0.65f, 0.75f)
                : ColorLocked;

            // Button
            row.button.interactable = unlocked;
            if (!unlocked)
            {
                row.buttonImage.color = ColorButtonLocked;
                row.buttonLabel.text  = "Bloqueado";
            }
            else if (!canAfford)
            {
                row.buttonImage.color = ColorButtonNoMoney;
                long deficit = totalCost - StoreDatabase.Instance.currentMoney;
                row.button.interactable = false;
            }
            else
            {
                row.buttonImage.color = ColorButtonBuy;
                row.buttonLabel.text  = "Comprar";
                row.button.interactable = true;
            }
        }

        // ── Event handlers ────────────────────────────────────────────────────────

        private void OnBuyClicked(ProductScriptableObject product)
        {
            if (product == null) return;

            if (!IsUnlocked(product))
            {
                if (statusLabel != null)
                    statusLabel.text = "Bloqueado — desbloquéalo en el Árbol.";
                Debug.LogWarning(LogPrefix + "Tried to purchase locked product: " + product.title);
                return;
            }

            long totalCost = product.buyPrice * product.packageCount;
            if (!StoreDatabase.CanPurchase(totalCost))
            {
                long deficit = totalCost - StoreDatabase.Instance.currentMoney;
                string msg = "Fondos insuficientes: faltan " + StoreDatabase.FromLongToStringMoney(deficit);
                if (statusLabel != null)
                    statusLabel.text = msg;
                UIGame.Instance?.ShowMessage(msg);
                Debug.Log(LogPrefix + msg);
                return;
            }

            // Delegate to the existing asset purchase pipeline.
            DeliverySystem.Purchase(product);

            string successMsg = "Pedido realizado: " + product.title + " ×" + product.packageCount;
            if (statusLabel != null)
                statusLabel.text = successMsg;
            Debug.Log(LogPrefix + successMsg);

            RefreshAll();
        }

        private void OnMoneyUpdate(string current, string change)
        {
            if (isActiveAndEnabled)
                RefreshAll();
        }

        private void OnProductNodeUnlocked(NodeData node)
        {
            if (isActiveAndEnabled)
                RefreshAll();
        }

        private void OnProductPurchased(ProductScriptableObject product)
        {
            // Notify achievement system.
            AchievementSystem.RegisterProductOrdered(product);
        }

        private void OnDataLoaded()
        {
            if (isActiveAndEnabled)
                RefreshAll();
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static bool IsUnlocked(ProductScriptableObject product)
        {
            if (EntrepreneurTreeProductUnlockAdapter.Instance != null)
                return EntrepreneurTreeProductUnlockAdapter.Instance.IsProductUnlocked(product);
            // Fallback: no adapter → allow purchase (same as original asset behaviour).
            return true;
        }

        private static string GetCategoryText(ProductScriptableObject product)
        {
            if (EntrepreneurTreeProductUnlockAdapter.Instance != null)
            {
                string group = EntrepreneurTreeProductUnlockAdapter.Instance.GetLockedGroupForProduct(product);
                if (string.IsNullOrEmpty(group))
                    return "Disponible   +1 pto. si es primer pedido";
            }

            return "Disponible   +1 pto. si es primer pedido";
        }

        private static string GetLockReason(ProductScriptableObject product)
        {
            if (EntrepreneurTreeProductUnlockAdapter.Instance != null)
            {
                string group = EntrepreneurTreeProductUnlockAdapter.Instance.GetLockedGroupForProduct(product);
                if (!string.IsNullOrEmpty(group))
                    return "Requiere nodo: " + group;
            }

            return "Bloqueado en el Árbol del Emprendedor";
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
            rt.anchorMin  = new Vector2(anchorMinX, anchorMinY);
            rt.anchorMax  = new Vector2(anchorMaxX, anchorMaxY);
            rt.offsetMin  = new Vector2(offsetMinX, offsetMinY);
            rt.offsetMax  = new Vector2(offsetMaxX, offsetMaxY);
        }

        private static TMP_Text AddText(GameObject parent, string childName, string text,
                                         float fontSize, FontStyles style, Color color,
                                         float anchorMinX, float anchorMinY,
                                         float anchorMaxX, float anchorMaxY,
                                         float offsetMinX, float offsetMinY,
                                         float offsetMaxX, float offsetMaxY)
        {
            GameObject go = CreateChild(childName, parent);
            RectTransform rt = go.GetComponent<RectTransform>();
            SetAnchors(rt, anchorMinX, anchorMinY, anchorMaxX, anchorMaxY,
                       offsetMinX, offsetMinY, offsetMaxX, offsetMaxY);

            TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = fontSize;
            tmp.fontStyle = style;
            tmp.color     = color;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return tmp;
        }

        private static TMP_Text AddText(GameObject parent, string childName, string text,
                                         float fontSize, FontStyles style, Color color)
        {
            return AddText(parent, childName, text, fontSize, style, color,
                           0f, 0f, 1f, 1f, 4f, 4f, -4f, -4f);
        }

        // ── Inner types ───────────────────────────────────────────────────────────

        private class ProductRow
        {
            public ProductScriptableObject product;
            public Image     background;
            public TMP_Text  nameLabel;
            public TMP_Text  categoryLabel;
            public TMP_Text  priceLabel;
            public Button    button;
            public Image     buttonImage;
            public TMP_Text  buttonLabel;
        }
    }
}
