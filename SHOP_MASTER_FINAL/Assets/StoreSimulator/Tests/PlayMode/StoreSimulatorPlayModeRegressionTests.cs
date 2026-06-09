//Adaptado por POMPIC 20100333
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator.Tests
{
    public class StoreSimulatorPlayModeRegressionTests
    {
        [Test]
        public void ProductPricingCalculator_ClampsPricesAndKeepsDocumentedProbabilities()
        {
            Type productType = FindGameType("FLOBUK.StoreSimulator.ProductScriptableObject");
            Type calculatorType = FindGameType("FLOBUK.StoreSimulator.ProductPricingCalculator");
            ScriptableObject product = ScriptableObject.CreateInstance(productType);
            SetField(product, "buyPrice", 80L);
            SetField(product, "marketPrice", 100L);
            SetField(product, "storePrice", 100L);

            Assert.AreEqual(100L, InvokeLong(calculatorType, "GetIdealPrice", product));
            Assert.AreEqual(300L, InvokeLong(calculatorType, "GetMaxPrice", product));
            Assert.AreEqual(0L, InvokeLong(calculatorType, "ClampPrice", product, -25L));
            Assert.AreEqual(300L, InvokeLong(calculatorType, "ClampPrice", product, 999L));
            Assert.AreEqual(1f, InvokeFloat(calculatorType, "GetPurchaseProbability", product, 100L));
            Assert.Less(InvokeFloat(calculatorType, "GetPurchaseProbability", product, 300L), 1f);
            Assert.Greater(InvokeFloat(calculatorType, "GetExtraPurchaseProbability", product, 50L), 0f);

            UnityEngine.Object.DestroyImmediate(product);
        }

        [Test]
        public void SaveGameSystem_AlternateProfileKeyUsesDistinctSavePath()
        {
            Type saveGameType = FindGameType("FLOBUK.StoreSimulator.SaveGameSystem");
            MethodInfo getSavePath = saveGameType.GetMethod("GetSavePath", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(getSavePath);

            string defaultPath = (string)getSavePath.Invoke(null, new object[] { string.Empty });
            string profilePath = (string)getSavePath.Invoke(null, new object[] { "fase6_profile" });

            Assert.AreNotEqual(defaultPath, profilePath);
            Assert.AreEqual("save.dat", Path.GetFileName(defaultPath));
            Assert.AreEqual("fase6_profile.dat", Path.GetFileName(profilePath));
        }

        [Test]
        public void ProductPricingCalculator_NullAndZeroIdealInputsStaySafe()
        {
            Type productType = FindGameType("FLOBUK.StoreSimulator.ProductScriptableObject");
            Type calculatorType = FindGameType("FLOBUK.StoreSimulator.ProductPricingCalculator");
            ScriptableObject product = ScriptableObject.CreateInstance(productType);
            SetField(product, "buyPrice", 0L);
            SetField(product, "marketPrice", 0L);
            SetField(product, "storePrice", 0L);

            Assert.AreEqual(0L, InvokeLong(calculatorType, "GetIdealPrice", new object[] { null }));
            Assert.AreEqual(0L, InvokeLong(calculatorType, "GetMaxPrice", new object[] { null }));
            Assert.AreEqual(0L, InvokeLong(calculatorType, "ClampPrice", null, 999L));
            Assert.AreEqual(1f, InvokeFloat(calculatorType, "GetPurchaseProbability", null, 999L));
            Assert.AreEqual(0f, InvokeFloat(calculatorType, "GetExtraPurchaseProbability", null, 0L));

            Assert.AreEqual(0L, InvokeLong(calculatorType, "GetIdealPrice", product));
            Assert.AreEqual(1f, InvokeFloat(calculatorType, "GetPurchaseProbability", product, 0L));
            Assert.AreEqual(0f, InvokeFloat(calculatorType, "GetExtraPurchaseProbability", product, 0L));

            UnityEngine.Object.DestroyImmediate(product);
        }

        [Test]
        public void ShopExpansionManager_ClampsAreasAndScalesRiskWithoutRunawayValues()
        {
            Type expansionType = FindGameType("FLOBUK.StoreSimulator.ShopExpansionManager");

            SetStaticField(expansionType, "pendingSaleUnits", 0);
            SetStaticField(expansionType, "purchasedStorageUnits", 0);
            Assert.AreEqual(24, GetStaticIntProperty(expansionType, "MaxSaleUnits"));
            Assert.AreEqual(192, GetStaticIntProperty(expansionType, "CurrentSaleAreaM2"));
            Assert.AreEqual(32, GetStaticIntProperty(expansionType, "CurrentStorageAreaM2"));
            Assert.AreEqual(10, InvokeInt(expansionType, "GetExpandedCustomerSpawnRate", 10));
            Assert.AreEqual(0.02f, InvokeFloat(expansionType, "GetShoplifterSpawnChance"), 0.0001f);

            SetStaticField(expansionType, "pendingSaleUnits", 2);
            SetStaticField(expansionType, "purchasedStorageUnits", 2);
            Assert.AreEqual(224, GetStaticIntProperty(expansionType, "CurrentSaleAreaM2"));
            Assert.AreEqual(96, GetStaticIntProperty(expansionType, "CurrentStorageAreaM2"));
            Assert.AreEqual(13, InvokeInt(expansionType, "GetExpandedCustomerSpawnRate", 10));
            Assert.Greater(InvokeFloat(expansionType, "GetShoplifterSpawnChance"), 0.02f);
            Assert.Less(InvokeFloat(expansionType, "GetShoplifterSpawnChance"), 0.065f);

            SetStaticField(expansionType, "pendingSaleUnits", 999);
            SetStaticField(expansionType, "purchasedStorageUnits", 999);
            Assert.AreEqual(24, GetStaticIntProperty(expansionType, "PurchasedSaleUnits"));
            Assert.AreEqual(4, GetStaticIntProperty(expansionType, "PurchasedStorageUnits"));
            Assert.AreEqual(576, GetStaticIntProperty(expansionType, "CurrentSaleAreaM2"));
            Assert.AreEqual(160, GetStaticIntProperty(expansionType, "CurrentStorageAreaM2"));
            Assert.AreEqual(46, InvokeInt(expansionType, "GetExpandedCustomerSpawnRate", 10));
            Assert.AreEqual(0.065f, InvokeFloat(expansionType, "GetShoplifterSpawnChance"), 0.0001f);
        }

        [UnityTest]
        public IEnumerator UIManagementUIBootstrap_EnsureKeepsSingleManagementEntry()
        {
            Type desktopType = FindGameType("FLOBUK.StoreSimulator.UIShopDesktop");
            Type bootstrapType = FindGameType("FLOBUK.StoreSimulator.UIManagementUIBootstrap");

            GameObject root = new GameObject("Desktop Root", typeof(RectTransform));
            root.SetActive(false);
            Component desktop = root.AddComponent(desktopType);

            GameObject contentArea = new GameObject("ContentArea", typeof(RectTransform));
            contentArea.transform.SetParent(root.transform, false);

            GameObject navigation = new GameObject("Navigation", typeof(RectTransform));
            navigation.transform.SetParent(root.transform, false);
            GameObject categories = new GameObject("Categories", typeof(RectTransform));
            categories.transform.SetParent(navigation.transform, false);

            GameObject template = new GameObject("Template Button", typeof(RectTransform), typeof(Image), typeof(Button));
            template.transform.SetParent(categories.transform, false);

            MethodInfo ensure = bootstrapType.GetMethod("Ensure", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(ensure);

            ensure.Invoke(null, new object[] { desktop });
            yield return null;
            ensure.Invoke(null, new object[] { desktop });
            yield return null;

            Assert.AreEqual(1, CountChildrenNamed(navigation.transform, "Button - Management"));
            Assert.AreEqual(1, CountChildrenNamed(contentArea.transform, "Management"));

            UnityEngine.Object.DestroyImmediate(root);
        }

        private static Type FindGameType(string typeName)
        {
            Type type = AppDomainAssemblies()
                .Select(assembly => assembly.GetType(typeName))
                .FirstOrDefault(foundType => foundType != null);
            Assert.NotNull(type, "Could not find type " + typeName);
            return type;
        }

        private static Assembly[] AppDomainAssemblies()
        {
            return System.AppDomain.CurrentDomain.GetAssemblies();
        }

        private static void SetField(object target, string fieldName, long value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(field, "Could not find field " + fieldName);
            field.SetValue(target, value);
        }

        private static void SetStaticField(Type type, string fieldName, int value)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(field, "Could not find field " + fieldName);
            field.SetValue(null, value);
        }

        private static int GetStaticIntProperty(Type type, string propertyName)
        {
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(property, "Could not find property " + propertyName);
            return Convert.ToInt32(property.GetValue(null));
        }

        private static long InvokeLong(Type type, string methodName, params object[] args)
        {
            return Convert.ToInt64(Invoke(type, methodName, args));
        }

        private static int InvokeInt(Type type, string methodName, params object[] args)
        {
            return Convert.ToInt32(Invoke(type, methodName, args));
        }

        private static float InvokeFloat(Type type, string methodName, params object[] args)
        {
            return Convert.ToSingle(Invoke(type, methodName, args));
        }

        private static object Invoke(Type type, string methodName, params object[] args)
        {
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(method, "Could not find method " + methodName);
            return method.Invoke(null, args);
        }

        private static int CountChildrenNamed(Transform parent, string objectName)
        {
            int count = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == objectName)
                    count++;

                count += CountChildrenNamed(child, objectName);
            }

            return count;
        }
    }
}
