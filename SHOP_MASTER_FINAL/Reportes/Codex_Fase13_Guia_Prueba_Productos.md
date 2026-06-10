<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 13 - Guia corta de prueba de productos

## Escena
- Abrir `Assets/StoreSimulator/Scenes/Game.unity`.
- Entrar a Play Mode.
- Confirmar consola sin errores rojos.

## Ruta normal en computadora
1. Mover al jugador hasta la computadora/laptop del local.
2. Interactuar con la computadora.
3. Abrir `PRODUCTS`.
4. Confirmar encabezado o listado equivalente con 47 productos.
5. Revisar que `Leche`, `Sal`, `Agua`, `Pasta` y `Azucar` esten disponibles desde el inicio.
6. Seleccionar un producto bloqueado, por ejemplo `Harina` o `Mozzarella`.
7. Confirmar mensaje `Falta desbloquear: <nodo>`.
8. Abrir `ARBOL`.
9. Desbloquear la rama requerida con puntos disponibles o usando la ruta QA de Fase 12.
10. Volver a `PRODUCTS`.
11. Confirmar que el producto ya aparece comprable.

## Compra
1. Con dinero suficiente, comprar `Leche`.
2. Confirmar descuento de dinero.
3. Confirmar que se genera una entrega/paquete real.
4. Comprar un producto placeholder desbloqueado, por ejemplo `Harina`.
5. Confirmar que tambien genera entrega y no rompe por asset provisional.
6. Intentar comprar sin dinero suficiente.
7. Confirmar mensaje de faltante y que no se genera paquete.

## Precios
1. Abrir `PRECIOS`.
2. Confirmar que aparecen 47 productos.
3. Cambiar precio de un producto base.
4. Cambiar precio de un placeholder.
5. Guardar partida si el flujo de la escena lo permite.
6. Cargar partida.
7. Confirmar que el precio modificado persiste.

## Placeholders que deben verse
- Seco: `Harina`, `Arroz`, `Pan`, `Cafe`.
- Refrigerado: `Cheddar`, `Yogurt natural`, `Manzana`, `Cola`.
- Congelado/Lujo: `Res`, `Pescado`, `Trufa`, `Caviar`.
- Electrodomestico: `Refrigerador`, `Microondas`, `Horno`, `Mesa`, `Licuadora`.

## Capturas esperadas
- Revisar `Reportes/Capturas_Fase13/`.
- Deben existir 14 PNG:
  - `Products_01_Todos.png`
  - `Products_02_Basicos1_Disponibles.png`
  - `Products_03_CategoriaBloqueada.png`
  - `Products_04_PlaceholderSeco.png`
  - `Products_05_PlaceholderRefrigerado.png`
  - `Products_06_PlaceholderLujo.png`
  - `Products_07_ElectrodomesticoPlaceholder.png`
  - `Products_08_FondosInsuficientes.png`
  - `Products_09_CompraCorrecta.png`
  - `Precios_01_Todos.png`
  - `Precios_02_ProductoPlaceholder.png`
  - `Precios_03_PrecioCero.png`
  - `Precios_04_Maximo300.png`
  - `Arbol_Productos_01_RamaProductoDesbloqueada.png`

## Criterios de aprobacion
- PRODUCTS muestra 47/47 productos.
- PRECIOS permite editar productos base y placeholders.
- Productos bloqueados no se compran y explican el nodo faltante.
- Productos desbloqueados se compran por el flujo real.
- Compra sin fondos no crea entrega.
- Compra con fondos crea entrega.
- Guardado/carga conserva precios.
- No aparecen Canvas paralelos ni UI fuera del estilo del asset.
- No hay errores rojos en consola.
