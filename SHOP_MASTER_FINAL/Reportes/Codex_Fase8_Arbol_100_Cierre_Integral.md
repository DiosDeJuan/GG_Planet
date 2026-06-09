<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 8 - Arbol del Emprendedor al 100%

## 18.1 Encabezado

- Fase: Codex Fase 8 - Arbol del Emprendedor al 100%.
- Rama base: `origin/codex/fase7-cierre-amarillo-playmode-release-candidate`.
- Commit base esperado: `62d6f73`.
- Commit base verificado: `62d6f731a84f369fdcbea9a4b9baa1e2600a176e`.
- Rama nueva: `codex/fase8-arbol-100-cierre-integral`.
- Responsable: Codex.
- Proyecto: `SHOP_MASTER_FINAL`.
- Repositorio: `DiosDeJuan/GG_Planet`.
- Fecha: 2026-06-09.

## 18.2 Documentos revisados

Documentos:

- `Documentos/NEW_Requerimientos.docx`: confirma computadora, Arbol, productos, empleados 1-18, seguridad 33/66/99, guardado, precios, reportes y logros.
- `Documentos/propuestas juanito (2).docx`: confirma Arbol RPG, productos por ramas, puntos por logros, empleados, seguridad, Cafeina, Carismatico y Huevo dorado secreto.
- `Documentos/Protocolo prpuesta Juanito (1).pdf`: confirma progresion por logros/puntos, inventario, empleados, seguridad, reportes y Final de Monopolio.
- `Documentos/Guia Gantt (1).pdf`: confirma formato de actividades, responsables, fechas y duraciones.

Reportes:

- `Reportes/Codex_Fase7_Cierre_Amarillo_PlayMode_ReleaseCandidate.md`.
- `Reportes/Codex_Fase7_PlayModeResults.xml`.
- `Reportes/Codex_Fase7_UnityBatchmode.log`.
- `Reportes/Codex_Fase6_QA_PlayMode_Estabilizacion_Final.md`.
- `Reportes/Codex_Fase5_Cierre_Espacios_Precios_Reportes_Finales_QA.md`.
- `Reportes/Codex_Fase4_1_Hotfix_Movimiento_Arbol_QA.md`.
- `Reportes/Codex_Fase4_2_Computadora_Licenses_Arbol_UI.md`.
- `Reportes/Codex_Fase4_3_Arbol_Visual_RPG_Nodos.md`.
- `Reportes/Codex_Fase4_4_Arbol_Requisitos_Logros_Puntos_QA.md`.

## 18.3 Definicion usada de Arbol al 100%

Se considero 100% tecnico cuando el Arbol mantiene datos completos, IDs estables, prerequisitos reales, costo correcto, estados UI, puntos por logros, bloqueo por prerequisito/puntos, efectos conectados donde existe sistema real, persistencia segura, tests PlayMode y reporte honesto de pendientes. No se considero 100% visual humano porque no hubo Play Mode manual interactivo.

Productos reales encontrados en assets: solo `Product_A` a `Product_E`, equivalentes a Leche, Sal, Agua, Pasta y Azucar. Los productos documentados de ramas posteriores se mantienen como nodos/hook de Arbol y mapeo por nombre, pero no se inventaron assets, prefabs ni StoreDatabase paralelos.

## 18.4 Matriz completa de nodos

| ID | Nombre | Categoria | Costo | Prereq doc | Prereq impl | Efecto esperado | Efecto conectado | UI | Estado final | Pendiente |
| --- | --- | --- | ---: | --- | --- | --- | --- | --- | --- | --- |
| productos_basicos_1 | Productos Basicos 1 | Producto | 0 | Inicio | Ninguno | Leche, Sal, Agua, Pasta, Azucar | Real en StoreDatabase/Products A-E | Si | OK | Ninguno |
| productos_basicos_2 | Productos Basicos 2 | Producto | 1 | Basicos 1 | productos_basicos_1 | Harina, Arroz, Frijoles, Pan, Aceite | Hook/mapeo por nombre; sin assets reales | Si | OK tecnico | Faltan assets reales |
| productos_basicos_3 | Productos Basicos 3 | Producto | 1 | Basicos 2 | productos_basicos_2 | Cafe, Huevo | Hook/mapeo por nombre; sin assets reales | Si | OK tecnico | Faltan assets reales |
| lacteos_1 | Lacteos 1 | Producto | 1 | Basicos 3 | productos_basicos_3 | Cheddar, Yogurt, Mantequilla | Hook/mapeo por nombre; sin assets reales | Si | OK tecnico | Faltan assets reales |
| lacteos_2 | Lacteos 2 | Producto | 1 | Lacteos 1 | lacteos_1 | Queso americano, Queso crema | Hook/mapeo por nombre; sin assets reales | Si | OK tecnico | Faltan assets reales |
| especias_1 | Especias 1 | Producto | 1 | Basicos 3 | productos_basicos_3 | Pimienta negra, Canela | Hook/mapeo por nombre; sin assets reales | Si | OK tecnico | Faltan assets reales |
| productos_frescos_1 | Productos Frescos 1 | Producto | 1 | Lacteos 1 | lacteos_1 | Manzana, Platano, Jitomate, Cebolla | Hook/mapeo por nombre; sin assets reales | Si | OK tecnico | Faltan assets reales |
| productos_frescos_2 | Productos Frescos 2 | Producto | 1 | Frescos 1 | productos_frescos_1 | Uvas, Zanahorias, Ajo | Hook/mapeo por nombre; sin assets reales | Si | OK tecnico | Faltan assets reales |
| productos_higiene | Productos de Higiene | Producto | 1 | Especias 1 | especias_1 | Jabon, Papel, Detergente, Pasta dental | Hook/mapeo por nombre; sin assets reales | Si | OK tecnico | Faltan assets reales |
| sodas | Sodas | Producto | 1 | Higiene | productos_higiene | Cola, Cola sin azucar, Limon | Hook/mapeo por nombre; sin assets reales | Si | OK tecnico | Faltan assets reales |
| proteina_1 | Proteina 1 | Producto | 1 | Empleado 5 | empleado_5 | Res, Pollo, Cerdo, Pescado | Nodo/hook; sin assets reales | Si | OK tecnico | Faltan assets reales |
| productos_lujo_1 | Productos de Lujo 1 | Producto | 1 | Sodas | sodas | Trufa, Chocolate, Caviar | Hook/mapeo por nombre; sin assets reales | Si | OK tecnico | Faltan assets reales |
| electrodomesticos_1 | Electrodomesticos 1 | Producto | 1 | Lujo 1 | productos_lujo_1 | Refrigerador, Microondas, Horno, Licuadora | Hook/mapeo por nombre; sin assets reales | Si | OK tecnico | Faltan assets reales |
| empleado_1 | Empleado 1 | Empleado | 1 | Especias 1 | especias_1 | Disponible en app Empleados | `EmployeeManager.TryHire` | Si | OK | QA manual NPC |
| empleado_2 | Empleado 2 | Empleado | 1 | Higiene | productos_higiene | Disponible en app Empleados | `EmployeeManager.TryHire` | Si | OK | QA manual NPC |
| empleado_3 | Empleado 3 | Empleado | 1 | Sodas | sodas | Disponible en app Empleados | `EmployeeManager.TryHire` | Si | OK | QA manual NPC |
| empleado_4 | Empleado 4 | Empleado | 1 | Lacteos 1 | lacteos_1 | Disponible en app Empleados | `EmployeeManager.TryHire` | Si | OK | QA manual NPC |
| empleado_5 | Empleado 5 | Empleado | 1 | Lacteos 1 | lacteos_1 | Disponible en app Empleados | `EmployeeManager.TryHire` | Si | OK | QA manual NPC |
| empleado_6 | Empleado 6 | Empleado | 1 | Especias 1 | especias_1 | Disponible en app Empleados | `EmployeeManager.TryHire` | Si | OK | QA manual NPC |
| empleado_7 | Empleado 7 | Empleado | 1 | Empleado 5 | empleado_5 | Habilita Seguridad 1 | `EmployeeManager` + prereq | Si | OK | QA manual NPC |
| empleado_8 | Empleado 8 | Empleado | 1 | Sodas | sodas | Habilita Seguridad 2 | `EmployeeManager` + prereq | Si | OK | QA manual NPC |
| empleado_9 | Empleado 9 | Empleado | 1 | Higiene | productos_higiene | Disponible en app Empleados | `EmployeeManager.TryHire` | Si | OK | QA manual NPC |
| empleado_10 | Empleado 10 | Empleado | 1 | Empleado 1 | empleado_1 | Disponible en app Empleados | `EmployeeManager.TryHire` | Si | OK | QA manual NPC |
| empleado_11 | Empleado 11 | Empleado | 1 | Seguridad 1 | seguridad_1 | Disponible en app Empleados | `EmployeeManager.TryHire` | Si | OK | QA manual NPC |
| empleado_12 | Empleado 12 | Empleado | 1 | Empleado 13 | empleado_13 | Disponible en app Empleados | `EmployeeManager.TryHire` | Si | OK | QA manual NPC |
| empleado_13 | Empleado 13 | Empleado | 1 | Lujo 1 | productos_lujo_1 | Habilita Empleado 12 | `EmployeeManager` + prereq | Si | OK | QA manual NPC |
| empleado_14 | Empleado 14 | Empleado | 1 | Electrodomesticos 1 | electrodomesticos_1 | Habilita Seguridad 3 | `EmployeeManager` + prereq | Si | OK | QA manual NPC |
| empleado_15 | Empleado 15 | Empleado | 1 | Seguridad 2 | seguridad_2 | Habilita Carismatico | `EmployeeManager` + prereq | Si | OK | QA manual NPC |
| empleado_16 | Empleado 16 | Empleado | 1 | Proteina 1 | proteina_1 | Disponible en app Empleados | `EmployeeManager.TryHire` | Si | OK | QA manual NPC |
| empleado_17 | Empleado 17 | Empleado | 1 | Frescos 2 | productos_frescos_2 | Disponible en app Empleados | `EmployeeManager.TryHire` | Si | OK | QA manual NPC |
| empleado_18 | Empleado 18 | Empleado | 1 | Seguridad 3 | seguridad_3 | Disponible en app Empleados | `EmployeeManager.TryHire` | Si | OK | QA manual NPC |
| seguridad_1 | Seguridad Nivel 1 | Seguridad | 1 | Empleado 7 | empleado_7 | 33% arresto automatico | `EntrepreneurProgress.SecurityLevel`, `SecurityManager` | Si | OK | QA robo real |
| seguridad_2 | Seguridad Nivel 2 | Seguridad | 1 | Empleado 8 | empleado_8 | 66% arresto automatico | `EntrepreneurProgress.SecurityLevel`, `SecurityManager` | Si | OK | QA robo real |
| seguridad_3 | Seguridad Nivel 3 | Seguridad | 1 | Empleado 14 | empleado_14 | 99% arresto automatico | `EntrepreneurProgress.SecurityLevel`, `SecurityManager` | Si | OK | QA robo real |
| mejora_cafeina | Cafeina | Mejora | 1 | Frescos 2 | productos_frescos_2 | Empleados 10% mas rapidos | `EmployeeWorkSpeedMultiplier`, cajero/surtidor | Si | OK | QA manual ritmo |
| mejora_carismatico | Carismatico | Mejora | 1 | Empleado 15 | empleado_15 | Cajeros +5% ventas | `CashierRevenueMultiplier`, `CashDesk` | Si | OK | QA venta con cajero |

## 18.5 Matriz de logros

| ID | Nombre | Condicion documental | Metrica real | Fuente | Recompensa | Cobro unico | Persistencia | Estado | Pendiente |
| --- | --- | --- | --- | --- | ---: | --- | --- | --- | --- |
| primeras_ventas | Primeras Ventas | 15 ventas | Si | ventas caja/self | 1 | Si | Si | OK | Ninguno |
| venta_rapida | Venta Rapida | 10 ventas en 1 min | Si | tiempos de venta | 1 | Si | Si | OK | Ninguno |
| ingresos_1 | Objetivo Ingresos 1 | $5,000 | Si | totalIncome | 1 | Si | Si | OK | Ninguno |
| ingresos_2 | Objetivo Ingresos 2 | $10,000 | Si | totalIncome | 1 | Si | Si | OK | Ninguno |
| ingresos_3 | Objetivo Ingresos 3 | $12,000 | Si | totalIncome | 1 | Si | Si | OK | Ninguno |
| ingresos_4 | Objetivo Ingresos 4 | $15,000 | Si | totalIncome | 1 | Si | Si | OK | Ninguno |
| ingresos_5 | Objetivo Ingresos 5 | $17,000 | Si | totalIncome | 1 | Si | Si | OK | Ninguno |
| ingresos_6 | Objetivo Ingresos 6 | $20,000 | Si | totalIncome | 1 | Si | Si | OK | Ninguno |
| ingresos_7 | Objetivo Ingresos 7 | $25,000 | Si | totalIncome | 1 | Si | Si | OK | Ninguno |
| ingresos_8 | Objetivo Ingresos 8 | $35,000 | Si | totalIncome | 1 | Si | Si | OK | Ninguno |
| ingresos_9 | Objetivo Ingresos 9 | $50,000 | Si | totalIncome | 1 | Si | Si | OK | Ninguno |
| lluvia_dinero | Lluvia de Dinero | $100,000 | Si | totalIncome | 1 | Si | Si | OK | Ninguno |
| ventas_diarias_1 | Ventas Diarias 1 | $1,000/dia | Si | best/currentDayIncome | 1 | Si | Si | OK | Ninguno |
| ventas_diarias_2 | Ventas Diarias 2 | $1,500/dia | Si | best/currentDayIncome | 1 | Si | Si | OK | Ninguno |
| ventas_diarias_3 | Ventas Diarias 3 | $5,000/dia | Si | best/currentDayIncome | 1 | Si | Si | OK | Ninguno |
| ventas_diarias_4 | Ventas Diarias 4 | $10,000/dia | Si | best/currentDayIncome | 1 | Si | Si | OK | Ninguno |
| ventas_diarias_5 | Ventas Diarias 5 | $15,000/dia | Si | best/currentDayIncome | 1 | Si | Si | OK | Ninguno |
| ventas_diarias_6 | Ventas Diarias 6 | $20,000/dia | Si | best/currentDayIncome | 1 | Si | Si | OK | Ninguno |
| cliente_lujo | Cliente de Lujo | vender lujo | Hook por producto | soldItems + nodo | 1 | Si | Si | OK tecnico | Falta asset lujo real |
| primer_empleado | Primer Empleado | contratar 1 | Si | EmployeeManager | 1 | Si | Si | OK | QA manual |
| supermercado_crecimiento | Supermercado Crecimiento | 300 m2 | Si | ShopExpansionManager | 1 | Si | Si | OK | QA manual |
| imperialista | Imperialista | tamano maximo | Si | ShopExpansionManager | 1 | Si | Si | OK | QA manual |
| surtido_completo | Surtido Completo | todos basicos | Si | nodos basicos 1-3 | 1 | Si | Si | OK | Ninguno |
| dedicado | Dedicado | jugar 3 dias | Si | DayCycleSystem | 1 | Si | Si | OK | QA dias |
| fiel | Fiel | jugar 5 dias | Si | DayCycleSystem | 1 | Si | Si | OK | QA dias |
| emprendedor | Emprendedor | jugar 7 dias | Si | DayCycleSystem | 1 | Si | Si | OK | QA dias |
| huevo_dorado | Huevo dorado | creador cliente, Arbol completo | No | Hook formal | 1 | Si | Si | Hook | Falta cliente creador |
| red_seguridad | Red de Seguridad | seguridad completa | Si | nodos seguridad | 1 | Si | Si | OK | QA robo |
| almacenamiento_maximizado | Almacenamiento Maximizado | storage max | Si | ShopExpansionManager | 1 | Si | Si | OK | QA manual |
| eficiencia_maximo | Eficiencia al Maximo | estantes abastecidos 1 semana | No | Hook formal | 1 | Si | Si | Hook nuevo | Falta metrica semanal estable |
| limpieza_impecable | Limpieza Impecable | limpieza 1 semana | No | Hook formal | 1 | Si | Si | Hook nuevo | Falta sistema suciedad/limpieza |
| precio_perfecto | Precio Perfecto | precios sin quejas 1 semana | No | Hook formal | 1 | Si | Si | Hook | Falta metrica semanal |
| lindo_hogar | Lindo hogar | vender electrodomestico | Hook por producto | soldItems + nodo | 1 | Si | Si | OK tecnico | Falta asset electrodomestico real |
| maximo_empleo | Maximo Empleo | 18 empleados con rol | Si | EmployeeManager | 1 | Si | Si | OK | QA manual |
| optimizacion_total | Optimizacion Total | todas mejoras | Si | nodos mejoras | 1 | Si | Si | OK | Ninguno |
| bajo_presion | Bajo Presion | $500/dia sin empleados | Si | currentDayIncome + EmployeeManager | 1 | Si | Si | OK | QA manual |
| optimista | Optimista | todas mejoras | Si | nodos mejoras | 1 | Si | Si | OK | Ninguno |
| paciente | Paciente | queja por precios | Si | Customer.RegisterPriceComplaint | 1 | Si | Si | OK | QA manual |
| donador | Donador | 5 productos a $0 | Si | soldItems fixedPrice | 1 | Si | Si | OK | QA manual |
| batman | Batman | arrestar ratero | Si | SecurityManager manual | 1 | Si | Si | OK | QA robo |
| rapidez | Rapidez | primera caja registradora | No | Hook formal | 1 | Si | Si | Hook | Falta evento compra caja separado |
| perezoso | Perezoso | no correr 1 dia | No | Hook formal | 1 | Si | Si | Hook | Falta tracking correr |
| arbol_completo | Arbol Completo | todos los nodos | Si | EntrepreneurProgress | 0 | Si | Si | Simbolico | Ninguno |

## 18.6 QA funcional

### Computadora/ARBOL

- Revisado: `UIShopDesktop`, `EntrepreneurTreeUIBootstrap`, `ContentArea/Licenses`, boton `LICENSES` -> `ARBOL`.
- Bug encontrado: `Ensure` podia agregar listener runtime repetido si se invocaba mas de una vez.
- Bug corregido: `EntrepreneurTreeNavigationBinding` idempotente.
- Evidencia: `EntrepreneurTree_UIBootstrap_DoesNotCreateMainCanvas`, `EntrepreneurTree_RebuildDoesNotDuplicateNodesOrListeners`.
- Pendiente: Play Mode humano visual.

### Datos del Arbol

- Revisado: IDs, costos, prerequisitos, categorias y beneficios.
- Bugs encontrados: ninguno critico en nodos.
- Evidencia: `EntrepreneurTree_HasAllDocumentedCoreNodes`, `EmployeesHaveDocumentedPrerequisites`, `SecurityLevels...`, `Improvements...`.
- Pendiente: assets reales para productos documentados posteriores a Basicos 1.

### UI visual RPG

- Revisado: layout, scroll, conexiones, panel detalle, estados, puntos y logros.
- Bug corregido: listener idempotente del tab.
- Evidencia: test de reconstruccion sin duplicar nodos/listener.
- Pendiente: validacion visual humana en resolucion final.

### Productos

- Revisado: `EntrepreneurProgress.IsProductUnlocked`, `ItemDatabase`, `DeliverySystem`, `UIShopItemProduct`.
- Bugs: no se inventaron productos faltantes.
- Evidencia: `UnlockEffectsGateProductsEmployeesSecurity`.
- Pendiente: StoreDatabase/prefabs para productos documentados no presentes.

### Empleados

- Revisado: `EmployeeManager`, `UIEmployeesPanel`, roles, save/load.
- Bugs: ninguno nuevo.
- Evidencia: prerequisitos 1-18 y test de hire bloqueado/desbloqueado.
- Pendiente: prueba visual NPC/roles en escena.

### Seguridad

- Revisado: `SecurityManager`, `SecurityLevel`, stats y prerequisitos.
- Bugs: ninguno nuevo.
- Evidencia: 33/66/99 probado por PlayMode.
- Pendiente: provocar robo real en Play Mode humano.

### Mejoras

- Revisado: Cafeina y Carismatico.
- Bugs: ninguno nuevo.
- Evidencia: multiplicadores 1.1 y 1.05 probados.
- Pendiente: observar ritmo/ventas con cajero en partida.

### Logros

- Revisado: definiciones, hooks, cobro unico, persistencia.
- Bugs encontrados: faltaban `eficiencia_maximo` y `limpieza_impecable`.
- Bugs corregidos: agregados como hooks formales no auto-completables.
- Evidencia: `EntrepreneurAchievements_HasAllDocumentedDefinitions` y `RewardOnlyOnce`.
- Pendiente: metricas reales para hooks.

### Puntos

- Revisado: `AvailablePoints`, consumo, mensajes.
- Evidencia: `UnlockRequiresProgressPoint`, `UnlockConsumesOnePoint`, `RewardOnlyOnce`.
- Pendiente: ninguno tecnico conocido.

### Save/load

- Revisado: `EntrepreneurProgress.SaveToJSON/LoadFromJSON` y logros.
- Evidencia: `SaveLoadPersistsUnlocksAndPoints`, `SaveLoadDoesNotDuplicateRewards`.
- Pendiente: save/load manual de partida completa.

### GameEndingService

- Revisado: Monopoly depende de `EntrepreneurProgress.IsTreeComplete()` y todos los espacios comprados.
- Bugs: ninguno nuevo.
- Evidencia: build verde y revision de codigo.
- Pendiente: UI/pantalla final dedicada fuera de alcance.

### Tests PlayMode

- Resultado final: 20/20 passed.
- Evidencia: `Reportes/Codex_Fase8_Arbol100_PlayModeResults.xml`.

## 18.7 Bugs encontrados

| ID | Modulo | Descripcion | Severidad | Causa raiz | Archivo(s) | Estado |
| --- | --- | --- | --- | --- | --- | --- |
| F8-BUG-001 | ARBOL UI | `Ensure` podia duplicar listener runtime en boton ARBOL | Media | Listener anonimo sin binding idempotente | `EntrepreneurTreeUIBootstrap.cs` | Corregido |
| F8-BUG-002 | Logros | Faltaban logros documentados `Eficiencia al Maximo` y `Limpieza Impecable` | Media | Catalogo previo no incluia hooks sin metrica | `EntrepreneurAchievementDefinition.cs` | Corregido como hook |
| F8-PEND-001 | Productos | Solo existen assets reales para Basicos 1 | Media | StoreDatabase contiene Product_A-E | ScriptableObjects/Products | Pendiente real |
| F8-PEND-002 | QA manual | No hubo Play Mode humano | Media | Entorno batchmode automatizado | N/A | Pendiente Isaac |

## 18.8 Bugs corregidos

| ID | Correccion | Archivo(s) | Riesgo | Validacion |
| --- | --- | --- | --- | --- |
| F8-BUG-001 | Se agrego `EntrepreneurTreeNavigationBinding` para reconfigurar el listener propio sin duplicarlo | `EntrepreneurTreeUIBootstrap.cs` | Bajo | PlayMode `RebuildDoesNotDuplicateNodesOrListeners` |
| F8-BUG-002 | Se agregaron `eficiencia_maximo` y `limpieza_impecable` como hooks persistibles | `EntrepreneurAchievementDefinition.cs` | Bajo | PlayMode `EntrepreneurAchievements_HasAllDocumentedDefinitions` |

## 18.9 Tests agregados

| Test | Sistema cubierto | Que valida | Resultado | Evidencia |
| --- | --- | --- | --- | --- |
| EntrepreneurTree_HasAllDocumentedCoreNodes | Nodos | 36 nodos y categorias | Passed | XML Fase 8 |
| EntrepreneurTree_BasicProducts1UnlockedByDefault | Defaults | Basicos 1 desbloqueado | Passed | XML Fase 8 |
| EntrepreneurTree_EmployeesHaveDocumentedPrerequisites | Empleados | prerequisitos 1-18 | Passed | XML Fase 8 |
| EntrepreneurTree_SecurityLevelsHaveDocumentedPrerequisitesAndValues | Seguridad | prereq y niveles 33/66/99 | Passed | XML Fase 8 |
| EntrepreneurTree_ImprovementsHaveDocumentedPrerequisites | Mejoras | Cafeina/Carismatico y multiplicadores | Passed | XML Fase 8 |
| EntrepreneurTree_UnlockRequiresPrerequisites | Unlock | bloqueo por prereq | Passed | XML Fase 8 |
| EntrepreneurTree_UnlockRequiresProgressPoint | Puntos | bloqueo sin puntos | Passed | XML Fase 8 |
| EntrepreneurTree_UnlockConsumesOnePoint | Puntos | consumo de 1 punto | Passed | XML Fase 8 |
| EntrepreneurAchievements_RewardOnlyOnce | Logros | no duplica recompensa | Passed | XML Fase 8 |
| EntrepreneurAchievements_HasAllDocumentedDefinitions | Logros | todos los logros documentados + hooks | Passed | XML Fase 8 |
| EntrepreneurTree_SaveLoadPersistsUnlocksAndPoints | Save/load | nodos y puntos persisten | Passed | XML Fase 8 |
| EntrepreneurTree_SaveLoadDoesNotDuplicateRewards | Save/load | carga no duplica puntos | Passed | XML Fase 8 |
| EntrepreneurTree_UnlockEffectsGateProductsEmployeesSecurity | Efectos | producto/empleado/seguridad gateados | Passed | XML Fase 8 |
| EntrepreneurTree_UIBootstrap_DoesNotCreateMainCanvas | UI | no Canvas nuevo | Passed | XML Fase 8 |
| EntrepreneurTree_RebuildDoesNotDuplicateNodesOrListeners | UI | no duplica nodos/listener | Passed | XML Fase 8 |

## 18.10 Actividades tipo Gantt

| ID | Actividad | Requerimientos relacionados | Responsable | Fecha | Duracion estimada | Resultado |
| --- | --- | --- | --- | --- | ---: | --- |
| G1 | Preparacion de rama y base | Todos | Codex | 2026-06-09 | 0.5 h | Base 62d6f73 verificada |
| G2 | Revision documental del Arbol | RQF3, logros, Gantt | Codex | 2026-06-09 | 1.0 h | Documentos revisados |
| G3 | Auditoria de nodos | Productos, empleados, seguridad, mejoras | Codex | 2026-06-09 | 1.0 h | 36 nodos verificados |
| G4 | Auditoria de prerequisitos | RQF8, RQF11, RQF12 | Codex | 2026-06-09 | 1.0 h | Prereq validados |
| G5 | Auditoria de UI visual | RQF3 | Codex | 2026-06-09 | 1.0 h | Listener corregido |
| G6 | Auditoria de logros | Logros documentados | Codex | 2026-06-09 | 1.0 h | 2 hooks agregados |
| G7 | Auditoria de puntos | Progreso | Codex | 2026-06-09 | 0.5 h | Consumo/cobro probado |
| G8 | Auditoria de efectos reales | Productos/empleados/seguridad/mejoras | Codex | 2026-06-09 | 1.0 h | Gate central probado |
| G9 | Auditoria de save/load | RQF21 | Codex | 2026-06-09 | 1.0 h | Persistencia probada |
| G10 | Correccion nodos/prerequisitos | Arbol | Codex | 2026-06-09 | 0.5 h | Sin cambios de nodos |
| G11 | Correccion UI | Arbol UI | Codex | 2026-06-09 | 0.5 h | Binding idempotente |
| G12 | Correccion logros/puntos | Logros | Codex | 2026-06-09 | 0.5 h | Hooks agregados |
| G13 | Correccion persistencia | Save/load | Codex | 2026-06-09 | 0.5 h | Sin cambio necesario |
| G14 | Ampliacion PlayMode tests | QA | Codex | 2026-06-09 | 1.5 h | 15 tests nuevos |
| G15 | Validacion build/tests | QA | Codex | 2026-06-09 | 1.0 h | Build y PlayMode verdes |
| G16 | Reporte y cierre | Evidencia | Codex | 2026-06-09 | 1.0 h | Reporte Fase 8 |

## 18.11 Validaciones

- Base git: `origin/codex/fase7-cierre-amarillo-playmode-release-candidate` verificada en `62d6f731a84f369fdcbea9a4b9baa1e2600a176e`.
- Status inicial: solo TMP preexistentes modificados dentro del proyecto.
- `dotnet build .\SHOP_MASTER_FINAL.sln --no-restore`: compilacion correcta, 0 advertencias, 0 errores.
- Unity PlayMode batchmode: `Test run completed. Exiting with code 0 (Ok). Run completed.`
- PlayMode tests: 20 total, 20 passed, 0 failed.
- Animo: sin coincidencias funcionales en `Assets`, `Packages`, `ProjectSettings`, `Documentos`; sin rutas dentro de `SHOP_MASTER_FINAL`.
- TMP: `Bangers SDF.asset` y `Roboto-Bold SDF.asset` permanecen fuera del stage.
- Play Mode manual: no ejecutado; pendiente real para Isaac.

## 18.12 Estado final

**AMARILLO**.

Justificacion:

- Arbol completo por codigo/tests.
- Logros documentados presentes; hooks honestos donde falta metrica real.
- `dotnet build` verde.
- Unity PlayMode batchmode verde.
- No hay errores criticos conocidos.
- Falta Play Mode manual humano para declarar VERDE.

## 18.13 Confirmaciones obligatorias

- `Animo/` no fue reintroducido.
- No se stagearon `Bangers SDF.asset` ni `Roboto-Bold SDF.asset`.
- No se creo Canvas principal nuevo.
- No se creo Arbol paralelo.
- No se creo progreso paralelo.
- No se creo sistema de logros paralelo.
- No se creo inventario paralelo.
- No se creo delivery paralelo.
- No se creo economia paralela.
- No se trabajo menu/ajustes/build final.
- No se sobrescribieron reportes anteriores.
- Los archivos modificados tienen inscripcion POMPIC cuando aplica.
- Excepciones de inscripcion: XML de Unity Test Runner y log Unity generados; no se comentan para no romper evidencia generada.

## 18.14 Pendientes reales

- Play Mode manual completo por Isaac.
- Validacion visual final del Arbol en resolucion de entrega.
- Productos documentados posteriores a Basicos 1 sin StoreDatabase/prefab real.
- Hook de Huevo dorado pendiente por falta de cliente creador.
- Hook de Limpieza Impecable pendiente por falta de sistema de suciedad/limpieza.
- Hook de Eficiencia al Maximo pendiente por falta de metrica semanal estable de estantes abastecidos.
- Hook de Rapidez pendiente por falta de evento especifico de compra de caja registradora.
- Hook de Perezoso pendiente por falta de tracking diario de correr.
- Hook de Precio Perfecto pendiente por falta de metrica semanal de precios sin quejas.
