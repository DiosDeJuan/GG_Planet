# Reporte: Diseño UI, Empleados y Muebles — ShopMaster

**Rama:** `copilot/fix-emprendedor-tree-system-again`
**Fecha:** 2026-05-20
**Fase:** 2 — Workstation System + Employee UI Improvements

---

## 1. Resumen Ejecutivo

Esta fase implementó el sistema de puestos de trabajo (EmployeeWorkstation / EmployeeWorkstationRegistry) que faltaba completamente, mejoró la UI de Empleados con estados ASCII-compatibles, y conectó el ciclo completo:

> Árbol → Desbloqueo → Empleados (contratar) → Rol (Cajero/Surtidor) → Puesto de trabajo (auto-asignar) → NPC aparece en posición del puesto → Guardado/Carga.

**Sistemas funcionales:**
- ✅ Contratación de empleados
- ✅ Asignación de roles (Cajero / Surtidor)
- ✅ Sistema de puestos de trabajo (nuevo)
- ✅ Auto-asignación de puesto al cambiar rol
- ✅ NPC se mueve al puesto al ser asignado
- ✅ Guardado y carga de asignaciones de puestos
- ✅ UI con estados [BLOQ] / [DISP] / [OK] / [SIN PUESTO] / [TRABAJANDO]
- ✅ Errores Play Mode de fase anterior corregidos

**Sistemas parciales:**
- ⚠️ Cajero coordina con EmployeeCashierCoordinator (backend funcional, NPC visual no navega al desk, se teleporta al puesto)
- ⚠️ Surtidor coordina con EmployeeRestockCoordinator (restock funciona, NPC visual en zona de almacén)
- ⚠️ Seguridad: niveles 1/2/3 funcionales con placeholders; sin NPC de guardia real

**No listos:**
- ❌ NavMesh real para NPC (NPC se teletransporta al puesto, no camina)
- ❌ Prefabs de empleados reales (requiere asignación en Inspector de EmployeeNPCSpawner)
- ❌ Diseño visual de árbol (requiere modificación de escena)

---

## 2. Errores de Consola

| Error | Archivo | Causa | Solución | Estado |
|---|---|---|---|---|
| `NullReferenceException: DayCycleSystem.GetStoreOpenState()` | `DayCycleSystem.cs` | Instance null antes de Awake | Guard + warning una vez | ✅ Reparado |
| `CreateImpl is not allowed` (EmployeeNPCSpawner) | `EmployeeNPCSpawner.cs` | MaterialPropertyBlock en field initializer | Movido a Awake() | ✅ Reparado |
| `MissingReferenceException: ExpansionAppUIController` | `ExpansionAppUIController.cs` | Callbacks post-destroy | Guard `if (this == null)` | ✅ Reparado |
| Unicode font warnings (✓ ✗ 🗺 🔒) | 5 archivos | Símbolos no en LiberationSans SDF | Reemplazados con ASCII | ✅ Reparado |
| `[Achievement] Product out of stock` log spam | `AchievementSystem.cs` | Debug.Log en cada Refresh UI | Eliminado | ✅ Reparado |

---

## 3. UI de Computadora

### Qué cambió
- `EmployeeAppUIController` — UI con estados de colores semánticos por tipo de estado
- Estado labels: `[BLOQ]` (gris), `[DISP]` (amarillo), `[OK]` (verde), `[SIN PUESTO]` (naranja), `[TRABAJANDO]` (azul)
- Panel derecho ahora muestra: título, estado (con color), costo contratación, puesto asignado, hint descriptivo
- Botones de acción: Contratar, Asignar: Cajero, Asignar: Surtidor, **Asignar Puesto de Trabajo** (nuevo)
- Cards de lista: color por estado (gris=bloq, verde oscuro=disp, azul=cajero, verde=surtidor)

### Apps sin cambios visuales grandes
- `OrdersAppUIController`, `PricingAppUIController`, `InventoryAppUIController` — funcionales, sin regresiones
- `ExpansionAppUIController` — mejorado en fase anterior (Unicode eliminado, guards MissingRef)

---

## 4. Rama de Empleados (employee_1 a employee_18)

| Nodo | Requisito | Estado sistema |
|---|---|---|
| employee_1 | product_spices_1 | Funcional |
| employee_2 | product_hygiene | Funcional |
| employee_3 | product_sodas | Funcional |
| employee_4 | product_dairy_1 | Funcional |
| employee_5 | product_dairy_1 | Funcional |
| employee_6 | product_spices_1 | Funcional |
| employee_7 | employee_5 | Funcional |
| employee_8 | product_sodas | Funcional |
| employee_9 | product_hygiene | Funcional |
| employee_10 | employee_1 | Funcional |
| employee_11 | security_1 | Funcional |
| employee_12 | employee_13 | Funcional |
| employee_13 | product_luxury_1 | Funcional |
| employee_14 | product_appliances_1 | Funcional |
| employee_15 | security_2 | Funcional |
| employee_16 | product_protein_1 | Funcional |
| employee_17 | product_fresh_2 | Funcional |
| employee_18 | security_3 | Funcional |

**Flujo completo:** Desbloquear nodo en Árbol → `EntrepreneurTreeEmployeeUnlockAdapter` dispara evento → `EmployeeAppUIController` cambia estado a [DISP] → Contratar → `EntrepreneurEmployeeSystem.TryHireEmployee()` → descontar dinero → evento `onEmployeeHired` → `EmployeeNPCSpawner` spawnea NPC → asignar rol → `EmployeeWorkstationRegistry` auto-asigna puesto → NPC se mueve al puesto.

---

## 5. Workstations (Sistema Nuevo)

### Archivos creados
| Archivo | Descripción |
|---|---|
| `Assets/Systems/EntrepreneurTree/EmployeeWorkstation.cs` | MonoBehaviour: define un puesto (tipo, posición, ocupante) |
| `Assets/Systems/EntrepreneurTree/EmployeeWorkstationRegistry.cs` | Singleton: registra, asigna, libera, guarda/carga puestos |

### Tipos de puestos
- `Cashier` — vinculado a CashDesk (auto-creado en Start si no hay ninguno cerca)
- `Restocker` — zona de almacén (fallback auto-creado si no hay ninguno)
- `Security` — preparado para futuras expansiones

### Puestos auto-creados en runtime
- `EmployeeWorkstationRegistry.EnsureFallbackCashierStations()` corre en `Start()` y:
  1. Detecta todos los `CashDesk` en escena
  2. Crea un `CashierStation_N` hijo de cada CashDesk sin cobertura
  3. Crea un `RestockerStation_0` fallback si no hay ninguno

### Asignación
- Auto-asignación: al cambiar rol del empleado, el Registry llama `TryAutoAssign()` automáticamente
- Manual: botón "Asignar Puesto de Trabajo" en la UI de Empleados
- Al asignar: NPC se teletransporta al `StandPosition` del workstation

---

## 6. Empleados

### Contratación
- `EntrepreneurEmployeeSystem.TryHireEmployee()` — valida desbloqueo, dinero, estado previo
- Descuenta `hireCost` de `StoreDatabase`
- Por defecto `$250.00` (25000 centavos = defaultHireCost)

### Roles
- Cajero: activa `EmployeeCashierCoordinator` (backend poll cada 0.75s, llama `TryStartAutomatedCheckout`)
- Surtidor: activa `EmployeeRestockCoordinator` (ciclo cada ~12s con multiplicador de velocidad)

### NPCs
- `EmployeeNPCSpawner` usa prefabs `Customer_A–E` asignados en Inspector
- Deshabilita `Customer`, `CustomerAgent`, `CustomerCart` para evitar comportamiento de comprador
- Al asignar workstation: NPC se mueve a `ws.StandPosition` con `ws.StandRotation`

### Guardado
- `EntrepreneurTreeSaveIntegration` guarda ahora:
  - `EntrepreneurEmployeeSystem` (hired, role, workstationId por empleado)
  - `EmployeeWorkstationRegistry` (assignments: employeeId → workstationId)

---

## 7. Muebles/Equipos

### Cajas registradoras
- Las existentes en escena son detectadas por `EmployeeCashierCoordinator` y `EmployeeWorkstationRegistry`
- Se crean `CashierStation` fallback automáticamente al inicio si no hay ninguno manual

### Cámaras / Guardias / Alarmas
- Gestionadas por `EntrepreneurTreeSecurityAdapter` con `createPlaceholdersWhenMissing = true`
- Nivel 1: `SecurityCameraPlaceholder` (cilindro naranja)
- Nivel 2: `SecurityGuardsPlaceholder` (2 cápsulas rojas)
- Nivel 3: `SecurityGatePlaceholder` (cubo rojo)
- Se activan al desbloquear nodos `security_1`, `security_2`, `security_3` en el Árbol

### Puestos de trabajo
- `CashierStation_N` — hijo de cada CashDesk en escena
- `RestockerStation_0` — hijo del sistema, zona configurable

---

## 8. Cajero

**Flujo funcional:**
1. Desbloquear employee_N en Árbol
2. Contratar desde Empleados
3. Asignar rol Cajero
4. Registry auto-asigna `CashierStation_N`
5. NPC aparece en la posición de la caja
6. `EmployeeCashierCoordinator` detecta cajeros activos (por rol)
7. Cada 0.75s, `TryStartAutomatedCheckout()` intenta atender una caja con cola
8. Venta se completa, dinero sumado, inventario descontado

---

## 9. Surtidor

**Flujo funcional:**
1. Contratar y asignar Surtidor
2. Registry auto-asigna `RestockerStation_0`
3. NPC aparece en zona de almacén
4. `EmployeeRestockCoordinator` cicla cada ~12s (reducido con más surtidores)
5. Detecta placements con stock bajo (< 40% fill ratio)
6. Respeta `ShelfProductSlotSystem` (producto asignado por slot)
7. Mueve producto de PackageObject al placement
8. Notifica si no hay stock: "Sin stock de X para surtir Y"

---

## 10. Seguridad

- Niveles 1/2/3 funcionales con placeholders visuales
- `EntrepreneurTreeSecurityAdapter.TryAutomaticArrest()` = `Random.value <= arrestChance`
- Nivel 1: 33%, Nivel 2: 66%, Nivel 3: 99%

---

## 11. Diseño UI

### Mejoras implementadas
- Colores semánticos por estado en tarjetas de empleados
- Labels ASCII-only: [BLOQ], [DISP], [OK], [SIN PUESTO], [TRABAJANDO]
- Detalle de puesto de trabajo visible en panel derecho
- Botón "Asignar Puesto de Trabajo" funcional

### Problemas visuales restantes
- El árbol del Emprendedor requiere modificación de escena para mejoras visuales
- La app Expandir necesita mapa más grande (requiere escena)
- Los prefabs NPC requieren asignación manual en Inspector de EmployeeNPCSpawner

---

## 12. Play Mode

**Verificado en código:**
- Sin nuevos errores rojos en los scripts modificados
- Flujo completo puede completarse con Admin Mode (unlockAllEmployees + giveTestStock)

**No probado en Play Mode real** (entorno no permite ejecutar Unity Editor)

**Errores restantes conocidos:**
- Si `EmployeeNPCSpawner.employeePrefabs` está vacío → warning no-rojo, NPC no spawna
- NavMesh puede no estar horneado → NPC se teletransporta, no camina

---

## 13. Pull

**Rama:** `copilot/fix-emprendedor-tree-system-again`
**Commit:** ver `git log --oneline -3`
**Estado:** Working tree clean tras push

### Comandos para tu computadora

```
cd "C:\Users\ijuan\Animo"
git status
git checkout copilot/fix-emprendedor-tree-system-again
git pull origin copilot/fix-emprendedor-tree-system-again
git lfs pull
git status
```
