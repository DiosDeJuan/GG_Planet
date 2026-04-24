// Árbol del Emprendedor — NodeData
// ScriptableObject that describes a single node in the Entrepreneur Tree.
// Create assets via: Create > EntrepreneurTree > NodeData

using System.Collections.Generic;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// The category a tree node belongs to.
    /// </summary>
    public enum TreeNodeType
    {
        Product,
        Employee,
        Security,
        Improvement
    }

    /// <summary>
    /// Defines one node in the Entrepreneur Tree (product, employee, security level or improvement).
    /// Each instance is a ScriptableObject asset stored under Assets/Data/.
    ///
    /// HOW TO CREATE:
    ///   Right-click in the Project window → Create → EntrepreneurTree → NodeData
    ///   Fill in the fields and assign it to your TreeData asset's "nodes" list.
    /// </summary>
    [CreateAssetMenu(fileName = "NodeData", menuName = "EntrepreneurTree/NodeData")]
    public class NodeData : ScriptableObject
    {
        [Header("Identity")]
        /// <summary>Unique string identifier – used for dependency checks and save/load.</summary>
        public string id;

        /// <summary>Category this node belongs to (product / employee / security / improvement).</summary>
        public TreeNodeType nodeType;

        [Header("Display")]
        /// <summary>Display name shown inside the tree UI.</summary>
        public string title;

        /// <summary>Short description of what this node unlocks or improves.</summary>
        [TextArea(2, 5)]
        public string description;

        /// <summary>Optional icon displayed on the node button.</summary>
        public Sprite icon;

        [Header("Cost & Prerequisites")]
        /// <summary>Number of progress points the player must spend to unlock this node.</summary>
        public int cost = 1;

        /// <summary>
        /// IDs of nodes that must already be unlocked before this one becomes available.
        /// Leave empty for root nodes.
        /// </summary>
        public List<string> requiredNodeIds = new List<string>();

        [Header("Tree Layout")]
        /// <summary>
        /// Pixel position of this node's centre inside the scrollable tree canvas.
        /// Adjust these values in the Inspector to arrange the visual tree layout.
        /// Positive X = right, positive Y = up.
        /// </summary>
        public Vector2 uiPosition;

        // ── Runtime state ────────────────────────────────────────────────────────
        // NonSerialized so the value is never baked into the asset file itself.
        // EntrepreneurTreeManager sets this at load time and at runtime on unlock.

        /// <summary>
        /// Whether this node has been unlocked by the player in the current session.
        /// Populated at runtime by EntrepreneurTreeManager – not persisted to the asset.
        /// </summary>
        [System.NonSerialized]
        public bool isUnlocked = false;
    }
}
