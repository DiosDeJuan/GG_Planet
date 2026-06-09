<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 6 - QA PlayMode, estabilizacion final y regresion integral

## Resumen

Fase 6 se enfoco en estabilizar y validar lo integrado hasta Fase 5, sin agregar otro bloque grande de features.

Estado final: **AMARILLO**.

- Unity batchmode compila y ejecuta PlayMode Test Runner automatizado.
- PlayMode automatizado: 2 pruebas ejecutadas, 2 pasadas, 0 fallidas.
- `dotnet build` compila solucion y proyecto de pruebas sin warnings ni errores.
- Se corrigio una regresion quirurgica en carga de perfiles alternos de `SaveGameSystem`.
- No se reintrodujo `Animo/`.
- No se stagearon los assets TMP preexistentes marcados como intocables.
- Play Mode manual humano queda pendiente para Isaac.

## Base de trabajo

- Repositorio: `DiosDeJuan/GG_Planet`.
- Proyecto Unity valido: `SHOP_MASTER_FINAL/`.
- Asset base: `Store Simulator - Supermarket Game Template`.
- Rama base indicada: `origin/codex/fase5-cierre-espacios-precios-reportes-finales-qa`.
- Commit base esperado: `6898077`.
- Commit base verificado:

```text
6898077f0c5cd5ce2a993ebd0f3cd8b73f0e1fbc
```

- Rama creada:

```text
codex/fase6-qa-playmode-estabilizacion-final
```

## Pre-check git

Comandos ejecutados:

```text
git fetch origin
git status --short
git rev-parse HEAD
git rev-parse origin/codex/fase5-cierre-espacios-precios-reportes-finales-qa
```

Resultado:

- `HEAD` y rama remota base coincidian en `6898077f0c5cd5ce2a993ebd0f3cd8b73f0e1fbc`.
- El root Git real esta en `C:/Users/ijuan`, por encima de `SHOP_MASTER_FINAL`.
- El status global muestra ruido externo al proyecto y un `Animo/` no rastreado fuera del alcance; no se uso ni se stageo.
- Se detectaron como modificados preexistentes los dos assets TMP indicados por el usuario; quedaron fuera del stage.

## Auditoria realizada

Se revisaron los frentes de Fase 5:

- `SaveGameSystem`.
- `ProductPricingCalculator`.
- `ShopExpansionManager`.
- `GameEndingService`.
- `StatsDatabase`.
- `CashDesk`.
- `Customer`.
- `UIManagementPanel`.
- Reporte de Fase 5 y log Unity batchmode de Fase 5.

No se crearon sistemas paralelos de economia, inventario, delivery, caja, finales, reportes ni UI principal.

## Correccion quirurgica

### `SaveGameSystem.Load(otherKey)`

Hallazgo:

- `SaveGameSystem.Save(otherKey)` calculaba y escribia el archivo alterno correctamente.
- `SaveGameSystem.Load(otherKey)` calculaba `fileName`, pero luego leia siempre `save.dat`.
- Esto rompia perfiles alternos o cualquier carga con llave distinta, aunque el metodo publico lo permitia.

Correccion:

- Se agrego `GetSavePath(otherKey)`.
- `Save()` y `Load()` usan la misma ruta centralizada.
- `Load("perfil")` ahora busca `perfil.dat`, no `save.dat`.

Archivo:

- `Assets/StoreSimulator/Scripts/SaveGameSystem.cs`.

## Regresion PlayMode agregada

Se agrego una suite PlayMode pequena y enfocada:

- `Assets/StoreSimulator/Tests/PlayMode/StoreSimulatorPlayModeRegressionTests.asmdef`.
- `Assets/StoreSimulator/Tests/PlayMode/StoreSimulatorPlayModeRegressionTests.cs`.

Pruebas:

1. `ProductPricingCalculator_ClampsPricesAndKeepsDocumentedProbabilities`
   - Verifica precio ideal.
   - Verifica maximo 300%.
   - Verifica minimo `$0.00`.
   - Verifica probabilidad completa en precio ideal.
   - Verifica que sobreprecio reduce probabilidad.
   - Verifica compra extra con precio bajo.

2. `SaveGameSystem_AlternateProfileKeyUsesDistinctSavePath`
   - Verifica que el save default use `save.dat`.
   - Verifica que una llave alterna use `fase6_profile.dat`.
   - Verifica que ambas rutas sean distintas.

Nota tecnica:

- El proyecto principal vive en `Assembly-CSharp`, sin asmdef propio.
- Para evitar reestructurar todo el asset, los tests usan reflexion contra `Assembly-CSharp`.
- Esto permite PlayMode Test Runner sin mover sistemas existentes ni convertir el proyecto a asmdefs.

## PlayMode Test Runner

El primer intento con `-quit` compilo, pero no ejecuto tests ni genero XML.

Se habilito:

```text
ProjectSettings.asset -> playModeTestRunnerEnabled: 1
```

La corrida efectiva uso Unity batchmode sin `-quit`, dejando que `-runTests` cerrara el editor:

```text
"C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\ijuan\SHOP_MASTER_FINAL" -runTests -testPlatform playmode -logFile "C:\Users\ijuan\SHOP_MASTER_FINAL\Reportes\Codex_Fase6_UnityBatchmode.log" -testResults "C:\Users\ijuan\SHOP_MASTER_FINAL\Reportes\Codex_Fase6_PlayModeResults.xml"
```

Resultado XML:

```text
testcasecount="2"
result="Passed"
total="2"
passed="2"
failed="0"
inconclusive="0"
skipped="0"
```

Evidencia:

- `Reportes/Codex_Fase6_UnityBatchmode.log`.
- `Reportes/Codex_Fase6_PlayModeResults.xml`.

## Validaciones ejecutadas

### Unity PlayMode

Resultado:

```text
Test run completed. Exiting with code 0 (Ok). Run completed.
```

Resumen:

- 2 tests PlayMode.
- 2 passed.
- 0 failed.

### Compilacion .NET

Comando:

```text
dotnet build .\SHOP_MASTER_FINAL.sln --no-restore
```

Resultado:

```text
Compilacion correcta.
0 Advertencia(s)
0 Errores
```

### Whitespace

Comando:

```text
git diff --check -- .
```

Resultado:

- Sin errores de whitespace.
- Git aviso que algunos archivos LF seran reemplazados por CRLF cuando Git los toque.

### Busqueda funcional `Animo`

Comando:

```text
rg -n "Animo" .\Assets .\Packages .\ProjectSettings .\Documentos
```

Resultado:

- Sin coincidencias funcionales.

## Assets TMP excluidos

Se mantuvieron fuera del stage:

- `Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Bangers SDF.asset`.
- `Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset`.

Motivo:

- Cambios preexistentes no relacionados.
- El usuario pidio no tocarlos ni stagearlos.

## Inscripcion POMPIC

Incluyen inscripcion:

- `Assets/StoreSimulator/Scripts/SaveGameSystem.cs`.
- `Assets/StoreSimulator/Tests/PlayMode/StoreSimulatorPlayModeRegressionTests.cs`.
- `Reportes/Codex_Fase6_QA_PlayMode_Estabilizacion_Final.md`.

Excepciones justificadas:

- `.meta`: Unity los genera y no deben recibir comentarios.
- `.asmdef`: JSON estricto; comentarios romperian el formato.
- `.csproj` y `.sln`: generados por Unity/Visual Studio Tools; la cabecera indica que cambios manuales se sobrescriben.
- `ProjectSettings/ProjectSettings.asset`: asset serializado de Unity; no admite comentario POMPIC sin riesgo de romper formato.
- `ProjectSettings/SceneTemplateSettings.json`: generado por Unity; JSON estricto.
- `Reportes/Codex_Fase6_PlayModeResults.xml`: generado por Unity Test Runner.
- `Reportes/Codex_Fase6_UnityBatchmode.log`: generado por Unity.

## Archivos de Fase 6

Cambios funcionales:

- `Assets/StoreSimulator/Scripts/SaveGameSystem.cs`.
- `ProjectSettings/ProjectSettings.asset`.

Pruebas:

- `Assets/StoreSimulator/Tests/PlayMode/StoreSimulatorPlayModeRegressionTests.asmdef`.
- `Assets/StoreSimulator/Tests/PlayMode/StoreSimulatorPlayModeRegressionTests.cs`.
- Metas generadas por Unity para la carpeta y archivos de tests.

Evidencia:

- `Reportes/Codex_Fase6_QA_PlayMode_Estabilizacion_Final.md`.
- `Reportes/Codex_Fase6_UnityBatchmode.log`.
- `Reportes/Codex_Fase6_PlayModeResults.xml`.

Regenerados por Unity/IDE:

- `Assembly-CSharp.csproj`.
- `FLOBUK.StoreSimulator.PlayModeTests.csproj`.
- `SHOP_MASTER_FINAL.sln`.
- `ProjectSettings/SceneTemplateSettings.json`.

## Limitaciones honestas

- No se hizo QA visual manual interactivo dentro del editor con humano moviendo al jugador.
- La suite PlayMode cubre regresiones tecnicas acotadas, no reemplaza una pasada manual completa de UI, layout, clientes, caja y reportes diarios.
- Siguen vigentes pendientes de Fase 5 que requieren decision de diseno o referencias fisicas del asset:
  - representacion fisica de almacenamiento expandido si existe en escenas/prefabs;
  - devolucion fisica de productos abandonados en caja;
  - pantalla final dedicada para Monopoly/Bancarrota;
  - balance fino de crecimiento de clientes por expansion.

## Checklist QA manual recomendado

1. Abrir escena principal.
2. Entrar a Play Mode.
3. Abrir computadora/laptop.
4. Confirmar app `GESTION`.
5. Revisar `ESPACIOS`, `PRECIOS` y `AYUDA`.
6. Comprar espacio de venta con fondos suficientes.
7. Confirmar descuento, m2 y progreso.
8. Comprar almacenamiento y confirmar persistencia visual.
9. Ajustar precio a `$0.00`.
10. Subir precio por encima de 300% y confirmar clamp.
11. Observar clientes con precio ideal, bajo y alto.
12. Dejar espera en caja y confirmar alerta/cliente perdido.
13. Cerrar dia y revisar renta, luz, agotados y clientes perdidos.
14. Guardar/cargar y confirmar persistencia.
15. Revisar consola sin errores rojos.

## Estado final

**AMARILLO**.

La fase deja evidencia automatizada real de PlayMode, corrige una regresion de guardado/carga y no agrega features paralelas. Falta una pasada manual de gameplay visual completa para declarar verde total de experiencia.

## Anexo ampliado solicitado por Fase 6

### Encabezado operativo

- Fase: Codex Fase 6 - QA Play Mode, estabilizacion final y regresion integral.
- Rama base: `origin/codex/fase5-cierre-espacios-precios-reportes-finales-qa`.
- Commit base esperado: `6898077`.
- Commit base remoto verificado nuevamente: `6898077f0c5cd5ce2a993ebd0f3cd8b73f0e1fbc`.
- Rama nueva: `codex/fase6-qa-playmode-estabilizacion-final`.
- Fecha de ejecucion: 2026-06-09.
- Responsable: Codex.
- Proyecto: `SHOP_MASTER_FINAL`.
- Repositorio: `DiosDeJuan/GG_Planet`.

### Documentos revisados

Se reviso el contenido disponible en `Documentos/` antes de ampliar esta auditoria:

- `NEW_Requerimientos.docx`: fuente principal de RQF; confirma computadora, arbol, expansiones, empleados, seguridad, ladrones, guardado, clientela, pagos, precios y ventas.
- `propuestas juanito (2).docx`: confirma concepto SHOP MASTER, oficina/laptop como centro de gestion, arbol RPG, espacios, empleados, compra, tableta de construccion, productos iniciales y progresion por logros.
- `Protocolo prpuesta Juanito (1).pdf`: confirma objetivos de simulacion empresarial, inventario, precios, clientes, empleados, seguridad, reportes diarios y guardado automatico.
- `Guia Gantt (1).pdf`: confirma que las actividades deben mapear requerimientos, responsable, fecha y duracion; ninguna actividad debe superar 6 horas.
- `Reportes/Codex_Fase5_Cierre_Espacios_Precios_Reportes_Finales_QA.md`: fuente de pendientes reales de Fase 5.

Hallazgos documentales usados:

- Productos Basicos 1 iniciales: Leche, Sal, Agua, Pasta y Azucar.
- Venta: espacios de 16 m2 por `$1,750`.
- Almacenamiento: espacios de 32 m2 por `$2,500`.
- Seguridad automatica: 33%, 66%, 99%.
- Ladrones por expansion: hasta 6.5%.
- Clientes: base documental 50-75 y crecimiento con expansion.
- Precios: minimo `$0.00`, maximo 300% del precio ideal.
- Reporte diario: ventas, gastos, salarios, robos, renta, luz, agotados, clientes perdidos y desempeno.
- Gantt: actividades menores o iguales a 6 horas.

### Alcance de la fase

Esta fase fue de QA, estabilizacion, regresion y correccion quirurgica. No se busco rehacer el juego.

Confirmado que no se reescribio:

- Arbol del Emprendedor.
- Sistema de clientes.
- Inventario.
- Delivery.
- Economia.
- Canvas principal.

### Matriz de requerimientos

| Requerimiento | Modulo | Estado observado | Evidencia / validacion | Accion tomada | Pendiente |
| --- | --- | --- | --- | --- | --- |
| RQF1-RQF2 | Computadora / productos | Validado por codigo y build | `UIShopDesktop`, `UIShopItemProduct`, `StoreDatabase`, Fase 5 | Sin cambio | Play Mode manual de compra completa |
| RQF3-RQF4 | ARBOL | Validado por codigo y reportes previos | `EntrepreneurTree/*`, Fase 4.3/4.4 | Sin cambio | Play Mode manual de UI visual |
| RQF5-RQF7 | GESTION / ESPACIOS | Validado por codigo y batchmode | `UIManagementPanel`, `ShopExpansionManager` | Sin cambio | Plano visual y storage fisico si asset lo permite |
| RQF8-RQF9 | Empleados | Validado por codigo | `Employees/*`, `EmployeeManager` | Sin cambio | Play Mode manual de contratacion/rol |
| RQF10-RQF12 | Seguridad | Validado por codigo | `SecurityManager`, `EntrepreneurProgress` | Sin cambio | Prueba manual de desbloqueos |
| RQF13-RQF16 | Ladrones | Parcial por codigo | `SecurityManager`, `ShoplifterAgent`, `ShopExpansionManager` | Sin cambio | Balance y aparicion en Play Mode real |
| RQF17-RQF20 | Intercepcion y reporte de robos | Validado por codigo/build | `ShoplifterInteractableProxy`, `StatsDatabase`, `UIStats` | Sin cambio | Prueba manual de robo/escape |
| RQF21 | Guardado | Validado por codigo, build y PlayMode automatizado parcial | `SaveGameSystem`, test de ruta alterna | Corregido `Load(otherKey)` | Prueba manual de save/load de partida completa |
| RQF22 | Clientela | Validado por codigo parcial | `CustomerSystem`, `ShopExpansionManager.GetExpandedCustomerSpawnRate` | Sin cambio | Balance real de 50-75 vs asset actual |
| RQF23-RQF24 | Pagos | Validado por codigo | `CashDesk`, `SelfCheckout`, `UICashDeskTerminal`, `UICashDeskRegister` | Sin cambio | Play Mode manual tarjeta/efectivo |
| RQF25-RQF27 empleados | Gestion empleados | Validado por codigo | `UIEmployeesPanel`, `EmployeeRuntimeAgent`, `EmployeeManager` | Sin cambio | Prueba manual de NPC, cajero y surtidor |
| RQF28-RQF29 precios/ventas | Pricing / Customer / caja | Validado por codigo y PlayMode automatizado parcial | `ProductPricingCalculator`, test PlayMode, `Customer`, `CashDesk` | Test de regresion agregado | Prueba manual de venta real y UI |
| RQF25-RQF28 duplicados precios | Precios ampliados | Validado por codigo y test | `UIPriceLabelWindow`, `UIManagementPanel`, `ProductPricingCalculator` | Test de clamp/probabilidad agregado | Prueba visual de textos |
| RQF34-RQF36 | Reportes / caja / instrucciones | Validado por codigo parcial | `StatsDatabase`, `UIStats`, `CashDesk`, `UIManagementPanel` | Sin cambio | Play Mode manual de reporte diario |
| RQNF1-RQNF5 | Usabilidad / integracion | Validado por revision | No Canvas paralelo, apps dentro de computadora | Sin cambio | QA visual en resolucion final |
| RQNF6-RQNF10 | Persistencia / estabilidad | Validado por build y test | `SaveGameSystem`, PlayMode 2/2 | Corregido save alterno | Prueba manual save completo |
| RQNF11-RQNF15 | Precios / consistencia | Validado por test | `ProductPricingCalculator_ClampsPrices...` | Test agregado | Ajuste de UX si hay texto cortado |
| RQNF16-RQNF20 | Rendimiento / reporte / accesibilidad | Validado por batchmode parcial | Unity batchmode exit 0, reporte Fase 6 | Sin cambio | Perfilado y Play Mode humano |

### QA funcional por modulos

| Modulo | Que se reviso | Resultado | Bugs encontrados | Bugs corregidos | Pendientes |
| --- | --- | --- | --- | --- | --- |
| PlayerController / movimiento | Action maps, cursor, movimiento, desuscripcion | Sin NRE evidente por codigo; `UI` map protegido | Ninguno nuevo | Ninguno | Probar caminar/correr/saltar/agacharse en Play Mode real |
| Computadora | Apps en `ContentArea`, navigation, listeners | Integracion mantiene `UIShopDesktop` real | Ninguno nuevo | Ninguno | Validar solapes y texto cortado visualmente |
| ARBOL | Entrada ARBOL, nodos, prerequisitos, logros | Fuente de verdad sigue en `EntrepreneurProgress` | Ninguno nuevo | Ninguno | Play Mode manual de unlock y mensajes |
| GESTION / ESPACIOS | Costos, fondos, m2, storage, final hooks | Codigo consistente con Fase 5 | Storage fisico sigue no confirmado | Ninguno | Confirmar soporte fisico de almacenamiento |
| GESTION / PRECIOS | Clamp, precio ideal, probabilidades, UI | Test PlayMode cubre clamp/probabilidad | Ninguno nuevo | Test agregado | Validar UI y guardado visual |
| GESTION / AYUDA | Ubicacion dentro de GESTION, texto guia | Existe y no crea Canvas paralelo | Ninguno nuevo | Ninguno | Validar legibilidad en resolucion final |
| PRODUCTS / compra / delivery | Productos iniciales, bloqueo, compra, delivery | Productos Basicos 1 existen en assets | Ninguno nuevo | Ninguno | Compra real en Play Mode |
| Empleados / cajero / surtidor | Contratacion, roles, salarios, NPC | Integrado por codigo | Ninguno nuevo | Ninguno | Prueba de asignacion real |
| Seguridad / ladrones | Niveles, probabilidades, metricas | Codigo compila y conecta a stats | Ninguno nuevo | Ninguno | Provocar robo real y revisar alertas |
| CashDesk / SelfCheckout | Cobro, abandono, limpieza de fila | Codigo limpia cliente abandonado | No hay origen fisico para devolver producto | Ninguno | Devolucion exacta si el asset conserva origen en futuro |
| CustomerSystem | Spawn, productos, rechazo por precio | Usa `ShopExpansionManager` y `ProductPricingCalculator` | Posible balance pendiente | Ninguno | Balance real con clientes en escena |
| StatsDatabase / UIStats | Reporte diario, gastos, seguridad, empleados | Campos persistidos y mostrados por codigo | Ninguno nuevo | Ninguno | Validar pantalla de cierre de dia |
| SaveGameSystem | Persistencia de sistemas nuevos y llave alterna | Bug real encontrado | `Load(otherKey)` leia `save.dat` | Corregido | Probar save/load completo en partida |
| GameEndingService | Monopoly/Bancarrota, persistencia | Hooks estables por codigo | Sin UI final dedicada | Ninguno | Decision de diseno para pantalla final |
| Pricing | Formula central, clamp, NaN/Infinity | Test PlayMode verde | Ninguno nuevo | Test agregado | Probar UI fisica de etiqueta |
| Expansion | Venta/storage, costos, guardado, ladrones | Codigo consistente | Storage fisico no confirmado | Ninguno | Validar escena/prefabs antes de tocar serializados |

### Bugs encontrados

| ID | Modulo | Descripcion | Severidad | Causa raiz | Archivo(s) | Estado |
| --- | --- | --- | --- | --- | --- | --- |
| F6-BUG-001 | SaveGameSystem | `Load(otherKey)` ignoraba la llave alterna y leia siempre `save.dat` | Media | Ruta de carga duplicada y no centralizada | `SaveGameSystem.cs` | Corregido |
| F6-BUG-002 | QA PlayMode | Con `-quit`, Unity compilaba pero no ejecutaba Test Runner ni generaba XML | Baja / entorno | La ejecucion CLI cerraba antes de correr tests | Comando QA / reporte | Mitigado usando `-runTests` sin `-quit` |
| F6-PEND-001 | Expansion storage | No hay confirmacion de representacion fisica segura para almacenamiento | Media | Requiere escena/prefab compatible | `ShopExpansionManager` | Pendiente diseno/escena |
| F6-PEND-002 | CashDesk | No hay referencia al origen fisico para devolver productos al estante al abandonar caja | Media | El flujo en caja ya tiene item instanciado en conveyor | `CashDesk.cs` | Pendiente si se amplian referencias |

### Bugs corregidos

| ID | Correccion aplicada | Archivo(s) modificados | Riesgo | Como se valido |
| --- | --- | --- | --- | --- |
| F6-BUG-001 | Se agrego `GetSavePath(otherKey)` y `Save`/`Load` usan la misma ruta | `Assets/StoreSimulator/Scripts/SaveGameSystem.cs` | Bajo | `dotnet build`, Unity batchmode, PlayMode test `SaveGameSystem_AlternateProfileKeyUsesDistinctSavePath` |
| F6-QA-001 | Se agrego suite PlayMode de regresion y se habilito PlayMode Test Runner | `Assets/StoreSimulator/Tests/PlayMode/*`, `ProjectSettings.asset` | Bajo | XML `Codex_Fase6_PlayModeResults.xml`: 2/2 passed |

### Actividades tipo Gantt

| ID | Actividad | Requerimientos relacionados | Responsable | Fecha | Duracion estimada | Resultado |
| --- | --- | --- | --- | --- | ---: | --- |
| G1 | Preparacion de rama y validacion base | Todos | Codex | 2026-06-09 | 0.5 h | Base `6898077` verificada |
| G2 | Revision Documentos y Reportes | Todos | Codex | 2026-06-09 | 1.5 h | Documentos y Fase 5 revisados |
| G3 | Auditoria computadora, ARBOL y GESTION | RQF1-RQF9 | Codex | 2026-06-09 | 1.5 h | Sin cambios invasivos |
| G4 | Auditoria compra, delivery, inventario y precios | RQF1, RQF2, RQF28-RQF29 | Codex | 2026-06-09 | 1.0 h | Test pricing agregado |
| G5 | Auditoria empleados, seguridad y ladrones | RQF8-RQF20 | Codex | 2026-06-09 | 1.0 h | Pendientes manuales documentados |
| G6 | Auditoria clientes, caja, reportes y guardado | RQF21-RQF24, RQF34-RQF36 | Codex | 2026-06-09 | 1.5 h | Bug save alterno corregido |
| G7 | Correcciones quirurgicas | RQF21, RQNF persistencia | Codex | 2026-06-09 | 1.0 h | `Load(otherKey)` corregido |
| G8 | Validacion `dotnet build` | RQNF estabilidad | Codex | 2026-06-09 | 0.5 h | 0 errores, 0 warnings |
| G9 | Validacion Unity batchmode / PlayMode | RQNF estabilidad | Codex | 2026-06-09 | 1.0 h | 2/2 PlayMode passed |
| G10 | Revision git diff/check/stage | Control QA | Codex | 2026-06-09 | 0.5 h | TMP y Animo fuera del stage |
| G11 | Elaboracion reporte | Evidencia | Codex | 2026-06-09 | 1.5 h | Reporte ampliado |
| G12 | Commit y push | Entrega | Codex | 2026-06-09 | 0.5 h | Rama publicada |

### Validaciones ejecutadas ampliadas

Comandos y resultados:

```text
git rev-parse origin/codex/fase5-cierre-espacios-precios-reportes-finales-qa
```

Resultado: `6898077f0c5cd5ce2a993ebd0f3cd8b73f0e1fbc`.

```text
git status --short -- .
```

Resultado final de proyecto despues del primer commit/push: solo TMP preexistentes modificados y no stageados.

```text
git diff --check -- .
git diff --cached --check
```

Resultado: sin errores despues de limpiar whitespace en archivos generados stageados.

```text
dotnet build .\SHOP_MASTER_FINAL.sln --no-restore
```

Resultado: compilacion correcta, 0 advertencias, 0 errores.

```text
"C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\ijuan\SHOP_MASTER_FINAL" -runTests -testPlatform playmode -logFile "C:\Users\ijuan\SHOP_MASTER_FINAL\Reportes\Codex_Fase6_UnityBatchmode.log" -testResults "C:\Users\ijuan\SHOP_MASTER_FINAL\Reportes\Codex_Fase6_PlayModeResults.xml"
```

Resultado: `Test run completed. Exiting with code 0 (Ok). Run completed.`

```text
rg -n "Animo" .\Assets .\Packages .\ProjectSettings .\Documentos
```

Resultado: sin coincidencias funcionales.

```text
Get-ChildItem -LiteralPath . -Recurse -Force | Where-Object { $_.Name -match 'Animo|animo' }
```

Resultado: sin rutas dentro de `SHOP_MASTER_FINAL`.

```text
git status --short -- "Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Bangers SDF.asset"
git status --short -- "Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset"
```

Resultado: ambos aparecen modificados en working tree, no stageados.

```text
git diff --cached --name-only | Select-String -Pattern "Animo|Bangers SDF.asset|Roboto-Bold SDF.asset"
```

Resultado antes del commit: sin coincidencias.

### Play Mode

Play Mode automatizado: **ejecutado realmente** con Unity Test Runner en batchmode.

- Resultado: 2 tests, 2 passed, 0 failed.
- Evidencia: `Reportes/Codex_Fase6_PlayModeResults.xml`.

Play Mode manual humano: **pendiente para Isaac**.

No se jugo manualmente con input humano dentro del editor. Por eso el estado integral no debe declararse VERDE total.

Checklist manual para Isaac:

1. Abrir escena principal.
2. Entrar a Play Mode.
3. Ver consola sin errores rojos al iniciar.
4. Caminar, correr, saltar, agacharse e interactuar.
5. Abrir computadora.
6. Abrir PRODUCTOS/SHOP y comprar Productos Basicos 1.
7. Validar delivery y paquete.
8. Abrir ARBOL y revisar nodos, puntos, requisitos y mensajes.
9. Abrir EMPLEADOS y contratar/asignar si hay recursos.
10. Abrir GESTION/ESPACIOS y validar compra o bloqueo por fondos.
11. Abrir GESTION/PRECIOS, cambiar precio y confirmar.
12. Abrir AYUDA y revisar legibilidad.
13. Esperar clientes y validar caja/venta/espera.
14. Provocar alerta por espera si es posible.
15. Validar seguridad/ladrones si se puede sin trucos invasivos.
16. Cerrar dia y revisar reporte diario.
17. Guardar/cargar y confirmar persistencia.
18. Salir de Play Mode y revisar consola.

### Confirmaciones obligatorias

- `Animo/` no fue reintroducido.
- No se stagearon `Bangers SDF.asset` ni `Roboto-Bold SDF.asset`.
- No se creo Canvas principal paralelo.
- No se creo inventario paralelo.
- No se creo delivery paralelo.
- No se creo economia paralela.
- No se sobrescribieron reportes anteriores.
- Los archivos modificados incluyen inscripcion cuando aplica.
- Las excepciones de inscripcion estan justificadas por formato generado, JSON estricto, XML generado o serializacion Unity.

### Estado final ampliado

**AMARILLO**.

Motivo:

- `dotnet build` correcto.
- Unity batchmode / PlayMode automatizado correcto.
- No hay errores criticos conocidos.
- Play Mode manual humano queda pendiente para Isaac.
- Persisten pendientes menores/visuales/de diseno heredados de Fase 5.

### Pendientes reales ampliados

- Play Mode manual completo por Isaac.
- Validacion visual de layout en resolucion final.
- Confirmar representacion fisica de expansion de almacenamiento si hay soporte seguro en escenas/prefabs.
- Confirmar decision de diseno sobre `UPGRADES` viejo vs `GESTION/ESPACIOS`.
- UI final dedicada para Monopoly/Bancarrota solo si se decide implementarla mas adelante.
- Devolucion fisica exacta de productos a estanteria si el asset llega a conservar origen del item.
- Balance real de clientela por expansion con partida jugada.
- Prueba manual de ladrones, escape, arresto automatico y metricas del reporte diario.
