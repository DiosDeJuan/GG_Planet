<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 4.2 - Computadora: Licenses unido al Arbol del Emprendedor

## Rama

`codex/fase4-2-computadora-licenses-arbol-ui`

## Base

Base solicitada: `origin/codex/fase4-1-hotfix-movimiento-arbol-qa`

Commit base esperado y confirmado: `94bde5e`

## Problema reportado

- La computadora abre, pero el Arbol del Emprendedor no aparecia donde debia.
- La pestana original `LICENSES` seguia visible al jugador.
- El usuario pidio reutilizar esa pestana como entrada oficial al Arbol y renombrarla.
- La pestana `Empleados` aparecia flotando/desalineada.
- Habia texto vertical/cortado por UI inyectada fuera del panel correcto.
- En esta fase se usa "computadora" en reporte y textos nuevos; nombres heredados como `UIShopDesktop` se mantienen.

## Documentos revisados

- `Documentos/NEW_Requerimientos.docx`: RQF3, RQF4, RQF8, RQF11/RQF12/RQF18, RQF21, RQF28, RQF36 y no funcionales de UI clara.
- `Documentos/propuestas juanito (2).docx`: computadora/oficina como centro de administracion, apps de compra, Arbol, empleados y progresion por puntos.
- `Documentos/Protocolo prpuesta Juanito (1).pdf`: interfaz clara, Arbol como progresion oficial, compra, inventario, seguridad y guardado.
- `Documentos/Guia Gantt (1).pdf`: desglose de actividades cortas para reporte.

## Reportes previos revisados

- `Reportes/Fase1_Arbol_Emprendedor_UI_Computadora.md`
- `Reportes/Codex_Fase2_Productos_Compra_Delivery_Inventario.md`
- `Reportes/Codex_Fase3_Empleados_Arbol_Laptop_NPCs.md` (nombre heredado del reporte anterior)
- `Reportes/Codex_Fase4_Seguridad_Ladrones_Mejoras_Arbol_Reportes.md`
- `Reportes/Codex_Fase4_1_Hotfix_Movimiento_Arbol_QA.md`

## Sistemas de UI encontrados

- `UIShopDesktop` es la computadora del asset y llama los bootstraps del Arbol y empleados durante `Start`.
- La navegacion superior vive en `Navigation/Categories`, que tiene un `HorizontalLayoutGroup` con botones del asset.
- Los paneles reales viven bajo `ContentArea`: `Products`, `Storage`, `Licenses`, `Upgrades`, `Boosters`, `Customization`.
- `ContentArea` tiene `UIShopCategoryHelper`, que desactiva paneles hermanos y activa el panel seleccionado.
- `LICENSES` era un `UIShopCategory` para `LicenseScriptableObject`, con panel propio y start hidden.
- `EntrepreneurProgress` ya es la fuente de verdad del proyecto para puntos, nodos, empleados, seguridad y mejoras.
- `ItemDatabase` y `UIShopItemProduct` ya filtran productos por `EntrepreneurProgress`; el sistema de licencias del asset queda como auxiliar si un producto heredado usa `requiredLicense`.

## Causa raiz

- `EntrepreneurTreeUIBootstrap` buscaba el primer objeto llamado `Categories`; ese nombre existe en la fila de tabs, no en el area principal.
- Por esa busqueda, el panel del Arbol se creaba dentro de la navegacion, fuera de `ContentArea`.
- El mismo bootstrap clonaba un boton nuevo bajo la raiz de `Navigation`, no bajo `Navigation/Categories`, provocando un boton fuera del layout.
- `UIEmployeesUIBootstrap` repetia el mismo patron: panel y boton generados en padres incorrectos, lo que explicaba la pestana `Empleados` flotante.
- El texto vertical/cortado era consecuencia de esos objetos UI dentro de contenedores con anchors/layout no pensados para paneles grandes.
- No se detecto un canvas nuevo necesario ni un sistema paralelo de desbloqueo; el problema era de inyeccion de UI y padres equivocados.

## Correccion aplicada

- `EntrepreneurTreeUIBootstrap` ahora busca `ContentArea` y reutiliza su hijo directo `Licenses`.
- El panel `Licenses` se convierte en host runtime del Arbol: se deshabilita su `UIShopCategory`, se oculta su contenido viejo de licencias y se agrega `Entrepreneur Tree Content`.
- El boton existente `Button - Licenses` se conserva, pero su texto visible se cambia a `ARBOL`, con no-wrap y autosizing.
- No se crea el viejo boton `Button - Entrepreneur Tree`; si aparece en runtime, se elimina.
- El Arbol se renderiza dentro del panel principal de la computadora, con header, puntos, columnas de Productos/Empleados/Seguridad/Mejoras, scroll y detalle de nodo.
- `EntrepreneurTreeUI` fue ajustado a fondo claro, tarjetas blancas/grises y boton rosa del asset para evitar una UI ajena.
- `UIEmployeesUIBootstrap` ahora crea el boton `EMPLEADOS` dentro de `Navigation/Categories` y su panel bajo `ContentArea`, sin flotar sobre la computadora.
- No se tocaron prefabs ni escenas YAML para evitar romper GUIDs o serializacion de Unity.

## Requerimientos trabajados

| Requerimiento | Estado | Evidencia | Archivo relacionado |
|---|---|---|---|
| RQF3 | Cubierto por codigo | `LICENSES` se reutiliza como entrada visible `ARBOL` y abre el Arbol en `ContentArea/Licenses`. | `EntrepreneurTreeUIBootstrap.cs`, `EntrepreneurTreeUI.cs` |
| RQF4 | Cubierto por codigo existente | `EntrepreneurProgress.TryUnlock` mantiene mensajes de prerequisitos y puntos. | `EntrepreneurProgress.cs`, `EntrepreneurTreeUI.cs` |
| RQF8 | Cubierto por codigo existente | Empleados 1-18 siguen definidos en el Arbol y el panel de empleados queda integrado en tabs reales. | `EntrepreneurTreeNodeDefinition.cs`, `UIEmployeesUIBootstrap.cs` |
| RQF11 | Cubierto por codigo existente | Seguridad nivel 1 sigue en el Arbol con prerequisito Empleado 7. | `EntrepreneurTreeNodeDefinition.cs` |
| RQF12 | Cubierto por codigo existente | Seguridad nivel 2 sigue en el Arbol con prerequisito Empleado 8. | `EntrepreneurTreeNodeDefinition.cs` |
| RQF18 | Cubierto por codigo existente | Seguridad nivel 3 sigue en el Arbol con prerequisito Empleado 14. | `EntrepreneurTreeNodeDefinition.cs` |
| RQF21 | Cubierto por codigo existente | `SaveGameSystem` guarda/carga `EntrepreneurProgress`, `EmployeeManager` y `SecurityManager`. | `SaveGameSystem.cs` |
| RQF28 mejoras operativas | Cubierto por codigo existente | Cafeina y Carismatico siguen como nodos y multiplicadores en `EntrepreneurProgress`. | `EntrepreneurProgress.cs`, `EntrepreneurTreeNodeDefinition.cs` |
| RQF36 | Cubierto por codigo existente | `Normalize` y `TryUnlock` impiden estados sin prerequisitos validos. | `EntrepreneurProgress.cs` |
| RQNF8 | Cubierto por codigo | El Arbol muestra Bloqueado, Disponible y Desbloqueado. | `EntrepreneurTreeUI.cs` |
| RQNF9 | Cubierto por codigo existente | Productos, empleados, seguridad y mejoras consultan `EntrepreneurProgress`. | `ItemDatabase.cs`, `EmployeeManager.cs`, `SecurityManager.cs` |
| RQNF16 | Parcial | Layout corregido por codigo; falta QA visual interactiva en Unity. | `EntrepreneurTreeUI.cs` |
| RQNF17 | Parcial | Se reutiliza tab/panel del asset y color rosa; falta confirmacion visual local. | `EntrepreneurTreeUIBootstrap.cs`, `UIEmployeesUIBootstrap.cs` |
| RQNF18 | Parcial | Arbol queda dentro de computadora por codigo; Play Mode manual queda pendiente. | `UIShopDesktop.cs`, bootstraps |

## Actividades tipo Gantt

| Actividad | Requerimientos que abona | Responsable | Fecha | Duracion estimada | Estado |
|---|---|---|---|---|---|
| Revisar documentos y reportes base | RQF3, RQF4, RQF8, RQF21 | Codex | 2026-06-08 | 1.0 h | Completado |
| Auditar prefab/script de computadora | RQNF16, RQNF17, RQNF18 | Codex | 2026-06-08 | 1.5 h | Completado |
| Reutilizar `Licenses` como host del Arbol | RQF3, RQNF18 | Codex | 2026-06-08 | 1.5 h | Completado |
| Corregir bootstrap de empleados sin flotante | RQF8, RQNF16 | Codex | 2026-06-08 | 1.0 h | Completado |
| Ajustar estilos/layout del Arbol | RQNF16, RQNF17 | Codex | 2026-06-08 | 1.0 h | Completado |
| QA Play Mode visual | RQNF16, RQNF17, RQNF18 | Isaac | 2026-06-08 | 1.0 h | Pendiente |

## Archivos modificados

- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeUIBootstrap.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeUI.cs`
- `Assets/StoreSimulator/Scripts/Employees/UIEmployeesUIBootstrap.cs`

## Archivos nuevos

- `Reportes/Codex_Fase4_2_Computadora_Licenses_Arbol_UI.md`

## Validaciones realizadas

- `git status --untracked-files=no`: ejecutado sobre el root Git real `C:\Users\ijuan`; mostro solo cambios dentro de `SHOP_MASTER_FINAL` antes del reporte. El `git status` completo tambien muestra carpetas personales no rastreadas del root padre; no se stagearon.
- `git diff --check`: correcto, sin errores de whitespace; Git aviso que algunos `.cs` cambiaran LF a CRLF cuando Git los toque.
- `dotnet build .\SHOP_MASTER_FINAL.sln`: correcto, 0 advertencias, 0 errores.
- Unity batchmode: se intento con `C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe -batchmode -projectPath C:\Users\ijuan\SHOP_MASTER_FINAL -quit -logFile C:\Users\ijuan\SHOP_MASTER_FINAL\Reportes\Codex_Fase4_2_UnityBatchmode.log`. El comando devolvio codigo 0, pero no genero el log solicitado. Habia una instancia de Unity abierta (`Unity`, PID 23612), por lo que no se certifica batchmode en verde.
- `Editor.log`: se pudo leer con permiso elevado; mostraba carga de proyecto, recarga de assemblies y escena `Game.unity` sin errores C# reportados en ese momento, pero no sustituye Play Mode manual ni batchmode final certificado.
- Busqueda funcional de `Animo` en `Assets`, `Packages`, `ProjectSettings` y `Documentos`: sin resultados.

## Checklist visual de computadora

| Punto | Estado |
|---|---|
| `LICENSES` renombrado | Cubierto por codigo: boton visible cambia a `ARBOL`. |
| Arbol visible dentro de computadora | Cubierto por codigo: host `ContentArea/Licenses`; pendiente QA visual. |
| Sin `Empleados` flotante | Cubierto por codigo: boton ahora se instancia en `Navigation/Categories`; pendiente QA visual. |
| Sin texto vertical | Cubierto por correccion de padres/layout; pendiente QA visual. |
| Cerrar computadora devuelve control | Sin cambios en `UIShopDesktop.Exit`; pendiente Play Mode manual. |
| Productos Basicos 1 siguen desbloqueados | Confirmado por `EntrepreneurTreeDefinitions.DefaultUnlockedNodeId`. |
| Nodo bloqueado muestra requisito | Confirmado por `EntrepreneurProgress.GetStateDescription` y `TryUnlock`. |
| Nodo sin puntos muestra mensaje | Confirmado por `EntrepreneurProgress.TryUnlock`. |
| Nodo desbloqueado actualiza UI | Confirmado por `onProgressChanged` y `EntrepreneurTreeUI.Refresh`. |

## Play Mode manual

Pendiente de probar por Isaac en Unity local. No se ejecuto prueba interactiva real de movimiento, apertura/cierre de computadora ni click visual en tabs.

## Inscripcion POMPIC

- Los tres scripts modificados ya inician con `//Adaptado por POMPIC 20100333`.
- Este reporte inicia con `<!-- Adaptado por POMPIC 20100333 -->`.
- No se modificaron `.meta`, prefabs, escenas ni archivos YAML, por lo que no hubo excepciones de inscripcion en assets delicados.

## Riesgos o pendientes

- Falta QA visual en Play Mode para confirmar que `ARBOL` y `EMPLEADOS` caben en la fila superior a la resolucion real de Isaac.
- Unity batchmode no quedo certificado porque el comando no genero log con una instancia de Unity abierta.
- El sistema heredado `LicenseScriptableObject` se conserva como auxiliar para productos con `requiredLicense`; la UI visible de licencias fue redirigida al Arbol.
- Si en una fase futura se agregan mas tabs a la computadora, conviene convertir la navegacion superior en un layout con scroll o segunda fila formal.

## Confirmacion Animo

No se modifico, no se recreo y no se reintrodujo `Animo/`. La busqueda funcional dentro de `SHOP_MASTER_FINAL` no encontro referencias en `Assets`, `Packages`, `ProjectSettings` ni `Documentos`. El root Git padre muestra un `Animo/` no rastreado preexistente; no se uso, no se agrego al indice y no forma parte de este commit.
