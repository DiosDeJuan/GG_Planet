//Adaptado por POMPIC 20100333
using System.Collections;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    public class ShoplifterAgent : Interactable
    {
        private SecurityManager manager;
        private Customer customer;
        private CustomerAgent customerAgent;
        private Animator animator;
        private PlacementObject stolenPlacement;
        private ProductScriptableObject stolenProduct;
        private long stolenValue;
        private bool hasStolen;
        private bool isResolved;

        public ShoplifterType Type { get; private set; }

        public void Configure(SecurityManager securityManager, Customer attachedCustomer, ShoplifterType shoplifterType)
        {
            manager = securityManager;
            customer = attachedCustomer;
            Type = shoplifterType;
            customerAgent = GetComponent<CustomerAgent>();
            animator = GetComponent<Animator>();
            ConfigureColliderProxies();
            StartCoroutine(TheftRoutine());
        }

        public override void OnBecameFocus()
        {
            if (!hasStolen || isResolved)
                return;

            UIGame.AddAction("LeftClick", "Detener", true);
        }

        public override bool Interact(string actionName)
        {
            if (actionName != "LeftClick")
                return false;

            if (!hasStolen || isResolved)
            {
                ShowMessage("Sin evidencia de robo.");
                return false;
            }

            Arrest(false);
            return true;
        }

        public override void OnLostFocus()
        {
            UIGame.RemoveAction("LeftClick");
        }

        private IEnumerator TheftRoutine()
        {
            yield return new WaitForSeconds(Random.Range(6f, 14f));

            if (customer == null || isResolved || DayCycleSystem.GetStoreOpenState() == StoreOpenState.Closed)
                yield break;

            stolenPlacement = manager.FindTargetPlacement(Type);
            if (stolenPlacement == null || stolenPlacement.IsEmpty())
                yield break;

            if (customer.currentStep == CustomerStep.Queue || customer.currentStep == CustomerStep.Pay || customer.currentStep == CustomerStep.GoHome)
                yield break;

            Transform grabSpot = stolenPlacement.grabSpot;
            if (customerAgent != null && grabSpot != null)
            {
                customerAgent.SetDestination(grabSpot.position, false);
                yield return new WaitForSeconds(Random.Range(2.5f, 4.5f));
            }

            StealOneProduct();
            if (!hasStolen)
                yield break;

            manager.OnTheftDetected(this, stolenProduct, stolenValue);
            customer.ShowUnhappy("Robo detectado.");
            SendCustomerTowardExit();
            yield return new WaitForSeconds(0.5f);

            if (!manager.TryAutomaticArrest(this))
                StartCoroutine(EscapeRoutine());
        }

        private void StealOneProduct()
        {
            if (stolenPlacement == null || stolenPlacement.IsEmpty())
                return;

            stolenProduct = stolenPlacement.product;
            stolenValue = stolenProduct != null ? stolenProduct.storePrice : 0;
            Transform stolenItem = stolenPlacement.Remove();
            if (stolenItem != null)
                Destroy(stolenItem.gameObject);

            hasStolen = stolenProduct != null;
            if (animator != null)
                animator.Play("Grab");
        }

        private IEnumerator EscapeRoutine()
        {
            yield return new WaitForSeconds(manager.ManualInterventionWindowSeconds);

            if (isResolved)
                yield break;

            isResolved = true;
            manager.RegisterEscapedTheft(stolenValue);
            ShowMessage("Ladron escapo con mercancia.");
            if (customer != null)
                customer.GoHome();
        }

        public void Arrest(bool automatic)
        {
            if (isResolved)
                return;

            isResolved = true;
            RestoreProduct();
            manager.RegisterArrest(stolenValue, automatic);
            ShowMessage(automatic ? "Seguridad detuvo a un ladron." : "Ladron detenido.");
            if (customer != null)
                customer.GoHome();
        }

        private void RestoreProduct()
        {
            if (stolenPlacement == null || stolenProduct == null)
                return;

            if (!stolenPlacement.IsPlaceable(stolenProduct))
                return;

            Vector3 localPosition = stolenPlacement.Add(stolenProduct);
            Quaternion worldRotation = stolenPlacement.transform.rotation * Quaternion.Euler(0, stolenPlacement.orientation, 0);
            Vector3 worldPosition = stolenPlacement.container.TransformPoint(localPosition);
            Instantiate(stolenProduct.prefab, worldPosition, worldRotation, stolenPlacement.container);
        }

        private void SendCustomerTowardExit()
        {
            if (customerAgent != null && StoreDatabase.Instance != null && StoreDatabase.Instance.storeEntry != null)
                customerAgent.SetDestination(StoreDatabase.Instance.storeEntry.position, false);
        }

        private void ConfigureColliderProxies()
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                GameObject obj = colliders[i].gameObject;
                if (obj == gameObject || obj.GetComponent<Interactable>() != null)
                    continue;

                ShoplifterInteractableProxy proxy = obj.AddComponent<ShoplifterInteractableProxy>();
                proxy.Configure(this);
            }
        }

        private void ShowMessage(string text)
        {
            if (UIGame.Instance != null)
                UIGame.Instance.ShowMessage(text);
        }
    }
}
