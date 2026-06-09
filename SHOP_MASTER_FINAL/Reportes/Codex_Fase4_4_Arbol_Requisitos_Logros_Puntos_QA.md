<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 4.4 - Arbol del Emprendedor: requisitos, logros, puntos y QA funcional

## Resumen

Se continuo la rama `codex/fase4-4-arbol-requisitos-logros-puntos-qa` desde la base `801d723` (`Codex Fase 4.3: arbol visual con nodos y ramas`) sin reiniciar ni descartar el trabajo parcial. La fase agrega un sistema centralizado de logros para otorgar puntos de progreso, persiste logros y metricas acumuladas dentro de `EntrepreneurProgress`, conecta ventas reales, empleados, seguridad manual y nodos del Arbol, y amplia la UI del Arbol para mostrar puntos, progreso, porcentaje y lista de logros.

El Arbol existente de Fase 4.3 no se rehizo: se extendio su progreso, sus eventos y su panel dentro de la computadora/laptop.

## Documentos revisados

- `Documentos/NEW_Requerimientos.docx`
- `Documentos/propuestas juanito (2).docx`
- `Documentos/Protocolo prpuesta Juanito (1).pdf`
- `Documentos/Guia Gantt (1).pdf`

## Reportes previos revisados

- `Reportes/Fase1_Arbol_Emprendedor_UI_Computadora.md`
- `Reportes/Codex_Fase2_Productos_Compra_Delivery_Inventario.md`
- `Reportes/Codex_Fase3_Empleados_Arbol_Laptop_NPCs.md`
- `Reportes/Codex_Fase4_Seguridad_Ladrones_Mejoras_Arbol_Reportes.md`
- `Reportes/Codex_Fase4_1_Hotfix_Movimiento_Arbol_QA.md`
- `Reportes/Codex_Fase4_2_Computadora_Licenses_Arbol_UI.md`
- `Reportes/Codex_Fase4_3_Arbol_Visual_RPG_Nodos.md`

## Requerimientos auditados

- RQF3: el Arbol sigue integrado en computadora/laptop y ahora muestra puntos, progreso y logros.
- RQF4/RQF36: se conserva validacion centralizada de prerequisitos en `EntrepreneurProgress.TryUnlock`.
- RQF8: empleados siguen ligados a nodos `empleado_1` a `empleado_18`; se agregan logros por contratacion y maximo empleo.
- RQF11/RQF12/RQF18: seguridad sigue derivada de nodos `seguridad_1`, `seguridad_2`, `seguridad_3`; se agregan logros de red de seguridad y arrestos manuales.
- RQF17/RQF21: estado de Arbol, puntos, logros, recompensas reclamadas y metricas acumuladas se guardan/cargan en el JSON del progreso.
- RQF28/RQF29: Cafeina y Carismatico siguen como mejoras del Arbol; se agrega logro de optimizacion total.
- RQNF1/RQNF3/RQNF7: se mantiene flujo de bajo acoplamiento y se valida por compilacion C# y Unity batchmode.
- RQNF8/RQNF9/RQNF16/RQNF17/RQNF18: la UI mantiene estados de nodos y agrega mensajes legibles de logros.
- RQNF20: se agrega texto visible que explica que los puntos vienen de logros.

## Auditoria de nodos del Arbol

El Arbol mantiene 36 nodos reales definidos en `EntrepreneurTreeNodeDefinition`:

- Productos: `productos_basicos_1`, `productos_basicos_2`, `productos_basicos_3`, `lacteos_1`, `lacteos_2`, `especias_1`, `productos_frescos_1`, `productos_frescos_2`, `productos_higiene`, `sodas`, `proteina_1`, `productos_lujo_1`, `electrodomesticos_1`.
- Empleados: `empleado_1` a `empleado_18`.
- Seguridad: `seguridad_1`, `seguridad_2`, `seguridad_3`.
- Mejoras: `mejora_cafeina`, `mejora_carismatico`.

`productos_basicos_1` sigue desbloqueado por defecto. Los demas nodos mantienen costo de 1 punto y dependen de sus prerequisitos centralizados, por lo que no hay bypass por UI.

## Sistema de logros implementado

Se agregaron:

- `EntrepreneurAchievementDefinition`: catalogo de logros, nombre, descripcion, recompensa y estado de hook.
- `EntrepreneurAchievementManager`: seguimiento de logros, puntos otorgados una sola vez, metricas acumuladas, guardado/carga y evaluacion.

Logros funcionales conectados:

- Ventas: `primeras_ventas`, `venta_rapida`, `ventas_diarias_1` a `ventas_diarias_6`.
- Ingresos: `ingresos_1` a `ingresos_9`, `lluvia_dinero`.
- Productos: `cliente_lujo`, `lindo_hogar`, `donador`.
- Empleados: `primer_empleado`, `maximo_empleo`.
- Seguridad: `red_seguridad`, `batman`.
- Arbol/mejoras: `surtido_completo`, `optimizacion_total`, `optimista`, `arbol_completo`.
- Tiempo/estilo de juego: `dedicado`, `fiel`, `emprendedor`, `bajo_presion`.
- Expansion: `imperialista`.

Logros registrados como hook formal porque faltan metricas estables en el asset o requieren flujo de fase posterior:

- `supermercado_crecimiento`
- `almacenamiento_maximizado`
- `rapidez`
- `precio_perfecto`
- `paciente`
- `perezoso`
- `huevo_dorado`

Los hooks aparecen en la lista de logros como pendientes y no otorgan puntos automaticamente hasta que exista una metrica confiable.

## Puntos y recompensas

- Cada logro completado funcional puede otorgar puntos mediante `EntrepreneurProgress.AddProgressPoint`.
- El otorgamiento real se centraliza en `EntrepreneurAchievementManager.TryAwardKnownAchievement`.
- `claimedRewardIds` evita que un logro entregue puntos mas de una vez.
- `arbol_completo` queda como logro simbolico sin punto para evitar punto sobrante al final.
- `AvailablePoints` se mantiene como la fuente de puntos disponibles.

## Guardado y carga

`EntrepreneurProgress.SaveToJSON` ahora incluye:

- `progressPoints`
- `unlockedNodeIds`
- `treeCompletionNotified`
- `achievements`

`achievements` guarda:

- logros completados
- recompensas reclamadas
- ventas acumuladas
- productos vendidos con precio cero
- dias jugados
- arrestos manuales
- ingreso total
- ingreso diario actual
- mejor ingreso diario

Al cargar, `EntrepreneurProgress.LoadFromJSON` restaura logros y despues `SaveGameSystem` llama `EntrepreneurAchievementManager.EvaluateAll()` para revalidar estados derivados de empleados, seguridad, nodos y expansiones.

## Integraciones realizadas

- Ventas normales: `CashDesk.OnBillCustomer` registra venta real antes de limpiar el carrito.
- Self checkout: `SelfCheckout.OnBillCustomer` registra venta real del cliente.
- Empleados: `EmployeeManager` avisa al sistema de logros al contratar, asignar rol y cargar.
- Seguridad: `SecurityManager.RegisterArrest` registra `batman` solo para arrestos manuales.
- Nodos: `EntrepreneurProgress.TryUnlock` notifica cada nodo desbloqueado y detecta Arbol completo.
- UI: `EntrepreneurTreeUI` muestra puntos, progreso `X/Y`, porcentaje y lista de logros con estado.

## Actividades tipo Gantt

| Actividad | Responsable | Duracion estimada | Estado |
|---|---:|---:|---|
| Auditar reportes Fase 1 a Fase 4.3 | Codex | 1h | VERDE |
| Revisar documentos base y reglas de progreso | Codex | 1h | VERDE |
| Auditar sistemas existentes de Arbol, ventas, empleados y seguridad | Codex | 1.5h | VERDE |
| Implementar catalogo de logros y manager | Codex | 2h | VERDE |
| Conectar ventas, empleados, seguridad y nodos | Codex | 1.5h | VERDE |
| Persistir logros y metricas | Codex | 1h | VERDE |
| Actualizar UI del Arbol | Codex | 1.5h | VERDE |
| Validacion dotnet y Unity batchmode | Codex | 1h | VERDE |
| Play Mode funcional de computadora/laptop | Isaac | 1h | PENDIENTE |

Todas las actividades tecnicas se mantienen por debajo de 6 horas estimadas.

## Archivos modificados

- `Assembly-CSharp.csproj`
- `Assets/StoreSimulator/Scripts/CashDesk.cs`
- `Assets/StoreSimulator/Scripts/SelfCheckout.cs`
- `Assets/StoreSimulator/Scripts/SaveGameSystem.cs`
- `Assets/StoreSimulator/Scripts/Employees/EmployeeManager.cs`
- `Assets/StoreSimulator/Scripts/Security/SecurityManager.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurProgress.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeUI.cs`

## Archivos nuevos

- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurAchievementDefinition.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurAchievementDefinition.cs.meta`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurAchievementManager.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurAchievementManager.cs.meta`
- `Reportes/Codex_Fase4_4_UnityBatchmode.log`
- `Reportes/Codex_Fase4_4_Arbol_Requisitos_Logros_Puntos_QA.md`

## Inscripcion POMPIC

- Todos los archivos `.cs` nuevos/modificados revisados inician con `//Adaptado por POMPIC 20100333`.
- `Assembly-CSharp.csproj` inicia con `<!-- Adaptado por POMPIC 20100333 -->` despues de la declaracion XML.
- Este reporte inicia con `<!-- Adaptado por POMPIC 20100333 -->`.
- Los `.meta` nuevos no llevan inscripcion porque Unity puede romper o regenerar esos archivos si se agregan comentarios manuales.

## Validaciones ejecutadas

- `git branch --show-current`: `codex/fase4-4-arbol-requisitos-logros-puntos-qa`.
- `git status --short --untracked-files=all -- SHOP_MASTER_FINAL`: cambios pendientes solo bajo `SHOP_MASTER_FINAL`.
- `git log --oneline -8`: base visible `801d723 Codex Fase 4.3: arbol visual con nodos y ramas`.
- `git diff --check -- SHOP_MASTER_FINAL`: sin errores de whitespace; solo advertencias LF/CRLF.
- `dotnet build .\SHOP_MASTER_FINAL.sln`: compilacion correcta, 0 advertencias, 0 errores.
- Unity batchmode: `C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe -batchmode -projectPath C:\Users\ijuan\SHOP_MASTER_FINAL -quit -logFile Reportes\Codex_Fase4_4_UnityBatchmode.log`.
- Resultado Unity batchmode: salida 0; el log muestra compilacion de scripts con `Tundra build success` y no contiene `error CS`, `Scripts have compiler errors`, `Compilation failed` ni `Fatal Error`.
- `rg -n "Animo" .\Assets .\Packages .\ProjectSettings .\Documentos`: sin coincidencias funcionales.

## Cambios detectados no incluidos

Durante la validacion aparecieron cambios en:

- `Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Bangers SDF.asset`
- `Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset`

Son assets de TextMesh Pro con tablas de glifos modificadas y no forman parte del alcance de Fase 4.4. No se incluyen en el commit de esta fase para no mezclar cambios visuales ajenos al Arbol/logros.

## Validaciones no realizadas

No se ejecuto Play Mode manual desde el editor. Isaac debe validar en Unity:

1. Abrir escena principal.
2. Abrir computadora/laptop.
3. Entrar al Arbol del Emprendedor.
4. Confirmar que aparecen puntos, progreso `X/Y`, porcentaje y boton/lista de logros.
5. Vender productos desde caja normal y self checkout.
6. Confirmar que los logros de venta otorgan puntos una sola vez.
7. Contratar/asignar empleados y confirmar logros relacionados.
8. Desbloquear seguridad y confirmar logros de red de seguridad.
9. Capturar un ladron manualmente y confirmar logro Batman.
10. Guardar/cargar y confirmar que logros, puntos y nodos persisten.
11. Revisar consola sin errores rojos.

## Estado final

AMARILLO.

El codigo compila por `dotnet build` y Unity batchmode recompilo scripts sin errores. Queda pendiente la prueba Play Mode manual de la UI y del flujo completo dentro del editor.

## Pendientes para siguiente fase

- Conectar hooks pendientes cuando existan metricas fiables: precio perfecto, paciencia/pereza, rapidez, huevo dorado, crecimiento/almacenamiento maximo.
- Profundizar UI de logros si se requiere iconografia o filtros por categoria.
- Validar visualmente en editor que la lista de logros no se desborde en resoluciones pequenas.
