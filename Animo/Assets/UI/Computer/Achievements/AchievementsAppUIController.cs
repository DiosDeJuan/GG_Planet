// AchievementsAppUIController
// Runtime achievements panel integrated in the computer UPGRADES/Expansions area.
// Mirrors the Employees and Expansion app pattern from EntrepreneurTreeUIBootstrap.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Builds and manages the Logros (Achievements) panel inside the in-game computer.
    /// Mirrors EmployeeAppUIController: no external prefab required, pure code UI.
    /// </summary>
    [DisallowMultipleComponent]
    public class AchievementsAppUIController : MonoBehaviour
    {
        private const string LogPrefix = "[Achievements] ";

        // ── Static achievement metadata ────────────────────────────────────────

        private struct AchievementMeta
        {
            public string displayName;
            public string description;
            public string category;

            public AchievementMeta(string displayName, string description, string category)
            {
                this.displayName = displayName;
                this.description = description;
                this.category    = category;
            }
        }

        private static readonly Dictionary<AchievementId, AchievementMeta> Metadata =
            new Dictionary<AchievementId, AchievementMeta>
        {
            // Ventas / Revenue
            { AchievementId.FirstSale,           new AchievementMeta("Primera Venta",         "Completa tu primera venta con un cliente feliz.",       "Ventas") },
            { AchievementId.Revenue1000,          new AchievementMeta("Objetivo 1",            "Gana $1,000 en total.",                                "Ventas") },
            { AchievementId.Revenue5000,          new AchievementMeta("Objetivo 2",            "Gana $5,000 en total.",                                "Ventas") },
            { AchievementId.Revenue10000,         new AchievementMeta("Objetivo 3",            "Gana $10,000 en total.",                               "Ventas") },
            { AchievementId.Revenue12000,         new AchievementMeta("Objetivo 4",            "Gana $12,000 en total.",                               "Ventas") },
            { AchievementId.Revenue15000,         new AchievementMeta("Objetivo 5",            "Gana $15,000 en total.",                               "Ventas") },
            { AchievementId.Revenue17000,         new AchievementMeta("Objetivo 6",            "Gana $17,000 en total.",                               "Ventas") },
            { AchievementId.Revenue20000,         new AchievementMeta("Objetivo 7",            "Gana $20,000 en total.",                               "Ventas") },
            { AchievementId.Revenue25000,         new AchievementMeta("Objetivo 8",            "Gana $25,000 en total.",                               "Ventas") },
            { AchievementId.Revenue35000,         new AchievementMeta("Objetivo 9",            "Gana $35,000 en total.",                               "Ventas") },
            { AchievementId.Revenue50000,         new AchievementMeta("Objetivo 10",           "Gana $50,000 en total.",                               "Ventas") },
            { AchievementId.Revenue100000,        new AchievementMeta("Lluvia de Dinero",      "Gana $100,000 en total.",                              "Ventas") },
            { AchievementId.DailyRevenue1000,     new AchievementMeta("Ventas Diarias 1",      "Gana $1,000 en un solo día.",                          "Ventas") },
            { AchievementId.DailyRevenue1500,     new AchievementMeta("Ventas Diarias 2",      "Gana $1,500 en un solo día.",                          "Ventas") },
            { AchievementId.DailyRevenue5000,     new AchievementMeta("Ventas Diarias 3",      "Gana $5,000 en un solo día.",                          "Ventas") },
            { AchievementId.DailyRevenue10000,    new AchievementMeta("Ventas Diarias 4",      "Gana $10,000 en un solo día.",                         "Ventas") },
            { AchievementId.DailyRevenue15000,    new AchievementMeta("Ventas Diarias 5",      "Gana $15,000 en un solo día.",                         "Ventas") },
            { AchievementId.DailyRevenue20000,    new AchievementMeta("Ventas Diarias 6",      "Gana $20,000 en un solo día.",                         "Ventas") },
            { AchievementId.GoldenEgg,            new AchievementMeta("Huevo Dorado",          "Gana $50,000 en un solo día (secreto).",               "Ventas") },
            // Días
            { AchievementId.Play3Days,            new AchievementMeta("Dedicado",              "Sobrevive 3 días de juego.",                           "Días") },
            { AchievementId.Play5Days,            new AchievementMeta("Fiel",                  "Sobrevive 5 días de juego.",                           "Días") },
            { AchievementId.Play7Days,            new AchievementMeta("Emprendedor",           "Sobrevive 7 días de juego.",                           "Días") },
            { AchievementId.Play30Days,           new AchievementMeta("Veterano",              "Sobrevive 30 días de juego.",                          "Días") },
            // Empleados
            { AchievementId.HireFirstEmployee,    new AchievementMeta("Primer Empleado",       "Desbloquea el primer nodo de empleado en el árbol.",  "Empleados") },
            { AchievementId.Hire5Employees,       new AchievementMeta("Equipo Pequeño",        "Desbloquea 5 empleados en el árbol.",                  "Empleados") },
            { AchievementId.Hire10Employees,      new AchievementMeta("Equipo Mediano",        "Desbloquea 10 empleados en el árbol.",                 "Empleados") },
            { AchievementId.FirstEmployeeHiredReal, new AchievementMeta("Contratación Real",  "Contrata tu primer empleado desde la app.",            "Empleados") },
            { AchievementId.MaxEmployment,        new AchievementMeta("Máximo Empleo",         "Contrata y asigna los 18 empleados.",                  "Empleados") },
            // Expansión
            { AchievementId.ExpandStore,          new AchievementMeta("Primera Expansión",    "Compra cualquier zona de expansión.",                  "Expansión") },
            { AchievementId.MaxSupermarket,       new AchievementMeta("Imperialista",          "Alcanza el tamaño máximo del supermercado.",           "Expansión") },
            { AchievementId.MaxStorage,           new AchievementMeta("Almacenamiento Máx.",   "Maximiza todas las zonas de almacenamiento.",          "Expansión") },
            // Árbol
            { AchievementId.UnlockAllProducts,    new AchievementMeta("Surtido Completo",     "Desbloquea todos los productos básicos del árbol.",    "Árbol") },
            { AchievementId.TotalOptimization,    new AchievementMeta("Optimista",             "Desbloquea todas las mejoras del árbol.",              "Árbol") },
            { AchievementId.UnlockAllTree,        new AchievementMeta("Árbol Completo",        "Desbloquea todos los nodos del árbol.",                "Árbol") },
            // Seguridad
            { AchievementId.Batman,               new AchievementMeta("Batman",                "Arresta a tu primer ladrón.",                          "Seguridad") },
            { AchievementId.RedSeguridad,         new AchievementMeta("Red de Seguridad",      "Desbloquea los 3 niveles de seguridad.",               "Seguridad") },
            // Precios / servicio
            { AchievementId.Donador,              new AchievementMeta("Donador",               "Vende 5 productos a $0.00.",                           "Precios") },
            { AchievementId.LuxuryProductSold,    new AchievementMeta("Cliente de Lujo",       "Vende un producto de lujo.",                           "Precios") },
            { AchievementId.ApplianceProductSold, new AchievementMeta("Lindo Hogar",           "Vende un electrodoméstico.",                           "Precios") },
            { AchievementId.Paciente,             new AchievementMeta("Paciente",              "Un cliente se quejó del precio.",                     "Precios") },
            { AchievementId.PrecioPerfecto,       new AchievementMeta("Precio Perfecto",       "7 días consecutivos sin quejas de precio.",           "Precios") },
            // Inventario
            { AchievementId.FullStockDay,         new AchievementMeta("Stock Completo",        "Todos los productos en anaqueles a la vez.",          "Inventario") },
            { AchievementId.WrongPlacement,       new AchievementMeta("Colocación Incorrecta", "Colocaste un producto en un mueble incompatible.",    "Inventario") },
            // Secreto
            { AchievementId.HuevoDorado,          new AchievementMeta("Huevo Dorado (Secreto)","???",                                                 "Secreto") },
        };

        // ── Runtime fields ─────────────────────────────────────────────────────

        private bool appBuilt;
        private TMP_Text summaryLabel;
        private TMP_Text pointsLabel;
        private readonly List<AchievementRowUI> rows = new List<AchievementRowUI>();

        // ── Unity lifecycle ────────────────────────────────────────────────────

        void Awake()
        {
            BuildUI();
            AchievementSystem.onAchievementCompleted += OnAchievementCompleted;
            SaveGameSystem.dataLoadEvent += OnDataLoaded;
        }


        void OnEnable()
        {
            RefreshAll();
        }


        void OnDestroy()
        {
            AchievementSystem.onAchievementCompleted -= OnAchievementCompleted;
            SaveGameSystem.dataLoadEvent -= OnDataLoaded;
        }


        // ── Refresh ────────────────────────────────────────────────────────────

        private void OnAchievementCompleted(AchievementId id)
        {
            RefreshAll();
        }


        private void OnDataLoaded()
        {
            RefreshAll();
        }


        private void RefreshAll()
        {
            if (!appBuilt) return;

            int completed = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                bool done = AchievementSystem.IsCompleted(rows[i].id);
                rows[i].SetCompleted(done);
                if (done) completed++;
            }

            if (summaryLabel != null)
                summaryLabel.text = "Completados: " + completed + " / " + rows.Count;

            if (pointsLabel != null)
            {
                int pts = EntrepreneurTreeManager.GetAvailablePoints();
                pointsLabel.text = "Puntos del Árbol disponibles: " + pts;
            }
        }


        // ── UI construction ────────────────────────────────────────────────────

        private void BuildUI()
        {
            if (appBuilt) return;
            appBuilt = true;

            RectTransform rootRT = GetComponent<RectTransform>();
            if (rootRT == null) return;

            Image bg = GetComponent<Image>();
            if (bg == null) bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0.07f, 0.08f, 0.11f, 0.97f);

            // ── Header ──────────────────────────────────────────────────────────
            GameObject header = CreateChild("Header", gameObject);
            {
                RectTransform rt = header.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0.88f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.offsetMin = new Vector2(10f, 0f);
                rt.offsetMax = new Vector2(-10f, 0f);
            }

            TMP_Text title = AddText(header, "Title", "LOGROS", 22, FontStyles.Bold, new Color(1f, 0.85f, 0.2f));
            SetAnchors(title.rectTransform, 0f, 0.55f, 1f, 1f, 0f, 0f, 0f, 0f);
            title.alignment = TextAlignmentOptions.Left;

            summaryLabel = AddText(header, "Summary", "Completados: 0 / 0", 13, FontStyles.Normal, new Color(0.7f, 0.7f, 0.7f));
            SetAnchors(summaryLabel.rectTransform, 0f, 0.25f, 0.55f, 0.55f, 0f, 0f, 0f, 0f);
            summaryLabel.alignment = TextAlignmentOptions.Left;

            pointsLabel = AddText(header, "Points", "Puntos disponibles: 0", 13, FontStyles.Normal, new Color(0.55f, 0.85f, 0.55f));
            SetAnchors(pointsLabel.rectTransform, 0.55f, 0.25f, 1f, 0.55f, 0f, 0f, 0f, 0f);
            pointsLabel.alignment = TextAlignmentOptions.Right;

            // ── Scroll view ──────────────────────────────────────────────────────
            GameObject scrollGO = CreateChild("ScrollView", gameObject);
            ScrollRect scrollRect = scrollGO.AddComponent<ScrollRect>();
            {
                RectTransform rt = scrollGO.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(1f, 0.87f);
                rt.offsetMin = new Vector2(6f, 4f);
                rt.offsetMax = new Vector2(-6f, -2f);
            }

            GameObject viewport = CreateChild("Viewport", scrollGO);
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            viewport.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
            {
                RectTransform rt = viewport.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            GameObject content = CreateChild("Content", viewport);
            {
                RectTransform rt = content.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot     = new Vector2(0.5f, 1f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.padding   = new RectOffset(4, 4, 4, 4);
            vlg.spacing   = 4;
            vlg.childAlignment       = TextAnchor.UpperLeft;
            vlg.childControlWidth    = true;
            vlg.childControlHeight   = false;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport   = viewport.GetComponent<RectTransform>();
            scrollRect.content    = content.GetComponent<RectTransform>();
            scrollRect.horizontal = false;
            scrollRect.vertical   = true;
            scrollRect.scrollSensitivity = 30f;

            // ── Build rows ───────────────────────────────────────────────────────
            foreach (KeyValuePair<AchievementId, AchievementMeta> kv in Metadata)
                rows.Add(CreateRow(kv.Key, kv.Value, content));

            Debug.Log(LogPrefix + "Achievements app built with " + rows.Count + " achievements.");
        }


        private static AchievementRowUI CreateRow(AchievementId id, AchievementMeta meta, GameObject parent)
        {
            GameObject rowGO = CreateChild("Row_" + id, parent);
            LayoutElement le = rowGO.AddComponent<LayoutElement>();
            le.preferredHeight = 48f;
            Image rowBg = rowGO.AddComponent<Image>();
            // Initial color is set by AchievementRowUI.SetCompleted(false) via RefreshAll.

            TMP_Text badge = AddText(rowGO, "Badge", "[" + meta.category + "]", 9, FontStyles.Normal, new Color(0.55f, 0.65f, 0.75f));
            SetAnchors(badge.rectTransform, 0f, 0.6f, 0.2f, 1f, 4f, 0f, 0f, 0f);
            badge.alignment = TextAlignmentOptions.BottomLeft;

            TMP_Text nameLabel = AddText(rowGO, "Name", meta.displayName, 13, FontStyles.Bold, Color.white);
            SetAnchors(nameLabel.rectTransform, 0.2f, 0.5f, 0.78f, 1f, 4f, 0f, 0f, 0f);
            nameLabel.alignment = TextAlignmentOptions.Left;

            TMP_Text reward = AddText(rowGO, "Reward", "+1 pto.", 10, FontStyles.Normal, new Color(0.9f, 0.75f, 0.2f));
            SetAnchors(reward.rectTransform, 0.82f, 0.5f, 1f, 1f, 0f, 0f, -4f, 0f);
            reward.alignment = TextAlignmentOptions.Right;

            TMP_Text desc = AddText(rowGO, "Desc", meta.description, 10, FontStyles.Normal, new Color(0.65f, 0.65f, 0.65f));
            SetAnchors(desc.rectTransform, 0f, 0f, 0.76f, 0.52f, 4f, 2f, 0f, 0f);
            desc.alignment = TextAlignmentOptions.Left;

            TMP_Text status = AddText(rowGO, "Status", "●", 14, FontStyles.Bold, new Color(0.35f, 0.35f, 0.35f));
            SetAnchors(status.rectTransform, 0.76f, 0f, 1f, 0.52f, 0f, 2f, -4f, 0f);
            status.alignment = TextAlignmentOptions.Right;

            return new AchievementRowUI(id, rowBg, nameLabel, status);
        }


        // ── Helpers ───────────────────────────────────────────────────────────

        private static GameObject CreateChild(string name, GameObject parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            return go;
        }


        private static TMP_Text AddText(GameObject parent, string name, string text, int size, FontStyles style, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text         = text;
            tmp.fontSize     = size;
            tmp.fontStyle    = style;
            tmp.color        = color;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return tmp;
        }


        private static void SetAnchors(RectTransform rt,
            float ancMinX, float ancMinY, float ancMaxX, float ancMaxY,
            float offsetMinX, float offsetMinY, float offsetMaxX, float offsetMaxY)
        {
            rt.anchorMin = new Vector2(ancMinX, ancMinY);
            rt.anchorMax = new Vector2(ancMaxX, ancMaxY);
            rt.offsetMin = new Vector2(offsetMinX, offsetMinY);
            rt.offsetMax = new Vector2(offsetMaxX, offsetMaxY);
        }


        // ── Inner row type ────────────────────────────────────────────────────

        private class AchievementRowUI
        {
            public readonly AchievementId id;
            private readonly Image    background;
            private readonly TMP_Text nameLabel;
            private readonly TMP_Text statusDot;

            private static readonly Color ColorDone    = new Color(0.10f, 0.20f, 0.12f, 0.95f);
            private static readonly Color ColorPending = new Color(0.13f, 0.14f, 0.18f, 0.90f);
            private static readonly Color DotDone      = new Color(0.25f, 0.85f, 0.40f);
            private static readonly Color DotPending   = new Color(0.35f, 0.35f, 0.35f);

            public AchievementRowUI(AchievementId id, Image bg, TMP_Text nameLabel, TMP_Text statusDot)
            {
                this.id         = id;
                this.background = bg;
                this.nameLabel  = nameLabel;
                this.statusDot  = statusDot;
            }

            public void SetCompleted(bool done)
            {
                if (background != null) background.color = done ? ColorDone : ColorPending;
                if (nameLabel  != null) nameLabel.color  = done ? new Color(0.85f, 1f, 0.85f) : Color.white;
                if (statusDot  != null) statusDot.color  = done ? DotDone : DotPending;
            }
        }
    }
}
