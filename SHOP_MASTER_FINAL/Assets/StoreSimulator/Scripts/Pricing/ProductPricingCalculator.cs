//Adaptado por POMPIC 20100333
using System;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    public static class ProductPricingCalculator
    {
        public const float PriceSensitivity = 0.1f;

        public static long GetIdealPrice(ProductScriptableObject product)
        {
            if (product == null)
                return 0;

            if (product.marketPrice > 0)
                return product.marketPrice;

            if (product.storePrice > 0)
                return product.storePrice;

            return Math.Max(0, product.buyPrice);
        }

        public static long GetMaxPrice(ProductScriptableObject product)
        {
            return Mathf.RoundToInt(GetIdealPrice(product) * 3f);
        }

        public static long ClampPrice(ProductScriptableObject product, long price)
        {
            return Math.Min(Math.Max(price, 0), GetMaxPrice(product));
        }

        public static float GetPurchaseProbability(ProductScriptableObject product, long price = -1)
        {
            long ideal = GetIdealPrice(product);
            if (ideal <= 0)
                return 1f;

            long current = price >= 0 ? price : product.storePrice;
            if (current <= ideal)
                return 1f;

            float differenceDollars = (current - ideal) / 100f;
            float probability = 1f / (1f + PriceSensitivity * differenceDollars * differenceDollars);
            return Mathf.Clamp01(probability);
        }

        public static float GetExtraPurchaseProbability(ProductScriptableObject product, long price = -1)
        {
            long ideal = GetIdealPrice(product);
            if (ideal <= 0)
                return 0f;

            long current = price >= 0 ? price : product.storePrice;
            if (current >= ideal)
                return 0f;

            return Mathf.Clamp01(0.1f * ((ideal - current) / (float)ideal));
        }

        public static string FormatPercent(float value)
        {
            return Mathf.RoundToInt(Mathf.Clamp01(value) * 100f) + "%";
        }

        public static string GetPriceFeedback(ProductScriptableObject product, long price)
        {
            long ideal = GetIdealPrice(product);
            if (price <= 0)
                return "Precio $0.00: esta venta no generara ingresos directos.";

            if (price > GetMaxPrice(product))
                return "El precio maximo es 300% del precio ideal.";

            if (ideal > 0 && price > ideal)
                return "Precio alto: reduce la probabilidad de compra.";

            if (ideal > 0 && price < ideal)
                return "Precio bajo: puede aumentar ventas extra.";

            return "Precio actualizado.";
        }
    }
}
