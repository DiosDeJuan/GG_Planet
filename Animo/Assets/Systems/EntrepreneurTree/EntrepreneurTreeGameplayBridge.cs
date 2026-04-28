using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Central façade for gameplay queries against the entrepreneur tree state.
    /// </summary>
    public class EntrepreneurTreeGameplayBridge : MonoBehaviour
    {
        public static EntrepreneurTreeGameplayBridge Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }


        public bool IsProductGroupUnlocked(string productGroupId)
        {
            if (EntrepreneurTreeManager.Instance?.treeData == null || string.IsNullOrEmpty(productGroupId))
                return false;

            NodeData node = EntrepreneurTreeManager.Instance.treeData.GetNodeById(productGroupId);
            return node != null && node.isUnlocked;
        }


        public bool IsProductUnlocked(ProductScriptableObject product)
        {
            if (product == null)
                return false;

            return EntrepreneurTreeProductUnlockAdapter.Instance == null ||
                   EntrepreneurTreeProductUnlockAdapter.Instance.IsProductUnlocked(product);
        }


        public bool IsEmployeeUnlocked(int employeeNumber)
        {
            return EntrepreneurTreeEmployeeUnlockAdapter.Instance != null &&
                   EntrepreneurTreeEmployeeUnlockAdapter.Instance.IsEmployeeUnlocked(employeeNumber);
        }


        public int GetSecurityLevel()
        {
            return EntrepreneurTreeSecurityAdapter.Instance != null
                ? EntrepreneurTreeSecurityAdapter.Instance.GetSecurityLevel()
                : 0;
        }


        public float GetSecurityArrestChance()
        {
            return EntrepreneurTreeSecurityAdapter.Instance != null
                ? EntrepreneurTreeSecurityAdapter.Instance.GetArrestChance()
                : 0f;
        }


        public float GetEmployeeSpeedMultiplier()
        {
            return EntrepreneurTreeUpgradeAdapter.Instance != null
                ? EntrepreneurTreeUpgradeAdapter.Instance.GetEmployeeSpeedMultiplier()
                : 1f;
        }


        public float GetSalesMultiplier()
        {
            return EntrepreneurTreeUpgradeAdapter.Instance != null
                ? EntrepreneurTreeUpgradeAdapter.Instance.GetSalesMultiplier()
                : 1f;
        }


        public bool HasUpgrade(string upgradeId)
        {
            return EntrepreneurTreeUpgradeAdapter.Instance != null &&
                   EntrepreneurTreeUpgradeAdapter.Instance.HasUpgrade(upgradeId);
        }


        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
