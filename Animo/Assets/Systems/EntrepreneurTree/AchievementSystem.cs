// Árbol del Emprendedor — AchievementSystem
// Tracks in-game milestones and awards 1 progress point to the tree on first completion.
// Attach to the Systems game object in the Game scene alongside EntrepreneurTreeManager.

using System;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// All achievements the player can complete.
    /// Each one awards exactly 1 progress point to the Entrepreneur Tree on first completion.
    /// Add new IDs here as the game grows; existing IDs must not be renamed (save compatibility).
    /// </summary>
    public enum AchievementId
    {
        FirstSale,           // First happy customer
        Revenue1000,         // Earned $1,000 total
        Revenue5000,         // Earned $5,000 total
        Revenue10000,        // Earned $10,000 total
        Revenue12000,        // Earned $12,000 total
        Revenue15000,        // Earned $15,000 total
        Revenue17000,        // Earned $17,000 total
        Revenue20000,        // Earned $20,000 total
        Revenue25000,        // Earned $25,000 total
        Revenue35000,        // Earned $35,000 total
        Revenue50000,        // Earned $50,000 total
        Revenue100000,       // Earned $100,000 total
        DailyRevenue1000,    // Earned $1,000 in a single day
        DailyRevenue1500,    // Earned $1,500 in a single day
        DailyRevenue5000,    // Earned $5,000 in a single day
        DailyRevenue10000,   // Earned $10,000 in a single day
        DailyRevenue15000,   // Earned $15,000 in a single day
        DailyRevenue20000,   // Earned $20,000 in a single day
        Play3Days,           // Survived 3 in-game days
        Play5Days,           // Survived 5 in-game days
        HireFirstEmployee,   // Unlocked first employee node in the tree
        Hire5Employees,      // Unlocked 5 employee nodes
        Hire10Employees,     // Unlocked 10 employee nodes
        ExpandStore,         // Purchased any store expansion
        Play7Days,           // Survived 7 in-game days
        Play30Days,          // Survived 30 in-game days
        UnlockAllProducts,   // Unlocked every product node in the tree
        UnlockAllTree,       // Unlocked every node in the tree
        GoldenEgg,           // Hidden: earn $50,000 in a single day
        TotalOptimization,   // Hidden: unlock every improvement node
        RedSeguridad,        // Unlock security levels 1, 2 and 3
        Batman,              // Detain/capture first thief
        FirstEmployeeHiredReal, // First employee hired from employee app
        MaxEmployment,       // 18 employees hired and assigned
        MaxSupermarket,      // All expansion zones purchased
        MaxStorage,          // All storage expansion zones purchased
        // ── Pricing ──────────────────────────────────────────────────────────────
        Donador,             // Sold 5 products at $0.00 (generous pricing)
        LuxuryProductSold,   // Sold a luxury-category product
        ApplianceProductSold,// Sold an appliance-category product
        // ── Inventory ────────────────────────────────────────────────────────────
        FullStockDay,        // All products stocked on shelves at the same time
        WrongPlacement,      // Placed a product in an incompatible furniture type
    }


    /// <summary>
    /// Tracks which achievements have been completed and awards points on first completion.
    ///
    /// The class hooks into existing game events (money, customers, purchases, day-cycle)
    /// to detect achievements automatically.  You can also call <see cref="Complete"/> manually
    /// from any script to trigger a custom achievement.
    ///
    /// SCENE SETUP:
    ///   Add this component to the same GameObject as EntrepreneurTreeManager.
    ///   No additional Inspector setup is required.
    /// </summary>
    public class AchievementSystem : MonoBehaviour
    {
        private const string LogPrefix = "[Achievement] ";
        /// <summary>Singleton instance.</summary>
        public static AchievementSystem Instance { get; private set; }

        /// <summary>Fired when a new achievement is completed for the first time.</summary>
        public static event Action<AchievementId> onAchievementCompleted;

        // ── Revenue thresholds (in cents: $1 = 100 cents) ─────────────────────────
        private const long ThresholdRevenue1000Cents  =   100000;  // $1,000
        private const long ThresholdRevenue5000Cents  =   500000;  // $5,000
        private const long ThresholdRevenue10000Cents = 1000000;   // $10,000
        private const long ThresholdRevenue12000Cents = 1200000;   // $12,000
        private const long ThresholdRevenue15000Cents = 1500000;   // $15,000
        private const long ThresholdRevenue17000Cents = 1700000;   // $17,000
        private const long ThresholdRevenue20000Cents = 2000000;   // $20,000
        private const long ThresholdRevenue25000Cents = 2500000;   // $25,000
        private const long ThresholdRevenue35000Cents = 3500000;   // $35,000
        private const long ThresholdRevenue50000Cents = 5000000;   // $50,000
        private const long ThresholdRevenue100000Cents = 10000000; // $100,000
        private const long ThresholdDailyRevenue1000Cents = 100000;   // $1,000
        private const long ThresholdDailyRevenue1500Cents = 150000;   // $1,500
        private const long ThresholdDailyRevenue5000Cents = 500000;   // $5,000
        private const long ThresholdDailyRevenue10000Cents = 1000000; // $10,000
        private const long ThresholdDailyRevenue15000Cents = 1500000; // $15,000
        private const long ThresholdDailyRevenue20000Cents = 2000000; // $20,000
        private const long ThresholdGoldenEggCents    = 5000000;   // $50,000 in one day

        // Persisted set of completed achievement IDs.
        private HashSet<AchievementId> completedAchievements = new HashSet<AchievementId>();

        // Running counter for lifetime money earned (in cents) — used for revenue milestones.
        private long lifetimeMoneyEarnedCents = 0;

        // Running counter of in-game days survived.
        private int daysPlayed = 0;

        // Revenue earned in the current day (in cents) — reset on day load, used for GoldenEgg.
        private long dailyRevenue = 0;


        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning(LogPrefix + "Duplicate AchievementSystem detected. Destroying duplicate instance.");
                Destroy(this);
                return;
            }

            Instance = this;

            // Hook into existing game-event bus.
            StoreDatabase.onMoneyUpdate      += OnMoneyUpdate;
            CustomerSystem.onCustomerLeft    += OnCustomerLeft;
            UpgradeSystem.onUpgradePurchase  += OnUpgradePurchase;
            DayCycleSystem.onDayLoaded       += OnDayLoaded;
            DayCycleSystem.onDayFinished     += OnDayFinished;
            EntrepreneurTreeManager.onNodeUnlocked += OnNodeUnlocked;
            SupermarketExpansionSystem.onZonePurchased += OnZonePurchased;
        }


        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Marks an achievement as completed, awarding 1 progress point if not already done.
        /// Safe to call multiple times for the same achievement; duplicate calls are no-ops.
        /// </summary>
        public static void Complete(AchievementId achievement)
        {
            if (Instance == null) return;
            if (Instance.completedAchievements.Contains(achievement)) return;

            Instance.completedAchievements.Add(achievement);
            EntrepreneurTreeManager.AddPoints(1);

            onAchievementCompleted?.Invoke(achievement);
            UIGame.AddNotification("Achievement unlocked:\n" + FormatName(achievement),
                                   otherColor: new Color(1f, 0.8f, 0f));
        }


        /// <summary>Returns whether the specified achievement has already been completed.</summary>
        public static bool IsCompleted(AchievementId achievement)
        {
            return Instance != null && Instance.completedAchievements.Contains(achievement);
        }


        public static void RegisterThiefCaptured()
        {
            Complete(AchievementId.Batman);
        }


        public static void RegisterSecurityCompleted()
        {
            Complete(AchievementId.RedSeguridad);
        }


        public static void RegisterEmployeeHired(int hiredCount, int assignedCount, int maxEmployees)
        {
            if (hiredCount >= 1)
                Complete(AchievementId.FirstEmployeeHiredReal);

            if (hiredCount >= maxEmployees && assignedCount >= maxEmployees)
                Complete(AchievementId.MaxEmployment);
        }


        public static void RegisterAllEmployeesAssigned(int hiredCount, int assignedCount, int maxEmployees)
        {
            if (hiredCount >= maxEmployees && assignedCount >= maxEmployees)
                Complete(AchievementId.MaxEmployment);
        }


        public static void RegisterAllUpgradesUnlocked(bool allUnlocked)
        {
            if (allUnlocked)
                Complete(AchievementId.TotalOptimization);
        }


        /// <summary>
        /// Called by OrdersAppUIController when a product is purchased via DeliverySystem.
        /// Currently used as an extensibility hook; specific product-category achievements
        /// (luxury_sale, appliance_sale) require a pricing/scanner API not yet available.
        /// </summary>
        public static void RegisterProductOrdered(ProductScriptableObject product)
        {
            // Hook: first order placed — no matching AchievementId yet, reserved for future.
            // When a dedicated "first_order" achievement is added to AchievementId, complete it here.
            if (product != null)
                Debug.Log(LogPrefix + "Product ordered: " + product.title + " (hook registered).");
        }

        // ── Pricing hooks ─────────────────────────────────────────────────────────

        // Counter for products sold at $0.  Not persisted via EntrepreneurTreeSaveIntegration
        // because it resets on each session — 5 free sales in one session earns the award.
        private static int zeroSaleCount;

        /// <summary>
        /// Call this when a customer buys a product whose storePrice was $0.
        /// After 5 such sales in the current session, completes AchievementId.Donador.
        /// </summary>
        public static void RegisterProductSoldAtZero(ProductScriptableObject product)
        {
            if (Instance == null) return;
            zeroSaleCount++;
            Debug.Log(LogPrefix + "Free sale registered for '" +
                      (product != null ? product.title : "?") + "' (" + zeroSaleCount + "/5).");
            if (zeroSaleCount >= 5)
                Complete(AchievementId.Donador);
        }

        /// <summary>
        /// Call this when a customer complains that a product is too expensive.
        /// Hook prepared — no dedicated AchievementId yet (Paciente achievement).
        /// </summary>
        public static void RegisterPriceComplaint(ProductScriptableObject product)
        {
            if (product == null) return;
            // Hook: reserved for AchievementId.Paciente when added.
            Debug.Log(LogPrefix + "Price complaint hook: '" + product.title + "'.");
        }

        /// <summary>
        /// Call this when a customer successfully purchases a luxury-category product.
        /// </summary>
        public static void RegisterLuxuryProductSold(ProductScriptableObject product)
        {
            if (Instance == null) return;
            Debug.Log(LogPrefix + "Luxury product sold: '" +
                      (product != null ? product.title : "?") + "'.");
            Complete(AchievementId.LuxuryProductSold);
        }

        /// <summary>
        /// Call this when a customer successfully purchases an appliance-category product.
        /// </summary>
        public static void RegisterApplianceProductSold(ProductScriptableObject product)
        {
            if (Instance == null) return;
            Debug.Log(LogPrefix + "Appliance product sold: '" +
                      (product != null ? product.title : "?") + "'.");
            Complete(AchievementId.ApplianceProductSold);
        }

        // ── Inventory hooks ───────────────────────────────────────────────────

        /// <summary>
        /// Hook: called when all products have shelf stock at the same time.
        /// Completes <see cref="AchievementId.FullStockDay"/> on first occurrence.
        /// </summary>
        public static void RegisterFullStockDay()
        {
            if (Instance == null) return;
            Debug.Log(LogPrefix + "Full stock day detected — completing FullStockDay achievement.");
            Complete(AchievementId.FullStockDay);
        }

        /// <summary>
        /// Hook: called when a product's total stock (boxes + shelf) reaches zero.
        /// Prepared for future "out-of-stock penalty" achievement; no completion now.
        /// </summary>
        public static void RegisterProductOutOfStock(ProductScriptableObject product)
        {
            // Hook prepared. No achievement completion until a real event source is available.
            Debug.Log(LogPrefix + "Product out of stock (hook): '" +
                      (product != null ? product.title : "?") + "'.");
        }

        /// <summary>
        /// Hook: called when a product is placed in an incompatible furniture type.
        /// Completes <see cref="AchievementId.WrongPlacement"/> on first occurrence.
        /// </summary>
        public static void RegisterWrongPlacement(ProductScriptableObject product)
        {
            if (Instance == null) return;
            Debug.Log(LogPrefix + "Wrong placement detected for: '" +
                      (product != null ? product.title : "?") + "' — completing WrongPlacement achievement.");
            Complete(AchievementId.WrongPlacement);
        }


#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static void GrantPointForTesting()
        {
            EntrepreneurTreeManager.AddPoints(1);
            Debug.Log(LogPrefix + "GrantPointForTesting called. +1 point.");
        }

        [ContextMenu("Grant Point For Testing")]
        private void GrantPointForTestingContextMenu()
        {
            GrantPointForTesting();
        }
#endif


        // ── Event handlers — auto-detection ──────────────────────────────────────

        // Money change: track lifetime earnings and daily revenue for milestones.
        private void OnMoneyUpdate(string current, string changeString)
        {
            long change = StoreDatabase.FromStringToLongMoney(changeString);
            if (change <= 0) return;    // spending money, not earning

            lifetimeMoneyEarnedCents += change;
            dailyRevenue             += change;

            if (lifetimeMoneyEarnedCents >= ThresholdRevenue1000Cents)  Complete(AchievementId.Revenue1000);
            if (lifetimeMoneyEarnedCents >= ThresholdRevenue5000Cents)  Complete(AchievementId.Revenue5000);
            if (lifetimeMoneyEarnedCents >= ThresholdRevenue10000Cents) Complete(AchievementId.Revenue10000);
            if (lifetimeMoneyEarnedCents >= ThresholdRevenue12000Cents) Complete(AchievementId.Revenue12000);
            if (lifetimeMoneyEarnedCents >= ThresholdRevenue15000Cents) Complete(AchievementId.Revenue15000);
            if (lifetimeMoneyEarnedCents >= ThresholdRevenue17000Cents) Complete(AchievementId.Revenue17000);
            if (lifetimeMoneyEarnedCents >= ThresholdRevenue20000Cents) Complete(AchievementId.Revenue20000);
            if (lifetimeMoneyEarnedCents >= ThresholdRevenue25000Cents) Complete(AchievementId.Revenue25000);
            if (lifetimeMoneyEarnedCents >= ThresholdRevenue35000Cents) Complete(AchievementId.Revenue35000);
            if (lifetimeMoneyEarnedCents >= ThresholdRevenue50000Cents) Complete(AchievementId.Revenue50000);
            if (lifetimeMoneyEarnedCents >= ThresholdRevenue100000Cents) Complete(AchievementId.Revenue100000);
            if (dailyRevenue >= ThresholdDailyRevenue1000Cents) Complete(AchievementId.DailyRevenue1000);
            if (dailyRevenue >= ThresholdDailyRevenue1500Cents) Complete(AchievementId.DailyRevenue1500);
            if (dailyRevenue >= ThresholdDailyRevenue5000Cents) Complete(AchievementId.DailyRevenue5000);
            if (dailyRevenue >= ThresholdDailyRevenue10000Cents) Complete(AchievementId.DailyRevenue10000);
            if (dailyRevenue >= ThresholdDailyRevenue15000Cents) Complete(AchievementId.DailyRevenue15000);
            if (dailyRevenue >= ThresholdDailyRevenue20000Cents) Complete(AchievementId.DailyRevenue20000);
            if (dailyRevenue             >= ThresholdGoldenEggCents)    Complete(AchievementId.GoldenEgg);
        }


        // Customer leaves: detect first sale.
        private void OnCustomerLeft(bool wasHappy)
        {
            if (wasHappy) Complete(AchievementId.FirstSale);
        }


        // Reset daily revenue at the start of each day.
        private void OnDayLoaded()
        {
            dailyRevenue = 0;
        }


        // Day finished: increment day counter and check day-count milestones.
        private void OnDayFinished()
        {
            daysPlayed++;
            if (daysPlayed >= 3)  Complete(AchievementId.Play3Days);
            if (daysPlayed >= 5)  Complete(AchievementId.Play5Days);
            if (daysPlayed >= 7)  Complete(AchievementId.Play7Days);
            if (daysPlayed >= 30) Complete(AchievementId.Play30Days);
        }


        // Upgrade/expansion purchased: detect store expansion.
        private void OnUpgradePurchase(PurchasableScriptableObject purchasable)
        {
            if (purchasable is ExpansionScriptableObject) Complete(AchievementId.ExpandStore);
        }


        // Zone purchased from SupermarketExpansionSystem: detect expansion milestones.
        private void OnZonePurchased(ExpansionZoneData zone)
        {
            if (zone == null || SupermarketExpansionSystem.Instance == null)
                return;

            Complete(AchievementId.ExpandStore);

            int purchasedCount = SupermarketExpansionSystem.Instance.GetPurchasedZonesCount();
            int totalCount     = SupermarketExpansionSystem.Instance.Zones.Count;
            if (totalCount > 0 && purchasedCount >= totalCount)
                Complete(AchievementId.MaxSupermarket);

            int storageCount    = SupermarketExpansionSystem.Instance.GetPurchasedStorageExpansionCount();
            int storageExpTotal = 0;
            IReadOnlyList<ExpansionZoneData> allZones = SupermarketExpansionSystem.Instance.Zones;
            for (int i = 0; i < allZones.Count; i++)
            {
                ExpansionZoneData z = allZones[i];
                if (z != null && z.type == ExpansionZoneType.Storage && z.price > 0)
                    storageExpTotal++;
            }
            if (storageExpTotal > 0 && storageCount >= storageExpTotal)
                Complete(AchievementId.MaxStorage);
        }


        // Node unlocked in the tree: detect employee / product / full-tree milestones.
        private void OnNodeUnlocked(NodeData node)
        {
            if (EntrepreneurTreeManager.Instance == null ||
                EntrepreneurTreeManager.Instance.treeData == null) return;

            List<NodeData> allNodes = EntrepreneurTreeManager.Instance.treeData.nodes;

            // Count unlocked employees.
            int unlockedEmployees = allNodes.FindAll(
                n => n != null && n.nodeType == TreeNodeType.Employee && n.isUnlocked).Count;
            if (unlockedEmployees >= 1)  Complete(AchievementId.HireFirstEmployee);
            if (unlockedEmployees >= 5)  Complete(AchievementId.Hire5Employees);
            if (unlockedEmployees >= 10) Complete(AchievementId.Hire10Employees);

            // All product nodes unlocked?
            bool allProducts = allNodes.TrueForAll(
                n => n == null || n.nodeType != TreeNodeType.Product || n.isUnlocked);
            if (allProducts) Complete(AchievementId.UnlockAllProducts);

            // All improvement nodes unlocked?
            bool allImprovements = allNodes.TrueForAll(
                n => n == null || n.nodeType != TreeNodeType.Improvement || n.isUnlocked);
            if (allImprovements) Complete(AchievementId.TotalOptimization);

            // Every single node unlocked?
            bool allUnlocked = allNodes.TrueForAll(n => n == null || n.isUnlocked);
            if (allUnlocked) Complete(AchievementId.UnlockAllTree);

            if (EntrepreneurTreeManager.IsNodeUnlocked("security_1") &&
                EntrepreneurTreeManager.IsNodeUnlocked("security_2") &&
                EntrepreneurTreeManager.IsNodeUnlocked("security_3"))
                RegisterSecurityCompleted();
        }


        // ── Persistence ───────────────────────────────────────────────────────────

        /// <summary>Serialises achievement state to JSON for persistence.</summary>
        public JSONNode SaveToJSON()
        {
            JSONNode data = new JSONObject();
            data["lifetimeMoneyEarned"] = lifetimeMoneyEarnedCents;
            data["daysPlayed"]          = daysPlayed;

            JSONArray arr = new JSONArray();
            foreach (AchievementId id in completedAchievements)
                arr.Add(id.ToString());
            data["completed"] = arr;

            return data;
        }


        /// <summary>Restores achievement state from a previously saved JSONNode.</summary>
        public void LoadFromJSON(JSONNode data)
        {
            completedAchievements.Clear();
            lifetimeMoneyEarnedCents = 0;
            daysPlayed          = 0;

            if (data == null || data.Count == 0)
                return;

            lifetimeMoneyEarnedCents = data["lifetimeMoneyEarned"].AsLong;
            daysPlayed          = data["daysPlayed"].AsInt;

            JSONArray arr = data["completed"].AsArray;
            for (int i = 0; i < arr.Count; i++)
            {
                AchievementId id;
                if (Enum.TryParse(arr[i].Value, out id))
                    completedAchievements.Add(id);
            }
        }


        // ── Helpers ───────────────────────────────────────────────────────────────

        // Convert camelCase enum name to a readable string.
        private static string FormatName(AchievementId id)
        {
            string raw = id.ToString();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (char c in raw)
            {
                if (char.IsUpper(c) && sb.Length > 0) sb.Append(' ');
                sb.Append(c);
            }
            return sb.ToString();
        }


        void OnDestroy()
        {
            StoreDatabase.onMoneyUpdate          -= OnMoneyUpdate;
            CustomerSystem.onCustomerLeft        -= OnCustomerLeft;
            UpgradeSystem.onUpgradePurchase      -= OnUpgradePurchase;
            DayCycleSystem.onDayLoaded           -= OnDayLoaded;
            DayCycleSystem.onDayFinished         -= OnDayFinished;
            EntrepreneurTreeManager.onNodeUnlocked -= OnNodeUnlocked;
            SupermarketExpansionSystem.onZonePurchased -= OnZonePurchased;

            if (Instance == this)
                Instance = null;
        }
    }
}
