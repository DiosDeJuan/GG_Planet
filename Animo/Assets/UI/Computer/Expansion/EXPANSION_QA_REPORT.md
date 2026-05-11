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
