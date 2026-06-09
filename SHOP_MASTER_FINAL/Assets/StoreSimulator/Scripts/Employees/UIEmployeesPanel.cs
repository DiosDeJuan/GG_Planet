//Adaptado por POMPIC 20100333
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    public class UIEmployeesPanel : MonoBehaviour
    {
        private static readonly Color PanelBackground = new Color(0.035f, 0.04f, 0.07f, 0.99f);
        private static readonly Color CardLocked = new Color(0.11f, 0.12f, 0.16f, 0.92f);
        private static readonly Color CardAvailable = new Color(0.1f, 0.17f, 0.24f, 0.98f);
        private static readonly Color CardHired = new Color(0.11f, 0.27f, 0.18f, 0.98f);
        private static readonly Color AccentPink = new Color(1f, 0f, 0.392f, 1f);
        private static readonly Color AccentCyan = new Color(0.1f, 0.88f, 1f, 1f);
        private static readonly Color TextMuted = new Color(0.68f, 0.72f, 0.82f, 1f);

        private Transform listContainer;
        private TMP_Text summaryText;
        private TMP_Text detailTitle;
        private TMP_Text detailText;
        private string selectedEmployeeId = "empleado_1";
        private string activeFilter = "Todos";

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

            Image background = gameObject.AddComponent<Image>();
            background.color = PanelBackground;

            VerticalLayoutGroup layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 10, 10);
            layout.spacing = 8;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            BuildHeader(transform);
            BuildFilters(transform);
            BuildBody(transform);
        }

        public void Refresh()
        {
            if (listContainer == null)
                return;

            EmployeeManager manager = EmployeeManager.EnsureInstance();
            foreach (Transform child in listContainer)
                Destroy(child.gameObject);

            IReadOnlyList<EntrepreneurTreeNodeDefinition> definitions = EmployeeManager.EmployeeDefinitions;
            int hiredCount = manager.GetHiredEmployeeCount();
            long salaryTotal = hiredCount * EmployeeManager.DefaultDailySalary;
            summaryText.text = "Contratados: " + hiredCount + "/18   Salarios diarios: " + StoreDatabase.FromLongToStringMoney(salaryTotal) + "   Filtro: " + activeFilter;

            if (!definitions.Any(definition => definition.Id == selectedEmployeeId))
                selectedEmployeeId = definitions.Count > 0 ? definitions[0].Id : string.Empty;

            foreach (EntrepreneurTreeNodeDefinition definition in definitions)
            {
                if (PassesFilter(definition, manager))
                    CreateEmployeeCard(definition, manager);
            }

            RefreshDetails(manager);
        }

        private void BuildHeader(Transform parent)
        {
            GameObject header = CreatePanel("Header", parent, new Color(0.075f, 0.085f, 0.13f, 0.98f));
            header.AddComponent<LayoutElement>().preferredHeight = 76;

            HorizontalLayoutGroup group = header.AddComponent<HorizontalLayoutGroup>();
            group.padding = new RectOffset(14, 14, 8, 8);
            group.spacing = 12;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;

            GameObject textBox = CreateBox("Header Text", header.transform);
            VerticalLayoutGroup textLayout = textBox.AddComponent<VerticalLayoutGroup>();
            textLayout.spacing = 2;

            TMP_Text title = CreateText("Title", textBox.transform, 22, FontStyles.Bold, TextAlignmentOptions.Left);
            title.text = "EMPLEADOS";
            TMP_Text subtitle = CreateText("Subtitle", textBox.transform, 12, FontStyles.Normal, TextAlignmentOptions.Left);
            subtitle.text = "Contrata empleados desbloqueados por el Arbol y asigna roles operativos sin duplicar NPCs.";
            subtitle.color = TextMuted;

            summaryText = CreateText("Summary", header.transform, 14, FontStyles.Bold, TextAlignmentOptions.Right);
            summaryText.color = AccentCyan;
            summaryText.GetComponent<LayoutElement>().preferredWidth = 340;
        }

        private void BuildFilters(Transform parent)
        {
            GameObject filters = CreatePanel("Filters", parent, new Color(0.075f, 0.085f, 0.13f, 0.98f));
            filters.AddComponent<LayoutElement>().preferredHeight = 40;

            HorizontalLayoutGroup group = filters.AddComponent<HorizontalLayoutGroup>();
            group.padding = new RectOffset(8, 8, 6, 6);
            group.spacing = 6;
            group.childControlWidth = false;
            group.childControlHeight = true;

            string[] filterNames = { "Todos", "Bloqueados", "Disponibles", "Contratados", "Cajeros", "Surtidores" };
            for (int i = 0; i < filterNames.Length; i++)
            {
                string filter = filterNames[i];
                CreateActionButton(filters.transform, filter, () =>
                {
                    activeFilter = filter;
                    Refresh();
                }, 122, 30);
            }
        }

        private void BuildBody(Transform parent)
        {
            GameObject body = CreateBox("Body", parent);
            body.AddComponent<LayoutElement>().flexibleHeight = 1;
            HorizontalLayoutGroup group = body.AddComponent<HorizontalLayoutGroup>();
            group.spacing = 10;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandHeight = true;

            ScrollRect scroll = CreateScroll("Employees Scroll", body.transform);
            listContainer = scroll.content;

            GameObject details = CreatePanel("Details", body.transform, new Color(0.075f, 0.085f, 0.13f, 0.98f));
            LayoutElement detailsLayout = details.AddComponent<LayoutElement>();
            detailsLayout.preferredWidth = 300;
            detailsLayout.minWidth = 300;

            VerticalLayoutGroup detailsGroup = details.AddComponent<VerticalLayoutGroup>();
            detailsGroup.padding = new RectOffset(12, 12, 12, 12);
            detailsGroup.spacing = 8;
            detailsGroup.childControlWidth = true;
            detailsGroup.childControlHeight = false;

            detailTitle = CreateText("Detail Title", details.transform, 19, FontStyles.Bold, TextAlignmentOptions.Left);
            detailText = CreateText("Detail Text", details.transform, 13, FontStyles.Normal, TextAlignmentOptions.Left);
            detailText.color = TextMuted;
            LayoutElement detailTextLayout = detailText.GetComponent<LayoutElement>();
            detailTextLayout.flexibleHeight = 1;
            detailTextLayout.minHeight = 260;
        }

        private void CreateEmployeeCard(EntrepreneurTreeNodeDefinition definition, EmployeeManager manager)
        {
            EmployeeState state = manager.GetState(definition.Id);
            bool unlocked = EntrepreneurProgress.IsUnlocked(definition.Id);
            Color cardColor = state.isHired ? CardHired : unlocked ? CardAvailable : CardLocked;
            GameObject card = CreatePanel(definition.Id, listContainer, cardColor);
            card.AddComponent<LayoutElement>().minHeight = 118;

            Button selectButton = card.AddComponent<Button>();
            selectButton.targetGraphic = card.GetComponent<Image>();
            selectButton.onClick.AddListener(() =>
            {
                selectedEmployeeId = definition.Id;
                RefreshDetails(manager);
            });

            VerticalLayoutGroup layout = card.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 8, 8);
            layout.spacing = 6;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            TMP_Text title = CreateText("Title", card.transform, 15, FontStyles.Bold, TextAlignmentOptions.Left);
            title.text = definition.Title + "  |  " + GetStateLabel(unlocked, state);
            title.color = state.isHired ? new Color(0.5f, 1f, 0.64f, 1f) : unlocked ? AccentCyan : TextMuted;

            TMP_Text info = CreateText("Info", card.transform, 12, FontStyles.Normal, TextAlignmentOptions.Left);
            info.color = TextMuted;
            info.text = "Requiere: " + GetRequirementText(definition) + "\nSalario: " + StoreDatabase.FromLongToStringMoney(EmployeeManager.DefaultDailySalary) + "/dia   Rol: " + GetRoleLabel(state.role);

            GameObject actions = CreateBox("Actions", card.transform);
            HorizontalLayoutGroup actionsLayout = actions.AddComponent<HorizontalLayoutGroup>();
            actionsLayout.spacing = 6;
            actionsLayout.childControlWidth = false;
            actionsLayout.childControlHeight = true;

            if (!state.isHired)
            {
                Button hire = CreateActionButton(actions.transform, unlocked ? "Contratar" : "Bloqueado", () => ShowResult(manager.TryHire(definition.Id, out string message), message), 112, 32);
                hire.interactable = unlocked;
            }
            else
            {
                CreateActionButton(actions.transform, "Cajero", () => ShowResult(manager.TryAssignRole(definition.Id, EmployeeRole.Cashier, out string message), message), 96, 32);
                CreateActionButton(actions.transform, "Surtidor", () => ShowResult(manager.TryAssignRole(definition.Id, EmployeeRole.Restocker, out string message), message), 96, 32);
                CreateActionButton(actions.transform, "Sin rol", () => ShowResult(manager.TryAssignRole(definition.Id, EmployeeRole.None, out string message), message), 96, 32);
            }
        }

        private bool PassesFilter(EntrepreneurTreeNodeDefinition definition, EmployeeManager manager)
        {
            EmployeeState state = manager.GetState(definition.Id);
            bool unlocked = EntrepreneurProgress.IsUnlocked(definition.Id);
            switch (activeFilter)
            {
                case "Bloqueados": return !unlocked;
                case "Disponibles": return unlocked && !state.isHired;
                case "Contratados": return state.isHired;
                case "Cajeros": return state.isHired && state.role == EmployeeRole.Cashier;
                case "Surtidores": return state.isHired && state.role == EmployeeRole.Restocker;
                default: return true;
            }
        }

        private void RefreshDetails(EmployeeManager manager)
        {
            if (detailTitle == null || detailText == null || string.IsNullOrEmpty(selectedEmployeeId))
                return;

            EntrepreneurTreeNodeDefinition definition = EntrepreneurTreeDefinitions.Get(selectedEmployeeId);
            if (definition == null)
                return;

            EmployeeState state = manager.GetState(definition.Id);
            bool unlocked = EntrepreneurProgress.IsUnlocked(definition.Id);
            detailTitle.text = definition.Title;
            detailText.text =
                "Estado: " + GetStateLabel(unlocked, state) + "\n" +
                "Prerequisito: " + GetRequirementText(definition) + "\n" +
                "Rol actual: " + GetRoleLabel(state.role) + "\n" +
                "Salario diario: " + StoreDatabase.FromLongToStringMoney(EmployeeManager.DefaultDailySalary) + "\n" +
                "Nodo del Arbol: " + definition.Id + "\n\n" +
                "Beneficio:\nDisponible para contratarse desde esta app cuando su nodo del Arbol este desbloqueado. Puede operar como cajero automatico o surtidor segun el rol asignado.\n\n" +
                (unlocked ? "Listo para contratar o gestionar rol." : EntrepreneurProgress.GetStateDescription(definition).Replace("\n", " - "));
        }

        private string GetRequirementText(EntrepreneurTreeNodeDefinition definition)
        {
            if (definition.Prerequisites.Length == 0)
                return "Ninguno";

            return string.Join(", ", System.Array.ConvertAll(definition.Prerequisites, EntrepreneurTreeDefinitions.GetTitle));
        }

        private string GetStateLabel(bool unlocked, EmployeeState state)
        {
            if (state.isHired)
                return "Contratado";
            return unlocked ? "Disponible" : "Bloqueado";
        }

        private string GetRoleLabel(EmployeeRole role)
        {
            if (role == EmployeeRole.Cashier)
                return "Cajero";
            if (role == EmployeeRole.Restocker)
                return "Surtidor";
            return "Sin rol";
        }

        private void ShowResult(bool success, string message)
        {
            if (UIGame.Instance != null)
                UIGame.Instance.ShowMessage(message);

            if (success && UIGame.Instance != null)
                UIGame.AddNotification(message);

            Refresh();
        }

        private static GameObject CreateBox(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            obj.GetComponent<Image>().color = color;
            return obj;
        }

        private static TMP_Text CreateText(string name, Transform parent, int size, FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject obj = CreateBox(name, parent);
            TMP_Text text = obj.AddComponent<TextMeshProUGUI>();
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = Color.white;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Max(9, size - 4);
            text.fontSizeMax = size;
            obj.AddComponent<LayoutElement>().minHeight = size * 1.55f;
            return text;
        }

        private static Button CreateActionButton(Transform parent, string label, UnityEngine.Events.UnityAction action, float width, float height)
        {
            GameObject obj = CreatePanel(label, parent, AccentPink);
            Button button = obj.AddComponent<Button>();
            button.targetGraphic = obj.GetComponent<Image>();
            TMP_Text text = CreateText("Label", obj.transform, 12, FontStyles.Bold, TextAlignmentOptions.Center);
            text.text = label;
            LayoutElement element = obj.AddComponent<LayoutElement>();
            element.preferredWidth = width;
            element.minWidth = width;
            element.preferredHeight = height;
            element.minHeight = height;
            button.onClick.AddListener(action);
            return button;
        }

        private static ScrollRect CreateScroll(string name, Transform parent)
        {
            GameObject scrollObject = CreatePanel(name, parent, new Color(0.025f, 0.03f, 0.055f, 0.96f));
            scrollObject.AddComponent<LayoutElement>().flexibleHeight = 1;
            ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollObject.transform, false);
            viewport.GetComponent<Image>().color = Color.clear;
            viewport.GetComponent<Mask>().showMaskGraphic = false;
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(6, 6);
            viewportRect.offsetMax = new Vector2(-6, -6);

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
            scroll.vertical = true;
            scroll.scrollSensitivity = 24f;
            return scroll;
        }
    }
}
