//Adaptado por POMPIC 20100333
/*  This file is part of the "Store Simulator" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store, Epic FAB).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using UnityEngine;
using UnityEngine.InputSystem;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Let's the player control this character to move, look and jump around the scene.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// Returns a reference to this script instance.
        /// </summary>
        public static PlayerController Instance { get; private set; }

        [Header("Movement")]
        /// <summary>
        /// Speed for walking around.
        /// </summary>
        [Range(1, 20)]
        public int speed = 5;

        /// <summary>
        /// Speed while holding the run key.
        /// </summary>
        [Range(1, 20)]
        public float runSpeed = 10f;

        /// <summary>
        /// Multiplier applied while crouching.
        /// </summary>
        [Range(0.1f, 1f)]
        public float crouchSpeedMultiplier = 0.55f;

        [Header("Rotation")]
        /// <summary>
        /// A reference to the player's camera transform.
        /// </summary>
        public Transform cameraTransform;

        /// <summary>
        /// Multiplier for looking around.
        /// </summary>
        [Range(0.1f, 2)]
        public float viewSensitivity = 0.5f;

        /// <summary>
        /// Clamping the values of looking up and down to not exceed certain angles.
        /// </summary>
        public Vector2 viewClamp = new Vector2(-40, 40);

        [Header("Gravity")]
        /// <summary>
        /// Multiplier of gravity applied to the player.
        /// </summary>
        [Range(0.1f, 2f)]
        public float gravityMultiplier = 0.3f;

        /// <summary>
        /// Upwards force applied when jumping. 
        /// </summary>
        [Range(1, 5)]
        public int jumpForce = 2;

        [Header("Crouch")]
        /// <summary>
        /// CharacterController height while crouching.
        /// </summary>
        [Range(0.5f, 2f)]
        public float crouchHeight = 1f;

        [Header("Hands")]
        /// <summary>
        /// A reference to the character's hands as parents when picking up objects.
        /// </summary>
        public Transform hands;

        [Header("Mobile")]
        /// <summary>
        /// Joysticks that should be visible when running on mobile platform only.
        /// </summary>
        public GameObject[] joysticks;

        //reference to the underlying CharacterController component
        private CharacterController characterController;
        //currently allowed movement states
        private MovementState movementState = MovementState.All;
        //previous movement state before changing it
        private MovementState previousMovementState = MovementState.All;
        //movement input directly read from the keyboard/mobile joystick
        private Vector2 moveInput;
        //cache of movement input for further processing
        private Vector2 moveCache;
        //movement direction based on input cache and gravity
        private Vector3 moveDir;
        //rotation input directly read from the mouse/mobile joystick
        private Vector2 viewInput;
        //player rotation with sensitivity applied
        private Vector3 playerRotation;
        //camera rotation only, with sensitivity applied
        private Vector3 cameraRotation;
        //skip one update frame after locking mouse cursor to discard delta
        private bool skipMouseDelta = false;
        //gravity value including multiplier when not grounded
        private float gravityVelocity;
        //the package that is currently carried around
        private PackageObject handsPackage;
        //cached PlayerInput reference used for clean subscription handling
        private PlayerInput playerInput;
        //cached gameplay action map used by the real PlayerInput
        private InputActionMap gameplayActionMap;
        //cached optional UI action map, if the project ever adds one
        private InputActionMap uiActionMap;
        //whether this controller is currently subscribed to PlayerInput callbacks
        private bool inputSubscribed;
        //warning guard for optional UI action map logs
        private static bool missingUiActionMapWarningLogged;
        //warning guard for fixtures/scenes without store entry
        private static bool missingStoreEntryWarningLogged;
        //last known reason for movement being blocked, useful for QA
        private string lastBlockReason = "Gameplay";
        //whether the player is currently sprinting
        private bool isSprinting;
        //whether the player is currently crouching
        private bool isCrouching;
        //last cursor mode requested by gameplay/UI transitions
        private bool wantsGameplayCursorLocked = true;
        //initial CharacterController dimensions for crouch restore
        private float defaultControllerHeight;
        private Vector3 defaultControllerCenter;
        //initial local camera position for crouch restore
        private Vector3 defaultCameraLocalPosition;
#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_INCLUDE_TESTS
        //diagnostic overlay visibility, toggled with F9
        private bool showMovementDiagnostics;
#endif

        public MovementState CurrentMovementState => movementState;
        public bool CanMove => movementState == MovementState.All && characterController != null && characterController.enabled;
        public bool CanLook => movementState == MovementState.All || movementState == MovementState.RotationOnly;
        public bool IsGrounded => characterController != null && characterController.isGrounded;
        public bool IsCrouching => isCrouching;
        public bool IsSprinting => isSprinting;
        public bool IsInputSubscribed => inputSubscribed;
        public bool WantsGameplayCursorLocked => wantsGameplayCursorLocked;
        public Vector2 LastMoveInput => moveInput;
        public Vector2 LastViewInput => viewInput;
        public string LastBlockReason => lastBlockReason;
        public string ActiveActionMapName => playerInput != null && playerInput.currentActionMap != null ? playerInput.currentActionMap.name : string.Empty;
        public float CurrentSpeed => GetCurrentSpeed();
        public float WalkSpeed => speed;
        public float RunSpeed => runSpeed;


        //initialize references
        void Awake()
        {
            Instance = this;
            SetCursorForGameplay(true);

            characterController = GetComponent<CharacterController>();
            defaultControllerHeight = characterController != null ? characterController.height : 0f;
            defaultControllerCenter = characterController != null ? characterController.center : Vector3.zero;

            #if UNITY_ANDROID || UNITY_IOS
                for(int i = 0; i < joysticks.Length; i++)
                    joysticks[i].SetActive(true);
            #endif

            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
            if (cameraTransform != null)
                defaultCameraLocalPosition = cameraTransform.localPosition;

            EnsureInputReady(true);
        }


        //initialize variables
        void Start()
        {
            if (StoreDatabase.Instance != null && StoreDatabase.Instance.storeEntry != null)
                transform.LookAt(StoreDatabase.Instance.storeEntry.position + Vector3.up);
            else if (!missingStoreEntryWarningLogged)
            {
                Debug.Log("PlayerController skipped initial store-entry look because StoreDatabase or storeEntry is missing.");
                missingStoreEntryWarningLogged = true;
            }

            RestoreGameplayInput();
        }


        void OnEnable()
        {
            if (Instance == null)
                Instance = this;

            EnsureInputReady(false);
        }


        void OnDisable()
        {
            UnsubscribeInput();
        }


        //apply different inputs
        void Update()
        {
            if (!EnsureInputReady(false))
                lastBlockReason = "PlayerInput no disponible";

#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_INCLUDE_TESTS
            HandleQaShortcuts();
#endif

            ReadSupplementalKeyboardState();

            switch(movementState)
            {
                case MovementState.All:
                    ApplyGravity();
                    ApplyRotation();
                    ApplyMovement();
                    break;
                
                case MovementState.RotationOnly:
                    ApplyRotation();
                    break;
            }
        }


        /// <summary>
        /// Returns the last remembered movement state before it changed to the current state.
        /// </summary>
        public static MovementState GetPreviousMovementState()
        {
            return Instance != null ? Instance.previousMovementState : MovementState.All;
        }


        /// <summary>
        /// Returns the Transform component of the player's camera.
        /// </summary>
        public static Transform GetCameraTransform()
        {
            return Instance != null ? Instance.cameraTransform : null;
        }


        /// <summary>
        /// Apply a different fixed rotation i.e. when entering an object, like the CashDesk.
        /// </summary>
        public static void SetCameraRotation(Quaternion newRotation)
        {
            if (Instance != null)
                Instance.cameraRotation = newRotation.eulerAngles;
        }


        /// <summary>
        /// Change movement state to allow or disallow certain inputs.
        /// </summary>
        public static void SetMovementState(MovementState state, bool lockCursor)
        {
            if (Instance == null)
                return;

            if (Cursor.lockState == CursorLockMode.None && lockCursor == true)
            {
                if (Mouse.current != null)
                    Mouse.current.WarpCursorPosition(new Vector2(Screen.width / 2, Screen.height / 2));

                Instance.skipMouseDelta = true;
            }

            #if UNITY_ANDROID || UNITY_IOS
                for(int i = 0; i < Instance.joysticks.Length; i++)
                    Instance.joysticks[i].SetActive(state == MovementState.All);
                if (state == MovementState.RotationOnly)
                    Instance.joysticks[1].SetActive(true);
            #endif

            SetCursorForGameplay(lockCursor);
            Instance.previousMovementState = Instance.movementState;
            Instance.movementState = state;
            Instance.lastBlockReason = state == MovementState.All ? "Gameplay" : "UI/controlled object";
            if (state == MovementState.All)
                Instance.EnsureGameplayActionMap();
        }


        /// <summary>
        /// Restore the real gameplay input state after closing UI or controlled objects.
        /// </summary>
        public static void RestoreGameplayInput()
        {
            if (Instance == null)
                return;

            Instance.RestoreGameplayInputInternal();
        }


        /// <summary>
        /// Enable or disable gameplay input through the same state path used by the asset.
        /// </summary>
        public static void SetGameplayInputEnabled(bool enabled)
        {
            if (enabled)
                RestoreGameplayInput();
            else
                SetMovementState(MovementState.None, false);
        }


        /// <summary>
        /// Returns the active PlayerInput used for gameplay subscriptions.
        /// </summary>
        public static PlayerInput GetActivePlayerInput()
        {
            if (Instance == null)
                return PlayerInput.GetPlayerByIndex(0);

            Instance.EnsureInputReady(false);
            return Instance.playerInput;
        }


        /// <summary>
        /// Move PackageObject to the player's hands.
        /// </summary>
        public void Carry(PackageObject package)
        {
            handsPackage = package;
            handsPackage.GetComponent<Rigidbody>().isKinematic = true;
            handsPackage.GetComponent<Animation>().Play("OpenPackage");

            InteractionSystem.MoveToTargetArc(package.transform, hands, Vector3.zero, Quaternion.identity);
        }


        /// <summary>
        /// Destroy or throw away the active package and reenable its physics. 
        /// </summary>
        public void Drop(bool withDestroy = false)
        {
            if (withDestroy)
                Destroy(handsPackage.gameObject);
            else
            {
                StopAllCoroutines(); //in case carry is still animating
                handsPackage.transform.SetParent(null);
                handsPackage.GetComponent<Animation>().Play("ClosePackage");
                handsPackage.GetComponent<Collider>().enabled = true;

                Rigidbody rigidbody = handsPackage.GetComponent<Rigidbody>();
                rigidbody.isKinematic = false;
                rigidbody.AddForce(hands.forward * 10, ForceMode.Impulse);
            }

            handsPackage = null;
        }


        //react on user input
        private void OnAction(InputAction.CallbackContext context)
        {
            switch(context.action.name)
            {
                case "Move":
                    moveInput = context.ReadValue<Vector2>();
                    break;
                case "View":
                case "Look":
                    viewInput = context.ReadValue<Vector2>();
                    if (context.control.device.name == "Gamepad")
                        viewInput *= viewSensitivity;

                    break;
                case "Sprint":
                    isSprinting = context.ReadValue<float>() > 0.1f;
                    break;
                case "Crouch":
                    SetCrouching(context.ReadValue<float>() > 0.1f);
                    break;
                case "Jump":
                    if (context.started)
                    {
                        ApplyJump();
                    }
                    break;
            }
        }


        //movement calculations
        private void ApplyMovement()
        {
            //prevent direction changes while jumping
            if (characterController.isGrounded) moveCache = moveInput;

            Vector3 horizontal = transform.TransformDirection(new Vector3(moveCache.x, 0f, moveCache.y));
            moveDir = horizontal * GetCurrentSpeed();
            characterController.Move((moveDir + Vector3.up * gravityVelocity) * Time.deltaTime);
        }


        //rotation calculations
        private void ApplyRotation()
        {
            //after locking the mouse cursor back to the screen center on desktop
            //we need to skip the first frame with non-zero mouse delta values
            //since otherwise Unity generates a high delta resulting in a camera jump
            if (skipMouseDelta)
            {
                if (viewInput == Vector2.zero)
                    return;

                skipMouseDelta = false;
                return;
            }

            //Player
            if (movementState == MovementState.All)
            {
                playerRotation.y += viewInput.x * viewSensitivity;
                transform.localRotation = Quaternion.Euler(playerRotation);
            }
            else //Camera Only
            {
                cameraRotation.y += viewInput.x * viewSensitivity;
            }

            //Camera
            cameraRotation.x += -viewInput.y * viewSensitivity;
            cameraRotation.x = Mathf.Clamp(cameraRotation.x, viewClamp.x, viewClamp.y);
            if (cameraTransform != null)
                cameraTransform.localRotation = Quaternion.Euler(cameraRotation);
        }


        //gravity calculations
        private void ApplyGravity()
        {
            if (characterController.isGrounded && gravityVelocity < 0)
            {
                gravityVelocity = -1;
            }
            else
            {
                gravityVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
            }
        }


        //jumping calculations
        private void ApplyJump()
        {
            if (movementState != MovementState.All)
                return;

            if (characterController.isGrounded)
            {
                gravityVelocity += jumpForce;
            }
        }


        private bool EnsureInputReady(bool logWarnings)
        {
            if (playerInput == null)
                playerInput = ResolvePlayerInput();

            if (playerInput == null)
            {
                if (logWarnings)
                    Debug.LogWarning("PlayerController could not find PlayerInput. Movement will retry until PlayerInput is available.");
                return false;
            }

            ResolveActionMaps(logWarnings);
            EnsureGameplayActionMap();
            if (!inputSubscribed)
            {
                playerInput.onActionTriggered += OnAction;
                inputSubscribed = true;
            }

            return true;
        }


        private PlayerInput ResolvePlayerInput()
        {
            PlayerInput localInput = GetComponent<PlayerInput>();
            if (localInput != null)
                return localInput;

            PlayerInput indexedInput = PlayerInput.GetPlayerByIndex(0);
            if (indexedInput != null)
                return indexedInput;

            PlayerInput[] inputs = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
            return inputs.Length > 0 ? inputs[0] : null;
        }


        private void ResolveActionMaps(bool logWarnings)
        {
            if (playerInput == null || playerInput.actions == null)
                return;

            gameplayActionMap = playerInput.actions.FindActionMap("Default", false)
                ?? playerInput.actions.FindActionMap("Player", false)
                ?? (playerInput.actions.actionMaps.Count > 0 ? playerInput.actions.actionMaps[0] : null);

            uiActionMap = playerInput.actions.FindActionMap("UI", false);
            if (uiActionMap == null && logWarnings && !missingUiActionMapWarningLogged)
            {
                Debug.LogWarning("PlayerController did not find optional Input Action Map 'UI'. Continuing with gameplay input.");
                missingUiActionMapWarningLogged = true;
            }
        }


        private void EnsureGameplayActionMap()
        {
            if (playerInput == null || playerInput.actions == null)
                return;

            if (gameplayActionMap == null)
                ResolveActionMaps(false);

            if (gameplayActionMap != null)
            {
                if (!gameplayActionMap.enabled)
                    gameplayActionMap.Enable();

                if (playerInput.currentActionMap != gameplayActionMap)
                    playerInput.SwitchCurrentActionMap(gameplayActionMap.name);
            }

            if (uiActionMap != null && uiActionMap != gameplayActionMap)
                uiActionMap.Disable();
        }


        private void UnsubscribeInput()
        {
            if (playerInput != null && inputSubscribed)
                playerInput.onActionTriggered -= OnAction;

            inputSubscribed = false;
        }


        private void RestoreGameplayInputInternal()
        {
            EnsureInputReady(false);
            Time.timeScale = 1f;
            if (characterController != null)
                characterController.enabled = true;
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
            if (cameraTransform != null)
                cameraTransform.gameObject.SetActive(true);

            moveInput = Vector2.zero;
            moveCache = Vector2.zero;
            viewInput = Vector2.zero;
            gravityVelocity = characterController != null && characterController.isGrounded ? -1f : gravityVelocity;
            SetCrouching(false);
            SetMovementState(MovementState.All, true);
            lastBlockReason = "Gameplay restaurado";
        }


        private static void SetCursorForGameplay(bool gameplay)
        {
            if (Instance != null)
                Instance.wantsGameplayCursorLocked = gameplay;
            Cursor.lockState = gameplay ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !gameplay;
        }


        private void ReadSupplementalKeyboardState()
        {
            if (Keyboard.current == null || movementState != MovementState.All)
                return;

            bool runPressed = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            bool crouchPressed = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed || Keyboard.current.cKey.isPressed;
            isSprinting = runPressed;
            SetCrouching(crouchPressed);
        }


        private float GetCurrentSpeed()
        {
            float current = isSprinting ? Mathf.Max(runSpeed, speed) : speed;
            return isCrouching ? current * crouchSpeedMultiplier : current;
        }


        private void SetCrouching(bool crouching)
        {
            if (isCrouching == crouching)
                return;

            isCrouching = crouching;
            if (characterController != null && defaultControllerHeight > 0f)
            {
                characterController.height = crouching ? Mathf.Min(defaultControllerHeight, crouchHeight) : defaultControllerHeight;
                characterController.center = crouching ? defaultControllerCenter + Vector3.down * ((defaultControllerHeight - characterController.height) * 0.5f) : defaultControllerCenter;
            }

            if (cameraTransform != null)
            {
                Vector3 target = defaultCameraLocalPosition;
                if (crouching)
                    target += Vector3.down * Mathf.Max(0.1f, (defaultControllerHeight - crouchHeight) * 0.5f);
                cameraTransform.localPosition = target;
            }
        }


#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_INCLUDE_TESTS
        private void HandleQaShortcuts()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.f7Key.wasPressedThisFrame)
            {
                UIShopDesktop.RestoreGameplayInputForQA();
                Debug.Log("QA Restore Gameplay Input ejecutado");
            }

            if (Keyboard.current.f9Key.wasPressedThisFrame)
                showMovementDiagnostics = !showMovementDiagnostics;
        }


        void OnGUI()
        {
            if (!showMovementDiagnostics)
                return;

            GUILayout.BeginArea(new Rect(12, 12, 420, 250), GUI.skin.box);
            GUILayout.Label("Movement QA Diagnostics (F9)");
            GUILayout.Label("Position: " + transform.position.ToString("F3"));
            GUILayout.Label("Speed: " + CurrentSpeed.ToString("F2") + " Grounded: " + IsGrounded);
            GUILayout.Label("canMove: " + CanMove + " canLook: " + CanLook + " state: " + movementState);
            GUILayout.Label("ActionMap: " + ActiveActionMapName + " subscribed: " + inputSubscribed);
            GUILayout.Label("Cursor: " + Cursor.lockState + " visible: " + Cursor.visible);
            GUILayout.Label("timeScale: " + Time.timeScale.ToString("F2") + " CC enabled: " + (characterController != null && characterController.enabled));
            GUILayout.Label("Move: " + moveInput.ToString("F2") + " View: " + viewInput.ToString("F2"));
            GUILayout.Label("Sprinting: " + isSprinting + " Crouching: " + isCrouching);
            GUILayout.Label("Last block: " + lastBlockReason);
            GUILayout.Label("F7 restaura movimiento");
            GUILayout.EndArea();
        }


        public void ApplyMovementInputForQA(Vector2 input, float seconds)
        {
            EnsureInputReady(false);
            moveInput = input;
            moveCache = input;
            if (movementState != MovementState.All || characterController == null || !characterController.enabled)
                return;

            Vector3 horizontal = transform.TransformDirection(new Vector3(input.x, 0f, input.y));
            characterController.Move(horizontal * GetCurrentSpeed() * Mathf.Max(0f, seconds));
        }


        public void ApplyLookInputForQA(Vector2 input)
        {
            viewInput = input;
            ApplyRotation();
        }


        public void SetCrouchingForQA(bool crouching)
        {
            SetCrouching(crouching);
        }


        public void SetMovementDiagnosticsVisibleForQA(bool visible)
        {
            showMovementDiagnostics = visible;
        }
#endif


        void OnDestroy()
        {
            UnsubscribeInput();
        }
    }
}
