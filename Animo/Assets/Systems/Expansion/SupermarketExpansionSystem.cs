using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleJSON;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    public class SupermarketExpansionSystem : MonoBehaviour
    {
        private const string LogPrefix = "[ExpansionApp] ";
        private const string SaveFileName = "expansionApp";
        private readonly List<ExpansionZoneData> zones = new List<ExpansionZoneData>();
        private readonly HashSet<string> purchasedZoneIds = new HashSet<string>();

        public static SupermarketExpansionSystem Instance { get; private set; }
        public static event Action onZonesChanged;
        public static event Action<ExpansionZoneData> onZonePurchased;

        public IReadOnlyList<ExpansionZoneData> Zones => zones;

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
                reason = "Zona ya comprada.";
                return false;
            }

            if (zone.state == ExpansionZoneState.Blocked)
            {
                reason = string.IsNullOrEmpty(zone.blockedReason) ? "Zona bloqueada." : zone.blockedReason;
                return false;
            }

            if (!StoreDatabase.CanPurchase(zone.price))
            {
                long money = StoreDatabase.Instance != null ? StoreDatabase.Instance.currentMoney : 0;
                missingFunds = Math.Max(0L, zone.price - money);
                reason = "Fondos insuficientes.";
                Debug.Log(LogPrefix + "Purchase failed: insufficient funds. Missing " + missingFunds + ".");
                return false;
            }

            StoreDatabase.AddRemoveMoney(-zone.price);
            zone.state = ExpansionZoneState.Purchased;
            zone.blockedReason = string.Empty;
            purchasedZoneIds.Add(zone.id);
            UnlockNextBlockedZone(zone.type);
            onZonePurchased?.Invoke(zone);
            onZonesChanged?.Invoke();
            Debug.Log(LogPrefix + "Purchased zone: " + zone.id + ".");
            return true;
        }

        public ExpansionZoneData GetZone(string zoneId)
        {
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i] != null && zones[i].id == zoneId)
                    return zones[i];
            }

            return null;
        }

        private void EnsureDefaultZones()
        {
            if (zones.Count > 0)
                return;

            zones.Add(new ExpansionZoneData { id = "sales_start", displayName = "Venta Inicial", type = ExpansionZoneType.Sales, sizeSquareMeters = 192, price = 0, state = ExpansionZoneState.Purchased, description = "Área inicial de venta.", mapPosition = new Vector2(20f, 160f), mapSize = new Vector2(180f, 130f) });
            zones.Add(new ExpansionZoneData { id = "storage_start", displayName = "Almacenamiento Inicial", type = ExpansionZoneType.Storage, sizeSquareMeters = 32, price = 0, state = ExpansionZoneState.Purchased, description = "Almacén inicial.", mapPosition = new Vector2(220f, 160f), mapSize = new Vector2(80f, 80f) });
            zones.Add(new ExpansionZoneData { id = "office", displayName = "Oficina", type = ExpansionZoneType.Office, sizeSquareMeters = 32, price = 0, state = ExpansionZoneState.Purchased, description = "Oficina del jugador.", mapPosition = new Vector2(220f, 70f), mapSize = new Vector2(80f, 80f) });

            zones.Add(new ExpansionZoneData { id = "sales_1", displayName = "Venta A", type = ExpansionZoneType.Sales, sizeSquareMeters = 16, price = 175000, state = ExpansionZoneState.Available, description = "Expansión de venta de 16 m².", mapPosition = new Vector2(20f, 20f), mapSize = new Vector2(80f, 60f) });
            zones.Add(new ExpansionZoneData { id = "sales_2", displayName = "Venta B", type = ExpansionZoneType.Sales, sizeSquareMeters = 16, price = 175000, state = ExpansionZoneState.Available, description = "Expansión de venta de 16 m².", mapPosition = new Vector2(110f, 20f), mapSize = new Vector2(80f, 60f) });
            zones.Add(new ExpansionZoneData { id = "sales_3", displayName = "Venta C", type = ExpansionZoneType.Sales, sizeSquareMeters = 16, price = 175000, state = ExpansionZoneState.Blocked, blockedReason = "Compra primero otra zona de venta.", description = "Expansión de venta de 16 m².", mapPosition = new Vector2(200f, 20f), mapSize = new Vector2(80f, 60f) });
            zones.Add(new ExpansionZoneData { id = "storage_1", displayName = "Almacén A", type = ExpansionZoneType.Storage, sizeSquareMeters = 32, price = 250000, state = ExpansionZoneState.Available, description = "Expansión de almacén de 32 m².", mapPosition = new Vector2(310f, 160f), mapSize = new Vector2(90f, 80f) });
            zones.Add(new ExpansionZoneData { id = "storage_2", displayName = "Almacén B", type = ExpansionZoneType.Storage, sizeSquareMeters = 32, price = 250000, state = ExpansionZoneState.Blocked, blockedReason = "Compra primero Almacén A.", description = "Expansión de almacén de 32 m².", mapPosition = new Vector2(310f, 70f), mapSize = new Vector2(90f, 80f) });

            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i].state == ExpansionZoneState.Purchased)
                    purchasedZoneIds.Add(zones[i].id);
            }
        }

        private void UnlockNextBlockedZone(ExpansionZoneType type)
        {
            for (int i = 0; i < zones.Count; i++)
            {
                ExpansionZoneData zone = zones[i];
                if (zone.type != type || zone.state != ExpansionZoneState.Blocked)
                    continue;

                zone.state = ExpansionZoneState.Available;
                zone.blockedReason = string.Empty;
                return;
            }
        }

        private void OnSave()
        {
            JSONNode data = new JSONObject();
            JSONArray purchased = new JSONArray();
            foreach (string zoneId in purchasedZoneIds)
                purchased.Add(zoneId);
            data["purchased"] = purchased;

            string path = Application.persistentDataPath + "/" + SaveFileName + SaveGameSystem.fileExt;
            File.WriteAllBytes(path, Encoding.ASCII.GetBytes(data.ToString()));
        }

        private void OnLoad()
        {
            ResetToDefaults();
            string path = Application.persistentDataPath + "/" + SaveFileName + SaveGameSystem.fileExt;
            if (!File.Exists(path))
            {
                onZonesChanged?.Invoke();
                return;
            }

            string json = Encoding.ASCII.GetString(File.ReadAllBytes(path));
            JSONNode data = JSON.Parse(json);
            JSONArray purchased = data["purchased"].AsArray;
            for (int i = 0; i < purchased.Count; i++)
            {
                string zoneId = purchased[i].Value;
                ExpansionZoneData zone = GetZone(zoneId);
                if (zone == null)
                    continue;

                zone.state = ExpansionZoneState.Purchased;
                zone.blockedReason = string.Empty;
                purchasedZoneIds.Add(zone.id);
            }

            onZonesChanged?.Invoke();
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
    }
}
