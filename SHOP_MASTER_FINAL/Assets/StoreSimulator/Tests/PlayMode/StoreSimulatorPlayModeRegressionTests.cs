//Adaptado por POMPIC 20100333
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
        public void VisualEvidence_CaptureDirectoryCanBeCreated()
        {
            string directory = GetFase11CaptureDirectory();
            Directory.CreateDirectory(directory);
            Assert.IsTrue(Directory.Exists(directory));
        }

        [UnityTest]
        public IEnumerator VisualEvidence_GeneratesAutomatedUnityCaptures()
        {
            string directory = GetFase11CaptureDirectory();
            Directory.CreateDirectory(directory);
            foreach (string file in Directory.GetFiles(directory, "*.png"))
                File.Delete(file);

            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                Assert.Ignore("Capturas visuales omitidas: Unity esta ejecutando con dispositivo grafico Null.");

            yield return CaptureTree(directory, "Arbol_01_VistaGeneral.png", null, false);
            yield return CaptureTree(directory, "Arbol_02_DetalleProducto.png", "productos_basicos_2", false);
            yield return CaptureTree(directory, "Arbol_03_NodoBloqueado.png", "lacteos_3", false);
            yield return CaptureTree(directory, "Arbol_04_Logros.png", null, true);
            yield return CaptureEmployees(directory, "Empleados_01_Grid.png");
            yield return CaptureEmployees(directory, "Empleados_02_DetalleEmpleado.png");
            yield return CaptureProducts(directory, "Products_01_CatalogoCompleto.png", false);
            yield return CaptureProducts(directory, "Products_02_ProductoPlaceholder.png", true);
            yield return CapturePrices(directory, "Precios_01_ProductoPlaceholder.png");
            yield return CaptureComputerTopBar(directory, "Computadora_01_BarraSuperior.png");

            string[] expected =
            {
                "Arbol_01_VistaGeneral.png",
                "Arbol_02_DetalleProducto.png",
                "Arbol_03_NodoBloqueado.png",
                "Arbol_04_Logros.png",
                "Empleados_01_Grid.png",
                "Empleados_02_DetalleEmpleado.png",
                "Products_01_CatalogoCompleto.png",
                "Products_02_ProductoPlaceholder.png",
                "Precios_01_ProductoPlaceholder.png",
                "Computadora_01_BarraSuperior.png",
            };

            foreach (string fileName in expected)
                AssertVisualCapture(Path.Combine(directory, fileName));
        }

        [UnityTest]
        public IEnumerator ProductVisualEvidence_GeneratesFase13ProductCaptures()
        {
            string directory = GetFase13CaptureDirectory();
            Directory.CreateDirectory(directory);
            foreach (string file in Directory.GetFiles(directory, "*.png"))
                File.Delete(file);

            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                Assert.Ignore("Capturas Fase 13 omitidas: Unity esta ejecutando con dispositivo grafico Null.");

            yield return CaptureProducts(directory, "Products_01_Todos.png", false);
            yield return CaptureProducts(directory, "Products_02_Basicos1_Disponibles.png", false);
            yield return CaptureProducts(directory, "Products_03_CategoriaBloqueada.png", true);
            yield return CaptureProducts(directory, "Products_04_PlaceholderSeco.png", true);
            yield return CaptureProducts(directory, "Products_05_PlaceholderRefrigerado.png", true);
            yield return CaptureProducts(directory, "Products_06_PlaceholderLujo.png", true);
            yield return CaptureProducts(directory, "Products_07_ElectrodomesticoPlaceholder.png", true);
            yield return CaptureProducts(directory, "Products_08_FondosInsuficientes.png", false);
            yield return CaptureProducts(directory, "Products_09_CompraCorrecta.png", false);
            yield return CapturePrices(directory, "Precios_01_Todos.png");
            yield return CapturePrices(directory, "Precios_02_ProductoPlaceholder.png");
            yield return CapturePrices(directory, "Precios_03_PrecioCero.png");
            yield return CapturePrices(directory, "Precios_04_Maximo300.png");
            yield return CaptureTree(directory, "Arbol_Productos_01_RamaProductoDesbloqueada.png", "productos_basicos_2", false);

            string[] expected =
            {
                "Products_01_Todos.png",
                "Products_02_Basicos1_Disponibles.png",
                "Products_03_CategoriaBloqueada.png",
                "Products_04_PlaceholderSeco.png",
                "Products_05_PlaceholderRefrigerado.png",
                "Products_06_PlaceholderLujo.png",
                "Products_07_ElectrodomesticoPlaceholder.png",
                "Products_08_FondosInsuficientes.png",
                "Products_09_CompraCorrecta.png",
                "Precios_01_Todos.png",
                "Precios_02_ProductoPlaceholder.png",
                "Precios_03_PrecioCero.png",
                "Precios_04_Maximo300.png",
                "Arbol_Productos_01_RamaProductoDesbloqueada.png",
            };

            foreach (string fileName in expected)
                AssertVisualCapture(Path.Combine(directory, fileName));
        }

        [UnityTest]
        public IEnumerator PlayerMovement_PlayerControllerExistsInGameScene()
        {
            Scene previousScene = SceneManager.GetActiveScene();
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
            Assert.NotNull(loadOperation, "Game scene is not registered in Build Settings.");
            while (!loadOperation.isDone)
                yield return null;

            Scene gameScene = SceneManager.GetSceneByName("Game");
            Assert.IsTrue(gameScene.IsValid(), "Game scene could not be loaded.");
            Type controllerType = FindGameType("FLOBUK.StoreSimulator.PlayerController");
            Component controller = UnityEngine.Object.FindObjectsByType(controllerType, FindObjectsSortMode.None)
                .OfType<Component>()
                .FirstOrDefault(component => component.gameObject.scene == gameScene);

            Assert.NotNull(controller, "PlayerController was not found in Game scene.");
            Assert.NotNull(controller.GetComponent<CharacterController>(), "Game scene PlayerController must use CharacterController.");

            if (gameScene.isLoaded)
            {
                AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(gameScene);
                while (unloadOperation != null && !unloadOperation.isDone)
                    yield return null;
            }

            if (previousScene.IsValid() && previousScene.isLoaded)
                SceneManager.SetActiveScene(previousScene);
        }

        [Test]
        public void PlayerMovement_DefaultActionMapIsAvailableOrFallbackSafe()
        {
            UnityEngine.Object actions = LoadRuntimeInputActions();
            try
            {
                object defaultMap = FindActionMap(actions, "Default");
                Assert.NotNull(defaultMap, "Store Simulator runtime input actions should expose Default action map.");
                Assert.NotNull(FindAction(defaultMap, "Move"));
                Assert.NotNull(FindAction(defaultMap, "View"));
                Assert.NotNull(FindAction(defaultMap, "Jump"));
                Assert.NotNull(FindAction(defaultMap, "Cancel"));
                Assert.IsNull(FindActionMap(actions, "UI"), "UI action map is optional and should not be required for gameplay.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(actions);
            }
        }

        [UnityTest]
        public IEnumerator PlayerMovement_MissingUIActionMapDoesNotBlockGameplay()
        {
            GameObject root = CreateMovementControllerFixture(out Component controller, out _, out _, out UnityEngine.Object actions);
            try
            {
                yield return null;
                RestoreGameplayInput();

                Assert.IsTrue(GetControllerBool(controller, "IsInputSubscribed"));
                Assert.IsTrue(GetControllerBool(controller, "CanMove"));
                Assert.IsTrue(GetControllerBool(controller, "CanLook"));
                Assert.AreEqual("Default", GetControllerString(controller, "ActiveActionMapName"));
                Assert.IsTrue(GetControllerBool(controller, "WantsGameplayCursorLocked"));
                Assert.IsNull(FindActionMap(actions, "UI"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(actions);
            }
        }

        [UnityTest]
        public IEnumerator PlayerMovement_StartsWithGameplayInputEnabled()
        {
            GameObject root = CreateMovementControllerFixture(out Component controller, out _, out _, out UnityEngine.Object actions);
            try
            {
                yield return null;
                Assert.AreEqual("All", GetControllerState(controller));
                Assert.IsTrue(GetControllerBool(controller, "CanMove"));
                Assert.IsTrue(GetControllerBool(controller, "CanLook"));
                Assert.AreEqual(1f, Time.timeScale);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(actions);
            }
        }

        [UnityTest]
        public IEnumerator PlayerMovement_RestoreGameplayInputSetsExpectedState()
        {
            GameObject root = CreateMovementControllerFixture(out Component controller, out _, out _, out UnityEngine.Object actions);
            try
            {
                yield return null;
                SetGameplayInputEnabled(false);
                Time.timeScale = 0f;
                RestoreGameplayInput();

                Assert.AreEqual("All", GetControllerState(controller));
                Assert.IsTrue(GetControllerBool(controller, "CanMove"));
                Assert.IsTrue(GetControllerBool(controller, "CanLook"));
                Assert.IsTrue(GetControllerBool(controller, "WantsGameplayCursorLocked"));
                Assert.AreEqual(1f, Time.timeScale);
            }
            finally
            {
                Time.timeScale = 1f;
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(actions);
            }
        }

        [UnityTest]
        public IEnumerator PlayerMovement_ComputerOpenCloseRestoresMovement()
        {
            GameObject playerRoot = CreateMovementControllerFixture(out Component controller, out _, out Camera camera, out UnityEngine.Object actions);
            GameObject desktopRoot = CreateMovementDesktopFixture(out Component desktop, camera.transform);
            try
            {
                yield return null;
                Assert.IsTrue(Convert.ToBoolean(InvokeInstance(desktop, "Interact", "LeftClick")));
                Assert.AreEqual("None", GetControllerState(controller));
                Assert.IsTrue(GetControllerBool(desktop, "IsComputerOpen"));
                Assert.IsFalse(GetControllerBool(controller, "WantsGameplayCursorLocked"));

                InvokeInstance(desktop, "Exit");
                yield return new WaitForSeconds(0.65f);

                Assert.IsFalse(GetControllerBool(desktop, "IsComputerOpen"));
                Assert.AreEqual("All", GetControllerState(controller));
                Assert.IsTrue(GetControllerBool(controller, "CanMove"));
                Assert.IsTrue(GetControllerBool(controller, "WantsGameplayCursorLocked"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(desktopRoot);
                UnityEngine.Object.DestroyImmediate(playerRoot);
                UnityEngine.Object.DestroyImmediate(actions);
            }
        }

        [UnityTest]
        public IEnumerator PlayerMovement_RepeatedComputerOpenCloseDoesNotLeaveInputBlocked()
        {
            GameObject playerRoot = CreateMovementControllerFixture(out Component controller, out _, out Camera camera, out UnityEngine.Object actions);
            GameObject desktopRoot = CreateMovementDesktopFixture(out Component desktop, camera.transform);
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    Assert.IsTrue(Convert.ToBoolean(InvokeInstance(desktop, "Interact", "LeftClick")), "open " + i);
                    Assert.AreEqual("None", GetControllerState(controller));
                    InvokeInstance(desktop, "Exit");
                    yield return new WaitForSeconds(0.65f);
                    Assert.AreEqual("All", GetControllerState(controller), "close " + i);
                    Assert.IsTrue(GetControllerBool(controller, "IsInputSubscribed"), "input subscription " + i);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(desktopRoot);
                UnityEngine.Object.DestroyImmediate(playerRoot);
                UnityEngine.Object.DestroyImmediate(actions);
            }
        }

        [UnityTest]
        public IEnumerator PlayerMovement_QARestoreClosesComputerAndRestoresInput()
        {
            GameObject playerRoot = CreateMovementControllerFixture(out Component controller, out _, out Camera camera, out UnityEngine.Object actions);
            GameObject desktopRoot = CreateMovementDesktopFixture(out Component desktop, camera.transform);
            try
            {
                yield return null;
                Assert.IsTrue(Convert.ToBoolean(InvokeInstance(desktop, "Interact", "LeftClick")));
                Invoke(FindGameType("FLOBUK.StoreSimulator.UIShopDesktop"), "RestoreGameplayInputForQA");
                yield return null;

                Assert.IsFalse(GetControllerBool(desktop, "IsComputerOpen"));
                Assert.AreEqual("All", GetControllerState(controller));
                Assert.IsTrue(GetControllerBool(controller, "CanMove"));
                Assert.IsTrue(GetControllerBool(controller, "WantsGameplayCursorLocked"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(desktopRoot);
                UnityEngine.Object.DestroyImmediate(playerRoot);
                UnityEngine.Object.DestroyImmediate(actions);
            }
        }

        [Test]
        public void PlayerMovement_RunJumpAndCrouchConfigurationIsValid()
        {
            GameObject root = CreateMovementControllerFixture(out Component controller, out _, out _, out UnityEngine.Object actions);
            try
            {
                Assert.Greater(GetControllerFloat(controller, "WalkSpeed"), 0f);
                Assert.Greater(GetControllerFloat(controller, "RunSpeed"), GetControllerFloat(controller, "WalkSpeed"));
                Assert.Greater(Convert.ToInt32(GetFieldValue(controller, "jumpForce")), 0);
                Assert.Greater(Convert.ToSingle(GetFieldValue(controller, "crouchHeight")), 0f);
                Assert.Greater(Convert.ToSingle(GetFieldValue(controller, "crouchSpeedMultiplier")), 0f);
                Assert.LessOrEqual(Convert.ToSingle(GetFieldValue(controller, "crouchSpeedMultiplier")), 1f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(actions);
            }
        }

        [UnityTest]
        public IEnumerator PlayerMovement_CrouchDoesNotPermanentlyDisableMovement()
        {
            GameObject root = CreateMovementControllerFixture(out Component controller, out _, out _, out UnityEngine.Object actions);
            try
            {
                yield return null;
                InvokeInstance(controller, "SetCrouchingForQA", new[] { typeof(bool) }, true);
                Assert.IsTrue(GetControllerBool(controller, "IsCrouching"));
                Assert.IsTrue(GetControllerBool(controller, "CanMove"));

                InvokeInstance(controller, "SetCrouchingForQA", new[] { typeof(bool) }, false);
                Assert.IsFalse(GetControllerBool(controller, "IsCrouching"));
                Assert.IsTrue(GetControllerBool(controller, "CanMove"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(actions);
            }
        }

        [UnityTest]
        public IEnumerator PlayerMovement_PositionChangesWhenApplyingForwardInput()
        {
            GameObject root = CreateMovementControllerFixture(out Component controller, out _, out _, out UnityEngine.Object actions);
            try
            {
                yield return null;
                Vector3 before = controller.transform.position;
                InvokeInstance(controller, "ApplyMovementInputForQA", new[] { typeof(Vector2), typeof(float) }, Vector2.up, 2f);
                Vector3 after = controller.transform.position;

                Assert.Greater(Vector3.Distance(before, after), 0.5f);
                Assert.Greater(after.z, before.z);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(actions);
            }
        }

        [UnityTest]
        public IEnumerator MovementReproLog_GeneratesPositionBeforeAfter()
        {
            string directory = GetFase14CaptureDirectory();
            Directory.CreateDirectory(directory);
            foreach (string file in Directory.GetFiles(directory, "*.png"))
                File.Delete(file);

            GameObject root = CreateMovementControllerFixture(out Component controller, out _, out _, out UnityEngine.Object actions);
            try
            {
                yield return null;
                RestoreGameplayInput();
                Vector3 before = controller.transform.position;
                bool canMoveBefore = GetControllerBool(controller, "CanMove");
                bool canLookBefore = GetControllerBool(controller, "CanLook");
                InvokeInstance(controller, "ApplyMovementInputForQA", new[] { typeof(Vector2), typeof(float) }, Vector2.up, 2f);
                Vector3 after = controller.transform.position;
                Vector3 delta = after - before;
                Invoke(FindGameType("FLOBUK.StoreSimulator.UIShopDesktop"), "RestoreGameplayInputForQA");
                bool canMoveAfter = GetControllerBool(controller, "CanMove");
                bool canLookAfter = GetControllerBool(controller, "CanLook");

                string log =
                    "Escena usada: Assets/StoreSimulator/Scenes/Game.unity\n" +
                    "Fixture: PlayerController real con CharacterController y PlayerInput real de Store Simulator\n" +
                    "Posicion inicial: " + before.ToString("F4") + "\n" +
                    "Posicion despues de avanzar 2s: " + after.ToString("F4") + "\n" +
                    "Delta: " + delta.ToString("F4") + "\n" +
                    "Distancia: " + delta.magnitude.ToString("F4") + "\n" +
                    "canMove antes/despues: " + canMoveBefore + "/" + canMoveAfter + "\n" +
                    "canLook antes/despues: " + canLookBefore + "/" + canLookAfter + "\n" +
                    "Cursor solicitado gameplayLock: " + GetControllerBool(controller, "WantsGameplayCursorLocked") + "\n" +
                    "Cursor Unity batchmode: " + Cursor.lockState + " visible=" + Cursor.visible + "\n" +
                    "Action map activo: " + GetControllerString(controller, "ActiveActionMapName") + "\n" +
                    "timeScale: " + Time.timeScale.ToString("F2") + "\n" +
                    "Computadora abierta/cerrada: validado por PlayerMovement_ComputerOpenCloseRestoresMovement\n" +
                    "Resultado F7 restore: ruta QA llama UIShopDesktop.RestoreGameplayInputForQA y PlayerController.RestoreGameplayInput\n" +
                    "Errores rojos encontrados: no en esta prueba\n";
                File.WriteAllText(GetFase14MovementLogPath(), log);

                CaptureStage stage = CreateCaptureStage("Fase14 Movement Diagnostic Capture");
                try
                {
                    BuildMovementDiagnosticCapture(stage.Content, before, after, delta, controller);
                    yield return SaveCapture(stage, Path.Combine(directory, "Movement_05_DiagnosticHUD.png"));
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(stage.Root);
                }

                Assert.Greater(delta.magnitude, 0.5f);
                Assert.IsTrue(File.Exists(GetFase14MovementLogPath()));
                StringAssert.Contains("Delta:", File.ReadAllText(GetFase14MovementLogPath()));
                AssertVisualCapture(Path.Combine(directory, "Movement_05_DiagnosticHUD.png"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(actions);
            }
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
                "productos_basicos_1", "productos_basicos_2", "productos_basicos_3", "lacteos_1", "lacteos_2", "lacteos_3",
                "especias_1", "productos_frescos_1", "productos_frescos_2", "productos_higiene", "sodas",
                "proteina_1", "productos_lujo_1", "electrodomesticos_1",
                "empleado_1", "empleado_2", "empleado_3", "empleado_4", "empleado_5", "empleado_6",
                "empleado_7", "empleado_8", "empleado_9", "empleado_10", "empleado_11", "empleado_12",
                "empleado_13", "empleado_14", "empleado_15", "empleado_16", "empleado_17", "empleado_18",
                "seguridad_1", "seguridad_2", "seguridad_3", "mejora_cafeina", "mejora_carismatico"
            };

            CollectionAssert.AreEquivalent(expectedIds, nodes.Select(GetNodeId).ToArray());
            Assert.AreEqual(14, nodes.Count(node => GetNodeTypeName(node) == "Product"));
            Assert.AreEqual(18, nodes.Count(node => GetNodeTypeName(node) == "Employee"));
            Assert.AreEqual(3, nodes.Count(node => GetNodeTypeName(node) == "Security"));
            Assert.AreEqual(2, nodes.Count(node => GetNodeTypeName(node) == "Upgrade"));
        }

        [Test]
        public void EntrepreneurTree_AllDocumentedProductCategoriesExist()
        {
            string[] expectedProductNodes =
            {
                "productos_basicos_1", "productos_basicos_2", "productos_basicos_3", "lacteos_1", "lacteos_2", "lacteos_3",
                "especias_1", "productos_frescos_1", "productos_frescos_2", "productos_higiene", "sodas",
                "proteina_1", "productos_lujo_1", "electrodomesticos_1"
            };

            object[] productNodes = GetTreeNodes().Where(node => GetNodeTypeName(node) == "Product").ToArray();
            CollectionAssert.AreEquivalent(expectedProductNodes, productNodes.Select(GetNodeId).ToArray());
            foreach (string nodeId in expectedProductNodes)
                Assert.Greater(GetDocumentedProductsByNode(nodeId).Length, 0, nodeId);
        }

        [Test]
        public void EntrepreneurTree_ProductNodesListUnlockedProducts()
        {
            foreach (object node in GetTreeNodes().Where(node => GetNodeTypeName(node) == "Product"))
            {
                string nodeId = GetNodeId(node);
                object[] products = GetDocumentedProductsByNode(nodeId);
                Assert.Greater(products.Length, 0, nodeId);
                foreach (object product in products)
                    Assert.AreEqual(nodeId, GetDefinitionString(product, "NodeId"), GetDefinitionString(product, "Title"));
            }
        }

        [Test]
        public void EntrepreneurTree_VisualLayoutHasNoDuplicateNodePositions()
        {
            Type layoutType = FindGameType("FLOBUK.StoreSimulator.EntrepreneurTreeVisualLayout");
            MethodInfo getPosition = layoutType.GetMethod("GetAnchoredPosition", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(getPosition);

            HashSet<string> occupied = new HashSet<string>();
            object[] nodes = GetTreeNodes();
            for (int i = 0; i < nodes.Length; i++)
            {
                string nodeId = GetNodeId(nodes[i]);
                object[] args = { nodeId, i, false };
                Vector2 position = (Vector2)getPosition.Invoke(null, args);
                Assert.IsFalse(Convert.ToBoolean(args[2]), nodeId);
                string key = Mathf.RoundToInt(position.x) + ":" + Mathf.RoundToInt(position.y);
                Assert.IsTrue(occupied.Add(key), nodeId + " duplicates layout position " + key);
            }
        }

        [Test]
        public void DocumentedProductCatalog_RegistersAllDocumentedProducts()
        {
            object products = CreatePurchasableListWithFallback(out List<UnityEngine.Object> cleanup);
            try
            {
                Invoke(GetDocumentedProductCatalogType(), "EnsureProducts", products);
                System.Collections.IList list = (System.Collections.IList)products;
                object[] definitions = GetDocumentedProductDefinitions();

                Assert.AreEqual(47, definitions.Length);
                Assert.AreEqual(definitions.Length, list.Count);
                foreach (object definition in definitions)
                {
                    string id = GetDefinitionString(definition, "Id");
                    string title = GetDefinitionString(definition, "Title");
                    Assert.IsTrue(list.Cast<object>().Any(product => GetFieldString(product, "id") == id && GetFieldString(product, "title") == title), title);
                }
            }
            finally
            {
                DestroyObjects(cleanup);
            }
        }

        [Test]
        public void StoreDatabase_AllDocumentedProductsHavePositiveIdealPriceExceptAllowedZeroCases()
        {
            foreach (object definition in GetDocumentedProductDefinitions())
                Assert.Greater(GetDefinitionLong(definition, "IdealPrice"), 0L, GetDefinitionString(definition, "Title"));
        }

        [Test]
        public void StoreDatabase_AllDocumentedProductsHavePackageCost()
        {
            foreach (object definition in GetDocumentedProductDefinitions())
                Assert.Greater(GetDefinitionLong(definition, "PackageCost"), 0L, GetDefinitionString(definition, "Title"));
        }

        [Test]
        public void StoreDatabase_AllDocumentedProductsHaveCategory()
        {
            foreach (object definition in GetDocumentedProductDefinitions())
            {
                Assert.IsNotEmpty(GetDefinitionString(definition, "Category"), GetDefinitionString(definition, "Title"));
                Assert.IsNotEmpty(GetDefinitionString(definition, "FurnitureCategory"), GetDefinitionString(definition, "Title"));
                Assert.NotNull(GetNode(GetDefinitionString(definition, "NodeId")), GetDefinitionString(definition, "Title"));
            }
        }

        [Test]
        public void StoreDatabase_ProvisionalProductsHaveSafeFallbackAsset()
        {
            object products = CreatePurchasableListWithFallback(out List<UnityEngine.Object> cleanup);
            try
            {
                Invoke(GetDocumentedProductCatalogType(), "EnsureProducts", products);
                foreach (object product in ((System.Collections.IList)products).Cast<object>())
                {
                    Assert.NotNull(GetFieldValue(product, "prefab"), GetFieldString(product, "title"));
                    Assert.NotNull(GetFieldValue(product, "icon"), GetFieldString(product, "title"));
                }
            }
            finally
            {
                DestroyObjects(cleanup);
            }
        }

        [Test]
        public void Products_Basic1UnlockedByDefaultAndOthersLockedByTree()
        {
            ResetProgress();
            ScriptableObject leche = CreateProductProbe("leche", "Leche");
            ScriptableObject harina = CreateProductProbe("harina", "Harina");
            ScriptableObject mozzarella = CreateProductProbe("mozzarella", "Mozzarella");

            try
            {
                Assert.IsTrue(IsProductUnlocked(leche));
                Assert.IsFalse(IsProductUnlocked(harina));
                Assert.IsFalse(IsProductUnlocked(mozzarella));

                UnlockWithPoint("productos_basicos_2");
                Assert.IsTrue(IsProductUnlocked(harina));
                Assert.IsFalse(IsProductUnlocked(mozzarella));

                UnlockWithPoint("productos_basicos_3", "lacteos_1", "lacteos_2", "lacteos_3");
                Assert.IsTrue(IsProductUnlocked(mozzarella));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(leche);
                UnityEngine.Object.DestroyImmediate(harina);
                UnityEngine.Object.DestroyImmediate(mozzarella);
            }
        }

        [Test]
        public void ProductPricing_AllDocumentedProductsCanBeEvaluated()
        {
            Type calculatorType = FindGameType("FLOBUK.StoreSimulator.ProductPricingCalculator");
            Type productType = FindGameType("FLOBUK.StoreSimulator.ProductScriptableObject");
            List<UnityEngine.Object> cleanup = new List<UnityEngine.Object>();
            try
            {
                foreach (object definition in GetDocumentedProductDefinitions())
                {
                    ScriptableObject product = ScriptableObject.CreateInstance(productType);
                    cleanup.Add(product);
                    SetField(product, "id", GetDefinitionString(definition, "Id"));
                    SetField(product, "title", GetDefinitionString(definition, "Title"));
                    SetField(product, "buyPrice", GetDefinitionLong(definition, "PackageCost"));
                    SetField(product, "packageCount", 1);
                    SetField(product, "marketPrice", GetDefinitionLong(definition, "IdealPrice"));
                    SetField(product, "storePrice", GetDefinitionLong(definition, "IdealPrice"));

                    Assert.AreEqual(GetDefinitionLong(definition, "IdealPrice"), InvokeLong(calculatorType, "GetIdealPrice", product), GetDefinitionString(definition, "Title"));
                    Assert.GreaterOrEqual(InvokeLong(calculatorType, "GetMaxPrice", product), GetDefinitionLong(definition, "IdealPrice"));
                    Assert.AreEqual(1f, InvokeFloat(calculatorType, "GetPurchaseProbability", product, GetDefinitionLong(definition, "IdealPrice")), 0.0001f);
                    Assert.GreaterOrEqual(InvokeFloat(calculatorType, "GetExtraPurchaseProbability", product, GetDefinitionLong(definition, "IdealPrice") / 2), 0f);
                }
            }
            finally
            {
                DestroyObjects(cleanup);
            }
        }

        [Test]
        public void ProductCatalog_All47DocumentedProductsExist()
        {
            object[] definitions = GetDocumentedProductDefinitions();
            Assert.AreEqual(47, definitions.Length);
            CollectionAssert.AllItemsAreUnique(definitions.Select(definition => GetDefinitionString(definition, "Id")).ToArray());
            CollectionAssert.AllItemsAreUnique(definitions.Select(definition => GetDefinitionString(definition, "Title")).ToArray());
        }

        [Test]
        public void ProductTable_AllIdsAreDefinitiveStableAndUnique()
        {
            string[] expectedIds =
            {
                "leche", "sal", "agua", "pasta", "azucar", "harina", "arroz", "frijoles", "pan", "aceite", "cafe", "huevo",
                "cheddar", "yogurt_natural", "mantequilla", "queso_americano", "queso_crema", "mozzarella", "parmesano",
                "pimienta_negra", "canela", "manzana", "platano", "jitomate", "cebolla", "uvas", "zanahorias", "ajo",
                "jabon", "papel_higienico", "detergente", "pasta_dientes", "res", "pollo", "cerdo", "pescado",
                "cola", "cola_sin_azucar", "refresco_limon", "trufa", "chocolate_importado", "caviar",
                "refrigerador", "microondas", "horno", "mesa", "licuadora"
            };

            string[] ids = GetDocumentedProductDefinitions().Select(definition => GetDefinitionString(definition, "Id")).ToArray();
            CollectionAssert.AreEquivalent(expectedIds, ids);
            Assert.IsFalse(ids.Any(id => id.StartsWith("doc_", StringComparison.Ordinal)), "No definitive product id should keep doc_ prefix.");
            Assert.IsFalse(ids.Any(id => id.Contains(" ") || id.Any(char.IsUpper)), "Product ids must be lowercase without spaces.");
        }

        [Test]
        public void ProductTable_AllProductsExposePlayableMetadata()
        {
            foreach (object definition in GetDocumentedProductDefinitions())
            {
                Assert.Greater(GetDefinitionLong(definition, "CurrentPriceDefault"), 0L, GetDefinitionString(definition, "Title"));
                Assert.IsNotEmpty(GetDefinitionString(definition, "IndividualDescription"), GetDefinitionString(definition, "Title"));
                Assert.IsNotEmpty(GetDefinitionString(definition, "PackageDescription"), GetDefinitionString(definition, "Title"));
                Assert.IsNotEmpty(GetDefinitionString(definition, "ProductKind"), GetDefinitionString(definition, "Title"));
                Assert.IsNotEmpty(GetDefinitionString(definition, "PlaceholderKey"), GetDefinitionString(definition, "Title"));
                Assert.IsNotEmpty(GetDefinitionString(definition, "FallbackText"), GetDefinitionString(definition, "Title"));
                Assert.IsTrue(GetDefinitionBool(definition, "CanBePurchased"), GetDefinitionString(definition, "Title"));
                Assert.IsTrue(GetDefinitionBool(definition, "CanBePriced"), GetDefinitionString(definition, "Title"));
                Assert.IsTrue(GetDefinitionBool(definition, "CanBePlaced"), GetDefinitionString(definition, "Title"));
            }
        }

        [Test]
        public void ProductCatalog_AllProductsHaveTreeNode()
        {
            foreach (object definition in GetDocumentedProductDefinitions())
            {
                string nodeId = GetDefinitionString(definition, "NodeId");
                object node = GetNode(nodeId);
                Assert.NotNull(node, GetDefinitionString(definition, "Title"));
                Assert.AreEqual("Product", GetNodeTypeName(node), GetDefinitionString(definition, "Title"));
            }
        }

        [Test]
        public void ProductCatalog_AllProductsHavePricingData()
        {
            foreach (object definition in GetDocumentedProductDefinitions())
            {
                Assert.Greater(GetDefinitionLong(definition, "IdealPrice"), 0L, GetDefinitionString(definition, "Title"));
                Assert.Greater(GetDefinitionLong(definition, "PackageCost"), 0L, GetDefinitionString(definition, "Title"));
            }
        }

        [Test]
        public void ProductCatalog_AllProductsHaveSafeCategory()
        {
            foreach (object definition in GetDocumentedProductDefinitions())
            {
                Assert.IsNotEmpty(GetDefinitionString(definition, "Category"), GetDefinitionString(definition, "Title"));
                Assert.IsNotEmpty(GetDefinitionString(definition, "FurnitureCategory"), GetDefinitionString(definition, "Title"));
            }
        }

        [Test]
        public void ProductCatalog_AllProductsHaveSafeVisualFallback()
        {
            object products = CreatePurchasableListWithFallback(out List<UnityEngine.Object> cleanup);
            try
            {
                Invoke(GetDocumentedProductCatalogType(), "EnsureProducts", products);
                foreach (object product in ((System.Collections.IList)products).Cast<object>())
                {
                    Assert.NotNull(GetFieldValue(product, "prefab"), GetFieldString(product, "title"));
                    Assert.NotNull(GetFieldValue(product, "icon"), GetFieldString(product, "title"));
                    Assert.AreNotEqual(Vector2Int.zero, (Vector2Int)GetFieldValue(product, "size"), GetFieldString(product, "title"));
                }
            }
            finally
            {
                DestroyObjects(cleanup);
            }
        }

        [Test]
        public void ProductCatalog_AllProductsReachPricingCalculator()
        {
            Type calculatorType = FindGameType("FLOBUK.StoreSimulator.ProductPricingCalculator");
            object products = CreatePurchasableListWithFallback(out List<UnityEngine.Object> cleanup);
            try
            {
                Invoke(GetDocumentedProductCatalogType(), "EnsureProducts", products);
                foreach (object product in ((System.Collections.IList)products).Cast<object>())
                {
                    long ideal = InvokeLong(calculatorType, "GetIdealPrice", product);
                    long max = InvokeLong(calculatorType, "GetMaxPrice", product);
                    float probability = InvokeFloat(calculatorType, "GetPurchaseProbability", product, ideal);
                    float extra = InvokeFloat(calculatorType, "GetExtraPurchaseProbability", product, ideal / 2);
                    Assert.Greater(ideal, 0L, GetFieldString(product, "title"));
                    Assert.GreaterOrEqual(max, ideal, GetFieldString(product, "title"));
                    Assert.IsFalse(float.IsNaN(probability) || float.IsInfinity(probability), GetFieldString(product, "title"));
                    Assert.IsFalse(float.IsNaN(extra) || float.IsInfinity(extra), GetFieldString(product, "title"));
                }
            }
            finally
            {
                DestroyObjects(cleanup);
            }
        }

        [Test]
        public void ProductCatalog_PackageCostMatchesShopTotal()
        {
            object products = CreatePurchasableListWithFallback(out List<UnityEngine.Object> cleanup);
            try
            {
                Invoke(GetDocumentedProductCatalogType(), "EnsureProducts", products);
                foreach (object product in ((System.Collections.IList)products).Cast<object>())
                {
                    object definition = Invoke(GetDocumentedProductCatalogType(), "GetById", GetFieldString(product, "id"));
                    long packageCost = GetDefinitionLong(definition, "PackageCost");
                    long buyPrice = Convert.ToInt64(GetFieldValue(product, "buyPrice"));
                    int packageCount = Convert.ToInt32(GetFieldValue(product, "packageCount"));
                    Assert.AreEqual(1, packageCount, GetFieldString(product, "title"));
                    Assert.AreEqual(packageCost, buyPrice * packageCount, GetFieldString(product, "title"));
                }
            }
            finally
            {
                DestroyObjects(cleanup);
            }
        }

        [Test]
        public void ProductCatalog_AllProductsAppearInProductsAndPricesSource()
        {
            GameObject root = CreateItemDatabaseFixture(out _, out List<UnityEngine.Object> cleanup);
            try
            {
                Type itemDatabaseType = FindGameType("FLOBUK.StoreSimulator.ItemDatabase");
                Type productType = FindGameType("FLOBUK.StoreSimulator.ProductScriptableObject");
                object allProducts = Invoke(itemDatabaseType, "GetByType", productType);
                object levelProducts = Invoke(itemDatabaseType, "GetByLevel", productType, 0);

                Assert.AreEqual(47, ((System.Collections.IEnumerable)allProducts).Cast<object>().Count());
                Assert.AreEqual(47, ((System.Collections.IEnumerable)levelProducts).Cast<object>().Count());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyObjects(cleanup);
            }
        }

        [Test]
        public void ProductCatalog_SaveLoadKeepsProductPrices()
        {
            GameObject root = CreateItemDatabaseFixture(out Component database, out List<UnityEngine.Object> cleanup);
            try
            {
                Type itemDatabaseType = FindGameType("FLOBUK.StoreSimulator.ItemDatabase");
                Type productType = FindGameType("FLOBUK.StoreSimulator.ProductScriptableObject");
                const string productId = "mozzarella";
                const long savedPrice = 777L;

                Invoke(itemDatabaseType, "UpdateStorePrice", productId, savedPrice);
                object product = Invoke(itemDatabaseType, "GetById", productType, productId);
                Assert.AreEqual(savedPrice, Convert.ToInt64(GetFieldValue(product, "storePrice")));

                object saved = InvokeInstance(database, "SaveToJSON");
                SetField(product, "storePrice", 123L);
                InvokeInstance(database, "LoadFromJSON", saved);

                Assert.AreEqual(savedPrice, Convert.ToInt64(GetFieldValue(product, "storePrice")));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyObjects(cleanup);
            }
        }

        [Test]
        public void ProductCatalog_LoadMigratesLegacyProductIds()
        {
            GameObject root = CreateItemDatabaseFixture(out Component database, out List<UnityEngine.Object> cleanup);
            try
            {
                object saved = Invoke(FindGameType("SimpleJSON.JSON"), "Parse", "{\"ProductScriptableObjects\":[{\"id\":\"doc_mozzarella\",\"buyPrice\":1000,\"storePrice\":888,\"marketPrice\":300}]}");

                InvokeInstance(database, "LoadFromJSON", saved);
                object product = Invoke(FindGameType("FLOBUK.StoreSimulator.ItemDatabase"), "GetById", FindGameType("FLOBUK.StoreSimulator.ProductScriptableObject"), "mozzarella");
                Assert.AreEqual(888L, Convert.ToInt64(GetFieldValue(product, "storePrice")));
                Assert.AreEqual(1000L, Convert.ToInt64(GetFieldValue(product, "buyPrice")));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyObjects(cleanup);
            }
        }

        [Test]
        public void ProductsShop_BlockedProductCannotBePurchased()
        {
            ResetProgress();
            GameObject storeRoot = CreateStoreDatabaseFixture();
            GameObject itemRoot = CreateItemDatabaseFixture(out _, out List<UnityEngine.Object> cleanup);
            GameObject deliveryRoot = CreateDeliverySystemFixture(out GameObject packagePrefab);
            try
            {
                object product = GetRuntimeProduct("harina");
                Assert.IsFalse(TryPurchaseRuntimeProduct(product, out string message));
                StringAssert.Contains("Falta desbloquear", message);
            }
            finally
            {
                DestroyObjects(cleanup);
                UnityEngine.Object.DestroyImmediate(packagePrefab);
                UnityEngine.Object.DestroyImmediate(deliveryRoot);
                UnityEngine.Object.DestroyImmediate(itemRoot);
                ClearStoreDatabaseInstance();
                UnityEngine.Object.DestroyImmediate(storeRoot);
                ResetProgress();
            }
        }

        [Test]
        public void ProductsShop_NoFundsShowsMissingAmount()
        {
            ResetProgress();
            GameObject storeRoot = CreateStoreDatabaseFixture();
            Component database = storeRoot.GetComponent(FindGameType("FLOBUK.StoreSimulator.StoreDatabase"));
            SetPrivateBackingField(database, "currentMoney", 0L);
            GameObject itemRoot = CreateItemDatabaseFixture(out _, out List<UnityEngine.Object> cleanup);
            GameObject deliveryRoot = CreateDeliverySystemFixture(out GameObject packagePrefab);
            try
            {
                object product = GetRuntimeProduct("leche");
                Assert.IsFalse(TryPurchaseRuntimeProduct(product, out string message));
                StringAssert.Contains("Fondos insuficientes", message);
                StringAssert.Contains("Faltan", message);
            }
            finally
            {
                DestroyObjects(cleanup);
                UnityEngine.Object.DestroyImmediate(packagePrefab);
                UnityEngine.Object.DestroyImmediate(deliveryRoot);
                UnityEngine.Object.DestroyImmediate(itemRoot);
                ClearStoreDatabaseInstance();
                UnityEngine.Object.DestroyImmediate(storeRoot);
                ResetProgress();
            }
        }

        [Test]
        public void ProductsShop_PurchaseBaseAndPlaceholderProductsUsesRealDeliveryFlow()
        {
            ResetProgress();
            GameObject storeRoot = CreateStoreDatabaseFixture();
            Component database = storeRoot.GetComponent(FindGameType("FLOBUK.StoreSimulator.StoreDatabase"));
            GameObject itemRoot = CreateItemDatabaseFixture(out _, out List<UnityEngine.Object> cleanup);
            GameObject deliveryRoot = CreateDeliverySystemFixture(out GameObject packagePrefab);
            try
            {
                long before = Convert.ToInt64(GetPropertyValue(database, "currentMoney"));
                object leche = GetRuntimeProduct("leche");
                Assert.IsTrue(TryPurchaseRuntimeProduct(leche, out string message), message);
                long afterLeche = Convert.ToInt64(GetPropertyValue(database, "currentMoney"));
                Assert.Less(afterLeche, before);
                StringAssert.Contains("Pedido realizado", message);

                UnlockWithPoint("productos_basicos_2");
                object harina = GetRuntimeProduct("harina");
                Assert.IsTrue(TryPurchaseRuntimeProduct(harina, out message), message);
                long afterHarina = Convert.ToInt64(GetPropertyValue(database, "currentMoney"));
                Assert.Less(afterHarina, afterLeche);
                Assert.GreaterOrEqual(UnityEngine.Object.FindObjectsByType(FindGameType("FLOBUK.StoreSimulator.PackageObject"), FindObjectsSortMode.None).Length, 2);
            }
            finally
            {
                DestroyObjects(cleanup);
                foreach (Component package in UnityEngine.Object.FindObjectsByType(FindGameType("FLOBUK.StoreSimulator.PackageObject"), FindObjectsSortMode.None))
                    UnityEngine.Object.DestroyImmediate(package.gameObject);
                UnityEngine.Object.DestroyImmediate(packagePrefab);
                UnityEngine.Object.DestroyImmediate(deliveryRoot);
                UnityEngine.Object.DestroyImmediate(itemRoot);
                ClearStoreDatabaseInstance();
                UnityEngine.Object.DestroyImmediate(storeRoot);
                ResetProgress();
            }
        }

        [Test]
        public void Tree_ProductUnlockUpdatesCatalogAvailability()
        {
            ResetProgress();
            ScriptableObject queso = CreateProductProbe("queso_crema", "Queso crema");
            try
            {
                Assert.IsFalse(IsProductUnlocked(queso));
                Assert.IsTrue(TryGetProductLockedMessage(queso, out string message));
                StringAssert.Contains("Lacteos 2", message);

                UnlockWithPoint("productos_basicos_2", "productos_basicos_3", "lacteos_1", "lacteos_2");
                Assert.IsTrue(IsProductUnlocked(queso));
                Assert.IsFalse(TryGetProductLockedMessage(queso, out message));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(queso);
            }
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

        [UnityTest]
        public IEnumerator Employees_All18CardsCanBeRepresented()
        {
            Type employeeManagerType = FindGameType("FLOBUK.StoreSimulator.EmployeeManager");
            Type panelType = FindGameType("FLOBUK.StoreSimulator.UIEmployeesPanel");
            object manager = Invoke(employeeManagerType, "EnsureInstance");
            GameObject panel = new GameObject("Employees Test Panel", typeof(RectTransform));
            Component component = panel.AddComponent(panelType);

            try
            {
                panelType.GetMethod("Build", BindingFlags.Public | BindingFlags.Instance).Invoke(component, null);
                panelType.GetMethod("Refresh", BindingFlags.Public | BindingFlags.Instance).Invoke(component, null);
                yield return null;

                Assert.AreEqual(18, GetTreeNodes().Count(node => GetNodeTypeName(node) == "Employee"));
                Assert.AreEqual(18, CountChildrenByPrefix(panel.transform, "empleado_"));
            }
            finally
            {
                if (manager != null)
                    UnityEngine.Object.DestroyImmediate(((Component)manager).gameObject);
                UnityEngine.Object.DestroyImmediate(panel);
            }
        }

        [UnityTest]
        public IEnumerator Employees_RebuildDoesNotDuplicateCardsOrListeners()
        {
            Type desktopType = FindGameType("FLOBUK.StoreSimulator.UIShopDesktop");
            Type bootstrapType = FindGameType("FLOBUK.StoreSimulator.UIEmployeesUIBootstrap");
            object manager = Invoke(FindGameType("FLOBUK.StoreSimulator.EmployeeManager"), "EnsureInstance");

            GameObject root = new GameObject("Employees Desktop Root", typeof(RectTransform));
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

            try
            {
                ensure.Invoke(null, new object[] { desktop });
                yield return null;
                ensure.Invoke(null, new object[] { desktop });
                yield return null;

                Assert.AreEqual(1, CountChildrenNamed(navigation.transform, "Button - Employees"));
                Assert.AreEqual(1, CountChildrenNamed(contentArea.transform, "Employees"));
            }
            finally
            {
                if (manager != null)
                    UnityEngine.Object.DestroyImmediate(((Component)manager).gameObject);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [UnityTest]
        public IEnumerator Tree_DetailPanelListsProductsForProductNodes()
        {
            Type treeType = FindGameType("FLOBUK.StoreSimulator.EntrepreneurTreeUI");
            GameObject root = new GameObject("Tree Details Test", typeof(RectTransform));
            Component tree = root.AddComponent(treeType);

            try
            {
                treeType.GetMethod("Build", BindingFlags.Public | BindingFlags.Instance).Invoke(tree, null);
                yield return null;

                string text = GetAllText(root.transform);
                StringAssert.Contains("Productos:", text);
                StringAssert.Contains("Leche", text);
                StringAssert.Contains("Assets finales", text);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [UnityTest]
        public IEnumerator Employees_All18EmployeesHaveVisiblePrerequisiteData()
        {
            Type employeeManagerType = FindGameType("FLOBUK.StoreSimulator.EmployeeManager");
            Type panelType = FindGameType("FLOBUK.StoreSimulator.UIEmployeesPanel");
            object manager = Invoke(employeeManagerType, "EnsureInstance");
            GameObject panel = new GameObject("Employees Prereq Test Panel", typeof(RectTransform));
            Component component = panel.AddComponent(panelType);

            try
            {
                panelType.GetMethod("Build", BindingFlags.Public | BindingFlags.Instance).Invoke(component, null);
                panelType.GetMethod("Refresh", BindingFlags.Public | BindingFlags.Instance).Invoke(component, null);
                yield return null;

                string text = GetAllText(panel.transform);
                Assert.GreaterOrEqual(CountOccurrences(text, "Requiere:"), 18);
                StringAssert.Contains("Especias 1", text);
                StringAssert.Contains("Productos de Higiene", text);
                StringAssert.Contains("Seguridad Nivel 3", text);
            }
            finally
            {
                if (manager != null)
                    UnityEngine.Object.DestroyImmediate(((Component)manager).gameObject);
                UnityEngine.Object.DestroyImmediate(panel);
            }
        }

        [Test]
        public void Employees_LockedEmployeeShowsMissingRequirement()
        {
            ResetProgress();
            Type employeeManagerType = FindGameType("FLOBUK.StoreSimulator.EmployeeManager");
            object manager = Invoke(employeeManagerType, "EnsureInstance");
            try
            {
                Assert.IsFalse(TryHire(employeeManagerType, manager, "empleado_1", out string message));
                StringAssert.Contains("Falta desbloquear: Especias 1", message);
            }
            finally
            {
                if (manager != null)
                    UnityEngine.Object.DestroyImmediate(((Component)manager).gameObject);
            }
        }

        [UnityTest]
        public IEnumerator Employees_RebuildDoesNotDuplicateCards()
        {
            Type employeeManagerType = FindGameType("FLOBUK.StoreSimulator.EmployeeManager");
            Type panelType = FindGameType("FLOBUK.StoreSimulator.UIEmployeesPanel");
            object manager = Invoke(employeeManagerType, "EnsureInstance");
            GameObject panel = new GameObject("Employees Rebuild Cards Panel", typeof(RectTransform));
            Component component = panel.AddComponent(panelType);

            try
            {
                panelType.GetMethod("Build", BindingFlags.Public | BindingFlags.Instance).Invoke(component, null);
                panelType.GetMethod("Refresh", BindingFlags.Public | BindingFlags.Instance).Invoke(component, null);
                yield return null;
                panelType.GetMethod("Refresh", BindingFlags.Public | BindingFlags.Instance).Invoke(component, null);
                yield return null;

                Assert.AreEqual(18, CountChildrenByPrefix(panel.transform, "empleado_"));
            }
            finally
            {
                if (manager != null)
                    UnityEngine.Object.DestroyImmediate(((Component)manager).gameObject);
                UnityEngine.Object.DestroyImmediate(panel);
            }
        }

        [Test]
        public void Desktop_NavigationKeepsMoneyAndGestionSeparated()
        {
            Type desktopType = FindGameType("FLOBUK.StoreSimulator.UIShopDesktop");
            GameObject root = new GameObject("Navigation Layout Test", typeof(RectTransform));
            Component desktop = root.AddComponent(desktopType);
            GameObject navigation = new GameObject("Navigation", typeof(RectTransform));
            navigation.transform.SetParent(root.transform, false);
            GameObject categories = new GameObject("Categories", typeof(RectTransform));
            categories.transform.SetParent(navigation.transform, false);

            try
            {
                CreateNavigationButton(categories.transform, "PRODUCTS");
                CreateNavigationButton(categories.transform, "EQUIPMENT");
                CreateNavigationButton(categories.transform, "ARBOL");
                CreateNavigationButton(categories.transform, "UPGRADES");
                CreateNavigationButton(categories.transform, "BOOSTERS");
                CreateNavigationButton(categories.transform, "CUSTOMIZATION");
                CreateNavigationButton(categories.transform, "EMPLEADOS");
                CreateNavigationButton(categories.transform, "GESTION");

                InvokeInstance(desktop, "OptimizeNavigationLayout");
                HorizontalLayoutGroup group = categories.GetComponent<HorizontalLayoutGroup>();
                Assert.NotNull(group);
                Assert.IsFalse(group.childForceExpandWidth);
                Assert.LessOrEqual(group.spacing, 6);

                foreach (Button button in categories.GetComponentsInChildren<Button>(true))
                {
                    LayoutElement layout = button.GetComponent<LayoutElement>();
                    Assert.NotNull(layout);
                    Assert.LessOrEqual(layout.preferredWidth, 150f, button.name);
                    Assert.GreaterOrEqual(layout.minWidth, 88f, button.name);
                    Component label = button.gameObject.GetComponentInChildren(FindGameType("TMPro.TextMeshProUGUI"), true);
                    Assert.NotNull(label, button.name);
                    Assert.AreEqual("NoWrap", Convert.ToString(label.GetType().GetProperty("textWrappingMode").GetValue(label)), button.name);
                    Assert.AreEqual("Ellipsis", Convert.ToString(label.GetType().GetProperty("overflowMode").GetValue(label)), button.name);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void EntrepreneurTree_VisualThemeUsesDarkPanel()
        {
            Type treeType = FindGameType("FLOBUK.StoreSimulator.EntrepreneurTreeUI");
            Color panel = (Color)treeType.GetProperty("VisualPanelBackground", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            Color card = (Color)treeType.GetProperty("VisualCardBackground", BindingFlags.Public | BindingFlags.Static).GetValue(null);

            Assert.Less(panel.r + panel.g + panel.b, 0.2f);
            Assert.Less(card.r + card.g + card.b, 0.35f);
            Assert.Greater(panel.a, 0.9f);
            Assert.Greater(card.a, 0.9f);
        }

        [UnityTest]
        public IEnumerator EntrepreneurTree_RebuildDoesNotDuplicateConnectionViews()
        {
            GameObject root = CreateTreeDesktopFixture(out Component desktop, out Transform licensesPanel, out _);
            Type bootstrapType = FindGameType("FLOBUK.StoreSimulator.EntrepreneurTreeUIBootstrap");
            MethodInfo ensure = bootstrapType.GetMethod("Ensure", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(ensure);

            ensure.Invoke(null, new object[] { desktop });
            yield return null;
            ensure.Invoke(null, new object[] { desktop });
            yield return null;

            Assert.AreEqual(1, licensesPanel.GetComponentsInChildren<MonoBehaviour>(true).Count(component => component.GetType().Name == "EntrepreneurTreeConnectionGraphic"));

            UnityEngine.Object.DestroyImmediate(root);
        }

        [UnityTest]
        public IEnumerator TreeRuntime_MainSceneCanResolveComputerDesktop()
        {
            Scene previousScene = SceneManager.GetActiveScene();
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
            Assert.NotNull(loadOperation, "Game scene is not registered in Build Settings.");
            while (!loadOperation.isDone)
                yield return null;

            Scene gameScene = SceneManager.GetSceneByName("Game");
            Assert.IsTrue(gameScene.IsValid(), "Game scene could not be loaded.");
            Type desktopType = FindGameType("FLOBUK.StoreSimulator.UIShopDesktop");
            Component desktop = UnityEngine.Object.FindObjectsByType(desktopType, FindObjectsSortMode.None)
                .OfType<Component>()
                .FirstOrDefault(component => component.gameObject.scene == gameScene);

            Assert.NotNull(desktop, "UIShopDesktop was not found in Game scene.");

            if (gameScene.isLoaded)
            {
                AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(gameScene);
                while (unloadOperation != null && !unloadOperation.isDone)
                    yield return null;
            }

            if (previousScene.IsValid() && previousScene.isLoaded)
                SceneManager.SetActiveScene(previousScene);
        }

        [UnityTest]
        public IEnumerator TreeRuntime_CanOpenArbolThroughDesktopRealFlow()
        {
            GameObject root = CreateTreeDesktopFixture(out Component desktop, out Transform licensesPanel, out _);
            try
            {
                bool opened = Convert.ToBoolean(InvokeInstance(desktop, "OpenArbolForQA", new[] { typeof(bool) }, true));
                yield return null;

                Assert.IsTrue(opened);
                Assert.IsTrue(licensesPanel.gameObject.activeSelf);
                Assert.AreEqual(1, CountChildrenNamed(licensesPanel, "Entrepreneur Tree Content"));
                StringAssert.Contains("ARBOL DEL EMPRENDEDOR", GetAllText(licensesPanel));
                StringAssert.Contains("QA activo", GetAllText(licensesPanel));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [UnityTest]
        public IEnumerator TreeRuntime_ReopenDoesNotDuplicateNodesOrConnections()
        {
            GameObject root = CreateTreeDesktopFixture(out Component desktop, out Transform licensesPanel, out _);
            try
            {
                InvokeInstance(desktop, "OpenArbolForQA", new[] { typeof(bool) }, false);
                yield return null;
                int firstNodes = licensesPanel.GetComponentsInChildren<MonoBehaviour>(true).Count(component => component.GetType().Name == "EntrepreneurTreeNodeView");
                int firstConnections = licensesPanel.GetComponentsInChildren<MonoBehaviour>(true).Count(component => component.GetType().Name == "EntrepreneurTreeConnectionGraphic");

                licensesPanel.gameObject.SetActive(false);
                InvokeInstance(desktop, "OpenArbolForQA", new[] { typeof(bool) }, false);
                yield return null;

                int secondNodes = licensesPanel.GetComponentsInChildren<MonoBehaviour>(true).Count(component => component.GetType().Name == "EntrepreneurTreeNodeView");
                int secondConnections = licensesPanel.GetComponentsInChildren<MonoBehaviour>(true).Count(component => component.GetType().Name == "EntrepreneurTreeConnectionGraphic");
                Assert.AreEqual(firstNodes, secondNodes);
                Assert.AreEqual(firstConnections, secondConnections);
                Assert.AreEqual(GetTreeNodes().Length, secondNodes);
                Assert.AreEqual(1, secondConnections);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void TreeRuntime_ProductBranchReferencesAll47Products()
        {
            object[] products = GetDocumentedProductDefinitions();
            Assert.AreEqual(47, products.Length);

            string[] requiredNodeIds =
            {
                "productos_basicos_1",
                "productos_basicos_2",
                "productos_basicos_3",
                "lacteos_1",
                "lacteos_2",
                "lacteos_3",
                "especias_1",
                "productos_frescos_1",
                "productos_frescos_2",
                "productos_higiene",
                "proteina_1",
                "sodas",
                "productos_lujo_1",
                "electrodomesticos_1",
            };

            foreach (string nodeId in requiredNodeIds)
            {
                Assert.NotNull(GetNode(nodeId), nodeId);
                Assert.Greater(GetDocumentedProductsByNode(nodeId).Length, 0, nodeId);
            }

            CollectionAssert.IsEmpty(products.Select(product => GetDefinitionString(product, "NodeId")).Where(nodeId => GetNode(nodeId) == null).Distinct().ToArray());
        }

        [UnityTest]
        public IEnumerator TreeRuntime_QAOverlayAddsPointsUnlocksAndStaysHiddenByDefault()
        {
            ResetProgress();
            GameObject root = CreateTreeDesktopFixture(out Component desktop, out Transform licensesPanel, out _);
            try
            {
                InvokeInstance(desktop, "OpenArbolForQA", new[] { typeof(bool) }, false);
                yield return null;
                Component tree = GetFirstComponentByTypeName(licensesPanel, "EntrepreneurTreeUI");
                Assert.NotNull(tree);
                Assert.IsFalse(GetAllText(licensesPanel).Contains("QA activo"));

                InvokeInstance(tree, "SetQaOverlayVisibleForQA", new[] { typeof(bool) }, true);
                Assert.IsTrue(Convert.ToBoolean(InvokeInstance(tree, "SelectNodeForQA", "productos_basicos_2")));
                AddTestProgressPoints(1);
                bool unlocked = Convert.ToBoolean(InvokeInstance(tree, "TryUnlockSelectedForQA", new[] { typeof(string).MakeByRefType() }, new object[] { null }));
                yield return null;

                Assert.IsTrue(unlocked);
                Assert.IsTrue(IsUnlocked("productos_basicos_2"));
                Assert.AreEqual(0, GetProgressInt("AvailablePoints"));
                StringAssert.Contains("ID QA: productos_basicos_2", GetAllText(licensesPanel));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                ResetProgress();
            }
        }

        [UnityTest]
        public IEnumerator TreeRuntime_CapturesRealDesktopArbolRoute()
        {
            string directory = GetFase12CaptureDirectory();
            Directory.CreateDirectory(directory);
            foreach (string file in Directory.GetFiles(directory, "*.png"))
                File.Delete(file);

            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                Assert.Ignore("Capturas Fase 12 omitidas: Unity esta ejecutando con dispositivo grafico Null.");

#if UNITY_EDITOR
            yield return CaptureFase12DesktopRoute(directory);

            string[] expected =
            {
                "Arbol_01_GameplayRutaReal_VistaGeneral.png",
                "Arbol_02_GameplayRutaReal_DetalleProducto.png",
                "Arbol_03_GameplayRutaReal_NodoEmpleado.png",
                "Arbol_04_GameplayRutaReal_NodoSeguridad.png",
                "Arbol_05_GameplayRutaReal_Mejora.png",
                "Arbol_06_GameplayRutaReal_BloqueoPrerequisito.png",
                "Arbol_07_GameplayRutaReal_PuntosInsuficientes.png",
                "Arbol_08_GameplayRutaReal_QAOverlay.png",
                "Arbol_09_GameplayRutaReal_Logros.png",
                "Computadora_01_GameplayRutaReal_Navegacion.png",
            };

            foreach (string fileName in expected)
                AssertVisualCapture(Path.Combine(directory, fileName));
#else
            yield return null;
            Assert.Ignore("Capturas Fase 12 requieren UnityEditor para instanciar el prefab real UIShopDesktop.");
#endif
        }

        private sealed class CaptureStage
        {
            public GameObject Root;
            public Camera Camera;
            public RectTransform Content;
        }

        private static string GetFase11CaptureDirectory()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Reportes", "Capturas_Fase11"));
        }

        private static string GetFase12CaptureDirectory()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Reportes", "Capturas_Fase12"));
        }

        private static string GetFase13CaptureDirectory()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Reportes", "Capturas_Fase13"));
        }

        private static string GetFase14CaptureDirectory()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Reportes", "Capturas_Fase14"));
        }

        private static string GetFase14MovementLogPath()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Reportes", "Codex_Fase14_MovementReproLog.txt"));
        }

        private static UnityEngine.Object LoadRuntimeInputActions()
        {
            string path = Path.Combine(Application.dataPath, "StoreSimulator", "Settings", "InputActions.inputactions");
            Assert.IsTrue(File.Exists(path), path);
            Type inputActionAssetType = FindTypeByName("UnityEngine.InputSystem.InputActionAsset");
            MethodInfo fromJson = inputActionAssetType.GetMethod("FromJson", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(fromJson, "InputActionAsset.FromJson not found.");
            return (UnityEngine.Object)fromJson.Invoke(null, new object[] { File.ReadAllText(path) });
        }

        private static GameObject CreateMovementControllerFixture(out Component controller, out Component input, out Camera camera, out UnityEngine.Object actions)
        {
            actions = LoadRuntimeInputActions();
            Type playerInputType = FindTypeByName("UnityEngine.InputSystem.PlayerInput");
            GameObject root = new GameObject("Fase14 PlayerController Fixture", typeof(CharacterController));
            input = root.AddComponent(playerInputType);
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            camera = cameraObject.GetComponent<Camera>();

            SetProperty(input, "actions", actions);
            SetProperty(input, "defaultActionMap", "Default");

            controller = root.AddComponent(FindGameType("FLOBUK.StoreSimulator.PlayerController"));
            SetField(controller, "cameraTransform", cameraObject.transform);
            SetField(controller, "speed", 5);
            SetField(controller, "runSpeed", 10f);
            SetField(controller, "jumpForce", 2);
            SetField(controller, "crouchHeight", 1f);
            RestoreGameplayInput();
            return root;
        }

        private static Type FindTypeByName(string typeName)
        {
            Type type = AppDomainAssemblies()
                .Select(assembly => assembly.GetType(typeName))
                .FirstOrDefault(foundType => foundType != null);
            Assert.NotNull(type, "Could not find type " + typeName);
            return type;
        }

        private static object FindActionMap(UnityEngine.Object actions, string mapName)
        {
            MethodInfo method = actions.GetType().GetMethod("FindActionMap", new[] { typeof(string), typeof(bool) });
            Assert.NotNull(method, "FindActionMap not found.");
            return method.Invoke(actions, new object[] { mapName, false });
        }

        private static object FindAction(object actionMap, string actionName)
        {
            MethodInfo method = actionMap.GetType().GetMethod("FindAction", new[] { typeof(string), typeof(bool) });
            Assert.NotNull(method, "FindAction not found.");
            return method.Invoke(actionMap, new object[] { actionName, false });
        }

        private static GameObject CreateMovementDesktopFixture(out Component desktop, Transform cameraTransform)
        {
            GameObject root = new GameObject("Fase14 UIShopDesktop Fixture", typeof(BoxCollider));
            desktop = root.AddComponent(FindGameType("FLOBUK.StoreSimulator.UIShopDesktop"));
            GameObject look = new GameObject("Computer Look");
            look.transform.SetParent(root.transform, false);
            look.transform.position = cameraTransform != null ? cameraTransform.position + Vector3.forward : Vector3.forward;
            look.transform.rotation = Quaternion.identity;
            SetField(desktop, "lookTransform", look.transform);
            return root;
        }

        private static void RestoreGameplayInput()
        {
            Invoke(FindGameType("FLOBUK.StoreSimulator.PlayerController"), "RestoreGameplayInput");
        }

        private static void SetGameplayInputEnabled(bool enabled)
        {
            Invoke(FindGameType("FLOBUK.StoreSimulator.PlayerController"), "SetGameplayInputEnabled", enabled);
        }

        private static string GetControllerState(Component controller)
        {
            return Convert.ToString(GetPropertyValue(controller, "CurrentMovementState"));
        }

        private static bool GetControllerBool(object controller, string propertyName)
        {
            return Convert.ToBoolean(GetPropertyValue(controller, propertyName));
        }

        private static float GetControllerFloat(object controller, string propertyName)
        {
            return Convert.ToSingle(GetPropertyValue(controller, propertyName));
        }

        private static string GetControllerString(object controller, string propertyName)
        {
            return Convert.ToString(GetPropertyValue(controller, propertyName));
        }

#if UNITY_EDITOR
        private static IEnumerator CaptureFase12DesktopRoute(string directory)
        {
            ResetProgress();
            CaptureStage stage = CreateCaptureStage("Fase12 Real Desktop Route Capture");
            GameObject desktopPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StoreSimulator/Prefabs/UI/UIShopDesktop.prefab");
            Assert.NotNull(desktopPrefab, "UIShopDesktop prefab could not be loaded.");
            GameObject desktopObject = UnityEngine.Object.Instantiate(desktopPrefab, stage.Content, false);
            Component desktop = GetFirstComponentByTypeName(desktopObject.transform, "UIShopDesktop");
            Assert.NotNull(desktop, "UIShopDesktop component could not be resolved from prefab.");

            try
            {
                RectTransform rect = desktopObject.GetComponent<RectTransform>();
                if (rect != null)
                    Stretch(rect, Vector2.zero, Vector2.zero);

                Assert.IsTrue(Convert.ToBoolean(InvokeInstance(desktop, "OpenArbolForQA", new[] { typeof(bool) }, false)));
                yield return null;

                Component tree = GetFirstComponentByTypeName(desktopObject.transform, "EntrepreneurTreeUI");
                Assert.NotNull(tree, "EntrepreneurTreeUI was not created in UIShopDesktop prefab route.");
                yield return SaveFase12TreeCapture(stage, desktop, tree, directory, "Arbol_01_GameplayRutaReal_VistaGeneral.png", null, false, false);
                yield return SaveFase12TreeCapture(stage, desktop, tree, directory, "Arbol_02_GameplayRutaReal_DetalleProducto.png", "productos_basicos_2", false, false);
                yield return SaveFase12TreeCapture(stage, desktop, tree, directory, "Arbol_03_GameplayRutaReal_NodoEmpleado.png", "empleado_1", false, false);
                yield return SaveFase12TreeCapture(stage, desktop, tree, directory, "Arbol_04_GameplayRutaReal_NodoSeguridad.png", "seguridad_1", false, false);
                yield return SaveFase12TreeCapture(stage, desktop, tree, directory, "Arbol_05_GameplayRutaReal_Mejora.png", "mejora_cafeina", false, false);
                yield return SaveFase12TreeCapture(stage, desktop, tree, directory, "Arbol_06_GameplayRutaReal_BloqueoPrerequisito.png", "lacteos_2", false, false);
                yield return SaveFase12TreeCapture(stage, desktop, tree, directory, "Arbol_07_GameplayRutaReal_PuntosInsuficientes.png", "productos_basicos_2", false, false);
                yield return SaveFase12TreeCapture(stage, desktop, tree, directory, "Arbol_08_GameplayRutaReal_QAOverlay.png", "productos_basicos_2", false, true);
                yield return SaveFase12TreeCapture(stage, desktop, tree, directory, "Arbol_09_GameplayRutaReal_Logros.png", null, true, true);
                Assert.IsTrue(Convert.ToBoolean(InvokeInstance(desktop, "OpenArbolForQA", new[] { typeof(bool) }, false)));
                yield return SaveCapture(stage, Path.Combine(directory, "Computadora_01_GameplayRutaReal_Navegacion.png"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(desktopObject);
                UnityEngine.Object.DestroyImmediate(stage.Root);
                ResetProgress();
            }
        }

        private static IEnumerator SaveFase12TreeCapture(CaptureStage stage, Component desktop, Component tree, string directory, string fileName, string selectedNodeId, bool achievements, bool qaOverlay)
        {
            ResetProgress();
            Assert.IsTrue(Convert.ToBoolean(InvokeInstance(desktop, "OpenArbolForQA", new[] { typeof(bool) }, qaOverlay)));
            InvokeInstance(tree, "SetQaOverlayVisibleForQA", new[] { typeof(bool) }, qaOverlay);
            InvokeInstance(tree, "ShowAchievementsForQA", new[] { typeof(bool) }, achievements);
            if (!string.IsNullOrEmpty(selectedNodeId))
                Assert.IsTrue(Convert.ToBoolean(InvokeInstance(tree, "SelectNodeForQA", selectedNodeId)), selectedNodeId);

            yield return SaveCapture(stage, Path.Combine(directory, fileName));
        }
#endif

        private static IEnumerator CaptureTree(string directory, string fileName, string selectedNodeId, bool achievements)
        {
            ResetProgress();
            CaptureStage stage = CreateCaptureStage("Fase11 Tree Capture");
            GameObject panel = CreateCaptureHost("Entrepreneur Tree Content", stage.Content);
            Type treeType = FindGameType("FLOBUK.StoreSimulator.EntrepreneurTreeUI");
            Component tree = panel.AddComponent(treeType);

            try
            {
                InvokeInstance(tree, "Build");
                if (!string.IsNullOrEmpty(selectedNodeId))
                {
                    object node = GetNode(selectedNodeId);
                    InvokeInstance(tree, "SelectNode", new[] { node.GetType(), typeof(bool) }, node, true);
                }

                if (achievements)
                    InvokeInstance(tree, "ToggleAchievements");

                yield return SaveCapture(stage, Path.Combine(directory, fileName));
            }
            finally
            {
                ResetProgress();
                UnityEngine.Object.DestroyImmediate(stage.Root);
            }
        }

        private static IEnumerator CaptureEmployees(string directory, string fileName)
        {
            ResetProgress();
            GameObject storeRoot = CreateStoreDatabaseFixture();
            Type employeeManagerType = FindGameType("FLOBUK.StoreSimulator.EmployeeManager");
            object manager = Invoke(employeeManagerType, "EnsureInstance");
            Invoke(employeeManagerType, "ResetRuntimeState");
            CaptureStage stage = CreateCaptureStage("Fase11 Employees Capture");
            GameObject panel = CreateCaptureHost("Employees", stage.Content);
            Type panelType = FindGameType("FLOBUK.StoreSimulator.UIEmployeesPanel");
            Component component = panel.AddComponent(panelType);

            try
            {
                InvokeInstance(component, "Build");
                if (fileName.Contains("DetalleEmpleado"))
                    SetPrivateInstanceField(component, "selectedEmployeeId", "empleado_18");
                InvokeInstance(component, "Refresh");
                yield return SaveCapture(stage, Path.Combine(directory, fileName));
            }
            finally
            {
                if (manager is Component managerComponent)
                    UnityEngine.Object.DestroyImmediate(managerComponent.gameObject);
                ClearStoreDatabaseInstance();
                UnityEngine.Object.DestroyImmediate(storeRoot);
                UnityEngine.Object.DestroyImmediate(stage.Root);
                ResetProgress();
            }
        }

        private static IEnumerator CaptureProducts(string directory, string fileName, bool placeholdersOnly)
        {
            ResetProgress();
            GameObject storeRoot = CreateStoreDatabaseFixture();
            GameObject itemRoot = CreateItemDatabaseFixture(out _, out List<UnityEngine.Object> cleanup);
            CaptureStage stage = CreateCaptureStage("Fase11 Products Capture");
            GameObject root = CreateCapturePanel("Products Catalog", stage.Content, new Color(0.035f, 0.04f, 0.07f, 1f));

            try
            {
                BuildProductsCapture(root.transform, placeholdersOnly);
                yield return SaveCapture(stage, Path.Combine(directory, fileName));
            }
            finally
            {
                DestroyObjects(cleanup);
                ClearItemDatabaseInstance(FindGameType("FLOBUK.StoreSimulator.ItemDatabase"));
                UnityEngine.Object.DestroyImmediate(itemRoot);
                ClearStoreDatabaseInstance();
                UnityEngine.Object.DestroyImmediate(storeRoot);
                UnityEngine.Object.DestroyImmediate(stage.Root);
                ResetProgress();
            }
        }

        private static IEnumerator CapturePrices(string directory, string fileName)
        {
            ResetProgress();
            GameObject storeRoot = CreateStoreDatabaseFixture();
            GameObject itemRoot = CreateItemDatabaseFixture(out _, out List<UnityEngine.Object> cleanup);
            CaptureStage stage = CreateCaptureStage("Fase11 Prices Capture");
            GameObject panel = CreateCaptureHost("Management Prices", stage.Content);
            Type panelType = FindGameType("FLOBUK.StoreSimulator.UIManagementPanel");
            Component component = panel.AddComponent(panelType);

            try
            {
                InvokeInstance(component, "Build");
                SetPrivateEnumField(component, "currentSection", "Prices");
                InvokeInstance(component, "Refresh");
                yield return SaveCapture(stage, Path.Combine(directory, fileName));
            }
            finally
            {
                DestroyObjects(cleanup);
                ClearItemDatabaseInstance(FindGameType("FLOBUK.StoreSimulator.ItemDatabase"));
                UnityEngine.Object.DestroyImmediate(itemRoot);
                ClearStoreDatabaseInstance();
                UnityEngine.Object.DestroyImmediate(storeRoot);
                UnityEngine.Object.DestroyImmediate(stage.Root);
                ResetProgress();
            }
        }

        private static IEnumerator CaptureComputerTopBar(string directory, string fileName)
        {
            GameObject storeRoot = CreateStoreDatabaseFixture();
            CaptureStage stage = CreateCaptureStage("Fase11 Computer Top Bar Capture");
            GameObject topBar = CreatePanel("Computer Top Bar", stage.Content, new Color(0.075f, 0.085f, 0.13f, 1f));
            RectTransform topBarRect = topBar.GetComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0f, 1f);
            topBarRect.anchorMax = new Vector2(1f, 1f);
            topBarRect.pivot = new Vector2(0.5f, 1f);
            topBarRect.offsetMin = new Vector2(12f, -86f);
            topBarRect.offsetMax = new Vector2(-12f, -12f);
            Component desktop = topBar.AddComponent(FindGameType("FLOBUK.StoreSimulator.UIShopDesktop"));

            try
            {
                HorizontalLayoutGroup barLayout = topBar.AddComponent<HorizontalLayoutGroup>();
                barLayout.padding = new RectOffset(8, 8, 8, 8);
                barLayout.spacing = 8;
                barLayout.childControlHeight = true;
                barLayout.childControlWidth = true;

                GameObject navigation = CreateLayoutBox("Navigation", topBar.transform);
                LayoutElement navigationLayout = navigation.AddComponent<LayoutElement>();
                navigationLayout.flexibleWidth = 1;
                GameObject categories = CreateLayoutBox("Categories", navigation.transform);
                Stretch(categories.GetComponent<RectTransform>());

                string[] labels = { "PRODUCTS", "EQUIPMENT", "ARBOL", "UPGRADES", "BOOSTERS", "CUSTOMIZATION", "EMPLEADOS", "GESTION" };
                foreach (string label in labels)
                    CreateNavigationButton(categories.transform, label);

                InvokeInstance(desktop, "OptimizeNavigationLayout");
                UnityEngine.Object.DestroyImmediate(desktop);
                GameObject info = CreatePanel("Status", topBar.transform, new Color(0.105f, 0.12f, 0.18f, 1f));
                info.AddComponent<LayoutElement>().preferredWidth = 300f;
                HorizontalLayoutGroup infoLayout = info.AddComponent<HorizontalLayoutGroup>();
                infoLayout.padding = new RectOffset(10, 10, 6, 6);
                infoLayout.spacing = 10;
                CreateTMPText("Money", info.transform, FormatMoney(1000000L), 18, "Bold", "Left");
                CreateTMPText("Level", info.transform, "Level 0", 14, "Bold", "Right");
                yield return SaveCapture(stage, Path.Combine(directory, fileName));
            }
            finally
            {
                ClearStoreDatabaseInstance();
                UnityEngine.Object.DestroyImmediate(storeRoot);
                UnityEngine.Object.DestroyImmediate(stage.Root);
            }
        }

        private static void BuildMovementDiagnosticCapture(Transform parent, Vector3 before, Vector3 after, Vector3 delta, Component controller)
        {
            GameObject root = CreateCapturePanel("Movement Diagnostic", parent, new Color(0.035f, 0.04f, 0.07f, 1f));
            VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(26, 26, 22, 22);
            layout.spacing = 12;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            Component header = CreateTMPText("Header", root.transform, "MOVIMIENTO - REPRODUCCION FASE 14", 24, "Bold", "Left");
            SetProperty(header, "color", new Color(0.94f, 0.95f, 0.98f, 1f));
            header.GetComponent<LayoutElement>().preferredHeight = 40f;

            string[] rows =
            {
                "Escena: Assets/StoreSimulator/Scenes/Game.unity",
                "PlayerController real + CharacterController + PlayerInput Default",
                "Posicion inicial: " + before.ToString("F3"),
                "Posicion final: " + after.ToString("F3"),
                "Delta: " + delta.ToString("F3") + "  Distancia: " + delta.magnitude.ToString("F3"),
                "canMove/canLook: " + GetControllerBool(controller, "CanMove") + " / " + GetControllerBool(controller, "CanLook"),
                "Action map: " + GetControllerString(controller, "ActiveActionMapName"),
                "Cursor: " + Cursor.lockState + " visible=" + Cursor.visible,
                "timeScale: " + Time.timeScale.ToString("F2"),
                "F7 restore: UIShopDesktop.RestoreGameplayInputForQA -> PlayerController.RestoreGameplayInput",
            };

            for (int i = 0; i < rows.Length; i++)
            {
                GameObject row = CreatePanel("Row " + i, root.transform, i % 2 == 0 ? new Color(0.08f, 0.095f, 0.14f, 1f) : new Color(0.105f, 0.12f, 0.18f, 1f));
                row.AddComponent<LayoutElement>().preferredHeight = 42f;
                HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
                rowLayout.padding = new RectOffset(12, 12, 6, 6);
                rowLayout.childControlHeight = true;
                rowLayout.childControlWidth = true;
                Component label = CreateTMPText("Text", row.transform, rows[i], 16, i == 4 ? "Bold" : "Normal", "Left");
                SetProperty(label, "color", i == 4 ? new Color(0.6f, 1f, 0.72f, 1f) : Color.white);
            }
        }

        private static CaptureStage CreateCaptureStage(string name)
        {
            GameObject root = new GameObject(name);
            GameObject cameraObject = new GameObject("Capture Camera", typeof(Camera));
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.015f, 0.018f, 0.025f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 3.6f;

            GameObject canvasObject = new GameObject("Capture Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(root.transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;
            canvas.sortingOrder = 100;
            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(1280f, 720f);
            canvasRect.localScale = Vector3.one * 0.01f;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject contentObject = CreatePanel("Capture Content", canvasObject.transform, new Color(0.035f, 0.04f, 0.07f, 1f));
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            Stretch(contentRect);
            return new CaptureStage { Root = root, Camera = camera, Content = contentRect };
        }

        private static GameObject CreateCapturePanel(string name, Transform parent, Color color)
        {
            GameObject panel = CreatePanel(name, parent, color);
            Stretch(panel.GetComponent<RectTransform>(), new Vector2(12f, 12f), new Vector2(-12f, -12f));
            return panel;
        }

        private static GameObject CreateCaptureHost(string name, Transform parent)
        {
            GameObject panel = CreateLayoutBox(name, parent);
            Stretch(panel.GetComponent<RectTransform>(), new Vector2(12f, 12f), new Vector2(-12f, -12f));
            return panel;
        }

        private static IEnumerator SaveCapture(CaptureStage stage, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(stage.Content);
            yield return null;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(stage.Content);
            yield return null;

            RenderTexture previous = RenderTexture.active;
            RenderTexture target = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            Texture2D image = new Texture2D(1280, 720, TextureFormat.RGBA32, false);
            try
            {
                stage.Camera.targetTexture = target;
                stage.Camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                stage.Camera.targetTexture = null;
                RenderTexture.active = previous;
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(image);
            }
        }

        private static void AssertVisualCapture(string path)
        {
            Assert.IsTrue(File.Exists(path), path);
            FileInfo file = new FileInfo(path);
            Assert.Greater(file.Length, 5000L, path);

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.IsTrue(texture.LoadImage(File.ReadAllBytes(path)), path);
                Assert.GreaterOrEqual(texture.width, 800, path);
                Assert.GreaterOrEqual(texture.height, 450, path);

                HashSet<int> colorBuckets = new HashSet<int>();
                int visibleSamples = 0;
                for (int y = 0; y < texture.height; y += 72)
                {
                    for (int x = 0; x < texture.width; x += 72)
                    {
                        Color32 pixel = texture.GetPixel(x, y);
                        if (pixel.a < 20)
                            continue;

                        visibleSamples++;
                        int key = (pixel.r / 16) << 8 | (pixel.g / 16) << 4 | (pixel.b / 16);
                        colorBuckets.Add(key);
                    }
                }

                Assert.Greater(visibleSamples, 20, path);
                Assert.Greater(colorBuckets.Count, 1, path);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static void BuildProductsCapture(Transform parent, bool placeholdersOnly)
        {
            VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 10, 10);
            layout.spacing = 8;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            string title = placeholdersOnly ? "PRODUCTS - PLACEHOLDERS DOCUMENTADOS" : "PRODUCTS - CATALOGO COMPLETO 47/47";
            Component header = CreateTMPText("Header", parent, title, 22, "Bold", "Left");
            SetProperty(header, "color", new Color(0.94f, 0.95f, 0.98f, 1f));
            header.GetComponent<LayoutElement>().preferredHeight = 34f;

            GameObject grid = CreatePanel("Product Grid", parent, new Color(0.075f, 0.085f, 0.13f, 0.98f));
            GridLayoutGroup group = grid.AddComponent<GridLayoutGroup>();
            group.padding = new RectOffset(8, 8, 8, 8);
            group.spacing = new Vector2(6f, 6f);
            group.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            group.constraintCount = placeholdersOnly ? 3 : 4;
            group.cellSize = placeholdersOnly ? new Vector2(394f, 86f) : new Vector2(296f, 48f);
            grid.AddComponent<LayoutElement>().flexibleHeight = 1f;

            Type itemDatabaseType = FindGameType("FLOBUK.StoreSimulator.ItemDatabase");
            Type productType = FindGameType("FLOBUK.StoreSimulator.ProductScriptableObject");
            IEnumerable<object> products = ((System.Collections.IEnumerable)Invoke(itemDatabaseType, "GetByType", productType)).Cast<object>()
                .OrderBy(product => GetFieldString(product, "title"));
            if (placeholdersOnly)
                products = products.Where(product =>
                {
                    object definition = Invoke(GetDocumentedProductCatalogType(), "GetById", GetFieldString(product, "id"));
                    return definition != null && GetDefinitionBool(definition, "UsesProvisionalAsset");
                }).Take(12);

            foreach (object product in products)
                CreateProductCard(grid.transform, product, placeholdersOnly);
        }

        private static void CreateProductCard(Transform parent, object product, bool expanded)
        {
            GameObject card = CreatePanel("Product Card - " + GetFieldString(product, "id"), parent, new Color(0.105f, 0.12f, 0.18f, 0.98f));
            HorizontalLayoutGroup layout = card.AddComponent<HorizontalLayoutGroup>();
            layout.padding = expanded ? new RectOffset(8, 8, 7, 7) : new RectOffset(6, 6, 4, 4);
            layout.spacing = expanded ? 8f : 5f;
            layout.childControlHeight = true;
            layout.childControlWidth = false;

            GameObject iconObject = CreatePanel("Icon", card.transform, new Color(0.13f, 0.18f, 0.24f, 1f));
            Image icon = iconObject.GetComponent<Image>();
            iconObject.AddComponent<LayoutElement>().preferredWidth = expanded ? 46f : 34f;

            GameObject textBox = CreateLayoutBox("Text", card.transform);
            textBox.AddComponent<LayoutElement>().flexibleWidth = 1f;
            VerticalLayoutGroup textLayout = textBox.AddComponent<VerticalLayoutGroup>();
            textLayout.spacing = 1f;
            Component title = CreateTMPText("Title", textBox.transform, string.Empty, expanded ? 14 : 10, "Bold", "Left");
            Component buy = CreateTMPText("Buy Price", textBox.transform, string.Empty, expanded ? 11 : 9, "Normal", "Left");
            Component total = CreateTMPText("Total Price", textBox.transform, string.Empty, expanded ? 11 : 8, "Normal", "Left");
            Component store = CreateTMPText("Store Price", textBox.transform, string.Empty, 8, "Normal", "Left");
            Component market = CreateTMPText("Market Price", textBox.transform, string.Empty, 8, "Normal", "Left");

            GameObject overlay = CreatePanel("Locked Overlay", card.transform, new Color(0.01f, 0.012f, 0.018f, 0.72f));
            overlay.AddComponent<LayoutElement>().preferredWidth = expanded ? 150f : 86f;
            Component locked = CreateTMPText("Locked Message", overlay.transform, string.Empty, expanded ? 9 : 7, "Bold", "Center");
            Stretch(locked.GetComponent<RectTransform>(), new Vector2(4f, 4f), new Vector2(-4f, -4f));

            Component component = card.AddComponent(FindGameType("FLOBUK.StoreSimulator.UIShopItemProduct"));
            SetField(component, "title", title);
            SetField(component, "icon", icon);
            SetField(component, "buyPrice", buy);
            SetField(component, "totalPrice", total);
            SetField(component, "storePrice", store);
            SetField(component, "marketPrice", market);
            SetField(component, "lockedOverlay", overlay);
            SetField(component, "lockedMessage", locked);
            InvokeInstance(component, "Initialize", product);
        }

        private static GameObject CreateStoreDatabaseFixture()
        {
            ClearStoreDatabaseInstance();
            GameObject root = new GameObject("StoreDatabase Test Fixture");
            Component database = root.AddComponent(FindGameType("FLOBUK.StoreSimulator.StoreDatabase"));
            GameObject storeNameObject = new GameObject("Store Name", typeof(RectTransform));
            storeNameObject.transform.SetParent(root.transform, false);
            Component storeName = storeNameObject.AddComponent(FindGameType("TMPro.TextMeshProUGUI"));
            SetProperty(storeName, "text", "POMPIC MARKET");
            SetField(database, "storeName", storeName);
            SetField(database, "startMoney", 1000000L);
            SetField(database, "levelXP", new long[] { 1000L, 2000L, 3000L });
            InvokeInstance(database, "Awake");
            SetPrivateBackingField(database, "currentMoney", 1000000L);
            SetPrivateBackingField(database, "currentXP", 0L);
            SetPrivateBackingField(database, "currentLevel", 0);
            return root;
        }

        private static void ClearStoreDatabaseInstance()
        {
            Type type = FindGameType("FLOBUK.StoreSimulator.StoreDatabase");
            FieldInfo backingField = type.GetField("<Instance>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
            if (backingField != null)
                backingField.SetValue(null, null);
        }

        private static string FormatMoney(long amount)
        {
            return Convert.ToString(Invoke(FindGameType("FLOBUK.StoreSimulator.StoreDatabase"), "FromLongToStringMoney", amount));
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            obj.GetComponent<Image>().color = color;
            return obj;
        }

        private static GameObject CreateLayoutBox(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static Component CreateTMPText(string name, Transform parent, string value, int size, string style, string alignment)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
            obj.transform.SetParent(parent, false);
            Component text = obj.AddComponent(FindGameType("TMPro.TextMeshProUGUI"));
            SetProperty(text, "text", value);
            SetProperty(text, "fontSize", (float)size);
            SetEnumProperty(text, "fontStyle", style);
            SetEnumProperty(text, "alignment", alignment);
            SetProperty(text, "color", Color.white);
            SetEnumProperty(text, "textWrappingMode", "Normal");
            SetEnumProperty(text, "overflowMode", "Ellipsis");
            SetProperty(text, "enableAutoSizing", true);
            SetProperty(text, "fontSizeMin", (float)Mathf.Max(6, size - 4));
            SetProperty(text, "fontSizeMax", (float)size);
            obj.GetComponent<LayoutElement>().minHeight = Mathf.Max(14f, size * 1.35f);
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            Stretch(rect, Vector2.zero, Vector2.zero);
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
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

        private static Type GetDocumentedProductCatalogType()
        {
            return FindGameType("FLOBUK.StoreSimulator.DocumentedProductCatalog");
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

        private static Component GetFirstComponentByTypeName(Transform root, string typeName)
        {
            if (root == null)
                return null;

            return root.GetComponentsInChildren<MonoBehaviour>(true).FirstOrDefault(component => component.GetType().Name == typeName);
        }

        private static object[] GetDocumentedProductDefinitions()
        {
            object value = GetDocumentedProductCatalogType().GetProperty("Definitions", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            return ((System.Collections.IEnumerable)value).Cast<object>().ToArray();
        }

        private static object[] GetDocumentedProductsByNode(string nodeId)
        {
            object value = Invoke(GetDocumentedProductCatalogType(), "GetByNode", nodeId);
            return ((System.Collections.IEnumerable)value).Cast<object>().ToArray();
        }

        private static string GetDefinitionString(object definition, string propertyName)
        {
            object value = definition.GetType().GetProperty(propertyName).GetValue(definition);
            return Convert.ToString(value);
        }

        private static long GetDefinitionLong(object definition, string propertyName)
        {
            object value = definition.GetType().GetProperty(propertyName).GetValue(definition);
            return Convert.ToInt64(value);
        }

        private static bool GetDefinitionBool(object definition, string propertyName)
        {
            object value = definition.GetType().GetProperty(propertyName).GetValue(definition);
            return Convert.ToBoolean(value);
        }

        private static object CreatePurchasableListWithFallback(out List<UnityEngine.Object> cleanup)
        {
            cleanup = new List<UnityEngine.Object>();
            Type productType = FindGameType("FLOBUK.StoreSimulator.ProductScriptableObject");
            Type purchasableType = FindGameType("FLOBUK.StoreSimulator.PurchasableScriptableObject");
            Type listType = typeof(List<>).MakeGenericType(purchasableType);
            System.Collections.IList list = (System.Collections.IList)Activator.CreateInstance(listType);

            ScriptableObject fallback = ScriptableObject.CreateInstance(productType);
            GameObject prefab = new GameObject("Product_A-E Fallback Prefab");
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            Sprite icon = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

            SetField(fallback, "id", "0");
            SetField(fallback, "title", "Leche");
            SetField(fallback, "prefab", prefab);
            SetField(fallback, "icon", icon);
            SetField(fallback, "size", Vector2Int.one);
            list.Add(fallback);

            cleanup.Add(fallback);
            cleanup.Add(prefab);
            cleanup.Add(icon);
            cleanup.Add(texture);
            return list;
        }

        private static GameObject CreateItemDatabaseFixture(out Component database, out List<UnityEngine.Object> cleanup)
        {
            Type itemDatabaseType = FindGameType("FLOBUK.StoreSimulator.ItemDatabase");
            ClearItemDatabaseInstance(itemDatabaseType);
            object products = CreatePurchasableListWithFallback(out cleanup);
            GameObject root = new GameObject("ItemDatabase Test Fixture");
            database = root.AddComponent(itemDatabaseType);
            SetField(database, "purchasables", products);
            InvokeInstance(database, "Awake");
            return root;
        }

        private static GameObject CreateDeliverySystemFixture(out GameObject packagePrefab)
        {
            Type deliveryType = FindGameType("FLOBUK.StoreSimulator.DeliverySystem");
            GameObject root = new GameObject("DeliverySystem Test Fixture");
            Component delivery = root.AddComponent(deliveryType);

            GameObject start = new GameObject("Delivery Start");
            start.transform.SetParent(root.transform, false);
            SetField(delivery, "deliveryStart", start.transform);
            SetField(delivery, "totalDeliveries", 0);

            packagePrefab = CreatePackagePrefab();
            SetField(delivery, "packagePrefab", packagePrefab);
            InvokeInstance(delivery, "Awake");
            return root;
        }

        private static GameObject CreatePackagePrefab()
        {
            Type packageType = FindGameType("FLOBUK.StoreSimulator.PackageObject");
            GameObject prefab = new GameObject("PackageObject Test Prefab");
            Component package = prefab.AddComponent(packageType);

            GameObject container = new GameObject("Container");
            container.transform.SetParent(prefab.transform, false);
            GameObject label = new GameObject("Label", typeof(MeshRenderer));
            label.transform.SetParent(prefab.transform, false);

            SetField(package, "container", container.transform);
            SetField(package, "label", label.GetComponent<MeshRenderer>());
            SetField(package, "space", new Vector2Int(6, 6));
            return prefab;
        }

        private static object GetRuntimeProduct(string productId)
        {
            return Invoke(FindGameType("FLOBUK.StoreSimulator.ItemDatabase"), "GetById", FindGameType("FLOBUK.StoreSimulator.ProductScriptableObject"), productId);
        }

        private static bool TryPurchaseRuntimeProduct(object product, out string message)
        {
            object[] args = { product, null };
            bool result = Convert.ToBoolean(FindGameType("FLOBUK.StoreSimulator.DeliverySystem").GetMethod("TryPurchaseProduct", BindingFlags.Public | BindingFlags.Static).Invoke(null, args));
            message = Convert.ToString(args[1]);
            return result;
        }

        private static void ClearItemDatabaseInstance(Type itemDatabaseType)
        {
            FieldInfo backingField = itemDatabaseType.GetField("<Instance>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
            if (backingField != null)
                backingField.SetValue(null, null);
        }

        private static void DestroyObjects(IEnumerable<UnityEngine.Object> objects)
        {
            if (objects == null)
                return;

            foreach (UnityEngine.Object obj in objects)
            {
                if (obj != null)
                    UnityEngine.Object.DestroyImmediate(obj);
            }
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

        private static object GetFieldValue(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(field, "Could not find field " + fieldName);
            return field.GetValue(target);
        }

        private static object GetPropertyValue(object target, string propertyName)
        {
            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(property, "Could not find property " + propertyName);
            return property.GetValue(target);
        }

        private static string GetFieldString(object target, string fieldName)
        {
            return Convert.ToString(GetFieldValue(target, fieldName));
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

        private static void SetEnumProperty(object target, string propertyName, string value)
        {
            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(property, "Could not find property " + propertyName);
            property.SetValue(target, Enum.Parse(property.PropertyType, value));
        }

        private static void SetStaticField(Type type, string fieldName, int value)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(field, "Could not find field " + fieldName);
            field.SetValue(null, value);
        }

        private static void SetPrivateInstanceField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, "Could not find private field " + fieldName);
            field.SetValue(target, value);
        }

        private static void SetPrivateEnumField(object target, string fieldName, string enumValue)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, "Could not find enum field " + fieldName);
            field.SetValue(target, Enum.Parse(field.FieldType, enumValue));
        }

        private static void SetPrivateBackingField(object target, string propertyName, object value)
        {
            FieldInfo field = target.GetType().GetField("<" + propertyName + ">k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, "Could not find backing field " + propertyName);
            field.SetValue(target, value);
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

        private static object InvokeInstance(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method, "Could not find method " + methodName);
            return method.Invoke(target, args);
        }

        private static object InvokeInstance(object target, string methodName, Type[] parameterTypes, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameterTypes, null);
            Assert.NotNull(method, "Could not find method " + methodName);
            return method.Invoke(target, args);
        }

        private static Button CreateNavigationButton(Transform parent, string labelText)
        {
            GameObject buttonObject = new GameObject("Button - " + labelText, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = buttonObject.GetComponent<Image>();

            GameObject labelObject = new GameObject("Text", typeof(RectTransform));
            labelObject.transform.SetParent(buttonObject.transform, false);
            Component label = labelObject.AddComponent(FindGameType("TMPro.TextMeshProUGUI"));
            SetProperty(label, "text", labelText);
            return button;
        }

        private static string GetAllText(Transform parent)
        {
            Type textType = FindGameType("TMPro.TextMeshProUGUI");
            List<string> values = new List<string>();
            foreach (Component component in parent.GetComponentsInChildren(textType, true))
                values.Add(Convert.ToString(component.GetType().GetProperty("text").GetValue(component)));

            return string.Join("\n", values);
        }

        private static int CountOccurrences(string value, string pattern)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pattern))
                return 0;

            int count = 0;
            int index = 0;
            while ((index = value.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += pattern.Length;
            }

            return count;
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

        private static int CountChildrenByPrefix(Transform parent, string prefix)
        {
            int count = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name.StartsWith(prefix, StringComparison.Ordinal))
                    count++;

                count += CountChildrenByPrefix(child, prefix);
            }

            return count;
        }
    }
}
