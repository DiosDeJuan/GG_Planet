//Adaptado por POMPIC 20100333
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    public static class UIEmployeesUIBootstrap
    {
        private const string ContentAreaName = "ContentArea";
        private const string GeneratedEmployeesButtonName = "Button - Employees";
        private const string GeneratedEmployeesPanelName = "Employees";
        private const string NavigationName = "Navigation";
        private const string NavigationTabsName = "Categories";

        public static void Ensure(UIShopDesktop desktop)
        {
            if (desktop == null)
                return;

            EmployeeManager.EnsureInstance();
            Transform contentArea = FindRecursive(desktop.transform, ContentAreaName);
            Transform navigation = FindRecursive(desktop.transform, NavigationName);
            if (contentArea == null || navigation == null)
                return;

            Transform navigationTabs = FindDirectChild(navigation, NavigationTabsName) ?? navigation;
            RemoveGeneratedObject(navigation, GeneratedEmployeesButtonName);

            GameObject employeesPanel = EnsureEmployeesPanel(contentArea);
            Button employeesButton = CreateEmployeesButton(navigationTabs);
            UIShopCategoryHelper helper = contentArea.GetComponent<UIShopCategoryHelper>();
            employeesButton.onClick.AddListener(() =>
            {
                if (helper != null)
                    helper.Show(employeesPanel);
                else
                    employeesPanel.SetActive(true);
            });
        }

        private static GameObject EnsureEmployeesPanel(Transform contentArea)
        {
            Transform existing = FindDirectChild(contentArea, GeneratedEmployeesPanelName);
            if (existing != null)
            {
                existing.gameObject.SetActive(false);
                return existing.gameObject;
            }

            GameObject panel = new GameObject(GeneratedEmployeesPanelName, typeof(RectTransform));
            panel.transform.SetParent(contentArea, false);
            Stretch(panel.GetComponent<RectTransform>());
            panel.SetActive(false);

            UIEmployeesPanel employeesPanel = panel.AddComponent<UIEmployeesPanel>();
            employeesPanel.Build();
            return panel;
        }

        private static Button CreateEmployeesButton(Transform navigationTabs)
        {
            Button templateButton = navigationTabs.GetComponentInChildren<Button>(true);
            GameObject buttonObject;
            Button button;
            if (templateButton != null)
            {
                buttonObject = Object.Instantiate(templateButton.gameObject, navigationTabs, false);
                buttonObject.name = GeneratedEmployeesButtonName;
                button = buttonObject.GetComponent<Button>();
            }
            else
            {
                buttonObject = new GameObject(GeneratedEmployeesButtonName, typeof(RectTransform), typeof(Image), typeof(Button));
                buttonObject.transform.SetParent(navigationTabs, false);
                buttonObject.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 40);
                button = buttonObject.GetComponent<Button>();
            }

            ConfigureButtonText(buttonObject, "EMPLEADOS");
            return button;
        }

        private static void ConfigureButtonText(GameObject buttonObject, string label)
        {
            TMP_Text text = buttonObject.GetComponentInChildren<TMP_Text>(true);
            if (text == null)
                return;

            text.text = label;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.enableAutoSizing = true;
            text.fontSizeMin = 12;
            text.fontSizeMax = 24;
            text.overflowMode = TextOverflowModes.Overflow;
            text.alignment = TextAlignmentOptions.Center;
        }

        private static Transform FindRecursive(Transform parent, string name)
        {
            if (parent == null)
                return null;

            if (parent.name == name)
                return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform result = FindRecursive(parent.GetChild(i), name);
                if (result != null)
                    return result;
            }

            return null;
        }

        private static Transform FindDirectChild(Transform parent, string name)
        {
            if (parent == null)
                return null;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == name)
                    return child;
            }

            return null;
        }

        private static void RemoveGeneratedObject(Transform parent, string objectName)
        {
            if (parent == null)
                return;

            Transform generated = FindRecursive(parent, objectName);
            if (generated == null)
                return;

            generated.gameObject.SetActive(false);
            Object.Destroy(generated.gameObject);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}
