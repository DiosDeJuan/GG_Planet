//Adaptado por POMPIC 20100333
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    public static class EntrepreneurTreeUIBootstrap
    {
        private const string LicensesPanelName = "Licenses";
        private const string LegacyTreeButtonName = "Button - Entrepreneur Tree";
        private const string TreeContentName = "Entrepreneur Tree Content";

        public static void Ensure(UIShopDesktop desktop)
        {
            if (desktop == null)
                return;

            Transform contentArea = FindRecursive(desktop.transform, "ContentArea");
            Transform navigation = FindRecursive(desktop.transform, "Navigation");
            if (contentArea == null || navigation == null)
                return;

            RemoveGeneratedObject(navigation, LegacyTreeButtonName);

            Transform licensesPanel = FindDirectChild(contentArea, LicensesPanelName);
            if (licensesPanel == null)
                return;

            PrepareLicensesPanelAsTreeHost(licensesPanel);
            EnsureTreeContent(licensesPanel);
            licensesPanel.gameObject.SetActive(false);
            Button licensesButton = FindButton(navigation, "Button - Licenses", "LICENSES");
            ConfigureTabText(licensesButton, "ARBOL");

            UIShopCategoryHelper helper = contentArea.GetComponent<UIShopCategoryHelper>();
            if (licensesButton != null)
            {
                licensesButton.onClick.AddListener(() =>
                {
                    if (helper != null)
                        helper.Show(licensesPanel.gameObject);
                    else
                        licensesPanel.gameObject.SetActive(true);
                });
            }
        }

        private static void PrepareLicensesPanelAsTreeHost(Transform licensesPanel)
        {
            UIShopCategory category = licensesPanel.GetComponent<UIShopCategory>();
            if (category != null)
            {
                category.enabled = false;
                Object.Destroy(category);
            }

            for (int i = licensesPanel.childCount - 1; i >= 0; i--)
            {
                Transform child = licensesPanel.GetChild(i);
                if (child.name == TreeContentName)
                    continue;

                child.gameObject.SetActive(false);
                Object.Destroy(child.gameObject);
            }
        }

        private static GameObject EnsureTreeContent(Transform licensesPanel)
        {
            Transform existing = FindDirectChild(licensesPanel, TreeContentName);
            if (existing != null)
                return existing.gameObject;

            GameObject treeContent = new GameObject(TreeContentName, typeof(RectTransform));
            treeContent.transform.SetParent(licensesPanel, false);
            Stretch(treeContent.GetComponent<RectTransform>());

            EntrepreneurTreeUI treeUI = treeContent.AddComponent<EntrepreneurTreeUI>();
            treeUI.Build();
            return treeContent;
        }

        private static Button FindButton(Transform root, string objectName, string currentText)
        {
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].name == objectName)
                    return buttons[i];
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                TMP_Text text = buttons[i].GetComponentInChildren<TMP_Text>(true);
                if (text != null && text.text.Trim().ToUpperInvariant() == currentText)
                    return buttons[i];
            }

            return null;
        }

        private static void ConfigureTabText(Button button, string label)
        {
            if (button == null)
                return;

            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
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
