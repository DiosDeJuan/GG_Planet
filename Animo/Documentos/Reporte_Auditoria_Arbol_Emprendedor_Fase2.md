# Reporte Auditoría Árbol del Emprendedor - Fase 2

## Resumen Ejecutivo

La Fase 2 partió del reporte anterior y mantuvo sus nueve correcciones. El baseline previo pasó nuevamente: Unity batchmode correcto y runner anterior con `Failures=0`.

Se añadieron pruebas Play Mode deterministas sobre `Assets/StoreSimulator/Scenes/Game.unity`. El nuevo runner invoca botones reales de computadora, desbloquea nodos desde `NodeUI`, compra desde la app `COMPRA`, contrata desde la app `EMPLEADOS`, procesa una venta automática con `CashDesk`, mueve un producto real de paquete a anaquel, ejecuta robo/captura/escape/arresto automático y escribe/carga datos del Árbol desde archivo temporal.

Durante la primera pasada se encontraron y corrigieron dos defectos reales:

1. `OrdersAppUIController` agregaba un segundo `Image` al panel `Compra`, provocando `NullReferenceException` al abrir la pestaña por primera vez.
2. El cajero automático llamaba `UICashDeskTerminal.Exit(true)` aunque el terminal nunca hubiera sido controlado por el jugador, intentando restaurar cámara/input de jugador durante checkout IA.

También se agregó una pestaña `AYUDA` dentro de la computadora y se eliminaron las tres advertencias de APIs obsoletas solicitadas.

Resultado final:

- `dotnet build`: `0` errores, `0` warnings.
- Unity batchmode: éxito.
- Runner Play Mode Fase 2: `Audit finished. Failures=0`.
- Consola funcional: sin `NullReferenceException`, `MissingReferenceException`, asserts ni errores críticos.

`git status` no pudo ejecutarse porque el entorno Git falla antes de inspeccionar el workspace con `fatal: cannot change to 'C:/Users/ijuan'`. No se revirtió ningún archivo.

## Estado General Fase 2

| Requerimiento | Estado anterior | Estado nuevo | Evidencia real | Prueba ejecutada | Archivos modificados | Observaciones |
| ------------- | --------------- | ------------ | -------------- | ---------------- | -------------------- | ------------- |
| RQF3 | AMARILLO | VERDE | Laptop real abre y botón Árbol activa `EntrepreneurTreeRoot` | Play Mode UI | Runner Fase 2 | Flujo real `UIShopDesktop.Interact("LeftClick")` |
| RQF4 | AMARILLO | VERDE | Nodo bloqueado, disponible y desbloqueado cambia estado visual | Click real `NodeUI.OnPointerClick` | Runner Fase 2 | Descuenta 1 punto |
| RQF8 | AMARILLO | VERDE | Contratación UI crea NPC visible con `NavMeshAgent` y estación | Play Mode UI/NPC | Runner Fase 2 | Mantiene fix previo |
| RQF11 | AMARILLO | VERDE | Cámara, guardias y arco se activan; sin colliders bloqueantes | Play Mode seguridad | Runner Fase 2 | Visuales runtime limpios |
| RQF12 | AMARILLO | AMARILLO | Prerrequisitos definidos y rechazo general probado | Play Mode parcial | Sin cambio | Falta barrido de rechazo específico para niveles 1/2/3 |
| RQF18 | AMARILLO | VERDE | Nivel 3 activa 99%, arresta ladrón real y restaura producto | Play Mode robo real | Runner Fase 2 | Reporte diario validado |
| RQF21 | AMARILLO | AMARILLO | Archivo temporal base y auxiliar escritos; Árbol recargado | Disco temporal | Runner Fase 2 | Falta reconstrucción integral de todos los sistemas tras recarga de escena |
| RQF36 | AMARILLO | VERDE | Click bloqueado no salta prerequisito ni gasta puntos | Play Mode UI | Runner Fase 2 | Estado visual comprobado |
| RQF28 Mejoras | AMARILLO | VERDE | Venta cajero aplica 1.05 una vez; Cafeína queda 1.10 tras reload | Play Mode caja + disco | Runner Fase 2 | Sin stacking |
| RQNF3 | AMARILLO | AMARILLO | Disco restaura puntos, empleado, rol, seguridad y mejoras | Disco temporal | Runner Fase 2 | Falta validar precios, expansiones, inventario y NPC reconstruido tras cambio de escena |
| RQNF7 | AMARILLO | VERDE | Botón compra real, falta de fondos clara, costo exacto | Play Mode UI | `OrdersAppUIController.cs` | Sin error inesperado |
| RQNF8 | AMARILLO | VERDE | Estados visuales y puntos se actualizan al pulsar nodo | Play Mode UI | Runner Fase 2 | Bloqueado/disponible/desbloqueado |
| RQNF9 | AMARILLO | VERDE | Producto inicial comprable solo mediante estado del Árbol | Play Mode UI | Mantiene adaptador previo | Paquete real creado |
| RQNF18 | AMARILLO | VERDE | Tabs reales `EXPANDIR`, `COMPRA`, `EMPLEADOS`, `AYUDA` navegables | Play Mode UI | Bootstrap + Ayuda | Sin botones muertos en recorrido probado |
| RQNF20 | AMARILLO | VERDE | App `AYUDA` accesible dentro de computadora | Play Mode UI | `InstructionsAppUIController.cs` | Explica Árbol y sistemas conectados |
| RQF13 | AMARILLO | VERDE | Cliente real convertido en ladrón común roba carrito real | Play Mode robo | Runner Fase 2 | Usa `Customer`, `CustomerCart`, `ShoplifterAgent` |
| RQF14 | AMARILLO | VERDE | Escape descuenta valor exacto del artículo reservado | Play Mode robo | Runner Fase 2 | Sin robo ficticio |
| RQF17 | AMARILLO | VERDE | Captura manual restaura producto y suma puntos | Play Mode robo | Runner Fase 2 | Reporte diario incluye captura |
| RQF25 | AMARILLO | VERDE | Tab, abrir app, seleccionar empleado y contratar funcionan | Play Mode UI | Runner Fase 2 | Dinero y roster actualizados |
| RQF26 | AMARILLO | AMARILLO | Venta automática real de tarjeta termina y acredita 1.05 | Play Mode caja | `CashDesk.cs` | Falta caso efectivo y medición exacta de ambos tiempos |
| RQF27 | AMARILLO | AMARILLO | Tarea real mueve producto de paquete a slot asignado | Play Mode surtido | Runner Fase 2 | Falta recorrido visible del NPC almacén → anaquel |

## Requerimientos que pasaron a VERDE

- `RQF3`, `RQF4`, `RQF8`, `RQF11`, `RQF18`, `RQF36`
- `RQF28` Mejoras
- `RQNF7`, `RQNF8`, `RQNF9`, `RQNF18`, `RQNF20`
- `RQF13`, `RQF14`, `RQF17`, `RQF25`

Cada uno tiene evidencia Play Mode dentro de `Game.unity` en el log Fase 2.

## Requerimientos que siguen AMARILLO

- `RQF12`: falta probar visual y funcionalmente el rechazo específico de cada nivel de seguridad antes de sus empleados requeridos.
- `RQF21`, `RQNF3`: el archivo temporal real funciona para Árbol, empleado, rol, seguridad y mejoras. Falta cargar una escena nueva y verificar inventario, precios, expansiones y reconstrucción NPC sin duplicados.
- `RQF26`: la venta automática real por tarjeta funciona y Carismático aplica una vez. Falta escenario efectivo y medición determinista de `0.5s/producto`, `1.5s` tarjeta y `2.5s` efectivo.
- `RQF27`: el coordinador transfiere producto real del paquete al anaquel asignado. El NPC visual conserva estación, pero no existe todavía recorrido animado almacén → anaquel → almacén.

## Requerimientos en ROJO

No quedaron requerimientos en ROJO en esta fase.

## Flujo UI Computadora

Validado en Play Mode:

`UIShopDesktop.Interact("LeftClick") → EXPANDIR → OpenEntrepreneurTreeButton → NodeUI → COMPRA → EMPLEADOS → AYUDA`

Se invocaron botones reales mediante `Button.onClick.Invoke()` y se comprobó activación de paneles reales.

## Flujo Productos

Validado:

`Árbol → product_basic_1 → COMPRA → Row_0/BtnBuy → DeliverySystem.Purchase → PackageObject`

También se comprobó:

- Fondos insuficientes muestran mensaje claro.
- Compra válida descuenta costo exacto.
- Pedido exitoso crea paquete real.
- El error de apertura inicial de `COMPRA` quedó corregido.

## Flujo Empleados/NPCs

Validado:

`Árbol → employee_1 → EMPLEADOS → OpenEmployeesAppButton → Employee_1 → HireButton → roster → NPC visible → NavMeshAgent → workstation cajero`

La transferencia posterior al rol Surtidor también ejecutó una tarea lógica real.

## Flujo Cajero

Validado:

`Customer real controlado → CustomerCart real → CashDesk → TryStartAutomatedCheckout → escaneo → tarjeta → venta → dinero + Carismático`

El balance aumentó exactamente con una aplicación `1.05`. Se corrigió la restauración indebida de cámara/input durante atención IA.

Pendiente: efectivo y medición exacta por modalidad.

## Flujo Surtidor

Validado:

`PackageObject real → ShelfProductSlotSystem.AssignProduct → EmployeeRestockCoordinator.ExecuteSingleTask → PlacementObject.count +1 → PackageObject.count -1`

Pendiente: movimiento visual del NPC desde fuente hasta anaquel.

## Flujo Ladrones y Seguridad

Validado:

1. Ladrón común real con artículo de carrito real.
2. Captura manual: restaura producto y otorga puntos.
3. Escape: descuenta valor exacto reservado.
4. Seguridad nivel 3: 99%, arresto automático y restauración.
5. Reporte diario: captura manual, escape y arresto automático.
6. Visuales seguridad: cámara, guardias y arco activos sin colliders que bloqueen navegación.

## Flujo Guardado/Carga

Archivo temporal base usado:

`Application.persistentDataPath/phase2_green_audit.dat`

Archivos auxiliares respaldados y restaurados después de la prueba:

- `entrepreneurTree.dat`
- `shelfSlots.dat`
- `expansionApp.dat`

Validado desde disco:

- Puntos `77`.
- Empleado contratado.
- Rol Surtidor.
- Seguridad nivel 3.
- Cafeína `1.10`.
- Carismático `1.05`.

El archivo temporal se elimina al terminar. Los auxiliares previos del usuario se restauran.

## Archivos modificados

| Archivo | Motivo |
| ------- | ------ |
| `Assets/UI/Computer/Orders/OrdersAppUIController.cs` | Reutilizar `Image` existente al abrir `COMPRA` |
| `Assets/StoreSimulator/Scripts/CashDesk.cs` | Evitar restauración de cámara/input para checkout IA |
| `Assets/Systems/EntrepreneurTree/EntrepreneurTreeUIBootstrap.cs` | Integrar tab `AYUDA` |
| `Assets/UI/Computer/Instructions/InstructionsAppUIController.cs` | Guía accesible dentro del juego |
| `Assets/UI/Computer/Instructions/InstructionsAppUIController.cs.meta` | Meta generado por Unity |
| `Assets/Systems/EntrepreneurTree/GameEndSystem.cs` | Sustituir API TMP obsoleta |
| `Assets/StoreSimulator/Scripts/UIStats.cs` | Sustituir API TMP obsoleta |
| `Assets/Systems/EntrepreneurTree/EntrepreneurTreeUpgradeAdapter.cs` | Sustituir búsqueda Unity obsoleta |
| `Assets/StoreSimulator/Editor/ShopMasterEntrepreneurTreeGreenAuditRunner.cs` | Runner Play Mode Fase 2 |
| `Assets/StoreSimulator/Editor/ShopMasterEntrepreneurTreeGreenAuditRunner.cs.meta` | Meta generado por Unity |

## Logs generados

- Batchmode Fase 2: `C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Arbol_Emprendedor_Fase2.log`
- Play Mode Fase 2: `C:\Users\ijuan\Animo\Documentos\Unity_PlayMode_Auditoria_Arbol_Emprendedor_Fase2.log`
- Editor Play Mode Fase 2: `C:\Users\ijuan\Animo\Documentos\Unity_PlayMode_Arbol_Emprendedor_Fase2_Editor.log`
- Reporte Fase 2: `C:\Users\ijuan\Animo\Documentos\Reporte_Auditoria_Arbol_Emprendedor_Fase2.md`

## Comandos usados

```powershell
dotnet build .\Animo.sln --nologo
```

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\ijuan\Animo' -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Arbol_Emprendedor_Fase2.log'
```

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' -batchmode -projectPath 'C:\Users\ijuan\Animo' -executeMethod FLOBUK.StoreSimulator.Editor.ShopMasterEntrepreneurTreeGreenAuditRunner.Run -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_PlayMode_Arbol_Emprendedor_Fase2_Editor.log'
```

## Consola

- Errores C#: no.
- `NullReferenceException`: no en pasada final.
- `MissingReferenceException`: no.
- Asserts: no.
- Warnings obsoletos solicitados: corregidos.
- `dotnet build`: `0` errores, `0` warnings.
- Runner Fase 2: `Failures=0`.

## Pendientes reales

1. Probar y medir caja automática con efectivo.
2. Implementar recorrido visual del NPC surtidor entre fuente y anaquel si se exige representación física completa.
3. Extender persistencia temporal para recarga real de escena y verificar inventario, precios, expansiones y reconstrucción NPC sin duplicados.
4. Añadir barrido específico de prerrequisitos de `security_1`, `security_2` y `security_3`.
5. Ejecutar QA visual manual en resoluciones objetivo para revisar composición final de pestañas y textos.
