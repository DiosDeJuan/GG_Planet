# ShopMaster — HANDOFF AFTER PULL

## 1) Estado general del sistema

La integración del **Árbol del Emprendedor** está funcional a nivel de código y enlazada con la computadora del juego en el panel `UPGRADES/Expansions` sin reemplazar la UI base del asset.

Fase 4 cerró con hardening estático, correcciones de riesgos runtime y documentación de QA.  
**Unity Editor no estuvo disponible en sandbox**, por lo que la validación final de Play Mode real sigue pendiente en máquina local.

## 2) Rama actual

`copilot/tree-of-entrepreneur-skills`

## 3) Último commit importante

- `a82b9b5` — *Phase 4 static validation hardening and QA report*

## 4) Sistemas implementados (resumen)

- Árbol del Emprendedor integrado en computadora (UPGRADES/Expansions).
- Productos bloqueados/desbloqueados vía árbol.
- App de empleados dentro de computadora.
- Contratación de 18 empleados.
- Roles Cajero/Surtidor con persistencia.
- Mejoras Cafeína y Carismático conectadas.
- Seguridad por niveles (33/66/99) con arresto automático.
- Sistema de ladrones con detección, captura manual y escape.
- Bridge de inventario de robos (reserva, restauración, confirmación de pérdida).
- Reabasto real desde `PackageObject` hacia `PlacementObject`.
- Reporte diario de robos/empleados/seguridad.
- Integración de guardado/carga no invasiva.

## 5) Archivos más importantes

- `Assets/Systems/EntrepreneurTree/EntrepreneurTreeManager.cs`
- `Assets/Systems/EntrepreneurTree/EntrepreneurTreeGameplayBridge.cs`
- `Assets/Systems/EntrepreneurTree/EntrepreneurTreeUIBootstrap.cs`
- `Assets/UI/Computer/Upgrades/UpgradesUIController.cs`
- `Assets/UI/Computer/Employees/EmployeeAppUIController.cs`
- `Assets/Systems/EntrepreneurTree/EntrepreneurEmployeeSystem.cs`
- `Assets/Systems/EntrepreneurTree/EmployeeRestockCoordinator.cs`
- `Assets/Systems/EntrepreneurTree/ShoplifterSystem.cs`
- `Assets/Systems/EntrepreneurTree/ShoplifterAgent.cs`
- `Assets/Systems/EntrepreneurTree/ShoplifterInteractable.cs`
- `Assets/Systems/EntrepreneurTree/RobberyInventoryBridge.cs`
- `Assets/Systems/EntrepreneurTree/EntrepreneurTreeSecurityAdapter.cs`
- `Assets/Systems/EntrepreneurTree/EntrepreneurTreeUpgradeAdapter.cs`
- `Assets/Systems/EntrepreneurTree/EntrepreneurTreeSaveIntegration.cs`
- `Assets/StoreSimulator/Scripts/StatsDatabase.cs`
- `Assets/StoreSimulator/Scripts/UIStats.cs`
- `Assets/Systems/EntrepreneurTree/PHASE4_QA_REPORT.md`

## 6) Cómo está integrada la computadora

- El bootstrap runtime (`EntrepreneurTreeUIBootstrap`) ubica `ContentArea/Expansions`.
- Inyecta `UpgradesUIController` y `EmployeeAppUIController` sobre el panel existente.
- Conserva el contenido legacy y alterna vista legacy ↔ árbol.
- Crea sistemas faltantes sólo si no existen (`GetComponent` + `AddComponent` con guardas).

## 7) Cómo funciona el Árbol del Emprendedor

- `EntrepreneurTreeManager` controla puntos, validación de prerequisitos, desbloqueo y eventos.
- `TreeData` principal: `Assets/Data/EntrepreneurTree/EntrepreneurTreeData.asset`.
- Nodo raíz por defecto: `product_basic_1`.
- `EntrepreneurTreeGameplayBridge` expone consultas para productos, empleados, seguridad y upgrades.

## 8) Cómo funciona la app de empleados

- `EmployeeAppUIController` crea/reusa jerarquía UI dentro de Expansions.
- Lista 18 empleados, muestra detalle y habilita contratación/asignación según estado.
- No contrata automáticamente al desbloquear nodo; contratación es explícita desde app.

## 9) Roles Cajero/Surtidor

- Definidos en `EmployeeRole` (`None`, `Cashier`, `Restocker`).
- Persisten por `EntrepreneurEmployeeSystem.SaveToJSON()/LoadFromJSON()`.
- Impactan multiplicadores de velocidad de caja/reabasto.

## 10) Reabasto (surtidores)

- `EmployeeRestockCoordinator` ejecuta ciclos por cantidad de surtidores activos.
- Busca `PlacementObject` con bajo stock y `PackageObject` compatible por producto/storageType.
- Transfiere unidades y aplica velocidad por Cafeína + rol.
- Guardas runtime añadidas para evitar NRE por package vacío/container nulo.

## 11) Sistema de ladrones

- `ShoplifterSystem` marca clientes como ladrones, calcula valores objetivo y resuelve estados.
- `ShoplifterAgent` mantiene estado de robo/captura/escape.
- `RobberyInventoryBridge` reserva y restaura ítems robados contra placements reales.

## 12) Captura manual

- `ShoplifterInteractable` usa acción `Action` (tecla E mapeada en InputActions) y fallback mouse.
- Prompt mostrado: “Presiona E para detener ladrón”.
- Captura requiere rango válido.

## 13) Seguridad automática

- `EntrepreneurTreeSecurityAdapter` recalcula nivel desde nodos de seguridad desbloqueados.
- Probabilidades objetivo: 0 / 0.33 / 0.66 / 0.99.
- Placeholders visuales se crean si faltan referencias.

## 14) Reporte diario

- `StatsDatabase` registra aparición, detección, arrestos, escapes, pérdidas y recuperaciones.
- Incluye snapshots de empleados y seguridad.
- `UIStats` consume snapshot persistido y muestra resumen extendido de robos.

## 15) Guardado/carga

- `SaveGameSystem` guarda/carga base.
- `EntrepreneurTreeSaveIntegration` guarda datos de árbol/empleados/ladrones en archivo adicional `entrepreneurTree.dat`.
- Evita tocar la implementación original del asset base.

## 16) Qué quedó pendiente

Pendiente **obligatorio** en máquina local con Unity:

1. Compilación real del proyecto en Editor.
2. Play Mode completo con consola abierta.
3. Ejecución de casos pendientes de `PHASE4_QA_REPORT.md` (A/B/C/D/G/I/J).
4. Confirmación final de cero errores runtime en flujo normal.

## 17) Qué probar primero en Unity

Orden recomendado:

1. Abrir escena principal y revisar consola.
2. Bucle de computadora (abrir/cerrar + UPGRADES/Expansions/Árbol/App Empleados).
3. Reabasto de surtidores.
4. Flujo de ladrón (captura y escape).
5. Cierre de día + UIStats.
6. Save/Load y verificación de no duplicación de managers.

## 18) Qué NO tocar sin cuidado

- `EntrepreneurTreeUIBootstrap` (inyección runtime en UI legacy).
- `UpgradesUIController` y jerarquía dinámica del árbol.
- `EmployeeAppUIController` y nombres de nodos UI runtime.
- `EntrepreneurTreeSaveIntegration` (compatibilidad de save cruzado).
- IDs de nodos en `EntrepreneurTreeDefinition` / `EntrepreneurTreeData.asset`.

## 19) Mejor punto para continuar después del pull

Continuar desde **validación runtime real en Unity** y actualizar estados `Pendiente` a `OK/Falló` dentro de:

- `Assets/Systems/EntrepreneurTree/PHASE4_QA_REPORT.md`

## 20) Próximo prompt recomendado (siguiente conversación)

> “Continuemos desde `copilot/tree-of-entrepreneur-skills`. Ya existe `PHASE4_QA_REPORT.md` y `HANDOFF_AFTER_PULL.md`. Quiero ejecutar cierre runtime en Unity: revisar consola, correr matriz QA pendiente (A/B/C/D/G/I/J), corregir cualquier error encontrado y dejar PR final lista para merge con evidencias por caso.”

