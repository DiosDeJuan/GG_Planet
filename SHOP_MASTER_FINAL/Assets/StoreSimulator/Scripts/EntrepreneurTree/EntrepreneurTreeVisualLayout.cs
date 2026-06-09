//Adaptado por POMPIC 20100333
using System.Collections.Generic;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    public static class EntrepreneurTreeVisualLayout
    {
        public static readonly Vector2 ContentSize = new Vector2(3200f, 1300f);
        public static readonly Vector2 NodeSize = new Vector2(178f, 68f);

        private const float LeftMargin = 130f;
        private const float TopMargin = 115f;
        private const float MaxRawY = 420f;

        private static readonly Dictionary<string, Vector2> positions = new Dictionary<string, Vector2>
        {
            { "productos_basicos_1", new Vector2(0f, 0f) },
            { "productos_basicos_2", new Vector2(240f, 0f) },
            { "productos_basicos_3", new Vector2(480f, 0f) },

            { "lacteos_1", new Vector2(720f, 160f) },
            { "lacteos_2", new Vector2(1200f, 160f) },
            { "productos_frescos_1", new Vector2(960f, 300f) },
            { "productos_frescos_2", new Vector2(1200f, 300f) },
            { "mejora_cafeina", new Vector2(1440f, 420f) },
            { "empleado_17", new Vector2(1440f, 300f) },
            { "empleado_4", new Vector2(960f, 160f) },
            { "empleado_5", new Vector2(960f, 20f) },
            { "empleado_7", new Vector2(1200f, 20f) },
            { "seguridad_1", new Vector2(1440f, 20f) },
            { "empleado_11", new Vector2(1680f, 20f) },
            { "proteina_1", new Vector2(1200f, -120f) },
            { "empleado_16", new Vector2(1440f, -120f) },

            { "especias_1", new Vector2(720f, -220f) },
            { "empleado_1", new Vector2(960f, -340f) },
            { "empleado_10", new Vector2(1200f, -340f) },
            { "empleado_6", new Vector2(960f, -220f) },
            { "productos_higiene", new Vector2(1200f, -220f) },
            { "empleado_9", new Vector2(1440f, -220f) },
            { "empleado_2", new Vector2(1200f, -460f) },
            { "sodas", new Vector2(1440f, -460f) },
            { "empleado_3", new Vector2(1680f, -460f) },
            { "empleado_8", new Vector2(1440f, -620f) },
            { "seguridad_2", new Vector2(1680f, -620f) },
            { "empleado_15", new Vector2(1920f, -620f) },
            { "mejora_carismatico", new Vector2(2160f, -620f) },

            { "productos_lujo_1", new Vector2(1920f, -460f) },
            { "empleado_13", new Vector2(2160f, -460f) },
            { "empleado_12", new Vector2(2400f, -460f) },
            { "electrodomesticos_1", new Vector2(2160f, -300f) },
            { "empleado_14", new Vector2(2400f, -300f) },
            { "seguridad_3", new Vector2(2640f, -300f) },
            { "empleado_18", new Vector2(2880f, -300f) },
        };

        public static Vector2 GetAnchoredPosition(string nodeId, int fallbackIndex, out bool usedFallback)
        {
            if (positions.TryGetValue(nodeId, out Vector2 rawPosition))
            {
                usedFallback = false;
                return ConvertRawPosition(rawPosition);
            }

            usedFallback = true;
            int column = fallbackIndex % 4;
            int row = fallbackIndex / 4;
            return new Vector2(LeftMargin + 2520f + column * 220f, -(TopMargin + row * 110f));
        }

        private static Vector2 ConvertRawPosition(Vector2 rawPosition)
        {
            return new Vector2(LeftMargin + rawPosition.x, -(TopMargin + (MaxRawY - rawPosition.y)));
        }
    }
}
