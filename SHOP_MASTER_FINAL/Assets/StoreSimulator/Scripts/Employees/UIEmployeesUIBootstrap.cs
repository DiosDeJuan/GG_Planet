//Adaptado por POMPIC 20100333
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    public static class UIEmployeesUIBootstrap
    {
        public static void Ensure(UIShopDesktop desktop)
        {
            if (desktop == null)
                return;

            EmployeeManager.EnsureInstance();
            Transform categories = FindRecursive(desktop.transform, "Categories");
            Transform navigation = FindRecursive(desktop.transform, "Navigation");
            if (categories == null || navigation == null || FindRecursive(categories, "Employees") != null)
                return;

            GameObject panel = new GameObject("Employees", typeof(RectTransform));
            panel.transform.SetParent(categories, false);
            panel.SetActive(false);
            UIEmployeesPanel employeesPanel = panel.AddComponent<UIEmployeesPanel>();
            employeesPanel.Build();

            Button templateButton = navigation.GetComponentInChildren<Button>(true);
            GameObject buttonObject;
            Button button;
            if (templateButton != null)
            {
                buttonObject = Object.Instantiate(templateButton.gameObject, navigation, false);
                buttonObject.name = "Button - Employees";
                button = buttonObject.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
            }
            else
            {
                buttonObject = new GameObject("Button - Employees", typeof(RectTransform), typeof(Image), typeof(Button));
                buttonObject.transform.SetParent(navigation, false);
                buttonObject.GetComponent<RectTransform>().sizeDelta = new Vector2(210, 40);
                button = buttonObject.GetComponent<Button>();
            }

            TMP_Text text = buttonObject.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = "Empleados";
                text.textWrappingMode = TextWrappingModes.NoWrap;
            }

            UIShopCategoryHelper helper = categories.GetComponent<UIShopCategoryHelper>();
            button.onClick.AddListener(() =>
            {
                if (helper != null)
                    helper.Show(panel);
                else
                    panel.SetActive(true);
            });
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
    }
}
