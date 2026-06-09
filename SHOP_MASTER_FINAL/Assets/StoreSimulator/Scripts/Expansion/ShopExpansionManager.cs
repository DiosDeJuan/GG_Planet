//Adaptado por POMPIC 20100333
using System;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    public static class ShopExpansionManager
    {
        public const int InitialSaleAreaM2 = 192;
        public const int MaxSaleAreaM2 = 576;
        public const int SaleUnitAreaM2 = 16;
        public const long SaleUnitCost = 175000;
        public const int PhysicalExpansionUnitSize = 6;

        public const int InitialStorageAreaM2 = 32;
        public const int StorageUnitAreaM2 = 32;
        public const int MaxStorageUnits = 4;
        public const long StorageUnitCost = 250000;

        public const float CustomerIncreasePerSaleUnit = 0.15f;
        public const float BaseShoplifterChance = 0.02f;
        public const float MaxShoplifterChance = 0.065f;

        public static event Action onSpacesChanged;

        private static int pendingSaleUnits;
        private static int purchasedStorageUnits;
        private static bool initialized;

        public static int MaxSaleUnits => (MaxSaleAreaM2 - InitialSaleAreaM2) / SaleUnitAreaM2;
        public static int PurchasedSaleUnits => Mathf.Clamp(GetPurchasedPhysicalExpansionUnits() + pendingSaleUnits, 0, MaxSaleUnits);
        public static int PurchasedStorageUnits => Mathf.Clamp(purchasedStorageUnits, 0, MaxStorageUnits);
        public static int CurrentSaleAreaM2 => InitialSaleAreaM2 + PurchasedSaleUnits * SaleUnitAreaM2;
        public static int CurrentStorageAreaM2 => InitialStorageAreaM2 + PurchasedStorageUnits * StorageUnitAreaM2;
        public static int MaxStorageAreaM2 => InitialStorageAreaM2 + MaxStorageUnits * StorageUnitAreaM2;
        public static bool AreAllSaleSpacesPurchased => PurchasedSaleUnits >= MaxSaleUnits;
        public static bool AreAllStorageSpacesPurchased => PurchasedStorageUnits >= MaxStorageUnits;
        public static bool AreAllSpacesPurchased => AreAllSaleSpacesPurchased && AreAllStorageSpacesPurchased;

        static ShopExpansionManager()
        {
            EnsureInitialized();
        }

        public static void EnsureInitialized()
        {
            if (initialized)
                return;

            initialized = true;
            UpgradeSystem.onUpgradePurchase += OnUpgradePurchased;
            StoreDatabase.onLevelUpdate += OnLevelUpdated;
        }

        public static void ResetToDefaults()
        {
            pendingSaleUnits = 0;
            purchasedStorageUnits = 0;
            onSpacesChanged?.Invoke();
        }

        public static bool TryPurchaseSaleSpace(out string message)
        {
            EnsureInitialized();
            if (AreAllSaleSpacesPurchased)
            {
                message = "Expansion maxima de venta alcanzada.";
                return false;
            }

            if (!StoreDatabase.CanPurchase(SaleUnitCost))
            {
                long missing = SaleUnitCost - StoreDatabase.Instance.currentMoney;
                message = "Fondos insuficientes. Faltan " + StoreDatabase.FromLongToStringMoney(missing) + ".";
                return false;
            }

            StoreDatabase.AddRemoveMoney(-SaleUnitCost);
            pendingSaleUnits++;
            TryActivatePendingPhysicalExpansions();
            onSpacesChanged?.Invoke();
            EntrepreneurAchievementManager.EvaluateAll();
            GameEndingService.EvaluateMonopoly();
            message = "Espacio de venta comprado: +" + SaleUnitAreaM2 + " m2.";
            return true;
        }

        public static bool TryPurchaseStorageSpace(out string message)
        {
            EnsureInitialized();
            if (AreAllStorageSpacesPurchased)
            {
                message = "Expansion maxima de almacenamiento alcanzada.";
                return false;
            }

            if (!StoreDatabase.CanPurchase(StorageUnitCost))
            {
                long missing = StorageUnitCost - StoreDatabase.Instance.currentMoney;
                message = "Fondos insuficientes. Faltan " + StoreDatabase.FromLongToStringMoney(missing) + ".";
                return false;
            }

            StoreDatabase.AddRemoveMoney(-StorageUnitCost);
            purchasedStorageUnits++;
            onSpacesChanged?.Invoke();
            EntrepreneurAchievementManager.EvaluateAll();
            GameEndingService.EvaluateMonopoly();
            message = "Espacio de almacenamiento comprado: +" + StorageUnitAreaM2 + " m2.";
            return true;
        }

        public static int GetExpandedCustomerSpawnRate(int baseSpawnRate)
        {
            float multiplier = 1f + PurchasedSaleUnits * CustomerIncreasePerSaleUnit;
            return Mathf.Max(1, Mathf.RoundToInt(baseSpawnRate * multiplier));
        }

        public static float GetSaleExpansionProgress01()
        {
            return MaxSaleUnits <= 0 ? 0f : Mathf.Clamp01(PurchasedSaleUnits / (float)MaxSaleUnits);
        }

        public static float GetShoplifterSpawnChance()
        {
            return Mathf.Lerp(BaseShoplifterChance, MaxShoplifterChance, GetSaleExpansionProgress01());
        }

        public static long GetDailyRentExpense()
        {
            long baseMoney = StoreDatabase.Instance != null ? StoreDatabase.Instance.startMoney : 250000;
            float percent = 0.05f + PurchasedSaleUnits * 0.015f;
            return Mathf.RoundToInt(baseMoney * percent);
        }

        public static long GetDailyElectricityExpense()
        {
            long baseMoney = StoreDatabase.Instance != null ? StoreDatabase.Instance.startMoney : 250000;
            return Mathf.RoundToInt(baseMoney * 0.05f);
        }

        public static JSONNode SaveToJSON()
        {
            JSONNode data = new JSONObject();
            data["pendingSaleUnits"] = pendingSaleUnits;
            data["purchasedStorageUnits"] = purchasedStorageUnits;
            return data;
        }

        public static void LoadFromJSON(JSONNode data)
        {
            pendingSaleUnits = 0;
            purchasedStorageUnits = 0;

            if (data != null && data.Count > 0)
            {
                pendingSaleUnits = Mathf.Clamp(data["pendingSaleUnits"].AsInt, 0, MaxSaleUnits);
                purchasedStorageUnits = Mathf.Clamp(data["purchasedStorageUnits"].AsInt, 0, MaxStorageUnits);
            }

            TryActivatePendingPhysicalExpansions();
            onSpacesChanged?.Invoke();
        }

        private static void OnUpgradePurchased(PurchasableScriptableObject purchasable)
        {
            if (purchasable is not ExpansionScriptableObject)
                return;

            onSpacesChanged?.Invoke();
            EntrepreneurAchievementManager.EvaluateAll();
            GameEndingService.EvaluateMonopoly();
        }

        private static void OnLevelUpdated(int level)
        {
            TryActivatePendingPhysicalExpansions();
        }

        private static void TryActivatePendingPhysicalExpansions()
        {
            while (pendingSaleUnits >= PhysicalExpansionUnitSize)
            {
                ExpansionScriptableObject nextExpansion = GetNextUnlockablePhysicalExpansion();
                if (nextExpansion == null)
                    return;

                pendingSaleUnits -= PhysicalExpansionUnitSize;
                UpgradeSystem.GrantPurchased(nextExpansion);
            }
        }

        private static ExpansionScriptableObject GetNextUnlockablePhysicalExpansion()
        {
            if (ItemDatabase.Instance == null || StoreDatabase.Instance == null)
                return null;

            return ItemDatabase.GetByType(typeof(ExpansionScriptableObject))
                .OfType<ExpansionScriptableObject>()
                .OrderBy(expansion => int.TryParse(expansion.id, out int numericId) ? numericId : int.MaxValue)
                .FirstOrDefault(expansion => !expansion.isPurchased && CanUnlockPhysicalExpansion(expansion));
        }

        private static bool CanUnlockPhysicalExpansion(ExpansionScriptableObject expansion)
        {
            if (expansion == null)
                return false;

            if (StoreDatabase.Instance.currentLevel < expansion.requiredLevel)
                return false;

            if (string.IsNullOrEmpty(expansion.otherRequired))
                return true;

            return ItemDatabase.TryGetById(typeof(ExpansionScriptableObject), expansion.otherRequired, out PurchasableScriptableObject required)
                   && required is ExpansionScriptableObject otherExpansion
                   && otherExpansion.isPurchased;
        }

        private static int GetPurchasedPhysicalExpansionUnits()
        {
            if (ItemDatabase.Instance == null)
                return 0;

            int purchasedPhysicalExpansions = ItemDatabase.GetByType(typeof(ExpansionScriptableObject))
                .OfType<ExpansionScriptableObject>()
                .Count(expansion => expansion.isPurchased);

            return purchasedPhysicalExpansions * PhysicalExpansionUnitSize;
        }
    }
}
