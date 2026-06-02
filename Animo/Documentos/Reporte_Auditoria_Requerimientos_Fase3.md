# Reporte Auditoria Requerimientos ShopMaster - Fase 3

## Resumen Ejecutivo

La Fase 3 partio de las auditorias del Arbol del Emprendedor y mantuvo los VERDES previos. El runner Fase 2 se repitio despues de los cambios y termino con `Failures=0`.

Se cerraron con evidencia Play Mode real:

- `RQF12`: prerequisitos especificos de seguridad para Empleados 7, 8 y 14, rechazo sin gasto de puntos, visuales y probabilidades 33%, 66% y 99%.
- `RQF26`: cajero automatico con cliente, carrito y caja reales; tarjeta y efectivo medidos; Carismatico aplicado una sola vez.
- `RQNF4`: guardado atomico principal, `.bak` real y recuperacion ante archivo principal corrupto.

Tambien se avanzaron expansion, precios y surtido. La app `PRECIOS` tenia un `NullReferenceException` real al abrir por intentar agregar un segundo `Image`; quedo corregido. El surtidor ahora implementa estados y ruta fisica con `NavMeshAgent`, pero permanece AMARILLO: inicia `GoingToStorage`, resuelve un destino alcanzable y despues se atasca antes de recoger producto. El timeout conserva stock y evita bucles.

## Baseline

- `git status`: no pudo ejecutarse. Error exacto: `fatal: cannot change to 'C:/Users/ijuan'`.
- `dotnet build .\Animo.sln --nologo`: correcto, `0` errores, `0` warnings.
- Unity batchmode baseline: correcto, `Exiting batchmode successfully now!`.
- Runner Fase 2 repetido despues de todos los cambios: `Audit finished. Failures=0`.
- Runner Fase 3 final: `Audit finished. Failures=0`.

Regresiones reales encontradas durante Fase 3:

1. `PricingAppUIController.BuildUI()` agregaba un segundo `Image` al panel real y generaba `NullReferenceException`.
2. `SaveGameSystem` aceptaba JSON truncado como nodo parcial de `SimpleJSON`, por lo que no activaba backup.
3. El surtidor logico no tenia recorrido NPC fisico. Se implemento la ruta, pero el prefab visual reutilizado aun se atasca en locomocion.

## Estado General de Requerimientos

| Requerimiento | Area | Estado anterior | Estado nuevo | Evidencia real | Prueba ejecutada | Archivos modificados | Observaciones |
| ------------- | ---- | --------------- | ------------ | -------------- | ---------------- | -------------------- | ------------- |
| RQF12 | Seguridad | AMARILLO | VERDE | Rechazo y desbloqueo `security_1/2/3`; visuales; 33/66/99 | Play Mode Fase 3 | Runner Fase 3 | Empleados requeridos: 7, 8 y 14 |
| RQF21 | Guardado automatico | AMARILLO | AMARILLO | Save atomico y backup funcionan | Play Mode Fase 3 | `SaveGameSystem.cs` | Falta cierre de dia y recarga completa de escena |
| RQNF3 | Persistencia completa | AMARILLO | AMARILLO | Base, arbol, expansiones y slots escriben en disco | Play Mode Fase 2/3 | Saves auxiliares | Falta reconstruccion integral tras reload real |
| RQNF4 | Recuperacion | No cerrado | VERDE | Principal corrupto recupera `.bak` valido | Play Mode Fase 3 | `SaveGameSystem.cs` | Log: `Primary save was invalid. Backup loaded successfully.` |
| RQF26 | Cajero automatico | AMARILLO | VERDE | Tarjeta `1.75s`; efectivo `2.99s`; ingreso correcto | Play Mode Fase 3 | Runner Fase 3 | 1 producto: `0.5s + 1.5s/2.5s`, tolerancia de frames |
| RQF27 | Surtidor | AMARILLO | AMARILLO | Inicia ruta real y resuelve NavMesh; timeout conserva stock | Play Mode Fase 3 | `EmployeeRestockCoordinator.cs` | Prefab NPC se atasca antes de pickup |
| RQF5/RQF6/RQF7 | Expansion | No auditado integral | AMARILLO | App real abre; mapa existe; costos exactos; rechazo sin fondos | Play Mode Fase 3 | Sin cambio funcional | Falta pulsar confirmacion UI y verificar zona navegable con cliente |
| RQF22 | Clientela | No auditado integral | AMARILLO | Expansion venta cambia multiplicador a `1.15` | Play Mode Fase 3 | Sin cambio funcional | Falta observar plan diario y clientes en zona nueva |
| RQF23/RQF24 | Pago manual | No auditado integral | AMARILLO | Terminal y registro reales presentes | Revision integrada | Sin cambio | Falta manejar camara, tarjeta y denominaciones por UI |
| RQF28/RQF29 precios | Precios y ventas | No auditado integral | AMARILLO | App real abre; ideal, alto, limite y cero pasan | Play Mode Fase 3 | UI precios | Falta venta completa alterada desde input real |
| RQF25/RQF26/RQF27/RQF28 duplicados | Probabilidades | No auditado integral | AMARILLO | Formula, limite y extra a `$0.00` pasan | Play Mode Fase 3 | UI etiqueta precio | Falta barrido UI visual e ingreso cero en venta completa |
| RQF34/RQF35 | Reporte y alertas | Parcial | AMARILLO | Infraestructura y timer 7s existen | Revision integrada | Sin cambio | Falta cierre de dia real y abandono observado |
| RQNF5/RQNF6 | Procesos | Parcial | AMARILLO | Restauracion por espera y compatibilidad de muebles existen | Revision integrada | Sin cambio | Falta runner visual de abandono y colocacion incorrecta |
| RQNF16/RQNF17/RQNF18 | UI | Parcial | AMARILLO | Apps principales navegables; `PRECIOS` corregido | Play Mode Fase 2/3 | `PricingAppUIController.cs` | Falta QA visual por resoluciones |
| RQNF1/RQNF2/RQNF19 | Rendimiento y ajustes | Pendiente | AMARILLO | Compatibilidad estatica revisada | Revision integrada | Sin cambio | Falta captura FPS y completar resolucion, brillo y controles |

## AMARILLOS Cerrados

### RQF12

Faltaba el barrido especifico de seguridad. El runner reinicia el arbol, intenta cada nivel antes del empleado requerido, confirma rechazo sin gasto, desbloquea el camino necesario y valida visuales y porcentajes. Estado final: VERDE.

### RQF21 y RQNF3

Se reforzo escritura atomica y backup en save principal y auxiliares. La recuperacion principal ante corrupcion quedo probada. Falta recarga real de escena con reconstruccion integral de todos los sistemas. Estado final: AMARILLO.

### RQF26

Se midieron ventas automaticas reales:

- Tarjeta: `1.75s` con un producto, esperado aproximado `2.00s`.
- Efectivo: `2.99s` con un producto, esperado aproximado `3.00s`.
- Ambas acreditan balance con Carismatico una sola vez.

Estado final: VERDE.

### RQF27

Se implementaron estados `Idle`, `GoingToStorage`, `PickingStock`, `GoingToShelf`, `PlacingProduct` y `ReturningOrIdle`, busqueda de destino alcanzable, timeout y proteccion de stock. La escena registra:

`Navigation target (9.99, 0.08, 4.16) resolved near (10.00, 2.00, 7.50).`

Despues registra:

`Navigation timeout in state GoingToStorage.`

Estado final: AMARILLO.

## Nuevos Requerimientos Avanzados

- Expansion: app, mapa, precios `$1,750` y `$2,500`, rechazo sin fondos y multiplicador `15%` comprobados. El documento contradice su texto principal con una tabla de `10%`; se conserva `15%`.
- Precios: app real abierta sin excepciones; ideal `100%`, precio alto reduce probabilidad, limite `300%` bloqueado y `$0.00` permitido con probabilidad extra.
- Reporte diario, alertas, pagos manuales, rendimiento y ajustes: infraestructura revisada; permanecen AMARILLO hasta ejecutar flujos visibles completos.

## Flujo de Expansion

`Computadora -> EXPANDIR -> mapa -> zona venta/almacen -> costo -> compra/rechazo -> multiplicador clientela -> save auxiliar`

## Flujo de Pagos Manuales

Pendiente de cierre visible:

`Cliente -> caja -> tarjeta/efectivo -> UI -> cambio correcto/incorrecto -> venta -> inventario -> balance`

## Flujo de Cajero Automatico

`Empleado contratado -> rol Cajero -> cliente real -> carrito real -> CashDesk -> escaneo -> tarjeta/efectivo -> dinero -> Carismatico`

## Flujo de Surtidor

Implementado:

`Stock almacen -> slot asignado -> NPC -> GoingToStorage -> PickingStock -> GoingToShelf -> PlacingProduct -> ReturningOrIdle`

Pendiente real: el NPC no supera `GoingToStorage` en `Game.unity`.

## Flujo de Precios y Probabilidad

`Computadora -> PRECIOS -> producto -> ideal -> alto -> limite 300% -> $0.00 -> probabilidad compra -> probabilidad extra`

La ventana de etiqueta real tambien usa `ProductPricingSystem` para validar y mostrar feedback.

## Flujo de Guardado/Carga

- Save principal: `.tmp -> validacion estructural -> principal -> .bak`.
- Arbol, expansiones y slots: `.tmp -> validacion -> principal -> .bak`.
- Recuperacion principal corrupta: VERDE.
- Reload completo de escena y NPC sin duplicados: AMARILLO.

## Flujo de Reporte Diario

La infraestructura incluye ventas, gastos, salarios, robos, renta, luz, neto, agotados y clientes perdidos. Falta ejecutar cierre de dia real antes de elevar `RQF34`.

## UI y Notificaciones

- Revisadas en Play Mode: Arbol, COMPRA, EMPLEADOS, AYUDA, EXPANDIR y PRECIOS.
- Corregido: `PRECIOS` ya no agrega un segundo `Image`.
- Guardado y recuperacion muestran notificaciones especificas.
- Falta QA visual por resoluciones objetivo.

## Rendimiento y Ajustes

- No se midieron FPS promedio/minimo/maximo en esta pasada.
- `UISettings` persiste volumen y sensibilidad.
- Resolucion, brillo y controles configurables no quedaron demostrados integralmente.

## Archivos Modificados

| Archivo | Motivo |
| ------- | ------ |
| `Assets/Systems/EntrepreneurTree/EmployeeRestockCoordinator.cs` | Ruta NPC fisica, estados, NavMesh alcanzable, timeout y proteccion de stock |
| `Assets/StoreSimulator/Scripts/SaveGameSystem.cs` | Validacion estructural de JSON y recuperacion `.bak` real |
| `Assets/Systems/EntrepreneurTree/EntrepreneurTreeSaveIntegration.cs` | Validacion temporal y lectura de backup auxiliar |
| `Assets/Systems/Expansion/SupermarketExpansionSystem.cs` | Validacion temporal y lectura de backup auxiliar |
| `Assets/StoreSimulator/Scripts/UIPriceLabelWindow.cs` | Validacion 300%, feedback y probabilidades en ventana real |
| `Assets/UI/Computer/Pricing/PricingAppUIController.cs` | Reutilizar `Image` existente al abrir PRECIOS |
| `Assets/StoreSimulator/Editor/ShopMasterFullRequirementsPhase3Runner.cs` | Runner Play Mode reproducible Fase 3 |
| `Assets/StoreSimulator/Editor/ShopMasterFullRequirementsPhase3Runner.cs.meta` | Meta generado por Unity |

## Logs Generados

- `Documentos/Unity_Batchmode_Fase3_Baseline.log`
- `Documentos/Unity_PlayMode_Fase3_Baseline_Fase2Runner.log`
- `Documentos/Unity_Batchmode_Requerimientos_Fase3.log`
- `Documentos/Unity_PlayMode_Auditoria_Requerimientos_Fase3.log`
- `Documentos/Unity_PlayMode_Requerimientos_Fase3_Editor.log`
- `Documentos/Reporte_Auditoria_Requerimientos_Fase3.md`

## Comandos Usados

```powershell
git status --short --branch
dotnet build .\Animo.sln --nologo
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\ijuan\Animo' -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Fase3_Baseline.log'
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' -batchmode -projectPath 'C:\Users\ijuan\Animo' -executeMethod FLOBUK.StoreSimulator.Editor.ShopMasterEntrepreneurTreeGreenAuditRunner.Run -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_PlayMode_Fase3_Baseline_Fase2Runner.log'
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\ijuan\Animo' -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Requerimientos_Fase3.log'
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' -batchmode -projectPath 'C:\Users\ijuan\Animo' -executeMethod FLOBUK.StoreSimulator.Editor.ShopMasterFullRequirementsPhase3Runner.Run -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_PlayMode_Requerimientos_Fase3_Editor.log'
```

## Consola

- Errores C#: no.
- `NullReferenceException`: no en pasada final.
- `MissingReferenceException`: no.
- Asserts: no.
- Warnings relevantes: compatibilidad URP Render Graph; timeout de locomocion de surtidor.
- Warnings corregidos: `PRECIOS` con segundo `Image`; JSON truncado aceptado como save valido.
- Warnings pendientes: locomocion del prefab reutilizado para surtidor.

## Pendientes Reales

1. Resolver locomocion real del surtidor: el `NavMeshAgent` obtiene ruta completa, pero el prefab visual no avanza.
2. Ejecutar reload completo de `Game.unity` y reconstruccion integral de dinero, dia, inventario, muebles, precios, expansiones, NPC, roles, seguridad, slots y reporte.
3. Automatizar pagos manuales de tarjeta y efectivo por UI, incluido cambio correcto e incorrecto.
4. Ejecutar abandono real tras alerta de 7 segundos y cierre de dia con reporte visible.
5. Medir FPS y completar/persistir resolucion, brillo y controles.
6. Hacer QA visual manual en resoluciones objetivo.
