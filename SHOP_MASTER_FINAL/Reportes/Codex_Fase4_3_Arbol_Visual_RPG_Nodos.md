<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 4.3 - Arbol Visual RPG: nodos, ramas y conexiones

## Rama

`codex/fase4-3-arbol-visual-rpg-nodos`

## Base

Base solicitada y confirmada: `origin/codex/fase4-2-computadora-licenses-arbol-ui`

Commit base confirmado antes de editar: `7e02e99`

## Resumen

Se reemplazo la vista plana del Arbol del Emprendedor por una vista visual tipo arbol de habilidades RPG dentro del mismo panel `ARBOL` de la computadora. La nueva UI muestra nodos interactivos, ramas separadas por posicion, lineas entre prerequisitos reales, estados visibles y panel de detalle con desbloqueo conectado a `EntrepreneurProgress`.

No se creo un Canvas nuevo, no se rehizo el sistema de progreso, no se tocaron prefabs ni escenas YAML. La integracion sigue entrando por el flujo corregido en Fase 4.2: `UIShopDesktop` llama al bootstrap y el bootstrap reutiliza `ContentArea/Licenses` como host del Arbol.

## Documentos revisados

- `Documentos/NEW_Requerimientos.docx`: RQF3, RQF4, RQF8, RQF11/RQF12/RQF18, RQF21, RQF28, RQF36 y requerimientos no funcionales de UI clara.
- `Documentos/propuestas juanito (2).docx`: computadora/oficina como centro de administracion y Arbol como progresion oficial.
- `Documentos/Protocolo prpuesta Juanito (1).pdf`: interfaz clara, progresion, compra, inventario, seguridad y guardado.
- `Documentos/Guia Gantt (1).pdf`: estructura de seguimiento y evidencias.

## Reporte base revisado

Se reviso `Reportes/Codex_Fase4_2_Computadora_Licenses_Arbol_UI.md`.

Hallazgos usados:

- `LICENSES` ya se reutiliza visualmente como `ARBOL`.
- `ContentArea/Licenses` es el host runtime correcto del Arbol.
- `EntrepreneurProgress` ya es fuente de verdad para puntos, nodos, empleados, seguridad y mejoras.
- La fase anterior dejo pendiente QA visual en Play Mode, pero `dotnet build` estaba limpio.

## Problema corregido

El Arbol ya estaba integrado en la computadora, pero aun se leia como una lista/panel plano. Le faltaban:

- nodos visuales reconocibles,
- conexiones entre prerequisitos,
- ramas diferenciadas,
- sensacion de progresion,
- scroll/paneo para recorrer el arbol completo.

## Causa raiz

`EntrepreneurTreeUI` construia columnas y tarjetas mediante layout automatico. Esa estructura era funcional para desbloquear, pero no representaba visualmente la relacion entre nodos ni permitia ver caminos de progreso. El problema no estaba en `EntrepreneurProgress`; estaba en la representacion visual.

## Correccion aplicada

- `EntrepreneurTreeUI` ahora construye un panel con header, leyenda, `ScrollRect`, contenido grande, capa de conexiones, nodos y panel de detalle.
- `EntrepreneurTreeVisualLayout` centraliza posiciones de nodos por ID, respetando la definicion real del arbol.
- `EntrepreneurTreeConnectionGraphic` dibuja lineas UI entre nodos usando `Graphic` y `VertexHelper`, sin sprites externos ni prefabs nuevos.
- `EntrepreneurTreeNodeView` encapsula la tarjeta clickeable del nodo con nombre, tipo, estado y costo.
- Las conexiones se calculan desde `childNode.Prerequisites`, no desde una lista duplicada.
- Los colores de lineas cambian segun estado: bloqueado, disponible, sin puntos o desbloqueado.
- El panel de detalle conserva nombre, tipo, costo, estado, requisitos, beneficio, mensaje contextual y boton `Desbloquear`.
- El desbloqueo sigue usando `EntrepreneurProgress.TryUnlock`, con `onProgressChanged` para refrescar nodos, lineas y detalle.

## Ramas y nodos visuales

Se ubicaron visualmente las ramas principales:

- Productos iniciales y progresion basica: `productos_basicos_1`, `productos_basicos_2`, `productos_basicos_3`.
- Rama lacteos/frescos: `lacteos_1`, `lacteos_2`, `productos_frescos_1`, `productos_frescos_2`.
- Rama especias/higiene/sodas/lujo/electrodomesticos.
- Rama empleados 1-18 conectada a sus prerequisitos reales.
- Rama seguridad: `seguridad_1`, `seguridad_2`, `seguridad_3`.
- Rama mejoras: `mejora_cafeina`, `mejora_carismatico`.

## Integracion con sistemas existentes

- Progreso: `EntrepreneurProgress`.
- Definiciones y prerequisitos: `EntrepreneurTreeDefinitions`.
- Entrada desde computadora: `EntrepreneurTreeUIBootstrap`, sin cambios en esta fase.
- Panel reutilizado: `ContentArea/Licenses`, renombrado visualmente a `ARBOL` desde Fase 4.2.
- Guardado/carga: sin cambios; se conserva lo ya integrado en `SaveGameSystem` mediante `EntrepreneurProgress`.

## No duplicacion

Confirmado:

- No se creo sistema paralelo de progreso.
- No se creo sistema paralelo de guardado.
- No se creo economia paralela.
- No se creo Canvas nuevo.
- No se tocaron prefabs ni escenas.
- No se uso ni se reintrodujo `Animo/`.

## Archivos modificados

- `Assembly-CSharp.csproj`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeUI.cs`

## Archivos nuevos

- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeConnectionGraphic.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeConnectionGraphic.cs.meta`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeNodeView.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeNodeView.cs.meta`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeVisualLayout.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeVisualLayout.cs.meta`
- `Reportes/Codex_Fase4_3_UnityBatchmode.log`
- `Reportes/Codex_Fase4_3_Arbol_Visual_RPG_Nodos.md`

## Inscripcion POMPIC

- Los scripts nuevos y modificados inician con `//Adaptado por POMPIC 20100333`.
- `Assembly-CSharp.csproj` ya tenia `<!-- Adaptado por POMPIC 20100333 -->` al inicio y se conservo.
- Este reporte inicia con `<!-- Adaptado por POMPIC 20100333 -->`.
- Se agregaron `.meta` estandar para los scripts nuevos, sin inscripcion POMPIC porque el prompt pide no comentar archivos `.meta`.

## Validaciones ejecutadas

- `git branch --show-current`: `codex/fase4-3-arbol-visual-rpg-nodos`.
- `git rev-parse --short HEAD`: `7e02e99` antes del commit de esta fase.
- `git status --short --untracked-files=no`: al inicio mostro solo `EntrepreneurTreeUI.cs` modificado por el trabajo en curso.
- `git diff --check -- SHOP_MASTER_FINAL`: correcto, sin errores de whitespace; Git aviso normal de conversion LF a CRLF en archivos tocados.
- `dotnet build .\SHOP_MASTER_FINAL.sln`: correcto, 0 advertencias, 0 errores.
- Busqueda funcional `rg -n "Animo" .\Assets .\Packages .\ProjectSettings .\Documentos`: sin resultados.
- Unity batchmode: se intento con `C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe -batchmode -quit -projectPath C:\Users\ijuan\SHOP_MASTER_FINAL -logFile C:\Users\ijuan\SHOP_MASTER_FINAL\Reportes\Codex_Fase4_3_UnityBatchmode.log`.

## Resultado Unity batchmode

Estado: AMARILLO.

El comando devolvio codigo de salida `0`, pero el log indica bloqueo externo:

`It looks like another Unity instance is running with this project open.`

Tambien quedo registrado:

`Fatal Error! It looks like another Unity instance is running with this project open.`

Proceso detectado antes de batchmode:

- `Unity`, PID `23612`, iniciado el `08/06/2026 09:39:48 p. m.`

No se cerro esa instancia para evitar interrumpir el editor local de Isaac sin permiso.

## Play Mode manual

No se ejecuto Play Mode manual desde este entorno. Debe probarlo Isaac localmente.

## Estado final

AMARILLO.

El codigo C# compila en verde por `dotnet build`, el arbol visual queda implementado por codigo y conectado a `EntrepreneurProgress`. La validacion visual/Unity queda amarilla porque Unity batchmode fue bloqueado por una instancia abierta del mismo proyecto y no se realizo Play Mode interactivo.

## Pendientes para Isaac

1. Cerrar o guardar la instancia actual de Unity si quiere repetir batchmode.
2. Abrir Unity.
3. Abrir la escena principal.
4. Abrir la computadora.
5. Entrar a `ARBOL`.
6. Confirmar que se ven nodos, lineas y ramas.
7. Confirmar que el scroll permite recorrer todo el arbol.
8. Seleccionar nodos bloqueados, disponibles y desbloqueados.
9. Confirmar que el panel de detalle cambia con cada nodo.
10. Confirmar que `Productos Basicos 1` aparece desbloqueado.
11. Agregar puntos solo desde herramienta interna/editor si aplica.
12. Desbloquear un nodo disponible.
13. Confirmar que cambia el estado del nodo y de sus conexiones sin cerrar la computadora.
14. Guardar/cargar y confirmar persistencia del estado del Arbol.
15. Revisar consola sin errores rojos.

## Evidencia

- Log Unity batchmode: `Reportes/Codex_Fase4_3_UnityBatchmode.log`
- Compilacion C#: `dotnet build .\SHOP_MASTER_FINAL.sln` con 0 errores y 0 advertencias.
