<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 3 - Empleados, Arbol, Laptop y NPCs

## Rama

codex/fase3-empleados-arbol-laptop-npcs

## Base

origin/codex/fase2-productos-compra-delivery-inventario

Commit base esperado: ddde8d6

## Documentos revisados

- `Documentos/NEW_Requerimientos.docx`: RQF8, RQF9, RQF21, RQF25, RQF26, RQF27, RQF34, tabla de Empleados 1 a 18 y guardado.
- `Documentos/propuestas juanito (2).docx`: laptop/oficina, app Empleados, app Compra, Arbol del Emprendedor, paquetes y modo constructor.
- `Documentos/Protocolo prpuesta Juanito (1).pdf`: objetivo general de ShopMaster, gestion de supermercado, progresion y administracion.
- `Documentos/Guia Gantt (1).pdf`: guia de planificacion; no agrega reglas funcionales nuevas para empleados.
- `Documentos/desktop.ini`: revisado como archivo de sistema, sin contenido funcional para la fase.

## Reportes previos revisados

- `Reportes/Fase1_Arbol_Emprendedor_UI_Computadora.md`
- `Reportes/Codex_Fase2_Productos_Compra_Delivery_Inventario.md`
- `Reportes/Dotnet_Fase1_Arbol_Build.log`
- `Reportes/Unity_Fase1_Arbol_Compile_Attempt.log`
- `Reportes/Unity_Fase2_Productos_Compra_Delivery_Inventario.log`

## Objetivo

Se implemento gestion de empleados desde la laptop/computadora existente, usando el Arbol del Emprendedor como fuente de verdad para desbloqueos. La fase agrega contratacion, roles Cajero y Surtidor, NPC visible reutilizando prefabs de clientes, guardado/carga de empleados, automatizacion de caja y surtido con sistemas reales, y salario diario.

## Sistemas existentes encontrados

- UI laptop/computadora: `UIShopDesktop`, `UIShopCategoryHelper`, botones y categorias existentes.
- Arbol: `EntrepreneurProgress`, `EntrepreneurTreeDefinitions`, `EntrepreneurTreeUI` y bootstrap ya existentes de Fase 1.
- Guardado: `SaveGameSystem` con `SimpleJSON`, junto a `StoreDatabase`, `ItemDatabase`, `StorageSystem`, `DeliverySystem`, `StatsDatabase` y `EntrepreneurProgress`.
- Caja/checkout: `CashDesk`, `CheckoutObject`, `UICashDeskCart`, `UICashDeskTerminal`, `UICashDeskRegister`, `PaymentItem`.
- Clientes: `Customer`, `CustomerCart`, `CustomerSystem`, `CustomerAgent`.
- Storage/inventario/productos: `PackageObject`, `PlacementObject`, `StorageObject`, `PlacementSystem`, `StoreDatabase`, `ItemDatabase`.
- Dia/reporte/economia: `DayCycleSystem`, `StatsDatabase`, `UIStats`, `StoreDatabase.AddRemoveMoney`.
- Prefabs/NPCs: prefabs reales `Assets/StoreSimulator/Prefabs/Customers/Customer_A.prefab` a `Customer_E.prefab`.

## Sistemas reutilizados

- Navegacion y paneles de laptop de `UIShopDesktop`.
- Nodos de Empleados 1 a 18 ya definidos en `EntrepreneurTreeDefinitions`.
- Estado de desbloqueo desde `EntrepreneurProgress`.
- Flujo real de caja de `CashDesk`.
- Stock fisico real de `PackageObject` y muebles `PlacementObject`.
- Prefabs visuales reales de clientes para NPCs de empleados.
- Economia real por `StoreDatabase.AddRemoveMoney`.
- Reporte diario existente por `StatsDatabase` y `UIStats`.

## Archivos modificados

- `Assembly-CSharp.csproj`
- `Assets/StoreSimulator/Scripts/CashDesk.cs`
- `Assets/StoreSimulator/Scripts/PlayerController.cs`
- `Assets/StoreSimulator/Scripts/SaveGameSystem.cs`
- `Assets/StoreSimulator/Scripts/StatsDatabase.cs`
- `Assets/StoreSimulator/Scripts/UIShopDesktop.cs`
- `Assets/StoreSimulator/Scripts/UIStats.cs`

## Archivos nuevos

- `Assets/StoreSimulator/Scripts/Employees.meta`
- `Assets/StoreSimulator/Scripts/Employees/EmployeeRole.cs`
- `Assets/StoreSimulator/Scripts/Employees/EmployeeRole.cs.meta`
- `Assets/StoreSimulator/Scripts/Employees/EmployeeState.cs`
- `Assets/StoreSimulator/Scripts/Employees/EmployeeState.cs.meta`
- `Assets/StoreSimulator/Scripts/Employees/EmployeeManager.cs`
- `Assets/StoreSimulator/Scripts/Employees/EmployeeManager.cs.meta`
- `Assets/StoreSimulator/Scripts/Employees/EmployeeRuntimeAgent.cs`
- `Assets/StoreSimulator/Scripts/Employees/EmployeeRuntimeAgent.cs.meta`
- `Assets/StoreSimulator/Scripts/Employees/UIEmployeesPanel.cs`
- `Assets/StoreSimulator/Scripts/Employees/UIEmployeesPanel.cs.meta`
- `Assets/StoreSimulator/Scripts/Employees/UIEmployeesUIBootstrap.cs`
- `Assets/StoreSimulator/Scripts/Employees/UIEmployeesUIBootstrap.cs.meta`
- `Reportes/Codex_Fase3_Empleados_Arbol_Laptop_NPCs.md`

## Cambios realizados

### Arbol del Emprendedor

Se verifico que los nodos `empleado_1` a `empleado_18` ya existian y respetaban la tabla requerida. No se duplicaron nodos. La app Empleados consulta `EntrepreneurProgress.IsUnlocked`.

### App Empleados

Se agrego `UIEmployeesUIBootstrap` desde `UIShopDesktop`, creando una pestana "Empleados" dentro de la navegacion existente. `UIEmployeesPanel` lista 18 empleados, muestra Bloqueado/Disponible/Contratado, requisito faltante, salario diario, descripcion de roles y botones de contratar/asignar rol.

### Contratacion

`EmployeeManager.TryHire` valida que el nodo de empleado este desbloqueado, evita doble contratacion y no inventa costo inicial porque no se encontro costo de contratacion propio en el asset.

### NPC visible

Al contratar se instancia un NPC visible reutilizando prefabs de `CustomerSystem.customerPrefabs`. Se deshabilitan comportamientos de cliente para no romper IA de clientes y se agrega `EmployeeRuntimeAgent`.

### Rol Cajero

`CashDesk` ahora puede recibir un empleado asignado. Si tiene cajero automatico, escanea productos del cliente y factura con el total real del carrito, reutilizando `Scan`, `ProceedPayment`, `HasPaid` y `OnBillCustomer`. No se crea venta paralela ni dinero por fuera.

Tiempo aplicado: `0.5s * productos + 1.5s tarjeta / 2.5s efectivo`, ajustado por Cafeina y limitado entre 1 y 12 segundos.

### Rol Surtidor

El surtidor busca muebles `PlacementObject` ya configurados con producto y espacio disponible. Luego mueve un item real desde un `PackageObject` del mismo producto hacia el mueble usando `PlacementObject.Add`, `PackageObject.Remove` e `InteractionSystem.MoveToTargetArc`. No instancia productos gratis.

### Guardado/carga

`SaveGameSystem` guarda y carga `EmployeeManager`. Se persisten empleados contratados, rol y puesto de caja si aplica. Saves antiguos cargan con lista vacia.

### Salarios/reporte diario

`EmployeeManager` descuenta $60/dia por empleado contratado al cerrar el dia con `DayCycleSystem.onDayFinished`. Se protege contra doble cobro por dia y se registra `employeeSalarySpent` en `StatsDatabase`. `UIStats` muestra el gasto de salarios junto al dinero gastado si existe.

### Notificaciones

Se agregaron mensajes claros para empleado bloqueado, contratado, ya contratado, rol cajero, rol surtidor, sin cajas, sin producto en almacen, mueble sin producto asignado y puesto no valido.

## Requerimientos cubiertos

| Requerimiento | Estado | Nota |
| --- | --- | --- |
| RQF8 | Cubierto | Empleados 1 a 18 se desbloquean desde nodos existentes del Arbol. |
| RQF9 | Cubierto | App Empleados asigna Cajero o Surtidor. |
| RQF21 | Cubierto | Guardado/carga de empleados y roles agregado sin romper sistemas previos. |
| RQF25 | Cubierto | App muestra bloqueados, disponibles, contratados, contratar y asignar roles. |
| RQF26 | Parcial | Cajero automatiza `CashDesk` real; pendiente prueba Play Mode de flujo completo con cliente. |
| RQF27 | Parcial | Surtidor usa `PackageObject` y `PlacementObject` reales; UI de asignacion nueva de slots queda limitada al producto ya asignado del mueble. |
| RQF34 | Parcial | Salario diario integrado a cierre de dia y stats; pendiente QA visual del reporte en Unity. |
| RQF36 | Cubierto | El Arbol mantiene prerequisitos y estados por `EntrepreneurProgress`. |
| RQNF3 | Cubierto | No se agregan sistemas paralelos de dinero, venta ni inventario. |
| RQNF7 | Cubierto | Validaciones de bloqueo, contratacion, caja y stock. |
| RQNF8 | Cubierto | Estados se derivan del Arbol. |
| RQNF9 | Cubierto | Estados visibles en texto. |
| RQNF16 | Cubierto | Mensajes especificos en espanol. |
| RQNF17 | Cubierto | Reutiliza UI de laptop y estilos de panel/boton existentes. |
| RQNF18 | Cubierto | Integrado dentro de computadora/laptop existente. |

## Validaciones realizadas

- `git status --short --branch -- SHOP_MASTER_FINAL`: cambios limitados a `SHOP_MASTER_FINAL`.
- `git diff --check -- SHOP_MASTER_FINAL`: sin errores; solo avisos CRLF esperados por Git.
- `dotnet build .\SHOP_MASTER_FINAL.sln`: correcto, 0 advertencias, 0 errores.
- Unity batchmode:
  - Ejecutable encontrado: `C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe`.
  - Intentado con `-batchmode -quit -projectPath "C:\Users\ijuan\SHOP_MASTER_FINAL" -logFile "C:\Users\ijuan\SHOP_MASTER_FINAL\Reportes\Codex_Fase3_UnityBatchmode.log"`.
  - Resultado: salida `-2147483645` y no se genero log dedicado. `Editor.log` mostro recarga de ensamblados sin errores CS, pero el resultado no permite declarar Unity batchmode en verde.

## Play Mode manual

Pendiente de probar por Isaac en Unity local.

## Inscripcion POMPIC

Se agrego inscripcion en todos los `.cs`, `.csproj` y `.md` modificados o nuevos de esta fase.

No se agrego inscripcion a archivos `.meta` porque podria romper formato/importacion de Unity.

## Riesgos o pendientes

- Validar en Play Mode que el boton "Empleados" abre correctamente desde la laptop.
- Validar contratacion visual y posicion de NPC en la escena real.
- Validar cajero automatico con clientes reales en cola.
- Validar surtidor con paquetes existentes y muebles ya configurados.
- Validar que el texto adicional de salarios en `UIStats.moneySpent` no desborde en la escena de reporte.
- Unity batchmode no quedo certificado en verde por salida temprana sin log dedicado, aunque `dotnet build` compila correctamente.

## Confirmacion Animo

No se modifico, no se recreo y no se reintrodujo `Animo/`. El staging debe mantenerse limitado a `SHOP_MASTER_FINAL`.
