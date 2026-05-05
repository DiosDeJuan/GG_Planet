using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Applies entrepreneur tree security progression to runtime gameplay values and optional visuals.
    /// This adapter exposes security level and arrest probability for connection with a future real shoplifter system.
    /// </summary>
    public class EntrepreneurTreeSecurityAdapter : MonoBehaviour
    {
        // Este adapter expone nivel de seguridad y probabilidad de arresto para conectarse con el futuro sistema real de ladrones.
        private const string LogPrefix = "[EntrepreneurTree] ";
        public static EntrepreneurTreeSecurityAdapter Instance { get; private set; }

        [Header("Optional visuals")]
        public GameObject securityLevel1Visual;
        public GameObject securityLevel2Visual;
        public GameObject securityLevel3Visual;

        private int securityLevel;
        private float arrestChance;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            EntrepreneurTreeManager.onSecurityNodeUnlocked += OnSecurityNodeUnlocked;
            SaveGameSystem.dataLoadEvent += OnDataLoaded;
        }


        void Start()
        {
            RecalculateFromTree();
        }


        public int GetSecurityLevel()
        {
            return securityLevel;
        }


        public float GetArrestChance()
        {
            return arrestChance;
        }


        public bool IsSecurityUnlocked(int level)
        {
            switch (level)
            {
                case 1:
                    return securityLevel >= 1;
                case 2:
                    return securityLevel >= 2;
                case 3:
                    return securityLevel >= 3;
                default:
                    return false;
            }
        }

        public bool TryAutomaticArrest()
        {
            if (arrestChance <= 0f)
                return false;

            return UnityEngine.Random.value <= arrestChance;
        }


        private void OnSecurityNodeUnlocked(NodeData node)
        {
            RecalculateFromTree();
            Debug.Log(LogPrefix + "Security gameplay unlock applied: " + node?.id);
        }


        private void OnDataLoaded()
        {
            RecalculateFromTree();
            Debug.Log(LogPrefix + "Security gameplay state reapplied after load.");
        }


        private void RecalculateFromTree()
        {
            int coverage = EntrepreneurTreeManager.GetSecurityCoveragePercent();
            if (coverage >= 99)
            {
                securityLevel = 3;
                arrestChance = 0.99f;
            }
            else if (coverage >= 66)
            {
                securityLevel = 2;
                arrestChance = 0.66f;
            }
            else if (coverage >= 33)
            {
                securityLevel = 1;
                arrestChance = 0.33f;
            }
            else
            {
                securityLevel = 0;
                arrestChance = 0f;
            }

            ApplyVisuals();
        }


        private void ApplyVisuals()
        {
            if (securityLevel1Visual != null) securityLevel1Visual.SetActive(securityLevel >= 1);
            if (securityLevel2Visual != null) securityLevel2Visual.SetActive(securityLevel >= 2);
            if (securityLevel3Visual != null) securityLevel3Visual.SetActive(securityLevel >= 3);
        }


        void OnDestroy()
        {
            EntrepreneurTreeManager.onSecurityNodeUnlocked -= OnSecurityNodeUnlocked;
            SaveGameSystem.dataLoadEvent -= OnDataLoaded;

            if (Instance == this)
                Instance = null;
        }
    }
}
