<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 15 - Cierre total del Arbol del Emprendedor

## Estado

VERDE tecnico.

## Base

- Rama base: `origin/codex/fase14-movimiento-input-playtest-reproducible`
- Commit base esperado: `4aafbd23a2bd2a394920a2eb28c85c8a8fa478a2`
- Rama de trabajo: `codex/fase15-cierre-total-arbol-emprendedor`

## Objetivo

Cerrar el Arbol del Emprendedor sin crear sistemas paralelos: validar grafo completo, ruta real desde la computadora, desbloqueo total, persistencia de efectos, logros, capturas y auditoria runtime.

## Cambios realizados

- Se amplio la regresion PlayMode existente en `StoreSimulatorPlayModeRegressionTests.cs`.
- Se agrego validacion Fase 15 de integridad del grafo: 37 nodos, ids unicos, prerequisitos existentes, alcanzabilidad desde `productos_basicos_1` y sin ciclos funcionales.
- Se agrego prueba de cierre completo: desbloquea todos los nodos con la ruta real de `EntrepreneurProgress.TryUnlock`, confirma Arbol completo, logro `arbol_completo`, seguridad nivel 3 y mejoras activas.
- Se agrego generacion de `Codex_Fase15_ArbolRuntimeAudit.txt` desde PlayMode.
- Se agrego generacion de capturas Fase 15 desde el prefab real `UIShopDesktop.prefab` y `OpenArbolForQA`.

## Evidencia esperada

- `Reportes/Codex_Fase15_ArbolRuntimeAudit.txt`
- `Reportes/Capturas_Fase15/`
- `Reportes/Codex_Fase15_UnityPlayMode.log`
- `Reportes/Codex_Fase15_PlayModeResults.xml`

## Evidencia generada

- `Reportes/Codex_Fase15_ArbolRuntimeAudit.txt`: 37 nodos, 37 alcanzables, 47 productos documentados, cierre completo PASS, errores 0.
- `Reportes/Capturas_Fase15/Arbol_Final_01_VistaGeneral.png`
- `Reportes/Capturas_Fase15/Arbol_Final_02_Productos.png`
- `Reportes/Capturas_Fase15/Arbol_Final_03_Empleados.png`
- `Reportes/Capturas_Fase15/Arbol_Final_04_Seguridad.png`
- `Reportes/Capturas_Fase15/Arbol_Final_05_Mejoras.png`
- `Reportes/Capturas_Fase15/Arbol_Final_06_QAOverlay.png`
- `Reportes/Capturas_Fase15/Arbol_Final_07_Logros.png`
- `Reportes/Capturas_Fase15/Arbol_Final_08_Completado.png`
- `Reportes/Codex_Fase15_PlayModeResults.xml`: 80 tests, 80 passed, 0 failed, 0 skipped.
- `Reportes/Codex_Fase15_UnityPlayMode.log`: `Test run completed. Exiting with code 0 (Ok). Run completed.`

## Inscripcion POMPIC

- Archivos `.cs` modificados: inscripcion presente al inicio.
- Archivos `.md` nuevos: inscripcion presente al inicio.
- Archivos generados `.txt`, `.xml`, `.log` y `.png`: sin inscripcion para no alterar evidencia generada por runtime o formato de salida.

## Validacion

- `dotnet build .\SHOP_MASTER_FINAL.sln`: correcto, 0 advertencias, 0 errores.
- Unity batchmode PlayMode:
  - Comando: `Unity.exe -batchmode -projectPath C:\Users\ijuan\SHOP_MASTER_FINAL -runTests -testPlatform PlayMode -logFile Reportes\Codex_Fase15_UnityPlayMode.log -testResults Reportes\Codex_Fase15_PlayModeResults.xml`
  - Resultado final: 80/80 passed.

## Nota de estabilizacion

La primera corrida PlayMode detecto una asercion demasiado estricta en la nueva prueba Fase 15: esperaba 0 puntos restantes al completar el Arbol, pero el flujo real otorga puntos extra por logros como parte del sistema existente. Se corrigio la prueba para validar que no haya deuda negativa ni efectos rotos, sin tocar la economia ni crear bypasses.

## Cambios excluidos del stage

- `Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Bangers SDF.asset`
- `Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset`

Ambos assets TMP aparecen modificados como preexistentes y se dejan fuera del commit por regla explicita.

## Riesgos abiertos

No quedan bloqueadores automaticos conocidos. Queda recomendado playtest humano final de Isaac siguiendo `Reportes/Codex_Fase15_Guia_Prueba_Arbol_Final.md`, porque las capturas y PlayMode validan estructura/ruta, pero no sustituyen la percepcion humana completa de gameplay.
