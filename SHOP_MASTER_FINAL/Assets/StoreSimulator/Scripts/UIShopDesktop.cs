//Adaptado por POMPIC 20100333
/*  This file is part of the "Store Simulator" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store, Epic FAB).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

namespace FLOBUK.StoreSimulator
{   
    /// <summary>
    /// Shop opened on a device allowing to purchase various available goods in order to replenish or extend the store.
    /// </summary>
    public class UIShopDesktop : Interactable
    {
        /// <summary>
        /// The initial position and rotation the player camera should move to when entering this object.
        /// </summary>
        public Transform lookTransform;

        /// <summary>
        /// The speed used when entering this object.
        /// </summary>
        public float lerpSpeed = 2f;

        /// <summary>
        /// Label for displaying the amount of player currency that can be spent.
        /// </summary>
        public TMP_Text moneyDisplay;

        /// <summary>
        /// Label for displaying the experience level.
        /// </summary>
        public TMP_Text levelDisplay;

        /// <summary>
        /// Label for displaying the number of the day played.
        /// </summary>
        public TMP_Text dayDisplay;

        /// <summary>
        /// Label for displaying the current formatted time string.
        /// </summary>
        public TMP_Text timeDisplay;

        /// <summary>
        /// Input field allowing players to change the name of the store.
        /// </summary>
        public TMP_InputField storeNameInput;

        /// <summary>
        /// Clip to play whenever a button has been clicked, or none if not set. 
        /// </summary>
        public AudioClip clickClip;

        //previous camera position that should be transitioned back to when leaving
        private Vector3 prevCamPosition;
        //previous camera rotation that should be transitioned back to when leaving
        private Quaternion prevCamRotation;
        //reference to collider used for detecting an interaction
        private Collider col;
        //whether the computer is currently controlling player input
        private bool isOpen;
        //whether this desktop is subscribed to input callbacks
        private bool actionSubscribed;
        //currently open desktop used by QA restore
        private static UIShopDesktop activeDesktop;

        public bool IsComputerOpen => isOpen;


        //initialize references
        void Awake()
        {
            col = GetComponent<Collider>();

            StoreDatabase.onMoneyUpdate += OnMoneyUpdate;
            StoreDatabase.onLevelUpdate += OnLevelUpdate;
            DayCycleSystem.onTimeUpdate += OnTimeUpdate;
        }


        //initialize variables
        IEnumerator Start()
        {
            if (StoreDatabase.Instance != null)
                OnMoneyUpdate(StoreDatabase.GetMoneyString(), string.Empty);
            if (DayCycleSystem.Instance != null)
                OnTimeUpdate(DayCycleSystem.GetTimeString());
            if (levelDisplay != null && StoreDatabase.Instance != null)
                levelDisplay.text = StoreDatabase.GetLevelString();
            if (dayDisplay != null && DayCycleSystem.Instance != null)
                dayDisplay.text = DayCycleSystem.GetDayString();
            if (storeNameInput != null && StoreDatabase.Instance != null)
            {
                storeNameInput.text = StoreDatabase.GetStoreName();
                storeNameInput.onEndEdit.AddListener((x) => StoreDatabase.SetStoreName(x));
            }

            EntrepreneurTreeUIBootstrap.Ensure(this);
            UIEmployeesUIBootstrap.Ensure(this);
            UIManagementUIBootstrap.Ensure(this);
            OptimizeNavigationLayout();

            yield return new WaitForSeconds(1);
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for(int i = 0; i < buttons.Length; i++)
                buttons[i].onClick.AddListener(() => AudioSystem.Play2D(clickClip));
        }


        /// <summary>
        /// Interactable override, adding UI action.
        /// </summary>
        public override void OnBecameFocus()
        {
            UIGame.AddAction("LeftClick", "Use", true);
        }


        /// <summary>
        /// Interactable override, react on player interaction.
        /// This object can be "controlled" by the player.
        /// </summary>
        public override bool Interact(string actionName)
        {
            if (actionName != "LeftClick") return false;
            if (isOpen) return true;

            SubscribeInput();
            if (UIGame.Instance != null)
                UIGame.AddAction("Esc", "Exit");
            
            if (UIGame.Instance != null)
                UIGame.Instance.SetVisible(false);
            PlayerController.SetGameplayInputEnabled(false);
            if (InteractionSystem.Instance != null)
                InteractionSystem.SetInteractionState(InteractionState.None);
            if (col != null)
                col.enabled = false;
            isOpen = true;
            activeDesktop = this;

            Transform camTransform = PlayerController.GetCameraTransform();
            if (camTransform != null)
            {
                prevCamPosition = camTransform.localPosition;
                prevCamRotation = camTransform.localRotation;

                if (lookTransform != null && InteractionSystem.Instance != null)
                    InteractionSystem.MoveToTargetLinear(camTransform, null, lookTransform.position, Quaternion.LookRotation(lookTransform.forward), lerpSpeed, false);
            }
            return true;
        }


        /// <summary>
        /// Interactable override, removing UI action.
        /// </summary>
        public override void OnLostFocus()
        {
            UIGame.RemoveAction("LeftClick");
        }


        /// <summary>
        /// Cancel controlling this object and return to player movement mode.
        /// </summary>
        public void Exit()
        {
            if (!isOpen)
            {
                RestorePlayerAfterComputerClosed();
                return;
            }

            UnsubscribeInput();
            if (UIGame.Instance != null)
                UIGame.RemoveAction("Esc");

            Transform camTransform = PlayerController.GetCameraTransform();
            if (camTransform != null && InteractionSystem.Instance != null)
                InteractionSystem.MoveToTargetLinear(camTransform, null, prevCamPosition, prevCamRotation, lerpSpeed, true);

            Invoke("ReEnable", 0.5f);
        }


        //after exiting the controlled state, player movement is re-enabled with a short delay
        private void ReEnable()
        {
            RestorePlayerAfterComputerClosed();
        }


        private void RestorePlayerAfterComputerClosed()
        {
            isOpen = false;
            if (activeDesktop == this)
                activeDesktop = null;
            UnsubscribeInput();
            if (UIGame.Instance != null)
                UIGame.RemoveAction("Esc");
            if (InteractionSystem.Instance != null)
                InteractionSystem.SetInteractionState(InteractionState.All);
            PlayerController.RestoreGameplayInput();
            if (col != null)
                col.enabled = true;

            if (UIGame.Instance != null)
                UIGame.Instance.SetVisible(true);
        }


        private void SubscribeInput()
        {
            if (actionSubscribed)
                return;

            PlayerInput input = PlayerController.GetActivePlayerInput();
            if (input == null)
                return;

            input.onActionTriggered += OnAction;
            actionSubscribed = true;
        }


        private void UnsubscribeInput()
        {
            if (!actionSubscribed)
                return;

            PlayerInput input = PlayerController.GetActivePlayerInput();
            if (input != null)
                input.onActionTriggered -= OnAction;
            actionSubscribed = false;
        }


        //subscribed to money change
        private void OnMoneyUpdate(string money, string change)
        {
            if (moneyDisplay != null)
                moneyDisplay.text = money;
        }


        //subscribed to level change
        //use the pre-formatted string instead of value only
        private void OnLevelUpdate(int level)
        {
            if (levelDisplay != null)
                levelDisplay.text = StoreDatabase.GetLevelString();
        }


        //subscribed to time change
        private void OnTimeUpdate(string time)
        {
            if (timeDisplay != null)
                timeDisplay.text = time;
        }


#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_INCLUDE_TESTS
        public bool OpenArbolForQA(bool showQaOverlay = true)
        {
            return OpenAppForQA("ARBOL", showQaOverlay);
        }


        public static void RestoreGameplayInputForQA()
        {
            if (activeDesktop != null)
                activeDesktop.RestorePlayerAfterComputerClosed();
            else
                PlayerController.RestoreGameplayInput();
        }


        public bool OpenAppForQA(string appLabel, bool showQaOverlay = false)
        {
            EnsureComputerAppsForQA();

            Transform contentArea = FindRecursive(transform, "ContentArea");
            if (contentArea == null)
                return false;

            Transform targetPanel = ResolveAppPanelForQA(contentArea, appLabel);
            if (targetPanel == null)
                return false;

            UIShopCategoryHelper helper = contentArea.GetComponent<UIShopCategoryHelper>();
            if (helper == null)
                helper = contentArea.gameObject.AddComponent<UIShopCategoryHelper>();

            helper.Show(targetPanel.gameObject);

            EntrepreneurTreeUI tree = targetPanel.GetComponentInChildren<EntrepreneurTreeUI>(true);
            if (tree != null)
                tree.SetQaOverlayVisibleForQA(showQaOverlay);

            return targetPanel.gameObject.activeSelf;
        }


        private void EnsureComputerAppsForQA()
        {
            EntrepreneurTreeUIBootstrap.Ensure(this);
            UIEmployeesUIBootstrap.Ensure(this);
            UIManagementUIBootstrap.Ensure(this);
            OptimizeNavigationLayout();
        }


        private static Transform ResolveAppPanelForQA(Transform contentArea, string appLabel)
        {
            string normalized = string.IsNullOrWhiteSpace(appLabel) ? string.Empty : appLabel.Trim().ToUpperInvariant();
            switch (normalized)
            {
                case "ARBOL":
                case "TREE":
                case "ENTREPRENEUR_TREE":
                    return FindDirectChildForQA(contentArea, "Licenses");
                case "EMPLEADOS":
                case "EMPLOYEES":
                    return FindDirectChildForQA(contentArea, "Employees");
                case "GESTION":
                case "MANAGEMENT":
                    return FindDirectChildForQA(contentArea, "Management");
                default:
                    return FindDirectChildForQA(contentArea, appLabel);
            }
        }


        private static Transform FindDirectChildForQA(Transform parent, string objectName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(objectName))
                return null;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == objectName)
                    return child;
            }

            return null;
        }
#endif


        private void OptimizeNavigationLayout()
        {
            Transform navigation = FindRecursive(transform, "Navigation");
            Transform categories = navigation != null ? FindRecursive(navigation, "Categories") : null;
            if (categories == null)
                return;

            HorizontalLayoutGroup layout = categories.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
                layout = categories.gameObject.AddComponent<HorizontalLayoutGroup>();

            layout.spacing = 6;
            layout.padding = new RectOffset(4, 4, 2, 2);
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            Button[] buttons = categories.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
                ConfigureNavigationButton(buttons[i]);
        }


        private void ConfigureNavigationButton(Button button)
        {
            if (button == null)
                return;

            LayoutElement layoutElement = button.GetComponent<LayoutElement>();
            if (layoutElement == null)
                layoutElement = button.gameObject.AddComponent<LayoutElement>();

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            string text = label != null ? label.text : button.gameObject.name;
            float preferredWidth = Mathf.Clamp(72f + text.Length * 5.5f, 96f, 150f);
            layoutElement.minWidth = 88f;
            layoutElement.preferredWidth = preferredWidth;
            layoutElement.minHeight = 34f;
            layoutElement.preferredHeight = 38f;

            if (label == null)
                return;

            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.enableAutoSizing = true;
            label.fontSizeMin = 9;
            label.fontSizeMax = 15;
            label.alignment = TextAlignmentOptions.Center;
        }


        private static Transform FindRecursive(Transform parent, string objectName)
        {
            if (parent == null)
                return null;

            if (parent.name == objectName)
                return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindRecursive(parent.GetChild(i), objectName);
                if (found != null)
                    return found;
            }

            return null;
        }


        //react on user input
        private void OnAction(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                switch(context.action.name)
                {
                    case "Cancel":
                        Exit();
                        break;
                }
            }
        }


        //unsubscribe from events
        void OnDestroy()
        {
            if (activeDesktop == this)
                activeDesktop = null;
            UnsubscribeInput();
            StoreDatabase.onMoneyUpdate -= OnMoneyUpdate;
            StoreDatabase.onLevelUpdate -= OnLevelUpdate;
            DayCycleSystem.onTimeUpdate -= OnTimeUpdate;
        }
    }
}
