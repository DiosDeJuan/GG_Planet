// Árbol del Emprendedor — TreeData
// ScriptableObject that holds the complete tree definition.
// Create ONE instance via: Create > EntrepreneurTree > TreeData
// Then assign all your NodeData assets to its "nodes" list in the Inspector.

using System.Collections.Generic;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Container ScriptableObject for the full Entrepreneur Tree.
    /// Assign the single instance to EntrepreneurTreeManager.treeData in the scene.
    ///
    /// HOW TO CREATE:
    ///   Right-click in Project → Create → EntrepreneurTree → TreeData
    ///   Then populate the "nodes" list with all your NodeData assets.
    /// </summary>
    [CreateAssetMenu(fileName = "EntrepreneurTreeData", menuName = "EntrepreneurTree/TreeData")]
    public class TreeData : ScriptableObject
    {
        /// <summary>Ordered list of every node that exists in the tree.</summary>
        public List<NodeData> nodes = new List<NodeData>();

        /// <summary>
        /// Returns the NodeData whose id matches, or null if none is found.
        /// </summary>
        public NodeData GetNodeById(string nodeId)
        {
            return nodes.Find(n => n.id == nodeId);
        }
    }
}
