//Adaptado por POMPIC 20100333
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    public class EmployeeManagementUI : MonoBehaviour
    {
        private readonly Dictionary<int, EmployeeCardWidgets> cards = new Dictionary<int, EmployeeCardWidgets>();

        private TMP_Text summaryLabel;
        private TMP_Text lockLabel;
        private bool built;

        void OnEnable()
        {
            EmployeeSystem.onEmployeesChanged += Refresh;
            EntrepreneurProgress.onProgressChanged += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            EmployeeSystem.onEmployeesChanged -= Refresh;
            EntrepreneurProgress.onProgressChanged -= Refresh;
        }

        public void Build()
        {
            if (built)
                return;

            built = true;
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image background = gameObject.AddComponent<Image>();
            background.color = new Color(0.05f, 0.06f, 0.07f, 0.94f);

            VerticalLayoutGroup root = gameObject.AddComponent<VerticalLayoutGroup>();
            root.padding = new RectOffset(18, 18, 16, 16);
            root.spacing = 10;
            root.childControlWidth = true;
            root.childControlHeight = true;
            root.childForceExpandWidth = true;
            root.childForceExpandHeight = false;

            TMP_Text title = CreateText("Title", transform, "Empleados", 28, FontStyles.Bold, TextAlignmentOptions.Left);
            title.color = Color.white;

            summaryLabel = CreateText("Summary", transform, string.Empty, 14, FontStyles.Normal, TextAlignmentOptions.Left);
            summaryLabel.color = new Color(0.84f, 0.87f, 0.89f);

            lockLabel = CreateText("LockMessage", transform, string.Empty, 14, FontStyles.Bold, TextAlignmentOptions.Left);
            lockLabel.color = new Color(1f, 0.78f, 0.25f);

            GameObject scroll = CreateScrollView("Employees Scroll", transform, out Transform content);
            LayoutElement scrollLayout = scroll.AddComponent<LayoutElement>();
            scrollLayout.flexibleHeight = 1;
            scrollLayout.minHeight = 500;

            VerticalLayoutGroup list = content.gameObject.AddComponent<VerticalLayoutGroup>();
            list.padding = new RectOffset(10, 10, 10, 10);
            list.spacing = 10;
            list.childControlWidth = true;
            list.childControlHeight = true;
            list.childForceExpandWidth = true;
            list.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            for (int i = 1; i <= EmployeeSystem.MaxEmployees; i++)
                cards[i] = CreateEmployeeCard(content, i);

            Refresh();
        }

        public void Refresh()
        {
            if (summaryLabel == null || EmployeeSystem.Instance == null)
                return;

            int unlocked = EmployeeSystem.Instance.GetUnlockedCount();
            int hired = EmployeeSystem.Instance.GetHiredCount();
            summaryLabel.text = "Desbloqueados: " + unlocked + " | Contratados: " + hired + " | Límite máximo: 18";

            bool appUnlocked = EmployeeSystem.Instance.HasUnlockedEmployees();
            lockLabel.text = appUnlocked
                ? ""
                : "Desbloquea Empleado 1 en el Árbol del Emprendedor para gestionar empleados.";

            IReadOnlyList<EmployeeData> employees = EmployeeSystem.Instance.GetAllEmployees();
            foreach (EmployeeData employee in employees)
            {
                if (!cards.TryGetValue(employee.employeeNumber, out EmployeeCardWidgets card))
                    continue;

                bool unlockedEmployee = EntrepreneurProgress.IsEmployeeUnlocked(employee.employeeNumber);
                card.state.text = employee.hired
                    ? "Contratado"
                    : unlockedEmployee ? "Desbloqueado" : "Bloqueado";

                card.role.text = "Rol actual: " + GetRoleLabel(employee.role);
                card.workstation.text = "Puesto: " + employee.status;

                if (!unlockedEmployee)
                    card.requirement.text = EmployeeSystem.Instance.GetLockedEmployeeMessage(employee.employeeNumber);
                else if (!employee.hired)
                    card.requirement.text = "Disponible para contratación.";
                else
                    card.requirement.text = "";

                card.hireButton.interactable = unlockedEmployee && !employee.hired;
                card.cashierButton.interactable = employee.hired;
                card.restockerButton.interactable = employee.hired;
            }
        }

        private EmployeeCardWidgets CreateEmployeeCard(Transform parent, int employeeNumber)
        {
            GameObject panel = CreatePanel("Employee_" + employeeNumber, parent, new Color(0.09f, 0.11f, 0.13f, 0.95f));
            LayoutElement layout = panel.AddComponent<LayoutElement>();
            layout.preferredHeight = 180;

            VerticalLayoutGroup group = panel.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(10, 10, 10, 10);
            group.spacing = 4;
            group.childControlWidth = true;
            group.childControlHeight = true;

            TMP_Text title = CreateText("Title", panel.transform, "Empleado " + employeeNumber, 20, FontStyles.Bold, TextAlignmentOptions.Left);
            title.color = Color.white;

            TMP_Text state = CreateText("State", panel.transform, string.Empty, 14, FontStyles.Bold, TextAlignmentOptions.Left);
            TMP_Text role = CreateText("Role", panel.transform, string.Empty, 13, FontStyles.Normal, TextAlignmentOptions.Left);
            TMP_Text workstation = CreateText("Workstation", panel.transform, string.Empty, 13, FontStyles.Normal, TextAlignmentOptions.Left);
            TMP_Text requirement = CreateText("Requirement", panel.transform, string.Empty, 12, FontStyles.Italic, TextAlignmentOptions.Left);
            requirement.color = new Color(0.92f, 0.83f, 0.63f);

            GameObject buttonRow = CreateLayoutBox("Buttons", panel.transform);
            HorizontalLayoutGroup buttonGroup = buttonRow.AddComponent<HorizontalLayoutGroup>();
            buttonGroup.spacing = 6;
            buttonGroup.childControlWidth = true;
            buttonGroup.childControlHeight = true;
            buttonGroup.childForceExpandWidth = false;

            Button hireButton = CreateButton(buttonRow.transform, "Contratar", () => HireEmployee(employeeNumber));
            Button cashierButton = CreateButton(buttonRow.transform, "Cajero", () => AssignRole(employeeNumber, EmployeeRole.Cashier));
            Button restockerButton = CreateButton(buttonRow.transform, "Surtidor", () => AssignRole(employeeNumber, EmployeeRole.Restocker));

            return new EmployeeCardWidgets
            {
                state = state,
                role = role,
                workstation = workstation,
                requirement = requirement,
                hireButton = hireButton,
                cashierButton = cashierButton,
                restockerButton = restockerButton
            };
        }

        private void HireEmployee(int employeeNumber)
        {
            if (EmployeeSystem.Instance.TryHireEmployee(employeeNumber, out string message))
            {
                ShowMessage(message);
                Refresh();
                return;
            }

            ShowMessage(message);
            Refresh();
        }

        private void AssignRole(int employeeNumber, EmployeeRole role)
        {
            if (EmployeeSystem.Instance.TryAssignRole(employeeNumber, role, out string message))
            {
                ShowMessage(message);
                Refresh();
                return;
            }

            ShowMessage(message);
            Refresh();
        }

        private void ShowMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message) || UIGame.Instance == null)
                return;

            UIGame.Instance.ShowMessage(message);
        }

        private static string GetRoleLabel(EmployeeRole role)
        {
            switch (role)
            {
                case EmployeeRole.Cashier:
                    return "Cajero";
                case EmployeeRole.Restocker:
                    return "Surtidor";
                default:
                    return "Sin rol";
            }
        }

        private static GameObject CreateLayoutBox(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = CreateLayoutBox(name, parent);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static TMP_Text CreateText(string name, Transform parent, string text, int size, FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject obj = CreateLayoutBox(name, parent);
            TextMeshProUGUI label = obj.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.raycastTarget = false;
            obj.AddComponent<LayoutElement>();
            return label;
        }

        private static Button CreateButton(Transform parent, string text, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObj = CreatePanel("Button_" + text, parent, new Color(0.14f, 0.24f, 0.35f, 1f));
            LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
            layout.preferredHeight = 38;
            layout.preferredWidth = 140;

            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonObj.GetComponent<Image>();
            button.onClick.AddListener(action);

            TMP_Text label = CreateText("Text", buttonObj.transform, text, 13, FontStyles.Bold, TextAlignmentOptions.Center);
            label.color = Color.white;
            return button;
        }

        private static GameObject CreateScrollView(string name, Transform parent, out Transform content)
        {
            GameObject scroll = CreatePanel(name, parent, new Color(0.07f, 0.08f, 0.09f, 0.95f));
            ScrollRect scrollRect = scroll.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 25f;

            GameObject viewport = CreateLayoutBox("Viewport", scroll.transform);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(8, 8);
            viewportRect.offsetMax = new Vector2(-8, -8);
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = Color.clear;
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject contentObject = CreateLayoutBox("Content", viewport.transform);
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0);

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            content = contentObject.transform;
            return scroll;
        }

        private class EmployeeCardWidgets
        {
            public TMP_Text state;
            public TMP_Text role;
            public TMP_Text workstation;
            public TMP_Text requirement;
            public Button hireButton;
            public Button cashierButton;
            public Button restockerButton;
        }
    }

    public static class EmployeeManagementUIBootstrap
    {
        public static void Ensure(UIShopDesktop desktop)
        {
            if (desktop == null)
                return;

            EmployeeSystemBootstrap.EnsureInScene();

            Transform categories = FindRecursive(desktop.transform, "Categories");
            Transform navigation = FindRecursive(desktop.transform, "Navigation");
            if (categories == null || navigation == null)
                return;

            Transform existingPanel = FindRecursive(categories, "Employees");
            GameObject panel;
            EmployeeManagementUI ui;

            if (existingPanel != null)
            {
                panel = existingPanel.gameObject;
                ui = panel.GetComponent<EmployeeManagementUI>();
                if (ui == null)
                    ui = panel.AddComponent<EmployeeManagementUI>();
            }
            else
            {
                panel = new GameObject("Employees", typeof(RectTransform));
                panel.transform.SetParent(categories, false);
                panel.SetActive(false);
                ui = panel.AddComponent<EmployeeManagementUI>();
                ui.Build();
            }

            if (ui != null)
                ui.Build();

            Button button = FindEmployeesButton(navigation);
            if (button == null)
                button = CreateEmployeesButton(navigation);

            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = "Empleados";
                text.textWrappingMode = TextWrappingModes.NoWrap;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                UIShopCategoryHelper helper = categories.GetComponent<UIShopCategoryHelper>();
                if (helper != null)
                    helper.Show(panel);
                else
                    panel.SetActive(true);

                if (ui != null)
                    ui.Refresh();

                if (EmployeeSystem.Instance != null && !EmployeeSystem.Instance.HasUnlockedEmployees() && UIGame.Instance != null)
                    UIGame.Instance.ShowMessage("Empleados bloqueado. Desbloquea Empleado 1 en el Árbol del Emprendedor.");
            });
        }

        private static Button FindEmployeesButton(Transform navigation)
        {
            for (int i = 0; i < navigation.childCount; i++)
            {
                Transform child = navigation.GetChild(i);
                if (child.name.Contains("Employees"))
                    return child.GetComponent<Button>();
            }

            return null;
        }

        private static Button CreateEmployeesButton(Transform navigation)
        {
            Button templateButton = navigation.GetComponentInChildren<Button>(true);
            if (templateButton != null)
            {
                GameObject buttonObject = Object.Instantiate(templateButton.gameObject, navigation, false);
                buttonObject.name = "Button - Employees";
                return buttonObject.GetComponent<Button>();
            }

            GameObject fallback = new GameObject("Button - Employees", typeof(RectTransform), typeof(Image), typeof(Button));
            fallback.transform.SetParent(navigation, false);
            RectTransform rect = fallback.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(210, 40);
            return fallback.GetComponent<Button>();
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
