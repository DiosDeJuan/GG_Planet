//Adaptado por POMPIC 20100333
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

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

        private static long InvokeLong(Type type, string methodName, params object[] args)
        {
            return Convert.ToInt64(Invoke(type, methodName, args));
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
    }
}
