<!-- Adaptado por POMPIC 20100333 -->

# Codex Fase 11 - QA Visual, Capturas, Arbol, Productos y Empleados

## 26.1 Resumen ejecutivo

Estado final: VERDE.

Fase 11 se enfoco en evidencia visual real, regresion PlayMode y correccion quirurgica. No se agregaron sistemas paralelos de Arbol, productos, precios, empleados, guardado ni UI. Se extendio la regresion existente con un flujo de capturas automatizadas usando componentes reales del asset y se corrigio una fuga menor de evento en `UIShopDesktop`.

## 26.2 Base y rama

- Rama base solicitada: `origin/codex/fase10-cierre-visual-arbol-productos-empleados`
- Commit base esperado: `366905b`
- HEAD inicial verificado: `366905b98694d0fe5bba8ae8c5dcb85378bd7e37`
- Rama de trabajo: `codex/fase11-qa-visual-capturas-arbol-productos-empleados`

## 26.3 Archivos modificados

- `Assets/StoreSimulator/Tests/PlayMode/StoreSimulatorPlayModeRegressionTests.cs`
- `Assets/StoreSimulator/Scripts/UIShopDesktop.cs`
- `Reportes/Codex_Fase11_QA_Visual_Capturas_Arbol_Productos_Empleados.md`
- `Reportes/Codex_Fase11_UnityPlayMode.log`
- `Reportes/Codex_Fase11_PlayModeResults.xml`
- `Reportes/Capturas_Fase11/*.png`

No se modificaron ni stagearon los assets TMP indicados por la fase.

## 26.4 Inscripciones POMPIC

- `.cs`: conservan `//Adaptado por POMPIC 20100333`.
- `.md`: este reporte incluye `<!-- Adaptado por POMPIC 20100333 -->`.
- PNG/XML/log: sin inscripcion por ser artefactos generados/evidencia; agregar comentarios romperia o ensuciaria el formato.

## 26.5 Cambio tecnico

Se agregaron dos pruebas PlayMode:

- `VisualEvidence_CaptureDirectoryCanBeCreated`
- `VisualEvidence_GeneratesAutomatedUnityCaptures`

La prueba visual monta fixtures temporales con componentes reales:

- `EntrepreneurTreeUI`
- `UIEmployeesPanel`
- `UIManagementPanel`
- `UIShopItemProduct`
- `UIShopDesktop.OptimizeNavigationLayout`

No se crean managers alternos ni flujos de UI ajenos al asset.

## 26.6 Bug corregido

`UIShopDesktop.OnDestroy()` no removia la suscripcion a `StoreDatabase.onLevelUpdate`. Se corrigio para evitar callbacks a instancias destruidas durante pruebas, recargas o desmontajes de UI.

## 26.7 Evidencia de compilacion

Comando:

```powershell
dotnet build .\SHOP_MASTER_FINAL.sln --no-restore
```

Resultado:

- Compilacion correcta.
- 0 advertencias.
- 0 errores.

## 26.8 Evidencia PlayMode final

Comando final usado para evidencia visual real:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe" -batchmode -projectPath "C:\Users\ijuan\SHOP_MASTER_FINAL" -runTests -testPlatform PlayMode -logFile "C:\Users\ijuan\SHOP_MASTER_FINAL\Reportes\Codex_Fase11_UnityPlayMode.log" -testResults "C:\Users\ijuan\SHOP_MASTER_FINAL\Reportes\Codex_Fase11_PlayModeResults.xml"
```

Resultado XML:

- `total="51"`
- `passed="51"`
- `failed="0"`
- `skipped="0"`

Log:

- `Reportes/Codex_Fase11_UnityPlayMode.log`
- Renderer usado: `Direct3D 11.0`, `NVIDIA GeForce RTX 3050 Ti Laptop GPU`
- Cierre: `Test run completed. Exiting with code 0 (Ok). Run completed.`

## 26.9 Nota sobre -nographics

La corrida con `-nographics` usa `NullGfx`, lo que impide validar capturas visuales reales: Unity puede producir imagen plana o fallar `RenderTexture`. La prueba detecta esa condicion y omite la captura con mensaje explicito. La evidencia final de capturas se genero en batchmode sin `-nographics`, con Direct3D activo.

## 26.10 Capturas generadas

Carpeta:

`Reportes/Capturas_Fase11/`

| Archivo | Evidencia |
|---|---:|
| `Arbol_01_VistaGeneral.png` | 107665 bytes |
| `Arbol_02_DetalleProducto.png` | 103434 bytes |
| `Arbol_03_NodoBloqueado.png` | 105777 bytes |
| `Arbol_04_Logros.png` | 54627 bytes |
| `Empleados_01_Grid.png` | 67119 bytes |
| `Empleados_02_DetalleEmpleado.png` | 68581 bytes |
| `Products_01_CatalogoCompleto.png` | 201803 bytes |
| `Products_02_ProductoPlaceholder.png` | 101211 bytes |
| `Precios_01_ProductoPlaceholder.png` | 38920 bytes |
| `Computadora_01_BarraSuperior.png` | 23561 bytes |

La prueba valida existencia, tamano minimo, dimensiones y que la imagen no sea plana.

## 26.11 Arbol

Validado visualmente por capturas:

- Vista general del Arbol.
- Detalle de nodo de producto.
- Nodo bloqueado.
- Panel de logros.

La UI usada es `EntrepreneurTreeUI`, no una replica.

## 26.12 Empleados

Validado visualmente por capturas:

- Grid de empleados.
- Detalle de empleado.

La UI usada es `UIEmployeesPanel` con `EmployeeManager` real.

## 26.13 Productos

Validado visualmente por capturas:

- Catalogo completo documentado.
- Productos placeholder documentados.

Las tarjetas usan `UIShopItemProduct.Initialize` y productos reales registrados por `ItemDatabase`/`DocumentedProductCatalog`.

## 26.14 Precios

Validado visualmente por captura:

- Vista de precios desde `UIManagementPanel` en seccion `Prices`.

El flujo usa `ItemDatabase`, `ProductPricingCalculator` y `StoreDatabase` reales.

## 26.15 Computadora

Validado visualmente por captura:

- Barra superior con botones `PRODUCTS`, `EQUIPMENT`, `ARBOL`, `UPGRADES`, `BOOSTERS`, `CUSTOMIZATION`, `EMPLEADOS`, `GESTION`.

Se reutiliza `UIShopDesktop.OptimizeNavigationLayout`.

## 26.16 Regresion automatizada cubierta

La suite PlayMode final cubre 51 pruebas:

- Precios y probabilidad de compra.
- Guardado/perfiles.
- Expansion de tienda y riesgo.
- Arbol y logros.
- Productos documentados.
- Empleados y roles.
- UI de computadora y gestion.
- Capturas visuales Fase 11.

## 26.17 Riesgos residuales

- Las capturas son automatizadas, no una sesion humana de gameplay libre.
- La evidencia visual requiere dispositivo grafico real; en `NullGfx` se omite con mensaje claro.
- No se hizo redisenio artistico manual, solo estabilizacion y validacion de UI existente.

## 26.18 Exclusiones cumplidas

- No se reintrodujo la carpeta descartada externa.
- No se borraron reportes anteriores.
- No se stagearon TMP preexistentes.
- No se crearon sistemas paralelos.
- No se regeneraron escenas ni prefabs de gameplay.

## 26.19 Veredicto

Release Candidate visual: VERDE.

La fase queda con compilacion limpia, PlayMode verde 51/51, capturas reales generadas y evidencia reproducible en `Reportes/`.
