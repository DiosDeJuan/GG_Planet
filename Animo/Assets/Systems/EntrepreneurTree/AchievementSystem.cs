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
        private const string LogPrefix = "[EntrepreneurTree] ";
        /// <summary>Singleton instance.</summary>
        public static AchievementSystem Instance { get; private set; }

        /// <summary>Fired when a new achievement is completed for the first time.</summary>
        public static event Action<AchievementId> onAchievementCompleted;

        // ── Revenue thresholds (in cents: $1 = 100 cents) ─────────────────────────
        private const long ThresholdRevenue1000Cents  =   100_000;  // $1,000
        private const long ThresholdRevenue5000Cents  =   500_000;  // $5,000
        private const long ThresholdRevenue10000Cents = 1_000_000;  // $10,000
        private const long ThresholdGoldenEggCents    = 5_000_000;  // $50,000 in one day

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
                Destroy(gameObject);
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


#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static void GrantPointForTesting()
        {
            EntrepreneurTreeManager.AddPoints(1);
            Debug.Log(LogPrefix + "GrantPointForTesting called. +1 point.");
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
            if (daysPlayed >= 7)  Complete(AchievementId.Play7Days);
            if (daysPlayed >= 30) Complete(AchievementId.Play30Days);
        }


        // Upgrade/expansion purchased: detect store expansion.
        private void OnUpgradePurchase(PurchasableScriptableObject purchasable)
        {
            if (purchasable is ExpansionScriptableObject) Complete(AchievementId.ExpandStore);
        }


        // Node unlocked in the tree: detect employee / product / full-tree milestones.
        private void OnNodeUnlocked(NodeData node)
        {
            if (EntrepreneurTreeManager.Instance == null ||
                EntrepreneurTreeManager.Instance.treeData == null) return;

            List<NodeData> allNodes = EntrepreneurTreeManager.Instance.treeData.nodes;

            // Count unlocked employees.
            int unlockedEmployees = allNodes.FindAll(
                n => n.nodeType == TreeNodeType.Employee && n.isUnlocked).Count;
            if (unlockedEmployees >= 1)  Complete(AchievementId.HireFirstEmployee);
            if (unlockedEmployees >= 5)  Complete(AchievementId.Hire5Employees);
            if (unlockedEmployees >= 10) Complete(AchievementId.Hire10Employees);

            // All product nodes unlocked?
            bool allProducts = allNodes.TrueForAll(
                n => n.nodeType != TreeNodeType.Product || n.isUnlocked);
            if (allProducts) Complete(AchievementId.UnlockAllProducts);

            // All improvement nodes unlocked?
            bool allImprovements = allNodes.TrueForAll(
                n => n.nodeType != TreeNodeType.Improvement || n.isUnlocked);
            if (allImprovements) Complete(AchievementId.TotalOptimization);

            // Every single node unlocked?
            bool allUnlocked = allNodes.TrueForAll(n => n.isUnlocked);
            if (allUnlocked) Complete(AchievementId.UnlockAllTree);
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
                if (Enum.TryParse(arr[i].Value, out AchievementId id))
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

            if (Instance == this)
                Instance = null;
        }
    }
}
