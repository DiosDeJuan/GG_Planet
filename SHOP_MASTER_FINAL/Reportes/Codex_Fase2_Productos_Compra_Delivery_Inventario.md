<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 2 - Productos, compra, delivery e inventario

## Resumen

Se conecto la compra de productos de la computadora/laptop con el Arbol del Emprendedor existente de Fase 1, sin crear inventario, delivery, economia ni canvas paralelos. El flujo de compra ahora valida producto bloqueado por Arbol, fondos insuficientes, producto mal configurado y falta de delivery/paquete antes de descontar dinero o crear el pedido.

Tambien se ajustaron los cinco productos reales del asset como placeholders documentados: Product_A = Leche, Product_B = Sal, Product_C = Agua, Product_D = Pasta y Product_E = Azucar. Todos quedan dentro de Productos Basicos 1, desbloqueado por defecto.

## Documentos revisados

- `Documentos/NEW_Requerimientos.docx`
- `Documentos/propuestas juanito (2).docx`
- `Documentos/Protocolo prpuesta Juanito (1).pdf`
- `Documentos/Guia Gantt (1).pdf`

## Reporte base revisado

Se reviso `Reportes/Fase1_Arbol_Emprendedor_UI_Computadora.md`. La Fase 1 dejo el Arbol integrado en `UIShopDesktop`, con `EntrepreneurProgress` persistido en `SaveGameSystem`, `Productos Basicos 1` desbloqueado por defecto y una primera consulta desde `UIShopItemProduct`.

## Requerimientos usados

- RQF1: la compra desde computadora muestra productos disponibles y bloqueos en la UI existente.
- RQF2: la compra valida fondos antes de descontar dinero y muestra el monto faltante.
- RQF3: se reutiliza el acceso al Arbol dentro de la computadora de Fase 1.
- RQF4: los productos consultan prerequisitos/nodos del Arbol antes de permitir compra.
- RQF21: no se creo guardado paralelo; se mantiene `SaveGameSystem`, `StoreDatabase`, `ItemDatabase`, `DeliverySystem` y `EntrepreneurProgress`.
- RQF28/RQF29: se preserva el estado de mejoras del Arbol y se evita que clientes pidan productos bloqueados.
- RQNF7: se validan recursos y configuracion antes de comprar.
- RQNF8/RQNF9: los estados del Arbol siguen siendo la fuente de verdad para desbloqueos.
- RQNF16/RQNF17/RQNF18: se reemplazan mensajes genericos por mensajes claros dentro de la UI existente.

## Auditoria del sistema de productos

Scripts revisados:

- `Assets/StoreSimulator/Scripts/UIShopDesktop.cs`
- `Assets/StoreSimulator/Scripts/UIShopCategory.cs`
- `Assets/StoreSimulator/Scripts/UIShopItem.cs`
- `Assets/StoreSimulator/Scripts/UIShopItemProduct.cs`
- `Assets/StoreSimulator/Scripts/ProductScriptableObject.cs`
- `Assets/StoreSimulator/Scripts/ItemDatabase.cs`
- `Assets/StoreSimulator/Scripts/StoreDatabase.cs`
- `Assets/StoreSimulator/Scripts/DeliverySystem.cs`
- `Assets/StoreSimulator/Scripts/PackageObject.cs`
- `Assets/StoreSimulator/Scripts/StorageSystem.cs`
- `Assets/StoreSimulator/Scripts/CustomerCart.cs`
- `Assets/StoreSimulator/Scripts/SaveGameSystem.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurProgress.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeNodeDefinition.cs`

Assets revisados/modificados:

- `Assets/StoreSimulator/ScriptableObjects/Products/Product_A.asset`
- `Assets/StoreSimulator/ScriptableObjects/Products/Product_B.asset`
- `Assets/StoreSimulator/ScriptableObjects/Products/Product_C.asset`
- `Assets/StoreSimulator/ScriptableObjects/Products/Product_D.asset`
- `Assets/StoreSimulator/ScriptableObjects/Products/Product_E.asset`

## Mapeo producto a Arbol

Mapeo real aplicado:

- Product_A / id `0` -> `productos_basicos_1` -> Leche
- Product_B / id `1` -> `productos_basicos_1` -> Sal
- Product_C / id `2` -> `productos_basicos_1` -> Agua
- Product_D / id `3` -> `productos_basicos_1` -> Pasta
- Product_E / id `4` -> `productos_basicos_1` -> Azucar

`EntrepreneurTreeDefinitions` ahora soporta mapeo por id real y por nombre normalizado para grupos documentales: `productos_basicos_1`, `productos_basicos_2`, `productos_basicos_3`, `lacteos_1`, `especias_1`, `productos_frescos_1`, `productos_frescos_2`, `productos_higiene`, `sodas`, `proteina_1`, `productos_lujo_1` y `electrodomesticos_1`.

No se crearon productos falsos. `lacteos_2` queda pendiente porque no existe nodo de Fase 1 ni producto real asociado en el asset actual.

## Compra de productos

Causa encontrada:

- `UIShopItemProduct` solo hacia una consulta parcial al Arbol y delegaba a `DeliverySystem.Purchase`.
- `DeliverySystem.Purchase` solo distinguia dinero insuficiente con texto generico en ingles.
- No habia validacion previa de producto mal configurado, delivery faltante o paquete sin `PackageObject`.
- Product_E estaba en nivel/licencia, por lo que no cumplia el placeholder obligatorio de Azucar inicial.

Solucion aplicada:

- `UIShopItemProduct` valida y muestra estados visibles: `No configurado`, `Producto bloqueado`, `Fondos insuficientes` y licencia requerida.
- `DeliverySystem.TryPurchaseProduct` valida producto, Arbol, fondos y paquete antes de descontar dinero.
- Mensajes nuevos:
  - `Producto bloqueado. Desbloquea [nodo] en el Arbol del Emprendedor.`
  - `Fondos insuficientes. Faltan $X.XX.`
  - `Producto no configurado correctamente: [nombre/id]. Revisar catalogo.`
  - `No se pudo crear el pedido porque falta el sistema de entrega o inventario.`
  - `Pedido realizado: [producto] x[cantidad].`

El mensaje `error inesperado` no aparece en esta ruta normal de compra.

## Delivery/inventario/storage

Se reutilizo el flujo real:

- `DeliverySystem` instancia `packagePrefab` en la zona de delivery.
- `PackageObject.Add` carga el producto comprado dentro del paquete.
- `StorageSystem` sigue manejando unpack/place/pickup de almacenamiento.
- `DeliverySystem.SaveToJSON` conserva paquetes pendientes.
- `StorageSystem.SaveToJSON` conserva storage/placements segun flujo existente.

No se creo delivery paralelo ni inventario paralelo. Queda pendiente validar manualmente en Play Mode que la zona fisica de delivery de la escena este correctamente referenciada en todos los casos.

## UI de productos

La UI existente de `UIShopCategory` y `UIShopItemProduct` refresca las tarjetas cuando cambia el dinero o el Arbol. Los productos desbloqueados por `productos_basicos_1` aparecen dentro de la categoria `Productos Basicos 1`.

Estados visibles:

- Disponible: sin overlay.
- Bloqueado por Arbol: overlay con nodo requerido.
- Fondos insuficientes: overlay con monto faltante.
- No configurado: overlay con indicacion de revisar catalogo.

## Guardado/carga

Se mantiene el guardado existente:

- `EntrepreneurProgress` guarda nodos y puntos.
- `StoreDatabase` guarda dinero, XP, nivel y nombre de tienda.
- `ItemDatabase` guarda precios de productos y licencias compradas.
- `DeliverySystem` guarda paquetes/pedidos pendientes.
- `StorageSystem` guarda objetos de storage colocados.

No se duplico guardado. Los productos desbloqueados se derivan del estado persistido del Arbol.

## No duplicacion

- No inventario paralelo.
- No delivery paralelo.
- No economia paralela.
- No Canvas paralelo.
- `Animo/` fue retirado del indice del repositorio con `git rm -r --cached --ignore-unmatch Animo`.
- No se encontraron referencias funcionales a `Animo/` dentro de `SHOP_MASTER_FINAL`.

## Archivos modificados

- `Assets/StoreSimulator/Scripts/DeliverySystem.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurProgress.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeNodeDefinition.cs`
- `Assets/StoreSimulator/Scripts/ItemDatabase.cs`
- `Assets/StoreSimulator/Scripts/UIShopCategory.cs`
- `Assets/StoreSimulator/Scripts/UIShopItemProduct.cs`
- `Assets/StoreSimulator/ScriptableObjects/Products/Product_A.asset`
- `Assets/StoreSimulator/ScriptableObjects/Products/Product_B.asset`
- `Assets/StoreSimulator/ScriptableObjects/Products/Product_C.asset`
- `Assets/StoreSimulator/ScriptableObjects/Products/Product_D.asset`
- `Assets/StoreSimulator/ScriptableObjects/Products/Product_E.asset`
- `Reportes/Codex_Fase2_Productos_Compra_Delivery_Inventario.md`
- `Animo/` eliminado del indice del repositorio.

## Inscripcion POMPIC

Con inscripcion:

- `DeliverySystem.cs`
- `EntrepreneurProgress.cs`
- `EntrepreneurTreeNodeDefinition.cs`
- `ItemDatabase.cs`
- `UIShopCategory.cs`
- `UIShopItemProduct.cs`
- `Codex_Fase2_Productos_Compra_Delivery_Inventario.md`

Excepciones justificadas:

- `Product_A.asset` a `Product_E.asset`: no se agrego comentario porque son assets serializados de Unity YAML.
- Archivos bajo `Animo/`: eliminados del indice; no se modificaron para agregar inscripcion.

## Validaciones posibles desde nube

- Rama usada: `codex/fase2-productos-compra-delivery-inventario`
- Base verificada: `e7f88e1`
- `dotnet build .\SHOP_MASTER_FINAL.sln`: correcto, 0 errores, 0 advertencias.
- Unity batchmode:
  - Comando: `"C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\ijuan\SHOP_MASTER_FINAL" -logFile "C:\Users\ijuan\SHOP_MASTER_FINAL\Reportes\Unity_Fase2_Productos_Compra_Delivery_Inventario.log"`
  - Resultado: `Tundra build success`, `Application.AssetDatabase Initial Refresh End`, `Exiting batchmode successfully now!`, codigo 0.
- Busqueda de referencias funcionales a `Animo/` dentro de `SHOP_MASTER_FINAL`: sin resultados.

## Validaciones no posibles desde nube

No se ejecuto Play Mode manual ni se interactuo fisicamente con la computadora/laptop. Debe validarlo Isaac en Unity local.

## Estado final

AMARILLO: compilacion C# y Unity batchmode correctas; falta prueba manual Play Mode de compra, delivery fisico, storage y persistencia visual.

## Pendientes para siguiente fase

La siguiente fase sera Empleados:

- desbloqueo desde Arbol
- app Empleados bloqueada/desbloqueada
- contratacion
- roles Cajero/Surtidor
- NPC visible
- guardado/carga

## Pruebas manuales para Isaac

1. Abrir Unity.
2. Abrir escena principal.
3. Abrir computadora/laptop.
4. Entrar a Productos.
5. Confirmar Productos Basicos 1 disponibles.
6. Intentar comprar producto bloqueado cuando exista uno mapeado a rama bloqueada.
7. Ver mensaje de bloqueo por Arbol.
8. Desbloquear Productos Basicos 2 desde el Arbol cuando haya puntos.
9. Volver a Productos.
10. Confirmar que productos desbloqueados ya pueden comprarse.
11. Intentar comprar sin fondos.
12. Ver mensaje con monto faltante.
13. Comprar con fondos suficientes.
14. Confirmar descuento de dinero.
15. Confirmar pedido/delivery/inventario.
16. Guardar/cargar.
17. Confirmar persistencia si aplica.
18. Revisar consola sin errores rojos.

## Evidencia

- `Reportes/Unity_Fase2_Productos_Compra_Delivery_Inventario.log`
- Salida de `dotnet build .\SHOP_MASTER_FINAL.sln`: compilacion correcta, 0 advertencias, 0 errores.
