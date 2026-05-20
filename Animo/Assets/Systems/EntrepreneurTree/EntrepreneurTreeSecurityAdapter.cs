using System;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Applies entrepreneur tree security progression to runtime gameplay values and optional visuals.
    /// Exposes security level, arrest probability, and events for the shoplifter system.
    /// </summary>
    public class EntrepreneurTreeSecurityAdapter : MonoBehaviour
    {
        private const string LogPrefix = "[Security] ";
        public static EntrepreneurTreeSecurityAdapter Instance { get; private set; }

        /// <summary>Fired whenever the security level changes (0–3).</summary>
        public static event Action<int> onSecurityLevelChanged;

        /// <summary>Fired whenever the auto-arrest chance changes (0–0.99).</summary>
        public static event Action<float> onAutoArrestChanceChanged;

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

        public int GetCurrentSecurityLevel()
        {
            return securityLevel;
        }


        public float GetArrestChance()
        {
            return arrestChance;
        }


        /// <summary>Returns the auto-arrest chance (0–1). Alias for GetArrestChance().</summary>
        public float GetAutoArrestChance()
        {
            return arrestChance;
        }

        public float GetAutomaticArrestChance()
        {
            return arrestChance;
        }

        public string GetSecuritySystemName()
        {
            switch (securityLevel)
            {
                case 1:
                    return "Camaras";
                case 2:
                    return "Guardias";
                case 3:
                    return "Alarma";
                default:
                    return "Sin seguridad";
            }
        }


        /// <summary>Refreshes security level from the current tree state. Safe to call at any time.</summary>
        public void RefreshFromTree()
        {
            RecalculateFromTree();
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

            float roll = UnityEngine.Random.value;
            bool success = roll <= arrestChance;
            Debug.Log(LogPrefix + "Automatic arrest roll. Level=" + securityLevel
                + ", Chance=" + arrestChance.ToString("0.00")
                + ", Roll=" + roll.ToString("0.00")
                + ", Result=" + (success ? "success" : "fail"));
            return success;
        }


        /// <summary>Tries automatic arrest using current arrest chance. Agent parameter accepted for future use.</summary>
        public bool TryAutoArrest(ShoplifterAgent agent)
        {
            return TryAutomaticArrest();
        }


        private void OnSecurityNodeUnlocked(NodeData node)
        {
            RecalculateFromTree();
            Debug.Log(LogPrefix + "Security node unlocked: " + node?.id + " → level " + securityLevel + " / " + Mathf.RoundToInt(arrestChance * 100) + "%");
        }


        private void OnDataLoaded()
        {
            RecalculateFromTree();
            Debug.Log(LogPrefix + "Security level restored after load: " + securityLevel + " / " + Mathf.RoundToInt(arrestChance * 100) + "%");
        }


        private void RecalculateFromTree()
        {
            int prevLevel = securityLevel;
            float prevChance = arrestChance;

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

            Debug.Log(LogPrefix + "Security level refreshed from tree: " + securityLevel + " / " + Mathf.RoundToInt(arrestChance * 100) + "%");

            bool levelChanged = securityLevel != prevLevel;
            bool chanceChanged = !Mathf.Approximately(arrestChance, prevChance);

            if (levelChanged)
            {
                onSecurityLevelChanged?.Invoke(securityLevel);
                if (securityLevel > 0 && UIGame.Instance != null)
                    UIGame.AddNotification(
                        "Seguridad nivel " + securityLevel + " activa: " + Mathf.RoundToInt(arrestChance * 100) + "% de arresto automático.",
                        otherColor: new Color(0.20f, 0.72f, 0.36f));
            }

            if (chanceChanged)
                onAutoArrestChanceChanged?.Invoke(arrestChance);

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
                securityLevel1Visual = CreatePlaceholder("SecurityCameraPlaceholder", PrimitiveType.Cylinder, new Vector3(-1.2f, 2f, 0f), new Vector3(0.2f, 0.25f, 0.2f), new Color(0.95f, 0.45f, 0.1f));
            if (securityLevel2Visual == null)
                securityLevel2Visual = CreateGuardPairPlaceholder();
            if (securityLevel3Visual == null)
                securityLevel3Visual = CreatePlaceholder("SecurityGatePlaceholder", PrimitiveType.Cube, new Vector3(1.2f, 2f, 0f), new Vector3(1.2f, 1.8f, 0.2f), new Color(1f, 0.2f, 0.2f));
        }


        private GameObject CreateGuardPairPlaceholder()
        {
            GameObject root = new GameObject("SecurityGuardsPlaceholder");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0f, 0f, 0f);

            CreatePlaceholder("SecurityGuard_A", PrimitiveType.Capsule, new Vector3(-0.35f, 1f, 0f), new Vector3(0.35f, 1f, 0.35f), new Color(0.85f, 0.25f, 0.15f), root.transform);
            CreatePlaceholder("SecurityGuard_B", PrimitiveType.Capsule, new Vector3(0.35f, 1f, 0f), new Vector3(0.35f, 1f, 0.35f), new Color(0.85f, 0.25f, 0.15f), root.transform);
            root.SetActive(false);
            return root;
        }


        private GameObject CreatePlaceholder(string name, PrimitiveType primitive, Vector3 localOffset, Vector3 localScale, Color color, Transform parentOverride = null)
        {
            GameObject go = GameObject.CreatePrimitive(primitive);
            go.name = name;
            go.transform.SetParent(parentOverride != null ? parentOverride : transform, false);
            go.transform.localPosition = localOffset;
            go.transform.localScale = localScale;

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
