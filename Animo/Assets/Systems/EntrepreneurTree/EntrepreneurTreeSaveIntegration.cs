// Árbol del Emprendedor — EntrepreneurTreeSaveIntegration
// Non-invasive bridge between our new systems and the existing SaveGameSystem.
// Saves/loads to a SEPARATE file so the asset's SaveGameSystem.cs is never touched.

using System;
using System.IO;
using System.Text;
using UnityEngine;
using SimpleJSON;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Bridges EntrepreneurTreeManager and AchievementSystem with the existing SaveGameSystem
    /// WITHOUT modifying any asset scripts.
    ///
    /// Strategy:
    ///   • Hooks into SaveGameSystem.dataSaveEvent  → writes our data to a second JSON file.
    ///   • Hooks into SaveGameSystem.dataLoadEvent  → reads our data from that file.
    ///   • The main save file is untouched.
    ///
    /// SCENE SETUP:
    ///   Add this component to the same GameObject as EntrepreneurTreeManager and AchievementSystem.
    /// </summary>
    public class EntrepreneurTreeSaveIntegration : MonoBehaviour
    {
        private const string LogPrefix = "[EntrepreneurTree] ";
        // File written next to the main "save.dat" file.
        private const string fileName = "entrepreneurTree";


        void Awake()
        {
            SaveGameSystem.dataSaveEvent += OnSave;
            SaveGameSystem.dataLoadEvent += OnLoad;
            Debug.Log(LogPrefix + "Save integration subscribed to SaveGameSystem events.");
        }


        // ── Save ──────────────────────────────────────────────────────────────────

        private void OnSave()
        {
            JSONNode data = new JSONObject();

            if (EntrepreneurTreeManager.Instance != null)
                data["EntrepreneurTreeManager"] = EntrepreneurTreeManager.Instance.SaveToJSON();

            if (AchievementSystem.Instance != null)
                data["AchievementSystem"] = AchievementSystem.Instance.SaveToJSON();

            if (EntrepreneurEmployeeSystem.Instance != null)
                data["EntrepreneurEmployeeSystem"] = EntrepreneurEmployeeSystem.Instance.SaveToJSON();

            if (ShoplifterSystem.Instance != null)
                data["ShoplifterSystem"] = ShoplifterSystem.Instance.SaveToJSON();

            if (EmployeeWorkstationRegistry.Instance != null)
                data["EmployeeWorkstationRegistry"] = EmployeeWorkstationRegistry.Instance.SaveToJSON();

            byte[] bytes = Encoding.UTF8.GetBytes(data.ToString());
            string path  = Path.Combine(Application.persistentDataPath, fileName + SaveGameSystem.fileExt);

            try { WriteAtomic(path, bytes); }
            catch (Exception e)
            {
                Debug.LogWarning(LogPrefix + "Failed to save progress data at path '" + path + "': " + e.Message);
                return;
            }

            Debug.Log(LogPrefix + "Progress saved: " + path);
            if (UIGame.Instance != null)
                UIGame.AddNotification("Partida guardada correctamente.", otherColor: new Color(0.25f, 0.80f, 0.40f));
        }


        // ── Load ──────────────────────────────────────────────────────────────────

        private void OnLoad()
        {
            string path = Path.Combine(Application.persistentDataPath, fileName + SaveGameSystem.fileExt);

            if (!File.Exists(path) && !File.Exists(path + SaveGameSystem.backupExt))
            {
                // New game or first run – reset managers to defaults.
                EntrepreneurTreeManager.Instance?.LoadFromJSON(null);
                AchievementSystem.Instance?.LoadFromJSON(null);
                EntrepreneurEmployeeSystem.Instance?.LoadFromJSON(null);
                ShoplifterSystem.Instance?.LoadFromJSON(null);
                Debug.Log(LogPrefix + "No EntrepreneurTree save file found. Loaded defaults.");
                return;
            }

            path = ResolveValidProgressPath(path);
            if (string.IsNullOrEmpty(path))
            {
                EntrepreneurTreeManager.Instance?.LoadFromJSON(null);
                AchievementSystem.Instance?.LoadFromJSON(null);
                EntrepreneurEmployeeSystem.Instance?.LoadFromJSON(null);
                ShoplifterSystem.Instance?.LoadFromJSON(null);
                Debug.LogWarning(LogPrefix + "Progress file and backup were invalid. Loaded defaults.");
                return;
            }

            byte[] bytes;
            string json;
            try
            {
                bytes = File.ReadAllBytes(path);
                json = Encoding.UTF8.GetString(bytes);
            }
            catch (Exception e)
            {
                Debug.LogWarning(LogPrefix + "Failed to read progress file. Loading defaults. " + e.Message);
                EntrepreneurTreeManager.Instance?.LoadFromJSON(null);
                AchievementSystem.Instance?.LoadFromJSON(null);
                EntrepreneurEmployeeSystem.Instance?.LoadFromJSON(null);
                ShoplifterSystem.Instance?.LoadFromJSON(null);
                return;
            }

            if (string.IsNullOrEmpty(json))
            {
                EntrepreneurTreeManager.Instance?.LoadFromJSON(null);
                AchievementSystem.Instance?.LoadFromJSON(null);
                EntrepreneurEmployeeSystem.Instance?.LoadFromJSON(null);
                ShoplifterSystem.Instance?.LoadFromJSON(null);
                Debug.LogWarning(LogPrefix + "Progress file was empty. Loaded defaults.");
                return;
            }

            JSONNode data;
            try
            {
                data = JSON.Parse(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning(LogPrefix + "Progress file JSON parse failed. Loading defaults. " + e.Message);
                EntrepreneurTreeManager.Instance?.LoadFromJSON(null);
                AchievementSystem.Instance?.LoadFromJSON(null);
                EntrepreneurEmployeeSystem.Instance?.LoadFromJSON(null);
                ShoplifterSystem.Instance?.LoadFromJSON(null);
                return;
            }

            EntrepreneurTreeManager.Instance?.LoadFromJSON(data["EntrepreneurTreeManager"]);
            AchievementSystem.Instance?.LoadFromJSON(data["AchievementSystem"]);
            EntrepreneurEmployeeSystem.Instance?.LoadFromJSON(data["EntrepreneurEmployeeSystem"]);
            ShoplifterSystem.Instance?.LoadFromJSON(data["ShoplifterSystem"]);
            EmployeeWorkstationRegistry.Instance?.LoadFromJSON(data["EmployeeWorkstationRegistry"]);
            Debug.Log(LogPrefix + "Progress loaded successfully.");
        }


        void OnDestroy()
        {
            SaveGameSystem.dataSaveEvent -= OnSave;
            SaveGameSystem.dataLoadEvent -= OnLoad;
        }


        /// <summary>
        /// Returns a copy of a single component's data from the EntrepreneurTree save file.
        /// Returns an empty JSONObject when the file does not exist or cannot be read.
        /// </summary>
        public static JSONNode ReadComponentData(string component)
        {
            string path = Path.Combine(Application.persistentDataPath, fileName + SaveGameSystem.fileExt);
            if (!File.Exists(path))
                return new JSONObject();

            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                string json  = Encoding.UTF8.GetString(bytes);
                if (string.IsNullOrEmpty(json))
                    return new JSONObject();

                JSONNode data = JSON.Parse(json);
                JSONNode node = data[component];
                return node != null ? node.Clone() : new JSONObject();
            }
            catch (Exception e)
            {
                Debug.LogWarning(LogPrefix + "ReadComponentData(\"" + component + "\") failed: " + e.Message);
                return new JSONObject();
            }
        }


        private static void WriteAtomic(string path, byte[] bytes)
        {
            string tempPath = path + ".tmp";
            string backupPath = path + SaveGameSystem.backupExt;

            File.WriteAllBytes(tempPath, bytes);
            byte[] verify = File.ReadAllBytes(tempPath);
            if (verify == null || verify.Length != bytes.Length)
                throw new IOException("Temporary save verification failed.");
            if (JSON.Parse(Encoding.UTF8.GetString(verify)) == null)
                throw new IOException("Temporary progress save JSON validation failed.");

            if (File.Exists(path))
                File.Copy(path, backupPath, true);
            if (File.Exists(path))
                File.Delete(path);
            File.Move(tempPath, path);
        }


        private static string ResolveValidProgressPath(string path)
        {
            if (CanParse(path))
                return path;

            string backupPath = path + SaveGameSystem.backupExt;
            if (!CanParse(backupPath))
                return string.Empty;

            Debug.LogWarning(LogPrefix + "Primary progress save invalid. Loaded backup.");
            if (UIGame.Instance != null)
                UIGame.AddNotification("Progreso recuperado desde respaldo.", otherColor: new Color(1f, 0.65f, 0.18f));
            return backupPath;
        }


        private static bool CanParse(string path)
        {
            if (!File.Exists(path))
                return false;

            try
            {
                string json = Encoding.UTF8.GetString(File.ReadAllBytes(path));
                return !string.IsNullOrWhiteSpace(json) && JSON.Parse(json) != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
