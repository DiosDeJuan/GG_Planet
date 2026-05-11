using System;
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
        public string blockedReason;
        public Vector2 mapPosition;
        public Vector2 mapSize;
    }
}
