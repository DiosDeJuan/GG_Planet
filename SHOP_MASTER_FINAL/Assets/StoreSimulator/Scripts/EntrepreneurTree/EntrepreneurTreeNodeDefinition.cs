//Adaptado por POMPIC 20100333
using System.Collections.Generic;
using System.Linq;

namespace FLOBUK.StoreSimulator
{
    public enum EntrepreneurTreeNodeType
    {
        Product,
        Employee,
        Security,
        Upgrade
    }

    public enum EntrepreneurTreeNodeState
    {
        Unlocked,
        Available,
        Locked
    }

    public sealed class EntrepreneurTreeNodeDefinition
    {
        public string Id { get; }
        public string Title { get; }
        public EntrepreneurTreeNodeType Type { get; }
        public int Cost { get; }
        public string Benefit { get; }
        public string[] Prerequisites { get; }

        public EntrepreneurTreeNodeDefinition(string id, string title, EntrepreneurTreeNodeType type, int cost, string benefit, params string[] prerequisites)
        {
            Id = id;
            Title = title;
            Type = type;
            Cost = cost;
            Benefit = benefit;
            Prerequisites = prerequisites ?? new string[0];
        }
    }

    public static class EntrepreneurTreeDefinitions
    {
        public const string DefaultUnlockedNodeId = "productos_basicos_1";

        private static readonly List<EntrepreneurTreeNodeDefinition> nodes = new List<EntrepreneurTreeNodeDefinition>
        {
            new EntrepreneurTreeNodeDefinition(DefaultUnlockedNodeId, "Productos Basicos 1", EntrepreneurTreeNodeType.Product, 0, "Leche, sal, agua, pasta y azucar disponibles desde el inicio."),
            new EntrepreneurTreeNodeDefinition("productos_basicos_2", "Productos Basicos 2", EntrepreneurTreeNodeType.Product, 1, "Harina, arroz, frijoles, pan y aceite.", DefaultUnlockedNodeId),
            new EntrepreneurTreeNodeDefinition("productos_basicos_3", "Productos Basicos 3", EntrepreneurTreeNodeType.Product, 1, "Cafe y huevo.", "productos_basicos_2"),
            new EntrepreneurTreeNodeDefinition("lacteos_1", "Lacteos 1", EntrepreneurTreeNodeType.Product, 1, "Rama de lacteos desbloqueada.", "productos_basicos_3"),
            new EntrepreneurTreeNodeDefinition("lacteos_2", "Lacteos 2", EntrepreneurTreeNodeType.Product, 1, "Rama avanzada de lacteos desbloqueada.", "lacteos_1"),
            new EntrepreneurTreeNodeDefinition("lacteos_3", "Lacteos 3", EntrepreneurTreeNodeType.Product, 1, "Mozzarella y parmesano con assets provisionales seguros.", "lacteos_2"),
            new EntrepreneurTreeNodeDefinition("especias_1", "Especias 1", EntrepreneurTreeNodeType.Product, 1, "Rama de especias desbloqueada.", "productos_basicos_3"),
            new EntrepreneurTreeNodeDefinition("productos_frescos_1", "Productos Frescos 1", EntrepreneurTreeNodeType.Product, 1, "Manzana, platano, jitomate y cebolla.", "lacteos_1"),
            new EntrepreneurTreeNodeDefinition("productos_frescos_2", "Productos Frescos 2", EntrepreneurTreeNodeType.Product, 1, "Uvas, zanahorias y ajo.", "productos_frescos_1"),
            new EntrepreneurTreeNodeDefinition("productos_higiene", "Productos de Higiene", EntrepreneurTreeNodeType.Product, 1, "Jabon, papel higienico, detergente y pasta dental.", "especias_1"),
            new EntrepreneurTreeNodeDefinition("sodas", "Sodas", EntrepreneurTreeNodeType.Product, 1, "Bebidas y sodas para ampliar el catalogo.", "productos_higiene"),
            new EntrepreneurTreeNodeDefinition("proteina_1", "Proteina 1", EntrepreneurTreeNodeType.Product, 1, "Productos de proteina preparados para la rama avanzada.", "empleado_5"),
            new EntrepreneurTreeNodeDefinition("productos_lujo_1", "Productos de Lujo 1", EntrepreneurTreeNodeType.Product, 1, "Trufa, chocolate importado y caviar.", "sodas"),
            new EntrepreneurTreeNodeDefinition("electrodomesticos_1", "Electrodomesticos 1", EntrepreneurTreeNodeType.Product, 1, "Electrodomesticos desbloqueables en fases avanzadas.", "productos_lujo_1"),

            new EntrepreneurTreeNodeDefinition("empleado_1", "Empleado 1", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "especias_1"),
            new EntrepreneurTreeNodeDefinition("empleado_2", "Empleado 2", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "productos_higiene"),
            new EntrepreneurTreeNodeDefinition("empleado_3", "Empleado 3", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "sodas"),
            new EntrepreneurTreeNodeDefinition("empleado_4", "Empleado 4", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "lacteos_1"),
            new EntrepreneurTreeNodeDefinition("empleado_5", "Empleado 5", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "lacteos_1"),
            new EntrepreneurTreeNodeDefinition("empleado_6", "Empleado 6", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "especias_1"),
            new EntrepreneurTreeNodeDefinition("empleado_7", "Empleado 7", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "empleado_5"),
            new EntrepreneurTreeNodeDefinition("empleado_8", "Empleado 8", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "sodas"),
            new EntrepreneurTreeNodeDefinition("empleado_9", "Empleado 9", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "productos_higiene"),
            new EntrepreneurTreeNodeDefinition("empleado_10", "Empleado 10", EntrepreneurTreeNodeType.Employee, 2, "Empleado preparado para contratarse desde la app de empleados.", "empleado_1"),
            new EntrepreneurTreeNodeDefinition("empleado_11", "Empleado 11", EntrepreneurTreeNodeType.Employee, 2, "Empleado preparado para contratarse desde la app de empleados.", "seguridad_1"),
            new EntrepreneurTreeNodeDefinition("empleado_12", "Empleado 12", EntrepreneurTreeNodeType.Employee, 2, "Empleado preparado para contratarse desde la app de empleados.", "empleado_13"),
            new EntrepreneurTreeNodeDefinition("empleado_13", "Empleado 13", EntrepreneurTreeNodeType.Employee, 2, "Empleado preparado para contratarse desde la app de empleados.", "productos_lujo_1"),
            new EntrepreneurTreeNodeDefinition("empleado_14", "Empleado 14", EntrepreneurTreeNodeType.Employee, 2, "Empleado preparado para contratarse desde la app de empleados.", "electrodomesticos_1"),
            new EntrepreneurTreeNodeDefinition("empleado_15", "Empleado 15", EntrepreneurTreeNodeType.Employee, 2, "Empleado preparado para contratarse desde la app de empleados.", "seguridad_2"),
            new EntrepreneurTreeNodeDefinition("empleado_16", "Empleado 16", EntrepreneurTreeNodeType.Employee, 2, "Empleado preparado para contratarse desde la app de empleados.", "proteina_1"),
            new EntrepreneurTreeNodeDefinition("empleado_17", "Empleado 17", EntrepreneurTreeNodeType.Employee, 2, "Empleado preparado para contratarse desde la app de empleados.", "productos_frescos_2"),
            new EntrepreneurTreeNodeDefinition("empleado_18", "Empleado 18", EntrepreneurTreeNodeType.Employee, 2, "Empleado preparado para contratarse desde la app de empleados.", "seguridad_3"),

            new EntrepreneurTreeNodeDefinition("seguridad_1", "Seguridad Nivel 1", EntrepreneurTreeNodeType.Security, 2, "Camaras con 33% de probabilidad de arresto automatico.", "empleado_7"),
            new EntrepreneurTreeNodeDefinition("seguridad_2", "Seguridad Nivel 2", EntrepreneurTreeNodeType.Security, 3, "Guardias con 66% de probabilidad de arresto automatico.", "empleado_8"),
            new EntrepreneurTreeNodeDefinition("seguridad_3", "Seguridad Nivel 3", EntrepreneurTreeNodeType.Security, 3, "Alarmas con 99% de probabilidad de arresto automatico.", "empleado_14"),

            new EntrepreneurTreeNodeDefinition("mejora_cafeina", "Cafeina", EntrepreneurTreeNodeType.Upgrade, 3, "Empleados trabajan 10% mas rapido.", "productos_frescos_2"),
            new EntrepreneurTreeNodeDefinition("mejora_carismatico", "Carismatico", EntrepreneurTreeNodeType.Upgrade, 3, "Cajeros generan 5% mas por venta.", "empleado_15"),
        };

        private static readonly Dictionary<string, EntrepreneurTreeNodeDefinition> nodesById = nodes.ToDictionary(node => node.Id);

        public static IReadOnlyList<EntrepreneurTreeNodeDefinition> Nodes => nodes;

        public static EntrepreneurTreeNodeDefinition Get(string id)
        {
            nodesById.TryGetValue(id, out EntrepreneurTreeNodeDefinition node);
            return node;
        }

        public static string GetTitle(string id)
        {
            EntrepreneurTreeNodeDefinition node = Get(id);
            return node != null ? node.Title : id;
        }

        public static bool TryGetProductNode(ProductScriptableObject product, out EntrepreneurTreeNodeDefinition node)
        {
            string nodeId = GetKnownProductNodeId(product);
            node = string.IsNullOrEmpty(nodeId) ? null : Get(nodeId);
            return node != null;
        }

        public static string GetKnownProductNodeId(ProductScriptableObject product)
        {
            if (product == null)
                return string.Empty;

            string nodeId = GetKnownProductNodeId(product.id);
            return string.IsNullOrEmpty(nodeId) ? GetKnownProductNodeIdByName(product.title) : nodeId;
        }

        public static string GetKnownProductNodeId(string productId)
        {
            switch (DocumentedProductCatalog.GetCanonicalProductId(productId))
            {
                case "leche":
                case "sal":
                case "agua":
                case "pasta":
                case "azucar":
                    return DefaultUnlockedNodeId;
                case "harina":
                case "arroz":
                case "frijoles":
                case "pan":
                case "aceite":
                    return "productos_basicos_2";
                case "cafe":
                case "huevo":
                    return "productos_basicos_3";
                case "cheddar":
                case "yogurt_natural":
                case "mantequilla":
                    return "lacteos_1";
                case "queso_americano":
                case "queso_crema":
                    return "lacteos_2";
                case "mozzarella":
                case "parmesano":
                    return "lacteos_3";
                case "pimienta_negra":
                case "canela":
                    return "especias_1";
                case "manzana":
                case "platano":
                case "jitomate":
                case "cebolla":
                    return "productos_frescos_1";
                case "uvas":
                case "zanahorias":
                case "ajo":
                    return "productos_frescos_2";
                case "jabon":
                case "papel_higienico":
                case "detergente":
                case "pasta_dientes":
                    return "productos_higiene";
                case "res":
                case "pollo":
                case "cerdo":
                case "pescado":
                    return "proteina_1";
                case "cola":
                case "cola_sin_azucar":
                case "refresco_limon":
                    return "sodas";
                case "trufa":
                case "chocolate_importado":
                case "caviar":
                    return "productos_lujo_1";
                case "refrigerador":
                case "microondas":
                case "horno":
                case "mesa":
                case "licuadora":
                    return "electrodomesticos_1";
                default:
                    return string.Empty;
            }
        }

        private static string GetKnownProductNodeIdByName(string productTitle)
        {
            switch (NormalizeProductKey(productTitle))
            {
                case "product_a":
                case "leche":
                case "milk":
                case "product_b":
                case "sal":
                case "salt":
                case "product_c":
                case "agua":
                case "water":
                case "product_d":
                case "pasta":
                case "product_e":
                case "azucar":
                case "sugar":
                    return DefaultUnlockedNodeId;

                case "harina":
                case "arroz":
                case "frijoles":
                case "pan":
                case "aceite":
                    return "productos_basicos_2";

                case "cafe":
                case "huevo":
                    return "productos_basicos_3";

                case "lacteos":
                case "lacteos_1":
                case "queso":
                case "yogurt":
                case "mantequilla":
                    return "lacteos_1";

                case "lacteos_2":
                case "queso_americano":
                case "crema":
                case "queso_crema":
                case "helado":
                case "leche_saborizada":
                    return "lacteos_2";

                case "lacteos_3":
                case "mozzarella":
                case "parmesano":
                    return "lacteos_3";

                case "especias":
                case "pimienta":
                case "pimienta_negra":
                case "oregano":
                case "canela":
                    return "especias_1";

                case "manzana":
                case "platano":
                case "jitomate":
                case "cebolla":
                    return "productos_frescos_1";

                case "uvas":
                case "zanahoria":
                case "zanahorias":
                case "ajo":
                    return "productos_frescos_2";

                case "jabon":
                case "papel_higienico":
                case "detergente":
                case "pasta_dental":
                case "pasta_de_dientes":
                    return "productos_higiene";

                case "soda":
                case "sodas":
                case "refresco":
                case "cola":
                case "cola_sin_azucar":
                case "refresco_de_limon":
                case "refresco_limon":
                    return "sodas";

                case "proteina":
                case "proteina_1":
                case "res":
                case "pollo":
                case "cerdo":
                case "pescado":
                    return "proteina_1";

                case "trufa":
                case "chocolate_importado":
                case "caviar":
                    return "productos_lujo_1";

                case "electrodomestico":
                case "electrodomesticos":
                case "refrigerador":
                case "microondas":
                case "horno":
                case "mesa":
                case "licuadora":
                    return "electrodomesticos_1";

                default:
                    return string.Empty;
            }
        }

        private static string NormalizeProductKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value.Trim()
                .ToLowerInvariant()
                .Replace(" ", "_")
                .Replace("-", "_");
        }
    }
}
