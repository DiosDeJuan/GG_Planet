# Reporte de Verificación de Requerimientos — ShopMaster
**Fecha:** 2026-05-20  
**Rama:** `copilot/fix-emprendedor-tree-system`  
**Auditor:** Copilot Agent (Fase 3)  
**Nota general:** Play Mode real NO ejecutado en este entorno (sin Unity Editor). Todos los estados son resultado de auditoría estática de código, assets y prefabs.

---

## Leyenda de estados

| Estado | Significado |
|--------|-------------|
| **VERIFICADO** | Probado en Play Mode o prueba funcional equivalente con evidencia |
| **PARCIAL** | Código/assets existen pero falta prueba en Play Mode, conexión de Inspector, o hay comportamiento incompleto |
| **NO_LISTO** | No existe o está completamente desconectado del flujo del juego |
| **BLOQUEADO** | Requiere Inspector, prefab, escena, NavMesh, Animator u otra dependencia en Unity Editor para poder ejecutarse |

---

## Matriz de Requerimientos Funcionales (RQF)

| ID | Descripción resumida | Archivos involucrados | Estado | Evidencia encontrada | Qué falta | Prioridad |
|----|---------------------|----------------------|--------|---------------------|-----------|-----------|
| **RQF1** | Acceder a computadora con catálogo de productos, desactivando bloqueados | `UIShopItemProduct.cs`, `EntrepreneurTreeProductUnlockAdapter.cs`, `UpgradesUIController.cs` | **PARCIAL** | UIShopItemProduct revisa árbol + licencia; desbloqueo overlay implementado; texto "Requires License License X" corregido a "Requiere: X" | Sin prueba Play Mode; no verificado que overlay se oculte correctamente en transición | Alta |
| **RQF2** | Mostrar mensaje con monto faltante si no hay fondos | `UIShopItemProduct.cs`, `UpgradeSystem.cs`, `StoreDatabase.cs` | **PARCIAL** | `UpgradeSystem.Purchase` muestra `UIGame.ShowMessage` con monto faltante; `StoreDatabase.CanPurchase` validado en código | Sin prueba Play Mode; no verificado que mensaje aparezca en pantalla | Alta |
| **RQF3** | Árbol del Emprendedor con nodos, costos en puntos, estados BLOQ/LISTO/OK | `EntrepreneurTreeManager.cs`, `UpgradesUIController.cs`, `NodeUI.cs`, `EntrepreneurTreeDefinition.cs` | **PARCIAL** | 120+ nodos definidos; UI procedimental con colores por estado; puntos gestionados en `EntrepreneurTreeManager`; guardado/carga implementado | Sin prueba Play Mode; el árbol genera UI procedimentalmente (sin Inspector references) — depende del prefab/escena | Alta |
| **RQF4** | Solo desbloquear si se cumplen prerequisitos; notificación si falta | `EntrepreneurTreeManager.cs`, `NodeUI.cs` | **PARCIAL** | `EntrepreneurTreeManager.TryUnlock()` valida prerequisites y puntos; muestra `UIGame.AddNotification` | Sin prueba Play Mode | Alta |
| **RQF5** | Expandir supermercado con plano de zonas | `SupermarketExpansionSystem.cs`, `ExpansionAppUIController.cs`, `ExpansionMapRenderer.cs` | **PARCIAL** | Sistema completo implementado; `ExpansionMapRenderer.Rebuild()` escala dinámicamente zonas al panel; zonas clicables; detalle derecho funcional | Sin prueba Play Mode; posibles referencias de Inspector no asignadas en escena | Alta |
| **RQF6** | Costo de expansión: $1,750 venta (16m²), $2,500 almacén (32m²) | `SupermarketExpansionSystem.cs` | **PARCIAL** | `SaleZoneCostCents=175000`, `StorageZoneCostCents=250000` configurados en código; validación de fondos implementada | Sin prueba Play Mode | Alta |
| **RQF7** | Mensaje de error al intentar comprar espacio sin fondos | `ExpansionAppUIController.cs`, `SupermarketExpansionSystem.cs` | **PARCIAL** | `BuySelectedZone()` verifica fondos y llama `UIGame.ShowMessage` con monto faltante | Sin prueba Play Mode | Alta |
| **RQF8** | Desbloquear empleados desde Árbol del Emprendedor | `EntrepreneurTreeEmployeeUnlockAdapter.cs`, `EntrepreneurEmployeeSystem.cs` | **PARCIAL** | Adapter suscrito a `onEmployeeNodeUnlocked`; actualiza set de `unlockedEmployees`; `EntrepreneurEmployeeSystem.IsEmployeeUnlocked()` consulta el adapter | Sin prueba Play Mode; depende de que árbol esté activo en escena | Alta |
| **RQF9** | Asignar roles Cajero/Surtidor desde app Empleados | `EmployeeAppUIController.cs`, `EntrepreneurEmployeeSystem.cs` | **PARCIAL** | `EmployeeAppUIController` crea UI procedimentalmente; botones Cajero/Surtidor; `TryAssignRole()` implementado; validación de dinero y estado | Sin prueba Play Mode; UI procedimental depende de que el panel esté disponible | Alta |
| **RQF10** | Control manual de seguridad sin niveles desbloqueados | `ShoplifterInteractable.cs`, `ShoplifterSystem.cs` | **PARCIAL** | `ShoplifterInteractable` permite interacción manual con ladrón; detiene ladrón y reintegra productos | Sin prueba Play Mode | Media |
| **RQF11** | 3 niveles de seguridad: 33%, 66%, 99% arresto automático | `EntrepreneurTreeSecurityAdapter.cs`, `ShoplifterSystem.cs` | **PARCIAL** | Probabilidades configuradas en `EntrepreneurTreeSecurityAdapter`; `ShoplifterSystem` consulta adapter para arresto auto | Sin prueba Play Mode | Media |
| **RQF12** | Notificación si intenta desbloquear seguridad sin prerequisites | `EntrepreneurTreeManager.cs` | **PARCIAL** | Mismo mecanismo que RQF4 | Sin prueba Play Mode | Media |
| **RQF13** | Ladrones con elementos visuales diferenciados; 1 ladrón por 25 clientes | `ShoplifterSystem.cs`, `ShoplifterAgent.cs` | **PARCIAL** | `ShoplifterSystem` genera ladrones; `ShoplifterAgent` con tipos Normal/Sospechoso/Experto; elementos visuales definidos en prefabs; ratio configurable | Sin prueba Play Mode; prefabs de ladrón deben asignarse en Inspector | Media |
| **RQF14** | Ladrón asigna valor de robo objetivo; rango $2.5–$6 básico, hasta $50/$200 | `ShoplifterAgent.cs`, `ShoplifterSystem.cs` | **PARCIAL** | Lógica de valor de robo implementada en `ShoplifterAgent.stealTargetValue`; escala con nivel | Sin prueba Play Mode | Media |
| **RQF15** | 3 tipos de ladrón con comportamientos diferenciados | `ShoplifterAgent.cs` (ShoplifterType enum) | **PARCIAL** | Normal, Sospechoso, Experto implementados en `ShoplifterSystem` | Sin prueba Play Mode; animaciones y visual no verificados | Media |
| **RQF16** | Dificultad escala con expansiones; hasta 6.5% del total diario | `ShoplifterSystem.cs` | **PARCIAL** | Frecuencia aumenta con compras de expansión en código | Sin prueba Play Mode | Baja |
| **RQF17** | Jugador intercepta ladrón manualmente; productos regresan; recompensa de puntos | `ShoplifterInteractable.cs` | **PARCIAL** | Interacción implementada; productos se reintegran; evento para puntos enviado | Sin prueba Play Mode | Media |
| **RQF18** | Niveles de seguridad en árbol: Cámaras 33%, Guardias 66%, Alarmas 99% | `EntrepreneurTreeDefinition.cs`, `EntrepreneurTreeSecurityAdapter.cs` | **PARCIAL** | Nodos `security_1/2/3` definidos; probabilidades asignadas | Sin prueba Play Mode | Media |
| **RQF19** | Alerta visual y sonora al detectar ladrón | `ShoplifterSystem.cs`, `UIGame.cs` | **PARCIAL** | `UIGame.AddNotification` llamado en detección/escape/arresto | Sin prueba Play Mode; alerta sonora depende de OneShotAudio prefab | Media |
| **RQF20** | Registrar robos en reporte diario | `StatsDatabase.cs`, `EntrepreneurTreeSaveIntegration.cs` | **PARCIAL** | `StatsDatabase` registra `robberyMoneyLost`; incluido en `BuildInventoryExpansionSummary` | Sin prueba Play Mode | Media |
| **RQF21** | Guardado automático al finalizar día; notificación visual | `SaveGameSystem.cs`, `EntrepreneurTreeSaveIntegration.cs` | **PARCIAL** | `SaveGameSystem` guarda al fin del día; `EntrepreneurTreeSaveIntegration` guarda árbol, empleados, expansiones; notificación `UIGame.AddNotification` implementada | Sin prueba Play Mode | Alta |
| **RQF22** | 50–75 clientes iniciales; +15% por espacio de venta comprado | `CustomerSystem.cs`, `ExpansionCustomerDemandAdapter.cs` | **PARCIAL** | `ExpansionCustomerDemandAdapter` aplica fórmula `baseRate × (1 + 0.15 × salesZones)` | Sin prueba Play Mode | Alta |
| **RQF23** | Clientes pagan con tarjeta: jugador acepta en caja | `CashDesk.cs`, `UICashDeskTerminal.cs` | **PARCIAL** | Sistema de pago de asset intacto; `CashDesk.TryStartAutomatedCheckout()` para cajeros | Sin prueba Play Mode | Alta |
| **RQF24** | Pago en efectivo: jugador entrega cambio manualmente | `CashDeskCash.cs` (asset) | **PARCIAL** | Sistema de pago en efectivo del asset existente | Sin prueba Play Mode | Alta |
| **RQF25 (gestión)** | App Empleados: ver desbloqueados, contratar, asignar rol | `EmployeeAppUIController.cs`, `EntrepreneurEmployeeSystem.cs` | **PARCIAL** | UI completa construida procedimentalmente; contratar/asignar rol funcional en código; validación de dinero implementada | Sin prueba Play Mode; depende de que el panel EMPLEADOS esté conectado en el prefab de computadora | Alta |
| **RQF26 (cajero)** | Cajeros atienden automáticamente; 0.5s/producto; tarjeta 1.5s, efectivo 2.5s | `EmployeeCashierCoordinator.cs`, `CashDesk.cs` | **PARCIAL** | `EmployeeCashierCoordinator` llama `TryStartAutomatedCheckout(speedMultiplier)` cada 0.75s; `CashDesk.AutomatedCheckout` procesa items con tiempos configurables | Sin prueba Play Mode; tiempo exacto por producto depende de configuración de `CashDesk` | Alta |
| **RQF27 (surtidor)** | Surtidores reponen automáticamente; anaqueles habilitan asignación; stock → anaquel | `EmployeeRestockCoordinator.cs`, `ShelfProductSlotSystem.cs` | **PARCIAL** | `EmployeeRestockCoordinator` cicla restocking; respeta `ShelfProductSlotSystem`; notificaciones de stock faltante implementadas | Sin prueba Play Mode; asignación de slots requiere UI en escena (ShelfSlot UI no verificado) | Alta |
| **RQF28 (precios UI)** | UI de precios con nombre, precio ideal, precio actual; min $0, max 300% | `PricingAppUIController.cs`, `ProductPricingSystem.cs` | **PARCIAL** | `PricingAppUIController` implementado con filas por producto; botones ±$0.10 y ±$0.50; validación de límites | Sin prueba Play Mode; no muestra probabilidad de compra explícitamente en la fila (solo text-based) | Alta |
| **RQF29 (ventas auto)** | Ventas descontan inventario; dinero al jugador; notificación si producto agotado | `Customer.cs` (asset), `CustomerCart.cs`, `StoreDatabase.cs` | **PARCIAL** | Asset maneja venta y descuento de inventario; `StatsDatabase` registra; notificación de bajo stock en `ProductInventorySystem` | Sin prueba Play Mode | Alta |
| **RQF34 (reporte diario)** | Reporte al final del día: ventas, gastos, salarios, robos, clientes perdidos | `StatsDatabase.cs`, `UIStats.cs` | **PARCIAL** | `BuildInventoryExpansionSummary` genera reporte con finanzas, clientes, stock, expansión, logros | Sin prueba Play Mode; salarios de empleados no calculados en reporte | Media |
| **RQF35 (alerta 7s)** | Alerta visual/sonora si cliente espera >7s; cliente abandona si no es atendido | `CustomerSystem.cs` (asset), `RQNF5` | **PARCIAL** | `Customer.waitDelay` maneja abandono; `UIGame.AddNotification` para alertas | Sin prueba Play Mode; tiempo exacto de 7s depende de `waitDelay` configurado en Customer prefabs | Media |
| **RQF36 (bloqueo árbol)** | No desbloquear sin cumplir prerequisitos; notificación con qué falta | `EntrepreneurTreeManager.cs` | **PARCIAL** | `TryUnlock()` verifica prerequisites; envía notificación con lista de nodos faltantes | Sin prueba Play Mode | Alta |

---

## Requerimientos de Probabilidad de Compra (RQF25–RQF28 precios)

| ID | Descripción | Archivos | Estado | Qué falta |
|----|-------------|---------|--------|-----------|
| **RQF25-PROB** | Fórmula: Pcompra = clamp(1 - k×(delta/Pideal), 0, 1) | `ProductPricingSystem.cs` | **PARCIAL** | Fórmula implementada con `k=probabilitySlope=0.1`; no verificada en Play Mode |
| **RQF26-EXTRA** | Pextra = clamp((delta/Pideal) × 0.10, 0, 1) cuando Pactual < Pideal | `ProductPricingSystem.cs` | **PARCIAL** | Implementada en `GetExtraPurchaseProbability()`; no verificada en Play Mode |
| **RQF27-UI** | Mostrar Pcompra y Pextra en interfaz de precios | `PricingAppUIController.cs` | **PARCIAL** | `PricingAppUIController` muestra porcentaje de compra en texto; Pextra mostrada; no verificado visualmente |
| **RQF28-CERO** | Permitir precio $0; Pcompra=100%, Pextra calculada normalmente | `ProductPricingSystem.cs` | **PARCIAL** | Validación de precio mínimo 0 implementada |

---

## Requerimientos No Funcionales (RQNF)

| ID | Descripción | Archivos | Estado | Qué falta |
|----|-------------|---------|--------|-----------|
| **RQNF1** | ≥30 FPS en hardware moderado | N/A (configuración Unity) | **NO_LISTO** | Sin prueba de rendimiento; no verificable sin Play Mode |
| **RQNF2** | Compatible con Windows 10+, 2 núcleos, 4GB RAM, 7GB almacenamiento | N/A | **NO_LISTO** | Sin build ni prueba de compatibilidad |
| **RQNF3** | Guardado automático al fin del día restaura todos los estados | `SaveGameSystem.cs`, `EntrepreneurTreeSaveIntegration.cs` | **PARCIAL** | Guardado implementado; restaura: dinero, día, inventario, productos colocados, precios, expansiones, empleados, árbol, seguridad | Sin prueba Play Mode de carga correcta |
| **RQNF4** | Sin pérdida de información durante guardado; conservar última versión válida | `EntrepreneurTreeSaveIntegration.cs` | **PARCIAL** | Try/catch con fallback a `LoadFromJSON(null)` implementado | Sin prueba Play Mode |
| **RQNF5** | Clientes no esperan >7s sin atención; abandono y cancelación registrados | `Customer.cs`, `CustomerSystem.cs` | **PARCIAL** | Asset maneja abandono; `StatsDatabase` registra clientes perdidos | Sin prueba Play Mode |
| **RQNF6** | Productos solo en mobiliario correcto según categoría | `ShelfProductSlotSystem.cs`, `EmployeeRestockCoordinator.cs` | **PARCIAL** | `ShelfProductSlotSystem.CanPlaceProductOnFurniture()` valida categoría; notificación implementada | Sin prueba Play Mode |
| **RQNF7** | Validar recursos antes de cualquier acción (expansión, compra, desbloqueo, contratación) | Múltiples sistemas | **PARCIAL** | Validaciones implementadas en todos los sistemas clave | Sin prueba Play Mode |
| **RQNF8** | Nodos del árbol conservan estado correcto durante la partida | `EntrepreneurTreeManager.cs`, `EntrepreneurTreeSaveIntegration.cs` | **PARCIAL** | Estado guardado/cargado en JSON | Sin prueba Play Mode |
| **RQNF9** | Mejora/empleado/producto/seguridad no se activa sin nodo correspondiente | Múltiples adapters | **PARCIAL** | Adapters verifican árbol antes de activar cualquier función | Sin prueba Play Mode |
| **RQNF10** | Probabilidad calculada en tiempo real al modificar precio | `ProductPricingSystem.cs`, `PricingAppUIController.cs` | **PARCIAL** | `ProductPricingSystem.onPriceChanged` event; `PricingAppUIController` suscrito | Sin prueba Play Mode |
| **RQNF11** | Interfaz de precios muestra precio ideal, precio actual, Pcompra y Pextra | `PricingAppUIController.cs` | **PARCIAL** | Implementado en filas de UI | Sin prueba Play Mode visual |
| **RQNF12** | Validar precio: mínimo $0, máximo 300% precio ideal | `ProductPricingSystem.cs` | **PARCIAL** | `SetPrice()` valida límites con clamp | Sin prueba Play Mode |
| **RQNF13** | Indicar al jugador que precios bajos = posible venta extra | `PricingAppUIController.cs` | **PARCIAL** | Texto de indicador en fila de precio | Sin prueba Play Mode visual |
| **RQNF14** | Indicar que precios altos reducen probabilidad | `PricingAppUIController.cs` | **PARCIAL** | Texto de indicador en fila de precio | Sin prueba Play Mode visual |
| **RQNF15** | Permitir $0 como estrategia; indicar que no genera ingresos directos | `ProductPricingSystem.cs`, `PricingAppUIController.cs` | **PARCIAL** | Permitido; warning no explícito en UI | Sin prueba Play Mode |
| **RQNF16** | UI clara e intuitiva para todas las funciones principales | Todos los UIController | **PARCIAL** | Todos los paneles construidos procedimentalmente con estilo consistente | Sin verificación visual en Play Mode |
| **RQNF17** | Notificaciones específicas por acción | `UIGame.cs`, múltiples sistemas | **PARCIAL** | Todos los sistemas usan `UIGame.AddNotification` o `ShowMessage` | Sin prueba Play Mode |
| **RQNF18** | Interfaces de computadora con estructura visual consistente | Todos los AppUIController | **PARCIAL** | Misma paleta de colores y layout en Orders, Achievements, Pricing, Employees, Expansion, Upgrades | Sin verificación visual |
| **RQNF19** | Opciones básicas de ajustes (resolución, volumen, brillo, controles) | N/A | **NO_LISTO** | No implementado | Requiere crear menú de ajustes |
| **RQNF20** | Instrucciones claras para mecánicas principales | N/A | **NO_LISTO** | No implementado | Requiere crear tutorial o pantalla de instrucciones |

---

## Resumen ejecutivo

### Lo que funciona en código (PARCIAL — sin prueba Play Mode)
| Sistema | Estado código | Qué falta para VERIFICADO |
|---------|--------------|--------------------------|
| Computadora (acceso, productos, compra) | ✅ Código completo | Play Mode + Inspector refs |
| Árbol del Emprendedor (nodos, puntos, desbloqueos) | ✅ Código completo | Play Mode |
| Sincronía Árbol→Licencias→Products | ✅ `EntrepreneurTreeLicenseBridge` + `ProductUnlockAdapter` | Play Mode |
| Expansión (mapa centrado, zonas, compra) | ✅ Código completo | Play Mode + Inspector refs |
| Empleados (contratar, rol, guardar) | ✅ Código completo | Play Mode |
| Cajero automático (CashDesk auto-checkout) | ✅ `EmployeeCashierCoordinator` | Play Mode + CashDesk refs |
| Surtidor automático (restocking por ciclo) | ✅ `EmployeeRestockCoordinator` | Play Mode + PlacementObject refs |
| NPC visual de empleados | ✅ `EmployeeNPCSpawner` creado | Play Mode + `employeePrefabs` asignados en Inspector |
| Precios y probabilidad de compra | ✅ `ProductPricingSystem` + `PricingAppUIController` | Play Mode |
| Guardado/carga de todo el estado | ✅ `EntrepreneurTreeSaveIntegration` + `SaveGameSystem` | Play Mode |
| Ladrones y seguridad | ✅ `ShoplifterSystem` + `ShoplifterAgent` | Play Mode + prefabs ladrón |
| Reporte diario | ✅ `StatsDatabase` + `UIStats` | Play Mode |
| Logros | ✅ `AchievementSystem` | Play Mode |
| Final Monopolio/Bancarrota | ✅ `GameEndSystem` | Play Mode |

### Lo que NO está listo
| Requerimiento | Estado | Razón |
|--------------|--------|-------|
| RQNF1 (rendimiento 30 FPS) | NO_LISTO | Sin prueba de rendimiento |
| RQNF2 (compatibilidad Windows) | NO_LISTO | Sin build |
| RQNF19 (opciones/ajustes) | NO_LISTO | No implementado |
| RQNF20 (instrucciones/tutorial) | NO_LISTO | No implementado |
| NPCs empleados visibles | BLOQUEADO | `EmployeeNPCSpawner.employeePrefabs` vacío hasta asignación en Inspector |
| Lineas blancas en árbol | CORREGIDO | `img.color = Color.clear` antes de `ConnectionLineUI.Initialize()` |

---

## Empleados — estado detallado

| Aspecto | Estado | Archivos | Notas |
|---------|--------|---------|-------|
| App UI (contratar, roles, estados) | PARCIAL | `EmployeeAppUIController.cs` | UI procedimental completa; no verificado en Play Mode |
| Desbloqueo desde árbol | PARCIAL | `EntrepreneurTreeEmployeeUnlockAdapter.cs` | Adapter suscrito a evento; funcional en código |
| Contratación con validación de dinero | PARCIAL | `EntrepreneurEmployeeSystem.TryHireEmployee()` | Validación implementada |
| Rol Cajero (automation) | PARCIAL | `EmployeeCashierCoordinator.cs` | Automation en `CashDesk.TryStartAutomatedCheckout()` |
| Rol Surtidor (automation) | PARCIAL | `EmployeeRestockCoordinator.cs` | Ciclo de restocking implementado con slot awareness |
| NPC visual en escena | BLOQUEADO | `EmployeeNPCSpawner.cs` | Nuevo componente creado; necesita `employeePrefabs` asignados en Inspector con Customer_A–E |
| Asignación de anaquel por surtidor | PARCIAL | `ShelfProductSlotSystem.cs` | Sistema existe; UI de asignación de ranura depende de escena |
| Guardado/carga de empleados | PARCIAL | `EntrepreneurTreeSaveIntegration.cs` | `EntrepreneurEmployeeSystem.SaveToJSON()` integrado |

---

## Expansión — estado detallado

| Aspecto | Estado | Notas |
|---------|--------|-------|
| Mapa escala y se centra | PARCIAL | `ExpansionMapRenderer.Rebuild()` calcula bounds dinámicamente; `ForceUpdateCanvases` solo si rect=0 |
| Zonas clicables | PARCIAL | `ExpansionZoneButtonUI` implementado |
| Panel detalle derecho | PARCIAL | `ExpansionAppUIController` muestra nombre, tipo, m², costo, estado, requisito |
| Botón Comprar Expansión activo solo si zona seleccionable | PARCIAL | Implementado en `ExpansionAppUIController.UpdateDetailPanel()` |
| Validación de fondos con monto faltante | PARCIAL | Implementado en `BuySelectedZone()` |
| Impacto en clientela (+15%) | PARCIAL | `ExpansionCustomerDemandAdapter` implementado |
| Guardado | PARCIAL | `SupermarketExpansionSystem.SaveToJSON()` integrado |

---

## Productos — estado detallado

| Aspecto | Estado | Notas |
|---------|--------|-------|
| Azúcar (Product_E) sin `requiredLicense` | CORREGIDO | `requiredLicense` eliminado en `Product_E.asset` |
| Básicos 1 desbloqueados desde inicio | PARCIAL | `product_basic_1` nodo auto-desbloqueado en `EnsureDefaultUnlockedNodes()`; `License_0.requiredLevel=0` |
| 41 nuevos productos creados | VERIFICADO (estático) | `Product_F.asset` a `Product_AT.asset` creados con prefabs reutilizados; registrados en `ItemDatabase.prefab` |
| Texto "License License X" corregido | CORREGIDO | `UIShopItemLicense.cs` y `UIShopItemProduct.cs` corregidos → "Requiere: X" |
| Desbloquear Básicos 2 habilita Harina-Aceite | PARCIAL | `EntrepreneurTreeProductUnlockAdapter` mapea `product_basic_2` → IDs 5–9; `LicenseBridge` sincroniza License_1 |
| Todos los productos comprables con descuento de dinero | PARCIAL | `UpgradeSystem.Purchase()` llama `StoreDatabase.AddRemoveMoney(-cost)` |

---

## Guardado — estado detallado

| Dato | Se guarda | Archivo |
|------|-----------|---------|
| Dinero | ✅ | `SaveGameSystem` (asset) |
| Día actual | ✅ | `SaveGameSystem` (asset) |
| Inventario/stock | ✅ | `SaveGameSystem` (asset) + `ProductInventorySystem` |
| Productos colocados en muebles | ✅ | `SaveGameSystem` (asset) |
| Precios modificados | ✅ | `ItemDatabase` (asset) guarda storePrice |
| Expansiones compradas | ✅ | `SupermarketExpansionSystem.SaveToJSON()` en `entrepreneurTree.dat` |
| Empleados contratados y roles | ✅ | `EntrepreneurEmployeeSystem.SaveToJSON()` en `entrepreneurTree.dat` |
| Nodos del árbol | ✅ | `EntrepreneurTreeManager.SaveToJSON()` en `entrepreneurTree.dat` |
| Puntos del árbol | ✅ | Incluidos en `EntrepreneurTreeManager.SaveToJSON()` |
| Licencias desbloqueadas | ✅ | Asset `ItemDatabase` guarda `isPurchased` |
| Seguridad | ✅ | `EntrepreneurTreeSaveIntegration` guarda nodos de seguridad |
| Robos/reportes históricos | ✅ | `ShoplifterSystem.SaveToJSON()` |
| Asignaciones de anaquel | ✅ | `ShelfProductSlotSystem` guarda en `shelfSlots.dat` |
| Posición NPC empleados | ❌ | `EmployeeNPCSpawner` re-spawnea desde datos; posición exacta no persiste |
| Salarios de empleados en reporte | ❌ | `StatsDatabase` no calcula salarios aún |
| Ajustes (volumen, resolución) | ❌ | RQNF19: no implementado |

---

## Checklist de pruebas manuales

### Escena: `Assets/StoreSimulator/Scenes/Game.unity`

| # | Acción | Resultado esperado |
|---|--------|--------------------|
| 1 | Abrir juego, entrar a computadora | Barra superior con todos los botones; ningún duplicado |
| 2 | PRODUCTS → BÁSICOS | Leche, Sal, Agua, Pasta, Azúcar comprables sin overlay de licencia |
| 3 | Intentar comprar Leche sin dinero | Mensaje "Te faltan $X.XX" |
| 4 | UPGRADES → Árbol | Árbol visible con nodos; Básicos 1 en estado OK; sin barras blancas |
| 5 | Pulsar nodo "Productos Básicos 2" con punto disponible | Nodo cambia a OK; notificación; License 1 se activa |
| 6 | PRODUCTS → BÁSICOS | Harina, Arroz, Frijoles, Pan, Aceite comprables |
| 7 | LICENSES | License "Productos Básicos 1" aparece como desbloqueada; License 1 = "Productos Básicos 2" |
| 8 | EMPLEADOS | Empleado #1 aparece BLOQUEADO |
| 9 | Desbloquear nodo empleado en árbol, volver a EMPLEADOS | Empleado #1 disponible para contratar |
| 10 | Contratar Empleado #1 | Dinero descontado; notificación verde |
| 11 | Asignar rol CAJERO | Botón activo; cajero ahora visible en EmployeeAppUI |
| 12 | Esperar hasta que llegue cliente a caja | Cajero procesa venta automáticamente |
| 13 | Contratar Empleado #2, asignar SURTIDOR | Surtidor activo |
| 14 | Verificar que mueble de venta tiene slot de asignación | Interfaz de slot disponible |
| 15 | EXPANDIR | Mapa centrado, ocupa panel izquierdo, leyenda legible |
| 16 | Seleccionar zona | Panel derecho con detalle: nombre, tipo, m², costo, estado |
| 17 | Pulsar "Comprar Expansión" con fondos | Expansión aplicada; metros actualizados; mapa actualizado |
| 18 | Cerrar y abrir computadora | Sin duplicados; estado persiste |
| 19 | Cerrar día (flip sign) | Reporte diario con ventas, clientes, robos, expansiones |
| 20 | Reiniciar play / cargar | Estado anterior restaurado: árbol, empleados, expansiones, precios |
| 21 | Revisar Console | 0 errores NullReferenceException relacionados con sistemas propios |

---

## Próximos 6 requerimientos recomendados (orden de prioridad)

| Prioridad | Requerimiento | Sistema | Razón técnica |
|-----------|--------------|---------|---------------|
| 1 | **Verificar Play Mode de Básicos 1** (RQF1, RQF2) | `UIShopItemProduct`, `UpgradeSystem` | Bloqueo fundamental de la demo |
| 2 | **Verificar Play Mode de Cajero** (RQF26) | `EmployeeCashierCoordinator`, `CashDesk` | Ciclo de venta completo depende de esto |
| 3 | **Verificar Play Mode de Surtidor + Anaquel** (RQF27) | `EmployeeRestockCoordinator`, `ShelfProductSlotSystem` | Flujo de stock → reposición visible |
| 4 | **Verificar Play Mode de Expansión** (RQF5–7) | `SupermarketExpansionSystem`, `ExpansionMapRenderer` | Demuestra integración mapa–mundo |
| 5 | **Asignar prefabs en Inspector y probar NPC empleado** | `EmployeeNPCSpawner` | Evidencia visual de empleados; requiere abrir escena y asignar Customer_A–E |
| 6 | **Instrucciones básicas + ajustes** (RQNF19–20) | Sin implementar | Últimos requerimientos no funcionales pendientes |

---

## Riesgos identificados

| Riesgo | Impacto | Mitigación |
|--------|---------|-----------|
| `EmployeeNPCSpawner.employeePrefabs` vacío | NPCs no aparecen visualmente | Asignar Customer_A–E en Inspector de la escena Game |
| `UpgradesUIController.linePrefab` null | Líneas del árbol creadas procedimentalmente (sin prefab) | Imagen de línea correctamente inicializada con `Color.clear` |
| Todas las nuevas UI son procedimentales | Requiere que el panel de computadora tenga los tabs correctos asignados | Verificar en escena Game que los tabs estén registrados |
| `UIGame.Instance` puede ser null en Intro scene | NullReferenceException en notificaciones fuera de Game scene | Guards implementados con `if (UIGame.Instance != null)` |
| 41 productos reutilizan prefabs A–E | Aspecto visual idéntico entre productos | Se documenta como temporal; requiere 3D assets únicos |
| Salarios de empleados no en reporte | Reporte diario incompleto (RQF34) | Agregar `employeeHireCost × hiredCount` a StatsDatabase |
| NavMesh no bakeado para NPC empleados | `EmployeeNPCSpawner` instancia NPC pero NavMeshAgent no puede navegar | NavMesh debe estar bakeado en escena Game |

---

*Generado automáticamente por Copilot Agent — Fase 3 — 2026-05-20*
