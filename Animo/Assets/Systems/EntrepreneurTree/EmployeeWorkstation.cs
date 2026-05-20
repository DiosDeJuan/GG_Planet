// Árbol del Emprendedor — EmployeeWorkstation
// Defines a physical workstation in the store that an employee NPC can be assigned to.
// Place this component on any GameObject that represents a work position (cash desk, restock point, etc.).
// The EmployeeWorkstationRegistry auto-discovers all instances in the scene.

using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    public enum EmployeeWorkstationType
    {
        Cashier   = 0,
        Restocker = 1,
        Security  = 2
    }

    /// <summary>
    /// Marks a scene location as a workstation that can be assigned to an employee.
    /// Auto-registers with <see cref="EmployeeWorkstationRegistry"/> on Awake.
    /// </summary>
    [DisallowMultipleComponent]
    public class EmployeeWorkstation : MonoBehaviour
    {
        private const string LogPrefix = "[Workstation] ";

        [Header("Configuration")]
        [Tooltip("Unique stable ID used for save/load. Auto-generated from name + instanceID if left empty.")]
        public string workstationId = string.Empty;

        [Tooltip("Type of work this station supports.")]
        public EmployeeWorkstationType workstationType = EmployeeWorkstationType.Cashier;

        [Tooltip("Optional point the NPC faces while working. Falls back to this transform.")]
        public Transform lookTarget;

        [Header("State (runtime)")]
        [SerializeField, HideInInspector]
        private int _occupiedByEmployeeId = -1;

        // ── Properties ────────────────────────────────────────────────────────────

        /// <summary>True when an employee has been assigned to this station.</summary>
        public bool IsOccupied => _occupiedByEmployeeId > 0;

        /// <summary>Employee ID currently assigned, or -1 if unoccupied.</summary>
        public int OccupiedByEmployeeId => _occupiedByEmployeeId;

        /// <summary>World position where the NPC should stand.</summary>
        public Vector3 StandPosition => transform.position;

        /// <summary>World direction the NPC should face (toward lookTarget if set).</summary>
        public Quaternion StandRotation
        {
            get
            {
                if (lookTarget != null)
                {
                    Vector3 dir = lookTarget.position - transform.position;
                    dir.y = 0f;
                    return dir.sqrMagnitude > 0.001f
                        ? Quaternion.LookRotation(dir.normalized)
                        : transform.rotation;
                }
                return transform.rotation;
            }
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        void Awake()
        {
            if (string.IsNullOrEmpty(workstationId))
                workstationId = name + "_" + GetInstanceID().ToString();

            EmployeeWorkstationRegistry.Register(this);
        }

        void OnDestroy()
        {
            EmployeeWorkstationRegistry.Unregister(this);
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>Assigns this station to an employee. Returns false if already occupied by another.</summary>
        public bool TryAssign(int employeeId)
        {
            if (IsOccupied && _occupiedByEmployeeId != employeeId)
            {
                Debug.LogWarning(LogPrefix + "Station '" + workstationId + "' already occupied by employee #" + _occupiedByEmployeeId);
                return false;
            }

            _occupiedByEmployeeId = employeeId;
            Debug.Log(LogPrefix + "Station '" + workstationId + "' assigned to employee #" + employeeId);
            return true;
        }

        /// <summary>Releases the workstation from its current occupant.</summary>
        public void Release()
        {
            if (_occupiedByEmployeeId > 0)
                Debug.Log(LogPrefix + "Station '" + workstationId + "' released from employee #" + _occupiedByEmployeeId);
            _occupiedByEmployeeId = -1;
        }

        /// <summary>
        /// Force-sets occupant (used during save/load restore to rebuild state without going through TryAssign logic).
        /// </summary>
        public void RestoreOccupant(int employeeId)
        {
            _occupiedByEmployeeId = employeeId;
        }

        // ── Editor gizmo ──────────────────────────────────────────────────────────

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Color c = workstationType == EmployeeWorkstationType.Cashier   ? new Color(0.25f, 0.55f, 1.00f, 0.85f)
                    : workstationType == EmployeeWorkstationType.Restocker ? new Color(0.25f, 0.80f, 0.30f, 0.85f)
                    :                                                         new Color(0.90f, 0.30f, 0.20f, 0.85f);
            Gizmos.color = c;
            Gizmos.DrawWireSphere(transform.position, 0.35f);
            if (lookTarget != null)
            {
                Gizmos.color = new Color(c.r, c.g, c.b, 0.45f);
                Gizmos.DrawLine(transform.position, lookTarget.position);
            }

            UnityEditor.Handles.color = c;
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.6f,
                "[" + workstationType.ToString().Substring(0, 2).ToUpper() + "] "
                + (IsOccupied ? "#" + _occupiedByEmployeeId : "libre"));
        }
#endif
    }
}
