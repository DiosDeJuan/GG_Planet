/*  This file is part of the "Store Simulator" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store, Epic FAB).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using System.Collections.Generic;
using System.Text;
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
        public long robberyValueRecovered { get; private set; }
        public int robbedProducts { get; private set; }
        public int recoveredProducts { get; private set; }
        public int employeesHired { get; private set; }
        public int employeesCashier { get; private set; }
        public int employeesRestocker { get; private set; }

        private readonly Dictionary<string, int> robbedProductsByName = new Dictionary<string, int>();
        private readonly Dictionary<string, int> recoveredProductsByName = new Dictionary<string, int>();


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
            robberyMoneyLost = robberyValueRecovered = 0;
            robbedProducts = recoveredProducts = 0;
            robbedProductsByName.Clear();
            recoveredProductsByName.Clear();
            RefreshEmployeeSnapshot();
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


        public static void RegisterThiefDetected(long stolenValue, int productCount, IReadOnlyList<RobbedItem> items = null)
        {
            if (Instance == null) return;
            Instance.thievesDetected++;
            Instance.robbedProducts += Mathf.Max(0, productCount);
            Instance.AccumulateItemDictionary(Instance.robbedProductsByName, items, false);
        }


        public static void RegisterThiefAutomaticArrest(int recoveredCount, long recoveredValue, IReadOnlyList<RobbedItem> items = null)
        {
            if (Instance == null) return;
            Instance.thievesAutoArrested++;
            Instance.recoveredProducts += Mathf.Max(0, recoveredCount);
            Instance.robberyValueRecovered += Mathf.Max(0, recoveredValue);
            Instance.AccumulateItemDictionary(Instance.recoveredProductsByName, items, true);
        }


        public static void RegisterThiefManualArrest(int recoveredCount, long recoveredValue, IReadOnlyList<RobbedItem> items = null)
        {
            if (Instance == null) return;
            Instance.thievesManualArrested++;
            Instance.recoveredProducts += Mathf.Max(0, recoveredCount);
            Instance.robberyValueRecovered += Mathf.Max(0, recoveredValue);
            Instance.AccumulateItemDictionary(Instance.recoveredProductsByName, items, true);
        }


        public static void RegisterThiefEscaped(long moneyLost, int productCount, IReadOnlyList<RobbedItem> items = null)
        {
            if (Instance == null) return;
            Instance.thievesEscaped++;
            Instance.robberyMoneyLost += Mathf.Max(0, moneyLost);
            if (items != null && items.Count > 0)
                Instance.AccumulateItemDictionary(Instance.robbedProductsByName, items, false);
        }


        public static string GetDailyRobberySummary()
        {
            if (Instance == null)
                return string.Empty;

            Instance.RefreshEmployeeSnapshot();

            float effectiveness = 0f;
            int handled = Instance.thievesAutoArrested + Instance.thievesManualArrested;
            int denominator = handled + Instance.thievesEscaped;
            if (denominator <= 0)
                denominator = Instance.thievesDetected;
            if (denominator > 0)
                effectiveness = ((float)handled / denominator) * 100f;

            int securityLevel = EntrepreneurTreeGameplayBridge.Instance != null
                ? EntrepreneurTreeGameplayBridge.Instance.GetSecurityLevel()
                : 0;
            float securityChance = EntrepreneurTreeGameplayBridge.Instance != null
                ? EntrepreneurTreeGameplayBridge.Instance.GetSecurityArrestChance() * 100f
                : 0f;

            return "Robos del día:\n" +
                   "- Ladrones aparecidos: " + Instance.thievesAppeared + "\n" +
                   "- Ladrones detectados: " + Instance.thievesDetected + "\n" +
                   "- Arrestos automáticos: " + Instance.thievesAutoArrested + "\n" +
                   "- Detenidos manualmente: " + Instance.thievesManualArrested + "\n" +
                   "- Escaparon: " + Instance.thievesEscaped + "\n" +
                   "- Pérdida total: " + StoreDatabase.FromLongToStringMoney(Instance.robberyMoneyLost) + "\n" +
                   "- Valor recuperado: " + StoreDatabase.FromLongToStringMoney(Instance.robberyValueRecovered) + "\n" +
                   "- Productos robados: " + Instance.robbedProducts + "\n" +
                   "- Productos recuperados: " + Instance.recoveredProducts + "\n" +
                   Instance.BuildProductSummaryLine() +
                   "- Efectividad seguridad: " + effectiveness.ToString("0") + "%\n" +
                   "- Seguridad actual: Nivel " + securityLevel + " / " + securityChance.ToString("0") + "%\n\n" +
                   "Empleados:\n" +
                   "- Contratados: " + Instance.employeesHired + "/" + EntrepreneurEmployeeSystem.MaxEmployees + "\n" +
                   "- Cajeros: " + Instance.employeesCashier + "\n" +
                   "- Surtidores: " + Instance.employeesRestocker;
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
            data["robberyValueRecovered"] = robberyValueRecovered;
            data["robbedProducts"] = robbedProducts;
            data["recoveredProducts"] = recoveredProducts;
            data["employeesHired"] = employeesHired;
            data["employeesCashier"] = employeesCashier;
            data["employeesRestocker"] = employeesRestocker;
            data["robbedProductsByName"] = SerializeDictionary(robbedProductsByName);
            data["recoveredProductsByName"] = SerializeDictionary(recoveredProductsByName);
            
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
            robberyValueRecovered = data["robberyValueRecovered"].AsLong;
            robbedProducts = data["robbedProducts"].AsInt;
            recoveredProducts = data["recoveredProducts"].AsInt;
            employeesHired = data["employeesHired"].AsInt;
            employeesCashier = data["employeesCashier"].AsInt;
            employeesRestocker = data["employeesRestocker"].AsInt;
            DeserializeDictionary(data["robbedProductsByName"].AsArray, robbedProductsByName);
            DeserializeDictionary(data["recoveredProductsByName"].AsArray, recoveredProductsByName);
        }


        private void RefreshEmployeeSnapshot()
        {
            if (EntrepreneurEmployeeSystem.Instance == null)
            {
                employeesHired = employeesCashier = employeesRestocker = 0;
                return;
            }

            employeesHired = EntrepreneurEmployeeSystem.Instance.GetHiredCount();
            employeesCashier = EntrepreneurEmployeeSystem.Instance.GetRoleCount(EmployeeRole.Cashier);
            employeesRestocker = EntrepreneurEmployeeSystem.Instance.GetRoleCount(EmployeeRole.Restocker);
        }


        private void AccumulateItemDictionary(Dictionary<string, int> dict, IReadOnlyList<RobbedItem> items, bool recovered)
        {
            if (dict == null || items == null || items.Count == 0)
                return;

            for (int i = 0; i < items.Count; i++)
            {
                RobbedItem item = items[i];
                if (item == null || string.IsNullOrEmpty(item.productName))
                    continue;

                int quantity = Mathf.Max(1, item.quantity);
                int existing;
                dict.TryGetValue(item.productName, out existing);
                dict[item.productName] = existing + quantity;
            }

            if (recovered)
                RefreshEmployeeSnapshot();
        }


        private string BuildProductSummaryLine()
        {
            StringBuilder sb = new StringBuilder();
            if (robbedProductsByName.Count > 0)
                sb.Append("- Productos robados detalle: ").Append(FormatTopProducts(robbedProductsByName)).Append('\n');
            if (recoveredProductsByName.Count > 0)
                sb.Append("- Productos recuperados detalle: ").Append(FormatTopProducts(recoveredProductsByName)).Append('\n');
            return sb.ToString();
        }


        private static string FormatTopProducts(Dictionary<string, int> dict)
        {
            if (dict == null || dict.Count == 0)
                return "N/A";

            List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>(dict);
            list.Sort((a, b) => b.Value.CompareTo(a.Value));

            int max = Mathf.Min(5, list.Count);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < max; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(list[i].Key).Append(" x").Append(list[i].Value);
            }

            return sb.ToString();
        }


        private static JSONArray SerializeDictionary(Dictionary<string, int> dict)
        {
            JSONArray array = new JSONArray();
            if (dict == null)
                return array;

            foreach (KeyValuePair<string, int> pair in dict)
            {
                JSONNode node = new JSONObject();
                node["name"] = pair.Key;
                node["count"] = pair.Value;
                array.Add(node);
            }

            return array;
        }


        private static void DeserializeDictionary(JSONArray array, Dictionary<string, int> dict)
        {
            if (dict == null)
                return;

            dict.Clear();
            if (array == null)
                return;

            for (int i = 0; i < array.Count; i++)
            {
                JSONNode node = array[i];
                string name = node["name"];
                if (string.IsNullOrEmpty(name))
                    continue;

                dict[name] = node["count"].AsInt;
            }
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
