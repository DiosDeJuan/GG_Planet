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
        private static readonly Color ColorLocked    = new Color(0.22f, 0.22f, 0.24f, 0.85f);
        private static readonly Color RootHighlight  = new Color(0.95f, 0.82f, 0.20f, 1f);

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
                ApplyState(true, true);
            else if (EntrepreneurTreeManager.CanUnlockNode(data.id))
                ApplyState(false, true);
            else
                ApplyState(false, false);
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

        private void ApplyState(bool unlocked, bool available)
        {
            Color typeColor = GetTypeColor(data != null ? data.nodeType : TreeNodeType.Product);
            Color fillColor = ColorLocked;
            if (unlocked)
                fillColor = data != null && data.id == EntrepreneurTreeDefinition.DefaultUnlockedNodeId ? RootHighlight : typeColor;
            else if (available)
                fillColor = Color.Lerp(typeColor, Color.white, 0.28f);

            if (background) background.color = fillColor;
            if (titleLabel)
            {
                string statusPrefix = unlocked ? "[OK] " : available ? "[LISTO] " : "[BLOQ] ";
                titleLabel.text = (data != null ? statusPrefix + data.title : titleLabel.text);
                titleLabel.color = unlocked || available ? Color.white : new Color(0.85f, 0.85f, 0.85f, 0.85f);
            }

            // Hide the icon when the node is locked so it stays mysterious.
            if (iconImage) iconImage.gameObject.SetActive(unlocked || available);
        }

        private static Color GetTypeColor(TreeNodeType type)
        {
            switch (type)
            {
                case TreeNodeType.Product:
                    return new Color(0.13f, 0.63f, 0.52f, 1f);
                case TreeNodeType.Employee:
                    return new Color(0.19f, 0.42f, 0.80f, 1f);
                case TreeNodeType.Security:
                    return new Color(0.82f, 0.37f, 0.16f, 1f);
                case TreeNodeType.Improvement:
                    return new Color(0.55f, 0.30f, 0.75f, 1f);
                default:
                    return new Color(0.45f, 0.45f, 0.45f, 1f);
            }
        }


        void OnDestroy()
        {
            EntrepreneurTreeManager.onNodeUnlocked  -= OnAnyNodeUnlocked;
            EntrepreneurTreeManager.onPointsChanged -= OnPointsChanged;
        }
    }
}
