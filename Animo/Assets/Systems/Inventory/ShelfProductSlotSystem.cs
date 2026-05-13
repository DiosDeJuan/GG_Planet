using System;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Logical shelf-slot system.
    ///
    /// Bridges the asset's <see cref="PlacementObject"/> components and the
    /// product catalogue so the player (or restockers) can assign a specific
    /// product to each shelf/rack placement.
    ///
    /// Design notes
    /// ────────────
    /// • Does NOT add new components to PlacementObject prefabs.
    /// • Discovers PlacementObjects at runtime via FindObjectsByType.
    /// • Sets <see cref="PlacementObject.product"/> when the player assigns a product,
    ///   so the existing <see cref="EmployeeRestockCoordinator"/> automatically
    ///   prefers those placements during its restock cycles.
    /// • Persists assignments in a separate file "<see cref="SaveFileName"/>.dat"
    ///   by subscribing to <see cref="SaveGameSystem"/>'s save/load events.
    ///
    /// Log prefix: [Shelf]
    /// </summary>
    public class ShelfProductSlotSystem : MonoBehaviour
    {
        private const string LogPrefix    = "[Shelf] ";
        private const string SaveFileName = "shelfSlots";
        private const int    SaveVersion  = 1;

        // ── Singleton ──────────────────────────────────────────────────────────
        public static ShelfProductSlotSystem Instance { get; private set; }

        // ── Events ─────────────────────────────────────────────────────────────
        /// <summary>Fired whenever a slot assignment is added, changed, or cleared.</summary>
        public static event Action onSlotsChanged;

        // ── Internal state ─────────────────────────────────────────────────────
        /// Key   = stable placement identifier (GameObject name).
        /// Value = id of the assigned ProductScriptableObject, or null if none.
        private readonly Dictionary<string, string> _assignments =
            new Dictionary<string, string>(StringComparer.Ordinal);

        // Cache of all registered PlacementObjects, refreshed on load and on demand.
        private readonly List<PlacementObject> _registeredPlacements =
            new List<PlacementObject>();

        // ── Unity lifecycle ────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            SaveGameSystem.dataSaveEvent += OnSave;
            SaveGameSystem.dataLoadEvent += OnLoad;
        }

        private void OnDisable()
        {
            SaveGameSystem.dataSaveEvent -= OnSave;
            SaveGameSystem.dataLoadEvent -= OnLoad;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start()
        {
            ScanScene();
        }

        // ── Scene scan ─────────────────────────────────────────────────────────

        /// <summary>
        /// Discovers all <see cref="PlacementObject"/> components in the current scene
        /// and registers them.  Existing assignments are re-applied.
        /// </summary>
        public void ScanScene()
        {
            _registeredPlacements.Clear();
#if UNITY_2022_2_OR_NEWER
            PlacementObject[] found = FindObjectsByType<PlacementObject>(FindObjectsSortMode.None);
#else
            PlacementObject[] found = FindObjectsOfType<PlacementObject>();
#endif
            for (int i = 0; i < found.Length; i++)
            {
                if (found[i] != null)
                    _registeredPlacements.Add(found[i]);
            }

            Debug.Log(LogPrefix + "Scene scan: " + _registeredPlacements.Count + " placement(s) registered.");
            ApplyAssignmentsToScene();
        }

        // ── Public query API ───────────────────────────────────────────────────

        /// <summary>All discovered PlacementObjects (read-only).</summary>
        public IReadOnlyList<PlacementObject> RegisteredPlacements => _registeredPlacements;

        /// <summary>Returns the product assigned to <paramref name="placement"/>, or null.</summary>
        public ProductScriptableObject GetAssignedProduct(PlacementObject placement)
        {
            if (placement == null)
                return null;
            string key = GetKey(placement);
            if (!_assignments.TryGetValue(key, out string productIdStr))
                return null;
            return FindProductById(productIdStr);
        }

        /// <summary>How many placements currently have <paramref name="product"/> assigned.</summary>
        public int GetAssignedCount(ProductScriptableObject product)
        {
            if (product == null)
                return 0;
            string idStr = product.id.ToString();
            int count = 0;
            foreach (string v in _assignments.Values)
            {
                if (string.Equals(v, idStr, StringComparison.Ordinal))
                    count++;
            }
            return count;
        }

        /// <summary>True if at least one placement has any product assigned.</summary>
        public bool HasAnySlotAssigned()
        {
            foreach (string v in _assignments.Values)
            {
                if (!string.IsNullOrEmpty(v))
                    return true;
            }
            return false;
        }

        // ── Assignment ─────────────────────────────────────────────────────────

        /// <summary>
        /// Assigns <paramref name="product"/> to <paramref name="placement"/>.
        /// Pass null as product to clear the assignment.
        /// </summary>
        /// <returns>True on success; false if compatibility fails or placement is null.</returns>
        public bool AssignProduct(PlacementObject placement,
                                  ProductScriptableObject product,
                                  out string reason)
        {
            reason = null;

            if (placement == null)
            {
                reason = "Mueble no válido.";
                return false;
            }

            // Clearing an assignment is always allowed.
            if (product == null)
            {
                ClearAssignment(placement);
                return true;
            }

            // Validate tree unlock.
            if (EntrepreneurTreeGameplayBridge.Instance != null
                && !EntrepreneurTreeGameplayBridge.Instance.IsProductUnlocked(product))
            {
                reason = "Producto bloqueado en el Árbol del Emprendedor.";
                return false;
            }

            // Validate StorageType compatibility.
            if (!IsCompatible(product, placement))
            {
                reason = string.Format(
                    "Incompatible: {0} ({1}) no puede ir en {2} ({3}).",
                    product.title, product.storageType,
                    placement.gameObject.name, placement.storageType);
                if (UIGame.Instance != null)
                    UIGame.AddNotification(
                        "Mueble incorrecto: " + product.title + " → " + placement.gameObject.name,
                        otherColor: new Color(1f, 0.30f, 0.30f, 1f));
                Debug.LogWarning(LogPrefix + reason);
                return false;
            }

            string key       = GetKey(placement);
            string productId = product.id.ToString();
            _assignments[key] = productId;

            // Apply to the live PlacementObject so EmployeeRestockCoordinator picks it up.
            placement.product = product;

            Debug.Log(LogPrefix + "Assigned '" + product.title + "' → '" + placement.gameObject.name + "'.");
            onSlotsChanged?.Invoke();
            return true;
        }

        /// <summary>Removes the product assignment for <paramref name="placement"/>.</summary>
        public void ClearAssignment(PlacementObject placement)
        {
            if (placement == null)
                return;

            string key = GetKey(placement);
            if (_assignments.ContainsKey(key))
            {
                _assignments.Remove(key);
                placement.product = null;
                Debug.Log(LogPrefix + "Cleared assignment for '" + placement.gameObject.name + "'.");
                onSlotsChanged?.Invoke();
            }
        }

        // ── Compatibility helper ───────────────────────────────────────────────

        /// <summary>
        /// Checks StorageType compatibility between a product and a placement.
        /// Default (0) always matches any placement.
        /// </summary>
        public static bool IsCompatible(ProductScriptableObject product, PlacementObject placement)
        {
            if (product == null || placement == null)
                return false;
            // Default storageType on product = any placement is OK.
            if (product.storageType == StorageType.Default)
                return true;
            return product.storageType == placement.storageType;
        }

        // ── Save / Load ────────────────────────────────────────────────────────

        private void OnSave()
        {
            JSONNode root = new JSONObject();
            root["version"] = SaveVersion;

            JSONArray arr = new JSONArray();
            foreach (KeyValuePair<string, string> kv in _assignments)
            {
                if (string.IsNullOrEmpty(kv.Value))
                    continue;
                JSONObject entry = new JSONObject();
                entry["name"]      = kv.Key;
                entry["productId"] = kv.Value;
                arr.Add(entry);
            }
            root["slots"] = arr;

            string path = Path.Combine(Application.persistentDataPath, SaveFileName + SaveGameSystem.fileExt);
            try
            {
                File.WriteAllText(path, root.ToString());
                Debug.Log(LogPrefix + "Saved " + arr.Count + " slot assignment(s).");
            }
            catch (Exception ex)
            {
                Debug.LogError(LogPrefix + "Save failed: " + ex.Message);
            }
        }

        private void OnLoad()
        {
            _assignments.Clear();

            string path = Path.Combine(Application.persistentDataPath, SaveFileName + SaveGameSystem.fileExt);
            if (!File.Exists(path))
            {
                Debug.Log(LogPrefix + "No saved slot file found — starting fresh.");
                ScanScene();
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                JSONNode root = JSON.Parse(json);
                if (root == null)
                {
                    Debug.LogWarning(LogPrefix + "Slot save file is empty or invalid.");
                    ScanScene();
                    return;
                }

                JSONArray arr = root["slots"].AsArray;
                if (arr != null)
                {
                    for (int i = 0; i < arr.Count; i++)
                    {
                        string name      = arr[i]["name"].Value;
                        string productId = arr[i]["productId"].Value;
                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(productId))
                            _assignments[name] = productId;
                    }
                }

                Debug.Log(LogPrefix + "Loaded " + _assignments.Count + " slot assignment(s).");
            }
            catch (Exception ex)
            {
                Debug.LogError(LogPrefix + "Load failed: " + ex.Message);
            }

            ScanScene();     // re-register placements and reapply loaded assignments
            onSlotsChanged?.Invoke();
        }

        // ── Internals ──────────────────────────────────────────────────────────

        private void ApplyAssignmentsToScene()
        {
            for (int i = 0; i < _registeredPlacements.Count; i++)
            {
                PlacementObject pl = _registeredPlacements[i];
                if (pl == null)
                    continue;

                string key = GetKey(pl);
                if (!_assignments.TryGetValue(key, out string productIdStr))
                    continue;

                ProductScriptableObject product = FindProductById(productIdStr);
                if (product == null)
                {
                    Debug.LogWarning(LogPrefix + "Saved product id '" + productIdStr
                        + "' not found in ItemDatabase — clearing assignment for '" + pl.gameObject.name + "'.");
                    _assignments.Remove(key);
                    continue;
                }

                pl.product = product;
                Debug.Log(LogPrefix + "Restored '" + product.title + "' → '" + pl.gameObject.name + "'.");
            }
        }

        /// <summary>
        /// Stable key for a PlacementObject.  Uses the full GameObject path so
        /// duplicate names in different scene hierarchies stay distinct.
        /// </summary>
        private static string GetKey(PlacementObject placement)
        {
            return GetFullPath(placement.transform);
        }

        private static string GetFullPath(Transform t)
        {
            if (t.parent == null)
                return t.name;
            return GetFullPath(t.parent) + "/" + t.name;
        }

        private static ProductScriptableObject FindProductById(string idStr)
        {
            if (string.IsNullOrEmpty(idStr))
                return null;
            if (!int.TryParse(idStr, out int id))
                return null;

            List<PurchasableScriptableObject> all =
                ItemDatabase.GetByType(typeof(ProductScriptableObject));
            for (int i = 0; i < all.Count; i++)
            {
                ProductScriptableObject product = all[i] as ProductScriptableObject;
                if (product != null && product.id == id)
                    return product;
            }
            return null;
        }
    }
}
