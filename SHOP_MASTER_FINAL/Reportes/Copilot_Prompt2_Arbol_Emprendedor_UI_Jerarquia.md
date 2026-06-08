<!-- Adaptado por POMPIC 20100333 -->
# Copilot Prompt 2/5 - Árbol del Emprendedor UI, jerarquía y prerequisitos

## Resumen
Se auditó la implementación actual del Árbol del Emprendedor y se corrigió la parte funcional/visual para que quede integrada a la computadora del asset, con jerarquía de nodos, filtros por categoría, detalle de nodo, estados claros (Desbloqueado/Disponible/Bloqueado/Falta/Puntos insuficientes), y persistencia conectada al flujo existente de guardado.

## Documentos revisados
- `SHOP_MASTER_FINAL/Documentos/NEW_Requerimientos.docx`
- `SHOP_MASTER_FINAL/Documentos/propuestas juanito (2).docx`
- `SHOP_MASTER_FINAL/Documentos/Protocolo prpuesta Juanito (1).pdf`
- `SHOP_MASTER_FINAL/Documentos/Guia Gantt (1).pdf`
- `SHOP_MASTER_FINAL/Reportes/Copilot_Prompt1_Limpieza_Estabilidad_Arbol.md`
- `SHOP_MASTER_FINAL/Reportes/Fase1_Arbol_Emprendedor_UI_Computadora.md`

## Requerimientos usados
- **RQF3**: acceso al Árbol desde computadora conservado en `UIShopDesktop` + `EntrepreneurTreeUIBootstrap`.
- **RQF4**: desbloqueo bloqueado por prerequisitos y con mensaje de faltantes en `EntrepreneurProgress`.
- **RQF8**: empleados 1 a 18 definidos como nodos de tipo `Employee` con prerequisitos documentales.
- **RQF11**: seguridad 1/2/3 definida como nodos `SecurityLevel` con costo y beneficio documental.
- **RQF12**: seguridad condicionada por prerequisitos (`empleado_7`, `empleado_8`, `empleado_14`).
- **RQF18**: seguridad persistida vía nodos desbloqueados y consultable por `GetUnlockedSecurityLevel()`.
- **RQF36**: normalización de carga y limpieza de nodos inválidos conservando defaults válidos.
- **RQF28 de mejoras**: nodos `mejora_cafeina` y `mejora_carismatico` con multiplicadores consultables.
- **RQNF8**: lógica centralizada de estados y prerequisitos.
- **RQNF9**: estado mostrado en texto, no solo por color.
- **RQNF18**: UI integrada dentro del flujo real de la laptop/computadora del asset.

## Estado del Árbol
- Categorías activas: **Productos, Empleados, Seguridad, Mejoras**.
- Nodos centralizados en `EntrepreneurTreeNodeDefinition.cs` con campos:
  `id`, `displayName`, `category`, `description`, `benefit`, `cost`, `requiredNodeIds`, `unlockType`, `sortOrder`, `visualColumn`, `visualRow`.
- Prerequisitos documentales implementados para productos, empleados, seguridad y mejoras.
- Costo de nodos desbloqueables: **1 punto** (excepto `productos_basicos_1`, costo 0).
- Estados visibles: `Desbloqueado`, `Disponible`, `Bloqueado`, `Falta: ...`, `Puntos insuficientes`.
- `Productos Básicos 1` queda forzado como default desbloqueado en reset/carga/normalización.

## UI de computadora
- Integración: sigue dentro de `UIShopDesktop` mediante `EntrepreneurTreeUIBootstrap`, sin Canvas paralelo.
- Reutilización de asset: `Navigation`, `Categories`, `UIShopCategoryHelper`, `Button`, `Image`, `ScrollRect`, `TMP` y layouts de Unity UI.
- Evita duplicado:
  - Reusa panel existente `Entrepreneur Tree` si ya existe.
  - Reusa botón de navegación existente del árbol o lo crea una sola vez.
  - Limpia listeners y vuelve a enlazar solo una acción de abrir panel.
- Scroll/layout:
  - ScrollView horizontal/vertical para mapa de nodos.
  - Distribución por `visualColumn` y `visualRow` para evitar amontonamiento.
  - Filtros internos: `Todo`, `Productos`, `Empleados`, `Seguridad`, `Mejoras`.
- Detalles:
  - Muestra nombre, categoría, estado, costo, requisitos, razón actual de bloqueo, descripción, beneficio y productos incluidos (si aplica).
  - Botón `Desbloquear` habilitado solo en estado `Disponible`.

## Productos desbloqueables
- Nodos implementados/validados:
  - `productos_basicos_1`, `productos_basicos_2`, `productos_basicos_3`, `lacteos_1`, `especias_1`, `productos_frescos_1`, `productos_frescos_2`, `lacteos_2`, `productos_higiene`, `sodas`, `proteina_1`, `productos_lujo_1`, `electrodomesticos_1`.
- `proteina_1` quedó con prerequisito `lacteos_1` (justificación: en este repo no hay tabla técnica adicional de prerequisito para `proteina_1`; se eligió el nodo documental más cercano y consistente con la rama de perecederos).
- Mapeo producto->nodo:
  - Por `id` real actual del asset (`0..4`) para no romper catálogo existente.
  - Soporte adicional por nombre normalizado para futuros productos documentales.
- Si no hay mapeo conocido, se evita error rojo y se muestra mensaje genérico de desbloqueo por Árbol.

## Empleados desbloqueables
- Empleados **1 a 18** definidos como nodos `Employee`, costo 1 y beneficio estándar:
  `Empleado disponible para contratación desde la app Empleados.`
- No se contratan automáticamente; solo quedan desbloqueados para sistemas posteriores.
- Se respetó la cadena documental de prerequisitos indicada en `NEW_Requerimientos.docx`.

## Seguridad
- `seguridad_1`: requiere `empleado_7`, costo 1, cámaras, 33% arresto automático.
- `seguridad_2`: requiere `empleado_8`, costo 1, guardias, 66% arresto automático.
- `seguridad_3`: requiere `empleado_14`, costo 1, alarma/arcos, 99% arresto automático.
- Consulta limpia disponible en `EntrepreneurProgress.GetUnlockedSecurityLevel()`.

## Mejoras
- `mejora_cafeina`: requiere `productos_frescos_2`, costo 1, +10% velocidad empleados.
- `mejora_carismatico`: requiere `empleado_15`, costo 1, +5% ingresos cajeros.
- Consultas disponibles:
  - `IsUpgradeUnlocked("mejora_cafeina")`
  - `IsUpgradeUnlocked("mejora_carismatico")`
  - `GetEmployeeSpeedMultiplier()`
  - `GetCashierSalesMultiplier()`

## Guardado/carga
- Se mantiene integración con `SaveGameSystem` existente (sin sistema paralelo).
- El árbol guarda/carga mediante `EntrepreneurProgress`:
  - puntos disponibles (`progressPoints`)
  - nodos desbloqueados (`unlockedNodeIds`)
  - derivados de nodos: productos, empleados, seguridad y mejoras desbloqueadas.
- Protección de saves viejos:
  - reset a defaults seguros al cargar,
  - `productos_basicos_1` siempre desbloqueado,
  - nodos desconocidos o con prerequisitos inválidos se normalizan y remueven.

## Archivos modificados
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeNodeDefinition.cs`
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurProgress.cs`
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeUI.cs`
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeUIBootstrap.cs`
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/UIShopItemProduct.cs`
- `SHOP_MASTER_FINAL/Reportes/Copilot_Prompt2_Arbol_Emprendedor_UI_Jerarquia.md`

## Inscripción POMPIC
Archivos con inscripción aplicada:
- `//Adaptado por POMPIC 20100333`
  - `EntrepreneurTreeNodeDefinition.cs`
  - `EntrepreneurProgress.cs`
  - `EntrepreneurTreeUI.cs`
  - `EntrepreneurTreeUIBootstrap.cs`
  - `UIShopItemProduct.cs`
- `<!-- Adaptado por POMPIC 20100333 -->`
  - `Copilot_Prompt2_Arbol_Emprendedor_UI_Jerarquia.md`

Excepciones justificadas:
- No se editaron `.meta`, binarios ni assets serializados delicados en esta fase.

## Validaciones posibles desde la nube
- Revisión estática de integración entre scripts del árbol, desktop, productos y guardado.
- Revisión de no-duplicación de panel/botón en bootstrap por inspección de código.
- Verificación de referencias de `Animo` en `SHOP_MASTER_FINAL` (solo aparecen en reporte histórico, no en código funcional).
- Intento de compilación .NET del proyecto (`dotnet build SHOP_MASTER_FINAL.sln`).

## Validaciones no posibles desde la nube
- No fue posible ejecutar Unity Editor/Play Mode en este entorno cloud.
- No fue posible compilar realmente con Unity porque el contenedor no tiene reference assemblies de .NET Framework 4.7.1 requeridas por los csproj generados de Unity.
- Debe validar Isaac localmente en Unity:
  - abrir laptop/computadora,
  - abrir/cerrar Árbol varias veces y verificar no duplicados,
  - revisar scroll/filtros/layout visual,
  - desbloquear nodos y verificar puntos/prerequisitos en tiempo real,
  - guardar/cargar partida y confirmar persistencia,
  - validar bloqueo/desbloqueo de productos en UI de compra.

## Estado final
- **AMARILLO**
  - Lógica y UI del Árbol quedaron integradas y más estables por inspección de código.
  - Faltan pruebas locales en Unity para confirmar comportamiento visual/final en ejecución real.

## Pendientes para Prompt 3/5
- Completar mapeo exhaustivo de catálogo real cuando estén disponibles todos los productos documentales en assets.
- Conectar desbloqueos del Árbol a flujo completo de compra/delivery/inventario.
- Integrar efectos funcionales completos de empleados/seguridad con sus sistemas de gameplay finales.
- Ajustar mensajes UX finales en compra según flujo definitivo de bloqueo de productos.
