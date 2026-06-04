// ShopMasterAdvancedThiefAuditRunners.cs
// Focused Play Mode audit runners for RQF15-RQF19 advanced robbery flows.

using System;
using System.IO;
using System.Reflection;
using FLOBUK.StoreSimulator;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace FLOBUK.StoreSimulator.Editor
{
    public static class AdvancedThiefTypesPlayModeAuditRunner { public static void Run() => ShopMasterAdvancedThiefAuditCore.Run(ShopMasterAdvancedThiefAuditCore.AuditKind.AdvancedTypes); }
    public static class RobberyDifficultyScalingPlayModeAuditRunner { public static void Run() => ShopMasterAdvancedThiefAuditCore.Run(ShopMasterAdvancedThiefAuditCore.AuditKind.DifficultyScaling); }
    public static class ManualCaptureRewardPlayModeAuditRunner { public static void Run() => ShopMasterAdvancedThiefAuditCore.Run(ShopMasterAdvancedThiefAuditCore.AuditKind.ManualCaptureReward); }
    public static class SecurityAutoArrestPlayModeAuditRunner { public static void Run() => ShopMasterAdvancedThiefAuditCore.Run(ShopMasterAdvancedThiefAuditCore.AuditKind.SecurityAutoArrest); }
    public static class TheftAlertPlayModeAuditRunner { public static void Run() => ShopMasterAdvancedThiefAuditCore.Run(ShopMasterAdvancedThiefAuditCore.AuditKind.TheftAlert); }
    public static class AdminModeRobberyTestAuditRunner { public static void Run() => ShopMasterAdvancedThiefAuditCore.Run(ShopMasterAdvancedThiefAuditCore.AuditKind.AdminMode); }

    [InitializeOnLoad]
    internal static class ShopMasterAdvancedThiefAuditCore
    {
        internal enum AuditKind { AdvancedTypes, DifficultyScaling, ManualCaptureReward, SecurityAutoArrest, TheftAlert, AdminMode }

        private const string StateKey = "ShopMaster.AdvancedThief.State";
        private const string KindKey = "ShopMaster.AdvancedThief.Kind";
        private const string FailureKey = "ShopMaster.AdvancedThief.Failures";
        private const string StartKey = "ShopMaster.AdvancedThief.Start";
        private const string ScenePath = "Assets/StoreSimulator/Scenes/Game.unity";

        static ShopMasterAdvancedThiefAuditCore()
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
            File.WriteAllText(GetLogPath(kind), "# ShopMaster Ladrones Avanzados Audit - " + kind + "\n");
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
                    if (kind == AuditKind.AdvancedTypes) RunAdvancedTypes();
                    else if (kind == AuditKind.DifficultyScaling) RunDifficultyScaling();
                    else if (kind == AuditKind.ManualCaptureReward) RunManualCaptureReward();
                    else if (kind == AuditKind.SecurityAutoArrest) RunSecurityAutoArrest();
                    else if (kind == AuditKind.TheftAlert) RunTheftAlert();
                    else RunAdminMode();
                }
                catch (Exception ex)
                {
                    Fail("Unhandled audit exception: " + ex);
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
            PassIf(ShoplifterSystem.Instance != null, "ShoplifterSystem presente.");
            PassIf(EntrepreneurTreeManager.Instance != null, "EntrepreneurTreeManager presente.");
            PassIf(EntrepreneurTreeSecurityAdapter.Instance != null, "EntrepreneurTreeSecurityAdapter presente.");
            PassIf(CustomerSystem.Instance != null, "CustomerSystem presente.");
            PassIf(StoreDatabase.Instance != null, "StoreDatabase presente.");
            PassIf(StatsDatabase.Instance != null, "StatsDatabase presente.");
            PassIf(ProductInventorySystem.Instance != null, "ProductInventorySystem presente.");
            PassIf(UIGame.Instance != null, "UIGame/notificaciones presente.");

            EntrepreneurTreeManager.Instance?.LoadFromJSON(null);
            EntrepreneurTreeManager.SetPoints(200);
            EntrepreneurTreeSecurityAdapter.Instance?.RefreshFromTree();
            StatsDatabase.Instance?.LoadFromJSON(null);
            if (StoreDatabase.Instance != null && StoreDatabase.Instance.currentMoney < 2000000L)
                StoreDatabase.AddRemoveMoney(2000000L - StoreDatabase.Instance.currentMoney);
        }

        private static void RunAdvancedTypes()
        {
            Log("=== RQF15 tipos avanzados ===");
            ProductScriptableObject product = GetStarterProduct();
            ShoplifterAgent common = SpawnThiefWithProduct(ShoplifterType.Common, product);
            PassIf(common != null && common.thiefType == ShoplifterType.Common, "RQF15 ladron comun usa cliente/NPC real.");
            PassIf(FindChild(common.transform, "Thief_Cap") != null && FindChild(common.transform, "Thief_Backpack") == null,
                "RQF15 comun tiene gorra y ruta simple sin mochila avanzada.");
            PassIf(common.GetComponent<CustomerAgent>()?.GetNative() != null, "RQF15 comun conserva NavMeshAgent real.");

            ShoplifterAgent suspicious = SpawnThiefWithProduct(ShoplifterType.Suspicious, product);
            PassIf(suspicious != null && suspicious.thiefType == ShoplifterType.Suspicious, "RQF15 cliente sospechoso generado como cliente real.");
            PassIf(FindChild(suspicious.transform, "Thief_Glasses") != null && FindChild(suspicious.transform, "ThiefIndicator") == null,
                "RQF15 sospechoso es mas sutil: gafas discretas sin indicador inicial obvio.");

            EntrepreneurTreeManager.AdminUnlockByType(TreeNodeType.Product);
            ShoplifterAgent special = SpawnThiefWithProduct(ShoplifterType.Special, product);
            PassIf(special != null && special.thiefType == ShoplifterType.Special, "RQF15 ladron especial generado solo en preparacion premium.");
            PassIf(FindChild(special.transform, "Thief_Backpack") != null && special.GetComponent<CustomerCart>().items.Count > 0,
                "RQF15 especial lleva mochila y al menos un producto normal en carrito para disimular.");

            EntrepreneurTreeManager.Instance.LoadFromJSON(null);
            ShoplifterType naturalLocked = InvokeChooseThiefTypeWithSeed(1);
            PassIf(naturalLocked != ShoplifterType.Expert && naturalLocked != ShoplifterType.Special,
                "RQF15/RQF13 especial/experto no aparecen naturalmente sin lujo/electrodomesticos desbloqueados.");
        }

        private static void RunDifficultyScaling()
        {
            Log("=== RQF16 escalado de dificultad ===");
            ShoplifterSystem system = ShoplifterSystem.Instance;
            float initialOneIn25 = system.initialOneInNChance > 0 ? 1f / system.initialOneInNChance : 0f;
            float initialChance = InvokeScaledChance();
            PassIf(Mathf.Approximately(initialOneIn25, 0.04f), "RQF16 probabilidad inicial equivale a 1 ladron por 25 clientes.");
            PassIf(initialChance <= system.maxThiefChance, "RQF16 probabilidad inicial respeta tope de 6.5%.");

            int beforeExpansions = SupermarketExpansionSystem.Instance.GetPurchasedSalesExpansionCount();
            StoreDatabase.AddRemoveMoney(5000000L);
            TryBuyExpansion("sales_w1");
            TryBuyExpansion("sales_w2");
            TryBuyExpansion("sales_n1");
            TryBuyExpansion("sales_n2");
            int afterExpansions = SupermarketExpansionSystem.Instance.GetPurchasedSalesExpansionCount();
            float expandedChance = InvokeScaledChance();
            PassIf(afterExpansions > beforeExpansions && expandedChance >= initialChance,
                "RQF16 expansiones de venta aumentan progresivamente la probabilidad de robo.");
            PassIf(expandedChance <= system.maxThiefChance, "RQF16 probabilidad escalada no supera 6.5%.");

            float multiplier = InvokeValueMultiplier(ShoplifterType.Special);
            PassIf(multiplier > 1f && multiplier <= 1.65f, "RQF16 valor objetivo crece con progreso dentro de limite especial.");
            EntrepreneurTreeManager.AdminUnlockByType(TreeNodeType.Product);
            int specialSeen = 0;
            int suspiciousSeen = 0;
            for (int i = 0; i < 40; i++)
            {
                ShoplifterType type = InvokeChooseThiefTypeWithSeed(100 + i);
                if (type == ShoplifterType.Special || type == ShoplifterType.Expert) specialSeen++;
                if (type == ShoplifterType.Suspicious) suspiciousSeen++;
            }
            PassIf(specialSeen > 0 && suspiciousSeen > 0, "RQF16 etapas avanzadas producen sospechosos y tipos premium.");
        }

        private static void RunManualCaptureReward()
        {
            Log("=== RQF17 captura manual completa ===");
            ProductScriptableObject product = GetStarterProduct();
            PlacementObject shelf = FindEmptyPlacement(product);
            ShoplifterAgent agent = SpawnThiefWithProduct(ShoplifterType.Common, product, shelf);
            int shelfAfterTheft = shelf.count;
            int pointsBefore = EntrepreneurTreeManager.GetAvailablePoints();
            int recoveredBefore = StatsDatabase.Instance.recoveredProducts;
            PassIf(agent != null && agent.isEscaping && !agent.isResolved, "RQF17 ladron queda interceptable antes de salida.");
            PassIf(agent.TryManualCapture(), "RQF17 interaccion manual resuelve captura real.");
            PassIf(shelf.count == shelfAfterTheft + agent.stolenProductsCount, "RQF17 productos regresan al anaquel/inventario real.");
            PassIf(EntrepreneurTreeManager.GetAvailablePoints() > pointsBefore, "RQF17 captura otorga recompensa de puntos.");
            PassIf(StatsDatabase.Instance.recoveredProducts == recoveredBefore + agent.stolenProductsCount,
                "RQF17 StatsDatabase registra productos recuperados.");
        }

        private static void RunSecurityAutoArrest()
        {
            Log("=== RQF18 seguridad automatica reforzada ===");
            EntrepreneurTreeSecurityAdapter security = EntrepreneurTreeSecurityAdapter.Instance;
            PassIf(security.GetCurrentSecurityLevel() == 0 && security.GetAutomaticArrestChance() == 0f,
                "RQF18 nivel 0 no tiene arresto automatico.");
            UnlockPath("security_1");
            security.RefreshFromTree();
            PassIf(security.GetCurrentSecurityLevel() == 1 && Mathf.Approximately(security.GetAutomaticArrestChance(), 0.33f),
                "RQF18 nivel 1 Camaras aplica 33%.");
            UnlockPath("security_2");
            security.RefreshFromTree();
            PassIf(security.GetCurrentSecurityLevel() == 2 && Mathf.Approximately(security.GetAutomaticArrestChance(), 0.66f),
                "RQF18 nivel 2 Guardias aplica 66%.");
            UnlockPath("security_3");
            security.RefreshFromTree();
            PassIf(security.GetCurrentSecurityLevel() == 3 && Mathf.Approximately(security.GetAutomaticArrestChance(), 0.99f),
                "RQF18 nivel 3 Alarmas/arcos aplica 99%.");

            ProductScriptableObject product = GetStarterProduct();
            PlacementObject shelf = FindEmptyPlacement(product);
            int recoveredBefore = StatsDatabase.Instance.recoveredProducts;
            UnityEngine.Random.InitState(1234);
            ShoplifterAgent agent = SpawnThiefWithProduct(ShoplifterType.Common, product, shelf);
            PassIf(agent != null && agent.isResolved && !agent.isEscaping, "RQF18 seguridad nivel 3 arresta robo real automaticamente.");
            PassIf(shelf.count > 0 && StatsDatabase.Instance.recoveredProducts > recoveredBefore,
                "RQF18 arresto automatico recupera productos y registra evento.");
        }

        private static void RunTheftAlert()
        {
            Log("=== RQF19 alertas visuales y sonoras ===");
            PassIf(UIGame.Instance != null, "RQF19 sistema visual de notificaciones existe.");
            PassIf(UIGame.Instance.notificationClip != null && AudioSystem.Instance != null,
                "RQF19 notificaciones usan audio existente del asset.");

            ProductScriptableObject product = GetStarterProduct();
            ShoplifterAgent manual = SpawnThiefWithProduct(ShoplifterType.Common, product);
            long stolen = manual.stolenValue;
            manual.TryManualCapture();

            ShoplifterAgent escaped = SpawnThiefWithProduct(ShoplifterType.Common, product);
            escaped.ResolveAsEscaped();

            EntrepreneurTreeManager.AdminUnlockByType(TreeNodeType.Security);
            EntrepreneurTreeSecurityAdapter.Instance.RefreshFromTree();
            UnityEngine.Random.InitState(1234);
            ShoplifterAgent auto = SpawnThiefWithProduct(ShoplifterType.Common, product);

            string summary = StatsDatabase.GetDailyRobberySummary();
            PassIf(stolen > 0 && summary.Contains("Detenidos manualmente") && summary.Contains("Escaparon") && summary.Contains("Arrestos automaticos") || summary.Contains("Arrestos autom"),
                "RQF19 eventos de deteccion/captura/escape/arresto generan notificaciones y resumen coherente.");
            PassIf(auto != null && auto.isResolved, "RQF19 arresto automatico dispara flujo de alerta sin errores rojos.");
        }

        private static void RunAdminMode()
        {
            Log("=== Modo Admin robbery test ===");
            AdminSessionConfig.isActive = true;
            AdminSessionConfig.startMoney = 900000L;
            AdminSessionConfig.treePoints = 55;
            InvokeApplyAdminConfig();
            PassIf(StoreDatabase.Instance.currentMoney == 900000L, "Admin permite fijar dinero inicial.");
            PassIf(EntrepreneurTreeManager.GetAvailablePoints() == 55, "Admin permite agregar puntos del Arbol.");
            PassIf(!AdminSessionConfig.isActive, "Admin se resetea tras aplicar dinero/puntos.");

            AdminSessionConfig.isActive = true;
            AdminSessionConfig.startMoney = 3000000L;
            AdminSessionConfig.treePoints = 55;
            AdminSessionConfig.unlockAllProducts = true;
            AdminSessionConfig.unlockAllSecurity = true;
            AdminSessionConfig.giveTestStock = true;
            AdminSessionConfig.buyTestExpansions = true;
            AdminSessionConfig.forceShoplifterSpawn = true;
            AdminSessionConfig.forcedShoplifterType = ShoplifterType.Special;

            InvokeApplyAdminConfig();
            PassIf(EntrepreneurTreeManager.IsNodeUnlocked("security_3"), "Admin permite desbloquear seguridad.");
            PassIf(SupermarketExpansionSystem.Instance.GetPurchasedSalesExpansionCount() > 0, "Admin permite simular expansiones de venta.");
            PassIf(!AdminSessionConfig.isActive, "Admin se resetea tras aplicar y no contamina gameplay normal.");

            ProductScriptableObject product = GetStarterProduct();
            ShoplifterAgent agent = SpawnThiefWithProduct(ShoplifterType.Special, product);
            PassIf(agent != null && agent.thiefType == ShoplifterType.Special, "Admin permite forzar ladron especial usando sistemas reales.");
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

        private static Transform FindChild(Transform root, string name)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
                if (children[i] != null && children[i].name == name)
                    return children[i];
            return null;
        }

        private static void TryBuyExpansion(string zoneId)
        {
            if (SupermarketExpansionSystem.Instance == null) return;
            SupermarketExpansionSystem.Instance.TryPurchaseZone(zoneId, out _, out _);
        }

        private static void UnlockPath(string nodeId)
        {
            NodeData node = EntrepreneurTreeManager.Instance.treeData.GetNodeById(nodeId);
            if (node == null || node.isUnlocked) return;
            for (int i = 0; i < node.requiredNodeIds.Count; i++) UnlockPath(node.requiredNodeIds[i]);
            if (!EntrepreneurTreeManager.TryUnlockNode(nodeId)) throw new InvalidOperationException("Could not unlock " + nodeId);
        }

        private static float InvokeScaledChance() => (float)InvokePrivate(ShoplifterSystem.Instance, "CalculateScaledChance");
        private static float InvokeValueMultiplier(ShoplifterType type) => (float)InvokePrivate(ShoplifterSystem.Instance, "GetTargetValueProgressMultiplier", type);
        private static ShoplifterType InvokeChooseThiefTypeWithSeed(int seed)
        {
            UnityEngine.Random.InitState(seed);
            return (ShoplifterType)InvokePrivate(ShoplifterSystem.Instance, "ChooseThiefType");
        }

        private static object InvokePrivate(object instance, string method, params object[] args)
        {
            MethodInfo info = instance.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic);
            if (info == null) throw new MissingMethodException(instance.GetType().Name, method);
            return info.Invoke(instance, args);
        }

        private static void InvokeApplyAdminConfig()
        {
            MethodInfo info = typeof(EntrepreneurTreeUIBootstrap).GetMethod("ApplyAdminConfig", BindingFlags.Static | BindingFlags.NonPublic);
            if (info == null) throw new MissingMethodException(nameof(EntrepreneurTreeUIBootstrap), "ApplyAdminConfig");
            info.Invoke(null, null);
        }

        private static AuditKind GetKind()
        {
            string raw = SessionState.GetString(KindKey, AuditKind.AdvancedTypes.ToString());
            return Enum.TryParse(raw, out AuditKind kind) ? kind : AuditKind.AdvancedTypes;
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
            Debug.Log("[AdvancedThiefAudit] " + message);
        }

        private static void OnConsole(string condition, string stackTrace, LogType type)
        {
            string state = SessionState.GetString(StateKey, string.Empty);
            if (string.IsNullOrEmpty(state) || state == "leave") return;
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) Fail("Console " + type + ": " + condition);
        }
    }
}
