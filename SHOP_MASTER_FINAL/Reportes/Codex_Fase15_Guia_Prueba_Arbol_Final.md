<!-- Adaptado por POMPIC 20100333 -->
# Guia de prueba final - Arbol del Emprendedor

## Objetivo

Validar manualmente que el Arbol del Emprendedor se puede usar desde la computadora real del juego y que no parece un sistema externo.

## Ruta recomendada para Isaac

1. Abrir `SHOP_MASTER_FINAL` en Unity.
2. Cargar la escena `Assets/StoreSimulator/Scenes/Game.unity`.
3. Entrar a Play Mode.
4. Abrir la computadora del supermercado.
5. En la barra de apps, abrir `ARBOL`.
6. Confirmar que aparece `ARBOL DEL EMPRENDEDOR` dentro de la computadora, no en una ventana separada.
7. Revisar que el QA overlay no aparezca por defecto en una ruta normal.
8. Seleccionar nodos de producto, empleado, seguridad y mejora.
9. Confirmar que el panel de detalle muestra beneficio, costo, prerequisitos y mensajes claros de bloqueo.
10. Activar F8 si se esta en Editor/Development/Test y confirmar que aparece el panel QA.
11. Usar `+1`, `Prereqs` y `Desbloquear` para desbloquear una rama tardia.
12. Abrir `Ver logros` y confirmar que los logros se listan sin romper el layout.
13. Cerrar y reabrir la computadora; confirmar que no se duplican nodos ni lineas.

## Puntos de aceptacion

- El Arbol vive en la app `ARBOL` de `UIShopDesktop`.
- No hay canvas principal nuevo creado por el arbol.
- Hay 37 nodos: 14 productos, 18 empleados, 3 seguridad, 2 mejoras.
- Los 47 productos documentados estan cubiertos por nodos de producto.
- Los estados visuales diferencian desbloqueado, disponible y bloqueado.
- El desbloqueo completo activa seguridad nivel 3, mejora de velocidad 1.10 y multiplicador de caja 1.05.
- La captura Fase 15 muestra vista general, productos, empleados, seguridad, mejoras, QA, logros y completado.

## Evidencia automatica esperada

- `Reportes/Codex_Fase15_ArbolRuntimeAudit.txt`
- `Reportes/Capturas_Fase15/Arbol_Final_01_VistaGeneral.png`
- `Reportes/Capturas_Fase15/Arbol_Final_08_Completado.png`
- `Reportes/Codex_Fase15_PlayModeResults.xml`
- `Reportes/Codex_Fase15_UnityPlayMode.log`
