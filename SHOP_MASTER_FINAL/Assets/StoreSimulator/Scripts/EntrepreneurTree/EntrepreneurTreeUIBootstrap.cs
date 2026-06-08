//Adaptado por POMPIC 20100333
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    public static class EntrepreneurTreeUIBootstrap
    {
        public static void Ensure(UIShopDesktop desktop)
        {
            if (desktop == null)
                return;

            Transform categories = FindRecursive(desktop.transform, "Categories");
            Transform navigation = FindRecursive(desktop.transform, "Navigation");
            if (categories == null || navigation == null)
                return;

            Transform existingPanel = FindRecursive(categories, "Entrepreneur Tree");
            GameObject treePanel;
            if (existingPanel != null)
            {
                treePanel = existingPanel.gameObject;
            }
            else
            {
                treePanel = new GameObject("Entrepreneur Tree", typeof(RectTransform));
                treePanel.transform.SetParent(categories, false);
                treePanel.SetActive(false);
                EntrepreneurTreeUI treeUI = treePanel.AddComponent<EntrepreneurTreeUI>();
                treeUI.Build();
            }

            Button button = FindTreeButton(navigation);
            if (button == null)
                button = CreateTreeButton(navigation);

            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = "Árbol";
                text.textWrappingMode = TextWrappingModes.NoWrap;
            }

            button.onClick.RemoveAllListeners();
            UIShopCategoryHelper helper = categories.GetComponent<UIShopCategoryHelper>();
            button.onClick.AddListener(() =>
            {
                if (helper != null)
                    helper.Show(treePanel);
                else
                    treePanel.SetActive(true);
            });
        }

        private static Button FindTreeButton(Transform navigation)
        {
            if (navigation == null)
                return null;

            for (int i = 0; i < navigation.childCount; i++)
            {
                Transform child = navigation.GetChild(i);
                if (!child.name.Contains("Entrepreneur Tree"))
                    continue;

                return child.GetComponent<Button>();
            }

            return null;
        }

        private static Button CreateTreeButton(Transform navigation)
        {
            Button templateButton = navigation.GetComponentInChildren<Button>(true);
            GameObject buttonObject;
            Button button;

            if (templateButton != null)
            {
                buttonObject = Object.Instantiate(templateButton.gameObject, navigation, false);
                buttonObject.name = "Button - Entrepreneur Tree";
                button = buttonObject.GetComponent<Button>();
            }
            else
            {
                buttonObject = new GameObject("Button - Entrepreneur Tree", typeof(RectTransform), typeof(Image), typeof(Button));
                buttonObject.transform.SetParent(navigation, false);
                RectTransform rect = buttonObject.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(210, 40);
                button = buttonObject.GetComponent<Button>();
            }

            return button;
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
