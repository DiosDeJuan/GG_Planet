//Adaptado por POMPIC 20100333
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FLOBUK.StoreSimulator
{
    public enum EntrepreneurTreeNodeCategory
    {
        Productos,
        Empleados,
        Seguridad,
        Mejoras
    }

    public enum EntrepreneurTreeNodeUnlockType
    {
        ProductGroup,
        Employee,
        SecurityLevel,
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
        public string DisplayName { get; }
        public EntrepreneurTreeNodeCategory Category { get; }
        public string Description { get; }
        public string Benefit { get; }
        public int Cost { get; }
        public string[] RequiredNodeIds { get; }
        public EntrepreneurTreeNodeUnlockType UnlockType { get; }
        public int SortOrder { get; }
        public int VisualColumn { get; }
        public int VisualRow { get; }
        public string[] ProductNames { get; }

        public EntrepreneurTreeNodeDefinition(
            string id,
            string displayName,
            EntrepreneurTreeNodeCategory category,
            string description,
            string benefit,
            int cost,
            EntrepreneurTreeNodeUnlockType unlockType,
            int sortOrder,
            int visualColumn,
            int visualRow,
            string[] requiredNodeIds,
            string[] productNames = null)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            Description = description;
            Benefit = benefit;
            Cost = cost;
            UnlockType = unlockType;
            SortOrder = sortOrder;
            VisualColumn = visualColumn;
            VisualRow = visualRow;
            RequiredNodeIds = requiredNodeIds ?? Array.Empty<string>();
            ProductNames = productNames ?? Array.Empty<string>();
        }
    }

    public static class EntrepreneurTreeDefinitions
    {
        public const string DefaultUnlockedNodeId = "productos_basicos_1";

        private static readonly List<EntrepreneurTreeNodeDefinition> nodes = new List<EntrepreneurTreeNodeDefinition>
        {
            // Productos
            new EntrepreneurTreeNodeDefinition(
                id: DefaultUnlockedNodeId,
                displayName: "Productos Básicos 1",
                category: EntrepreneurTreeNodeCategory.Productos,
                description: "Canasta base inicial del supermercado.",
                benefit: "Leche, Sal, Agua, Pasta y Azúcar disponibles desde el inicio.",
                cost: 0,
                unlockType: EntrepreneurTreeNodeUnlockType.ProductGroup,
                sortOrder: 10,
                visualColumn: 0,
                visualRow: 0,
                requiredNodeIds: Array.Empty<string>(),
                productNames: new[] { "Leche", "Sal", "Agua", "Pasta", "Azúcar" }),
            new EntrepreneurTreeNodeDefinition(
                id: "productos_basicos_2",
                displayName: "Productos Básicos 2",
                category: EntrepreneurTreeNodeCategory.Productos,
                description: "Expande básicos de despensa.",
                benefit: "Harina, Arroz, Frijoles, Pan y Aceite disponibles.",
                cost: 1,
                unlockType: EntrepreneurTreeNodeUnlockType.ProductGroup,
                sortOrder: 20,
                visualColumn: 1,
                visualRow: 1,
                requiredNodeIds: new[] { DefaultUnlockedNodeId },
                productNames: new[] { "Harina", "Arroz", "Frijoles", "Pan", "Aceite" }),
            new EntrepreneurTreeNodeDefinition(
                id: "productos_basicos_3",
                displayName: "Productos Básicos 3",
                category: EntrepreneurTreeNodeCategory.Productos,
                description: "Completa básicos con consumo diario.",
                benefit: "Café y Huevo disponibles.",
                cost: 1,
                unlockType: EntrepreneurTreeNodeUnlockType.ProductGroup,
                sortOrder: 30,
                visualColumn: 2,
                visualRow: 2,
                requiredNodeIds: new[] { "productos_basicos_2" },
                productNames: new[] { "Café", "Huevo" }),
            new EntrepreneurTreeNodeDefinition(
                id: "lacteos_1",
                displayName: "Lácteos 1",
                category: EntrepreneurTreeNodeCategory.Productos,
                description: "Rama de lácteos inicial.",
                benefit: "Cheddar, Yogurt natural y Mantequilla disponibles.",
                cost: 1,
                unlockType: EntrepreneurTreeNodeUnlockType.ProductGroup,
                sortOrder: 40,
                visualColumn: 3,
                visualRow: 1,
                requiredNodeIds: new[] { "productos_basicos_3" },
                productNames: new[] { "Cheddar", "Yogurt natural", "Mantequilla" }),
            new EntrepreneurTreeNodeDefinition(
                id: "especias_1",
                displayName: "Especias 1",
                category: EntrepreneurTreeNodeCategory.Productos,
                description: "Rama de especias y condimentos.",
                benefit: "Pimienta negra y Canela disponibles.",
                cost: 1,
                unlockType: EntrepreneurTreeNodeUnlockType.ProductGroup,
                sortOrder: 50,
                visualColumn: 3,
                visualRow: 3,
                requiredNodeIds: new[] { "productos_basicos_3" },
                productNames: new[] { "Pimienta negra", "Canela" }),
            new EntrepreneurTreeNodeDefinition(
                id: "productos_frescos_1",
                displayName: "Productos Frescos 1",
                category: EntrepreneurTreeNodeCategory.Productos,
                description: "Inicia perecederos frescos.",
                benefit: "Manzana, Plátano, Jitomate y Cebolla disponibles.",
                cost: 1,
                unlockType: EntrepreneurTreeNodeUnlockType.ProductGroup,
                sortOrder: 60,
                visualColumn: 4,
                visualRow: 1,
                requiredNodeIds: new[] { "lacteos_1" },
                productNames: new[] { "Manzana", "Plátano", "Jitomate", "Cebolla" }),
            new EntrepreneurTreeNodeDefinition(
                id: "productos_frescos_2",
                displayName: "Productos Frescos 2",
                category: EntrepreneurTreeNodeCategory.Productos,
                description: "Segunda etapa de frescos.",
                benefit: "Uvas, Zanahorias y Ajo disponibles.",
                cost: 1,
                unlockType: EntrepreneurTreeNodeUnlockType.ProductGroup,
                sortOrder: 70,
                visualColumn: 5,
                visualRow: 1,
                requiredNodeIds: new[] { "productos_frescos_1" },
                productNames: new[] { "Uvas", "Zanahorias", "Ajo" }),
            new EntrepreneurTreeNodeDefinition(
                id: "lacteos_2",
                displayName: "Lácteos 2",
                category: EntrepreneurTreeNodeCategory.Productos,
                description: "Lácteos avanzados.",
                benefit: "Queso americano y Queso crema disponibles.",
                cost: 1,
                unlockType: EntrepreneurTreeNodeUnlockType.ProductGroup,
                sortOrder: 80,
                visualColumn: 4,
                visualRow: 2,
                requiredNodeIds: new[] { "lacteos_1" },
                productNames: new[] { "Queso americano", "Queso crema" }),
            new EntrepreneurTreeNodeDefinition(
                id: "productos_higiene",
                displayName: "Productos de Higiene",
                category: EntrepreneurTreeNodeCategory.Productos,
                description: "Línea de higiene personal y hogar.",
                benefit: "Jabón, Papel higiénico, Detergente y Pasta de dientes disponibles.",
                cost: 1,
                unlockType: EntrepreneurTreeNodeUnlockType.ProductGroup,
                sortOrder: 90,
                visualColumn: 4,
                visualRow: 3,
                requiredNodeIds: new[] { "especias_1" },
                productNames: new[] { "Jabón", "Papel higiénico", "Detergente", "Pasta de dientes" }),
            new EntrepreneurTreeNodeDefinition(
                id: "sodas",
                displayName: "Sodas",
                category: EntrepreneurTreeNodeCategory.Productos,
                description: "Bebidas carbonatadas.",
                benefit: "Cola, Cola sin azúcar y Refresco de limón disponibles.",
                cost: 1,
                unlockType: EntrepreneurTreeNodeUnlockType.ProductGroup,
                sortOrder: 100,
                visualColumn: 5,
                visualRow: 3,
                requiredNodeIds: new[] { "productos_higiene" },
                productNames: new[] { "Cola", "Cola sin azúcar", "Refresco de limón" }),
            new EntrepreneurTreeNodeDefinition(
                id: "proteina_1",
                displayName: "Proteína 1",
                category: EntrepreneurTreeNodeCategory.Productos,
                description: "Proteínas frescas de alta demanda.",
                benefit: "Res, Pollo, Cerdo y Pescado disponibles.",
                cost: 1,
                unlockType: EntrepreneurTreeNodeUnlockType.ProductGroup,
                sortOrder: 110,
                visualColumn: 5,
                visualRow: 2,
                requiredNodeIds: new[] { "lacteos_1" },
                productNames: new[] { "Res", "Pollo", "Cerdo", "Pescado" }),
            new EntrepreneurTreeNodeDefinition(
                id: "productos_lujo_1",
                displayName: "Productos de Lujo 1",
                category: EntrepreneurTreeNodeCategory.Productos,
                description: "Línea premium para tickets altos.",
                benefit: "Trufa, Chocolate importado y Caviar disponibles.",
                cost: 1,
                unlockType: EntrepreneurTreeNodeUnlockType.ProductGroup,
                sortOrder: 120,
                visualColumn: 6,
                visualRow: 3,
                requiredNodeIds: new[] { "sodas" },
                productNames: new[] { "Trufa", "Chocolate importado", "Caviar" }),
            new EntrepreneurTreeNodeDefinition(
                id: "electrodomesticos_1",
                displayName: "Electrodomésticos 1",
                category: EntrepreneurTreeNodeCategory.Productos,
                description: "Electrónica y artículos de valor.",
                benefit: "Refrigerador, Microondas, Horno y Licuadora/Mesa disponibles según catálogo real.",
                cost: 1,
                unlockType: EntrepreneurTreeNodeUnlockType.ProductGroup,
                sortOrder: 130,
                visualColumn: 7,
                visualRow: 3,
                requiredNodeIds: new[] { "productos_lujo_1" },
                productNames: new[] { "Refrigerador", "Microondas", "Horno", "Licuadora", "Mesa" }),

            // Empleados
            new EntrepreneurTreeNodeDefinition("empleado_1", "Empleado 1", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 200, 4, 4, new[] { "especias_1" }),
            new EntrepreneurTreeNodeDefinition("empleado_2", "Empleado 2", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 210, 5, 4, new[] { "productos_higiene" }),
            new EntrepreneurTreeNodeDefinition("empleado_3", "Empleado 3", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 220, 6, 4, new[] { "sodas" }),
            new EntrepreneurTreeNodeDefinition("empleado_4", "Empleado 4", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 230, 4, 5, new[] { "lacteos_1" }),
            new EntrepreneurTreeNodeDefinition("empleado_5", "Empleado 5", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 240, 4, 6, new[] { "lacteos_1" }),
            new EntrepreneurTreeNodeDefinition("empleado_6", "Empleado 6", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 250, 4, 7, new[] { "especias_1" }),
            new EntrepreneurTreeNodeDefinition("empleado_7", "Empleado 7", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 260, 5, 6, new[] { "empleado_5" }),
            new EntrepreneurTreeNodeDefinition("empleado_8", "Empleado 8", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 270, 6, 6, new[] { "sodas" }),
            new EntrepreneurTreeNodeDefinition("empleado_9", "Empleado 9", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 280, 5, 5, new[] { "productos_higiene" }),
            new EntrepreneurTreeNodeDefinition("empleado_10", "Empleado 10", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 290, 5, 7, new[] { "empleado_1" }),
            new EntrepreneurTreeNodeDefinition("empleado_11", "Empleado 11", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 300, 6, 8, new[] { "seguridad_1" }),
            new EntrepreneurTreeNodeDefinition("empleado_12", "Empleado 12", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 310, 7, 7, new[] { "empleado_13" }),
            new EntrepreneurTreeNodeDefinition("empleado_13", "Empleado 13", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 320, 7, 6, new[] { "productos_lujo_1" }),
            new EntrepreneurTreeNodeDefinition("empleado_14", "Empleado 14", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 330, 8, 6, new[] { "electrodomesticos_1" }),
            new EntrepreneurTreeNodeDefinition("empleado_15", "Empleado 15", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 340, 7, 8, new[] { "seguridad_2" }),
            new EntrepreneurTreeNodeDefinition("empleado_16", "Empleado 16", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 350, 6, 5, new[] { "proteina_1" }),
            new EntrepreneurTreeNodeDefinition("empleado_17", "Empleado 17", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 360, 6, 2, new[] { "productos_frescos_2" }),
            new EntrepreneurTreeNodeDefinition("empleado_18", "Empleado 18", EntrepreneurTreeNodeCategory.Empleados, "Desbloqueo de empleado para contratación futura.", "Empleado disponible para contratación desde la app Empleados.", 1, EntrepreneurTreeNodeUnlockType.Employee, 370, 9, 8, new[] { "seguridad_3" }),

            // Seguridad
            new EntrepreneurTreeNodeDefinition(
                id: "seguridad_1",
                displayName: "Seguridad Nivel 1",
                category: EntrepreneurTreeNodeCategory.Seguridad,
                description: "Nivel inicial de protección.",
                benefit: "Cámaras de seguridad y 33% de probabilidad de arresto automático.",
                cost: 1,
                unlockType: EntrepreneurTreeNodeUnlockType.SecurityLevel,
                sortOrder: 400,
                visualColumn: 6,
                visualRow: 7,
                requiredNodeIds: new[] { "empleado_7" }),
            new EntrepreneurTreeNodeDefinition(
                id: "seguridad_2",
                displayName: "Seguridad Nivel 2",
                category: EntrepreneurTreeNodeCategory.Seguridad,
                description: "Nivel intermedio de protección.",
                benefit: "Guardias de seguridad y 66% de probabilidad de arresto automático.",
                cost: 1,
                unlockType: EntrepreneurTreeNodeUnlockType.SecurityLevel,
                sortOrder: 410,
                visualColumn: 7,
                visualRow: 8,
                requiredNodeIds: new[] { "empleado_8" }),
            new EntrepreneurTreeNodeDefinition(
                id: "seguridad_3",
                displayName: "Seguridad Nivel 3",
                category: EntrepreneurTreeNodeCategory.Seguridad,
                description: "Nivel máximo de protección.",
                benefit: "Alarmas/arcos antihurto y 99% de probabilidad de arresto automático.",
                cost: 1,
                unlockType: EntrepreneurTreeNodeUnlockType.SecurityLevel,
                sortOrder: 420,
                visualColumn: 8,
                visualRow: 8,
                requiredNodeIds: new[] { "empleado_14" }),

            // Mejoras
            new EntrepreneurTreeNodeDefinition(
                id: "mejora_cafeina",
                displayName: "Cafeína",
                category: EntrepreneurTreeNodeCategory.Mejoras,
                description: "Mejora operativa para el personal.",
                benefit: "Empleados trabajan 10% más rápido.",
                cost: 1,
                unlockType: EntrepreneurTreeNodeUnlockType.Upgrade,
                sortOrder: 500,
                visualColumn: 6,
                visualRow: 1,
                requiredNodeIds: new[] { "productos_frescos_2" }),
            new EntrepreneurTreeNodeDefinition(
                id: "mejora_carismatico",
                displayName: "Carismático",
                category: EntrepreneurTreeNodeCategory.Mejoras,
                description: "Mejora comercial para cajas.",
                benefit: "Cajeros generan 5% más ingresos por venta atendida.",
                cost: 1,
                unlockType: EntrepreneurTreeNodeUnlockType.Upgrade,
                sortOrder: 510,
                visualColumn: 8,
                visualRow: 9,
                requiredNodeIds: new[] { "empleado_15" })
        };

        private static readonly Dictionary<string, EntrepreneurTreeNodeDefinition> nodesById = nodes.ToDictionary(node => node.Id);

        private static readonly Dictionary<string, string> productNodeByProductId = new Dictionary<string, string>
        {
            { "0", DefaultUnlockedNodeId },
            { "1", DefaultUnlockedNodeId },
            { "2", DefaultUnlockedNodeId },
            { "3", DefaultUnlockedNodeId },
            { "4", DefaultUnlockedNodeId }
        };

        private static readonly Dictionary<string, string> productNodeByKnownAlias = new Dictionary<string, string>
        {
            { "product_a", DefaultUnlockedNodeId },
            { "product_b", DefaultUnlockedNodeId },
            { "product_c", DefaultUnlockedNodeId },
            { "product_d", DefaultUnlockedNodeId },
            { "product_e", DefaultUnlockedNodeId },
            { "product a", DefaultUnlockedNodeId },
            { "product b", DefaultUnlockedNodeId },
            { "product c", DefaultUnlockedNodeId },
            { "product d", DefaultUnlockedNodeId },
            { "product e", DefaultUnlockedNodeId },
            { "leche", DefaultUnlockedNodeId },
            { "sal", DefaultUnlockedNodeId },
            { "agua", DefaultUnlockedNodeId },
            { "pasta", DefaultUnlockedNodeId },
            { "azucar", DefaultUnlockedNodeId }
        };

        private static readonly Dictionary<string, string> productNodeByNormalizedTitle = BuildProductTitleMap();

        public static IReadOnlyList<EntrepreneurTreeNodeDefinition> Nodes => nodes;

        public static EntrepreneurTreeNodeDefinition Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            nodesById.TryGetValue(id, out EntrepreneurTreeNodeDefinition node);
            return node;
        }

        public static string GetDisplayName(string id)
        {
            EntrepreneurTreeNodeDefinition node = Get(id);
            return node != null ? node.DisplayName : id;
        }

        public static IEnumerable<EntrepreneurTreeNodeDefinition> GetNodesByCategory(EntrepreneurTreeNodeCategory? category)
        {
            IEnumerable<EntrepreneurTreeNodeDefinition> result = nodes;
            if (category.HasValue)
                result = result.Where(node => node.Category == category.Value);

            return result.OrderBy(node => node.VisualColumn).ThenBy(node => node.VisualRow).ThenBy(node => node.SortOrder).ThenBy(node => node.DisplayName);
        }

        public static string GetKnownProductNodeId(string productId)
        {
            if (string.IsNullOrEmpty(productId))
                return string.Empty;

            return productNodeByProductId.TryGetValue(productId, out string nodeId) ? nodeId : string.Empty;
        }

        public static string GetKnownProductNodeId(ProductScriptableObject product)
        {
            if (product == null)
                return string.Empty;

            string byId = GetKnownProductNodeId(product.id);
            if (!string.IsNullOrEmpty(byId))
                return byId;

            if (string.IsNullOrEmpty(product.title))
                return string.Empty;

            string normalized = NormalizeText(product.title);
            if (productNodeByNormalizedTitle.TryGetValue(normalized, out string nodeId))
                return nodeId;

            string normalizedName = NormalizeText(product.name);
            if (productNodeByKnownAlias.TryGetValue(normalizedName, out nodeId))
                return nodeId;

            return productNodeByKnownAlias.TryGetValue(normalized, out nodeId) ? nodeId : string.Empty;
        }

        public static string GetUnlockRequirementLabel(ProductScriptableObject product)
        {
            string nodeId = GetKnownProductNodeId(product);
            if (string.IsNullOrEmpty(nodeId))
                return "Producto bloqueado. Desbloquea el nodo requerido en el Árbol del Emprendedor.";

            return "Producto bloqueado. Desbloquea " + GetDisplayName(nodeId) + " en el Árbol del Emprendedor.";
        }

        public static string GetProductLockedPurchaseMessage(ProductScriptableObject product)
        {
            string nodeId = GetKnownProductNodeId(product);
            if (string.IsNullOrEmpty(nodeId))
                return "Producto bloqueado. Desbloquea el nodo requerido en el Árbol del Emprendedor.";

            return "Producto bloqueado. Desbloquea " + GetDisplayName(nodeId) + " en el Árbol del Emprendedor.";
        }

        private static Dictionary<string, string> BuildProductTitleMap()
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            foreach (EntrepreneurTreeNodeDefinition node in nodes)
            {
                if (node.UnlockType != EntrepreneurTreeNodeUnlockType.ProductGroup)
                    continue;

                for (int i = 0; i < node.ProductNames.Length; i++)
                {
                    string normalized = NormalizeText(node.ProductNames[i]);
                    if (!string.IsNullOrEmpty(normalized) && !map.ContainsKey(normalized))
                        map.Add(normalized, node.Id);
                }
            }

            return map;
        }

        private static string NormalizeText(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            string normalized = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            StringBuilder builder = new StringBuilder(normalized.Length);
            for (int i = 0; i < normalized.Length; i++)
            {
                char current = normalized[i];
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(current);
                if (category != UnicodeCategory.NonSpacingMark)
                    builder.Append(current);
            }

            return builder.ToString().Normalize(NormalizationForm.FormC).Trim();
        }
    }
}
