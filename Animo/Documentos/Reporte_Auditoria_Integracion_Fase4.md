# Reporte Auditoria Integracion ShopMaster - Fase 4
## Resumen Ejecutivo

La Fase 4 avanzo la integracion sin reiniciar el proyecto ni degradar los verdes previos.

Se cerro en VERDE `RQF27`: el surtidor contratado usa su NPC visible real, recorre NavMesh hasta almacen, recoge una unidad, camina al anaquel compatible, coloca el producto y descuenta exactamente una unidad del paquete. No hubo timeout, teleport ni duplicacion.

Durante la auditoria final se detecto una regresion reproducible de batchmode al cerrar la computadora: `Mouse.current` era nulo en `PlayerController.SetMovementState()`. Se agrego un guard minimo y la repeticion final paso sin excepciones.

El proyecto queda **LISTO CON AMARILLOS DOCUMENTADOS**. No quedan ROJOS ni errores criticos, pero se recomienda una Fase 5 de QA jugable/manual antes de considerar cerrados reload integral, expansion visual, pagos manuales, cierre diario, resoluciones y ajustes persistentes.

## Baseline Fase 4

- Git desde `C:\Users\ijuan\Animo`: fallo ambiental exacto `fatal: cannot change to 'C:/Users/ijuan'`.
- Git con `git -C "C:\Users\ijuan\Animo"`: mismo fallo ambiental exacto.
- `dotnet build .\Animo.sln --nologo`: correcto, `0` warnings, `0` errores.
- Unity batchmode baseline: correcto.
- Runner Fase 3 baseline repetido: `Audit finished. Failures=0`.
- Regresiones previas detectadas antes de editar: ninguna.
- Regresion detectada por el nuevo runner: `Mouse.current == null` al cerrar computadora en batchmode. Corregida y repetida con `Failures=0`.

## Estado General

| Requerimiento | Area | Estado Fase 3 | Estado Fase 4 | Evidencia real | Prueba ejecutada | Archivos modificados | Observaciones |
| --- | --- | --- | --- | --- | --- | --- | --- |
| RQF12 | Seguridad | VERDE | VERDE | Runner Fase 3 repetido sin regresion | Play Mode real | Ninguno en Fase 4 | Se conserva prerequisito 7/8/14 y visuales 33/66/99 |
| RQF26 | Caja automatica | VERDE | VERDE | Runner Fase 3 repetido sin regresion | Tarjeta `1.75s`, efectivo `2.99s` en evidencia previa | Ninguno en Fase 4 | Sin regresion |
| RQNF4 | Backup atomico | VERDE | VERDE | Runner Fase 3 repetido sin regresion | Save real temporal y recuperacion `.bak` | Ninguno en Fase 4 | Sin regresion |
| RQF27 | Surtidor | AMARILLO | VERDE | NPC recorrio `44.63m` a almacen y `15.35m` a anaquel; pickup, colocacion y descuento real | Play Mode real en `Game.unity` | `EmployeeRestockCoordinator.cs`, runner Fase 3 | Sin timeout ni teleport |
| RQF21 / RQNF3 | Save/load integral | AMARILLO | AMARILLO | Backup atomico verde | Runner Fase 3 y Fase 4 | Ninguno en Fase 4 | Falta unload/reload completo de escena con reconstruccion integral |
| RQF5 / RQF6 / RQF7 | Expansion | AMARILLO | AMARILLO | Backend, costos y bloqueos existentes | Smoke runtime Fase 4 | Ninguno en Fase 4 | Falta compra UI completa, zona real navegable y reload persistido |
| RQF22 | Clientela por expansion | AMARILLO | AMARILLO | Bonus principal de `15%` verificado en escena | Smoke runtime Fase 4 | Ninguno en Fase 4 | Falta observar planner diario y clientes en zona nueva |
| RQF23 / RQF24 | Pagos manuales | AMARILLO | AMARILLO | Caja manual real presente | Smoke runtime Fase 4 | Ninguno en Fase 4 | Falta operar terminal y denominaciones desde UI con cliente real |
| RQF28 / RQF29 / RQNF10-RQNF15 | Precios y ventas | AMARILLO | AMARILLO | Sistema de precios presente y formulas previamente auditadas | Smoke runtime Fase 4 | Ninguno en Fase 4 | Falta venta real alterada desde input UI |
| RQF34 / RQF35 / RQNF5 | Reporte y espera | AMARILLO | AMARILLO | `StatsDatabase` real presente y codigo de espera a 7 segundos integrado | Smoke runtime Fase 4 | Ninguno en Fase 4 | Falta cierre de dia y abandono observado |
| RQNF6 | Muebles compatibles | AMARILLO | AMARILLO | Surtidor uso slot compatible real | Play Mode real | Ninguno en Fase 4 | Falta intento manual invalido con notificacion visible |
| RQNF16 / RQNF17 / RQNF18 | UI | AMARILLO | AMARILLO | Computadora abre por interaccion real y existen botones activos | Smoke runtime Fase 4 | `PlayerController.cs` | Falta QA visual multi-resolucion |
| RQNF1 / RQNF2 | Rendimiento y compatibilidad | AMARILLO | AMARILLO | Muestra editor: promedio `1402.03`, minimo `23.71`, maximo `2325.03`, `8469` frames | Runner Fase 4 | Ninguno | Muestra basica no certifica hardware minimo ni carga completa |
| RQNF19 | Ajustes | AMARILLO | AMARILLO | UI de ajustes presente | Smoke runtime Fase 4 | Ninguno en Fase 4 | Falta persistencia de resolucion, volumen, brillo y controles |

## Cierres de AMARILLOS

### RQF27

**Cerrado en VERDE.** La causa raiz no era un NPC congelado: el timeout fijo de `20s` expiraba antes de completar el trayecto largo desde el spawn de cliente hasta almacen. La telemetria demostro movimiento real continuo.

Se cambio la espera para calcular un presupuesto proporcional a la ruta NavMesh cuando `pathPending` termina. El coordinador actualiza animacion, registra distancia y mantiene timeout como proteccion.

### Pendientes conservados

- `RQF21/RQNF3`: falta reload real integral.
- `RQF5/RQF6/RQF7/RQF22`: falta UI, zona real y clientela observada.
- `RQF23/RQF24`: falta manejo manual de tarjeta y efectivo.
- Precios/probabilidad: falta venta real modificada por input.
- `RQF34/RQF35/RQNF5`: falta cierre de dia y abandono observado.
- `RQNF6`: falta rechazo visual manual de mueble invalido.
- `RQNF16/RQNF17/RQNF18`: falta QA visual por resolucion.
- `RQNF1/RQNF2/RQNF19`: falta hardware representativo y persistencia de ajustes.

## Flujo Surtidor

Stock real -> NPC visible contratado -> `GoingToStorage` -> recorrido `44.63m` -> `PickingStock` -> `GoingToShelf` -> recorrido `15.35m` -> `PlacingProduct` -> anaquel aumenta una unidad -> paquete disminuye una unidad -> `ReturningOrIdle`.

No se uso teleport como flujo normal. No se duplico NPC ni producto.

## Flujo Guardado/Carga

Fase 3 dejo VERDE save temporal atomico y recuperacion `.bak`. Fase 4 no marca VERDE reconstruccion integral: falta estado creado -> save temporal -> unload escena -> reload `Game.unity` -> load -> reconstruccion completa -> validacion de duplicados.

## Flujo Expansion

Computadora -> app Expandir -> backend de compra -> costos -> mapa/zona -> clientela -> persistencia.

La integracion backend existe y conserva `15%` de incremento de demanda por expansion de venta. Persiste la contradiccion documental con la tabla de `10%`; se mantiene `15%` porque lo exige el texto principal. Falta QA jugable completo.

## Flujo Pagos Manuales

Cliente -> caja -> tarjeta/efectivo -> UI -> cambio -> venta -> balance -> inventario.

La caja real existe y la caja automatica sigue VERDE. Los clicks manuales de terminal y denominaciones siguen pendientes.

## Flujo Precios

Precio ideal -> alto -> bajo -> cero -> probabilidad -> venta real -> balance/inventario.

La app y formulas siguen integradas. Falta ejecutar la venta completa con precio modificado desde input UI real.

## Flujo Reporte Diario

Ventas -> gastos -> robos -> clientes perdidos -> agotados -> neto -> guardado -> siguiente dia.

`StatsDatabase` existe en escena. Falta cierre real de dia con datos visibles y continuidad observada.

## UI y Resoluciones

El runner abrio la computadora por `UIShopDesktop.Interact("LeftClick")`, encontro botones activos y cerro la UI real. Se corrigio el cierre en batchmode cuando no existe mouse activo.

Pendiente QA visual a `1280x720`, `1366x768` y `1920x1080`.

## Rendimiento y Ajustes

Muestra basica de FPS tomada en editor batchmode durante seis segundos:

- Promedio: `1402.03 FPS`.
- Minimo: `23.71 FPS`.
- Maximo: `2325.03 FPS`.
- Frames: `8469`.

Esta muestra no certifica `RQNF1/RQNF2`: no representa hardware minimo ni escenario cargado con clientes, robos y pagos. `RQNF19` conserva pendiente de persistencia.

## Archivos Modificados

- `Assets/Systems/EntrepreneurTree/EmployeeRestockCoordinator.cs`: timeout NavMesh adaptativo, telemetria de llegada y animacion visual del empleado.
- `Assets/StoreSimulator/Editor/ShopMasterFullRequirementsPhase3Runner.cs`: ventana suficiente para recorrido fisico largo.
- `Assets/StoreSimulator/Scripts/PlayerController.cs`: guard para `Mouse.current == null`.
- `Assets/StoreSimulator/Editor/ShopMasterFinalIntegrationPhase4Runner.cs`: nuevo auditor de integracion Fase 4.
- `Assets/StoreSimulator/Editor/ShopMasterFinalIntegrationPhase4Runner.cs.meta`: metadata generada por Unity.

## Logs Generados

- `C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Fase4_Baseline.log`
- `C:\Users\ijuan\Animo\Documentos\Unity_PlayMode_Fase4_Baseline_Fase3Runner.log`
- `C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Integracion_Fase4.log`
- `C:\Users\ijuan\Animo\Documentos\Unity_PlayMode_Auditoria_Integracion_Fase4.log`
- `C:\Users\ijuan\Animo\Documentos\Unity_PlayMode_Integracion_Fase4_Editor.log`
- `C:\Users\ijuan\Animo\Documentos\Unity_PlayMode_Fase4_RestockFix_Fase3Runner.log`
- `C:\Users\ijuan\Animo\Documentos\Reporte_Auditoria_Integracion_Fase4.md`

## Comandos Usados

```powershell
git status --short --branch
git diff --stat
git -C "C:\Users\ijuan\Animo" status --short --branch
git -C "C:\Users\ijuan\Animo" diff --stat
dotnet build .\Animo.sln --nologo
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\ijuan\Animo' -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Fase4_Baseline.log'
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' -batchmode -projectPath 'C:\Users\ijuan\Animo' -executeMethod FLOBUK.StoreSimulator.Editor.ShopMasterFullRequirementsPhase3Runner.Run -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_PlayMode_Fase4_Baseline_Fase3Runner.log'
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\ijuan\Animo' -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Integracion_Fase4.log'
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' -batchmode -projectPath 'C:\Users\ijuan\Animo' -executeMethod FLOBUK.StoreSimulator.Editor.ShopMasterFinalIntegrationPhase4Runner.Run -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_PlayMode_Integracion_Fase4_Editor.log'
```

## Consola

- Errores C#: no.
- `NullReferenceException`: no en repeticion final.
- `MissingReferenceException`: no.
- Asserts: no.
- `FAIL:`: no en repeticion final.
- Warning relevante corregido: cierre de computadora en batchmode sin mouse activo.
- Warnings pendientes no criticos: URP advierte uso sin RenderGraph; mapeo opcional `product_appliances_1 -> mesa/mesas` no encontrado.

## Estado para Pull/Push

**LISTO CON AMARILLOS DOCUMENTADOS.**

Build, batchmode y runner final pasan sin ROJOS ni errores criticos. Antes de cierre funcional total conviene ejecutar Fase 5 enfocada en QA jugable/manual y reload integral.

## Pendientes Reales

1. Recarga real de `Game.unity` con reconstruccion integral y validacion de duplicados.
2. Compra visual completa de expansiones, zonas navegables, persistencia y clientela observada.
3. Pagos manuales por tarjeta y efectivo con denominaciones.
4. Venta real con precio alterado desde input.
5. Cierre de dia con reporte visible y abandono por espera de 7 segundos.
6. Rechazo visual de colocacion en mueble invalido.
7. QA visual multi-resolucion.
8. Certificacion FPS en hardware representativo y escenario cargado.
9. Persistencia real de resolucion, volumen, brillo y controles.
