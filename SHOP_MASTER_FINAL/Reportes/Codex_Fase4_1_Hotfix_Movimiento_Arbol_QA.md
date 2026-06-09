<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 4.1 - Hotfix Movimiento y Auditoria del Arbol

## Rama

`codex/fase4-1-hotfix-movimiento-arbol-qa`

## Base

- Base: `origin/codex/fase4-seguridad-ladrones-mejoras-arbol-reportes`
- Commit base esperado: `54c81b8`
- Commit base verificado en historial: `54c81b8 Codex Fase 4: seguridad ladrones y mejoras del arbol`

## Problema reportado

- El jugador no se puede mover despues de Fase 4.
- Unity local reporto `NullReferenceException` en `PlayerController.Awake`.
- Archivo: `Assets/StoreSimulator/Scripts/PlayerController.cs`.
- Linea reportada por Unity local: 115.

## Causa raiz

La linea 115 ejecutaba `playerInput.actions.FindActionMap("UI").Disable();` bajo Unity 6000.

La escena usa el asset real `Assets/StoreSimulator/Settings/InputActions.inputactions`, que contiene el action map `Default` con acciones `Move`, `View`, `Jump`, `LeftClick`, `RightClick`, `Enter`, `Cancel` y `Action`, pero no contiene un action map llamado `UI`.

Por eso `FindActionMap("UI")` devolvia `null` y el `.Disable()` lanzaba `NullReferenceException`. El error cortaba `Awake` antes de ejecutar `playerInput.onActionTriggered += OnAction`, dejando el movimiento y mouse look sin recibir input.

No fue introducido por Fase 4: `git diff 5ca6c1a..54c81b8 -- PlayerController.cs` no mostro cambios. El fallo depende de configuracion runtime/asset de Input System, por eso `dotnet build` no lo detecta.

## Correccion aplicada

- `PlayerController` ahora trata el action map `UI` como opcional.
- Si existe, lo deshabilita como antes.
- Si no existe, registra un `Debug.LogWarning` y continua con el input de gameplay.
- La suscripcion a `OnAction` ya no queda bloqueada por el action map faltante.
- Se agregaron defensas para `cameraTransform`, `StoreDatabase.Instance.storeEntry` y `Mouse.current` sin desactivar movimiento.
- Se agrego `OnDestroy` para desuscribir `PlayerInput` limpiamente.

La solucion no usa `try/catch`, no desactiva `PlayerController`, no crea controladores alternos y no toca escenas/prefabs YAML.

## Auditoria del Arbol

### IDs y nodos revisados

- Productos: `productos_basicos_1`, `productos_basicos_2`, `productos_basicos_3`, `especias_1`, `lacteos_1`, `lacteos_2`, `productos_frescos_1`, `productos_frescos_2`, `productos_higiene`, `sodas`, `proteina_1`, `productos_lujo_1`, `electrodomesticos_1`.
- Empleados: `empleado_1` a `empleado_18`.
- Seguridad: `seguridad_1`, `seguridad_2`, `seguridad_3`.
- Mejoras: `mejora_cafeina`, `mejora_carismatico`.

### Cambios de auditoria

- Se agrego el nodo faltante `lacteos_2` con prerequisito `lacteos_1`.
- No se encontraron IDs duplicados en los nodos reales del Arbol.
- No se encontraron prerequisitos apuntando a IDs inexistentes.
- `productos_basicos_1` sigue como nodo inicial por `DefaultUnlockedNodeId`.
- Productos Basicos 1 sigue mapeando los productos reales:
  - Product_A / id `0`: Leche
  - Product_B / id `1`: Sal
  - Product_C / id `2`: Agua
  - Product_D / id `3`: Pasta
  - Product_E / id `4`: Azucar
- Los cinco productos anteriores siguen con `requiredLevel: 0` y sin `requiredLicense`.

### Dependencias confirmadas

- Empleado 1 -> Especias 1
- Empleado 2 -> Productos de Higiene
- Empleado 3 -> Sodas
- Empleado 4 -> Lacteos 1
- Empleado 5 -> Lacteos 1
- Empleado 6 -> Especias 1
- Empleado 7 -> Empleado 5
- Empleado 8 -> Sodas
- Empleado 9 -> Productos de Higiene
- Empleado 10 -> Empleado 1
- Empleado 11 -> Nivel de Seguridad 1
- Empleado 12 -> Empleado 13
- Empleado 13 -> Productos de Lujo 1
- Empleado 14 -> Electrodomesticos 1
- Empleado 15 -> Nivel de Seguridad 2
- Empleado 16 -> Proteina 1
- Empleado 17 -> Productos Frescos 2
- Empleado 18 -> Nivel de Seguridad 3
- Seguridad 1 -> Empleado 7
- Seguridad 2 -> Empleado 8
- Seguridad 3 -> Empleado 14
- Cafeina -> Productos Frescos 2
- Carismatico -> Empleado 15

### Fuente de verdad

- Productos: `EntrepreneurProgress.IsProductUnlocked` y `EntrepreneurTreeDefinitions.GetKnownProductNodeId`.
- Empleados: `EmployeeManager` consulta `EntrepreneurProgress.IsUnlocked`.
- Seguridad: `SecurityManager` deriva `EntrepreneurProgress.SecurityLevel`.
- Cafeina: `EmployeeWorkSpeedMultiplier` deriva de `mejora_cafeina`.
- Carismatico: `CashierRevenueMultiplier` deriva de `mejora_carismatico`.

No se agrego progreso paralelo.

### Mensajes y puntos

- Prerequisitos faltantes ahora usan: `Falta desbloquear: {nombreDelNodo}`.
- Puntos insuficientes ahora usa: `No tienes puntos de progreso suficientes.`
- Se agrego `EntrepreneurProgress.onNodeUnlocked(nodeId)` como hook limpio para sistemas que requieran reaccionar al nodo exacto.
- `onProgressChanged` se conserva para UIs ya existentes.
- `progressPoints` no puede quedar negativo.

### Seguridad

Se corrigieron las probabilidades de arresto automatico en `SecurityManager`:

- Seguridad 1: 33%
- Seguridad 2: 66%
- Seguridad 3: 99%

## Requerimientos trabajados primero

| Requerimiento | Estado | Evidencia | Archivo relacionado |
| --- | --- | --- | --- |
| RQF3 | Cubierto | Arbol sigue integrado desde laptop/computadora por bootstrap en `UIShopDesktop`. | `UIShopDesktop.cs`, `EntrepreneurTreeUIBootstrap.cs` |
| RQF4 | Cubierto | `TryUnlock` valida prerequisitos y muestra requisito faltante especifico. | `EntrepreneurProgress.cs` |
| RQF8 | Cubierto | Empleados 1 a 18 existen y dependen del Arbol. | `EntrepreneurTreeNodeDefinition.cs`, `EmployeeManager.cs` |
| RQF11 | Cubierto | Seguridad 1 a 3 existen como nodos del Arbol. | `EntrepreneurTreeNodeDefinition.cs` |
| RQF12 | Cubierto | Seguridad requiere Empleado 7, 8 y 14 respectivamente. | `EntrepreneurTreeNodeDefinition.cs` |
| RQF18 | Cubierto | `SecurityManager` usa nivel derivado del Arbol y arrestos 33/66/99. | `SecurityManager.cs`, `EntrepreneurProgress.cs` |
| RQF21 | Cubierto | Guardado mantiene Arbol, empleados y seguridad derivada; saves viejos usan JSON vacio sin crashear. | `SaveGameSystem.cs`, `EntrepreneurProgress.cs` |
| RQF28 mejoras operativas | Cubierto | Cafeina y Carismatico existen, no aplican si no estan desbloqueadas. | `EntrepreneurProgress.cs`, `EmployeeRuntimeAgent.cs`, `CashDesk.cs` |
| RQF36 | Cubierto | Ningun nodo se desbloquea si falta prerequisito o puntos. | `EntrepreneurProgress.cs` |
| RQNF3 | Cubierto | Guardado conserva progreso mediante sistemas existentes, sin guardado paralelo. | `SaveGameSystem.cs` |
| RQNF7 | Cubierto | Validacion especifica para puntos/prerequisitos. | `EntrepreneurProgress.cs` |
| RQNF8 | Cubierto | Estados Bloqueado/Disponible/Desbloqueado calculados centralmente. | `EntrepreneurProgress.cs`, `EntrepreneurTreeUI.cs` |
| RQNF9 | Cubierto | Productos, empleados, seguridad y mejoras consultan el Arbol antes de activarse. | `ItemDatabase.cs`, `EmployeeManager.cs`, `SecurityManager.cs`, `CashDesk.cs` |
| RQNF16 | Cubierto | Mensajes claros en Arbol, productos y empleados. | `EntrepreneurProgress.cs`, `UIShopItemProduct.cs`, `EmployeeManager.cs` |
| RQNF17 | Cubierto | Notificaciones especificas para prerequisitos y puntos. | `EntrepreneurProgress.cs`, `EntrepreneurTreeUI.cs` |
| RQNF18 | Cubierto | UI del Arbol sigue dentro de la laptop y estructura existente. | `UIShopDesktop.cs`, `EntrepreneurTreeUIBootstrap.cs` |

## Actividades tipo Gantt

| Actividad | Requerimientos que abona | Responsable | Fecha | Duracion estimada | Estado |
| --- | --- | --- | --- | --- | --- |
| Crear rama hotfix desde Fase 4 | Control de version | Codex | 2026-06-08 | 0.25 h | Completado |
| Revisar documentos y reportes previos | RQF3, RQF4, RQF8, RQF21 | Codex | 2026-06-08 | 1.0 h | Completado |
| Diagnosticar PlayerController Awake | Hotfix movimiento | Codex | 2026-06-08 | 0.75 h | Completado |
| Corregir null del Input Action Map UI | Hotfix movimiento | Codex | 2026-06-08 | 0.5 h | Completado |
| Auditar IDs y prerequisitos del Arbol | RQF4, RQF8, RQF36, RQNF8 | Codex | 2026-06-08 | 1.25 h | Completado |
| Agregar `lacteos_2` y reforzar mensajes | RQF4, RQF36, RQNF16, RQNF17 | Codex | 2026-06-08 | 0.75 h | Completado |
| Ajustar seguridad 33/66/99 | RQF11, RQF12, RQF18 | Codex | 2026-06-08 | 0.25 h | Completado |
| Compilacion y validaciones tecnicas | Validacion | Codex | 2026-06-08 | 1.0 h | Completado |
| Prueba Play Mode movimiento/laptop | Hotfix movimiento, RQF3 | Isaac | 2026-06-08 | 1.0 h | Pendiente |
| QA visual completa del Arbol | RQNF16, RQNF17, RQNF18 | Isaac | 2026-06-08 | 1.5 h | Pendiente |

## Archivos modificados

- `Assets/StoreSimulator/Scripts/PlayerController.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurProgress.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeNodeDefinition.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeUI.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeUIBootstrap.cs`
- `Assets/StoreSimulator/Scripts/Security/SecurityManager.cs`

## Archivos nuevos

- `Reportes/Codex_Fase4_1_Hotfix_Movimiento_Arbol_QA.md`

## Validaciones realizadas

- `git status --short --branch -- SHOP_MASTER_FINAL`: cambios limitados a scripts de Fase 4.1 y este reporte.
- `git log --oneline -8`: base contiene `54c81b8`.
- `git branch --show-current`: `codex/fase4-1-hotfix-movimiento-arbol-qa`.
- `git diff 5ca6c1a..54c81b8 -- PlayerController.cs`: sin cambios, por lo que Fase 4 no introdujo el error en ese archivo.
- Auditoria automatica de nodos: 35 nodos detectados por regex literal, sin duplicados y sin prerequisitos faltantes; `productos_basicos_1` existe por constante `DefaultUnlockedNodeId`.
- `dotnet build .\SHOP_MASTER_FINAL.sln`: correcto, 0 advertencias, 0 errores.
- Unity batchmode:
  - Comando intentado con Unity `6000.0.37f1` y log `Reportes/Codex_Fase4_1_UnityBatchmode.log`.
  - Resultado: no genero log batchmode ni codigo util; dejo proceso Unity vivo (`Id 27816`), que fue cerrado.
  - `Editor.log` pudo leerse despues con permisos elevados y contenia el `NullReferenceException` viejo reportado en `PlayerController.cs:115`; no hubo Play Mode post-fix certificado desde Codex.

## Play Mode manual

Pendiente de probar por Isaac en Unity local.

Pasos exactos:

1. Abrir Unity con `C:\Users\ijuan\SHOP_MASTER_FINAL`.
2. Abrir la escena principal del Store Simulator.
3. Presionar Play.
4. Verificar que no aparezca `NullReferenceException` en `PlayerController.Awake`.
5. Moverse con WASD.
6. Mover camara/mouse look.
7. Interactuar con laptop/computadora.
8. Abrir y cerrar el Arbol del Emprendedor.
9. Cerrar laptop y verificar que el jugador vuelve a moverse.
10. Interactuar con caja/objetos basicos si estan disponibles en escena.

## Inscripcion POMPIC

- Se conserva/agrego `//Adaptado por POMPIC 20100333` en los scripts modificados.
- Este reporte inicia con `<!-- Adaptado por POMPIC 20100333 -->`.
- No se modificaron `.meta`, prefabs ni escenas YAML, por lo que no se agrego inscripcion en archivos sensibles de Unity.

## Riesgos o pendientes

- Play Mode manual queda pendiente para confirmar movimiento real, mouse look y laptop sin congelamiento.
- Unity batchmode no quedo verde desde este entorno porque no genero log y abrio un proceso Unity vivo.
- Falta QA visual del Arbol en editor para confirmar layout/scroll/textos en resolucion real.
- El reporte diario UI puede requerir QA visual por lineas adicionales agregadas en Fase 4.
- No se tocaron escenas ni prefabs; cualquier referencia serializada rota debe validarse en Unity local.

## Confirmacion Animo

No se modifico, no se recreo y no se reintrodujo `Animo/`. El alcance se mantuvo en `SHOP_MASTER_FINAL`.
