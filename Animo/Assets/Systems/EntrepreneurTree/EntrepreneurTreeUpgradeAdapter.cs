using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Applies entrepreneur tree upgrades to gameplay multipliers.
    /// </summary>
    public class EntrepreneurTreeUpgradeAdapter : MonoBehaviour
    {
        private const string LogPrefix = "[EntrepreneurTree] ";
        private const string CaffeineId = "upgrade_caffeine";
        private const string CharismaticId = "upgrade_charismatic";

        private const float BaseMultiplier = 1f;
        private const float CaffeineMultiplier = 1.10f;
        private const float CharismaticMultiplier = 1.05f;
        private const float MinSelfCheckoutScanDelay = 0.2f;

        public static EntrepreneurTreeUpgradeAdapter Instance { get; private set; }

        [Header("Optional runtime bonus application")]
        [Tooltip("Enabled by default: charismatic applies +5% only on positive money deltas not already boosted by checkout flows.")]
        public bool applyCharismaticBonusOnMoneyEvents = true;

        private bool hasCaffeine;
        private bool hasCharismatic;
        private bool applyingSalesBonus;
        private readonly Dictionary<int, float> baseCheckoutSpeeds = new Dictionary<int, float>();
        private readonly Dictionary<int, float> baseSelfCheckoutScanDelays = new Dictionary<int, float>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            EntrepreneurTreeManager.onUpgradeNodeUnlocked += OnUpgradeNodeUnlocked;
            EntrepreneurEmployeeSystem.onEmployeeRoleChanged += OnEmployeeRoleChanged;
            EntrepreneurEmployeeSystem.onEmployeeHired += OnEmployeeHired;
            SaveGameSystem.dataLoadEvent += OnDataLoaded;
            StoreDatabase.onMoneyUpdate += OnMoneyUpdate;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        void Start()
        {
            RefreshFromTree();
        }


        public float GetEmployeeSpeedMultiplier()
        {
            return hasCaffeine ? CaffeineMultiplier : BaseMultiplier;
        }


        public float GetSalesMultiplier()
        {
            return hasCharismatic ? CharismaticMultiplier : BaseMultiplier;
        }


        public bool HasUpgrade(string upgradeId)
        {
            switch (upgradeId)
            {
                case CaffeineId:
                    return hasCaffeine;
                case CharismaticId:
                    return hasCharismatic;
                default:
                    return false;
            }
        }

        public long ApplySalesBonus(long baseIncome)
        {
            if (baseIncome <= 0 || !hasCharismatic)
                return baseIncome;

            return (long)Math.Floor(baseIncome * CharismaticMultiplier);
        }

        public bool ShouldApplyCharismaticBonusOnMoneyEvents()
        {
            return applyCharismaticBonusOnMoneyEvents && hasCharismatic;
        }

        public void CreditSaleIncome(long baseIncome)
        {
            if (baseIncome <= 0)
                return;

            long finalIncome = ApplySalesBonus(baseIncome);
            applyingSalesBonus = true;
            try
            {
                StoreDatabase.AddRemoveMoney(finalIncome);
            }
            finally
            {
                applyingSalesBonus = false;
            }
        }


        private void OnUpgradeNodeUnlocked(NodeData node)
        {
            RefreshFromTree();
            Debug.Log(LogPrefix + "Upgrade gameplay effect applied: " + node?.id);
        }


        private void OnDataLoaded()
        {
            RefreshFromTree();
            Debug.Log(LogPrefix + "Upgrade gameplay state reapplied after load.");
        }


        private void RefreshFromTree()
        {
            hasCaffeine = IsNodeUnlocked(CaffeineId);
            hasCharismatic = IsNodeUnlocked(CharismaticId);
            ApplyRuntimeSpeedEffects();
            AchievementSystem.RegisterAllUpgradesUnlocked(hasCaffeine && hasCharismatic);
        }


        private static bool IsNodeUnlocked(string nodeId)
        {
            if (EntrepreneurTreeManager.Instance == null || EntrepreneurTreeManager.Instance.treeData == null)
                return false;

            NodeData node = EntrepreneurTreeManager.Instance.treeData.GetNodeById(nodeId);
            return node != null && node.isUnlocked;
        }


        private void OnMoneyUpdate(string current, string changeString)
        {
            if (!ShouldApplyCharismaticBonusOnMoneyEvents() || applyingSalesBonus)
                return;

            long income = StoreDatabase.FromStringToLongMoney(changeString);
            if (income <= 0)
                return;

            long bonus = (long)Math.Floor(income * (CharismaticMultiplier - BaseMultiplier));
            if (bonus <= 0)
                return;

            applyingSalesBonus = true;
            try
            {
                StoreDatabase.AddRemoveMoney(bonus);
            }
            finally
            {
                applyingSalesBonus = false;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyRuntimeSpeedEffects();
        }

        private void OnEmployeeRoleChanged(int employeeId, EmployeeRole role)
        {
            ApplyRuntimeSpeedEffects();
        }

        private void OnEmployeeHired(int employeeId)
        {
            ApplyRuntimeSpeedEffects();
        }

        private void ApplyRuntimeSpeedEffects()
        {
            float speedMultiplier = GetEmployeeSpeedMultiplier();
            float cashierMultiplier = EntrepreneurEmployeeSystem.Instance != null
                ? EntrepreneurEmployeeSystem.Instance.GetCashierSpeedMultiplier()
                : 1f;
            speedMultiplier *= cashierMultiplier;
            CheckoutObject[] checkoutObjects = FindObjectsOfType<CheckoutObject>(true);
            HashSet<int> liveIds = new HashSet<int>();
            for (int i = 0; i < checkoutObjects.Length; i++)
            {
                CheckoutObject checkout = checkoutObjects[i];
                if (checkout == null)
                    continue;

                int id = checkout.GetInstanceID();
                liveIds.Add(id);
                if (!baseCheckoutSpeeds.ContainsKey(id))
                    baseCheckoutSpeeds[id] = checkout.lerpSpeed;

                checkout.lerpSpeed = baseCheckoutSpeeds[id] * speedMultiplier;

                if (checkout is SelfCheckout selfCheckout)
                {
                    if (!baseSelfCheckoutScanDelays.ContainsKey(id))
                        baseSelfCheckoutScanDelays[id] = selfCheckout.scanDelay;

                    selfCheckout.scanDelay = Mathf.Max(MinSelfCheckoutScanDelay, baseSelfCheckoutScanDelays[id] / speedMultiplier);
                }
            }

            PruneStaleRuntimeCaches(baseCheckoutSpeeds, liveIds);
            PruneStaleRuntimeCaches(baseSelfCheckoutScanDelays, liveIds);
        }

        private static void PruneStaleRuntimeCaches(Dictionary<int, float> cache, HashSet<int> liveIds)
        {
            if (cache == null || cache.Count == 0)
                return;

            List<int> stale = new List<int>();
            foreach (int key in cache.Keys)
            {
                if (!liveIds.Contains(key))
                    stale.Add(key);
            }

            for (int i = 0; i < stale.Count; i++)
                cache.Remove(stale[i]);
        }


        void OnDestroy()
        {
            EntrepreneurTreeManager.onUpgradeNodeUnlocked -= OnUpgradeNodeUnlocked;
            EntrepreneurEmployeeSystem.onEmployeeRoleChanged -= OnEmployeeRoleChanged;
            EntrepreneurEmployeeSystem.onEmployeeHired -= OnEmployeeHired;
            SaveGameSystem.dataLoadEvent -= OnDataLoaded;
            StoreDatabase.onMoneyUpdate -= OnMoneyUpdate;
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (Instance == this)
                Instance = null;
        }
    }
}
