// GameEndSystem — ShopMaster Phase 6
// Handles Monopoly Final and Bankruptcy Final game endings.
// Attach to the Systems GameObject in the Game scene alongside EntrepreneurTreeManager.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Monitors end-of-day conditions for Monopoly Final and Bankruptcy Final.
    ///
    /// Monopoly Final:
    ///   Triggers when all expansion zones are purchased AND all Entrepreneur Tree nodes are unlocked.
    ///   Shows an overlay with "Has convertido tu supermercado en un imperio."
    ///   Player can continue playing or end the session.
    ///
    /// Bankruptcy Final:
    ///   Triggers at end of day when money ≤ 0.
    ///   Shows a Game Over overlay and returns to main menu.
    ///
    /// SCENE SETUP:
    ///   Add this component to the Systems GameObject in the Game scene.
    ///   No additional Inspector assignments required.
    /// </summary>
    public class GameEndSystem : MonoBehaviour
    {
        private const string LogPrefix = "[GameEnd] ";

        // Scene indices (must match ProjectSettings/EditorBuildSettings.asset: 0=Intro, 1=Game, 2=Stats)
        private const int IntroSceneIndex = 0;

        public static GameEndSystem Instance { get; private set; }

        // ── State ─────────────────────────────────────────────────────────────────
        private bool monopolyTriggered;
        private bool bankruptcyTriggered;

        // ── UI references (created at runtime) ──────────────────────────────────
        private GameObject overlayRoot;


        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DayCycleSystem.onDayFinished += OnDayFinished;
            EntrepreneurTreeManager.onNodeUnlocked += OnNodeUnlocked;
            SupermarketExpansionSystem.onZonePurchased += OnZonePurchased;
        }


        // ── Event handlers ────────────────────────────────────────────────────────

        private void OnDayFinished()
        {
            if (!bankruptcyTriggered)
                CheckBankruptcy();

            if (!monopolyTriggered)
                CheckMonopoly();
        }

        private void OnNodeUnlocked(NodeData node)
        {
            if (!monopolyTriggered)
                CheckMonopoly();
        }

        private void OnZonePurchased(ExpansionZoneData zone)
        {
            if (!monopolyTriggered)
                CheckMonopoly();
        }


        // ── Condition checks ──────────────────────────────────────────────────────

        private void CheckMonopoly()
        {
            if (!AllExpansionsPurchased()) return;
            if (!AllTreeNodesUnlocked())   return;

            monopolyTriggered = true;
            Debug.Log(LogPrefix + "Monopoly condition met. Showing Monopoly Final overlay.");
            ShowMonopolyOverlay();
        }

        private void CheckBankruptcy()
        {
            if (StoreDatabase.Instance == null) return;
            long money = StoreDatabase.Instance.currentMoney;
            if (money > 0) return;

            bankruptcyTriggered = true;
            Debug.Log(LogPrefix + "Bankruptcy condition met (money=" + money + "). Showing Game Over overlay.");
            ShowBankruptcyOverlay();
        }

        private static bool AllExpansionsPurchased()
        {
            if (SupermarketExpansionSystem.Instance == null)
                return false;

            int purchased = SupermarketExpansionSystem.Instance.GetPurchasedZonesCount();
            int total     = SupermarketExpansionSystem.Instance.Zones.Count;
            return total > 0 && purchased >= total;
        }

        private static bool AllTreeNodesUnlocked()
        {
            if (EntrepreneurTreeManager.Instance == null ||
                EntrepreneurTreeManager.Instance.treeData == null)
                return false;

            var nodes = EntrepreneurTreeManager.Instance.treeData.nodes;
            if (nodes == null || nodes.Count == 0)
                return false;

            for (int i = 0; i < nodes.Count; i++)
            {
                NodeData n = nodes[i];
                if (n != null && !n.isUnlocked)
                    return false;
            }

            return true;
        }


        // ── Overlay UI creation ───────────────────────────────────────────────────

        private void ShowMonopolyOverlay()
        {
            // Complete the secret achievement if not already done.
            AchievementSystem.Complete(AchievementId.UnlockAllTree);
            AchievementSystem.Complete(AchievementId.HuevoDorado);

            Canvas canvas = FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogWarning(LogPrefix + "No canvas found for Monopoly overlay.");
                return;
            }

            overlayRoot = BuildOverlay(canvas.transform, "MonopolyOverlay");
            Transform card = BuildCard(overlayRoot.transform, new Color(0.07f, 0.10f, 0.06f, 0.98f));

            BuildTitle(card, "🏆  ¡MONOPOLIO!", new Color(1f, 0.85f, 0.1f));
            BuildBody(card,
                "Has convertido tu supermercado en un imperio.\n\n" +
                "Dominas el mercado local. Nadie puede competir contigo.",
                new Color(0.88f, 0.92f, 0.88f));

            BuildButton(card, "Seguir jugando", new Color(0.15f, 0.45f, 0.22f), () =>
            {
                if (overlayRoot != null) Destroy(overlayRoot);
            });

            BuildButton(card, "Terminar partida", new Color(0.40f, 0.15f, 0.15f), () =>
            {
                SaveGameSystem.Save();
                SceneManager.LoadScene(IntroSceneIndex);
            });

            Debug.Log(LogPrefix + "Monopoly Final overlay shown.");
            if (UIGame.Instance != null)
                UIGame.AddNotification("¡Monopolio alcanzado! Tu supermercado domina la ciudad.", otherColor: new Color(1f, 0.85f, 0.1f));
        }

        private void ShowBankruptcyOverlay()
        {
            Canvas canvas = FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogWarning(LogPrefix + "No canvas found for Bankruptcy overlay.");
                return;
            }

            overlayRoot = BuildOverlay(canvas.transform, "BankruptcyOverlay");
            Transform card = BuildCard(overlayRoot.transform, new Color(0.10f, 0.04f, 0.04f, 0.98f));

            BuildTitle(card, "💸  BANCARROTA", new Color(0.95f, 0.22f, 0.22f));
            BuildBody(card,
                "Tu negocio ha quebrado.\n\n" +
                "Sin fondos para operar, el supermercado cierra sus puertas.",
                new Color(0.92f, 0.88f, 0.84f));

            BuildButton(card, "Volver al menú", new Color(0.30f, 0.12f, 0.12f), () =>
            {
                SceneManager.LoadScene(IntroSceneIndex);
            });

            Debug.Log(LogPrefix + "Bankruptcy Game Over overlay shown.");
            if (UIGame.Instance != null)
                UIGame.AddNotification("¡Bancarrota! El negocio no puede continuar.", otherColor: new Color(0.95f, 0.22f, 0.22f));
        }


        // ── Admin helpers ─────────────────────────────────────────────────────────

        /// <summary>Admin helper: immediately shows the Monopoly Final overlay for testing.</summary>
        public static void AdminTestMonopoly()
        {
            if (Instance == null)
            {
                Debug.LogWarning(LogPrefix + "GameEndSystem instance not found for admin test.");
                return;
            }

            Debug.Log("[AdminMode] Triggered monopoly test.");
            Instance.monopolyTriggered = false; // allow re-trigger
            Instance.ShowMonopolyOverlay();
            Instance.monopolyTriggered = true;
        }

        /// <summary>Admin helper: immediately shows the Bankruptcy Game Over overlay for testing.</summary>
        public static void AdminTestBankruptcy()
        {
            if (Instance == null)
            {
                Debug.LogWarning(LogPrefix + "GameEndSystem instance not found for admin test.");
                return;
            }

            Debug.Log("[AdminMode] Triggered bankruptcy test.");
            Instance.bankruptcyTriggered = false; // allow re-trigger
            Instance.ShowBankruptcyOverlay();
            Instance.bankruptcyTriggered = true;
        }


        // ── UI helpers ────────────────────────────────────────────────────────────

        private static Canvas FindMainCanvas()
        {
#if UNITY_2022_2_OR_NEWER
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
#else
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
#endif
            for (int i = 0; i < canvases.Length; i++)
            {
                if (canvases[i] != null && canvases[i].renderMode == RenderMode.ScreenSpaceOverlay)
                    return canvases[i];
            }

            return null;
        }

        private static GameObject BuildOverlay(Transform canvasRoot, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(canvasRoot, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);
            go.transform.SetAsLastSibling();
            return go;
        }

        private static Transform BuildCard(Transform overlayRoot, Color bgColor)
        {
            GameObject card = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            card.transform.SetParent(overlayRoot, false);
            RectTransform rt = card.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(500f, 340f);
            rt.anchoredPosition = Vector2.zero;
            card.GetComponent<Image>().color = bgColor;

            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(36, 36, 28, 28);
            vlg.spacing = 16f;
            vlg.childControlWidth   = true;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight  = false;
            vlg.childForceExpandHeight = false;

            return card.transform;
        }

        private static void BuildTitle(Transform card, string text, Color color)
        {
            GameObject go = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(card, false);
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 52f;
            TextMeshProUGUI lbl = go.GetComponent<TextMeshProUGUI>();
            lbl.text      = text;
            lbl.fontSize  = 28f;
            lbl.color     = color;
            lbl.fontStyle = FontStyles.Bold;
            lbl.alignment = TextAlignmentOptions.Center;
        }

        private static void BuildBody(Transform card, string text, Color color)
        {
            GameObject go = new GameObject("Body", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(card, false);
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 100f;
            TextMeshProUGUI lbl = go.GetComponent<TextMeshProUGUI>();
            lbl.text              = text;
            lbl.fontSize          = 17f;
            lbl.color             = color;
            lbl.alignment         = TextAlignmentOptions.Center;
            lbl.textWrappingMode = TextWrappingModes.Normal;
        }

        private static void BuildButton(Transform card, string label, Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(card, false);
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 48f;
            Image img = go.GetComponent<Image>();
            img.color = bgColor;
            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            GameObject txtGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            txtGO.transform.SetParent(go.transform, false);
            RectTransform txtRT = txtGO.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = Vector2.zero;
            txtRT.offsetMax = Vector2.zero;
            TextMeshProUGUI lbl = txtGO.GetComponent<TextMeshProUGUI>();
            lbl.text      = label;
            lbl.fontSize  = 18f;
            lbl.color     = Color.white;
            lbl.alignment = TextAlignmentOptions.Center;
        }


        void OnDestroy()
        {
            DayCycleSystem.onDayFinished -= OnDayFinished;
            EntrepreneurTreeManager.onNodeUnlocked -= OnNodeUnlocked;
            SupermarketExpansionSystem.onZonePurchased -= OnZonePurchased;

            if (Instance == this)
                Instance = null;
        }
    }
}
