using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Connects supermarket expansion purchases to the customer spawn rate.
    ///
    /// How it works:
    ///   - On Awake, captures CustomerSystem.spawnRate as the "base rate" before
    ///     any expansion bonuses are applied.
    ///   - Each time a Sales-type expansion zone is purchased, the spawn rate is
    ///     recalculated as:  baseRate × GetCustomerCapacityMultiplier()
    ///   - The same recalculation runs after a save-file is loaded, so persisted
    ///     zone purchases are reflected on next session without double-deducting money.
    ///   - If CustomerSystem is not present in the scene, a single warning is logged
    ///     and the adapter becomes a no-op so it never breaks gameplay.
    ///
    /// Tune the per-zone bonus via SupermarketExpansionSystem.SalesExpansionCustomerBonusPercent.
    /// </summary>
    public class ExpansionCustomerDemandAdapter : MonoBehaviour
    {
        private const string LogPrefix = "[Expansion] ";

        /// <summary>
        /// Hard cap on the customer spawn rate after expansion scaling.
        /// Set to 0 to disable the cap.
        /// With 14 purchasable Sales zones at the default 10% bonus each,
        /// the multiplier tops out at 1 + 14×0.10 = 2.4×.
        /// A default base rate of ~50 customers/min × 2.4 = 120 → this cap
        /// prevents runaway values if baseRate is set higher in the Inspector.
        /// </summary>
        [Tooltip("Maximum customer spawn rate after expansion scaling (0 = no cap).")]
        public int maxSpawnRate = 120;

        private int baseSpawnRate = -1;
        private bool warnedOnce;

        void Awake()
        {
            SupermarketExpansionSystem.onZonePurchased += OnZonePurchased;
            SupermarketExpansionSystem.onZonesReset    += OnZonesReset;
            SaveGameSystem.dataLoadEvent               += OnDataLoaded;
        }

        void Start()
        {
            // Capture base rate from the inspector-configured CustomerSystem value.
            // Start() runs after Awake(), so CustomerSystem.Instance should exist.
            CaptureBaseRate();
        }

        void OnDestroy()
        {
            SupermarketExpansionSystem.onZonePurchased -= OnZonePurchased;
            SupermarketExpansionSystem.onZonesReset    -= OnZonesReset;
            SaveGameSystem.dataLoadEvent               -= OnDataLoaded;
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void OnZonePurchased(ExpansionZoneData zone)
        {
            if (zone == null || zone.type != ExpansionZoneType.Sales)
                return;

            ApplySpawnRateBonus();
        }

        private void OnZonesReset()
        {
            // Called after a save file is loaded — re-apply all purchased zone bonuses.
            ApplySpawnRateBonus();
        }

        private void OnDataLoaded()
        {
            // Extra safety: re-apply after data load finishes.
            ApplySpawnRateBonus();
        }

        // ── Core logic ────────────────────────────────────────────────────────

        private void CaptureBaseRate()
        {
            if (CustomerSystem.Instance == null)
            {
                WarnOnce();
                return;
            }

            // Only capture once — baseSpawnRate stays fixed as the "clean" rate.
            if (baseSpawnRate < 0)
                baseSpawnRate = CustomerSystem.Instance.spawnRate;
        }

        private void ApplySpawnRateBonus()
        {
            if (CustomerSystem.Instance == null)
            {
                WarnOnce();
                return;
            }

            // Ensure we have a baseline captured.
            if (baseSpawnRate < 0)
                CaptureBaseRate();
            if (baseSpawnRate < 0)
                return;

            if (SupermarketExpansionSystem.Instance == null)
                return;

            float multiplier = SupermarketExpansionSystem.Instance.GetCustomerCapacityMultiplier();
            int newRate = Mathf.Max(1, Mathf.RoundToInt(baseSpawnRate * multiplier));
            if (maxSpawnRate > 0)
                newRate = Mathf.Min(newRate, maxSpawnRate);

            if (CustomerSystem.Instance.spawnRate == newRate)
                return; // nothing changed, avoid spammy logs

            CustomerSystem.Instance.spawnRate = newRate;
            int expansionCount = SupermarketExpansionSystem.Instance.GetPurchasedSalesExpansionCount();
            Debug.Log(LogPrefix + "Customer spawn rate updated to " + newRate
                + " (base=" + baseSpawnRate
                + ", expansions=" + expansionCount
                + ", multiplier=" + multiplier.ToString("F2") + ").");
        }

        private void WarnOnce()
        {
            if (warnedOnce)
                return;

            warnedOnce = true;
            Debug.LogWarning(LogPrefix
                + "CustomerSystem integration pending: no CustomerSystem.Instance found. "
                + "Customer demand bonuses will not be applied.");
        }

        // ── Public query API (for UI) ─────────────────────────────────────────

        /// <summary>
        /// Returns the current customer spawn rate including expansion bonuses,
        /// or -1 if CustomerSystem is not available.
        /// </summary>
        public int GetCurrentSpawnRate()
        {
            return CustomerSystem.Instance != null ? CustomerSystem.Instance.spawnRate : -1;
        }

        /// <summary>Base spawn rate before any expansion bonuses.</summary>
        public int GetBaseSpawnRate() => baseSpawnRate;
    }
}
