/*  This file is part of the "Store Simulator" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store, Epic FAB).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */
/*  Adaptado por Isaac Victoria. */

using System;
using System.IO;
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
        public const string backupExt = ".bak";

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
            SceneManager.sceneLoaded += Instance.OnSceneLoaded;
        }


        /// <summary>
        /// Gather data from all game systems and save them to the device.
        /// Allow overwriting the file name with a different key, e.g. for another player profile.
        /// </summary>
        public static void Save(string otherKey = "")
        {
            string fileName = otherKey == string.Empty ? fileKey : otherKey;
            JSONNode data = new JSONObject();

            SafeSaveComponent(data, "UISettings", () => UISettings.Instance != null ? UISettings.Instance.SaveToJSON() : new JSONObject());
            SafeSaveComponent(data, "ItemDatabase", () => ItemDatabase.Instance != null ? ItemDatabase.Instance.SaveToJSON() : new JSONObject());
            SafeSaveComponent(data, "StoreDatabase", () => StoreDatabase.Instance != null ? StoreDatabase.Instance.SaveToJSON() : new JSONObject());
            SafeSaveComponent(data, "DayCycleSystem", () => DayCycleSystem.Instance != null ? DayCycleSystem.Instance.SaveToJSON() : new JSONObject());
            SafeSaveComponent(data, "StorageSystem", () => StorageSystem.Instance != null ? StorageSystem.Instance.SaveToJSON() : new JSONObject());
            SafeSaveComponent(data, "DeliverySystem", () => DeliverySystem.Instance != null ? DeliverySystem.Instance.SaveToJSON() : new JSONObject());
            SafeSaveComponent(data, "DailyEventSystem", () => DailyEventSystem.Instance != null ? DailyEventSystem.Instance.SaveToJSON() : new JSONObject());
            SafeSaveComponent(data, "CustomerSystem", () => CustomerSystem.Instance != null ? CustomerSystem.Instance.SaveToJSON() : new JSONObject());
            SafeSaveComponent(data, "TutorialSystem", () => TutorialSystem.Instance != null ? TutorialSystem.Instance.SaveToJSON() : new JSONObject());
            SafeSaveComponent(data, "StatsDatabase", () => StatsDatabase.Instance != null ? StatsDatabase.Instance.SaveToJSON() : new JSONObject());

            byte[] dataAsBytes = Encoding.UTF8.GetBytes(data.ToString());
            string path = Path.Combine(Application.persistentDataPath, fileName + fileExt);
            try
            {
                WriteAtomic(path, dataAsBytes);
                Debug.Log("[SaveSystem] Save completed: " + path);
                if (UIGame.Instance != null)
                    UIGame.AddNotification("Partida guardada correctamente.", otherColor: new Color(0.25f, 0.80f, 0.40f));
            }
            catch (Exception e)
            {
                Debug.LogError("[SaveSystem] Save failed. Last valid backup was preserved. " + e.Message);
                if (UIGame.Instance != null)
                    UIGame.AddNotification("Error al guardar. Se conservó el último respaldo.", otherColor: new Color(1f, 0.30f, 0.20f));
            }

            //notify subscribed scripts of data update
            InvokeEventSafe(dataSaveEvent, "dataSaveEvent");
        }


        /// <summary>
        /// Loads the local data and applies it to all game systems.
        /// Allow overwriting the file name with a different key, e.g. for another player profile.
        /// </summary>
        public static void Load(string otherKey = "")
        {
            string fileName = otherKey == string.Empty ? fileKey : otherKey;
            string dataString = string.Empty;

            string path = Path.Combine(Application.persistentDataPath, fileName + fileExt);
            if (File.Exists(path))
            {
                try
                {
                    byte[] dataAsBytes = File.ReadAllBytes(path);
                    dataString = Encoding.UTF8.GetString(dataAsBytes);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[SaveSystem] Load failed for primary save. Trying backup. " + e.Message);
                    dataString = TryReadBackup(path);
                }
            }
            else
            {
                dataString = TryReadBackup(path);
            }
            
            //savegame not found - create new game instead
            if (string.IsNullOrEmpty(dataString))
            {
                New();
                return;
            }

            Instance.gameData = TryParseSaveData(dataString, path, false);
            if (Instance.gameData == null)
            {
                string backupString = TryReadBackup(path);
                Instance.gameData = TryParseSaveData(backupString, path, true);
                if (Instance.gameData == null)
                {
                    Debug.LogWarning("[SaveSystem] Primary and backup saves are invalid. Starting new game.");
                    if (UIGame.Instance != null)
                        UIGame.AddNotification("No se pudo cargar la partida. Iniciando partida segura.", otherColor: new Color(1f, 0.30f, 0.20f));
                    New();
                    return;
                }

                Debug.LogWarning("[SaveSystem] Primary save was invalid. Backup loaded successfully.");
                if (UIGame.Instance != null)
                    UIGame.AddNotification("Save principal danado. Cargando backup.", otherColor: new Color(1f, 0.65f, 0.18f));
            }
            SceneManager.sceneLoaded += Instance.OnSceneLoaded;
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


        private static void WriteAtomic(string path, byte[] bytes)
        {
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string tempPath = path + ".tmp";
            string backupPath = path + backupExt;

            File.WriteAllBytes(tempPath, bytes);

            // Basic integrity check before replacing the known-good save.
            byte[] verifyBytes = File.ReadAllBytes(tempPath);
            if (verifyBytes == null || verifyBytes.Length != bytes.Length)
                throw new IOException("Temporary save verification failed.");
            if (!IsValidSaveRoot(JSON.Parse(Encoding.UTF8.GetString(verifyBytes))))
                throw new IOException("Temporary save JSON validation failed.");

            if (File.Exists(path))
                File.Copy(path, backupPath, true);

            if (File.Exists(path))
                File.Delete(path);
            File.Move(tempPath, path);
        }


        private static void SafeSaveComponent(JSONNode root, string key, Func<JSONNode> saveFunc)
        {
            try
            {
                JSONNode componentData = saveFunc != null ? saveFunc() : new JSONObject();
                root[key] = componentData ?? new JSONObject();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SaveSystem] Could not collect save data for " + key + ": " + e.Message);
                root[key] = new JSONObject();
            }
        }


        private static void SafeLoadComponent(string key, JSONNode root, Action<JSONNode> loadAction)
        {
            try
            {
                loadAction?.Invoke(root != null ? root[key] : null);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SaveSystem] Could not load data for " + key + ": " + e.Message);
            }
        }


        private static void InvokeEventSafe(Action evt, string eventName)
        {
            if (evt == null)
                return;

            Delegate[] listeners = evt.GetInvocationList();
            for (int i = 0; i < listeners.Length; i++)
            {
                Action listener = listeners[i] as Action;
                if (listener == null)
                    continue;

                try
                {
                    listener();
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[SaveSystem] Listener failed during " + eventName + ": " + e.Message);
                }
            }
        }


        private static string TryReadBackup(string path)
        {
            string backupPath = path + backupExt;
            if (!File.Exists(backupPath))
                return string.Empty;

            try
            {
                Debug.LogWarning("[SaveSystem] Loading backup save: " + backupPath);
                return Encoding.UTF8.GetString(File.ReadAllBytes(backupPath));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SaveSystem] Backup load failed: " + e.Message);
                return string.Empty;
            }
        }


        private static JSONNode TryParseSaveData(string dataString, string path, bool isBackup)
        {
            if (string.IsNullOrEmpty(dataString))
                return null;

            try
            {
                JSONNode parsed = JSON.Parse(dataString);
                if (!IsValidSaveRoot(parsed))
                {
                    Debug.LogWarning("[SaveSystem] " + (isBackup ? "Backup" : "Primary") + " save JSON parse returned null: " + path);
                    return null;
                }
                return parsed;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SaveSystem] " + (isBackup ? "Backup" : "Primary") + " save JSON parse failed: " + e.Message);
                return null;
            }
        }


        private static bool IsValidSaveRoot(JSONNode parsed)
        {
            return parsed != null && !parsed.IsNull && parsed.Count > 0 &&
                   parsed["StoreDatabase"] != null && parsed["StoreDatabase"].Count > 0 &&
                   parsed["ItemDatabase"] != null && parsed["ItemDatabase"].Count > 0;
        }


        //called on scene change for both a new or loaded save file
        //this makes sure we apply our local save file to the game systems in the scene
        //OnSceneLoaded is called after Awake(), but before Start(), making it possible to time actions
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            JSONNode data = gameData ?? new JSONObject();
            SafeLoadComponent("UISettings", data, node => { if (UISettings.Instance != null) UISettings.Instance.LoadFromJSON(node); });
            SafeLoadComponent("ItemDatabase", data, node => { if (ItemDatabase.Instance != null) ItemDatabase.Instance.LoadFromJSON(node); });
            SafeLoadComponent("StoreDatabase", data, node => { if (StoreDatabase.Instance != null) StoreDatabase.Instance.LoadFromJSON(node); });
            SafeLoadComponent("DayCycleSystem", data, node => { if (DayCycleSystem.Instance != null) DayCycleSystem.Instance.LoadFromJSON(node); });
            SafeLoadComponent("StorageSystem", data, node => { if (StorageSystem.Instance != null) StorageSystem.Instance.LoadFromJSON(node); });
            SafeLoadComponent("DeliverySystem", data, node => { if (DeliverySystem.Instance != null) DeliverySystem.Instance.LoadFromJSON(node); });
            SafeLoadComponent("DailyEventSystem", data, node => { if (DailyEventSystem.Instance != null) DailyEventSystem.Instance.LoadFromJSON(node); });
            SafeLoadComponent("CustomerSystem", data, node => { if (CustomerSystem.Instance != null) CustomerSystem.Instance.LoadFromJSON(node); });
            SafeLoadComponent("TutorialSystem", data, node => { if (TutorialSystem.Instance != null) TutorialSystem.Instance.LoadFromJSON(node); });
            SafeLoadComponent("StatsDatabase", data, node => { if (StatsDatabase.Instance != null) StatsDatabase.Instance.LoadFromJSON(node); });
            
            //notify subscribed scripts of data update
            InvokeEventSafe(dataLoadEvent, "dataLoadEvent");
            SceneManager.sceneLoaded -= Instance.OnSceneLoaded;
        }
    }
}
