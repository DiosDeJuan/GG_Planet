<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 16 - Modo Admin Intro, Progresion y Costos del Arbol

## Base

- Rama solicitada: `codex/fase16-intro-modo-admin-progresion-costos-arbol`
- Rama base recomendada: `origin/codex/fase15-cierre-total-arbol-emprendedor`
- Fallback: `origin/codex/fase14-movimiento-input-playtest-reproducible`
- Nota Git: el entorno local reporto `fatal: cannot change to 'C:/Users/ijuan'`; se deja pendiente confirmar rama/commit/push con Git fuera de ese bloqueo.

## Documentos revisados

- `Documentos/NEW_Requerimientos.docx`: computadora, Arbol del Emprendedor, puntos, empleados, seguridad, mensajes claros y economia real.
- `Documentos/propuestas juanito (2).docx`: propuesta ShopMaster / supermercado gestionable y accesible.
- `Documentos/Protocolo prpuesta Juanito (1).pdf`: disponible en carpeta de documentos.
- `Documentos/Guia Gantt (1).pdf`: disponible en carpeta de documentos.
- `Reportes/Codex_Fase15_Cierre_Total_Arbol_Emprendedor.md`: base tecnica de 37 nodos, cierre total y PlayMode verde.
- `Reportes/Codex_Fase15_ArbolRuntimeAudit.txt`: evidencia previa del Arbol completo.

## Objetivo

Agregar Modo Admin visible desde el intro real, sin Canvas paralelo y sin economia, progreso o Arbol paralelos. El modo sirve para probar dinero, nivel, puntos y desbloqueos del flujo real sin farmear.

## Problema heredado

El Arbol estaba completo en Fase 15, pero todos los desbloqueables costaban 1 punto y no existia una ruta visible de intro para preparar partidas Admin con dinero, niveles y paquetes de prueba.

## Decisiones de arquitectura

- `UIIntro` crea los controles Admin dentro del objeto `Buttons` del Canvas existente.
- `AdminModeService` vive junto al guardado real y no crea base de datos paralela.
- Dinero Admin usa `StoreDatabase.AddRemoveMoney`.
- Nivel Admin extiende `StoreDatabase.currentLevel`.
- Puntos por nivel y puntos Admin entran por `EntrepreneurProgress`, sin pasar por logros.
- Desbloqueos Admin usan `EntrepreneurProgress.TryUnlock`.
- Save/load persiste `AdminMode`, `isAdminSave`, paquetes aplicados, nivel y puntos otorgados.

## Archivos modificados

- `Assets/StoreSimulator/Scripts/UIIntro.cs`
- `Assets/StoreSimulator/Scripts/SaveGameSystem.cs`
- `Assets/StoreSimulator/Scripts/StoreDatabase.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurProgress.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeNodeDefinition.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeUI.cs`
- `Assets/StoreSimulator/Tests/PlayMode/StoreSimulatorPlayModeRegressionTests.cs`
- `Reportes/Codex_Fase16_AdminModeAudit.txt`
- `Reportes/Codex_Fase16_Guia_Prueba_Modo_Admin.md`
- `Reportes/Codex_Fase16_Modo_Admin_Intro_Progresion_Costos_Arbol.md`

## Boton de intro

- Texto OFF: `MODO ADMIN: OFF`
- Texto ON: `MODO ADMIN: ON`
- Acciones: Admin Basico, Admin Medio, Admin Total y reset de ayudas de sesion.
- No se crea Canvas nuevo.

## Modo Admin

- Activar marca `adminModeEnabled` e `isAdminSave`.
- Admin Basico: dinero real +10000, +5 niveles, +5 puntos.
- Admin Medio: dinero real +50000, +20 niveles, desbloquea el primer 60% usando flujo real y agrega puntos Admin faltantes si hace falta.
- Admin Total: dinero real +250000, sube niveles para cubrir el Arbol, agrega puntos faltantes y desbloquea todo por `TryUnlock`.
- Los paquetes quedan registrados en `adminAppliedPackages` para no duplicar en save/load.

## Nivel y puntos

- Nivel minimo de jugador: 1.
- Subir de nivel 1 a 2 otorga 1 punto.
- Cada nivel adicional otorga exactamente 1 punto.
- `lastLevelRewarded` evita duplicacion.
- `pointsGrantedByLevel` y `pointsGrantedByAdmin` se guardan para auditoria.

## Costos escalonados del Arbol

- Nodos totales: 37.
- Nodos desbloqueables: 36.
- `productos_basicos_1`: costo 0.
- Primer 60%: 22 nodos costo 1.
- Siguiente 30%: 10 nodos costo 2.
- Ultimo 10%: 4 nodos costo 3.
- Puntos totales requeridos: 54.

## Matriz de nodos y costos

| Indice | ID | Costo nuevo | Tramo | Prerequisitos |
|---:|---|---:|---|---|
| 0 | productos_basicos_1 | 0 | inicial | ninguno |
| 1 | productos_basicos_2 | 1 | 60% | productos_basicos_1 |
| 2 | productos_basicos_3 | 1 | 60% | productos_basicos_2 |
| 3 | lacteos_1 | 1 | 60% | productos_basicos_3 |
| 4 | lacteos_2 | 1 | 60% | lacteos_1 |
| 5 | lacteos_3 | 1 | 60% | lacteos_2 |
| 6 | especias_1 | 1 | 60% | productos_basicos_3 |
| 7 | productos_frescos_1 | 1 | 60% | lacteos_1 |
| 8 | productos_frescos_2 | 1 | 60% | productos_frescos_1 |
| 9 | productos_higiene | 1 | 60% | especias_1 |
| 10 | sodas | 1 | 60% | productos_higiene |
| 11 | proteina_1 | 1 | 60% | empleado_5 |
| 12 | productos_lujo_1 | 1 | 60% | sodas |
| 13 | electrodomesticos_1 | 1 | 60% | productos_lujo_1 |
| 14 | empleado_1 | 1 | 60% | especias_1 |
| 15 | empleado_2 | 1 | 60% | productos_higiene |
| 16 | empleado_3 | 1 | 60% | sodas |
| 17 | empleado_4 | 1 | 60% | lacteos_1 |
| 18 | empleado_5 | 1 | 60% | lacteos_1 |
| 19 | empleado_6 | 1 | 60% | especias_1 |
| 20 | empleado_7 | 1 | 60% | empleado_5 |
| 21 | empleado_8 | 1 | 60% | sodas |
| 22 | empleado_9 | 1 | 60% | productos_higiene |
| 23 | empleado_10 | 2 | 30% | empleado_1 |
| 24 | empleado_11 | 2 | 30% | seguridad_1 |
| 25 | empleado_12 | 2 | 30% | empleado_13 |
| 26 | empleado_13 | 2 | 30% | productos_lujo_1 |
| 27 | empleado_14 | 2 | 30% | electrodomesticos_1 |
| 28 | empleado_15 | 2 | 30% | seguridad_2 |
| 29 | empleado_16 | 2 | 30% | proteina_1 |
| 30 | empleado_17 | 2 | 30% | productos_frescos_2 |
| 31 | empleado_18 | 2 | 30% | seguridad_3 |
| 32 | seguridad_1 | 2 | 30% | empleado_7 |
| 33 | seguridad_2 | 3 | 10% | empleado_8 |
| 34 | seguridad_3 | 3 | 10% | empleado_14 |
| 35 | mejora_cafeina | 3 | 10% | productos_frescos_2 |
| 36 | mejora_carismatico | 3 | 10% | empleado_15 |

Validacion UI: `EntrepreneurTreeNodeView` y detalle leen `node.Cost`. Validacion TryUnlock: `EntrepreneurProgress.TryUnlock` descuenta `node.Cost`. Persistencia: costos no se guardan porque son definicion.

## Matriz Admin

| Accion | Sistema real | Metodo | Dato | Persistencia | Duplicacion | Test |
|---|---|---|---|---|---|---|
| Activar Admin | SaveGameSystem | AdminModeService | adminModeEnabled/isAdminSave | AdminMode JSON | bandera persistida | AdminIntro, Basic |
| Admin Basico dinero | StoreDatabase | AddRemoveMoney | currentMoney | StoreDatabase JSON | adminAppliedPackages | BasicPackage |
| Admin Basico niveles | StoreDatabase | SetPlayerLevelForAdmin | currentLevel | StoreDatabase/AdminMode JSON | lastLevelRewarded | BasicPackage |
| Puntos por nivel | EntrepreneurProgress | AddProgressPointsFromLevel | progressPoints | EntrepreneurProgress JSON | lastLevelRewarded | BasicPackage |
| Puntos Admin | EntrepreneurProgress | AddProgressPointsFromAdmin | progressPoints | EntrepreneurProgress JSON | solo faltantes | TotalPackage |
| Desbloqueos | EntrepreneurProgress | TryUnlock | unlockedNodeIds | EntrepreneurProgress JSON | IsUnlocked y paquetes | TotalPackage |

## Pruebas

- `AdminIntro_Fase16ButtonExistsInRealIntroMenu`
- `TreeCosts_Fase16UsesTieredCostsFromRealDefinitions`
- `TreeCosts_Fase16TryUnlockSpendsActualTierCost`
- `AdminMode_Fase16BasicPackageAddsRealMoneyLevelsAndTreePoints`
- `AdminMode_Fase16PackageDoesNotDuplicateAfterSaveLoad`
- `AdminMode_Fase16TotalUnlocksAllTreeNodesThroughRealFlow`
- `AdminMode_Fase16RuntimeAuditCanBeGenerated`

## Actividades tipo Gantt

| ID | Actividad | Requerimientos | Responsable | Fecha | Duracion | Resultado |
|---|---|---|---|---|---:|---|
| A1 | Auditar intro y sistemas | RQNF16, RQNF18 | POMPIC / Codex | 2026-06-10 | 2h | Completado |
| A2 | Integrar boton Admin | RQNF16, RQNF17 | POMPIC / Codex | 2026-06-10 | 3h | Completado |
| A3 | Estado Admin persistente | RQF21, RQNF3 | POMPIC / Codex | 2026-06-10 | 3h | Completado |
| A4 | Dinero Admin real | RQF1, RQF2 | POMPIC / Codex | 2026-06-10 | 2h | Completado |
| A5 | Nivel y puntos por nivel | RQF3, RQF4 | POMPIC / Codex | 2026-06-10 | 4h | Completado |
| A6 | Costos escalonados | RQF3, RQF4, RQF36 | POMPIC / Codex | 2026-06-10 | 4h | Completado |
| A7 | Paquetes Admin | RQF8, RQF11, RQF18 | POMPIC / Codex | 2026-06-10 | 4h | Completado |
| A8 | Save/load sin duplicacion | RQF21 | POMPIC / Codex | 2026-06-10 | 4h | Completado |
| A9 | PlayMode y auditoria | RQNF8, RQNF9 | POMPIC / Codex | 2026-06-10 | 5h | En validacion |
| A10 | Reporte y guia | documentacion | POMPIC / Codex | 2026-06-10 | 4h | Completado |

## Validaciones

- `dotnet build .\SHOP_MASTER_FINAL.sln --no-restore`: PASS, 0 warnings, 0 errores.
- Unity PlayMode: PASS, 88/88 tests.
- Tests nuevos Fase 16: PASS.
- Runtime audit: PASS en `Reportes/Codex_Fase16_AdminModeAudit.txt`.
- Capturas: generadas en `Reportes/Capturas_Fase16/`.
- Consola: sin `NullReferenceException`, `MissingReferenceException`, `Compilation failed`, `error CS` ni `Fatal Error`.
- TMP Bangers/Roboto: no stageados.
- Builds: no stageados.
- `git diff --cached --check`: PASS tras normalizar espacios finales del log generado.

## Estado final

VERDE tecnico.

## Pendientes reales

- Playtest humano final siguiendo la Guia Isaac para validar percepcion visual y flujo de presentacion.
- Mantener fuera del stage los cambios preexistentes de TMP Bangers/Roboto.
