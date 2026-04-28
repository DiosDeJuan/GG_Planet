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
            public string[] titleKeywords;
        }

        public static EntrepreneurTreeProductUnlockAdapter Instance { get; private set; }

        [Header("Optional custom mapping")]
        public List<ProductGroupMapping> customMappings = new List<ProductGroupMapping>();

        private readonly Dictionary<string, HashSet<string>> groupToProductIds = new Dictionary<string, HashSet<string>>();
        private readonly Dictionary<string, string> productIdToGroup = new Dictionary<string, string>();
        private readonly HashSet<string> unlockedGroups = new HashSet<string>();
        private bool mappingBuilt;

        private static readonly ProductGroupMapping[] DefaultMappings =
        {
            NewGroup("product_basic_1", "Productos Básicos 1", keywords: new [] { "leche", "milk", "sal", "salt", "agua", "water", "pasta", "azucar", "sugar" }),
            NewGroup("product_basic_2", "Productos Básicos 2", keywords: new [] { "harina", "flour", "arroz", "rice", "frijol", "bean", "pan", "bread", "aceite", "oil" }),
            NewGroup("product_basic_3", "Productos Básicos 3", keywords: new [] { "cafe", "coffee", "huevo", "egg" }),
            NewGroup("product_dairy_1", "Lácteos 1", keywords: new [] { "cheddar", "yogurt", "mantequilla", "butter" }),
            NewGroup("product_dairy_2", "Lácteos 2", keywords: new [] { "americano", "american", "cream cheese", "queso crema" }),
            NewGroup("product_spices_1", "Especias 1", keywords: new [] { "pimienta", "pepper", "canela", "cinnamon" }),
            NewGroup("product_fresh_1", "Productos Frescos 1", keywords: new [] { "manzana", "apple", "banana", "plátano", "platano", "jitomate", "tomato", "cebolla", "onion" }),
            NewGroup("product_fresh_2", "Productos Frescos 2", keywords: new [] { "uva", "grape", "zanahoria", "carrot", "ajo", "garlic" }),
            NewGroup("product_hygiene", "Productos de Higiene", keywords: new [] { "jabon", "soap", "papel", "toilet", "detergente", "toothpaste", "pasta de dientes" }),
            NewGroup("product_protein_1", "Proteína 1", keywords: new [] { "res", "beef", "pollo", "chicken", "cerdo", "pork", "pescado", "fish" }),
            NewGroup("product_sodas", "Sodas", keywords: new [] { "cola", "lemon", "limon", "soda" }),
            NewGroup("product_luxury_1", "Productos de Lujo 1", keywords: new [] { "trufa", "truffle", "chocolate", "caviar" }),
            NewGroup("product_appliances_1", "Electrodomésticos 1", keywords: new [] { "refrigerador", "fridge", "microondas", "microwave", "horno", "oven", "licuadora", "blender" }),
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

            if (!productIdToGroup.TryGetValue(product.id, out string groupId))
                return true;

            return unlockedGroups.Contains(groupId);
        }


        public string GetLockedGroupForProduct(ProductScriptableObject product)
        {
            if (product == null)
                return string.Empty;

            BuildMappingIfNeeded();
            if (!productIdToGroup.TryGetValue(product.id, out string groupId))
                return string.Empty;

            return unlockedGroups.Contains(groupId) ? string.Empty : groupId;
        }


        public IReadOnlyCollection<string> GetUnlockedGroups()
        {
            return unlockedGroups;
        }


        private static ProductGroupMapping NewGroup(string nodeId, string displayName, string[] keywords = null)
        {
            return new ProductGroupMapping
            {
                nodeId = nodeId,
                displayName = displayName,
                titleKeywords = keywords ?? Array.Empty<string>(),
                productIds = Array.Empty<string>(),
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
                AddKeywordProducts(mapping, allProducts, ids);

                foreach (string productId in ids)
                    productIdToGroup[productId] = mapping.nodeId;

                if (ids.Count > 0)
                    Debug.Log(LogPrefix + "Mapped product node " + mapping.nodeId + " to " + ids.Count + " products.");
            }
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


        private static void AddKeywordProducts(ProductGroupMapping mapping, List<ProductScriptableObject> allProducts, HashSet<string> ids)
        {
            if (mapping.titleKeywords == null || mapping.titleKeywords.Length == 0)
                return;

            for (int i = 0; i < allProducts.Count; i++)
            {
                ProductScriptableObject product = allProducts[i];
                if (product == null || string.IsNullOrEmpty(product.title))
                    continue;

                string normalizedTitle = product.title.ToLowerInvariant();
                for (int k = 0; k < mapping.titleKeywords.Length; k++)
                {
                    string keyword = mapping.titleKeywords[k];
                    if (string.IsNullOrEmpty(keyword))
                        continue;

                    if (normalizedTitle.Contains(keyword.ToLowerInvariant()))
                    {
                        ids.Add(product.id);
                        break;
                    }
                }
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
            UIShopItemProduct[] items = FindObjectsByType<UIShopItemProduct>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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
