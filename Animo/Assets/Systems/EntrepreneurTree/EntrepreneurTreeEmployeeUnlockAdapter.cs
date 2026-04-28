using System.Collections.Generic;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Tracks unlocked employee nodes and exposes a query API for employee-related systems.
    /// This adapter exposes unlocked employee state so it can be connected to a future real employee app/system.
    /// </summary>
    public class EntrepreneurTreeEmployeeUnlockAdapter : MonoBehaviour
    {
        // Este adapter expone el estado de empleados desbloqueados para conectarse con la futura app/sistema real de empleados.
        private const string LogPrefix = "[EntrepreneurTree] ";
        public static EntrepreneurTreeEmployeeUnlockAdapter Instance { get; private set; }

        private readonly HashSet<int> unlockedEmployees = new HashSet<int>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            EntrepreneurTreeManager.onEmployeeNodeUnlocked += OnEmployeeNodeUnlocked;
            SaveGameSystem.dataLoadEvent += OnDataLoaded;
        }


        void Start()
        {
            RefreshFromTree();
        }


        public bool IsEmployeeUnlocked(int employeeNumber)
        {
            return unlockedEmployees.Contains(employeeNumber);
        }


        public IReadOnlyCollection<int> GetUnlockedEmployees()
        {
            return unlockedEmployees;
        }


        private void OnEmployeeNodeUnlocked(NodeData node)
        {
            int employeeNumber = TryParseEmployeeNumber(node?.id);
            if (employeeNumber <= 0)
                return;

            unlockedEmployees.Add(employeeNumber);
            Debug.Log(LogPrefix + "Employee gameplay unlock applied: employee_" + employeeNumber);
        }


        private void OnDataLoaded()
        {
            RefreshFromTree();
            Debug.Log(LogPrefix + "Employee gameplay unlock state reapplied after load.");
        }


        private void RefreshFromTree()
        {
            unlockedEmployees.Clear();
            if (EntrepreneurTreeManager.Instance == null || EntrepreneurTreeManager.Instance.treeData == null)
                return;

            for (int i = 0; i < EntrepreneurTreeManager.Instance.treeData.nodes.Count; i++)
            {
                NodeData node = EntrepreneurTreeManager.Instance.treeData.nodes[i];
                if (node == null || node.nodeType != TreeNodeType.Employee || !node.isUnlocked)
                    continue;

                int employeeNumber = TryParseEmployeeNumber(node.id);
                if (employeeNumber > 0)
                    unlockedEmployees.Add(employeeNumber);
            }
        }


        private static int TryParseEmployeeNumber(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId) || !nodeId.StartsWith("employee_"))
                return 0;

            string suffix = nodeId.Substring("employee_".Length);
            return int.TryParse(suffix, out int value) ? value : 0;
        }


        void OnDestroy()
        {
            EntrepreneurTreeManager.onEmployeeNodeUnlocked -= OnEmployeeNodeUnlocked;
            SaveGameSystem.dataLoadEvent -= OnDataLoaded;

            if (Instance == this)
                Instance = null;
        }
    }
}
