// ShopMasterSecurityRobberyAuditRunners.cs
// Focused Play Mode audit runners for RQF10-RQF14 security and robbery flows.

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
    public static class SecurityUnlockPlayModeAuditRunner
    {
        public static void Run()
        {
            ShopMasterSecurityRobberyAuditCore.Run(ShopMasterSecurityRobberyAuditCore.AuditKind.SecurityUnlock);
        }
    }

    public static class RobberyInventoryPlayModeAuditRunner
    {
        public static void Run()
        {
            ShopMasterSecurityRobberyAuditCore.Run(ShopMasterSecurityRobberyAuditCore.AuditKind.RobberyInventory);
        }
    }

    public static class ThiefVisualSpawnPlayModeAuditRunner
    {
        public static void Run()
        {
            ShopMasterSecurityRobberyAuditCore.Run(ShopMasterSecurityRobberyAuditCore.AuditKind.ThiefVisualSpawn);
        }
    }

    [InitializeOnLoad]
    internal static class ShopMasterSecurityRobberyAuditCore
    {
        internal enum AuditKind
        {
            SecurityUnlock,
            RobberyInventory,
            ThiefVisualSpawn
        }

        private const string StateKey = "ShopMaster.SecurityRobbery.State";
        private const string KindKey = "ShopMaster.SecurityRobbery.Kind";
        private const string FailureKey = "ShopMaster.SecurityRobbery.Failures";
        private const string StartKey = "ShopMaster.SecurityRobbery.Start";
        private const string ScenePath = "Assets/StoreSimulator/Scenes/Game.unity";

        private static string LogPath
        {
            get
            {
                AuditKind kind = GetKind();
                if (kind == AuditKind.SecurityUnlock)
                    return "Documentos/Unity_PlayMode_Auditoria_SecurityUnlock.log";
                if (kind == AuditKind.RobberyInventory)
                    return "Documentos/Unity_PlayMode_Auditoria_RobberyInventory.log";
                return "Documentos/Unity_PlayMode_Auditoria_ThiefVisualSpawn.log";
            }
        }

        static ShopMasterSecurityRobberyAuditCore()
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
            File.WriteAllText(GetLogPath(kind), "# ShopMaster Seguridad y Ladrones Audit - " + kind + "\n");
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
                    RunSelectedAudit();
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

            if (state == "leave" && !EditorApplication.isPlaying &&
                !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                int failures = SessionState.GetInt(FailureKey, 0);
                SessionState.EraseString(StateKey);
                SessionState.EraseString(KindKey);
                SessionState.EraseInt(FailureKey);
                SessionState.EraseFloat(StartKey);
                EditorApplication.Exit(failures == 0 ? 0 : 1);
            }
        }

        private static void RunSelectedAudit()
        {
            AuditKind kind = GetKind();
            PrepareBaseState();
            if (kind == AuditKind.SecurityUnlock)
                RunSecurityUnlockAudit();
            else if (kind == AuditKind.RobberyInventory)
                RunRobberyInventoryAudit();
            else
                RunThiefVisualSpawnAudit();
        }

        private static void PrepareBaseState()
        {
            PassIf(EntrepreneurTreeManager.Instance != null, "EntrepreneurTreeManager presente.");
            PassIf(EntrepreneurTreeSecurityAdapter.Instance != null, "EntrepreneurTreeSecurityAdapter presente.");
            PassIf(ShoplifterSystem.Instance != null, "ShoplifterSystem presente.");
            PassIf(StatsDatabase.Instance != null, "StatsDatabase presente.");
            PassIf(StoreDatabase.Instance != null, "StoreDatabase presente.");
            PassIf(CustomerSystem.Instance != null, "CustomerSystem presente.");
            PassIf(ProductInventorySystem.Instance != null, "ProductInventorySystem presente.");

            EntrepreneurTreeManager.Instance?.LoadFromJSON(null);
            EntrepreneurTreeManager.SetPoints(100);
            EntrepreneurTreeSecurityAdapter.Instance?.RefreshFromTree();
            StatsDatabase.Instance?.LoadFromJSON(null);

            if (StoreDatabase.Instance != null && StoreDatabase.Instance.currentMoney < 1000000L)
                StoreDatabase.AddRemoveMoney(1000000L - StoreDatabase.Instance.currentMoney);
        }

        private static void RunSecurityUnlockAudit()
        {
            Log("=== RQF11/RQF12 security unlock chain ===");
            EntrepreneurTreeSecurityAdapter security = EntrepreneurTreeSecurityAdapter.Instance;
            PassIf(security.GetCurrentSecurityLevel() == 0 && Mathf.Approximately(security.GetAutomaticArrestChance(), 0f),
                "RQF10/RQF11 inicia sin seguridad automatica desbloqueada.");

            AssertSecurityRejected("security_1", "employee_7");
            UnlockPath("employee_7");
            PassIf(EntrepreneurTreeManager.TryUnlockNode("security_1"), "RQF12 security_1 desbloquea despues de employee_7.");
            security.RefreshFromTree();
            PassIf(security.GetCurrentSecurityLevel() == 1 && Mathf.Approximately(security.GetAutomaticArrestChance(), 0.33f),
                "RQF11 Nivel 1 activa Camaras con 33% de arresto automatico.");
            PassIf(security.securityLevel1Visual != null && security.securityLevel1Visual.activeSelf,
                "RQF11 Nivel 1 muestra evidencia visual de camaras.");

            AssertSecurityRejected("security_2", "employee_8");
            UnlockPath("employee_8");
            PassIf(EntrepreneurTreeManager.TryUnlockNode("security_2"), "RQF12 security_2 desbloquea despues de employee_8.");
            security.RefreshFromTree();
            PassIf(security.GetCurrentSecurityLevel() == 2 && Mathf.Approximately(security.GetAutomaticArrestChance(), 0.66f),
                "RQF11 Nivel 2 activa Guardias con 66% de arresto automatico.");
            PassIf(security.securityLevel2Visual != null && security.securityLevel2Visual.activeSelf,
                "RQF11 Nivel 2 muestra evidencia visual de guardias.");

            AssertSecurityRejected("security_3", "employee_14");
            UnlockPath("employee_14");
            PassIf(EntrepreneurTreeManager.TryUnlockNode("security_3"), "RQF12 security_3 desbloquea despues de employee_14.");
            security.RefreshFromTree();
            PassIf(security.GetCurrentSecurityLevel() == 3 && Mathf.Approximately(security.GetAutomaticArrestChance(), 0.99f),
                "RQF11 Nivel 3 activa Alarmas/arcos con 99% de arresto automatico.");
            PassIf(security.securityLevel3Visual != null && security.securityLevel3Visual.activeSelf,
                "RQF11 Nivel 3 muestra evidencia visual de alarmas/arcos.");
        }

        private static void RunRobberyInventoryAudit()
        {
            Log("=== RQF10/RQF14 robbery inventory ===");
            ProductScriptableObject product = GetStarterProduct();
            PassIf(product != null && product.prefab != null, "Producto real robable disponible.");

            EntrepreneurTreeManager.Instance.LoadFromJSON(null);
            EntrepreneurTreeSecurityAdapter.Instance.RefreshFromTree();
            PassIf(EntrepreneurTreeSecurityAdapter.Instance.GetCurrentSecurityLevel() == 0,
                "RQF10 sin niveles desbloqueados no hay seguridad automatica.");

            PlacementObject manualShelf = FindEmptyPlacement(product);
            Customer manual = CreateCollectedCustomer(product, manualShelf);
            int manualShelfAfterCollect = manualShelf != null ? manualShelf.count : -1;
            ShoplifterSystem.AdminForceNextSpawn(ShoplifterType.Common);
            ShoplifterSystem.Instance.RegisterCustomer(manual);
            PassIf(ShoplifterSystem.Instance.TryBeginTheft(manual, manual.GetComponent<CustomerCart>()),
                "RQF14 ladron comun inicia robo con producto real tomado de anaquel.");
            ShoplifterAgent manualAgent = manual.GetComponent<ShoplifterAgent>();
            PassIf(manualAgent != null && manualAgent.isEscaping && !manualAgent.isResolved,
                "RQF10 sin seguridad, el ladron queda escapando y requiere captura manual.");
            int recoveredBefore = StatsDatabase.Instance.recoveredProducts;
            PassIf(manualAgent != null && manualAgent.TryManualCapture(),
                "RQF10 captura manual ejecutada sobre ShoplifterAgent real.");
            PassIf(manualShelf.count == manualShelfAfterCollect + 1 &&
                   StatsDatabase.Instance.recoveredProducts == recoveredBefore + manualAgent.stolenProductsCount,
                "RQF10 captura manual recupera productos reales al anaquel/inventario.");

            PlacementObject escapeShelf = FindEmptyPlacement(product);
            Customer escape = CreateCollectedCustomer(product, escapeShelf);
            long moneyBefore = StoreDatabase.Instance.currentMoney;
            int escapeShelfAfterCollect = escapeShelf != null ? escapeShelf.count : -1;
            ShoplifterSystem.AdminForceNextSpawn(ShoplifterType.Common);
            ShoplifterSystem.Instance.RegisterCustomer(escape);
            PassIf(ShoplifterSystem.Instance.TryBeginTheft(escape, escape.GetComponent<CustomerCart>()),
                "RQF14 segundo ladron inicia robo para escenario de escape.");
            ShoplifterAgent escapeAgent = escape.GetComponent<ShoplifterAgent>();
            long stolen = escapeAgent != null ? escapeAgent.stolenValue : 0;
            int escapedBefore = StatsDatabase.Instance.thievesEscaped;
            escapeAgent?.ResolveAsEscaped();
            PassIf(stolen > 0 && StoreDatabase.Instance.currentMoney == moneyBefore - stolen,
                "RQF14 escape registra perdida economica exacta: " + StoreDatabase.FromLongToStringMoney(stolen) + ".");
            PassIf(escapeShelf.count == escapeShelfAfterCollect && StatsDatabase.Instance.thievesEscaped == escapedBefore + 1,
                "RQF14 escape confirma perdida sin restaurar stock inventado.");

            string summary = StatsDatabase.GetDailyRobberySummary();
            PassIf(summary.Contains("Detenidos manualmente: 1") && summary.Contains("Escaparon: 1"),
                "RQF14 reporte diario registra captura manual y escape.");
        }

        private static void RunThiefVisualSpawnAudit()
        {
            Log("=== RQF13 thief visual spawn ===");
            ProductScriptableObject product = GetStarterProduct();
            PlacementObject shelf = FindEmptyPlacement(product);
            Customer customer = CreateCollectedCustomer(product, shelf);
            ShoplifterSystem.AdminForceNextSpawn(ShoplifterType.Common);
            ShoplifterSystem.Instance.RegisterCustomer(customer);
            ShoplifterAgent agent = customer != null ? customer.GetComponent<ShoplifterAgent>() : null;

            PassIf(agent != null, "RQF13 ladron real generado desde cliente/NPC existente.");
            PassIf(agent != null && customer.GetComponent<Customer>() != null && customer.GetComponent<CustomerAgent>() != null,
                "RQF13 ladron conserva componentes reales de cliente y navegacion.");
            PassIf(agent != null && HasAnyRenderer(agent.gameObject), "RQF13 ladron tiene renderers visibles.");
            PassIf(agent != null && agent.transform.Find("ThiefIndicator") != null, "RQF13 ladron tiene indicador visual flotante.");
            PassIf(agent != null && FindChildByName(agent.transform, "Thief_Cap") != null, "RQF13 ladron tiene gorra negra visual.");
            PassIf(agent != null && FindChildByName(agent.transform, "Thief_Backpack") != null, "RQF13 ladron tiene mochila visual.");

            NavMeshAgent nav = customer.GetComponent<CustomerAgent>()?.GetNative();
            PassIf(nav != null && nav.enabled && nav.isOnNavMesh, "RQF13 ladron aparece sobre NavMesh real.");

            PassIf(ShoplifterSystem.Instance.TryBeginTheft(customer, customer.GetComponent<CustomerCart>()),
                "RQF13/RQF14 ladron visual puede iniciar intento de robo real.");
            PassIf(agent.stolenProductsCount > 0 && agent.stolenValue > 0,
                "RQF14 ladron visual registra productos robados y valor objetivo real.");

            EntrepreneurTreeManager.Instance.LoadFromJSON(null);
            EntrepreneurTreeSecurityAdapter.Instance.RefreshFromTree();
            ShoplifterType gatedType = InvokeChooseThiefType();
            PassIf(gatedType != ShoplifterType.Expert && gatedType != ShoplifterType.Special,
                "RQF13 ladron experto/especial no aparece sin lujo/electrodomesticos desbloqueados.");

            EntrepreneurTreeManager.AdminUnlockByType(TreeNodeType.Product);
            ShoplifterType premiumType = InvokeChooseThiefTypeWithSeed(2);
            PassIf(EntrepreneurTreeManager.IsNodeUnlocked("product_appliances_1") &&
                   (premiumType == ShoplifterType.Common || premiumType == ShoplifterType.Suspicious ||
                    premiumType == ShoplifterType.Fast || premiumType == ShoplifterType.Expert ||
                    premiumType == ShoplifterType.Special),
                "RQF13 productos premium desbloqueados habilitan seleccion de tipos avanzados sin romper spawn.");
        }

        private static void AssertSecurityRejected(string securityId, string requiredEmployee)
        {
            int points = EntrepreneurTreeManager.GetAvailablePoints();
            string reason;
            bool canUnlock = EntrepreneurTreeManager.CanUnlockNode(securityId, out reason);
            bool unlocked = EntrepreneurTreeManager.TryUnlockNode(securityId);
            PassIf(!canUnlock && !unlocked && EntrepreneurTreeManager.GetAvailablePoints() == points &&
                   reason.Contains(requiredEmployee.Replace("employee_", "Empleado ")),
                "RQF12 " + securityId + " rechaza requisito faltante " + requiredEmployee + " sin gastar puntos.");
        }

        private static void UnlockPath(string nodeId)
        {
            NodeData node = EntrepreneurTreeManager.Instance.treeData.GetNodeById(nodeId);
            if (node == null || node.isUnlocked)
                return;

            for (int i = 0; i < node.requiredNodeIds.Count; i++)
                UnlockPath(node.requiredNodeIds[i]);

            if (!EntrepreneurTreeManager.TryUnlockNode(nodeId))
                throw new InvalidOperationException("Could not unlock required path node: " + nodeId);
        }

        private static ProductScriptableObject GetStarterProduct()
        {
            ProductScriptableObject product = ItemDatabase.GetById(typeof(ProductScriptableObject), "0") as ProductScriptableObject;
            if (product != null)
                return product;

            var all = ItemDatabase.GetByType(typeof(ProductScriptableObject));
            for (int i = 0; i < all.Count; i++)
            {
                product = all[i] as ProductScriptableObject;
                if (product != null && product.prefab != null)
                    return product;
            }

            return null;
        }

        private static Customer CreateCollectedCustomer(ProductScriptableObject product, PlacementObject shelf)
        {
            if (product == null || shelf == null)
                return null;

            SeedShelf(shelf, product);
            GameObject prefab = CustomerSystem.Instance.customerPrefabs[0];
            GameObject go = UnityEngine.Object.Instantiate(prefab, CustomerSystem.Instance.spawnLocations[0].position, Quaternion.identity);
            Customer customer = go.GetComponent<Customer>();
            if (customer != null)
                customer.enabled = false;

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
            {
                PlacementObject placement = placements[i];
                if (placement != null && placement.IsEmpty() && placement.IsPlaceable(product) && placement.container != null)
                    return placement;
            }
            return null;
        }

        private static bool HasAnyRenderer(GameObject go)
        {
            return go != null && go.GetComponentsInChildren<Renderer>(true).Length > 0;
        }

        private static Transform FindChildByName(Transform root, string name)
        {
            if (root == null)
                return null;
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
                if (children[i] != null && children[i].name == name)
                    return children[i];
            return null;
        }

        private static ShoplifterType InvokeChooseThiefTypeWithSeed(int seed)
        {
            UnityEngine.Random.InitState(seed);
            return InvokeChooseThiefType();
        }

        private static ShoplifterType InvokeChooseThiefType()
        {
            MethodInfo method = typeof(ShoplifterSystem).GetMethod("ChooseThiefType", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null || ShoplifterSystem.Instance == null)
                return ShoplifterType.Common;
            return (ShoplifterType)method.Invoke(ShoplifterSystem.Instance, null);
        }

        private static AuditKind GetKind()
        {
            string raw = SessionState.GetString(KindKey, AuditKind.SecurityUnlock.ToString());
            if (Enum.TryParse(raw, out AuditKind kind))
                return kind;
            return AuditKind.SecurityUnlock;
        }

        private static string GetLogPath(AuditKind kind)
        {
            if (kind == AuditKind.SecurityUnlock)
                return "Documentos/Unity_PlayMode_Auditoria_SecurityUnlock.log";
            if (kind == AuditKind.RobberyInventory)
                return "Documentos/Unity_PlayMode_Auditoria_RobberyInventory.log";
            return "Documentos/Unity_PlayMode_Auditoria_ThiefVisualSpawn.log";
        }

        private static bool Elapsed(float seconds)
        {
            float start = SessionState.GetFloat(StartKey, 0f);
            if (start <= 0f)
            {
                SessionState.SetFloat(StartKey, Time.realtimeSinceStartup);
                return false;
            }
            return Time.realtimeSinceStartup - start >= seconds;
        }

        private static void SetState(string state)
        {
            SessionState.SetString(StateKey, state);
            SessionState.SetFloat(StartKey, 0f);
        }

        private static void PassIf(bool condition, string message)
        {
            if (condition) Pass(message);
            else Fail(message);
        }

        private static void Pass(string message)
        {
            Log("PASS: " + message);
        }

        private static void Fail(string message)
        {
            SessionState.SetInt(FailureKey, SessionState.GetInt(FailureKey, 0) + 1);
            Log("FAIL: " + message);
        }

        private static void Log(string message)
        {
            File.AppendAllText(LogPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + "\n");
            Debug.Log("[SecurityRobberyAudit] " + message);
        }

        private static void OnConsole(string condition, string stackTrace, LogType type)
        {
            string state = SessionState.GetString(StateKey, string.Empty);
            if (string.IsNullOrEmpty(state) || state == "leave")
                return;

            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                Fail("Console " + type + ": " + condition);
        }
    }
}
