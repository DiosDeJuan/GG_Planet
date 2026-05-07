using System.Collections.Generic;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    public static class EntrepreneurTreeDefinition
    {
        public const string DefaultUnlockedNodeId = "product_basic_1";

        public static List<NodeData> CreateRuntimeNodes()
        {
            List<NodeData> nodes = new List<NodeData>();
            for (int i = 0; i < Definitions.Length; i++)
            {
                NodeDefinition def = Definitions[i];
                NodeData node = ScriptableObject.CreateInstance<NodeData>();
                node.name = def.id;
                node.id = def.id;
                node.nodeType = def.type;
                node.title = def.title;
                node.description = def.description;
                node.cost = def.cost;
                node.uiPosition = def.position;
                node.requiredNodeIds = new List<string>(def.requiredIds);
                nodes.Add(node);
            }

            return nodes;
        }

        public static void SynchronizeTreeData(TreeData treeData)
        {
            if (treeData == null)
                return;

            if (treeData.nodes == null)
                treeData.nodes = new List<NodeData>();

            Dictionary<string, NodeData> byId = new Dictionary<string, NodeData>();
            for (int i = 0; i < treeData.nodes.Count; i++)
            {
                NodeData node = treeData.nodes[i];
                if (node == null || string.IsNullOrEmpty(node.id))
                    continue;

                if (!byId.ContainsKey(node.id))
                    byId.Add(node.id, node);
            }

            for (int i = 0; i < Definitions.Length; i++)
            {
                NodeDefinition def = Definitions[i];
                NodeData node;
                if (!byId.TryGetValue(def.id, out node))
                {
                    node = ScriptableObject.CreateInstance<NodeData>();
                    node.name = def.id;
                    node.hideFlags = HideFlags.DontSave;
                    treeData.nodes.Add(node);
                    byId[def.id] = node;
                }

                ApplyDefinition(node, def);
            }
        }

        private static void ApplyDefinition(NodeData node, NodeDefinition def)
        {
            node.id = def.id;
            node.nodeType = def.type;
            node.title = def.title;
            node.description = def.description;
            node.cost = def.cost;
            node.uiPosition = def.position;
            node.requiredNodeIds = new List<string>(def.requiredIds);
        }


        private static readonly NodeDefinition[] Definitions =
        {
            new NodeDefinition("product_basic_1", TreeNodeType.Product, "Productos Básicos 1", "Desbloquea el surtido inicial esencial.", 0, new string[0], new Vector2(-1120f, 420f)),
            new NodeDefinition("product_basic_2", TreeNodeType.Product, "Productos Básicos 2", "Expande el surtido base para más ventas.", 1, new[] { "product_basic_1" }, new Vector2(-860f, 420f)),
            new NodeDefinition("product_basic_3", TreeNodeType.Product, "Productos Básicos 3", "Tercera etapa de productos base.", 1, new[] { "product_basic_2" }, new Vector2(-600f, 420f)),
            new NodeDefinition("product_dairy_1", TreeNodeType.Product, "Lácteos 1", "Incorpora la primera línea de lácteos.", 1, new[] { "product_basic_2" }, new Vector2(-860f, 200f)),
            new NodeDefinition("product_dairy_2", TreeNodeType.Product, "Lácteos 2", "Amplía el catálogo de lácteos.", 1, new[] { "product_dairy_1" }, new Vector2(-600f, 200f)),
            new NodeDefinition("product_dairy_3", TreeNodeType.Product, "Lácteos 3", "Desbloquea mozzarella y parmesano.", 1, new[] { "product_dairy_2" }, new Vector2(-340f, 200f)),
            new NodeDefinition("product_spices_1", TreeNodeType.Product, "Especias 1", "Desbloquea sección inicial de especias.", 1, new[] { "product_basic_2" }, new Vector2(-860f, -20f)),
            new NodeDefinition("product_fresh_1", TreeNodeType.Product, "Productos Frescos 1", "Activa la primera sección de frescos.", 1, new[] { "product_basic_2" }, new Vector2(-860f, -240f)),
            new NodeDefinition("product_fresh_2", TreeNodeType.Product, "Productos Frescos 2", "Expande la categoría de frescos.", 1, new[] { "product_fresh_1" }, new Vector2(-600f, -240f)),
            new NodeDefinition("product_hygiene", TreeNodeType.Product, "Productos de Higiene", "Habilita higiene y cuidado personal.", 1, new[] { "product_basic_3" }, new Vector2(-340f, 420f)),
            new NodeDefinition("product_protein_1", TreeNodeType.Product, "Proteína 1", "Añade productos proteicos clave.", 1, new[] { "product_fresh_2" }, new Vector2(-340f, -240f)),
            new NodeDefinition("product_sodas", TreeNodeType.Product, "Sodas", "Desbloquea bebidas gaseosas.", 1, new[] { "product_basic_3" }, new Vector2(-80f, 200f)),
            new NodeDefinition("product_luxury_1", TreeNodeType.Product, "Productos de Lujo 1", "Abre una gama premium inicial.", 1, new[] { "product_fresh_2" }, new Vector2(-80f, -240f)),
            new NodeDefinition("product_appliances_1", TreeNodeType.Product, "Electrodomésticos 1", "Inicia la línea de electrodomésticos.", 1, new[] { "product_basic_3" }, new Vector2(-80f, -20f)),

            new NodeDefinition("employee_1", TreeNodeType.Employee, "Empleado 1", "Primer empleado operativo.", 1, new[] { "product_spices_1" }, new Vector2(-600f, -500f)),
            new NodeDefinition("employee_2", TreeNodeType.Employee, "Empleado 2", "Refuerzo en tareas de sala.", 1, new[] { "product_hygiene" }, new Vector2(-340f, -500f)),
            new NodeDefinition("employee_3", TreeNodeType.Employee, "Empleado 3", "Apoyo para alta demanda.", 1, new[] { "product_sodas" }, new Vector2(-80f, -500f)),
            new NodeDefinition("employee_4", TreeNodeType.Employee, "Empleado 4", "Especialista en lácteos.", 1, new[] { "product_dairy_1" }, new Vector2(-600f, -700f)),
            new NodeDefinition("employee_5", TreeNodeType.Employee, "Empleado 5", "Operador de reposición.", 1, new[] { "product_dairy_1" }, new Vector2(-470f, -700f)),
            new NodeDefinition("employee_6", TreeNodeType.Employee, "Empleado 6", "Soporte de inventario.", 1, new[] { "product_spices_1" }, new Vector2(-340f, -700f)),
            new NodeDefinition("employee_7", TreeNodeType.Employee, "Empleado 7", "Supervisor de turno.", 1, new[] { "employee_5" }, new Vector2(-210f, -700f)),
            new NodeDefinition("employee_8", TreeNodeType.Employee, "Empleado 8", "Gestión de caja extendida.", 1, new[] { "product_sodas" }, new Vector2(-80f, -700f)),
            new NodeDefinition("employee_9", TreeNodeType.Employee, "Empleado 9", "Atención de piso.", 1, new[] { "product_hygiene" }, new Vector2(50f, -700f)),
            new NodeDefinition("employee_10", TreeNodeType.Employee, "Empleado 10", "Encargado de apertura.", 1, new[] { "employee_1" }, new Vector2(180f, -700f)),
            new NodeDefinition("employee_11", TreeNodeType.Employee, "Empleado 11", "Operador de seguridad interna.", 1, new[] { "security_1" }, new Vector2(310f, -700f)),
            new NodeDefinition("employee_12", TreeNodeType.Employee, "Empleado 12", "Especialista avanzado.", 1, new[] { "employee_13" }, new Vector2(440f, -700f)),
            new NodeDefinition("employee_13", TreeNodeType.Employee, "Empleado 13", "Coordinador premium.", 1, new[] { "product_luxury_1" }, new Vector2(310f, -500f)),
            new NodeDefinition("employee_14", TreeNodeType.Employee, "Empleado 14", "Técnico de electrodomésticos.", 1, new[] { "product_appliances_1" }, new Vector2(440f, -500f)),
            new NodeDefinition("employee_15", TreeNodeType.Employee, "Empleado 15", "Jefe de operaciones.", 1, new[] { "security_2" }, new Vector2(570f, -700f)),
            new NodeDefinition("employee_16", TreeNodeType.Employee, "Empleado 16", "Responsable de frescos premium.", 1, new[] { "product_protein_1" }, new Vector2(570f, -500f)),
            new NodeDefinition("employee_17", TreeNodeType.Employee, "Empleado 17", "Control de calidad frescos.", 1, new[] { "product_fresh_2" }, new Vector2(700f, -500f)),
            new NodeDefinition("employee_18", TreeNodeType.Employee, "Empleado 18", "Director de seguridad.", 1, new[] { "security_3" }, new Vector2(830f, -700f)),

            new NodeDefinition("security_1", TreeNodeType.Security, "Nivel de Seguridad 1 / Cámaras / 33%", "Habilita cámaras y cobertura básica de seguridad.", 1, new[] { "employee_7" }, new Vector2(60f, -920f)),
            new NodeDefinition("security_2", TreeNodeType.Security, "Nivel de Seguridad 2 / Guardias / 66%", "Habilita guardias y cobertura media.", 1, new[] { "employee_8" }, new Vector2(320f, -920f)),
            new NodeDefinition("security_3", TreeNodeType.Security, "Nivel de Seguridad 3 / Alarmas / 99%", "Habilita alarmas y cobertura casi total.", 1, new[] { "employee_14" }, new Vector2(580f, -920f)),

            new NodeDefinition("upgrade_caffeine", TreeNodeType.Improvement, "Cafeína", "+10% velocidad de empleados.", 1, new[] { "product_fresh_2" }, new Vector2(180f, -240f)),
            new NodeDefinition("upgrade_charismatic", TreeNodeType.Improvement, "Carismático", "+5% ventas.", 1, new[] { "employee_15" }, new Vector2(830f, -500f)),
        };


        private struct NodeDefinition
        {
            public readonly string id;
            public readonly TreeNodeType type;
            public readonly string title;
            public readonly string description;
            public readonly int cost;
            public readonly string[] requiredIds;
            public readonly Vector2 position;

            public NodeDefinition(string id, TreeNodeType type, string title, string description, int cost, string[] requiredIds, Vector2 position)
            {
                this.id = id;
                this.type = type;
                this.title = title;
                this.description = description;
                this.cost = cost;
                this.requiredIds = requiredIds;
                this.position = position;
            }
        }
    }
}
