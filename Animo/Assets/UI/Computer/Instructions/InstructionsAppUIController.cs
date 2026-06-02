using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// In-game quick guide for ShopMaster systems added around the Entrepreneur Tree.
    /// </summary>
    public class InstructionsAppUIController : MonoBehaviour
    {
        public const string GuideText =
            "GUIA RAPIDA SHOPMASTER\n\n" +
            "Computadora: acercate a la laptop y usa clic izquierdo. Usa ESC para salir.\n\n" +
            "Arbol del Emprendedor: abre EXPANSIONS y pulsa Abrir Arbol. Gana puntos con logros y deteniendo ladrones. " +
            "Cada nodo muestra costo y requisitos; desbloquea primero los nodos anteriores.\n\n" +
            "Productos y pedidos: abre COMPRA. Solo puedes comprar productos desbloqueados. " +
            "Cada pedido descuenta dinero y llega como paquete para surtir la tienda.\n\n" +
            "Empleados: abre EMPLEADOS. Desbloquea empleados en el Arbol, contratalos y asigna rol Cajero o Surtidor. " +
            "Los cajeros atienden cajas disponibles. Los surtidores reponen espacios asignados cuando existe stock.\n\n" +
            "Inventario y expansion: usa INVENTARIO para revisar stock y EXPANDIR para aumentar el supermercado.\n\n" +
            "Seguridad y ladrones: desbloquea Camaras, Guardias y Alarmas en el Arbol. " +
            "Si un ladron escapa, pierdes el valor real robado. Acercate y usa E para detenerlo manualmente.\n\n" +
            "Guardado: la partida se guarda al cerrar el dia. Las notificaciones indican bloqueos, fondos insuficientes, " +
            "pedidos, contrataciones, seguridad y robos.";

        private bool built;

        void OnEnable()
        {
            if (!built)
                BuildUI();
        }

        private void BuildUI()
        {
            built = true;
            Image bg = gameObject.GetComponent<Image>();
            if (bg == null)
                bg = gameObject.AddComponent<Image>();
            bg.color = ComputerUITheme.RootBg;

            GameObject header = CreateUIObject("Header", transform);
            SetStretch(header.GetComponent<RectTransform>(), 0f, 0.88f, 1f, 1f);
            header.AddComponent<Image>().color = ComputerUITheme.HeaderBg;
            TMP_Text title = CreateText("Title", header.transform, "INSTRUCCIONES", 24);
            title.fontStyle = FontStyles.Bold;
            title.color = ComputerUITheme.TextPrimary;

            GameObject scrollObject = CreateUIObject("Scroll", transform);
            SetStretch(scrollObject.GetComponent<RectTransform>(), 0f, 0f, 1f, 0.88f);
            ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;

            GameObject viewport = CreateUIObject("Viewport", scrollObject.transform);
            SetStretch(viewport.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f);
            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            GameObject content = CreateUIObject("Content", viewport.transform);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0f, 900f);

            TMP_Text body = CreateText("GuideText", content.transform, GuideText, 18);
            body.alignment = TextAlignmentOptions.TopLeft;
            body.textWrappingMode = TextWrappingModes.Normal;
            body.color = ComputerUITheme.TextSecondary;
            RectTransform bodyRect = body.GetComponent<RectTransform>();
            bodyRect.offsetMin = new Vector2(24f, 20f);
            bodyRect.offsetMax = new Vector2(-24f, -20f);

            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRect;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static TMP_Text CreateText(string name, Transform parent, string value, float size)
        {
            GameObject go = CreateUIObject(name, parent);
            SetStretch(go.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f);
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            return text;
        }

        private static void SetStretch(RectTransform rt, float minX, float minY, float maxX, float maxY)
        {
            rt.anchorMin = new Vector2(minX, minY);
            rt.anchorMax = new Vector2(maxX, maxY);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
