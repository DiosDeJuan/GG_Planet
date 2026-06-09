//Adaptado por POMPIC 20100333
using System;
using System.Collections;
using System.Collections.Generic;
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

        [Test]
        public void EntrepreneurTree_HasAllDocumentedCoreNodes()
        {
            object[] nodes = GetTreeNodes();
            string[] expectedIds =
            {
                "productos_basicos_1", "productos_basicos_2", "productos_basicos_3", "lacteos_1", "lacteos_2",
                "especias_1", "productos_frescos_1", "productos_frescos_2", "productos_higiene", "sodas",
                "proteina_1", "productos_lujo_1", "electrodomesticos_1",
                "empleado_1", "empleado_2", "empleado_3", "empleado_4", "empleado_5", "empleado_6",
                "empleado_7", "empleado_8", "empleado_9", "empleado_10", "empleado_11", "empleado_12",
                "empleado_13", "empleado_14", "empleado_15", "empleado_16", "empleado_17", "empleado_18",
                "seguridad_1", "seguridad_2", "seguridad_3", "mejora_cafeina", "mejora_carismatico"
            };

            CollectionAssert.AreEquivalent(expectedIds, nodes.Select(GetNodeId).ToArray());
            Assert.AreEqual(13, nodes.Count(node => GetNodeTypeName(node) == "Product"));
            Assert.AreEqual(18, nodes.Count(node => GetNodeTypeName(node) == "Employee"));
            Assert.AreEqual(3, nodes.Count(node => GetNodeTypeName(node) == "Security"));
            Assert.AreEqual(2, nodes.Count(node => GetNodeTypeName(node) == "Upgrade"));
        }

        [Test]
        public void EntrepreneurTree_BasicProducts1UnlockedByDefault()
        {
            ResetProgress();

            Assert.IsTrue(IsUnlocked("productos_basicos_1"));
            Assert.AreEqual(0, GetProgressInt("AvailablePoints"));
            Assert.AreEqual("Unlocked", GetStateName(GetNode("productos_basicos_1")));
        }

        [Test]
        public void EntrepreneurTree_EmployeesHaveDocumentedPrerequisites()
        {
            Dictionary<string, string> expected = new Dictionary<string, string>
            {
                { "empleado_1", "especias_1" },
                { "empleado_2", "productos_higiene" },
                { "empleado_3", "sodas" },
                { "empleado_4", "lacteos_1" },
                { "empleado_5", "lacteos_1" },
                { "empleado_6", "especias_1" },
                { "empleado_7", "empleado_5" },
                { "empleado_8", "sodas" },
                { "empleado_9", "productos_higiene" },
                { "empleado_10", "empleado_1" },
                { "empleado_11", "seguridad_1" },
                { "empleado_12", "empleado_13" },
                { "empleado_13", "productos_lujo_1" },
                { "empleado_14", "electrodomesticos_1" },
                { "empleado_15", "seguridad_2" },
                { "empleado_16", "proteina_1" },
                { "empleado_17", "productos_frescos_2" },
                { "empleado_18", "seguridad_3" },
            };

            foreach (KeyValuePair<string, string> pair in expected)
            {
                object node = GetNode(pair.Key);
                Assert.NotNull(node, pair.Key);
                CollectionAssert.AreEqual(new[] { pair.Value }, GetNodePrerequisites(node), pair.Key);
                Assert.AreEqual(1, GetNodeCost(node), pair.Key);
            }
        }

        [Test]
        public void EntrepreneurTree_SecurityLevelsHaveDocumentedPrerequisitesAndValues()
        {
            AssertNodePrerequisiteAndBenefit("seguridad_1", "empleado_7", "33%");
            AssertNodePrerequisiteAndBenefit("seguridad_2", "empleado_8", "66%");
            AssertNodePrerequisiteAndBenefit("seguridad_3", "empleado_14", "99%");

            ResetProgress();
            UnlockWithPoint("productos_basicos_2", "productos_basicos_3", "lacteos_1", "empleado_5", "empleado_7", "seguridad_1");
            Assert.AreEqual(1, GetProgressInt("SecurityLevel"));

            UnlockWithPoint("especias_1", "productos_higiene", "sodas", "empleado_8", "seguridad_2");
            Assert.AreEqual(2, GetProgressInt("SecurityLevel"));

            UnlockWithPoint("productos_lujo_1", "electrodomesticos_1", "empleado_14", "seguridad_3");
            Assert.AreEqual(3, GetProgressInt("SecurityLevel"));
        }

        [Test]
        public void EntrepreneurTree_ImprovementsHaveDocumentedPrerequisites()
        {
            AssertNodePrerequisiteAndBenefit("mejora_cafeina", "productos_frescos_2", "10%");
            AssertNodePrerequisiteAndBenefit("mejora_carismatico", "empleado_15", "5%");

            ResetProgress();
            Assert.AreEqual(1f, GetProgressFloat("EmployeeWorkSpeedMultiplier"));
            UnlockWithPoint("productos_basicos_2", "productos_basicos_3", "lacteos_1", "productos_frescos_1", "productos_frescos_2", "mejora_cafeina");
            Assert.AreEqual(1.1f, GetProgressFloat("EmployeeWorkSpeedMultiplier"), 0.0001f);

            ResetProgress();
            Assert.AreEqual(1f, GetProgressFloat("CashierRevenueMultiplier"));
            UnlockWithPoint("productos_basicos_2", "productos_basicos_3", "especias_1", "productos_higiene", "sodas", "empleado_8", "seguridad_2", "empleado_15", "mejora_carismatico");
            Assert.AreEqual(1.05f, GetProgressFloat("CashierRevenueMultiplier"), 0.0001f);
        }

        [Test]
        public void EntrepreneurTree_UnlockRequiresPrerequisites()
        {
            ResetProgress();
            AddTestProgressPoints(1);

            Assert.IsFalse(TryUnlock("empleado_1", out string message));
            StringAssert.Contains("Falta desbloquear: Especias 1", message);
            Assert.IsFalse(IsUnlocked("empleado_1"));
            Assert.AreEqual(1, GetProgressInt("AvailablePoints"));
        }

        [Test]
        public void EntrepreneurTree_UnlockRequiresProgressPoint()
        {
            ResetProgress();

            Assert.IsFalse(TryUnlock("productos_basicos_2", out string message));
            Assert.AreEqual("No tienes puntos de progreso suficientes.", message);
            Assert.AreEqual("Locked", GetStateName(GetNode("productos_basicos_2")));
        }

        [Test]
        public void EntrepreneurTree_UnlockConsumesOnePoint()
        {
            ResetProgress();
            AddTestProgressPoints(1);

            Assert.IsTrue(TryUnlock("productos_basicos_2", out string message), message);
            Assert.IsTrue(IsUnlocked("productos_basicos_2"));
            Assert.AreEqual(0, GetProgressInt("AvailablePoints"));
        }

        [Test]
        public void EntrepreneurAchievements_RewardOnlyOnce()
        {
            ResetProgress();

            Assert.IsTrue(AddProgressPoint("primeras_ventas", "Primeras Ventas"));
            Assert.IsFalse(AddProgressPoint("primeras_ventas", "Primeras Ventas"));
            Assert.AreEqual(1, GetProgressInt("AvailablePoints"));
            Assert.IsTrue(IsAchievementCompleted("primeras_ventas"));
            Assert.IsTrue(IsAchievementRewardClaimed("primeras_ventas"));
        }

        [Test]
        public void EntrepreneurAchievements_HasAllDocumentedDefinitions()
        {
            string[] expectedIds =
            {
                "primeras_ventas", "venta_rapida",
                "ingresos_1", "ingresos_2", "ingresos_3", "ingresos_4", "ingresos_5", "ingresos_6", "ingresos_7", "ingresos_8", "ingresos_9", "lluvia_dinero",
                "ventas_diarias_1", "ventas_diarias_2", "ventas_diarias_3", "ventas_diarias_4", "ventas_diarias_5", "ventas_diarias_6",
                "cliente_lujo", "primer_empleado", "supermercado_crecimiento", "imperialista", "surtido_completo", "dedicado", "fiel", "emprendedor",
                "huevo_dorado", "red_seguridad", "almacenamiento_maximizado", "eficiencia_maximo", "limpieza_impecable", "precio_perfecto",
                "lindo_hogar", "maximo_empleo", "optimizacion_total", "bajo_presion", "optimista", "paciente", "donador", "batman", "rapidez", "perezoso",
                "arbol_completo"
            };

            object[] definitions = GetAchievementDefinitions();
            CollectionAssert.AreEquivalent(expectedIds, definitions.Select(GetAchievementId).ToArray());
            Assert.AreEqual(0, GetAchievementRewardPoints(GetAchievement("arbol_completo")));
            Assert.IsTrue(GetAchievementIsHook(GetAchievement("eficiencia_maximo")));
            Assert.IsTrue(GetAchievementIsHook(GetAchievement("limpieza_impecable")));
        }

        [Test]
        public void EntrepreneurTree_SaveLoadPersistsUnlocksAndPoints()
        {
            ResetProgress();
            AddTestProgressPoints(2);
            Assert.IsTrue(TryUnlock("productos_basicos_2", out string message), message);

            object saved = SaveProgress();
            ResetProgress();
            LoadProgress(saved);

            Assert.IsTrue(IsUnlocked("productos_basicos_1"));
            Assert.IsTrue(IsUnlocked("productos_basicos_2"));
            Assert.AreEqual(1, GetProgressInt("AvailablePoints"));
        }

        [Test]
        public void EntrepreneurTree_SaveLoadDoesNotDuplicateRewards()
        {
            ResetProgress();
            Assert.IsTrue(AddProgressPoint("primeras_ventas", "Primeras Ventas"));

            object saved = SaveProgress();
            LoadProgress(saved);
            Invoke(GetAchievementManagerType(), "EvaluateAll");

            Assert.AreEqual(1, GetProgressInt("AvailablePoints"));
            Assert.IsTrue(IsAchievementRewardClaimed("primeras_ventas"));
        }

        [Test]
        public void EntrepreneurTree_UnlockEffectsGateProductsEmployeesSecurity()
        {
            ResetProgress();
            ScriptableObject harina = CreateProductProbe("harina", "Harina");

            Assert.IsFalse(IsProductUnlocked(harina));
            Assert.IsTrue(TryGetProductLockedMessage(harina, out string productMessage));
            StringAssert.Contains("Productos Basicos 2", productMessage);

            UnlockWithPoint("productos_basicos_2");
            Assert.IsTrue(IsProductUnlocked(harina));

            Type employeeManagerType = FindGameType("FLOBUK.StoreSimulator.EmployeeManager");
            object manager = Invoke(employeeManagerType, "EnsureInstance");
            try
            {
                Assert.IsFalse(TryHire(employeeManagerType, manager, "empleado_1", out string employeeMessage));
                StringAssert.Contains("Falta desbloquear", employeeMessage);

                UnlockWithPoint("productos_basicos_3", "especias_1", "empleado_1");
                Assert.IsTrue(TryHire(employeeManagerType, manager, "empleado_1", out employeeMessage), employeeMessage);

                UnlockWithPoint("lacteos_1", "empleado_5", "empleado_7", "seguridad_1");
                Assert.AreEqual(1, GetProgressInt("SecurityLevel"));
            }
            finally
            {
                if (manager != null)
                    UnityEngine.Object.DestroyImmediate(((Component)manager).gameObject);
                UnityEngine.Object.DestroyImmediate(harina);
            }
        }

        [UnityTest]
        public IEnumerator EntrepreneurTree_UIBootstrap_DoesNotCreateMainCanvas()
        {
            int canvasCountBefore = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length;
            GameObject root = CreateTreeDesktopFixture(out Component desktop, out _, out _);
            Type bootstrapType = FindGameType("FLOBUK.StoreSimulator.EntrepreneurTreeUIBootstrap");
            MethodInfo ensure = bootstrapType.GetMethod("Ensure", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(ensure);

            ensure.Invoke(null, new object[] { desktop });
            yield return null;

            int canvasCountAfter = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length;
            Assert.AreEqual(canvasCountBefore, canvasCountAfter);
            Assert.AreEqual(1, CountChildrenNamed(root.transform, "Entrepreneur Tree Content"));

            UnityEngine.Object.DestroyImmediate(root);
        }

        [UnityTest]
        public IEnumerator EntrepreneurTree_RebuildDoesNotDuplicateNodesOrListeners()
        {
            GameObject root = CreateTreeDesktopFixture(out Component desktop, out Transform licensesPanel, out Button licensesButton);
            Type bootstrapType = FindGameType("FLOBUK.StoreSimulator.EntrepreneurTreeUIBootstrap");
            MethodInfo ensure = bootstrapType.GetMethod("Ensure", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(ensure);

            ensure.Invoke(null, new object[] { desktop });
            yield return null;
            ensure.Invoke(null, new object[] { desktop });
            yield return null;

            Assert.AreEqual(1, CountChildrenNamed(root.transform, "Entrepreneur Tree Content"));
            Assert.AreEqual(GetTreeNodes().Length, licensesPanel.GetComponentsInChildren<MonoBehaviour>(true).Count(component => component.GetType().Name == "EntrepreneurTreeNodeView"));
            Assert.AreEqual(1, licensesButton.GetComponents<MonoBehaviour>().Count(component => component.GetType().Name == "EntrepreneurTreeNavigationBinding"));

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

        private static void AssertNodePrerequisiteAndBenefit(string nodeId, string prerequisiteId, string benefitText)
        {
            object node = GetNode(nodeId);
            Assert.NotNull(node, nodeId);
            CollectionAssert.AreEqual(new[] { prerequisiteId }, GetNodePrerequisites(node), nodeId);
            Assert.AreEqual(1, GetNodeCost(node), nodeId);
            StringAssert.Contains(benefitText, GetNodeBenefit(node));
        }

        private static void UnlockWithPoint(params string[] nodeIds)
        {
            foreach (string nodeId in nodeIds)
            {
                if (IsUnlocked(nodeId))
                    continue;

                AddTestProgressPoints(1);
                Assert.IsTrue(TryUnlock(nodeId, out string message), nodeId + ": " + message);
            }
        }

        private static void AddTestProgressPoints(int amount)
        {
            Type progressType = FindGameType("FLOBUK.StoreSimulator.EntrepreneurProgress");
            Invoke(progressType, "AddPointsForInternalTesting", amount);
        }

        private static ScriptableObject CreateProductProbe(string id, string title)
        {
            Type productType = FindGameType("FLOBUK.StoreSimulator.ProductScriptableObject");
            ScriptableObject product = ScriptableObject.CreateInstance(productType);
            SetField(product, "id", id);
            SetField(product, "title", title);
            SetField(product, "packageCount", 1);
            return product;
        }

        private static GameObject CreateTreeDesktopFixture(out Component desktop, out Transform licensesPanel, out Button licensesButton)
        {
            Type desktopType = FindGameType("FLOBUK.StoreSimulator.UIShopDesktop");
            GameObject root = new GameObject("UIShopDesktop Test Root", typeof(RectTransform));
            root.SetActive(false);
            desktop = root.AddComponent(desktopType);

            GameObject contentArea = new GameObject("ContentArea", typeof(RectTransform));
            contentArea.transform.SetParent(root.transform, false);

            GameObject navigation = new GameObject("Navigation", typeof(RectTransform));
            navigation.transform.SetParent(root.transform, false);
            GameObject categories = new GameObject("Categories", typeof(RectTransform));
            categories.transform.SetParent(navigation.transform, false);

            GameObject buttonObject = new GameObject("Button - Licenses", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(categories.transform, false);
            licensesButton = buttonObject.GetComponent<Button>();
            GameObject textObject = new GameObject("Text", typeof(RectTransform));
            textObject.transform.SetParent(buttonObject.transform, false);
            Component label = textObject.AddComponent(FindGameType("TMPro.TextMeshProUGUI"));
            SetProperty(label, "text", "LICENSES");

            GameObject licenses = new GameObject("Licenses", typeof(RectTransform));
            licenses.transform.SetParent(contentArea.transform, false);
            licensesPanel = licenses.transform;
            return root;
        }

        private static Type GetProgressType()
        {
            return FindGameType("FLOBUK.StoreSimulator.EntrepreneurProgress");
        }

        private static Type GetDefinitionsType()
        {
            return FindGameType("FLOBUK.StoreSimulator.EntrepreneurTreeDefinitions");
        }

        private static Type GetAchievementManagerType()
        {
            return FindGameType("FLOBUK.StoreSimulator.EntrepreneurAchievementManager");
        }

        private static Type GetAchievementDefinitionsType()
        {
            return FindGameType("FLOBUK.StoreSimulator.EntrepreneurAchievementDefinitions");
        }

        private static object[] GetTreeNodes()
        {
            object value = GetDefinitionsType().GetProperty("Nodes", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            return ((System.Collections.IEnumerable)value).Cast<object>().ToArray();
        }

        private static object GetNode(string nodeId)
        {
            return Invoke(GetDefinitionsType(), "Get", nodeId);
        }

        private static object[] GetAchievementDefinitions()
        {
            object value = GetAchievementDefinitionsType().GetProperty("Achievements", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            return ((System.Collections.IEnumerable)value).Cast<object>().ToArray();
        }

        private static object GetAchievement(string achievementId)
        {
            return Invoke(GetAchievementDefinitionsType(), "Get", achievementId);
        }

        private static string GetAchievementId(object achievement)
        {
            return Convert.ToString(achievement.GetType().GetProperty("Id").GetValue(achievement));
        }

        private static int GetAchievementRewardPoints(object achievement)
        {
            return Convert.ToInt32(achievement.GetType().GetProperty("RewardPoints").GetValue(achievement));
        }

        private static bool GetAchievementIsHook(object achievement)
        {
            return Convert.ToBoolean(achievement.GetType().GetProperty("IsHook").GetValue(achievement));
        }

        private static string GetNodeId(object node)
        {
            return Convert.ToString(node.GetType().GetProperty("Id").GetValue(node));
        }

        private static string GetNodeTypeName(object node)
        {
            return Convert.ToString(node.GetType().GetProperty("Type").GetValue(node));
        }

        private static int GetNodeCost(object node)
        {
            return Convert.ToInt32(node.GetType().GetProperty("Cost").GetValue(node));
        }

        private static string GetNodeBenefit(object node)
        {
            return Convert.ToString(node.GetType().GetProperty("Benefit").GetValue(node));
        }

        private static string[] GetNodePrerequisites(object node)
        {
            return (string[])node.GetType().GetProperty("Prerequisites").GetValue(node);
        }

        private static void ResetProgress()
        {
            Invoke(GetProgressType(), "ResetToDefaults");
        }

        private static bool IsUnlocked(string nodeId)
        {
            return Convert.ToBoolean(Invoke(GetProgressType(), "IsUnlocked", nodeId));
        }

        private static int GetProgressInt(string propertyName)
        {
            return Convert.ToInt32(GetProgressType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static).GetValue(null));
        }

        private static float GetProgressFloat(string propertyName)
        {
            return Convert.ToSingle(GetProgressType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static).GetValue(null));
        }

        private static string GetStateName(object node)
        {
            return Convert.ToString(Invoke(GetProgressType(), "GetState", node));
        }

        private static bool TryUnlock(string nodeId, out string message)
        {
            object[] args = { nodeId, null };
            bool result = Convert.ToBoolean(Invoke(GetProgressType(), "TryUnlock", args));
            message = Convert.ToString(args[1]);
            return result;
        }

        private static bool AddProgressPoint(string sourceId, string reason)
        {
            return Convert.ToBoolean(Invoke(GetProgressType(), "AddProgressPoint", sourceId, reason));
        }

        private static object SaveProgress()
        {
            return Invoke(GetProgressType(), "SaveToJSON");
        }

        private static void LoadProgress(object data)
        {
            Invoke(GetProgressType(), "LoadFromJSON", data);
        }

        private static bool IsAchievementCompleted(string achievementId)
        {
            return Convert.ToBoolean(Invoke(GetAchievementManagerType(), "IsCompleted", achievementId));
        }

        private static bool IsAchievementRewardClaimed(string achievementId)
        {
            return Convert.ToBoolean(Invoke(GetAchievementManagerType(), "IsRewardClaimed", achievementId));
        }

        private static bool IsProductUnlocked(ScriptableObject product)
        {
            return Convert.ToBoolean(Invoke(GetProgressType(), "IsProductUnlocked", product));
        }

        private static bool TryGetProductLockedMessage(ScriptableObject product, out string message)
        {
            object[] args = { product, null };
            bool result = Convert.ToBoolean(Invoke(GetProgressType(), "TryGetProductLockedMessage", args));
            message = Convert.ToString(args[1]);
            return result;
        }

        private static bool TryHire(Type employeeManagerType, object manager, string employeeId, out string message)
        {
            object[] args = { employeeId, null };
            bool result = Convert.ToBoolean(employeeManagerType.GetMethod("TryHire", BindingFlags.Public | BindingFlags.Instance).Invoke(manager, args));
            message = Convert.ToString(args[1]);
            return result;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(field, "Could not find field " + fieldName);
            field.SetValue(target, value);
        }

        private static void SetProperty(object target, string propertyName, object value)
        {
            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(property, "Could not find property " + propertyName);
            property.SetValue(target, value);
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
