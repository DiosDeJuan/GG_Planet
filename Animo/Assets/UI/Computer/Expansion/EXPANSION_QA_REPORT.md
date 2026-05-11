# EXPANSION QA REPORT

## Archivos creados
- `Assets/Systems/Expansion/ExpansionZoneData.cs`
- `Assets/Systems/Expansion/SupermarketExpansionSystem.cs`
- `Assets/UI/Computer/Expansion/ExpansionAppUIController.cs`
- `Assets/UI/Computer/Expansion/ExpansionMapRenderer.cs`
- `Assets/UI/Computer/Expansion/ExpansionZoneButtonUI.cs`

## Archivos modificados
- `Assets/Systems/EntrepreneurTree/EntrepreneurTreeUIBootstrap.cs`

## Cómo probar la pestaña
1. Entrar a Play Mode.
2. Abrir la computadora del supermercado.
3. Verificar botón `EXPANDIR` junto a las pestañas existentes.
4. Clic en `EXPANDIR` para abrir panel de expansión.
5. Seleccionar una zona del mapa.
6. Revisar detalle de nombre, tipo, tamaño, precio, estado y descripción.
7. Probar compra con fondos insuficientes y confirmar mensaje de faltante.
8. Probar compra con fondos suficientes y confirmar descuento/estado comprado.
9. Cerrar y abrir computadora varias veces y validar que no se duplique el botón.

## Reglas de expansión implementadas
- Mapa 2D top-down con zonas de venta, almacenamiento y oficina.
- Estado por zona: `Comprada`, `Disponible`, `Bloqueada`.
- Compra validada con `StoreDatabase.CanPurchase`.
- Costos:
  - Venta 16 m²: `$1,750`
  - Almacenamiento 32 m²: `$2,500`
- Al comprar:
  - descuento de dinero,
  - cambio de estado a comprada,
  - refresco de mapa y panel.

## Estado de save/load
- Implementado guardado/carga en archivo separado `expansionApp.dat` (escucha eventos de `SaveGameSystem`).
- Persisten IDs de zonas compradas.

## Riesgos residuales
- QA runtime final pendiente en Unity Editor (no disponible en sandbox).
- El estilo visual usa UI runtime sin prefabs dedicados; puede requerir ajuste fino en Editor para pixel-perfect.

---

## Update — 2026-05-11 (Integración auditada)

### Estado de integración
- `EXPANDIR` sigue integrado en la computadora sin reemplazar tabs base.
- Se reforzó la vinculación del botón para evitar duplicación de listeners al recargar/integrar escena.
- Se endureció persistencia de expansión para archivos inexistentes, vacíos o corruptos.

### Cómo probar en Unity
1. Play Mode > abrir computadora > confirmar tab `EXPANDIR` (una sola vez).
2. Abrir/cerrar computadora varias veces y cambiar de escena si aplica.
3. Entrar a `EXPANDIR`, seleccionar zonas y verificar panel derecho completo.
4. Intentar compra con fondos insuficientes y con fondos suficientes.
5. Guardar/cargar y confirmar persistencia de zonas compradas.

### Riesgos de UI
- Ajuste visual de layout runtime (mapa y paneles) puede requerir calibración en resoluciones extremas.
- Verificar que el tab creado no compita con estilos/animaciones de botones del asset base.

### Riesgos de save/load
- Persistencia separada (`expansionApp.dat`) depende del ciclo de eventos de `SaveGameSystem`.
- Se agregó manejo defensivo; aún debe validarse en ciclo real de guardado/carga dentro de Unity.

### Logs esperados
- `[ExpansionApp] Expansion tab created.`
- `[ExpansionApp] Expansion panel created.`
- `[ExpansionApp] Loaded X zones.`
- `[ExpansionApp] Selected zone: zoneId.`
- `[ExpansionApp] Purchase failed: insufficient funds. Missing X.`
- `[ExpansionApp] Purchased zone: zoneId.`
- `[ExpansionApp] Map refresh complete.`
