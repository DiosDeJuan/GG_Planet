# Reporte Fase 5RQ - Integracion PlayMode

Fecha: 2026-06-03

## 1. Resumen ejecutivo

Se corrigio el bloqueador de compilacion en `ShopMasterWarehouseAndNPCVisualAuditRunner`: el runner ya no accede a `ItemDatabase.products` y usa la API real `ItemDatabase.GetByType(typeof(ProductScriptableObject))`.

Unity compila en batchmode con codigo 0. Play Mode quedo probado con evidencia verde para computadora/catalogo, compra con fondos, arbol, expansion y surtidor/anaquel real. La auditoria visual adicional de almacen/NPC aun deja riesgos de NavMesh/ruta al `DeliveryStart`, aunque el flujo RQF27 principal si paso con stock real en anaquel.

## 2. Error inicial de compilacion y solucion aplicada

Error inicial:

`Assets/StoreSimulator/Editor/ShopMasterWarehouseAndNPCVisualAuditRunner.cs`

`'ItemDatabase' does not contain a definition for 'products'`

Solucion:

- Reemplace `db.products` por `ItemDatabase.GetByType(typeof(ProductScriptableObject))`.
- Filtre cada entrada a `ProductScriptableObject`.
- Agregue fallback seguro al primer producto valido.
- Corregi errores de runners editor por `JSONNode` ambiguo usando `var`.
- Agregue `System.Collections.Generic` donde el runner usa `List<>`.

Evidencia:

- `Documentos/Unity_Fase_5RQ_Compile.log`
- Log final: `Exiting batchmode successfully now!` y `return code 0`.

## 3. Archivos modificados

- `Assets/StoreSimulator/Editor/ShopMasterWarehouseAndNPCVisualAuditRunner.cs`
- `Assets/StoreSimulator/Editor/ShopMasterEntrepreneurTreePointsAuditRunner.cs`
- `Assets/StoreSimulator/Editor/ShopMasterEntrepreneurTreeFullAuditRunner.cs`
- `Assets/StoreSimulator/Editor/ShopMasterExpansionPlayModeAuditRunner.cs`
- `Assets/StoreSimulator/Scenes/Game.unity`

## 4. Requerimientos trabajados

### RQF1 - Computadora y catalogo

Documento: la computadora debe abrir UI funcional, catalogo con disponibles/bloqueados y causas claras.

Antes: existian `UIShopDesktop`, `OrdersAppUIController`, `ComputerUITheme` y bootstrap del asset.

Corregido/conectado: se valido la computadora real y el panel COMPRA real. El catalogo usa `ItemDatabase.GetByType`, `EntrepreneurTreeProductUnlockAdapter`, estados bloqueados y botones deshabilitados por falta de dinero.

Sistemas reutilizados: `UIShopDesktop`, `OrdersAppUIController`, `ComputerUITheme`, `EntrepreneurTreeProductUnlockAdapter`, `ItemDatabase`.

Prueba Play Mode: `Unity_PlayMode_Auditoria_Arbol_Emprendedor_Fase2.log`.

Estado: VERDE.

Evidencia: `PASS: RQF3 player flow opens the real laptop interactable`, `PASS: Real computer tab button 'COMPRA' activates panel 'Compra'`, `PASS: Orders UI controller is attached to active panel`.

### RQF2 - Compra con fondos y mensajes claros

Documento: comprar solo con dinero suficiente; si falta dinero mostrar monto faltante; compra descuenta dinero y genera inventario/paquete real.

Antes: `DeliverySystem.Purchase` y `OrdersAppUIController` ya estaban conectados, pero la fase necesitaba compilacion y prueba real.

Corregido/conectado: se verifico compra desde UI, mensaje de fondos insuficientes y paquete real por DeliverySystem.

Sistemas reutilizados: `OrdersAppUIController`, `DeliverySystem`, `StoreDatabase`, `PackageObject`, `UIGame`.

Prueba Play Mode: `Unity_PlayMode_Auditoria_Arbol_Emprendedor_Fase2.log`.

Estado: VERDE.

Evidencia: `PASS: RQNF7 insufficient funds click shows a clear UI message`, `PASS: RQNF9 real UI purchase creates a delivery package`, `PASS: RQNF7 real UI purchase deducts exact cost`. Log Unity: `[Orders] Fondos insuficientes: faltan $6.30`.

### RQF3 - Arbol del Emprendedor real

Documento: accesible desde computadora; nodos con estado/costo/requisito/beneficio; desbloqueos afectan catalogo y empleados.

Antes: existian `EntrepreneurTreeManager`, `UpgradesUIController`, `NodeUI`, `ConnectionLineUI`, adapters de productos/empleados.

Corregido/conectado: se probo abrir arbol desde computadora, estados bloqueado/disponible/desbloqueado, intento sin requisito y desbloqueo real de `product_basic_2`.

Sistemas reutilizados: `UpgradesUIController`, `NodeUI`, `ConnectionLineUI`, `EntrepreneurTreeManager`, `EntrepreneurTreeProductUnlockAdapter`, `EntrepreneurEmployeeSystem`.

Prueba Play Mode: `Unity_PlayMode_Auditoria_Arbol_Emprendedor_Fase2.log`.

Estado: VERDE.

Evidencia: `PASS: RQF3 real Open Entrepreneur Tree button exists`, `PASS: RQF36 blocked node click cannot skip prerequisite or spend points`, `PASS: RQF4 real node click unlocks, spends one point and renders unlocked state`, `PASS: RQF25 real employee app open button works`.

### RQF5 - Expansion con mapa y escena real

Documento: app desde computadora, mapa visual, compra con dinero, estado actualizado y cambio real en escena.

Antes: existian `ExpansionAppUIController`, `ExpansionMapRenderer`, `SupermarketExpansionSystem` y `ExpansionRealWorldBridge`.

Corregido/conectado: se agrego `ShopMasterExpansionPlayModeAuditRunner` para probar el flujo en Play Mode. La prueba abre computadora, entra a EXPANDIR, selecciona `sales_w1`, compra la zona, verifica dinero, estado comprado y activacion de zona real.

Sistemas reutilizados: `ExpansionAppUIController`, `ExpansionMapRenderer`, `SupermarketExpansionSystem`, `ExpansionRealWorldBridge`, `StoreDatabase`.

Prueba Play Mode: `Unity_PlayMode_Auditoria_Expansion_Fase5RQ.log`.

Estado: VERDE.

Evidencia: `PASS: EXPANDIR tab activates the real expansion panel`, `PASS: Buying expansion changes zone state to Purchased`, `PASS: Buying expansion deducts exact money from StoreDatabase`, `PASS: Buying expansion activates or creates a real scene zone`, `Audit finished. Failures=0`.

### RQF27 - Surtidor, anaqueles y stock real

Documento: empleado surtidor, asignacion de producto a anaquel, stock real, reposicion desde paquete/almacen, sin inventario paralelo.

Antes: existian `ShelfProductSlotSystem`, `ProductInventorySystem`, `EmployeeRestockCoordinator`, `EmployeeNPCSpawner`, `EntrepreneurEmployeeSystem`.

Corregido/conectado: se ajusto la auditoria de almacen/NPC para preparar progreso, fondos y rol `Restocker`. Se persistio `WarehouseZone` en `Game.unity` y se ejecuto validacion NavMesh. El runner principal ya valida asignacion y reposicion real.

Sistemas reutilizados: `ShelfProductSlotSystem`, `PlacementObject`, `PackageObject`, `EmployeeRestockCoordinator`, `EmployeeNPCSpawner`, `EntrepreneurEmployeeSystem`, `StoreDatabase`.

Prueba Play Mode: `Unity_PlayMode_Auditoria_Arbol_Emprendedor_Fase2.log` y `Unity_PlayMode_Auditoria_Almacen_NPC_Visual.log`.

Estado: VERDE para stock/anaquel/reposicion real; AMARILLO para ruta visual completa al `DeliveryStart`.

Evidencia verde: `PASS: RQF27 compatible empty shelf placement exists`, `PASS: RQF27 product assignment to real shelf slot succeeds`, `PASS: RQF27 restocker task moves one real package item into assigned shelf`.

Evidencia amarilla: `Unity_PlayMode_Auditoria_Almacen_NPC_Visual.log` reporta `NPC 'Employee_1_NPC' no puede navegar al almacén/DeliveryStart` y `LastTaskCompletedVisualRoute=false`.

## 5. Evidencia

- Compilacion: `Documentos/Unity_Fase_5RQ_Compile.log`
- Computadora/arbol/compra/surtidor real: `Documentos/Unity_PlayMode_Auditoria_Arbol_Emprendedor_Fase2.log`
- Expansion RQF5: `Documentos/Unity_PlayMode_Auditoria_Expansion_Fase5RQ.log`
- Almacen/NPC visual: `Documentos/Unity_PlayMode_Auditoria_Almacen_NPC_Visual.log`
- Warehouse setup: `Documentos/Unity_WarehouseSceneSetup.log`
- NavMesh: `Documentos/Unity_NavMeshRebuild.log`

## 6. Errores restantes

- `ShopMasterWarehouseAndNPCVisualAuditRunner` queda con `Failures=6` por baseline de runners previos no encontrados y ruta incompleta al `DeliveryStart`.
- `WarehouseZoneBootstrap` todavia puede encontrar por keyword un hijo si no se endurece como el runner; en logs aparece `Using existing scene WarehouseZone: 'WarehouseWideDoor'`.

## 7. Riesgos tecnicos

- El rebake automatico de NavMesh no ejecuto `NavMeshSurface.BuildNavMesh()` porque el simbolo `UNITY_AI_NAVIGATION` no estaba disponible en este contexto; el runner valido cobertura cercana, pero recomienda bake manual en Inspector si se necesita ruta exacta al almacen.
- `ExpansionRealWorldBridge` puede activar `ExpansionObject` real si matchea ID, o crear placeholder; en la prueba activo un `ExpansionObject` real via expansion `1`.
- La auditoria de surtidor funcional esta verde, pero la auditoria visual de recorrido completo al pickup necesita una fase dedicada de NavMesh/warehouse.

## 8. Siguiente fase recomendada

Fase siguiente: corregir `WarehouseZoneBootstrap.FindWarehouseZone()` para preferir el root exacto `WarehouseZone`, rebakear NavMesh desde el Inspector o via `NavMeshSurface` con define/import correcto, y repetir `ShopMasterWarehouseAndNPCVisualAuditRunner` hasta `Failures=0`.
