using System;
using System.Collections.Generic;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Maps entrepreneur product nodes to real ProductScriptableObject entries from the asset.
    /// Used by gameplay and shop UI to know whether a product can be purchased.
    /// </summary>
    public class EntrepreneurTreeProductUnlockAdapter : MonoBehaviour
    {
        private const string LogPrefix = "[EntrepreneurTree] ";

        [Serializable]
        public class ProductGroupMapping
        {
            public string nodeId;
            public string displayName;
            public string[] productIds;
            public ProductScriptableObject[] products;
            public string[] aliases;
            public string[] titleKeywords;
        }

        public static EntrepreneurTreeProductUnlockAdapter Instance { get; private set; }

        [Header("Optional custom mapping")]
        public List<ProductGroupMapping> customMappings = new List<ProductGroupMapping>();

        private readonly Dictionary<string, HashSet<string>> groupToProductIds = new Dictionary<string, HashSet<string>>();
        private readonly Dictionary<string, string> productIdToGroup = new Dictionary<string, string>();
        private readonly HashSet<string> unlockedGroups = new HashSet<string>();
        private readonly HashSet<string> loggedUnmappedProducts = new HashSet<string>();
        private readonly HashSet<string> loggedLockedProducts = new HashSet<string>();
        private readonly HashSet<string> loggedStarterProducts = new HashSet<string>();
        private bool mappingBuilt;

        private static readonly ProductGroupMapping[] DefaultMappings =
        {
            // This project only ships Product_A..E assets; IDs 0..4 are mapped explicitly to avoid weak-only keyword matching.
            NewGroup("product_basic_1", "Productos Básicos 1", ids: new[] { "0", "1", "2", "3", "4" }, aliases: new[] { "leche", "sal", "agua", "pasta", "azúcar" }, keywords: new [] { "milk", "salt", "water", "pasta", "sugar" }),
            NewGroup("product_basic_2", "Productos Básicos 2", aliases: new[] { "harina", "arroz", "frijoles", "pan", "aceite" }, keywords: new [] { "flour", "rice", "bean", "bread", "oil" }),
            NewGroup("product_basic_3", "Productos Básicos 3", aliases: new[] { "café", "huevo" }, keywords: new [] { "coffee", "egg" }),
            NewGroup("product_dairy_1", "Lácteos 1", aliases: new[] { "cheddar", "yogurt natural", "mantequilla" }, keywords: new [] { "cheddar", "yogurt", "butter" }),
            NewGroup("product_dairy_2", "Lácteos 2", aliases: new[] { "queso americano", "queso crema" }, keywords: new [] { "american cheese", "cream cheese" }),
            NewGroup("product_dairy_3", "Lácteos 3", aliases: new[] { "mozzarella", "parmesano" }, keywords: new [] { "mozzarella", "parmesan" }),
            NewGroup("product_spices_1", "Especias 1", aliases: new[] { "pimienta negra", "canela" }, keywords: new [] { "pepper", "cinnamon" }),
            NewGroup("product_fresh_1", "Productos Frescos 1", aliases: new[] { "manzana", "plátano", "jitomate", "cebolla" }, keywords: new [] { "apple", "banana", "tomato", "onion" }),
            NewGroup("product_fresh_2", "Productos Frescos 2", aliases: new[] { "uvas", "zanahorias", "ajo" }, keywords: new [] { "grape", "carrot", "garlic" }),
            NewGroup("product_hygiene", "Productos de Higiene", aliases: new[] { "jabón", "papel higiénico", "detergente", "pasta de dientes" }, keywords: new [] { "soap", "toilet paper", "detergent", "toothpaste" }),
            NewGroup("product_protein_1", "Proteína 1", aliases: new[] { "res", "pollo", "cerdo", "pescado" }, keywords: new [] { "beef", "chicken", "pork", "fish" }),
            NewGroup("product_sodas", "Sodas", aliases: new[] { "cola", "cola sin azúcar", "refresco de limón" }, keywords: new [] { "cola", "soda", "lemon soda" }),
            NewGroup("product_luxury_1", "Productos de Lujo 1", aliases: new[] { "trufa", "chocolate importado", "caviar" }, keywords: new [] { "truffle", "imported chocolate", "caviar" }),
            NewGroup("product_appliances_1", "Electrodomésticos 1", aliases: new[] { "refrigerador", "microondas", "horno", "licuadora", "mesa", "mesas" }, keywords: new [] { "fridge", "microwave", "oven", "blender", "table" }),
        };

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            EntrepreneurTreeManager.onProductNodeUnlocked += OnProductNodeUnlocked;
            SaveGameSystem.dataLoadEvent += OnDataLoaded;
        }


        void Start()
        {
            BuildMappingIfNeeded();
            RefreshUnlockedGroupsFromTree();
            RefreshVisibleProductItems();
        }


        public bool IsProductUnlocked(ProductScriptableObject product)
        {
            if (product == null)
                return false;

            BuildMappingIfNeeded();

            string groupId;
            if (!productIdToGroup.TryGetValue(product.id, out groupId))
            {
                if (loggedUnmappedProducts.Add(product.id))
                    Debug.Log(LogPrefix + "Product has no mapping: " + product.title + " using fallback: available.");
                return true;
            }

            bool isUnlocked = unlockedGroups.Contains(groupId);
            if (!isUnlocked && loggedLockedProducts.Add(product.id))
                Debug.Log(LogPrefix + "Product locked: " + product.title + " requires " + groupId + ".");
            if (isUnlocked && groupId == EntrepreneurTreeDefinition.DefaultUnlockedNodeId && loggedStarterProducts.Add(product.id))
                Debug.Log(LogPrefix + "Starter product available: " + product.title + "/" + product.id + ".");

            return isUnlocked;
        }


        public string GetLockedGroupForProduct(ProductScriptableObject product)
        {
            if (product == null)
                return string.Empty;

            BuildMappingIfNeeded();
            string groupId;
            if (!productIdToGroup.TryGetValue(product.id, out groupId))
                return string.Empty;

            return unlockedGroups.Contains(groupId) ? string.Empty : groupId;
        }


        public IReadOnlyCollection<string> GetUnlockedGroups()
        {
            return unlockedGroups;
        }


        private static ProductGroupMapping NewGroup(string nodeId, string displayName, string[] ids = null, string[] aliases = null, string[] keywords = null)
        {
            return new ProductGroupMapping
            {
                nodeId = nodeId,
                displayName = displayName,
                productIds = ids ?? Array.Empty<string>(),
                aliases = aliases ?? Array.Empty<string>(),
                titleKeywords = keywords ?? Array.Empty<string>(),
                products = Array.Empty<ProductScriptableObject>()
            };
        }


        private void BuildMappingIfNeeded()
        {
            if (mappingBuilt || ItemDatabase.Instance == null)
                return;

            mappingBuilt = true;
            groupToProductIds.Clear();
            productIdToGroup.Clear();

            List<ProductGroupMapping> allMappings = new List<ProductGroupMapping>(DefaultMappings);
            if (customMappings != null)
                allMappings.AddRange(customMappings);

            List<PurchasableScriptableObject> allProductsRaw = ItemDatabase.GetByType(typeof(ProductScriptableObject));
            List<ProductScriptableObject> allProducts = new List<ProductScriptableObject>();
            for (int i = 0; i < allProductsRaw.Count; i++)
            {
                if (allProductsRaw[i] is ProductScriptableObject product)
                    allProducts.Add(product);
            }

            for (int i = 0; i < allMappings.Count; i++)
            {
                ProductGroupMapping mapping = allMappings[i];
                if (mapping == null || string.IsNullOrEmpty(mapping.nodeId))
                    continue;

                if (!groupToProductIds.ContainsKey(mapping.nodeId))
                    groupToProductIds[mapping.nodeId] = new HashSet<string>();

                HashSet<string> ids = groupToProductIds[mapping.nodeId];

                AddExplicitProducts(mapping, ids);
                AddAliasProducts(mapping, allProducts, ids);
                AddKeywordProducts(mapping, allProducts, ids);

                foreach (string productId in ids)
                    productIdToGroup[productId] = mapping.nodeId;

                Debug.Log(LogPrefix + "Product mapping: " + mapping.nodeId + " -> " + ids.Count + " products found.");
            }

            LogUnmappedProducts(allProducts);
            Debug.Log(LogPrefix + "Product unlock mapping loaded: " + productIdToGroup.Count + " entries.");
        }


        private void AddExplicitProducts(ProductGroupMapping mapping, HashSet<string> ids)
        {
            if (mapping.productIds != null)
            {
                for (int i = 0; i < mapping.productIds.Length; i++)
                {
                    if (!string.IsNullOrEmpty(mapping.productIds[i]))
                        ids.Add(mapping.productIds[i]);
                }
            }

            if (mapping.products != null)
            {
                for (int i = 0; i < mapping.products.Length; i++)
                {
                    ProductScriptableObject product = mapping.products[i];
                    if (product != null && !string.IsNullOrEmpty(product.id))
                        ids.Add(product.id);
                }
            }
        }


        private void AddAliasProducts(ProductGroupMapping mapping, List<ProductScriptableObject> allProducts, HashSet<string> ids)
        {
            if (mapping.aliases == null || mapping.aliases.Length == 0)
                return;

            for (int i = 0; i < mapping.aliases.Length; i++)
            {
                string alias = mapping.aliases[i];
                if (string.IsNullOrEmpty(alias))
                    continue;

                ProductScriptableObject resolved = FindByAlias(alias, allProducts);
                if (resolved == null)
                {
                    Debug.LogWarning(LogPrefix + "Product mapping warning: " + mapping.nodeId + " -> " + alias + " not found.");
                    continue;
                }

                ids.Add(resolved.id);
            }
        }


        private static ProductScriptableObject FindByAlias(string alias, List<ProductScriptableObject> allProducts)
        {
            string normalizedAlias = Normalize(alias);
            for (int i = 0; i < allProducts.Count; i++)
            {
                ProductScriptableObject product = allProducts[i];
                if (product == null)
                    continue;

                if (Normalize(product.title) == normalizedAlias ||
                    Normalize(product.name) == normalizedAlias ||
                    Normalize(product.id) == normalizedAlias)
                    return product;
            }

            return null;
        }


        private static void AddKeywordProducts(ProductGroupMapping mapping, List<ProductScriptableObject> allProducts, HashSet<string> ids)
        {
            if (mapping.titleKeywords == null || mapping.titleKeywords.Length == 0)
                return;

            for (int i = 0; i < allProducts.Count; i++)
            {
                ProductScriptableObject product = allProducts[i];
                if (product == null || string.IsNullOrEmpty(product.title))
                    continue;

                string normalizedTitle = Normalize(product.title);
                for (int k = 0; k < mapping.titleKeywords.Length; k++)
                {
                    string keyword = mapping.titleKeywords[k];
                    if (string.IsNullOrEmpty(keyword))
                        continue;

                    if (normalizedTitle.Contains(Normalize(keyword)))
                    {
                        ids.Add(product.id);
                        break;
                    }
                }
            }
        }


        private static string Normalize(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Trim().ToLowerInvariant();
        }


        private void LogUnmappedProducts(List<ProductScriptableObject> allProducts)
        {
            for (int i = 0; i < allProducts.Count; i++)
            {
                ProductScriptableObject product = allProducts[i];
                if (product == null || string.IsNullOrEmpty(product.id))
                    continue;

                if (!productIdToGroup.ContainsKey(product.id))
                    Debug.LogWarning(LogPrefix + "Product mapping warning: unmapped product id " + product.id + " (" + product.title + ").");
            }
        }


        private void RefreshUnlockedGroupsFromTree()
        {
            unlockedGroups.Clear();
            if (EntrepreneurTreeManager.Instance == null || EntrepreneurTreeManager.Instance.treeData == null)
                return;

            for (int i = 0; i < EntrepreneurTreeManager.Instance.treeData.nodes.Count; i++)
            {
                NodeData node = EntrepreneurTreeManager.Instance.treeData.nodes[i];
                if (node == null || node.nodeType != TreeNodeType.Product)
                    continue;

                if (node.isUnlocked)
                    unlockedGroups.Add(node.id);
            }

            if (unlockedGroups.Contains(EntrepreneurTreeDefinition.DefaultUnlockedNodeId))
                Debug.Log(LogPrefix + "Starter node unlocked: " + EntrepreneurTreeDefinition.DefaultUnlockedNodeId + ".");
        }


        private void OnProductNodeUnlocked(NodeData node)
        {
            if (node == null || string.IsNullOrEmpty(node.id))
                return;

            unlockedGroups.Add(node.id);
            Debug.Log(LogPrefix + "Product gameplay unlock applied for group: " + node.id);
            RefreshVisibleProductItems();
        }


        private void OnDataLoaded()
        {
            BuildMappingIfNeeded();
            RefreshUnlockedGroupsFromTree();
            RefreshVisibleProductItems();
            Debug.Log(LogPrefix + "Product gameplay unlock state reapplied after load.");
        }


        private static void RefreshVisibleProductItems()
        {
            UIShopItemProduct[] items = UnityEngine.Object.FindObjectsByType<UIShopItemProduct>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < items.Length; i++)
            {
                UIShopItemProduct item = items[i];
                if (item == null || item.purchasable == null)
                    continue;

                item.Initialize(item.purchasable);
            }
        }


        void OnDestroy()
        {
            EntrepreneurTreeManager.onProductNodeUnlocked -= OnProductNodeUnlocked;
            SaveGameSystem.dataLoadEvent -= OnDataLoaded;

            if (Instance == this)
                Instance = null;
        }
    }
}
