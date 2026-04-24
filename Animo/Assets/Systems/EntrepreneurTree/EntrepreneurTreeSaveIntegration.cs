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
        // File written next to the main "save.dat" file.
        private const string fileName = "entrepreneurTree";


        void Awake()
        {
            SaveGameSystem.dataSaveEvent += OnSave;
            SaveGameSystem.dataLoadEvent += OnLoad;
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
            catch (Exception) { }
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
                return;
            }

            byte[] bytes = File.ReadAllBytes(path);
            string json  = Encoding.ASCII.GetString(bytes);

            if (string.IsNullOrEmpty(json))
                return;

            JSONNode data = JSON.Parse(json);

            EntrepreneurTreeManager.Instance?.LoadFromJSON(data["EntrepreneurTreeManager"]);
            AchievementSystem.Instance?.LoadFromJSON(data["AchievementSystem"]);
        }


        void OnDestroy()
        {
            SaveGameSystem.dataSaveEvent -= OnSave;
            SaveGameSystem.dataLoadEvent -= OnLoad;
        }
    }
}
