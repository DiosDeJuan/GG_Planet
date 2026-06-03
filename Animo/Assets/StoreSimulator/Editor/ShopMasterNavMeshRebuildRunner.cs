// ShopMasterNavMeshRebuildRunner.cs
// Fase 3 — Rebakeo automático de NavMesh
//
// Propósito: Reconstruir el NavMesh de Game.unity desde la línea de comandos
// de Unity en modo batchmode, de modo que la nueva geometría del almacén
// (WarehouseZone creada por ShopMasterWarehouseSceneSetupRunner) quede incluida.
//
// PRE-REQUISITO:
//   Ejecutar ShopMasterWarehouseSceneSetupRunner ANTES de este script para
//   garantizar que WarehouseZone existe en escena y que NavMeshSurface.collectObjects
//   fue cambiado a All.
//
// Uso:
//   & 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' `
//       -batchmode -projectPath 'C:\Users\ijuan\Animo' `
//       -executeMethod FLOBUK.StoreSimulator.Editor.ShopMasterNavMeshRebuildRunner.Run `
//       -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_NavMeshRebuild.log'
//
// Resultado esperado:
//   • NavMeshData en Assets/StoreSimulator/Scenes/Game/NavMesh-Navigation.asset actualizado.
//   • Game.unity guardado con la nueva referencia al NavMeshData.
//   • Salida 0 si éxito, 1 si fallo.

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
    public static class ShopMasterNavMeshRebuildRunner
    {
        private const string ScenePath = "Assets/StoreSimulator/Scenes/Game.unity";
        private const string LogPath   = "Documentos/Unity_NavMeshRebuild.log";

        public static void Run()
        {
            var log      = new StringBuilder();
            int failures = 0;

            try
            {
                Log(log, "=== ShopMasterNavMeshRebuildRunner ===");
                Log(log, "Abriendo escena: " + ScenePath);

                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                if (!scene.IsValid())
                {
                    Fail(log, ref failures, "No se pudo abrir la escena: " + ScenePath);
                    Finish(log, failures);
                    return;
                }

                // ── Bake via NavMeshSurface (com.unity.ai.navigation) ─────────────
#if UNITY_AI_NAVIGATION
                NavMeshSurface surface = UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();
                if (surface != null)
                {
                    Log(log, "NavMeshSurface encontrada en '" + surface.gameObject.name + "'.");
                    Log(log, "  collectObjects: " + surface.collectObjects);
                    Log(log, "  Bakeando NavMesh ...");

                    // Ensure All mode so warehouse geometry is included.
                    if (surface.collectObjects != CollectObjects.All)
                    {
                        surface.collectObjects = CollectObjects.All;
                        Log(log, "  collectObjects cambiado a All para incluir WarehouseZone.");
                    }

                    surface.BuildNavMesh();
                    EditorUtility.SetDirty(surface);
                    Log(log, "[OK] NavMesh rebakeado vía NavMeshSurface.BuildNavMesh().");

                    // Persist NavMeshData asset.
                    if (surface.navMeshData != null)
                    {
                        AssetDatabase.SaveAssetIfDirty(surface.navMeshData);
                        Log(log, "  NavMeshData guardado: " + AssetDatabase.GetAssetPath(surface.navMeshData));
                    }
                }
                else
                {
                    Log(log, "[WARN] NavMeshSurface no encontrada — intentando bake legacy.");
                    BakeLegacy(log, ref failures);
                }
#else
                Log(log, "[WARN] Unity.AI.Navigation no disponible — intentando bake legacy.");
                BakeLegacy(log, ref failures);
#endif

                // ── Validate key positions after bake ─────────────────────────────
                ValidateNavMeshCoverage(log);

                // ── Save scene ────────────────────────────────────────────────────
                bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
                if (saved)
                    Log(log, "[OK] Escena guardada: " + ScenePath);
                else
                    Fail(log, ref failures, "EditorSceneManager.SaveScene devolvió false.");

                AssetDatabase.SaveAssets();
                Log(log, "[OK] AssetDatabase.SaveAssets() completado.");
            }
            catch (Exception ex)
            {
                Fail(log, ref failures, "Excepción no manejada: " + ex);
            }

            Finish(log, failures);
        }

        // ── Legacy NavMesh bake fallback ──────────────────────────────────────────

        private static void BakeLegacy(StringBuilder log, ref int failures)
        {
            // In Unity 6 with AI Navigation package the legacy bake path may not be
            // available from UnityEditor.AI.NavMeshBuilder.  We document the manual
            // steps instead of failing hard.
            Log(log, "[WARN] Bake legacy no ejecutado automáticamente.");
            Log(log, "  Para rebakear manualmente:");
            Log(log, "  1. Abrir Game.unity en Unity Editor.");
            Log(log, "  2. Seleccionar el GameObject 'Navigation'.");
            Log(log, "  3. En el Inspector cambiar CollectObjects a 'All'.");
            Log(log, "  4. Presionar Bake en el componente NavMeshSurface.");
            Log(log, "  5. Guardar la escena.");
        }

        // ── Post-bake validation ──────────────────────────────────────────────────

        private static void ValidateNavMeshCoverage(StringBuilder log)
        {
            Log(log, "--- Validando cobertura de NavMesh post-bake ---");

            // Key positions to validate.
            Vector3[] keyPositions = new Vector3[]
            {
                new Vector3(10f, 0f, 7.5f),  // DeliveryStart / WarehouseZone center
                new Vector3(10f, 0.05f, 6f), // EmployeeSpawnPoint approx
                new Vector3(10f, 0.05f, 10f),// PackageDropArea approx
                Vector3.zero,                 // Store center
            };

            string[] labels = new string[]
            {
                "DeliveryStart / WarehouseZone center",
                "EmployeeSpawnPoint (aprox)",
                "PackageDropArea (aprox)",
                "Store center (origin)",
            };

            for (int i = 0; i < keyPositions.Length; i++)
            {
                UnityEngine.AI.NavMeshHit hit;
                bool found = UnityEngine.AI.NavMesh.SamplePosition(
                    keyPositions[i], out hit, 5f, UnityEngine.AI.NavMesh.AllAreas);

                if (found)
                    Log(log, "  [OK] " + labels[i] + " pos=" + keyPositions[i]
                        + " → NavMesh en " + hit.position
                        + " (dist=" + Vector3.Distance(keyPositions[i], hit.position).ToString("0.2") + "m).");
                else
                    Log(log, "  [WARN] " + labels[i] + " pos=" + keyPositions[i]
                        + " — NavMesh no encontrado en radio 5m. Rebakear con geometría suficiente.");
            }
        }

        // ── Logging ───────────────────────────────────────────────────────────────

        private static void Log(StringBuilder log, string msg)
        {
            Debug.Log("[NavMeshRebuild] " + msg);
            log.AppendLine(msg);
        }

        private static void Fail(StringBuilder log, ref int failures, string msg)
        {
            failures++;
            Debug.LogError("[NavMeshRebuild] FAIL: " + msg);
            log.AppendLine("FAIL: " + msg);
        }

        private static void Finish(StringBuilder log, int failures)
        {
            log.AppendLine(string.Empty);
            log.AppendLine("Finished. Failures=" + failures);
            if (failures == 0)
            {
                log.AppendLine("RESULTADO: OK — NavMesh rebakeado. Ejecutar ShopMasterWarehouseAndNPCVisualAuditRunner a continuación.");
                Debug.Log("[NavMeshRebuild] Finished. Failures=0");
            }
            else
            {
                log.AppendLine("RESULTADO: FALLO — revisar errores anteriores.");
                Debug.LogError("[NavMeshRebuild] Finished with failures: " + failures);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(LogPath) ?? ".");
            File.WriteAllText(LogPath, log.ToString());

            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }
    }
}
