<!-- Adaptado por POMPIC 20100333 -->
# Copilot Prompt 4/5 - Empleados, roles, NPC, cajero y surtidor

## Resumen
Se integró una app de **Empleados** dentro de la computadora del asset, conectada al **Árbol del Emprendedor** para bloqueo/desbloqueo real. Se implementó contratación de empleados 1-18, asignación de roles Cajero/Surtidor, spawn de NPC visible reutilizando prefabs de cliente, asignación de puesto, procesamiento automático básico para caja y surtido, además de guardado/carga integrado en `SaveGameSystem` sin sistema paralelo.

## Documentos revisados
- `SHOP_MASTER_FINAL/Documentos/NEW_Requerimientos.docx`
- `SHOP_MASTER_FINAL/Documentos/propuestas juanito (2).docx`
- `SHOP_MASTER_FINAL/Documentos/Protocolo prpuesta Juanito (1).pdf`
- `SHOP_MASTER_FINAL/Documentos/Guia Gantt (1).pdf`

## Reportes previos revisados
- `SHOP_MASTER_FINAL/Reportes/Copilot_Prompt1_Limpieza_Estabilidad_Arbol.md`
- `SHOP_MASTER_FINAL/Reportes/Copilot_Prompt2_Arbol_Emprendedor_UI_Jerarquia.md`
- `SHOP_MASTER_FINAL/Reportes/Copilot_Prompt3_Productos_Compra_Delivery_Inventario.md`
- `SHOP_MASTER_FINAL/Reportes/Fase1_Arbol_Emprendedor_UI_Computadora.md`

## Requerimientos usados
- **RQF8**: desbloqueo de empleados desde árbol mantenido por `EntrepreneurProgress.IsEmployeeUnlocked`.
- **RQF9**: roles Cajero/Surtidor con asignación desde app Empleados.
- **RQF21**: guardado/carga de empleados/roles/puestos/NPC integrado en `SaveGameSystem`.
- **RQF25/RQF26/RQF27**: no se alteró la lógica de probabilidad/precios de productos; se preservó integración previa.
- **RQF34**: se agregó `GetDailyEmployeeSalaryCost()` y descuento diario de salario al finalizar día.
- **RQNF3**: carga de saves viejos tolerada (si no hay bloque `EmployeeSystem`, se inicializa vacío).
- **RQNF7**: no se creó economía paralela; salarios y ventas siguen en `StoreDatabase`.
- **RQNF16/RQNF17/RQNF18**: mensajes claros y app integrada en UI existente de laptop/computadora.

## Auditoría de sistemas existentes
- **Empleados encontrados**: no existía sistema de empleados funcional previo.
- **NPC encontrados**: sistema de clientes (`CustomerSystem`, `Customer`, `CustomerAgent`) reutilizado para prefab visual de empleado.
- **Caja encontrada**: `CashDesk`, `CheckoutObject`, `UICashDesk*`, flujo real de cola/pago.
- **Storage/inventario encontrados**: `StorageSystem`, `PlacementSystem`, `PlacementObject`, `PackageObject`, `DeliverySystem`.
- **Reutilizado**:
  - UI de computadora (`UIShopDesktop`, `UIShopCategoryHelper`, navegación/categorías existentes).
  - Flujo de caja y cola real (`CashDesk`).
  - Flujo de productos en storage/shelves (`PackageObject` + `PlacementObject`).
  - Guardado real (`SaveGameSystem`).
- **No existía**:
  - App Empleados funcional.
  - Datos persistentes de empleados/roles.
  - Automatización mínima para cajero/surtidor NPC.

## App de Empleados
- Se añadió panel `Employees` en `Categories` y botón `Empleados` en `Navigation` vía bootstrap.
- Estado bloqueado cuando no hay empleados desbloqueados en Árbol:
  - mensaje al abrir: `Empleados bloqueado. Desbloquea Empleado 1 en el Árbol del Emprendedor.`
  - banner interno: `Desbloquea Empleado 1 en el Árbol del Emprendedor para gestionar empleados.`
- Muestra 1 a 18 con estado por empleado:
  - Bloqueado / Desbloqueado / Contratado
  - rol actual
  - estado de puesto
  - requisito faltante si está bloqueado.

## Contratación
- Validaciones aplicadas:
  - no contrata bloqueados por Árbol.
  - no duplica contratación.
  - respeta límite máximo 18.
- Mensajes implementados:
  - `Empleado bloqueado. Desbloquea [nodo] en el Árbol.`
  - `Empleado ya contratado.`
  - `Límite de empleados alcanzado (18).`
  - `Empleado contratado, pero falta puesto disponible.`
- Costo de contratación:
  - sin costo inicial (no se encontró costo explícito de contratación en sistema real del asset).

## NPC visible
- Prefab reutilizado: primer prefab disponible de `CustomerSystem.customerPrefabs`.
- Spawn: punto seguro cercano a `StoreDatabase.storeEntry` con offset por empleado + `NavMesh.SamplePosition` cuando disponible.
- Prevención de duplicados:
  - `EmployeeNPCMarker` por número de empleado.
  - restauración de NPC existentes al cargar.
  - limpieza de NPC huérfanos en carga.
- Pendiente visual:
  - requiere validación manual en Unity local para verificar colocación exacta en escena.

## Rol Cajero
- Asignación de puesto:
  - se asigna `CashDesk` libre real de escena.
  - si no hay caja libre, estado `Esperando puesto disponible`.
- Atención automática básica:
  - se añadió `CashDesk.AutoProcessCurrentCustomer(...)` para escaneo/pago automático con flujo existente.
  - aplica multiplicador `Carismático` con `EntrepreneurProgress.GetCashierSalesMultiplier()` como ingreso adicional.
- Dependencias Play Mode:
  - validar ritmo/comportamiento real de cola y UX en Unity local.

## Rol Surtidor
- Conexión con storage/inventario/shelves:
  - busca paquetes (`PackageObject`) con producto.
  - busca `PlacementObject` compatible y coloca producto.
- Aplicación `Cafeína`:
  - frecuencia de trabajo modulada por `EntrepreneurProgress.GetEmployeeSpeedMultiplier()`.
- Estructura mínima preparada:
  - `assignedProductId`
  - `assignedShelfId`
  - estado de puesto.
- Si no hay puesto/stock:
  - estado `Esperando puesto disponible`.

## Guardado/carga
- Guardado integrado en `SaveGameSystem` bajo bloque `EmployeeSystem`.
- Datos guardados:
  - contratado/no contratado
  - rol
  - puesto asignado (`assignedWorkstationId`, tipo)
  - `assignedShelfId`, `assignedProductId`
  - `npcInstanceId`
  - salario diario y multiplicadores
  - estado de puesto
- Carga:
  - reinicializa defaults seguros.
  - carga datos si existen.
  - respawnea/reusa NPC contratado y evita duplicados.
- Saves viejos:
  - si no existe `EmployeeSystem`, inicia lista vacía sin romper partida.

## Reporte diario / salario
- Implementado método: `EmployeeSystem.GetDailyEmployeeSalaryCost()`.
- Integración actual:
  - al finalizar día (`DayCycleSystem.onDayFinished`) se descuenta salario total de empleados en `StoreDatabase`.
- Pendiente:
  - validar visualmente en UI de estadísticas del día (Unity local) que el costo se refleje como gasto esperado.

## No duplicación
- No se creó sistema paralelo de inventario.
- No se creó sistema paralelo de caja.
- No se creó sistema paralelo de economía.
- No se creó Canvas paralelo.
- No se reintrodujo `Animo/`.

## Archivos modificados
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/UIShopDesktop.cs`
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/SaveGameSystem.cs`
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/CashDesk.cs`
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurProgress.cs`
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/EmployeeSystem.cs` (nuevo)
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/EmployeeManagementUI.cs` (nuevo)
- `SHOP_MASTER_FINAL/Reportes/Copilot_Prompt4_Empleados_Roles_NPC_Cajero_Surtidor.md` (nuevo)

## Inscripción POMPIC
Archivos con `//Adaptado por POMPIC 20100333`:
- `Assets/StoreSimulator/Scripts/UIShopDesktop.cs`
- `Assets/StoreSimulator/Scripts/SaveGameSystem.cs`
- `Assets/StoreSimulator/Scripts/CashDesk.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurProgress.cs` (ya lo tenía)
- `Assets/StoreSimulator/Scripts/EmployeeSystem.cs`
- `Assets/StoreSimulator/Scripts/EmployeeManagementUI.cs`

Archivo `.md` con `<!-- Adaptado por POMPIC 20100333 -->`:
- `Reportes/Copilot_Prompt4_Empleados_Roles_NPC_Cajero_Surtidor.md`

Excepciones:
- No se modificaron `.meta` ni binarios delicados.

## Validaciones posibles desde la nube
- Revisión estática de integración con Árbol, UI, caja, storage y save.
- Verificación de bloqueo de app y mensajes explícitos por código.
- Confirmación de que `Animo/` no existe en código funcional (solo referencias históricas en reportes).
- `dotnet build SHOP_MASTER_FINAL.sln` ejecutado (falla por entorno Linux sin .NET Framework 4.7.1 targeting pack).

## Validaciones no posibles desde la nube
- No se pudo abrir Unity Editor ni Play Mode.
- No se pudo validar visualmente spawn/animación/NPC en escena.
- No se pudo validar runtime final de cajero/surtidor en navegación real.

## Estado final
- App Empleados integrada y conectada al Árbol: **VERDE**
- Bloqueo/desbloqueo por Árbol: **VERDE**
- Contratación + roles + límite 18: **VERDE**
- NPC visible con prevención de duplicados: **AMARILLO** (falta prueba visual local)
- Cajero automático básico sobre caja real: **AMARILLO** (falta Play Mode)
- Surtidor automático básico sobre storage/shelves: **AMARILLO** (falta Play Mode)
- Guardado/carga de empleados/roles/puestos: **VERDE**
- Build cloud: **AMARILLO** (limitación de targeting pack, no error funcional confirmado)

## Pendientes para Prompt 5/5
- Expansión/mapa (no abordado en este prompt).
- Seguridad/ladrones completos (no abordado en este prompt).
- Ajustes finales de reportes diarios integrales y auditoría runtime completa en Unity local.
