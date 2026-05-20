using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Backend-only cashier automation.  It watches existing CashDesk instances on
    /// a low-frequency coroutine and lets hired Cashier employees complete one
    /// customer checkout each without replacing the manual player flow.
    /// </summary>
    public class EmployeeCashierCoordinator : MonoBehaviour
    {
        private const string LogPrefix = "[CashierAI] ";
        private const float PollIntervalSeconds = 0.75f;

        private readonly List<CashDesk> cashDesks = new List<CashDesk>();
        private Coroutine pollRoutine;

        public static EmployeeCashierCoordinator Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EntrepreneurEmployeeSystem.onEmployeeHired += OnEmployeeChanged;
            EntrepreneurEmployeeSystem.onEmployeeRoleChanged += OnEmployeeRoleChanged;
        }

        void OnEnable()
        {
            RefreshCashDesks();
            StartPolling();
        }

        void OnDisable()
        {
            StopPolling();
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            EntrepreneurEmployeeSystem.onEmployeeHired -= OnEmployeeChanged;
            EntrepreneurEmployeeSystem.onEmployeeRoleChanged -= OnEmployeeRoleChanged;
            if (Instance == this)
                Instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshCashDesks();
            StartPolling();
        }

        private void OnEmployeeChanged(int employeeId)
        {
            StartPolling();
        }

        private void OnEmployeeRoleChanged(int employeeId, EmployeeRole role)
        {
            StartPolling();
        }

        private void RefreshCashDesks()
        {
            cashDesks.Clear();
#if UNITY_2022_2_OR_NEWER
            CashDesk[] found = FindObjectsByType<CashDesk>(FindObjectsSortMode.None);
#else
            CashDesk[] found = FindObjectsOfType<CashDesk>();
#endif
            for (int i = 0; i < found.Length; i++)
            {
                if (found[i] != null)
                    cashDesks.Add(found[i]);
            }

            if (cashDesks.Count > 0)
                Debug.Log(LogPrefix + "Registered " + cashDesks.Count + " cash desk(s).");
        }

        private void StartPolling()
        {
            if (!isActiveAndEnabled || pollRoutine != null)
                return;

            pollRoutine = StartCoroutine(PollCashierWork());
        }

        private void StopPolling()
        {
            if (pollRoutine == null)
                return;

            StopCoroutine(pollRoutine);
            pollRoutine = null;
        }

        private IEnumerator PollCashierWork()
        {
            WaitForSeconds wait = new WaitForSeconds(PollIntervalSeconds);
            while (true)
            {
                int availableCashiers = GetAvailableCashierCount();
                if (availableCashiers > 0)
                {
                    if (cashDesks.Count == 0)
                        RefreshCashDesks();

                    int assigned = 0;
                    for (int i = 0; i < cashDesks.Count && assigned < availableCashiers; i++)
                    {
                        CashDesk desk = cashDesks[i];
                        if (desk == null)
                            continue;

                        if (desk.TryStartAutomatedCheckout(GetEmployeeSpeedMultiplier()))
                            assigned++;
                    }
                }

                yield return wait;
            }
        }

        private int GetAvailableCashierCount()
        {
            if (EntrepreneurEmployeeSystem.Instance == null)
                return 0;

            return EntrepreneurEmployeeSystem.Instance.GetRoleCount(EmployeeRole.Cashier);
        }

        private float GetEmployeeSpeedMultiplier()
        {
            float multiplier = 1f;
            if (EntrepreneurEmployeeSystem.Instance != null)
                multiplier *= EntrepreneurEmployeeSystem.Instance.GetCashierSpeedMultiplier();
            if (EntrepreneurTreeUpgradeAdapter.Instance != null)
                multiplier *= EntrepreneurTreeUpgradeAdapter.Instance.GetEmployeeSpeedMultiplier();

            return Mathf.Max(0.1f, multiplier);
        }
    }
}
