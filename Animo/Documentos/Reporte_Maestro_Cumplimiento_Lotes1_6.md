# Reporte Maestro de Cumplimiento - Lotes 1 al 6 - ShopMaster

## 1. Resumen ejecutivo
- Fecha: 2026-05-19
- Rama: main
- Commit inicial: e18ab80
- Commit final si aplica: e18ab80, con cambios locales sin confirmar de los lotes previos y de auditorias recientes.
- Total de requerimientos revisados: 33
- Total LISTO / VERIFICADO: 0
- Total NO LISTO / FALTA CORREGIR: 33
- Porcentaje real de cumplimiento: 0%
- Resultado batchmode: ejecutado al final de esta auditoria; Unity compilo sin errores C# ni excepciones criticas buscadas en log.
- Play Mode real: no hubo Play Mode interactivo real ni prueba automatizada equivalente que cubra gameplay. Por la regla del prompt maestro, ningun requerimiento puede quedar en verde.
- Advertencia Lote 4: `Documentos/Reporte_Verificacion_Lote4_Ladrones_Robo_Captura.md` no existe. El estado del Lote 4 se reconstruyo revisando scripts reales de ladrones, robo, escalado y captura.
- Documentos base: `Reporte_Cumplimiento_Requerimientos.md` y `Matriz_Cumplimiento_Requerimientos.csv` fueron leidos. `propuestas juanito (2).docx` pudo extraerse parcialmente. `NEW_Requerimientos.docx` estaba bloqueado por otro proceso al intentar abrirlo directamente; se uso la extraccion consolidada ya presente en el reporte de cumplimiento previo. Los PDF base fueron considerados por la sintesis previa del reporte de cumplimiento.

## 2. Criterio de evaluación usado
- LISTO / VERIFICADO significa que el requisito compila sin errores C#, fue probado en Play Mode real o con una prueba automatizada equivalente documentada, cumple el comportamiento pedido y no tiene bug critico abierto relacionado.
- NO LISTO / FALTA CORREGIR significa que falta prueba real, falta conexion visual/escena, hay bug funcional, el sistema solo fue revisado estaticamente o solo compila.
- No se usa el estado intermedio solicitado en reportes anteriores porque el prompt maestro lo prohibe: si no hay evidencia ejecutada de gameplay, el requisito no esta listo.
- Evidencia exigida: archivo/clase/metodo, prueba ejecutada, resultado observable y faltante exacto cuando no queda listo.

## 3. Tabla general de cumplimiento

| Lote | ID | Requerimiento completo | Estado | Evidencia | Qué se probó | Qué falta si no está listo | Archivos relacionados |
|---|---|---|---|---|---|---|---|
| 1 | RQF28-PRICE | El sistema debera permitir que el jugador modifique el precio de venta de cada producto individual mediante una UI interactiva. La interfaz debera mostrar nombre del producto, precio ideal, precio actual, limite minimo de $0.00, limite maximo de 300% sobre precio ideal y boton de confirmacion. | NO LISTO / FALTA CORREGIR | `PricingAppUIController.ConfirmPrice`, `ProductPricingSystem.TrySetCurrentPrice`, bootstrap de pricing existente. | Revision estatica y batchmode. | Ejecutar Play Mode: abrir computadora, app Precios, editar ideal/alto/invalido/$0.00, confirmar persistencia y efecto real. | `Assets/UI/Computer/Pricing/PricingAppUIController.cs`; `Assets/Systems/Pricing/ProductPricingSystem.cs`; `Assets/Systems/EntrepreneurTree/EntrepreneurTreeUIBootstrap.cs` |
| 1 | RQF25-PRICE | El sistema debe calcular la probabilidad de compra de un producto en funcion del precio establecido por el jugador. | NO LISTO / FALTA CORREGIR | `Customer` llama `ProductPurchaseProbabilityAdapter.ShouldCustomerBuy`; adapter consulta `ProductPricingSystem`. | Revision estatica y batchmode. | Observar clientes reales comprando/rechazando con precio ideal, alto y bajo. | `Assets/StoreSimulator/Scripts/Customer.cs`; `Assets/Systems/Pricing/ProductPurchaseProbabilityAdapter.cs`; `Assets/Systems/Pricing/ProductPricingSystem.cs` |
| 1 | RQF26-PRICE | Cuando el precio actual es menor que el precio ideal, el sistema debe calcular la probabilidad de que el cliente compre un producto adicional. | NO LISTO / FALTA CORREGIR | `ProductPricingSystem.GetExtraPurchaseProbabilityForPrice`; `Customer` llama `ShouldCustomerBuyExtra` y valida stock. | Revision estatica y batchmode. | Probar en Play Mode compra extra con stock suficiente, stock bajo y stock agotado, sin productos fantasma. | `Customer.cs`; `CustomerCart.cs`; `ProductInventorySystem.cs`; `ProductPricingSystem.cs` |
| 1 | RQF27-PRICE | El sistema debe mostrar en la interfaz de precios tanto la probabilidad de compra como la probabilidad de compra extra. | NO LISTO / FALTA CORREGIR | `PricingAppUIController.RefreshRow` muestra porcentajes calculados sobre precio pendiente. | Revision estatica y batchmode. | Abrir UI en Play Mode y confirmar refresco inmediato, legibilidad y valores con precio alto/bajo/$0.00. | `Assets/UI/Computer/Pricing/PricingAppUIController.cs` |
| 1 | RQF28-PRICEZERO | El sistema debe permitir vender productos a precio cero $0.00 como estrategia comercial. | NO LISTO / FALTA CORREGIR | `ProductPricingSystem` permite 0; UI incluye accion de precio cero; caja suma precio real. | Revision estatica y batchmode. | Ejecutar venta real a $0.00, comprobar balance, reporte, compra extra y ausencia de NaN/Infinity. | `ProductPricingSystem.cs`; `PricingAppUIController.cs`; `CashDesk.cs`; `StatsDatabase.cs` |
| 1 | RQF35/RQNF5 | El sistema debera alertar al jugador cuando un cliente lleve 7 segundos esperando en caja sin ser atendido; si no es atendido despues de la alerta abandonara el supermercado sin completar compra y los productos regresaran al inventario. | NO LISTO / FALTA CORREGIR | `Customer.NotifyCheckoutWaitingForService`, `CheckoutWaitTimer`, `CashDesk.TryCancelWaitingCustomer`. | Revision estatica y batchmode. | Probar abandono, atencion manual y cajero automatico en Play Mode; confirmar alerta visual/sonora, inventario y stats. | `Customer.cs`; `CashDesk.cs`; `CustomerCart.cs`; `StatsDatabase.cs` |
| 2 | RQF25-EMP | El sistema debera permitir al jugador gestionar empleados desde la aplicacion Empleados en la computadora de la oficina; consultar desbloqueados, contratar disponibles y asignarles rol operativo. | NO LISTO / FALTA CORREGIR | `EmployeeAppUIController`, `EntrepreneurEmployeeSystem.TryHireEmployee`. | Revision estatica y batchmode. | Abrir app en Play Mode; contratar con/sin fondos; verificar bloqueado, duplicado, descuento y refresco. | `EmployeeAppUIController.cs`; `EntrepreneurEmployeeSystem.cs`; `EntrepreneurTreeUIBootstrap.cs` |
| 2 | RQF26-EMP | Los empleados asignados como cajeros deberan atender automaticamente a clientes en cajas; tiempo entre 1 y 12 segundos con 0.5s por producto, +1.5s tarjeta y +2.5s efectivo. | NO LISTO / FALTA CORREGIR | `EmployeeCashierCoordinator`, `CashDesk.TryStartAutomatedCheckout`, `AutomatedCheckout`. | Revision estatica y batchmode. | Probar cliente real, tarjeta, efectivo, intento manual durante atencion, balance unico y no cobrar abandonados. | `EmployeeCashierCoordinator.cs`; `CashDesk.cs`; `Customer.cs`; `StatsDatabase.cs` |
| 2 | RQF9 | El jugador debe poder asignar roles a empleados desbloqueados: cajero o surtidor, desde la computadora, con descripcion clara. | NO LISTO / FALTA CORREGIR | `EmployeeAppUIController.OnRoleClicked`; `EntrepreneurEmployeeSystem.TryAssignRole`; save/load de rol. | Revision estatica y batchmode. | Asignar Cajero/Surtidor en Play Mode, guardar/cargar y confirmar rol persistente y UI actualizada. | `EmployeeAppUIController.cs`; `EntrepreneurEmployeeSystem.cs`; `EntrepreneurTreeSaveIntegration.cs` |
| 2 | RQF21 | El sistema debera guardar automaticamente la partida al finalizar cada dia conservando dinero, dia, inventario, productos colocados, precios, expansiones, empleados, roles, desbloqueos del arbol y seguridad. | NO LISTO / FALTA CORREGIR | `UIGame.LeaveToNext` llama `SaveGameSystem.Save`; integraciones escuchan `dataSaveEvent`. | Revision estatica y batchmode. | Finalizar dia en Play Mode, verificar notificacion y archivos con datos reales de todos los subsistemas. | `UIGame.cs`; `SaveGameSystem.cs`; `EntrepreneurTreeSaveIntegration.cs`; `SupermarketExpansionSystem.cs`; `ShelfProductSlotSystem.cs` |
| 2 | RQNF3 | El guardado automatico al finalizar cada dia debe conservar correctamente el progreso y restaurarlo al cargar partida. | NO LISTO / FALTA CORREGIR | `SaveGameSystem.Load`, `OnSceneLoaded`, `dataLoadEvent`, integraciones complementarias. | Revision estatica y batchmode. | Guardar, reiniciar/cargar y confirmar dinero, dia, precios, empleados, roles, expansiones, arbol, seguridad y slots. | `SaveGameSystem.cs`; `EntrepreneurTreeSaveIntegration.cs`; `ProductPricingSystem.cs`; `SupermarketExpansionSystem.cs`; `ShelfProductSlotSystem.cs` |
| 3 | RQF27-EMP | Los empleados asignados como surtidores deberan abastecer automaticamente muebles de venta con productos del almacen y habilitar espacios de asignacion donde el jugador selecciona producto por espacio. | NO LISTO / FALTA CORREGIR | `EmployeeRestockCoordinator`; `ShelfProductSlotSystem.AssignProductToSlot`; `NeedsRestock`. | Revision estatica y batchmode. | Probar flujo jugador asigna slot, surtidor repone, falta stock notifica, asignacion persiste. | `EmployeeRestockCoordinator.cs`; `ShelfProductSlotSystem.cs`; `ProductInventorySystem.cs` |
| 3 | RQNF6 | El sistema debera verificar que los productos sean colocados en el mobiliario correcto segun su categoria. | NO LISTO / FALTA CORREGIR | `ShelfProductSlotSystem.CanPlaceProductOnFurniture` centraliza validacion. | Revision estatica y batchmode. | Intentar combinaciones validas/invalidas en Play Mode y confirmar rechazo por jugador y surtidor. | `ShelfProductSlotSystem.cs`; `ProductInventorySystem.cs`; `PlacementObject.cs`; `PlacementSystem.cs` |
| 3 | RQF29 | El sistema debera procesar automaticamente ventas; al completarse venta, productos se descuentan del inventario y dinero se agrega al balance; si producto se agota se muestra notificacion. | NO LISTO / FALTA CORREGIR | `CashDesk.OnBillCustomer`, `StatsDatabase.RegisterSoldProduct`, quejas por producto no disponible. | Revision estatica y batchmode. | Probar venta manual y automatica con stock bajo, confirmar balance unico, descuento unico y notificacion agotado. | `CashDesk.cs`; `CustomerCart.cs`; `ProductInventorySystem.cs`; `StatsDatabase.cs` |
| 3 | RQF34 | El sistema debera generar reporte diario al finalizar cada dia con ventas, gastos, salarios, robos, renta, luz, ganancias netas, productos agotados, clientes perdidos y desempeno general. | NO LISTO / FALTA CORREGIR | `StatsDatabase` arma resumen ampliado; `UIStats` lo muestra. | Revision estatica y batchmode. | Cerrar dia con actividad real y validar cada campo con datos no falsos ni cero incorrecto. | `StatsDatabase.cs`; `UIStats.cs`; `UIGame.cs` |
| 3 | RQNF4 | El sistema debera evitar perdida de informacion durante guardado automatico; si ocurre error debe notificar y conservar ultima version valida. | NO LISTO / FALTA CORREGIR | `SaveGameSystem.WriteAtomic`, `.tmp`, `.bak`, fallback backup; slots tambien tienen backup. | Revision estatica y batchmode. | Simular save corrupto/fallo controlado y comprobar recuperacion desde backup sin perdida. | `SaveGameSystem.cs`; `ShelfProductSlotSystem.cs`; `EntrepreneurTreeSaveIntegration.cs` |
| 4 | RQF13 | El sistema debera generar ladrones dentro del supermercado; al inicio la aparicion sera de 1 ladron por cada 25 clientes y tendran elementos visuales identificables. | NO LISTO / FALTA CORREGIR | `ShoplifterSystem.ShouldBecomeThief`, `initialOneInNChance`, `ShoplifterAgent.ApplyVisualMarker`. | Revision estatica y batchmode. | Forzar/observar 25 clientes en Play Mode, confirmar ladron real y marcador visual sin asset externo. | `ShoplifterSystem.cs`; `ShoplifterAgent.cs`; `Customer.cs` |
| 4 | RQF14 | El sistema debera asignar a cada ladron un valor de robo objetivo antes de iniciar su comportamiento; seleccionara productos disponibles y priorizara productos de mayor valor cuando existan. | NO LISTO / FALTA CORREGIR | `GetTargetStealValue`; `RobberyInventoryBridge.TryReserveStolenItems` ordena alto valor para Expert/Special. | Revision estatica y batchmode. | Probar comun/especial con productos reales, valor objetivo, valor robado y sin stock fantasma. | `ShoplifterSystem.cs`; `ShoplifterAgent.cs`; `RobberyInventoryBridge.cs` |
| 4 | RQF15 | El sistema debera incluir tres tipos de ladrones con comportamientos diferenciados: comun, sospechoso y especial. | NO LISTO / FALTA CORREGIR | Enum incluye Common, Suspicious, Special y otros; diferencias de seleccion/velocidad/valor existen por codigo. | Revision estatica y batchmode. | Probar comportamiento real por tipo: sospechoso por precio alto, especial alto valor/disimulo, comun aleatorio. | `ShoplifterAgent.cs`; `ShoplifterSystem.cs`; `RobberyInventoryBridge.cs` |
| 4 | RQF16 | El sistema debera aumentar dificultad de robos conforme crezca supermercado, usando expansiones de venta, hasta maximo 6.5% del total de clientes diarios. | NO LISTO / FALTA CORREGIR | `CalculateScaledChance` usa `GetPurchasedSalesExpansionCount` y `maxThiefChance=0.065`. | Revision estatica y batchmode. | Probar sin expansiones/con expansiones/muchas expansiones y confirmar chance por logs o test automatizado. | `ShoplifterSystem.cs`; `SupermarketExpansionSystem.cs` |
| 4 | RQF17 | El jugador debera interceptar manualmente a un ladron antes de que escape; si lo detiene recupera productos y recibe puntos; si escapa se pierden productos y notifica valor perdido. | NO LISTO / FALTA CORREGIR | `ShoplifterInteractable`, `ShoplifterAgent.TryManualCapture`, `NotifyManualArrest`, `NotifyEscaped`. | Revision estatica y batchmode. | Capturar/escapar ladrones en Play Mode y confirmar productos, puntos, stats y no duplicados. | `ShoplifterInteractable.cs`; `ShoplifterAgent.cs`; `ShoplifterSystem.cs`; `RobberyInventoryBridge.cs` |
| 5 | RQF10 | El jugador debe poder controlar manualmente la seguridad del supermercado cuando no tenga niveles de seguridad desbloqueados. | NO LISTO / FALTA CORREGIR | Captura manual no consulta nivel de seguridad; guardas anti duplicado. | Revision estatica y batchmode. | Probar security level 0, captura manual, recuperacion, recompensa y no duplicado. | `ShoplifterInteractable.cs`; `ShoplifterAgent.cs`; `ShoplifterSystem.cs` |
| 5 | RQF11 | El sistema permitira desbloquear 3 niveles de seguridad desde el Arbol del Emprendedor: 33%, 66% y 99% de arresto automatico. | NO LISTO / FALTA CORREGIR | `security_1/2/3` definidos; `EntrepreneurTreeSecurityAdapter.GetAutomaticArrestChance` devuelve 0/0.33/0.66/0.99. | Revision estatica y batchmode. | Desbloquear nodos en Play Mode, guardar/cargar y confirmar nivel activo. | `EntrepreneurTreeDefinition.cs`; `EntrepreneurTreeSecurityAdapter.cs`; `EntrepreneurTreeManager.cs` |
| 5 | RQF12 | El jugador necesitara desbloquear habilidades previas para niveles de seguridad y recibira notificacion si faltan requisitos. | NO LISTO / FALTA CORREGIR | `security_1` requiere `employee_7`, `security_2` employee_8, `security_3` employee_14; `CanUnlockNode` valida. | Revision estatica y batchmode. | Intentar unlock sin prerequisito en Play Mode, confirmar mensaje y puntos intactos. | `EntrepreneurTreeDefinition.cs`; `EntrepreneurTreeManager.cs`; `UpgradesUIController.cs` |
| 5 | RQF18 | El sistema debera permitir desbloquear mejoras de seguridad desde el Arbol para automatizar captura de ladrones: camaras 33%, guardias 66%, alarmas 99%. | NO LISTO / FALTA CORREGIR | `ShoplifterSystem.TryAutomaticArrest` consulta adapter real y roll. | Revision estatica y batchmode. | Forzar robos con seguridad 0/1/2/3 y confirmar arresto, fallo, recuperacion y stats. | `ShoplifterSystem.cs`; `EntrepreneurTreeSecurityAdapter.cs`; `StatsDatabase.cs` |
| 5 | RQF19 | El sistema debera mostrar alerta visual y sonora cuando ocurra evento de ladrones. | NO LISTO / FALTA CORREGIR | `ShowShoplifterNotification`, `UIGame.AddNotification`, `AudioSystem.Play2D` con null safety. | Revision estatica y batchmode. | Ver notificaciones y audio real en Play Mode con clip presente/ausente. | `ShoplifterSystem.cs`; `UIGame.cs`; `AudioSystem.cs` |
| 5 | RQF20 | El sistema debera registrar robos en reporte diario. | NO LISTO / FALTA CORREGIR | `StatsDatabase.RegisterThief*`, `BuildDailyRobberySummary`, `UIStats`. | Revision estatica y batchmode. | Generar eventos reales, cerrar dia y confirmar reporte con robos, valores y efectividad. | `StatsDatabase.cs`; `UIStats.cs`; `ShoplifterSystem.cs` |
| 6 | RQF1 | El jugador debe poder acceder a la computadora del juego y ver catalogo de productos, desactivando productos no comprables por restricciones de nivel, desbloqueo o fondos. | NO LISTO / FALTA CORREGIR | `UIShopDesktop.Interact`; `EntrepreneurTreeUIBootstrap.EnsureOrdersTab`; `OrdersAppUIController.RefreshRow`. | Revision estatica y batchmode. | Abrir computadora real en Play Mode, app Compra, bloqueos y no duplicacion. | `UIShopDesktop.cs`; `EntrepreneurTreeUIBootstrap.cs`; `OrdersAppUIController.cs`; `UIShopItemProduct.cs` |
| 6 | RQF2 | El jugador solo podra realizar pedidos si tiene dinero suficiente; si no, se muestra mensaje claro con monto faltante. | NO LISTO / FALTA CORREGIR | `OrdersAppUIController` y `DeliverySystem.Purchase` validan fondos y faltante USD. | Revision estatica y batchmode. | Comprar con fondos y sin fondos en Play Mode; comprobar no pedido y balance no negativo. | `OrdersAppUIController.cs`; `DeliverySystem.cs`; `StoreDatabase.cs` |
| 6 | RQF3 | El jugador debe poder acceder al Arbol del Emprendedor desde la computadora; el arbol debe mostrar nodos graficos, costos y tipos. | NO LISTO / FALTA CORREGIR | `UpgradesUIController.BuildTree`, `NodeUI`, `ConnectionLineUI`, listeners idempotentes. | Revision estatica y batchmode. | Abrir arbol en Play Mode, confirmar nodos, lineas, costos, tipos, estados y no root duplicado. | `EntrepreneurTreeUIBootstrap.cs`; `UpgradesUIController.cs`; `NodeUI.cs`; `ConnectionLineUI.cs` |
| 6 | RQF4 | El jugador solo podra desbloquear habilidades si cumple requisitos previos; si no, se notifica que falta. | NO LISTO / FALTA CORREGIR | `EntrepreneurTreeManager.CanUnlockNode`, `TryUnlockNode`, UI de requisitos. | Revision estatica y batchmode. | Probar unlock fallido/exitoso en Play Mode y confirmar puntos intactos/refresco. | `EntrepreneurTreeManager.cs`; `EntrepreneurTreeDefinition.cs`; `UpgradesUIController.cs` |
| 6 | RQF5 | El sistema debe permitir expandir supermercado en espacios de venta y almacenamiento desde una app de computadora con plano/mapa. | NO LISTO / FALTA CORREGIR | `EnsureExpansionTab`, `ExpansionAppUIController`, `ExpansionMapRenderer`, `TryPurchaseZone`. | Revision estatica y batchmode. | Abrir app expansion en Play Mode, comprar venta/almacen, rechazo duplicado y backend actualizado. | `EntrepreneurTreeUIBootstrap.cs`; `ExpansionAppUIController.cs`; `ExpansionMapRenderer.cs`; `SupermarketExpansionSystem.cs` |
| 6 | RQF6 | El jugador debe ver costo antes de confirmar expansion: venta $1,750.00 por 16 m2 y almacenamiento $2,500.00 por 32 m2. | NO LISTO / FALTA CORREGIR | Dinero en centavos; `SalesPrice=175000`, `StoragePrice=250000`; UI muestra tipo/tamano/precio. | Revision estatica y batchmode. | Validar visualmente y comprobar descuento exacto en Play Mode. | `SupermarketExpansionSystem.cs`; `ExpansionAppUIController.cs`; `MoneyFormatter.cs` |

## 4. Detalle por lote

### Lote 1 - Precios, Compra y Caja
- RQF28-PRICE: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: UI y pricing existen. Evidencia Play Mode: no ejecutada. Bugs encontrados: no hay bug C# nuevo; falta prueba real. Bugs corregidos: ninguno nuevo en auditoria maestra. Faltante exacto: abrir app Precios y validar edicion/confirmacion/rango/persistencia/efecto en clientes.
- RQF25-PRICE: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: adapter conectado a Customer por codigo. Evidencia Play Mode: no ejecutada. Faltante exacto: observar clientes reales con precios ideal/alto/bajo.
- RQF26-PRICE: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: formula extra y llamada desde Customer. Evidencia Play Mode: no ejecutada. Faltante exacto: probar extra con stock real y sin stock.
- RQF27-PRICE: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: UI muestra porcentajes. Evidencia Play Mode: no ejecutada. Faltante exacto: validar refresco visual.
- RQF28-PRICEZERO: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: precio cero permitido. Evidencia Play Mode: no ejecutada. Faltante exacto: venta a $0.00 end-to-end.
- RQF35/RQNF5: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: timer 7s y cancelacion existen. Evidencia Play Mode: no ejecutada. Faltante exacto: casos abandono, jugador y cajero.

### Lote 2 - Empleados, Cajeros y Guardado
- RQF25-EMP: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: app y sistema de empleados existen. Evidencia Play Mode: no ejecutada. Faltante exacto: contratacion real y validaciones desde computadora.
- RQF26-EMP: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: coordinador y flujo automatico existen. Evidencia Play Mode: no ejecutada. Faltante exacto: cliente real, tiempos, pago, balance y conflicto con jugador.
- RQF9: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: roles y save/load existen. Evidencia Play Mode: no ejecutada. Faltante exacto: cambiar rol y recargar.
- RQF21: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: autosave esta conectado al cierre de dia. Evidencia Play Mode: no ejecutada. Faltante exacto: cerrar dia y revisar save real.
- RQNF3: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: carga base y complementaria existen. Evidencia Play Mode: no ejecutada. Faltante exacto: ciclo guardar/reiniciar/cargar con datos.

### Lote 3 - Surtido, Inventario, Reportes y Backup
- RQF27-EMP: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: slots y restock existen. Evidencia Play Mode: no ejecutada. Faltante exacto: surtidor real con slot asignado.
- RQNF6: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: validacion central existe. Evidencia Play Mode: no ejecutada. Faltante exacto: probar muebles compatibles e incompatibles.
- RQF29: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: ventas y agotados registran stats. Evidencia Play Mode: no ejecutada. Faltante exacto: venta manual/automatica con stock bajo.
- RQF34: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: reporte extendido existe. Evidencia Play Mode: no ejecutada. Faltante exacto: cierre de dia con datos reales.
- RQNF4: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: escritura atomica y backups existen. Evidencia Play Mode: no ejecutada. Faltante exacto: simular corrupcion/fallo y recuperar backup.

### Lote 4 - Ladrones, Robo y Captura
Nota: el reporte individual del Lote 4 no existe. La evaluacion se reconstruyo desde `ShoplifterSystem`, `ShoplifterAgent`, `ShoplifterInteractable` y `RobberyInventoryBridge`.
- RQF13: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: probabilidad 1/25 y marcador visual existen. Evidencia Play Mode: no ejecutada. Faltante exacto: observar aparicion real y gating de especial.
- RQF14: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: valor objetivo y reserva de items existen. Evidencia Play Mode: no ejecutada. Faltante exacto: confirmar productos reales y valor robado.
- RQF15: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: tipos existen, pero falta prueba de comportamiento diferenciado. Evidencia Play Mode: no ejecutada. Faltante exacto: forzar comun/sospechoso/especial.
- RQF16: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: escalado usa expansiones y tope 6.5%. Evidencia Play Mode: no ejecutada. Faltante exacto: comprobar chance con expansiones.
- RQF17: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: captura/escape/puntos/stats existen. Evidencia Play Mode: no ejecutada. Faltante exacto: captura y escape real sin duplicados.

### Lote 5 - Seguridad, Alertas y Reporte de Robos
- RQF10: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: captura manual no depende de seguridad. Evidencia Play Mode: no ejecutada. Faltante exacto: captura con seguridad 0.
- RQF11: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: niveles 33/66/99 existen. Evidencia Play Mode: no ejecutada. Faltante exacto: desbloquear y persistir niveles.
- RQF12: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: prerequisitos security_1/2/3 existen. Evidencia Play Mode: no ejecutada. Faltante exacto: probar mensaje y puntos intactos.
- RQF18: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: arresto automatico consulta adapter. Evidencia Play Mode: no ejecutada. Faltante exacto: roll real por niveles.
- RQF19: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: notificaciones y audio seguro existen. Evidencia Play Mode: no ejecutada. Faltante exacto: comprobar alerta visual/sonora.
- RQF20: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: stats de robos y UIStats existen. Evidencia Play Mode: no ejecutada. Faltante exacto: reporte diario con robos reales.

### Lote 6 - Computadora, Compra, Arbol y Expansion
- RQF1: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: computadora y app Compra existen. Evidencia Play Mode: no ejecutada. Faltante exacto: abrir computadora real y comprobar catalogo/bloqueos/no duplicados.
- RQF2: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: validacion fondos con faltante USD en UI y backend. Evidencia Play Mode: no ejecutada. Faltante exacto: compra con/sin fondos real.
- RQF3: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: arbol, nodos, lineas y listeners existen. Evidencia Play Mode: no ejecutada. Faltante exacto: abrir arbol y validar UI.
- RQF4: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: validacion de prerequisitos existe. Evidencia Play Mode: no ejecutada. Faltante exacto: unlock fallido/exitoso.
- RQF5: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: app expansion y mapa existen. Evidencia Play Mode: no ejecutada. Faltante exacto: comprar zonas y comprobar estado.
- RQF6: Estado NO LISTO / FALTA CORREGIR. Evidencia tecnica: costos correctos en centavos y MoneyFormatter. Evidencia Play Mode: no ejecutada. Faltante exacto: comprobar visual y descuento exacto.

## 5. Bugs corregidos durante auditoría maestra

| Bug | Requerimiento afectado | Causa | Archivo | Corrección | Estado |
|---|---|---|---|---|---|
| Ningun bug nuevo corregido durante esta auditoria maestra. | N/A | La auditoria encontro el bloqueo principal de falta de Play Mode, no un nuevo error C# o bug estatico inmediato que pudiera cerrarse sin prueba. | N/A | N/A | N/A |

## 6. Bugs pendientes

| Bug | Requerimiento afectado | Por qué impide estar listo | Archivo probable | Prioridad | Acción exacta necesaria |
|---|---|---|---|---|---|
| No hubo Play Mode real ni prueba automatizada equivalente. | Los 33 requerimientos | Sin ejecucion de gameplay no se puede confirmar comportamiento real. | Escenas `Game.unity`, `Stats.unity`, `UIShopDesktop.unity` y sistemas asociados | Critica | Ejecutar checklist Play Mode A-J y registrar evidencia por requisito. |
| Falta reporte individual del Lote 4. | RQF13-RQF17 | Falta trazabilidad documental del lote de ladrones aunque se reconstruyo desde scripts. | `Documentos/Reporte_Verificacion_Lote4_Ladrones_Robo_Captura.md` | Alta | Crear reporte faltante o anexar evidencia reconstruida al historial. |
| `NEW_Requerimientos.docx` estaba bloqueado por otro proceso. | Auditoria documental | Impidio lectura directa del documento durante esta sesion. | `Documentos/NEW_Requerimientos.docx` | Media | Cerrar proceso que lo bloquea y reextraer texto para comparar contra el reporte maestro. |
| Warnings obsoletos persisten en batchmode. | Calidad tecnica general | No bloquean compilacion, pero deben limpiarse. | `UIStats.cs`, `GameEndSystem.cs`, `EntrepreneurTreeUpgradeAdapter.cs` | Baja | Cambiar `enableWordWrapping` a `textWrappingMode` y `FindObjectsOfType` a `FindObjectsByType`. |

## 7. Pruebas ejecutadas
- Pruebas Play Mode: no ejecutadas.
- Pruebas batchmode: ejecutada Unity 6000.0.37f1 con log maestro.
- Pruebas estaticas: lectura de reportes existentes, revision de documentos disponibles, busquedas `rg` sobre pricing, customer, cash desk, empleados, save, inventario, surtido, ladrones, seguridad, stats, computadora, arbol y expansion.
- Pruebas no ejecutadas: checklist Play Mode A-J completo, pruebas de corrupcion real de save, pruebas de audio, pruebas de UI visual en resolucion, pruebas de comportamiento real de clientes/ladrones/cajeros/surtidores.

## 8. Resultado Unity batchmode
- Comando ejecutado:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\ijuan\Animo' -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Auditoria_Maestra_Lotes1_6.log'
```

- Ruta del log: `C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Auditoria_Maestra_Lotes1_6.log`
- Resultado: compilacion exitosa en batchmode.
- Errores encontrados: no se encontraron `error CS`, `Scripts have compiler errors`, `Compilation failed`, `NullReferenceException` ni `MissingReferenceException`.
- Errores corregidos: ninguno nuevo durante la auditoria maestra.
- Warnings relevantes: warnings preexistentes de TMP `enableWordWrapping` obsoleto y `Object.FindObjectsOfType<T>(bool)` obsoleto.

## 9. Lista final de requerimientos en verde

| ID | Requerimiento resumido | Evidencia de verificación |
|---|---|---|
| Ninguno | Ningun requerimiento cumple el criterio maestro de Play Mode/prueba automatizada equivalente. | N/A |

## 10. Lista final de requerimientos no listos

| ID | Requerimiento resumido | Qué falta exactamente | Próximo paso |
|---|---|---|---|
| RQF28-PRICE | UI modificar precio | Play Mode app Precios completa. | Ejecutar checklist D. |
| RQF25-PRICE | Probabilidad real por precio | Observar clientes reales. | Ejecutar checklist D. |
| RQF26-PRICE | Compra extra | Probar stock suficiente/agotado. | Ejecutar checklist D/G. |
| RQF27-PRICE | Mostrar probabilidades | Validar UI visual. | Ejecutar checklist D. |
| RQF28-PRICEZERO | Precio cero | Venta real a $0.00. | Ejecutar checklist D/E/J. |
| RQF35/RQNF5 | Espera 7s en caja | Casos abandono/jugador/cajero. | Ejecutar checklist E. |
| RQF25-EMP | App empleados | Contratacion real. | Ejecutar checklist F. |
| RQF26-EMP | Cajero automatico | Venta automatica real. | Ejecutar checklist E/F. |
| RQF9 | Roles empleados | Guardar/cargar rol. | Ejecutar checklist F/H. |
| RQF21 | Autosave final de dia | Cierre real con save. | Ejecutar checklist H/J. |
| RQNF3 | Carga completa | Reiniciar/cargar y comparar estado. | Ejecutar checklist H. |
| RQF27-EMP | Surtidor automatico | Asignacion slot y reposicion real. | Ejecutar checklist G. |
| RQNF6 | Mueble correcto | Intentos validos/invalidos. | Ejecutar checklist G. |
| RQF29 | Venta inventario/balance | Venta manual y automatica con stock bajo. | Ejecutar checklist E/G. |
| RQF34 | Reporte diario | Cierre de dia con datos reales. | Ejecutar checklist J. |
| RQNF4 | Backup save | Simular corrupcion/fallo. | Ejecutar checklist H. |
| RQF13 | Spawn ladron | Observar/forzar 25 clientes. | Ejecutar checklist I. |
| RQF14 | Valor robo | Confirmar productos reales y valor. | Ejecutar checklist I. |
| RQF15 | Tres tipos ladron | Forzar comun/sospechoso/especial. | Ejecutar checklist I. |
| RQF16 | Escalado robos | Probar expansiones y chance. | Ejecutar checklist C/I. |
| RQF17 | Captura manual | Captura/escape sin duplicados. | Ejecutar checklist I. |
| RQF10 | Seguridad manual sin niveles | Captura con nivel 0. | Ejecutar checklist I. |
| RQF11 | Niveles seguridad | Desbloquear y persistir 33/66/99. | Ejecutar checklist B/I/H. |
| RQF12 | Requisitos seguridad | Probar faltantes. | Ejecutar checklist B. |
| RQF18 | Arresto automatico | Seguridad 0/1/2/3. | Ejecutar checklist I. |
| RQF19 | Alertas ladrones | Visual/sonora. | Ejecutar checklist I. |
| RQF20 | Reporte robos | Robos reales en reporte. | Ejecutar checklist I/J. |
| RQF1 | Computadora y catalogo | Abrir computadora real. | Ejecutar checklist A. |
| RQF2 | Compra con fondos | Compra con/sin fondos. | Ejecutar checklist A. |
| RQF3 | Arbol desde computadora | Abrir arbol y revisar nodos. | Ejecutar checklist B. |
| RQF4 | Prerequisitos arbol | Unlock fallido/exitoso. | Ejecutar checklist B. |
| RQF5 | Expansion con mapa | Comprar zonas. | Ejecutar checklist C. |
| RQF6 | Costos expansion | Confirmar precio/descuento. | Ejecutar checklist C. |

## 11. Plan de cierre recomendado
1. Requisitos bloqueados por falta de Play Mode: ejecutar una sesion QA manual grabada o documentada con screenshots/logs siguiendo checklist A-J. Cada requisito debe recibir evidencia concreta.
2. Bugs funcionales: corregir cualquier fallo encontrado en computadora, precios, caja, cajeros, surtidores, ladrones y seguridad durante la sesion.
3. Bugs de persistencia: ejecutar ciclo guardar/cargar completo y corrupcion controlada de save/backup.
4. Bugs de UI/conexion: validar que apps no se dupliquen, que botones funcionen y que textos importantes se vean.
5. Limpieza tecnica: corregir warnings obsoletos de TMP y `FindObjectsOfType`, y crear el reporte faltante de Lote 4.

## 12. Prompt recomendado siguiente

```text
ANIMO CABRONES. Ejecutar QA Play Mode real de ShopMaster para cerrar los 33 requerimientos NO LISTO del Reporte_Maestro_Cumplimiento_Lotes1_6.md. No implementar features nuevos. Abrir Unity 6000.0.37f1, escena Game.unity, ejecutar checklist A-J del reporte maestro, capturar evidencia por requisito, corregir solo bugs que impidan pasar la prueba, repetir Play Mode tras cada correccion y actualizar Reporte_Maestro_Cumplimiento_Lotes1_6.md y Matriz_Maestra_Cumplimiento_Lotes1_6.csv marcando LISTO solo los requisitos con evidencia real. Ejecutar batchmode al final y no usar estados intermedios.
```
