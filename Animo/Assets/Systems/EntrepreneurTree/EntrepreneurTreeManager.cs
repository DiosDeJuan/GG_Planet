// Árbol del Emprendedor — EntrepreneurTreeManager
// Central manager: owns the progress-point pool, validates unlocks, broadcasts events.
// Attach to the Systems game object in the main Game scene (same one that holds UpgradeSystem, etc.)

using System;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Singleton manager for the Entrepreneur Tree system.
    ///
    /// Responsibilities:
    ///   • Track the player's current progress-point total.
    ///   • Validate and execute node unlocks (prerequisite check + cost deduction).
    ///   • Expose save/load helpers called by EntrepreneurTreeSaveIntegration.
    ///
    /// SCENE SETUP:
    ///   1. Add this component to any persistent GameObject in the Game scene (e.g. "Systems").
    ///   2. Assign your TreeData ScriptableObject to the "treeData" field in the Inspector.
    /// </summary>
    public class EntrepreneurTreeManager : MonoBehaviour
    {
        private const string LogPrefix = "[EntrepreneurTree] ";
        private const int SecurityLevel1Coverage = 33;
        private const int SecurityLevel2Coverage = 66;
        private const int SecurityLevel3Coverage = 99;
        /// <summary>Returns the singleton instance of this manager.</summary>
        public static EntrepreneurTreeManager Instance { get; private set; }

        /// <summary>
        /// Fired when a node is successfully unlocked.
        /// Passes the NodeData of the newly unlocked node.
        /// </summary>
        public static event Action<NodeData> onNodeUnlocked;
        public static event Action<NodeData> onProductNodeUnlocked;
        public static event Action<NodeData> onEmployeeNodeUnlocked;
        public static event Action<NodeData> onSecurityNodeUnlocked;
        public static event Action<NodeData> onUpgradeNodeUnlocked;

        /// <summary>
        /// Fired whenever the progress-point total changes (gain or spend).
        /// Parameters: (newTotal, change) — change is negative when spending points.
        /// </summary>
        public static event Action<int, int> onPointsChanged;

        /// <summary>
        /// The ScriptableObject that describes the full tree.
        /// Assign in the Inspector.
        /// </summary>
        public TreeData treeData;

        /// <summary>Current spendable progress-point balance.</summary>
        public int currentPoints { get; private set; }

        // Persisted set of unlocked node IDs.
        private HashSet<string> unlockedNodeIds = new HashSet<string>();


        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning(LogPrefix + "Duplicate EntrepreneurTreeManager detected. Destroying duplicate instance.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureDefaultUnlockedNodes();
        }


        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Award progress points to the player (e.g. from completing an achievement).
        /// Negative amounts are silently ignored.
        /// </summary>
        public static void AddPoints(int amount)
        {
            if (Instance == null || amount <= 0) return;

            Instance.currentPoints += amount;
            onPointsChanged?.Invoke(Instance.currentPoints, amount);
        }


        /// <summary>
        /// Attempt to unlock the node with the given id.
        /// Shows a UI message and returns false if prerequisites are not met or points are insufficient.
        /// Returns true and fires onNodeUnlocked on success.
        /// </summary>
        public static bool TryUnlockNode(string nodeId)
        {
            if (Instance == null) return false;

            NodeData node = Instance.treeData != null ? Instance.treeData.GetNodeById(nodeId) : null;
            if (node == null)
            {
                Debug.LogWarning(LogPrefix + "Unknown node unlock request: " + nodeId);
                UIGame.Instance?.ShowMessage("Unknown node: " + nodeId);
                return false;
            }

            if (node.isUnlocked)
            {
                UIGame.Instance?.ShowMessage(node.title + " is already unlocked.");
                return false;
            }

            List<string> missingNames = Instance.GetMissingRequirementNames(node);

            if (missingNames.Count > 0)
            {
                UIGame.Instance?.ShowMessage("Faltan requisitos: " + string.Join(", ", missingNames));
                return false;
            }

            // Check point balance.
            if (Instance.currentPoints < node.cost)
            {
                UIGame.Instance?.ShowMessage(
                    "No tienes suficientes puntos. Necesitas " + node.cost + ", tienes " + Instance.currentPoints + ".");
                return false;
            }

            // ── Perform unlock ────────────────────────────────────────────────────
            Instance.currentPoints -= node.cost;
            node.isUnlocked = true;
            Instance.unlockedNodeIds.Add(nodeId);

            onPointsChanged?.Invoke(Instance.currentPoints, -node.cost);
            onNodeUnlocked?.Invoke(node);
            Instance.DispatchTypedNodeUnlocked(node);
            UIGame.AddNotification("Unlocked: " + node.title, node.icon, Color.green);

            return true;
        }


        /// <summary>
        /// Returns true when the node is not yet unlocked, all prerequisites are met,
        /// and the player has enough points to pay its cost.
        /// </summary>
        public static bool CanUnlockNode(string nodeId)
        {
            if (Instance == null || Instance.treeData == null) return false;

            NodeData node = Instance.treeData.GetNodeById(nodeId);
            if (node == null || node.isUnlocked) return false;
            if (Instance.currentPoints < node.cost) return false;

            foreach (string reqId in node.requiredNodeIds)
            {
                NodeData req = Instance.treeData.GetNodeById(reqId);
                if (req == null || !req.isUnlocked) return false;
            }

            return true;
        }


        public static List<string> GetMissingRequirementNames(string nodeId)
        {
            if (Instance == null || Instance.treeData == null || string.IsNullOrEmpty(nodeId))
                return new List<string>();

            NodeData node = Instance.treeData.GetNodeById(nodeId);
            if (node == null)
                return new List<string>();

            return Instance.GetMissingRequirementNames(node);
        }


        public static int GetSecurityCoveragePercent()
        {
            if (Instance == null || Instance.treeData == null)
                return 0;

            if (Instance.IsNodeUnlocked("security_3"))
                return SecurityLevel3Coverage;
            if (Instance.IsNodeUnlocked("security_2"))
                return SecurityLevel2Coverage;
            if (Instance.IsNodeUnlocked("security_1"))
                return SecurityLevel1Coverage;
            return 0;
        }


        // ── Persistence ───────────────────────────────────────────────────────────

        /// <summary>
        /// Serialises the manager's runtime state to JSON.
        /// Called by EntrepreneurTreeSaveIntegration.
        /// </summary>
        public JSONNode SaveToJSON()
        {
            JSONNode data = new JSONObject();
            data["currentPoints"] = currentPoints;

            JSONArray unlockedArray = new JSONArray();
            foreach (string id in unlockedNodeIds)
                unlockedArray.Add(id);
            data["unlockedNodes"] = unlockedArray;

            return data;
        }


        /// <summary>
        /// Restores runtime state from a previously saved JSONNode.
        /// Called by EntrepreneurTreeSaveIntegration.
        /// </summary>
        public void LoadFromJSON(JSONNode data)
        {
            // Reset all node unlock flags first.
            if (treeData != null)
                foreach (NodeData node in treeData.nodes)
                    node.isUnlocked = false;

            unlockedNodeIds.Clear();
            currentPoints = 0;

            if (data == null || data.Count == 0)
            {
                EnsureDefaultUnlockedNodes();
                return;
            }

            currentPoints = data["currentPoints"].AsInt;

            JSONArray unlockedArray = data["unlockedNodes"].AsArray;
            for (int i = 0; i < unlockedArray.Count; i++)
            {
                string id = unlockedArray[i].Value;
                unlockedNodeIds.Add(id);

                NodeData node = treeData?.GetNodeById(id);
                if (node != null) node.isUnlocked = true;
            }

            if (unlockedNodeIds.Count == 0)
                EnsureDefaultUnlockedNodes();
        }




        private void EnsureDefaultUnlockedNodes()
        {
            if (treeData == null || treeData.nodes == null || treeData.nodes.Count == 0)
                return;

            if (unlockedNodeIds.Count > 0)
                return;

            for (int i = 0; i < treeData.nodes.Count; i++)
            {
                NodeData node = treeData.nodes[i];
                if (node == null || string.IsNullOrEmpty(node.id))
                    continue;

                if (node.id != EntrepreneurTreeDefinition.DefaultUnlockedNodeId)
                    continue;

                node.isUnlocked = true;
                unlockedNodeIds.Add(node.id);
                Debug.Log(LogPrefix + "Default node unlocked: " + node.id);
                return;
            }
        }


        private List<string> GetMissingRequirementNames(NodeData node)
        {
            List<string> missingNames = new List<string>();
            if (node == null || treeData == null || node.requiredNodeIds == null)
                return missingNames;

            for (int i = 0; i < node.requiredNodeIds.Count; i++)
            {
                string reqId = node.requiredNodeIds[i];
                NodeData req = treeData.GetNodeById(reqId);
                if (req == null || !req.isUnlocked)
                    missingNames.Add(req != null ? req.title : reqId);
            }

            return missingNames;
        }


        private bool IsNodeUnlocked(string nodeId)
        {
            NodeData node = treeData.GetNodeById(nodeId);
            return node != null && node.isUnlocked;
        }


        private void DispatchTypedNodeUnlocked(NodeData node)
        {
            if (node == null)
                return;

            switch (node.nodeType)
            {
                case TreeNodeType.Product:
                    onProductNodeUnlocked?.Invoke(node);
                    Debug.Log(LogPrefix + "Product node unlocked hook fired: " + node.id);
                    break;
                case TreeNodeType.Employee:
                    onEmployeeNodeUnlocked?.Invoke(node);
                    Debug.Log(LogPrefix + "Employee node unlocked hook fired: " + node.id);
                    break;
                case TreeNodeType.Security:
                    onSecurityNodeUnlocked?.Invoke(node);
                    Debug.Log(LogPrefix + "Security node unlocked hook fired: " + node.id + " (" + GetSecurityCoveragePercent() + "%)");
                    break;
                case TreeNodeType.Improvement:
                    onUpgradeNodeUnlocked?.Invoke(node);
                    Debug.Log(LogPrefix + "Upgrade node unlocked hook fired: " + node.id);
                    break;
            }
        }

        void OnDestroy()
        {
            // Reset ScriptableObject runtime flags when leaving play mode in the editor
            // so stale data does not persist across edit sessions.
#if UNITY_EDITOR
            if (treeData != null)
                foreach (NodeData node in treeData.nodes)
                    node.isUnlocked = false;
#endif

            if (Instance == this)
                Instance = null;
        }
    }
}
