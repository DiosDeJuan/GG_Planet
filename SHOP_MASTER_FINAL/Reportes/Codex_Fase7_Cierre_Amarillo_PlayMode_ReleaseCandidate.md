<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 7 - Cierre AMARILLO, PlayMode dirigido y Release Candidate

## Encabezado

- Repositorio: `DiosDeJuan/GG_Planet`.
- Proyecto Unity valido: `SHOP_MASTER_FINAL/`.
- Asset base: `Store Simulator - Supermarket Game Template`.
- Rama base: `origin/codex/fase6-qa-playmode-estabilizacion-final`.
- Commit base esperado: `ec3d0ab`.
- Commit base verificado: `ec3d0abeac3890fc1776228419156ad12d0fdf0e`.
- Commit tecnico previo Fase 6: `f4cded8`.
- Rama nueva: `codex/fase7-cierre-amarillo-playmode-release-candidate`.
- Fecha de ejecucion: 2026-06-09.
- Responsable: Codex.

## Resumen

Fase 7 se centro en cerrar pendientes AMARILLOS que si eran abordables desde este entorno: ampliar regresion automatizada, validar PlayMode dirigido por Unity Test Runner, confirmar que no se duplican entradas de GESTION y reforzar pruebas sobre pricing, guardado y expansion.

Estado final: **RELEASE CANDIDATE TECNICO / AMARILLO POR QA MANUAL HUMANO PENDIENTE**.

No se agregaron features grandes. No se tocaron escenas ni prefabs. No se creo Canvas principal paralelo, inventario paralelo, delivery paralelo ni economia paralela.

## Pre-check git

Comandos ejecutados:

```text
git fetch origin
git status --short
git status --short -- .
git rev-parse HEAD
git rev-parse origin/codex/fase6-qa-playmode-estabilizacion-final
git checkout -B codex/fase7-cierre-amarillo-playmode-release-candidate origin/codex/fase6-qa-playmode-estabilizacion-final
git rev-parse HEAD
```

Resultado:

- `HEAD` inicial y rama base remota apuntaban a `ec3d0abeac3890fc1776228419156ad12d0fdf0e`.
- La rama Fase 7 fue creada desde la base correcta.
- El status global del root Git sigue mostrando ruido externo al proyecto por estar el root en `C:/Users/ijuan`.
- Dentro de `SHOP_MASTER_FINAL`, antes de trabajar solo estaban modificados los dos TMP preexistentes:
  - `Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Bangers SDF.asset`.
  - `Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset`.

## Revision documental y reportes

Se tomo como base la revision documental ya ampliada en Fase 6 y se revalido contra sus pendientes:

- `Documentos/NEW_Requerimientos.docx`.
- `Documentos/propuestas juanito (2).docx`.
- `Documentos/Protocolo prpuesta Juanito (1).pdf`.
- `Documentos/Guia Gantt (1).pdf`.
- `Reportes/Codex_Fase6_QA_PlayMode_Estabilizacion_Final.md`.
- `Reportes/Codex_Fase6_PlayModeResults.xml`.
- `Reportes/Codex_Fase6_UnityBatchmode.log`.

Pendientes AMARILLOS heredados:

- Play Mode manual humano completo.
- Validacion visual de layout final.
- Confirmar soporte fisico seguro para expansion de almacenamiento.
- Confirmar decision `UPGRADES` viejo vs `GESTION/ESPACIOS`.
- UI final dedicada para Monopoly/Bancarrota si se decide mas adelante.
- Devolucion fisica exacta a estanteria si el flujo conserva origen del item.
- Balance real de clientela y ladrones en partida jugada.

## Acciones realizadas

### Regresion PlayMode ampliada

Se amplio `Assets/StoreSimulator/Tests/PlayMode/StoreSimulatorPlayModeRegressionTests.cs`.

Pruebas existentes conservadas:

- `ProductPricingCalculator_ClampsPricesAndKeepsDocumentedProbabilities`.
- `SaveGameSystem_AlternateProfileKeyUsesDistinctSavePath`.

Pruebas nuevas:

- `ProductPricingCalculator_NullAndZeroIdealInputsStaySafe`.
  - Cubre producto nulo.
  - Cubre precio ideal cero.
  - Cubre clamp seguro sin NaN/Infinity.
  - Cubre probabilidad segura con datos incompletos.

- `ShopExpansionManager_ClampsAreasAndScalesRiskWithoutRunawayValues`.
  - Cubre area inicial de venta y almacenamiento.
  - Cubre maximos de venta/storage.
  - Cubre clientela escalada.
  - Cubre probabilidad de ladron 2% a 6.5%.
  - Protege contra valores descontrolados si el save trae unidades fuera de rango.

- `UIManagementUIBootstrap_EnsureKeepsSingleManagementEntry`.
  - Cubre que `GESTION` se monta dentro de `ContentArea`.
  - Cubre que el boton `GESTION` se monta en `Navigation/Categories`.
  - Cubre que llamar el bootstrap dos veces no duplica panel ni boton.
  - Se probo con GameObjects temporales, sin tocar escenas serializadas.

### Correcciones de codigo de produccion

No se encontraron bugs de produccion que justificaran tocar managers, escenas o prefabs en esta fase.

La unica modificacion de codigo fue la suite de regresion PlayMode existente. Esto es intencional: la fase pedia estabilizar sin convertir el proyecto en una pila de parches sueltos.

## Matriz de cierre AMARILLO

| Pendiente Fase 6 | Estado Fase 7 | Evidencia | Cierre |
| --- | --- | --- | --- |
| Play Mode manual humano | Pendiente | No se abrio Unity Editor interactivo con humano jugando | Sigue AMARILLO |
| Regresion automatizada limitada | Mejorado | PlayMode 5/5 passed | Cerrado tecnico |
| Pricing con casos borde | Validado | Test null/ideal cero/clamp | Cerrado tecnico |
| Expansion/clientela/riesgo ladrones | Validado parcialmente | Test areas, clamp, spawn, 2%-6.5% | Cerrado tecnico, balance manual pendiente |
| GESTION duplicada/listeners | Validado parcialmente | Test bootstrap doble, un boton y un panel | Cerrado tecnico |
| Storage fisico | Pendiente | No se tocaron escenas/prefabs sin soporte claro | Pendiente diseno/escena |
| Devolucion fisica caja | Pendiente | Fase 6 confirmo falta de origen confiable | Pendiente diseno/flujo |
| Finales Monopoly/Bancarrota UI | Pendiente | Hook estable, sin pantalla dedicada | Pendiente diseno |

## QA funcional por modulos

| Modulo | Resultado Fase 7 | Evidencia | Pendiente |
| --- | --- | --- | --- |
| PlayerController / movimiento | Sin cambios; no se detecto bug automatizable nuevo | Revision Fase 6, build verde | Play Mode humano |
| Computadora | GESTION protegida contra duplicado por test | `UIManagementUIBootstrap_EnsureKeepsSingleManagementEntry` | Validacion visual |
| ARBOL | Sin cambios; fuente sigue `EntrepreneurProgress` | Reportes Fase 6 | Prueba manual de desbloqueos |
| GESTION / ESPACIOS | Formula y clamps validados | `ShopExpansionManager_ClampsAreas...` | Storage fisico |
| GESTION / PRECIOS | Casos borde validados | Tests de pricing | UI visual |
| GESTION / AYUDA | Sin cambios | Bootstrap GESTION estable | Legibilidad manual |
| PRODUCTS / delivery | Sin cambios | Fase 6 + build | Compra real manual |
| Empleados | Sin cambios | Fase 6 + build | NPC/roles manual |
| Seguridad / ladrones | Riesgo por expansion validado | Test 2%-6.5% | Robo real manual |
| CashDesk / SelfCheckout | Sin cambios | Build verde | Pago/abandono manual |
| CustomerSystem | Spawn por expansion validado por formula | Test `GetExpandedCustomerSpawnRate` | Balance real |
| Stats / UIStats | Sin cambios | Build verde | Reporte diario manual |
| SaveGameSystem | Ruta alterna ya protegida | Test heredado Fase 6 sigue passed | Save/load completo manual |
| GameEndingService | Sin cambios | Build verde | UI final pendiente |
| Pricing | Centralizado y protegido | 2 tests PlayMode | Ninguno tecnico conocido |
| Expansion | Clamps protegidos | Test PlayMode nuevo | Visual/storage |

## Validaciones ejecutadas

### dotnet build

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

### Unity PlayMode batchmode

Comando:

```text
"C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\ijuan\SHOP_MASTER_FINAL" -runTests -testPlatform playmode -logFile "C:\Users\ijuan\SHOP_MASTER_FINAL\Reportes\Codex_Fase7_UnityBatchmode.log" -testResults "C:\Users\ijuan\SHOP_MASTER_FINAL\Reportes\Codex_Fase7_PlayModeResults.xml"
```

Resultado:

```text
Test run completed. Exiting with code 0 (Ok). Run completed.
```

XML:

```text
testcasecount="5"
result="Passed"
total="5"
passed="5"
failed="0"
inconclusive="0"
skipped="0"
```

Evidencia:

- `Reportes/Codex_Fase7_UnityBatchmode.log`.
- `Reportes/Codex_Fase7_PlayModeResults.xml`.

### Animo

Comandos:

```text
rg -n "Animo|animo" .\Assets .\Packages .\ProjectSettings .\Documentos
Get-ChildItem -LiteralPath . -Recurse -Force | Where-Object { $_.Name -match 'Animo|animo' }
```

Resultado:

- Sin referencias funcionales.
- Sin rutas `Animo/animo` dentro de `SHOP_MASTER_FINAL`.

### TMP preexistentes

Los dos archivos siguen modificados en working tree y se mantienen fuera del stage:

- `Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Bangers SDF.asset`.
- `Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset`.

## Inscripcion POMPIC

Incluyen inscripcion:

- `Assets/StoreSimulator/Tests/PlayMode/StoreSimulatorPlayModeRegressionTests.cs`.
- `Reportes/Codex_Fase7_Cierre_Amarillo_PlayMode_ReleaseCandidate.md`.

Excepciones:

- `Reportes/Codex_Fase7_PlayModeResults.xml`: generado por Unity Test Runner; no se modifica manualmente.
- `Reportes/Codex_Fase7_UnityBatchmode.log`: generado por Unity; no se modifica manualmente.

## Play Mode

PlayMode automatizado dirigido: **ejecutado realmente**.

- 5 tests.
- 5 passed.
- 0 failed.
- Sin errores C#.
- Sin `Compilation failed`.
- Sin `Fatal Error`.

Play Mode manual humano: **no ejecutado**.

Motivo:

- Desde este entorno se ejecuto Unity Test Runner batchmode, no una sesion interactiva con humano moviendo al jugador.

Checklist pendiente para Isaac:

1. Abrir escena principal.
2. Entrar a Play Mode.
3. Revisar consola sin errores rojos al inicio.
4. Caminar, correr, saltar, agacharse e interactuar.
5. Abrir computadora/laptop.
6. Revisar PRODUCTOS/SHOP.
7. Comprar Productos Basicos 1 si hay fondos.
8. Validar delivery/paquete.
9. Abrir ARBOL y probar mensajes de requisito/puntos.
10. Abrir EMPLEADOS y probar bloqueo/contratacion/asignacion.
11. Abrir GESTION/ESPACIOS y probar fondos insuficientes/compra.
12. Abrir GESTION/PRECIOS y probar `$0.00`, precio ideal y 300%.
13. Abrir GESTION/AYUDA y validar legibilidad.
14. Esperar clientes y validar caja.
15. Provocar espera en caja si es posible.
16. Probar seguridad/ladrones si el flujo lo permite.
17. Cerrar dia y revisar reporte diario.
18. Guardar/cargar y revisar persistencia.
19. Salir de Play Mode y revisar consola final.

## Estado Release Candidate

**Release Candidate tecnico: SI.**

**Estado global: AMARILLO controlado.**

Justificacion:

- Build .NET verde.
- Unity PlayMode automatizado verde.
- Regresion ampliada de 2 a 5 pruebas.
- No hay errores criticos conocidos.
- No se tocaron escenas/prefabs delicados.
- Queda pendiente QA manual humano y decisiones de diseno/escena.

## Pendientes reales

- Play Mode manual completo por Isaac.
- Validacion visual final de computadora, ARBOL, EMPLEADOS y GESTION.
- Confirmar representacion fisica de expansion de almacenamiento.
- Decidir si `UPGRADES` viejo debe redirigir a `GESTION/ESPACIOS`.
- Disenar UI final dedicada Monopoly/Bancarrota si se requiere.
- Resolver devolucion fisica exacta a estanteria solo si se agrega referencia de origen real.
- Balance fino de clientes/ladrones en partida jugada.

## Confirmaciones finales

- `Animo/` no fue reintroducido.
- No se stagearon los TMP prohibidos.
- No se sobrescribieron reportes anteriores.
- No se creo Canvas principal paralelo.
- No se creo inventario paralelo.
- No se creo delivery paralelo.
- No se creo economia paralela.
- No se hicieron cambios destructivos.
