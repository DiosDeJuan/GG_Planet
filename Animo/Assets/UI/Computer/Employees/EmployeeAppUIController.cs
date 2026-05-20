using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Runtime employee app integrated in the computer UPGRADES/Expansions area.
    /// </summary>
    [DisallowMultipleComponent]
    public class EmployeeAppUIController : MonoBehaviour
    {
        private const string LogPrefix = "[EmployeeApp] ";
        private const int MaxEmployees = EntrepreneurEmployeeSystem.MaxEmployees;
        private const string CashierDescription = "Cajero: Atiende clientes en cajas registradoras, procesa pagos automáticamente y reduce abandono por espera.";
        private const string RestockerDescription = "Surtidor: Reabastece muebles de venta usando productos disponibles en almacén cuando existan espacios asignados.";

        private readonly Dictionary<int, EmployeeCardUI> cards = new Dictionary<int, EmployeeCardUI>();

        private GameObject appRoot;
        private GameObject listContent;
        private Button openButton;
        private Button closeButton;
        private TMP_Text detailTitle;
        private TMP_Text detailStatus;
        private TMP_Text detailCost;
        private TMP_Text detailHint;
        private Button hireButton;
        private Button cashierButton;
        private Button restockerButton;
        private int selectedEmployeeId = 1;
        private bool listenersBound;

        void Awake()
        {
            EnsureHierarchy();
            BindListeners();
            RefreshAll();
            ShowApp(false);
        }


        void OnEnable()
        {
            // When the dedicated panel becomes active (tab selected), always show the app root.
            ShowApp(true);
            RefreshAll();
        }


        private void BindListeners()
        {
            if (listenersBound)
                return;

            EntrepreneurTreeManager.onNodeUnlocked += OnTreeNodeUnlocked;
            EntrepreneurEmployeeSystem.onEmployeeHired += OnEmployeeHired;
            EntrepreneurEmployeeSystem.onEmployeeRoleChanged += OnEmployeeRoleChanged;
            SaveGameSystem.dataLoadEvent += OnDataLoaded;

            if (openButton != null)
                openButton.onClick.AddListener(() => ShowApp(true));
            if (closeButton != null)
                closeButton.onClick.AddListener(() => ShowApp(false));
            if (hireButton != null)
                hireButton.onClick.AddListener(OnHireClicked);
            if (cashierButton != null)
                cashierButton.onClick.AddListener(() => OnRoleClicked(EmployeeRole.Cashier));
            if (restockerButton != null)
                restockerButton.onClick.AddListener(() => OnRoleClicked(EmployeeRole.Restocker));

            listenersBound = true;
        }


        private void OnTreeNodeUnlocked(NodeData node)
        {
            if (node == null || node.nodeType != TreeNodeType.Employee)
                return;

            RefreshAll();
        }


        private void OnEmployeeHired(int employeeId)
        {
            SelectEmployee(employeeId);
            RefreshAll();
        }


        private void OnEmployeeRoleChanged(int employeeId, EmployeeRole role)
        {
            SelectEmployee(employeeId);
            RefreshAll();
        }


        private void OnDataLoaded()
        {
            RefreshAll();
        }


        private void ShowApp(bool show)
        {
            if (appRoot != null)
                appRoot.SetActive(show);
            if (openButton != null)
                openButton.gameObject.SetActive(!show);

            if (show)
                SelectEmployee(selectedEmployeeId);
        }


        private void SelectEmployee(int employeeId)
        {
            if (employeeId < 1 || employeeId > MaxEmployees)
            {
                Debug.LogWarning(LogPrefix + "Invalid employee id requested: " + employeeId);
            }

            selectedEmployeeId = Mathf.Clamp(employeeId, 1, MaxEmployees);
            foreach (KeyValuePair<int, EmployeeCardUI> pair in cards)
                pair.Value?.SetSelected(pair.Key == selectedEmployeeId);

            UpdateDetailPanel();
        }


        private void OnHireClicked()
        {
            if (EntrepreneurEmployeeSystem.Instance == null)
                return;

            string reason;
            if (!EntrepreneurEmployeeSystem.Instance.TryHireEmployee(selectedEmployeeId, out reason) && !string.IsNullOrEmpty(reason))
                UIGame.Instance?.ShowMessage(reason);

            RefreshAll();
        }


        private void OnRoleClicked(EmployeeRole role)
        {
            if (EntrepreneurEmployeeSystem.Instance == null)
                return;

            string reason;
            if (!EntrepreneurEmployeeSystem.Instance.TryAssignRole(selectedEmployeeId, role, out reason) && !string.IsNullOrEmpty(reason))
                UIGame.Instance?.ShowMessage(reason);

            RefreshAll();
        }


        private void RefreshAll()
        {
            for (int employeeId = 1; employeeId <= MaxEmployees; employeeId++)
            {
                EmployeeCardUI card;
                if (cards.TryGetValue(employeeId, out card) && card != null)
                    card.Refresh(GetStatus(employeeId));
            }

            UpdateDetailPanel();
        }


        private void UpdateDetailPanel()
        {
            EmployeeStatus status = GetStatus(selectedEmployeeId);

            if (detailTitle != null)
                detailTitle.text = "Empleado #" + selectedEmployeeId;

            if (detailStatus != null)
                detailStatus.text = "Estado: " + status.stateLabel;

            if (detailCost != null)
                detailCost.text = "Costo contratación: " + StoreDatabase.FromLongToStringMoney(status.hireCost);

            if (detailHint != null)
                detailHint.text = status.hint + "\n\n" + CashierDescription + "\n" + RestockerDescription;

            if (hireButton != null)
            {
                hireButton.gameObject.SetActive(!status.isHired);
                hireButton.interactable = status.canHire;
            }

            if (cashierButton != null)
            {
                cashierButton.gameObject.SetActive(status.isHired);
                cashierButton.interactable = status.isHired && status.role != EmployeeRole.Cashier;
            }

            if (restockerButton != null)
            {
                restockerButton.gameObject.SetActive(status.isHired);
                restockerButton.interactable = status.isHired && status.role != EmployeeRole.Restocker;
            }
        }


        private EmployeeStatus GetStatus(int employeeId)
        {
            EmployeeStatus status = new EmployeeStatus
            {
                hireCost = 0,
                isUnlocked = false,
                isHired = false,
                role = EmployeeRole.None,
                stateLabel = "Bloqueado",
                hint = "Desbloquea este empleado en el Árbol del Emprendedor",
                canHire = false
            };

            if (EntrepreneurEmployeeSystem.Instance == null)
                return status;

            EmployeeAssignment assignment = EntrepreneurEmployeeSystem.Instance.GetAssignment(employeeId);
            status.hireCost = assignment != null ? assignment.hireCost : 0;
            status.isUnlocked = EntrepreneurEmployeeSystem.Instance.IsEmployeeUnlocked(employeeId);
            status.isHired = assignment != null && assignment.isHired;
            status.role = assignment != null ? assignment.role : EmployeeRole.None;

            if (!status.isUnlocked)
            {
                status.stateLabel = "Bloqueado";
                status.hint = "Desbloquea este empleado en el Árbol del Emprendedor.";
                return status;
            }

            if (!status.isHired)
            {
                status.stateLabel = "Desbloqueado";
                status.hint = "Disponible para contratar.";
                string reason;
                status.canHire = EntrepreneurEmployeeSystem.Instance.CanHireEmployee(employeeId, out reason);
                if (!status.canHire && !string.IsNullOrEmpty(reason))
                    status.hint = reason;
                return status;
            }

            status.canHire = false;
            status.stateLabel = status.role == EmployeeRole.None
                ? "Contratado"
                : "Asignado: " + GetRoleLabel(status.role);
            status.hint = "Puedes cambiar su rol cuando lo necesites.";
            return status;
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


        private void EnsureHierarchy()
        {
            EnsureOpenButton();
            EnsureAppRoot();
            EnsureEmployeeCards();
        }


        private void EnsureOpenButton()
        {
            Transform existing = transform.Find("OpenEmployeesAppButton");
            if (existing != null)
            {
                openButton = existing.GetComponent<Button>();
                return;
            }

            GameObject open = CreateUIObject("OpenEmployeesAppButton", transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            RectTransform rt = open.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(280f, 46f);
            rt.anchoredPosition = new Vector2(-20f, -66f);
            Image bg = open.AddComponent<Image>();
            bg.color = new Color(0.18f, 0.26f, 0.46f, 0.95f);
            openButton = open.AddComponent<Button>();
            openButton.targetGraphic = bg;
            CreateText("Text", open.transform, "Abrir App Empleados", 18, TextAlignmentOptions.Center);
        }


        private void EnsureAppRoot()
        {
            Transform existing = transform.Find("EmployeesAppRoot");
            if (existing != null)
            {
                appRoot = existing.gameObject;
                CacheDetailReferences(existing);
                return;
            }

            appRoot = CreateUIObject("EmployeesAppRoot", transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            Image rootBg = appRoot.AddComponent<Image>();
            rootBg.color = new Color(0.05f, 0.08f, 0.13f, 0.95f);
            RectTransform rootRT = appRoot.GetComponent<RectTransform>();
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            GameObject header = CreateUIObject("Header", appRoot.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
            RectTransform headerRT = header.GetComponent<RectTransform>();
            headerRT.sizeDelta = new Vector2(0f, 70f);
            Image headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0.08f, 0.11f, 0.17f, 0.98f);
            CreateText("Title", header.transform, "App de Empleados", 28, TextAlignmentOptions.Left, new Vector2(20f, 0f), new Vector2(-20f, 0f));

            GameObject close = CreateUIObject("CloseButton", header.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f));
            RectTransform closeRT = close.GetComponent<RectTransform>();
            closeRT.sizeDelta = new Vector2(140f, 44f);
            closeRT.anchoredPosition = new Vector2(-20f, 0f);
            Image closeBg = close.AddComponent<Image>();
            closeBg.color = new Color(0.28f, 0.24f, 0.24f, 1f);
            closeButton = close.AddComponent<Button>();
            closeButton.targetGraphic = closeBg;
            CreateText("Text", close.transform, "Volver", 18, TextAlignmentOptions.Center);

            GameObject listPanel = CreateUIObject("ListPanel", appRoot.transform, new Vector2(0f, 0f), new Vector2(0.62f, 1f), new Vector2(0f, 0f));
            RectTransform listRT = listPanel.GetComponent<RectTransform>();
            listRT.offsetMin = new Vector2(16f, 16f);
            listRT.offsetMax = new Vector2(-8f, -82f);
            Image listBg = listPanel.AddComponent<Image>();
            listBg.color = new Color(0.09f, 0.12f, 0.18f, 0.95f);

            ScrollRect scrollRect = listPanel.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 24f;

            GameObject viewport = CreateUIObject("Viewport", listPanel.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.offsetMin = new Vector2(6f, 6f);
            viewportRT.offsetMax = new Vector2(-6f, -6f);
            Image viewportBg = viewport.AddComponent<Image>();
            viewportBg.color = new Color(0.06f, 0.09f, 0.14f, 0.9f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            GameObject content = CreateUIObject("Content", viewport.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.sizeDelta = new Vector2(0f, 1300f);
            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.spacing = 8;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            listContent = content;

            scrollRect.viewport = viewportRT;
            scrollRect.content = contentRT;

            GameObject detail = CreateUIObject("DetailPanel", appRoot.transform, new Vector2(0.62f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0f));
            RectTransform detailRT = detail.GetComponent<RectTransform>();
            detailRT.offsetMin = new Vector2(8f, 16f);
            detailRT.offsetMax = new Vector2(-16f, -82f);
            Image detailBg = detail.AddComponent<Image>();
            detailBg.color = new Color(0.09f, 0.11f, 0.16f, 0.98f);

            VerticalLayoutGroup detailLayout = detail.AddComponent<VerticalLayoutGroup>();
            detailLayout.padding = new RectOffset(16, 16, 16, 16);
            detailLayout.spacing = 10;
            detailLayout.childControlHeight = false;
            detailLayout.childControlWidth = true;
            detailLayout.childForceExpandHeight = false;
            detailLayout.childForceExpandWidth = true;

            detailTitle = CreateText("DetailTitle", detail.transform, "Empleado #1", 24, TextAlignmentOptions.Left);
            detailStatus = CreateText("DetailStatus", detail.transform, "Estado:", 20, TextAlignmentOptions.Left);
            detailCost = CreateText("DetailCost", detail.transform, "Costo:", 18, TextAlignmentOptions.Left);
            detailHint = CreateText("DetailHint", detail.transform, "", 16, TextAlignmentOptions.TopLeft);
            detailHint.textWrappingMode = TextWrappingModes.Normal;

            hireButton = CreateActionButton(detail.transform, "HireButton", "Contratar", new Color(0.14f, 0.42f, 0.22f, 1f));
            cashierButton = CreateActionButton(detail.transform, "CashierButton", "Asignar Cajero", new Color(0.10f, 0.45f, 0.55f, 1f));
            restockerButton = CreateActionButton(detail.transform, "RestockerButton", "Asignar Surtidor", new Color(0.58f, 0.34f, 0.08f, 1f));
        }


        private void CacheDetailReferences(Transform existingRoot)
        {
            Transform content = existingRoot.Find("ListPanel/Viewport/Content");
            if (content != null)
                listContent = content.gameObject;

            closeButton = existingRoot.Find("Header/CloseButton")?.GetComponent<Button>();
            detailTitle = existingRoot.Find("DetailPanel/DetailTitle")?.GetComponent<TMP_Text>();
            detailStatus = existingRoot.Find("DetailPanel/DetailStatus")?.GetComponent<TMP_Text>();
            detailCost = existingRoot.Find("DetailPanel/DetailCost")?.GetComponent<TMP_Text>();
            detailHint = existingRoot.Find("DetailPanel/DetailHint")?.GetComponent<TMP_Text>();
            hireButton = existingRoot.Find("DetailPanel/HireButton")?.GetComponent<Button>();
            cashierButton = existingRoot.Find("DetailPanel/CashierButton")?.GetComponent<Button>();
            restockerButton = existingRoot.Find("DetailPanel/RestockerButton")?.GetComponent<Button>();
        }


        private void EnsureEmployeeCards()
        {
            if (listContent == null)
                return;

            cards.Clear();
            for (int employeeId = 1; employeeId <= MaxEmployees; employeeId++)
            {
                GameObject cardObj = CreateUIObject("Employee_" + employeeId, listContent.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
                RectTransform cardRT = cardObj.GetComponent<RectTransform>();
                cardRT.sizeDelta = new Vector2(0f, 64f);
                LayoutElement le = cardObj.AddComponent<LayoutElement>();
                le.preferredHeight = 64f;

                Image bg = cardObj.AddComponent<Image>();
                Button button = cardObj.AddComponent<Button>();
                button.targetGraphic = bg;

                TMP_Text title = CreateText("Title", cardObj.transform, "Empleado #" + employeeId, 18, TextAlignmentOptions.Left,
                    new Vector2(0f, 0f), new Vector2(0.58f, 1f), new Vector2(14f, 4f), new Vector2(-6f, -4f));
                TMP_Text state = CreateText("State", cardObj.transform, "", 15, TextAlignmentOptions.Right,
                    new Vector2(0.58f, 0f), new Vector2(1f, 1f), new Vector2(4f, 4f), new Vector2(-12f, -4f));

                int capturedId = employeeId;
                button.onClick.AddListener(() => SelectEmployee(capturedId));

                cards.Add(employeeId, new EmployeeCardUI
                {
                    button = button,
                    background = bg,
                    title = title,
                    state = state
                });
            }
        }


        private static GameObject CreateUIObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            return go;
        }


        private static TMP_Text CreateText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
        {
            return CreateText(name, parent, text, fontSize, alignment, Vector2.zero, Vector2.zero);
        }


        private static TMP_Text CreateText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment, Vector2 offsetMin, Vector2 offsetMax)
        {
            return CreateText(name, parent, text, fontSize, alignment, Vector2.zero, Vector2.one, offsetMin, offsetMax);
        }


        private static TMP_Text CreateText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.text      = text;
            label.fontSize  = fontSize;
            label.color     = Color.white;
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode     = TextOverflowModes.Ellipsis;
            return label;
        }


        private static Button CreateActionButton(Transform parent, string name, string label, Color bgColor)
        {
            GameObject go = CreateUIObject(name, parent, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, 48f);
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 48f;
            Image bg = go.AddComponent<Image>();
            bg.color = bgColor;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = bg;
            CreateText("Text", go.transform, label, 18, TextAlignmentOptions.Center);
            return button;
        }


        private struct EmployeeStatus
        {
            public bool isUnlocked;
            public bool isHired;
            public bool canHire;
            public EmployeeRole role;
            public long hireCost;
            public string stateLabel;
            public string hint;
        }


        private class EmployeeCardUI
        {
            public Button button;
            public Image background;
            public TMP_Text title;
            public TMP_Text state;

            public void Refresh(EmployeeStatus status)
            {
                if (state != null)
                    state.text = status.stateLabel;

                if (background != null)
                {
                    if (!status.isUnlocked)
                        background.color = new Color(0.23f, 0.23f, 0.25f, 0.95f);
                    else if (!status.isHired)
                        background.color = new Color(0.17f, 0.30f, 0.55f, 0.95f);
                    else
                        background.color = new Color(0.17f, 0.50f, 0.28f, 0.95f);
                }
            }

            public void SetSelected(bool selected)
            {
                if (button == null || background == null)
                    return;

                Color c = background.color;
                c.a = selected ? 1f : 0.90f;
                background.color = c;
            }
        }


        void OnDestroy()
        {
            if (!listenersBound)
                return;

            EntrepreneurTreeManager.onNodeUnlocked -= OnTreeNodeUnlocked;
            EntrepreneurEmployeeSystem.onEmployeeHired -= OnEmployeeHired;
            EntrepreneurEmployeeSystem.onEmployeeRoleChanged -= OnEmployeeRoleChanged;
            SaveGameSystem.dataLoadEvent -= OnDataLoaded;

            listenersBound = false;
            Debug.Log(LogPrefix + "Employee app listeners removed.");
        }
    }
}
