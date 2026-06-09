//Adaptado por POMPIC 20100333
using System.Collections.Generic;

namespace FLOBUK.StoreSimulator
{
    public sealed class EntrepreneurAchievementDefinition
    {
        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public int RewardPoints { get; }
        public bool IsHook { get; }
        public string HookReason { get; }

        public EntrepreneurAchievementDefinition(string id, string title, string description, int rewardPoints = 1, bool isHook = false, string hookReason = "")
        {
            Id = id;
            Title = title;
            Description = description;
            RewardPoints = rewardPoints;
            IsHook = isHook;
            HookReason = hookReason;
        }
    }

    public static class EntrepreneurAchievementDefinitions
    {
        private static readonly List<EntrepreneurAchievementDefinition> achievements = new List<EntrepreneurAchievementDefinition>
        {
            new EntrepreneurAchievementDefinition("primeras_ventas", "Primeras Ventas", "Realiza tus primeras 15 ventas."),
            new EntrepreneurAchievementDefinition("venta_rapida", "Venta Rapida", "Completa 10 ventas en menos de un minuto."),

            new EntrepreneurAchievementDefinition("ingresos_1", "Objetivo de Ingresos 1", "Alcanza $5,000 en ingresos totales."),
            new EntrepreneurAchievementDefinition("ingresos_2", "Objetivo de Ingresos 2", "Alcanza $10,000 en ingresos totales."),
            new EntrepreneurAchievementDefinition("ingresos_3", "Objetivo de Ingresos 3", "Alcanza $12,000 en ingresos totales."),
            new EntrepreneurAchievementDefinition("ingresos_4", "Objetivo de Ingresos 4", "Alcanza $15,000 en ingresos totales."),
            new EntrepreneurAchievementDefinition("ingresos_5", "Objetivo de Ingresos 5", "Alcanza $17,000 en ingresos totales."),
            new EntrepreneurAchievementDefinition("ingresos_6", "Objetivo de Ingresos 6", "Alcanza $20,000 en ingresos totales."),
            new EntrepreneurAchievementDefinition("ingresos_7", "Objetivo de Ingresos 7", "Alcanza $25,000 en ingresos totales."),
            new EntrepreneurAchievementDefinition("ingresos_8", "Objetivo de Ingresos 8", "Alcanza $35,000 en ingresos totales."),
            new EntrepreneurAchievementDefinition("ingresos_9", "Objetivo de Ingresos 9", "Alcanza $50,000 en ingresos totales."),
            new EntrepreneurAchievementDefinition("lluvia_dinero", "Lluvia de Dinero", "Gana $100,000 en ingresos totales."),

            new EntrepreneurAchievementDefinition("ventas_diarias_1", "Ventas Diarias 1", "Supera $1,000 en ventas en un dia."),
            new EntrepreneurAchievementDefinition("ventas_diarias_2", "Ventas Diarias 2", "Supera $1,500 en ventas en un dia."),
            new EntrepreneurAchievementDefinition("ventas_diarias_3", "Ventas Diarias 3", "Supera $5,000 en ventas en un dia."),
            new EntrepreneurAchievementDefinition("ventas_diarias_4", "Ventas Diarias 4", "Supera $10,000 en ventas en un dia."),
            new EntrepreneurAchievementDefinition("ventas_diarias_5", "Ventas Diarias 5", "Supera $15,000 en ventas en un dia."),
            new EntrepreneurAchievementDefinition("ventas_diarias_6", "Ventas Diarias 6", "Supera $20,000 en ventas en un dia."),

            new EntrepreneurAchievementDefinition("cliente_lujo", "Cliente de Lujo", "Vende un producto de lujo."),
            new EntrepreneurAchievementDefinition("lindo_hogar", "Lindo hogar", "Vende tu primer electrodomestico."),
            new EntrepreneurAchievementDefinition("donador", "Donador", "Haz que se vendan 5 productos a $0.00."),

            new EntrepreneurAchievementDefinition("primer_empleado", "Primer Empleado", "Contrata tu primer empleado."),
            new EntrepreneurAchievementDefinition("maximo_empleo", "Maximo Empleo", "Contrata 18 empleados y asigna rol a cada uno."),

            new EntrepreneurAchievementDefinition("supermercado_crecimiento", "Supermercado en Crecimiento", "Expande el supermercado a 300 m2."),
            new EntrepreneurAchievementDefinition("imperialista", "Imperialista", "Llega al tamano maximo del supermercado."),
            new EntrepreneurAchievementDefinition("almacenamiento_maximizado", "Almacenamiento Maximizado", "Expande almacenamiento al maximo permitido."),
            new EntrepreneurAchievementDefinition("eficiencia_maximo", "Eficiencia al Maximo", "Mantiene estantes abastecidos durante una semana de juego.", 1, true, "No hay metrica semanal estable de estantes abastecidos."),
            new EntrepreneurAchievementDefinition("limpieza_impecable", "Limpieza Impecable", "Mantiene el supermercado limpio durante una semana de juego.", 1, true, "No existe sistema de suciedad o limpieza en esta copia del asset."),

            new EntrepreneurAchievementDefinition("surtido_completo", "Surtido Completo", "Desbloquea todos los productos basicos."),
            new EntrepreneurAchievementDefinition("red_seguridad", "Red de Seguridad", "Desbloquea y activa todas las medidas de seguridad."),
            new EntrepreneurAchievementDefinition("optimizacion_total", "Optimizacion Total", "Implementa todas las mejoras disponibles en el Arbol."),
            new EntrepreneurAchievementDefinition("optimista", "Optimista", "Desbloquea todas las mejoras."),
            new EntrepreneurAchievementDefinition("arbol_completo", "Arbol Completo", "Desbloquea todos los nodos del Arbol.", 0),

            new EntrepreneurAchievementDefinition("dedicado", "Dedicado", "Juega 3 dias dentro del juego."),
            new EntrepreneurAchievementDefinition("fiel", "Fiel", "Juega 5 dias dentro del juego."),
            new EntrepreneurAchievementDefinition("emprendedor", "Emprendedor", "Juega 7 dias dentro del juego."),

            new EntrepreneurAchievementDefinition("batman", "Batman", "Arresta tu primer ratero."),
            new EntrepreneurAchievementDefinition("rapidez", "Rapidez", "Compra tu primera caja registradora.", 1, true, "No hay evento especifico de compra de caja registradora separado de otros StorageObject."),
            new EntrepreneurAchievementDefinition("bajo_presion", "Bajo Presion", "Supera $500 en ventas en un dia sin contratar empleados."),
            new EntrepreneurAchievementDefinition("precio_perfecto", "Precio Perfecto", "Maximiza ganancias sin quejas por precios durante una semana.", 1, true, "No hay metrica semanal de precios sin quejas."),
            new EntrepreneurAchievementDefinition("paciente", "Paciente", "Consigue que un cliente se queje de tus precios."),
            new EntrepreneurAchievementDefinition("perezoso", "Perezoso", "No corras durante todo un dia.", 1, true, "No hay tracking diario de correr en PlayerController."),
            new EntrepreneurAchievementDefinition("huevo_dorado", "Huevo dorado", "Encuentrate al creador como cliente despues de completar el Arbol.", 1, true, "No existe cliente creador ni evento de encuentro en esta copia."),
        };

        private static readonly Dictionary<string, EntrepreneurAchievementDefinition> byId = BuildIndex();

        public static IReadOnlyList<EntrepreneurAchievementDefinition> Achievements => achievements;

        public static EntrepreneurAchievementDefinition Get(string id)
        {
            byId.TryGetValue(id, out EntrepreneurAchievementDefinition definition);
            return definition;
        }

        private static Dictionary<string, EntrepreneurAchievementDefinition> BuildIndex()
        {
            Dictionary<string, EntrepreneurAchievementDefinition> index = new Dictionary<string, EntrepreneurAchievementDefinition>();
            for (int i = 0; i < achievements.Count; i++)
                index[achievements[i].Id] = achievements[i];

            return index;
        }
    }
}
