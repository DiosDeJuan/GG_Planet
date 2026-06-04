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
using TMPro;
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
        private const string LogPath   = "Documentos/WarehouseSceneSetup_Report.txt";
        private const string StoreWallPrefabPath = "Assets/StoreSimulator/Prefabs/Environment/Store_Wall.prefab";
        private const string StoreWall2PrefabPath = "Assets/StoreSimulator/Prefabs/Environment/Store_Wall2.prefab";
        private const string StoreFloorPrefabPath = "Assets/StoreSimulator/Prefabs/Environment/Store_Floor5.prefab";
        private const string StoreDoorPrefabPath = "Assets/StoreSimulator/Prefabs/Environment/Store_Door.prefab";
        private const string RoofFlatPrefabPath = "Assets/StoreSimulator/Prefabs/Environment/Roof_Flat.prefab";
        private const string RoadLampPrefabPath = "Assets/StoreSimulator/Prefabs/Environment/Road_Lamp.prefab";
        private const string RoadContainerPrefabPath = "Assets/StoreSimulator/Prefabs/Environment/Road_Container.prefab";
        private const string ShelfBoxedPrefabPath = "Assets/StoreSimulator/Prefabs/Storage/Shelf_Boxed.prefab";

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
                {
                    EnsureWarehouseChildren(warehouseZone, log);
                    ReassignDeliveryStart(warehouseZone, log);
                }

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
                RebuildWarehouseChildren(existing, log);
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

            RebuildWarehouseChildren(zone, log);

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

        private static void RebuildWarehouseChildren(GameObject zone, StringBuilder log)
        {
            if (zone == null)
                return;

            for (int i = zone.transform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(zone.transform.GetChild(i).gameObject);

            CreatePrefabPart(zone, "WarehouseFloor", StoreFloorPrefabPath,
                new Vector3(0f, 0f, 0f), Quaternion.identity, new Vector3(4.2f, 1f, 3.2f), true);
            CreatePrefabPart(zone, "WarehouseWall_Back", StoreWallPrefabPath,
                new Vector3(0f, 1.5f, -HalfDepth), Quaternion.identity, new Vector3(4.4f, 1f, 1f), true);
            CreatePrefabPart(zone, "WarehouseWall_Left", StoreWall2PrefabPath,
                new Vector3(-HalfWidth, 1.5f, 0f), Quaternion.Euler(0f, 90f, 0f), new Vector3(3.2f, 1f, 1f), true);
            CreatePrefabPart(zone, "WarehouseWall_Right", StoreWall2PrefabPath,
                new Vector3(HalfWidth, 1.5f, 0f), Quaternion.Euler(0f, 90f, 0f), new Vector3(3.2f, 1f, 1f), true);
            CreatePrefabPart(zone, "WarehouseRoof", RoofFlatPrefabPath,
                new Vector3(0f, WallHeight + 0.05f, 0f), Quaternion.identity, new Vector3(4.2f, 1f, 3.2f), false);

            GameObject door = new GameObject("WarehouseWideDoor");
            door.transform.SetParent(zone.transform, false);
            door.transform.localPosition = new Vector3(0f, 0f, HalfDepth);
            CreatePrefabPart(door, "CargoDoor_Left_Open", StoreDoorPrefabPath,
                new Vector3(-HalfWidth + 0.75f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), Vector3.one, false);
            CreatePrefabPart(door, "CargoDoor_Right_Open", StoreDoorPrefabPath,
                new Vector3(HalfWidth - 0.75f, 0f, 0f), Quaternion.Euler(0f, -90f, 0f), Vector3.one, false);
            CreateBox(door, "CargoDoorFrame_Top",
                localPos: new Vector3(0f, WallHeight - 0.15f, 0f),
                size: new Vector3(HalfWidth * 2f, 0.25f, 0.25f));
            GameObject trigger = new GameObject("CargoPassageTrigger");
            trigger.transform.SetParent(door.transform, false);
            trigger.transform.localPosition = new Vector3(0f, 1.1f, 0.15f);
            BoxCollider passage = trigger.AddComponent<BoxCollider>();
            passage.isTrigger = true;
            passage.size = new Vector3(5.2f, 2.2f, 1.0f);

            CreatePrefabPart(zone, "WarehouseRack_Left", ShelfBoxedPrefabPath,
                new Vector3(-2.8f, 0f, -1.25f), Quaternion.Euler(0f, 90f, 0f), new Vector3(0.9f, 0.9f, 0.9f), true);
            CreatePrefabPart(zone, "WarehouseRack_Right", ShelfBoxedPrefabPath,
                new Vector3(2.8f, 0f, -1.25f), Quaternion.Euler(0f, -90f, 0f), new Vector3(0.9f, 0.9f, 0.9f), true);
            CreatePrefabPart(zone, "CargoContainer_Backdrop", RoadContainerPrefabPath,
                new Vector3(0f, 0f, -3.85f), Quaternion.Euler(0f, 90f, 0f), new Vector3(0.75f, 0.75f, 0.75f), true);
            CreatePrefabPart(zone, "CargoLamp_Left", RoadLampPrefabPath,
                new Vector3(-3.4f, 0f, 2.1f), Quaternion.identity, Vector3.one, false);
            CreatePrefabPart(zone, "CargoLamp_Right", RoadLampPrefabPath,
                new Vector3(3.4f, 0f, 2.1f), Quaternion.identity, Vector3.one, false);

            CreateDropMarker(zone, "PackageDropArea", new Vector3(0f, 0.04f, 1.35f), new Vector3(4.8f, 0.08f, 1.5f));
            CreateSign(zone, "Sign_Almacen", "ALMACEN", new Vector3(0f, 2.45f, 3.15f), Quaternion.Euler(0f, 180f, 0f), 1.6f);
            CreateSign(zone, "Sign_Pedidos", "PEDIDOS / CARGA", new Vector3(0f, 0.25f, 1.35f), Quaternion.Euler(90f, 0f, 0f), 1.1f);

            CreateNamedChild(zone, "EmployeeSpawnPoint",
                new Vector3(0f, 0.05f, -HalfDepth * 0.5f),
                Quaternion.Euler(0f, 180f, 0f));
            CreateNamedChild(zone, "DeliveryStartPoint", new Vector3(0f, 0.05f, 1.35f), Quaternion.identity);

            EditorUtility.SetDirty(zone);
            Log(log, "[OK] WarehouseZone reconstruida con prefabs del asset base, puerta abierta, senaletica y zona de pedidos.");
        }

        private static void ReassignDeliveryStart(GameObject warehouseZone, StringBuilder log)
        {
            DeliverySystem delivery = UnityEngine.Object.FindAnyObjectByType<DeliverySystem>();
            Transform deliveryStart = warehouseZone != null
                ? warehouseZone.transform.Find("DeliveryStartPoint")
                : null;

            if (delivery == null)
            {
                Log(log, "[WARN] DeliverySystem no encontrado; no se pudo reasignar DeliveryStartPoint.");
                return;
            }

            if (deliveryStart == null)
            {
                Log(log, "[WARN] DeliveryStartPoint no encontrado dentro de WarehouseZone.");
                return;
            }

            delivery.deliveryStart = deliveryStart;
            delivery.deliveryDirection = new Vector2(1.2f, 0f);
            delivery.totalDeliveries = Mathf.Max(delivery.totalDeliveries, 4);
            EditorUtility.SetDirty(delivery);
            Log(log, "[OK] DeliverySystem.deliveryStart reasignado a WarehouseZone/DeliveryStartPoint.");
        }

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
            if (kw == "warehouse")
            {
                GameObject exact = GameObject.Find("WarehouseZone");
                if (exact != null)
                    return exact;
            }

#if UNITY_2022_2_OR_NEWER
            GameObject[] all = UnityEngine.Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            GameObject[] all = UnityEngine.Object.FindObjectsOfType<GameObject>(true);
#endif
            foreach (GameObject go in all)
                if (go != null && go.name.ToLower().Contains(kw) && go.transform.parent == null)
                    return go;
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

        private static GameObject CreateNamedChild(GameObject parent, string childName, Vector3 localPos, Quaternion localRot)
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(parent.transform, false);
            child.transform.localPosition = localPos;
            child.transform.localRotation = localRot;
            EditorUtility.SetDirty(child);
            return child;
        }

        private static GameObject CreatePrefabPart(GameObject parent, string name, string prefabPath,
            Vector3 localPos, Quaternion localRot, Vector3 localScale, bool keepColliders)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject part = prefab != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(prefab)
                : GameObject.CreatePrimitive(PrimitiveType.Cube);

            part.name = name;
            part.transform.SetParent(parent.transform, false);
            part.transform.localPosition = localPos;
            part.transform.localRotation = localRot;
            part.transform.localScale = localScale;

            if (!keepColliders)
            {
                Collider[] colliders = part.GetComponentsInChildren<Collider>(true);
                for (int i = 0; i < colliders.Length; i++)
                    colliders[i].enabled = false;
            }

            EditorUtility.SetDirty(part);
            return part;
        }

        private static void CreateDropMarker(GameObject parent, string name, Vector3 localPos, Vector3 size)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = name;
            marker.transform.SetParent(parent.transform, false);
            marker.transform.localPosition = localPos;
            marker.transform.localScale = size;
            Collider collider = marker.GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = true;
            Renderer renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StoreSimulator/Materials/GridPlacement.mat");
            EditorUtility.SetDirty(marker);
        }

        private static void CreateSign(GameObject parent, string name, string text, Vector3 localPos, Quaternion localRot, float fontSize)
        {
            GameObject sign = new GameObject(name);
            sign.transform.SetParent(parent.transform, false);
            sign.transform.localPosition = localPos;
            sign.transform.localRotation = localRot;

            TextMeshPro tmp = sign.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;

            EditorUtility.SetDirty(sign);
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

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath) ?? ".");
                File.WriteAllText(LogPath, log.ToString());
            }
            catch (IOException ioEx)
            {
                Debug.LogWarning("[WarehouseSetup] Could not write standalone report because the log file is in use: " + ioEx.Message);
            }

            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }
    }
}
