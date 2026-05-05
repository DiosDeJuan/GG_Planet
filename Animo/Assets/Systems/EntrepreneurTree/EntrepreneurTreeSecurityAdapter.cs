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
        [Tooltip("When visual references are missing, create simple runtime placeholders as safe fallback.")]
        public bool createPlaceholdersWhenMissing = true;

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
            if (securityLevel >= 3)
                AchievementSystem.RegisterSecurityCompleted();
        }


        private void ApplyVisuals()
        {
            EnsureVisualPlaceholders();
            if (securityLevel1Visual != null) securityLevel1Visual.SetActive(securityLevel >= 1);
            if (securityLevel2Visual != null) securityLevel2Visual.SetActive(securityLevel >= 2);
            if (securityLevel3Visual != null) securityLevel3Visual.SetActive(securityLevel >= 3);
        }


        private void EnsureVisualPlaceholders()
        {
            if (!createPlaceholdersWhenMissing)
                return;

            if (securityLevel1Visual == null)
                securityLevel1Visual = CreatePlaceholder("SecurityCameraPlaceholder", new Vector3(-1.2f, 2f, 0f), new Color(0.95f, 0.45f, 0.1f));
            if (securityLevel2Visual == null)
                securityLevel2Visual = CreatePlaceholder("SecurityGuardPlaceholder", new Vector3(0f, 2f, 0f), new Color(0.85f, 0.25f, 0.15f));
            if (securityLevel3Visual == null)
                securityLevel3Visual = CreatePlaceholder("SecurityGatePlaceholder", new Vector3(1.2f, 2f, 0f), new Color(1f, 0.2f, 0.2f));
        }


        private GameObject CreatePlaceholder(string name, Vector3 localOffset, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localOffset;
            go.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);

            Collider col = go.GetComponent<Collider>();
            if (col != null)
                Destroy(col);

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
                renderer.material.color = color;

            go.SetActive(false);
            return go;
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
