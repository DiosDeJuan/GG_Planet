//Adaptado por POMPIC 20100333
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    public class UIManagementPanel : MonoBehaviour
    {
        private enum Section
        {
            Spaces,
            Prices,
            Help
        }

        private static readonly Color Background = new Color(0.92f, 0.93f, 0.95f, 1f);
        private static readonly Color Card = new Color(1f, 1f, 1f, 0.95f);
        private static readonly Color TextDark = new Color(0.15f, 0.16f, 0.18f, 1f);
        private static readonly Color TextMuted = new Color(0.42f, 0.44f, 0.48f, 1f);
        private static readonly Color Accent = new Color(0.92f, 0.18f, 0.42f, 1f);

        private readonly Dictionary<string, long> editedPrices = new Dictionary<string, long>();

        private Transform content;
        private TMP_Text messageLabel;
        private Section currentSection = Section.Spaces;
        private bool built;

        void OnEnable()
        {
            StoreDatabase.onMoneyUpdate += OnMoneyChanged;
            ItemDatabase.onStorePriceUpdate += OnStorePriceUpdated;
            ShopExpansionManager.onSpacesChanged += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            StoreDatabase.onMoneyUpdate -= OnMoneyChanged;
            ItemDatabase.onStorePriceUpdate -= OnStorePriceUpdated;
            ShopExpansionManager.onSpacesChanged -= Refresh;
        }

        public void Build()
        {
            if (built)
                return;

            built = true;
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            Stretch(rect);

            Image bg = gameObject.AddComponent<Image>();
            bg.color = Background;

            VerticalLayoutGroup rootLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(16, 16, 12, 12);
            rootLayout.spacing = 10;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;

            BuildHeader(transform);
            BuildTabs(transform);
            BuildScroll(transform);
            messageLabel = CreateText("Message", transform, string.Empty, 13, FontStyles.Bold, TextAlignmentOptions.Left);
            messageLabel.color = Accent;
            messageLabel.GetComponent<LayoutElement>().preferredHeight = 28;
        }

        private void BuildHeader(Transform parent)
        {
            GameObject header = CreatePanel("Header", parent, Card);
            LayoutElement layout = header.AddComponent<LayoutElement>();
            layout.preferredHeight = 76;

            VerticalLayoutGroup group = header.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(14, 14, 8, 8);
            group.spacing = 4;
            group.childControlWidth = true;
            group.childControlHeight = true;

            TMP_Text title = CreateText("Title", header.transform, "GESTION DEL SUPERMERCADO", 24, FontStyles.Bold, TextAlignmentOptions.Left);
            title.color = TextDark;
            TMP_Text subtitle = CreateText("Subtitle", header.transform, "Administra espacios, precios e instrucciones principales desde la computadora.", 13, FontStyles.Normal, TextAlignmentOptions.Left);
            subtitle.color = TextMuted;
        }

        private void BuildTabs(Transform parent)
        {
            GameObject tabs = CreatePanel("Section Tabs", parent, new Color(0.98f, 0.98f, 0.99f, 1f));
            LayoutElement layout = tabs.AddComponent<LayoutElement>();
            layout.preferredHeight = 44;

            HorizontalLayoutGroup group = tabs.AddComponent<HorizontalLayoutGroup>();
            group.padding = new RectOffset(8, 8, 6, 6);
            group.spacing = 8;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = true;

            CreateSectionButton(tabs.transform, "ESPACIOS", Section.Spaces);
            CreateSectionButton(tabs.transform, "PRECIOS", Section.Prices);
            CreateSectionButton(tabs.transform, "AYUDA", Section.Help);
        }

        private void BuildScroll(Transform parent)
        {
            GameObject scrollObject = CreatePanel("Content Scroll", parent, new Color(0.96f, 0.96f, 0.97f, 1f));
            LayoutElement layout = scrollObject.AddComponent<LayoutElement>();
            layout.flexibleHeight = 1;
            layout.minHeight = 410;

            ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 24f;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            GameObject viewport = CreateLayoutBox("Viewport", scrollObject.transform);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            Stretch(viewportRect, new Vector2(8, 8), new Vector2(-8, -8));
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = Color.clear;
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject contentObject = CreateLayoutBox("Content", viewport.transform);
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup contentGroup = contentObject.AddComponent<VerticalLayoutGroup>();
            contentGroup.padding = new RectOffset(4, 4, 4, 4);
            contentGroup.spacing = 8;
            contentGroup.childControlWidth = true;
            contentGroup.childControlHeight = true;
            contentGroup.childForceExpandWidth = true;
            contentGroup.childForceExpandHeight = false;
            ContentSizeFitter fitter = contentObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            content = contentObject.transform;
        }

        private void Refresh()
        {
            if (!built || content == null)
                return;

            for (int i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);

            switch (currentSection)
            {
                case Section.Spaces:
                    BuildSpacesSection();
                    break;
                case Section.Prices:
                    BuildPricesSection();
                    break;
                case Section.Help:
                    BuildHelpSection();
                    break;
            }
        }

        private void BuildSpacesSection()
        {
            CreateInfoCard("Area de venta",
                "Actual: " + ShopExpansionManager.CurrentSaleAreaM2 + " m2 / " + ShopExpansionManager.MaxSaleAreaM2 + " m2\n" +
                "Unidad: +" + ShopExpansionManager.SaleUnitAreaM2 + " m2\n" +
                "Costo: " + StoreDatabase.FromLongToStringMoney(ShopExpansionManager.SaleUnitCost) + "\n" +
                "Clientela estimada: +" + Mathf.RoundToInt(ShopExpansionManager.PurchasedSaleUnits * ShopExpansionManager.CustomerIncreasePerSaleUnit * 100f) + "%\n" +
                "Probabilidad ladron: " + ProductPricingCalculator.FormatPercent(ShopExpansionManager.GetShoplifterSpawnChance()),
                "Comprar venta", BuySaleSpace);

            CreateInfoCard("Area de almacenamiento",
                "Actual: " + ShopExpansionManager.CurrentStorageAreaM2 + " m2 / " + ShopExpansionManager.MaxStorageAreaM2 + " m2\n" +
                "Unidad: +" + ShopExpansionManager.StorageUnitAreaM2 + " m2\n" +
                "Costo: " + StoreDatabase.FromLongToStringMoney(ShopExpansionManager.StorageUnitCost) + "\n" +
                "Estado: " + (ShopExpansionManager.AreAllStorageSpacesPurchased ? "Maximo alcanzado" : "Disponible"),
                "Comprar almacen", BuyStorageSpace);

            CreateInfoCard("Finales",
                "Monopolio: requiere Arbol completo y todos los espacios comprados.\n" +
                "Estado: " + (GameEndingService.MonopolyAvailable ? "Disponible" : "Pendiente") + "\n" +
                "Bancarrota: se avisa al cierre del dia si el saldo queda negativo.\n" +
                "Estado: " + (GameEndingService.BankruptcyRisk ? "Riesgo detectado" : "Sin riesgo activo"),
                string.Empty, null);
        }

        private void BuildPricesSection()
        {
            List<ProductScriptableObject> products = ItemDatabase.GetByType(typeof(ProductScriptableObject))
                .OfType<ProductScriptableObject>()
                .OrderBy(product => product.title)
                .ToList();

            if (products.Count == 0)
            {
                CreateInfoCard("Precios", "No hay productos configurados en ItemDatabase.", string.Empty, null);
                return;
            }

            foreach (ProductScriptableObject product in products)
            {
                if (!editedPrices.ContainsKey(product.id))
                    editedPrices[product.id] = product.storePrice;

                long editedPrice = ProductPricingCalculator.ClampPrice(product, editedPrices[product.id]);
                editedPrices[product.id] = editedPrice;
                CreatePriceRow(product, editedPrice);
            }
        }

        private void BuildHelpSection()
        {
            CreateInfoCard("Instrucciones",
                "Compra productos en PRODUCTS y recibe cajas por delivery.\n" +
                "Coloca productos en estantes desde el modo constructor.\n" +
                "Ajusta precios en PRECIOS o con etiquetas fisicas de precio.\n" +
                "Atiende caja manualmente o contrata cajeros en EMPLEADOS.\n" +
                "Asigna surtidores para reponer productos desde almacenamiento.\n" +
                "Compra ESPACIOS para aumentar area, clientela y reto de seguridad.\n" +
                "Usa ARBOL para desbloquear productos, empleados, seguridad y mejoras.\n" +
                "Completa logros para ganar puntos de progreso.",
                string.Empty, null);
        }

        private void CreatePriceRow(ProductScriptableObject product, long editedPrice)
        {
            GameObject card = CreatePanel("Price - " + product.id, content, Card);
            LayoutElement layout = card.AddComponent<LayoutElement>();
            layout.preferredHeight = 116;

            HorizontalLayoutGroup group = card.AddComponent<HorizontalLayoutGroup>();
            group.padding = new RectOffset(12, 12, 8, 8);
            group.spacing = 10;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = true;

            TMP_Text info = CreateText("Info", card.transform, GetProductPriceText(product, editedPrice), 12, FontStyles.Normal, TextAlignmentOptions.Left);
            info.color = TextDark;
            info.GetComponent<LayoutElement>().flexibleWidth = 1;

            CreateSmallButton(card.transform, "-$0.25", () => AdjustPrice(product, -25));
            CreateSmallButton(card.transform, "+$0.25", () => AdjustPrice(product, 25));
            CreateSmallButton(card.transform, "Aplicar", () => ApplyPrice(product));
        }

        private string GetProductPriceText(ProductScriptableObject product, long price)
        {
            return product.title + "\n" +
                   "Ideal: " + StoreDatabase.FromLongToStringMoney(ProductPricingCalculator.GetIdealPrice(product)) +
                   " | Actual: " + StoreDatabase.FromLongToStringMoney(price) + "\n" +
                   "Pcompra: " + ProductPricingCalculator.FormatPercent(ProductPricingCalculator.GetPurchaseProbability(product, price)) +
                   " | Pextra: " + ProductPricingCalculator.FormatPercent(ProductPricingCalculator.GetExtraPurchaseProbability(product, price)) + "\n" +
                   ProductPricingCalculator.GetPriceFeedback(product, price);
        }

        private void BuySaleSpace()
        {
            bool success = ShopExpansionManager.TryPurchaseSaleSpace(out string message);
            ShowMessage(message, success);
        }

        private void BuyStorageSpace()
        {
            bool success = ShopExpansionManager.TryPurchaseStorageSpace(out string message);
            ShowMessage(message, success);
        }

        private void AdjustPrice(ProductScriptableObject product, long delta)
        {
            editedPrices[product.id] = ProductPricingCalculator.ClampPrice(product, editedPrices[product.id] + delta);
            Refresh();
        }

        private void ApplyPrice(ProductScriptableObject product)
        {
            long price = ProductPricingCalculator.ClampPrice(product, editedPrices[product.id]);
            ItemDatabase.UpdateStorePrice(product.id, price);
            ShowMessage(ProductPricingCalculator.GetPriceFeedback(product, price), true);
            Refresh();
        }

        private void ShowMessage(string message, bool success)
        {
            if (messageLabel != null)
            {
                messageLabel.text = message;
                messageLabel.color = success ? new Color(0.1f, 0.45f, 0.18f, 1f) : Accent;
            }

            if (UIGame.Instance != null)
                UIGame.Instance.ShowMessage(message);

            Refresh();
        }

        private void CreateInfoCard(string title, string body, string buttonLabel, UnityEngine.Events.UnityAction action)
        {
            GameObject card = CreatePanel(title, content, Card);
            LayoutElement layout = card.AddComponent<LayoutElement>();
            layout.preferredHeight = string.IsNullOrEmpty(buttonLabel) ? 150 : 174;

            VerticalLayoutGroup group = card.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(12, 12, 8, 8);
            group.spacing = 6;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;

            TMP_Text header = CreateText("Title", card.transform, title, 16, FontStyles.Bold, TextAlignmentOptions.Left);
            header.color = TextDark;
            TMP_Text text = CreateText("Body", card.transform, body, 12, FontStyles.Normal, TextAlignmentOptions.Left);
            text.color = TextMuted;
            text.GetComponent<LayoutElement>().flexibleHeight = 1;

            if (!string.IsNullOrEmpty(buttonLabel) && action != null)
            {
                Button button = CreateActionButton(buttonLabel, card.transform, buttonLabel);
                button.onClick.AddListener(action);
            }
        }

        private void CreateSectionButton(Transform parent, string label, Section section)
        {
            Button button = CreateActionButton(label, parent, label);
            button.onClick.AddListener(() =>
            {
                currentSection = section;
                Refresh();
            });
        }

        private void CreateSmallButton(Transform parent, string label, UnityEngine.Events.UnityAction action)
        {
            Button button = CreateActionButton(label, parent, label);
            LayoutElement layout = button.GetComponent<LayoutElement>();
            layout.preferredWidth = 96;
            layout.flexibleWidth = 0;
            button.onClick.AddListener(action);
        }

        private Button CreateActionButton(string name, Transform parent, string label)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            obj.transform.SetParent(parent, false);
            Image image = obj.GetComponent<Image>();
            image.color = Accent;
            Button button = obj.GetComponent<Button>();
            LayoutElement layout = obj.GetComponent<LayoutElement>();
            layout.preferredHeight = 34;

            TMP_Text text = CreateText("Text", obj.transform, label, 13, FontStyles.Bold, TextAlignmentOptions.Center);
            text.color = Color.white;
            Stretch(text.GetComponent<RectTransform>());
            return button;
        }

        private static TMP_Text CreateText(string name, Transform parent, string value, int size, FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            obj.transform.SetParent(parent, false);
            TMP_Text text = obj.GetComponent<TMP_Text>();
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Ellipsis;
            return text;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            Image image = obj.GetComponent<Image>();
            image.color = color;
            return obj;
        }

        private static GameObject CreateLayoutBox(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private void OnMoneyChanged(string money, string change)
        {
            Refresh();
        }

        private void OnStorePriceUpdated(ProductScriptableObject product, string newPrice)
        {
            if (product != null)
                editedPrices[product.id] = product.storePrice;

            Refresh();
        }

        private static void Stretch(RectTransform rect)
        {
            Stretch(rect, Vector2.zero, Vector2.zero);
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}
