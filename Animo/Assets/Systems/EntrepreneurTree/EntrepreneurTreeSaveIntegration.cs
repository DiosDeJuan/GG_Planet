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

            byte[] bytes = Encoding.ASCII.GetBytes(data.ToString());
            string path  = Application.persistentDataPath + "/" + fileName + SaveGameSystem.fileExt;

            try { File.WriteAllBytes(path, bytes); }
            catch (Exception e)
            {
                Debug.LogWarning(LogPrefix + "Failed to save progress data: " + e.Message);
                return;
            }

            Debug.Log(LogPrefix + "Progress saved: " + path);
        }


        // ── Load ──────────────────────────────────────────────────────────────────

        private void OnLoad()
        {
            string path = Application.persistentDataPath + "/" + fileName + SaveGameSystem.fileExt;

            if (!File.Exists(path))
            {
                // New game or first run – reset managers to defaults.
                EntrepreneurTreeManager.Instance?.LoadFromJSON(null);
                AchievementSystem.Instance?.LoadFromJSON(null);
                Debug.Log(LogPrefix + "No EntrepreneurTree save file found. Loaded defaults.");
                return;
            }

            byte[] bytes;
            string json;
            try
            {
                bytes = File.ReadAllBytes(path);
                json = Encoding.ASCII.GetString(bytes);
            }
            catch (Exception e)
            {
                Debug.LogWarning(LogPrefix + "Failed to read progress file. Loading defaults. " + e.Message);
                EntrepreneurTreeManager.Instance?.LoadFromJSON(null);
                AchievementSystem.Instance?.LoadFromJSON(null);
                return;
            }

            if (string.IsNullOrEmpty(json))
            {
                EntrepreneurTreeManager.Instance?.LoadFromJSON(null);
                AchievementSystem.Instance?.LoadFromJSON(null);
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
                return;
            }

            EntrepreneurTreeManager.Instance?.LoadFromJSON(data["EntrepreneurTreeManager"]);
            AchievementSystem.Instance?.LoadFromJSON(data["AchievementSystem"]);
            Debug.Log(LogPrefix + "TreeData loaded successfully.");
        }


        void OnDestroy()
        {
            SaveGameSystem.dataSaveEvent -= OnSave;
            SaveGameSystem.dataLoadEvent -= OnLoad;
        }
    }
}
