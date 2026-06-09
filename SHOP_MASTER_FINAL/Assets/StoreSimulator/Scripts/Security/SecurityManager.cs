//Adaptado por POMPIC 20100333
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    public class SecurityManager : MonoBehaviour
    {
        public static SecurityManager Instance { get; private set; }

        public float ManualInterventionWindowSeconds => 8f;

        private readonly List<ShoplifterAgent> activeShoplifters = new List<ShoplifterAgent>();
        private int shopliftersSpawnedToday;
        private int lastKnownSecurityLevel;

        public static SecurityManager EnsureInstance()
        {
            if (Instance != null)
                return Instance;

            GameObject obj = new GameObject("SecurityManager");
            return obj.AddComponent<SecurityManager>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            CustomerSystem.onCustomerSpawned += OnCustomerSpawned;
            DayCycleSystem.onDayLoaded += OnDayLoaded;
            DayCycleSystem.onDayStarted += OnDayStarted;
            DayCycleSystem.onDayFinished += OnDayFinished;
        }

        private void OnDayLoaded()
        {
            activeShoplifters.Clear();
            shopliftersSpawnedToday = 0;
            lastKnownSecurityLevel = EntrepreneurProgress.SecurityLevel;
        }

        private void OnDayStarted()
        {
            lastKnownSecurityLevel = EntrepreneurProgress.SecurityLevel;
            if (lastKnownSecurityLevel >= (int)SecurityLevel.Guards)
                ShowNotification("Guardias de seguridad activos.");
            else if (lastKnownSecurityLevel >= (int)SecurityLevel.Cameras)
                ShowNotification("Camaras de seguridad activas.");
        }

        private void OnDayFinished()
        {
            lastKnownSecurityLevel = EntrepreneurProgress.SecurityLevel;
            if (StatsDatabase.Instance != null)
                StatsDatabase.Instance.RecordSecurityLevel(lastKnownSecurityLevel);
        }

        private void OnCustomerSpawned(Customer customer)
        {
            if (customer == null || !ShouldPromoteToShoplifter())
                return;

            ShoplifterType type = ChooseShoplifterType();
            ShoplifterAgent shoplifter = customer.gameObject.AddComponent<ShoplifterAgent>();
            shoplifter.Configure(this, customer, type);
            activeShoplifters.Add(shoplifter);
            shopliftersSpawnedToday++;
        }

        private bool ShouldPromoteToShoplifter()
        {
            if (DayCycleSystem.GetStoreOpenState() != StoreOpenState.Open)
                return false;

            float chance = ShopExpansionManager.GetShoplifterSpawnChance();
            return Random.value <= chance;
        }

        private ShoplifterType ChooseShoplifterType()
        {
            float expansionProgress = ShopExpansionManager.GetSaleExpansionProgress01();
            bool hasLuxury = EntrepreneurProgress.IsUnlocked("productos_lujo_1") || EntrepreneurProgress.IsUnlocked("electrodomesticos_1");
            if (hasLuxury && Random.value <= Mathf.Lerp(0.1f, 0.35f, expansionProgress))
                return ShoplifterType.Special;

            if (Random.value <= Mathf.Lerp(0.25f, 0.45f, expansionProgress))
                return ShoplifterType.SuspiciousCustomer;

            return ShoplifterType.Common;
        }

        public PlacementObject FindTargetPlacement(ShoplifterType type)
        {
            PlacementObject[] placements = FindObjectsByType<PlacementObject>(FindObjectsSortMode.None);
            List<PlacementObject> stocked = placements.Where(placement => placement != null && !placement.IsEmpty() && placement.product != null).ToList();
            if (stocked.Count == 0)
                return null;

            if (type == ShoplifterType.SuspiciousCustomer)
            {
                List<PlacementObject> overpriced = stocked.Where(placement => placement.product.storePrice > placement.product.marketPrice).ToList();
                if (overpriced.Count > 0)
                    return overpriced[Random.Range(0, overpriced.Count)];
            }

            if (type == ShoplifterType.Special)
                return stocked.OrderByDescending(placement => placement.product.storePrice).FirstOrDefault();

            return stocked[Random.Range(0, stocked.Count)];
        }

        public void OnTheftDetected(ShoplifterAgent agent, ProductScriptableObject product, long value)
        {
            if (agent == null)
                return;

            ShowNotification("Robo detectado: " + (product != null ? product.name : "producto") + " (" + StoreDatabase.FromLongToStringMoney(value) + ")");
        }

        public bool TryAutomaticArrest(ShoplifterAgent agent)
        {
            int level = EntrepreneurProgress.SecurityLevel;
            if (level <= 0)
                return false;

            float chance = level == 1 ? 0.33f : level == 2 ? 0.66f : 0.99f;
            bool success = Random.value <= chance;
            if (StatsDatabase.Instance != null)
                StatsDatabase.Instance.RegisterSecurityAutoArrestAttempt(success);

            if (success && agent != null)
                agent.Arrest(true);

            return success;
        }

        public void RegisterEscapedTheft(long value)
        {
            if (StatsDatabase.Instance != null)
                StatsDatabase.Instance.RegisterTheftEscaped(value);
        }

        public void RegisterArrest(long recoveredValue, bool automatic)
        {
            if (StatsDatabase.Instance == null)
                return;

            if (automatic)
                StatsDatabase.Instance.RegisterAutomaticArrest(recoveredValue);
            else
            {
                StatsDatabase.Instance.RegisterManualArrest(recoveredValue);
                EntrepreneurAchievementManager.RegisterManualArrest();
            }
        }

        public JSONNode SaveToJSON()
        {
            JSONNode data = new JSONObject();
            data["lastKnownSecurityLevel"] = EntrepreneurProgress.SecurityLevel;
            data["shopliftersSpawnedToday"] = shopliftersSpawnedToday;
            return data;
        }

        public void LoadFromJSON(JSONNode data)
        {
            lastKnownSecurityLevel = EntrepreneurProgress.SecurityLevel;
            if (data == null || data.Count == 0)
                return;

            shopliftersSpawnedToday = data["shopliftersSpawnedToday"].AsInt;
            lastKnownSecurityLevel = data["lastKnownSecurityLevel"].AsInt;
        }

        private void ShowNotification(string text)
        {
            if (UIGame.Instance != null)
                UIGame.AddNotification(text);
        }

        void OnDestroy()
        {
            if (Instance != this)
                return;

            CustomerSystem.onCustomerSpawned -= OnCustomerSpawned;
            DayCycleSystem.onDayLoaded -= OnDayLoaded;
            DayCycleSystem.onDayStarted -= OnDayStarted;
            DayCycleSystem.onDayFinished -= OnDayFinished;
        }
    }
}
