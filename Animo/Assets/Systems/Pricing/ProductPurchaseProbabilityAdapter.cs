using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Adapter that exposes price-based purchase probability to the customer layer.
    ///
    /// DESIGN — WHY AN ADAPTER AND NOT A DIRECT EDIT TO Customer.cs:
    ///   Customer.cs (FLOBUK asset) already has a willingness-to-pay check at line ~281
    ///   that compares product.storePrice against a random maxPrice derived from
    ///   product.marketPrice.  Because ProductPricingSystem writes player changes to
    ///   product.storePrice (the same field the asset reads), the asset's native check
    ///   already respects price changes without any modification to Customer.cs.
    ///
    ///   This adapter provides:
    ///   1. A clean API for future code that needs to query probabilities (UI, tests).
    ///   2. ShouldCustomerBuyExtra() — the "bargain extra unit" feature not in the asset.
    ///   3. Achievement hooks fired at the moment of customer sale.
    ///
    /// CONNECTION STATUS:
    ///   - Native price check (storePrice > maxPrice) in Customer.cs:~281:
    ///     ALREADY CONNECTED — storePrice is written by ProductPricingSystem.
    ///   - ShouldCustomerBuyExtra():
    ///     HOOK PREPARED — connect in Customer.cs Collect() after cart.Add() to allow
    ///     an extra pickup when price is below ideal.  Requires modifying the FLOBUK
    ///     asset.  Left as a documented pending item to avoid fragile changes.
    ///   - Achievement hooks:
    ///     HOOK PREPARED — call FireSaleHooks() from the point where payment completes.
    ///
    /// SCENE SETUP:
    ///   Add this component to the same persistent GameObject as ProductPricingSystem.
    /// </summary>
    public class ProductPurchaseProbabilityAdapter : MonoBehaviour
    {
        private const string LogPrefix = "[Pricing] ";

        // ── Singleton ─────────────────────────────────────────────────────────────

        public static ProductPurchaseProbabilityAdapter Instance { get; private set; }

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
        /// Returns true if, based on the current storePrice, a random customer should
        /// decide to purchase this product.
        /// Delegates to ProductPricingSystem.ShouldCustomerBuy().
        /// </summary>
        public bool ShouldCustomerBuy(ProductScriptableObject product)
        {
            if (product == null) return false;
            if (ProductPricingSystem.Instance == null) return true; // safe fallback
            return ProductPricingSystem.Instance.ShouldCustomerBuy(product);
        }

        /// <summary>
        /// Returns true if, due to a below-ideal price, the customer should pick up
        /// an extra unit of this product.
        /// Connect in Customer.cs after cart.Add() if extra pick-up behaviour is wanted.
        /// </summary>
        public bool ShouldCustomerBuyExtra(ProductScriptableObject product)
        {
            if (product == null) return false;
            if (ProductPricingSystem.Instance == null) return false;
            return ProductPricingSystem.Instance.ShouldCustomerBuyExtra(product);
        }

        /// <summary>
        /// Returns the current sell price (storePrice) of a product in cents.
        /// This is the price CustomerBagItem.fixedPrice is set to when the customer
        /// calls CustomerCart.Add() — no additional wiring is needed.
        /// </summary>
        public long GetCurrentSellPrice(ProductScriptableObject product)
        {
            if (product == null) return 0;
            return product.storePrice;
        }

        /// <summary>
        /// Call this when a sale is completed (e.g. after successful checkout payment)
        /// to fire achievement hooks related to pricing.
        ///
        /// PENDING: wire this to the checkout/payment completion point in the asset.
        /// Likely location: StoreDatabase.AddRemoveMoney() or PaymentItem callback.
        /// </summary>
        public void FireSaleHooks(ProductScriptableObject product, long pricePaidCents)
        {
            if (product == null) return;
            if (pricePaidCents == 0)
                AchievementSystem.RegisterProductSoldAtZero(product);
        }
    }
}
