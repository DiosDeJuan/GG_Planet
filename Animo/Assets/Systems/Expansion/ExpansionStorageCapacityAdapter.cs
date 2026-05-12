using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Tracks purchased storage expansion zones and exposes storage capacity hooks.
    ///
    /// Current behavior:
    ///   - Listens to onZonePurchased and onZonesReset.
    ///   - Exposes GetCurrentStorageM2() and GetStorageCapacityMultiplier() for
    ///     future inventory or restocking systems to consume.
    ///   - Logs each storage expansion purchase so the effect is visible in Console.
    ///
    /// Hook for future integration:
    ///   When a physical inventory capacity system exists (e.g. a maximum shelf/storage
    ///   count), subscribe to OnStorageExpansionChanged and read GetStorageCapacityMultiplier()
    ///   to scale the limit.
    ///
    /// The asset's StoreDatabase does not currently enforce a hard storage limit, so
    /// this adapter intentionally does not modify any StoreDatabase field. It serves as
    /// a clean data layer that can be connected once the design requires it.
    /// </summary>
    public class ExpansionStorageCapacityAdapter : MonoBehaviour
    {
        private const string LogPrefix = "[Expansion] ";

        /// <summary>
        /// Fired whenever the storage capacity multiplier changes.
        /// Consumers (future inventory systems) should re-query GetStorageCapacityMultiplier().
        /// </summary>
        public static System.Action onStorageCapacityChanged;

        void Awake()
        {
            SupermarketExpansionSystem.onZonePurchased += OnZonePurchased;
            SupermarketExpansionSystem.onZonesReset    += OnZonesReset;
        }

        void OnDestroy()
        {
            SupermarketExpansionSystem.onZonePurchased -= OnZonePurchased;
            SupermarketExpansionSystem.onZonesReset    -= OnZonesReset;
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void OnZonePurchased(ExpansionZoneData zone)
        {
            if (zone == null || zone.type != ExpansionZoneType.Storage)
                return;

            LogStorageUpdate();
            onStorageCapacityChanged?.Invoke();
        }

        private void OnZonesReset()
        {
            // Re-report after a load so any subscriber gets a fresh value.
            onStorageCapacityChanged?.Invoke();
        }

        // ── Public query API ──────────────────────────────────────────────────

        /// <summary>
        /// Total purchased storage area in m² (initial + expansions).
        /// </summary>
        public int GetCurrentStorageM2()
        {
            return SupermarketExpansionSystem.Instance != null
                ? SupermarketExpansionSystem.Instance.GetPurchasedStorageAreaM2()
                : 0;
        }

        /// <summary>
        /// Storage capacity multiplier. 1.0 = base, 1.15 = +15 % per storage zone.
        /// Consume this when implementing inventory/restock capacity limits.
        /// </summary>
        public float GetStorageCapacityMultiplier()
        {
            return SupermarketExpansionSystem.Instance != null
                ? SupermarketExpansionSystem.Instance.GetStorageCapacityMultiplier()
                : 1f;
        }

        /// <summary>
        /// Number of paid storage expansion zones purchased.
        /// </summary>
        public int GetPurchasedStorageExpansionCount()
        {
            return SupermarketExpansionSystem.Instance != null
                ? SupermarketExpansionSystem.Instance.GetPurchasedStorageExpansionCount()
                : 0;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void LogStorageUpdate()
        {
            if (SupermarketExpansionSystem.Instance == null)
                return;

            int m2         = SupermarketExpansionSystem.Instance.GetPurchasedStorageAreaM2();
            float mult     = SupermarketExpansionSystem.Instance.GetStorageCapacityMultiplier();
            int expansions = SupermarketExpansionSystem.Instance.GetPurchasedStorageExpansionCount();

            Debug.Log(LogPrefix + "Storage capacity updated: "
                + m2 + " m² total, "
                + expansions + " expansion(s), "
                + "multiplier=" + mult.ToString("F2")
                + ". Hook: connect to inventory system when available.");
        }
    }
}
