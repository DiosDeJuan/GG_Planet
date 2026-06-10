<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 14 - Guia corta de prueba de movimiento

## Escena
- Abrir `Assets/StoreSimulator/Scenes/Game.unity`.
- Entrar a Play Mode.
- Hacer click en Game View si Unity no captura mouse/teclado de inmediato.

## Controles
- WASD: caminar.
- Mouse: mirar.
- Shift: correr.
- Space: saltar.
- Ctrl o C: agacharse.
- Click izquierdo / accion visible: interactuar.
- Escape o X: cerrar computadora/UI.
- F7: restaurar movimiento si se queda bloqueado.
- F9: mostrar/ocultar diagnostico de movimiento.

## Pasos
1. Entrar a Play Mode.
2. Confirmar que el cursor queda bloqueado/oculto al jugar.
3. Probar WASD adelante, atras, izquierda y derecha.
4. Mover mouse y confirmar que la camara mira.
5. Mantener Shift y confirmar que corre mas rapido.
6. Presionar Space y confirmar salto.
7. Mantener Ctrl o C y confirmar agacharse.
8. Caminar hasta la computadora/laptop.
9. Interactuar con la computadora.
10. Confirmar que el cursor queda visible/libre y el jugador no camina mientras la computadora esta abierta.
11. Abrir `ARBOL`.
12. Cerrar con X o Escape.
13. Confirmar que vuelve a moverse.
14. Repetir con `PRODUCTS`.
15. Repetir con `PRECIOS` o `GESTION`.
16. Repetir con `EMPLEADOS`.
17. Abrir/cerrar computadora 5 veces.
18. Confirmar que no duplica acciones ni bloquea movimiento.

## Si no se mueve
1. Hacer click dentro de Game View.
2. Presionar F7.
3. Confirmar que el cursor se bloquea y WASD vuelve.
4. Presionar F9 y revisar:
   - `canMove`
   - `canLook`
   - `ActionMap`
   - `timeScale`
   - `Cursor`
   - `CC enabled`
   - ultimo motivo de bloqueo
5. Tomar captura del diagnostico y copiar el ultimo mensaje de consola.

## Consola permitida
- Warning de Render Graph compatibility mode.
- Warning unico de action map `UI` opcional, si aparece una vez y no bloquea gameplay.

## Consola no permitida
- NullReferenceException.
- MissingReferenceException.
- Errores CS.
- Spam repetido de PlayerController.
- UI invisible bloqueando input despues de cerrar.

## Criterios de aprobacion
- WASD mueve al jugador.
- Mouse mira.
- Shift corre.
- Space salta.
- Ctrl/C agacha y al soltar recupera altura.
- Computadora abre y bloquea movimiento intencionalmente.
- Cerrar computadora restaura movimiento.
- ARBOL, PRODUCTS, PRECIOS/GESTION y EMPLEADOS no dejan input bloqueado.
- F7 recupera gameplay si algo queda atrapado.
- F9 muestra diagnostico y puede ocultarse.
- No hay errores rojos.
