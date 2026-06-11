<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 15 - Cierre total del Arbol del Emprendedor

## 1. Encabezado

- Proyecto: `SHOP_MASTER_FINAL` / ShopMaster.
- Rama base: `origin/codex/fase14-movimiento-input-playtest-reproducible`.
- Commit base: `4aafbd23a2bd2a394920a2eb28c85c8a8fa478a2`.
- Rama de trabajo: `codex/fase15-cierre-total-arbol-emprendedor`.
- Fecha de cierre tecnico: 2026-06-11.
- Estado: VERDE tecnico.

## 2. Documentos revisados

- `Documentos/NEW_Requerimientos.docx`: computadora del juego, Arbol del Emprendedor, puntos, prerequisitos, empleados, seguridad, reportes y costos visibles.
- `Documentos/propuestas juanito (2).docx`: capital inicial de $2,500, laptop/oficina como centro de administracion, Arbol estilo RPG, empleados, seguridad, reportes diarios, final Monopolio y final Bancarrota.
- `Documentos/Protocolo prpuesta Juanito (1).pdf`: progresion por logros/puntos, desbloqueo de mejoras y habilidades, seguridad, reportes y finales.
- `Documentos/Guia Gantt (1).pdf`: actividades con fecha, responsable y duracion; ninguna actividad de esta fase excede 6 horas.

## 3. Problema heredado

Fase 15 ya tenia grafo, ruta real, audit y capturas, pero el contrato actual exigia cierre total con costos escalonados. La evidencia anterior mostraba todos los nodos desbloqueables con costo 1 y solo 8 capturas `Arbol_Final_*`; eso no cumplia el criterio final.

## 4. Fuente de verdad del Arbol

- Definiciones: `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeNodeDefinition.cs`.
- Progreso real: `EntrepreneurProgress`.
- Logros/puntos: `EntrepreneurAchievementManager` y `EntrepreneurAchievementDefinitions`.
- UI real: `UIShopDesktop` reutilizando app `ARBOL`/`Licenses`.
- Guardado real: `SaveGameSystem` serializa el estado de `EntrepreneurProgress`.

## 5. Matriz completa de nodos

| Nodo | Rama | Costo | Prerequisito |
| --- | --- | ---: | --- |
| productos_basicos_1 | Producto | 0 | ninguno |
| productos_basicos_2 | Producto | 1 | productos_basicos_1 |
| productos_basicos_3 | Producto | 1 | productos_basicos_2 |
| lacteos_1 | Producto | 1 | productos_basicos_3 |
| lacteos_2 | Producto | 1 | lacteos_1 |
| lacteos_3 | Producto | 1 | lacteos_2 |
| especias_1 | Producto | 1 | productos_basicos_3 |
| productos_frescos_1 | Producto | 1 | lacteos_1 |
| productos_frescos_2 | Producto | 1 | productos_frescos_1 |
| productos_higiene | Producto | 1 | especias_1 |
| sodas | Producto | 1 | productos_higiene |
| proteina_1 | Producto | 1 | empleado_5 |
| productos_lujo_1 | Producto | 1 | sodas |
| electrodomesticos_1 | Producto | 1 | productos_lujo_1 |
| empleado_1 a empleado_9 | Empleado | 1 | prerequisitos documentales |
| empleado_10 a empleado_18 | Empleado | 2 | prerequisitos documentales |
| seguridad_1 | Seguridad | 2 | empleado_7 |
| seguridad_2 | Seguridad | 3 | empleado_8 |
| seguridad_3 | Seguridad | 3 | empleado_14 |
| mejora_cafeina | Mejora | 3 | productos_frescos_2 |
| mejora_carismatico | Mejora | 3 | empleado_15 |

## 6. Matriz de costos 0/1/2/3

- Total runtime: 37 nodos.
- Desbloqueables: 36 nodos, excluyendo `productos_basicos_1`.
- Politica aplicada por orden de `EntrepreneurTreeDefinitions.Nodes`: 60% inicial redondeado = 22 nodos costo 1; ultimo 10% redondeado hacia arriba = 4 nodos costo 3; tramo intermedio = 10 nodos costo 2.
- Resultado auditado: costo 0 = 1, costo 1 = 22, costo 2 = 10, costo 3 = 4.
- `TryUnlock` consume `node.Cost` real y el mensaje de puntos insuficientes muestra requerido/disponible.

## 7. Matriz de productos por nodo

- 14 nodos de producto cubren 47 productos documentados.
- `productos_basicos_1`: Leche, Sal, Agua, Pasta, Azucar.
- `productos_basicos_2`: Harina, Arroz, Frijoles, Pan, Aceite.
- `productos_basicos_3`: Cafe, Huevo.
- `lacteos_1`: Cheddar, Yogurt natural, Mantequilla.
- `lacteos_2`: Queso americano, Queso crema.
- `lacteos_3`: Mozzarella, Parmesano.
- `especias_1`: Pimienta negra, Canela.
- `productos_frescos_1`: Manzana, Platano, Jitomate, Cebolla.
- `productos_frescos_2`: Uvas, Zanahorias, Ajo.
- `productos_higiene`: Jabon, Papel higienico, Detergente, Pasta de dientes.
- `proteina_1`: Res, Pollo, Cerdo, Pescado.
- `sodas`: Cola, Cola sin azucar, Refresco de limon.
- `productos_lujo_1`: Trufa, Chocolate importado, Caviar.
- `electrodomesticos_1`: Refrigerador, Microondas, Horno, Mesa, Licuadora.

## 8. Matriz de logros

- Definiciones runtime: 43.
- Logros con metrica/hook operativo: 37.
- Hooks honestos pendientes: `eficiencia_maximo`, `limpieza_impecable`, `rapidez`, `precio_perfecto`, `perezoso`, `huevo_dorado`.
- Los hooks pendientes no otorgan puntos falsos por si mismos y no bloquean completar el Arbol.
- `arbol_completo` es simbolico y tiene recompensa 0 para no duplicar economia.

## 9. Matriz de efectos

- Producto: `EntrepreneurProgress.IsProductUnlocked` y `ItemDatabase` filtran por nodo real.
- Empleado: `EmployeeManager` valida `EntrepreneurProgress.IsUnlocked`.
- Seguridad: `SecurityLevel` llega a 1/2/3 con 33/66/99.
- Cafeina: `EmployeeWorkSpeedMultiplier` queda en 1.10, no se apila.
- Carismatico: `CashierRevenueMultiplier` queda en 1.05, no se apila.
- Final Monopolio: `GameEndingService` disponible y auditado; usa Arbol completo, no logros cosmeticos.

## 10. QA visual/capturas

Capturas generadas desde `UIShopDesktop.prefab` por ruta real QA:

- `Reportes/Capturas_Fase15/Arbol_01_RutaReal_VistaGeneral.png`
- `Reportes/Capturas_Fase15/Arbol_02_RamaProductos.png`
- `Reportes/Capturas_Fase15/Arbol_03_RamaEmpleados.png`
- `Reportes/Capturas_Fase15/Arbol_04_RamaSeguridad.png`
- `Reportes/Capturas_Fase15/Arbol_05_RamaMejoras.png`
- `Reportes/Capturas_Fase15/Arbol_06_DetalleNodoProducto.png`
- `Reportes/Capturas_Fase15/Arbol_07_DetalleNodoEmpleado.png`
- `Reportes/Capturas_Fase15/Arbol_08_DetalleNodoSeguridad.png`
- `Reportes/Capturas_Fase15/Arbol_09_DetalleMejora.png`
- `Reportes/Capturas_Fase15/Arbol_10_BloqueoPrerequisito.png`
- `Reportes/Capturas_Fase15/Arbol_11_PuntosInsuficientes.png`
- `Reportes/Capturas_Fase15/Arbol_12_DesbloqueoExitoso.png`
- `Reportes/Capturas_Fase15/Arbol_13_LogrosYPuntos.png`
- `Reportes/Capturas_Fase15/Arbol_14_QAOverlay.png`
- `Reportes/Capturas_Fase15/Arbol_15_PostSaveLoad.png`

## 11. Runtime audit

`Reportes/Codex_Fase15_ArbolRuntimeAudit.txt` registra 37 nodos, 36 desbloqueables, 47 productos, 18 empleados, prerequisitos completos, IDs sin duplicar, sin inalcanzables, `GameEndingService: available`, `Result: PASS`, `Errors: 0`, `Warnings: 0`.

## 12. Bugs encontrados

- Costos anteriores no eran escalonados: todos los desbloqueables costaban 1.
- Mensaje de puntos insuficientes no indicaba requerido/disponible.
- Evidencia visual anterior no usaba los 15 nombres esperados por Fase 15.

## 13. Bugs corregidos

- Costos finales: 0/1/2/3 = 1/22/10/4.
- `EntrepreneurProgress.TryUnlock` y panel de detalle reportan puntos requeridos y disponibles.
- Tests pagan `GetNodeCost` real.
- Capturas Fase15 regeneradas con 15 PNG no vacios.

## 14. Tests agregados

- `EntrepreneurTree_Fase15TierCostsMatchDocumentedPolicy`.
- `EntrepreneurTree_Fase15TryUnlockSpendsActualTierCost`.
- `EntrepreneurTree_Fase15RuntimeAuditCanBeGenerated` ahora valida `Result: PASS` y conteos de costo.
- Suite PlayMode actual: 82/82 passed.

## 15. Actividades tipo Gantt

| ID | Actividad | Requerimientos | Responsable | Fecha | Duracion | Resultado |
| --- | --- | --- | --- | --- | --- | --- |
| F15-A1 | Revisar documentos oficiales | RQF3/RQF4/RQNF8 | Codex | 2026-06-11 | 1h | OK |
| F15-A2 | Auditar grafo y costos heredados | RQF3/RQF4/RQF21 | Codex | 2026-06-11 | 1h | OK |
| F15-A3 | Corregir costos y mensajes | RQF3/RQF4/RQNF16 | Codex | 2026-06-11 | 2h | OK |
| F15-A4 | Regenerar PlayMode/audit/capturas | RQNF8/RQNF9/RQNF18 | Codex | 2026-06-11 | 2h | OK |
| F15-A5 | Actualizar guia y reporte | RQNF17/RQNF18 | Codex | 2026-06-11 | 1h | OK |

## 16. Validaciones

- `dotnet build .\SHOP_MASTER_FINAL.sln`: correcto, 0 advertencias, 0 errores.
- Unity batchmode PlayMode: `Unity.exe -batchmode -projectPath C:\Users\ijuan\SHOP_MASTER_FINAL -runTests -testPlatform PlayMode -testResults C:\Users\ijuan\SHOP_MASTER_FINAL\Reportes\Codex_Fase15_PlayModeResults.xml -logFile C:\Users\ijuan\SHOP_MASTER_FINAL\Reportes\Codex_Fase15_UnityPlayMode.log`; resultado 82/82 passed.
- Runtime audit: PASS.
- Consola/log: sin `error CS`, sin `Compilation failed`, sin `NullReferenceException`, sin `MissingReferenceException`, sin `Fatal Error`.
- Warning permitido: Render Graph compatibility mode.

## 17. Estado final

VERDE tecnico.

## 18. Confirmaciones obligatorias

- No se creo arbol paralelo.
- No se creo progreso paralelo.
- No se creo economia paralela.
- No se creo SaveGameSystem paralelo.
- `Animo/` no fue reintroducido.
- TMP Bangers/Roboto permanecen fuera de stage.
- No se stagearon builds.

## 19. Pendientes reales

- Playtest humano final de Isaac para percepcion de gameplay, lectura visual y comodidad de input.

## 20. Confirmacion explicita: no se toco Modo Admin

No se modifico, creo, renombro, movio ni stageo nada de Modo Admin. Esta fase solo toca Arbol del Emprendedor, pruebas y reportes Fase 15.
