//Adaptado por POMPIC 20100333
/*  This file is part of the "Store Simulator" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store, Epic FAB).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using TMPro;
using UnityEngine.UI;

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
        /// Optional label for displaying current state like Disponible/Fondos insuficientes.
        /// </summary>
        public TMP_Text stateLabel;

        /// <summary>
        /// Optional button reference to disable purchases when blocked.
        /// </summary>
        public Button purchaseButton;


        void Awake()
        {
            StoreDatabase.onMoneyUpdate += OnMoneyUpdate;
            EntrepreneurProgress.onProgressChanged += OnProgressChanged;
            UpgradeSystem.onUpgradePurchase += OnUpgradePurchase;
        }


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

            UpdatePurchaseState(product);
        }


        /// <summary>
        /// Forward the purchase call to the responsible system.
        /// </summary>
        public override void Purchase()
        {
            ProductScriptableObject product = purchasable as ProductScriptableObject;
            if (product == null)
            {
                if (UIGame.Instance != null)
                    UIGame.Instance.ShowMessage("Producto no configurado correctamente: sin referencia. Revisar catálogo.");

                return;
            }

            if (DeliverySystem.TryPurchase(product, out string message))
            {
                if (UIGame.Instance != null)
                    UIGame.Instance.ShowMessage(message);
            }
            else if (UIGame.Instance != null)
            {
                UIGame.Instance.ShowMessage(message);
            }

            UpdatePurchaseState(product);
        }


        private void OnMoneyUpdate(string money, string change)
        {
            ProductScriptableObject product = purchasable as ProductScriptableObject;
            if (product != null)
                UpdatePurchaseState(product);
        }


        private void OnProgressChanged()
        {
            ProductScriptableObject product = purchasable as ProductScriptableObject;
            if (product != null)
                UpdatePurchaseState(product);
        }


        private void OnUpgradePurchase(PurchasableScriptableObject otherPurchasable)
        {
            if (otherPurchasable is not LicenseScriptableObject)
                return;

            ProductScriptableObject product = purchasable as ProductScriptableObject;
            if (product != null)
                UpdatePurchaseState(product);
        }


        private void UpdatePurchaseState(ProductScriptableObject product)
        {
            bool canPurchase = DeliverySystem.CanPurchaseWithMessage(product, out string blockMessage);
            bool treeLocked = !EntrepreneurProgress.IsProductUnlocked(product);

            if (lockedOverlay)
                lockedOverlay.SetActive(!canPurchase);

            if (lockedMessage)
                lockedMessage.text = canPurchase ? "Disponible" : blockMessage;

            if (stateLabel)
            {
                if (canPurchase)
                    stateLabel.text = "Disponible";
                else if (treeLocked)
                    stateLabel.text = "Bloqueado por Árbol";
                else if (blockMessage.StartsWith("Fondos insuficientes"))
                    stateLabel.text = "Fondos insuficientes";
                else if (blockMessage.StartsWith("Producto no configurado"))
                    stateLabel.text = "No configurado";
                else
                    stateLabel.text = "Bloqueado";
            }

            if (purchaseButton == null)
                purchaseButton = GetComponentInChildren<Button>(true);

            if (purchaseButton != null)
                purchaseButton.interactable = canPurchase;
        }


        void OnDestroy()
        {
            StoreDatabase.onMoneyUpdate -= OnMoneyUpdate;
            EntrepreneurProgress.onProgressChanged -= OnProgressChanged;
            UpgradeSystem.onUpgradePurchase -= OnUpgradePurchase;
        }
    }
}
