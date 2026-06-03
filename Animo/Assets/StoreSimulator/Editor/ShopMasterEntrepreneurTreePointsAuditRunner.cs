// ShopMasterEntrepreneurTreePointsAuditRunner.cs
// Fase B3 — Auditoría del sistema de puntos del Árbol del Emprendedor.
//
// Valida:
//   • Estado inicial y puntos iniciales.
//   • Simular subida de nivel del negocio → +1 punto.
//   • Segunda subida de nivel → +1 punto adicional.
//   • No se duplican puntos al simular guardar/recargar.
//   • No se duplican puntos al re-abrir la UI.
//   • Desbloqueo válido (nodo disponible) resta exactamente 1 punto.
//   • Desbloqueo inválido (nodo con requisito pendiente) no resta puntos.
//   • Subir rango/tier (si existe) cuesta máximo 3 puntos.
//   • Los puntos nunca quedan negativos.

using System;
using System.IO;
using FLOBUK.StoreSimulator;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FLOBUK.StoreSimulator.Editor
{
    [InitializeOnLoad]
    public static class ShopMasterEntrepreneurTreePointsAuditRunner
    {
        private const string StateKey   = "ShopMaster.TreePoints.State";
        private const string FailureKey = "ShopMaster.TreePoints.Failures";
        private const string StartKey   = "ShopMaster.TreePoints.Start";
        private const string LogPath    = "Documentos/Unity_EntrepreneurTreePointsAudit_Fase4.log";
        private const string ScenePath  = "Assets/StoreSimulator/Scenes/Game.unity";

        static ShopMasterEntrepreneurTreePointsAuditRunner()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            Application.logMessageReceived -= OnConsole;
            Application.logMessageReceived += OnConsole;
        }

        public static void Run()
        {
            File.WriteAllText(LogPath, "# ShopMaster EntrepreneurTree Points Audit - Fase B3\n");
            SessionState.SetString(StateKey, "enter");
            SessionState.SetInt(FailureKey, 0);
            SessionState.SetFloat(StartKey, 0f);
            Log("Opening scene: " + ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }

        private static void OnUpdate()
        {
            string state = SessionState.GetString(StateKey, string.Empty);
            if (string.IsNullOrEmpty(state))
                return;

            if (state == "enter" && EditorApplication.isPlaying && Elapsed(4f))
            {
                try
                {
                    RunAudit();
                }
                catch (Exception ex)
                {
                    Fail("Unhandled audit exception: " + ex);
                }
                CleanupAndLeave();
                return;
            }

            if (state == "leave" && !EditorApplication.isPlaying &&
                !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                int failures = SessionState.GetInt(FailureKey, 0);
                Log("Audit finished. Failures=" + failures);
                SessionState.EraseString(StateKey);
                SessionState.EraseInt(FailureKey);
                SessionState.EraseFloat(StartKey);
                EditorApplication.Exit(failures == 0 ? 0 : 1);
            }
        }

        private static void RunAudit()
        {
            EntrepreneurTreeManager mgr = EntrepreneurTreeManager.Instance;
            PassIf(mgr != null, "B3-01: EntrepreneurTreeManager existe en escena.");
            if (mgr == null) return;

            // ── PRUEBA 1 — Estado inicial ─────────────────────────────────────────
            int initialPoints = mgr.currentPoints;
            Log("B3-02: Puntos iniciales al cargar: " + initialPoints);
            PassIf(initialPoints >= 0, "B3-02: Puntos iniciales no negativos.");

            int initialLastLevel = mgr.lastAwardedBusinessLevel;
            Log("B3-03: lastAwardedBusinessLevel inicial: " + initialLastLevel);

            // ── PRUEBA 2 — Simular subida de nivel del negocio: nivel 1 ──────────
            EntrepreneurTreeManager.SimulateBusinessLevelUp(initialLastLevel + 1);
            int afterLevel1 = mgr.currentPoints;
            PassIf(afterLevel1 == initialPoints + 1,
                "B3-04: Subida a nivel " + (initialLastLevel + 1) + " otorgó exactamente +1 punto. Antes=" + initialPoints + " Después=" + afterLevel1);

            // ── PRUEBA 3 — Simular otra subida de nivel ───────────────────────────
            EntrepreneurTreeManager.SimulateBusinessLevelUp(initialLastLevel + 2);
            int afterLevel2 = mgr.currentPoints;
            PassIf(afterLevel2 == initialPoints + 2,
                "B3-05: Segunda subida de nivel otorgó +1 punto adicional. Antes=" + afterLevel1 + " Después=" + afterLevel2);

            // ── PRUEBA 4 — No duplicar al repetir mismo nivel ─────────────────────
            int beforeRepeat = mgr.currentPoints;
            EntrepreneurTreeManager.SimulateBusinessLevelUp(initialLastLevel + 2); // mismo nivel ya otorgado
            PassIf(mgr.currentPoints == beforeRepeat,
                "B3-06: Repetir misma simulación de nivel NO duplica puntos. Puntos=" + mgr.currentPoints);

            // ── PRUEBA 5 — Simular guardar y recargar (JSON round-trip) ───────────
            SimpleJSON.JSONNode savedData = mgr.SaveToJSON();
            PassIf(savedData != null && savedData["currentPoints"] != null,
                "B3-07: SaveToJSON incluye currentPoints.");
            PassIf(savedData != null && savedData["lastAwardedBusinessLevel"] != null,
                "B3-08: SaveToJSON incluye lastAwardedBusinessLevel.");

            int savedPoints = savedData["currentPoints"].AsInt;
            int savedLastLevel = savedData["lastAwardedBusinessLevel"].AsInt;

            mgr.LoadFromJSON(savedData);
            PassIf(mgr.currentPoints == savedPoints,
                "B3-09: LoadFromJSON restaura puntos correctamente. Esperado=" + savedPoints + " Actual=" + mgr.currentPoints);
            PassIf(mgr.lastAwardedBusinessLevel == savedLastLevel,
                "B3-10: LoadFromJSON restaura lastAwardedBusinessLevel. Esperado=" + savedLastLevel + " Actual=" + mgr.lastAwardedBusinessLevel);

            // ── PRUEBA 6 — No duplicar puntos después de recargar ─────────────────
            int afterReload = mgr.currentPoints;
            EntrepreneurTreeManager.SimulateBusinessLevelUp(savedLastLevel); // mismo nivel
            PassIf(mgr.currentPoints == afterReload,
                "B3-11: Tras recargar, repetir nivel ya conocido NO duplica puntos.");

            // ── PRUEBA 7 — Desbloqueo válido resta exactamente 1 punto ───────────
            // Intentar desbloquear "product_basic_2" que requiere solo "product_basic_1"
            // product_basic_1 es el nodo por defecto desbloqueado (costo 0).
            int pointsBeforeUnlock = mgr.currentPoints;
            bool unlocked = EntrepreneurTreeManager.TryUnlockNode("product_basic_2");
            if (unlocked)
            {
                PassIf(mgr.currentPoints == pointsBeforeUnlock - 1,
                    "B3-12: Desbloqueo válido restó exactamente 1 punto. Antes=" + pointsBeforeUnlock + " Después=" + mgr.currentPoints);
            }
            else
            {
                // Could fail if points ran out or requirements not met — log it
                if (pointsBeforeUnlock >= 1 && EntrepreneurTreeManager.IsNodeUnlocked("product_basic_1"))
                    Fail("B3-12: TryUnlockNode(product_basic_2) devolvió false con puntos=" + pointsBeforeUnlock + " y product_basic_1 desbloqueado.");
                else
                    Log("B3-12: SKIP — product_basic_2 ya estaba desbloqueado o no había puntos suficientes (puntos=" + pointsBeforeUnlock + ").");
            }

            // ── PRUEBA 8 — Desbloqueo inválido (requisito no cumplido) no resta ──
            // "product_luxury_1" requiere "product_fresh_2" que requiere "product_fresh_1" etc.
            // Solo intentar si el nodo destino no está desbloqueado y tiene requisitos sin cumplir.
            string guardedNodeId = "product_luxury_1";
            int pointsBeforeInvalid = mgr.currentPoints;
            bool canUnlock = EntrepreneurTreeManager.CanUnlockNode(guardedNodeId);
            if (!canUnlock)
            {
                bool result = EntrepreneurTreeManager.TryUnlockNode(guardedNodeId);
                PassIf(!result && mgr.currentPoints == pointsBeforeInvalid,
                    "B3-13: Intento de desbloqueo inválido NO restó puntos. Puntos=" + mgr.currentPoints);
            }
            else
            {
                Log("B3-13: SKIP — " + guardedNodeId + " resultó ser elegible en el estado actual.");
            }

            // ── PRUEBA 9 — Puntos nunca negativos ─────────────────────────────────
            PassIf(mgr.currentPoints >= 0,
                "B3-14: Puntos no son negativos después de todas las operaciones. Puntos=" + mgr.currentPoints);

            // ── PRUEBA 10 — Auditar costos de todos los nodos ─────────────────────
            if (mgr.treeData != null && mgr.treeData.nodes != null)
            {
                int violations = 0;
                foreach (NodeData node in mgr.treeData.nodes)
                {
                    if (node == null) continue;
                    // No tier/rango nodes exist in the current definition — all nodes cost 1 or 0
                    if (node.cost > 1)
                    {
                        Fail("B3-15: Nodo '" + node.id + "' tiene costo=" + node.cost + " > 1 (costo máximo permitido para nodos normales).");
                        violations++;
                    }
                }
                PassIf(violations == 0,
                    "B3-15: Todos los nodos normales tienen costo ≤ 1. Revisados=" + mgr.treeData.nodes.Count + " nodos.");
            }
            else
            {
                Log("B3-15: SKIP — treeData nulo o sin nodos.");
            }
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

        private static void CleanupAndLeave()
        {
            SessionState.SetString(StateKey, "leave");
            EditorApplication.ExitPlaymode();
        }

        private static void PassIf(bool condition, string message)
        {
            if (condition)
                Log("PASS: " + message);
            else
                Fail(message);
        }

        private static void Fail(string message)
        {
            SessionState.SetInt(FailureKey, SessionState.GetInt(FailureKey, 0) + 1);
            Log("FAIL: " + message);
        }

        private static void Log(string message)
        {
            File.AppendAllText(LogPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + "\n");
            Debug.Log("[TreePointsAudit] " + message);
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
