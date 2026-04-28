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


        private static readonly NodeDefinition[] Definitions =
        {
            new NodeDefinition("product_basic_1", TreeNodeType.Product, "Productos Básicos 1", "Desbloquea el surtido inicial esencial.", 0, new string[0], new Vector2(-1120f, 420f)),
            new NodeDefinition("product_basic_2", TreeNodeType.Product, "Productos Básicos 2", "Expande el surtido base para más ventas.", 1, new[] { "product_basic_1" }, new Vector2(-860f, 420f)),
            new NodeDefinition("product_basic_3", TreeNodeType.Product, "Productos Básicos 3", "Tercera etapa de productos base.", 2, new[] { "product_basic_2" }, new Vector2(-600f, 420f)),
            new NodeDefinition("product_dairy_1", TreeNodeType.Product, "Lácteos 1", "Incorpora la primera línea de lácteos.", 2, new[] { "product_basic_2" }, new Vector2(-860f, 200f)),
            new NodeDefinition("product_dairy_2", TreeNodeType.Product, "Lácteos 2", "Amplía el catálogo de lácteos.", 3, new[] { "product_dairy_1" }, new Vector2(-600f, 200f)),
            new NodeDefinition("product_spices_1", TreeNodeType.Product, "Especias 1", "Desbloquea sección inicial de especias.", 2, new[] { "product_basic_2" }, new Vector2(-860f, -20f)),
            new NodeDefinition("product_fresh_1", TreeNodeType.Product, "Productos Frescos 1", "Activa la primera sección de frescos.", 2, new[] { "product_basic_2" }, new Vector2(-860f, -240f)),
            new NodeDefinition("product_fresh_2", TreeNodeType.Product, "Productos Frescos 2", "Expande la categoría de frescos.", 3, new[] { "product_fresh_1" }, new Vector2(-600f, -240f)),
            new NodeDefinition("product_hygiene", TreeNodeType.Product, "Productos de Higiene", "Habilita higiene y cuidado personal.", 3, new[] { "product_basic_3" }, new Vector2(-340f, 420f)),
            new NodeDefinition("product_protein_1", TreeNodeType.Product, "Proteína 1", "Añade productos proteicos clave.", 4, new[] { "product_fresh_2" }, new Vector2(-340f, -240f)),
            new NodeDefinition("product_sodas", TreeNodeType.Product, "Sodas", "Desbloquea bebidas gaseosas.", 3, new[] { "product_basic_3" }, new Vector2(-340f, 200f)),
            new NodeDefinition("product_luxury_1", TreeNodeType.Product, "Productos de Lujo 1", "Abre una gama premium inicial.", 5, new[] { "product_fresh_2" }, new Vector2(-80f, -240f)),
            new NodeDefinition("product_appliances_1", TreeNodeType.Product, "Electrodomésticos 1", "Inicia la línea de electrodomésticos.", 5, new[] { "product_fresh_2" }, new Vector2(-80f, -20f)),

            new NodeDefinition("employee_1", TreeNodeType.Employee, "Empleado 1", "Primer empleado operativo.", 2, new[] { "product_spices_1" }, new Vector2(-600f, -500f)),
            new NodeDefinition("employee_2", TreeNodeType.Employee, "Empleado 2", "Refuerzo en tareas de sala.", 2, new[] { "product_hygiene" }, new Vector2(-340f, -500f)),
            new NodeDefinition("employee_3", TreeNodeType.Employee, "Empleado 3", "Apoyo para alta demanda.", 2, new[] { "product_sodas" }, new Vector2(-80f, -500f)),
            new NodeDefinition("employee_4", TreeNodeType.Employee, "Empleado 4", "Especialista en lácteos.", 2, new[] { "product_dairy_1" }, new Vector2(-600f, -700f)),
            new NodeDefinition("employee_5", TreeNodeType.Employee, "Empleado 5", "Operador de reposición.", 2, new[] { "product_dairy_1" }, new Vector2(-470f, -700f)),
            new NodeDefinition("employee_6", TreeNodeType.Employee, "Empleado 6", "Soporte de inventario.", 2, new[] { "product_spices_1" }, new Vector2(-340f, -700f)),
            new NodeDefinition("employee_7", TreeNodeType.Employee, "Empleado 7", "Supervisor de turno.", 3, new[] { "employee_5" }, new Vector2(-210f, -700f)),
            new NodeDefinition("employee_8", TreeNodeType.Employee, "Empleado 8", "Gestión de caja extendida.", 3, new[] { "product_sodas" }, new Vector2(-80f, -700f)),
            new NodeDefinition("employee_9", TreeNodeType.Employee, "Empleado 9", "Atención de piso.", 3, new[] { "product_hygiene" }, new Vector2(50f, -700f)),
            new NodeDefinition("employee_10", TreeNodeType.Employee, "Empleado 10", "Encargado de apertura.", 3, new[] { "employee_1" }, new Vector2(180f, -700f)),
            new NodeDefinition("employee_11", TreeNodeType.Employee, "Empleado 11", "Operador de seguridad interna.", 3, new[] { "security_1" }, new Vector2(310f, -700f)),
            new NodeDefinition("employee_12", TreeNodeType.Employee, "Empleado 12", "Especialista avanzado.", 4, new[] { "employee_13" }, new Vector2(440f, -700f)),
            new NodeDefinition("employee_13", TreeNodeType.Employee, "Empleado 13", "Coordinador premium.", 4, new[] { "product_luxury_1" }, new Vector2(310f, -500f)),
            new NodeDefinition("employee_14", TreeNodeType.Employee, "Empleado 14", "Técnico de electrodomésticos.", 4, new[] { "product_appliances_1" }, new Vector2(440f, -500f)),
            new NodeDefinition("employee_15", TreeNodeType.Employee, "Empleado 15", "Jefe de operaciones.", 5, new[] { "security_2" }, new Vector2(570f, -700f)),
            new NodeDefinition("employee_16", TreeNodeType.Employee, "Empleado 16", "Responsable de frescos premium.", 4, new[] { "product_protein_1" }, new Vector2(570f, -500f)),
            new NodeDefinition("employee_17", TreeNodeType.Employee, "Empleado 17", "Control de calidad frescos.", 4, new[] { "product_fresh_2" }, new Vector2(700f, -500f)),
            new NodeDefinition("employee_18", TreeNodeType.Employee, "Empleado 18", "Director de seguridad.", 6, new[] { "security_3" }, new Vector2(830f, -700f)),

            new NodeDefinition("security_1", TreeNodeType.Security, "Nivel de Seguridad 1 / Cámaras / 33%", "Habilita cámaras y cobertura básica de seguridad.", 3, new[] { "employee_7" }, new Vector2(60f, -920f)),
            new NodeDefinition("security_2", TreeNodeType.Security, "Nivel de Seguridad 2 / Guardias / 66%", "Habilita guardias y cobertura media.", 4, new[] { "employee_8" }, new Vector2(320f, -920f)),
            new NodeDefinition("security_3", TreeNodeType.Security, "Nivel de Seguridad 3 / Alarmas / 99%", "Habilita alarmas y cobertura casi total.", 5, new[] { "employee_14" }, new Vector2(580f, -920f)),

            new NodeDefinition("upgrade_caffeine", TreeNodeType.Improvement, "Cafeína", "+10% velocidad de empleados.", 4, new[] { "product_fresh_2" }, new Vector2(180f, -240f)),
            new NodeDefinition("upgrade_charismatic", TreeNodeType.Improvement, "Carismático", "+5% ventas.", 5, new[] { "employee_15" }, new Vector2(830f, -500f)),
        };


        private readonly struct NodeDefinition
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
