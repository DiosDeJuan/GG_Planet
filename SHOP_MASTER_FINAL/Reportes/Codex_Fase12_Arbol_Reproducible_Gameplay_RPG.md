<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 12 - Arbol reproducible en gameplay, diseno RPG operativo y base de ramas para productos

## Encabezado
- Fase: Codex Fase 12 - Arbol reproducible en gameplay, diseno RPG operativo y base de ramas para productos.
- Rama base: `origin/codex/fase11-qa-visual-capturas-arbol-productos-empleados`.
- Commit base esperado: `0eb87594caa36b2ad0f3cf34dfce2e175af8ad21`.
- Rama nueva: `codex/fase12-arbol-reproducible-gameplay-rpg`.
- Responsable: Codex.
- Proyecto: `SHOP_MASTER_FINAL`.
- Repositorio: `DiosDeJuan/GG_Planet`.
- Fecha: 2026-06-10.

## Documentos Revisados
- `NEW_Requerimientos.docx`: RQF3, RQF4, RQF8, RQF10-RQF12, RQF21, RQF36; RQNF8, RQNF9, RQNF16, RQNF17, RQNF18.
- `propuestas juanito (2).docx`: laptop/computadora, Arbol del Emprendedor estilo RPG, productos, empleados, seguridad, mejoras y puntos por logros.
- `Protocolo prpuesta Juanito (1).pdf`: progreso por logros/puntos, arbol, laptop y validacion de gameplay.
- `Guia Gantt (1).pdf`: actividades con dependencias y duraciones acotadas.
- Reportes Fase 11, 10, 9, 8, 4.2, 4.3 y 4.4: continuidad visual, bootstraps existentes, persistencia y regresion heredada.

## Problema Heredado
Fase 11 dejo capturas automatizadas y 51 tests verdes, pero el riesgo seguia siendo que el Arbol funcionara mejor como fixture que como experiencia reproducible dentro de la computadora real. Esta fase cerro esa brecha sin crear otro arbol, progreso, logros, economia ni Canvas paralelo.

## Ruta Reproducible
- Escena principal: `Assets/StoreSimulator/Scenes/Game.unity`, tomada desde `ProjectSettings/EditorBuildSettings.asset`.
- Objeto real: computadora `Computer` en `Game.unity`, con `UIShopDesktop`.
- App real: `ARBOL` reutiliza el panel `Licenses` dentro de `ContentArea`.
- Metodo QA agregado: `UIShopDesktop.OpenArbolForQA(bool showQaOverlay = true)` y `OpenAppForQA("ARBOL")`.
- QA overlay: dentro de `EntrepreneurTreeUI`, oculto por defecto, activable con `F8` o por metodo QA en Editor/Development/Test.
- Guia Isaac: `Reportes/Codex_Fase12_Guia_Prueba_Arbol.md`.

## Cambios Del Arbol
| Area | Problema | Cambio | Archivo | Riesgo | Validacion |
|---|---|---|---|---|---|
| Desktop real | No habia API directa para abrir ARBOL en QA sin colisiones/input | `OpenArbolForQA` y `OpenAppForQA` llaman bootstraps reales y `UIShopCategoryHelper` | `Assets/StoreSimulator/Scripts/UIShopDesktop.cs` | Bajo | Tests `TreeRuntime_*` |
| Desktop robusto | Prefab real aislado podia fallar sin singletons | Guards en labels, collider y UIGame | `UIShopDesktop.cs` | Bajo | dotnet y PlayMode |
| Puntos QA | Metodo interno solo era comodo en Editor | `AddPointsForInternalTesting` queda limitado a Editor/Development/Test | `EntrepreneurProgress.cs` | Bajo | `TreeRuntime_QAOverlayAddsPoints...` |
| QA overlay | Isaac necesitaba probar puntos/desbloqueos rapido | Overlay con +1, +5, prereqs, reset, guardar/cargar QA | `EntrepreneurTreeUI.cs` | Medio | Captura `Arbol_08` |
| Scroll/nodos | Primer encuadre podia mostrar panel vacio | Inicio del ScrollRect arriba/izquierda y layout base visible | `EntrepreneurTreeUI.cs`, `EntrepreneurTreeVisualLayout.cs` | Bajo | Capturas Fase 12 |
| Capturas | Evidencia anterior era fixture | Captura con prefab real `UIShopDesktop` y ruta `OpenArbolForQA` | `StoreSimulatorPlayModeRegressionTests.cs` | Bajo | 10 PNG Fase 12 |

## Matriz De Ramas
| Rama | Nodos | Fuente documental | Estado visual | Estado funcional | Pendiente |
|---|---:|---|---|---|---|
| Productos | 14 | RQF3/RQF4, propuestas Juanito | Visible con nodos y detalle | 47 referencias listas | Tabla final de productos siguiente fase |
| Empleados | 18 | RQF8, propuestas Juanito | Visible por nodos E | Prerequisitos validados | Balance de contratacion |
| Seguridad | 3 | RQF10-RQF12 | Visible por nodos S | 33/66/99 expuesto | Prueba humana de arresto |
| Mejoras | 2 | propuestas Juanito | Visible por nodos M | Cafeina 10%, Carismatico 5% | Balance final |
| Logros/puntos | 43 logros | RQF21, protocolo | Panel de logros y contador | Puntos guardan/cargan por `EntrepreneurProgress` | Balance de recompensas |

## Matriz De Nodos Criticos
| Nodo | Tipo | Prerequisito | Costo | Estado inicial | Mensaje bloqueo | Efecto | Persistencia | Resultado |
|---|---|---|---:|---|---|---|---|---|
| Productos Basicos 1 | Producto | Ninguno | 0 | Desbloqueado | Ya desbloqueado | 5 productos base | Si | OK |
| Productos Basicos 2 | Producto | Basicos 1 | 1 | Disponible sin puntos | Puntos insuficientes | 5 productos | Si | OK |
| Lacteos 1 | Producto | Basicos 3 | 1 | Bloqueado | Falta desbloquear | Lacteos | Si | OK |
| Especias 1 | Producto | Basicos 3 | 1 | Bloqueado | Falta desbloquear | Especias | Si | OK |
| Productos Frescos 2 | Producto | Frescos 1 | 1 | Bloqueado | Falta desbloquear | Frescos 2 | Si | OK |
| Productos de Higiene | Producto | Especias 1 | 1 | Bloqueado | Falta desbloquear | Higiene | Si | OK |
| Sodas | Producto | Higiene | 1 | Bloqueado | Falta desbloquear | Sodas | Si | OK |
| Productos de Lujo 1 | Producto | Sodas | 1 | Bloqueado | Falta desbloquear | Lujo | Si | OK |
| Electrodomesticos 1 | Producto | Lujo 1 | 1 | Bloqueado | Falta desbloquear | Electrodomesticos | Si | OK |
| Empleado 1 | Empleado | Especias 1 | 1 | Bloqueado | Falta desbloquear | Contratable | Si | OK |
| Empleado 7 | Empleado | Empleado 5 | 1 | Bloqueado | Falta desbloquear | Contratable | Si | OK |
| Empleado 14 | Empleado | Electrodomesticos 1 | 1 | Bloqueado | Falta desbloquear | Contratable | Si | OK |
| Empleado 18 | Empleado | Seguridad 3 | 1 | Bloqueado | Falta desbloquear | Contratable | Si | OK |
| Seguridad Nivel 1 | Seguridad | Empleado 7 | 1 | Bloqueado | Falta desbloquear | 33% | Si | OK |
| Seguridad Nivel 2 | Seguridad | Empleado 8 | 1 | Bloqueado | Falta desbloquear | 66% | Si | OK |
| Seguridad Nivel 3 | Seguridad | Empleado 14 | 1 | Bloqueado | Falta desbloquear | 99% | Si | OK |
| Cafeina | Mejora | Frescos 2 | 1 | Bloqueado | Falta desbloquear | 10% velocidad | Si | OK |
| Carismatico | Mejora | Empleado 15 | 1 | Bloqueado | Falta desbloquear | 5% venta cajero | Si | OK |

## Productos Referenciados Por Rama
| Categoria | Productos referenciados | Cantidad | Nodo | Listo tabla | Pendiente |
|---|---|---:|---|---|---|
| Productos Basicos 1 | Leche, Sal, Agua, Pasta, Azucar | 5 | `productos_basicos_1` | Si | Assets finales ya base |
| Productos Basicos 2 | Harina, Arroz, Frijoles, Pan, Aceite | 5 | `productos_basicos_2` | Si | Tabla final |
| Productos Basicos 3 | Cafe, Huevo | 2 | `productos_basicos_3` | Si | Tabla final |
| Lacteos 1 | Cheddar, Yogurt natural, Mantequilla | 3 | `lacteos_1` | Si | Tabla final |
| Lacteos 2 | Queso americano, Queso crema | 2 | `lacteos_2` | Si | Tabla final |
| Lacteos 3 | Mozzarella, Parmesano | 2 | `lacteos_3` | Si | Tabla final |
| Especias 1 | Pimienta negra, Canela | 2 | `especias_1` | Si | Tabla final |
| Productos Frescos 1 | Manzana, Platano, Jitomate, Cebolla | 4 | `productos_frescos_1` | Si | Tabla final |
| Productos Frescos 2 | Uvas, Zanahorias, Ajo | 3 | `productos_frescos_2` | Si | Tabla final |
| Productos de Higiene | Jabon, Papel higienico, Detergente, Pasta de dientes | 4 | `productos_higiene` | Si | Tabla final |
| Proteina 1 | Res, Pollo, Cerdo, Pescado | 4 | `proteina_1` | Si | Tabla final |
| Sodas | Cola, Cola sin azucar, Refresco de limon | 3 | `sodas` | Si | Tabla final |
| Productos de Lujo 1 | Trufa, Chocolate importado, Caviar | 3 | `productos_lujo_1` | Si | Tabla final |
| Electrodomesticos 1 | Refrigerador, Microondas, Horno, Mesa, Licuadora | 5 | `electrodomesticos_1` | Si | Tabla final |
| Total | 47 productos | 47 | 14 nodos | Si | Datos finales fase siguiente |

## QA Overlay / Modo Prueba
| Archivo | Activacion | Funciones | Alcance | Proteccion | Impacto save | Riesgo | Validacion |
|---|---|---|---|---|---|---|---|
| `EntrepreneurTreeUI.cs` | `F8` o `OpenArbolForQA(true)` | +1, +5, prereqs, reset, guardar/cargar QA | Solo arbol | `UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_INCLUDE_TESTS` | Usa `SaveGameSystem` si existe | Bajo | `Arbol_08` y test QA |

## Capturas
| Captura | Ruta | Metodo | Origen | Valida | Resultado |
|---|---|---|---|---|---|
| Vista general | `Reportes/Capturas_Fase12/Arbol_01_GameplayRutaReal_VistaGeneral.png` | PlayMode | Prefab real desktop + QA real | Nodos y detalle | OK |
| Detalle producto | `Arbol_02_GameplayRutaReal_DetalleProducto.png` | PlayMode | Real desktop | Producto | OK |
| Nodo empleado | `Arbol_03_GameplayRutaReal_NodoEmpleado.png` | PlayMode | Real desktop | Empleado bloqueado | OK |
| Nodo seguridad | `Arbol_04_GameplayRutaReal_NodoSeguridad.png` | PlayMode | Real desktop | Seguridad | OK |
| Mejora | `Arbol_05_GameplayRutaReal_Mejora.png` | PlayMode | Real desktop | Mejora | OK |
| Bloqueo prerequisito | `Arbol_06_GameplayRutaReal_BloqueoPrerequisito.png` | PlayMode | Real desktop | Falta desbloquear | OK |
| Puntos insuficientes | `Arbol_07_GameplayRutaReal_PuntosInsuficientes.png` | PlayMode | Real desktop | Sin puntos | OK |
| QA overlay | `Arbol_08_GameplayRutaReal_QAOverlay.png` | PlayMode | Real desktop | QA activo | OK |
| Logros | `Arbol_09_GameplayRutaReal_Logros.png` | PlayMode | Real desktop | Panel logros | OK |
| Navegacion | `Computadora_01_GameplayRutaReal_Navegacion.png` | PlayMode | Real desktop | Boton ARBOL | OK |

## Bugs Encontrados
| ID | Modulo | Descripcion | Severidad | Causa raiz | Archivo | Estado |
|---|---|---|---|---|---|---|
| F12-B01 | Desktop | Prefab real podia NRE si faltaban singletons en batchmode | Media | Inicializacion asumida | `UIShopDesktop.cs` | Corregido |
| F12-B02 | Arbol | Ruta QA no existia sobre desktop real | Media | Falta API testeable | `UIShopDesktop.cs` | Corregido |
| F12-B03 | Capturas | Primeras capturas salian con ContentArea blanco | Media | `Start` reocultaba ARBOL y scroll iniciaba fuera | Tests/UI/Layout | Corregido |

## Bugs Corregidos
| ID | Correccion | Archivo | Riesgo | Validacion |
|---|---|---|---|---|
| F12-B01 | Guards de UI/singletons/collider | `UIShopDesktop.cs` | Bajo | dotnet, PlayMode |
| F12-B02 | `OpenArbolForQA` y `OpenAppForQA` | `UIShopDesktop.cs` | Bajo | Tests runtime |
| F12-B03 | Reabrir ARBOL antes de capturas, scroll inicial y layout visible | `StoreSimulatorPlayModeRegressionTests.cs`, `EntrepreneurTreeUI.cs`, `EntrepreneurTreeVisualLayout.cs` | Bajo | Capturas Fase 12 |

## Tests Agregados
| Test | Sistema | Que valida | Resultado | Evidencia |
|---|---|---|---|---|
| `TreeRuntime_MainSceneCanResolveComputerDesktop` | Escena real | `Game` carga y contiene `UIShopDesktop` | Passed | XML |
| `TreeRuntime_CanOpenArbolThroughDesktopRealFlow` | Desktop | ARBOL abre desde API real | Passed | XML |
| `TreeRuntime_ReopenDoesNotDuplicateNodesOrConnections` | UI | Reabrir no duplica nodos/conexiones | Passed | XML |
| `TreeRuntime_ProductBranchReferencesAll47Products` | Datos | 47 productos en 14 ramas | Passed | XML |
| `TreeRuntime_QAOverlayAddsPointsUnlocksAndStaysHiddenByDefault` | QA | Overlay oculto normal, suma puntos y desbloquea | Passed | XML |
| `TreeRuntime_CapturesRealDesktopArbolRoute` | Visual | 10 capturas con desktop real | Passed | XML/PNG |

## Actividades Tipo Gantt
| ID | Actividad | Requerimientos | Responsable | Fecha | Duracion | Resultado |
|---|---|---|---|---|---|---|
| 1 | Validacion rama/base | Base Fase 11 | Codex | 2026-06-10 | 0.5h | OK |
| 2 | Revision documental | RQF/RQNF | Codex | 2026-06-10 | 1.0h | OK |
| 3 | Auditoria gameplay-computadora-ARBOL | RQF3 | Codex | 2026-06-10 | 1.0h | OK |
| 4 | Apertura real del Arbol | RQF3/RQNF17 | Codex | 2026-06-10 | 1.0h | OK |
| 5 | Layout RPG dentro de computadora | RQNF16 | Codex | 2026-06-10 | 1.0h | OK |
| 6 | Ramas y nodos | RQF4/RQF8/RQF10 | Codex | 2026-06-10 | 1.0h | OK |
| 7 | Mensajes y estados | RQF36 | Codex | 2026-06-10 | 0.75h | OK |
| 8 | Modo QA | Validacion rapida | Codex | 2026-06-10 | 1.0h | OK |
| 9 | Persistencia | RQF21 | Codex | 2026-06-10 | 0.5h | OK |
| 10 | Capturas | Evidencia visual | Codex | 2026-06-10 | 1.0h | OK |
| 11 | Tests PlayMode | Regresion | Codex | 2026-06-10 | 1.0h | OK |
| 12 | Validacion tecnica | Build/Unity | Codex | 2026-06-10 | 0.75h | OK |
| 13 | Guia Isaac | QA manual | Codex | 2026-06-10 | 0.5h | OK |
| 14 | Reporte/cierre | Evidencia | Codex | 2026-06-10 | 1.0h | OK |

## Validaciones
- `dotnet build .\SHOP_MASTER_FINAL.sln`: correcto, 0 warnings, 0 errores.
- Unity PlayMode batchmode: `Reportes/Codex_Fase12_UnityPlayMode.log`, exit code 0.
- PlayMode tests: `57/57 Passed`, 0 failed, 0 skipped, `Reportes/Codex_Fase12_PlayModeResults.xml`.
- Renderer: Direct3D 11, NVIDIA GeForce RTX 3050 Ti Laptop GPU.
- Capturas: 10 PNG en `Reportes/Capturas_Fase12/`.
- Consola: sin `NullReferenceException`, sin `MissingReferenceException`, sin errores de compilacion.
- Warnings documentados: dos `CommandBuffer: temporary render texture` de URP durante captura; un warning opcional de `PlayerController` por action map UI ausente.
- `git diff --check -- SHOP_MASTER_FINAL`: limpio; solo warnings de line endings esperados en Windows.
- `Animo/`: no reintroducido.
- TMP Bangers/Roboto: presentes como modificados preexistentes, no deben stagearse.

## Estado Final
**VERDE tecnico con pendiente humano visual menor.** Build y PlayMode estan verdes, ARBOL abre desde ruta QA real conectada a `UIShopDesktop`, hay capturas con desktop real y guia Isaac. Pendiente real: validacion humana de gameplay fisico caminando hasta la computadora, porque la automatizacion usa `OpenArbolForQA` para evitar friccion de input/colisiones.

## Confirmaciones Obligatorias
- Animo/ no fue reintroducido.
- TMP Bangers/Roboto no fueron stageados.
- No se creo Arbol paralelo.
- No se creo progreso paralelo.
- No se creo sistema de logros paralelo.
- No se creo StoreDatabase paralelo.
- No se creo inventario paralelo.
- No se creo delivery paralelo.
- No se creo economia paralela.
- No se creo Canvas principal paralelo.
- No se trabajo menu principal.
- No se trabajo ajustes.
- No se trabajo build final.
- No se trabajo tabla definitiva de productos.
- La rama de productos del Arbol quedo preparada para los 47 productos documentados.
- El modo QA esta limitado a Editor/Development/Test.
- Los archivos modificados tienen inscripcion POMPIC cuando aplica.
- Excepciones de inscripcion: logs, XML de Unity Test Runner y PNG no llevan comentario porque son generados/binarios.

## Pendientes Reales
- Play Mode humano completo por Isaac caminando hasta la computadora real.
- Revision visual humana final del layout dentro del monitor.
- Tabla definitiva de productos en la siguiente fase.
- Ajuste fino de iconos finales de nodos.
- Balance final de progresion por logros y puntos.
- Prueba de clientes comprando categorias avanzadas cuando exista tabla final.
