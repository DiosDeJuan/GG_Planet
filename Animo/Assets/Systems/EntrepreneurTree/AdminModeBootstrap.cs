// Admin Mode — AdminModeBootstrap
// Injects an ADMIN button into the Intro scene at runtime (no scene modification required).
// When pressed, shows a configuration overlay; on confirm the admin config is stored and the
// game loads as a fresh New Game with those settings applied.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Runtime bootstrap that adds an ADMIN button to the main menu in the Intro scene.
    /// All UI is created procedurally so no scene files need to be modified.
    /// </summary>
    public static class AdminModeBootstrap
    {
        private const string LogPrefix = "[AdminMode] ";
        private static bool sceneHandlerRegistered;

        // ── Scene indices (must match ProjectSettings/EditorBuildSettings.asset) ─
        // 0 = Intro, 1 = Game, 2 = Stats
        private const int IntroSceneIndex = 0;
        private const int GameSceneIndex  = 1;

        // ── Config card dimensions ────────────────────────────────────────────
        private const float CardWidth  = 480f;
        private const float CardHeight = 820f;

        // ── Runtime-inject into every scene load ──────────────────────────────
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!sceneHandlerRegistered)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                sceneHandlerRegistered = true;
            }
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!IsIntroScene(scene))
                return;

            // Look for an existing Canvas in the scene.
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            Canvas targetCanvas = null;
            for (int i = 0; i < canvases.Length; i++)
            {
                if (canvases[i] != null && canvases[i].renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    targetCanvas = canvases[i];
                    break;
                }
            }

            if (targetCanvas == null)
            {
                Debug.LogWarning(LogPrefix + "No ScreenSpaceOverlay Canvas found in Intro scene. ADMIN button not injected.");
                return;
            }

            // Don't inject twice.
            if (targetCanvas.transform.Find("AdminModeOverlay") != null)
                return;

            InjectAdminUI(targetCanvas.transform);
            Debug.Log(LogPrefix + "ADMIN button injected into Intro scene.");
        }

        // ── UI Construction ────────────────────────────────────────────────────

        private static void InjectAdminUI(Transform canvasRoot)
        {
            // ── ADMIN button (bottom-right, non-intrusive) ────────────────────
            GameObject adminBtnGO = CreateUIObject("AdminButton", canvasRoot,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f));
            RectTransform btnRT = adminBtnGO.GetComponent<RectTransform>();
            btnRT.sizeDelta    = new Vector2(140f, 46f);
            btnRT.anchoredPosition = new Vector2(-24f, 24f);
            Image btnImg = adminBtnGO.AddComponent<Image>();
            btnImg.color = new Color(0.72f, 0.18f, 0.18f, 0.92f);
            Button adminBtn = adminBtnGO.AddComponent<Button>();
            adminBtn.targetGraphic = btnImg;
            CreateLabel("Text", adminBtnGO.transform, "ADMIN", 18);

            // ── Admin overlay panel (hidden by default) ───────────────────────
            GameObject overlay = CreateUIObject("AdminModeOverlay", canvasRoot,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            overlay.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            overlay.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            Image overlayBg = overlay.AddComponent<Image>();
            overlayBg.color = new Color(0f, 0f, 0f, 0.72f);
            overlay.SetActive(false);

            // ── Config card ───────────────────────────────────────────────────
            GameObject card = CreateUIObject("Card", overlay.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            RectTransform cardRT = card.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(CardWidth, CardHeight);
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.07f, 0.09f, 0.12f, 0.98f);

            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(28, 28, 24, 24);
            vlg.spacing = 14f;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandHeight = false;

            // Title
            CreateLabel("Title", card.transform, "⚙  MODO ADMIN  ⚙", 26f, new Color(1f, 0.82f, 0.2f), 40f, TextAlignmentOptions.Center);

            // ── Input rows ────────────────────────────────────────────────────
            TMP_InputField moneyInput    = CreateInputRow(card.transform, "Dinero inicial ($):",  "5000");
            TMP_InputField pointsInput   = CreateInputRow(card.transform, "Puntos Árbol:",        "0");

            // ── Toggle rows ───────────────────────────────────────────────────
            Toggle unlockProductsToggle   = CreateToggleRow(card.transform, "Desbloquear todos los productos");
            Toggle unlockEmployeesToggle  = CreateToggleRow(card.transform, "Desbloquear todos los empleados");
            Toggle unlockSecurityToggle   = CreateToggleRow(card.transform, "Desbloquear toda la seguridad");
            Toggle unlockAllTreeToggle    = CreateToggleRow(card.transform, "Desbloquear todo el Árbol");
            Toggle giveTestStockToggle    = CreateToggleRow(card.transform, "Stock de prueba (básicos)");
            Toggle buyExpansionsToggle    = CreateToggleRow(card.transform, "Comprar expansiones de prueba");
            Toggle prepareSalesTestToggle = CreateToggleRow(card.transform, "Prueba de ventas (dinero+stock+cajero)");
            Toggle completeAchievementsToggle = CreateToggleRow(card.transform, "Completar todos los logros");
            Toggle forceShoplifterToggle  = CreateToggleRow(card.transform, "Forzar ladrón (próximo cliente)");
            Toggle triggerMonopolyToggle  = CreateToggleRow(card.transform, "Simular final de monopolio");
            Toggle triggerBankruptcyToggle = CreateToggleRow(card.transform, "Simular bancarrota (Game Over)");

            // ── Buttons ───────────────────────────────────────────────────────
            GameObject buttonRow = CreateUIObject("ButtonRow", card.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            buttonRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 52f);
            HorizontalLayoutGroup hlg = buttonRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16f;
            hlg.childControlWidth = true;
            hlg.childForceExpandWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandHeight = true;

            Button cancelBtn = CreateCardButton("CancelButton", buttonRow.transform, "Cancelar",  new Color(0.38f, 0.22f, 0.22f, 1f));
            Button startBtn  = CreateCardButton("StartButton",  buttonRow.transform, "Iniciar →", new Color(0.14f, 0.42f, 0.22f, 1f));

            // ── Wire button events ────────────────────────────────────────────
            adminBtn.onClick.AddListener(() => overlay.SetActive(true));
            cancelBtn.onClick.AddListener(() => overlay.SetActive(false));

            startBtn.onClick.AddListener(() =>
            {
                ApplyAndStart(
                    moneyInput,
                    pointsInput,
                    unlockProductsToggle,
                    unlockEmployeesToggle,
                    unlockSecurityToggle,
                    unlockAllTreeToggle,
                    giveTestStockToggle,
                    buyExpansionsToggle,
                    prepareSalesTestToggle,
                    completeAchievementsToggle,
                    forceShoplifterToggle,
                    triggerMonopolyToggle,
                    triggerBankruptcyToggle);
            });
        }

        // ── Apply & transition ─────────────────────────────────────────────────

        private static void ApplyAndStart(
            TMP_InputField moneyInput,
            TMP_InputField pointsInput,
            Toggle unlockProducts,
            Toggle unlockEmployees,
            Toggle unlockSecurity,
            Toggle unlockAll,
            Toggle giveTestStock,
            Toggle buyExpansions,
            Toggle prepareSalesTest,
            Toggle completeAchievements,
            Toggle forceShoplifter,
            Toggle triggerMonopoly,
            Toggle triggerBankruptcy)
        {
            Debug.Log(LogPrefix + "Starting admin session...");

            // Parse money (convert dollars to cents). Cap to avoid overflow on × 100.
            long moneyDollars = 5000;
            if (moneyInput != null && !string.IsNullOrEmpty(moneyInput.text))
                long.TryParse(moneyInput.text, out moneyDollars);
            if (moneyDollars < 0) moneyDollars = 0;
            // long.MaxValue / 100 ≈ 92_233_720_368_547_758 — cap at a sane game maximum.
            // 9,999,999 dollars = $9.9M which is more than any realistic game session needs,
            // while safely fitting in a long after × 100 (cents) and UI rendering.
            const long MaxMoneyDollars = 9_999_999L;
            if (moneyDollars > MaxMoneyDollars) moneyDollars = MaxMoneyDollars;

            // Parse tree points.
            int pts = 0;
            if (pointsInput != null && !string.IsNullOrEmpty(pointsInput.text))
                int.TryParse(pointsInput.text, out pts);
            if (pts < 0) pts = 0;

            AdminSessionConfig.isActive           = true;
            AdminSessionConfig.startMoney         = moneyDollars * 100L; // convert to cents
            AdminSessionConfig.treePoints         = pts;
            AdminSessionConfig.unlockAllProducts  = unlockProducts  != null && unlockProducts.isOn;
            AdminSessionConfig.unlockAllEmployees = unlockEmployees != null && unlockEmployees.isOn;
            AdminSessionConfig.unlockAllSecurity  = unlockSecurity  != null && unlockSecurity.isOn;
            AdminSessionConfig.unlockEntireTree   = unlockAll       != null && unlockAll.isOn;
            AdminSessionConfig.giveTestStock      = giveTestStock   != null && giveTestStock.isOn;
            AdminSessionConfig.buyTestExpansions  = buyExpansions   != null && buyExpansions.isOn;
            AdminSessionConfig.prepareSalesTest   = prepareSalesTest != null && prepareSalesTest.isOn;
            AdminSessionConfig.completeAllAchievements = completeAchievements != null && completeAchievements.isOn;
            AdminSessionConfig.forceShoplifterSpawn    = forceShoplifter != null && forceShoplifter.isOn;
            AdminSessionConfig.triggerMonopolyTest     = triggerMonopoly != null && triggerMonopoly.isOn;
            AdminSessionConfig.triggerBankruptcyTest   = triggerBankruptcy != null && triggerBankruptcy.isOn;

            // prepareSalesTest is a convenience preset — it implies unlockProducts + giveTestStock.
            if (AdminSessionConfig.prepareSalesTest)
            {
                AdminSessionConfig.unlockAllProducts  = true;
                AdminSessionConfig.unlockAllEmployees = true;
                AdminSessionConfig.giveTestStock      = true;
                if (AdminSessionConfig.startMoney < 500000L)
                    AdminSessionConfig.startMoney = 500000L; // ensure at least $5,000
            }

            Debug.Log(LogPrefix + $"Config — money: ${moneyDollars}, points: {pts}, " +
                      $"products: {AdminSessionConfig.unlockAllProducts}, " +
                      $"employees: {AdminSessionConfig.unlockAllEmployees}, " +
                      $"security: {AdminSessionConfig.unlockAllSecurity}, " +
                      $"allTree: {AdminSessionConfig.unlockEntireTree}, " +
                      $"testStock: {AdminSessionConfig.giveTestStock}, " +
                      $"testExpansions: {AdminSessionConfig.buyTestExpansions}, " +
                      $"salesTest: {AdminSessionConfig.prepareSalesTest}");

            // Start a fresh game (same flow as clicking "New Game").
            UIIntro intro = Object.FindAnyObjectByType<UIIntro>();
            if (intro != null)
                intro.LoadGame(isNew: true);
            else
            {
                // Fallback: load directly.
                SaveGameSystem.New();
                SceneManager.LoadScene(GameSceneIndex);
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static bool IsIntroScene(Scene scene)
        {
            string nameLower = scene.name.ToLowerInvariant();
            return nameLower.Contains("intro") || scene.buildIndex == IntroSceneIndex;
        }

        private static GameObject CreateUIObject(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot     = pivot;
            return go;
        }

        private static TMP_Text CreateLabel(string name, Transform parent, string text,
            float fontSize, Color? color = null, float height = 36f,
            TextAlignmentOptions align = TextAlignmentOptions.Left)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin  = new Vector2(0f, 0f);
            rt.anchorMax  = new Vector2(1f, 0f);
            rt.pivot      = new Vector2(0.5f, 0f);
            rt.sizeDelta  = new Vector2(0f, height);
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            TextMeshProUGUI lbl = go.GetComponent<TextMeshProUGUI>();
            lbl.text      = text;
            lbl.fontSize  = fontSize;
            lbl.color     = color ?? Color.white;
            lbl.alignment = align;
            return lbl;
        }

        private static TMP_InputField CreateInputRow(Transform parent, string labelText, string defaultValue)
        {
            GameObject row = new GameObject("Row_" + labelText, typeof(RectTransform), typeof(CanvasRenderer));
            row.transform.SetParent(parent, false);
            RectTransform rowRT = row.GetComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0f, 0f);
            rowRT.anchorMax = new Vector2(1f, 0f);
            rowRT.pivot     = new Vector2(0.5f, 0f);
            rowRT.sizeDelta = new Vector2(0f, 50f);
            LayoutElement rowLE = row.AddComponent<LayoutElement>();
            rowLE.preferredHeight = 50f;
            HorizontalLayoutGroup rowHLG = row.AddComponent<HorizontalLayoutGroup>();
            rowHLG.spacing = 12f;
            rowHLG.childControlHeight = true;
            rowHLG.childForceExpandHeight = true;
            rowHLG.childControlWidth = false;
            rowHLG.childForceExpandWidth = false;

            // Label
            GameObject lblGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            lblGO.transform.SetParent(row.transform, false);
            LayoutElement lblLE = lblGO.AddComponent<LayoutElement>();
            lblLE.preferredWidth = 220f;
            TextMeshProUGUI lblTxt = lblGO.GetComponent<TextMeshProUGUI>();
            lblTxt.text      = labelText;
            lblTxt.fontSize  = 17f;
            lblTxt.color     = new Color(0.85f, 0.85f, 0.85f);
            lblTxt.alignment = TextAlignmentOptions.MidlineLeft;

            // Input field
            GameObject inputGO = new GameObject("Input", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image));
            inputGO.transform.SetParent(row.transform, false);
            LayoutElement inputLE = inputGO.AddComponent<LayoutElement>();
            inputLE.preferredWidth = 160f;
            Image inputBg = inputGO.GetComponent<Image>();
            inputBg.color = new Color(0.16f, 0.20f, 0.28f, 1f);
            TMP_InputField inputField = inputGO.AddComponent<TMP_InputField>();
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;

            // Text area
            GameObject textArea = new GameObject("Text Area", typeof(RectTransform), typeof(CanvasRenderer));
            textArea.transform.SetParent(inputGO.transform, false);
            RectTransform taRT = textArea.GetComponent<RectTransform>();
            taRT.anchorMin = Vector2.zero;
            taRT.anchorMax = Vector2.one;
            taRT.offsetMin = new Vector2(6f, 0f);
            taRT.offsetMax = new Vector2(-6f, 0f);
            textArea.AddComponent<RectMask2D>();

            GameObject inputTextGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            inputTextGO.transform.SetParent(textArea.transform, false);
            RectTransform inputTextRT = inputTextGO.GetComponent<RectTransform>();
            inputTextRT.anchorMin = Vector2.zero;
            inputTextRT.anchorMax = Vector2.one;
            inputTextRT.offsetMin = Vector2.zero;
            inputTextRT.offsetMax = Vector2.zero;
            TextMeshProUGUI inputText = inputTextGO.GetComponent<TextMeshProUGUI>();
            inputText.fontSize  = 17f;
            inputText.color     = Color.white;
            inputText.alignment = TextAlignmentOptions.MidlineLeft;
            inputField.textComponent   = inputText;
            inputField.textViewport    = taRT;
            inputField.text            = defaultValue;

            return inputField;
        }

        private static Toggle CreateToggleRow(Transform parent, string labelText)
        {
            GameObject row = new GameObject("ToggleRow_" + labelText, typeof(RectTransform),
                typeof(CanvasRenderer));
            row.transform.SetParent(parent, false);
            RectTransform rowRT = row.GetComponent<RectTransform>();
            rowRT.sizeDelta = new Vector2(0f, 36f);
            LayoutElement rowLE = row.AddComponent<LayoutElement>();
            rowLE.preferredHeight = 36f;
            HorizontalLayoutGroup rowHLG = row.AddComponent<HorizontalLayoutGroup>();
            rowHLG.spacing             = 12f;
            rowHLG.childControlHeight  = true;
            rowHLG.childForceExpandHeight = true;
            rowHLG.childControlWidth   = false;
            rowHLG.childForceExpandWidth = false;

            // Toggle background
            GameObject toggleGO = new GameObject("Toggle", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image));
            toggleGO.transform.SetParent(row.transform, false);
            LayoutElement toggleLE = toggleGO.AddComponent<LayoutElement>();
            toggleLE.preferredWidth  = 30f;
            toggleLE.preferredHeight = 30f;
            Image toggleBg = toggleGO.GetComponent<Image>();
            toggleBg.color = new Color(0.16f, 0.20f, 0.28f, 1f);
            Toggle toggle  = toggleGO.AddComponent<Toggle>();
            toggle.targetGraphic = toggleBg;

            // Checkmark
            GameObject checkGO = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image));
            checkGO.transform.SetParent(toggleGO.transform, false);
            RectTransform checkRT = checkGO.GetComponent<RectTransform>();
            checkRT.anchorMin = new Vector2(0.1f, 0.1f);
            checkRT.anchorMax = new Vector2(0.9f, 0.9f);
            checkRT.offsetMin = Vector2.zero;
            checkRT.offsetMax = Vector2.zero;
            Image checkImg = checkGO.GetComponent<Image>();
            checkImg.color = new Color(0.2f, 0.8f, 0.35f, 1f);
            toggle.graphic = checkImg;

            // Label
            GameObject lblGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            lblGO.transform.SetParent(row.transform, false);
            LayoutElement lblLE = lblGO.AddComponent<LayoutElement>();
            lblLE.flexibleWidth = 1f;
            TextMeshProUGUI lblTxt = lblGO.GetComponent<TextMeshProUGUI>();
            lblTxt.text      = labelText;
            lblTxt.fontSize  = 16f;
            lblTxt.color     = new Color(0.85f, 0.85f, 0.85f);
            lblTxt.alignment = TextAlignmentOptions.MidlineLeft;
            toggle.isOn      = false;

            return toggle;
        }

        private static Button CreateCardButton(string name, Transform parent, string label, Color bgColor)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image bg  = go.GetComponent<Image>();
            bg.color  = bgColor;
            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;

            GameObject lblGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            lblGO.transform.SetParent(go.transform, false);
            RectTransform lblRT = lblGO.GetComponent<RectTransform>();
            lblRT.anchorMin = Vector2.zero;
            lblRT.anchorMax = Vector2.one;
            lblRT.offsetMin = Vector2.zero;
            lblRT.offsetMax = Vector2.zero;
            TextMeshProUGUI lbl = lblGO.GetComponent<TextMeshProUGUI>();
            lbl.text      = label;
            lbl.fontSize  = 19f;
            lbl.color     = Color.white;
            lbl.alignment = TextAlignmentOptions.Center;
            return btn;
        }
    }
}
