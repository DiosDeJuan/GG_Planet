# Reporte de Verificación Lote 2 - Empleados, Cajeros y Guardado

## 1. Resumen ejecutivo
- Fecha: 2026-05-18
- Rama: main
- Commit inicial: e18ab80
- Estado general: se corrigieron bugs funcionales directos en app de empleados, asignación de roles, cajeros automáticos y robustez de guardado/carga. Unity batchmode compila sin errores C# ni excepciones detectadas en log. No hubo Play Mode interactivo, por lo que ningún requisito se marca como verificado en gameplay.
- Resultado batchmode: exitoso. Log: `C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Lote2_Empleados_Guardado.log`.

## 2. Requerimientos trabajados

| Requerimiento | Estado inicial | Trabajo realizado | Estado final | Evidencia técnica | Evidencia Play Mode | Archivos modificados |
|---|---|---|---|---|---|---|
| RQF25-EMP | Parcial | Se revisó la app Empleados, contratación, bloqueo, fondos, estados y refresco. Se añadieron descripciones claras y logs. | PARCIAL | `EmployeeAppUIController` se inyecta desde `EntrepreneurTreeUIBootstrap`; `TryHireEmployee` valida desbloqueo, fondos y duplicados. | BLOQUEADO: no se operó computadora en Play Mode. | `Assets/UI/Computer/Employees/EmployeeAppUIController.cs`, `Assets/Systems/EntrepreneurTree/EntrepreneurEmployeeSystem.cs` |
| RQF26-EMP | Parcial | Se corrigió conflicto cajero automático vs jugador y se separó bono Carismático solo para ventas atendidas por cajero. | PARCIAL | `EmployeeCashierCoordinator` detecta cashiers; `CashDesk.TryStartAutomatedCheckout` procesa ventas con tiempos 1-12s y bloqueos. | BLOQUEADO: falta probar cliente real, tarjeta/efectivo y balance. | `Assets/StoreSimulator/Scripts/CashDesk.cs`, `Assets/Systems/EntrepreneurTree/EntrepreneurTreeUpgradeAdapter.cs` |
| RQF9 | Parcial | Se agregaron textos exactos de rol Cajero/Surtidor y validación de desbloqueo antes de asignar rol. | PARCIAL | `TryAssignRole` solo permite roles a empleados desbloqueados y contratados; roles se serializan en `SaveToJSON`. | BLOQUEADO: falta asignar/cambiar rol desde UI en Play Mode y recargar. | `Assets/UI/Computer/Employees/EmployeeAppUIController.cs`, `Assets/Systems/EntrepreneurTree/EntrepreneurEmployeeSystem.cs` |
| RQF21 | Parcial | Se verificó autosave de fin de día y se endureció `SaveGameSystem` con guardado por subsistema, logs y notificación. | PARCIAL | `UIGame.LeaveToNext` llama `SaveGameSystem.Save`; `dataSaveEvent` dispara integraciones de árbol/empleados/expansión/slots. | BLOQUEADO: falta finalizar día manualmente y confirmar archivos actualizados. | `Assets/StoreSimulator/Scripts/SaveGameSystem.cs` |
| RQNF3 | Parcial | Se endureció carga por subsistema para que un fallo parcial no rompa toda la carga. | PARCIAL | `SaveGameSystem.OnSceneLoaded` usa `SafeLoadComponent`; `EntrepreneurTreeSaveIntegration` restaura empleados/roles desde save complementario. | BLOQUEADO: falta ciclo guardar, reiniciar escena/cargar y confirmar estado real. | `Assets/StoreSimulator/Scripts/SaveGameSystem.cs`, `Assets/Systems/EntrepreneurTree/EntrepreneurEmployeeSystem.cs` |

## 3. RQF25-EMP

La app Empleados se crea dentro de la computadora existente desde `EntrepreneurTreeUIBootstrap.EnsureEmployeeTab`, sobre el panel `Empleados`, y añade `EmployeeAppUIController`. No se creó computadora nueva ni flujo paralelo.

`EmployeeAppUIController` crea tarjetas para 18 empleados y lee estado real desde `EntrepreneurEmployeeSystem.GetAssignment`, `IsEmployeeUnlocked`, `CanHireEmployee` y `TryHireEmployee`. Distingue:
- Bloqueado.
- Desbloqueado pero no contratado.
- Contratado sin rol o con rol asignado.

La contratación ocurre en `EntrepreneurEmployeeSystem.TryHireEmployee`. Valida:
- Empleado existente.
- Desbloqueo por Árbol del Emprendedor.
- Que no esté contratado.
- Fondos suficientes mediante `StoreDatabase.CanPurchase`.
- Descuento de dinero con `StoreDatabase.AddRemoveMoney(-hireCost)`.

Correcciones:
- Se añadió log `[EmployeeApp]` al contratar.
- Se mantuvo refresco de UI por eventos `onEmployeeHired`, `onEmployeeRoleChanged` y `SaveGameSystem.dataLoadEvent`.
- El costo se muestra con `StoreDatabase.FromLongToStringMoney`, que usa formato USD centralizado.

Resultado: PARCIAL. La ruta técnica está conectada y compila, pero falta abrir la computadora en Play Mode y ejecutar contratación real con fondos/duplicado/sin fondos.

## 4. RQF26-EMP

`EmployeeCashierCoordinator` consulta `EntrepreneurEmployeeSystem.GetRoleCount(EmployeeRole.Cashier)` cada 0.75s y busca `CashDesk` cacheados por escena. No usa búsquedas por frame.

El cajero toma clientes mediante `CashDesk.TryStartAutomatedCheckout`, que ahora exige:
- Caja no controlada por jugador.
- Caja no procesando otro checkout automático.
- Terminal/register no interactuables.
- Cliente con bolsa y productos ya colocados en conveyor.

El tiempo se calcula en `CashDesk.AutomatedCheckout`:
- `productos * 0.5s`
- tarjeta `+1.5s`
- efectivo `+2.5s`
- dividido por multiplicador de velocidad.
- `Mathf.Clamp(..., 1f, 12f)`.

Cafeína se aplica mediante `EntrepreneurTreeUpgradeAdapter.GetEmployeeSpeedMultiplier`, usado por `EmployeeCashierCoordinator.GetEmployeeSpeedMultiplier`.

Carismático se corrigió para ventas de cajero:
- `CashDesk.OnBillCustomer` detecta `cashierHandledSale`.
- `EntrepreneurTreeUpgradeAdapter.CreditSaleIncome(baseIncome, cashierHandledSale)` aplica +5% solo si `cashierHandledSale == true`.
- El fallback de bonus por eventos de dinero quedó desactivado por defecto para no bonificar ventas manuales.

Prevención de doble cobro/conflictos:
- `isAutomatedCheckoutInProgress` impide reentrar al mismo checkout.
- `CashDesk.Interact` rechaza interacción manual si el cajero ya está atendiendo.
- El cliente recibe `NotifyCheckoutServiceStarted`, deteniendo abandono de 7s.
- `OnBillCustomer` sigue limpiando `cart`, `customerBag` y cola una sola vez.

Resultado: PARCIAL. La lógica compila y está conectada, pero falta Play Mode con cliente real, pago tarjeta, pago efectivo, intento manual durante atención automática y validación de balance.

## 5. RQF9

Roles disponibles:
- Cajero.
- Surtidor.

Descripciones añadidas en `EmployeeAppUIController`:
- Cajero: Atiende clientes en cajas registradoras, procesa pagos automáticamente y reduce abandono por espera.
- Surtidor: Reabastece muebles de venta usando productos disponibles en almacén cuando existan espacios asignados.

Asignación backend:
- `EmployeeAppUIController.OnRoleClicked` llama `EntrepreneurEmployeeSystem.TryAssignRole`.
- `TryAssignRole` valida que el empleado exista, esté desbloqueado, esté contratado, que el rol no sea `None` y que no tenga ya ese rol.
- Al cambiar rol dispara `onEmployeeRoleChanged`.

Persistencia:
- `EntrepreneurEmployeeSystem.SaveToJSON` guarda `employeeId`, `isHired`, `role` y `hireCost`.
- `LoadFromJSON` restaura esos campos y ahora sanitiza roles inválidos.
- `EntrepreneurTreeSaveIntegration` guarda/carga `EntrepreneurEmployeeSystem` en el save complementario.

Resultado: PARCIAL. La UI/backend/persistencia están conectados, pero falta Play Mode para asignar Cajero/Surtidor, guardar, cargar y confirmar conservación.

## 6. RQF21

El autosave de fin de día ocurre en `UIGame.LeaveToNext`, llamado después de `DayCycleSystem.onDayFinished`, antes de cargar la escena de estadísticas:
- `SaveGameSystem.Save()`
- `SceneManager.LoadScene(nextScene)`

Sistemas guardados en save base:
- UISettings.
- ItemDatabase, incluyendo precios modificados de productos.
- StoreDatabase, incluyendo dinero, XP y nivel.
- DayCycleSystem.
- StorageSystem.
- DeliverySystem.
- DailyEventSystem.
- CustomerSystem.
- TutorialSystem.
- StatsDatabase.

Sistemas guardados por integraciones en `SaveGameSystem.dataSaveEvent`:
- Árbol del Emprendedor.
- Logros.
- Empleados contratados y roles.
- Ladrones/seguridad lógica.
- Expansiones.
- Slots/asignaciones.

Corrección de robustez:
- `SaveGameSystem.Save` ahora usa `SafeSaveComponent` por subsistema.
- Si un subsistema falla al serializar, se registra warning `[SaveSystem]` y el guardado continúa con un objeto vacío para ese bloque.
- `dataSaveEvent` se invoca con `InvokeEventSafe`, evitando que un listener rompa el resto.
- La escritura atómica con `.tmp` y `.bak` existente se conserva.
- Se mantiene notificación visual de guardado correcto o error.

Resultado: PARCIAL. El autosave está conectado y más robusto, pero falta finalizar un día en Play Mode y confirmar archivos actualizados con datos reales.

## 7. RQNF3

La carga base ocurre en `SaveGameSystem.Load`, que lee `save.dat` y registra `OnSceneLoaded`. Al cargar escena, `OnSceneLoaded` aplica datos antes de disparar `dataLoadEvent`.

Orden de carga base:
1. UISettings.
2. ItemDatabase.
3. StoreDatabase.
4. DayCycleSystem.
5. StorageSystem.
6. DeliverySystem.
7. DailyEventSystem.
8. CustomerSystem.
9. TutorialSystem.
10. StatsDatabase.
11. `dataLoadEvent` para integraciones ShopMaster.

Corrección de robustez:
- `SafeLoadComponent` protege cada subsistema individualmente.
- `InvokeEventSafe` protege listeners de carga.
- Si un componente falla, se loguea `[SaveSystem]` y los demás siguen cargando.

Sistemas complementarios:
- `EntrepreneurTreeSaveIntegration.OnLoad` restaura árbol, logros, empleados/roles y ladrones.
- `SupermarketExpansionSystem`, `ShelfProductSlotSystem` y `ProductInventorySystem` reaccionan a `dataLoadEvent`.
- `ProductPricingSystem` no requiere archivo propio porque precios se restauran desde `ItemDatabase`.

Pendiente:
- No se probó el ciclo real: guardar, reiniciar/cargar escena y confirmar dinero, día, precio, empleado, rol y cajero automático activo.

Resultado: PARCIAL.

## 8. Bugs encontrados y corregidos

| Bug | Causa | Archivo | Corrección | Verificado |
|---|---|---|---|---|
| Roles no tenían descripción explícita del documento | La app mostraba hints generales | `Assets/UI/Computer/Employees/EmployeeAppUIController.cs` | Se añadieron descripciones claras de Cajero y Surtidor | Batchmode + revisión estática |
| Rol podía asignarse sin revalidar desbloqueo | `TryAssignRole` validaba contratado pero no desbloqueo | `Assets/Systems/EntrepreneurTree/EntrepreneurEmployeeSystem.cs` | Se agregó validación `IsEmployeeUnlocked` antes de asignar rol | Batchmode + revisión estática |
| Roles cargados podían aceptar enteros inválidos | `LoadFromJSON` casteaba directo a enum | `Assets/Systems/EntrepreneurTree/EntrepreneurEmployeeSystem.cs` | Se agregó `SanitizeRole` | Batchmode + revisión estática |
| Jugador podía intentar usar caja mientras cajero automático atendía | `Interact` no revisaba `isAutomatedCheckoutInProgress` | `Assets/StoreSimulator/Scripts/CashDesk.cs` | Se bloquea interacción manual con mensaje | Batchmode + revisión estática |
| Carismático podía bonificar ingresos generales/manuales | Bonus por eventos de dinero estaba activo y `CreditSaleIncome` no distinguía cajero | `Assets/Systems/EntrepreneurTree/EntrepreneurTreeUpgradeAdapter.cs`, `Assets/StoreSimulator/Scripts/CashDesk.cs` | Bonus por eventos desactivado por defecto y overload para venta atendida por cajero | Batchmode + revisión estática |
| Guardado/carga podía romperse por fallo de un subsistema | `SaveGameSystem` serializaba/cargaba todo en llamadas directas | `Assets/StoreSimulator/Scripts/SaveGameSystem.cs` | Se agregaron `SafeSaveComponent`, `SafeLoadComponent` e `InvokeEventSafe` | Batchmode + revisión estática |

## 9. Bugs pendientes

| Bug | Requerimiento afectado | Motivo | Prioridad |
|---|---|---|---|
| Sin Play Mode de app Empleados | RQF25-EMP, RQF9 | No se operó computadora/laptop en este entorno | Crítico QA |
| Sin prueba real de cajero con tarjeta/efectivo | RQF26-EMP | Falta cliente real llegando a caja | Crítico QA |
| Sin validación de balance una sola vez | RQF26-EMP | Requiere observar venta automática completa | Crítico QA |
| Sin prueba guardar/cargar ciclo completo | RQF21, RQNF3 | Batchmode compila pero no ejecuta flujo de partida | Crítico QA |
| Autoasignación inicial de rol Cashier al contratar debe validarse con diseño | RQF9 | El backend conserva comportamiento previo: al contratar sin rol, asigna Cashier por defecto | Medio |

## 10. Resultado Unity batchmode

- Comando ejecutado:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\ijuan\Animo' -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Lote2_Empleados_Guardado.log'
```

- Log generado:
  - `C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Lote2_Empleados_Guardado.log`
- Resultado:
  - `Tundra build success`
  - `Exiting batchmode successfully now`
  - código de salida 0
- Errores encontrados:
  - 0 coincidencias para `error CS`.
  - 0 coincidencias para `Scripts have compiler errors`.
  - 0 coincidencias para `Compilation failed`.
  - 0 coincidencias para `NullReferenceException`.
  - 0 coincidencias para `MissingReferenceException`.
- Warnings observados:
  - TMP `enableWordWrapping` obsoleto en archivos existentes.
  - `FindObjectsOfType` obsoleto en `EntrepreneurTreeUpgradeAdapter`.
  - Warning de compatibilidad Render Graph de URP.
- Errores corregidos:
  - No aparecieron errores C# tras los cambios.

## 11. Estado final exacto

- RQF25-EMP: PARCIAL
- RQF26-EMP: PARCIAL
- RQF9: PARCIAL
- RQF21: PARCIAL
- RQNF3: PARCIAL

## 12. Siguiente lote recomendado

Prompt propuesto para LOTE 3, sin ejecutarlo:

```text
Continuar ShopMaster con LOTE 3 solamente. No hacer revisión general ni rediseño visual.

Trabaja exactamente estos requerimientos:
- RQF27-EMP: Surtidor automático y asignación de producto por espacio.
- RQNF6: Validar productos en mobiliario correcto.
- RQF29: Ventas descuentan inventario, suman balance y notifican agotados.
- RQF34: Reporte diario con ventas, gastos, salarios, robos, renta/luz, neto, agotados y clientes perdidos.
- RQNF4: Backup y protección ante fallo de guardado.

Antes de tocar código lee:
- Documentos/Reporte_Verificacion_Lote1_Precios_Caja.md
- Documentos/Reporte_Verificacion_Lote2_Empleados_Guardado.md
- Documentos/Reporte_Avance_Codex_Backend.md

Orden:
1. Revisar git status, rama y commit inicial.
2. Revisar EmployeeRestockCoordinator, ShelfProductSlotSystem, ProductInventorySystem, PlacementObject, StorageObject, CashDesk, CustomerCart, StatsDatabase y SaveGameSystem.
3. Corregir solo bugs de Lote 3.
4. Ejecutar Unity batchmode.
5. Crear Documentos/Reporte_Verificacion_Lote3_Surtido_Inventario_Reportes_Backup.md.

No marques VERIFICADO EN PLAY MODE si no se prueba manualmente en Play Mode.
```
