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
        public int customersScheduled { get; private set; }
        public int customersEntered { get; private set; }
        public long salaryExpenses { get; private set; }
        public long rentExpenses { get; private set; }
        public long utilityExpenses { get; private set; }

        public int thievesAppeared { get; private set; }
        public int thievesDetected { get; private set; }
        public int thievesAutoArrested { get; private set; }
        public int thievesManualArrested { get; private set; }
        public int thievesEscaped { get; private set; }
        public long robberyValueStolen { get; private set; }
        public long robberyMoneyLost { get; private set; }
        public long robberyValueRecovered { get; private set; }
        public int robbedProducts { get; private set; }
        public int recoveredProducts { get; private set; }
        public int employeesHired { get; private set; }
        public int employeesCashier { get; private set; }
        public int employeesRestocker { get; private set; }
        public int securityLevelSnapshot { get; private set; }
        public float securityChanceSnapshot { get; private set; }

        // ── Expansion snapshot (captured when the store closes) ───────────────
        public int expansionZonesPurchasedToday { get; private set; }
        public int expansionTotalZones { get; private set; }
        public int expansionSalesAreaM2 { get; private set; }
        public int expansionStorageAreaM2 { get; private set; }

        // ── Achievement counters ───────────────────────────────────────────────
        public int achievementsCompletedToday { get; private set; }

        // ── Inventory snapshot (captured when the store closes) ───────────────
        public int lowStockProductCount { get; private set; }
        public int outOfStockProductCount { get; private set; }
        public string lowStockProductNames { get; private set; } = string.Empty;
        public int outOfStockSalesToday { get; private set; }
        public int unavailableProductComplaints { get; private set; }

        private const int LowStockThreshold = 3;
        private const int MaxLowStockNamesInReport = 8;

        private readonly Dictionary<string, int> robbedProductsByName = new Dictionary<string, int>();
        private readonly Dictionary<string, int> recoveredProductsByName = new Dictionary<string, int>();
        private readonly Dictionary<string, int> outOfStockProductsByName = new Dictionary<string, int>();
        private bool dailyExpensesApplied;


        //initialize references
        void Awake()
        {
            Instance = this;

            StoreDatabase.onMoneyUpdate += OnMoneyUpdate;
            StoreDatabase.onExperienceUpdate += OnExperienceUpdate;
            DayCycleSystem.onDayLoaded += OnDayLoaded;
            DayCycleSystem.onDayOver += OnDayOver;
            CustomerSystem.onCustomerLeft += OnCustomerLeft;
            CustomerSystem.onCustomerEntered += OnCustomerEntered;
            CustomerSystem.onDailyCustomerPlanCreated += OnDailyCustomerPlanCreated;
            SupermarketExpansionSystem.onZonePurchased += OnZonePurchased;
            AchievementSystem.onAchievementCompleted += OnAchievementCompleted;
        }


        //subscribed to day event
        private void OnDayLoaded()
        {
            moneyEarned = moneySpent = 0;
            xpEarned = 0;
            customersHappy = customersUnhappy = 0;
            customersScheduled = customersEntered = 0;
            salaryExpenses = rentExpenses = utilityExpenses = 0;
            dailyExpensesApplied = false;
            thievesAppeared = thievesDetected = thievesAutoArrested = thievesManualArrested = thievesEscaped = 0;
            robberyValueStolen = robberyMoneyLost = robberyValueRecovered = 0;
            robbedProducts = recoveredProducts = 0;
            robbedProductsByName.Clear();
            recoveredProductsByName.Clear();
            outOfStockProductsByName.Clear();
            outOfStockSalesToday = unavailableProductComplaints = 0;
            lowStockProductCount = outOfStockProductCount = 0;
            lowStockProductNames = string.Empty;
            expansionZonesPurchasedToday = 0;
            achievementsCompletedToday = 0;
            RefreshEmployeeSnapshot();
            RefreshSecuritySnapshot();
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
            else
            {
                customersUnhappy++;
                AchievementSystem.RegisterPriceComplaint();
            }
        }


        private void OnCustomerEntered(int enteredToday)
        {
            customersEntered = Mathf.Max(customersEntered, enteredToday);
        }


        private void OnDailyCustomerPlanCreated(int scheduled)
        {
            customersScheduled = scheduled;
        }


        //subscribed to expansion zone purchase
        private void OnZonePurchased(ExpansionZoneData zone)
        {
            expansionZonesPurchasedToday++;
        }


        //subscribed to achievement completion
        private void OnAchievementCompleted(AchievementId id)
        {
            achievementsCompletedToday++;
        }


        //subscribed to day-over event: capture end-of-day snapshots before the scene transitions
        private void OnDayOver()
        {
            ApplyDailyOperatingExpenses();
            RefreshEmployeeSnapshot();
            RefreshSecuritySnapshot();
            CaptureExpansionSnapshot();
            CaptureInventorySnapshot();
        }


        private void ApplyDailyOperatingExpenses()
        {
            if (dailyExpensesApplied || StoreDatabase.Instance == null)
                return;

            dailyExpensesApplied = true;

            int hired = EntrepreneurEmployeeSystem.Instance != null
                ? EntrepreneurEmployeeSystem.Instance.GetHiredCount()
                : employeesHired;
            salaryExpenses = hired * 6000L;

            long capitalBase = StoreDatabase.Instance.startMoney > 0
                ? StoreDatabase.Instance.startMoney
                : 100000L;
            int salesExpansions = SupermarketExpansionSystem.Instance != null
                ? SupermarketExpansionSystem.Instance.GetPurchasedSalesExpansionCount()
                : 0;
            rentExpenses = (long)System.Math.Floor(capitalBase * (0.05f + salesExpansions * 0.015f));

            // Placeholder-safe utility backend. Per-light detection stays for the visual/building phase.
            utilityExpenses = (long)System.Math.Floor(capitalBase * 0.05f);

            long total = salaryExpenses + rentExpenses + utilityExpenses;
            if (total <= 0)
                return;

            StoreDatabase.AddRemoveMoney(-total);
            Debug.Log("[ShopMaster] Daily operating expenses applied. Salaries="
                + StoreDatabase.FromLongToStringMoney(salaryExpenses)
                + ", rent=" + StoreDatabase.FromLongToStringMoney(rentExpenses)
                + ", utilities=" + StoreDatabase.FromLongToStringMoney(utilityExpenses) + ".");
        }


        private void CaptureExpansionSnapshot()
        {
            if (SupermarketExpansionSystem.Instance == null) return;

            expansionTotalZones = SupermarketExpansionSystem.Instance.GetPurchasedZonesCount();
            expansionSalesAreaM2 = SupermarketExpansionSystem.Instance.GetPurchasedSalesAreaM2();
            expansionStorageAreaM2 = SupermarketExpansionSystem.Instance.GetPurchasedStorageAreaM2();
        }


        private void CaptureInventorySnapshot()
        {
            if (ProductInventorySystem.Instance == null) return;

            List<PurchasableScriptableObject> all =
                ItemDatabase.GetByType(typeof(ProductScriptableObject));

            int outOf = 0;
            int lowOf = 0;
            var namesBuf = new StringBuilder();
            int namesAdded = 0;

            for (int i = 0; i < all.Count; i++)
            {
                ProductScriptableObject product = all[i] as ProductScriptableObject;
                if (product == null) continue;

                int total = ProductInventorySystem.Instance.GetTotalStock(product);
                if (total == 0)
                {
                    outOf++;
                    if (namesAdded < MaxLowStockNamesInReport)
                    {
                        if (namesAdded > 0) namesBuf.Append(", ");
                        namesBuf.Append(product.title).Append(" (agotado)");
                        namesAdded++;
                    }
                }
                else if (total < LowStockThreshold)
                {
                    lowOf++;
                    if (namesAdded < MaxLowStockNamesInReport)
                    {
                        if (namesAdded > 0) namesBuf.Append(", ");
                        namesBuf.Append(product.title).Append(" x").Append(total);
                        namesAdded++;
                    }
                }
            }

            outOfStockProductCount = outOf;
            lowStockProductCount = lowOf;
            lowStockProductNames = namesBuf.ToString();
        }


        /// <summary>
        /// Builds a summary of expansion, inventory, and achievement data from persisted JSON.
        /// <paramref name="totalAchievementsCompleted"/> and <paramref name="totalAchievementsPossible"/>
        /// are provided by the caller (read from AchievementSystem save data in UIStats).
        /// </summary>
        public static string BuildInventoryExpansionSummary(
            JSONNode data,
            int totalAchievementsCompleted,
            int totalAchievementsPossible)
        {
            if (data == null || data.Count == 0)
                return string.Empty;

            long moneyIn         = data["moneyEarned"].AsLong;
            long moneyOut        = data["moneySpent"].AsLong;
            long robberyLost     = data["robberyMoneyLost"].AsLong;
            int  customersHappyD = data["customersHappy"].AsInt;
            int  customersUnhappyD = data["customersUnhappy"].AsInt;
            int customersScheduledD = data["customersScheduled"].AsInt;
            int customersEnteredD = data["customersEntered"].AsInt;
            long salaryExpensesD = data["salaryExpenses"].AsLong;
            long rentExpensesD = data["rentExpenses"].AsLong;
            long utilityExpensesD = data["utilityExpenses"].AsLong;

            int zonesToday       = data["expansionZonesPurchasedToday"].AsInt;
            int zonesTotalBought = data["expansionTotalZones"].AsInt;
            int salesAreaM2      = data["expansionSalesAreaM2"].AsInt;
            int storageAreaM2    = data["expansionStorageAreaM2"].AsInt;
            int lowStockCount    = data["lowStockProductCount"].AsInt;
            int outOfStockCount  = data["outOfStockProductCount"].AsInt;
            string lowStockNames = data["lowStockProductNames"].Value;
            int outOfStockSalesD = data["outOfStockSalesToday"].AsInt;
            int unavailableComplaintsD = data["unavailableProductComplaints"].AsInt;
            string outOfStockSoldNames = FormatTopProductsFromJson(data["outOfStockProductsByName"].AsArray);
            int achievesToday    = data["achievementsCompletedToday"].AsInt;

            var sb = new StringBuilder();

            // ── Financial summary ──────────────────────────────────────────────
            // moneyOut is persisted as a negative long (all withdrawals are negative in StoreDatabase).
            // Net profit = revenue - |expenses|, which equals moneyIn + moneyOut since moneyOut < 0.
            long netProfit = moneyIn - System.Math.Abs(moneyOut);
            long operatingExpenses = salaryExpensesD + rentExpensesD + utilityExpensesD;
            long productExpenses = System.Math.Max(0, System.Math.Abs(moneyOut) - operatingExpenses);
            int lostCustomers = customersUnhappyD;
            string performanceSummary = BuildPerformanceSummary(netProfit, lostCustomers, robberyLost, outOfStockCount + outOfStockSalesD);
            sb.Append("Finanzas del día:\n");
            sb.Append("- Ventas totales: ").Append(StoreDatabase.FromLongToStringMoney(moneyIn)).Append('\n');
            sb.Append("- Gastos en productos: ").Append(StoreDatabase.FromLongToStringMoney(productExpenses)).Append('\n');
            sb.Append("- Gastos totales: ").Append(StoreDatabase.FromLongToStringMoney(-System.Math.Abs(moneyOut))).Append('\n');
            sb.Append("- Salarios: ").Append(StoreDatabase.FromLongToStringMoney(salaryExpensesD)).Append('\n');
            sb.Append("- Renta: ").Append(StoreDatabase.FromLongToStringMoney(rentExpensesD)).Append('\n');
            sb.Append("- Luz/servicios: ").Append(StoreDatabase.FromLongToStringMoney(utilityExpensesD)).Append('\n');
            // Always show robbery losses, including $0.00.
                sb.Append("- Pérdidas por robo: ").Append(StoreDatabase.FromLongToStringMoney(robberyLost)).Append('\n');
            sb.Append("- Ganancia neta: ").Append(StoreDatabase.FromLongToStringMoney(netProfit)).Append('\n');
            sb.Append("- Desempeno general: ").Append(performanceSummary).Append('\n');

            // ── Customer summary ───────────────────────────────────────────────
            sb.Append("\nClientes:\n");
            sb.Append("- Programados: ").Append(customersScheduledD).Append('\n');
            sb.Append("- Entraron: ").Append(customersEnteredD).Append('\n');
            sb.Append("- Atendidos: ").Append(customersHappyD + customersUnhappyD).Append('\n');
            sb.Append("- Satisfechos: ").Append(customersHappyD).Append('\n');
            sb.Append("- Inconformes: ").Append(customersUnhappyD).Append('\n');
            sb.Append("- Clientes perdidos: ").Append(lostCustomers).Append('\n');
            sb.Append("- Quejas por producto no disponible: ").Append(unavailableComplaintsD).Append('\n');

            sb.Append("\nExpansión:\n");
            sb.Append("- Zonas totales: ").Append(zonesTotalBought).Append('\n');
            sb.Append("- Área de ventas: ").Append(salesAreaM2).Append(" m²\n");
            sb.Append("- Área de almacén: ").Append(storageAreaM2).Append(" m²\n");
            sb.Append("- Compradas hoy: ").Append(zonesToday).Append('\n');

            sb.Append("\nInventario (cierre):\n");
            sb.Append("- Sin stock: ").Append(outOfStockCount).Append('\n');
            sb.Append("- Agotados por venta: ").Append(outOfStockSalesD).Append('\n');
            sb.Append("- Stock bajo (<3 und.): ").Append(lowStockCount).Append('\n');
            if (!string.IsNullOrEmpty(lowStockNames))
                sb.Append("- Detalle: ").Append(lowStockNames).Append('\n');
            if (!string.IsNullOrEmpty(outOfStockSoldNames))
                sb.Append("- Agotados vendidos: ").Append(outOfStockSoldNames).Append('\n');

            sb.Append("\nLogros:\n");
            sb.Append("- Desbloqueados hoy: ").Append(achievesToday).Append('\n');
            sb.Append("- Total: ").Append(totalAchievementsCompleted)
              .Append('/').Append(totalAchievementsPossible).Append('\n');

            return sb.ToString();
        }


        public static void RegisterSoldProduct(ProductScriptableObject product, int quantity)
        {
            if (Instance == null || product == null || quantity <= 0)
                return;

            if (ProductInventorySystem.Instance == null)
                return;

            int remaining = ProductInventorySystem.Instance.GetTotalStock(product);
            if (remaining > 0)
                return;

            Instance.outOfStockSalesToday++;
            Instance.AccumulateName(Instance.outOfStockProductsByName, product.title, quantity);
            if (UIGame.Instance != null)
                UIGame.AddNotification("Producto agotado: " + product.title + ". Los clientes podrian molestarse.", otherColor: new Color(1f, 0.65f, 0.18f));
            Debug.Log("[Inventory] Product out of stock after sale: " + product.title);
        }


        public static void RegisterUnavailableProductComplaint(ProductScriptableObject product)
        {
            if (Instance == null)
                return;

            Instance.unavailableProductComplaints++;
            if (product != null)
                Instance.AccumulateName(Instance.outOfStockProductsByName, product.title, 1);
            Debug.Log("[Inventory] Customer complaint for unavailable product: " + (product != null ? product.title : "Unknown"));
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
            Instance.robberyValueStolen += stolenValue > 0 ? stolenValue : 0;
            Instance.robbedProducts += Mathf.Max(0, productCount);
            Instance.AccumulateItemDictionary(Instance.robbedProductsByName, items, false);
        }


        public static void RegisterThiefAutomaticArrest(int recoveredCount, long recoveredValue, IReadOnlyList<RobbedItem> items = null)
        {
            if (Instance == null) return;
            Instance.thievesAutoArrested++;
            Instance.recoveredProducts += Mathf.Max(0, recoveredCount);
            Instance.robberyValueRecovered += recoveredValue > 0 ? recoveredValue : 0;
            Instance.AccumulateItemDictionary(Instance.recoveredProductsByName, items, true);
        }


        public static void RegisterThiefManualArrest(int recoveredCount, long recoveredValue, IReadOnlyList<RobbedItem> items = null)
        {
            if (Instance == null) return;
            Instance.thievesManualArrested++;
            Instance.recoveredProducts += Mathf.Max(0, recoveredCount);
            Instance.robberyValueRecovered += recoveredValue > 0 ? recoveredValue : 0;
            Instance.AccumulateItemDictionary(Instance.recoveredProductsByName, items, true);
        }


        public static void RegisterThiefEscaped(long moneyLost, int productCount, IReadOnlyList<RobbedItem> items = null)
        {
            if (Instance == null) return;
            Instance.thievesEscaped++;
            Instance.robberyMoneyLost += moneyLost > 0 ? moneyLost : 0;
            if (items != null && items.Count > 0)
                Instance.AccumulateItemDictionary(Instance.robbedProductsByName, items, false);
        }


        public static string GetDailyRobberySummary()
        {
            if (Instance == null)
                return string.Empty;

            Instance.RefreshEmployeeSnapshot();
            Instance.RefreshSecuritySnapshot();

            float effectiveness = 0f;
            int handled = Instance.thievesAutoArrested + Instance.thievesManualArrested;
            int denominator = Instance.thievesDetected;
            if (denominator > 0)
                effectiveness = ((float)handled / denominator) * 100f;

            int securityLevel = Instance.securityLevelSnapshot;
            float securityChance = Instance.securityChanceSnapshot;

            return "Robos del día:\n" +
                   "- Ladrones aparecidos: " + Instance.thievesAppeared + "\n" +
                   "- Robos ocurridos: " + Instance.thievesDetected + "\n" +
                   "- Ladrones detectados: " + Instance.thievesDetected + "\n" +
                   "- Arrestos automáticos: " + Instance.thievesAutoArrested + "\n" +
                   "- Detenidos manualmente: " + Instance.thievesManualArrested + "\n" +
                   "- Escaparon: " + Instance.thievesEscaped + "\n" +
                   "- Valor robado: " + StoreDatabase.FromLongToStringMoney(Instance.robberyValueStolen) + "\n" +
                   "- Valor perdido: " + StoreDatabase.FromLongToStringMoney(Instance.robberyMoneyLost) + "\n" +
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
        /// Builds robbery + employee summary from persisted JSON data.
        /// Used by UIStats scene where runtime gameplay singletons may not be available.
        /// </summary>
        public static string BuildDailyRobberySummary(JSONNode data)
        {
            if (data == null || data.Count == 0)
                return "Robos del día:\n- Sin datos disponibles.";

            int thievesAppearedData = data["thievesAppeared"].AsInt;
            int thievesDetectedData = data["thievesDetected"].AsInt;
            int thievesAutoData = data["thievesAutoArrested"].AsInt;
            int thievesManualData = data["thievesManualArrested"].AsInt;
            int thievesEscapedData = data["thievesEscaped"].AsInt;
            long stolenValueData = data["robberyValueStolen"].AsLong;
            long robberyLostData = data["robberyMoneyLost"].AsLong;
            long recoveredValueData = data["robberyValueRecovered"].AsLong;
            int robbedProductsData = data["robbedProducts"].AsInt;
            int recoveredProductsData = data["recoveredProducts"].AsInt;
            int employeesHiredData = data["employeesHired"].AsInt;
            int employeesCashierData = data["employeesCashier"].AsInt;
            int employeesRestockerData = data["employeesRestocker"].AsInt;
            int securityLevelData = data["securityLevelSnapshot"].AsInt;
            float securityChanceData = data["securityChanceSnapshot"].AsFloat;

            int handled = thievesAutoData + thievesManualData;
            int denominator = thievesDetectedData;
            float effectiveness = denominator > 0 ? (handled / (float)denominator) * 100f : 0f;

            return "Robos del día:\n" +
                   "- Ladrones aparecidos: " + thievesAppearedData + "\n" +
                   "- Robos ocurridos: " + thievesDetectedData + "\n" +
                   "- Ladrones detectados: " + thievesDetectedData + "\n" +
                   "- Arrestos automáticos: " + thievesAutoData + "\n" +
                   "- Detenidos manualmente: " + thievesManualData + "\n" +
                   "- Escaparon: " + thievesEscapedData + "\n" +
                   "- Valor robado: " + StoreDatabase.FromLongToStringMoney(stolenValueData) + "\n" +
                   "- Valor perdido: " + StoreDatabase.FromLongToStringMoney(robberyLostData) + "\n" +
                   "- Pérdida total: " + StoreDatabase.FromLongToStringMoney(robberyLostData) + "\n" +
                   "- Valor recuperado: " + StoreDatabase.FromLongToStringMoney(recoveredValueData) + "\n" +
                   "- Productos robados: " + robbedProductsData + "\n" +
                   "- Productos recuperados: " + recoveredProductsData + "\n" +
                   "- Efectividad seguridad: " + effectiveness.ToString("0") + "%\n" +
                   "- Seguridad actual: Nivel " + securityLevelData + " / " + securityChanceData.ToString("0") + "%\n\n" +
                   "Empleados:\n" +
                   "- Contratados: " + employeesHiredData + "/" + EntrepreneurEmployeeSystem.MaxEmployees + "\n" +
                   "- Cajeros: " + employeesCashierData + "\n" +
                   "- Surtidores: " + employeesRestockerData;
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
            data["customersScheduled"] = customersScheduled;
            data["customersEntered"] = customersEntered;
            data["salaryExpenses"] = salaryExpenses;
            data["rentExpenses"] = rentExpenses;
            data["utilityExpenses"] = utilityExpenses;
            data["thievesAppeared"] = thievesAppeared;
            data["thievesDetected"] = thievesDetected;
            data["thievesAutoArrested"] = thievesAutoArrested;
            data["thievesManualArrested"] = thievesManualArrested;
            data["thievesEscaped"] = thievesEscaped;
            data["robberyValueStolen"] = robberyValueStolen;
            data["robberyMoneyLost"] = robberyMoneyLost;
            data["robberyValueRecovered"] = robberyValueRecovered;
            data["robbedProducts"] = robbedProducts;
            data["recoveredProducts"] = recoveredProducts;
            data["employeesHired"] = employeesHired;
            data["employeesCashier"] = employeesCashier;
            data["employeesRestocker"] = employeesRestocker;
            data["securityLevelSnapshot"] = securityLevelSnapshot;
            data["securityChanceSnapshot"] = securityChanceSnapshot;
            data["robbedProductsByName"] = SerializeDictionary(robbedProductsByName);
            data["recoveredProductsByName"] = SerializeDictionary(recoveredProductsByName);
            data["expansionZonesPurchasedToday"] = expansionZonesPurchasedToday;
            data["expansionTotalZones"] = expansionTotalZones;
            data["expansionSalesAreaM2"] = expansionSalesAreaM2;
            data["expansionStorageAreaM2"] = expansionStorageAreaM2;
            data["achievementsCompletedToday"] = achievementsCompletedToday;
            data["lowStockProductCount"] = lowStockProductCount;
            data["outOfStockProductCount"] = outOfStockProductCount;
            data["lowStockProductNames"] = lowStockProductNames ?? string.Empty;
            data["outOfStockSalesToday"] = outOfStockSalesToday;
            data["unavailableProductComplaints"] = unavailableProductComplaints;
            data["outOfStockProductsByName"] = SerializeDictionary(outOfStockProductsByName);
            
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
            customersScheduled = data["customersScheduled"].AsInt;
            customersEntered = data["customersEntered"].AsInt;
            salaryExpenses = data["salaryExpenses"].AsLong;
            rentExpenses = data["rentExpenses"].AsLong;
            utilityExpenses = data["utilityExpenses"].AsLong;
            thievesAppeared = data["thievesAppeared"].AsInt;
            thievesDetected = data["thievesDetected"].AsInt;
            thievesAutoArrested = data["thievesAutoArrested"].AsInt;
            thievesManualArrested = data["thievesManualArrested"].AsInt;
            thievesEscaped = data["thievesEscaped"].AsInt;
            robberyValueStolen = data["robberyValueStolen"].AsLong;
            robberyMoneyLost = data["robberyMoneyLost"].AsLong;
            robberyValueRecovered = data["robberyValueRecovered"].AsLong;
            robbedProducts = data["robbedProducts"].AsInt;
            recoveredProducts = data["recoveredProducts"].AsInt;
            employeesHired = data["employeesHired"].AsInt;
            employeesCashier = data["employeesCashier"].AsInt;
            employeesRestocker = data["employeesRestocker"].AsInt;
            securityLevelSnapshot = data["securityLevelSnapshot"].AsInt;
            securityChanceSnapshot = data["securityChanceSnapshot"].AsFloat;
            DeserializeDictionary(data["robbedProductsByName"].AsArray, robbedProductsByName);
            DeserializeDictionary(data["recoveredProductsByName"].AsArray, recoveredProductsByName);
            expansionZonesPurchasedToday = data["expansionZonesPurchasedToday"].AsInt;
            expansionTotalZones = data["expansionTotalZones"].AsInt;
            expansionSalesAreaM2 = data["expansionSalesAreaM2"].AsInt;
            expansionStorageAreaM2 = data["expansionStorageAreaM2"].AsInt;
            achievementsCompletedToday = data["achievementsCompletedToday"].AsInt;
            lowStockProductCount = data["lowStockProductCount"].AsInt;
            outOfStockProductCount = data["outOfStockProductCount"].AsInt;
            lowStockProductNames = data["lowStockProductNames"].Value ?? string.Empty;
            outOfStockSalesToday = data["outOfStockSalesToday"].AsInt;
            unavailableProductComplaints = data["unavailableProductComplaints"].AsInt;
            DeserializeDictionary(data["outOfStockProductsByName"].AsArray, outOfStockProductsByName);
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


        private void RefreshSecuritySnapshot()
        {
            if (EntrepreneurTreeGameplayBridge.Instance != null)
            {
                securityLevelSnapshot = EntrepreneurTreeGameplayBridge.Instance.GetSecurityLevel();
                securityChanceSnapshot = EntrepreneurTreeGameplayBridge.Instance.GetSecurityArrestChance() * 100f;
                return;
            }

            if (EntrepreneurTreeSecurityAdapter.Instance != null)
            {
                securityLevelSnapshot = EntrepreneurTreeSecurityAdapter.Instance.GetCurrentSecurityLevel();
                securityChanceSnapshot = EntrepreneurTreeSecurityAdapter.Instance.GetAutomaticArrestChance() * 100f;
                return;
            }

            if (EntrepreneurTreeGameplayBridge.Instance == null)
            {
                securityLevelSnapshot = 0;
                securityChanceSnapshot = 0f;
                return;
            }
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


        private void AccumulateName(Dictionary<string, int> dict, string productName, int quantity)
        {
            if (dict == null || string.IsNullOrEmpty(productName))
                return;

            int existing;
            dict.TryGetValue(productName, out existing);
            dict[productName] = existing + Mathf.Max(1, quantity);
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


        private static string FormatTopProductsFromJson(JSONArray array)
        {
            if (array == null || array.Count == 0)
                return string.Empty;

            Dictionary<string, int> dict = new Dictionary<string, int>();
            DeserializeDictionary(array, dict);
            return FormatTopProducts(dict);
        }


        private static string BuildPerformanceSummary(long netProfit, int lostCustomers, long robberyLost, int outOfStockTotal)
        {
            int penalties = 0;
            if (netProfit < 0) penalties += 2;
            if (lostCustomers >= 5) penalties++;
            if (robberyLost > 0) penalties++;
            if (outOfStockTotal >= 3) penalties++;

            if (penalties <= 0 && netProfit > 0)
                return "Excelente";
            if (penalties <= 1)
                return "Bueno";
            if (penalties <= 3)
                return "Regular";
            return "Malo";
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
            DayCycleSystem.onDayOver -= OnDayOver;
            CustomerSystem.onCustomerLeft -= OnCustomerLeft;
            CustomerSystem.onCustomerEntered -= OnCustomerEntered;
            CustomerSystem.onDailyCustomerPlanCreated -= OnDailyCustomerPlanCreated;
            SupermarketExpansionSystem.onZonePurchased -= OnZonePurchased;
            AchievementSystem.onAchievementCompleted -= OnAchievementCompleted;
        }
    }
}
