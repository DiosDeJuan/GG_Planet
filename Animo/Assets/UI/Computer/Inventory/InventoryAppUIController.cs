using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// "INVENTARIO" computer-app panel.
    ///
    /// Built entirely in code at runtime — no prefab required.
    /// Shows per-product stock (boxes + shelves), required furniture type, and current status.
    ///
    /// Rules:
    ///   - No new Canvas.
    ///   - No floating UI outside the computer.
    ///   - Subscribes to ProductInventorySystem.onInventoryChanged and StoreDatabase.onMoneyUpdate.
    ///   - Listeners removed in OnDisable / OnDestroy.
    /// </summary>
    public class InventoryAppUIController : MonoBehaviour
    {
        private const string LogPrefix = "[Inventory] ";

        // ── Colours ─────────────────────────────────────────────────────────────
        private static readonly Color ColBg       = new Color(0.07f, 0.08f, 0.11f, 0.96f);
        private static readonly Color ColHeaderBg = new Color(0.10f, 0.12f, 0.16f, 1.00f);
        private static readonly Color ColRowA     = new Color(0.09f, 0.11f, 0.14f, 1.00f);
        private static readonly Color ColRowB     = new Color(0.07f, 0.09f, 0.12f, 1.00f);
        private static readonly Color ColGood     = new Color(0.30f, 0.85f, 0.30f, 1.00f);
        private static readonly Color ColWarn     = new Color(1.00f, 0.75f, 0.10f, 1.00f);
        private static readonly Color ColBad      = new Color(1.00f, 0.35f, 0.35f, 1.00f);
        private static readonly Color ColPending  = new Color(0.50f, 0.70f, 1.00f, 1.00f);
        private static readonly Color ColLocked   = new Color(0.50f, 0.50f, 0.55f, 1.00f);

        // ── Runtime row handles ─────────────────────────────────────────────────
        private readonly List<ProductRow> _rows = new List<ProductRow>();

        private struct ProductRow
        {
            public ProductScriptableObject Product;
            public TMP_Text BoxesLabel;
            public TMP_Text ShelfLabel;
            public TMP_Text StatusLabel;
        }

        // ── Notification for full-stock tracking ───────────────────────────────
        private bool _wasFullStockLastCheck;

        // ── Unity lifecycle ────────────────────────────────────────────────────
        private void Awake()
        {
            BuildUI();
        }

        private void OnEnable()
        {
            ProductInventorySystem.onInventoryChanged += Refresh;
            StoreDatabase.onMoneyUpdate               += OnMoneyUpdate;
            Refresh();
        }

        private void OnDisable()
        {
            ProductInventorySystem.onInventoryChanged -= Refresh;
            StoreDatabase.onMoneyUpdate               -= OnMoneyUpdate;
        }

        private void OnDestroy()
        {
            ProductInventorySystem.onInventoryChanged -= Refresh;
            StoreDatabase.onMoneyUpdate               -= OnMoneyUpdate;
        }

        // ── UI construction ────────────────────────────────────────────────────
        private void BuildUI()
        {
            RectTransform root = GetComponent<RectTransform>();
            if (root == null)
                return;

            // ── Header strip ────────────────────────────────────────────────────
            GameObject headerObj = new GameObject("Header",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            headerObj.transform.SetParent(transform, false);
            RectTransform headerRT = headerObj.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0f, 0.88f);
            headerRT.anchorMax = new Vector2(1f, 1.00f);
            headerRT.offsetMin = Vector2.zero;
            headerRT.offsetMax = Vector2.zero;
            headerObj.GetComponent<Image>().color = ColHeaderBg;

            AddLabel(headerObj.transform,
                "INVENTARIO",
                new Vector2(0f,  0.55f),
                new Vector2(1f,  1.00f),
                16, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, Color.white);

            AddLabel(headerObj.transform,
                "Consulta stock, almacenamiento y productos que necesitan surtido.",
                new Vector2(0f,  0.00f),
                new Vector2(1f,  0.55f),
                10, FontStyles.Normal, TextAlignmentOptions.MidlineLeft,
                new Color(0.70f, 0.72f, 0.80f, 1f));

            // ── Column header row ────────────────────────────────────────────────
            GameObject colHeader = new GameObject("ColHeader",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            colHeader.transform.SetParent(transform, false);
            RectTransform colHeaderRT = colHeader.GetComponent<RectTransform>();
            colHeaderRT.anchorMin = new Vector2(0f, 0.82f);
            colHeaderRT.anchorMax = new Vector2(1f, 0.88f);
            colHeaderRT.offsetMin = Vector2.zero;
            colHeaderRT.offsetMax = Vector2.zero;
            colHeader.GetComponent<Image>().color = ColHeaderBg;

            AddLabel(colHeader.transform, "Producto",         new Vector2(0.00f, 0f), new Vector2(0.28f, 1f), 9, FontStyles.Bold, TextAlignmentOptions.MidlineLeft,   Color.white);
            AddLabel(colHeader.transform, "Tipo mueble",      new Vector2(0.28f, 0f), new Vector2(0.50f, 1f), 9, FontStyles.Bold, TextAlignmentOptions.Midline, Color.white);
            AddLabel(colHeader.transform, "Cajas",            new Vector2(0.50f, 0f), new Vector2(0.65f, 1f), 9, FontStyles.Bold, TextAlignmentOptions.Midline, Color.white);
            AddLabel(colHeader.transform, "Estante",          new Vector2(0.65f, 0f), new Vector2(0.80f, 1f), 9, FontStyles.Bold, TextAlignmentOptions.Midline, Color.white);
            AddLabel(colHeader.transform, "Estado",           new Vector2(0.80f, 0f), new Vector2(1.00f, 1f), 9, FontStyles.Bold, TextAlignmentOptions.Midline, Color.white);

            // ── Scroll container for product rows ────────────────────────────────
            GameObject scrollArea = new GameObject("ScrollArea",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            scrollArea.transform.SetParent(transform, false);
            RectTransform scrollAreaRT = scrollArea.GetComponent<RectTransform>();
            scrollAreaRT.anchorMin = new Vector2(0f, 0.00f);
            scrollAreaRT.anchorMax = new Vector2(1f, 0.82f);
            scrollAreaRT.offsetMin = Vector2.zero;
            scrollAreaRT.offsetMax = Vector2.zero;
            scrollArea.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

            // Build rows for each product
            List<PurchasableScriptableObject> rawProducts =
                ItemDatabase.GetByType(typeof(ProductScriptableObject));
            ProductScriptableObject[] products =
                new ProductScriptableObject[rawProducts.Count];
            for (int j = 0; j < rawProducts.Count; j++)
                products[j] = rawProducts[j] as ProductScriptableObject;
            if (products == null || products.Length == 0)
            {
                Debug.LogWarning(LogPrefix + "No ProductScriptableObject assets found. Inventory UI will be empty.");
                return;
            }

            float rowH = 1f / products.Length;
            for (int i = 0; i < products.Length; i++)
            {
                ProductScriptableObject product = products[i];
                if (product == null)
                    continue;

                float yMin = 1f - (i + 1) * rowH;
                float yMax = 1f - i * rowH;

                GameObject rowObj = new GameObject("Row_" + product.id,
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                rowObj.transform.SetParent(scrollArea.transform, false);
                RectTransform rowRT = rowObj.GetComponent<RectTransform>();
                rowRT.anchorMin = new Vector2(0f, yMin);
                rowRT.anchorMax = new Vector2(1f, yMax);
                rowRT.offsetMin = Vector2.zero;
                rowRT.offsetMax = Vector2.zero;
                rowObj.GetComponent<Image>().color = (i % 2 == 0) ? ColRowA : ColRowB;

                // Product name
                AddLabel(rowObj.transform, product.title,
                    new Vector2(0.00f, 0f), new Vector2(0.28f, 1f),
                    9, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, Color.white);

                // Furniture type
                AddLabel(rowObj.transform, product.storageType.ToString(),
                    new Vector2(0.28f, 0f), new Vector2(0.50f, 1f),
                    9, FontStyles.Normal, TextAlignmentOptions.Midline,
                    new Color(0.70f, 0.72f, 0.80f, 1f));

                // Boxes (runtime)
                TMP_Text boxesLabel = AddLabel(rowObj.transform, "—",
                    new Vector2(0.50f, 0f), new Vector2(0.65f, 1f),
                    9, FontStyles.Normal, TextAlignmentOptions.Midline, Color.white);

                // Shelf (runtime)
                TMP_Text shelfLabel = AddLabel(rowObj.transform, "—",
                    new Vector2(0.65f, 0f), new Vector2(0.80f, 1f),
                    9, FontStyles.Normal, TextAlignmentOptions.Midline, Color.white);

                // Status (runtime)
                TMP_Text statusLabel = AddLabel(rowObj.transform, "—",
                    new Vector2(0.80f, 0f), new Vector2(1.00f, 1f),
                    9, FontStyles.Bold, TextAlignmentOptions.Midline, ColGood);

                _rows.Add(new ProductRow
                {
                    Product     = product,
                    BoxesLabel  = boxesLabel,
                    ShelfLabel  = shelfLabel,
                    StatusLabel = statusLabel,
                });
            }

            Debug.Log(LogPrefix + $"Inventory UI built with {_rows.Count} product rows.");
        }

        // ── Refresh ────────────────────────────────────────────────────────────
        private void Refresh()
        {
            ProductInventorySystem sys = ProductInventorySystem.Instance;

            bool allStocked = true;
            for (int i = 0; i < _rows.Count; i++)
            {
                ProductRow row = _rows[i];
                if (row.Product == null)
                    continue;

                bool locked = false;
                if (EntrepreneurTreeProductUnlockAdapter.Instance != null)
                    locked = !EntrepreneurTreeProductUnlockAdapter.Instance.IsProductUnlocked(row.Product);

                if (locked)
                {
                    SetRow(row, "—", "—", "Bloqueado", ColLocked);
                    continue;
                }

                if (sys == null)
                {
                    SetRow(row, "?", "?", "Sin sistema", ColWarn);
                    continue;
                }

                int boxes   = sys.GetPackageStock(row.Product);
                int shelf   = sys.GetShelfStock(row.Product);
                int pending = sys.GetPendingUnits(row.Product);

                string boxText   = pending > 0 ? $"{boxes} (+{pending})" : boxes.ToString();
                string shelfText = shelf.ToString();

                string status;
                Color  statusCol;

                if (boxes == 0 && shelf == 0 && pending == 0)
                {
                    status    = "Sin stock";
                    statusCol = ColBad;
                    allStocked = false;
                    AchievementSystem.RegisterProductOutOfStock(row.Product);
                }
                else if (pending > 0 && boxes == 0 && shelf == 0)
                {
                    status    = "Llegando";
                    statusCol = ColPending;
                }
                else if (shelf == 0 && boxes > 0)
                {
                    status    = "En bodega";
                    statusCol = ColWarn;
                    allStocked = false;
                }
                else if (shelf > 0 && boxes == 0)
                {
                    status    = "Solo estante";
                    statusCol = ColWarn;
                }
                else
                {
                    status    = "Correcto";
                    statusCol = ColGood;
                }

                SetRow(row, boxText, shelfText, status, statusCol);
            }

            // Full-stock achievement hook (fires once per transition to fully-stocked)
            if (allStocked && !_wasFullStockLastCheck)
                AchievementSystem.RegisterFullStockDay();

            _wasFullStockLastCheck = allStocked;
        }

        private void OnMoneyUpdate(string _current, string _change) => Refresh();

        private static void SetRow(ProductRow row, string boxes, string shelf, string status, Color statusColor)
        {
            if (row.BoxesLabel  != null) row.BoxesLabel.text  = boxes;
            if (row.ShelfLabel  != null) row.ShelfLabel.text  = shelf;
            if (row.StatusLabel != null)
            {
                row.StatusLabel.text  = status;
                row.StatusLabel.color = statusColor;
            }
        }

        // ── UI builder helper ──────────────────────────────────────────────────
        private static TMP_Text AddLabel(
            Transform parent,
            string text,
            Vector2 anchorMin, Vector2 anchorMax,
            float fontSize,
            FontStyles style,
            TextAlignmentOptions alignment,
            Color color)
        {
            GameObject go = new GameObject("Label_" + text.Substring(0, Mathf.Min(text.Length, 12)),
                typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin  = anchorMin;
            rt.anchorMax  = anchorMax;
            rt.offsetMin  = new Vector2(4f, 0f);
            rt.offsetMax  = new Vector2(-4f, 0f);

            TMP_Text label = go.GetComponent<TMP_Text>();
            label.text           = text;
            label.fontSize       = fontSize;
            label.fontStyle      = style;
            label.alignment      = alignment;
            label.color          = color;
            label.overflowMode   = TextOverflowModes.Ellipsis;
            label.textWrappingMode = TextWrappingModes.NoWrap;

            return label;
        }
    }
}
