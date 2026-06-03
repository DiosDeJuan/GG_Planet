// ShopMasterWarehouseSceneSetupRunner.cs
// Fase 3 — Persistir WarehouseZone en Game.unity
//
// Propósito: Agregar WarehouseZone como objeto persistente en la escena,
// de modo que quede disponible para rebakeo de NavMesh sin depender del
// componente WarehouseZoneBootstrap en runtime.
//
// Uso:
//   & 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' `
//       -batchmode -projectPath 'C:\Users\ijuan\Animo' `
//       -executeMethod FLOBUK.StoreSimulator.Editor.ShopMasterWarehouseSceneSetupRunner.Run `
//       -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_WarehouseSceneSetup.log'
//
// Resultado esperado:
//   • Game.unity contiene WarehouseZone con todos los hijos requeridos.
//   • NavMeshSurface cambia a CollectObjects.All para que el próximo rebake
//     incluya la geometría del almacén.
//   • La escena se guarda automáticamente.
//
// SIGUIENTE PASO después de ejecutar este script:
//   Ejecutar ShopMasterNavMeshRebuildRunner para rebakear el NavMesh.

using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_AI_NAVIGATION
using Unity.AI.Navigation;
#endif

namespace FLOBUK.StoreSimulator.Editor
{
    public static class ShopMasterWarehouseSceneSetupRunner
    {
        // ── Paths ─────────────────────────────────────────────────────────────────
        private const string ScenePath = "Assets/StoreSimulator/Scenes/Game.unity";
        private const string LogPath   = "Documentos/Unity_WarehouseSceneSetup.log";

        // ── Geometry constants — must match WarehouseZoneBootstrap values ─────────
        private const float HalfWidth  = 4f;
        private const float HalfDepth  = 3f;
        private const float WallHeight = 3f;
        private const float WallThick  = 0.25f;

        // ── Warehouse world position ───────────────────────────────────────────────
        // DeliverySystem.deliveryStart is typically at world position (10, 0, 7.5)
        // when GameSystems is at origin.  Place the warehouse centered there.
        private static readonly Vector3 WarehouseOrigin = new Vector3(10f, 0f, 7.5f);

        // ── Entry point ───────────────────────────────────────────────────────────

        public static void Run()
        {
            var log      = new StringBuilder();
            int failures = 0;

            try
            {
                Log(log, "=== ShopMasterWarehouseSceneSetupRunner ===");
                Log(log, "Opening scene: " + ScenePath);

                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                if (!scene.IsValid())
                {
                    Fail(log, ref failures, "No se pudo abrir la escena: " + ScenePath);
                    Finish(log, failures);
                    return;
                }

                Log(log, "Escena abierta correctamente.");

                // ── 1. Warehouse zone ─────────────────────────────────────────────
                GameObject warehouseZone = FindOrCreateWarehouseZone(log, ref failures);

                // ── 2. Ensure all required children ───────────────────────────────
                if (warehouseZone != null)
                    EnsureWarehouseChildren(warehouseZone, log);

                // ── 3. Expand NavMeshSurface to cover the whole store ─────────────
                ExpandNavMeshSurface(log, ref failures);

                // ── 4. Save ───────────────────────────────────────────────────────
                bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
                if (saved)
                    Log(log, "Escena guardada correctamente: " + ScenePath);
                else
                    Fail(log, ref failures, "EditorSceneManager.SaveScene devolvió false.");
            }
            catch (Exception ex)
            {
                Fail(log, ref failures, "Excepción no manejada: " + ex);
            }

            Finish(log, failures);
        }

        // ── Warehouse zone ────────────────────────────────────────────────────────

        private static GameObject FindOrCreateWarehouseZone(StringBuilder log, ref int failures)
        {
            // Search by keyword.
            GameObject existing = FindByKeyword("warehouse");
            if (existing != null)
            {
                Log(log, "[OK] WarehouseZone ya existe en escena: '" + existing.name
                    + "' pos=" + existing.transform.position);
                return existing;
            }

            Log(log, "WarehouseZone no encontrada — creando en " + WarehouseOrigin + " ...");

            GameObject zone = new GameObject("WarehouseZone");
            zone.transform.position = WarehouseOrigin;

            // Walls
            CreateBox(zone, "WarehouseWall_Back",
                localPos: new Vector3(0f, WallHeight * 0.5f, -HalfDepth),
                size:      new Vector3(HalfWidth * 2f + WallThick * 2f, WallHeight, WallThick));

            CreateBox(zone, "WarehouseWall_Left",
                localPos: new Vector3(-HalfWidth, WallHeight * 0.5f, 0f),
                size:      new Vector3(WallThick, WallHeight, HalfDepth * 2f));

            CreateBox(zone, "WarehouseWall_Right",
                localPos: new Vector3(HalfWidth, WallHeight * 0.5f, 0f),
                size:      new Vector3(WallThick, WallHeight, HalfDepth * 2f));

            // Floor — walkable; NavMesh will be baked on top of it.
            CreateBox(zone, "WarehouseFloor",
                localPos: new Vector3(0f, -0.05f, 0f),
                size:      new Vector3(HalfWidth * 2f, 0.1f, HalfDepth * 2f));

            // Wide door (open frame — two side pillars, no blocking geometry).
            GameObject door = new GameObject("WarehouseWideDoor");
            door.transform.SetParent(zone.transform, false);
            door.transform.localPosition = new Vector3(0f, 0f, HalfDepth);

            CreateBox(door, "DoorPillar_Left",
                localPos: new Vector3(-HalfWidth + 0.6f, WallHeight * 0.5f, 0f),
                size:      new Vector3(WallThick, WallHeight, WallThick));

            CreateBox(door, "DoorPillar_Right",
                localPos: new Vector3(HalfWidth - 0.6f, WallHeight * 0.5f, 0f),
                size:      new Vector3(WallThick, WallHeight, WallThick));

            // EmployeeSpawnPoint — inside the warehouse, slightly behind centre.
            GameObject spawnPt = new GameObject("EmployeeSpawnPoint");
            spawnPt.transform.SetParent(zone.transform, false);
            spawnPt.transform.localPosition = new Vector3(0f, 0.05f, -HalfDepth * 0.5f);
            spawnPt.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            // PackageDropArea — where packages from DeliverySystem land.
            GameObject dropArea = new GameObject("PackageDropArea");
            dropArea.transform.SetParent(zone.transform, false);
            dropArea.transform.localPosition = new Vector3(0f, 0.05f, HalfDepth * 0.4f);

            // DeliveryStartPoint — matches DeliverySystem.deliveryStart position.
            GameObject deliveryPt = new GameObject("DeliveryStartPoint");
            deliveryPt.transform.SetParent(zone.transform, false);
            deliveryPt.transform.localPosition = Vector3.zero; // zone is already at WarehouseOrigin

            Log(log, "[CREATED] WarehouseZone en " + WarehouseOrigin + " con "
                + zone.transform.childCount + " hijos.");

            // Mark scene dirty so SaveScene works.
            EditorUtility.SetDirty(zone);

            return zone;
        }

        private static void EnsureWarehouseChildren(GameObject zone, StringBuilder log)
        {
            EnsureNamedChild(zone, "EmployeeSpawnPoint",
                new Vector3(0f, 0.05f, -HalfDepth * 0.5f),
                Quaternion.Euler(0f, 180f, 0f), log);

            EnsureNamedChild(zone, "PackageDropArea",
                new Vector3(0f, 0.05f, HalfDepth * 0.4f),
                Quaternion.identity, log);

            EnsureNamedChild(zone, "DeliveryStartPoint",
                Vector3.zero,
                Quaternion.identity, log);
        }

        // ── NavMesh surface expansion ─────────────────────────────────────────────

        private static void ExpandNavMeshSurface(StringBuilder log, ref int failures)
        {
#if UNITY_AI_NAVIGATION
            NavMeshSurface surface = UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();
            if (surface == null)
            {
                Log(log, "[WARN] NavMeshSurface no encontrada en escena. "
                    + "El rebakeo de NavMesh requerirá intervención manual.");
                return;
            }

            Log(log, "NavMeshSurface encontrada en '" + surface.gameObject.name + "'.");
            Log(log, "  collectObjects antes: " + surface.collectObjects);

            if (surface.collectObjects != CollectObjects.All)
            {
                surface.collectObjects = CollectObjects.All;
                EditorUtility.SetDirty(surface);
                Log(log, "  collectObjects cambiado a: All (incluye toda la geometría de la escena).");
            }
            else
            {
                Log(log, "  collectObjects ya era All — sin cambios.");
            }
#else
            Log(log, "[WARN] Unity.AI.Navigation no disponible en este contexto — "
                + "NavMeshSurface.collectObjects no se pudo cambiar a All. "
                + "Actualizar manualmente en Inspector: Navigation → CollectObjects → All.");
#endif
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static GameObject FindByKeyword(string keyword)
        {
            string kw = keyword.ToLower();
#if UNITY_2022_2_OR_NEWER
            GameObject[] all = UnityEngine.Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            GameObject[] all = UnityEngine.Object.FindObjectsOfType<GameObject>(true);
#endif
            foreach (GameObject go in all)
                if (go != null && go.name.ToLower().Contains(kw))
                    return go;
            return null;
        }

        private static void EnsureNamedChild(GameObject parent, string childName,
            Vector3 localPos, Quaternion localRot, StringBuilder log)
        {
            Transform existing = parent.transform.Find(childName);
            if (existing != null)
            {
                Log(log, "  [OK] '" + childName + "' ya existe.");
                return;
            }

            GameObject child = new GameObject(childName);
            child.transform.SetParent(parent.transform, false);
            child.transform.localPosition = localPos;
            child.transform.localRotation = localRot;
            EditorUtility.SetDirty(child);
            Log(log, "  [CREATED] '" + childName + "' creado en local " + localPos);
        }

        private static void CreateBox(GameObject parent, string name, Vector3 localPos, Vector3 size)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent.transform, false);
            box.transform.localPosition = localPos;
            box.transform.localScale    = size;
            EditorUtility.SetDirty(box);
        }

        // ── Logging ───────────────────────────────────────────────────────────────

        private static void Log(StringBuilder log, string msg)
        {
            Debug.Log("[WarehouseSetup] " + msg);
            log.AppendLine(msg);
        }

        private static void Fail(StringBuilder log, ref int failures, string msg)
        {
            failures++;
            Debug.LogError("[WarehouseSetup] FAIL: " + msg);
            log.AppendLine("FAIL: " + msg);
        }

        private static void Finish(StringBuilder log, int failures)
        {
            log.AppendLine(string.Empty);
            log.AppendLine("Finished. Failures=" + failures);
            if (failures == 0)
            {
                log.AppendLine("RESULTADO: OK — escena actualizada. Ejecutar ShopMasterNavMeshRebuildRunner a continuación.");
                Debug.Log("[WarehouseSetup] Finished. Failures=0");
            }
            else
            {
                log.AppendLine("RESULTADO: FALLO — revisar errores anteriores.");
                Debug.LogError("[WarehouseSetup] Finished with failures: " + failures);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(LogPath) ?? ".");
            File.WriteAllText(LogPath, log.ToString());

            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }
    }
}
