//Adaptado por POMPIC 20100333
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    public sealed class DocumentedProductDefinition
    {
        public string Id { get; }
        public string Title { get; }
        public string Category { get; }
        public string NodeId { get; }
        public string FurnitureCategory { get; }
        public long IdealPrice { get; }
        public long PackageCost { get; }
        public long CurrentPriceDefault { get; }
        public string IndividualDescription { get; }
        public string PackageDescription { get; }
        public string ProductKind { get; }
        public string PlaceholderKey { get; }
        public string FallbackText { get; }
        public bool CanBePurchased { get; }
        public bool CanBePriced { get; }
        public bool CanBePlaced { get; }
        public bool UsesProvisionalAsset { get; }
        public string PlaceholderSource { get; }

        public DocumentedProductDefinition(string id, string title, string category, string nodeId, string furnitureCategory, long idealPrice, long packageCost, bool usesProvisionalAsset, string placeholderSource)
        {
            Id = id;
            Title = title;
            Category = category;
            NodeId = nodeId;
            FurnitureCategory = furnitureCategory;
            IdealPrice = idealPrice;
            PackageCost = packageCost;
            CurrentPriceDefault = idealPrice;
            IndividualDescription = title + " para venta individual.";
            PackageDescription = "Caja funcional de " + title + ".";
            ProductKind = ResolveKind(furnitureCategory, category);
            PlaceholderKey = usesProvisionalAsset ? "placeholder_" + ProductKind.ToLowerInvariant() : "asset_base";
            FallbackText = string.IsNullOrEmpty(title) ? id : title.Substring(0, 1).ToUpperInvariant();
            CanBePurchased = true;
            CanBePriced = true;
            CanBePlaced = true;
            UsesProvisionalAsset = usesProvisionalAsset;
            PlaceholderSource = placeholderSource;
        }

        private static string ResolveKind(string furnitureCategory, string category)
        {
            if (furnitureCategory == "Refrigeradores")
                return category == "Sodas" ? "Soda" : "Refrigerated";
            if (furnitureCategory == "Congeladores")
                return category == "Productos de Lujo 1" ? "Luxury" : "Frozen";
            if (furnitureCategory == "Electrodomesticos")
                return "Appliance";
            if (category == "Productos de Higiene")
                return "Hygiene";

            return "DryGood";
        }
    }

    public static class DocumentedProductCatalog
    {
        private const string ExistingAssetSource = "Store Simulator product asset";
        private const string ProvisionalAssetSource = "Placeholder from Product_A-E";

        private static readonly List<DocumentedProductDefinition> definitions = new List<DocumentedProductDefinition>
        {
            Product("leche", "Leche", "Productos Basicos 1", "productos_basicos_1", "Gondolas", 100, 1200, false, ExistingAssetSource),
            Product("sal", "Sal", "Productos Basicos 1", "productos_basicos_1", "Gondolas", 50, 200, false, ExistingAssetSource),
            Product("agua", "Agua", "Productos Basicos 1", "productos_basicos_1", "Gondolas", 50, 600, false, ExistingAssetSource),
            Product("pasta", "Pasta", "Productos Basicos 1", "productos_basicos_1", "Gondolas", 100, 500, false, ExistingAssetSource),
            Product("azucar", "Azucar", "Productos Basicos 1", "productos_basicos_1", "Gondolas", 100, 400, false, ExistingAssetSource),

            Product("harina", "Harina", "Productos Basicos 2", "productos_basicos_2", "Gondolas", 100, 800, true, ProvisionalAssetSource),
            Product("arroz", "Arroz", "Productos Basicos 2", "productos_basicos_2", "Gondolas", 150, 1500, true, ProvisionalAssetSource),
            Product("frijoles", "Frijoles", "Productos Basicos 2", "productos_basicos_2", "Gondolas", 200, 1500, true, ProvisionalAssetSource),
            Product("pan", "Pan", "Productos Basicos 2", "productos_basicos_2", "Gondolas", 200, 700, true, ProvisionalAssetSource),
            Product("aceite", "Aceite", "Productos Basicos 2", "productos_basicos_2", "Gondolas", 250, 2500, true, ProvisionalAssetSource),
            Product("cafe", "Cafe", "Productos Basicos 3", "productos_basicos_3", "Gondolas", 300, 2500, true, ProvisionalAssetSource),
            Product("huevo", "Huevo", "Productos Basicos 3", "productos_basicos_3", "Gondolas", 200, 800, true, ProvisionalAssetSource),

            Product("cheddar", "Cheddar", "Lacteos 1", "lacteos_1", "Refrigeradores", 300, 1200, true, ProvisionalAssetSource),
            Product("yogurt_natural", "Yogurt natural", "Lacteos 1", "lacteos_1", "Refrigeradores", 100, 600, true, ProvisionalAssetSource),
            Product("mantequilla", "Mantequilla", "Lacteos 1", "lacteos_1", "Refrigeradores", 200, 800, true, ProvisionalAssetSource),
            Product("queso_americano", "Queso americano", "Lacteos 2", "lacteos_2", "Refrigeradores", 250, 1000, true, ProvisionalAssetSource),
            Product("queso_crema", "Queso crema", "Lacteos 2", "lacteos_2", "Refrigeradores", 200, 800, true, ProvisionalAssetSource),
            Product("mozzarella", "Mozzarella", "Lacteos 3", "lacteos_3", "Refrigeradores", 300, 1000, true, ProvisionalAssetSource),
            Product("parmesano", "Parmesano", "Lacteos 3", "lacteos_3", "Refrigeradores", 500, 2000, true, ProvisionalAssetSource),

            Product("pimienta_negra", "Pimienta negra", "Especias 1", "especias_1", "Gondolas", 200, 700, true, ProvisionalAssetSource),
            Product("canela", "Canela", "Especias 1", "especias_1", "Gondolas", 100, 500, true, ProvisionalAssetSource),
            Product("manzana", "Manzana", "Productos Frescos 1", "productos_frescos_1", "Refrigeradores", 200, 2000, true, ProvisionalAssetSource),
            Product("platano", "Platano", "Productos Frescos 1", "productos_frescos_1", "Refrigeradores", 100, 1000, true, ProvisionalAssetSource),
            Product("jitomate", "Jitomate", "Productos Frescos 1", "productos_frescos_1", "Refrigeradores", 200, 2000, true, ProvisionalAssetSource),
            Product("cebolla", "Cebolla", "Productos Frescos 1", "productos_frescos_1", "Refrigeradores", 150, 1500, true, ProvisionalAssetSource),
            Product("uvas", "Uvas", "Productos Frescos 2", "productos_frescos_2", "Refrigeradores", 400, 1000, true, ProvisionalAssetSource),
            Product("zanahorias", "Zanahorias", "Productos Frescos 2", "productos_frescos_2", "Refrigeradores", 100, 500, true, ProvisionalAssetSource),
            Product("ajo", "Ajo", "Productos Frescos 2", "productos_frescos_2", "Refrigeradores", 150, 600, true, ProvisionalAssetSource),

            Product("jabon", "Jabon", "Productos de Higiene", "productos_higiene", "Gondolas", 100, 1500, true, ProvisionalAssetSource),
            Product("papel_higienico", "Papel higienico", "Productos de Higiene", "productos_higiene", "Gondolas", 500, 2500, true, ProvisionalAssetSource),
            Product("detergente", "Detergente", "Productos de Higiene", "productos_higiene", "Gondolas", 200, 2000, true, ProvisionalAssetSource),
            Product("pasta_dientes", "Pasta de dientes", "Productos de Higiene", "productos_higiene", "Gondolas", 100, 800, true, ProvisionalAssetSource),
            Product("res", "Res", "Proteina 1", "proteina_1", "Congeladores", 1000, 4000, true, ProvisionalAssetSource),
            Product("pollo", "Pollo", "Proteina 1", "proteina_1", "Congeladores", 500, 2000, true, ProvisionalAssetSource),
            Product("cerdo", "Cerdo", "Proteina 1", "proteina_1", "Congeladores", 700, 3000, true, ProvisionalAssetSource),
            Product("pescado", "Pescado", "Proteina 1", "proteina_1", "Congeladores", 800, 3000, true, ProvisionalAssetSource),

            Product("cola", "Cola", "Sodas", "sodas", "Refrigeradores", 150, 1200, true, ProvisionalAssetSource),
            Product("cola_sin_azucar", "Cola sin azucar", "Sodas", "sodas", "Refrigeradores", 150, 1200, true, ProvisionalAssetSource),
            Product("refresco_limon", "Refresco de limon", "Sodas", "sodas", "Refrigeradores", 150, 1200, true, ProvisionalAssetSource),
            Product("trufa", "Trufa", "Productos de Lujo 1", "productos_lujo_1", "Congeladores", 10000, 15000, true, ProvisionalAssetSource),
            Product("chocolate_importado", "Chocolate importado", "Productos de Lujo 1", "productos_lujo_1", "Congeladores", 300, 1500, true, ProvisionalAssetSource),
            Product("caviar", "Caviar", "Productos de Lujo 1", "productos_lujo_1", "Congeladores", 5000, 40000, true, ProvisionalAssetSource),

            Product("refrigerador", "Refrigerador", "Electrodomesticos 1", "electrodomesticos_1", "Electrodomesticos", 40000, 40000, true, ProvisionalAssetSource),
            Product("microondas", "Microondas", "Electrodomesticos 1", "electrodomesticos_1", "Electrodomesticos", 6000, 6000, true, ProvisionalAssetSource),
            Product("horno", "Horno", "Electrodomesticos 1", "electrodomesticos_1", "Electrodomesticos", 20000, 20000, true, ProvisionalAssetSource),
            Product("mesa", "Mesa", "Electrodomesticos 1", "electrodomesticos_1", "Electrodomesticos", 10000, 10000, true, ProvisionalAssetSource),
            Product("licuadora", "Licuadora", "Electrodomesticos 1", "electrodomesticos_1", "Electrodomesticos", 8000, 8000, true, ProvisionalAssetSource),
        };

        public static IReadOnlyList<DocumentedProductDefinition> Definitions => definitions;
        public static int TotalDocumentedProducts => definitions.Count;

        private static readonly Dictionary<string, string> legacyIds = new Dictionary<string, string>
        {
            { "0", "leche" }, { "1", "sal" }, { "2", "agua" }, { "3", "pasta" }, { "4", "azucar" },
            { "doc_harina", "harina" }, { "doc_arroz", "arroz" }, { "doc_frijoles", "frijoles" }, { "doc_pan", "pan" }, { "doc_aceite", "aceite" },
            { "doc_cafe", "cafe" }, { "doc_huevo", "huevo" }, { "doc_cheddar", "cheddar" }, { "doc_yogurt_natural", "yogurt_natural" },
            { "doc_mantequilla", "mantequilla" }, { "doc_queso_americano", "queso_americano" }, { "doc_queso_crema", "queso_crema" },
            { "doc_mozzarella", "mozzarella" }, { "doc_parmesano", "parmesano" }, { "doc_pimienta_negra", "pimienta_negra" },
            { "doc_canela", "canela" }, { "doc_manzana", "manzana" }, { "doc_platano", "platano" }, { "doc_jitomate", "jitomate" },
            { "doc_cebolla", "cebolla" }, { "doc_uvas", "uvas" }, { "doc_zanahorias", "zanahorias" }, { "doc_ajo", "ajo" },
            { "doc_jabon", "jabon" }, { "doc_papel_higienico", "papel_higienico" }, { "doc_detergente", "detergente" },
            { "doc_pasta_dientes", "pasta_dientes" }, { "doc_res", "res" }, { "doc_pollo", "pollo" }, { "doc_cerdo", "cerdo" },
            { "doc_pescado", "pescado" }, { "doc_cola", "cola" }, { "doc_cola_sin_azucar", "cola_sin_azucar" },
            { "doc_refresco_limon", "refresco_limon" }, { "doc_trufa", "trufa" }, { "doc_chocolate_importado", "chocolate_importado" },
            { "doc_caviar", "caviar" }, { "doc_refrigerador", "refrigerador" }, { "doc_microondas", "microondas" },
            { "doc_horno", "horno" }, { "doc_mesa", "mesa" }, { "doc_licuadora", "licuadora" },
        };

        public static IReadOnlyList<DocumentedProductDefinition> GetByNode(string nodeId)
        {
            return definitions.Where(definition => definition.NodeId == nodeId).ToList();
        }

        public static DocumentedProductDefinition GetById(string id)
        {
            string canonicalId = GetCanonicalProductId(id);
            return definitions.FirstOrDefault(definition => definition.Id == canonicalId);
        }

        public static bool TryGetCanonicalProductId(string id, out string canonicalId)
        {
            canonicalId = GetCanonicalProductId(id);
            if (string.IsNullOrEmpty(canonicalId))
                return false;

            string resolvedId = canonicalId;
            return definitions.Any(definition => definition.Id == resolvedId);
        }

        public static string GetCanonicalProductId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return string.Empty;

            string key = id.Trim();
            return legacyIds.TryGetValue(key, out string canonicalId) ? canonicalId : key;
        }

        public static bool IsDocumentedProduct(ProductScriptableObject product)
        {
            return product != null && (GetById(product.id) != null || definitions.Any(definition => Normalize(definition.Title) == Normalize(product.title)));
        }

        public static void EnsureProducts(List<PurchasableScriptableObject> purchasables)
        {
            if (purchasables == null)
                return;

            List<ProductScriptableObject> products = purchasables.OfType<ProductScriptableObject>().ToList();
            ProductScriptableObject fallback = products.FirstOrDefault(product => product != null && product.prefab != null && product.icon != null) ?? products.FirstOrDefault(product => product != null);
            foreach (DocumentedProductDefinition definition in definitions)
            {
                ProductScriptableObject product = FindProduct(products, definition);
                if (product == null)
                {
                    product = ScriptableObject.CreateInstance<ProductScriptableObject>();
                    product.id = definition.Id;
                    product.title = definition.Title;
                    product.name = "Product_" + definition.Id;
                    if (fallback != null)
                    {
                        product.icon = fallback.icon;
                        product.prefab = fallback.prefab;
                        product.size = fallback.size;
                    }

                    purchasables.Add(product);
                    products.Add(product);
                }

                ApplyDefinition(product, definition, fallback);
            }
        }

        public static string GetUnlockSummaryForNode(string nodeId)
        {
            IReadOnlyList<DocumentedProductDefinition> products = GetByNode(nodeId);
            if (products.Count == 0)
                return string.Empty;

            string names = string.Join(", ", products.Select(product => product.Title));
            string furniture = string.Join(", ", products.Select(product => product.FurnitureCategory).Distinct());
            int provisional = products.Count(product => product.UsesProvisionalAsset);
            string placeholders = provisional > 0 ? "\nPlaceholder seguro: " + string.Join(", ", products.Where(product => product.UsesProvisionalAsset).Select(product => product.Title)) : "\nAssets finales: productos base del asset.";
            return "Desbloquea " + products.Count + " producto(s): " + names + "\nMueble sugerido: " + furniture + "\nAssets provisionales: " + provisional + "/" + products.Count + placeholders;
        }

        private static ProductScriptableObject FindProduct(List<ProductScriptableObject> products, DocumentedProductDefinition definition)
        {
            return products.FirstOrDefault(product => product != null && GetCanonicalProductId(product.id) == definition.Id)
                ?? products.FirstOrDefault(product => product != null && Normalize(product.title) == Normalize(definition.Title));
        }

        private static void ApplyDefinition(ProductScriptableObject product, DocumentedProductDefinition definition, ProductScriptableObject fallback)
        {
            product.id = definition.Id;
            product.title = definition.Title;
            product.category = definition.Category;
            product.requiredLevel = 0;
            product.requiredLicense = string.Empty;
            product.buyPrice = definition.PackageCost;
            product.packageCount = 1;
            product.marketPrice = definition.IdealPrice;
            product.storePrice = product.storePrice <= 0 ? definition.IdealPrice : product.storePrice;
            product.storageType = GetStorageType(definition.FurnitureCategory);
            if (fallback == null)
                return;

            if (product.prefab == null)
                product.prefab = fallback.prefab;
            if (product.icon == null)
                product.icon = fallback.icon;
            if (product.size == Vector2Int.zero)
                product.size = fallback.size;
        }

        private static StorageType GetStorageType(string furnitureCategory)
        {
            switch (furnitureCategory)
            {
                case "Refrigeradores":
                    return StorageType.Cooled;
                case "Congeladores":
                    return StorageType.Frozen;
                default:
                    return StorageType.Default;
            }
        }

        private static DocumentedProductDefinition Product(string id, string title, string category, string nodeId, string furnitureCategory, long idealPrice, long packageCost, bool usesProvisionalAsset, string placeholderSource)
        {
            return new DocumentedProductDefinition(id, title, category, nodeId, furnitureCategory, idealPrice, packageCost, usesProvisionalAsset, placeholderSource);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant().Replace(" ", "_");
        }
    }
}
