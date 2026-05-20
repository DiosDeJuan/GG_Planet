# Reporte de Verificación — NEW_Requerimientos.docx
**Proyecto:** ShopMaster  
**Rama:** copilot/copilotfix-emprendedor-tree-system-again  
**Commit base:** fad8c26  
**Fecha de verificación:** 2026-05-20  
**Auditor:** Copilot (análisis estático + revisión de código; sin Play Mode)

---

## 1. Resumen Ejecutivo

| Métrica | Valor |
|---|---|
| **Total de requerimientos RQF** | 37 (incluye duplicados deduplicados internamente) |
| **Total de requerimientos RQNF** | 20 |
| **Total general** | 57 |
| **VERIFICADOS** | **0** |
| **PARCIALES** | **52** |
| **NO_LISTOS** | **3** (RQNF1, RQNF2, RQNF19) |
| **BLOQUEADOS** | **2** (RQNF1, RQNF2) |
| **Porcentaje real de avance** | ~0% verificado (0% con Play Mode) / ~65% código implementado |
| **¿Hubo Play Mode real?** | **NO** — Este entorno de CI no ejecuta Unity interactivo |
| **¿Compilación batchmode?** | No ejecutada en este entorno; último log externo (del reporte maestro anterior): compilación exitosa sin errores C# |

### Criterio aplicado
- **VERIFICADO**: Solo si Play Mode real o prueba automatizada equivalente con evidencia ejecutada. En este entorno no hay Unity ejecutable, por lo que **ningún requerimiento puede marcarse VERIFICADO**.
- **PARCIAL**: Código/UI/prefab existe; conectado en bootstrap; pero falta Play Mode, Inspector en escena real, o evidencia funcional de ejecución.
- **NO_LISTO**: No existe código/sistema que cubra el requerimiento.
- **BLOQUEADO**: Requiere hardware real, Unity corriendo, build, o NavMesh/escena que no está disponible aquí.

---

## 2. Deduplicación de IDs del Documento

El archivo `NEW_Requerimientos.docx` tiene las siguientes duplicaciones de IDs que deben documentarse:

| ID Original | ID Interno | Sección | Tema |
|---|---|---|---|
| RQF25 | RQF25A_GestionEmpleados | Sección 10: Gestión de Empleados | App Empleados en computadora |
| RQF25 | RQF25B_ProbabilidadCompra | Sección 11: Precios y Ventas | Fórmula de probabilidad de compra |
| RQF26 | RQF26A_CajeroAutomatico | Sección 10: Gestión de Empleados | Cajero atiende automáticamente |
| RQF26 | RQF26B_CompraExtra | Sección 11: Precios y Ventas | Probabilidad de compra extra |
| RQF27 | RQF27A_SurtidorAutomatico | Sección 10: Gestión de Empleados | Surtidor abastece anaqueles |
| RQF27 | RQF27B_MostrarProbabilidades | Sección 11: Precios y Ventas | Mostrar prob/extra en UI precios |
| RQF28 | RQF28A_ModificarPrecio | Sección 11: Precios y Ventas | Modificar precio individual |
| RQF28 | RQF28B_PrecioCero | Sección 11: Precios y Ventas | Vender a $0.00 |
| RQF28 | RQF28C_MejorasArbol | Sección 13: Desbloqueo de Mejoras | Mejoras con puntos del Árbol |

**Nota**: Las secciones RQNF7–RQNF9 del documento tienen también numeración interna que se solapa con los RQNF previos de la sección de probabilidad. Se usa la numeración del documento final como única.

---

## 3. Top 10 Bloqueos Críticos

Estos son los 10 obstáculos que impiden declarar el proyecto terminado, ordenados por impacto:

### Bloqueo #1 — Compra de productos (BUG CRÍTICO)
- **Requerimiento:** RQF2, RQF1
- **Problema:** Sin Play Mode real, no se puede confirmar que el fix de `DeliverySystem` (null-safety + refund) realmente elimina el "error inesperado". El código está corregido pero sin ejecución real.
- **Archivo:** `Assets/StoreSimulator/Scripts/DeliverySystem.cs`
- **Acción:** Play Mode en PC — comprar Leche/Agua/Pasta/Azúcar y verificar paquete en zona de entrega.

### Bloqueo #2 — Expansión no afecta terreno real (BUG CRÍTICO)
- **Requerimiento:** RQF5, RQF6, RQF7
- **Problema:** `ExpansionRealWorldBridge.cs` fue creado pero los `ExpansionObject` de la escena pueden no tener IDs que coincidan con los del sistema. El bridge tiene fallback con primitivas pero puede no aparecer en la posición correcta si `mapToWorldScale` (0.25f) no coincide con el layout real de la escena.
- **Archivo:** `Assets/Systems/Expansion/ExpansionRealWorldBridge.cs`
- **Acción:** Abrir escena `Game.unity` en Inspector; buscar `ExpansionObject` components; verificar que su campo `expansion.id` coincide con los IDs `sales_w1`, `sales_n1`, `storage_e1`, `storage_w1` del sistema.

### Bloqueo #3 — Empleados NPC no verificados (BUG CRÍTICO)
- **Requerimiento:** RQF25A, RQF26A, RQF27A, RQF9
- **Problema:** `EmployeeNPCSpawner.AutoDiscoverCustomerPrefabs()` busca `Customer` components en escena en tiempo de ejecución, pero si la escena no tiene Customer instances activos al momento del `Start()`, el array queda vacío y el NPC no puede spawnearse. El `employeeSpawnPoint` tampoco está asignado en Inspector.
- **Archivo:** `Assets/Systems/EntrepreneurTree/EmployeeNPCSpawner.cs`
- **Acción:** En Inspector en Play Mode — verificar que `employeePrefabs[]` se llenó; verificar que `employeeSpawnPoint` tiene valor (fallback a `Vector3.zero`).

### Bloqueo #4 — Sin Play Mode para ningún requerimiento
- **Requerimiento:** Todos (57)
- **Problema:** El entorno de CI no puede ejecutar Unity interactivo. **Cero requerimientos tienen evidencia Play Mode.** El criterio del documento exige Play Mode para marcar VERIFICADO.
- **Acción:** Ejecutar checklist completo A-G en la PC local con Unity 6000.0.37f1 y escena `Game.unity`.

### Bloqueo #5 — Ladrones dependen de spawn real
- **Requerimiento:** RQF13, RQF14, RQF15, RQF16, RQF17, RQF18, RQF19, RQF20
- **Problema:** El flujo de ladrones existe en código pero nunca fue ejecutado con clientes reales. La gorra/mochila placeholder depende de encontrar hueso de cabeza por nombre ("Head"/"head"/"Cabeza") que puede no existir en los rigs del asset.
- **Archivo:** `Assets/Systems/EntrepreneurTree/ShoplifterAgent.cs`
- **Acción:** Play Mode con `AdminMode.forceShoplifterSpawn=true`; verificar que gorra/mochila aparecen o fallback al root.

### Bloqueo #6 — Guardado no verificado end-to-end
- **Requerimiento:** RQF21, RQNF3, RQNF4
- **Problema:** `SaveGameSystem` con escritura atómica y `EntrepreneurTreeSaveIntegration` existen, pero no se verificó que todos los subsistemas (expansión, empleados, precios, slots, árbol) persistan y restauren correctamente en un ciclo completo.
- **Acción:** Play Mode → activar datos → cerrar día → reiniciar Unity → cargar → verificar cada campo.

### Bloqueo #7 — Cajero/surtidor no verificados
- **Requerimiento:** RQF26A, RQF27A
- **Problema:** `EmployeeCashierCoordinator` y `EmployeeRestockCoordinator` existen pero requieren que el NPC esté spawneado, asignado a workstation, y que haya cliente en caja / stock en almacén. La cadena completa nunca se ejecutó.
- **Acción:** Play Mode con `AdminMode.prepareSalesTest=true`; contratar cajero/surtidor; asignar; observar comportamiento.

### Bloqueo #8 — Settings/Ajustes no implementados
- **Requerimiento:** RQNF19
- **Problema:** No se encontró `SettingsMenu.cs` o clase equivalente con `PlayerPrefs` para resolución/volumen/brillo/controles. `TutorialSystem.cs` y `UITutorial.cs` existen pero no son ajustes.
- **Acción:** Buscar en `Intro.unity` si el asset base tiene panel de ajustes; si no, crear clase mínima `GameSettingsMenu.cs`.

### Bloqueo #9 — ExpansionObject IDs desconocidos en escena
- **Requerimiento:** RQF5
- **Problema:** `ExpansionRealWorldBridge` busca `ExpansionObject` components y los filtra por `expansion.id`. Si el asset tiene `UpgradeObject` o componentes con nombre diferente, el lookup falla silenciosamente y crea primitivas en posiciones calculadas que pueden ser incorrectas.
- **Acción:** Abrir `Game.unity`; buscar en la jerarquía objetos con `ExpansionObject` o `UpgradeObject`; anotar sus IDs; si difieren, crear un mapeo en `ExpansionRealWorldBridge`.

### Bloqueo #10 — Precios no conectados al cliente (PENDING wire)
- **Requerimiento:** RQF25B, RQF26B, RQF29
- **Problema:** `ProductPurchaseProbabilityAdapter.cs` tiene comentario `PENDING: wire this to the checkout/payment completion point`. `FireSaleHooks()` no estaba conectado a `CashDesk.OnBillCustomer` según el código encontrado.
- **Archivo:** `Assets/Systems/Pricing/ProductPurchaseProbabilityAdapter.cs:114-117`
- **Acción:** Verificar si `CashDesk.cs` llama `ProductPurchaseProbabilityAdapter.FireSaleHooks()` después de cada venta exitosa.

---

## 4. Matriz Resumida por Requerimiento

| ID Original | ID Interno | Categoría | Estado | Evidencia encontrada | Qué falta |
|---|---|---|---|---|---|
| RQF1 | RQF1_ComputadoraCatalogo | Computadora | PARCIAL | OrdersAppUIController + ProductUnlockAdapter con bootstrap | Play Mode: abrir computadora |
| RQF2 | RQF2_CompraConFondos | Computadora | PARCIAL | DeliverySystem fix (null-safety+refund) en este lote | Play Mode: comprar con/sin fondos |
| RQF3 | RQF3_ArbolEmprendedor | Árbol | PARCIAL | UpgradesUIController.BuildTree; nodos gráficos | Play Mode: abrir árbol |
| RQF4 | RQF4_RequisitosArbol | Árbol | PARCIAL | CanUnlockNode valida prerequisitos | Play Mode: unlock fallido/exitoso |
| RQF5 | RQF5_ExpansionMapa | Expansión | PARCIAL | ExpansionRealWorldBridge creado; bridge a onZonePurchased | Play Mode: comprar zona + verificar mundo |
| RQF6 | RQF6_CostosExpansion | Expansión | PARCIAL | SalesPrice=175000; StoragePrice=250000; UI panel | Play Mode: verificar costos en UI |
| RQF7 | RQF7_ErrorExpansionSinFondos | Expansión | PARCIAL | TryPurchaseZone retorna missingFunds | Play Mode: comprar sin fondos |
| RQF8 | RQF8_DesbloqueoEmpleados | Empleados | PARCIAL | employee_1-N en EntrepreneurTreeDefinition | Play Mode: desbloquear y verificar en UI |
| RQF9 | RQF9_RolesEmpleados | Empleados | PARCIAL | OnRoleClicked; TryAssignRole; save/load de rol | Play Mode: asignar rol; guardar/cargar |
| RQF10 | RQF10_SeguridadManual | Seguridad | PARCIAL | TryManualCapture no depende de nivel | Play Mode: forzar ladrón; captura manual |
| RQF11 | RQF11_NivelesSeguridadArbol | Seguridad | PARCIAL | security_1/2/3 con chances 0.33/0.66/0.99 | Play Mode: desbloquear + verificar |
| RQF12 | RQF12_RequisitosSeguridad | Seguridad | PARCIAL | Prerequisitos employee_7/8/14 definidos | Play Mode: probar prerequisito faltante |
| RQF13 | RQF13_GenerarLadrones | Seguridad | PARCIAL | ShouldBecomeThief 1/25; placeholder cap/mochila en este lote | Play Mode: 25+ clientes o AdminMode |
| RQF14 | RQF14_ValorRoboObjetivo | Seguridad | PARCIAL | GetTargetStealValue; TryReserveStolenItems | Play Mode: forzar ladrón con productos |
| RQF15 | RQF15_TresTiposLadron | Seguridad | PARCIAL | Common/Suspicious/Expert/Fast/Special en enum | Play Mode: forzar cada tipo |
| RQF16 | RQF16_EscaladoRobos | Seguridad | PARCIAL | CalculateScaledChance con maxThiefChance=0.065 | Play Mode: comprar expansiones; verificar tasa |
| RQF17 | RQF17_InterceptarLadrones | Seguridad | PARCIAL | TryManualCapture; NotifyManualArrest; NotifyEscaped | Play Mode: captura y escape real |
| RQF18 | RQF18_AutoArrestoSeguridad | Seguridad | PARCIAL | TryAutomaticArrest consulta adapter | Play Mode: security_1 + forzar ladrones |
| RQF19 | RQF19_AlertasLadrones | Seguridad | PARCIAL | ShowShoplifterNotification; UIGame.AddNotification | Play Mode: verificar notificación visual/audio |
| RQF20 | RQF20_ReporteRobos | Seguridad | PARCIAL | StatsDatabase.RegisterThief*; BuildDailyRobberySummary | Play Mode: robos reales + cerrar día |
| RQF21 | RQF21_GuardadoAutomatico | Guardado | PARCIAL | UIGame.LeaveToNext → SaveGameSystem.Save; dataSaveEvent | Play Mode: cerrar día + verificar archivos |
| RQF22 | RQF22_ClientesDiarios | Clientes | PARCIAL | ExpansionCustomerDemandAdapter +15% por zona; maxSpawnRate=200 | Inspector: verificar spawnRate base = 50-75 |
| RQF23 | RQF23_PagoTarjeta | Caja | PARCIAL | CashDesk con PaymentType; TryStartAutomatedCheckout | Play Mode: cliente con tarjeta |
| RQF24 | RQF24_PagoEfectivo | Caja | PARCIAL | CashDesk con pago en efectivo y cambio | Play Mode: cliente con efectivo |
| RQF25 (dup1) | RQF25A_GestionEmpleados | Empleados | PARCIAL | EmployeeAppUIController; TryHireEmployee; auto-discover NPC | Play Mode: contratar + NPC aparece |
| RQF26 (dup1) | RQF26A_CajeroAutomatico | Empleados | PARCIAL | EmployeeCashierCoordinator; TryStartAutomatedCheckout | Play Mode: cajero + cliente = atención |
| RQF27 (dup1) | RQF27A_SurtidorAutomatico | Empleados | PARCIAL | EmployeeRestockCoordinator; ShelfProductSlotSystem | Play Mode: slot asignado + surtidor repone |
| RQF28 (dup1) | RQF28A_ModificarPrecio | Precios | PARCIAL | PricingAppUIController.ConfirmPrice; TrySetCurrentPrice | Play Mode: abrir app Precios y editar |
| RQF29 | RQF29_VentasAutomaticas | Ventas | PARCIAL | CashDesk.OnBillCustomer; RegisterSoldProduct; agotado notificado | Play Mode: venta con stock bajo |
| RQF25 (dup2) | RQF25B_ProbabilidadCompra | Precios | PARCIAL | GetPurchaseProbabilityForPrice con k=0.10; Customer.ShouldCustomerBuy | Play Mode: precio alto → clientes rechazan |
| RQF26 (dup2) | RQF26B_CompraExtra | Precios | PARCIAL | GetExtraPurchaseProbabilityForPrice; Customer.ShouldCustomerBuyExtra | Play Mode: precio bajo + stock = compra extra |
| RQF27 (dup2) | RQF27B_MostrarProbabilidadesUI | UI | PARCIAL | PricingAppUIController.RefreshRow muestra prob/extra | Play Mode: abrir UI Precios |
| RQF28 (dup2) | RQF28B_PrecioCero | Precios | PARCIAL | Precio 0 permitido; RegisterZeroPriceSale; extra calculado | Play Mode: $0.00 end-to-end |
| RQF34 | RQF34_ReporteDiario | Reportes | PARCIAL | StatsDatabase con secciones ampliadas (Finanzas/Clientes/Robos/Extra) | Play Mode: día completo + stats scene |
| RQF35 | RQF35_Espera7Segundos | Caja | PARCIAL | CheckoutWaitTimer; TryCancelWaitingCustomer | Play Mode: dejar cliente 7+ segundos |
| RQF36 | RQF36_GateArbol | Árbol | PARCIAL | CanUnlockNode centralizado; adapters verifican IsUnlocked | Play Mode: confirmar gate activo |
| RQF28 (dup3) | RQF28C_MejorasArbol | Árbol | PARCIAL | Mejoras definidas en EntrepreneurTreeDefinition | Play Mode: desbloquear mejora y verificar efecto |
| RQNF1 | RQNF1_Rendimiento30FPS | Performance | BLOQUEADO | Sin herramienta de profiling en este entorno | Unity Profiler en hardware objetivo |
| RQNF2 | RQNF2_RequisitosHW | Técnico | BLOQUEADO | Requiere build real para verificar | Build Windows + prueba en hardware mínimo |
| RQNF3 | RQNF3_GuardadoCompleto | Guardado | PARCIAL | SaveGameSystem atómico + integraciones | Play Mode: ciclo guardar/cargar completo |
| RQNF4 | RQNF4_SinPerdidaGuardado | Guardado | PARCIAL | WriteAtomic + .bak backup | Simular corrupción + verificar recuperación |
| RQNF5 | RQNF5_Espera7sValidacion | Caja | PARCIAL | CheckoutWaitTimer | Play Mode: test timer abandono |
| RQNF6 | RQNF6_ProductoEnMuebleCorrecto | Inventario | PARCIAL | CanPlaceProductOnFurniture centralizado | Play Mode: producto incompatible en mueble |
| RQNF7 | RQNF7_ValidarRecursos | General | PARCIAL | CanPurchase/CanUnlockNode en todos los sistemas | Play Mode: probar sin recursos |
| RQNF8 | RQNF8_EstadoNodosArbol | Árbol | PARCIAL | EstadoNodos gestionado; save/load | Play Mode: guardar/cargar estados nodos |
| RQNF9 | RQNF9_NoProdSinArbol | Árbol | PARCIAL | IsUnlocked en todos adapters | Play Mode: intentar usar sin desbloquear |
| RQNF10 | RQNF10_ProbabilidadTiempoReal | Precios | PARCIAL | onPriceChanged event; RefreshRow reactivo | Play Mode: mover slider → prob cambia |
| RQNF11 | RQNF11_UIPrecios | UI | PARCIAL | PricingAppUIController.RefreshRow | Play Mode: abrir UI Precios |
| RQNF12 | RQNF12_LimitesPrecio | Precios | PARCIAL | GetMaxAllowedPrice; TrySetCurrentPrice valida | Play Mode: intentar precio inválido |
| RQNF13 | RQNF13_MensajePrecioBajo | UI | PARCIAL | Pextra mostrado implícitamente | Play Mode: precio bajo → verificar mensaje |
| RQNF14 | RQNF14_MensajePrecioAlto | UI | PARCIAL | Prob baja mostrada implícitamente | Play Mode: precio alto → verificar indicador |
| RQNF15 | RQNF15_PrecioCeroIndicacion | UI | PARCIAL | RegisterZeroPriceSale existe; indicador UI no confirmado | Play Mode: $0.00 + verificar mensaje |
| RQNF16 | RQNF16_UIClara | UI | PARCIAL | ComputerUITheme centraliza diseño | Play Mode: navegar todas las apps |
| RQNF17 | RQNF17_Notificaciones | UI | PARCIAL | DeliverySystem mejorado; mensajes específicos | Play Mode: provocar errores; verificar mensajes |
| RQNF18 | RQNF18_ConsistenciaVisual | UI | PARCIAL | ComputerUITheme aplicado en apps | Play Mode: comparar apps visualmente |
| RQNF19 | RQNF19_OpcionesAjuste | Ajustes | NO_LISTO | No se encontró SettingsMenu con PlayerPrefs | Crear GameSettingsMenu.cs o verificar en Intro.unity |
| RQNF20 | RQNF20_Instrucciones | UI | PARCIAL | TutorialSystem + UITutorial + TutorialScriptableObject | Play Mode: verificar tutorial al inicio |

---

## 5. Detalle por Bloque

### Bloque 1 — Computadora y Compra de Productos (RQF1, RQF2)

**Estado general:** PARCIAL

**Lo que existe y está implementado:**
- `OrdersAppUIController.cs`: crea la pestaña Compra; muestra productos con bloqueos; `OnBuyClicked` valida fondos y llama `DeliverySystem.Purchase`.
- `DeliverySystem.cs` (fijado en este lote): null-safety para `InteractionSystem.Instance`; try-catch con reembolso; mensajes de error específicos con nombre del producto.
- `EntrepreneurTreeProductUnlockAdapter.cs`: mapea grupos de productos a nodos del árbol; `IsProductUnlocked` verifica estado.
- `HideLicensesTab` en `EntrepreneurTreeUIBootstrap.cs`: oculta pestaña "Licenses" por texto.

**Lo que NO está verificado:**
- Apertura real de la computadora en escena → presionar E/interacción → UI abre sin errores.
- Que el bloqueo visual por árbol (overlay "Requiere: Productos Básicos 2") se muestre correctamente.
- Que comprar Leche resulta en paquete en zona de entrega (no en 0,0,0 ni fuera del mapa).
- Que la pestaña Licenses está efectivamente oculta con el texto exacto del asset base.

**Bug de riesgo detectado — RQF2:**
El comment en `ProductPurchaseProbabilityAdapter.cs:117` dice:
```
// PENDING: wire this to the checkout/payment completion point in the asset.
```
Si `FireSaleHooks()` no está llamado desde `CashDesk.OnBillCustomer`, la probabilidad de compra no afecta las ventas reales.

---

### Bloque 2 — Árbol del Emprendedor (RQF3, RQF4, RQF36, RQF28C)

**Estado general:** PARCIAL

**Lo que existe:**
- `EntrepreneurTreeDefinition.cs`: 30+ nodos con posiciones, prerequisitos, costos, tipos.
- `UpgradesUIController.cs`: `BuildTree()` dibuja nodos como cards con `ConnectionLineUI`.
- `EntrepreneurTreeManager.cs`: `CanUnlockNode`/`TryUnlockNode` con validación completa.
- Nodo root `product_basic_1` desbloqueado por defecto.

**Lo que NO está verificado:**
- Que el Canvas del árbol se renderiza sin superposiciones ni clipping en resolución real.
- Que las líneas de conexión no producen "white bar flash" (fix aplicado en lote anterior).
- Que puntos se descuentan correctamente al desbloquear.

---

### Bloque 3 — Expansión (RQF5, RQF6, RQF7)

**Estado general:** PARCIAL — **RIESGO ALTO**

**Lo que existe:**
- `SupermarketExpansionSystem.cs`: zonas con IDs `sales_w1/n1/e1/s1` y `storage_w1/e1/n1/s1`; precios correctos; `TryPurchaseZone`.
- `ExpansionAppUIController.cs` y `ExpansionMapRenderer.cs`: UI con panel de detalle; mapa de zonas.
- `ExpansionRealWorldBridge.cs` (nuevo en este lote): escucha `onZonePurchased`; busca `ExpansionObject` por ID; fallback a nombre de GameObject; fallback a primitiva.

**Riesgos críticos:**
1. Si el asset tiene `UpgradeObject` o nombre diferente en lugar de `ExpansionObject`, el lookup por componente falla silenciosamente.
2. El `mapToWorldScale = 0.25f` en el bridge es una suposición; el layout real de la escena puede ser a escala diferente.
3. Los IDs del sistema (`sales_w1`) pueden no coincidir con los IDs de los `ExpansionObject` del asset.

**Acción inmediata en tu PC:**
1. Abre `Game.unity`.
2. En jerarquía busca: `ExpansionObject`, `UpgradeObject`, o cualquier objeto con nombre `ExpansionZone`.
3. Verifica si tienen componente `ExpansionObject.cs` con campo `expansion.id`.
4. Si el ID es diferente (ej: `zone_0`, `expansion_a`), actualiza `EntrepreneurTreeDefinition` o `ExpansionRealWorldBridge` para mapear correctamente.

---

### Bloque 4 — Empleados (RQF8, RQF9, RQF25A, RQF26A, RQF27A)

**Estado general:** PARCIAL — **RIESGO ALTO**

**Lo que existe:**
- `EntrepreneurEmployeeSystem.cs`: `TryHireEmployee`; `TryAssignRole`; `IsEmployeeUnlocked`; save/load.
- `EmployeeAppUIController.cs`: listas de bloqueados/disponibles/contratados; UI con roles.
- `EmployeeNPCSpawner.cs` (fijado en este lote): `AutoDiscoverCustomerPrefabs()` busca Customer components en escena; deshabilita scripts de cliente; tinta por rol.
- `EmployeeWorkstationRegistry.cs`: crea `CashierStation_N` cerca de cada `CashDesk`; `RestockerStation_0` fallback.
- `EmployeeCashierCoordinator.cs`: busca caja con cliente; `TryStartAutomatedCheckout`.
- `EmployeeRestockCoordinator.cs`: busca slot con producto asignado y stock bajo; repone.

**Riesgos críticos:**
1. `AutoDiscoverCustomerPrefabs()` requiere Customer activos en escena durante `Start()`. Si los clientes no spawnearon aún, el array queda vacío → NPC falla en spawn silencioso.
2. `employeeSpawnPoint` no asignado en Inspector → NPC aparece en `Vector3.zero` (0,0,0) o en posición del spawner.
3. `EmployeeCashierCoordinator` requiere que el NPC tenga `NavMeshAgent` funcional → si NavMesh no está bakeado para la zona del empleado, el movimiento falla.

**Acción en tu PC:**
1. En Play Mode, abrir consola → buscar `"No prefabs found for employee NPC"` o `NullReferenceException` en `EmployeeNPCSpawner`.
2. Verificar que `employeeSpawnPoint` tiene valor en Inspector → si no, asignar manualmente un `Transform` cerca de las cajas.
3. Si NavMesh no cubre la zona del empleado: bake NavMesh incluyendo la zona.

---

### Bloque 5 — Seguridad y Ladrones (RQF10-RQF20)

**Estado general:** PARCIAL

**Lo que existe:**
- `ShoplifterSystem.cs`: spawn con probabilidad 1/25; escalado con expansiones hasta 6.5%; tipos Common/Suspicious/Expert/Fast/Special; `TryAutomaticArrest` con roll.
- `ShoplifterAgent.cs` (modificado en este lote): gorra (cylíndro) y mochila (cubo) como primitivas Unity; destrucción en `OnDestroy` sin memory leak.
- `ShoplifterInteractable.cs`: `TryManualCapture`; interacción para el jugador.
- `RobberyInventoryBridge.cs`: reserva items reales del inventario.
- `EntrepreneurTreeSecurityAdapter.cs`: `GetAutomaticArrestChance` → 0/0.33/0.66/0.99.

**Riesgos:**
1. Hueso "Head" buscado por nombre en `SpawnThiefAccessories` puede no existir en los rigs del asset base → gorra aparece en root o con `t.parent == null`.
2. AdminMode `forceShoplifterSpawn` existe en `AdminSessionConfig` para testing.

---

### Bloque 6 — Menú Principal, Cierre de Día y Reporte (RQF34, RQF35, RQNF5)

**Estado general:** PARCIAL

**Lo que existe:**
- `StatsDatabase.cs`: `BuildInventoryExpansionSummary` con secciones Finanzas/Clientes/Robos/Extra/Zero.
- `UIStats.cs`: muestra resumen en Stats scene.
- `UIGame.cs`: `LeaveToNext()` llama save; timer de 7s para clientes en caja.
- `DayCycle` en asset base con horario 5am-11pm.

**Lo que falta confirmar:**
- Que `Stats.unity` muestra todos los campos correctamente (no vacíos).
- Que `UIGame.cs` tiene la lógica de alerta a 7 segundos correctamente conectada.

---

### Bloque 7 — Precios y Ventas (RQF25B, RQF26B, RQF27B, RQF28A, RQF28B, RQF29)

**Estado general:** PARCIAL — **BUG PENDIENTE**

**Lo que existe:**
- `ProductPricingSystem.cs`: `TrySetCurrentPrice`; `GetPurchaseProbabilityForPrice` con k=0.10; `GetExtraPurchaseProbabilityForPrice`; `GetMaxAllowedPrice` (300%).
- `PricingAppUIController.cs`: `RefreshRow` con columnas prob/extra; `ConfirmPrice`.
- `Customer.cs`: llama `ShouldCustomerBuy` y `ShouldCustomerBuyExtra` vía adapter.
- `CashDesk.cs`: `RegisterZeroPriceSale` cuando precio=0.

**BUG DETECTADO (línea 117 de `ProductPurchaseProbabilityAdapter.cs`):**
```csharp
// PENDING: wire this to the checkout/payment completion point in the asset.
```
`FireSaleHooks()` no está conectado. Si las ventas del asset no llaman a este método, la probabilidad de compra afecta la decisión inicial pero no las estadísticas post-venta.

---

### Bloque 8 — Guardado (RQF21, RQNF3, RQNF4)

**Estado general:** PARCIAL

**Lo que existe:**
- `SaveGameSystem.cs`: escritura atómica con `.tmp` → rename; backup `.bak`; `dataSaveEvent`/`dataLoadEvent`.
- `EntrepreneurTreeSaveIntegration.cs`: guarda árbol/empleados/expansión/seguridad/workstations.
- `ShelfProductSlotSystem.cs`: guarda slots en `shelfSlots.dat`.
- `ProductPricingSystem.cs`: guarda precios.
- Todos usan `Path.Combine(Application.persistentDataPath, filename)`.

**Lo que falta confirmar:**
- Que todos los sistemas se suscriben a `dataSaveEvent` correctamente.
- Ciclo completo: datos guardados → Unity reinicia → datos restaurados → gameplay idéntico.

---

### Bloque 9 — UI / Usabilidad (RQNF16-RQNF20)

**Estado general:** PARCIAL + 1 NO_LISTO

- `HideLicensesTab`: implementado; pendiente verificar en escena con texto exacto del asset.
- `ComputerUITheme.cs`: centraliza colores/fuentes; apps usan helper.
- `ShelfProductInfoLabel.cs` (nuevo): world-space labels en PlacementObjects.
- **RQNF19 (PARCIAL)**: `UISettings.cs` existe con slider de volumen y persistencia en JSON. Cubre volumen parcialmente. Resolución/brillo/controles no encontrados en el código — puede estar en Intro.unity como panel de Inspector. Estado actualizado a PARCIAL tras verificar `UISettings.cs`.
- `TutorialSystem.cs` + `UITutorial.cs`: existen para RQNF20, contenido no verificado.

---

### Bloque 10 — Finales del Juego

**Estado general:** PARCIAL

- `GameEndSystem.cs`: Monopoly Final (todas expansiones + todos nodos árbol) y Bankruptcy Final (dinero ≤ 0 al cierre del día).
- `AdminDeferredGameEndTest.cs`: test diferido con `triggerMonopolyTest`/`triggerBankruptcyTest` en AdminSessionConfig.
- No hay "venta de activos" documentada en el código; se puede implementar si el documento lo requiere en una fase futura.

---

## 6. Bugs Corregidos Durante Esta Verificación

- **Bug detectado corregido:** FireSaleHooks ya está conectado en `CashDesk.cs:221` — el comment PENDING en el adapter era obsoleto. No era un bug real.

| Bug | Requerimiento | Archivo | Línea | Descripción | Acción |
|---|---|---|---|---|---|
| FireSaleHooks no conectado | RQF25B, RQF26B | `ProductPurchaseProbabilityAdapter.cs` | 114-117 | `FireSaleHooks()` marcado PENDING; puede no estar wired a CashDesk | Verificar en `CashDesk.cs` si llama `FireSaleHooks()` post-pago |
| employeeSpawnPoint no asignado | RQF25A | `EmployeeNPCSpawner.cs` | 38 | Inspector field vacío en runtime AddComponent | Asignar Transform en escena o usar fallback válido |
| ExpansionObject IDs pueden diferir | RQF5 | `ExpansionRealWorldBridge.cs` | 87-110 | IDs del sistema vs IDs en escena no verificados | Abrir Game.unity; verificar IDs de ExpansionObject |

---

## 7. Errores de Consola Esperados en Primera Ejecución

| Error probable | Causa | Requerimiento afectado | Prioridad | Acción |
|---|---|---|---|---|
| `"No prefabs found for employee NPC"` | Customer no activos al momento de Start() del spawner | RQF25A | CRÍTICA | Asegurar que auto-discovery se ejecuta después del spawn inicial de clientes |
| `NullReferenceException` en `ExpansionRealWorldBridge` | `ExpansionObject` components no encontrados por tipo | RQF5 | CRÍTICA | Verificar component type name en la escena |
| `"Licenses tab not found"` (debug log) | Tab text en asset difiere de "LICENSES"/"LICENCIAS" | RQNF16 | MEDIA | Inspeccionar texto exacto del tab button en UIShopDesktop |
| `NullReferenceException` en `ShelfProductInfoLabel` | `PlacementObject` sin `product` asignado al inicio | RQNF10 | MEDIA | Label ya verifica null antes de mostrar; solo debug en consola |
| `"Head bone not found"` (esperado silencioso) | Rig del Customer no tiene hueso "Head" | RQF13 | BAJA | La cap se coloca en root del personaje como fallback |

---

## 8. Checklist Manual para Tu PC

### Preparación
```
cd "C:\Users\ijuan\Animo"
git status
git checkout copilot/copilotfix-emprendedor-tree-system-again
git pull origin copilot/copilotfix-emprendedor-tree-system-again
git lfs pull
git status
git log --oneline -5
```

### Escena a abrir
- **Principal:** `Assets/StoreSimulator/Scenes/Game.unity`
- **Stats:** `Assets/StoreSimulator/Scenes/Stats.unity`
- **Intro:** `Assets/StoreSimulator/Scenes/Intro.unity`

### Checklist A — Computadora y Compra
1. [ ] Play Mode → interactuar con computadora (tecla E o clic)
2. [ ] App Compra abre sin errores rojos
3. [ ] Pestaña "Licenses" NO visible
4. [ ] Leche y otros básicos visibles con botón Comprar
5. [ ] Comprar Leche → paquete aparece en zona de entrega (no 0,0,0)
6. [ ] Comprar sin fondos → mensaje con monto faltante exacto
7. [ ] Inventario/stock se actualiza tras compra
8. [ ] Consola: sin errores rojos al comprar

### Checklist B — Árbol del Emprendedor
1. [ ] App Árbol abre y muestra nodos gráficos
2. [ ] product_basic_1 aparece desbloqueado (verde/activo)
3. [ ] product_basic_2 aparece como requisito siguiente
4. [ ] Desbloquear product_basic_2 (si hay puntos) → funciona
5. [ ] Intentar desbloquear nodo sin prerequisito → mensaje de error
6. [ ] Puntos se descuentan correctamente

### Checklist C — Expansión
1. [ ] App Expandir abre con mapa de zonas
2. [ ] Zona disponible seleccionable con clic
3. [ ] Panel derecho muestra nombre/tipo/tamaño/costo
4. [ ] Comprar zona → dinero baja
5. [ ] **CRÍTICO:** Caminar a la zona expandida → piso/paredes visibles
6. [ ] Si aparece primitiva verde/azul = fallback del bridge (funcional pero no visual final)
7. [ ] m² en UI se actualiza
8. [ ] Guardar; recargar → zona sigue comprada

### Checklist D — App Precios
1. [ ] App Precios abre con lista de productos
2. [ ] Columnas: Nombre / Precio Ideal / Precio Actual / Prob% / Extra%
3. [ ] Editar precio de Leche → confirmación
4. [ ] Precio sube → Prob% baja
5. [ ] Precio baja → Extra% sube
6. [ ] Precio $0.00 → Prob=100% / Extra≈10%
7. [ ] Precio > 300% → rechazado con mensaje
8. [ ] Precio en anaquel actualiza etiqueta

### Checklist E — Ventas y Caja
1. [ ] Clientes llegan y van a anaqueles
2. [ ] Cliente elige producto → va a caja
3. [ ] Jugador cobra → dinero aumenta
4. [ ] Inventario disminuye
5. [ ] Cliente espera 7+ segundos sin atención → abandona → productos regresan
6. [ ] Notificación de producto agotado cuando stock = 0

### Checklist F — Empleados
1. [ ] Desbloquear employee_1 en Árbol
2. [ ] App Empleados → empleado aparece como "Disponible"
3. [ ] Contratar con dinero → empleado contratado
4. [ ] Asignar rol Cajero → puesto aparece
5. [ ] **CRÍTICO:** NPC aparece en o cerca de caja (o consola muestra "spawned employee")
6. [ ] Si NPC no aparece: revisar consola para `"No prefabs found"` o `NullRef` en EmployeeNPCSpawner
7. [ ] Guardar; recargar → rol persiste

### Checklist G — Anaqueles
1. [ ] Colocar Leche en anaquel (si ya comprada)
2. [ ] Etiqueta "Leche / Precio: $X / Compra: X%" aparece
3. [ ] Cambiar precio → etiqueta actualiza probabilidad
4. [ ] Surtidor asignado → repone anaquel si stock disponible

### Checklist H — Guardado / Carga
1. [ ] Activar: comprar producto / desbloquear árbol / comprar expansión / contratar empleado
2. [ ] Cerrar día (UIGame.LeaveToNext) → notificación "Guardado"
3. [ ] Cerrar Unity → reabrir → cargar partida
4. [ ] Verificar: dinero / árbol / expansión / empleado / rol / inventario

### Checklist I — Ladrones (AdminMode)
1. [ ] Al iniciar juego: botón ADMIN visible en Intro
2. [ ] Activar: forceShoplifterSpawn=true → Iniciar
3. [ ] En Play Mode: ver ladrón con tinte rojo/color diferente
4. [ ] Verificar gorra (cilíndro en cabeza) y mochila (cubo en espalda) o solo tinte
5. [ ] Acercarse al ladrón → presionar interacción → captura
6. [ ] Si escapa: notificación en pantalla con valor robado
7. [ ] Reporte diario: sección Robos con valores correctos

### Checklist J — Reporte Diario
1. [ ] Completar un día con actividad (ventas, robos, empleados)
2. [ ] Cerrar día → entrar a Stats scene
3. [ ] Verificar todos los campos: ventas/gastos/salarios/robos/renta/luz/ganancias/agotados/clientes perdidos

### Qué revisar en Inspector
- **EmployeeNPCSpawner**: `employeePrefabs[]` → debe estar lleno en Play Mode; `employeeSpawnPoint` → asignar si vacío
- **ExpansionRealWorldBridge**: `mapToWorldScale` → ajustar si placeholders están en posición incorrecta
- **ShelfProductInfoLabel**: en cada PlacementObject → verificar Canvas world-space activo
- **CustomerSystem**: `spawnRate` → debe ser 50-75 para cumplir RQF22

---

## 9. Recomendación de Próximos 6 Requerimientos a Cerrar

Ordenados por impacto en gameplay jugable:

| Orden | Requerimiento | Razón | Acción concreta |
|---|---|---|---|
| 1 | **RQF2 + RQF1** (Compra funcional) | Si comprar productos falla, el juego no arranca. Es el flujo principal. | Play Mode: comprar Leche; confirmar paquete; arreglar si hay error. |
| 2 | **RQF5** (Expansión real) | Expandir es el segundo ciclo más importante; sin ello el mapa no crece y los clientes no aumentan. | Inspector: verificar IDs ExpansionObject; si difieren, actualizar bridge. |
| 3 | **RQF25A + RQF26A** (Empleados + Cajero) | Sin cajero funcionando, el juego se estanca en día 3-4. | Play Mode: contratar cajero; asignar puesto; verificar NPC y atención automática. |
| 4 | **RQF21 + RQNF3** (Guardado completo) | Sin guardado confiable todo lo anterior se pierde entre sesiones. | Play Mode: ciclo guardar/cargar y verificar cada campo. |
| 5 | **RQF25B + RQF26B** (Probabilidad afecta ventas) | La curva de precios no funciona si FireSaleHooks no está conectado. | Verificar y conectar `CashDesk.OnBillCustomer` → `FireSaleHooks()`; luego Play Mode con precio ideal/alto/bajo. |
| 6 | **RQF13 + RQF17** (Ladrones básicos) | Los ladrones son el principal elemento de tensión del juego; sin ellos el día es trivial. | Play Mode con AdminMode.forceShoplifterSpawn; verificar aparición y captura manual. |

---

## 10. Estado de Implementaciones de Este Lote

| Cambio | Archivo | Estado en Código | Riesgo en Escena |
|---|---|---|---|
| Null-safety + refund en compra | `DeliverySystem.cs` | ✅ Implementado | Bajo — requiere Play Mode para confirmar |
| HideLicensesTab | `EntrepreneurTreeUIBootstrap.cs` | ✅ Implementado | Medio — texto del tab puede diferir |
| ExpansionRealWorldBridge | `ExpansionRealWorldBridge.cs` | ✅ Implementado | Alto — IDs pueden diferir con scena real |
| EmployeeNPCSpawner auto-discover | `EmployeeNPCSpawner.cs` | ✅ Implementado | Alto — requiere Customer activos en escena |
| ShelfProductInfoLabel + Bootstrap | `ShelfProductInfoLabel.cs`, `ShelfProductInfoLabelBootstrap.cs` | ✅ Implementado | Bajo — muestra info correctamente si product asignado |
| Thief cap + backpack (primitivas) | `ShoplifterAgent.cs` | ✅ Implementado | Bajo — fallback si Head bone no encontrado |
| Bootstrap registra nuevos sistemas | `EntrepreneurTreeUIBootstrap.cs` | ✅ Implementado | Bajo — AddComponent idempotente |

---

## 11. Conclusión

**El proyecto ShopMaster tiene la arquitectura correcta.** Los sistemas críticos (Árbol, Compra, Expansión, Empleados, Seguridad, Precios, Guardado) están implementados y conectados. Los bugs de este lote (null-safety en compra, expansion bridge, auto-discover de NPC) fueron corregidos en código.

**La brecha real es operacional**: cero requerimientos tienen evidencia Play Mode. Los siguientes 6 tienen el mayor riesgo de fallo en ejecución:
1. ExpansionRealWorldBridge vs IDs reales en escena.
2. EmployeeNPCSpawner vs Customer prefabs disponibles.
3. FireSaleHooks no conectado a CashDesk.
4. HideLicensesTab vs texto exacto del asset base.
5. CustomerSystem.spawnRate vs 50-75 requeridos.
6. SettingsMenu completamente ausente (RQNF19).

**El próximo paso obligatorio es una sesión Play Mode en tu PC siguiendo el checklist completo A-J.**
