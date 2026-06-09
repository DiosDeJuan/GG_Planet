<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 4: Seguridad, ladrones, mejoras del arbol y reportes

## Rama y base

- Rama de trabajo: `codex/fase4-seguridad-ladrones-mejoras-arbol-reportes`
- Base solicitada: `origin/codex/fase3-empleados-arbol-laptop-npcs`
- Commit base verificado al iniciar: `5ca6c1a`
- Alcance: solo `SHOP_MASTER_FINAL`, sin reintroducir rutas o sistemas de `Animo`.

## Implementacion

### Seguridad progresiva

- Se agrego `SecurityManager` como sistema runtime unico para seguridad.
- El nivel activo se deriva de `EntrepreneurProgress.SecurityLevel`, por lo que no existe un progreso paralelo.
- Los nodos existentes del Arbol del Emprendedor activan:
  - `seguridad_1`: camaras, menor probabilidad de escape automatico.
  - `seguridad_2`: guardias, mayor probabilidad de detencion automatica.
  - `seguridad_3`: alarma, probabilidad muy alta de detencion automatica.
- El estado minimo del sistema se guarda con `SaveGameSystem` bajo `SecurityManager`.

### Ladrones integrados al flujo real de clientes

- `CustomerSystem` ahora expone `onCustomerSpawned`.
- `SecurityManager` promociona aleatoriamente clientes reales a ladrones al spawnear.
- Los tipos implementados son:
  - `Common`
  - `SuspiciousCustomer`
  - `Special`
- El robo usa `PlacementObject.Remove()` sobre mercancia real en estanterias.
- Una detencion recupera la mercancia con `PlacementObject.Add(product)` e instancia de nuevo el prefab real del producto.
- Si el ladron escapa, la unidad queda perdida de la estanteria.
- El jugador puede detener ladrones con la interaccion existente (`Interactable`) cuando hay evidencia de robo.
- Se agrego un proxy de interaccion para colliders hijos de prefabs de cliente.

### Mejoras del arbol

- `Cafeina` ahora impacta tambien a empleados reponedores:
  - ajusta el intervalo de trabajo.
  - escala la velocidad del `NavMeshAgent`.
- `Carismatico` ahora aplica un bono del 5% a ventas atendidas por cajero automatico asignado.
- El bono de `Carismatico` se registra en estadisticas diarias.

### Reportes y estadisticas diarias

- `StatsDatabase` ahora guarda:
  - robos totales.
  - valor robado.
  - valor recuperado.
  - ladrones escapados.
  - detenciones manuales.
  - detenciones automaticas.
  - intentos/exitos de detencion automatica.
  - nivel de seguridad al cierre.
  - ingresos extra por `Carismatico`.
- `UIStats` muestra lineas adicionales para perdidas por robo, recuperacion por seguridad, bono carismatico y conteo de robos/detenciones.

## Archivos principales

- `Assets/StoreSimulator/Scripts/Security/SecurityManager.cs`
- `Assets/StoreSimulator/Scripts/Security/ShoplifterAgent.cs`
- `Assets/StoreSimulator/Scripts/Security/ShoplifterInteractableProxy.cs`
- `Assets/StoreSimulator/Scripts/Security/SecurityLevel.cs`
- `Assets/StoreSimulator/Scripts/Security/ShoplifterType.cs`
- `Assets/StoreSimulator/Scripts/CustomerSystem.cs`
- `Assets/StoreSimulator/Scripts/StatsDatabase.cs`
- `Assets/StoreSimulator/Scripts/UIStats.cs`
- `Assets/StoreSimulator/Scripts/SaveGameSystem.cs`
- `Assets/StoreSimulator/Scripts/CashDesk.cs`
- `Assets/StoreSimulator/Scripts/Employees/EmployeeRuntimeAgent.cs`
- `Assembly-CSharp.csproj`

## Validacion

- `git status --short -- SHOP_MASTER_FINAL`: ejecutado desde la raiz real del repo.
- `git diff --check -- SHOP_MASTER_FINAL`: sin errores de whitespace; solo avisos normales de CRLF.
- `dotnet build .\SHOP_MASTER_FINAL.sln`: correcto, 0 advertencias, 0 errores.
- Unity batchmode:
  - Intento 1 con `Unity.exe -batchmode -quit`: retorno rapido sin generar log.
  - Intento 2 con `Start-Process -Wait`: excedio 5 minutos, no quedo proceso Unity vivo y no genero log.
  - Resultado: intento realizado, no certificado como verde por falta de log de Unity.

## Notas de diseno

- No se agregaron prefabs nuevos de seguridad; los ladrones se montan sobre prefabs reales de clientes para mantener el flujo existente.
- La seguridad activa siempre se calcula desde el arbol, evitando duplicar progreso.
- La perdida por robo se representa como inventario retirado de estanteria; no se descuenta dinero adicional para no duplicar el impacto economico.
