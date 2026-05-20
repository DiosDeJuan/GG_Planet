using TMPro;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Attaches to a <see cref="PlacementObject"/> and renders a small world-space
    /// label above the shelf showing:
    ///   • Product name
    ///   • Current price
    ///   • Purchase probability %
    ///   • Extra purchase probability % (when price is below ideal)
    ///
    /// Setup is automatic: <see cref="ShelfProductInfoLabelBootstrap"/> attaches this
    /// component to every PlacementObject in the scene on Start().
    ///
    /// The label is hidden when the slot is empty and updates whenever:
    ///   • A product is assigned / removed (PlacementObject.onProductChanged).
    ///   • The store price changes (ProductPricingSystem.onPriceChanged).
    /// </summary>
    [RequireComponent(typeof(PlacementObject))]
    public class ShelfProductInfoLabel : MonoBehaviour
    {
        private const string LogPrefix = "[ShelfLabel] ";

        // Height above the placement origin where the label floats.
        private const float LabelYOffset = 0.55f;

        // Size of the world-space canvas.
        private const float CanvasWidth  = 0.55f;
        private const float CanvasHeight = 0.22f;

        // Font sizes (world-space; scale should be tiny like 0.004).
        private const float FontSizeName  = 14f;
        private const float FontSizeInfo  = 11f;

        // ── Internal ─────────────────────────────────────────────────────────────

        private PlacementObject _placement;
        private Canvas          _canvas;
        private TMP_Text        _nameText;
        private TMP_Text        _infoText;
        private bool            _subscribed;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        void Awake()
        {
            _placement = GetComponent<PlacementObject>();
        }

        void Start()
        {
            CreateCanvas();
            Subscribe();
            Refresh();
        }

        void OnEnable()
        {
            if (_canvas != null)
                Subscribe();
        }

        void OnDisable()
        {
            Unsubscribe();
        }

        void OnDestroy()
        {
            Unsubscribe();
        }

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>Forces an immediate refresh of the label text and visibility.</summary>
        public void Refresh()
        {
            if (_placement == null || _canvas == null)
                return;

            ProductScriptableObject product = _placement.product;
            if (product == null)
            {
                _canvas.gameObject.SetActive(false);
                return;
            }

            _canvas.gameObject.SetActive(true);

            // Name.
            if (_nameText != null)
                _nameText.text = product.title ?? product.id;

            // Price + probability.
            if (_infoText != null)
                _infoText.text = BuildInfoLine(product);
        }

        // ── Private helpers ────────────────────────────────────────────────────────

        private static string BuildInfoLine(ProductScriptableObject product)
        {
            string priceStr = StoreDatabase.FromLongToStringMoney(product.storePrice);

            string probStr;
            string extraStr = string.Empty;

            if (ProductPricingSystem.Instance != null)
            {
                float prob  = ProductPricingSystem.Instance.GetPurchaseProbability(product);
                float extra = ProductPricingSystem.Instance.GetExtraPurchaseProbability(product);
                int probPct  = Mathf.RoundToInt(prob  * 100f);
                int extraPct = Mathf.RoundToInt(extra * 100f);
                probStr  = probPct + "%";
                if (extraPct > 0)
                    extraStr = " +" + extraPct + "% extra";
            }
            else
            {
                probStr = "?%";
            }

            return "P: " + priceStr + "  C: " + probStr + extraStr;
        }

        private void Subscribe()
        {
            if (_subscribed || _placement == null)
                return;

            _placement.onProductChanged    += OnProductChanged;
            ProductPricingSystem.onPriceChanged += OnPriceChanged;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed || _placement == null)
                return;

            _placement.onProductChanged    -= OnProductChanged;
            ProductPricingSystem.onPriceChanged -= OnPriceChanged;
            _subscribed = false;
        }

        private void OnProductChanged(ProductScriptableObject _)
        {
            Refresh();
        }

        private void OnPriceChanged(ProductScriptableObject changed)
        {
            if (_placement == null || _placement.product == null)
                return;

            if (changed == null || changed == _placement.product)
                Refresh();
        }

        // ── Canvas creation ────────────────────────────────────────────────────────

        private void CreateCanvas()
        {
            if (_canvas != null)
                return;

            // World-space canvas parented to this shelf slot.
            GameObject canvasGO = new GameObject("ShelfLabel_Canvas",
                typeof(Canvas), typeof(CanvasRenderer));
            canvasGO.transform.SetParent(transform, false);
            canvasGO.transform.localPosition = new Vector3(0f, LabelYOffset, 0f);
            // Billboard: face camera. A simple -Z toward the camera is close enough for shelf labels.
            // We'll let the canvas stay in world-space oriented toward +Z (same as shelf facing).

            _canvas = canvasGO.GetComponent<Canvas>();
            _canvas.renderMode   = RenderMode.WorldSpace;
            _canvas.sortingOrder = 1;

            RectTransform rt = canvasGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(CanvasWidth, CanvasHeight);
            // Scale down from canvas units to world units.
            canvasGO.transform.localScale = Vector3.one * 0.004f;
            // Adjust sizeDelta to match: 0.004 × 128 = 0.512, roughly half a metre wide.
            rt.sizeDelta = new Vector2(128f, 52f);

            // Background panel.
            GameObject bgGO = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(UnityEngine.UI.Image));
            bgGO.transform.SetParent(canvasGO.transform, false);
            RectTransform bgRt = bgGO.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            UnityEngine.UI.Image bgImg = bgGO.GetComponent<UnityEngine.UI.Image>();
            bgImg.color = new Color(0.05f, 0.07f, 0.10f, 0.80f);

            // Name label (top half).
            _nameText = CreateTMP(canvasGO, "NameLabel", FontSizeName, new Vector2(0f, 0.48f), new Vector2(1f, 1f));
            _nameText.fontStyle = TMPro.FontStyles.Bold;
            _nameText.color     = Color.white;
            _nameText.alignment = TMPro.TextAlignmentOptions.MidlineCenter;

            // Info label (bottom half).
            _infoText = CreateTMP(canvasGO, "InfoLabel", FontSizeInfo, new Vector2(0f, 0f), new Vector2(1f, 0.48f));
            _infoText.color     = new Color(0.80f, 0.90f, 0.60f);
            _infoText.alignment = TMPro.TextAlignmentOptions.MidlineCenter;

            // Start hidden until a product is assigned.
            canvasGO.SetActive(false);
        }

        private static TMP_Text CreateTMP(GameObject parent, string childName, float fontSize,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new GameObject(childName,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent.transform, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = new Vector2(2f, 1f);
            rt.offsetMax = new Vector2(-2f, -1f);
            TMP_Text tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.fontSize          = fontSize;
            tmp.overflowMode      = TMPro.TextOverflowModes.Ellipsis;
            tmp.textWrappingMode  = TMPro.TextWrappingModes.NoWrap;
            return tmp;
        }
    }
}
