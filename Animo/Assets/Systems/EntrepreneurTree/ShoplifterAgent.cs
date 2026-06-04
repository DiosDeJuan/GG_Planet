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
        private bool inventoryRestored;
        private bool inventoryLossConfirmed;
        private float baseSpeed;
        // Material created for the floating indicator — destroyed with this component.
        private Material indicatorMaterial;
        // Materials created for primitive accessories (cap, backpack) — destroyed with this component.
        private readonly System.Collections.Generic.List<Material> accessoryMaterials
            = new System.Collections.Generic.List<Material>();

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

            if (!TryCalculateStolenValues(cart))
            {
                Debug.LogWarning("[Robbery] Theft cancelled because no real cart products could be reserved.");
                return false;
            }

            theftStarted = true;
            system.NotifyThiefDetected(this);

            if (system.TryAutomaticArrest(this))
            {
                Resolve(autoArrest: true, escaped: false);
                owner.GoHome();
                return true;
            }

            isEscaping = true;
            interactable.SetInteractable(true);
            system.ShowShoplifterNotification("Seguridad fallo. Interven manualmente.", new Color(1f, 0.45f, 0.15f));
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
            if (inventoryRestored || inventoryLossConfirmed || inventoryBridge == null || reservedItems.Count == 0)
                return false;

            bool restored = inventoryBridge.RestoreStolenItems(reservedItems, out restoredCount, out restoredValue);
            if (restored)
                inventoryRestored = true;
            return restored;
        }


        public bool ConfirmInventoryLoss()
        {
            if (inventoryRestored || inventoryLossConfirmed || inventoryBridge == null || reservedItems.Count == 0)
                return false;

            bool confirmed = inventoryBridge.ConfirmStolenItems(reservedItems);
            if (confirmed)
                inventoryLossConfirmed = true;
            return confirmed;
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


        private bool TryCalculateStolenValues(CustomerCart cart)
        {
            long target = system.GetTargetStealValue(thiefType);
            reservedItems.Clear();
            long total = 0;
            int products = 0;
            bool hasReserved = inventoryBridge != null &&
                               inventoryBridge.TryReserveStolenItems(cart, thiefType, target, reservedItems, out total, out products);

            if (!hasReserved || total <= 0 || products <= 0)
                return false;

            stolenValue = total;
            stolenProductsCount = products;
            return true;
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
                // sharedMaterial.color is a read-only access to the asset colour — intentional;
                // we never write to sharedMaterial, so the shared asset is never modified.
                Color baseColor = renderer.sharedMaterial != null
                    ? renderer.sharedMaterial.color
                    : Color.white;
                block.SetColor("_Color", Color.Lerp(baseColor, markerColor, 0.75f));
                renderer.SetPropertyBlock(block);
            }

            if (thiefType != ShoplifterType.Suspicious)
                indicatorMaterial = SpawnFloatingIndicator(markerColor);

            // Add cap + backpack placeholder accessories so thieves are visually distinct.
            SpawnThiefAccessories(markerColor);
        }


        /// <summary>
        /// Spawns a primitive cap (flattened cylinder on head) and backpack (box on back)
        /// so thieves are immediately recognisable.  Uses primitives — no external assets required.
        /// </summary>
        private void SpawnThiefAccessories(Color accentColor)
        {
            // Try to find the character's head transform by name convention.
            // Common names in humanoid character rigs: "Head", "head", "Bip001 Head", "mixamorig:Head".
            Transform headBone = FindBoneByName(transform, "head") ?? FindBoneByName(transform, "Head");
            Transform capParent = headBone != null ? headBone : transform;
            if (thiefType == ShoplifterType.Suspicious)
            {
                SpawnGlasses(capParent, headBone != null ? Vector3.forward * 0.07f : new Vector3(0f, 1.68f, 0.18f));
                return;
            }

            // Cap — a flattened cylinder placed on/near the head.
            float capRadius = 0.115f;
            float capHeight = 0.06f;
            Vector3 capLocalPos = headBone != null
                ? Vector3.zero + Vector3.up * 0.12f   // just above head bone
                : new Vector3(0f, 1.75f, 0f);          // world-space fallback offset

            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cap.name = "Thief_Cap";
            cap.transform.SetParent(capParent, false);
            cap.transform.localPosition = capLocalPos;
            cap.transform.localScale    = new Vector3(capRadius * 2f, capHeight, capRadius * 2f);

            Collider capCol = cap.GetComponent<Collider>();
            if (capCol != null)
                Object.Destroy(capCol);

            ApplyPrimitiveMaterial(cap, new Color(0.12f, 0.12f, 0.12f)); // dark cap

            if (thiefType != ShoplifterType.Expert && thiefType != ShoplifterType.Special)
                return;

            // Backpack — a small box on the character's back.
            float bpWidth  = 0.18f;
            float bpHeight = 0.22f;
            float bpDepth  = 0.09f;
            // Position relative to character root: behind and mid-torso height.
            GameObject backpack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backpack.name = "Thief_Backpack";
            backpack.transform.SetParent(transform, false);
            backpack.transform.localPosition = new Vector3(0f, 1.1f, -0.22f);
            backpack.transform.localScale    = new Vector3(bpWidth, bpHeight, bpDepth);

            Collider bpCol = backpack.GetComponent<Collider>();
            if (bpCol != null)
                Object.Destroy(bpCol);

            ApplyPrimitiveMaterial(backpack, accentColor);
        }


        private void SpawnGlasses(Transform parent, Vector3 localPosition)
        {
            GameObject glasses = new GameObject("Thief_Glasses");
            glasses.transform.SetParent(parent, false);
            glasses.transform.localPosition = localPosition;

            CreateGlassesLens(glasses.transform, "Lens_L", new Vector3(-0.045f, 0f, 0f));
            CreateGlassesLens(glasses.transform, "Lens_R", new Vector3(0.045f, 0f, 0f));
        }


        private void CreateGlassesLens(Transform parent, string name, Vector3 localPosition)
        {
            GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lens.name = name;
            lens.transform.SetParent(parent, false);
            lens.transform.localPosition = localPosition;
            lens.transform.localScale = new Vector3(0.055f, 0.028f, 0.012f);

            Collider col = lens.GetComponent<Collider>();
            if (col != null)
                Object.Destroy(col);

            ApplyPrimitiveMaterial(lens, new Color(0.06f, 0.06f, 0.07f));
        }


        /// <summary>Searches child transforms for a bone whose name contains <paramref name="namePart"/>.</summary>
        private static Transform FindBoneByName(Transform root, string namePart)
        {
            if (root == null || string.IsNullOrEmpty(namePart))
                return null;

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child != null && child.name.IndexOf(namePart, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return child;
            }

            return null;
        }


        private void ApplyPrimitiveMaterial(GameObject go, Color color)
        {
            Renderer r = go.GetComponent<Renderer>();
            if (r == null)
                return;

            if (standardShader == null)
                standardShader = Shader.Find("Standard");

            Material mat = new Material(standardShader);
            mat.color = color;
            r.material = mat;
            accessoryMaterials.Add(mat);
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

            // Destroy accessory materials.
            for (int i = 0; i < accessoryMaterials.Count; i++)
            {
                if (accessoryMaterials[i] != null)
                    Object.Destroy(accessoryMaterials[i]);
            }
            accessoryMaterials.Clear();
        }
    }
}
