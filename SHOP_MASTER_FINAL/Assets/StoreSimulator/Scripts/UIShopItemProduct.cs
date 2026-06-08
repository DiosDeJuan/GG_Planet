//Adaptado por POMPIC 20100333
/*  This file is part of the "Store Simulator" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store, Epic FAB).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using TMPro;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// UI representation of a ProductScriptableObject that can be purchased.
    /// </summary>
    public class UIShopItemProduct : UIShopItem
    {
        /// <summary>
        /// Label for displaying the total price, based on package count multiplied by unit price.
        /// </summary>
        public TMP_Text totalPrice;

        /// <summary>
        /// Label for displaying the self-defined store price for one unit. Currently not used.
        /// </summary>
        public TMP_Text storePrice;

        /// <summary>
        /// Label for displaying the customer expected market price for one unit. Currently not used.
        /// </summary>
        public TMP_Text marketPrice;


        /// <summary>
        /// Extend or override the base UIShopItem initialization.
        /// Here we've implemented the display of additional prices defined in this class.
        /// </summary>
        public override void Initialize(PurchasableScriptableObject purchasable)
        {
            base.Initialize(purchasable);

            ProductScriptableObject product = purchasable as ProductScriptableObject;
            if (product == null)
                return;

            if (totalPrice) totalPrice.text = StoreDatabase.FromLongToStringMoney(product.buyPrice * product.packageCount);
            if (storePrice) storePrice.text = StoreDatabase.FromLongToStringMoney(product.storePrice);
            if (marketPrice) marketPrice.text = StoreDatabase.FromLongToStringMoney(product.marketPrice);

            if (!lockedOverlay)
                return;

            if (!EntrepreneurProgress.IsProductUnlocked(product))
            {
                lockedOverlay.SetActive(true);
                if (lockedMessage) lockedMessage.text = EntrepreneurTreeDefinitions.GetUnlockRequirementLabel(product);
                return;
            }

            if (!string.IsNullOrEmpty(product.requiredLicense))
            {
                LicenseScriptableObject requiredLicense = ItemDatabase.GetById(typeof(LicenseScriptableObject), product.requiredLicense) as LicenseScriptableObject;
                if (requiredLicense != null)
                {
                    lockedOverlay.SetActive(!requiredLicense.isPurchased);
                    if (lockedMessage) lockedMessage.text = "Requires License " + requiredLicense.title;
                    return;
                }
            }

            if (product.requiredLevel <= 0 || StoreDatabase.Instance.currentLevel >= product.requiredLevel)
                lockedOverlay.SetActive(false);
        }


        /// <summary>
        /// Forward the purchase call to the responsible system.
        /// </summary>
        public override void Purchase()
        {
            ProductScriptableObject product = purchasable as ProductScriptableObject;
            if (product != null && !EntrepreneurProgress.IsProductUnlocked(product))
            {
                if (UIGame.Instance != null)
                    UIGame.Instance.ShowMessage(EntrepreneurTreeDefinitions.GetUnlockRequirementLabel(product));

                return;
            }

            DeliverySystem.Purchase(purchasable);
        }
    }
}
