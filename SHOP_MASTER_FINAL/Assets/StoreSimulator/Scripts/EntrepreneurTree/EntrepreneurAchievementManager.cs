//Adaptado por POMPIC 20100333
using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    public static class EntrepreneurAchievementManager
    {
        public static event Action onAchievementsChanged;

        private static readonly HashSet<string> completedAchievementIds = new HashSet<string>();
        private static readonly HashSet<string> claimedRewardIds = new HashSet<string>();
        private static readonly Queue<float> recentSaleTimes = new Queue<float>();

        private static int lifetimeSalesCount;
        private static int zeroPriceProductsSold;
        private static int daysPlayed;
        private static int manualArrestsCount;
        private static long totalIncome;
        private static long currentDayIncome;
        private static long bestDailyIncome;

        static EntrepreneurAchievementManager()
        {
            DayCycleSystem.onDayFinished += OnDayFinished;
            UpgradeSystem.onUpgradePurchase += OnUpgradePurchased;
        }

        public static IReadOnlyList<EntrepreneurAchievementDefinition> Definitions => EntrepreneurAchievementDefinitions.Achievements;
        public static int CompletedCount => completedAchievementIds.Count;
        public static int RewardClaimedCount => claimedRewardIds.Count;
        public static int TotalAchievementCount => Definitions.Count;
        public static int ImplementedAchievementCount => Definitions.Count(definition => !definition.IsHook);
        public static int LifetimeSalesCount => lifetimeSalesCount;
        public static long TotalIncome => totalIncome;
        public static long BestDailyIncome => bestDailyIncome;
        public static int DaysPlayed => daysPlayed;
        public static int ManualArrestsCount => manualArrestsCount;

        public static void ResetToDefaults()
        {
            completedAchievementIds.Clear();
            claimedRewardIds.Clear();
            recentSaleTimes.Clear();
            lifetimeSalesCount = 0;
            zeroPriceProductsSold = 0;
            daysPlayed = 0;
            manualArrestsCount = 0;
            totalIncome = 0;
            currentDayIncome = 0;
            bestDailyIncome = 0;
            onAchievementsChanged?.Invoke();
        }

        public static bool IsCompleted(string achievementId)
        {
            return completedAchievementIds.Contains(achievementId);
        }

        public static bool IsRewardClaimed(string achievementId)
        {
            return claimedRewardIds.Contains(achievementId);
        }

        public static string GetStatusText(EntrepreneurAchievementDefinition definition)
        {
            if (definition == null)
                return "Pendiente";

            if (definition.IsHook && !IsCompleted(definition.Id))
                return "Hook";

            if (IsRewardClaimed(definition.Id))
                return "Reclamado";

            if (IsCompleted(definition.Id))
                return "Completado";

            return "Pendiente";
        }

        public static void RegisterSale(long amount, IList<CustomerBagItem> soldItems, bool automaticCashier, bool selfCheckout)
        {
            long positiveAmount = Math.Max(0, amount);
            lifetimeSalesCount++;
            totalIncome += positiveAmount;
            currentDayIncome += positiveAmount;
            bestDailyIncome = Math.Max(bestDailyIncome, currentDayIncome);

            RegisterSaleTiming();
            RegisterSoldProducts(soldItems);
            EvaluateSalesAchievements();
            EvaluateAll();
        }

        public static void RegisterEmployeeStateChanged()
        {
            EvaluateEmployeeAchievements();
            EvaluateAll();
        }

        public static void RegisterManualArrest()
        {
            manualArrestsCount++;
            TryCompleteAchievement("batman");
            onAchievementsChanged?.Invoke();
        }

        public static void RegisterNodeUnlocked(string nodeId)
        {
            EvaluateTreeAchievements();
            onAchievementsChanged?.Invoke();
        }

        public static void EvaluateAll()
        {
            EvaluateSalesAchievements();
            EvaluateEmployeeAchievements();
            EvaluateTreeAchievements();
            EvaluateExpansionAchievements();
            EvaluateDayAchievements();
        }

        public static bool TryAwardKnownAchievement(string achievementId, string displayReason)
        {
            EntrepreneurAchievementDefinition definition = EntrepreneurAchievementDefinitions.Get(achievementId);
            if (definition == null)
                return false;

            return TryCompleteAchievement(achievementId, displayReason);
        }

        public static JSONNode SaveToJSON()
        {
            JSONNode data = new JSONObject();
            data["lifetimeSalesCount"] = lifetimeSalesCount;
            data["zeroPriceProductsSold"] = zeroPriceProductsSold;
            data["daysPlayed"] = daysPlayed;
            data["manualArrestsCount"] = manualArrestsCount;
            data["totalIncome"] = totalIncome;
            data["currentDayIncome"] = currentDayIncome;
            data["bestDailyIncome"] = bestDailyIncome;

            JSONArray completed = new JSONArray();
            foreach (string achievementId in completedAchievementIds.OrderBy(id => id))
                completed.Add(achievementId);
            data["completedAchievementIds"] = completed;

            JSONArray claimed = new JSONArray();
            foreach (string achievementId in claimedRewardIds.OrderBy(id => id))
                claimed.Add(achievementId);
            data["claimedRewardIds"] = claimed;

            return data;
        }

        public static void LoadFromJSON(JSONNode data)
        {
            ResetToDefaults();
            if (data == null || data.Count == 0)
                return;

            lifetimeSalesCount = Mathf.Max(0, data["lifetimeSalesCount"].AsInt);
            zeroPriceProductsSold = Mathf.Max(0, data["zeroPriceProductsSold"].AsInt);
            daysPlayed = Mathf.Max(0, data["daysPlayed"].AsInt);
            manualArrestsCount = Mathf.Max(0, data["manualArrestsCount"].AsInt);
            totalIncome = Math.Max(0, data["totalIncome"].AsLong);
            currentDayIncome = Math.Max(0, data["currentDayIncome"].AsLong);
            bestDailyIncome = Math.Max(0, data["bestDailyIncome"].AsLong);

            JSONArray completed = data["completedAchievementIds"].AsArray;
            for (int i = 0; i < completed.Count; i++)
            {
                string id = completed[i].Value;
                if (EntrepreneurAchievementDefinitions.Get(id) != null)
                    completedAchievementIds.Add(id);
            }

            JSONArray claimed = data["claimedRewardIds"].AsArray;
            for (int i = 0; i < claimed.Count; i++)
            {
                string id = claimed[i].Value;
                if (EntrepreneurAchievementDefinitions.Get(id) != null)
                    claimedRewardIds.Add(id);
            }

            onAchievementsChanged?.Invoke();
        }

        private static void RegisterSaleTiming()
        {
            float now = Time.realtimeSinceStartup;
            recentSaleTimes.Enqueue(now);
            while (recentSaleTimes.Count > 0 && now - recentSaleTimes.Peek() > 60f)
                recentSaleTimes.Dequeue();

            if (recentSaleTimes.Count >= 10)
                TryCompleteAchievement("venta_rapida");
        }

        private static void RegisterSoldProducts(IList<CustomerBagItem> soldItems)
        {
            if (soldItems == null)
                return;

            for (int i = 0; i < soldItems.Count; i++)
            {
                CustomerBagItem item = soldItems[i];
                if (item == null || item.product == null)
                    continue;

                if (item.fixedPrice <= 0)
                    zeroPriceProductsSold += Mathf.Max(0, item.count);

                string nodeId = EntrepreneurTreeDefinitions.GetKnownProductNodeId(item.product);
                if (nodeId == "productos_lujo_1")
                    TryCompleteAchievement("cliente_lujo");
                else if (nodeId == "electrodomesticos_1")
                    TryCompleteAchievement("lindo_hogar");
            }
        }

        private static void EvaluateSalesAchievements()
        {
            if (lifetimeSalesCount >= 15)
                TryCompleteAchievement("primeras_ventas");

            TryCompleteIncomeAchievement("ingresos_1", 5000);
            TryCompleteIncomeAchievement("ingresos_2", 10000);
            TryCompleteIncomeAchievement("ingresos_3", 12000);
            TryCompleteIncomeAchievement("ingresos_4", 15000);
            TryCompleteIncomeAchievement("ingresos_5", 17000);
            TryCompleteIncomeAchievement("ingresos_6", 20000);
            TryCompleteIncomeAchievement("ingresos_7", 25000);
            TryCompleteIncomeAchievement("ingresos_8", 35000);
            TryCompleteIncomeAchievement("ingresos_9", 50000);
            TryCompleteIncomeAchievement("lluvia_dinero", 100000);

            TryCompleteDailyIncomeAchievement("ventas_diarias_1", 1000);
            TryCompleteDailyIncomeAchievement("ventas_diarias_2", 1500);
            TryCompleteDailyIncomeAchievement("ventas_diarias_3", 5000);
            TryCompleteDailyIncomeAchievement("ventas_diarias_4", 10000);
            TryCompleteDailyIncomeAchievement("ventas_diarias_5", 15000);
            TryCompleteDailyIncomeAchievement("ventas_diarias_6", 20000);

            if (zeroPriceProductsSold >= 5)
                TryCompleteAchievement("donador");

            if (currentDayIncome >= 50000 && EmployeeManager.Instance != null && EmployeeManager.Instance.GetHiredEmployeeCount() == 0)
                TryCompleteAchievement("bajo_presion");
        }

        private static void EvaluateEmployeeAchievements()
        {
            if (EmployeeManager.Instance == null)
                return;

            if (EmployeeManager.Instance.GetHiredEmployeeCount() >= 1)
                TryCompleteAchievement("primer_empleado");

            if (EmployeeManager.Instance.GetHiredEmployeeCount() >= 18 && EmployeeManager.Instance.GetHiredEmployeeWithRoleCount() >= 18)
                TryCompleteAchievement("maximo_empleo");
        }

        private static void EvaluateTreeAchievements()
        {
            if (EntrepreneurProgress.IsUnlocked("productos_basicos_1") && EntrepreneurProgress.IsUnlocked("productos_basicos_2") && EntrepreneurProgress.IsUnlocked("productos_basicos_3"))
                TryCompleteAchievement("surtido_completo");

            if (EntrepreneurProgress.IsUnlocked("seguridad_1") && EntrepreneurProgress.IsUnlocked("seguridad_2") && EntrepreneurProgress.IsUnlocked("seguridad_3"))
                TryCompleteAchievement("red_seguridad");

            bool allUpgrades = EntrepreneurProgress.IsUnlocked("mejora_cafeina") && EntrepreneurProgress.IsUnlocked("mejora_carismatico");
            if (allUpgrades)
            {
                TryCompleteAchievement("optimizacion_total");
                TryCompleteAchievement("optimista");
            }

            if (EntrepreneurProgress.IsTreeComplete())
                TryCompleteAchievement("arbol_completo");
        }

        private static void EvaluateExpansionAchievements()
        {
            if (ItemDatabase.Instance == null)
                return;

            List<ExpansionScriptableObject> expansions = ItemDatabase.GetByType(typeof(ExpansionScriptableObject)).OfType<ExpansionScriptableObject>().ToList();
            if (expansions.Count > 0 && expansions.All(expansion => expansion.isPurchased))
                TryCompleteAchievement("imperialista");
        }

        private static void EvaluateDayAchievements()
        {
            if (daysPlayed >= 3)
                TryCompleteAchievement("dedicado");
            if (daysPlayed >= 5)
                TryCompleteAchievement("fiel");
            if (daysPlayed >= 7)
                TryCompleteAchievement("emprendedor");
        }

        private static void TryCompleteIncomeAchievement(string achievementId, int dollars)
        {
            if (totalIncome >= dollars * 100L)
                TryCompleteAchievement(achievementId);
        }

        private static void TryCompleteDailyIncomeAchievement(string achievementId, int dollars)
        {
            if (bestDailyIncome >= dollars * 100L || currentDayIncome >= dollars * 100L)
                TryCompleteAchievement(achievementId);
        }

        private static bool TryCompleteAchievement(string achievementId, string overrideTitle = "")
        {
            EntrepreneurAchievementDefinition definition = EntrepreneurAchievementDefinitions.Get(achievementId);
            if (definition == null || completedAchievementIds.Contains(achievementId))
                return false;

            completedAchievementIds.Add(achievementId);
            if (definition.RewardPoints > 0 && !claimedRewardIds.Contains(achievementId))
            {
                claimedRewardIds.Add(achievementId);
                string title = string.IsNullOrEmpty(overrideTitle) ? definition.Title : overrideTitle;
                EntrepreneurProgress.AddProgressPointsFromAchievement(definition.RewardPoints, achievementId, title);
                ShowNotification("Logro completado: " + title + ". +" + definition.RewardPoints + " punto de progreso.");
            }
            else
            {
                ShowNotification("Logro completado: " + definition.Title + ".");
            }

            onAchievementsChanged?.Invoke();
            return true;
        }

        private static void OnDayFinished()
        {
            daysPlayed++;
            bestDailyIncome = Math.Max(bestDailyIncome, currentDayIncome);
            currentDayIncome = 0;
            EvaluateDayAchievements();
            onAchievementsChanged?.Invoke();
        }

        private static void OnUpgradePurchased(PurchasableScriptableObject purchasable)
        {
            if (purchasable is ExpansionScriptableObject)
                EvaluateExpansionAchievements();
        }

        private static void ShowNotification(string message)
        {
            if (UIGame.Instance != null)
                UIGame.AddNotification(message, otherColor: new Color(1f, 0.78f, 0.25f), otherDuration: 4f);
        }
    }
}
