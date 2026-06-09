//Adaptado por POMPIC 20100333
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    public class UIEmployeesPanel : MonoBehaviour
    {
        private Transform listContainer;
        private TMP_Text statusText;

        void OnEnable()
        {
            Refresh();
            EntrepreneurProgress.onProgressChanged += Refresh;
            EmployeeManager.onEmployeesChanged += Refresh;
        }

        void OnDisable()
        {
            EntrepreneurProgress.onProgressChanged -= Refresh;
            EmployeeManager.onEmployeesChanged -= Refresh;
        }

        public void Build()
        {
            RectTransform root = gameObject.GetComponent<RectTransform>();
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.spacing = 10;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            statusText = CreateText("Status", transform, 18, FontStyles.Bold);
            statusText.text = "Desbloquea un empleado en el Arbol del Emprendedor para usar esta app.";

            TMP_Text salaryText = CreateText("Salary", transform, 15, FontStyles.Normal);
            salaryText.text = "Salario diario: " + StoreDatabase.FromLongToStringMoney(EmployeeManager.DefaultDailySalary) + "/dia";

            TMP_Text rolesText = CreateText("Roles", transform, 14, FontStyles.Normal);
            rolesText.text = "Cajero: atiende clientes automaticamente en caja registradora.\nSurtidor: abastece muebles de venta usando productos del almacen.";

            ScrollRect scroll = CreateScroll("Employees Scroll", transform);
            listContainer = scroll.content;
        }

        public void Refresh()
        {
            if (listContainer == null)
                return;

            EmployeeManager manager = EmployeeManager.EnsureInstance();
            foreach (Transform child in listContainer)
                Destroy(child.gameObject);

            bool anyUnlocked = manager.HasAnyUnlockedEmployee();
            statusText.text = anyUnlocked
                ? "Empleados desbloqueados desde el Arbol del Emprendedor."
                : "Desbloquea un empleado en el Arbol del Emprendedor para usar esta app.";

            foreach (EntrepreneurTreeNodeDefinition definition in EmployeeManager.EmployeeDefinitions)
                CreateEmployeeRow(definition, manager, anyUnlocked);
        }

        private void CreateEmployeeRow(EntrepreneurTreeNodeDefinition definition, EmployeeManager manager, bool anyUnlocked)
        {
            GameObject row = CreatePanel(definition.Id, listContainer, new Color(0.13f, 0.15f, 0.17f, 1f));
            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 8, 8);
            layout.spacing = 8;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            row.AddComponent<LayoutElement>().minHeight = 76;

            TMP_Text info = CreateText("Info", row.transform, 14, FontStyles.Normal);
            info.alignment = TextAlignmentOptions.MidlineLeft;
            info.text = BuildInfoText(definition, manager.GetState(definition.Id));
            info.GetComponent<LayoutElement>().preferredWidth = 360;

            EmployeeState state = manager.GetState(definition.Id);
            bool unlocked = EntrepreneurProgress.IsUnlocked(definition.Id);
            if (unlocked && !state.isHired)
                CreateActionButton(row.transform, "Contratar", () => ShowResult(manager.TryHire(definition.Id, out string message), message));

            if (state.isHired)
            {
                CreateActionButton(row.transform, "Asignar Cajero", () => ShowResult(manager.TryAssignRole(definition.Id, EmployeeRole.Cashier, out string message), message));
                CreateActionButton(row.transform, "Asignar Surtidor", () => ShowResult(manager.TryAssignRole(definition.Id, EmployeeRole.Restocker, out string message), message));
            }

            if (!anyUnlocked || !unlocked)
            {
                Button locked = CreateActionButton(row.transform, "Bloqueado", () =>
                {
                    List<string> missing = EntrepreneurProgress.GetMissingPrerequisites(definition);
                    string required = missing.Count > 0 ? string.Join(", ", missing) : definition.Title;
                    ShowResult(false, "Empleado bloqueado. Falta desbloquear: " + required + ".");
                });
                locked.interactable = anyUnlocked;
            }
        }

        private string BuildInfoText(EntrepreneurTreeNodeDefinition definition, EmployeeState state)
        {
            bool unlocked = EntrepreneurProgress.IsUnlocked(definition.Id);
            string status = unlocked ? "Disponible para contratar" : EntrepreneurProgress.GetStateDescription(definition).Replace("\n", ". ");
            if (state.isHired)
                status = "Contratado";

            string role = state.role == EmployeeRole.Cashier ? "Cajero" : state.role == EmployeeRole.Restocker ? "Surtidor" : "Sin asignar";
            return definition.Title + "\nEstado: " + status + "\nRol actual: " + role;
        }

        private void ShowResult(bool success, string message)
        {
            if (UIGame.Instance != null)
                UIGame.Instance.ShowMessage(message);

            if (success && UIGame.Instance != null)
                UIGame.AddNotification(message);

            Refresh();
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            obj.GetComponent<Image>().color = color;
            return obj;
        }

        private static TMP_Text CreateText(string name, Transform parent, int size, FontStyles style)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            TMP_Text text = obj.AddComponent<TextMeshProUGUI>();
            text.fontSize = size;
            text.fontStyle = style;
            text.color = Color.white;
            text.textWrappingMode = TextWrappingModes.Normal;
            obj.AddComponent<LayoutElement>().minHeight = size * 1.6f;
            return text;
        }

        private static Button CreateActionButton(Transform parent, string label, UnityEngine.Events.UnityAction action)
        {
            GameObject obj = CreatePanel(label, parent, new Color(0.22f, 0.45f, 0.75f, 1f));
            Button button = obj.AddComponent<Button>();
            TMP_Text text = CreateText("Label", obj.transform, 13, FontStyles.Bold);
            text.text = label;
            text.alignment = TextAlignmentOptions.Center;
            LayoutElement element = obj.AddComponent<LayoutElement>();
            element.preferredWidth = 130;
            element.minHeight = 48;
            button.onClick.AddListener(action);
            return button;
        }

        private static ScrollRect CreateScroll(string name, Transform parent)
        {
            GameObject scrollObject = CreatePanel(name, parent, new Color(0.07f, 0.08f, 0.09f, 0.95f));
            scrollObject.AddComponent<LayoutElement>().flexibleHeight = 1;
            ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollObject.transform, false);
            viewport.GetComponent<Image>().color = new Color(0, 0, 0, 0.05f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            VerticalLayoutGroup contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 8;
            contentLayout.padding = new RectOffset(4, 4, 4, 4);
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandWidth = true;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            scroll.horizontal = false;
            return scroll;
        }
    }
}
