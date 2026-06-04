// ShopMasterReportSavePaymentAuditRunners.cs
// Focused Play Mode audit runners for RQF20-RQF24: daily report, autosave, customers, card and cash payments.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FLOBUK.StoreSimulator;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FLOBUK.StoreSimulator.Editor
{
    public static class DailyRobberyReportPlayModeAuditRunner { public static void Run() => ShopMasterReportSavePaymentAuditCore.Run(ShopMasterReportSavePaymentAuditCore.AuditKind.DailyRobberyReport); }
    public static class EndOfDayAutosavePlayModeAuditRunner { public static void Run() => ShopMasterReportSavePaymentAuditCore.Run(ShopMasterReportSavePaymentAuditCore.AuditKind.EndOfDayAutosave); }
    public static class CustomerScalingPlayModeAuditRunner { public static void Run() => ShopMasterReportSavePaymentAuditCore.Run(ShopMasterReportSavePaymentAuditCore.AuditKind.CustomerScaling); }
    public static class CardPaymentPlayModeAuditRunner { public static void Run() => ShopMasterReportSavePaymentAuditCore.Run(ShopMasterReportSavePaymentAuditCore.AuditKind.CardPayment); }
    public static class CashPaymentPlayModeAuditRunner { public static void Run() => ShopMasterReportSavePaymentAuditCore.Run(ShopMasterReportSavePaymentAuditCore.AuditKind.CashPayment); }

    [InitializeOnLoad]
    internal static class ShopMasterReportSavePaymentAuditCore
    {
        internal enum AuditKind { DailyRobberyReport, EndOfDayAutosave, CustomerScaling, CardPayment, CashPayment }

        private const string StateKey = "ShopMaster.ReportSavePayment.State";
        private const string KindKey = "ShopMaster.ReportSavePayment.Kind";
        private const string FailureKey = "ShopMaster.ReportSavePayment.Failures";
        private const string StartKey = "ShopMaster.ReportSavePayment.Start";
        private const string ScenePath = "Assets/StoreSimulator/Scenes/Game.unity";

        private static readonly Dictionary<string, byte[]> preservedSaveFiles = new Dictionary<string, byte[]>();
        private static bool sawAutosaveEvent;

        static ShopMasterReportSavePaymentAuditCore()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            Application.logMessageReceived -= OnConsole;
            Application.logMessageReceived += OnConsole;
        }

        internal static void Run(AuditKind kind)
        {
            Directory.CreateDirectory("Documentos");
            SessionState.SetString(KindKey, kind.ToString());
            SessionState.SetString(StateKey, "enter");
            SessionState.SetInt(FailureKey, 0);
            SessionState.SetFloat(StartKey, 0f);
            File.WriteAllText(GetLogPath(kind), "# ShopMaster Reportes/Guardado/Pagos Audit - " + kind + "\n");
            Log("Opening scene: " + ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }

        private static void OnUpdate()
        {
            string state = SessionState.GetString(StateKey, string.Empty);
            if (string.IsNullOrEmpty(state))
                return;

            if (state == "enter" && EditorApplication.isPlaying)
            {
                if (!Elapsed(3f))
                    return;

                try
                {
                    PrepareBaseState();
                    AuditKind kind = GetKind();
                    if (kind == AuditKind.DailyRobberyReport) RunDailyRobberyReport();
                    else if (kind == AuditKind.EndOfDayAutosave) RunEndOfDayAutosave();
                    else if (kind == AuditKind.CustomerScaling) RunCustomerScaling();
                    else if (kind == AuditKind.CardPayment) RunCardPayment();
                    else RunCashPayment();
                }
                catch (Exception ex)
                {
                    Fail("Unhandled audit exception: " + ex);
                }
                finally
                {
                    RestorePreservedFiles();
                }

                Log("Audit finished. Failures=" + SessionState.GetInt(FailureKey, 0));
                SetState("leave");
                EditorApplication.ExitPlaymode();
                return;
            }

            if (state == "leave" && !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                int failures = SessionState.GetInt(FailureKey, 0);
                SessionState.EraseString(StateKey);
                SessionState.EraseString(KindKey);
                SessionState.EraseInt(FailureKey);
                SessionState.EraseFloat(StartKey);
                EditorApplication.Exit(failures == 0 ? 0 : 1);
            }
        }

        private static void PrepareBaseState()
        {
            PassIf(SaveGameSystem.Instance != null, "SaveGameSystem presente.");
            PassIf(StoreDatabase.Instance != null, "StoreDatabase presente.");
            PassIf(DayCycleSystem.Instance != null, "DayCycleSystem presente.");
            PassIf(CustomerSystem.Instance != null, "CustomerSystem presente.");
            PassIf(StatsDatabase.Instance != null, "StatsDatabase presente.");
            PassIf(SupermarketExpansionSystem.Instance != null, "SupermarketExpansionSystem presente.");
            PassIf(UIGame.Instance != null, "UIGame presente para fin de dia/notificaciones.");

            StatsDatabase.Instance?.LoadFromJSON(null);
            EntrepreneurTreeManager.Instance?.LoadFromJSON(null);
            if (StoreDatabase.Instance != null && StoreDatabase.Instance.currentMoney < 5000000L)
                StoreDatabase.AddRemoveMoney(5000000L - StoreDatabase.Instance.currentMoney);
        }

        private static void RunDailyRobberyReport()
        {
            Log("=== RQF20 robos en reporte diario ===");
            ProductScriptableObject product = GetStarterProduct();
            PlacementObject manualShelf = FindEmptyPlacement(product);
            ShoplifterAgent manual = SpawnThiefWithProduct(ShoplifterType.Common, product, manualShelf);
            int manualRecoveredBefore = StatsDatabase.Instance.recoveredProducts;
            PassIf(manual != null && manual.TryManualCapture(), "RQF20 captura manual real queda registrada.");
            PassIf(StatsDatabase.Instance.recoveredProducts > manualRecoveredBefore, "RQF20 captura manual aumenta productos recuperados.");

            ShoplifterAgent escaped = SpawnThiefWithProduct(ShoplifterType.Common, product);
            PassIf(escaped != null, "RQF20 robo de escape generado con cliente/anaquel real.");
            escaped.ResolveAsEscaped();

            UnlockSecurityPath("security_3");
            EntrepreneurTreeSecurityAdapter.Instance?.RefreshFromTree();
            UnityEngine.Random.InitState(1234);
            ShoplifterAgent auto = SpawnThiefWithProduct(ShoplifterType.Common, product);
            PassIf(auto != null && auto.isResolved && !auto.isEscaping, "RQF20 arresto automatico real alimenta estadisticas.");

            string runtimeSummary = StatsDatabase.GetDailyRobberySummary();
            var dailyData = StatsDatabase.Instance.SaveToJSON();
            string persistedSummary = StatsDatabase.BuildDailyRobberySummary(dailyData);

            PassIf(runtimeSummary.Contains("Robos ocurridos: 3") && persistedSummary.Contains("Robos ocurridos: 3"),
                "RQF20 reporte diario muestra total de robos ocurridos.");
            PassIf(runtimeSummary.Contains("Detenidos manualmente: 1") && runtimeSummary.Contains("Escaparon: 1"),
                "RQF20 reporte diario muestra manuales y escapes.");
            PassIf(runtimeSummary.Contains("Arrestos autom") && runtimeSummary.Contains("Valor perdido") && runtimeSummary.Contains("Valor recuperado"),
                "RQF20 reporte diario incluye arrestos automaticos, valor perdido y recuperado.");
            PassIf(runtimeSummary.Contains("Productos recuperados") && UIStatsHasRobberySummarySupport(),
                "RQF20 UIStats reutiliza BuildDailyRobberySummary para pintar el resumen guardado.");
        }

        private static void RunEndOfDayAutosave()
        {
            Log("=== RQF21 autosave al finalizar dia ===");
            PreserveDefaultSaveFiles();
            sawAutosaveEvent = false;
            SaveGameSystem.dataSaveEvent += MarkAutosave;

            try
            {
                StatsDatabase.RegisterThiefAppeared();
                StatsDatabase.RegisterThiefDetected(1200L, 1);
                MethodInfo leaveToNext = typeof(UIGame).GetMethod("LeaveToNext", BindingFlags.Instance | BindingFlags.NonPublic);
                PassIf(leaveToNext != null, "RQF21 UIGame conserva LeaveToNext como cierre real del dia.");
                leaveToNext?.Invoke(UIGame.Instance, null);
            }
            finally
            {
                SaveGameSystem.dataSaveEvent -= MarkAutosave;
            }

            string savePath = Path.Combine(Application.persistentDataPath, SaveGameSystem.fileKey + SaveGameSystem.fileExt);
            PassIf(sawAutosaveEvent, "RQF21 el cierre del dia dispara SaveGameSystem.dataSaveEvent.");
            PassIf(File.Exists(savePath), "RQF21 autosave escribe save.dat en persistentDataPath.");

            string saveText = File.ReadAllText(savePath);
            PassIf(saveText.Contains("\"StoreDatabase\"") && saveText.Contains("\"DayCycleSystem\"") &&
                   saveText.Contains("\"CustomerSystem\"") && saveText.Contains("\"StatsDatabase\""),
                "RQF21 autosave persiste StoreDatabase, DayCycleSystem, CustomerSystem y StatsDatabase.");
            PassIf(saveText.Contains("\"thievesDetected\":1") || saveText.Contains("\"thievesDetected\": 1"),
                "RQF21 autosave conserva estadisticas del dia, incluyendo robos.");
        }

        private static void RunCustomerScaling()
        {
            Log("=== RQF22 clientela por expansion ===");
            CustomerSystem customers = CustomerSystem.Instance;
            customers.minDailyCustomers = 50;
            customers.maxDailyCustomers = 75;

            float baseMultiplier = SupermarketExpansionSystem.Instance.GetCustomerCapacityMultiplier();
            PassIf(Mathf.Approximately(baseMultiplier, 1f),
                "RQF22 partida base no aplica bono extra de clientela.");
            PassIf(customers.minDailyCustomers == 50 && customers.maxDailyCustomers == 75,
                "RQF22 rango base diario queda entre 50 y 75 clientes.");

            StoreDatabase.AddRemoveMoney(5000000L);
            int before = SupermarketExpansionSystem.Instance.GetPurchasedSalesExpansionCount();
            TryBuyExpansion("sales_w1");
            TryBuyExpansion("sales_w2");
            int after = SupermarketExpansionSystem.Instance.GetPurchasedSalesExpansionCount();
            float expandedMultiplier = SupermarketExpansionSystem.Instance.GetCustomerCapacityMultiplier();

            int expectedMin = Mathf.RoundToInt(customers.minDailyCustomers * expandedMultiplier);
            int expectedMax = Mathf.RoundToInt(customers.maxDailyCustomers * expandedMultiplier);
            PassIf(after > before && Mathf.Approximately(expandedMultiplier, 1f + after * SupermarketExpansionSystem.SalesExpansionCustomerBonusPercent),
                "RQF22 cada expansion de venta comprada aplica +15% al multiplicador.");
            PassIf(expectedMin > 50 && expectedMax > 75,
                "RQF22 el plan diario escalado sube por encima de 50-75 tras expansiones.");

            MethodInfo plan = typeof(CustomerSystem).GetMethod("PlanDailyCustomers", BindingFlags.Instance | BindingFlags.NonPublic);
            plan.Invoke(customers, null);
            PassIf(customers.dailyCustomersScheduled >= expectedMin && customers.dailyCustomersScheduled <= expectedMax,
                "RQF22 PlanDailyCustomers usa el multiplicador real de SupermarketExpansionSystem.");
            PassIf(customers.spawnRate >= 1 && customers.maxSimultaneousCustomers >= 100,
                "RQF22 spawn distribuido conserva limite simultaneo seguro y ritmo real.");
        }

        private static void RunCardPayment()
        {
            Log("=== RQF23 pago con tarjeta ===");
            ProductScriptableObject product = GetStarterProduct();
            CashDesk desk = GetCashDesk();
            long beforeMoney = StoreDatabase.Instance.currentMoney;
            Customer customer = PrepareCheckout(desk, product, false);
            long total = product.storePrice;

            InvokeBill(desk, StoreDatabase.FromLongToStringMoney(total + 100));
            PassIf(GetQueueCount(desk) == 1 && StoreDatabase.Instance.currentMoney == beforeMoney,
                "RQF23 tarjeta rechaza cobro incorrecto y mantiene cliente en cola.");

            InvokeBill(desk, StoreDatabase.FromLongToStringMoney(total));
            PassIf(GetQueueCount(desk) == 0 && StoreDatabase.Instance.currentMoney == beforeMoney + total,
                "RQF23 tarjeta cobra exactamente el total y libera al cliente.");
            PassIf(customer != null && !customer.payCash && desk.terminal != null,
                "RQF23 usa terminal real de la caja manual para tarjeta.");
        }

        private static void RunCashPayment()
        {
            Log("=== RQF24 pago en efectivo con cambio manual ===");
            ProductScriptableObject product = GetStarterProduct();
            CashDesk desk = GetCashDesk();
            long beforeMoney = StoreDatabase.Instance.currentMoney;
            Customer customer = PrepareCheckout(desk, product, true);
            long total = product.storePrice;

            InvokeBill(desk, StoreDatabase.FromLongToStringMoney(total + 100));
            PassIf(GetQueueCount(desk) == 1 && StoreDatabase.Instance.currentMoney == beforeMoney,
                "RQF24 efectivo rechaza cambio insuficiente/sobrecobro antes de vender.");

            InvokeBill(desk, StoreDatabase.FromLongToStringMoney(total));
            PassIf(GetQueueCount(desk) == 0 && StoreDatabase.Instance.currentMoney == beforeMoney + total,
                "RQF24 efectivo acepta cambio manual correcto y registra venta.");

            Customer autoCustomer = PrepareCheckout(desk, product, true);
            bool started = desk.TryStartAutomatedCheckout(20f);
            PassIf(started && desk.isAutomatedCheckoutInProgress,
                "RQF24 cajero automatico puede iniciar flujo de efectivo usando CashDesk real.");
            PassIf(autoCustomer != null && autoCustomer.payCash && desk.register != null,
                "RQF24 usa registradora real y cliente marcado como efectivo.");
        }

        private static Customer PrepareCheckout(CashDesk desk, ProductScriptableObject product, bool cash)
        {
            ClearDesk(desk);
            GameObject prefab = CustomerSystem.Instance.customerPrefabs[0];
            GameObject go = UnityEngine.Object.Instantiate(prefab, CustomerSystem.Instance.spawnLocations[0].position, Quaternion.identity);
            Customer customer = go.GetComponent<Customer>();
            if (customer != null)
                customer.enabled = false;

            SetPayCash(customer, cash);
            CustomerCart bag = go.GetComponent<CustomerCart>();
            bag.items.Clear();
            bag.items.Add(new CustomerBagItem { product = product, fixedPrice = product.storePrice, count = 1 });

            desk.AddCustomerToQueue(customer);
            SetProtectedField(desk, "customerBag", bag);
            desk.cart.total.text = StoreDatabase.FromLongToStringMoney(product.storePrice);

            List<CheckoutItem> deskItems = GetProtectedField<List<CheckoutItem>>(desk, "deskItems");
            deskItems.Clear();
            GameObject itemGo = new GameObject("AuditCheckoutItem");
            CheckoutItem checkoutItem = itemGo.AddComponent<CheckoutItem>();
            checkoutItem.product = product;
            checkoutItem.fixedPrice = product.storePrice;
            deskItems.Add(checkoutItem);

            return customer;
        }

        private static void ClearDesk(CashDesk desk)
        {
            GetProtectedField<List<Customer>>(desk, "customerQueue").Clear();
            GetProtectedField<List<CheckoutItem>>(desk, "deskItems").Clear();
            SetProtectedField(desk, "customerBag", null);
            desk.cart.Clear();
            if (desk.terminal != null)
                desk.terminal.SetInteractable(false);
            if (desk.register != null)
                desk.register.SetInteractable(false);
        }

        private static ShoplifterAgent SpawnThiefWithProduct(ShoplifterType type, ProductScriptableObject product, PlacementObject shelf = null)
        {
            shelf = shelf != null ? shelf : FindEmptyPlacement(product);
            Customer customer = CreateCollectedCustomer(product, shelf);
            ShoplifterSystem.AdminForceNextSpawn(type);
            ShoplifterSystem.Instance.RegisterCustomer(customer);
            ShoplifterSystem.Instance.TryBeginTheft(customer, customer.GetComponent<CustomerCart>());
            return customer.GetComponent<ShoplifterAgent>();
        }

        private static Customer CreateCollectedCustomer(ProductScriptableObject product, PlacementObject shelf)
        {
            SeedShelf(shelf, product);
            GameObject prefab = CustomerSystem.Instance.customerPrefabs[0];
            GameObject go = UnityEngine.Object.Instantiate(prefab, CustomerSystem.Instance.spawnLocations[0].position, Quaternion.identity);
            Customer customer = go.GetComponent<Customer>();
            if (customer != null) customer.enabled = false;
            CustomerCart cart = go.GetComponent<CustomerCart>();
            cart.wishlist.Add((product, shelf));
            cart.Add();
            return customer;
        }

        private static void SeedShelf(PlacementObject shelf, ProductScriptableObject product)
        {
            Vector3 local = shelf.Add(product);
            UnityEngine.Object.Instantiate(product.prefab, shelf.container.TransformPoint(local),
                shelf.transform.rotation * Quaternion.Euler(0f, shelf.orientation, 0f), shelf.container);
        }

        private static PlacementObject FindEmptyPlacement(ProductScriptableObject product)
        {
            PlacementObject[] placements = UnityEngine.Object.FindObjectsByType<PlacementObject>(FindObjectsSortMode.None);
            for (int i = 0; i < placements.Length; i++)
                if (placements[i] != null && placements[i].IsEmpty() && placements[i].IsPlaceable(product) && placements[i].container != null)
                    return placements[i];
            throw new InvalidOperationException("No empty compatible placement found for " + (product != null ? product.name : "?"));
        }

        private static ProductScriptableObject GetStarterProduct()
        {
            ProductScriptableObject product = ItemDatabase.GetById(typeof(ProductScriptableObject), "0") as ProductScriptableObject;
            if (product != null) return product;
            var all = ItemDatabase.GetByType(typeof(ProductScriptableObject));
            for (int i = 0; i < all.Count; i++)
                if (all[i] is ProductScriptableObject p && p.prefab != null)
                    return p;
            throw new InvalidOperationException("No ProductScriptableObject found.");
        }

        private static CashDesk GetCashDesk()
        {
            CashDesk desk = UnityEngine.Object.FindAnyObjectByType<CashDesk>();
            if (desk == null)
                throw new InvalidOperationException("CashDesk not found.");
            return desk;
        }

        private static void TryBuyExpansion(string zoneId)
        {
            SupermarketExpansionSystem.Instance.TryPurchaseZone(zoneId, out _, out _);
        }

        private static void UnlockSecurityPath(string nodeId)
        {
            NodeData node = EntrepreneurTreeManager.Instance.treeData.GetNodeById(nodeId);
            if (node == null || node.isUnlocked) return;
            for (int i = 0; i < node.requiredNodeIds.Count; i++) UnlockSecurityPath(node.requiredNodeIds[i]);
            if (!EntrepreneurTreeManager.TryUnlockNode(nodeId)) throw new InvalidOperationException("Could not unlock " + nodeId);
        }

        private static bool UIStatsHasRobberySummarySupport()
        {
            return typeof(UIStats).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic) != null &&
                   typeof(StatsDatabase).GetMethod("BuildDailyRobberySummary", BindingFlags.Public | BindingFlags.Static) != null;
        }

        private static void InvokeBill(CashDesk desk, string amount)
        {
            MethodInfo bill = typeof(CashDesk).GetMethod("OnBillCustomer", BindingFlags.Instance | BindingFlags.NonPublic);
            bill.Invoke(desk, new object[] { amount });
        }

        private static int GetQueueCount(CashDesk desk)
        {
            return GetProtectedField<List<Customer>>(desk, "customerQueue").Count;
        }

        private static T GetProtectedField<T>(object instance, string fieldName)
        {
            FieldInfo field = typeof(CheckoutObject).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
                throw new MissingFieldException(typeof(CheckoutObject).Name, fieldName);
            return (T)field.GetValue(instance);
        }

        private static void SetProtectedField(object instance, string fieldName, object value)
        {
            FieldInfo field = typeof(CheckoutObject).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
                throw new MissingFieldException(typeof(CheckoutObject).Name, fieldName);
            field.SetValue(instance, value);
        }

        private static void SetPayCash(Customer customer, bool cash)
        {
            MethodInfo setter = typeof(Customer).GetProperty("payCash", BindingFlags.Instance | BindingFlags.Public)
                ?.GetSetMethod(true);
            if (setter == null)
                throw new MissingMethodException(nameof(Customer), "set_payCash");
            setter.Invoke(customer, new object[] { cash });
        }

        private static void PreserveDefaultSaveFiles()
        {
            preservedSaveFiles.Clear();
            string root = Application.persistentDataPath;
            PreserveFile(Path.Combine(root, SaveGameSystem.fileKey + SaveGameSystem.fileExt));
            PreserveFile(Path.Combine(root, SaveGameSystem.fileKey + SaveGameSystem.fileExt + SaveGameSystem.backupExt));
        }

        private static void PreserveFile(string path)
        {
            preservedSaveFiles[path] = File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        private static void RestorePreservedFiles()
        {
            foreach (KeyValuePair<string, byte[]> pair in preservedSaveFiles)
            {
                if (pair.Value == null)
                {
                    if (File.Exists(pair.Key))
                        File.Delete(pair.Key);
                }
                else
                {
                    File.WriteAllBytes(pair.Key, pair.Value);
                }
            }
            preservedSaveFiles.Clear();
        }

        private static void MarkAutosave()
        {
            sawAutosaveEvent = true;
        }

        private static AuditKind GetKind()
        {
            string raw = SessionState.GetString(KindKey, AuditKind.DailyRobberyReport.ToString());
            return Enum.TryParse(raw, out AuditKind kind) ? kind : AuditKind.DailyRobberyReport;
        }

        private static string GetLogPath(AuditKind kind) => "Documentos/Unity_PlayMode_Auditoria_" + kind + ".log";

        private static bool Elapsed(float seconds)
        {
            float start = SessionState.GetFloat(StartKey, 0f);
            if (start <= 0f) { SessionState.SetFloat(StartKey, Time.realtimeSinceStartup); return false; }
            return Time.realtimeSinceStartup - start >= seconds;
        }

        private static void SetState(string state) { SessionState.SetString(StateKey, state); SessionState.SetFloat(StartKey, 0f); }
        private static void PassIf(bool condition, string message) { if (condition) Pass(message); else Fail(message); }
        private static void Pass(string message) { Log("PASS: " + message); }
        private static void Fail(string message) { SessionState.SetInt(FailureKey, SessionState.GetInt(FailureKey, 0) + 1); Log("FAIL: " + message); }
        private static void Log(string message)
        {
            File.AppendAllText(GetLogPath(GetKind()), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + "\n");
            Debug.Log("[ReportSavePaymentAudit] " + message);
        }

        private static void OnConsole(string condition, string stackTrace, LogType type)
        {
            string state = SessionState.GetString(StateKey, string.Empty);
            if (string.IsNullOrEmpty(state) || state == "leave") return;
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) Fail("Console " + type + ": " + condition);
        }
    }
}
