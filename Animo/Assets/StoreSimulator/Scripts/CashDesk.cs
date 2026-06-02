/*  This file is part of the "Store Simulator" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store, Epic FAB).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */
/*  Adaptado por Isaac Victoria. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Allows processing customer checkouts by card or cash manually.
    /// Customers place their items on it and wait in line.
    /// </summary>
    public class CashDesk : CheckoutObject
    {
        [Header("CashDesk")]
        /// <summary>
        /// The initial position and rotation the player camera should move to when entering this object.
        /// </summary>
        public Transform lookTransform;

        /// <summary>
        /// The clip to play when the cash register opens, or none if not set.
        /// </summary>
        public AudioClip registerClip;

        /// <summary>
        /// The clip to play when charging a customer failed, or none if not set.
        /// </summary>
        public AudioClip failureClip;

        /// <summary>
        /// Reference to the UI terminal component for customers paying by card.
        /// </summary>
        public UICashDeskTerminal terminal;

        /// <summary>
        /// Reference to the UI cash register component for customers paying with cash.
        /// </summary>
        public UICashDeskRegister register;

        /// <summary>
        /// Whether this object is currently controlled by the player.
        /// </summary>
        public bool isPlayerControlled { get; private set; }

        /// <summary>
        /// True while an employee cashier is processing the current customer.
        /// </summary>
        public bool isAutomatedCheckoutInProgress { get; private set; }

        //previous camera position that should be transitioned back to when leaving
        private Vector3 prevCamPosition;
        //previous camera rotation that should be transitioned back to when leaving
        private Quaternion prevCamRotation;
        //reference to colliders used for detecting an interaction
        private Collider[] cols;
        //reference to Animation component
        private Animation anim;
        private Coroutine automatedCheckoutRoutine;
        private bool checkoutAttendedByCashier;


        //initialize references
        void Awake()
        {
            cols = GetComponents<Collider>();
            anim = GetComponent<Animation>();

            terminal.onInputConfirmed += OnBillCustomer;
            register.onInputConfirmed += OnBillCustomer;
        }


        /// <summary>
        /// CheckoutObject override, adding new customers to the end of the queue.
        /// </summary>
        public override (Transform, int) AddCustomerToQueue(Customer customer)
        {
            if (customerQueue.Count == queuePositions.childCount)
                return (null, -1);

            customerQueue.Add(customer);
            return (queuePositions.GetChild(customerQueue.Count - 1), customerQueue.Count);
        }


        /// <summary>
        /// CheckoutObject override, getting last world position of queue positions.
        /// </summary>
        public override Vector3 GetLastQueuePosition()
        {
            return queuePositions.GetChild(queuePositions.childCount - 1).position;
        }


        /// <summary>
        /// CheckoutObject override, places all items from the "shopping" bag on this desk.
        /// </summary>
        public override void PlaceBagContents(CustomerCart bag)
        {
            //a reference is stored to not call this multiple times
            if (customerBag != null)
                return;

            customerBag = bag;
            List<CustomerBagItem> items = bag.items;

            if (!isPlayerControlled)
                UIGame.AddNotification("Customer waiting at Cash Desk");

            int itemIndex = 0;
            for (int i = 0; i < items.Count; i++)
            {
                for (int j = 0; j < items[i].count; j++)
                {
                    Transform child = conveyorPositions.GetChild(itemIndex);
                    GameObject bagItem = Instantiate(items[i].product.prefab, child.position, Quaternion.identity, child);

                    CheckoutItem deskItem = bagItem.AddComponent<CheckoutItem>();
                    deskItem.Initialize(items[i], this);
                    deskItems.Add(deskItem);

                    itemIndex++;
                }
            }

            if (customerQueue.Count > 0)
                customerQueue[0].NotifyCheckoutWaitingForService(this);
        }
        

        /// <summary>
        /// CheckoutObject override, do scanning and check for last item for initiating payment.
        /// </summary>
        public override void Scan(CheckoutItem item)
        {
            if (customerQueue.Count > 0)
                customerQueue[0].NotifyCheckoutServiceStarted();

            cart.Add(item);
            deskItems.Remove(item);
            AudioSystem.Play3D(scanClip, conveyorPositions.position);

            //this was the last item available, do payment
            if (deskItems.Count == 0)
                customerQueue[0].ProceedPayment(true);

            StartCoroutine(DestroyItem(item.transform));
        }


        /// <summary>
        /// CheckoutObject override, activates terminal or cash register based on customer preference.
        /// </summary>
        public override void ActivateCheckout()
        {
            if (customerQueue[0].payCash)
            {
                register.Initialize(cart.total.text);
                register.SetInteractable(true);

                AudioSystem.Play3D(registerClip, register.cashParent.position);
                anim.Play("CashRegister_Open");
            } 
            else terminal.SetInteractable(true);
        }


        /// <summary>
        /// CheckoutObject override, finish the customer by comparing billed amount with total due amount.
        /// Incorrect change plays a failed sound. Once billed the customer then leaves the queue for the next one.
        /// </summary>
        protected override void OnBillCustomer(string amount)
        {
            long billAmount = StoreDatabase.FromStringToLongMoney(amount);
            long cartAmount = StoreDatabase.FromStringToLongMoney(cart.total.text);

            //checkout mismatch
            if (billAmount != cartAmount)
            {
                bool checkoutError = false;

                switch (customerQueue[0].payCash)
                {
                    //we gave less change than required
                    case true:
                        if (billAmount > cartAmount)
                        {
                            register.given.color = Color.red;
                            checkoutError = true;
                        }
                        break;

                    //card checkouts need to match
                    case false:
                        terminal.input.GetComponent<Image>().color = new Color32(255, 99, 71, 255);
                        checkoutError = true;
                        break;
                }

                if (checkoutError)
                {
                    AudioSystem.Play2D(failureClip);
                    return;
                }
            }

            if (customerBag != null)
            {
                for (int i = 0; i < customerBag.items.Count; i++)
                {
                    CustomerBagItem sold = customerBag.items[i];
                    if (sold == null || sold.product == null)
                        continue;

                    if (ProductPurchaseProbabilityAdapter.Instance != null)
                        ProductPurchaseProbabilityAdapter.Instance.FireSaleHooks(sold.product, sold.fixedPrice);
                    StatsDatabase.RegisterSoldProduct(sold.product, sold.count);
                    if (sold.fixedPrice == 0)
                        StatsDatabase.RegisterZeroPriceSale(sold.product);
                }
            }

            cart.Clear();
            bool cashierHandledSale = checkoutAttendedByCashier || isAutomatedCheckoutInProgress;
            if (EntrepreneurTreeUpgradeAdapter.Instance != null)
                EntrepreneurTreeUpgradeAdapter.Instance.CreditSaleIncome(billAmount, cashierHandledSale);
            else
                StoreDatabase.AddRemoveMoney(billAmount);
            AudioSystem.Play2D(successClip);

            if (customerQueue[0].payCash)
            {
                if (billAmount > cartAmount)
                    customerQueue[0].ShowUnhappy("Overcharged!");

                register.SetInteractable(false);
                anim.Play("CashRegister_Close");
            }
            else
            {
                terminal.input.GetComponent<Image>().color = Color.white;
                if (terminal.isPlayerControlled)
                    terminal.Exit(true);
                else
                    terminal.SetInteractable(false);
            }

            customerBag = null;
            checkoutAttendedByCashier = false;
            customerQueue[0].GoHome();
            customerQueue.RemoveAt(0);
            StoreDatabase.AddRemoveExperience(3);

            for (int i = 0; i < customerQueue.Count; i++)
            {
                customerQueue[i].ProceedQueue(queuePositions.GetChild(i), i + 1);
            }
        }


        /// <summary>
        /// Called by EmployeeCashierCoordinator.  Starts a backend checkout flow only
        /// when the desk is idle for player control and a customer has already placed
        /// products on the conveyor.
        /// </summary>
        public bool TryStartAutomatedCheckout(float speedMultiplier)
        {
            if (isPlayerControlled || isAutomatedCheckoutInProgress || customerQueue.Count == 0)
                return false;
            if ((terminal != null && terminal.IsInteractable()) || (register != null && register.IsInteractable()))
                return false;
            if (customerBag == null || deskItems.Count == 0)
                return false;

            automatedCheckoutRoutine = StartCoroutine(AutomatedCheckout(speedMultiplier));
            return true;
        }


        public bool TryCancelWaitingCustomer(Customer customer)
        {
            if (customer == null || customerQueue.Count == 0 || customerQueue[0] != customer)
                return false;
            if (isPlayerControlled || isAutomatedCheckoutInProgress)
                return false;

            Debug.Log("[CustomerWait] Customer left checkout after waiting too long.");

            if (customerBag != null)
                customerBag.RestoreItemsToShelves();

            for (int i = deskItems.Count - 1; i >= 0; i--)
            {
                if (deskItems[i] != null)
                    Destroy(deskItems[i].gameObject);
            }
            deskItems.Clear();
            cart.Clear();
            customerBag = null;

            customer.ShowUnhappy("Waited too long at checkout.");
            customer.GoHome();
            customerQueue.RemoveAt(0);

            for (int i = 0; i < customerQueue.Count; i++)
                customerQueue[i].ProceedQueue(queuePositions.GetChild(i), i + 1);

            return true;
        }


        private IEnumerator AutomatedCheckout(float speedMultiplier)
        {
            isAutomatedCheckoutInProgress = true;
            speedMultiplier = Mathf.Max(0.1f, speedMultiplier);

            Customer customer = customerQueue.Count > 0 ? customerQueue[0] : null;
            if (customer != null)
                customer.NotifyCheckoutServiceStarted();

            int productCount = Mathf.Max(1, deskItems.Count);
            float scanDuration = productCount * 0.5f;
            float paymentDuration = customer != null && customer.payCash ? 2.5f : 1.5f;
            float totalDuration = Mathf.Clamp((scanDuration + paymentDuration) / speedMultiplier, 1f, 12f);
            float perItemDelay = Mathf.Max(0.05f, (totalDuration - paymentDuration / speedMultiplier) / productCount);

            Debug.Log("[CashierAI] Serving customer at " + name + " in " + totalDuration.ToString("0.0") + "s.");

            while (deskItems.Count > 0)
            {
                CheckoutItem item = deskItems[0];
                if (item != null)
                    Scan(item);
                yield return new WaitForSeconds(perItemDelay);
            }

            yield return new WaitForSeconds(Mathf.Max(0.05f, paymentDuration / speedMultiplier));

            if (customerQueue.Count > 0)
                customerQueue[0].PausePayment();

            try
            {
                if (customerQueue.Count > 0 && customerBag != null)
                {
                    checkoutAttendedByCashier = true;
                    OnBillCustomer(cart.total.text);
                }
            }
            finally
            {
                checkoutAttendedByCashier = false;
                isAutomatedCheckoutInProgress = false;
                automatedCheckoutRoutine = null;
            }
        }


        /// <summary>
        /// Interactable override, adding UI action.
        /// </summary>
        public override void OnBecameFocus()
        {
            UIGame.AddAction("LeftClick", "Use", true);
        }


        /// <summary>
        /// Interactable override, react on player interaction.
        /// This object can be "controlled" by the player.
        /// </summary>
        public override bool Interact(string actionName)
        {
            if (actionName != "LeftClick") return false;
            if (isAutomatedCheckoutInProgress)
            {
                UIGame.Instance?.ShowMessage("Un cajero ya está atendiendo esta caja.");
                return false;
            }

            PlayerInput.GetPlayerByIndex(0).onActionTriggered += OnAction;
            UIGame.AddAction("Esc", "Exit");

            isPlayerControlled = true;
            if (customerQueue.Count > 0)
                customerQueue[0].NotifyCheckoutServiceStarted();

            PlayerController.SetMovementState(MovementState.None, false);

            for(int i = 0; i < cols.Length; i++)
                cols[i].enabled = false;

            StartCoroutine(Enter());
            return true;
        }


        /// <summary>
        /// Interactable override, removing UI action.
        /// </summary>
        public override void OnLostFocus()
        {
            UIGame.RemoveAction("LeftClick");
        }


        /// <summary>
        /// Cancel controlling this object and return to player movement mode.
        /// </summary>
        public void Exit()
        {
            PlayerInput.GetPlayerByIndex(0).onActionTriggered -= OnAction;
            UIGame.RemoveAction("Esc");

            //in case we left during checkout
            if (terminal.isPlayerControlled)
            {
                terminal.Exit();
            }
            if (register.IsInteractable())
            {
                register.SetInteractable(false);
                anim.Play("CashRegister_Close");
            }

            //if there are any items placed already, disable them
            for (int i = 0; i < deskItems.Count; i++)
                deskItems[i].SetInteractable(false);

            //pause checkout for the current customer
            if (customerQueue.Count > 0)
                customerQueue[0].PausePayment();

            isPlayerControlled = false;
            Transform camTransform = PlayerController.GetCameraTransform();
            InteractionSystem.MoveToTargetLinear(camTransform, null, prevCamPosition, prevCamRotation, lerpSpeed, true);

            Invoke("ReEnable", 0.5f);
        }


        //react on user input
        private void OnAction(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                switch (context.action.name)
                {
                    case "Cancel":
                        Exit();
                        break;
                }
            }
        }


        //move item across the desk and destroy it at the end
        private IEnumerator DestroyItem(Transform item)
        {
            yield return InteractionSystem.MoveToTargetLinear(item, null, conveyorEndpoint.position, item.rotation, lerpSpeed, false);
            Destroy(item.gameObject);
        }


        //transition the player to the initial camera position and rotation (lookTransform)
        //if a customer is already waiting for checkout, that payment workflow is being continued
        private IEnumerator Enter()
        {
            Transform camTransform = PlayerController.GetCameraTransform();
            prevCamPosition = camTransform.localPosition;
            prevCamRotation = camTransform.localRotation;

            yield return InteractionSystem.MoveToTargetLinear(camTransform, null, lookTransform.position, Quaternion.LookRotation(lookTransform.forward), lerpSpeed, false);

            PlayerController.SetCameraRotation(camTransform.localRotation);
            PlayerController.SetMovementState(MovementState.RotationOnly, true);

            for(int i = 0; i < deskItems.Count; i++)
                deskItems[i].SetInteractable(true);

            if (customerBag != null && deskItems.Count == 0)
                customerQueue[0].ProceedPayment(true);
        }


        //after exiting the controlled state, player movement is re-enabled with a short delay
        private void ReEnable()
        {
            PlayerController.SetCameraRotation(prevCamRotation);
            PlayerController.SetMovementState(MovementState.All, true);

            for(int i = 0; i < cols.Length; i++)
                cols[i].enabled = true;
        }


        //unsubscribe from events
        void OnDestroy()
        {
            terminal.onInputConfirmed -= OnBillCustomer;
            register.onInputConfirmed -= OnBillCustomer;

            if (automatedCheckoutRoutine != null)
                StopCoroutine(automatedCheckoutRoutine);
        }
    }
}
