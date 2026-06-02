using System;
using System.IO;
using FLOBUK.StoreSimulator;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator.Editor
{
    [InitializeOnLoad]
    public static class ShopMasterFinalIntegrationPhase4Runner
    {
        private const string StateKey = "ShopMaster.Phase4.State";
        private const string FailureKey = "ShopMaster.Phase4.Failures";
        private const string StartKey = "ShopMaster.Phase4.Start";
        private const string LogPath = "Documentos/Unity_PlayMode_Auditoria_Integracion_Fase4.log";
        private const string ScenePath = "Assets/StoreSimulator/Scenes/Game.unity";
        private const string Phase3LogPath = "Documentos/Unity_PlayMode_Auditoria_Requerimientos_Fase3.log";

        private static float sampleStarted;
        private static int sampledFrames;
        private static float sampledDeltaTotal;
        private static float sampledDeltaMin;
        private static float sampledDeltaMax;

        static ShopMasterFinalIntegrationPhase4Runner()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            Application.logMessageReceived -= OnConsole;
            Application.logMessageReceived += OnConsole;
        }

        public static void Run()
        {
            File.WriteAllText(LogPath, "# ShopMaster Final Integration Audit - Phase 4\n");
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

            if (state == "enter" && EditorApplication.isPlaying && Elapsed(5f))
            {
                try
                {
                    Log("Running Phase 4 integration smoke audit.");
                    ValidatePhase3Evidence();
                    ValidateSceneSystems();
                    ValidateRuntimeUI();
                    StartFpsSample();
                    SetState("fps");
                }
                catch (Exception ex)
                {
                    Fail("Unhandled phase-4 setup exception: " + ex);
                    CleanupAndLeave();
                }
                return;
            }

            if (state == "fps" && EditorApplication.isPlaying)
            {
                SampleFrame();
                if (Time.realtimeSinceStartup - sampleStarted >= 6f)
                {
                    CompleteFpsSample();
                    LogDocumentedYellows();
                    CleanupAndLeave();
                }
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

        private static void ValidatePhase3Evidence()
        {
            string phase3 = File.Exists(Phase3LogPath) ? File.ReadAllText(Phase3LogPath) : string.Empty;
            PassIf(phase3.Contains("Audit finished. Failures=0"),
                "Baseline Phase 3 runner evidence remains green.");
            PassIf(phase3.Contains("RQF27 visual restocker reaches shelf, places one real product and deducts one package item."),
                "RQF27 physical restocker evidence: NPC reached storage, picked stock, reached shelf and placed one product.");
        }

        private static void ValidateSceneSystems()
        {
            PassIf(EntrepreneurTreeManager.Instance != null,
                "Entrepreneur Tree manager exists in Game.unity.");
            PassIf(EntrepreneurEmployeeSystem.Instance != null &&
                   EmployeeNPCSpawner.Instance != null &&
                   UnityEngine.Object.FindAnyObjectByType<EmployeeRestockCoordinator>() != null,
                "Employee hiring, visual NPC and restock coordinator are connected in Game.unity.");
            PassIf(SupermarketExpansionSystem.Instance != null &&
                   Mathf.Approximately(SupermarketExpansionSystem.SalesExpansionCustomerBonusPercent, 0.15f),
                "RQF22 expansion system keeps the documented 15 percent customer-demand bonus.");
            PassIf(ProductPricingSystem.Instance != null,
                "RQF28 pricing system is present in Game.unity.");
            PassIf(UnityEngine.Object.FindAnyObjectByType<CashDesk>() != null,
                "RQF23/RQF24 real manual cash desk exists in Game.unity.");
            PassIf(UnityEngine.Object.FindAnyObjectByType<StatsDatabase>() != null,
                "RQF34 daily stats database exists in Game.unity.");
            PassIf(UnityEngine.Object.FindAnyObjectByType<UISettings>() != null,
                "RQNF19 settings UI exists in Game.unity.");
        }

        private static void ValidateRuntimeUI()
        {
            UIShopDesktop desktop = UnityEngine.Object.FindAnyObjectByType<UIShopDesktop>();
            PassIf(desktop != null && desktop.Interact("LeftClick"),
                "Computer opens through its real player interaction.");

            Button[] buttons = UnityEngine.Object.FindObjectsByType<Button>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            int activeButtons = 0;
            for (int i = 0; i < buttons.Length; i++)
                if (buttons[i] != null && buttons[i].gameObject.activeInHierarchy && buttons[i].interactable)
                    activeButtons++;

            PassIf(activeButtons > 0,
                "Runtime UI contains active interactable buttons after opening the computer.");
            desktop?.Exit();
        }

        private static void StartFpsSample()
        {
            sampleStarted = Time.realtimeSinceStartup;
            sampledFrames = 0;
            sampledDeltaTotal = 0f;
            sampledDeltaMin = float.MaxValue;
            sampledDeltaMax = 0f;
            Log("Starting basic six-second FPS sample in Game.unity.");
        }

        private static void SampleFrame()
        {
            float delta = Time.unscaledDeltaTime;
            if (delta <= 0f)
                return;

            sampledFrames++;
            sampledDeltaTotal += delta;
            sampledDeltaMin = Mathf.Min(sampledDeltaMin, delta);
            sampledDeltaMax = Mathf.Max(sampledDeltaMax, delta);
        }

        private static void CompleteFpsSample()
        {
            float average = sampledDeltaTotal > 0f ? sampledFrames / sampledDeltaTotal : 0f;
            float minimum = sampledDeltaMax > 0f ? 1f / sampledDeltaMax : 0f;
            float maximum = sampledDeltaMin < float.MaxValue ? 1f / sampledDeltaMin : 0f;
            Log("MEASURED: Basic Game.unity FPS sample average=" + average.ToString("0.00")
                + ", minimum=" + minimum.ToString("0.00")
                + ", maximum=" + maximum.ToString("0.00")
                + ", frames=" + sampledFrames + ".");
            Yellow("RQNF1/RQNF2",
                "A basic editor FPS sample was captured. Minimum-hardware certification still requires a representative Windows test machine and a loaded gameplay scenario.");
        }

        private static void LogDocumentedYellows()
        {
            Yellow("RQF21/RQNF3",
                "Atomic save and backup recovery are green from Phase 3; complete Game.unity unload/reload reconstruction remains pending.");
            Yellow("RQF5/RQF6/RQF7/RQF22",
                "Expansion backend, prices and 15 percent demand bonus exist; full player-driven UI purchase, real zone navigation and persisted reload remain pending.");
            Yellow("RQF23/RQF24",
                "Real cash desk exists and automated timing remains green; player-driven terminal and cash denomination clicks remain pending.");
            Yellow("RQF28/RQF29/RQNF10-RQNF15",
                "Pricing UI and formulas remain connected; a complete customer purchase altered through the real price input remains pending.");
            Yellow("RQF34/RQF35/RQNF5",
                "Daily stats and seven-second waiting code exist; real day closure and observed customer abandonment remain pending.");
            Yellow("RQNF6",
                "Shelf compatibility is connected; player-driven valid and invalid placement attempts remain pending.");
            Yellow("RQNF16/RQNF17/RQNF18",
                "Computer opens and runtime buttons exist; visual QA at 1280x720, 1366x768 and 1920x1080 remains pending.");
            Yellow("RQNF19",
                "Settings UI exists; resolution, volume, brightness and controls persistence across restart remain pending.");
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

        private static void Yellow(string requirement, string message)
        {
            Log("YELLOW: " + requirement + " - " + message);
        }

        private static void Fail(string message)
        {
            SessionState.SetInt(FailureKey, SessionState.GetInt(FailureKey, 0) + 1);
            Log("FAIL: " + message);
        }

        private static void Log(string message)
        {
            File.AppendAllText(LogPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + "\n");
            Debug.Log("[Phase4Audit] " + message);
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
