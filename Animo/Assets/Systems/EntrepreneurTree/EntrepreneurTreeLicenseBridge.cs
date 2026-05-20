using System.Collections.Generic;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Bridges the Entrepreneur Tree's product-node unlocks with the asset's
    /// LicenseScriptableObject system, keeping both in sync.
    ///
    /// Mapping (tree node → license id):
    ///   product_basic_1  → "0"   (License 0 — auto-purchased from game start)
    ///   product_basic_2  → "1"
    ///   product_basic_3  → "2"
    ///   product_dairy_1  → "3"
    ///   (other product nodes have no mapped license — tree is the sole gate)
    ///
    /// When a tree product node is unlocked:
    ///   1. The corresponding LicenseScriptableObject.isPurchased is set to true.
    ///   2. UpgradeSystem.onUpgradePurchase is fired so UIShopCategory refreshes.
    ///   3. UIShopItemLicense instances update their purchased overlay.
    ///
    /// On save/load the license state comes from SaveGameSystem (or ItemDatabase
    /// editor-mode reset), so this bridge only needs to re-apply it at load time.
    /// </summary>
    public class EntrepreneurTreeLicenseBridge : MonoBehaviour
    {
        private const string LogPrefix = "[LicensesSync] ";

        public static EntrepreneurTreeLicenseBridge Instance { get; private set; }

        /// <summary>Maps tree node ID to the license ID it should unlock.</summary>
        private static readonly Dictionary<string, string> NodeToLicenseId =
            new Dictionary<string, string>
            {
                { "product_basic_1", "0" },
                { "product_basic_2", "1" },
                { "product_basic_3", "2" },
                { "product_dairy_1", "3" },
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
            SaveGameSystem.dataLoadEvent                  += OnDataLoaded;
        }

        void Start()
        {
            // Ensure licenses for already-unlocked nodes are marked purchased
            // (handles the product_basic_1 auto-unlock at new-game start).
            SyncAllUnlockedNodes();
        }

        void OnDestroy()
        {
            EntrepreneurTreeManager.onProductNodeUnlocked -= OnProductNodeUnlocked;
            SaveGameSystem.dataLoadEvent                  -= OnDataLoaded;

            if (Instance == this)
                Instance = null;
        }

        // ── Private ──────────────────────────────────────────────────────────

        private void OnProductNodeUnlocked(NodeData node)
        {
            if (node == null)
                return;

            ApplyLicenseForNode(node.id);
        }

        private void OnDataLoaded()
        {
            SyncAllUnlockedNodes();
            Debug.Log(LogPrefix + "License state re-synced after data load.");
        }

        private void SyncAllUnlockedNodes()
        {
            if (EntrepreneurTreeManager.Instance == null ||
                EntrepreneurTreeManager.Instance.treeData == null)
                return;

            List<NodeData> nodes = EntrepreneurTreeManager.Instance.treeData.nodes;
            if (nodes == null)
                return;

            for (int i = 0; i < nodes.Count; i++)
            {
                NodeData node = nodes[i];
                if (node != null && node.isUnlocked)
                    ApplyLicenseForNode(node.id);
            }
        }

        private void ApplyLicenseForNode(string nodeId)
        {
            if (!NodeToLicenseId.TryGetValue(nodeId, out string licenseId))
                return;

            if (ItemDatabase.Instance == null)
                return;

            LicenseScriptableObject license =
                ItemDatabase.GetById(typeof(LicenseScriptableObject), licenseId)
                as LicenseScriptableObject;

            if (license == null)
            {
                Debug.LogWarning(LogPrefix + "License id " + licenseId + " not found in ItemDatabase.");
                return;
            }

            if (license.isPurchased)
                return;  // Already purchased — nothing to do.

            license.isPurchased = true;

            // Fire the UpgradeSystem event so UIShopCategory and UIShopItemLicense
            // react exactly as if the player had purchased the license normally.
            UpgradeSystem.NotifyPurchase(license);

            Debug.Log(LogPrefix + "License " + licenseId + " (" + license.title
                + ") auto-purchased via tree node: " + nodeId + ".");
        }
    }
}
