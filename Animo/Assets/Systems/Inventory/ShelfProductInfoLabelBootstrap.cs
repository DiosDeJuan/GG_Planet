using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Attaches a <see cref="ShelfProductInfoLabel"/> to every <see cref="PlacementObject"/>
    /// in the scene at startup.
    ///
    /// This component is added automatically by <see cref="EntrepreneurTreeUIBootstrap"/> via
    /// <see cref="EntrepreneurTreeUIBootstrap.EnsureTreeSystems"/>.
    /// </summary>
    public class ShelfProductInfoLabelBootstrap : MonoBehaviour
    {
        private const string LogPrefix = "[ShelfLabelBoot] ";

        public static ShelfProductInfoLabelBootstrap Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            AttachLabels();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// Finds all PlacementObjects in the scene and adds ShelfProductInfoLabel to each
        /// that doesn't already have one.
        /// </summary>
        public void AttachLabels()
        {
            PlacementObject[] placements = FindPlacements();
            int attached = 0;
            for (int i = 0; i < placements.Length; i++)
            {
                PlacementObject p = placements[i];
                if (p == null)
                    continue;

                if (p.GetComponent<ShelfProductInfoLabel>() == null)
                {
                    p.gameObject.AddComponent<ShelfProductInfoLabel>();
                    attached++;
                }
            }

            if (attached > 0)
                Debug.Log(LogPrefix + "Attached ShelfProductInfoLabel to " + attached + " placement(s).");
        }

        private static PlacementObject[] FindPlacements()
        {
#if UNITY_2022_2_OR_NEWER
            return Object.FindObjectsByType<PlacementObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            return Object.FindObjectsOfType<PlacementObject>(true);
#endif
        }
    }
}
