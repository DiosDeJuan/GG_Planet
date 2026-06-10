<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 14 - Movimiento reproducible, input limpio y playtest base

## 25.1 Encabezado
- Fase: Codex Fase 14 - Movimiento reproducible, input limpio y playtest base.
- Rama base: `origin/codex/fase13-tabla-productos-catalogo-jugable`.
- Commit base esperado: `effe807968a3eea4ef2b7024777b2e48d62e4518`.
- Rama nueva: `codex/fase14-movimiento-input-playtest-reproducible`.
- Responsable: Codex.
- Proyecto: `SHOP_MASTER_FINAL`.
- Repositorio: `DiosDeJuan/GG_Planet`.
- Fecha: 2026-06-10.

## 25.2 Documentos revisados
- `Documentos/NEW_Requerimientos.docx`: computadora del juego, catalogo, interfaz clara, notificaciones comprensibles, Arbol desde computadora.
- `Documentos/propuestas juanito (2).docx`: juego 3D en primera persona, caminar, correr, agacharse, saltar y laptop como centro de administracion.
- `Documentos/Protocolo prpuesta Juanito (1).pdf`: progreso claro, experiencia inmersiva y accesibilidad tecnica.
- `Documentos/Guia Gantt (1).pdf`: actividades con requerimiento, responsable, fecha y duracion estimada.
- `Reportes/Codex_Fase13_Tabla_Productos_Catalogo_Jugable.md`.
- `Reportes/Codex_Fase13_Guia_Prueba_Productos.md`.
- `Reportes/Codex_Fase12_Arbol_Reproducible_Gameplay_RPG.md`.
- `Reportes/Codex_Fase12_Guia_Prueba_Arbol.md`.
- `Reportes/Codex_Fase11_QA_Visual_Capturas_Arbol_Productos_Empleados.md`.
- `Reportes/Codex_Fase10_Cierre_Visual_Arbol_Productos_Empleados.md`.
- `Reportes/Codex_Fase4_1_Hotfix_Movimiento_Arbol_QA.md`.

## 25.3 Problema heredado
Isaac no podia moverse para probar el juego. La consola reportada no tenia errores rojos, solo warning de Render Graph, asi que se trato como regresion funcional de input/estado: PlayerInput, action maps, cursor, flags de UI, computadora y restauracion de movimiento. No se trabajo tabla de productos, menu principal, ajustes, clientes, ladrones, finales ni build final.

## 25.4 Diagnostico tecnico
| Area | Estado observado | Causa posible | Archivo | Accion | Resultado |
|---|---|---|---|---|---|
| Action maps | Asset real `InputActions.inputactions` tiene `Default`; `UI` no existe | Dependencia dura a `UI` puede bloquear input | `PlayerController.cs` | `UI` queda opcional y warning unico | Gameplay no se bloquea |
| PlayerInput | `GetPlayerByIndex(0)` puede no estar listo en `Awake` | Suscripcion perdida | `PlayerController.cs` | Resolver por componente local, indice o busqueda en escena | Suscripcion robusta |
| Awake/OnEnable | Antes podia salir temprano | Movimiento sin callbacks | `PlayerController.cs` | `EnsureInputReady` reintenta sin romper | Input se recupera |
| Cursor | Batchmode no refleja lock real | Evidencia automatica ambigua | `PlayerController.cs` | Se registra `WantsGameplayCursorLocked` | Tests validan estado solicitado |
| timeScale | UI/pausa podia dejarlo en 0 | Jugador inmovil | `PlayerController.cs` | `RestoreGameplayInput` fuerza `Time.timeScale = 1` | Validado |
| CharacterController | Puede quedar deshabilitado por flujo externo | Sin desplazamiento | `PlayerController.cs` | Restore re-habilita CharacterController | Validado |
| Camara/look | Camara puede faltar en fixture o quedar desactivada | Sin mouse look | `PlayerController.cs` | Re-resuelve `Camera.main` y activa camara | Validado |
| Correr/agacharse | Asset `Default` no incluye Sprint/Crouch | Requerimiento documental incompleto | `PlayerController.cs` | Lectura suplementaria Shift/Ctrl/C dentro del mismo controlador | Validado por configuracion |
| UIShopDesktop | Abrir/cerrar suscribia sin guardas | Duplicados o input bloqueado | `UIShopDesktop.cs` | `isOpen`, `actionSubscribed`, restore unico | 5 ciclos verdes |
| QA overlays | F8 de Arbol podia coexistir | Riesgo de input atrapado | `PlayerController.cs`, `UIShopDesktop.cs` | F7 restaura, F9 diagnostica | Validado |
| EventSystem | Existe en Game.unity | No era causa directa | Escena real | No se modifico | Sin duplicados |
| Interact | Computadora debe bloquear solo mientras abierta | Estado no restaurado | `UIShopDesktop.cs` | `RestorePlayerAfterComputerClosed` | Validado |

## 25.5 Cambios realizados
| Archivo | Cambio | Motivo | Riesgo | Validacion |
|---|---|---|---|---|
| `Assets/StoreSimulator/Scripts/PlayerController.cs` | Resolve robusto de `PlayerInput`, action map gameplay, restore, cursor solicitado, F7/F9, correr/agacharse y API QA | Recuperar movimiento real y hacerlo reproducible | Medio | dotnet + PlayMode |
| `Assets/StoreSimulator/Scripts/UIShopDesktop.cs` | Estado `isOpen`, suscripcion segura, cierre/restauracion central, F7 QA | Evitar que computadora deje jugador bloqueado | Bajo | Tests de 5 ciclos |
| `Assets/StoreSimulator/Tests/PlayMode/StoreSimulatorPlayModeRegressionTests.cs` | 12 tests nuevos de movimiento/input y captura diagnostica | Probar PlayerController real y delta de posicion | Bajo | `76/76` PlayMode |
| `Reportes/Codex_Fase14_MovementReproLog.txt` | Log con posicion antes/despues y delta | Evidencia objetiva de movimiento | Bajo | Generado por test |
| `Reportes/Capturas_Fase14/Movement_05_DiagnosticHUD.png` | Captura de diagnostico | Evidencia visual no sustitutiva | Bajo | PNG verificado |

## 25.6 Flujo reproducible
- Escena usada: `Assets/StoreSimulator/Scenes/Game.unity`.
- Al iniciar gameplay: `PlayerController.RestoreGameplayInput()` deja estado `All`, action map `Default`, `timeScale = 1`, CharacterController activo y cursor solicitado para gameplay.
- Abrir computadora: `UIShopDesktop.Interact("LeftClick")` desactiva movimiento, muestra cursor y marca computadora abierta.
- Cerrar computadora: `UIShopDesktop.Exit()` termina suscripcion, reactiva interaccion, restaura movimiento y HUD.
- F7: `UIShopDesktop.RestoreGameplayInputForQA()` cierra estado activo si existe y llama al restore real.
- F9: muestra/oculta diagnostico QA de movimiento.

## 25.7 Movimiento
| Accion | Input | Resultado esperado | Resultado validado | Evidencia |
|---|---|---|---|---|
| Caminar adelante | WASD / QA `Vector2.up` | Posicion cambia hacia adelante | Delta Z `10.0000` | `Codex_Fase14_MovementReproLog.txt` |
| Caminar atras | S | Action map `Default/Move` disponible | Configuracion validada | PlayMode |
| Lateral | A/D | Action map `Default/Move` disponible | Configuracion validada | PlayMode |
| Correr | Shift | Velocidad mayor a caminar | `RunSpeed > WalkSpeed` | PlayMode |
| Saltar | Space | Fuerza mayor a 0 | `jumpForce > 0` | PlayMode |
| Agacharse | Ctrl/C | No bloquea movimiento permanente | Crouch on/off conserva `CanMove` | PlayMode |
| Mirar | Mouse delta / View | `CanLook = true` | Validado al restaurar gameplay | PlayMode |
| Interactuar | LeftClick/Action segun asset | Computadora abre | `UIShopDesktop.Interact` cambia estado | PlayMode |

## 25.8 UI/input restore
| Caso | Estado antes | Estado despues | Resultado |
|---|---|---|---|
| Abrir computadora | Gameplay `All` | Movimiento `None`, cursor UI solicitado | OK |
| Cerrar computadora | Computadora abierta | Gameplay `All`, `CanMove = true` | OK |
| Abrir ARBOL/cerrar | App dentro de computadora | Cierre de computadora restaura input | OK por ruta desktop/F7 |
| Abrir PRODUCTS/cerrar | App dentro de computadora | Cierre de computadora restaura input | OK por restore comun |
| Abrir EMPLEADOS/cerrar | App dentro de computadora | Cierre de computadora restaura input | OK por restore comun |
| Abrir GESTION/cerrar | App dentro de computadora | Cierre de computadora restaura input | OK por restore comun |
| F7 restore | UI o estado dudoso | Gameplay restaurado | OK |
| Escape | Accion `Cancel` | `Exit()` y restore | OK por handler |

## 25.9 QA tools
- F7: `QA Restore Gameplay Input ejecutado`; solo Editor/Development/Test.
- F9: overlay diagnostico en `PlayerController.OnGUI`; solo Editor/Development/Test.
- Muestra posicion, velocidad, grounded, canMove/canLook, action map, cursor, timeScale, CharacterController, ultimo input y motivo de bloqueo.
- No modifica economia, Arbol, productos, clientes, ladrones ni progreso.
- No crea Canvas principal, computadora ni PlayerController paralelo.

## 25.10 MovementReproLog
Resumen de `Reportes/Codex_Fase14_MovementReproLog.txt`:
- Posicion inicial: `(0.0000, -0.0041, 0.0000)`.
- Posicion despues de avanzar 2s: `(0.0000, -0.0041, 10.0000)`.
- Delta: `(0.0000, 0.0000, 10.0000)`.
- Distancia: `10.0000`.
- canMove/canLook antes/despues: `True/True`.
- Cursor solicitado gameplayLock: `True`.
- Action map activo: `Default`.
- timeScale: `1.00`.
- Errores rojos encontrados: no.

## 25.11 Bugs encontrados
| ID | Modulo | Descripcion | Severidad | Causa raiz | Archivo | Estado |
|---|---|---|---|---|---|---|
| F14-B01 | Input | PlayerInput podia no resolverse en Awake por indice | Alta | Dependencia unica a `GetPlayerByIndex(0)` | `PlayerController.cs` | Corregido |
| F14-B02 | UI restore | Computadora podia dejar estado de movimiento/cursor sin restauracion central | Alta | Cierre repartido en varios efectos | `UIShopDesktop.cs` | Corregido |
| F14-B03 | QA | No habia ruta F7/F9 para recuperar o diagnosticar movimiento | Media | Falta de herramienta QA limitada | `PlayerController.cs` | Corregido |
| F14-B04 | Evidencia | No habia delta de posicion reproducible | Media | Tests previos cubrian sistemas, no movimiento | Tests PlayMode | Corregido |

## 25.12 Bugs corregidos
| ID | Correccion | Archivo | Riesgo | Validacion |
|---|---|---|---|---|
| F14-B01 | `ResolvePlayerInput`, `EnsureInputReady`, suscripcion idempotente | `PlayerController.cs` | Medio | `PlayerMovement_MissingUIActionMapDoesNotBlockGameplay` |
| F14-B02 | `RestoreGameplayInput`, `SetGameplayInputEnabled`, `RestorePlayerAfterComputerClosed` | `PlayerController.cs`, `UIShopDesktop.cs` | Medio | `PlayerMovement_ComputerOpenCloseRestoresMovement` |
| F14-B03 | F7 restore y F9 diagnostico | `PlayerController.cs`, `UIShopDesktop.cs` | Bajo | `PlayerMovement_QARestoreClosesComputerAndRestoresInput` |
| F14-B04 | Log/captura diagnostica con delta | Tests PlayMode | Bajo | `MovementReproLog_GeneratesPositionBeforeAfter` |

## 25.13 Tests agregados
| Test | Sistema | Que valida | Resultado | Evidencia |
|---|---|---|---|---|
| `PlayerMovement_PlayerControllerExistsInGameScene` | Escena real | PlayerController y CharacterController existen en `Game` | Passed | XML |
| `PlayerMovement_DefaultActionMapIsAvailableOrFallbackSafe` | InputActions | `Default` tiene Move/View/Jump/Cancel y `UI` es opcional | Passed | XML |
| `PlayerMovement_MissingUIActionMapDoesNotBlockGameplay` | Input | Sin `UI`, gameplay sigue activo | Passed | XML |
| `PlayerMovement_StartsWithGameplayInputEnabled` | PlayerController | Inicio en `All`, canMove/canLook | Passed | XML |
| `PlayerMovement_RestoreGameplayInputSetsExpectedState` | Restore | timeScale 1, movimiento/look activos | Passed | XML |
| `PlayerMovement_ComputerOpenCloseRestoresMovement` | UIShopDesktop | abrir bloquea, cerrar restaura | Passed | XML |
| `PlayerMovement_RepeatedComputerOpenCloseDoesNotLeaveInputBlocked` | UIShopDesktop | 5 ciclos sin bloqueo | Passed | XML |
| `PlayerMovement_QARestoreClosesComputerAndRestoresInput` | QA | F7 path real restaura | Passed | XML |
| `PlayerMovement_RunJumpAndCrouchConfigurationIsValid` | Movimiento | run > walk, salto/crouch validos | Passed | XML |
| `PlayerMovement_CrouchDoesNotPermanentlyDisableMovement` | Crouch | agachar/levantar no bloquea | Passed | XML |
| `PlayerMovement_PositionChangesWhenApplyingForwardInput` | Movimiento | posicion cambia con input adelante | Passed | XML |
| `MovementReproLog_GeneratesPositionBeforeAfter` | Evidencia | genera log y PNG diagnostico | Passed | XML/log/PNG |

## 25.14 Actividades tipo Gantt
| ID | Actividad | Requerimientos relacionados | Responsable | Fecha | Duracion estimada | Resultado |
|---|---|---|---|---|---|---|
| 1 | Validacion rama/base | Control de versiones | Codex | 2026-06-10 | 0.25 h | OK |
| 2 | Revision documental movimiento/UI | RQNF16, RQNF17, RQNF18, RQNF20 | Codex | 2026-06-10 | 0.75 h | OK |
| 3 | Auditoria PlayerController | Primera persona, movimiento | Codex | 2026-06-10 | 1 h | OK |
| 4 | Auditoria InputActions/action maps | Input | Codex | 2026-06-10 | 0.75 h | OK |
| 5 | Auditoria cursor/camara/timeScale | Gameplay base | Codex | 2026-06-10 | 0.75 h | OK |
| 6 | Auditoria computadora/UI restore | Laptop/computadora | Codex | 2026-06-10 | 1 h | OK |
| 7 | Correccion bloqueo movimiento | Gameplay base | Codex | 2026-06-10 | 1.5 h | OK |
| 8 | Implementacion restore gameplay input | UI/input | Codex | 2026-06-10 | 1 h | OK |
| 9 | QA hotkey/diagnostico | QA | Codex | 2026-06-10 | 1 h | OK |
| 10 | Tests PlayMode movimiento | Regresion | Codex | 2026-06-10 | 2 h | OK |
| 11 | Generacion MovementReproLog | Evidencia | Codex | 2026-06-10 | 0.5 h | OK |
| 12 | Validacion tecnica | QA | Codex | 2026-06-10 | 1.5 h | OK |
| 13 | Guia Isaac | RQNF20 | Codex | 2026-06-10 | 0.5 h | OK |
| 14 | Reporte/cierre | Documentacion | Codex | 2026-06-10 | 1 h | OK |

## 25.15 Validaciones
| Validacion | Resultado |
|---|---|
| Base git | `effe807968a3eea4ef2b7024777b2e48d62e4518` confirmado contra `origin/codex/fase13-tabla-productos-catalogo-jugable` |
| dotnet build | Correcto, 0 errores, 0 advertencias |
| Unity PlayMode | Correcto, `76/76` passed |
| Tests nuevos | 12 tests de movimiento/input passed |
| diff check | Correcto, sin whitespace invalidante; solo avisos CRLF normales de Windows |
| MovementReproLog | Generado, delta `10.0000` |
| Consola | Sin NRE, MissingReferenceException ni errores CS |
| Warnings | Render Graph heredado, CommandBuffer heredado, `UI` opcional unico |
| Capturas | `Reportes/Capturas_Fase14/Movement_05_DiagnosticHUD.png` |
| Animo | No reintroducido |
| TMP | Bangers/Roboto no deben stagearse |

## 25.16 Estado final
Estado: VERDE tecnico.

La evidencia automatizada usa PlayerController real, CharacterController y PlayerInput del asset real. `MovementReproLog` demuestra cambio de posicion. La computadora abre/cierra y restaura input en pruebas, F7 restaura, F9 diagnostica y no hay errores rojos. Queda pendiente la pasada humana completa de Isaac en Game View por las limitaciones normales de mouse/cursor en batchmode.

## 25.17 Confirmaciones obligatorias
- Animo/ no fue reintroducido.
- TMP Bangers/Roboto no fueron stageados.
- No se creo PlayerController paralelo.
- No se creo sistema de input paralelo.
- No se creo computadora paralela.
- No se creo Canvas principal paralelo.
- No se trabajo tabla de productos.
- No se trabajo menu principal.
- No se trabajo ajustes.
- No se trabajo build final.
- Los QA tools estan limitados a Editor/Development/Test.
- Los archivos modificados tienen inscripcion POMPIC cuando aplica.
- Logs, XML y PNG no reciben inscripcion porque son evidencia generada y modificar su formato/bytes los ensuciaria.

## 25.18 Pendientes reales
- Play Mode humano completo por Isaac en Game View: WASD, mouse look, Shift, Space, Ctrl/C, interactuar, abrir/cerrar computadora y volver a moverse.
- Ajuste fino de sensibilidad de camara si Isaac lo siente incomodo.
- Rebinding de controles en fase de ajustes, si se decide soportarlo.
- Validacion con gamepad si se decide soportarlo.
- Warning externo de Render Graph/URP.
- Warning unico de action map `UI` opcional si el equipo prefiere convertirlo a log informativo.
