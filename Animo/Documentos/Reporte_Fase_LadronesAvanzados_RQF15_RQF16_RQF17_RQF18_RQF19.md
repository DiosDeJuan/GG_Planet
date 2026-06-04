# Reporte Fase Ladrones Avanzados - RQF15, RQF16, RQF17, RQF18, RQF19

Fecha: 2026-06-03
Proyecto: ShopMaster / Animo
Escena auditada: `Assets/StoreSimulator/Scenes/Game.unity`

## 1. Resumen ejecutivo

Estado general: VERDE con QA visual pendiente.

- Compilacion Unity batchmode: VERDE. `Unity_Fase_LadronesAvanzados_Compile_3.log` termina con `return code 0`.
- Play Mode: VERDE. Los seis runners de esta fase terminaron con `Audit finished. Failures=0`.
- RQF15: VERDE. Tres tipos principales probados: comun, sospechoso y especial, todos sobre cliente/NPC real.
- RQF16: VERDE. La probabilidad escala con expansiones/progreso y respeta el tope de 6.5%.
- RQF17: VERDE. Captura manual recupera producto real y otorga puntos.
- RQF18: VERDE. Seguridad nivel 3 arresta robo real, recupera producto y registra evento.
- RQF19: VERDE. Alertas usan `UIGame.AddNotification` y audio existente de notificacion.
- Modo Admin: VERDE. Permite dinero, puntos, desbloqueos, expansiones, stock y forzar tipo de ladron sin contaminar gameplay normal.

## 2. Resultado de compilacion

Resultado: VERDE.

Evidencia:
- `Documentos/Unity_Fase_LadronesAvanzados_Compile_3.log`: `Exiting batchmode successfully now!`
- `Documentos/Unity_Fase_LadronesAvanzados_Compile_3.log`: `Application will terminate with return code 0`

## 3. Resultado de Play Mode

Resultado: VERDE.

Runners ejecutados:
- `AdvancedThiefTypesPlayModeAuditRunner`
- `RobberyDifficultyScalingPlayModeAuditRunner`
- `ManualCaptureRewardPlayModeAuditRunner`
- `SecurityAutoArrestPlayModeAuditRunner`
- `TheftAlertPlayModeAuditRunner`
- `AdminModeRobberyTestAuditRunner`

Evidencia:
- `Unity_PlayMode_Auditoria_AdvancedTypes.log`: `Audit finished. Failures=0`
- `Unity_PlayMode_Auditoria_DifficultyScaling.log`: `Audit finished. Failures=0`
- `Unity_PlayMode_Auditoria_ManualCaptureReward.log`: `Audit finished. Failures=0`
- `Unity_PlayMode_Auditoria_SecurityAutoArrest.log`: `Audit finished. Failures=0`
- `Unity_PlayMode_Auditoria_TheftAlert.log`: `Audit finished. Failures=0`
- `Unity_PlayMode_Auditoria_AdminMode.log`: `Audit finished. Failures=0`

## 4. Sistemas del asset reutilizados

- `ShoplifterSystem`
- `ShoplifterAgent`
- `ShoplifterInteractable`
- `RobberyInventoryBridge`
- `EntrepreneurTreeSecurityAdapter`
- `EntrepreneurTreeManager`
- `ProductInventorySystem`
- `ShelfProductSlotSystem`
- `StatsDatabase`
- `StoreDatabase`
- `UIGame`
- `AudioSystem`
- `Customer`
- `CustomerAgent`
- `CustomerCart`
- `PlacementObject`
- `SupermarketExpansionSystem`
- `AdminModeBootstrap`
- `AdminSessionConfig`
- `EntrepreneurTreeUIBootstrap`

## 5. Prefabs/materiales reutilizados

- Clientes/NPCs existentes de `CustomerSystem.customerPrefabs`.
- `CustomerAgent` y `NavMeshAgent` reales del asset.
- Prefabs de productos reales desde `ProductScriptableObject.prefab`.
- Materiales runtime derivados del shader Standard para tintes/accesorios sobre el NPC real.
- `UIGame.notificationClip` y `AudioSystem.Play2D` para alertas sonoras.
- UI de Admin existente, ampliada con selector de tipo de ladron.

No se introdujo inventario paralelo, seguridad paralela ni NPC falso desconectado.

## 6. Estado del Modo Admin

Estado: VERDE.

Permite probar:
- Dinero inicial.
- Puntos del Arbol del Emprendedor.
- Desbloqueo de productos.
- Desbloqueo de seguridad.
- Stock de prueba.
- Expansiones de venta.
- Spawn forzado de ladron comun, sospechoso, rapido, experto o especial.

Cambios:
- Se agrego selector `Tipo de ladron` en el panel Admin, conectado a `AdminSessionConfig.forcedShoplifterType`.
- La aplicacion de Admin sigue usando `EntrepreneurTreeUIBootstrap.ApplyAdminConfig` y resetea `AdminSessionConfig` al terminar.

Evidencia:
- `Unity_PlayMode_Auditoria_AdminMode.log`: dinero inicial, puntos, seguridad, expansiones y ladron especial forzado pasan con `Failures=0`.

## 7. Archivos modificados

- `Assets/Systems/EntrepreneurTree/ShoplifterSystem.cs`
- `Assets/Systems/EntrepreneurTree/ShoplifterAgent.cs`
- `Assets/Systems/EntrepreneurTree/AdminModeBootstrap.cs`
- `Assets/StoreSimulator/Editor/ShopMasterAdvancedThiefAuditRunners.cs`
- `Documentos/Reporte_Fase_LadronesAvanzados_RQF15_RQF16_RQF17_RQF18_RQF19.md`
- Logs generados en `Documentos/`.

## 8. Estado por requerimiento

### RQF15 - Tres tipos de ladrones

Que pedia el documento:
- Ladron comun, cliente sospechoso y ladron especial con comportamientos/visuales diferenciados.
- Especial solo con premium disponible y con producto normal para disimular.

Que existia antes:
- Existian `ShoplifterType.Common`, `Suspicious`, `Expert`, `Fast`, `Special`.
- Todos usaban el mismo patron visual de cap/backpack, asi que la diferencia visual no era suficientemente clara.

Que corregi:
- `ShoplifterAgent` ahora diferencia accesorios:
  - Comun/Fast: gorra.
  - Sospechoso: gafas discretas y sin indicador inicial obvio.
  - Expert/Special: gorra y mochila.
- El ladron especial conserva carrito con producto real normal cuando se fuerza en auditoria.

Que prueba hice:
- Forzar comun, sospechoso y especial.
- Confirmar cliente/NPC real, `NavMeshAgent`, carrito real y visual diferenciado.
- Confirmar que especial/experto no aparecen naturalmente sin productos premium desbloqueados.

Evidencia:
- `Unity_PlayMode_Auditoria_AdvancedTypes.log`: `PASS: RQF15 comun tiene gorra y ruta simple sin mochila avanzada.`
- `Unity_PlayMode_Auditoria_AdvancedTypes.log`: `PASS: RQF15 sospechoso es mas sutil: gafas discretas sin indicador inicial obvio.`
- `Unity_PlayMode_Auditoria_AdvancedTypes.log`: `PASS: RQF15 especial lleva mochila y al menos un producto normal en carrito para disimular.`

Estado: VERDE.

### RQF16 - Escalado de dificultad

Que pedia el documento:
- Base equivalente a 1 ladron por 25 clientes.
- Aumento con expansiones.
- Tope 6.5%.
- Mas sospechosos/especiales en etapas avanzadas.
- Valor objetivo crece con progreso sin inventar stock.

Que existia antes:
- Ya existia `initialOneInNChance = 25`, `maxThiefChance = 0.065` y calculo base.
- Distribucion de tipos y valor objetivo eran demasiado estaticos.

Que corregi:
- `ShoplifterSystem.GetRobberyDifficulty01()` calcula dificultad con dia, nivel y expansiones.
- `ChooseThiefType()` escala chances de sospechoso, experto y especial segun dificultad y premium.
- `GetTargetStealValue()` escala el rango objetivo dentro de limites por tipo.

Que prueba hice:
- Medir probabilidad inicial.
- Comprar expansiones de venta.
- Confirmar probabilidad mayor o igual y <= 6.5%.
- Confirmar multiplicador de valor especial > 1 y <= 1.65.
- Confirmar aparicion de sospechosos y premium en muestras avanzadas.

Evidencia:
- `Unity_PlayMode_Auditoria_DifficultyScaling.log`: `PASS: RQF16 probabilidad inicial equivale a 1 ladron por 25 clientes.`
- `Unity_PlayMode_Auditoria_DifficultyScaling.log`: `PASS: RQF16 expansiones de venta aumentan progresivamente la probabilidad de robo.`
- `Unity_PlayMode_Auditoria_DifficultyScaling.log`: `PASS: RQF16 probabilidad escalada no supera 6.5%.`
- `Unity_PlayMode_Auditoria_DifficultyScaling.log`: `PASS: RQF16 etapas avanzadas producen sospechosos y tipos premium.`

Estado: VERDE.

### RQF17 - Captura manual completa

Que pedia el documento:
- Interceptar al ladron antes de salida.
- Recuperar productos reales.
- Registrar captura.
- Dar recompensa.
- No duplicar stock.

Que existia antes:
- `ShoplifterInteractable` y `ShoplifterAgent.TryManualCapture()` ya existian.
- Faltaba prueba avanzada de recompensa + recuperacion en esta fase.

Que corregi:
- Se agrego runner especifico `ManualCaptureRewardPlayModeAuditRunner`.
- Las alertas de captura ahora dicen claramente `Ladron detenido: productos recuperados`.

Que prueba hice:
- Sembrar producto real en anaquel.
- Crear cliente real y convertirlo en ladron.
- Capturar manualmente.
- Confirmar producto restaurado, puntos agregados y `StatsDatabase.recoveredProducts`.

Evidencia:
- `Unity_PlayMode_Auditoria_ManualCaptureReward.log`: `PASS: RQF17 interaccion manual resuelve captura real.`
- `Unity_PlayMode_Auditoria_ManualCaptureReward.log`: `PASS: RQF17 productos regresan al anaquel/inventario real.`
- `Unity_PlayMode_Auditoria_ManualCaptureReward.log`: `PASS: RQF17 captura otorga recompensa de puntos.`

Estado: VERDE.

### RQF18 - Seguridad automatica reforzada

Que pedia el documento:
- Nivel 0 sin arresto.
- Nivel 1/2/3 con 33/66/99%.
- Arresto automatico debe detener, recuperar, registrar y alertar.

Que existia antes:
- `EntrepreneurTreeSecurityAdapter` ya exponia nivel y porcentaje.
- Faltaba runner enfocado en arresto real con recuperacion.

Que corregi:
- Se agrego `SecurityAutoArrestPlayModeAuditRunner`.
- Mensaje de arresto automatico ahora es explicito: `Arresto automatico exitoso`.

Que prueba hice:
- Validar nivel 0.
- Desbloquear seguridad 1, 2 y 3.
- Confirmar chances 33/66/99.
- Forzar robo real con seguridad nivel 3 y semilla determinista.
- Confirmar arresto automatico y recuperacion.

Evidencia:
- `Unity_PlayMode_Auditoria_SecurityAutoArrest.log`: `PASS: RQF18 nivel 1 Camaras aplica 33%.`
- `Unity_PlayMode_Auditoria_SecurityAutoArrest.log`: `PASS: RQF18 nivel 2 Guardias aplica 66%.`
- `Unity_PlayMode_Auditoria_SecurityAutoArrest.log`: `PASS: RQF18 nivel 3 Alarmas/arcos aplica 99%.`
- `Unity_PlayMode_Auditoria_SecurityAutoArrest.log`: `PASS: RQF18 seguridad nivel 3 arresta robo real automaticamente.`

Estado: VERDE.

### RQF19 - Alertas visuales y sonoras

Que pedia el documento:
- Alertas para deteccion, robo iniciado, captura, arresto automatico, escape, recuperacion y valor perdido.
- Usar notificaciones y audio del asset.
- No duplicar ni usar mensajes genericos.

Que existia antes:
- `ShoplifterSystem.ShowShoplifterNotification()` ya usaba `UIGame.AddNotification`.
- `UIGame.AddNotification` ya reproduce `notificationClip` via `AudioSystem.Play2D`.

Que corregi:
- Mensajes de ladrones quedaron mas claros:
  - `Ladron detectado.`
  - `Robo en progreso. Valor objetivo: $X`
  - `Ladron detenido: productos recuperados.`
  - `Arresto automatico exitoso.`
  - `Ladron escapo: perdida de $X.`
- Se agrego runner `TheftAlertPlayModeAuditRunner`.

Que prueba hice:
- Confirmar `UIGame` y `notificationClip`.
- Ejecutar captura manual, escape y arresto automatico.
- Confirmar resumen coherente y ausencia de errores rojos.

Evidencia:
- `Unity_PlayMode_Auditoria_TheftAlert.log`: `PASS: RQF19 sistema visual de notificaciones existe.`
- `Unity_PlayMode_Auditoria_TheftAlert.log`: `PASS: RQF19 notificaciones usan audio existente del asset.`
- `Unity_PlayMode_Auditoria_TheftAlert.log`: `PASS: RQF19 eventos de deteccion/captura/escape/arresto generan notificaciones y resumen coherente.`

Estado: VERDE.

## 9. Bugs corregidos

- Tipos de ladron tenian visuales demasiado similares; ahora comun/sospechoso/especial se distinguen.
- Distribucion de tipos avanzados y valor objetivo eran poco sensibles al crecimiento; ahora escalan con dificultad.
- Alertas de robo eran menos especificas; ahora comunican robo en progreso, captura, arresto y perdida.
- Modo Admin podia forzar ladrón, pero no elegir tipo desde UI; ahora tiene selector.
- Faltaban runners especificos RQF15-RQF19; se agregaron seis entry points Play Mode.

## 10. Bugs pendientes

- La prueba de audio valida `notificationClip` y `AudioSystem`, pero Isaac debe escuchar el resultado en Editor normal.
- Los accesorios de ladron se generan con primitivas/materiales runtime del mismo estilo; si hay assets finales de mochila/gorra/lentes, conviene reemplazarlos.
- El runner valida arresto real nivel 3; niveles 1 y 2 se validan por probabilidad configurada, no por una muestra estadistica larga de robos reales.

## 11. Riesgos tecnicos

- Las probabilidades son aleatorias; los runners usan semillas deterministas donde hace falta.
- Si el set de productos premium cambia, el gating de especial debe volver a probarse con stock premium colocado visualmente.
- El Admin queda separado por `AdminSessionConfig.isActive`, pero debe seguir visible solo en flujo de prueba.

## 12. Que debe probar Isaac

- Abrir Modo Admin y forzar comun, sospechoso y especial.
- Confirmar visualmente: comun con gorra, sospechoso mas discreto, especial con mochila.
- Probar captura manual con tecla/accion real desde camara.
- Dejar escapar un ladron y confirmar notificacion/perdida.
- Desbloquear seguridad 1/2/3 y observar arrestos automaticos.
- Activar expansiones y revisar que haya mas actividad de robos.
- Escuchar notificaciones de robo/captura/escape/arresto.

## 13. Mejor siguiente fase recomendada

Siguiente fase recomendada: QA visual y balance fino de robos.

Objetivo:
- Reemplazar accesorios runtime por prefabs finales si existen.
- Hacer una muestra larga de 100-300 clientes para calibrar frecuencia real.
- Probar ladron especial con producto premium colocado en anaquel real.
- Ajustar mensajes/audio/camara para que el jugador perciba el evento sin saturacion.
