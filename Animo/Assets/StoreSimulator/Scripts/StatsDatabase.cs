/*  This file is part of the "Store Simulator" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store, Epic FAB).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using UnityEngine;
using SimpleJSON;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Caches certain values from events that happened during the day.
    /// Used in the day over scene to display statistical information to the player.
    /// </summary>
    public class StatsDatabase : MonoBehaviour
    {
        /// <summary>
        /// Returns a reference to this script instance.
        /// </summary>
        public static StatsDatabase Instance { get; private set; }

        /// <summary>
        /// Cache for money earned during the day.
        /// </summary>
        public long moneyEarned { get; private set; }

        /// <summary>
        /// Cache for money spent during the day.
        /// </summary>
        public long moneySpent { get; private set; }

        /// <summary>
        /// Cache for experience earned during the day. If only customers are taken into account,
        /// and customers give 1 XP, then this is the same as customerHappy - customersUnhappy.
        /// </summary>
        public long xpEarned { get; private set; }

        /// <summary>
        /// Cache for count of customers who left happy.
        /// </summary>
        public int customersHappy { get; private set; }

        /// <summary>
        /// Cache for count of customers who left unhappy.
        /// </summary>
        public int customersUnhappy { get; private set; }

        public int thievesAppeared { get; private set; }
        public int thievesDetected { get; private set; }
        public int thievesAutoArrested { get; private set; }
        public int thievesManualArrested { get; private set; }
        public int thievesEscaped { get; private set; }
        public long robberyMoneyLost { get; private set; }
        public int robbedProducts { get; private set; }
        public int recoveredProducts { get; private set; }


        //initialize references
        void Awake()
        {
            Instance = this;

            StoreDatabase.onMoneyUpdate += OnMoneyUpdate;
            StoreDatabase.onExperienceUpdate += OnExperienceUpdate;
            DayCycleSystem.onDayLoaded += OnDayLoaded;
            CustomerSystem.onCustomerLeft += OnCustomerLeft;
        }


        //subscribed to day event
        private void OnDayLoaded()
        {
            moneyEarned = moneySpent = 0;
            xpEarned = 0;
            customersHappy = customersUnhappy = 0;
            thievesAppeared = thievesDetected = thievesAutoArrested = thievesManualArrested = thievesEscaped = 0;
            robberyMoneyLost = 0;
            robbedProducts = recoveredProducts = 0;
        }


        //subscribed to money change
        private void OnMoneyUpdate(string current, string changeString)
        {
            long change = StoreDatabase.FromStringToLongMoney(changeString);

            if (change > 0) moneyEarned += change;
            else moneySpent += change;
        }


        //subscribed to experience change
        private void OnExperienceUpdate(long current, long change)
        {
            //if (change > 0) if only positive values should count
            xpEarned += change;
        }


        //subscribed to customer event
        private void OnCustomerLeft(bool wasHappy)
        {
            if (wasHappy) customersHappy++;
            else customersUnhappy++;
        }


        public static void RegisterThiefAppeared()
        {
            if (Instance == null) return;
            Instance.thievesAppeared++;
        }


        public static void RegisterThiefDetected(long stolenValue, int productCount)
        {
            if (Instance == null) return;
            Instance.thievesDetected++;
            Instance.robbedProducts += Mathf.Max(0, productCount);
        }


        public static void RegisterThiefAutomaticArrest(int recoveredCount)
        {
            if (Instance == null) return;
            Instance.thievesAutoArrested++;
            Instance.recoveredProducts += Mathf.Max(0, recoveredCount);
        }


        public static void RegisterThiefManualArrest(int recoveredCount)
        {
            if (Instance == null) return;
            Instance.thievesManualArrested++;
            Instance.recoveredProducts += Mathf.Max(0, recoveredCount);
        }


        public static void RegisterThiefEscaped(long moneyLost, int productCount)
        {
            if (Instance == null) return;
            Instance.thievesEscaped++;
            Instance.robberyMoneyLost += Mathf.Max(0, moneyLost);
        }


        public static string GetDailyRobberySummary()
        {
            if (Instance == null)
                return string.Empty;

            float effectiveness = 0f;
            int handled = Instance.thievesAutoArrested + Instance.thievesManualArrested;
            int denominator = Mathf.Max(Instance.thievesDetected, handled + Instance.thievesEscaped);
            if (denominator > 0)
                effectiveness = ((float)handled / denominator) * 100f;

            int securityLevel = EntrepreneurTreeGameplayBridge.Instance != null
                ? EntrepreneurTreeGameplayBridge.Instance.GetSecurityLevel()
                : 0;
            float securityChance = EntrepreneurTreeGameplayBridge.Instance != null
                ? EntrepreneurTreeGameplayBridge.Instance.GetSecurityArrestChance() * 100f
                : 0f;

            return "Robos del día:\n" +
                   "- Ladrones detectados: " + Instance.thievesDetected + "\n" +
                   "- Arrestos automáticos: " + Instance.thievesAutoArrested + "\n" +
                   "- Detenidos manualmente: " + Instance.thievesManualArrested + "\n" +
                   "- Escaparon: " + Instance.thievesEscaped + "\n" +
                   "- Pérdida total: " + StoreDatabase.FromLongToStringMoney(Instance.robberyMoneyLost) + "\n" +
                   "- Productos robados: " + Instance.robbedProducts + "\n" +
                   "- Productos recuperados: " + Instance.recoveredProducts + "\n" +
                   "- Efectividad seguridad: " + effectiveness.ToString("0") + "%\n" +
                   "- Seguridad actual: Nivel " + securityLevel + " / " + securityChance.ToString("0") + "%";
        }


        /// <summary>
        /// Reads component data that should be persisted and returns it as a JSONNode. 
        /// </summary>
        public JSONNode SaveToJSON()
        {
            JSONNode data = new JSONObject();
                
            data["moneyEarned"] = moneyEarned;
            data["moneySpent"] = moneySpent;
            data["xpEarned"] = xpEarned;
            data["customersHappy"] = customersHappy;
            data["customersUnhappy"] = customersUnhappy;
            data["thievesAppeared"] = thievesAppeared;
            data["thievesDetected"] = thievesDetected;
            data["thievesAutoArrested"] = thievesAutoArrested;
            data["thievesManualArrested"] = thievesManualArrested;
            data["thievesEscaped"] = thievesEscaped;
            data["robberyMoneyLost"] = robberyMoneyLost;
            data["robbedProducts"] = robbedProducts;
            data["recoveredProducts"] = recoveredProducts;
            
            return data;
        }


        /// <summary>
        /// Applies existing data coming from a JSONNode and overwrites it on this component.
        /// </summary>
        public void LoadFromJSON(JSONNode data)
        {
            if (data == null || data.Count == 0)
                return;

            moneyEarned = data["moneyEarned"].AsLong;
            moneySpent = data["moneySpent"].AsLong;
            xpEarned = data["xpEarned"].AsLong;
            customersHappy = data["customersHappy"].AsInt;
            customersUnhappy = data["customersUnhappy"].AsInt;
            thievesAppeared = data["thievesAppeared"].AsInt;
            thievesDetected = data["thievesDetected"].AsInt;
            thievesAutoArrested = data["thievesAutoArrested"].AsInt;
            thievesManualArrested = data["thievesManualArrested"].AsInt;
            thievesEscaped = data["thievesEscaped"].AsInt;
            robberyMoneyLost = data["robberyMoneyLost"].AsLong;
            robbedProducts = data["robbedProducts"].AsInt;
            recoveredProducts = data["recoveredProducts"].AsInt;
        }


        //unsubscribe from events
        void OnDestroy()
        {
            StoreDatabase.onMoneyUpdate -= OnMoneyUpdate;
            StoreDatabase.onExperienceUpdate -= OnExperienceUpdate;
            DayCycleSystem.onDayLoaded -= OnDayLoaded;
            CustomerSystem.onCustomerLeft -= OnCustomerLeft;
        }
    }
}
