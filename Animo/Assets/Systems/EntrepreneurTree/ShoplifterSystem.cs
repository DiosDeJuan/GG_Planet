using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    [Serializable]
    public class ShoplifterValueRange
    {
        public ShoplifterType type;
        public long minValue = 5000;
        public long maxValue = 20000;
    }

    /// <summary>
    /// Selects thief customers, resolves automatic/manual capture, and records robbery stats.
    /// </summary>
    public class ShoplifterSystem : MonoBehaviour
    {
        private const string LogPrefix = "[EntrepreneurTree] ";

        public static ShoplifterSystem Instance { get; private set; }

        [Header("Spawn probabilities")]
        [Range(0f, 1f)] public float baseThiefChance = 0.02f;
        [Range(0f, 1f)] public float maxThiefChance = 0.065f;
        [Min(1)] public int initialOneInNChance = 25;
        [Range(0f, 3f)] public float expansionDifficultyMultiplier = 1f;

        [Header("Manual capture")]
        [Range(0f, 1f)] public float manualCaptureRewardFraction = 0.15f;

        [Header("Type probabilities")]
        [Range(0f, 1f)] public float suspiciousChance = 0.22f;
        [Range(0f, 1f)] public float expertChance = 0.12f;
        [Range(0f, 1f)] public float fastChance = 0.10f;
        [Range(0f, 1f)] public float specialBaseChance = 0.03f;

        [Header("Theft value ranges (cents)")]
        public List<ShoplifterValueRange> valueRanges = new List<ShoplifterValueRange>
        {
            new ShoplifterValueRange { type = ShoplifterType.Common, minValue = 5000, maxValue = 20000 },
            new ShoplifterValueRange { type = ShoplifterType.Suspicious, minValue = 2500, maxValue = 15000 },
            new ShoplifterValueRange { type = ShoplifterType.Expert, minValue = 15000, maxValue = 50000 },
            new ShoplifterValueRange { type = ShoplifterType.Fast, minValue = 7500, maxValue = 25000 },
            new ShoplifterValueRange { type = ShoplifterType.Special, minValue = 30000, maxValue = 100000 },
        };

        private readonly Dictionary<int, ShoplifterAgent> activeAgents = new Dictionary<int, ShoplifterAgent>();
        private int cumulativeManualCaptures;
        private int cumulativeAutoCaptures;
        private int cumulativeEscapes;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            SaveGameSystem.dataLoadEvent += OnDataLoaded;
            DayCycleSystem.onDayLoaded += OnDayLoaded;
            DayCycleSystem.onDayFinished += OnDayFinished;
        }


        public void RegisterCustomer(Customer customer)
        {
            if (customer == null)
                return;

            int id = customer.GetInstanceID();
            if (activeAgents.ContainsKey(id))
                return;

            if (!ShouldBecomeThief())
                return;

            ShoplifterAgent agent = customer.GetComponent<ShoplifterAgent>();
            if (agent == null)
                agent = customer.gameObject.AddComponent<ShoplifterAgent>();

            ShoplifterType type = ChooseThiefType();
            agent.Initialize(this, customer, type);
            activeAgents[id] = agent;
            StatsDatabase.RegisterThiefAppeared();
            UIGame.AddNotification("¡Ladrón detectado!", otherColor: new Color(0.92f, 0.16f, 0.16f));
            Debug.Log(LogPrefix + "Assigned thief type " + type + " to customer " + id);
        }


        public bool TryBeginTheft(Customer customer, CustomerCart cart)
        {
            if (customer == null)
                return false;

            ShoplifterAgent agent;
            if (!activeAgents.TryGetValue(customer.GetInstanceID(), out agent) || agent == null)
                return false;

            return agent.TryStartTheft(cart);
        }


        public void OnCustomerReachedExit(Customer customer)
        {
            if (customer == null)
                return;

            int id = customer.GetInstanceID();
            ShoplifterAgent agent;
            if (!activeAgents.TryGetValue(id, out agent) || agent == null)
            {
                activeAgents.Remove(id);
                return;
            }

            if (!agent.isResolved && agent.isEscaping)
                agent.ResolveAsEscaped();

            activeAgents.Remove(id);
        }


        public bool TryAutomaticArrest(ShoplifterAgent agent)
        {
            if (agent == null || EntrepreneurTreeSecurityAdapter.Instance == null)
                return false;

            return EntrepreneurTreeSecurityAdapter.Instance.TryAutomaticArrest();
        }


        public void NotifyThiefDetected(ShoplifterAgent agent)
        {
            if (agent == null)
                return;

            StatsDatabase.RegisterThiefDetected(agent.stolenValue, agent.stolenProductsCount);
            UIGame.AddNotification("¡Ladrón detectado! Valor objetivo: " + StoreDatabase.FromLongToStringMoney(agent.stolenValue),
                otherColor: new Color(1f, 0.28f, 0.18f));
        }


        public void NotifyAutomaticArrest(ShoplifterAgent agent)
        {
            if (agent == null)
                return;

            cumulativeAutoCaptures++;
            StatsDatabase.RegisterThiefAutomaticArrest(agent.stolenProductsCount);
            AchievementSystem.RegisterThiefCaptured();
            UIGame.AddNotification("¡Ladrón arrestado automáticamente!", otherColor: new Color(0.2f, 0.8f, 0.24f));
        }


        public void NotifyManualArrest(ShoplifterAgent agent)
        {
            if (agent == null)
                return;

            cumulativeManualCaptures++;
            long reward = (long)Mathf.Floor(agent.stolenValue * manualCaptureRewardFraction);
            if (reward > 0)
                StoreDatabase.AddRemoveMoney(reward);

            StatsDatabase.RegisterThiefManualArrest(agent.stolenProductsCount);
            AchievementSystem.RegisterThiefCaptured();
            UIGame.AddNotification("¡Ladrón detenido por el jugador!", otherColor: new Color(0.2f, 0.82f, 0.28f));
        }


        public void NotifyEscaped(ShoplifterAgent agent)
        {
            if (agent == null)
                return;

            cumulativeEscapes++;
            if (agent.stolenValue > 0)
                StoreDatabase.AddRemoveMoney(-agent.stolenValue);

            StatsDatabase.RegisterThiefEscaped(agent.stolenValue, agent.stolenProductsCount);
            UIGame.AddNotification("Un ladrón escapó con " + StoreDatabase.FromLongToStringMoney(agent.stolenValue) + " en productos.",
                otherColor: new Color(0.75f, 0.15f, 0.12f));
        }


        public long GetTargetStealValue(ShoplifterType type)
        {
            for (int i = 0; i < valueRanges.Count; i++)
            {
                if (valueRanges[i] == null || valueRanges[i].type != type)
                    continue;

                long min = Mathf.Max(0, (int)valueRanges[i].minValue);
                long max = Mathf.Max(min, (int)valueRanges[i].maxValue);
                if (max <= min)
                    return min;

                return UnityEngine.Random.Range((int)min, (int)max + 1);
            }

            return 10000;
        }


        public Color GetVisualColor(ShoplifterType type)
        {
            switch (type)
            {
                case ShoplifterType.Common:
                    return new Color(0.85f, 0.23f, 0.23f, 1f);
                case ShoplifterType.Suspicious:
                    return new Color(0.82f, 0.75f, 0.26f, 1f);
                case ShoplifterType.Expert:
                    return new Color(0.74f, 0.15f, 0.68f, 1f);
                case ShoplifterType.Fast:
                    return new Color(0.93f, 0.45f, 0.20f, 1f);
                case ShoplifterType.Special:
                    return new Color(0.92f, 0.13f, 0.47f, 1f);
                default:
                    return Color.red;
            }
        }


        public JSONNode SaveToJSON()
        {
            JSONNode data = new JSONObject();
            data["manualCaptures"] = cumulativeManualCaptures;
            data["autoCaptures"] = cumulativeAutoCaptures;
            data["escapes"] = cumulativeEscapes;
            return data;
        }


        public void LoadFromJSON(JSONNode data)
        {
            cumulativeManualCaptures = 0;
            cumulativeAutoCaptures = 0;
            cumulativeEscapes = 0;

            if (data == null || data.Count == 0)
                return;

            cumulativeManualCaptures = data["manualCaptures"].AsInt;
            cumulativeAutoCaptures = data["autoCaptures"].AsInt;
            cumulativeEscapes = data["escapes"].AsInt;
        }


        private bool ShouldBecomeThief()
        {
            float oneInNChance = initialOneInNChance > 0 ? 1f / initialOneInNChance : 0f;
            float scaledChance = Mathf.Clamp(CalculateScaledChance(), 0f, maxThiefChance);
            float chance = Mathf.Clamp(Mathf.Max(oneInNChance, scaledChance), 0f, 1f);
            return UnityEngine.Random.value <= chance;
        }


        private float CalculateScaledChance()
        {
            float dayFactor = DayCycleSystem.Instance != null ? DayCycleSystem.Instance.currentDay / 100f : 0f;
            float levelFactor = StoreDatabase.Instance != null ? StoreDatabase.Instance.currentLevel / 25f : 0f;
            float moneyFactor = 0f;
            if (StoreDatabase.Instance != null)
            {
                long baseline = 250000;
                long dynamicMoney = Math.Max(0L, StoreDatabase.Instance.currentMoney - baseline);
                moneyFactor = dynamicMoney / 15000000f;
            }

            float combined = Mathf.Clamp01((dayFactor + levelFactor + moneyFactor) * expansionDifficultyMultiplier);
            return Mathf.Lerp(baseThiefChance, maxThiefChance, combined);
        }


        private ShoplifterType ChooseThiefType()
        {
            float specialChance = specialBaseChance;
            if (DayCycleSystem.Instance != null && DayCycleSystem.Instance.currentDay > 0)
                specialChance += Mathf.Floor(DayCycleSystem.Instance.currentDay / 10f) * 0.01f;
            specialChance = Mathf.Clamp01(specialChance);

            float roll = UnityEngine.Random.value;
            if (roll <= specialChance)
                return ShoplifterType.Special;

            roll -= specialChance;
            if (roll <= expertChance)
                return ShoplifterType.Expert;

            roll -= expertChance;
            if (roll <= fastChance)
                return ShoplifterType.Fast;

            roll -= fastChance;
            if (roll <= suspiciousChance)
                return ShoplifterType.Suspicious;

            return ShoplifterType.Common;
        }


        private void OnDataLoaded()
        {
            activeAgents.Clear();
        }

        private void OnDayLoaded()
        {
            activeAgents.Clear();
        }

        private void OnDayFinished()
        {
            string summary = StatsDatabase.GetDailyRobberySummary();
            if (!string.IsNullOrEmpty(summary))
                UIGame.AddNotification(summary, otherColor: new Color(0.98f, 0.88f, 0.28f));
        }


        void OnDestroy()
        {
            SaveGameSystem.dataLoadEvent -= OnDataLoaded;
            DayCycleSystem.onDayLoaded -= OnDayLoaded;
            DayCycleSystem.onDayFinished -= OnDayFinished;
            if (Instance == this)
                Instance = null;
        }
    }
}
