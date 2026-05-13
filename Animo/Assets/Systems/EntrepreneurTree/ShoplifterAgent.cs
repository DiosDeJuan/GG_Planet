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
        private const float FastThiefSpeedMultiplier = 1.25f;
        /// <summary>Height above the character root at which the floating indicator is spawned.</summary>
        private const float IndicatorHeightOffset = 2.2f;
        /// <summary>Uniform scale applied to the floating indicator sphere.</summary>
        private const float IndicatorScale = 0.28f;

        // Cached once to avoid repeated Shader.Find() calls on each thief spawn.
        private static Shader standardShader;

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
        // Material created for the floating indicator — destroyed with this component.
        private Material indicatorMaterial;

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
            long total = 0;
            int products = 0;
            bool hasReserved = inventoryBridge != null &&
                               inventoryBridge.TryReserveStolenItems(cart, thiefType, target, reservedItems, out total, out products);

            if (!hasReserved || total <= 0)
            {
                total = target;
                products = Mathf.Max(1, Mathf.FloorToInt(total / 1000f));
                if (!hasReserved)
                    Debug.LogWarning("[Robbery] Inventory bridge did not return stolen items. Using fallback theft values.");
            }

            stolenValue = total;
            stolenProductsCount = products;
        }


        private void ApplyVisualMarker()
        {
            Color markerColor = system.GetVisualColor(thiefType);

            // Apply a stronger tint using MaterialPropertyBlock to avoid instantiating
            // per-renderer materials, which would create untracked material leaks.
            var block = new MaterialPropertyBlock();
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                    continue;

                renderer.GetPropertyBlock(block);
                Color baseColor = renderer.sharedMaterial != null
                    ? renderer.sharedMaterial.color
                    : Color.white;
                block.SetColor("_Color", Color.Lerp(baseColor, markerColor, 0.75f));
                renderer.SetPropertyBlock(block);
            }

            // Spawn a small floating sphere above the thief as a clear world-space warning indicator.
            indicatorMaterial = SpawnFloatingIndicator(markerColor);
        }


        private Material SpawnFloatingIndicator(Color color)
        {
            // Position the indicator slightly above the character's head.
            Vector3 offset = Vector3.up * IndicatorHeightOffset;

            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.name = "ThiefIndicator";
            indicator.transform.SetParent(transform, false);
            indicator.transform.localPosition = offset;
            indicator.transform.localScale    = new Vector3(IndicatorScale, IndicatorScale, IndicatorScale);

            // Remove physics — purely visual.
            Collider col = indicator.GetComponent<Collider>();
            if (col != null)
                Object.Destroy(col);

            // Use a single material instance tied to this indicator's lifetime; it is
            // destroyed together with the indicator GameObject when the thief leaves.
            Material mat = null;
            Renderer rend = indicator.GetComponent<Renderer>();
            if (rend != null)
            {
                if (standardShader == null)
                    standardShader = Shader.Find("Standard");
                mat = new Material(standardShader);
                mat.color = color;
                mat.SetFloat("_Metallic",   0f);
                mat.SetFloat("_Smoothness", 0.4f);
                rend.material = mat;
            }

            // Attach a simple bob animation.
            IndicatorBobber bobber = indicator.AddComponent<IndicatorBobber>();
            bobber.baseLocalY = offset.y;

            return mat; // caller may cache to destroy explicitly if needed
        }


        private void ApplySpeedModifier()
        {
            if (owner == null)
                return;

            float multiplier = thiefType == ShoplifterType.Fast || thiefType == ShoplifterType.Special
                ? FastThiefSpeedMultiplier
                : 1f;
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


        void OnDestroy()
        {
            // Destroy the indicator material to prevent memory leaks.
            if (indicatorMaterial != null)
                Object.Destroy(indicatorMaterial);
        }
    }
}
