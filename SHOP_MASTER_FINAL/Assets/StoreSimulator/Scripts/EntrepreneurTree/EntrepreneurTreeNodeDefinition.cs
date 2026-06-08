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
            new EntrepreneurTreeNodeDefinition("empleado_10", "Empleado 10", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "empleado_1"),
            new EntrepreneurTreeNodeDefinition("empleado_11", "Empleado 11", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "seguridad_1"),
            new EntrepreneurTreeNodeDefinition("empleado_12", "Empleado 12", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "empleado_13"),
            new EntrepreneurTreeNodeDefinition("empleado_13", "Empleado 13", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "productos_lujo_1"),
            new EntrepreneurTreeNodeDefinition("empleado_14", "Empleado 14", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "electrodomesticos_1"),
            new EntrepreneurTreeNodeDefinition("empleado_15", "Empleado 15", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "seguridad_2"),
            new EntrepreneurTreeNodeDefinition("empleado_16", "Empleado 16", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "proteina_1"),
            new EntrepreneurTreeNodeDefinition("empleado_17", "Empleado 17", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "productos_frescos_2"),
            new EntrepreneurTreeNodeDefinition("empleado_18", "Empleado 18", EntrepreneurTreeNodeType.Employee, 1, "Empleado preparado para contratarse desde la app de empleados.", "seguridad_3"),

            new EntrepreneurTreeNodeDefinition("seguridad_1", "Seguridad Nivel 1", EntrepreneurTreeNodeType.Security, 1, "Camaras con 33% de probabilidad de arresto automatico.", "empleado_7"),
            new EntrepreneurTreeNodeDefinition("seguridad_2", "Seguridad Nivel 2", EntrepreneurTreeNodeType.Security, 1, "Guardias con 66% de probabilidad de arresto automatico.", "empleado_8"),
            new EntrepreneurTreeNodeDefinition("seguridad_3", "Seguridad Nivel 3", EntrepreneurTreeNodeType.Security, 1, "Alarmas con 99% de probabilidad de arresto automatico.", "empleado_14"),

            new EntrepreneurTreeNodeDefinition("mejora_cafeina", "Cafeina", EntrepreneurTreeNodeType.Upgrade, 1, "Empleados trabajan 10% mas rapido.", "productos_frescos_2"),
            new EntrepreneurTreeNodeDefinition("mejora_carismatico", "Carismatico", EntrepreneurTreeNodeType.Upgrade, 1, "Cajeros generan 5% mas por venta.", "empleado_15"),
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

        public static string GetKnownProductNodeId(string productId)
        {
            switch (productId)
            {
                case "0":
                case "1":
                case "2":
                case "3":
                    return DefaultUnlockedNodeId;
                case "4":
                    return "productos_basicos_2";
                default:
                    return string.Empty;
            }
        }
    }
}
