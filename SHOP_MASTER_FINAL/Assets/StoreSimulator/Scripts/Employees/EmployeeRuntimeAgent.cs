//Adaptado por POMPIC 20100333
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace FLOBUK.StoreSimulator
{
    public class EmployeeRuntimeAgent : MonoBehaviour
    {
        private string employeeId;
        private NavMeshAgent agent;
        private Coroutine workRoutine;
        private float baseAgentSpeed = -1f;

        public void Configure(string newEmployeeId)
        {
            employeeId = newEmployeeId;
            agent = GetComponent<NavMeshAgent>();
            if (agent != null && baseAgentSpeed < 0)
                baseAgentSpeed = agent.speed;
            if (workRoutine != null)
                StopCoroutine(workRoutine);

            workRoutine = StartCoroutine(WorkRoutine());
        }

        private IEnumerator WorkRoutine()
        {
            while (true)
            {
                ApplyCafeinaMovement();
                EmployeeManager manager = EmployeeManager.EnsureInstance();
                EmployeeRole role = manager.GetRole(employeeId);
                if (role == EmployeeRole.Cashier)
                    MoveTo(manager.GetAssignedWorkstationTransform(employeeId));
                else if (role == EmployeeRole.Restocker && manager.TryRestockOne(out string restockMessage, out Vector3 target))
                    MoveTo(target);
                else if (role == EmployeeRole.Restocker && UIGame.Instance != null)
                {
                    manager.TryRestockOne(out string fallbackMessage, out _);
                    UIGame.Instance.ShowMessage(fallbackMessage);
                }

                yield return new WaitForSeconds(5f / EntrepreneurProgress.EmployeeWorkSpeedMultiplier);
            }
        }

        private void ApplyCafeinaMovement()
        {
            if (agent != null && baseAgentSpeed > 0)
                agent.speed = baseAgentSpeed * EntrepreneurProgress.EmployeeWorkSpeedMultiplier;
        }

        private void MoveTo(Transform target)
        {
            if (target != null)
                MoveTo(target.position);
        }

        private void MoveTo(Vector3 target)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh)
                agent.SetDestination(target);
            else
                transform.position = target;
        }

        void OnDestroy()
        {
            if (workRoutine != null)
                StopCoroutine(workRoutine);
        }
    }
}
