// Árbol del Emprendedor — ConnectionLineUI
// Draws a thin rectangular UI line between two node RectTransforms.
// One instance is created per dependency edge by UpgradesUIController.

using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Renders a thin UI line connecting two node elements in the scrollable tree canvas.
    ///
    /// The line is drawn by rotating and stretching an Image to span from the centre of
    /// the "from" node (prerequisite) to the centre of the "to" node (dependent).
    ///
    /// Colour:
    ///   Dim grey  = at least one endpoint is not yet unlocked.
    ///   Bright green = both endpoints are unlocked.
    ///
    /// PREFAB SETUP:
    ///   Create a UI GameObject with an Image component.
    ///   Set Image colour to white — this script overrides it at runtime.
    ///   Set pivot to (0.5, 0.5) and anchor to (0.5, 0.5) so anchoredPosition-based
    ///   placement works correctly within the scroll content RectTransform.
    ///   Attach this script.
    ///   Make sure the linePrefab's RectTransform sibling order places it below nodes
    ///   (UpgradesUIController calls SetAsFirstSibling on instantiation).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ConnectionLineUI : MonoBehaviour
    {
        /// <summary>Thickness of the line in pixels. Adjust in the Inspector.</summary>
        public float lineThickness = 4f;

        private static readonly Color ColorLocked   = new Color(0.46f, 0.46f, 0.46f, 0.78f);
        private static readonly Color ColorUnlocked = new Color(0.20f, 0.75f, 0.20f, 0.80f);

        private Image       lineImage;
        private NodeUI      fromNode;   // prerequisite end
        private NodeUI      toNode;     // dependent end


        void Awake()
        {
            lineImage = GetComponent<Image>();
        }


        /// <summary>
        /// Bind this line to the two NodeUI instances it connects.
        /// Must be called once after instantiation by UpgradesUIController.
        /// </summary>
        public void Initialize(NodeUI from, NodeUI to)
        {
            fromNode = from;
            toNode   = to;

            UpdateGeometry();
            UpdateColor();

            EntrepreneurTreeManager.onNodeUnlocked += OnNodeUnlocked;
        }


        /// <summary>
        /// Recalculate position, rotation and length to span between the two node centres.
        /// Call if a node's anchoredPosition changes at runtime.
        /// </summary>
        public void UpdateGeometry()
        {
            if (fromNode == null || toNode == null) return;

            RectTransform fromRT = fromNode.GetComponent<RectTransform>();
            RectTransform toRT   = toNode.GetComponent<RectTransform>();
            RectTransform lineRT = GetComponent<RectTransform>();

            Vector2 a = fromRT.anchoredPosition;
            Vector2 b = toRT.anchoredPosition;

            // Centre the line between the two nodes.
            lineRT.anchoredPosition = (a + b) * 0.5f;

            // Stretch to cover the full distance.
            float length = Vector2.Distance(a, b);
            lineRT.sizeDelta = new Vector2(length, lineThickness);

            // Rotate toward the target node.
            Vector2 direction = b - a;
            float   angle     = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            lineRT.localRotation = Quaternion.Euler(0f, 0f, angle);
        }


        // ── Helpers ───────────────────────────────────────────────────────────────

        private void UpdateColor()
        {
            if (lineImage == null) return;

            bool lit = fromNode != null && fromNode.data != null && fromNode.data.isUnlocked
                    && toNode   != null && toNode.data   != null && toNode.data.isUnlocked;

            lineImage.color = lit ? ColorUnlocked : ColorLocked;
        }


        private void OnNodeUnlocked(NodeData _) => UpdateColor();


        void OnDestroy()
        {
            EntrepreneurTreeManager.onNodeUnlocked -= OnNodeUnlocked;
        }
    }
}
