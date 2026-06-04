# Reporte Fase Reportes/Guardado/Pagos - RQF20 a RQF24

Fecha: 2026-06-03

## Resultado general

- Compilacion: VERDE.
- Play Mode: VERDE.
- Runners ejecutados: 5/5 con `Failures=0`.
- Estado final: RQF20, RQF21, RQF22, RQF23 y RQF24 verificados con sistemas reales del asset.

## Compilacion

Log: `Documentos/Unity_Fase_ReportesGuardadoPagos_Compile_3.log`

Evidencia:

- `ExitCode: 0`
- `Tundra build success`
- Sin `error CS`, sin `Compilation failed`, sin `Scripts have compiler errors`.

## Play Mode

Logs focalizados generados:

- `Documentos/Unity_PlayMode_Auditoria_DailyRobberyReport.log`
- `Documentos/Unity_PlayMode_Auditoria_EndOfDayAutosave.log`
- `Documentos/Unity_PlayMode_Auditoria_CustomerScaling.log`
- `Documentos/Unity_PlayMode_Auditoria_CardPayment.log`
- `Documentos/Unity_PlayMode_Auditoria_CashPayment.log`

Todos terminaron con:

- `Audit finished. Failures=0`

## RQF20 - Robos en reporte diario

Estado: VERDE.

Evidencia del runner `DailyRobberyReportPlayModeAuditRunner`:

- Genera robo con cliente, anaquel y producto real.
- Verifica captura manual real y productos recuperados.
- Verifica escape real.
- Verifica arresto automatico real con seguridad nivel 3.
- Verifica resumen runtime y resumen persistido con:
  - `Robos ocurridos: 3`
  - `Detenidos manualmente: 1`
  - `Escaparon: 1`
  - arrestos automaticos
  - valor perdido
  - valor recuperado
  - productos recuperados
- Verifica que `UIStats` reutiliza `StatsDatabase.BuildDailyRobberySummary`.

## RQF21 - Autosave al finalizar dia

Estado: VERDE.

Evidencia del runner `EndOfDayAutosavePlayModeAuditRunner`:

- Invoca el cierre real del dia desde `UIGame.LeaveToNext`.
- Verifica que el cierre dispara `SaveGameSystem.dataSaveEvent`.
- Verifica que se escribe `save.dat` en `Application.persistentDataPath`.
- Verifica que el autosave incluye:
  - `StoreDatabase`
  - `DayCycleSystem`
  - `CustomerSystem`
  - `StatsDatabase`
- Verifica que las estadisticas del dia, incluyendo robos, quedan dentro del guardado.
- El runner preserva y restaura el `save.dat`/backup previo para no contaminar la partida del usuario.

## RQF22 - Clientela por expansion

Estado: VERDE.

Evidencia del runner `CustomerScalingPlayModeAuditRunner`:

- Verifica rango base diario `50-75` clientes.
- Verifica multiplicador base `1.0`.
- Compra expansiones de venta reales con `SupermarketExpansionSystem.TryPurchaseZone`.
- Verifica que cada expansion de venta comprada aplica `+15%`.
- Verifica que el plan diario escalado sube por encima de `50-75`.
- Invoca `CustomerSystem.PlanDailyCustomers` y confirma que usa el multiplicador de `SupermarketExpansionSystem`.
- Verifica que el spawn mantiene ritmo real y limite simultaneo seguro.

## RQF23 - Pago con tarjeta

Estado: VERDE.

Evidencia del runner `CardPaymentPlayModeAuditRunner`:

- Prepara cliente real con `payCash = false`.
- Usa `CashDesk` y terminal real.
- Cobro incorrecto: se rechaza, el cliente permanece en cola y el dinero no cambia.
- Cobro exacto: se acepta, el cliente sale de cola y el dinero aumenta por el total correcto.

## RQF24 - Pago en efectivo con cambio manual

Estado: VERDE.

Evidencia del runner `CashPaymentPlayModeAuditRunner`:

- Prepara cliente real con `payCash = true`.
- Usa `CashDesk` y registradora real.
- Cambio incorrecto/sobrecobro: se rechaza antes de registrar la venta.
- Cambio correcto: se acepta, el cliente sale de cola y el dinero aumenta por el total correcto.
- Verifica que el cajero automatico puede iniciar el flujo de efectivo con `CashDesk.TryStartAutomatedCheckout`.

## Sistemas del asset reutilizados

- `StatsDatabase`
- `UIStats`
- `SaveGameSystem`
- `UIGame`
- `DayCycleSystem`
- `CustomerSystem`
- `SupermarketExpansionSystem`
- `StoreDatabase`
- `CashDesk`
- `UICashDeskTerminal`
- `UICashDeskRegister`
- `Customer`
- `CustomerCart`
- `ShoplifterSystem`
- `ShoplifterAgent`
- `EntrepreneurTreeManager`
- `EntrepreneurTreeSecurityAdapter`

## Prefabs/materiales reutilizados

- Prefabs reales de `CustomerSystem.customerPrefabs`.
- Prefabs reales de producto desde `ItemDatabase`/`ProductScriptableObject`.
- Anaqueles/placements reales de escena mediante `PlacementObject`.
- Terminal real asignado en `CashDesk.terminal`.
- Registradora real asignada en `CashDesk.register`.
- Prefabs de pago real de `CustomerSystem.cardPrefab` y `CustomerSystem.cashPrefab` quedan cubiertos por el flujo de cliente/caja del asset.
- No se agregaron materiales ni prefabs nuevos.

## Archivos modificados

- `Assets/StoreSimulator/Editor/ShopMasterReportSavePaymentAuditRunners.cs`
- `Documentos/Reporte_Fase_ReportesGuardadoPagos_RQF20_RQF21_RQF22_RQF23_RQF24.md`

## Resultado de runners

- `DailyRobberyReportPlayModeAuditRunner`: VERDE, `Failures=0`.
- `EndOfDayAutosavePlayModeAuditRunner`: VERDE, `Failures=0`.
- `CustomerScalingPlayModeAuditRunner`: VERDE, `Failures=0`.
- `CardPaymentPlayModeAuditRunner`: VERDE, `Failures=0`.
- `CashPaymentPlayModeAuditRunner`: VERDE, `Failures=0`.

## Bugs pendientes

- No quedaron bugs bloqueantes detectados en RQF20-RQF24.
- La auditoria de pagos valida la logica de caja y los estados de cola/dinero. Queda recomendado probar manualmente la interaccion visual completa con clicks de botones/denominaciones desde camara de jugador.
- `git status` desde este workspace sigue mostrando el problema externo conocido de raiz Git: `fatal: cannot change to 'C:/Users/ijuan'`.

## Que debe probar Isaac

- Terminar un dia desde gameplay normal y confirmar que aparece el reporte con robos, valores perdidos/recuperados y productos recuperados.
- Cerrar el juego despues del fin de dia, volver a abrir y confirmar que dinero, dia, clientes y estadisticas se restauran.
- Comprar expansiones de venta y observar que el flujo de clientes se siente mas alto durante el dia.
- Cobrar con tarjeta en caja manual:
  - monto incorrecto debe fallar
  - monto exacto debe cobrar
- Cobrar en efectivo con cambio manual:
  - cambio incorrecto debe fallar
  - cambio correcto debe cobrar
- Contratar/asignar cajero y confirmar que atiende clientes de tarjeta y efectivo sin intervencion.

## Mejor siguiente punto de trabajo

El mejor siguiente punto es una pasada visual/manual de UX sobre caja y reporte diario: asegurar que el jugador entiende cuanto debe cobrar, cuanto cambio dar, y que el resumen de robos sea facil de leer al cierre del dia.
