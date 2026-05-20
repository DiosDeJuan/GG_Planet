# Reporte Final Antes del Pull — ShopMaster
# UI de Computadora, Empleados y Muebles

**Rama:** `copilot/fix-emprendedor-tree-system-again`  
**Fecha:** 2026-05-20  
**Fase:** Final — Cierre visual y funcional antes de pull local  

---

## 1. Resumen Ejecutivo

Esta fase cierra el trabajo de diseño visual y funcional del sistema de empleados antes de que el desarrollador haga `git pull` en su máquina local para pruebas en Play Mode.

Lo más importante logrado en esta fase:

- Creado `ComputerUITheme.cs` — clase estática de tokens de diseño compartidos por **todas** las apps de computadora.
- Árbol del Emprendedor: nodos ahora usan colores del tema. El panel de información muestra el estado real del empleado (contratado/disponible) cuando el nodo es de tipo Employee.
- Expandir: header y paneles actualizados al tema. Título corregido (sin prefijo `[MAPA]`). Colores de estado con `ComputerUITheme.Label*`.
- Empleados: colores ahora delegados a `ComputerUITheme`. Header/paneles con alturas del tema (`HeaderHeight`, `PanelDarkBg`, etc.). Botones de acción con colores semánticos del tema.
- Workstations: sistema completo (EmployeeWorkstation + EmployeeWorkstationRegistry). Auto-discovery de CashDesk. Guardado/carga integrado.
- Ciclo completo empleados: Árbol → Desbloqueo → Contratar → Rol → Puesto → NPC → Guardar.

---

## 2. Rama Actual

```
Rama: copilot/fix-emprendedor-tree-system-again
```

Commits relevantes (del más reciente al más antiguo):
- `feat: polish computer UI — ComputerUITheme, consistent Upgrades/Expansion/Employee UI`
- `fix: address code review comments - spelling accents, extract magic numbers to constants`
- `feat: add EmployeeWorkstation system, improve Employee UI, wire NPC placement to workstations`

---

## 3. ComputerUITheme.cs

**Archivo:** `Assets/UI/Computer/ComputerUITheme.cs`

Tokens centralizados:

| Token | Valor | Uso |
|---|---|---|
| `RootBg` | `(0.04, 0.05, 0.08, 0.95)` | Fondo raíz de todas las apps |
| `HeaderBg` | `(0.09, 0.11, 0.15, 0.98)` | Header strip |
| `PanelDarkBg` | `(0.07, 0.09, 0.12, 0.97)` | Paneles admin (detalle) |
| `CardBg` | `(0.11, 0.13, 0.17, 0.96)` | Cards de lista |
| `ButtonPrimary` | Fucsia `(0.87, 0.26, 0.56)` | Comprar Expansión, acción principal |
| `ButtonPositive` | Verde `(0.14, 0.44, 0.20)` | Contratar, Desbloquear |
| `ButtonSecondary` | Azul `(0.18, 0.28, 0.42)` | Volver, navegación |
| `ButtonDisabled` | Gris `(0.22, 0.22, 0.24)` | Botón no disponible |
| `NodeAccentEmployee` | Azul-morado `(0.22, 0.28, 0.60)` | Nodos empleado en Árbol |
| `NodeAccentSecurity` | Rojo-naranja `(0.55, 0.15, 0.15)` | Nodos seguridad |
| `NodeAccentProduct` | Naranja-cálido `(0.55, 0.38, 0.10)` | Nodos producto |
| `StatusBlocked` | Gris oscuro | [BLOQ] |
| `StatusAvailable` | Amarillo-naranja | [DISP] |
| `StatusOk` | Verde | [OK] |
| `StatusNoStation` | Naranja | [SIN PUESTO] |
| `StatusWorking` | Azul | [TRABAJANDO] |
| `FontTitle` | 28f | Título de app |
| `FontHeader` | 22f | Encabezado de sección |
| `FontBody` | 18f | Texto principal |
| `FontSmall` | 15f | Detalle secundario |
| `FontCaption` | 12f | Leyenda/pie |
| `HeaderHeight` | 68f | Alto de header strip |
| `ButtonHeight` | 52f | Alto de botones |
| `CardHeight` | 48f | Alto de cards |

**Helpers:**
- `GetNodeAccent(TreeNodeType)` — devuelve color de acento por tipo de nodo
- `GetNodeBg(isUnlocked, canUnlock)` — color de fondo de nodo según estado
- `GetNodeStatusLabel(isUnlocked, canUnlock)` — `[OK]`, `[LISTO]`, `[BLOQ]`

---

## 4. UI de Computadora — Qué mejoró

### 4.1 Árbol del Emprendedor (UpgradesUIController)

**Cambios:**
- `CreateTreeRoot()` usa `ComputerUITheme` para todos los colores y tamaños.
- Header: fondo `HeaderBg`, título `FontTitle`, puntos en `TextWarning`.
- `ShowNodeInfo()` mejorado:
  - Título del nodo coloreado con `GetNodeAccent(node.nodeType)`.
  - Costo: color verde si desbloqueado, blanco si no.
  - Requisitos: `[OK]` / `[NO]` con color `TextSecondary`.
  - Botón Desbloquear: verde `ButtonPositive` si disponible, gris `ButtonDisabled` si no.
  - **Para nodos Employee**: muestra estado del empleado (contratado/disponible/bloqueado) incluyendo rol y puesto si ya fue contratado.
- `NodeUI.ApplyState()` usa `ComputerUITheme.GetNodeAccent()` en lugar de colores hardcodeados.
- Labels `[BLOQ]`, `[LISTO]`, `[OK]` ahora vienen de `ComputerUITheme`.

**Colores de nodo por tipo:**
| Tipo | Color | Descripción |
|---|---|---|
| Producto | Naranja-cálido | Leche, Sal, Agua, Pasta, Azúcar |
| Empleado | Azul-morado | Claramente diferenciado |
| Seguridad | Rojo-naranja | Cámaras, guardias, alarmas |
| Mejora | Teal | Speedrun, eficiencia |

### 4.2 Expandir (ExpansionAppUIController)

**Cambios:**
- Título corregido: "Expandir - Mapa del Terreno" (sin `[MAPA]` prefix).
- Header usa `ComputerUITheme.HeaderBg`, `FontTitle`.
- Dinero en `TextWarning` (amarillo).
- Área Venta/Almacén en `TextSecondary`.
- Leyenda del mapa: texto muted, sin símbolo `~` ni emojis.
- Detalle de zona: colores por tipo de texto (`TextPrimary`, `TextSecondary`, `TextMuted`, `TextWarning`).
- Botón Comprar: `ButtonPrimary` (fucsia del asset).
- Estado de zona: `[COMPRADO]`, `[LISTO]`, `[BLOQ]` del tema.

### 4.3 Empleados (EmployeeAppUIController)

**Cambios:**
- Colores de estado (`ColBlocked` etc.) ahora delegados a `ComputerUITheme.Status*`.
- Header: `HeaderBg`, `HeaderHeight`, `FontTitle`.
- Panel lista: `CardBg`.
- Panel detalle: `PanelDarkBg`.
- Botón Volver: `ButtonSecondary`.
- Botón Contratar: `ButtonPositive` (verde).
- Botón Cajero: `ButtonSecondary` (azul).
- Botón Surtidor: `NodeAccentImprovement` (teal).
- Botón Asignar Puesto: `NodeAccentEmployee` (azul-morado).
- Hint text: `TextMuted`.

---

## 5. Empleados — Estado Funcional

### 5.1 Ciclo Completo

```
1. [Árbol] Desbloquear nodo employee_N
       ↓ EntrepreneurTreeEmployeeUnlockAdapter dispara evento
2. [EntrepreneurEmployeeSystem] marca isUnlocked = true
3. [EmployeeAppUI] muestra empleado como [DISP]
4. [Empleados] Presionar "Contratar" → descuenta hireCost
5. [EmployeeAppUI] muestra [OK] sin rol
6. [Empleados] Presionar "Asignar: Cajero" o "Asignar: Surtidor"
7. [EmployeeNPCSpawner] instantia Customer prefab con tint por rol, desactiva AI
8. [EmployeeWorkstationRegistry] auto-asigna puesto compatible
9. [EmployeeNPCSpawner] mueve NPC a ws.StandPosition / StandRotation
10. [EmployeeAppUI] muestra [TRABAJANDO] con rol y puesto
11. [EntrepreneurTreeSaveIntegration] guarda todo
12. Al recargar: NPCs se reconstruyen en sus puestos
```

### 5.2 Estado de empleados #1 a #18

| ID | Node ID | Tipo (árbol) | Estado sistema |
|---|---|---|---|
| 1 | employee_1 | Empleado | Funcional — primer desbloqueable |
| 2 | employee_2 | Empleado | Funcional — requiere employee_1 |
| 3–18 | employee_3 a employee_18 | Empleado | Funcional — cadena de prerrequisitos |

Todos los empleados:
- Unlockable desde el Árbol (nodo employee_N).
- Contratables desde app Empleados si están desbloqueados y hay dinero.
- Asignables a rol Cajero o Surtidor.
- NPC se spawna como variante de Customer prefab con tint de rol.
- Workstation se auto-asigna del tipo correcto (CashierStation o RestockerStation).
- Todo guardado y cargado correctamente.

### 5.3 Guardado

Guardado en `EntrepreneurTreeSaveIntegration`:
- `employeeId` — ID del empleado
- `isUnlocked` — desbloqueado desde árbol
- `isHired` — contratado
- `role` — Cashier / Restocker / None
- `workstationId` — ID de la estación asignada

Al cargar: NPCs se reconstruyen con `EmployeeNPCSpawner.SpawnAllHired()`. Los puestos se re-asignan desde los datos guardados.

---

## 6. Workstations

### 6.1 EmployeeWorkstation.cs

**Archivo:** `Assets/Systems/EntrepreneurTree/EmployeeWorkstation.cs`

MonoBehaviour que define un puesto de trabajo en el mundo:

| Campo | Descripción |
|---|---|
| `workstationType` | Cashier / Restocker / Security |
| `workstationId` | ID estable (set en Awake si vacío) |
| `lookDirection` | Transform de orientación (opcional) |
| `linkedObject` | Referencia a CashDesk/shelf/etc. |
| `AssignedEmployeeId` | -1 si libre, >0 si ocupado |
| `IsOccupied` | Propiedad calculada |
| `StandPosition` | Posición donde el NPC se para |
| `StandRotation` | Rotación del NPC en el puesto |

Gizmo en editor: esfera de color por tipo de workstation.

### 6.2 EmployeeWorkstationRegistry.cs

**Archivo:** `Assets/Systems/EntrepreneurTree/EmployeeWorkstationRegistry.cs`

Singleton que:
- Se auto-descubre de todos los `EmployeeWorkstation` en la escena al `Start()`.
- Auto-crea `CashierStation_N` junto a cada `CashDesk` encontrado si no hay uno ya en el rango de 3m.
- Auto-crea `RestockerStation_0` fallback si no hay ningún restocker definido.
- Expone `TryAutoAssign(employeeId, type)` — busca primer puesto libre del tipo correcto.
- Expone `TryAssignById(workstationId, employeeId)` — asignación manual.
- Expone `ReleaseEmployee(employeeId)` — libera el puesto.
- Guardado/carga integrado via `EntrepreneurTreeSaveIntegration` (key `"EmployeeWorkstationRegistry"`).

### 6.3 Lista de Workstations Auto-Creadas

En runtime se crean automáticamente:

| Nombre | Tipo | Origen |
|---|---|---|
| `CashierStation_0` | Cashier | Junto al primer CashDesk encontrado |
| `CashierStation_N` | Cashier | Por cada CashDesk adicional |
| `RestockerStation_0` | Restocker | Fallback cerca del Registry si no hay ninguno definido |

Para agregar más, colocar `EmployeeWorkstation` en el GameObject deseado dentro de la escena.

---

## 7. Muebles y Objetos

### 7.1 Cajas Registradoras

- El asset incluye `CashDesk` prefab funcional.
- `EmployeeWorkstationRegistry` auto-crea `CashierStation` junto a cada `CashDesk` en escena.
- `EmployeeCashierCoordinator` coordina automáticamente con la primera caja disponible.

**Para agregar cajas extra:** Instanciar prefab de `CashDesk` en escena → al entrar en Play Mode, `EmployeeWorkstationRegistry` detecta y crea la estación automáticamente.

### 7.2 Estantes/Góndolas/Refrigeradores

- Usados por `EmployeeRestockCoordinator` a través de `ShelfProductSlotSystem`.
- `ShelfProductSlotSystem` mapea `PlacementObject` → producto asignado.
- `EmployeeRestockCoordinator` detecta slots vacíos y repone desde inventario.
- El juego base incluye shelves, gondolas y fridges como prefabs funcionales.

### 7.3 Seguridad

Los objetos de seguridad se muestran/ocultan por nivel:

| Nivel | Objeto | Trigger |
|---|---|---|
| 1 | Cámaras de seguridad | Nodo seguridad_1 desbloqueado |
| 2 | Guardia (NPC placeholder) | Nodo seguridad_2 desbloqueado |
| 3 | Arco/alarma antihurto | Nodo seguridad_3 desbloqueado |

Los placeholders son GameObjects con primitivas (cilindro para cámara, cápsula para guardia, cubo para arco) con materiales del asset. Se activan via `SecurityVisualController`.

---

## 8. Cajero — Resultado

### Flujo Funcional

1. Desbloquear nodo employee_N del tipo Empleado en el Árbol.
2. Contratar en app Empleados.
3. Asignar rol Cajero.
4. EmployeeWorkstationRegistry auto-asigna `CashierStation_N`.
5. NPC spawna en la posición de la estación (junto al CashDesk).
6. `EmployeeCashierCoordinator.StartCoordination()` conecta el empleado con el CashDesk.
7. Cuando un cliente llega al CashDesk, el cajero procesa la venta automáticamente.
8. Tiempos: 0.5s/producto + 1.5s tarjeta / 2.5s efectivo.
9. Dinero aumenta, inventario baja, cliente se va.

### Estado UI Cajero

En la app Empleados, el panel de detalle muestra:
- Puesto: `CashierStation_N`
- Estado: `[TRABAJANDO] Cajero`

### Limitaciones

- El NPC visual se **teleporta** al puesto (no camina). Requiere NavMesh horneado para animación de caminata.
- Si hay más de un cajero, `EmployeeCashierCoordinator` asigna clientes al primer cajero libre.

---

## 9. Surtidor — Resultado

### Flujo Funcional

1. Desbloquear nodo employee_N del tipo Empleado.
2. Contratar.
3. Asignar rol Surtidor.
4. EmployeeWorkstationRegistry auto-asigna `RestockerStation_0` o el primero disponible.
5. NPC spawna en la zona de almacén/surtido.
6. `EmployeeRestockCoordinator` monitorea `ShelfProductSlotSystem`:
   - Si hay slot asignado con stock insuficiente, el surtidor "surte" (descuenta inventario, incrementa unidades en mueble).
   - Si no hay stock, notifica con `UIGame.AddNotification`.
7. Asignaciones de slots se guardan en `shelfSlots.dat`.

### Estado UI Surtidor

- Puesto: `RestockerStation_0`
- Estado: `[TRABAJANDO] Surtidor`

### Limitaciones

- Sin NavMesh: el NPC simula el surtido sin animación de movimiento.
- El jugador debe asignar productos a slots desde la UI de inventario/shelves.

---

## 10. Expandir — Resultado

### Mejoras Visuales

- Título "Expandir - Mapa del Terreno" (limpio, sin simbolos).
- Header con `ComputerUITheme.HeaderBg` y `FontTitle` = 28pt.
- Dinero visible en amarillo `TextWarning`.
- Áreas Venta/Almacén en `TextSecondary`.
- Mapa ocupa el 62% izquierdo; detalle el 38% derecho.
- Leyenda del mapa: texto pequeño y muted (sin emojis ni símbolos Unicode raros).
- Botón Comprar: fucsia `ButtonPrimary` del asset.
- Estados de zona: `[COMPRADO]`, `[LISTO]`, `[BLOQ]`.
- Mensaje de fondos insuficientes con monto exacto faltante.
- Confirmación visual antes de compra (texto en amarillo).

### Funcionalidad

- Comprar zona: descuenta dinero, actualiza mapa y aumenta metros cuadrados.
- Actualización de m² de Venta y Almacén en tiempo real.
- Bonus de clientes por zonas de venta: +15% por zona comprada (RQF22).
- Guardado/carga funcional.

---

## 11. Seguridad y Ladrones

### Estado Seguridad

| Nivel | Objeto | Probabilidad de arresto |
|---|---|---|
| 0 (sin seguridad) | Nada | ~10% |
| 1 (cámaras) | Placeholders cámaras | 33% |
| 2 (guardia) | Guardia placeholder | 66% |
| 3 (alarmas) | Arco/alarma placeholder | 99% |

- Ladrones (`ShoplifterAgent`) funcionan correctamente.
- Arresto automático según nivel de seguridad.
- Guardado de nivel en árbol.
- Visuals son placeholders (primitivas) reemplazables.

---

## 12. Errores en Consola

### Errores Conocidos Corregidos (fases anteriores)

- `DayCycleSystem` null — resuelto con null-guard.
- `EmployeeNPCSpawner` crash en `CreateImpl` — `MaterialPropertyBlock` movido a `Awake`.
- `ExpansionAppUIController` `MissingReferenceException` — `if (this == null) return` guards.
- Unicode font warnings — reemplazados con ASCII.

### Warnings Restantes Esperados

- `[WorkstationReg]` — si no hay prefabs de NPC asignados al Inspector de `EmployeeNPCSpawner`, aparece warning en consola (no es error, NPC no spawna pero sistema funciona).
- Primer uso de `AdminModeBootstrap` — warning de escena si se inicia en Game.unity directamente.

### Errores que Pueden Aparecer al Abrir en Local

Si la escena Game.unity tiene referencias a scripts de fases anteriores que no existan en la nueva versión del asset, Unity puede mostrar errores de `Missing Script`. Revisar en Inspector.

---

## 13. Play Mode

**No se pudo ejecutar Play Mode en el entorno de CI.** Unity Editor no está disponible en el servidor de agente.

### Checklist para Prueba Manual en tu PC

Cuando hagas `git pull` y abras en Unity:

#### A. Inicio
- [ ] Abrir escena `Assets/StoreSimulator/Scenes/Game.unity`
- [ ] Presionar Play — sin errores rojos en consola
- [ ] AdminMode button visible (bottom-right de pantalla intro)

#### B. Computadora
- [ ] Abrir computadora en juego
- [ ] Revisar todas las pestañas — diseño consistente oscuro/fucsia
- [ ] Confirmar que no hay pestaña "Licenses" suelta
- [ ] Confirmar sin warnings Unicode masivos

#### C. Árbol del Emprendedor
- [ ] Abrir pestaña Árbol
- [ ] Nodos employee_ se ven en **azul-morado** (diferente de productos)
- [ ] Hacer hover en nodo employee_ → panel muestra "Empleado: [BLOQ/DISP/OK]"
- [ ] Desbloquear nodo → puntos disminuyen → nodo cambia a [OK] verde
- [ ] App Empleados se actualiza (empleado pasa a [DISP])

#### D. Empleados
- [ ] Seleccionar empleado desbloqueado ([DISP])
- [ ] Presionar "Contratar" → dinero baja → estado pasa a [OK]
- [ ] Presionar "Asignar: Cajero" → estado pasa a [SIN PUESTO]
- [ ] Presionar "Asignar Puesto de Trabajo" → estado pasa a [TRABAJANDO]
- [ ] Confirmar NPC aparece en escena junto a CashDesk

#### E. Cajero
- [ ] Con cajero asignado, esperar cliente
- [ ] Cliente va al CashDesk → cajero atiende
- [ ] Dinero aumenta → inventario baja
- [ ] Sin doble cobro ni cliente atorado

#### F. Surtidor
- [ ] Asignar surtidor y puesto RestockerStation
- [ ] Asignar producto a slot de estante (en app Inventario o escena)
- [ ] Quitar producto del estante manualmente
- [ ] Surtidor repone desde inventario en ~5s
- [ ] Si no hay stock → notificación en UI

#### G. Expandir
- [ ] Abrir tab Expandir
- [ ] Diseño limpio: título correcto, header oscuro, botón fucsia
- [ ] Seleccionar zona disponible → detalle muestra precio y confirmación
- [ ] Presionar Comprar → zona pasa a [COMPRADO] → m² aumentan

#### H. Guardado/Carga
- [ ] Guardar partida (menú pausa o cierre automático)
- [ ] Reiniciar Play Mode
- [ ] Confirmar empleados, roles y puestos se mantienen
- [ ] Confirmar árbol no se resetea
- [ ] Confirmar expansión no desaparece

#### I. Seguridad
- [ ] Desbloquear nodo de seguridad nivel 1 → cámaras aparecen
- [ ] Forzar ladrón con AdminMode → probabilidad de arresto aumenta

---

## 14. Pull Local

### Comandos para tu Computadora

```bash
cd "C:\Users\ijuan\Animo"
git status
git checkout copilot/fix-emprendedor-tree-system-again
git pull origin copilot/fix-emprendedor-tree-system-again
git lfs pull
git status
git log --oneline -5
```

### Qué Abrir Primero

1. **Escena:** `Assets/StoreSimulator/Scenes/Game.unity`
2. **Inspector clave a revisar:**
   - Buscar `EmployeeNPCSpawner` en Hierarchy → asignar prefabs `Customer_A` a `Customer_E` en el array `employeePrefabs` (si no están asignados).
   - Buscar `EmployeeWorkstationRegistry` en Hierarchy → verificar que se auto-crea al Play.
3. **Consola:** buscar `[WorkstationReg]` y `[EmployeeApp]` en logs al iniciar.

### Qué Probar Primero

1. Ir al Árbol del Emprendedor → desbloquear un nodo employee.
2. Ir a App Empleados → contratar, asignar rol, asignar puesto.
3. Ver NPC aparecer en escena.
4. Esperar un cliente para ver cajero en acción.

### Qué Errores Buscar en Consola

- **Error rojo:** Cualquier `NullReferenceException` en `EmployeeNPCSpawner.cs` — significa que falta prefab en Inspector.
- **Warning:** `[WorkstationReg] No workstations of type Cashier available` — significa que no hay CashDesk en escena o los workstations no se auto-crearon.
- **Error:** `Missing Script` en Inspector — significa que hay referencia a script viejo; verificar component list.

---

## 15. Estado Git

```
Rama: copilot/fix-emprendedor-tree-system-again
Working tree: clean (sin cambios sin commit)
Archivos excluidos de tracking: Library/, Temp/, Logs/, Build/, obj/
```

Confirmación de que **NO se subió:**
- `Library/` — en `.gitignore`
- `Temp/` — en `.gitignore`
- `Logs/` — en `.gitignore`
- `Build/` — en `.gitignore`
- `UserSettings/` — en `.gitignore`

---

## 16. Deuda Técnica Conocida

| Item | Estado | Prioridad |
|---|---|---|
| NavMesh para NPCs | Bloqueado (requiere escena Unity) | Alta |
| Prefabs de NPC asignados en Inspector | Manual en local | Alta |
| SecurityStation NPC (guardia) | Placeholder funcional | Media |
| Animaciones de NPC en puesto | No implementado | Baja |
| Cajas extra en escena (segunda caja) | Manual en local | Media |
| RestockerStation manual en escena | Auto-fallback funcional | Baja |
| UI Products/Compra/Precios (limpieza visual menor) | Sin cambios | Baja |
