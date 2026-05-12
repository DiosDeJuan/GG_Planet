using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleJSON;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Manages the supermarket expansion grid: zone definitions, purchase logic,
    /// adjacency-based unlocking, and save/load integration.
    /// </summary>
    public class SupermarketExpansionSystem : MonoBehaviour
    {
        private const string LogPrefix = "[Expansion] ";
        private const string SaveFileName = "expansionApp";

        private readonly List<ExpansionZoneData> zones = new List<ExpansionZoneData>();
        private readonly HashSet<string> purchasedZoneIds = new HashSet<string>();

        public static SupermarketExpansionSystem Instance { get; private set; }

        /// <summary>Fired after any zone purchase or incremental state change.</summary>
        public static event Action onZonesChanged;

        /// <summary>Fired after a full zone reset (e.g. on data load). UI should rebuild map.</summary>
        public static event Action onZonesReset;

        /// <summary>Fired immediately after a zone is successfully purchased.</summary>
        public static event Action<ExpansionZoneData> onZonePurchased;

        public IReadOnlyList<ExpansionZoneData> Zones => zones;

        // ── Map coordinate constants ─────────────────────────────────────────
        // Grid: 8 cols × 5 rows, cell 44 px, stride 48 px, origin (3,3).
        // Total canvas: ~383 × 239 px.
        private const float C = 44f;   // cell size
        private const float S = 48f;   // cell stride
        private const float O = 3f;    // origin offset

        // ── Prices ───────────────────────────────────────────────────────────
        private const long SalesPrice   = 175000L;  // $1,750 (cents)
        private const long StoragePrice = 250000L;  // $2,500 (cents)

        // ─────────────────────────────────────────────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            EnsureDefaultZones();
            SaveGameSystem.dataSaveEvent += OnSave;
            SaveGameSystem.dataLoadEvent += OnLoad;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Attempts to purchase the zone with the given ID.
        /// Returns true on success; on failure sets missingFunds or reason.
        /// </summary>
        public bool TryPurchaseZone(string zoneId, out long missingFunds, out string reason)
        {
            missingFunds = 0;
            reason = string.Empty;

            ExpansionZoneData zone = GetZone(zoneId);
            if (zone == null)
            {
                reason = "Zona no encontrada.";
                return false;
            }

            if (zone.state == ExpansionZoneState.Purchased)
            {
                reason = "Esta zona ya fue comprada.";
                return false;
            }

            if (zone.state == ExpansionZoneState.Blocked)
            {
                reason = string.IsNullOrEmpty(zone.blockedReason)
                    ? "Zona bloqueada: compra primero una zona vecina."
                    : zone.blockedReason;
                return false;
            }

            if (StoreDatabase.Instance == null)
            {
                reason = "Sistema de dinero no disponible.";
                Debug.LogWarning(LogPrefix + "Purchase failed: StoreDatabase.Instance is null.");
                return false;
            }

            if (!StoreDatabase.CanPurchase(zone.price))
            {
                long money = StoreDatabase.Instance.currentMoney;
                missingFunds = Math.Max(0L, zone.price - money);
                reason = "Fondos insuficientes.";
                Debug.Log(LogPrefix + "Purchase blocked: insufficient funds for " + zoneId
                    + ". Missing " + StoreDatabase.FromLongToStringMoney(missingFunds) + ".");
                return false;
            }

            StoreDatabase.AddRemoveMoney(-zone.price);
            zone.state = ExpansionZoneState.Purchased;
            zone.blockedReason = string.Empty;
            purchasedZoneIds.Add(zone.id);

            EvaluateBlockedZones();

            onZonePurchased?.Invoke(zone);
            onZonesChanged?.Invoke();

            Debug.Log(LogPrefix + "Zone " + zoneId + " purchased for "
                + StoreDatabase.FromLongToStringMoney(zone.price) + ".");
            return true;
        }

        public ExpansionZoneData GetZone(string zoneId)
        {
            for (int i = 0; i < zones.Count; i++)
                if (zones[i] != null && zones[i].id == zoneId)
                    return zones[i];
            return null;
        }

        /// <summary>Total purchased sales area in m².</summary>
        public int GetPurchasedSalesAreaM2()
        {
            int total = 0;
            for (int i = 0; i < zones.Count; i++)
            {
                ExpansionZoneData z = zones[i];
                if (z != null && z.type == ExpansionZoneType.Sales && z.state == ExpansionZoneState.Purchased)
                    total += z.sizeSquareMeters;
            }
            return total;
        }

        /// <summary>Total purchased storage area in m².</summary>
        public int GetPurchasedStorageAreaM2()
        {
            int total = 0;
            for (int i = 0; i < zones.Count; i++)
            {
                ExpansionZoneData z = zones[i];
                if (z != null && z.type == ExpansionZoneType.Storage && z.state == ExpansionZoneState.Purchased)
                    total += z.sizeSquareMeters;
            }
            return total;
        }

        /// <summary>Number of zones currently purchased (including initial).</summary>
        public int GetPurchasedZonesCount() => purchasedZoneIds.Count;

        /// <summary>
        /// Returns true if the zone exists, is Available, and the player has enough money.
        /// </summary>
        public bool CanPurchaseZone(string zoneId)
        {
            ExpansionZoneData zone = GetZone(zoneId);
            if (zone == null || zone.state != ExpansionZoneState.Available)
                return false;
            if (StoreDatabase.Instance == null)
                return false;
            return StoreDatabase.CanPurchase(zone.price);
        }

        // ── Zone definitions ──────────────────────────────────────────────────

        private void EnsureDefaultZones()
        {
            if (zones.Count > 0)
                return;

            // ── Helpers ────────────────────────────────────────────────────
            // GridPos(col, row) → pixel anchor in the map canvas.
            // Col / Row numbering: col 0 = left, row 0 = bottom.

            // ── Initial purchased zones ────────────────────────────────────

            // initial_sales  : cols 2-5, rows 1-3  →  (99,51) size (188,140)
            zones.Add(Zone("initial_sales", "Área de Venta Inicial",
                ExpansionZoneType.Sales, 192, 0, ExpansionZoneState.Purchased,
                "Área principal de venta del supermercado.",
                "Área base de ventas activa.", null, new string[0],
                Col(2), Row(1), 4 * S - 4, 3 * S - 4));

            // initial_storage: col 6, rows 1-2  →  (291,51) size (44,92)
            zones.Add(Zone("initial_storage", "Almacén Inicial",
                ExpansionZoneType.Storage, 32, 0, ExpansionZoneState.Purchased,
                "Almacén trasero inicial.",
                "Almacén base activo.", null, new string[0],
                Col(6), Row(1), C, 2 * S - 4));

            // initial_office : col 6, row 0  →  (291,3) size (44,44)
            zones.Add(Zone("initial_office", "Oficina",
                ExpansionZoneType.Office, 32, 0, ExpansionZoneState.Purchased,
                "Oficina del gerente.",
                "Oficina base activa.", null, new string[0],
                Col(6), Row(0), C, C));

            // ── Sales expansions ────────────────────────────────────────────

            // Available immediately (adjacent to initial_sales)
            zones.Add(Zone("sales_w1", "Venta Oeste 1",
                ExpansionZoneType.Sales, 16, SalesPrice, ExpansionZoneState.Available,
                "Expansión de venta hacia el oeste.",
                "+16 m² de venta. Atrae más clientes al área oeste.", null,
                new[] { "initial_sales" },
                Col(1), Row(2), C, C));

            zones.Add(Zone("sales_w2", "Venta Oeste 2",
                ExpansionZoneType.Sales, 16, SalesPrice, ExpansionZoneState.Available,
                "Segunda expansión de venta hacia el oeste.",
                "+16 m² de venta. Mejora el flujo de clientes.", null,
                new[] { "initial_sales" },
                Col(1), Row(3), C, C));

            zones.Add(Zone("sales_n1", "Venta Norte 1",
                ExpansionZoneType.Sales, 16, SalesPrice, ExpansionZoneState.Available,
                "Expansión de venta hacia el norte.",
                "+16 m² de venta. Amplía la sala principal.", null,
                new[] { "initial_sales" },
                Col(2), Row(4), C, C));

            zones.Add(Zone("sales_n2", "Venta Norte 2",
                ExpansionZoneType.Sales, 16, SalesPrice, ExpansionZoneState.Available,
                "Segunda expansión de venta hacia el norte.",
                "+16 m² de venta. Aumenta la capacidad de clientes.", null,
                new[] { "initial_sales" },
                Col(3), Row(4), C, C));

            // Blocked (require purchasing a ring-1 zone first)
            zones.Add(Zone("sales_w3", "Venta Oeste 3",
                ExpansionZoneType.Sales, 16, SalesPrice, ExpansionZoneState.Blocked,
                "Expansión lejana hacia el oeste.",
                "+16 m² de venta en zona premium.",
                "Compra primero Venta Oeste 1.", new[] { "sales_w1" },
                Col(0), Row(2), C, C));

            zones.Add(Zone("sales_w4", "Venta Oeste 4",
                ExpansionZoneType.Sales, 16, SalesPrice, ExpansionZoneState.Blocked,
                "Segunda expansión lejana hacia el oeste.",
                "+16 m² de venta adicional.",
                "Compra primero Venta Oeste 2.", new[] { "sales_w2" },
                Col(0), Row(3), C, C));

            zones.Add(Zone("sales_n3", "Venta Norte 3",
                ExpansionZoneType.Sales, 16, SalesPrice, ExpansionZoneState.Blocked,
                "Tercera expansión de venta norte.",
                "+16 m² de venta. Zona de alto tráfico.",
                "Compra primero Venta Norte 1.", new[] { "sales_n1" },
                Col(4), Row(4), C, C));

            zones.Add(Zone("sales_n4", "Venta Norte 4",
                ExpansionZoneType.Sales, 16, SalesPrice, ExpansionZoneState.Blocked,
                "Cuarta expansión de venta norte.",
                "+16 m² de venta. Área de exhibición.",
                "Compra primero Venta Norte 2.", new[] { "sales_n2" },
                Col(5), Row(4), C, C));

            zones.Add(Zone("sales_s1", "Venta Sur 1",
                ExpansionZoneType.Sales, 16, SalesPrice, ExpansionZoneState.Blocked,
                "Expansión de venta al sur del área oeste.",
                "+16 m² de venta. Zona de entrada.",
                "Compra primero Venta Oeste 1.", new[] { "sales_w1" },
                Col(1), Row(1), C, C));

            zones.Add(Zone("sales_s2", "Venta Sur 2",
                ExpansionZoneType.Sales, 16, SalesPrice, ExpansionZoneState.Blocked,
                "Expansión de venta frontal.",
                "+16 m² de venta. Zona de entrada principal.",
                "Compra primero Venta Sur 1.", new[] { "sales_s1" },
                Col(1), Row(0), C, C));

            // ── Storage expansions ──────────────────────────────────────────

            zones.Add(Zone("storage_n1", "Almacén Norte 1",
                ExpansionZoneType.Storage, 32, StoragePrice, ExpansionZoneState.Available,
                "Expansión del almacén hacia el norte.",
                "+32 m² de almacén. Más capacidad para pedidos.", null,
                new[] { "initial_storage" },
                Col(6), Row(3), C, C));

            zones.Add(Zone("storage_e1", "Almacén Este 1",
                ExpansionZoneType.Storage, 32, StoragePrice, ExpansionZoneState.Available,
                "Expansión del almacén hacia el este.",
                "+32 m² de almacén. Más espacio para surtidores.", null,
                new[] { "initial_storage" },
                Col(7), Row(1), C, C));

            zones.Add(Zone("storage_n2", "Almacén Norte 2",
                ExpansionZoneType.Storage, 32, StoragePrice, ExpansionZoneState.Blocked,
                "Segunda expansión del almacén norte.",
                "+32 m² de almacén adicional.",
                "Compra primero Almacén Norte 1.", new[] { "storage_n1" },
                Col(6), Row(4), C, C));

            zones.Add(Zone("storage_e2", "Almacén Este 2",
                ExpansionZoneType.Storage, 32, StoragePrice, ExpansionZoneState.Blocked,
                "Segunda expansión del almacén este.",
                "+32 m² de almacén adicional.",
                "Compra primero Almacén Este 1.", new[] { "storage_e1" },
                Col(7), Row(2), C, C));

            // ── Mark initial purchased zones ───────────────────────────────
            for (int i = 0; i < zones.Count; i++)
                if (zones[i].state == ExpansionZoneState.Purchased)
                    purchasedZoneIds.Add(zones[i].id);

            Debug.Log(LogPrefix + "Default zones initialized: " + zones.Count + " zones.");
        }

        // ── Adjacency ─────────────────────────────────────────────────────────

        /// <summary>
        /// After any purchase, re-evaluates all Blocked zones.
        /// A zone becomes Available when all its requiredPurchasedZoneIds are purchased.
        /// </summary>
        private void EvaluateBlockedZones()
        {
            for (int i = 0; i < zones.Count; i++)
            {
                ExpansionZoneData zone = zones[i];
                if (zone == null || zone.state != ExpansionZoneState.Blocked)
                    continue;

                if (AreRequirementsMet(zone))
                {
                    zone.state = ExpansionZoneState.Available;
                    zone.blockedReason = string.Empty;
                    Debug.Log(LogPrefix + "Zone unlocked: " + zone.id + " is now Available.");
                }
            }
        }

        private bool AreRequirementsMet(ExpansionZoneData zone)
        {
            if (zone.requiredPurchasedZoneIds == null || zone.requiredPurchasedZoneIds.Count == 0)
                return true;

            for (int i = 0; i < zone.requiredPurchasedZoneIds.Count; i++)
                if (!purchasedZoneIds.Contains(zone.requiredPurchasedZoneIds[i]))
                    return false;

            return true;
        }

        // ── Save / Load ───────────────────────────────────────────────────────

        private void OnSave()
        {
            try
            {
                JSONNode data = new JSONObject();
                JSONArray purchased = new JSONArray();
                foreach (string zoneId in purchasedZoneIds)
                    purchased.Add(zoneId);
                data["purchased"] = purchased;
                data["version"] = 2;

                string path = Application.persistentDataPath + "/" + SaveFileName + SaveGameSystem.fileExt;
                File.WriteAllBytes(path, Encoding.UTF8.GetBytes(data.ToString()));
                Debug.Log(LogPrefix + "Expansion data saved (" + purchasedZoneIds.Count + " zones).");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(LogPrefix + "Save failed: " + ex.Message);
            }
        }

        private void OnLoad()
        {
            try
            {
                ResetToDefaults();
                string path = Application.persistentDataPath + "/" + SaveFileName + SaveGameSystem.fileExt;

                if (!File.Exists(path))
                {
                    Debug.Log(LogPrefix + "No expansion save file found. Using defaults.");
                    onZonesReset?.Invoke();
                    return;
                }

                string json = Encoding.UTF8.GetString(File.ReadAllBytes(path));
                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.LogWarning(LogPrefix + "Expansion save file is empty.");
                    onZonesReset?.Invoke();
                    return;
                }

                JSONNode data = JSON.Parse(json);
                if (data == null || data.IsNull)
                {
                    Debug.LogWarning(LogPrefix + "Expansion save file could not be parsed.");
                    onZonesReset?.Invoke();
                    return;
                }

                JSONArray purchased = data["purchased"].AsArray;
                if (purchased == null)
                {
                    Debug.LogWarning(LogPrefix + "Expansion save file has no purchased zone array.");
                    onZonesReset?.Invoke();
                    return;
                }

                int loadedCount = 0;
                for (int i = 0; i < purchased.Count; i++)
                {
                    string zoneId = purchased[i].Value;
                    ExpansionZoneData zone = GetZone(zoneId);
                    if (zone == null)
                    {
                        Debug.LogWarning(LogPrefix + "Ignoring unknown zone id from save: " + zoneId);
                        continue;
                    }

                    zone.state = ExpansionZoneState.Purchased;
                    zone.blockedReason = string.Empty;
                    purchasedZoneIds.Add(zone.id);
                    loadedCount++;
                }

                // Re-evaluate adjacency after loading all purchased zones.
                EvaluateBlockedZones();

                Debug.Log(LogPrefix + "Loaded " + loadedCount + " purchased zones from save.");
                onZonesReset?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(LogPrefix + "Load failed: " + ex.Message + ". Restoring defaults.");
                ResetToDefaults();
                onZonesReset?.Invoke();
            }
        }

        private void ResetToDefaults()
        {
            zones.Clear();
            purchasedZoneIds.Clear();
            EnsureDefaultZones();
        }

        void OnDestroy()
        {
            SaveGameSystem.dataSaveEvent -= OnSave;
            SaveGameSystem.dataLoadEvent -= OnLoad;
            if (Instance == this)
                Instance = null;
        }

        // ── Static helpers for zone construction ──────────────────────────────

        private static float Col(int c) => O + c * S;
        private static float Row(int r) => O + r * S;

        private static ExpansionZoneData Zone(
            string id, string displayName,
            ExpansionZoneType type, int sizeM2, long price,
            ExpansionZoneState state,
            string description, string benefit,
            string blockedReason, string[] reqs,
            float x, float y, float w, float h)
        {
            return new ExpansionZoneData
            {
                id              = id,
                displayName     = displayName,
                type            = type,
                sizeSquareMeters = sizeM2,
                price           = price,
                state           = state,
                description     = description,
                benefit         = benefit,
                blockedReason   = blockedReason,
                requiredPurchasedZoneIds = reqs != null
                    ? new System.Collections.Generic.List<string>(reqs)
                    : new System.Collections.Generic.List<string>(),
                mapPosition = new Vector2(x, y),
                mapSize     = new Vector2(w, h)
            };
        }
    }
}
