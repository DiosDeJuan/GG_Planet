# Reporte de Verificacion Lote 1 - Precios, Compra y Caja

## 1. Resumen ejecutivo
- Fecha: 2026-05-18
- Rama: main
- Commit inicial: e18ab80
- Estado general del lote: compilacion batchmode correcta; se corrigieron puntos funcionales de precios, compra extra y espera en caja, pero no se ejecuto Play Mode interactivo. Por criterio del lote, ningun requisito se marca como "VERIFICADO EN PLAY MODE".
- Resultado batchmode: exitoso. Unity 6000.0.37f1 recompilo scripts con `Tundra build success` y salio con codigo 0. No se encontraron `error CS`, `Scripts have compiler errors`, `Compilation failed`, `NullReferenceException` ni `MissingReferenceException`.

## 2. Requerimientos trabajados

| Requerimiento | Estado inicial | Trabajo realizado | Estado final | Evidencia Play Mode | Archivos modificados |
|---|---|---|---|---|---|
| RQF28-PRICE | Parcial | Se agrego flujo de precio pendiente, boton OK, boton $0, validacion 0 a 300%, confirmacion explicita y formato USD. | PARCIAL | BLOQUEADO: no se ejecuto Play Mode interactivo; validado por revision estatica y batchmode. | `Assets/UI/Computer/Pricing/PricingAppUIController.cs`, `Assets/Systems/Pricing/ProductPricingSystem.cs` |
| RQF25-PRICE | Parcial | Se verifico que `Customer.Collect` llama al adapter real y se agregaron logs QA bajo `UNITY_EDITOR` para decision compra/rechazo. | PARCIAL | BLOQUEADO: falta observar clientes reales comprando/rechazando en Play Mode. | `Assets/StoreSimulator/Scripts/Customer.cs`, `Assets/Systems/Pricing/ProductPurchaseProbabilityAdapter.cs` |
| RQF26-PRICE | Parcial | Se verifico formula de extra y se agrego validacion explicita de stock en anaquel antes de intentar unidad extra. | PARCIAL | BLOQUEADO: falta observar toma extra real con stock y sin stock en Play Mode. | `Assets/StoreSimulator/Scripts/Customer.cs`, `Assets/Systems/Pricing/ProductPricingSystem.cs`, `Assets/Systems/Pricing/ProductPurchaseProbabilityAdapter.cs` |
| RQF27-PRICE | Parcial | La UI ahora muestra siempre compra y extra, incluso cuando extra es 0%, usando preview del precio pendiente. | PARCIAL | BLOQUEADO: falta abrir app Precios y confirmar refresco visual en Play Mode. | `Assets/UI/Computer/Pricing/PricingAppUIController.cs`, `Assets/Systems/Pricing/ProductPricingSystem.cs` |
| RQF28-PRICEZERO | Parcial | Se agrego boton `$0`, aviso de ingreso cero y preview de probabilidad/extra para precio cero. | PARCIAL | BLOQUEADO: falta completar una venta a $0.00 en Play Mode y comparar balance/reporte. | `Assets/UI/Computer/Pricing/PricingAppUIController.cs` |
| RQF35/RQNF5 | Parcial | Se verifico timer 7s existente y se corrigio que la interaccion manual con caja marque al cliente como atendido. | PARCIAL | BLOQUEADO: faltan casos A/B/C en Play Mode: abandono, atencion manual y cajero automatico. | `Assets/StoreSimulator/Scripts/CashDesk.cs`, `Assets/StoreSimulator/Scripts/Customer.cs` |

## 3. RQF28-PRICE

### Que se reviso
- `Assets/UI/Computer/Pricing/PricingAppUIController.cs`
- `Assets/Systems/Pricing/ProductPricingSystem.cs`
- `Assets/Systems/EntrepreneurTree/EntrepreneurTreeUIBootstrap.cs`

### Que se corrigio
- La UI de precios ya no aplica cada cambio inmediatamente con los botones de incremento/decremento.
- Cada fila mantiene `pendingPrice`.
- Se agrego boton `OK` para confirmar el precio real con `ProductPricingSystem.TrySetCurrentPrice`.
- Se agrego boton `$0` para colocar precio cero como estrategia comercial.
- Se agrego validacion visual explicita cuando el precio pendiente queda fuera de rango: `$0.00` a 300% del precio ideal.
- Se mantiene el formato USD usando `StoreDatabase.FromLongToStringMoney`, que ya esta conectado al formateo centralizado.

### Evidencia de UI
- `PricingAppUIController.BuildRow`: crea botones `-$0.50`, `-$0.10`, `+$0.10`, `+$0.50`, `$0`, `OK`, `IDEAL`.
- `PricingAppUIController.ApplyDelta`: actualiza solo el precio pendiente.
- `PricingAppUIController.ConfirmPrice`: persiste el precio real en `ProductPricingSystem`.
- `PricingAppUIController.RefreshRow`: muestra precio actual, precio nuevo e ideal.

### Resultado final
PARCIAL. El codigo cumple la ruta funcional esperada y compila, pero falta prueba Play Mode interactiva abriendo computadora/laptop, app Precios y confirmando cambios producto por producto.

## 4. RQF25-PRICE

### Metodo exacto donde Customer decide comprar
- `Customer.Collect` obtiene el producto actual y calcula `shouldBuy`.

### Metodo exacto donde consulta ProductPurchaseProbabilityAdapter
- `Customer.Collect` llama:
  - `ProductPurchaseProbabilityAdapter.Instance.ShouldCustomerBuy(product)`

### Evidencia de que usa probabilidad real
- `ProductPurchaseProbabilityAdapter.ShouldCustomerBuy` obtiene:
  - `ProductPricingSystem.Instance.GetPurchaseProbability(product)`
- `ProductPricingSystem.GetPurchaseProbability` delega a:
  - `GetPurchaseProbabilityForPrice(product, product.storePrice)`
- Si el precio actual esta por debajo o igual al ideal, la probabilidad es 1.0.
- Si el precio actual supera el ideal, baja usando `probabilitySlope` y queda limitada a 0..1.
- Se agrego log QA bajo `UNITY_EDITOR` con producto, precio ideal, precio actual, probabilidad y resultado.

### Resultado
PARCIAL. La decision de compra ya esta conectada a pricing real y no depende solamente de la formula vieja del asset, pero falta observar el comportamiento de clientes en Play Mode con precio ideal y precio alto.

## 5. RQF26-PRICE

### Como se calcula compra extra
- `ProductPricingSystem.GetExtraPurchaseProbabilityForPrice` devuelve 0 si `Pactual >= Pideal`.
- Si `Pactual < Pideal`, calcula `(ideal - actual) / ideal * 0.10`, limitado a 0..1.
- Con precio ideal $5.00 y actual $0.00, la probabilidad extra esperada es 10%.

### Donde se aplica en Customer
- `Customer.Collect` evalua, despues de tomar el producto base:
  - `HasExtraStock(product)`
  - `ProductPurchaseProbabilityAdapter.Instance.ShouldCustomerBuyExtra(product)`
  - `cart.TryAddExtraCurrentProduct()`

### Como se valida stock
- `Customer.HasExtraStock` consulta `ProductInventorySystem.Instance.GetShelfStock(product) > 0`.
- Si no existe `ProductInventorySystem`, mantiene fallback permisivo para no romper el asset base, pero en runtime normal usa inventario real.
- `CustomerCart.TryAddExtraCurrentProduct` intenta agregar una unidad adicional del producto actual mediante el flujo del carrito.

### Resultado
PARCIAL. La compra extra esta conectada al flujo real del cliente y ahora valida stock antes del intento, pero falta Play Mode con stock alto, stock bajo y stock agotado para descartar productos fantasma.

## 6. RQF27-PRICE

### Donde se muestran porcentajes
- `PricingAppUIController.RefreshRow` muestra:
  - `Compra: X%   Extra: Y%`

### Como se refrescan
- Al tocar botones de precio se actualiza `pendingPrice` y se llama `RefreshRow`.
- Al confirmar precio, `ProductPricingSystem` dispara evento y `OnStorePriceUpdate` sincroniza el precio pendiente con `product.storePrice`.
- Las probabilidades se calculan con preview:
  - `ProductPricingSystem.GetPurchaseProbabilityForPrice`
  - `ProductPricingSystem.GetExtraPurchaseProbabilityForPrice`

### Resultado
PARCIAL. La UI ya calcula y muestra probabilidades con actualizacion inmediata por codigo, pero falta confirmar visualmente en Play Mode que los textos se ven y no se solapan.

## 7. RQF28-PRICEZERO

### Que pasa con balance
- `ProductPricingSystem.TrySetCurrentPrice` permite precio 0 porque solo rechaza valores menores a 0 y mayores a 300%.
- El cobro base de `CashDesk` suma el `billAmount`; si el producto cuesta $0.00, esa linea aporta exactamente 0.

### Que pasa con reporte
- Las ventas de $0.00 no deberian generar NaN ni infinito porque las probabilidades evitan division por cero usando precio ideal, no precio actual.
- Falta verificar en Play Mode que el reporte diario registre la venta sin romper totales.

### Que pasa con compra extra
- Precio $0.00 mantiene probabilidad de compra en 100%.
- Si el precio ideal es mayor que 0, puede generar probabilidad extra hasta 10% con la formula actual.

### Resultado
PARCIAL. El precio cero esta soportado en UI, pricing y formula, pero falta prueba completa de venta, balance y reporte en Play Mode.

## 8. RQF35/RQNF5

### Timer de 7 segundos
- `Customer.NotifyCheckoutWaitingForService` arranca el timer al entrar en espera de caja.
- `Customer.CheckoutWaitTimer` espera 7 segundos antes de alertar.

### Alerta
- Despues de 7 segundos sin servicio:
  - Log: `[CustomerWait] Customer has waited 7 seconds at checkout.`
  - Notificacion UI: `Cliente esperando demasiado en caja.`
  - Animacion/estado unhappy con `ShowUnhappy("Waiting too long.")`.
- No se verifico alerta sonora; no se encontro prueba Play Mode de audio para este lote.

### Abandono
- Si pasan 5 segundos adicionales y sigue sin servicio, `Customer.CheckoutWaitTimer` intenta:
  - `desk.TryCancelWaitingCustomer(this)`
  - fallback `GoHome()`

### Recuperacion de productos
- `CashDesk.TryCancelWaitingCustomer` elimina al cliente de cola y llama `customer.GoHomeFromCheckoutTimeout()`.
- El flujo de cancelacion usa recuperacion best-effort mediante carrito/sistemas existentes.

### Registro en stats
- `Customer.ShowUnhappy` incrementa clientes perdidos por medio de `StatsDatabase.OnCustomerLeft`.

### Resultado con jugador
- Se corrigio `CashDesk.Interact` para llamar `NotifyCheckoutServiceStarted` cuando el jugador toma control de una caja con cliente en cola.
- Falta prueba Play Mode manual para confirmar que no abandona mientras el jugador atiende.

### Resultado con cajero automatico
- `CashDesk.TryStartAutomatedCheckout` llama `customer.NotifyCheckoutServiceStarted` antes del procesamiento automatico.
- Falta prueba Play Mode con empleado cajero real para confirmar que no abandona y no duplica cobro.

## 9. Bugs encontrados y corregidos

| Bug | Causa | Archivo | Correccion | Verificado |
|---|---|---|---|---|
| UI de precios no tenia confirmacion explicita | Los botones aplicaban precio directamente al sistema | `Assets/UI/Computer/Pricing/PricingAppUIController.cs` | Se agrego `pendingPrice` y boton `OK` con `ConfirmPrice` | Batchmode + revision estatica |
| No habia accion directa para $0.00 | Solo se podia llegar restando manualmente | `Assets/UI/Computer/Pricing/PricingAppUIController.cs` | Se agrego boton `$0` y aviso de ingreso cero | Batchmode + revision estatica |
| Probabilidad extra podia no mostrarse cuando era 0% | La UI dependia de estado textual por rangos | `Assets/UI/Computer/Pricing/PricingAppUIController.cs` | Se muestra siempre `Compra` y `Extra` como porcentaje | Batchmode + revision estatica |
| Compra extra no validaba explicitamente stock de inventario antes del intento | El flujo dependia de que el carrito/placement fallara internamente | `Assets/StoreSimulator/Scripts/Customer.cs` | Se agrego `HasExtraStock` consultando `ProductInventorySystem.GetShelfStock` | Batchmode + revision estatica |
| Cliente podia seguir temporizador aunque jugador tomara caja antes del primer scan | `Interact` marcaba caja controlada pero no notificaba servicio al cliente | `Assets/StoreSimulator/Scripts/CashDesk.cs` | Se llama `NotifyCheckoutServiceStarted` al interactuar con caja ocupada | Batchmode + revision estatica |

## 10. Bugs pendientes

| Bug | Requerimiento afectado | Motivo | Prioridad |
|---|---|---|---|
| No hay verificacion Play Mode interactiva de app Precios | RQF28-PRICE, RQF27-PRICE | Batchmode no opera computadora/laptop ni valida layout visible | Critico para cierre QA |
| No hay observacion Play Mode de clientes aceptando/rechazando por precio | RQF25-PRICE | Falta correr escena Game y comparar comportamiento con precio ideal vs 300% | Critico |
| No hay observacion Play Mode de compra extra con stock agotado | RQF26-PRICE | Falta probar producto barato con stock suficiente y sin stock | Critico |
| No hay venta Play Mode a $0.00 con comparacion de balance/reporte | RQF28-PRICEZERO | Falta completar flujo de caja real | Critico |
| No se probaron casos A/B/C de espera en caja | RQF35/RQNF5 | Falta Play Mode con abandono, jugador y cajero automatico | Critico |
| Alerta sonora no verificada | RQF35/RQNF5 | No se confirmo clip/sistema de audio disponible en Play Mode | Medio |

## 11. Resultado Unity batchmode

- Comando ejecutado:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\ijuan\Animo' -logFile 'C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Lote1_Precios_Caja.log'
```

- Log generado:
  - `C:\Users\ijuan\Animo\Documentos\Unity_Batchmode_Lote1_Precios_Caja.log`
- Resultado:
  - `Tundra build success`
  - `Exiting batchmode successfully now`
  - Codigo de salida 0
- Errores encontrados:
  - Ningun `error CS`.
  - Ningun `Scripts have compiler errors`.
  - Ningun `Compilation failed`.
  - Ningun `NullReferenceException`.
  - Ningun `MissingReferenceException`.
- Advertencias observadas:
  - Warnings preexistentes de APIs obsoletas de TMP y `FindObjectsOfType`.
  - Warning de Render Graph compatibility mode de URP.
- Errores corregidos:
  - No hubo errores C# despues de los cambios de este lote.

## 12. Siguiente lote recomendado

Prompt propuesto para continuar, sin ejecutarlo en esta sesion:

```text
Continuar ShopMaster con LOTE 2 solamente. No revisar todo el proyecto ni redisenar UI.

Trabaja exactamente estos requerimientos:
- RQF25-EMP: App Empleados, contratar empleados y asignar rol desde la computadora.
- RQF26-EMP: Cajeros automaticos con tiempos reales entre 1 y 12 segundos, 0.5s por producto, +1.5s tarjeta, +2.5s efectivo, sin duplicar cobros ni romper pago manual.
- RQF9: Roles cajero/surtidor con descripcion clara y validacion de desbloqueos.
- RQF21: Autosave al final del dia.
- RQNF3: Carga completa sin perdida de dinero, dia, inventario, precios, empleados, roles y desbloqueos.
- RQNF4: Backup y proteccion ante fallo de guardado.

Orden obligatorio:
1. Revisar git status, rama y commit inicial.
2. Leer Documentos/Reporte_Verificacion_Lote1_Precios_Caja.md y Documentos/Reporte_Avance_Codex_Backend.md.
3. Revisar EmployeeAppUIController, EntrepreneurEmployeeSystem, EmployeeCashierCoordinator, CashDesk, SaveGameSystem, EntrepreneurTreeSaveIntegration y sistemas relacionados.
4. Corregir solo bugs de este lote.
5. Ejecutar Unity batchmode y corregir errores C#.
6. Crear Documentos/Reporte_Verificacion_Lote2_Empleados_Guardado.md con estados: VERIFICADO EN PLAY MODE, CORREGIDO Y VERIFICADO, PARCIAL, BLOQUEADO o NO CUMPLE.

No marques Play Mode como verificado si no se prueba interactivamente.
```
