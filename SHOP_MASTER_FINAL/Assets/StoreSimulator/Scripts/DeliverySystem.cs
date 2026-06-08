//Adaptado por POMPIC 20100333
/*  This file is part of the "Store Simulator" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store, Epic FAB).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using System;
using UnityEngine;
using SimpleJSON;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Manages the instantiation (delivery) of packages, i.e. products or storage, that the player have bought.
    /// </summary>
    public class DeliverySystem : MonoBehaviour
    {
        /// <summary>
        /// Returns a reference to this script instance.
        /// </summary>
        public static DeliverySystem Instance { get; private set; }

        /// <summary>
        /// Event fired when a product has been bought to inform the player.
        /// </summary>
        public static event Action<ProductScriptableObject> onProductPurchase;

        /// <summary>
        /// The starting point of where packages should start to be spawned at.
        /// </summary>
        public Transform deliveryStart;

        /// <summary>
        /// The direction to spawn new packages in.
        /// </summary>
        public Vector2 deliveryDirection;

        /// <summary>
        /// Count of maximum packages that can be spawned in the deliveryDirection.
        /// </summary>
        public int totalDeliveries = 1;

        /// <summary>
        /// Box prefab that should contain the bought item.
        /// </summary>
        public GameObject packagePrefab;

        /// <summary>
        /// Clip to play when a PackageObject has been picked up, or none if not set.
        /// </summary>
        public AudioClip pickupClip;

        /// <summary>
        /// Clip to play when a PackageObject has been dropped, or none if not set.
        /// </summary>
        public AudioClip dropClip;


        //initialize variables
        void Awake()
        {
            Instance = this;
        }


        /// <summary>
        /// Called from a UIShopItem instance when trying to purchase that specific item.
        /// Does an internal check for money and then spawns the package.
        /// </summary>
        public static void Purchase(PurchasableScriptableObject purchasable)
        {
            if (!TryPurchase(purchasable, out string message))
            {
                if (UIGame.Instance != null && !string.IsNullOrEmpty(message))
                    UIGame.Instance.ShowMessage(message);
                return;
            }

            if (UIGame.Instance != null && !string.IsNullOrEmpty(message))
                UIGame.Instance.ShowMessage(message);
        }


        public static bool TryPurchase(PurchasableScriptableObject purchasable, out string message)
        {
            if (!CanPurchaseWithMessage(purchasable, out message))
                return false;

            int amount = GetPackageAmount(purchasable);
            long totalPrice = purchasable.buyPrice * amount;

            //spawn package and amount of items within that package
            try
            {
                Vector3 deliveryPosition = Instance.GetDeliveryPosition();
                GameObject newPackage = Instantiate(Instance.packagePrefab, deliveryPosition + new Vector3(0, 2, 0), Quaternion.identity);
                PackageObject packageObject = newPackage != null ? newPackage.GetComponent<PackageObject>() : null;
                if (packageObject == null)
                {
                    message = "No se pudo crear el pedido porque falta el sistema de entrega/inventario.";
                    Debug.LogError("[DeliverySystem] packagePrefab no tiene PackageObject. purchasable=" + GetProductContext(purchasable));
                    return false;
                }

                packageObject.Add(purchasable, amount);
            }
            catch (Exception e)
            {
                message = "Error inesperado al procesar compra. Revisa consola para más detalles.";
                Debug.LogError("[DeliverySystem] Compra fallida para " + GetProductContext(purchasable) + ". Exception: " + e);
                return false;
            }

            //subtract money only after package creation succeeds
            StoreDatabase.AddRemoveMoney(-totalPrice);

            ProductScriptableObject product = purchasable as ProductScriptableObject;
            onProductPurchase?.Invoke(product);
            message = "Pedido realizado: " + GetProductContext(purchasable) + " x" + amount + ".";
            return true;
        }


        public static bool CanPurchaseWithMessage(PurchasableScriptableObject purchasable, out string message)
        {
            if (purchasable == null)
            {
                message = "Producto no configurado correctamente: sin referencia. Revisar catálogo.";
                return false;
            }

            ProductScriptableObject product = purchasable as ProductScriptableObject;
            if (product != null)
            {
                if (!EntrepreneurProgress.IsProductUnlocked(product))
                {
                    message = EntrepreneurTreeDefinitions.GetProductLockedPurchaseMessage(product);
                    return false;
                }

                if (!string.IsNullOrEmpty(product.requiredLicense))
                {
                    try
                    {
                        LicenseScriptableObject requiredLicense = ItemDatabase.GetById(typeof(LicenseScriptableObject), product.requiredLicense) as LicenseScriptableObject;
                        if (requiredLicense != null && !requiredLicense.isPurchased)
                        {
                            message = "Producto bloqueado. Requiere licencia " + requiredLicense.title + ".";
                            return false;
                        }
                    }
                    catch (Exception)
                    {
                        message = "Producto no configurado correctamente: " + GetProductContext(product) + ". Revisar catálogo.";
                        return false;
                    }
                }
            }

            if (StoreDatabase.Instance != null && purchasable.requiredLevel > StoreDatabase.Instance.currentLevel)
            {
                message = "Producto bloqueado. Requiere nivel " + purchasable.requiredLevel + ".";
                return false;
            }

            if (!ValidateDeliveryDependencies(out message))
                return false;

            if (!ValidatePurchasableConfiguration(purchasable, out message))
                return false;

            int amount = GetPackageAmount(purchasable);
            long totalPrice = purchasable.buyPrice * amount;
            if (!StoreDatabase.CanPurchase(totalPrice))
            {
                long missingAmount = Mathf.Max(0, totalPrice - StoreDatabase.Instance.currentMoney);
                message = "Fondos insuficientes. Faltan " + StoreDatabase.FromLongToStringMoney(missingAmount) + ".";
                return false;
            }

            message = string.Empty;
            return true;
        }


        //calculate the lowest possible position to deliver new packages
        private Vector3 GetDeliveryPosition()
        {
            //raycast properties
            Vector3 lowestPosition = deliveryStart.position;
            float highestDistance = 0f;
            float rayLength = 50;

            //starting from the deliveryStart position, do a raycast until the end of deliveryDirection to find the lowest
            //position in height by raycasting against all packages that have already been spawned at the delivery area
            for(int i = 0; i < totalDeliveries; i++)
            {
                Vector3 rayPosition = deliveryStart.position + new Vector3(i * deliveryDirection.x, 0, i * deliveryDirection.y);
                Ray ray = new Ray(rayPosition + Vector3.up * rayLength, Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hit, rayLength, InteractionSystem.Instance.layerMask))
                {
                    float hitDistance = Vector3.Distance(ray.origin, hit.point);
                    if (hitDistance > highestDistance)
                    {
                        lowestPosition = hit.point;
                        highestDistance = hitDistance;
                    }
                }
                else
                {
                    lowestPosition = rayPosition;
                    break;
                }
            }

            return lowestPosition;
        }


        private static bool ValidateDeliveryDependencies(out string message)
        {
            if (Instance == null || StoreDatabase.Instance == null || InteractionSystem.Instance == null || Instance.deliveryStart == null || Instance.packagePrefab == null)
            {
                message = "No se pudo crear el pedido porque falta el sistema de entrega/inventario.";
                return false;
            }

            message = string.Empty;
            return true;
        }


        private static bool ValidatePurchasableConfiguration(PurchasableScriptableObject purchasable, out string message)
        {
            if (string.IsNullOrWhiteSpace(purchasable.id) || string.IsNullOrWhiteSpace(purchasable.title))
            {
                message = "Producto no configurado correctamente: " + GetProductContext(purchasable) + ". Revisar catálogo.";
                return false;
            }

            if (purchasable.buyPrice < 0)
            {
                message = "Producto no configurado correctamente: " + GetProductContext(purchasable) + ". Revisar catálogo.";
                return false;
            }

            ProductScriptableObject product = purchasable as ProductScriptableObject;
            if (product != null)
            {
                if (product.prefab == null || product.packageCount <= 0)
                {
                    message = "Producto no configurado correctamente: " + GetProductContext(product) + ". Revisar catálogo.";
                    return false;
                }
            }

            message = string.Empty;
            return true;
        }


        private static int GetPackageAmount(PurchasableScriptableObject purchasable)
        {
            ProductScriptableObject product = purchasable as ProductScriptableObject;
            return product != null ? product.packageCount : 1;
        }


        private static string GetProductContext(PurchasableScriptableObject purchasable)
        {
            if (purchasable == null)
                return "sin_id";

            if (!string.IsNullOrWhiteSpace(purchasable.title))
                return purchasable.title;

            if (!string.IsNullOrWhiteSpace(purchasable.id))
                return purchasable.id;

            return purchasable.name;
        }


        /// <summary>
        /// Reads component data that should be persisted and returns it as a JSONNode. 
        /// </summary>
        public JSONNode SaveToJSON()
        {
            JSONNode data = new JSONObject();

            JSONNode objectArray = new JSONArray();

            PackageObject[] objects = FindObjectsByType<PackageObject>(FindObjectsSortMode.None);
            for(int i = 0; i < objects.Length; i++)
                objectArray[i] = objects[i].SaveToJSON();

            data["PackageObjects"] = objectArray;

            return data;
        }


        /// <summary>
        /// Applies existing data coming from a JSONNode and overwrites it on this component.
        /// </summary>
        public void LoadFromJSON(JSONNode data)
        {
            if (data == null || data.Count == 0)
                return;

            JSONArray objectsArray = data["PackageObjects"].AsArray;
            for(int i = 0; i < objectsArray.Count; i++)
            {
                GameObject go = Instantiate(packagePrefab);
                go.GetComponent<PackageObject>().LoadFromJSON(objectsArray[i]);
            }
        }
    }
}