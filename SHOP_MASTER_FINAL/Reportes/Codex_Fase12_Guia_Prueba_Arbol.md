<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 12 - Guia corta de prueba del Arbol

## Escena
- Abrir `Assets/StoreSimulator/Scenes/Game.unity`.
- Entrar a Play Mode.
- Confirmar consola sin errores rojos.

## Ruta normal
1. Mover al jugador hasta la computadora/laptop del local.
2. Interactuar con la computadora.
3. En la navegacion superior, abrir `ARBOL`.
4. Confirmar que el Arbol aparece dentro de la computadora, sin Canvas principal nuevo.
5. Confirmar tema oscuro RPG, nodos, conexiones, puntos, progreso y panel de detalle.

## Ruta QA rapida
- En Editor/Development/Test, el Arbol puede abrirse por `UIShopDesktop.OpenArbolForQA()`.
- Con el Arbol abierto, presionar `F8` para mostrar u ocultar el overlay QA.
- El overlay QA permite `+1`, `+5`, `Prereqs`, `Reset`, `Guardar QA` y `Cargar QA`.
- `Guardar QA` y `Cargar QA` usan `SaveGameSystem` si existe en la escena; si no existe, el overlay lo reporta sin romper la UI.

## Pruebas manuales
1. Abrir `ARBOL`.
2. Seleccionar `Productos Basicos 1`; debe aparecer desbloqueado.
3. Seleccionar `Productos Basicos 2`; debe mostrar costo 1 punto y productos asociados.
4. Intentar desbloquear sin puntos; debe indicar `No tienes puntos de progreso suficientes.`
5. Seleccionar un empleado bloqueado, por ejemplo `Empleado 1`; debe indicar `Falta desbloquear: Especias 1`.
6. Seleccionar `Seguridad Nivel 1`; debe mostrar requisito `Empleado 7` y beneficio 33%.
7. Seleccionar `Cafeina`; debe mostrar 10% mas rapido.
8. Activar QA con `F8`, agregar puntos y desbloquear un nodo disponible.
9. Confirmar que consume 1 punto y cambia estado visual.
10. Cambiar entre `PRODUCTS`, `ARBOL`, `EMPLEADOS` y `GESTION`.
11. Volver a `ARBOL`; confirmar que no duplica nodos ni conexiones.
12. Probar `Guardar QA` y `Cargar QA` si `SaveGameSystem` esta activo.
13. Cerrar computadora y confirmar que el jugador recupera control.

## Capturas esperadas
- Revisar `Reportes/Capturas_Fase12/`.
- Deben existir 10 PNG de ruta desktop real, incluyendo vista general, nodos de producto/empleado/seguridad/mejora, bloqueo, puntos insuficientes, overlay QA, logros y navegacion de computadora.

## Criterios de aprobacion
- Play Mode sin errores rojos.
- `ARBOL` abre dentro de la computadora real.
- No aparece Canvas paralelo.
- Nodos y panel de detalle son visibles.
- Mensajes de prerequisito y puntos insuficientes son claros.
- Desbloqueo consume 1 punto.
- Cerrar y reabrir no duplica UI.
- Los 47 productos documentados tienen referencia de rama para la fase siguiente.
