using System;
using System.Collections.Generic;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    public enum ExpansionZoneType
    {
        Sales = 0,
        Storage = 1,
        Office = 2
    }

    public enum ExpansionZoneState
    {
        Purchased = 0,
        Available = 1,
        Blocked = 2
    }

    [Serializable]
    public class ExpansionZoneData
    {
        public string id;
        public string displayName;
        public ExpansionZoneType type;
        public int sizeSquareMeters;
        public long price;
        public ExpansionZoneState state;
        public string description;
        /// <summary>Short benefit text shown in the detail panel when the zone is selected.</summary>
        public string benefit;
        public string blockedReason;
        /// <summary>All zone IDs that must be purchased before this zone becomes available.</summary>
        public List<string> requiredPurchasedZoneIds = new List<string>();
        public Vector2 mapPosition;
        public Vector2 mapSize;
    }
}
