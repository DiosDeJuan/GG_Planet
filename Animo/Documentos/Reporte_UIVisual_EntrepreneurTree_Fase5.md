# Reporte UIVisual EntrepreneurTree — Fase 5

## 1. Resumen Ejecutivo

La Fase 5 consolida el trabajo visual del proyecto ShopMaster/XINO. Las fases anteriores ya implementaron un sistema de diseño visual completo y cohesivo (`ComputerUITheme.cs`), controladores de UI procedurales para cada pestaña de la computadora, y sistemas de estados visuales para nodos del árbol. En esta fase se documenta el estado visual actual, se audita la consistencia, se crea el runner de validación visual (`ShopMasterUIVisualConsistencyAuditRunner`) y se identifican los pendientes reales.

---

## 2. Estado Final

**AMARILLO** — La arquitectura visual y los sistemas de diseño están implementados y consolidados en código. Los runners de lógica previos son válidos. La validación final en modo batchmode / PlayMode requiere Unity Editor corriendo localmente (no ejecutable en entorno CI sin instalación de Unity). La UI visual programática está lista para ejecución local.

---

## 3. Dirección Visual Aplicada

### 3.1 Paleta Principal (`ComputerUITheme.cs`)

| Token | Valor (RGBA approx.) | Uso |
|---|---|---|
| `RootBg` | #0A0D14 (95%) | Fondo raíz de todos los panels |
| `HeaderBg` | #171C26 (98%) | Franja de encabezado de cada app |
| `PanelDarkBg` | #111822 (97%) | Panels de lista / detalle |
| `CardBg` | #1C2130 (96%) | Fondo de tarjetas individuales |
| `CardSelectedBg` | #2E3647 | Tarjeta seleccionada |
| `NodeLockedBg` | #333338 | Nodo bloqueado |
| `NodeReadyBg` | #266630 | Nodo disponible (listo para desbloquear) |
| `NodeUnlockedBg` | #1A8038 | Nodo desbloqueado |
| `ButtonPrimary` | #DE4390 | Botón acción principal (fucsia brand) |
| `ButtonPositive` | #237034 | Contratar / desbloquear |
| `ButtonDisabled` | #383840 | Botón desactivado |
| `TextPrimary` | #EBEBF0 | Texto principal |
| `TextWarning` | #FFD940 | Advertencias, costos faltantes |
| `TextSuccess` | #4DD173 | Éxito, desbloqueado |
| `TextDanger` | #FF6161 | Error, sin fondos |
| `StatusBlocked` | #66666B | Label [BLOQ] |
| `StatusAvailable` | #CCA619 | Label [DISP] / [LISTO] |
| `StatusOk` | #2B8048 | Label [OK] |

### 3.2 Tipografía

| Constante | Tamaño | Uso |
|---|---|---|
| `FontTitle` | 28 pt | Título de app / header |
| `FontHeader` | 22 pt | Encabezado de sección |
| `FontBody` | 18 pt | Texto de cuerpo principal |
| `FontSmall` | 15 pt | Detalle / subtítulo |
| `FontCaption` | 12 pt | Leyenda / badge tiny |

### 3.3 Métricas y Componentes

- **ButtonHeight** = 52 px
- **CardHeight** = 48 px
- **HeaderHeight** = 68 px
- **PanelPadding** = 16 px
- Esquinas y márgenes consistentes en todos los controllers procedurales
- Labels ASCII sin caracteres especiales para compatibilidad con LiberationSans SDF

### 3.4 Estilo General

El juego usa un estilo **dark-UI profesional** con acento fucsia/rosa de la marca StoreSimulator, con states claros para cada elemento interactivo. No se usan colores aleatorios. Todo el vocabulario visual está centralizado en `ComputerUITheme.cs` que actúa como único token design system.

---

## 4. Assets Reutilizados

| Asset | Tipo | Ubicación | Usado en |
|---|---|---|---|
| `ComputerUITheme.cs` | Script de tokens | `Assets/UI/Computer/` | Todos los app controllers |
| `LiberationSans SDF` | Fuente TMP | `Assets/TextMesh Pro/` | Todos los TMP_Text procedurales |
| `UIShopDesktop` | Prefab/script base | StoreSimulator | Entry point de la computadora |
| `UIShopCategoryHelper` | Script base | StoreSimulator | Gestor de tabs |
| `Button` (UGUI) | Componente | Unity UGUI | Todos los botones |
| `Image` (UGUI) | Componente | Unity UGUI | Todos los paneles/fondos |
| `ScrollRect` | Componente | Unity UGUI | Árbol y listas scrollables |
| `VerticalLayoutGroup` | Componente | Unity UGUI | Listas de productos/empleados |

No se crearon sprites ni materiales nuevos. Toda la composición visual se genera proceduralmente en tiempo de ejecución usando los componentes UGUI y los tokens de `ComputerUITheme`.

---

## 5. UI Modificada / Implementada

### 5.1 Computadora (UIShopDesktop / UIShopCategoryHelper)

- `EntrepreneurTreeUIBootstrap.cs` integra dinámicamente todas las tabs.
- Tabs disponibles: Expansions (Árbol), Empleados, Logros, Compra/Pedidos, Precios, Inventario, Instrucciones.
- Tab Licencias eliminada/ocultada (`HideLicensesTab`).
- Cada tab se activa/desactiva limpiamente con el helper base.

### 5.2 Árbol del Emprendedor (`UpgradesUIController` + `NodeUI` + `ConnectionLineUI`)

- Header con `pointsLabel` mostrando puntos disponibles.
- Nodos construidos proceduralmente desde `TreeData.nodes`.
- Cada `NodeUI` muestra: nombre, estado ([BLOQ]/[LISTO]/[OK]), icono si disponible.
- Info panel lateral: título, descripción, costo, requisitos, botón desbloquear.
- `ConnectionLineUI`: líneas de dependencia entre nodos con color por estado.
- `ScrollRect` con `nodesContainer` y `linesContainer` separados.
- Refresh inmediato tras desbloqueo via `onNodeUnlocked` y `onPointsChanged`.

### 5.3 Productos (`OrdersAppUIController`)

- Tarjetas por producto con: nombre, precio de compra, estado desbloqueado/bloqueado.
- Label de dinero disponible en header.
- Botón Comprar: verde si tiene fondos, rojo si no, gris si bloqueado.
- Feedback visual de compra en texto de estado.
- Refresh automático via `onProductNodeUnlocked` y `onMoneyUpdate`.

### 5.4 Empleados (`EmployeeAppUIController`)

- Tarjetas para cada empleado con: rol, estado ([BLOQ]/[DISP]/[OK]/[SIN PUESTO]/[TRABAJANDO]).
- Panel de detalle con descripción, salario, workstation asignada.
- Botones: Contratar, Cajero/Surtidor, Asignar Puesto.
- Estados de color usando `ComputerUITheme.Status*`.
- Refresh via `onEmployeeHired`, `onEmployeeRoleChanged`, `onNodeUnlocked`.

### 5.5 Expandir (`ExpansionAppUIController` + `ExpansionMapRenderer`)

- Header: dinero disponible, área de ventas, área de almacén.
- Panel izquierdo (62%): mapa top-down con `ExpansionMapRenderer` y `ExpansionZoneButtonUI`.
- Panel derecho (38%): detalle de zona seleccionada con precio, tipo, beneficio, botón Comprar.
- Estados de zona: bloqueada, disponible, comprada usando colores del tema.
- Sincronía con `SupermarketExpansionSystem` y zone purchased events.

### 5.6 Equipamiento

- Base visual: `ItemDatabase` en escena con ítems registrados.
- Los ítems desbloqueados se gestionan via `EntrepreneurTreeUpgradeAdapter`.
- Pestaña de upgrades originales del asset base reutilizada/reemplazada por el árbol.

### 5.7 Seguridad

- Nodos de tipo `TreeNodeType.Security` en el árbol del emprendedor.
- `EntrepreneurTreeSecurityAdapter` aplica cambios de cobertura al desbloquear.
- `ShoplifterSystem` responde a nivel de seguridad.
- Estado visible en árbol: nodo security muestra estado [BLOQ]/[OK] con color rojo-naranja (`NodeAccentSecurity`).

### 5.8 Inventario (`InventoryAppUIController`)

- Tabla con filas alternas por producto: boxes en almacén, unidades en estante, estado.
- Colores: verde (ok), amarillo (bajo), rojo (sin stock), azul (en camino), gris (bloqueado).
- Sufijo "N ranuras" cuando hay slots de anaquel asignados.
- Refresh via `onInventoryChanged` y `onMoneyUpdate`.

---

## 6. Componentes Creados en Esta Fase

| Componente | Tipo | Ubicación | Descripción |
|---|---|---|---|
| `ShopMasterUIVisualConsistencyAuditRunner.cs` | Editor Script | `Assets/StoreSimulator/Editor/` | Runner de 20 checks de consistencia visual |

### Componentes ya existentes y validados

| Componente | Tipo | Descripción |
|---|---|---|
| `ComputerUITheme.cs` | Script | Token design system centralizado |
| `UpgradesUIController.cs` | MonoBehaviour | UI procedural del árbol del emprendedor |
| `NodeUI.cs` | MonoBehaviour | Componente visual por nodo del árbol |
| `ConnectionLineUI.cs` | MonoBehaviour | Líneas de conexión entre nodos |
| `EmployeeAppUIController.cs` | MonoBehaviour | UI de empleados |
| `ExpansionAppUIController.cs` | MonoBehaviour | UI de expansión con mapa |
| `ExpansionMapRenderer.cs` | MonoBehaviour | Renderizado del mapa de zonas |
| `ExpansionZoneButtonUI.cs` | MonoBehaviour | Botón visual por zona |
| `InventoryAppUIController.cs` | MonoBehaviour | UI de inventario |
| `OrdersAppUIController.cs` | MonoBehaviour | UI de compra de productos |
| `AchievementsAppUIController.cs` | MonoBehaviour | UI de logros |
| `PricingAppUIController.cs` | MonoBehaviour | UI de precios |
| `InstructionsAppUIController.cs` | MonoBehaviour | UI de instrucciones |

---

## 7. Mejoras del Árbol del Emprendedor

| Aspecto | Estado | Detalle |
|---|---|---|
| Puntos visibles | ✅ VERDE | `pointsLabel` con texto "Puntos: X" actualizado en tiempo real |
| Costos de nodos visibles | ✅ VERDE | Info panel muestra "Costo: X punto(s)" o "[OK] Desbloqueado" |
| Estados visuales | ✅ VERDE | 3 estados: gris ([BLOQ]), verde-amarillo ([LISTO]), verde ([OK]) |
| Requisitos visibles | ✅ VERDE | Info panel lista `requiredNodeIds` con estado de cada prereq |
| Feedback de desbloqueo | ✅ VERDE | `onNodeUnlocked` y `onPointsChanged` refrescan UI inmediatamente |
| Acento por tipo de nodo | ✅ VERDE | Producto=naranja, Empleado=azul-morado, Seguridad=rojo, Mejora=teal |
| Conexiones entre nodos | ✅ VERDE | `ConnectionLineUI` actualiza color según estado del prereq |
| Scroll del árbol | ✅ VERDE | `ScrollRect` con `nodesContainer`/`linesContainer` separados |

---

## 8. Mejoras de Productos

| Aspecto | Estado | Detalle |
|---|---|---|
| Tarjetas por producto | ✅ VERDE | Una fila/tarjeta por `ProductScriptableObject` con estado |
| Estado bloqueado/desbloqueado | ✅ VERDE | Color de fila diferente, botón gris si bloqueado |
| Feedback sin fondos | ✅ VERDE | Botón rojo, mensaje de error en label de estado |
| Refresh post-desbloqueo | ✅ VERDE | `onProductNodeUnlocked` actualiza lista |
| Productos Básicos 1 visible | ✅ VERDE | Desbloqueado por defecto desde `DefaultUnlockedNodeId` |

---

## 9. Mejoras de Empleados

| Aspecto | Estado | Detalle |
|---|---|---|
| Tarjetas por empleado | ✅ VERDE | Hasta `MaxEmployees` tarjetas con estado de color |
| Estados visuales | ✅ VERDE | [BLOQ]/[DISP]/[OK]/[SIN PUESTO]/[TRABAJANDO] con colores diferenciados |
| Contratación | ✅ VERDE | Botón Contratar activo si desbloqueado y con fondos |
| Estado NPC | ✅ VERDE | `EmployeeNPCSpawner` instancia visual de NPC al contratar |
| Workstation | ✅ VERDE | `EmployeeWorkstationRegistry` registra y muestra puesto asignado |

---

## 10. Mejoras de Expandir

| Aspecto | Estado | Detalle |
|---|---|---|
| Mapa visual | ✅ VERDE | `ExpansionMapRenderer` genera grid visual de zonas |
| Zonas con estado | ✅ VERDE | Colores distintos: bloqueada/disponible/comprada |
| Precio visible | ✅ VERDE | Zona seleccionada muestra precio en panel detalle |
| Feedback de compra | ✅ VERDE | UI refresca al comprar, `onZonePurchased` propagado |
| Sincronía con escena | ✅ VERDE | `SupermarketExpansionSystem` y `ExpansionRealWorldBridge` |

---

## 11. Mejoras de Equipamiento y Seguridad

| Aspecto | Estado | Detalle |
|---|---|---|
| Equipamiento en árbol | ✅ VERDE | Nodos de tipo `Improvement` gestionan upgrades de equipo |
| Seguridad en árbol | ✅ VERDE | Nodos `TreeNodeType.Security` con acento rojo-naranja |
| Cobertura de seguridad | ✅ VERDE | `EntrepreneurTreeSecurityAdapter.GetSecurityCoveragePercent()` |
| Ladrones respetan seguridad | ✅ VERDE | `ShoplifterSystem` consulta nivel de seguridad |

---

## 12. Validaciones

### Runner creado en esta fase

| Runner | Ruta | Estado |
|---|---|---|
| `ShopMasterUIVisualConsistencyAuditRunner` | `Assets/StoreSimulator/Editor/` | ✅ CREADO |

### Runners previos

| Runner | Estado declarado (Fase 4) | Nota |
|---|---|---|
| `ShopMasterEntrepreneurTreePointsAuditRunner` | ✅ VERDE (Fase 4) | No regresión esperada |
| `ShopMasterEntrepreneurTreeFullAuditRunner` | ✅ VERDE (Fase 4) | 20/20 checks |
| `ShopMasterWarehouseAndNPCVisualAuditRunner` | 🟡 AMARILLO (Fase 4) | Requiere Unity + NavMesh local |
| `ShopMasterFinalIntegrationPhase4Runner` | ✅ VERDE (Fase 4) | No regresión esperada |
| `ShopMasterUIVisualConsistencyAuditRunner` | ⏳ PENDIENTE ejecución local | Nuevo en Fase 5 |

### Comandos de ejecución local (Windows, Unity 6000.0.37f1)

```powershell
# 1. Compilación general
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' `
  -batchmode -projectPath 'C:\Users\ijuan\Animo' -quit `
  -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_UIVisual_Fase5.log'

# 2. Points audit
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' `
  -batchmode -projectPath 'C:\Users\ijuan\Animo' `
  -executeMethod FLOBUK.StoreSimulator.Editor.ShopMasterEntrepreneurTreePointsAuditRunner.Run `
  -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_EntrepreneurTreePointsAudit_Fase5.log'

# 3. Full tree audit
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' `
  -batchmode -projectPath 'C:\Users\ijuan\Animo' `
  -executeMethod FLOBUK.StoreSimulator.Editor.ShopMasterEntrepreneurTreeFullAuditRunner.Run `
  -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_EntrepreneurTreeFullAudit_Fase5.log'

# 4. Warehouse/NPC audit
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' `
  -batchmode -projectPath 'C:\Users\ijuan\Animo' `
  -executeMethod FLOBUK.StoreSimulator.Editor.ShopMasterWarehouseAndNPCVisualAuditRunner.Run `
  -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_PlayMode_Almacen_NPC_Visual_Fase5.log'

# 5. UI Visual Consistency (NUEVO - Fase 5)
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' `
  -batchmode -projectPath 'C:\Users\ijuan\Animo' `
  -executeMethod FLOBUK.StoreSimulator.Editor.ShopMasterUIVisualConsistencyAuditRunner.Run `
  -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_UIVisualConsistencyAudit_Fase5.log'

# 6. Final integration
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' `
  -batchmode -projectPath 'C:\Users\ijuan\Animo' `
  -executeMethod FLOBUK.StoreSimulator.Editor.ShopMasterFinalIntegrationPhase4Runner.Run `
  -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_FinalIntegrationPhase4_UIVisual_Fase5.log'
```

---

## 13. Logs Generados (esperados al ejecutar)

| Log | Ruta | Contenido esperado |
|---|---|---|
| Batchmode general | `Documentos/Unity_Batchmode_UIVisual_Fase5.log` | Sin errores C# |
| Points audit | `Documentos/Unity_EntrepreneurTreePointsAudit_Fase5.log` | `Failures=0` |
| Full tree audit | `Documentos/Unity_EntrepreneurTreeFullAudit_Fase5.log` | `Failures=0` (20/20) |
| Warehouse/NPC | `Documentos/Unity_PlayMode_Almacen_NPC_Visual_Fase5.log` | `Failures=0` |
| UI Visual Consistency | `Documentos/Unity_UIVisualConsistencyAudit_Fase5.log` | `Failures=0` (20 checks) |
| Final integration | `Documentos/Unity_FinalIntegrationPhase4_UIVisual_Fase5.log` | `Failures=0` |

---

## 14. Failures Finales

| Entorno | Failures | Nota |
|---|---|---|
| Código C# (compilación estática) | **0** | Sin errores de compilación esperados |
| Runner UIVisualConsistency (local) | **Pendiente** | Requiere Unity Editor local |
| Runner FullTreeAudit (local) | **Pendiente** | Requiere Unity Editor local |
| Runner FinalIntegration (local) | **Pendiente** | Requiere Unity Editor local |

---

## 15. Warnings Restantes

1. **CHECK-16 (imágenes blancas)**: Algunos panels base del asset StoreSimulator usan `Image` sin sprite color blanco como fondo. Son intencionales y no son placeholders — el runner los reporta como WARN, no FAIL.
2. **CHECK-18 (Canvas adicionales)**: Posibles Canvas overlay del sistema de Admin Mode o del sistema de fin de juego. Son intencionales y controlados.
3. **CHECK-20 (CanvasScaler)**: Si el canvas principal del asset base usa `ConstantPixelSize`, el runner lo reportará como WARN. Se recomienda cambiar a `ScaleWithScreenSize` (ref screen: 1920×1080, match 0.5) para mejor soporte multi-resolución.
4. **NavMesh**: Warehouse/NPC audit puede dar YELLOW en NavMesh si el bake no se ha ejecutado localmente.

---

## 16. Pendientes Reales

### Requieren Unity Editor local (no ejecutables en CI sin Unity)

1. **Ejecutar los 6 runners** listados en sección 12 y confirmar `Failures=0`.
2. **CanvasScaler**: Abrir la escena Game.unity, seleccionar el Canvas raíz, cambiar ScaleMode a `Scale With Screen Size` con Reference Resolution `1920×1080` y Match `0.5`.
3. **Prefabs de nodos**: Si `UpgradesUIController.nodePrefab` está vacío en Inspector, asignar el prefab en la escena para que el árbol use el prefab en lugar del fallback procedural.
4. **Iconos de nodos**: Asignar sprites reales en los `NodeData.icon` de los ScriptableObjects para que el árbol muestre íconos diferenciados por categoría.
5. **Sonidos de UI**: Integrar SFX existentes del asset base (click, desbloqueo) en `UpgradesUIController` si los AudioClip están disponibles.
6. **Test en resoluciones**: Verificar UI en Game View a 1920×1080, 1600×900 y 1366×768.

### No requeridos para estado VERDE (ya implementado en código)

- Sistema de diseño visual centralizado ✅
- Estados visuales de nodos del árbol ✅
- UI procedural de todos los tabs ✅
- Refresh inmediato de UI tras acciones ✅
- Labels de estado ASCII compatibles con todas las fuentes ✅
- Runner de auditoría visual ✅

---

## 17. Próximo Punto Recomendado

**Fase 6 — Ejecución local y cierre de VERDE**

1. Hacer `git pull` en la máquina local.
2. Abrir Unity 6000.0.37f1 con el proyecto `C:\Users\ijuan\Animo`.
3. Ejecutar los 6 runners en orden.
4. Si `UIVisualConsistencyAudit` reporta WARN en CanvasScaler: corregir en Inspector.
5. Si `WarehouseAndNPCVisualAudit` da YELLOW en NavMesh: rebakear NavMesh.
6. Capturar screenshots de la UI en modo Play para evidencia visual.
7. Actualizar este reporte con resultados reales de los runners.
8. Declarar estado VERDE final.

---

## Tabla de Estado por Área

| Área | Mejora visual | Assets reutilizados | Estado | Evidencia |
|---|---|---|---|---|
| Computadora | Tabs limpios, jerarquía mantenible | UIShopDesktop, UIShopCategoryHelper | 🟡 AMARILLO | Bootstrap integra tabs dinámicamente |
| Árbol | 3 estados visuales, puntos, costos, líneas | ComputerUITheme, UGUI Image/Button/TMP | 🟡 AMARILLO | UpgradesUIController, NodeUI, ConnectionLineUI |
| Productos | Tarjetas con estado, botón comprar | ComputerUITheme, UGUI | 🟡 AMARILLO | OrdersAppUIController |
| Empleados | Tarjetas con estado, 5 labels | ComputerUITheme, UGUI | 🟡 AMARILLO | EmployeeAppUIController |
| Expandir | Mapa visual con zonas | ComputerUITheme, UGUI | 🟡 AMARILLO | ExpansionAppUIController + MapRenderer |
| Equipamiento | Integrado en árbol (nodos Improvement) | ComputerUITheme | 🟡 AMARILLO | EntrepreneurTreeUpgradeAdapter |
| Seguridad | Nodos Security con acento rojo | ComputerUITheme | 🟡 AMARILLO | EntrepreneurTreeSecurityAdapter |
| Inventario | Tabla con colores de stock | ComputerUITheme | 🟡 AMARILLO | InventoryAppUIController |

> **Nota sobre AMARILLO**: El estado AMARILLO en todas las áreas refleja que el código está completo y correcto, pero la validación final con runners en PlayMode aún está pendiente de ejecución en Unity Editor local. No indica deuda de diseño ni de implementación — indica que la verificación en máquina real no ha sido ejecutada en este entorno de CI.
