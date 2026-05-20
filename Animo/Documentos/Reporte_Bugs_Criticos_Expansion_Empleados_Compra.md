# Reporte de Bugs Críticos: Expansión, Empleados, Compra de Productos

**Fecha:** 2026-05-20  
**Rama:** `copilot/copilotfix-emprendedor-tree-system-again`  
**Fase:** Reparación funcional pesada (lote actual)

---

## 1. Bug Crítico: Error al comprar productos

### Descripción antes del fix
Al hacer clic en el botón BUY/Comprar en la pestaña Products del escritorio (UIShopDesktop), el juego mostraba un error inesperado (excepción no controlada) o el dinero se descontaba pero el paquete nunca llegaba.

### Causa raíz identificada
En `DeliverySystem.Purchase()`:
1. **`InteractionSystem.Instance.layerMask` sin null-check**: Si el `InteractionSystem` todavía no había inicializado su singleton cuando `GetDeliveryPosition()` era invocado, se lanzaba un `NullReferenceException`.
2. **El dinero se descontaba ANTES de que se ejecutara el código de spawn del paquete**. Si la excepción ocurría en `GetDeliveryPosition()`, el dinero se perdía sin que el paquete fuera creado (sin reembolso).
3. **Sin mensajes diagnósticos**: El catch no existía, y el usuario sólo veía el error en consola sin feedback en pantalla.

### Archivos tocados
- `Animo/Assets/StoreSimulator/Scripts/DeliverySystem.cs`

### Cambios realizados
- `GetDeliveryPosition()`: cambio de `InteractionSystem.Instance.layerMask` a acceso seguro con fallback a `Physics.DefaultRaycastLayers`.
- `Purchase()`: Todo el bloque de creación del paquete ahora está dentro de `try { ... } catch (Exception ex)`, con reembolso del dinero si ocurre cualquier excepción, y mensaje claro al usuario y log detallado a consola.
- Si `PackageObject` es null en el prefab, también se reembolsa y se muestra mensaje diagnóstico.

### Pruebas pendientes (requieren Play Mode real)
- Comprar Leche, Agua, Pasta, Azúcar.
- Desbloquear "Productos Básicos 2" y comprar Harina/Arroz/Frijoles/Pan/Aceite.
- Comprar sin fondos suficientes y confirmar mensaje con monto faltante.
- Confirmar que el paquete llega a la zona de entrega sin atravesar el piso.

### Estado
**PARCIAL** — Corrección de código aplicada. No verificado con Play Mode real.

---

## 2. Bug Crítico: Expandir no afecta el terreno real

### Descripción antes del fix
La pestaña Expandir permitía comprar zonas (con validación de fondos y actualización de UI), pero ningún cambio ocurría en el mundo 3D de la escena. El `SupermarketExpansionSystem` sólo actualizaba el modelo de datos. Los `ExpansionObject` originales del asset escuchan `UpgradeSystem.onUpgradePurchase`, evento que el nuevo sistema nunca disparaba.

### Causa raíz identificada
- **Desconexión total entre `SupermarketExpansionSystem` y los objetos 3D de la escena.** El sistema de expansión nuevo (del Árbol del Emprendedor) compra zonas como datos puros pero nunca activa `ExpansionObject`, `StorageGrid`, ni crea nada en el mundo.
- El `ExpansionObject` original de FLOBUK activa paredes/grids al recibir `UpgradeSystem.onUpgradePurchase`, pero ese evento no era disparado por el nuevo sistema.

### Archivos tocados/creados
- **Nuevo:** `Animo/Assets/Systems/Expansion/ExpansionRealWorldBridge.cs`
- `Animo/Assets/Systems/EntrepreneurTree/EntrepreneurTreeUIBootstrap.cs` (registro del bridge)

### Cambios realizados
- **`ExpansionRealWorldBridge.cs`**: Nuevo componente que:
  1. Se subscribe a `SupermarketExpansionSystem.onZonePurchased` y `onZonesReset`.
  2. Al comprar una zona, busca un `ExpansionObject` en escena cuyo `expansion.id` coincida con el `zone.id`.
  3. Si encuentra un `ExpansionObject`, marca `isPurchased = true` y dispara `UpgradeSystem.NotifyPurchase()`, activando el comportamiento original del asset.
  4. Si no existe `ExpansionObject` para la zona, busca un `GameObject` por nombre convencional.
  5. Si no existe ningún objeto, **crea un placeholder de primitivas Unity** (piso semitransparente por tipo: verde=ventas, azul=almacén, naranja=oficina).
  6. En `dataLoadEvent`, restaura el estado de todas las zonas previamente compradas.

### Pruebas pendientes (requieren Play Mode real)
1. Abrir computadora → Expandir.
2. Seleccionar zona disponible.
3. Comprar → verificar cambio inmediato en el mundo.
4. Caminar hacia la zona → confirmar que el terreno/piso existe.
5. Guardar → cerrar → abrir → confirmar persistencia.

### Estado
**PARCIAL** — Bridge creado. Requiere que haya `ExpansionObject` con IDs coincidentes en escena, o se usarán placeholders. **Requiere validación en Play Mode.**

### Qué falta para 100%
- Configurar en escena `ExpansionObject` con IDs que coincidan con los zone IDs: `sales_w1`, `sales_n1`, `storage_e1`, etc.
- Ajustar `MapToWorldScale` si los placeholders no aparecen en posición correcta.

---

## 3. Bug Crítico: Empleados no se contratan / no aparecen como NPC

### Descripción antes del fix
Los empleados se desbloqueaban en el Árbol pero al intentar contratar desde la UI de Empleados no aparecía ningún NPC. `EmployeeNPCSpawner.employeePrefabs` era un array vacío porque el componente se añade en tiempo de ejecución.

### Causa raíz identificada
- `EntrepreneurTreeUIBootstrap.EnsureTreeSystems()` hace `AddComponent<EmployeeNPCSpawner>()` en runtime.
- Los campos serializados (employeePrefabs, employeeSpawnPoint) quedan vacíos/null.
- `SpawnOrRefresh()` hace early-return si `employeePrefabs` está vacío.

### Archivos tocados
- `Animo/Assets/Systems/EntrepreneurTree/EmployeeNPCSpawner.cs`

### Cambios realizados
- Nuevo método privado `AutoDiscoverCustomerPrefabs()`: usa `FindObjectsByType<Customer>()` para encontrar NPCs de tipo Customer en escena y los retorna como fuente de prefabs.
- En `Start()`: si `employeePrefabs` está vacío, llama a `AutoDiscoverCustomerPrefabs()` antes de `RespawnAll()`.
- En `OnDataLoaded()`: también llama a `AutoDiscoverCustomerPrefabs()` si necesario.

### Estado
**PARCIAL** — Auto-descubrimiento implementado. **Requiere Play Mode para confirmar.**

---

## 4. Pestaña Licencias — Ocultar

### Cambios realizados
- `EntrepreneurTreeUIBootstrap.IntegrateScene()` llama `HideLicensesTab(helper)`.
- `HideLicensesTab()`: busca botones con texto "LICENSES", "LICENCIAS", "LICENCE" y los oculta.
- La lógica interna de licencias NO se toca.

### Estado
**PARCIAL** — Código aplicado. Requiere verificar texto exacto del botón en Play Mode.

---

## 5. Anaqueles: Etiqueta con nombre y probabilidad

### Archivos creados
- `Animo/Assets/Systems/Inventory/ShelfProductInfoLabel.cs`
- `Animo/Assets/Systems/Inventory/ShelfProductInfoLabelBootstrap.cs`

### Estado
**PARCIAL** — Implementado con Canvas World-Space. Requiere Play Mode.

---

## 6. Ladrones: Gorra y mochila (placeholders)

### Archivos tocados
- `Animo/Assets/Systems/EntrepreneurTree/ShoplifterAgent.cs`

### Cambios realizados
- `ApplyVisualMarker()` llama a `SpawnThiefAccessories(color)`.
- Crea gorra (cilindro aplanado en cabeza) y mochila (cubo en espalda) con primitivas Unity.
- Materiales destruidos en `OnDestroy()`.

### Estado
**PARCIAL** — Placeholders implementados. Posición exacta depende del rig.

---

## Resumen de archivos modificados/creados

| Archivo | Acción |
|---|---|
| `Animo/Assets/StoreSimulator/Scripts/DeliverySystem.cs` | Modificado |
| `Animo/Assets/Systems/EntrepreneurTree/EntrepreneurTreeUIBootstrap.cs` | Modificado |
| `Animo/Assets/Systems/Expansion/ExpansionRealWorldBridge.cs` | **Creado** |
| `Animo/Assets/Systems/EntrepreneurTree/EmployeeNPCSpawner.cs` | Modificado |
| `Animo/Assets/Systems/EntrepreneurTree/ShoplifterAgent.cs` | Modificado |
| `Animo/Assets/Systems/Inventory/ShelfProductInfoLabel.cs` | **Creado** |
| `Animo/Assets/Systems/Inventory/ShelfProductInfoLabelBootstrap.cs` | **Creado** |

---

## Criterio de aceptación

| Criterio | Estado |
|---|---|
| Comprar productos ya no lanza "error inesperado" | **PARCIAL** |
| Licenses desaparece de UI | **PARCIAL** |
| Expandir compra zonas y terreno real crece | **PARCIAL** |
| Empleados se contratan y aparecen como NPC | **PARCIAL** |
| Anaqueles muestran nombre + probabilidad | **PARCIAL** |
| Ladrones tienen gorra/mochila | **PARCIAL** |
| Sin errores rojos nuevos | **PENDIENTE** — Requiere compilación en Unity |

**Nota:** Ningún criterio puede marcarse VERIFICADO sin Play Mode real.
