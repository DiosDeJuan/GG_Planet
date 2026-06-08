<!-- Adaptado por POMPIC 20100333 -->
# Copilot Prompt 3/5 - Productos, compra, delivery e inventario

## Resumen
Se auditó el flujo real del asset para productos/compras/delivery/inventario y se corrigió el flujo de compra para evitar mensajes genéricos, agregando validaciones explícitas de desbloqueo por Árbol, fondos, configuración de producto y dependencias de delivery. También se reforzó el mapeo producto→nodo y se alinearon los 5 productos existentes como placeholders de **Productos Básicos 1** desde el inicio.

## Documentos revisados
- `SHOP_MASTER_FINAL/Documentos/NEW_Requerimientos.docx`
- `SHOP_MASTER_FINAL/Documentos/propuestas juanito (2).docx`
- `SHOP_MASTER_FINAL/Documentos/Protocolo prpuesta Juanito (1).pdf`
- `SHOP_MASTER_FINAL/Documentos/Guia Gantt (1).pdf`

## Reportes previos revisados
- `SHOP_MASTER_FINAL/Reportes/Copilot_Prompt1_Limpieza_Estabilidad_Arbol.md`
- `SHOP_MASTER_FINAL/Reportes/Copilot_Prompt2_Arbol_Emprendedor_UI_Jerarquia.md`
- `SHOP_MASTER_FINAL/Reportes/Fase1_Arbol_Emprendedor_UI_Computadora.md`

## Requerimientos usados
- **RQF1**: se mantiene compra desde computadora/laptop con catálogo de productos y estado de disponibilidad.
- **RQF2**: compra valida fondos y ahora muestra `Fondos insuficientes. Faltan $X.XX.`.
- **RQF3**: el bloqueo/desbloqueo de productos sigue conectado al Árbol del Emprendedor.
- **RQF4**: compra de producto bloqueado muestra nodo requerido del Árbol.
- **RQF21**: se mantiene integración con `SaveGameSystem` y sistemas reales (`ItemDatabase`, `DeliverySystem`, `StorageSystem`, `EntrepreneurProgress`).
- **RQF28/RQF29**: no se reescribió el sistema de precios/ventas; se conservó la integración con `ItemDatabase`/`StoreDatabase` y flujo de inventario existente.
- **RQNF7**: se evita cálculo paralelo y se reutiliza economía real (`StoreDatabase`) en tiempo de compra.
- **RQNF8/RQNF9**: estados y bloqueos se muestran de forma explícita en UI de producto y mensajes de rechazo.
- **RQNF16/RQNF17/RQNF18**: mensajes claros y específicos dentro de la UI existente del asset, sin canvas paralelo.

## Auditoría del sistema de productos
### Scripts revisados
- `Assets/StoreSimulator/Scripts/ProductScriptableObject.cs`
- `Assets/StoreSimulator/Scripts/PurchasableScriptableObject.cs`
- `Assets/StoreSimulator/Scripts/UIShopItem.cs`
- `Assets/StoreSimulator/Scripts/UIShopItemProduct.cs`
- `Assets/StoreSimulator/Scripts/UIShopCategory.cs`
- `Assets/StoreSimulator/Scripts/UIShopDesktop.cs`
- `Assets/StoreSimulator/Scripts/DeliverySystem.cs`
- `Assets/StoreSimulator/Scripts/ItemDatabase.cs`
- `Assets/StoreSimulator/Scripts/StoreDatabase.cs`
- `Assets/StoreSimulator/Scripts/PackageObject.cs`
- `Assets/StoreSimulator/Scripts/SaveGameSystem.cs`
- `Assets/StoreSimulator/Scripts/StorageSystem.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurProgress.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeNodeDefinition.cs`

### ScriptableObjects revisados
- `Assets/StoreSimulator/ScriptableObjects/Products/Product_A.asset`
- `Assets/StoreSimulator/ScriptableObjects/Products/Product_B.asset`
- `Assets/StoreSimulator/ScriptableObjects/Products/Product_C.asset`
- `Assets/StoreSimulator/ScriptableObjects/Products/Product_D.asset`
- `Assets/StoreSimulator/ScriptableObjects/Products/Product_E.asset`

### Flujo actual identificado
- La UI de compra usa `UIShopItemProduct`.
- El catálogo por tipo/nivel se arma con `UIShopCategory` + `ItemDatabase`.
- La compra usa `DeliverySystem`, con descuento vía `StoreDatabase` y creación de paquetes con `PackageObject`.
- Inventario/persistencia se apoya en `DeliverySystem.SaveToJSON`, `StorageSystem.SaveToJSON`, `ItemDatabase.SaveToJSON` y `SaveGameSystem`.

### Qué sistema manda
- **Economía/fondos**: `StoreDatabase`.
- **Catálogo/configuración de productos**: `ItemDatabase` + `ProductScriptableObject`.
- **Pedido/delivery**: `DeliverySystem` + `PackageObject`.
- **Bloqueo por Árbol**: `EntrepreneurProgress` + `EntrepreneurTreeDefinitions`.

## Mapeo producto → Árbol
- Se mantuvo mapeo centralizado en `EntrepreneurTreeNodeDefinition.cs`.
- Se reforzó fallback por:
  1. ID real (`productNodeByProductId`),
  2. título normalizado (`productNodeByNormalizedTitle`),
  3. alias/nombre normalizado (`productNodeByKnownAlias`) para casos genéricos `Product_A..E`.
- Estado actual del catálogo real encontrado: solo 5 productos (`A-E`), no catálogo documental completo por grupo.
- Para cumplir inicio obligatorio, los productos existentes `id 0..4` quedaron mapeados a `productos_basicos_1`.
- Productos faltantes del documento (Básicos 2/3, Lácteos, Frescos, Higiene, Sodas, Proteína, Lujo, Electrodomésticos) siguen pendientes como assets reales individuales; no se creó un catálogo masivo falso en esta fase.

## Compra de productos
### Causa del “error inesperado”
Se detectó que `DeliverySystem.Purchase` no validaba dependencias ni configuración antes de instanciar, por lo que cualquier referencia faltante podía terminar en excepción genérica durante compra.

### Solución aplicada
- Se añadió validación central en `DeliverySystem.CanPurchaseWithMessage(...)`.
- Se añadió flujo seguro `DeliverySystem.TryPurchase(...)` con:
  - validación de desbloqueo por Árbol,
  - validación de nivel/licencia,
  - validación de configuración del producto,
  - validación de sistemas delivery/inventario,
  - validación de fondos y monto faltante,
  - mensaje de éxito al completar pedido.
- Se actualizó `UIShopItemProduct` para usar el flujo nuevo y refrescar estados con eventos de dinero/progreso/licencias.

### Validaciones de compra implementadas
- **Falta desbloqueo Árbol**: `Producto bloqueado. Desbloquea [nodo] en el Árbol del Emprendedor.`
- **Falta dinero**: `Fondos insuficientes. Faltan $X.XX.`
- **Producto mal configurado**: `Producto no configurado correctamente: [nombre/id]. Revisar catálogo.`
- **Falta delivery/inventario**: `No se pudo crear el pedido porque falta el sistema de entrega/inventario.`
- **Compra correcta**: `Pedido realizado: [producto] x[cantidad].`

## Delivery/inventario/storage
- Se reutilizó el sistema existente `DeliverySystem` (sin sistema paralelo).
- El flujo de compra sigue creando `PackageObject` y guardándose en `DeliverySystem.SaveToJSON()`.
- `StorageSystem` y `ItemDatabase` permanecen como sistemas reales de stock/configuración guardable.
- Pendiente: creación del catálogo documental completo con assets reales para todos los grupos del Árbol.

## UI de productos
- `UIShopItemProduct` ahora refleja estado y bloqueo de forma explícita:
  - Disponible
  - Bloqueado por Árbol
  - Fondos insuficientes
  - No configurado
- El botón de compra se desactiva cuando no se puede comprar (`Button.interactable = false`).
- Se conserva el estilo del asset porque se reutiliza el item/panel existente (sin rediseño total ni canvas nuevo).

## Guardado/carga
- Se mantiene `SaveGameSystem` existente.
- Se conserva guardado de:
  - desbloqueos del Árbol (`EntrepreneurProgress`),
  - paquetes/pedidos (`DeliverySystem`),
  - objetos/stock de storage (`StorageSystem`),
  - datos de productos/precios/licencias (`ItemDatabase`),
  - dinero/XP/nivel (`StoreDatabase`).
- No se creó sistema de save paralelo.
- Riesgo para partidas viejas: cambio de títulos/categoría en productos A-E (placeholder semántico) puede alterar texto visible del catálogo, pero mantiene IDs y referencias base del asset.

## No duplicación
- No se creó inventario paralelo.
- No se creó economía paralela.
- No se creó delivery paralelo.
- No se creó Canvas paralelo.

## Archivos modificados
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/DeliverySystem.cs`
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/UIShopItemProduct.cs`
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeNodeDefinition.cs`
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/ScriptableObjects/Products/Product_A.asset`
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/ScriptableObjects/Products/Product_B.asset`
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/ScriptableObjects/Products/Product_C.asset`
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/ScriptableObjects/Products/Product_D.asset`
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/ScriptableObjects/Products/Product_E.asset`
- `SHOP_MASTER_FINAL/Reportes/Copilot_Prompt3_Productos_Compra_Delivery_Inventario.md`

## Inscripción POMPIC
Archivos de código con inscripción `//Adaptado por POMPIC 20100333`:
- `Assets/StoreSimulator/Scripts/DeliverySystem.cs`
- `Assets/StoreSimulator/Scripts/UIShopItemProduct.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeNodeDefinition.cs`

Archivo markdown con inscripción `<!-- Adaptado por POMPIC 20100333 -->`:
- `Reportes/Copilot_Prompt3_Productos_Compra_Delivery_Inventario.md`

Excepciones justificadas:
- Se modificaron `.asset` de productos (no archivos de código), por lo que no aplica inscripción de cabecera de código.

## Validaciones posibles desde la nube
- `Animo/` no existe en el repositorio actual.
- No se detectaron referencias funcionales nuevas a `Animo/`.
- No se crearon sistemas duplicados de inventario/economía/delivery.
- Validación estática de flujo de compra y mensajes específicos por inspección de código.
- `dotnet build SHOP_MASTER_FINAL.sln` intentado antes y después de cambios.

## Validaciones no posibles desde la nube
- No se pudo ejecutar Unity Editor ni Play Mode.
- El build .NET falla por limitación de entorno (falta targeting pack .NET Framework 4.7.1), no por una validación de runtime Unity.
- No se pudo validar visualmente en escena el layout final de estados en la laptop.

## Estado final
- Lógica de compra con mensajes específicos: **VERDE**
- Bloqueo/desbloqueo de productos por Árbol en flujo de compra/UI: **VERDE**
- Productos Básicos 1 iniciales con placeholders existentes: **VERDE**
- Integración con delivery/inventario real del asset: **VERDE**
- Persistencia guardado/carga sin sistema paralelo: **VERDE**
- Validación runtime real en Unity local: **AMARILLO**
- Catálogo documental completo como assets reales (más de A-E): **AMARILLO**

## Pendientes para Prompt 4/5
- Implementar empleados desbloqueables desde Árbol con contratación real.
- Definir roles cajero/surtidor con NPC visible.
- Conectar asignación de puesto con flujo real de tienda.
- Mantener integración con sistemas existentes sin duplicar economía/inventario/delivery.

## Pruebas que Isaac debe hacer en Unity local
1. Abrir Unity local.
2. Abrir escena principal.
3. Abrir computadora/laptop.
4. Entrar a Productos.
5. Confirmar Productos Básicos 1 disponibles.
6. Confirmar productos bloqueados por Árbol (si existen assets en esos grupos en su escena).
7. Abrir Árbol y desbloquear Productos Básicos 2.
8. Volver a Productos y verificar que ahora aparecen/compran los desbloqueados (si hay assets mapeados a ese grupo).
9. Intentar comprar sin fondos y verificar mensaje con monto faltante.
10. Comprar con fondos suficientes.
11. Verificar que se descuenta dinero.
12. Verificar que el pedido/inventario/storage recibe el producto.
13. Guardar/cargar y verificar que desbloqueos/inventario se mantienen si aplica.
14. Confirmar que no aparece “error inesperado” como mensaje genérico de compra normal.
15. Confirmar que no hay errores rojos en consola.
