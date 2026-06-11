<!-- Adaptado por POMPIC 20100333 -->
# Guia de prueba final - Arbol del Emprendedor

## Objetivo

Validar manualmente que el Arbol del Emprendedor abre desde la computadora real, muestra costos escalonados, respeta prerequisitos, consume puntos reales, aplica efectos, persiste y devuelve el movimiento al cerrar.

## Pasos para Isaac

1. Abrir `SHOP_MASTER_FINAL` en Unity.
2. Cargar `Assets/StoreSimulator/Scenes/Game.unity`.
3. Entrar a Play Mode.
4. Confirmar que el jugador se mueve.
5. Abrir la computadora.
6. Abrir `ARBOL`.
7. Revisar vista general.
8. Revisar rama Productos.
9. Revisar rama Empleados.
10. Revisar rama Seguridad.
11. Revisar rama Mejoras.
12. Revisar logros/puntos.
13. Seleccionar `Productos Basicos 1`.
14. Seleccionar `Productos Basicos 2`.
15. Confirmar costo 1 y mensaje de puntos insuficientes si tienes 0 puntos.
16. Seleccionar `Empleado 1`.
17. Confirmar mensaje `Falta desbloquear: Especias 1` si falta prerequisito.
18. Seleccionar `Empleado 7`.
19. Seleccionar `Seguridad Nivel 1`.
20. Seleccionar `Cafeina`.
21. Confirmar que el panel muestra ID, rama/tipo, estado, costo, prerequisitos y beneficio.
22. Activar QA del Arbol con F8 si estas en Editor/Development/Test.
23. Agregar puntos con `+1` o `+5`.
24. Desbloquear un nodo disponible.
25. Confirmar que los puntos se consumen segun costo real.
26. Desbloquear prerequisitos de un nodo tardio desde QA si hace falta.
27. Desbloquear un nodo costo 2 o costo 3 y confirmar consumo.
28. Cambiar a `PRODUCTS`.
29. Confirmar que el producto desbloqueado aparece disponible.
30. Volver a `ARBOL`.
31. Guardar/cargar partida.
32. Confirmar que nodos, puntos, seguridad y mejoras persisten sin duplicarse.
33. Cerrar computadora.
34. Confirmar que el movimiento vuelve.
35. Revisar consola y confirmar que no hay errores rojos.

## Evidencia automatica

- `Reportes/Codex_Fase15_ArbolRuntimeAudit.txt`
- `Reportes/Codex_Fase15_PlayModeResults.xml`
- `Reportes/Codex_Fase15_UnityPlayMode.log`
- `Reportes/Capturas_Fase15/Arbol_01_RutaReal_VistaGeneral.png`
- `Reportes/Capturas_Fase15/Arbol_15_PostSaveLoad.png`

## Resultado esperado

- 37 nodos totales.
- 14 productos, 18 empleados, 3 seguridad, 2 mejoras.
- 47 productos referenciados.
- Costos 0/1/2/3: 1/22/10/4.
- Runtime audit: PASS.
- PlayMode: 82/82 passed.
- No se toca Modo Admin.
