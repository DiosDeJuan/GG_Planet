//Adaptado por POMPIC 20100333
/*  This file is part of the "Store Simulator" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store, Epic FAB).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Main UI script in the intro scene.
    /// </summary>
    public class UIIntro : MonoBehaviour
    {
        /// <summary>
        /// Scene to load for the actual game.
        /// </summary>
        public string gameScene;

        /// <summary>
        /// Container of a background element, allowing easy access to its alpha value.
        /// This element prevents additional user input in scene transitions.
        /// </summary>
        public CanvasGroup blockerGroup;

        private Button adminToggleButton;
        private TMP_Text adminToggleLabel;
        private TMP_Text adminStatusLabel;


        void Start()
        {
            BuildAdminModeControls();
            RefreshAdminModeUI("Modo Admin listo para pruebas.");
        }


        /// <summary>
        /// Lerp alpha value of the UI canvas group to let it fade in or out over time.
        /// </summary>
        public static IEnumerator FadeInOut(CanvasGroup group, float delay, bool fadeIn)
        {
            float startingAlpha = fadeIn ? 0 : 1;
            float targetAlpha = fadeIn ? 1 : 0;
            float lerpDuration = 0.5f;
            float lerpProgress = 0f;
            
            if (delay > 0)
                yield return new WaitForSecondsRealtime(delay);

            if (fadeIn) group.gameObject.SetActive(true);
            while (lerpProgress < lerpDuration)
            {
                lerpProgress += Time.deltaTime;
                float a = Mathf.Lerp(startingAlpha, targetAlpha, lerpProgress / lerpDuration);

                group.alpha = a;
                yield return null;
            }

            group.alpha = targetAlpha;
            if (!fadeIn) group.gameObject.SetActive(false);
        } 


        /// <summary>
        /// Create or load an existing save file and transition to the game scene.
        /// </summary>
        public void LoadGame(bool isNew)
        {
            AdminModeService.MarkIntroFlowRequested();
            if (isNew)
                SaveGameSystem.New();
            else
                SaveGameSystem.Load();

            StartCoroutine(FadeInOut(blockerGroup, 0, true));
            Invoke("LoadScene", 1);
        }


        /// <summary>
        /// Shut down the game.
        /// </summary>
        public void Exit()
        {
            Application.Quit();
        }


        //load scene
        private void LoadScene()
        {
            SceneManager.LoadScene(gameScene);
        }


        private void BuildAdminModeControls()
        {
            Transform buttonsRoot = FindRecursive(transform, "Buttons");
            if (buttonsRoot == null || buttonsRoot.Find("Button - Admin Toggle") != null)
                return;

            Button template = buttonsRoot.GetComponentInChildren<Button>(true);
            if (template == null)
                return;

            adminToggleButton = CreateAdminButton(template, buttonsRoot, "Button - Admin Toggle", string.Empty, ToggleAdminMode);
            adminToggleLabel = adminToggleButton.GetComponentInChildren<TMP_Text>(true);
            CreateAdminButton(template, buttonsRoot, "Button - Admin Basic", "ADMIN BASICO", () => RequestAdminPackage(AdminModePackage.Basic));
            CreateAdminButton(template, buttonsRoot, "Button - Admin Medium", "ADMIN MEDIO", () => RequestAdminPackage(AdminModePackage.Medium));
            CreateAdminButton(template, buttonsRoot, "Button - Admin Total", "ADMIN TOTAL", () => RequestAdminPackage(AdminModePackage.Total));
            CreateAdminButton(template, buttonsRoot, "Button - Admin Reset", "RESET ADMIN", ResetAdminSession);

            GameObject statusObject = new GameObject("Admin Mode Status", typeof(RectTransform));
            statusObject.transform.SetParent(buttonsRoot, false);
            adminStatusLabel = statusObject.AddComponent<TextMeshProUGUI>();
            adminStatusLabel.alignment = TextAlignmentOptions.Center;
            adminStatusLabel.fontSize = 14;
            adminStatusLabel.enableAutoSizing = true;
            adminStatusLabel.fontSizeMin = 9;
            adminStatusLabel.fontSizeMax = 14;
            adminStatusLabel.textWrappingMode = TextWrappingModes.Normal;
            adminStatusLabel.color = Color.white;

            LayoutElement layout = statusObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 58;
            layout.minHeight = 42;
        }


        private Button CreateAdminButton(Button template, Transform parent, string objectName, string label, UnityEngine.Events.UnityAction action)
        {
            Button button = Instantiate(template, parent);
            button.name = objectName;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);

            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = label;
                text.enableAutoSizing = true;
                text.fontSizeMin = 10;
                text.fontSizeMax = 22;
                text.textWrappingMode = TextWrappingModes.NoWrap;
                text.overflowMode = TextOverflowModes.Ellipsis;
                text.alignment = TextAlignmentOptions.Center;
            }

            return button;
        }


        private void ToggleAdminMode()
        {
            bool enabled = AdminModeService.SetIntroAdminEnabled(!AdminModeService.IntroAdminEnabled);
            RefreshAdminModeUI(enabled ? "Modo Admin activado. Esta partida quedara marcada como ADMIN." : "Modo Admin desactivado para nuevas acciones.");
        }


        private void RequestAdminPackage(AdminModePackage package)
        {
            AdminModeService.RequestIntroPackage(package);
            RefreshAdminModeUI("Paquete " + AdminModeService.GetPackageLabel(package) + " preparado. Se aplicara al iniciar o continuar partida.");
        }


        private void ResetAdminSession()
        {
            AdminModeService.ResetIntroAdminSession();
            RefreshAdminModeUI("Ayudas Admin de esta sesion reiniciadas.");
        }


        private void RefreshAdminModeUI(string message)
        {
            if (adminToggleLabel != null)
                adminToggleLabel.text = AdminModeService.IntroAdminEnabled ? "MODO ADMIN: ON" : "MODO ADMIN: OFF";

            if (adminStatusLabel != null)
            {
                adminStatusLabel.text =
                    "Estado: " + (AdminModeService.IntroAdminEnabled ? "ADMIN ON" : "ADMIN OFF") + "\n" +
                    "Nivel de jugador: " + AdminModeService.GetPlayerLevelForDisplay() +
                    " | Puntos Arbol: " + EntrepreneurProgress.AvailablePoints +
                    " | Dinero: " + AdminModeService.GetMoneyForDisplay() + "\n" +
                    "Nodos: " + EntrepreneurProgress.GetUnlockedNodeCount() + "/" + EntrepreneurProgress.GetTotalNodeCount() + "\n" +
                    "Cada nivel subido otorga 1 punto del Arbol. " + message;
            }
        }


        private static Transform FindRecursive(Transform parent, string name)
        {
            if (parent == null)
                return null;

            if (parent.name == name)
                return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform result = FindRecursive(parent.GetChild(i), name);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
