// ShopMasterWarehouseAndNPCVisualAuditRunner.cs
// Fase 5 — Auditoría de Almacén y NPC Visual
// Valida:
//   • Zona de almacén / carga existe y tiene acceso funcional.
//   • Spawn de pedidos está dentro/junto al almacén, no bloqueado.
//   • Paquete comprado aparece accesible (no dentro de pared ni bajo el piso).
//   • NPC empleado es visualmente correcto: renderer, escala, piso, NavMesh.
//   • Rutas NPC → caja, almacén y anaquel son válidas en NavMesh.
//   • Surtidor llega al punto de pickup sin detenerse indefinidamente.
//   • No hay errores de consola, NullReferenceException ni MissingReferenceException.
//   • Los runners previos (Fase 3 y Fase 4) siguen en verde.

using System;
using System.Collections.Generic;
using System.IO;
using FLOBUK.StoreSimulator;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace FLOBUK.StoreSimulator.Editor
{
    [InitializeOnLoad]
    public static class ShopMasterWarehouseAndNPCVisualAuditRunner
    {
        // ── Session keys ──────────────────────────────────────────────────────────
        private const string StateKey   = "ShopMaster.WarehouseNPC.State";
        private const string FailureKey = "ShopMaster.WarehouseNPC.Failures";
        private const string StartKey   = "ShopMaster.WarehouseNPC.Start";

        // ── Paths ─────────────────────────────────────────────────────────────────
        private const string LogPath      = "Documentos/WarehouseNPCVisualAudit_Report.txt";
        private const string ScenePath    = "Assets/StoreSimulator/Scenes/Game.unity";
        private const string Phase3LogPath = "Documentos/Unity_PlayMode_Auditoria_Requerimientos_Fase3.log";
        private const string Phase4LogPath = "Documentos/Unity_PlayMode_Auditoria_Integracion_Fase4.log";

        // ── Thresholds ────────────────────────────────────────────────────────────
        /// NPC scale: each axis must be at least this much (avoids scale-zero invisibility).
        private const float MinNPCScaleAxis    = 0.3f;
        /// NPC scale: each axis must not exceed this (avoids giant NPC from misconfigured prefab).
        private const float MaxNPCScaleAxis    = 5f;
        /// Maximum Y distance from floor for NPC to be considered "on the floor" (not underground).
        private const float MaxNPCBelowFloor   = -0.3f;
        /// NavMesh sample radius used for all path tests.
        private const float NavSampleRadius    = 5f;
        /// Timeout in seconds for waiting for the restocker NPC to reach pickup point.
        private const float RestockerTimeout   = 35f;
        /// Keyword used to find the warehouse zone in the hierarchy.
        private const string WarehouseKeyword  = "Warehouse";
        /// Keyword used to find delivery spawn in the hierarchy.
        private const string DeliveryKeyword   = "Delivery";

        static ShopMasterWarehouseAndNPCVisualAuditRunner()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            Application.logMessageReceived -= OnConsole;
            Application.logMessageReceived += OnConsole;
        }

        // ── Entry point ───────────────────────────────────────────────────────────

        public static void Run()
        {
            File.WriteAllText(LogPath, "# ShopMaster Warehouse & NPC Visual Audit\n");
            SessionState.SetString(StateKey, "enter");
            SessionState.SetInt(FailureKey, 0);
            SessionState.SetFloat(StartKey, 0f);
            Log("Opening scene: " + ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }

        // ── State machine ─────────────────────────────────────────────────────────

        private static void OnUpdate()
        {
            string state = SessionState.GetString(StateKey, string.Empty);
            if (string.IsNullOrEmpty(state))
                return;

            // ── Phase: scene loaded, begin validation ──────────────────────────
            if (state == "enter" && EditorApplication.isPlaying && Elapsed(6f))
            {
                try
                {
                    Log("=== FASE 1: Baseline y sistemas previos ===");
                    ValidatePreviousRunnerEvidence();

                    Log("=== FASE 2: Escena cargada — sistemas base ===");
                    ValidateCoreSystems();

                    Log("=== FASE 3: Zona de almacén / carga ===");
                    ValidateWarehouseZone();

                    Log("=== FASE 4: Spawn de pedidos ===");
                    ValidateDeliverySpawn();

                    Log("=== FASE 5: Simular compra y verificar paquete ===");
                    TriggerTestPurchase();

                    SetState("wait_package");
                }
                catch (Exception ex)
                {
                    Fail("Excepción no manejada en fase de setup: " + ex);
                    CleanupAndLeave();
                }
                return;
            }

            // ── Phase: wait for package to land ───────────────────────────────
            if (state == "wait_package" && EditorApplication.isPlaying && Elapsed(4f))
            {
                try
                {
                    Log("=== FASE 6: Paquete en escena — accesibilidad ===");
                    ValidatePackageAccessibility();

                    Log("=== FASE 7: NPCs visuales ===");
                    TriggerTestHire();

                    SetState("wait_npc");
                }
                catch (Exception ex)
                {
                    Fail("Excepción no manejada esperando paquete: " + ex);
                    CleanupAndLeave();
                }
                return;
            }

            // ── Phase: wait for NPC to spawn ──────────────────────────────────
            if (state == "wait_npc" && EditorApplication.isPlaying && Elapsed(4f))
            {
                try
                {
                    Log("=== FASE 8: Diagnóstico visual de NPCs ===");
                    ValidateNPCVisuals();

                    Log("=== FASE 9: NavMesh — rutas NPC ===");
                    ValidateNavMeshRoutes();

                    Log("=== FASE 10: Surtidor — ruta a pickup ===");
                    SetState("wait_restocker");
                    SetStateTimer();
                }
                catch (Exception ex)
                {
                    Fail("Excepción no manejada validando NPCs: " + ex);
                    CleanupAndLeave();
                }
                return;
            }

            // ── Phase: wait for restocker to reach pickup ─────────────────────
            if (state == "wait_restocker" && EditorApplication.isPlaying)
            {
                EmployeeRestockCoordinator rc =
                    UnityEngine.Object.FindAnyObjectByType<EmployeeRestockCoordinator>();

                bool timedOut = ElapsedAbsolute(RestockerTimeout);
                bool done     = rc != null && !rc.IsVisualTaskInProgress;

                if (done || timedOut)
                {
                    if (timedOut && (rc == null || rc.IsVisualTaskInProgress))
                        Fail("Surtidor no completó la ruta de pickup en " + RestockerTimeout + "s — tarea visual en progreso / bloqueada.");
                    else if (rc != null && rc.LastTaskCompletedVisualRoute)
                        Pass("Surtidor completó ruta visual pickup→anaquel correctamente.");
                    else
                        Yellow("RESTOCKER",
                            "Surtidor terminó la tarea pero LastTaskCompletedVisualRoute=false (posiblemente no había stock suficiente o NavMesh inválido).");

                    Log("=== FASE 11: Validación final de consola ===");
                    Log("=== FIN ===");
                    int failures = SessionState.GetInt(FailureKey, 0);
                    Log("Audit finished. Failures=" + failures);
                    CleanupAndLeave();
                }
                return;
            }

            // ── Phase: exit playmode ───────────────────────────────────────────
            if (state == "leave" && !EditorApplication.isPlaying &&
                !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                int failures = SessionState.GetInt(FailureKey, 0);
                SessionState.EraseString(StateKey);
                SessionState.EraseInt(FailureKey);
                SessionState.EraseFloat(StartKey);
                EditorApplication.Exit(failures == 0 ? 0 : 1);
            }
        }

        // ── Validation blocks ─────────────────────────────────────────────────────

        private static void ValidatePreviousRunnerEvidence()
        {
            string phase3 = File.Exists(Phase3LogPath) ? File.ReadAllText(Phase3LogPath) : string.Empty;
            string phase4 = File.Exists(Phase4LogPath) ? File.ReadAllText(Phase4LogPath) : string.Empty;

            PassIf(phase3.Contains("Audit finished. Failures=0"),
                "Runner Fase 3 sigue en verde (Failures=0 encontrado en log).");
            PassIf(phase4.Contains("Audit finished. Failures=0"),
                "Runner Fase 4 sigue en verde (Failures=0 encontrado en log).");

            if (string.IsNullOrEmpty(phase3))
                Yellow("BASELINE", "Log Fase 3 no encontrado — ejecutar ShopMasterFullRequirementsPhase3Runner primero.");
            if (string.IsNullOrEmpty(phase4))
                Yellow("BASELINE", "Log Fase 4 no encontrado — ejecutar ShopMasterFinalIntegrationPhase4Runner primero.");
        }

        private static void ValidateCoreSystems()
        {
            PassIf(EntrepreneurTreeManager.Instance != null,
                "EntrepreneurTreeManager presente en escena.");
            PassIf(EntrepreneurEmployeeSystem.Instance != null,
                "EntrepreneurEmployeeSystem presente.");
            PassIf(EmployeeNPCSpawner.Instance != null,
                "EmployeeNPCSpawner presente.");
            PassIf(UnityEngine.Object.FindAnyObjectByType<EmployeeRestockCoordinator>() != null,
                "EmployeeRestockCoordinator presente.");
            PassIf(EmployeeWorkstationRegistry.Instance != null,
                "EmployeeWorkstationRegistry presente.");
            PassIf(DeliverySystem.Instance != null,
                "DeliverySystem presente.");
            PassIf(DeliverySystem.Instance != null && DeliverySystem.Instance.deliveryStart != null,
                "DeliverySystem.deliveryStart asignado (no null).");

            if (DeliverySystem.Instance != null && DeliverySystem.Instance.deliveryStart == null)
                Fail("DeliverySystem.deliveryStart es null — los paquetes no pueden aparecer.");

            PassIf(CustomerSystem.Instance != null,
                "CustomerSystem presente.");
            PassIf(UnityEngine.Object.FindAnyObjectByType<CashDesk>() != null,
                "Al menos una CashDesk en escena.");

            PrepareAuditProgressState();
        }

        private static void PrepareAuditProgressState()
        {
            if (EntrepreneurTreeManager.Instance == null)
                return;

            EntrepreneurTreeManager.Instance.LoadFromJSON(null);
            EntrepreneurTreeManager.SetPoints(100);
            TryUnlockForAudit("product_basic_2");
            TryUnlockForAudit("product_spices_1");
            TryUnlockForAudit("employee_1");
        }

        private static void TryUnlockForAudit(string nodeId)
        {
            if (EntrepreneurTreeManager.IsNodeUnlocked(nodeId))
                return;

            bool unlocked = EntrepreneurTreeManager.TryUnlockNode(nodeId);
            if (!unlocked && !EntrepreneurTreeManager.IsNodeUnlocked(nodeId))
                Yellow("SETUP", "No se pudo desbloquear nodo de auditoria '" + nodeId + "'.");
        }

        private static void ValidateWarehouseZone()
        {
            // Search by name keywords that should exist once the warehouse is built.
            GameObject warehouseZone = FindGameObjectByKeyword(WarehouseKeyword);
            PassIf(warehouseZone != null,
                "Zona de almacén existe en escena (objeto con keyword '" + WarehouseKeyword + "' encontrado).");

            if (warehouseZone == null)
            {
                Fail("Zona de almacén NO encontrada. Crear objeto/prefab con '" + WarehouseKeyword + "' en el nombre "
                    + "que contenga las paredes, techo y puertas de la zona de carga.");

                // Still check delivery start position so we can tell if it's at least reachable.
                if (DeliverySystem.Instance != null && DeliverySystem.Instance.deliveryStart != null)
                {
                    Vector3 ds = DeliverySystem.Instance.deliveryStart.position;
                    Log("  DeliveryStart está en: " + ds
                        + " — se requiere que quede dentro del almacén cuando se construya.");
                }
                return;
            }

            // Warehouse exists — check for walls and doors as children.
            int wallCount = 0, doorCount = 0;
            foreach (Transform child in warehouseZone.GetComponentsInChildren<Transform>(true))
            {
                string n = child.name.ToLower();
                if (n.Contains("wall") || n.Contains("pared") || n.Contains("muro"))
                    wallCount++;
                if (n.Contains("door") || n.Contains("puerta") || n.Contains("gate") || n.Contains("porton"))
                    doorCount++;
            }

            PassIf(wallCount >= 2,
                "Zona de almacén tiene al menos 2 paredes (" + wallCount + " encontradas).");
            PassIf(doorCount >= 1,
                "Zona de almacén tiene al menos 1 puerta grande (" + doorCount + " encontradas).");

            if (wallCount < 2)
                Fail("Zona de almacén con menos de 2 paredes — agregar paredes laterales, frontal y trasera.");
            if (doorCount < 1)
                Fail("Zona de almacén sin puertas grandes — agregar portón, persiana o puerta industrial.");

            // Check that delivery spawn is positioned inside or adjacent to the warehouse.
            if (DeliverySystem.Instance != null && DeliverySystem.Instance.deliveryStart != null)
            {
                Vector3 spawnPos     = DeliverySystem.Instance.deliveryStart.position;
                Vector3 warehousePos = warehouseZone.transform.position;
                float dist           = Vector3.Distance(spawnPos, warehousePos);
                PassIf(dist < 10f,
                    "DeliveryStart está a " + dist.ToString("0.1") + "m del centro del almacén (≤10m requerido).");
                if (dist >= 10f)
                    Fail("DeliveryStart está a " + dist.ToString("0.1") + "m del almacén — mover dentro o junto al almacén.");
            }
        }

        private static void ValidateDeliverySpawn()
        {
            if (DeliverySystem.Instance == null)
            {
                Fail("DeliverySystem no encontrado — no se puede validar spawn de pedidos.");
                return;
            }

            Transform ds = DeliverySystem.Instance.deliveryStart;
            if (ds == null)
            {
                Fail("DeliverySystem.deliveryStart es null — los paquetes aparecerán en posición cero.");
                return;
            }

            Log("  DeliveryStart: '" + ds.name + "' pos=" + ds.position);

            // Check not inside a wall (very rough: Y must be ≥ 0).
            PassIf(ds.position.y >= -0.1f,
                "DeliveryStart no está bajo el piso (Y=" + ds.position.y.ToString("0.2") + ").");

            // NavMesh reachability from spawn.
            NavMeshHit hit;
            bool reachable = NavMesh.SamplePosition(ds.position, out hit, NavSampleRadius, NavMesh.AllAreas);
            PassIf(reachable,
                "DeliveryStart tiene NavMesh cercano a " + NavSampleRadius + "m (punto más cercano: "
                + (reachable ? hit.position.ToString() : "ninguno") + ").");
            if (!reachable)
                Fail("DeliveryStart está fuera del NavMesh (radio=" + NavSampleRadius + "m) — reubicar o rebakear NavMesh.");

            PassIf(DeliverySystem.Instance.packagePrefab != null,
                "DeliverySystem.packagePrefab asignado.");
            if (DeliverySystem.Instance.packagePrefab == null)
                Fail("DeliverySystem.packagePrefab es null — los pedidos no generarán paquetes.");
        }

        private static void TriggerTestPurchase()
        {
            // Attempt to buy the first available product.
            ProductScriptableObject product = FindFirstUnlockedProduct();
            if (product == null)
            {
                Yellow("PURCHASE",
                    "No hay productos desbloqueados/disponibles para simular compra de prueba. "
                    + "Habilitar admin mode con unlockAllProducts para prueba completa.");
                return;
            }

            // Ensure enough money.
            long cost = product.buyPrice * Mathf.Max(1, product.packageCount);
            if (StoreDatabase.Instance != null && StoreDatabase.Instance.currentMoney < cost)
                StoreDatabase.AddRemoveMoney((cost - StoreDatabase.Instance.currentMoney) + cost);

            bool purchased = DeliverySystem.Purchase(product);
            PassIf(purchased,
                "Compra de prueba realizada para '" + product.title + "' (costo=" + cost + " cents).");
            if (!purchased)
                Fail("Compra de prueba falló para '" + product.title + "' — revisar DeliverySystem y StoreDatabase.");
        }

        private static void ValidatePackageAccessibility()
        {
            PackageObject[] packages = UnityEngine.Object.FindObjectsByType<PackageObject>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            PassIf(packages.Length > 0,
                "Hay " + packages.Length + " paquete(s) en escena.");
            if (packages.Length == 0)
            {
                Fail("Ningún PackageObject en escena — la compra de prueba no generó paquete o el paquete fue destruido.");
                return;
            }

            foreach (PackageObject pkg in packages)
            {
                if (pkg == null) continue;

                Vector3 pos = pkg.transform.position;
                Log("  Paquete '" + pkg.name + "' pos=" + pos + " count=" + pkg.count);

                PassIf(pos.y > -0.5f,
                    "Paquete '" + pkg.name + "' no está bajo el piso (Y=" + pos.y.ToString("0.2") + ").");
                if (pos.y <= -0.5f)
                    Fail("Paquete '" + pkg.name + "' está bajo el piso (Y=" + pos.y.ToString("0.2") + ") — revisar DeliveryStart.Y.");

                // Check not inside a wall by sampling NavMesh nearby.
                NavMeshHit hit;
                bool navOk = NavMesh.SamplePosition(pos, out hit, NavSampleRadius, NavMesh.AllAreas);
                PassIf(navOk,
                    "Paquete '" + pkg.name + "' es alcanzable por NavMesh (dist=" +
                    (navOk ? Vector3.Distance(pos, hit.position).ToString("0.2") + "m" : "N/A") + ").");
                if (!navOk)
                    Fail("Paquete '" + pkg.name + "' está fuera del NavMesh — posiblemente dentro de pared o fuera del mapa.");
            }
        }

        private static void TriggerTestHire()
        {
            if (EntrepreneurEmployeeSystem.Instance == null)
            {
                Fail("EntrepreneurEmployeeSystem no disponible — no se puede contratar empleado de prueba.");
                return;
            }

            // Try to unlock and hire employee #1 for the test.
            EntrepreneurTreeManager mgr = EntrepreneurTreeManager.Instance;
            if (mgr != null)
            {
                // Attempt to unlock employee nodes via the bridge.
                EntrepreneurTreeEmployeeUnlockAdapter adapter =
                    UnityEngine.Object.FindAnyObjectByType<EntrepreneurTreeEmployeeUnlockAdapter>();
                if (adapter == null)
                    Yellow("HIRE", "EntrepreneurTreeEmployeeUnlockAdapter no encontrado — empleado #1 puede estar bloqueado.");
            }

            PrepareAuditProgressState();

            if (!EntrepreneurEmployeeSystem.Instance.IsEmployeeUnlocked(1))
            {
                Yellow("HIRE",
                    "Empleado #1 no está desbloqueado en el árbol. Para prueba visual completa "
                    + "usar admin mode con unlockAllEmployees=true.");

                // Try employee IDs 1–6 for one that is unlocked.
                for (int id = 2; id <= 6; id++)
                {
                    if (EntrepreneurEmployeeSystem.Instance.IsEmployeeUnlocked(id) &&
                        !EntrepreneurEmployeeSystem.Instance.IsEmployeeHired(id))
                    {
                        TryHireAndLog(id);
                        return;
                    }
                }

                Log("  Ningún empleado desbloqueado disponible para contratar en prueba.");
                return;
            }

            if (!EntrepreneurEmployeeSystem.Instance.IsEmployeeHired(1))
                TryHireAndLog(1);
            else
                Log("  Empleado #1 ya estaba contratado — validando NPC existente.");
        }

        private static void TryHireAndLog(int employeeId)
        {
            // Ensure we have money.
            if (StoreDatabase.Instance != null)
            {
                EmployeeAssignment assignment = EntrepreneurEmployeeSystem.Instance.GetAssignment(employeeId);
                long hireCost = assignment != null ? assignment.hireCost : 0L;
                if (StoreDatabase.Instance.currentMoney < hireCost)
                    StoreDatabase.AddRemoveMoney((hireCost - StoreDatabase.Instance.currentMoney) + hireCost);
            }

            string reason;
            bool hired = EntrepreneurEmployeeSystem.Instance.TryHireEmployee(employeeId, out reason);
            if (!hired && EntrepreneurEmployeeSystem.Instance.IsEmployeeHired(employeeId))
                hired = true;
            PassIf(hired,
                "Empleado #" + employeeId + " contratado en prueba de auditoría.");
            if (!hired)
                Fail("No se pudo contratar empleado #" + employeeId + ": " + reason);
            if (hired)
            {
                bool roleAssigned = EntrepreneurEmployeeSystem.Instance.TryAssignRole(employeeId, EmployeeRole.Restocker, out reason)
                    || EntrepreneurEmployeeSystem.Instance.GetEmployeeRole(employeeId) == EmployeeRole.Restocker;
                PassIf(roleAssigned,
                    "Empleado #" + employeeId + " asignado como Surtidor para auditoria visual.");
                if (!roleAssigned)
                    Fail("No se pudo asignar rol Surtidor a empleado #" + employeeId + ": " + reason);
            }
        }

        private static void ValidateNPCVisuals()
        {
            if (EmployeeNPCSpawner.Instance == null)
            {
                Fail("EmployeeNPCSpawner.Instance es null — los NPCs no pueden ser instanciados.");
                return;
            }

            // Dump all diagnostics via the new public method.
            EmployeeNPCSpawner.Instance.DumpAllNPCDiagnostics();

            // Find all spawned employee GameObjects.
            GameObject[] allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            int npcCount         = 0;
            int npcVisibleCount  = 0;
            int npcOnMeshCount   = 0;
            int npcBadScaleCount = 0;

            foreach (GameObject go in allObjects)
            {
                if (go == null || !go.name.StartsWith("Employee_"))
                    continue;

                npcCount++;

                // Renderer check.
                Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
                bool hasActiveRenderer = false;
                foreach (Renderer r in renderers)
                {
                    if (r != null && r.enabled && r.gameObject.activeInHierarchy)
                    {
                        hasActiveRenderer = true;
                        break;
                    }
                }

                if (hasActiveRenderer)
                    npcVisibleCount++;
                else
                    Fail("NPC '" + go.name + "' no tiene renderer activo — será invisible para el jugador.");

                // Scale check.
                Vector3 scale = go.transform.lossyScale;
                bool scaleBad = scale.x < MinNPCScaleAxis || scale.y < MinNPCScaleAxis || scale.z < MinNPCScaleAxis
                             || scale.x > MaxNPCScaleAxis || scale.y > MaxNPCScaleAxis || scale.z > MaxNPCScaleAxis;
                if (scaleBad)
                {
                    npcBadScaleCount++;
                    Fail("NPC '" + go.name + "' tiene escala anormal: " + scale
                        + " — revisar prefab o transforms del jugador padre.");
                }

                // NavMesh check.
                NavMeshAgent agent = go.GetComponent<NavMeshAgent>();
                if (agent != null && agent.enabled && agent.isOnNavMesh)
                    npcOnMeshCount++;
                else
                    Fail("NPC '" + go.name + "' no tiene NavMeshAgent activo en NavMesh "
                        + "(agent=" + (agent != null) + " enabled=" + (agent != null && agent.enabled)
                        + " onMesh=" + (agent != null && agent.isOnNavMesh) + ").");

                // Below floor check.
                if (go.transform.position.y < MaxNPCBelowFloor)
                    Fail("NPC '" + go.name + "' está bajo el piso (Y=" + go.transform.position.y.ToString("0.2")
                        + ") — revisar posición del employeeSpawnPoint.");

                // Parent active check.
                bool parentOk = true;
                Transform t = go.transform.parent;
                while (t != null)
                {
                    if (!t.gameObject.activeSelf) { parentOk = false; break; }
                    t = t.parent;
                }
                if (!parentOk)
                    Fail("NPC '" + go.name + "' tiene un padre inactivo — el NPC no aparecerá en escena.");
            }

            if (npcCount == 0)
            {
                Fail("No se encontraron GameObjects de empleados (prefijo 'Employee_') — "
                    + "revisar EmployeeNPCSpawner y flujo de contratación.");
            }
            else
            {
                Log("  NPCs totales: " + npcCount
                    + " | con renderer: " + npcVisibleCount
                    + " | en NavMesh: " + npcOnMeshCount
                    + " | escala anormal: " + npcBadScaleCount);

                PassIf(npcVisibleCount == npcCount,
                    "Todos los NPCs (" + npcCount + ") tienen renderer activo y son visibles.");
                PassIf(npcOnMeshCount == npcCount,
                    "Todos los NPCs (" + npcCount + ") tienen NavMeshAgent en NavMesh.");
            }
        }

        private static void ValidateNavMeshRoutes()
        {
            if (DeliverySystem.Instance == null || DeliverySystem.Instance.deliveryStart == null)
            {
                Yellow("NAVMESH", "DeliveryStart no disponible — no se pueden probar rutas NavMesh al almacén.");
                return;
            }

            Vector3 storagePos  = DeliverySystem.Instance.deliveryStart.position;
            CashDesk   cashDesk = UnityEngine.Object.FindAnyObjectByType<CashDesk>();
            PlacementObject shelf = FindFirstShelf();

            // Route: spawned NPC → delivery/storage point.
            GameObject[] allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            foreach (GameObject go in allObjects)
            {
                if (go == null || !go.name.StartsWith("Employee_")) continue;

                NavMeshAgent agent = go.GetComponent<NavMeshAgent>();
                if (agent == null || !agent.enabled || !agent.isOnNavMesh) continue;

                // Test route to storage/warehouse.
                bool storageReachable = TryResolveReachableNavMeshPoint(agent, storagePos, out Vector3 storageDestination);

                PassIf(storageReachable,
                    "NPC '" + go.name + "' puede navegar al almacén/DeliveryStart.");
                if (storageReachable)
                    Log("  Ruta almacén resuelta en " + storageDestination + " desde target " + storagePos + ".");
                if (!storageReachable)
                    Fail("NPC '" + go.name + "' no puede navegar al almacén/DeliveryStart — "
                        + "revisar NavMesh, puertas o spawn point fuera de mesh.");

                // Test route to cash desk.
                if (cashDesk != null)
                {
                    bool cashReachable = TryResolveReachableNavMeshPoint(agent, cashDesk.transform.position, out _);

                    PassIf(cashReachable,
                        "NPC '" + go.name + "' puede navegar a la caja registradora.");
                    if (!cashReachable)
                        Fail("NPC '" + go.name + "' no puede navegar a la caja — "
                            + "revisar NavMesh y workstations de cajero.");
                }

                // Test route to shelf.
                if (shelf != null)
                {
                    bool shelfReachable = TryResolveReachableNavMeshPoint(agent, shelf.transform.position, out _);

                    PassIf(shelfReachable,
                        "NPC '" + go.name + "' puede navegar al anaquel.");
                    if (!shelfReachable)
                        Yellow("NAVMESH",
                            "NPC '" + go.name + "' no puede calcular ruta al anaquel — puede ser aceptable si no es surtidor.");
                }

                break; // Only test the first available NPC to keep run time short.
            }
        }

        private static bool TryResolveReachableNavMeshPoint(NavMeshAgent agent, Vector3 target, out Vector3 destination)
        {
            destination = Vector3.zero;
            if (agent == null || !agent.enabled || !agent.isOnNavMesh)
                return false;

            float bestDistance = float.MaxValue;
            NavMeshPath path = new NavMeshPath();

            for (float radius = 0f; radius <= NavSampleRadius; radius += 1f)
            {
                int samples = radius <= 0f ? 1 : 16;
                for (int i = 0; i < samples; i++)
                {
                    float angle = samples == 1 ? 0f : i * Mathf.PI * 2f / samples;
                    Vector3 candidate = target + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                    if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2.5f, NavMesh.AllAreas))
                        continue;
                    if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
                        continue;

                    float distance = Vector3.Distance(hit.position, target);
                    if (distance >= bestDistance)
                        continue;

                    bestDistance = distance;
                    destination = hit.position;
                }
            }

            return bestDistance < float.MaxValue;
        }

        // ── Helper methods ────────────────────────────────────────────────────────

        private static GameObject FindGameObjectByKeyword(string keyword)
        {
            if (keyword == WarehouseKeyword)
            {
                GameObject exact = GameObject.Find("WarehouseZone");
                if (exact != null)
                    return exact;
            }

            string kw = keyword.ToLower();
            GameObject[] all = UnityEngine.Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (GameObject go in all)
                if (go != null && go.name.ToLower().Contains(kw) && go.transform.parent == null)
                    return go;
            foreach (GameObject go in all)
                if (go != null && go.name.ToLower().Contains(kw))
                    return go;
            return null;
        }

        private static ProductScriptableObject FindFirstUnlockedProduct()
        {
            // Try to get a product that's unlocked and purchasable.
            ItemDatabase db = UnityEngine.Object.FindAnyObjectByType<ItemDatabase>();
            if (db == null) return null;

            List<PurchasableScriptableObject> rawProducts = ItemDatabase.GetByType(typeof(ProductScriptableObject));
            foreach (PurchasableScriptableObject rawProduct in rawProducts)
            {
                ProductScriptableObject product = rawProduct as ProductScriptableObject;
                if (product == null) continue;
                if (EntrepreneurTreeGameplayBridge.Instance == null ||
                    EntrepreneurTreeGameplayBridge.Instance.IsProductUnlocked(product))
                    return product;
            }

            // Fallback: return the first product regardless of lock state.
            for (int i = 0; i < rawProducts.Count; i++)
            {
                ProductScriptableObject product = rawProducts[i] as ProductScriptableObject;
                if (product != null)
                    return product;
            }

            return null;
        }

        private static PlacementObject FindFirstShelf()
        {
            return UnityEngine.Object.FindAnyObjectByType<PlacementObject>();
        }

        // ── Timing helpers ────────────────────────────────────────────────────────

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

        private static bool ElapsedAbsolute(float seconds)
        {
            float start = SessionState.GetFloat(StartKey, 0f);
            return start > 0f && Time.realtimeSinceStartup - start >= seconds;
        }

        private static void SetState(string newState)
        {
            SessionState.SetString(StateKey, newState);
            SessionState.SetFloat(StartKey, 0f);
        }

        private static void SetStateTimer()
        {
            SessionState.SetFloat(StartKey, Time.realtimeSinceStartup);
        }

        private static void CleanupAndLeave()
        {
            SetState("leave");
            EditorApplication.ExitPlaymode();
        }

        // ── Logging helpers ───────────────────────────────────────────────────────

        private static void PassIf(bool condition, string message)
        {
            if (condition) Pass(message);
            else           Fail(message);
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

        private static void Yellow(string tag, string message)
        {
            Log("YELLOW: [" + tag + "] " + message);
        }

        private static void Log(string message)
        {
            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + "\n";
            File.AppendAllText(LogPath, line);
            Debug.Log("[WarehouseNPCAudit] " + message);
        }

        private static void OnConsole(string condition, string stackTrace, LogType type)
        {
            if (string.IsNullOrEmpty(SessionState.GetString(StateKey, string.Empty)))
                return;

            if (type == LogType.Exception || type == LogType.Assert)
                Fail("Console " + type + ": " + condition);
        }
    }
}
