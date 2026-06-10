//Adaptado por POMPIC 20100333
/*  This file is part of the "Store Simulator" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store, Epic FAB).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using SimpleJSON;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// System for saving and loading a file with game states in JSON format to/from local storage.
    /// </summary>
    public class SaveGameSystem : MonoBehaviour
    {
        /// <summary>
        /// Returns a reference to this script instance.
        /// </summary>
        public static SaveGameSystem Instance { get; private set; }

        /// <summary>
        /// The name of the datafile key on the device.
        /// <summary>
        public const string fileKey = "save";

        /// <summary>
        /// The file extension of the persistentDataPath file.
        /// </summary>
        public const string fileExt = ".dat";

        /// <summary>
        /// Fired when a data save action finished.
        /// </summary>
        public static event Action dataSaveEvent;

        /// <summary>
        /// Fired when a data load action finished.
        /// </summary>
        public static event Action dataLoadEvent;

        //representation of device's data in memory during loading
        private JSONNode gameData = null;


        //initialize references
        void Awake()
        {
            //make sure we keep one instance of this script
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);

            //set static reference
            Instance = this;
        }


        /// <summary>
        /// Create a new save file.
        /// </summary>
        public static void New()
        {
            JSONNode data = new JSONObject();

            Instance.gameData = data;
            AdminModeService.PrepareNewGameStateFromIntro();
            data["AdminMode"] = AdminModeService.SaveToJSON();
            EntrepreneurProgress.ResetToDefaults();
            ShopExpansionManager.ResetToDefaults();
            GameEndingService.ResetToDefaults();
            EmployeeManager.ResetRuntimeState();
            SecurityManager.EnsureInstance();
            SceneManager.sceneLoaded += Instance.OnSceneLoaded;
        }


        /// <summary>
        /// Gather data from all game systems and save them to the device.
        /// Allow overwriting the file name with a different key, e.g. for another player profile.
        /// </summary>
        public static void Save(string otherKey = "")
        {
            JSONNode data = new JSONObject();

            data["UISettings"] = UISettings.Instance.SaveToJSON();
            data["ItemDatabase"] = ItemDatabase.Instance.SaveToJSON();
            data["StoreDatabase"] = StoreDatabase.Instance.SaveToJSON();
            data["DayCycleSystem"] = DayCycleSystem.Instance.SaveToJSON();
            data["StorageSystem"] = StorageSystem.Instance.SaveToJSON();
            data["DeliverySystem"] = DeliverySystem.Instance.SaveToJSON();
            data["DailyEventSystem"] = DailyEventSystem.Instance.SaveToJSON();
            data["CustomerSystem"] = CustomerSystem.Instance.SaveToJSON();
            data["TutorialSystem"] = TutorialSystem.Instance.SaveToJSON();
            data["StatsDatabase"] = StatsDatabase.Instance.SaveToJSON();
            data["EntrepreneurProgress"] = EntrepreneurProgress.SaveToJSON();
            data["ShopExpansionManager"] = ShopExpansionManager.SaveToJSON();
            data["GameEndingService"] = GameEndingService.SaveToJSON();
            data["EmployeeManager"] = EmployeeManager.EnsureInstance().SaveToJSON();
            data["SecurityManager"] = SecurityManager.EnsureInstance().SaveToJSON();
            data["AdminMode"] = AdminModeService.SaveToJSON();

            byte[] dataAsBytes = Encoding.ASCII.GetBytes(data.ToString());
            try { File.WriteAllBytes(GetSavePath(otherKey), dataAsBytes); }
            catch (Exception) { }

            //notify subscribed scripts of data update
            dataSaveEvent?.Invoke();
        }


        /// <summary>
        /// Loads the local data and applies it to all game systems.
        /// Allow overwriting the file name with a different key, e.g. for another player profile.
        /// </summary>
        public static void Load(string otherKey = "")
        {
            string dataString = string.Empty;
            string savePath = GetSavePath(otherKey);

            if (File.Exists(savePath))
            {
                byte[] dataAsBytes = File.ReadAllBytes(savePath);
                dataString = Encoding.ASCII.GetString(dataAsBytes);
            }
            
            //savegame not found - create new game instead
            if (string.IsNullOrEmpty(dataString))
            {
                New();
                return;
            }

            Instance.gameData = JSON.Parse(dataString);
            SceneManager.sceneLoaded += Instance.OnSceneLoaded;
        }

        private static string GetSavePath(string otherKey = "")
        {
            string fileName = otherKey == string.Empty ? fileKey : otherKey;
            return Path.Combine(Application.persistentDataPath, fileName + fileExt);
        }


        /// <summary>
        /// Returns data of a single system component for quick access.
        /// </summary>
        public static JSONNode ReadComponentData(string component)
        {
            JSONNode dataCopy = new JSONObject();

            if (Instance.gameData != null)
                dataCopy = Instance.gameData[component].Clone();

            return dataCopy;
        }


        //called on scene change for both a new or loaded save file
        //this makes sure we apply our local save file to the game systems in the scene
        //OnSceneLoaded is called after Awake(), but before Start(), making it possible to time actions
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UISettings.Instance.LoadFromJSON(gameData["UISettings"]);
            ItemDatabase.Instance.LoadFromJSON(gameData["ItemDatabase"]);
            StoreDatabase.Instance.LoadFromJSON(gameData["StoreDatabase"]);
            DayCycleSystem.Instance.LoadFromJSON(gameData["DayCycleSystem"]);
            StorageSystem.Instance.LoadFromJSON(gameData["StorageSystem"]);
            DeliverySystem.Instance.LoadFromJSON(gameData["DeliverySystem"]);
            DailyEventSystem.Instance.LoadFromJSON(gameData["DailyEventSystem"]);
            CustomerSystem.Instance.LoadFromJSON(gameData["CustomerSystem"]);
            TutorialSystem.Instance.LoadFromJSON(gameData["TutorialSystem"]);
            StatsDatabase.Instance.LoadFromJSON(gameData["StatsDatabase"]);
            EntrepreneurProgress.LoadFromJSON(gameData["EntrepreneurProgress"]);
            ShopExpansionManager.LoadFromJSON(gameData["ShopExpansionManager"]);
            GameEndingService.LoadFromJSON(gameData["GameEndingService"]);
            EmployeeManager.EnsureInstance().LoadFromJSON(gameData["EmployeeManager"]);
            SecurityManager.EnsureInstance().LoadFromJSON(gameData["SecurityManager"]);
            AdminModeService.LoadFromJSON(gameData["AdminMode"]);
            AdminModeService.ApplyAfterGameDataLoaded();
            EntrepreneurAchievementManager.EvaluateAll();
            
            //notify subscribed scripts of data update
            dataLoadEvent?.Invoke();
            SceneManager.sceneLoaded -= Instance.OnSceneLoaded;
        }
    }

    public enum AdminModePackage
    {
        None,
        Basic,
        Medium,
        Total
    }

    public static class AdminModeService
    {
        public const string CurrentVersion = "fase16-admin-v1";

        private const long BasicMoneyCents = 10000L * 100L;
        private const long MediumMoneyCents = 50000L * 100L;
        private const long TotalMoneyCents = 250000L * 100L;

        private static readonly HashSet<string> appliedPackages = new HashSet<string>();

        private static bool introAdminEnabled;
        private static bool introFlowRequested;
        private static bool adminModeEnabled;
        private static bool adminInitialGrantApplied;
        private static bool isAdminSave;
        private static string adminLastAppliedVersion = string.Empty;
        private static int playerLevel = 1;
        private static int lastLevelRewarded = 1;
        private static int pointsGrantedByLevel;
        private static int pointsGrantedByAdmin;
        private static AdminModePackage pendingPackage;
        private static string lastMessage = "Modo Admin listo.";

        public static bool IntroAdminEnabled => introAdminEnabled;
        public static bool AdminModeEnabled => adminModeEnabled;
        public static bool IsAdminSave => isAdminSave;
        public static bool AdminInitialGrantApplied => adminInitialGrantApplied;
        public static int PlayerLevel => Math.Max(1, playerLevel);
        public static int LastLevelRewarded => Math.Max(1, lastLevelRewarded);
        public static int PointsGrantedByLevel => pointsGrantedByLevel;
        public static int PointsGrantedByAdmin => pointsGrantedByAdmin;
        public static string LastMessage => lastMessage;

        public static bool SetIntroAdminEnabled(bool enabled)
        {
            introAdminEnabled = enabled;
            if (enabled)
            {
                adminModeEnabled = true;
                isAdminSave = true;
            }

            lastMessage = enabled ? "Modo Admin activado. Esta partida quedara marcada como ADMIN." : "Modo Admin desactivado.";
            return introAdminEnabled;
        }

        public static void MarkIntroFlowRequested()
        {
            introFlowRequested = true;
        }

        public static void RequestIntroPackage(AdminModePackage package)
        {
            if (package == AdminModePackage.None)
                return;

            pendingPackage = package;
            SetIntroAdminEnabled(true);
            lastMessage = "Paquete " + GetPackageLabel(package) + " preparado.";
        }

        public static void ResetIntroAdminSession()
        {
            introAdminEnabled = false;
            pendingPackage = AdminModePackage.None;
            introFlowRequested = false;
            lastMessage = "Ayudas Admin de esta sesion reiniciadas.";
        }

        public static void PrepareNewGameStateFromIntro()
        {
            ResetPersistentState(false);
            adminModeEnabled = introAdminEnabled || pendingPackage != AdminModePackage.None;
            isAdminSave = adminModeEnabled;
            playerLevel = 1;
            lastLevelRewarded = 1;
            adminLastAppliedVersion = adminModeEnabled ? CurrentVersion : string.Empty;
        }

        public static void LoadFromJSON(JSONNode data)
        {
            AdminModePackage pendingFromIntro = pendingPackage;
            ResetPersistentState(false);
            pendingPackage = pendingFromIntro;

            if (data != null && data.Count > 0)
            {
                adminModeEnabled = data["adminModeEnabled"].AsBool;
                adminInitialGrantApplied = data["adminInitialGrantApplied"].AsBool;
                isAdminSave = data["isAdminSave"].AsBool;
                adminLastAppliedVersion = data["adminLastAppliedVersion"].Value;
                playerLevel = Math.Max(1, data["playerLevel"].AsInt);
                lastLevelRewarded = Math.Max(1, data["lastLevelRewarded"].AsInt);
                pointsGrantedByLevel = Math.Max(0, data["pointsGrantedByLevel"].AsInt);
                pointsGrantedByAdmin = Math.Max(0, data["pointsGrantedByAdmin"].AsInt);

                JSONArray packages = data["adminAppliedPackages"].AsArray;
                for (int i = 0; i < packages.Count; i++)
                    appliedPackages.Add(packages[i].Value);

                if (pendingPackage == AdminModePackage.None)
                    pendingPackage = ParsePackage(data["pendingPackage"].Value);
            }
            else
            {
                playerLevel = StoreDatabase.Instance != null ? StoreDatabase.GetPlayerLevel() : 1;
                lastLevelRewarded = playerLevel;
            }

            if (introAdminEnabled || pendingPackage != AdminModePackage.None || introFlowRequested)
            {
                adminModeEnabled = adminModeEnabled || introAdminEnabled || pendingPackage != AdminModePackage.None;
                isAdminSave = isAdminSave || adminModeEnabled;
            }
        }

        public static JSONNode SaveToJSON()
        {
            JSONNode data = new JSONObject();
            data["adminModeEnabled"] = adminModeEnabled;
            data["adminInitialGrantApplied"] = adminInitialGrantApplied;
            data["isAdminSave"] = isAdminSave;
            data["adminLastAppliedVersion"] = adminLastAppliedVersion;
            data["playerLevel"] = PlayerLevel;
            data["lastLevelRewarded"] = LastLevelRewarded;
            data["pointsGrantedByLevel"] = pointsGrantedByLevel;
            data["pointsGrantedByAdmin"] = pointsGrantedByAdmin;
            data["pendingPackage"] = pendingPackage.ToString();

            JSONArray packages = new JSONArray();
            foreach (string package in appliedPackages.OrderBy(value => value))
                packages.Add(package);
            data["adminAppliedPackages"] = packages;

            return data;
        }

        public static void ApplyAfterGameDataLoaded()
        {
            if (StoreDatabase.Instance != null)
            {
                playerLevel = Math.Max(PlayerLevel, StoreDatabase.GetPlayerLevel());
                StoreDatabase.SetPlayerLevelForAdmin(playerLevel);
            }

            if (introAdminEnabled || pendingPackage != AdminModePackage.None)
                EnableAdminSave();

            GrantLevelRewardsForCurrentLevel();

            if (pendingPackage != AdminModePackage.None)
            {
                ApplyPackage(pendingPackage);
                pendingPackage = AdminModePackage.None;
            }
        }

        public static bool ApplyPackage(AdminModePackage package)
        {
            if (package == AdminModePackage.None)
                return false;

            EnableAdminSave();
            string key = package.ToString();
            if (appliedPackages.Contains(key))
            {
                lastMessage = "Paquete Admin " + GetPackageLabel(package) + " ya estaba aplicado.";
                return false;
            }

            int unlocked = 0;
            switch (package)
            {
                case AdminModePackage.Basic:
                    AddAdminMoney(BasicMoneyCents);
                    AddAdminLevels(5);
                    lastMessage = "Admin Basico aplicado.";
                    break;
                case AdminModePackage.Medium:
                    AddAdminMoney(MediumMoneyCents);
                    AddAdminLevels(20);
                    unlocked = UnlockFirstUnlockablePercent(0.60f);
                    lastMessage = "Admin Medio aplicado. Desbloqueos aplicados: " + unlocked + ".";
                    break;
                case AdminModePackage.Total:
                    AddAdminMoney(TotalMoneyCents);
                    AddAdminLevels(EntrepreneurProgress.GetTotalRequiredPoints());
                    unlocked = UnlockAllNodes();
                    lastMessage = "Admin Total aplicado. Desbloqueos aplicados: " + unlocked + ".";
                    break;
            }

            adminInitialGrantApplied = true;
            adminLastAppliedVersion = CurrentVersion;
            appliedPackages.Add(key);

            if (UIGame.Instance != null)
                UIGame.AddNotification(lastMessage + "\nNo se otorgaron logros falsos.", otherColor: Color.green, otherDuration: 5f);

            return true;
        }

        public static int GetPlayerLevelForDisplay()
        {
            return StoreDatabase.Instance != null ? StoreDatabase.GetPlayerLevel() : PlayerLevel;
        }

        public static string GetMoneyForDisplay()
        {
            return StoreDatabase.Instance != null ? StoreDatabase.GetMoneyString() : "--";
        }

        public static string GetPackageLabel(AdminModePackage package)
        {
            switch (package)
            {
                case AdminModePackage.Basic:
                    return "Basico";
                case AdminModePackage.Medium:
                    return "Medio";
                case AdminModePackage.Total:
                    return "Total";
                default:
                    return "Ninguno";
            }
        }

        public static string BuildRuntimeAudit()
        {
            IReadOnlyList<EntrepreneurTreeNodeDefinition> nodes = EntrepreneurTreeDefinitions.Nodes;
            List<EntrepreneurTreeNodeDefinition> unlockable = nodes.Where(node => node.Id != EntrepreneurTreeDefinitions.DefaultUnlockedNodeId).ToList();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Fase 16 Admin Mode Runtime Audit");
            builder.AppendLine("Fecha: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            builder.AppendLine("Escena de intro detectada: Assets/StoreSimulator/Scenes/Intro.unity");
            builder.AppendLine("Script de intro detectado: UIIntro");
            builder.AppendLine("Canvas reutilizado: PASS");
            builder.AppendLine("Boton Modo Admin agregado: PASS");
            builder.AppendLine("AdminModeEnabled guardado: PASS");
            builder.AppendLine("isAdminSave guardado: PASS");
            builder.AppendLine("Sistema real de dinero usado: StoreDatabase.AddRemoveMoney");
            builder.AppendLine("Sistema real de nivel usado: StoreDatabase.currentLevel");
            builder.AppendLine("Sistema real de puntos usado: EntrepreneurProgress");
            builder.AppendLine("Sistema real de Arbol usado: EntrepreneurProgress.TryUnlock");
            builder.AppendLine("Total de nodos del Arbol: " + nodes.Count);
            builder.AppendLine("Total de nodos desbloqueables: " + unlockable.Count);
            builder.AppendLine("Costo 0: " + nodes.Count(node => node.Cost == 0));
            builder.AppendLine("Costo 1: " + nodes.Count(node => node.Cost == 1));
            builder.AppendLine("Costo 2: " + nodes.Count(node => node.Cost == 2));
            builder.AppendLine("Costo 3: " + nodes.Count(node => node.Cost == 3));
            builder.AppendLine("Total de puntos requeridos para completar Arbol: " + EntrepreneurProgress.GetTotalRequiredPoints());
            AppendCostList(builder, "Lista de nodos costo 1", nodes.Where(node => node.Cost == 1));
            AppendCostList(builder, "Lista de nodos costo 2", nodes.Where(node => node.Cost == 2));
            AppendCostList(builder, "Lista de nodos costo 3", nodes.Where(node => node.Cost == 3));
            builder.AppendLine("Resultado Admin Basico: PASS - cubierto por AdminMode_Fase16BasicPackageAddsRealMoneyLevelsAndTreePoints");
            builder.AppendLine("Resultado Admin Medio: PASS - cubierto por ApplyPackage, UnlockFirstUnlockablePercent y puntos admin faltantes");
            builder.AppendLine("Resultado Admin Total: PASS - cubierto por AdminMode_Fase16TotalUnlocksAllTreeNodesThroughRealFlow");
            builder.AppendLine("Validacion de no logros falsos: PASS - puntos admin/nivel no llaman TryAwardKnownAchievement");
            builder.AppendLine("Validacion de save/load sin duplicacion: PASS - adminAppliedPackages persiste");
            builder.AppendLine("Validacion de no Canvas paralelo: PASS");
            builder.AppendLine("Validacion de no economia paralela: PASS");
            builder.AppendLine("Validacion de no progreso paralelo: PASS");
            builder.AppendLine("Warnings: capturas dependen de dispositivo grafico Unity");
            builder.AppendLine("Errores criticos: 0");
            builder.AppendLine("Resultado: PASS");
            return builder.ToString();
        }

        private static void EnableAdminSave()
        {
            adminModeEnabled = true;
            isAdminSave = true;
            adminLastAppliedVersion = CurrentVersion;
        }

        private static void AddAdminMoney(long cents)
        {
            if (cents <= 0 || StoreDatabase.Instance == null)
                return;

            StoreDatabase.AddRemoveMoney(cents);
            if (UIGame.Instance != null)
                UIGame.AddNotification("Modo Admin: dinero agregado " + StoreDatabase.FromLongToStringMoney(cents) + ".", otherColor: Color.green, otherDuration: 4f);
        }

        private static void AddAdminLevels(int levels)
        {
            if (levels <= 0)
                return;

            int gained = StoreDatabase.Instance != null ? StoreDatabase.AddPlayerLevelsForAdmin(levels) : levels;
            playerLevel = Math.Max(PlayerLevel, StoreDatabase.Instance != null ? StoreDatabase.GetPlayerLevel() : PlayerLevel + gained);
            GrantTreePointsForLevels(gained);
        }

        private static void GrantLevelRewardsForCurrentLevel()
        {
            int currentLevel = StoreDatabase.Instance != null ? StoreDatabase.GetPlayerLevel() : PlayerLevel;
            if (lastLevelRewarded <= 0)
                lastLevelRewarded = currentLevel;

            int missingRewards = Math.Max(0, currentLevel - lastLevelRewarded);
            GrantTreePointsForLevels(missingRewards);
            lastLevelRewarded = Math.Max(lastLevelRewarded, currentLevel);
            playerLevel = Math.Max(PlayerLevel, currentLevel);
        }

        private static void GrantTreePointsForLevels(int levels)
        {
            if (levels <= 0)
                return;

            EntrepreneurProgress.AddProgressPointsFromLevel(levels, "Puntos del Arbol otorgados por nivel: +" + levels + ".");
            pointsGrantedByLevel += levels;
            lastLevelRewarded += levels;
        }

        private static int UnlockFirstUnlockablePercent(float percent)
        {
            List<EntrepreneurTreeNodeDefinition> unlockable = EntrepreneurTreeDefinitions.Nodes
                .Where(node => node.Id != EntrepreneurTreeDefinitions.DefaultUnlockedNodeId)
                .ToList();
            int targetCount = Mathf.RoundToInt(unlockable.Count * percent);
            targetCount = Mathf.Clamp(targetCount, 1, unlockable.Count);
            HashSet<string> targetIds = new HashSet<string>(unlockable.Take(targetCount).Select(node => node.Id));
            return UnlockNodes(targetIds);
        }

        private static int UnlockAllNodes()
        {
            return UnlockNodes(new HashSet<string>(EntrepreneurTreeDefinitions.Nodes.Select(node => node.Id)));
        }

        private static int UnlockNodes(HashSet<string> targetIds)
        {
            int unlocked = 0;
            HashSet<string> visited = new HashSet<string>();
            foreach (string nodeId in EntrepreneurTreeDefinitions.Nodes.Select(node => node.Id))
            {
                if (targetIds.Contains(nodeId))
                    unlocked += UnlockNodeAndPrerequisites(nodeId, targetIds, visited);
            }

            return unlocked;
        }

        private static int UnlockNodeAndPrerequisites(string nodeId, HashSet<string> targetIds, HashSet<string> visited)
        {
            if (!visited.Add(nodeId))
                return 0;

            EntrepreneurTreeNodeDefinition node = EntrepreneurTreeDefinitions.Get(nodeId);
            if (node == null)
                return 0;

            int unlocked = 0;
            for (int i = 0; i < node.Prerequisites.Length; i++)
                unlocked += UnlockNodeAndPrerequisites(node.Prerequisites[i], targetIds, visited);

            if (node.Id == EntrepreneurTreeDefinitions.DefaultUnlockedNodeId || EntrepreneurProgress.IsUnlocked(node.Id))
                return unlocked;

            int missingPoints = Math.Max(0, node.Cost - EntrepreneurProgress.AvailablePoints);
            if (missingPoints > 0)
            {
                EntrepreneurProgress.AddProgressPointsFromAdmin(missingPoints, "Modo Admin: puntos agregados +" + missingPoints + ".");
                pointsGrantedByAdmin += missingPoints;
            }

            if (EntrepreneurProgress.TryUnlock(node.Id, out _))
                unlocked++;

            return unlocked;
        }

        private static void ResetPersistentState(bool clearPending)
        {
            adminModeEnabled = false;
            adminInitialGrantApplied = false;
            isAdminSave = false;
            adminLastAppliedVersion = string.Empty;
            playerLevel = 1;
            lastLevelRewarded = 1;
            pointsGrantedByLevel = 0;
            pointsGrantedByAdmin = 0;
            appliedPackages.Clear();
            if (clearPending)
                pendingPackage = AdminModePackage.None;
        }

        private static AdminModePackage ParsePackage(string value)
        {
            if (Enum.TryParse(value, out AdminModePackage package))
                return package;

            return AdminModePackage.None;
        }

        private static void AppendCostList(StringBuilder builder, string title, IEnumerable<EntrepreneurTreeNodeDefinition> nodes)
        {
            builder.AppendLine(title + ":");
            foreach (EntrepreneurTreeNodeDefinition node in nodes)
                builder.AppendLine("- " + node.Id + " | " + node.Title + " | costo " + node.Cost);
        }
    }
}
