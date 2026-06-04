// ShopMasterExpansionPlayModeAuditRunner.cs
// Fase 5RQ - RQF5 Expansion desde computadora con mapa y cambio real en escena.

using System;
using System.IO;
using FLOBUK.StoreSimulator;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator.Editor
{
    [InitializeOnLoad]
    public static class ShopMasterExpansionPlayModeAuditRunner
    {
        private const string StateKey = "ShopMaster.ExpansionFase5.State";
        private const string FailureKey = "ShopMaster.ExpansionFase5.Failures";
        private const string StartKey = "ShopMaster.ExpansionFase5.Start";
        private const string LogPath = "Documentos/Unity_PlayMode_Auditoria_Expansion_Fase5RQ.log";
        private const string ScenePath = "Assets/StoreSimulator/Scenes/Game.unity";
        private const string TestZoneId = "sales_w1";

        static ShopMasterExpansionPlayModeAuditRunner()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            Application.logMessageReceived -= OnConsole;
            Application.logMessageReceived += OnConsole;
        }

        public static void Run()
        {
            File.WriteAllText(LogPath, "# ShopMaster Expansion PlayMode Audit - Fase 5RQ\n");
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

            if (state == "enter" && EditorApplication.isPlaying && Elapsed(6f))
            {
                try
                {
                    RunAudit();
                }
                catch (Exception ex)
                {
                    Fail("Unhandled expansion audit exception: " + ex);
                }

                SetState("leave");
                EditorApplication.ExitPlaymode();
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
            Log("Running RQF5 expansion audit.");

            UIShopDesktop desktop = UnityEngine.Object.FindAnyObjectByType<UIShopDesktop>();
            PassIf(desktop != null && desktop.Interact("LeftClick"),
                "Computer opens through the real laptop interaction.");

            Button expandButton = FindButtonContaining("EXPANDIR");
            PassIf(expandButton != null, "Computer has EXPANDIR tab button.");
            expandButton?.onClick.Invoke();

            Transform expansionPanel = FindContentPanel("Expandir");
            PassIf(expansionPanel != null && expansionPanel.gameObject.activeSelf,
                "EXPANDIR tab activates the real expansion panel.");

            ExpansionAppUIController app = expansionPanel != null
                ? expansionPanel.GetComponentInChildren<ExpansionAppUIController>(true)
                : null;
            PassIf(app != null, "ExpansionAppUIController is attached to expansion panel.");

            SupermarketExpansionSystem system = SupermarketExpansionSystem.Instance;
            PassIf(system != null, "SupermarketExpansionSystem exists.");
            PassIf(ExpansionRealWorldBridge.Instance != null, "ExpansionRealWorldBridge exists.");

            ExpansionZoneData zone = system != null ? system.GetZone(TestZoneId) : null;
            PassIf(zone != null, "Test zone exists: " + TestZoneId + ".");
            PassIf(zone != null && zone.state == ExpansionZoneState.Available,
                "Test zone starts Available on the map.");

            app?.SelectZone(TestZoneId);
            TMP_Text stateText = expansionPanel != null
                ? expansionPanel.Find("DetailsPanel/ZoneState")?.GetComponent<TMP_Text>()
                : null;
            PassIf(stateText != null && stateText.text.Contains("Disponible"),
                "Selecting test zone shows Available state in detail panel.");

            Button buyButton = expansionPanel != null
                ? expansionPanel.Find("DetailsPanel/BuyButton")?.GetComponent<Button>()
                : null;
            PassIf(buyButton != null && buyButton.interactable,
                "Buy button is interactable for available expansion.");

            long price = zone != null ? zone.price : 0L;
            if (StoreDatabase.Instance != null && StoreDatabase.Instance.currentMoney < price)
                StoreDatabase.AddRemoveMoney((price - StoreDatabase.Instance.currentMoney) + price);

            long moneyBefore = StoreDatabase.Instance != null ? StoreDatabase.Instance.currentMoney : 0L;
            buyButton?.onClick.Invoke();

            ExpansionZoneData purchased = system != null ? system.GetZone(TestZoneId) : null;
            PassIf(purchased != null && purchased.state == ExpansionZoneState.Purchased,
                "Buying expansion changes zone state to Purchased.");
            PassIf(StoreDatabase.Instance != null && StoreDatabase.Instance.currentMoney == moneyBefore - price,
                "Buying expansion deducts exact money from StoreDatabase.");

            GameObject realZone = GameObject.Find("ExpansionZone_REAL_" + TestZoneId)
                ?? GameObject.Find("ExpansionZone_" + TestZoneId)
                ?? GameObject.Find(TestZoneId);
            PassIf(realZone != null && realZone.activeInHierarchy,
                "Buying expansion activates or creates a real scene zone.");

            app?.SelectZone(TestZoneId);
            stateText = expansionPanel != null
                ? expansionPanel.Find("DetailsPanel/ZoneState")?.GetComponent<TMP_Text>()
                : null;
            PassIf(stateText != null && stateText.text.Contains("Comprad"),
                "Expansion detail panel refreshes to Purchased state.");

            desktop?.Exit();
        }

        private static Transform FindContentPanel(string panelName)
        {
            GameObject[] all = UnityEngine.Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].name == panelName)
                    return all[i].transform;
            }
            return null;
        }

        private static Button FindButtonContaining(string text)
        {
            Button[] buttons = UnityEngine.Object.FindObjectsByType<Button>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null)
                    continue;

                TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                if (label != null && label.text != null &&
                    label.text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                    return button;
            }
            return null;
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
            if (condition) Log("PASS: " + message);
            else Fail(message);
        }

        private static void Fail(string message)
        {
            SessionState.SetInt(FailureKey, SessionState.GetInt(FailureKey, 0) + 1);
            Log("FAIL: " + message);
        }

        private static void Log(string message)
        {
            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + "\n";
            File.AppendAllText(LogPath, line);
            Debug.Log("[ExpansionFase5Audit] " + message);
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
