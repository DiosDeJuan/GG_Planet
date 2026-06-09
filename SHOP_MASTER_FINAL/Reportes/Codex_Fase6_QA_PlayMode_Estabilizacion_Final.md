<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 6 - QA PlayMode, estabilizacion final y regresion integral

## Resumen

Fase 6 se enfoco en estabilizar y validar lo integrado hasta Fase 5, sin agregar otro bloque grande de features.

Estado final: **VERDE tecnico con pendiente manual visual**.

- Unity batchmode compila y ejecuta PlayMode Test Runner.
- PlayMode automatizado: 2 pruebas ejecutadas, 2 pasadas, 0 fallidas.
- `dotnet build` compila solucion y proyecto de pruebas sin warnings ni errores.
- Se corrigio una regresion quirurgica en carga de perfiles alternos de `SaveGameSystem`.
- No se reintrodujo `Animo/`.
- No se stagearon los assets TMP preexistentes marcados como intocables.

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

**VERDE tecnico / AMARILLO visual manual**.

La fase deja evidencia automatizada real de PlayMode, corrige una regresion de guardado/carga y no agrega features paralelas. Falta una pasada manual de gameplay visual completa para declarar verde total de experiencia.
