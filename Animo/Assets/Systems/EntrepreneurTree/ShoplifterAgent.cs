using System.Collections.Generic;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    public enum ShoplifterType
    {
        Common = 0,
        Suspicious = 1,
        Expert = 2,
        Fast = 3,
        Special = 4
    }

    /// <summary>
    /// Runtime marker/behavior for a customer selected as thief.
    /// </summary>
    public class ShoplifterAgent : MonoBehaviour
    {
        private readonly List<RobbedItem> reservedItems = new List<RobbedItem>();
        private IRobberyInventoryBridge inventoryBridge;

        public ShoplifterType thiefType { get; private set; }
        public bool isEscaping { get; private set; }
        public bool isResolved { get; private set; }
        public long stolenValue { get; private set; }
        public int stolenProductsCount { get; private set; }
        public IReadOnlyList<RobbedItem> stolenItems => reservedItems;

        private Customer owner;
        private ShoplifterSystem system;
        private ShoplifterInteractable interactable;
        private bool initialized;
        private bool theftStarted;
        private float baseSpeed;

        public void Initialize(ShoplifterSystem sourceSystem, Customer customer, ShoplifterType type)
        {
            if (initialized)
                return;

            system = sourceSystem;
            owner = customer;
            thiefType = type;
            inventoryBridge = new EntrepreneurTreeRobberyInventoryBridge();
            interactable = GetComponent<ShoplifterInteractable>();
            if (interactable == null)
                interactable = gameObject.AddComponent<ShoplifterInteractable>();
            interactable.Initialize(this);

            baseSpeed = GetCurrentMovementSpeed();
            ApplyVisualMarker();
            ApplySpeedModifier();
            initialized = true;
        }


        public Customer GetCustomer()
        {
            return owner;
        }


        public bool TryStartTheft(CustomerCart cart)
        {
            if (!initialized || owner == null || theftStarted || isResolved)
                return false;

            theftStarted = true;
            CalculateStolenValues(cart);
            system.NotifyThiefDetected(this);

            if (system.TryAutomaticArrest(this))
            {
                Resolve(autoArrest: true, escaped: false);
                owner.GoHome();
                return true;
            }

            isEscaping = true;
            interactable.SetInteractable(true);
            UIGame.AddNotification("Seguridad falló. Intervén manualmente.", otherColor: new Color(1f, 0.45f, 0.15f));
            owner.GoHome();
            return true;
        }


        public bool TryManualCapture()
        {
            if (!isEscaping || isResolved)
                return false;

            Resolve(autoArrest: false, escaped: false);
            return true;
        }


        public void ResolveAsEscaped()
        {
            if (!isEscaping || isResolved)
                return;

            Resolve(autoArrest: false, escaped: true);
        }


        public bool CanBeCaptured()
        {
            return isEscaping && !isResolved;
        }


        public bool TryRestoreInventory(out int restoredCount, out long restoredValue)
        {
            restoredCount = 0;
            restoredValue = 0;
            if (inventoryBridge == null || reservedItems.Count == 0)
                return false;

            return inventoryBridge.RestoreStolenItems(reservedItems, out restoredCount, out restoredValue);
        }


        public bool ConfirmInventoryLoss()
        {
            if (inventoryBridge == null || reservedItems.Count == 0)
                return false;

            return inventoryBridge.ConfirmStolenItems(reservedItems);
        }


        private void Resolve(bool autoArrest, bool escaped)
        {
            isResolved = true;
            isEscaping = false;
            if (interactable != null)
                interactable.SetInteractable(false);

            if (autoArrest)
            {
                system.NotifyAutomaticArrest(this);
                return;
            }

            if (escaped)
            {
                system.NotifyEscaped(this);
                return;
            }

            system.NotifyManualArrest(this);
        }


        private void CalculateStolenValues(CustomerCart cart)
        {
            long target = system.GetTargetStealValue(thiefType);
            reservedItems.Clear();
            long total;
            int products;
            bool hasReserved = inventoryBridge != null &&
                               inventoryBridge.TryReserveStolenItems(cart, thiefType, target, reservedItems, out total, out products);

            if (!hasReserved || total <= 0)
            {
                total = target;
                products = Mathf.Max(1, Mathf.FloorToInt(total / 1000f));
            }

            stolenValue = total;
            stolenProductsCount = products;
        }


        private void ApplyVisualMarker()
        {
            Color markerColor = system.GetVisualColor(thiefType);
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.material == null)
                    continue;

                renderer.material.color = Color.Lerp(renderer.material.color, markerColor, 0.45f);
            }
        }


        private void ApplySpeedModifier()
        {
            if (owner == null)
                return;

            float multiplier = thiefType == ShoplifterType.Fast || thiefType == ShoplifterType.Special ? 1.25f : 1f;
            SetCurrentMovementSpeed(baseSpeed * multiplier);
        }


        private float GetCurrentMovementSpeed()
        {
            CustomerAgent agent = owner != null ? owner.GetComponent<CustomerAgent>() : null;
            if (agent == null)
                return 0f;

            var native = agent.GetNative();
            return native != null ? native.speed : 0f;
        }


        private void SetCurrentMovementSpeed(float speed)
        {
            if (speed <= 0f)
                return;

            CustomerAgent agent = owner != null ? owner.GetComponent<CustomerAgent>() : null;
            if (agent == null)
                return;

            var native = agent.GetNative();
            if (native != null)
                native.speed = speed;
        }
    }
}
