# Reporte de Verificacion Lote 6 - Computadora, Compra, Arbol y Expansion

## 1. Resumen ejecutivo
- Fecha: 2026-05-19 23:14:34 -06:00
- Rama: main
- Commit inicial: e18ab80
- Estado general: PARCIAL. Se reviso y corrigio backend funcional del lote, y Unity batchmode compilo limpio. No se marca ningun requisito como VERIFICADO EN PLAY MODE porque no hubo prueba interactiva manual real.
- Resultado batchmode: EXITOSO. Unity salio con `Exiting batchmode successfully now!`; no se encontraron `error CS`, `Scripts have compiler errors`, `Compilation failed`, `NullReferenceException` ni `MissingReferenceException`.
- Nota sobre Lote 4: `Documentos/Reporte_Verificacion_Lote4_Ladrones_Robo_Captura.md` no existe al momento de este lote. Se documento la ausencia y no bloqueo el avance, segun instruccion.

## 2. Requerimientos trabajados

| Requerimiento | Estado inicial | Trabajo realizado | Estado final | Evidencia tecnica | Evidencia Play Mode | Archivos modificados |
|---|---|---|---|---|---|---|
| RQF1 | Parcial | Se reviso que `UIShopDesktop` abre la computadora real y que `EntrepreneurTreeUIBootstrap` inyecta `COMPRA` sin duplicar tabs. Se confirmo catalogo desde `StoreDatabase`/`ItemDatabase` y bloqueo por arbol/fondos en `OrdersAppUIController`. | PARCIAL | `UIShopDesktop.Interact`, `EntrepreneurTreeUIBootstrap.EnsureOrdersTab`, `OrdersAppUIController.RefreshRow`. | No realizada. | Ninguno directo para RQF1. |
| RQF2 | Parcial con bug | Se corrigio el flujo base `DeliverySystem.Purchase` para validar fondos con mensaje de monto faltante, null-checks y reembolso si el paquete no contiene `PackageObject`. | PARCIAL | `DeliverySystem.Purchase` ahora usa `StoreDatabase.CanPurchase`, `StoreDatabase.FromLongToStringMoney` y mensaje `Fondos insuficientes. Faltan $X.XX.` | No realizada. | `Assets/StoreSimulator/Scripts/DeliverySystem.cs` |
| RQF3 | Parcial con riesgo funcional | Se reviso integracion del arbol en computadora. Se corrigio binding idempotente de botones runtime para evitar que `Abrir Arbol`, `Volver` o `Desbloquear` queden sin listener si las referencias aparecen despues. | PARCIAL | `EntrepreneurTreeUIBootstrap` integra `UpgradesUIController`; `UpgradesUIController.BuildTree`, `NodeUI`, `ConnectionLineUI`. | No realizada. | `Assets/UI/Computer/Upgrades/UpgradesUIController.cs` |
| RQF4 | Parcial | Se reviso `CanUnlockNode`/`TryUnlockNode`: valida nodo, estado, prerequisitos y puntos; no gasta puntos si falla; muestra razon por `UIGame`. Se reforzo listener del boton desbloquear. | PARCIAL | `EntrepreneurTreeManager.CanUnlockNode`, `TryUnlockNode`, `UpgradesUIController.ShowNodeInfo`, `OnUnlockButtonClicked`. | No realizada. | `Assets/UI/Computer/Upgrades/UpgradesUIController.cs` |
| RQF5 | Parcial | Se reviso que `EXPANDIR` se inyecta en computadora, construye mapa con `ExpansionMapRenderer`, selecciona zonas y compra con `SupermarketExpansionSystem.TryPurchaseZone`. | PARCIAL | `EntrepreneurTreeUIBootstrap.EnsureExpansionTab`, `ExpansionAppUIController`, `ExpansionMapRenderer`, `SupermarketExpansionSystem.TryPurchaseZone`. | No realizada. | Ninguno directo para RQF5. |
| RQF6 | Parcial | Se verifico escala monetaria: el proyecto guarda dinero en centavos. `SalesPrice=175000` equivale a `$1,750.00`; `StoragePrice=250000` equivale a `$2,500.00`. UI muestra precio/tipo/tamano antes de comprar y backend usa el mismo precio. | PARCIAL | `SupermarketExpansionSystem.SalesPrice`, `StoragePrice`, `ExpansionAppUIController.ShowZoneDetail`, `BuildStateMessage`. | No realizada. | Ninguno directo para RQF6. |

## 3. RQF1
- La computadora real esta implementada en `Assets/StoreSimulator/Scripts/UIShopDesktop.cs`. `Interact("LeftClick")` abre la vista del desktop, desactiva HUD/movimiento y mueve la camara al `lookTransform`.
- `EntrepreneurTreeUIBootstrap` encuentra el `ContentArea` de la computadora del asset e inyecta apps runtime, incluyendo `COMPRA`, reutilizando paneles/botones existentes cuando ya estan presentes.
- El catalogo de productos esta en `Assets/UI/Computer/Orders/OrdersAppUIController.cs`; se llena desde `ItemDatabase.GetByType(typeof(ProductScriptableObject))`.
- Los productos bloqueados se desactivan por `EntrepreneurTreeProductUnlockAdapter.Instance.IsProductUnlocked(product)` cuando el adapter existe. Sin adapter, mantiene fallback al flujo base para no romper el asset.
- Productos sin fondos muestran estado `Faltan $X.XX` y quedan deshabilitados.
- Pendiente: validar en Play Mode que el jugador abre la computadora desde `Game.unity`, que la app `COMPRA` aparece visualmente y que no hay duplicacion al cerrar/abrir.

## 4. RQF2
- `OrdersAppUIController.OnBuyClicked` ya validaba fondos antes de llamar al sistema base, pero `DeliverySystem.Purchase` seguia teniendo un mensaje generico en ingles y `UIGame.Instance.ShowMessage` sin null-check.
- Correccion aplicada: `DeliverySystem.Purchase` valida `purchasable`, `StoreDatabase.Instance`, `DeliverySystem.Instance`, `packagePrefab` y `deliveryStart` antes de descontar dinero.
- Si faltan fondos, calcula `missingFunds = totalCost - currentMoney`, muestra `Fondos insuficientes. Faltan $X.XX.` y no genera pedido.
- El dinero no queda negativo por este flujo porque no se llama `StoreDatabase.AddRemoveMoney` si `CanPurchase(totalCost)` falla.
- Si el prefab de paquete no tiene `PackageObject` despues de instanciar, el flujo reembolsa el total y destruye el paquete fallido.
- Pendiente: prueba Play Mode con balance suficiente e insuficiente para confirmar pedido/descuento/no pedido desde la UI real.

## 5. RQF3
- El arbol se integra en la computadora real mediante `EntrepreneurTreeUIBootstrap` y `UpgradesUIController`.
- `UpgradesUIController.BuildTree` construye nodos desde `EntrepreneurTreeManager.Instance.treeData`, crea lineas por `requiredNodeIds` y muestra costos en puntos.
- `NodeUI` muestra estado visual de nodo bloqueado, disponible y desbloqueado, y etiqueta de tipo.
- `ConnectionLineUI` conecta nodos y colorea lineas segun estado.
- Correccion aplicada: los listeners de `Abrir Arbol del Emprendedor`, `Volver` y `Desbloquear` ahora se enlazan de forma idempotente cuando existen las referencias runtime, sin depender de una unica llamada inicial.
- Pendiente: prueba Play Mode para confirmar que se abre desde la computadora, que no se duplica root y que los botones responden.

## 6. RQF4
- `EntrepreneurTreeManager.CanUnlockNode` y `TryUnlockNode` ya aplican validacion por nodo existente, estado desbloqueado, prerequisitos y puntos.
- Si faltan prerequisitos, `TryUnlockNode` devuelve false, no descuenta puntos y manda mensaje por `UIGame.Instance?.ShowMessage(reason)`.
- `UpgradesUIController.ShowNodeInfo` muestra prerequisitos con nombre legible cuando el nodo requerido existe en `TreeData`.
- La UI refresca nodos y panel de informacion al desbloquear por eventos `onNodeUnlocked` y `onPointsChanged`.
- Correccion relacionada: el boton `Desbloquear` ahora conserva listener aunque haya sido creado por autoconfiguracion runtime.
- Pendiente: prueba Play Mode intentando desbloquear un nodo bloqueado, verificando mensaje y que los puntos no bajen.

## 7. RQF5
- `EntrepreneurTreeUIBootstrap.EnsureExpansionTab` crea/reutiliza la app `EXPANDIR` dentro de la computadora.
- `ExpansionAppUIController` construye header, resumen, panel de mapa, detalle de zona y boton de compra.
- `ExpansionMapRenderer` genera botones de zonas en un plano/mapa funcional con estados comprado, disponible y bloqueado.
- `SupermarketExpansionSystem` define zonas iniciales, zonas de venta y almacenamiento, estados, prerequisitos por adyacencia y eventos de refresco.
- `TryPurchaseZone` impide comprar zonas ya compradas o bloqueadas, descuenta dinero solo en exito, marca la zona comprada y dispara eventos para UI/adapters.
- Pendiente: prueba Play Mode para seleccionar zonas, comprar, refrescar mapa y comprobar rechazo por compra duplicada.

## 8. RQF6
- Escala monetaria detectada: `StoreDatabase.currentMoney`, `buyPrice` y costos de expansion estan en centavos.
- Costos finales usados:
  - Venta: `SalesPrice = 175000L`, mostrado como `$1,750.00`, tamano `16 m2`.
  - Almacenamiento: `StoragePrice = 250000L`, mostrado como `$2,500.00`, tamano `32 m2`.
- `ExpansionAppUIController.ShowZoneDetail` muestra tipo, tamano y precio antes de comprar.
- `BuildStateMessage` muestra confirmacion con el costo exacto antes de descontar.
- `SupermarketExpansionSystem.TryPurchaseZone` descuenta el mismo `zone.price` que se muestra en la UI.
- No se corrigieron `175000/250000` porque no son bug de escala: representan centavos y son coherentes con `MoneyFormatter`.
- Pendiente: prueba Play Mode confirmando que descuento real coincide con el costo mostrado.

## 9. Bugs encontrados y corregidos

| Bug | Causa | Archivo | Correccion | Verificado |
|---|---|---|---|---|
| Compra base sin fondos mostraba mensaje generico y sin monto faltante. | `DeliverySystem.Purchase` usaba texto fijo `Not enough money to purchase this item`. | `Assets/StoreSimulator/Scripts/DeliverySystem.cs` | Mensaje USD con faltante exacto usando `StoreDatabase.FromLongToStringMoney`. | Batchmode limpio; falta Play Mode. |
| Posible NullReference si `UIGame.Instance` era null al rechazar compra sin fondos. | Llamada directa `UIGame.Instance.ShowMessage`. | `Assets/StoreSimulator/Scripts/DeliverySystem.cs` | Cambio a `UIGame.Instance?.ShowMessage` y logs `[Orders]`. | Batchmode limpio; falta Play Mode. |
| Posible descuento antes de detectar entrega mal configurada. | `DeliverySystem.Purchase` descontaba antes de validar sistema/prefab. | `Assets/StoreSimulator/Scripts/DeliverySystem.cs` | Validacion previa de `Instance`, `packagePrefab`, `deliveryStart`; reembolso si falta `PackageObject`. | Batchmode limpio; falta Play Mode. |
| Riesgo de botones del arbol sin listener tras autoconfiguracion runtime. | `BindListeners` salia si ya habia enlazado eventos aunque botones aparecieran luego. | `Assets/UI/Computer/Upgrades/UpgradesUIController.cs` | Listeners separados e idempotentes para eventos, desbloquear, abrir arbol y volver. | Batchmode limpio; falta Play Mode. |

## 10. Bugs pendientes

| Bug | Requerimiento afectado | Motivo | Prioridad |
|---|---|---|---|
| No hay verificacion interactiva de computadora/catalogo/arbol/expansion. | RQF1-RQF6 | El entorno de esta sesion ejecuto batchmode, no Play Mode manual con interaccion del jugador. | Alta |
| `Documentos/Reporte_Verificacion_Lote4_Ladrones_Robo_Captura.md` sigue ausente. | Trazabilidad documental | El usuario indico documentar ausencia y no bloquear Lote 6. | Media |
| Warnings preexistentes de TMP `enableWordWrapping` y `FindObjectsOfType`. | No directo de Lote 6 | Batchmode compila, pero conviene limpiarlos en lote tecnico futuro. | Baja |

## 11. Resultado Unity batchmode
- Comando ejecutado:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\ijuan\Animo' -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Lote6_Computadora_Arbol_Expansion.log'
```

- Log generado: `C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Lote6_Computadora_Arbol_Expansion.log`
- Resultado: exitoso.
- Errores encontrados: ninguno de los patrones obligatorios.
- Errores corregidos: no hubo errores C# tras los cambios.
- Warnings observados: warnings preexistentes de `TMP_Text.enableWordWrapping` obsoleto y `Object.FindObjectsOfType<T>(bool)` obsoleto en archivos fuera del alcance directo de este lote.

## 12. Estado final exacto

- RQF1: PARCIAL
- RQF2: PARCIAL
- RQF3: PARCIAL
- RQF4: PARCIAL
- RQF5: PARCIAL
- RQF6: PARCIAL

## 13. Siguiente lote recomendado

Prompt recomendado para Lote 7:

```text
ANIMO CABRONES. Continuar el proyecto Unity "ShopMaster" desde el avance del Lote 6.

Trabaja exactamente:
- RQF7: Mensaje de error por fondos insuficientes en espacios de expansion.
- RQF8: Desbloqueo de empleados desde el Arbol del Emprendedor.
- RQF22: Generar 50-75 clientes diarios e incrementar 15% por expansion de venta.
- RQF23: Pago por tarjeta con interaccion en terminal.
- RQF24: Pago en efectivo con cambio manual, billetes/monedas y validacion.
- RQF28-UPG: Mejoras Cafeina +10% velocidad empleados y Carismatico +5% ventas.

Antes de tocar codigo, lee:
- Documentos/Reporte_Avance_Codex_Backend.md
- Documentos/Reporte_Verificacion_Lote1_Precios_Caja.md
- Documentos/Reporte_Verificacion_Lote2_Empleados_Guardado.md
- Documentos/Reporte_Verificacion_Lote3_Surtido_Inventario_Reportes_Backup.md
- Documentos/Reporte_Verificacion_Lote5_Seguridad_Alertas_Reportes.md
- Documentos/Reporte_Verificacion_Lote6_Computadora_Arbol_Expansion.md

No avances fuera de esos requisitos. Corrige bugs funcionales directos, ejecuta Unity batchmode y crea:
Documentos/Reporte_Verificacion_Lote7_Clientes_Pagos_Mejoras.md
No marques VERIFICADO si no hubo Play Mode real.
```
