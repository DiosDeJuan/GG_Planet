using System.Collections.Generic;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Central pricing service for all ProductScriptableObject assets.
    ///
    /// DESIGN:
    ///   storePrice  — player-facing sell price.  Written by this system and read by
    ///                 Customer.cs line ~281 to decide whether to buy.
    ///   marketPrice — the "ideal" reference price set in the SO inspector.
    ///                 ItemDatabase already saves/loads both fields, so no
    ///                 duplicate persistence is needed here.
    ///
    /// RULES:
    ///   Minimum sell price : $0.00 (0 cents).
    ///   Maximum sell price : 300% of marketPrice.
    ///   Purchase probability formula (k = 0.1 by default):
    ///     If storePrice &lt;= marketPrice  =&gt; P = 1.0
    ///     Else                           =&gt; P = clamp(1 - k * (delta/marketPrice), 0, 1)
    ///   Extra-purchase probability:
    ///     If storePrice &gt;= marketPrice  =&gt; Pextra = 0
    ///     Else                          =&gt; Pextra = clamp((delta/marketPrice) * 0.10, 0, 1)
    ///
    /// SCENE SETUP:
    ///   Add this component to any persistent GameObject (e.g. the same one as
    ///   EntrepreneurTreeManager).  It self-initialises in Awake().
    /// </summary>
    public class ProductPricingSystem : MonoBehaviour
    {
        private const string LogPrefix = "[Pricing] ";

        // Probability slope — how fast purchase probability drops above marketPrice.
        [Range(0.01f, 2f)]
        public float probabilitySlope = 0.10f;

        // ── Singleton ─────────────────────────────────────────────────────────────

        public static ProductPricingSystem Instance { get; private set; }

        // ── Events ────────────────────────────────────────────────────────────────

        /// <summary>Fired when any product's storePrice changes.</summary>
        public static event System.Action<ProductScriptableObject> onPriceChanged;

        // ── Internal ──────────────────────────────────────────────────────────────

        // Products that reported a $0 marketPrice so we only warn once.
        private readonly HashSet<string> warnedZeroMarketPrice = new HashSet<string>();

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the ideal (market reference) price of a product in cents.
        /// Falls back to buyPrice if marketPrice is not set.
        /// </summary>
        public long GetIdealPrice(ProductScriptableObject product)
        {
            if (product == null) return 0;
            if (product.marketPrice > 0) return product.marketPrice;
            // Fallback: use buyPrice as reference (warn once).
            if (!warnedZeroMarketPrice.Contains(product.id))
            {
                warnedZeroMarketPrice.Add(product.id);
                Debug.LogWarning(LogPrefix + "marketPrice is 0 for product '" + product.title +
                                 "' (id=" + product.id + "). Falling back to buyPrice=" + product.buyPrice + " as ideal price.");
            }
            return product.buyPrice > 0 ? product.buyPrice : 100L; // emergency fallback $1
        }

        /// <summary>Returns the current sell price (storePrice) in cents.</summary>
        public long GetCurrentPrice(ProductScriptableObject product)
        {
            if (product == null) return 0;
            return product.storePrice;
        }

        /// <summary>Returns the maximum allowed sell price: 300% of marketPrice.</summary>
        public long GetMaxAllowedPrice(ProductScriptableObject product)
        {
            if (product == null) return 0;
            return GetIdealPrice(product) * 3L;
        }

        /// <summary>
        /// Attempts to set a new storePrice for a product.
        /// Clamps to [0, 300% idealPrice].
        /// Returns true on success; false + reason on validation failure.
        /// </summary>
        public bool TrySetCurrentPrice(ProductScriptableObject product, long newPriceCents, out string reason)
        {
            if (product == null)
            {
                reason = "Producto no válido.";
                return false;
            }

            long max = GetMaxAllowedPrice(product);

            if (newPriceCents < 0)
            {
                reason = "El precio mínimo es $0.00.";
                return false;
            }

            if (newPriceCents > max)
            {
                reason = "El precio máximo es " + StoreDatabase.FromLongToStringMoney(max) +
                         " (300% del precio ideal).";
                return false;
            }

            // Use the asset's own API to update storePrice and fire its event.
            ItemDatabase.UpdateStorePrice(product.id, newPriceCents);
            onPriceChanged?.Invoke(product);
            Debug.Log(LogPrefix + product.title + " → precio actualizado: " +
                      StoreDatabase.FromLongToStringMoney(newPriceCents));
            reason = string.Empty;
            return true;
        }

        /// <summary>
        /// Resets storePrice back to idealPrice (marketPrice) for one product.
        /// </summary>
        public void ResetProductPrice(ProductScriptableObject product)
        {
            if (product == null) return;
            TrySetCurrentPrice(product, GetIdealPrice(product), out _);
        }

        /// <summary>Resets storePrice to idealPrice for all available products.</summary>
        public void ResetAllPrices()
        {
            if (ItemDatabase.Instance == null) return;
            var products = ItemDatabase.GetByType(typeof(ProductScriptableObject));
            foreach (var p in products)
            {
                ProductScriptableObject prod = p as ProductScriptableObject;
                if (prod != null)
                    TrySetCurrentPrice(prod, GetIdealPrice(prod), out _);
            }
        }

        /// <summary>
        /// Probability [0..1] that a customer buys this product at the current price.
        ///   currentPrice &lt;= idealPrice =&gt; 1.0
        ///   currentPrice &gt;  idealPrice =&gt; 1 - slope * (over / ideal), clamped to [0, 1]
        /// </summary>
        public float GetPurchaseProbability(ProductScriptableObject product)
        {
            if (product == null) return 0f;
            long ideal   = GetIdealPrice(product);
            long current = product.storePrice;
            if (ideal <= 0) return 1f;
            if (current <= ideal) return 1f;
            float over = (float)(current - ideal) / ideal;
            return Mathf.Clamp01(1f - probabilitySlope * over);
        }

        /// <summary>
        /// Extra purchase probability [0..1] when price is below ideal.
        ///   currentPrice &gt;= idealPrice =&gt; 0
        ///   currentPrice &lt;  idealPrice =&gt; (under / ideal) * 0.10, clamped to [0, 1]
        /// </summary>
        public float GetExtraPurchaseProbability(ProductScriptableObject product)
        {
            if (product == null) return 0f;
            long ideal   = GetIdealPrice(product);
            long current = product.storePrice;
            if (ideal <= 0 || current >= ideal) return 0f;
            float under = (float)(ideal - current) / ideal;
            return Mathf.Clamp01(under * 0.10f);
        }

        /// <summary>
        /// Checks whether the customer should buy based on purchase probability.
        /// Used by ProductPurchaseProbabilityAdapter.
        /// </summary>
        public bool ShouldCustomerBuy(ProductScriptableObject product)
        {
            return Random.value <= GetPurchaseProbability(product);
        }

        /// <summary>
        /// Checks whether the customer should pick up an extra unit (bargain effect).
        /// </summary>
        public bool ShouldCustomerBuyExtra(ProductScriptableObject product)
        {
            float extra = GetExtraPurchaseProbability(product);
            return extra > 0f && Random.value <= extra;
        }
    }
}
