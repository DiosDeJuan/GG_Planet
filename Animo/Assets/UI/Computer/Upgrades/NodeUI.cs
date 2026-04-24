// Árbol del Emprendedor — NodeUI
// Represents one node button inside the scrollable tree canvas.
// Attach to the NodePrefab used by UpgradesUIController.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Visual and interactive component for a single node in the Entrepreneur Tree UI.
    ///
    /// State colours:
    ///   Grey   = locked (prerequisites not met, or insufficient points)
    ///   Yellow = available to unlock right now
    ///   Green  = already unlocked
    ///
    /// Interactions:
    ///   Hover → asks UpgradesUIController to show the info panel for this node.
    ///   Click → attempts to unlock this node via EntrepreneurTreeManager.
    ///
    /// PREFAB SETUP:
    ///   Create a UI GameObject, give it:
    ///     - Image component (background) — referenced by "background"
    ///     - Child Image for the icon      — referenced by "iconImage"
    ///     - Child TMP_Text for the label  — referenced by "titleLabel"
    ///   Attach this script and assign the references in the Inspector.
    ///   The RectTransform's anchorMin/anchorMax should both be (0.5, 0.5) for
    ///   anchored-position based positioning.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class NodeUI : MonoBehaviour,
                          IPointerEnterHandler,
                          IPointerExitHandler,
                          IPointerClickHandler
    {
        // ── Visual state colours ──────────────────────────────────────────────────
        private static readonly Color ColorLocked    = new Color(0.35f, 0.35f, 0.35f, 1f); // grey
        private static readonly Color ColorAvailable = new Color(0.95f, 0.80f, 0.10f, 1f); // yellow
        private static readonly Color ColorUnlocked  = new Color(0.20f, 0.75f, 0.20f, 1f); // green

        // ── Inspector references ──────────────────────────────────────────────────
        [Header("Node Visuals")]
        /// <summary>Background Image whose tint colour reflects the current state.</summary>
        public Image background;

        /// <summary>Icon shown in the centre of the node (hidden when locked).</summary>
        public Image iconImage;

        /// <summary>Short name label rendered beneath or inside the node.</summary>
        public TMP_Text titleLabel;

        // ── Runtime ───────────────────────────────────────────────────────────────

        /// <summary>The data asset backing this UI element.</summary>
        public NodeData data { get; private set; }

        // Back-reference so click/hover can open the info panel.
        private UpgradesUIController controller;


        /// <summary>
        /// Called by UpgradesUIController immediately after this prefab is instantiated.
        /// Binds the node data and registers event listeners.
        /// </summary>
        public void Initialize(NodeData nodeData, UpgradesUIController ctrl)
        {
            data       = nodeData;
            controller = ctrl;

            if (titleLabel)              titleLabel.text = nodeData.title;
            if (iconImage && nodeData.icon) iconImage.sprite  = nodeData.icon;

            Refresh();

            EntrepreneurTreeManager.onNodeUnlocked += OnAnyNodeUnlocked;
            EntrepreneurTreeManager.onPointsChanged += OnPointsChanged;
        }


        /// <summary>
        /// Recalculate and apply the visual state for this node.
        /// Call this any time the node's state or the player's point balance may have changed.
        /// </summary>
        public void Refresh()
        {
            if (data == null) return;

            if (data.isUnlocked)
                ApplyState(ColorUnlocked);
            else if (EntrepreneurTreeManager.CanUnlockNode(data.id))
                ApplyState(ColorAvailable);
            else
                ApplyState(ColorLocked);
        }


        // ── IPointerEnterHandler ─────────────────────────────────────────────────

        public void OnPointerEnter(PointerEventData eventData)
        {
            controller?.ShowNodeInfo(data);
        }


        // ── IPointerExitHandler ──────────────────────────────────────────────────

        public void OnPointerExit(PointerEventData eventData)
        {
            controller?.HideNodeInfo();
        }


        // ── IPointerClickHandler ─────────────────────────────────────────────────

        public void OnPointerClick(PointerEventData eventData)
        {
            if (data == null) return;

            EntrepreneurTreeManager.TryUnlockNode(data.id);
            // Refresh happens via the onNodeUnlocked / onPointsChanged events below,
            // but calling it directly here removes one frame of visual delay.
            Refresh();
        }


        // ── Event callbacks ───────────────────────────────────────────────────────

        // Any node was unlocked — our own prerequisites may now be satisfied.
        private void OnAnyNodeUnlocked(NodeData _) => Refresh();

        // Point total changed — available status may have changed.
        private void OnPointsChanged(int total, int change) => Refresh();


        // ── Helpers ───────────────────────────────────────────────────────────────

        private void ApplyState(Color color)
        {
            if (background) background.color = color;

            // Hide the icon when the node is locked so it stays mysterious.
            if (iconImage) iconImage.gameObject.SetActive(color != ColorLocked);
        }


        void OnDestroy()
        {
            EntrepreneurTreeManager.onNodeUnlocked  -= OnAnyNodeUnlocked;
            EntrepreneurTreeManager.onPointsChanged -= OnPointsChanged;
        }
    }
}
