using UnityEngine;

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

        public static EntrepreneurTreeUpgradeAdapter Instance { get; private set; }

        private bool hasCaffeine;
        private bool hasCharismatic;
        private bool applyingSalesBonus;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            EntrepreneurTreeManager.onUpgradeNodeUnlocked += OnUpgradeNodeUnlocked;
            SaveGameSystem.dataLoadEvent += OnDataLoaded;
            StoreDatabase.onMoneyUpdate += OnMoneyUpdate;
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
            return upgradeId switch
            {
                CaffeineId => hasCaffeine,
                CharismaticId => hasCharismatic,
                _ => false
            };
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
            if (!hasCharismatic || applyingSalesBonus)
                return;

            long income = StoreDatabase.FromStringToLongMoney(changeString);
            if (income <= 0)
                return;

            long bonus = Mathf.FloorToInt(income * (CharismaticMultiplier - BaseMultiplier));
            if (bonus <= 0)
                return;

            applyingSalesBonus = true;
            StoreDatabase.AddRemoveMoney(bonus);
            applyingSalesBonus = false;
        }


        void OnDestroy()
        {
            EntrepreneurTreeManager.onUpgradeNodeUnlocked -= OnUpgradeNodeUnlocked;
            SaveGameSystem.dataLoadEvent -= OnDataLoaded;
            StoreDatabase.onMoneyUpdate -= OnMoneyUpdate;

            if (Instance == this)
                Instance = null;
        }
    }
}
