using System;
using System.Collections.Generic;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Aggregates per-product stock by querying the asset's existing PackageObjects (stored boxes)
    /// and PlacementObjects (on-shelf items).  Does NOT duplicate or own any physical items;
    /// it is a read layer on top of the asset's own scene objects.
    ///
    /// Additionally it:
    ///   - Fires <see cref="onInventoryChanged"/> whenever stock changes.
    ///   - Tracks "incoming orders" registered via <see cref="RegisterIncomingOrder"/> so the
    ///     UI can show a "Pending delivery" state before the box lands in the scene.
    ///   - Notifies via UIGame when a product's shelf stock falls to zero.
    ///   - Emits a warning (once per attempt) when a PlacementObject receives a product whose
    ///     required StorageType does not match the placement.
    ///
    /// Log prefix: [Inventory]
    /// </summary>
    public class ProductInventorySystem : MonoBehaviour
    {
        private const string LogPrefix = "[Inventory] ";

        // ── Singleton ──────────────────────────────────────────────────────────
        public static ProductInventorySystem Instance { get; private set; }

        // ── Events ─────────────────────────────────────────────────────────────
        /// <summary>Raised whenever package or placement counts change.</summary>
        public static event Action onInventoryChanged;

        // ── Incoming orders (not yet physically spawned) ────────────────────────
        private readonly Dictionary<string, int> _pendingOrderUnits =
            new Dictionary<string, int>(StringComparer.Ordinal);

        // ── Mismatch guard (rate-limit notifications) ───────────────────────────
        private readonly Dictionary<string, float> _lastMismatchNotifyTime =
            new Dictionary<string, float>(StringComparer.Ordinal);
        private const float MismatchNotifyCooldown = 6f;

        // ── Unity lifecycle ────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            DeliverySystem.onProductPurchase    += OnProductPurchased;
            SaveGameSystem.dataLoadEvent        += OnDataLoaded;
            ShelfProductSlotSystem.onSlotsChanged += OnSlotsChanged;
        }

        private void OnDisable()
        {
            DeliverySystem.onProductPurchase    -= OnProductPurchased;
            SaveGameSystem.dataLoadEvent        -= OnDataLoaded;
            ShelfProductSlotSystem.onSlotsChanged -= OnSlotsChanged;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // ── Public stock queries ───────────────────────────────────────────────

        /// <summary>
        /// Total units of <paramref name="product"/> inside all PackageObjects in the scene.
        /// This represents items stored in boxes (not yet on shelves).
        /// </summary>
        public int GetPackageStock(ProductScriptableObject product)
        {
            if (product == null)
                return 0;

            int total = 0;
            PackageObject[] packages =
                FindObjectsByType<PackageObject>(FindObjectsSortMode.None);
            for (int i = 0; i < packages.Length; i++)
            {
                PackageObject pkg = packages[i];
                if (pkg == null || pkg.purchasable == null)
                    continue;
                if (pkg.purchasable is ProductScriptableObject pkgProduct &&
                    pkgProduct == product)
                    total += pkg.count;
            }
            return total;
        }

        /// <summary>
        /// Total units of <paramref name="product"/> sitting on all PlacementObjects
        /// (i.e. visible on shelves/racks).
        /// </summary>
        public int GetShelfStock(ProductScriptableObject product)
        {
            if (product == null || StoreDatabase.Instance == null)
                return 0;

            int total = 0;
            PlacementObject[] placements =
                FindObjectsByType<PlacementObject>(FindObjectsSortMode.None);
            for (int i = 0; i < placements.Length; i++)
            {
                PlacementObject pl = placements[i];
                if (pl == null || pl.product == null)
                    continue;
                if (pl.product == product)
                    total += pl.count;
            }
            return total;
        }

        /// <summary>
        /// Total units across boxes + shelves.
        /// </summary>
        public int GetTotalStock(ProductScriptableObject product)
            => GetPackageStock(product) + GetShelfStock(product);

        /// <summary>
        /// Units pending delivery (order placed but PackageObject not yet in scene).
        /// </summary>
        public int GetPendingUnits(ProductScriptableObject product)
        {
            if (product == null)
                return 0;
            _pendingOrderUnits.TryGetValue(product.id.ToString(), out int pending);
            return pending;
        }

        /// <summary>
        /// Returns all <see cref="ProductScriptableObject"/> from <see cref="ItemDatabase"/>
        /// whose total stock (boxes + shelves) is below <paramref name="lowThreshold"/> units
        /// AND have no pending orders.
        /// </summary>
        public List<ProductScriptableObject> GetLowStockProducts(int lowThreshold = 3)
        {
            var result = new List<ProductScriptableObject>();
            List<PurchasableScriptableObject> all =
                ItemDatabase.GetByType(typeof(ProductScriptableObject));
            for (int i = 0; i < all.Count; i++)
            {
                ProductScriptableObject product = all[i] as ProductScriptableObject;
                if (product == null)
                    continue;
                if (GetTotalStock(product) < lowThreshold && GetPendingUnits(product) == 0)
                    result.Add(product);
            }
            return result;
        }

        // ── Placement mismatch validation ──────────────────────────────────────

        /// <summary>
        /// Checks whether <paramref name="product"/> is compatible with
        /// <paramref name="placement"/> (matching <see cref="StorageType"/>).
        /// Shows a one-per-cooldown UI notification when mismatched.
        /// Returns <c>true</c> if compatible.
        /// </summary>
        public bool ValidatePlacement(ProductScriptableObject product, PlacementObject placement)
        {
            if (product == null || placement == null)
                return true; // nothing to validate

            string reason;
            if (ShelfProductSlotSystem.CanPlaceProductOnFurniture(product, placement, out reason))
                return true;

            string key = product.id.ToString() + "_" + placement.GetInstanceID();
            float now = Time.time;
            _lastMismatchNotifyTime.TryGetValue(key, out float last);
            if (now - last < MismatchNotifyCooldown)
                return false;

            _lastMismatchNotifyTime[key] = now;
            string msg = string.Format(
                "Producto incorrecto: {0} no puede colocarse en {1}.",
                product.title, placement.gameObject.name);
            if (UIGame.Instance != null)
                UIGame.AddNotification(msg, otherColor: new Color(1f, 0.30f, 0.30f, 1f));
            Debug.LogWarning(LogPrefix + msg);
            if (!string.IsNullOrEmpty(reason))
                Debug.LogWarning(LogPrefix + reason);
            AchievementSystem.RegisterWrongPlacement(product);
            return false;
        }

        // ── Incoming order registration ────────────────────────────────────────

        /// <summary>
        /// Called by <see cref="OrdersAppUIController"/> (or any ordering system) to register
        /// units that are about to arrive.  Reduces the pending count once the
        /// <see cref="DeliverySystem.onProductPurchase"/> event fires and clears it on load.
        /// </summary>
        public void RegisterIncomingOrder(ProductScriptableObject product, int units)
        {
            if (product == null || units <= 0)
                return;

            string key = product.id.ToString();
            _pendingOrderUnits.TryGetValue(key, out int current);
            _pendingOrderUnits[key] = current + units;
            Debug.Log(LogPrefix + $"Incoming order registered: {product.title} x{units} (total pending: {_pendingOrderUnits[key]})");
            onInventoryChanged?.Invoke();
        }

        // ── Manual refresh trigger ─────────────────────────────────────────────

        /// <summary>Forces all subscribers to refresh their stock display.</summary>
        public void ForceRefresh() => onInventoryChanged?.Invoke();

        // ── Private helpers ────────────────────────────────────────────────────

        private void OnProductPurchased(ProductScriptableObject product)
        {
            if (product == null)
                return;

            // Clear pending units for this product (it has arrived)
            string key = product.id.ToString();
            _pendingOrderUnits.Remove(key);

            Debug.Log(LogPrefix + $"Product arrived: {product.title}. Stock: boxes={GetPackageStock(product)} shelf={GetShelfStock(product)}");
            onInventoryChanged?.Invoke();
        }

        private void OnDataLoaded()
        {
            _pendingOrderUnits.Clear();
            onInventoryChanged?.Invoke();
        }

        private void OnSlotsChanged() => onInventoryChanged?.Invoke();
    }
}
