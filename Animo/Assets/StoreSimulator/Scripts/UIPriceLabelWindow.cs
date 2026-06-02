/*  This file is part of the "Store Simulator" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store, Epic FAB).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */
/*  Adaptado por Isaac Victoria. */

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// UI script for showing a separate window overlay 
    /// </summary>
    public class UIPriceLabelWindow : MonoBehaviour
    {
        /// <summary>
        /// Label for displaying the name of the product to be modified.
        /// </summary>‚
        public TMP_Text title;

        /// <summary>
        /// Label for displaying the price of one item unit.
        /// </summary>‚
        public TMP_Text buyPrice;

        /// <summary>
        /// Input field for changing the selling price of one product unit.
        /// </summary>
        public TMP_InputField storePrice;

        /// <summary>
        /// Label for displaying the customer expected market price for one unit.
        /// </summary>
        public TMP_Text marketPrice;

        /// <summary>
        /// Label for displaying the profit based on buyPrice and storePrice.
        /// </summary>
        public TMP_Text profitResult;

        //a reference to the product the window was initialized with
        private ProductScriptableObject product;


        //initialize references
        void Awake()
        {
            storePrice.onValidateInput = OnValidateInput;
            storePrice.onEndEdit.AddListener((x) => Apply());
        }


        /// <summary>
        /// Show window after initialization of label fields with product data.
        /// </summary>
        public void Initialize(ProductScriptableObject product)
        {
            PlayerInput.GetPlayerByIndex(0).onActionTriggered += OnAction;
            UIGame.AddAction("Esc", "Exit");

            PlayerController.SetMovementState(MovementState.None, false);
            InteractionSystem.SetInteractionState(InteractionState.None);
            
            this.product = product;
            title.text = product.name;
            buyPrice.text = StoreDatabase.FromLongToStringMoney(product.buyPrice);
            storePrice.text = StoreDatabase.FromLongToStringMoney(product.storePrice);
            marketPrice.text = StoreDatabase.FromLongToStringMoney(product.marketPrice);

            CalculateProfit();
            gameObject.SetActive(true);
        }


        /// <summary>
        /// Recalculate expected profit margin after ending storePrice input field editing.
        /// </summary>
        public void Apply()
        {
            storePrice.text = StoreDatabase.FromLongToStringMoney(StoreDatabase.FromStringToLongMoney(storePrice.text));

            CalculateProfit();
        }


        /// <summary>
        /// Disable this window and re-enable player movement.
        /// </summary>
        public void Hide()
        {
            long newPrice = StoreDatabase.FromStringToLongMoney(storePrice.text);
            if (ProductPricingSystem.Instance != null &&
                !ProductPricingSystem.Instance.TrySetCurrentPrice(product, newPrice, out string reason))
            {
                UIGame.AddNotification(reason, otherColor: new Color(1f, 0.30f, 0.20f));
                CalculateProfit();
                return;
            }

            PlayerInput.GetPlayerByIndex(0).onActionTriggered -= OnAction;
            UIGame.RemoveAction("Esc");

            PlayerController.SetMovementState(MovementState.All, true);
            InteractionSystem.SetInteractionState(InteractionState.All);
            
            if (ProductPricingSystem.Instance == null)
                ItemDatabase.UpdateStorePrice(product.id, newPrice);
            gameObject.SetActive(false);
        }


        //validate input in storePrice input field
        //only allow numbers and one single dot/comma
        private char OnValidateInput(string text, int charIndex, char addedChar)
        {
            if (char.IsDigit(addedChar) || addedChar == ',' || addedChar == '.')
            {
                if (addedChar == ',') addedChar = '.';
                if (addedChar == '.' && text.Contains('.'))
                    return '\0';

                int firstDot = text.IndexOf('.');
                if (firstDot > 0 && text.Length > firstDot + 2 && charIndex == text.Length)
                    return '\0';

                return addedChar;
            }
            
            return '\0';
        }


        //react on user input
        private void OnAction(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                switch(context.action.name)
                {
                    case "Cancel":
                        Hide();
                        break;
                }
            }
        }


        //convert currency strings to long and calculate profit from that
        private void CalculateProfit()
        {
            long currentPrice = string.IsNullOrEmpty(storePrice.text) ? 0 : StoreDatabase.FromStringToLongMoney(storePrice.text);
            long profitValue = currentPrice - product.buyPrice;
            string profitText = profitValue >= 0 ? "<color=green>Profit: " : "<color=red>Loss: ";
            string feedback = string.Empty;
            if (ProductPricingSystem.Instance != null)
            {
                long ideal = ProductPricingSystem.Instance.GetIdealPrice(product);
                long max = ProductPricingSystem.Instance.GetMaxAllowedPrice(product);
                float purchase = ProductPricingSystem.Instance.GetPurchaseProbabilityForPrice(product, currentPrice);
                float extra = ProductPricingSystem.Instance.GetExtraPurchaseProbabilityForPrice(product, currentPrice);

                if (currentPrice > max)
                    feedback = "\n<color=red>Maximo permitido: " + StoreDatabase.FromLongToStringMoney(max) + ".</color>";
                else if (currentPrice == 0)
                    feedback = "\n<color=yellow>Precio $0.00: ingreso directo $0.00.</color>";
                else if (currentPrice < ideal)
                    feedback = "\n<color=green>Precio bajo: posibilidad de venta extra.</color>";
                else if (currentPrice > ideal)
                    feedback = "\n<color=yellow>Precio alto: menor probabilidad de compra.</color>";

                feedback += "\nCompra: " + Mathf.RoundToInt(purchase * 100f)
                    + "%  Extra: " + Mathf.RoundToInt(extra * 100f) + "%";
            }

            profitResult.text = profitText + StoreDatabase.FromLongToStringMoney(profitValue) + feedback;
        }
    }
}
