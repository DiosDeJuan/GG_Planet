# Reporte de Auditoría - Árbol del Emprendedor

## Resumen Ejecutivo

Se auditó `NEW_Requerimientos.docx`, la escena real `Assets/StoreSimulator/Scenes/Game.unity`, los scripts conectados a la computadora y los sistemas dependientes del Árbol del Emprendedor.

Se corrigieron tres causas funcionales principales:

1. Los productos podían quedar visibles pero no comprables porque `EntrepreneurTreeProductUnlockAdapter` decidía desde una caché desactualizada. Ahora consulta el estado vivo del árbol antes de permitir la compra.
2. Los empleados contratados podían no aparecer porque `EmployeeNPCSpawner` intentaba descubrir prefabs antes de que existiera `CustomerSystem` y no reintentaba al contratar. Ahora reintenta al contratar, cargar y cambiar de escena.
3. Las workstations fallback se registraban con un ID temporal y después cambiaban de ID sin reinscribirse. Ahora se registran con ID estable, se sincronizan con el roster y el NPC camina hacia la estación mediante `NavMeshAgent`.

También se corrigió integridad de robos: los ladrones Experto/Especial ya no aparecen antes de abrir Lujo o Electrodomésticos y un robo sin artículos reales reservados ya no genera una pérdida monetaria ficticia.

Se ejecutó compilación Unity batchmode y un runner automatizado dentro de Play Mode real. La pasada final terminó con `Failures=0`. No se marcan como VERDE los recorridos que todavía requieren interacción manual visible, cierre de día real o comportamiento prolongado de NPC.

El documento base presenta numeración duplicada: `RQF25`, `RQF26`, `RQF27`, `RQF28`, `RQNF7`, `RQNF8` y `RQNF9` aparecen en más de una sección. Este reporte usa el significado relacionado con Árbol y sistemas dependientes solicitado en la auditoría.

## Estado General

| Requerimiento | Área | Estado | Evidencia | Problema encontrado | Corrección realizada | Prueba realizada |
| ------------- | ---- | ------ | --------- | ------------------- | -------------------- | ---------------- |
| RQF3 | Acceso al Árbol | AMARILLO | `UpgradesUIController` existe conectado a computadora en `Game.unity` | No se automatizó clic visible del jugador | Sin cambio requerido | Play Mode: UI de computadora y controlador presentes |
| RQF4 | Desbloqueo de nodos | AMARILLO | Nodos renderizados; rechazo por prerrequisito; puntos no gastados al fallar | No se recorrieron manualmente todos los nodos | Sin cambio requerido | Play Mode: `product_basic_2`, `product_spices_1`, `employee_1` |
| RQF8 | Empleados desbloqueables | AMARILLO | Nodo desbloqueado, contratación backend y NPC visible | Prefabs no se descubrían tras arranque temprano | Reintento de prefabs y respawn | Play Mode: contratar empleado 1 y validar NPC |
| RQF11 | Seguridad | AMARILLO | Nivel 3 activa 99% | Visuales faltantes se reemplazan con placeholders | Se conserva lógica existente | Play Mode: nivel y porcentaje |
| RQF12 | Prerrequisitos seguridad | AMARILLO | Definidos en `EntrepreneurTreeDefinition` | No se recorrió cada rechazo de seguridad | Sin cambio requerido | Revisión de código + smoke parcial |
| RQF18 | Arresto automático | AMARILLO | Adaptador expone 33/66/99 y nivel 3=99% en escena | No se ejecutó evento real de robo y tirada automática | Sin cambio requerido | Play Mode: activación del nivel 3 |
| RQF21 | Guardado | AMARILLO | Round-trip JSON conserva puntos, nodos, empleado, rol y workstation | No se sobrescribió partida local con cierre/carga de día | Sin cambio requerido | Play Mode: serialización en memoria |
| RQF36 | Validación de desbloqueos | AMARILLO | Rechazo sin prerrequisito no consume puntos | Falta barrido manual de todos los nodos | Sin cambio requerido | Play Mode: rechazo de `employee_1` |
| RQF28 Mejoras | Cafeína y Carismático | AMARILLO | Multiplicadores 1.10 y 1.05 activos | Falta venta real con cajero y recarga desde disco | Sin cambio requerido | Play Mode: multiplicadores |
| RQNF3 | Persistencia completa | AMARILLO | Árbol y empleados sobreviven round-trip | Falta reinicio/carga desde archivos | Sin cambio requerido | Play Mode: round-trip JSON |
| RQNF7 | Validación de recursos | AMARILLO | Compra descuenta costo exacto; fallo de nodo no descuenta puntos | No se probaron todas las faltas de dinero | Compra devuelve éxito/fallo real | Play Mode: pedido real |
| RQNF8 | Estado de nodos | AMARILLO | Estado inicial y serialización comprobados | Falta revisión visual tras carga completa | Sin cambio requerido | Play Mode: render y round-trip |
| RQNF9 | Activación solo con nodo | AMARILLO | Producto inicial y gating premium comprobados | Falta barrido total de adapters | Consulta viva del árbol | Play Mode: productos y ladrones premium |
| RQNF18 | Consistencia visual UI | AMARILLO | Apps Árbol, Pedidos y Empleados conectadas | Falta QA visual manual por resolución | Sin cambio requerido | Play Mode: presencia y render |
| RQNF20 | Instrucciones | AMARILLO | Existe infraestructura de tutorial | Contenido y recorrido no validados | Sin cambio requerido | Revisión estática |
| RQF13 | Aparición de ladrones | AMARILLO | `ShoplifterSystem` presente; gating premium validado | No se ejecutó cliente ladrón real completo | Gating Lujo/Electrodomésticos | Play Mode: selección premium |
| RQF14 | Selección del robo | AMARILLO | Bridge reserva productos reales del carrito | Existía fallback de valor ficticio | Cancelar robo sin reserva real | Revisión de código + compilación |
| RQF17 | Intercepción manual | AMARILLO | Código recupera artículos y suma puntos | No se ejecutó interacción `E` contra ladrón real | Sin cambio requerido | Revisión integrada |
| RQF25 | App Empleados | AMARILLO | App presente; empleado contratable; roster actualizado | No se automatizó clic de contratación | Flujo NPC corregido | Play Mode: contratación backend real |
| RQF26 | Cajero | AMARILLO | NPC obtiene caja resoluble; código usa 0.5s/producto, 1.5s tarjeta, 2.5s efectivo | No se ejecutó cliente pagando con cajero | Workstation estable y movimiento NPC | Play Mode: estación de cajero |
| RQF27 | Surtidor | AMARILLO | Cambio de rol y estación de surtidor resoluble | No se observó recorrido físico paquete-anaquel | Workstation estable y movimiento NPC | Play Mode: rol y estación |

## Requerimientos Directos del Árbol

Los requerimientos directos revisados fueron:

- `RQF3`, `RQF4`, `RQF8`, `RQF11`, `RQF12`, `RQF18`, `RQF21`, `RQF36`
- `RQF28` de la sección Desbloqueo de Mejoras
- `RQNF3`, `RQNF7`, `RQNF8`, `RQNF9`, `RQNF18`, `RQNF20`

La prueba real confirmó que el árbol se construye dentro de la computadora, renderiza al menos 36 nodos runtime, parte con `product_basic_1`, rechaza saltos de prerrequisitos y no gasta puntos en un desbloqueo rechazado. La compra se validó con un paquete real y descuento exacto. Todos permanecen AMARILLO cuando falta interacción visible del jugador, carga desde disco o prueba exhaustiva.

## Requerimientos Dependientes del Árbol

Los requerimientos dependientes revisados fueron:

- `RQF13`, `RQF14`, `RQF17`
- `RQF25`, `RQF26`, `RQF27` de empleados

Se comprobó contratación, roster, NPC visible, `NavMeshAgent`, estación de cajero, cambio de rol y estación de surtidor. No se elevan a VERDE porque todavía falta una venta completa atendida por cajero, una reposición física observada y un evento real de ladrón con captura o escape.

## Diagnóstico específico: NPCs no aparecen

1. **Causa raíz encontrada:** `EmployeeNPCSpawner` se inicializaba antes que `CustomerSystem`; la autodetección de prefabs quedaba vacía y no se repetía al contratar.
2. **Scripts involucrados:** `EmployeeNPCSpawner.cs`, `EmployeeWorkstationRegistry.cs`, `EntrepreneurEmployeeSystem.cs`, `EmployeeCashierCoordinator.cs`, `EmployeeRestockCoordinator.cs`.
3. **Prefabs involucrados:** `Assets/StoreSimulator/Prefabs/Customers/Customer_A.prefab` a `Customer_E.prefab`, reutilizados como base visual de empleado.
4. **Spawn points involucrados:** se usa `employeeSpawnPoint`; si falta, se reutiliza el primer spawn de `CustomerSystem` y luego la workstation asignada.
5. **NavMesh:** `Game.unity` contiene NavMesh horneado. Play Mode confirmó triangulación y NPC con `NavMeshAgent`.
6. **Asignación de puesto:** las estaciones fallback cambiaban de ID después de registrarse. Ahora se desregistran, configuran y vuelven a registrar con ID estable.
7. **Contratación:** el roster se actualizaba, pero el NPC visual podía no construirse. Ahora la contratación fuerza redescubrimiento y spawn.
8. **Correcciones realizadas:** reintento de prefabs, respawn tras carga/escena, navegación a workstation con `NavMesh.SamplePosition`, sincronización roster-workstation.
9. **Prueba final:** Play Mode confirmó empleado contratado, roster, NPC activo, `NavMeshAgent`, workstation resoluble y cambio a surtidor.

## Flujo probado de empleados

Flujo probado:

`Árbol -> desbloqueo employee_1 -> backend Empleados -> contratación -> roster -> spawn NPC -> NavMeshAgent -> workstation cajero -> cambio rol surtidor -> workstation surtidor`

La app Empleados existe en la computadora. El runner usó el backend conectado a esa app; falta validar manualmente botones, animaciones y trabajo prolongado.

## Flujo probado de productos

Flujo probado:

`Árbol -> product_basic_1 -> puente gameplay -> DeliverySystem.Purchase -> descuento exacto -> PackageObject`

Después se desbloquearon nodos de producto y se pidió cada uno de los 46 productos reales del catálogo. Todos tienen ID, prefab, cantidad de paquete y precio no negativo; todos crearon entrega sin error. Falta un clic manual por la UI de Pedidos para declarar el recorrido visual completo VERDE.

## Flujo probado de seguridad

Flujo probado:

`Árbol -> desbloqueo seguridad -> EntrepreneurTreeSecurityAdapter -> nivel 3 -> 99% arresto automático`

La lógica se conecta con `ShoplifterSystem` y `StatsDatabase`. El reporte diario incluye robos, pérdidas, recuperaciones y arrestos. Queda AMARILLO porque no se ejecutó un cliente ladrón real hasta arresto/escape y los objetos de cámara, guardia y arco son placeholders runtime cuando no hay assets asignados.

## Archivos modificados

| Archivo | Motivo |
| ------- | ------ |
| `Assets/StoreSimulator/Scripts/DeliverySystem.cs` | Compra devuelve `bool`, reembolsa en fallos y registra bloqueo del árbol |
| `Assets/UI/Computer/Orders/OrdersAppUIController.cs` | UI solo muestra pedido exitoso cuando `DeliverySystem.Purchase` devuelve éxito |
| `Assets/Systems/EntrepreneurTree/EntrepreneurTreeProductUnlockAdapter.cs` | Consulta estado vivo del árbol y evita caché desfasada |
| `Assets/Systems/EntrepreneurTree/EmployeeNPCSpawner.cs` | Reintento de prefabs, respawn y navegación a workstation |
| `Assets/Systems/EntrepreneurTree/EmployeeWorkstationRegistry.cs` | IDs estables y sincronización con roster/NPC |
| `Assets/Systems/EntrepreneurTree/ShoplifterSystem.cs` | Gating de ladrón Experto/Especial por productos premium |
| `Assets/Systems/EntrepreneurTree/ShoplifterAgent.cs` | Cancela robo si no reserva artículos reales |
| `Assets/StoreSimulator/Editor/ShopMasterEntrepreneurTreeAuditRunner.cs` | Runner reproducible de auditoría Play Mode |
| `Assets/StoreSimulator/Editor/ShopMasterEntrepreneurTreeAuditRunner.cs.meta` | Meta generado por Unity |

## Errores corregidos

- Compra desbloqueada visible pero rechazada por caché desfasada.
- Mensaje falso de éxito de pedido cuando la entrega fallaba.
- NPC de empleado ausente por autodetección prematura de prefab.
- Workstation fallback imposible de resolver por ID mutado.
- Roster sin sincronización de workstation estable.
- NPC visual sin navegación a estación asignada.
- Ladrones premium disponibles antes de abrir productos de alto valor.
- Robo sin inventario real capaz de producir pérdida ficticia.

No aparecieron `NullReferenceException`, `MissingReferenceException`, errores C# ni fallos del runner en la pasada final.

## Pruebas realizadas

### Compilación

Comando:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\ijuan\Animo' -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Auditoria_Arbol_Emprendedor.log'
```

Resultado: Unity cerró con `Exiting batchmode successfully now!`. También se ejecutó:

```powershell
dotnet build .\Animo.sln --nologo
```

Resultado: `0 Errores`, `4 Advertencia(s)` obsoletas no críticas:

- `TMP_Text.enableWordWrapping` en `GameEndSystem.cs` y `UIStats.cs`.
- `Object.FindObjectsOfType<T>(bool)` en `EntrepreneurTreeUpgradeAdapter.cs`.

El editor mostró advertencias externas de licencia (`Code 10`, token/ULF) y luego resolvió entitlement. No fueron errores de gameplay ni impidieron compilar o ejecutar Play Mode.

### Escena y Play Mode

Comando:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' -batchmode -projectPath 'C:\Users\ijuan\Animo' -executeMethod FLOBUK.StoreSimulator.Editor.ShopMasterEntrepreneurTreeAuditRunner.Run -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_PlayMode_Auditoria_Arbol_Emprendedor_Editor.log'
```

Escena: `Assets/StoreSimulator/Scenes/Game.unity`.

Evidencia: `Documentos/Unity_PlayMode_Auditoria_Arbol_Emprendedor.log`.

Resultado final: `Audit finished. Failures=0`.

Pasos validados:

1. UI de computadora y apps Árbol, Pedidos y Empleados conectadas.
2. Nodos renderizados en runtime.
3. NavMesh horneado.
4. Nodo inicial desbloqueado.
5. Rechazo de prerrequisito sin gasto de puntos.
6. Desbloqueo de Básicos 2, Especias 1 y Empleado 1.
7. Catálogo de 46 productos válido.
8. Pedido inicial con paquete real y descuento exacto.
9. Pedido de los 46 productos tras desbloqueo de producto.
10. Gating premium de ladrón.
11. Contratación, NPC visible, `NavMeshAgent` y workstation.
12. Cambio a surtidor y estación resoluble.
13. Seguridad nivel 3 con 99%.
14. Cafeína 1.10 y Carismático 1.05.
15. Round-trip de árbol, puntos, contratación, rol y workstation.

## Pendientes reales

- Ejecutar recorrido manual desde laptop: clic de Árbol, Pedidos, Empleados y estados visuales de botones.
- Probar cajero con cliente real: escaneo, tarjeta, efectivo, tiempos y Carismático sobre ingreso final.
- Probar surtidor visible: asignación de espacio, paquete, anaquel, falta de stock y notificación.
- Probar ladrón real: aparición, reserva de artículos, arresto automático, captura manual, escape, recuperación y reporte diario.
- Asignar assets finales de cámaras, guardias y arcos; actualmente existen placeholders runtime.
- Probar cierre de día, escritura en disco, reinicio y carga sin duplicar Cafeína o Carismático.
- Revisar visualmente UI a resoluciones objetivo.
- Corregir las cuatro advertencias obsoletas cuando se programe mantenimiento técnico.
