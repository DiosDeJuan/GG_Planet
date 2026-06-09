<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 9 - Arbol visual, productos completos provisionales y Empleados premium

## 22.1 Encabezado

- Fase: Codex Fase 9 - Arbol visual definitivo, productos completos provisionales y Empleados premium.
- Rama base: `origin/codex/fase8-arbol-100-cierre-integral`.
- Commit base esperado: `9747bc8`.
- Commit base verificado: `9747bc86441a0dd928215ef3d2dd4521497dbf63`.
- Rama nueva: `codex/fase9-arbol-visual-productos-empleados-premium`.
- Responsable: Codex.
- Proyecto: `SHOP_MASTER_FINAL`.
- Repositorio: `DiosDeJuan/GG_Planet`.
- Fecha: 2026-06-09.

## 22.2 Documentos revisados

- `Documentos/NEW_Requerimientos.docx`.
- `Documentos/propuestas juanito (2).docx`.
- `Documentos/Protocolo prpuesta Juanito (1).pdf`.
- `Documentos/Guia Gantt (1).pdf`.
- Reporte Fase 8: `Reportes/Codex_Fase8_Arbol_100_Cierre_Integral.md`.
- Reportes Fase 4.2, 4.3, 4.4 sobre computadora, UI de Arbol, nodos, requisitos, logros y puntos.
- Reportes Fase 6 y Fase 7 para continuidad QA y PlayMode.

## 22.3 Problema visual corregido

La fase parte de un Arbol funcional pero visualmente debil: fondo claro/blanco, demasiado espacio vacio, nodos con poca jerarquia RPG, boton de accion demasiado grande, logros sin lectura compacta, nodos poco visibles, barra superior con riesgo de solape y una pestana Empleados con aspecto insuficiente. El cierre reemplaza esa lectura por un estilo oscuro, compacto, con colores de estado mas fuertes, panel de detalle mas util, nodos mas pequenos y una app Empleados con tarjetas, filtros, resumen y detalle lateral.

## 22.4 Cambios visuales del Arbol

| Area | Antes | Despues | Archivo | Riesgo | Validacion |
| --- | --- | --- | --- | --- | --- |
| Fondo/panel | Lectura clara y plana | Paleta oscura RPG con acentos cyan/magenta | `EntrepreneurTreeUI.cs` | Medio visual | PlayMode 32/32 |
| Header | Alto y con espacio libre | Header compacto con puntos y progreso visibles | `EntrepreneurTreeUI.cs` | Bajo | Build + PlayMode |
| Nodos | Grandes y con poco contraste | Nodos compactos, texto blanco, estados OK/LISTO/LOCK | `EntrepreneurTreeNodeView.cs` | Medio UI | `EntrepreneurTree_VisualLayoutHasNoDuplicateNodePositions` |
| Layout | 36 nodos, faltaba lacteos_3 | 37 nodos, 14 ramas de producto, sin posiciones duplicadas | `EntrepreneurTreeVisualLayout.cs` | Medio | Test de posiciones |
| Detalle | Beneficio generico | Detalle incluye productos desbloqueados por nodo | `EntrepreneurTreeUI.cs` | Bajo | Tests de catalogo |
| Barra superior | Riesgo de solape | Botones con ancho controlado, autosize y ellipsis | `UIShopDesktop.cs` | Bajo | Build + PlayMode |

## 22.5 Cambios visuales de Empleados

| Area | Antes | Despues | Archivo | Riesgo | Validacion |
| --- | --- | --- | --- | --- | --- |
| Panel | Lista simple | Panel oscuro premium con header y resumen | `UIEmployeesPanel.cs` | Medio | `Employees_All18CardsCanBeRepresented` |
| Navegacion | Pestana basica | Boton EMPLEADOS integrado a navegacion real | `UIEmployeesUIBootstrap.cs` | Bajo | `Employees_RebuildDoesNotDuplicateCardsOrListeners` |
| Filtros | Sin filtros visuales | Todos, Bloqueados, Disponibles, Contratados, Cajeros, Surtidores | `UIEmployeesPanel.cs` | Bajo | PlayMode |
| Tarjetas | Baja jerarquia | Tarjetas por empleado con prerequisito, salario y estado | `UIEmployeesPanel.cs` | Medio | 18 tarjetas en test |
| Roles | Accion dispersa | Botones Cajero, Surtidor y Sin rol conectados a `EmployeeManager` | `UIEmployeesPanel.cs` | Medio | Build + tests heredados |
| Detalle | Sin panel lateral premium | Detalle lateral con estado, prereq, rol y beneficio | `UIEmployeesPanel.cs` | Bajo | PlayMode |

## 22.6 Matriz de productos completos

| Categoria | Producto | Precio ideal | Costo paquete | Nodo del Arbol | Mueble/categoria | Asset | Compra | Precios | Estado final | Pendiente |
| --- | --- | ---: | ---: | --- | --- | --- | --- | --- | --- | --- |
| Productos Basicos 1 | Leche | 100 | 1200 | productos_basicos_1 | Gondolas | Real | Si | Si | OK | Ninguno |
| Productos Basicos 1 | Sal | 50 | 200 | productos_basicos_1 | Gondolas | Real | Si | Si | OK | Ninguno |
| Productos Basicos 1 | Agua | 50 | 600 | productos_basicos_1 | Gondolas | Real | Si | Si | OK | Ninguno |
| Productos Basicos 1 | Pasta | 100 | 500 | productos_basicos_1 | Gondolas | Real | Si | Si | OK | Ninguno |
| Productos Basicos 1 | Azucar | 100 | 400 | productos_basicos_1 | Gondolas | Real | Si | Si | OK | Ninguno |
| Productos Basicos 2 | Harina | 100 | 800 | productos_basicos_2 | Gondolas | Provisional | Si | Si | OK tecnico | Asset final |
| Productos Basicos 2 | Arroz | 150 | 1500 | productos_basicos_2 | Gondolas | Provisional | Si | Si | OK tecnico | Asset final |
| Productos Basicos 2 | Frijoles | 200 | 1500 | productos_basicos_2 | Gondolas | Provisional | Si | Si | OK tecnico | Asset final |
| Productos Basicos 2 | Pan | 200 | 700 | productos_basicos_2 | Gondolas | Provisional | Si | Si | OK tecnico | Asset final |
| Productos Basicos 2 | Aceite | 250 | 2500 | productos_basicos_2 | Gondolas | Provisional | Si | Si | OK tecnico | Asset final |
| Productos Basicos 3 | Cafe | 300 | 2500 | productos_basicos_3 | Gondolas | Provisional | Si | Si | OK tecnico | Asset final |
| Productos Basicos 3 | Huevo | 200 | 800 | productos_basicos_3 | Gondolas | Provisional | Si | Si | OK tecnico | Asset final |
| Lacteos 1 | Cheddar | 300 | 1200 | lacteos_1 | Refrigeradores | Provisional | Si | Si | OK tecnico | Asset final |
| Lacteos 1 | Yogurt natural | 100 | 600 | lacteos_1 | Refrigeradores | Provisional | Si | Si | OK tecnico | Asset final |
| Lacteos 1 | Mantequilla | 200 | 800 | lacteos_1 | Refrigeradores | Provisional | Si | Si | OK tecnico | Asset final |
| Lacteos 2 | Queso americano | 250 | 1000 | lacteos_2 | Refrigeradores | Provisional | Si | Si | OK tecnico | Asset final |
| Lacteos 2 | Queso crema | 200 | 800 | lacteos_2 | Refrigeradores | Provisional | Si | Si | OK tecnico | Asset final |
| Lacteos 3 | Mozzarella | 300 | 1000 | lacteos_3 | Refrigeradores | Provisional | Si | Si | OK tecnico | Asset final |
| Lacteos 3 | Parmesano | 500 | 2000 | lacteos_3 | Refrigeradores | Provisional | Si | Si | OK tecnico | Asset final |
| Especias 1 | Pimienta negra | 200 | 700 | especias_1 | Gondolas | Provisional | Si | Si | OK tecnico | Asset final |
| Especias 1 | Canela | 100 | 500 | especias_1 | Gondolas | Provisional | Si | Si | OK tecnico | Asset final |
| Productos Frescos 1 | Manzana | 200 | 2000 | productos_frescos_1 | Refrigeradores | Provisional | Si | Si | OK tecnico | Asset final |
| Productos Frescos 1 | Platano | 100 | 1000 | productos_frescos_1 | Refrigeradores | Provisional | Si | Si | OK tecnico | Asset final |
| Productos Frescos 1 | Jitomate | 200 | 2000 | productos_frescos_1 | Refrigeradores | Provisional | Si | Si | OK tecnico | Asset final |
| Productos Frescos 1 | Cebolla | 150 | 1500 | productos_frescos_1 | Refrigeradores | Provisional | Si | Si | OK tecnico | Asset final |
| Productos Frescos 2 | Uvas | 400 | 1000 | productos_frescos_2 | Refrigeradores | Provisional | Si | Si | OK tecnico | Asset final |
| Productos Frescos 2 | Zanahorias | 100 | 500 | productos_frescos_2 | Refrigeradores | Provisional | Si | Si | OK tecnico | Asset final |
| Productos Frescos 2 | Ajo | 150 | 600 | productos_frescos_2 | Refrigeradores | Provisional | Si | Si | OK tecnico | Asset final |
| Productos de Higiene | Jabon | 100 | 1500 | productos_higiene | Gondolas | Provisional | Si | Si | OK tecnico | Asset final |
| Productos de Higiene | Papel higienico | 500 | 2500 | productos_higiene | Gondolas | Provisional | Si | Si | OK tecnico | Asset final |
| Productos de Higiene | Detergente | 200 | 2000 | productos_higiene | Gondolas | Provisional | Si | Si | OK tecnico | Asset final |
| Productos de Higiene | Pasta de dientes | 100 | 800 | productos_higiene | Gondolas | Provisional | Si | Si | OK tecnico | Asset final |
| Proteina 1 | Res | 1000 | 4000 | proteina_1 | Congeladores | Provisional | Si | Si | OK tecnico | Asset final |
| Proteina 1 | Pollo | 500 | 2000 | proteina_1 | Congeladores | Provisional | Si | Si | OK tecnico | Asset final |
| Proteina 1 | Cerdo | 700 | 3000 | proteina_1 | Congeladores | Provisional | Si | Si | OK tecnico | Asset final |
| Proteina 1 | Pescado | 800 | 3000 | proteina_1 | Congeladores | Provisional | Si | Si | OK tecnico | Asset final |
| Sodas | Cola | 150 | 1200 | sodas | Refrigeradores | Provisional | Si | Si | OK tecnico | Asset final |
| Sodas | Cola sin azucar | 150 | 1200 | sodas | Refrigeradores | Provisional | Si | Si | OK tecnico | Asset final |
| Sodas | Refresco de limon | 150 | 1200 | sodas | Refrigeradores | Provisional | Si | Si | OK tecnico | Asset final |
| Productos de Lujo 1 | Trufa | 10000 | 15000 | productos_lujo_1 | Congeladores | Provisional | Si | Si | OK tecnico | Asset final |
| Productos de Lujo 1 | Chocolate importado | 300 | 1500 | productos_lujo_1 | Congeladores | Provisional | Si | Si | OK tecnico | Asset final |
| Productos de Lujo 1 | Caviar | 5000 | 40000 | productos_lujo_1 | Congeladores | Provisional | Si | Si | OK tecnico | Asset final |
| Electrodomesticos 1 | Refrigerador | 40000 | 40000 | electrodomesticos_1 | Electrodomesticos | Provisional | Si | Si | OK tecnico | Asset final |
| Electrodomesticos 1 | Microondas | 6000 | 6000 | electrodomesticos_1 | Electrodomesticos | Provisional | Si | Si | OK tecnico | Asset final |
| Electrodomesticos 1 | Horno | 20000 | 20000 | electrodomesticos_1 | Electrodomesticos | Provisional | Si | Si | OK tecnico | Asset final |
| Electrodomesticos 1 | Mesa | 10000 | 10000 | electrodomesticos_1 | Electrodomesticos | Provisional | Si | Si | OK tecnico | Asset final |
| Electrodomesticos 1 | Licuadora | 8000 | 8000 | electrodomesticos_1 | Electrodomesticos | Provisional | Si | Si | OK tecnico | Asset final |

## 22.7 Matriz de placeholders

Todos los productos sin asset final usan fallback seguro desde los productos reales existentes `Product_A-E` mediante `DocumentedProductCatalog.EnsureProducts`, reutilizando icono/prefab/size cuando faltan datos.

| Producto | Asset provisional usado | Motivo | Riesgo | Que falta para asset final | Estado |
| --- | --- | --- | --- | --- | --- |
| Harina, Arroz, Frijoles, Pan, Aceite | Placeholder from Product_A-E | No existe asset especifico en base | Visual repetido | Prefab/icono final | OK tecnico |
| Cafe, Huevo | Placeholder from Product_A-E | No existe asset especifico en base | Visual repetido | Prefab/icono final | OK tecnico |
| Cheddar, Yogurt natural, Mantequilla | Placeholder from Product_A-E | No existe asset especifico en base | Visual repetido | Prefab/icono final refrigerado | OK tecnico |
| Queso americano, Queso crema, Mozzarella, Parmesano | Placeholder from Product_A-E | No existe asset especifico en base | Visual repetido | Prefab/icono final lacteo | OK tecnico |
| Pimienta negra, Canela | Placeholder from Product_A-E | No existe asset especifico en base | Visual repetido | Prefab/icono final | OK tecnico |
| Manzana, Platano, Jitomate, Cebolla, Uvas, Zanahorias, Ajo | Placeholder from Product_A-E | No existe asset especifico en base | Visual repetido | Prefab/icono final fresco | OK tecnico |
| Jabon, Papel higienico, Detergente, Pasta de dientes | Placeholder from Product_A-E | No existe asset especifico en base | Visual repetido | Prefab/icono final higiene | OK tecnico |
| Res, Pollo, Cerdo, Pescado | Placeholder from Product_A-E | No existe asset especifico en base | Visual repetido | Prefab/icono final congelado | OK tecnico |
| Cola, Cola sin azucar, Refresco de limon | Placeholder from Product_A-E | No existe asset especifico en base | Visual repetido | Prefab/icono final bebida | OK tecnico |
| Trufa, Chocolate importado, Caviar | Placeholder from Product_A-E | No existe asset especifico en base | Visual repetido | Prefab/icono final lujo | OK tecnico |
| Refrigerador, Microondas, Horno, Mesa, Licuadora | Placeholder from Product_A-E | No existe asset especifico en base | Visual repetido y escala no final | Prefab/icono final electrodomestico | OK tecnico |

## 22.8 Matriz de nodos visuales

| Nodo | Categoria | Posicion/rama | Estado visual | Conexiones | Panel detalle | Estado final |
| --- | --- | --- | --- | --- | --- | --- |
| productos_basicos_1/2/3 | Producto | Rama inicial izquierda | OK/LISTO/LOCK | Activas por prereq | Lista productos | OK |
| lacteos_1/2/3 | Producto | Rama superior central | OK/LISTO/LOCK | Activas por prereq | Lista productos lacteos | OK |
| productos_frescos_1/2 | Producto | Rama media superior | OK/LISTO/LOCK | Activas por prereq | Lista frescos | OK |
| especias_1/productos_higiene/sodas | Producto | Rama inferior media | OK/LISTO/LOCK | Activas por prereq | Lista categorias | OK |
| proteina_1/productos_lujo_1/electrodomesticos_1 | Producto | Rama avanzada derecha | OK/LISTO/LOCK | Activas por prereq | Lista productos avanzados | OK |
| empleado_1 a empleado_18 | Empleado | Ramas conectadas a productos/seguridad | OK/LISTO/LOCK | Activas por prereq | Prereq y beneficio | OK |
| seguridad_1/2/3 | Seguridad | Ramas de empleados 7/8/14 | OK/LISTO/LOCK | Activas por prereq | Porcentaje 33/66/99 | OK |
| mejora_cafeina/mejora_carismatico | Mejora | Ramas finales | OK/LISTO/LOCK | Activas por prereq | Multiplicadores | OK |

## 22.9 Matriz de empleados

| Empleado | Prerequisito | Estado UI | Contratable | Rol | Guardado/carga | Estado final |
| --- | --- | --- | --- | --- | --- | --- |
| empleado_1 | especias_1 | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |
| empleado_2 | productos_higiene | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |
| empleado_3 | sodas | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |
| empleado_4 | lacteos_1 | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |
| empleado_5 | lacteos_1 | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |
| empleado_6 | especias_1 | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |
| empleado_7 | empleado_5 | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |
| empleado_8 | sodas | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |
| empleado_9 | productos_higiene | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |
| empleado_10 | empleado_1 | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |
| empleado_11 | seguridad_1 | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |
| empleado_12 | empleado_13 | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |
| empleado_13 | productos_lujo_1 | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |
| empleado_14 | electrodomesticos_1 | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |
| empleado_15 | seguridad_2 | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |
| empleado_16 | proteina_1 | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |
| empleado_17 | productos_frescos_2 | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |
| empleado_18 | seguridad_3 | Tarjeta/filtro/detalle | Si al desbloquear | Cajero/Surtidor/Sin rol | EmployeeManager JSON | OK |

## 22.10 Bugs encontrados

| ID | Modulo | Descripcion | Severidad | Causa raiz | Archivo | Estado |
| --- | --- | --- | --- | --- | --- | --- |
| F9-BUG-001 | Productos | Productos documentados posteriores a Basicos 1 no existian como datos comprables reales | Alta | Solo habia mapeos/hook, sin objetos en `ItemDatabase` | `ItemDatabase.cs`, `DocumentedProductCatalog.cs` | Corregido |
| F9-BUG-002 | Arbol | Faltaba rama visual/dato para Lacteos 3 con Mozzarella/Parmesano | Media | Matriz anterior no cubria todos los productos documentados | `EntrepreneurTreeNodeDefinition.cs`, `EntrepreneurTreeVisualLayout.cs` | Corregido |
| F9-BUG-003 | UI Arbol | Lectura visual poco RPG, espacios y contraste debiles | Media | Paleta/layout no priorizaban jerarquia visual | `EntrepreneurTreeUI.cs`, `EntrepreneurTreeNodeView.cs` | Corregido |
| F9-BUG-004 | Empleados | Pestana con lectura visual insuficiente | Media | Lista simple sin detalle/filtros claros | `UIEmployeesPanel.cs` | Corregido |
| F9-BUG-005 | Desktop | Riesgo de solape de barra superior al sumar apps | Baja | Botones sin ajuste compacto global | `UIShopDesktop.cs` | Corregido |

## 22.11 Bugs corregidos

| ID | Correccion | Archivo | Riesgo | Validacion |
| --- | --- | --- | --- | --- |
| F9-BUG-001 | Catalogo documentado de 47 productos integrado a `ItemDatabase` real | `DocumentedProductCatalog.cs`, `ItemDatabase.cs` | Medio | `DocumentedProductCatalog_RegistersAllDocumentedProducts` |
| F9-BUG-002 | Nodo `lacteos_3`, posicion y mapeos por nombre | `EntrepreneurTreeNodeDefinition.cs`, `EntrepreneurTreeVisualLayout.cs` | Bajo | `EntrepreneurTree_AllDocumentedProductCategoriesExist` |
| F9-BUG-003 | Paleta oscura, nodos compactos, detalle con productos por nodo | `EntrepreneurTreeUI.cs`, `EntrepreneurTreeNodeView.cs` | Medio | PlayMode 32/32 |
| F9-BUG-004 | Empleados premium con filtros, tarjetas y roles reales | `UIEmployeesPanel.cs` | Medio | `Employees_All18CardsCanBeRepresented` |
| F9-BUG-005 | Normalizacion de layout de botones en computadora | `UIShopDesktop.cs` | Bajo | Build + PlayMode |

## 22.12 Tests agregados

| Test | Que valida | Resultado | Evidencia |
| --- | --- | --- | --- |
| `EntrepreneurTree_AllDocumentedProductCategoriesExist` | 14 categorias de producto y catalogo por nodo | Passed | `Codex_Fase9_PlayModeResults.xml` |
| `EntrepreneurTree_ProductNodesListUnlockedProducts` | Cada nodo producto lista productos desbloqueados | Passed | XML |
| `EntrepreneurTree_VisualLayoutHasNoDuplicateNodePositions` | Sin fallback ni posiciones duplicadas | Passed | XML |
| `DocumentedProductCatalog_RegistersAllDocumentedProducts` | 47 productos registrados en lista real | Passed | XML |
| `StoreDatabase_AllDocumentedProductsHavePositiveIdealPriceExceptAllowedZeroCases` | Precios ideales positivos | Passed | XML |
| `StoreDatabase_AllDocumentedProductsHavePackageCost` | Costo de paquete positivo | Passed | XML |
| `StoreDatabase_AllDocumentedProductsHaveCategory` | Categoria, mueble y nodo validos | Passed | XML |
| `StoreDatabase_ProvisionalProductsHaveSafeFallbackAsset` | Placeholder seguro con prefab/icono | Passed | XML |
| `Products_Basic1UnlockedByDefaultAndOthersLockedByTree` | Basicos 1 inicio y bloqueo/desbloqueo por Arbol | Passed | XML |
| `ProductPricing_AllDocumentedProductsCanBeEvaluated` | Calculadora de precios soporta los 47 productos | Passed | XML |
| `Employees_All18CardsCanBeRepresented` | UI puede representar 18 tarjetas | Passed | XML |
| `Employees_RebuildDoesNotDuplicateCardsOrListeners` | Bootstrap no duplica panel/boton Empleados | Passed | XML |

## 22.13 Actividades tipo Gantt

| ID | Actividad | Requerimientos relacionados | Responsable | Fecha | Duracion estimada | Resultado |
| --- | --- | --- | --- | --- | --- | --- |
| F9-A01 | Validacion de base y rama | Base 9747bc8 | Codex | 2026-06-09 | 0.5h | OK |
| F9-A02 | Revision documental | Docs y reportes previos | Codex | 2026-06-09 | 1h | OK |
| F9-A03 | Auditoria visual del Arbol | UI RPG | Codex | 2026-06-09 | 1h | OK |
| F9-A04 | Rediseno visual del Arbol | Fondo, ramas, nodos, detalle | Codex | 2026-06-09 | 3h | OK |
| F9-A05 | Auditoria de pestana Empleados | UI empleados | Codex | 2026-06-09 | 0.5h | OK |
| F9-A06 | Rediseno visual de Empleados | Tarjetas/filtros/detalle | Codex | 2026-06-09 | 2h | OK |
| F9-A07 | Auditoria de productos existentes | Product_A-E | Codex | 2026-06-09 | 0.5h | OK |
| F9-A08 | Integracion de productos faltantes | 47 productos | Codex | 2026-06-09 | 2h | OK |
| F9-A09 | Asignacion de placeholders | Assets provisionales seguros | Codex | 2026-06-09 | 1h | OK |
| F9-A10 | Conexion productos-Arbol-compra-precios | ItemDatabase, Progress, Pricing | Codex | 2026-06-09 | 2h | OK |
| F9-A11 | Correccion barra superior | Navegacion computadora | Codex | 2026-06-09 | 0.5h | OK |
| F9-A12 | Ampliacion PlayMode tests | Regresion nueva | Codex | 2026-06-09 | 2h | OK |
| F9-A13 | Validacion build/tests | dotnet + Unity | Codex | 2026-06-09 | 1h | OK |
| F9-A14 | Revision staging | TMP/Animo/diff check | Codex | 2026-06-09 | 0.5h | OK preliminar |
| F9-A15 | Reporte y cierre | Evidencia honesta | Codex | 2026-06-09 | 1h | OK |

## 22.14 Validaciones

- `dotnet build .\SHOP_MASTER_FINAL.sln --no-restore`: compilacion correcta, 0 warnings, 0 errores.
- Unity PlayMode batchmode: ejecutado con `Codex_Fase9_UnityPlayMode.log`.
- Resultado PlayMode: 32 total, 32 passed, 0 failed, 0 skipped, `result=Passed`.
- XML: `Reportes/Codex_Fase9_PlayModeResults.xml`.
- Animo: busqueda en `Assets`, `Packages`, `ProjectSettings`, `Documentos` sin coincidencias ni rutas reintroducidas.
- TMP no stageados: `Bangers SDF.asset` y `Roboto-Bold SDF.asset` se mantienen fuera del stage.
- `git diff --check -- .`: sin errores de whitespace; solo warnings CRLF propios de Git en Windows.
- Play Mode manual real: pendiente. No hubo validacion humana visual interactiva.

## 22.15 Estado final

Estado final: AMARILLO.

Motivo: codigo y tests verdes, productos completos por datos reales integrados al flujo del asset, Arbol/Empleados cubiertos por PlayMode batchmode y sin errores criticos conocidos. No se marca VERDE porque falta Play Mode humano visual del Arbol y de Empleados dentro de la computadora; el XML no tiene ojos.

Pendientes reales:

- Validacion humana visual del Arbol en resolucion final.
- Validacion humana visual de la pestana Empleados.
- Sustituir placeholders por assets finales especificos de producto.
- QA manual de compra, delivery, colocacion y venta con productos provisionales en escena real.

## 22.16 Confirmaciones obligatorias

- Animo/ no fue reintroducido.
- TMP Bangers/Roboto no fueron stageados.
- No se creo Arbol paralelo.
- No se creo progreso paralelo.
- No se creo StoreDatabase paralelo.
- No se creo inventario paralelo.
- No se creo delivery paralelo.
- No se creo economia paralela.
- No se creo Canvas principal paralelo.
- Todos los productos documentados fueron integrados o documentados con causa exacta.
- Los productos sin asset final usan placeholder seguro.
- La pestana Empleados fue redisenada sin crear sistema paralelo.
- Los archivos modificados tienen inscripcion POMPIC cuando aplica.
- Excepciones de inscripcion: `.meta`, logs y XML generados por Unity no llevan comentario por formato/generacion; los assets TMP no fueron modificados por esta fase ni stageados.
