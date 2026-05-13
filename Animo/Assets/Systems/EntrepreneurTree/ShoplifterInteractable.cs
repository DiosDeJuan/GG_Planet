using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Interaction surface for manual thief capture.
    /// </summary>
    public class ShoplifterInteractable : Interactable
    {
        private const string CaptureActionPrimary = "Action";
        private const string CaptureActionMouseFallback = "LeftClick";
        private const string CaptureHintKey = "E";
        private const string CaptureHintActionLabel = "Detener ladrón";
        private const string CaptureHintPrompt = "Presiona E para detener ladrón";

        [SerializeField] private float captureRange = 2.25f;

        private ShoplifterAgent agent;
        private bool canInteract;

        public void Initialize(ShoplifterAgent targetAgent)
        {
            agent = targetAgent;
            SetInteractable(false);
        }


        public void SetInteractable(bool state)
        {
            canInteract = state;
            enabled = state;
        }


        public override bool ShouldSkipSystemChecks()
        {
            return false;
        }


        public override void OnBecameFocus()
        {
            if (!canInteract || agent == null || !agent.CanBeCaptured())
                return;

            if (UIGame.Instance != null)
                UIGame.AddAction(CaptureHintKey, CaptureHintActionLabel);
            UIGame.Instance?.ShowMessage(CaptureHintPrompt);
        }


        public override bool Interact(string actionName)
        {
            if (!canInteract || agent == null || !agent.CanBeCaptured())
                return false;

            if (actionName != CaptureActionPrimary && actionName != CaptureActionMouseFallback)
                return false;

            Transform cameraTransform = PlayerController.GetCameraTransform();
            if (cameraTransform != null)
            {
                float dist = Vector3.Distance(cameraTransform.position, transform.position);
                if (dist > captureRange)
                {
                    UIGame.Instance?.ShowMessage("Acércate más para detener al ladrón.");
                    return false;
                }
            }

            return agent.TryManualCapture();
        }


        public override void OnLostFocus()
        {
            if (UIGame.Instance != null)
                UIGame.RemoveAction(CaptureHintKey);
        }
    }
}
