using System.Collections.Generic;
using System;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    [System.Serializable]
    public class RobbedItem
    {
        public int productId;
        public string productName;
        public int quantity;
        public long unitValue;
        public long totalValue;
        public string sourceType;
        public ProductScriptableObject product;
    }

    public interface IRobberyInventoryBridge
    {
        bool TryReserveStolenItems(CustomerCart cart, ShoplifterType thiefType, long targetValue, List<RobbedItem> reservedItems, out long totalValue, out int totalCount);
        bool ConfirmStolenItems(IReadOnlyList<RobbedItem> reservedItems);
        bool RestoreStolenItems(IReadOnlyList<RobbedItem> reservedItems, out int restoredCount, out long restoredValue);
    }

    /// <summary>
    /// Default inventory bridge for theft flow using CustomerCart + PlacementObject stock.
    /// </summary>
    public class EntrepreneurTreeRobberyInventoryBridge : IRobberyInventoryBridge
    {
        private const string LogPrefix = "[EntrepreneurTree] ";

        public bool TryReserveStolenItems(CustomerCart cart, ShoplifterType thiefType, long targetValue, List<RobbedItem> reservedItems, out long totalValue, out int totalCount)
        {
            totalValue = 0;
            totalCount = 0;
            if (reservedItems == null || cart == null || cart.items == null || cart.items.Count == 0)
                return false;

            reservedItems.Clear();
            List<CustomerBagItem> sourceItems = new List<CustomerBagItem>(cart.items);
            if (thiefType == ShoplifterType.Expert || thiefType == ShoplifterType.Special)
                sourceItems.Sort((a, b) => (GetBagItemTotalValue(b).CompareTo(GetBagItemTotalValue(a))));

            for (int i = 0; i < sourceItems.Count; i++)
            {
                CustomerBagItem bagItem = sourceItems[i];
                if (bagItem == null || bagItem.product == null || bagItem.count <= 0)
                    continue;

                long unitValue = ResolveUnitValue(bagItem);
                int quantity = Mathf.Max(1, bagItem.count);
                long itemTotal = unitValue * quantity;

                RobbedItem robbedItem = new RobbedItem
                {
                    productId = bagItem.product.id,
                    productName = bagItem.product.name,
                    quantity = quantity,
                    unitValue = unitValue,
                    totalValue = itemTotal,
                    sourceType = "cart",
                    product = bagItem.product
                };

                reservedItems.Add(robbedItem);
                totalValue += itemTotal;
                totalCount += quantity;

                if (totalValue >= targetValue)
                    break;
            }

            return reservedItems.Count > 0;
        }


        public bool ConfirmStolenItems(IReadOnlyList<RobbedItem> reservedItems)
        {
            return reservedItems != null && reservedItems.Count > 0;
        }


        public bool RestoreStolenItems(IReadOnlyList<RobbedItem> reservedItems, out int restoredCount, out long restoredValue)
        {
            restoredCount = 0;
            restoredValue = 0;
            if (reservedItems == null || reservedItems.Count == 0)
                return false;

            bool restoredAny = false;
            for (int i = 0; i < reservedItems.Count; i++)
            {
                RobbedItem item = reservedItems[i];
                if (item == null || item.product == null || item.quantity <= 0)
                    continue;

                for (int q = 0; q < item.quantity; q++)
                {
                    if (!TryRestoreSingleItem(item.product))
                        continue;

                    restoredAny = true;
                    restoredCount++;
                    restoredValue += item.unitValue;
                }
            }

            return restoredAny;
        }


        private static long GetBagItemTotalValue(CustomerBagItem item)
        {
            if (item == null || item.product == null)
                return 0;

            return ResolveUnitValue(item) * Mathf.Max(1, item.count);
        }


        private static long ResolveUnitValue(CustomerBagItem item)
        {
            if (item == null || item.product == null)
                return 0;

            long unitValue = item.fixedPrice > 0 ? item.fixedPrice : item.product.storePrice;
            if (unitValue <= 0)
                unitValue = item.product.buyPrice;
            return Math.Max(0L, unitValue);
        }


        private static bool TryRestoreSingleItem(ProductScriptableObject product)
        {
            if (product == null || product.prefab == null)
                return false;

            PlacementObject target = FindPlacementForProduct(product);
            if (target == null)
            {
                Debug.LogWarning(LogPrefix + "Unable to restore stolen product due to missing/invalid placement: " + product.name);
                return false;
            }
            if (target.container == null)
            {
                Debug.LogWarning(LogPrefix + "Unable to restore stolen product because placement container is missing: " + product.name);
                return false;
            }
            if (!target.IsPlaceable(product))
            {
                Debug.LogWarning(LogPrefix + "Unable to restore stolen product because selected placement is not placeable: " + product.name);
                return false;
            }

            Vector3 localPosition = target.Add(product);
            Quaternion worldRotation = target.transform.rotation * Quaternion.Euler(0f, target.orientation, 0f);
            Vector3 worldPosition = target.container.TransformPoint(localPosition);
            Object.Instantiate(product.prefab, worldPosition, worldRotation, target.container);
            return true;
        }


        private static PlacementObject FindPlacementForProduct(ProductScriptableObject product)
        {
            PlacementObject[] placements = Object.FindObjectsByType<PlacementObject>(FindObjectsSortMode.None);
            PlacementObject bestExisting = null;
            PlacementObject bestEmpty = null;

            for (int i = 0; i < placements.Length; i++)
            {
                PlacementObject placement = placements[i];
                if (placement == null || !placement.IsPlaceable(product))
                    continue;
                if (placement.storageType != product.storageType)
                    continue;

                if (placement.product == product)
                {
                    if (bestExisting == null || placement.count < bestExisting.count)
                        bestExisting = placement;
                    continue;
                }

                if (placement.product == null && bestEmpty == null)
                    bestEmpty = placement;
            }

            return bestExisting != null ? bestExisting : bestEmpty;
        }
    }
}
