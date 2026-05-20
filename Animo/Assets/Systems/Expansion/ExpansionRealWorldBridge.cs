using System.Collections.Generic;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Bridges the data-driven <see cref="SupermarketExpansionSystem"/> with the real 3D scene.
    ///
    /// When a zone is purchased this component:
    ///   1. Searches the scene for an <see cref="ExpansionObject"/> whose linked
    ///      <see cref="ExpansionScriptableObject"/> has an id matching the zone id (exact match
    ///      or prefix match like "1" → "zone_1").
    ///   2. If no ExpansionObject is found, looks for a GameObject named
    ///      "ExpansionZone_REAL_<zoneId>" or "ExpansionZone_<zoneId>".
    ///   3. If still nothing found, creates a primitive placeholder floor at the expected
    ///      world position and names it "ExpansionZone_REAL_<zoneId>".
    ///
    /// On data load it restores all previously purchased zones so the scene state
    /// matches the saved data.
    ///
    /// SCENE SETUP:
    ///   This component is added automatically by <see cref="EntrepreneurTreeUIBootstrap"/>.
    ///   No manual configuration needed.
    /// </summary>
    public class ExpansionRealWorldBridge : MonoBehaviour
    {
        private const string LogPrefix = "[ExpansionBridge] ";

        // World-space scale factor: 1 map pixel ≈ this many Unity units.
        // Adjust if your scene has a different scale.
        private const float MapToWorldScale = 0.25f;

        // Y position for placeholder floors.
        private const float PlaceholderFloorY = 0.01f;

        // Height of placeholder walls.
        private const float PlaceholderWallHeight = 3.0f;

        // Thickness of placeholder walls.
        private const float PlaceholderWallThickness = 0.2f;

        // Material colors for placeholder zones.
        private static readonly Color SalesColor   = new Color(0.20f, 0.55f, 0.20f, 0.40f);
        private static readonly Color StorageColor = new Color(0.20f, 0.35f, 0.65f, 0.40f);
        private static readonly Color OfficeColor  = new Color(0.55f, 0.45f, 0.20f, 0.40f);

        public static ExpansionRealWorldBridge Instance { get; private set; }

        // Keep track of placeholder GameObjects we created so we can manage them.
        private readonly Dictionary<string, GameObject> placeholders =
            new Dictionary<string, GameObject>();

        // ── Lifecycle ──────────────────────────────────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            SupermarketExpansionSystem.onZonePurchased += OnZonePurchased;
            SupermarketExpansionSystem.onZonesReset    += OnZonesReset;
            SaveGameSystem.dataLoadEvent               += OnDataLoaded;
        }

        void Start()
        {
            // Restore scene state for any zones already purchased at startup.
            RestoreAllPurchasedZones();
        }

        void OnDestroy()
        {
            SupermarketExpansionSystem.onZonePurchased -= OnZonePurchased;
            SupermarketExpansionSystem.onZonesReset    -= OnZonesReset;
            SaveGameSystem.dataLoadEvent               -= OnDataLoaded;

            if (Instance == this)
                Instance = null;
        }

        // ── Event handlers ─────────────────────────────────────────────────────

        private void OnZonePurchased(ExpansionZoneData zone)
        {
            if (zone == null)
                return;

            ActivateZone(zone);
        }

        private void OnZonesReset()
        {
            RestoreAllPurchasedZones();
        }

        private void OnDataLoaded()
        {
            RestoreAllPurchasedZones();
        }

        // ── Core activation logic ──────────────────────────────────────────────

        private void RestoreAllPurchasedZones()
        {
            if (SupermarketExpansionSystem.Instance == null)
                return;

            IReadOnlyList<ExpansionZoneData> zones = SupermarketExpansionSystem.Instance.Zones;
            for (int i = 0; i < zones.Count; i++)
            {
                ExpansionZoneData zone = zones[i];
                if (zone != null && zone.state == ExpansionZoneState.Purchased && zone.price > 0)
                    ActivateZone(zone);
            }
        }

        private void ActivateZone(ExpansionZoneData zone)
        {
            // 1. Try matching ExpansionObject in the scene.
            if (TryActivateExpansionObject(zone))
                return;

            // 2. Try activating a GameObject named by convention.
            if (TryActivateNamedGameObject(zone))
                return;

            // 3. Create a placeholder if nothing found.
            CreateOrShowPlaceholder(zone);
        }

        // ── Attempt 1: ExpansionObject ─────────────────────────────────────────

        private bool TryActivateExpansionObject(ExpansionZoneData zone)
        {
            ExpansionObject[] allExpansionObjects = FindExpansionObjects();
            for (int i = 0; i < allExpansionObjects.Length; i++)
            {
                ExpansionObject eo = allExpansionObjects[i];
                if (eo == null || eo.expansion == null)
                    continue;

                string expId = eo.expansion.id ?? string.Empty;
                if (IsIdMatch(expId, zone.id))
                {
                    // Mark the ScriptableObject as purchased so ExpansionObject's
                    // Start() check doesn't re-hide it, and fire the UpgradeSystem
                    // event so ExpansionObject.OnExpansionPurchase responds.
                    if (!eo.expansion.isPurchased)
                    {
                        eo.expansion.isPurchased = true;
                        UpgradeSystem.NotifyPurchase(eo.expansion);
                        Debug.Log(LogPrefix + "Activated ExpansionObject for zone '" + zone.id
                            + "' via expansion '" + expId + "'.");
                    }

                    return true;
                }
            }

            return false;
        }

        private static bool IsIdMatch(string expansionId, string zoneId)
        {
            if (string.IsNullOrEmpty(expansionId) || string.IsNullOrEmpty(zoneId))
                return false;

            string expNorm  = expansionId.Trim().ToLowerInvariant();
            string zoneNorm = zoneId.Trim().ToLowerInvariant();

            // Exact match.
            if (expNorm == zoneNorm)
                return true;

            // Numeric suffix match: "1" matches "sales_w1", "storage_n1", etc.
            if (zoneNorm.EndsWith(expNorm))
                return true;

            // Allow "expansion_N" ↔ "zone_N" style matching.
            if (expNorm.Contains(zoneNorm) || zoneNorm.Contains(expNorm))
                return true;

            return false;
        }

        // ── Attempt 2: Named GameObject ────────────────────────────────────────

        private static bool TryActivateNamedGameObject(ExpansionZoneData zone)
        {
            // Look for GameObjects named with conventional prefixes.
            string[] candidateNames =
            {
                "ExpansionZone_REAL_" + zone.id,
                "ExpansionZone_"      + zone.id,
                "Expansion_"          + zone.id,
                zone.id,
            };

            for (int n = 0; n < candidateNames.Length; n++)
            {
                GameObject go = GameObject.Find(candidateNames[n]);
                if (go != null)
                {
                    go.SetActive(true);
                    Debug.Log(LogPrefix + "Activated zone '" + zone.id + "' via named GameObject '"
                        + candidateNames[n] + "'.");
                    return true;
                }
            }

            return false;
        }

        // ── Attempt 3: Placeholder primitive ──────────────────────────────────

        private void CreateOrShowPlaceholder(ExpansionZoneData zone)
        {
            // If we already created this placeholder before, just enable it.
            if (placeholders.TryGetValue(zone.id, out GameObject existing))
            {
                if (existing != null)
                {
                    existing.SetActive(true);
                    return;
                }

                placeholders.Remove(zone.id);
            }

            // Determine world position from zone's map position.
            // mapPosition is in pixels; we convert to approximate world coords.
            // The map origin (0,0) corresponds to roughly (0, 0, 0) in world space.
            Vector3 worldPos = MapToWorld(zone.mapPosition, zone.mapSize);

            // Build a placeholder root.
            GameObject root = new GameObject("ExpansionZone_REAL_" + zone.id);
            root.transform.position = worldPos;

            // Floor plane.
            float worldWidth  = zone.mapSize.x * MapToWorldScale;
            float worldDepth  = zone.mapSize.y * MapToWorldScale;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(root.transform, false);
            // Unity's Plane default is 10x10 units; scale down to desired size.
            floor.transform.localScale = new Vector3(worldWidth * 0.1f, 1f, worldDepth * 0.1f);
            floor.transform.localPosition = Vector3.zero;
            SetTransparentColor(floor, GetZoneColor(zone.type));

            // Remove collider from floor (avoid blocking customer pathfinding).
            Collider floorCol = floor.GetComponent<Collider>();
            if (floorCol != null)
                floorCol.enabled = false;

            placeholders[zone.id] = root;

            Debug.Log(LogPrefix + "Created placeholder for zone '" + zone.id
                + "' at world pos " + worldPos + " size (" + worldWidth + " x " + worldDepth + ").");
        }

        private static Vector3 MapToWorld(Vector2 mapPos, Vector2 mapSize)
        {
            // Center of the zone in world space, placed at floor level.
            float centerX = (mapPos.x + mapSize.x * 0.5f) * MapToWorldScale;
            float centerZ = (mapPos.y + mapSize.y * 0.5f) * MapToWorldScale;
            return new Vector3(centerX, PlaceholderFloorY, centerZ);
        }

        private static Color GetZoneColor(ExpansionZoneType type)
        {
            switch (type)
            {
                case ExpansionZoneType.Sales:   return SalesColor;
                case ExpansionZoneType.Storage: return StorageColor;
                default:                        return OfficeColor;
            }
        }

        private static void SetTransparentColor(GameObject go, Color color)
        {
            Renderer r = go.GetComponent<Renderer>();
            if (r == null)
                return;

            // Instantiate a new material so we don't pollute the default.
            Material mat = new Material(Shader.Find("Transparent/Diffuse") ?? Shader.Find("Standard"));
            mat.color = color;
            if (mat.HasProperty("_Mode"))
            {
                mat.SetFloat("_Mode", 3);           // Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }

            r.material = mat;
        }

        // ── Unity object finders ───────────────────────────────────────────────

        private static ExpansionObject[] FindExpansionObjects()
        {
#if UNITY_2022_2_OR_NEWER
            return Object.FindObjectsByType<ExpansionObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            return Object.FindObjectsOfType<ExpansionObject>(true);
#endif
        }
    }
}
