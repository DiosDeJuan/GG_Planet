<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 5 - Cierre espacios, precios, reportes y finales QA

## Resumen

Se implemento una capa integrada al asset base para cerrar los frentes pendientes de espacios, precios, reportes diarios, alertas de caja, instrucciones y hooks de finales Monopoly/Bancarrota.

La nueva funcionalidad vive dentro de la computadora existente mediante una app/seccion `GESTION`, sin crear otro Canvas principal ni rehacer el flujo de compra, inventario, arbol, clientes o seguridad.

Estado general: **AMARILLO**. Compila en `dotnet build` y en Unity batchmode, pero queda pendiente prueba manual en Play Mode para validar comportamiento visual/interactivo completo.

## Base de trabajo

- Rama base indicada: `origin/codex/fase4-4-arbol-requisitos-logros-puntos-qa`.
- Commit base indicado por el usuario: `6bdf640`.
- Rama usada para esta fase: `codex/fase5-cierre-espacios-precios-reportes-finales-qa`.
- Proyecto trabajado: `SHOP_MASTER_FINAL/`.
- No se trabajo dentro de `Animo/`.

## Documentos revisados

- `Documentos/NEW_Requerimientos.docx`.
- `Documentos/propuestas juanito (2).docx`.
- `Documentos/Protocolo prpuesta Juanito (1).pdf`.
- `Documentos/Guia Gantt (1).pdf`.

Hallazgos usados:

- Expansiones desde computadora: area de venta y area de almacenamiento.
- Area inicial de venta: 192 m2.
- Area maxima de venta: 576 m2.
- Area inicial de almacenamiento: 32 m2.
- Unidad de expansion de venta: 16 m2.
- Unidad de expansion de almacenamiento: 32 m2.
- Costo venta: `$1,750.00`.
- Costo almacenamiento: `$2,500.00`.
- Ladrones: probabilidad base 2%, maxima 6.5%, relacionada con expansion.
- Clientes: base 50-75 y aumento de 15% por expansion de venta.
- Precios: maximo 300%, minimo `$0.00`, moneda USD.
- Formulas documentadas para probabilidad de compra y compra extra.
- Reporte diario: ventas, ganancias, perdidas, productos robados, productos agotados, clientes perdidos, renta, luz, sueldos.
- Alerta de caja: espera mayor a 7 segundos.
- Finales: Monopoly y Bancarrota.
- Necesidad de instrucciones in-game.

## Reportes previos revisados

- `Reportes/Fase1_Arbol_Emprendedor_UI_Computadora.md`.
- `Reportes/Codex_Fase2_Productos_Compra_Delivery_Inventario.md`.
- `Reportes/Codex_Fase3_Empleados_Arbol_Laptop_NPCs.md`.
- `Reportes/Codex_Fase4_Seguridad_Ladrones_Mejoras_Arbol_Reportes.md`.
- `Reportes/Codex_Fase4_1_Hotfix_Movimiento_Arbol_QA.md`.
- `Reportes/Codex_Fase4_2_Computadora_Licenses_Arbol_UI.md`.
- `Reportes/Codex_Fase4_3_Arbol_Visual_RPG_Nodos.md`.
- `Reportes/Codex_Fase4_4_Arbol_Requisitos_Logros_Puntos_QA.md`.

Conclusiones:

- El Arbol del Emprendedor ya estaba integrado en computadora/laptop.
- La persistencia del arbol ya existia en `SaveGameSystem`.
- Productos, compra, delivery e inventario ya habian sido conectados en Fase 2.
- Empleados ya estaban conectados a arbol y guardado en Fase 3.
- Seguridad/ladrones y logros ya tenian base en Fase 4.
- Fase 5 no rehizo esos sistemas; se conecto encima de sus puntos de extension.

## Sistemas auditados

- Computadora/laptop: `UIShopDesktop`.
- Arbol y logros: `Assets/StoreSimulator/Scripts/EntrepreneurTree/`.
- Persistencia: `SaveGameSystem`, `ItemDatabase`, `StatsDatabase`.
- Expansiones del asset: `ExpansionScriptableObject`, `ExpansionObject`, `UIShopItemExpansion`, `UpgradeSystem`.
- Clientes: `CustomerSystem`, `Customer`, `CustomerCart`.
- Caja: `CashDesk`.
- Seguridad: `SecurityManager`.
- Precios: `ProductScriptableObject`, `PriceTag`, `UIPriceLabelWindow`.
- Reportes/estadisticas: `StatsDatabase`, `UIStats`.
- Productos reales y assets: `Assets/StoreSimulator/ScriptableObjects/Products/` y `Assets/StoreSimulator/ScriptableObjects/Expansions/`.

## Requerimientos usados

| Requerimiento | Estado | Evidencia |
| --- | --- | --- |
| RQF5 espacios desde computadora | **AMARILLO** | App `GESTION` agrega seccion `ESPACIOS` dentro de la computadora. |
| RQF6 espacio de venta | **AMARILLO** | `ShopExpansionManager` compra unidades de 16 m2 por `$1,750.00` y activa expansiones fisicas existentes cada 6 unidades. Pendiente Play Mode. |
| RQF7 espacio de almacenamiento | **AMARILLO** | `ShopExpansionManager` persiste unidades de almacenamiento de 32 m2 por `$2,500.00`. Falta hook visual/fisico de storage si el asset lo requiere. |
| RQF16 ladrones por expansion | **AMARILLO** | `SecurityManager` usa probabilidad escalada 2% a 6.5% desde `ShopExpansionManager`. Pendiente tuning en Play Mode. |
| RQF20 reporte diario | **AMARILLO** | `StatsDatabase` y `UIStats` agregan renta, luz, productos agotados y clientes perdidos. Pendiente validacion visual manual. |
| RQF21 guardado/carga | **VERDE tecnico** | `SaveGameSystem` guarda/carga espacios, finales y estadisticas nuevas sin crear save paralelo. |
| RQF22 clientes por expansion | **AMARILLO** | `CustomerSystem` multiplica spawn rate con +15% por unidad de venta. Pendiente prueba de balance. |
| RQF25 precio cero | **AMARILLO** | `ProductPricingCalculator` permite `$0.00`; `Customer` lo interpreta como probabilidad alta y sin ingreso por item. |
| RQF26 precio ideal | **AMARILLO** | Calculadora central usa precio ideal y formula documentada; `UIPriceLabelWindow` y `GESTION/PRECIOS` lo muestran. |
| RQF27 precio alto | **AMARILLO** | Probabilidad baja con precio mayor al ideal; clientes pueden rechazar compra y se registra queja. |
| RQF28 precio maximo 300% | **VERDE tecnico** | Precios se limitan a 300% del ideal. |
| RQF29 compra/venta con precio actual | **AMARILLO** | `Customer.Collect()` usa precio fijado y probabilidad central. Pendiente prueba manual con clientes reales. |
| RQF34 reporte campos completos | **AMARILLO** | Campos agregados al reporte existente. |
| RQF35 espera en caja | **AMARILLO** | `CashDesk` alerta tras 7s y puede registrar cliente perdido. Falta devolver producto a estanteria por ausencia de referencia original al llegar a caja. |
| RQNF3 persistencia | **VERDE tecnico** | Nuevos datos serializados en `SaveGameSystem` y `StatsDatabase`. |
| RQNF10-RQNF15 precios | **AMARILLO** | Reglas implementadas en calculadora central; pendiente QA de gameplay. |
| RQNF20 instrucciones | **AMARILLO** | Seccion `AYUDA` agregada en `GESTION`. Pendiente revision UX en editor. |
| Final Monopoly | **AMARILLO** | `GameEndingService` marca final disponible cuando arbol completo + espacios completos. No fuerza pantalla final. |
| Final Bancarrota | **AMARILLO** | `GameEndingService` marca riesgo si dinero queda bajo cero. No borra partida ni bloquea juego. |

## Implementacion tecnica

### App GESTION en computadora

Se agrego `UIManagementUIBootstrap` para montar una app nueva dentro de la computadora existente. Reutiliza la estructura de `UIShopDesktop`, `Navigation`, `ContentArea`, botones TMP y paneles runtime del mismo Canvas.

La app contiene:

- `ESPACIOS`: compra de area de venta y almacenamiento, progreso, costo, clientes, riesgo de ladrones y estado de finales.
- `PRECIOS`: lista de productos, precio ideal, precio actual, probabilidad de compra, probabilidad de compra extra, advertencias y botones para ajustar.
- `AYUDA`: instrucciones operativas para computadora, arbol, productos, empleados, expansiones, seguridad, precios, caja y reportes.

### Espacios y expansiones

Se creo `ShopExpansionManager` como adaptador sobre el sistema real de expansiones del asset:

- No crea inventario paralelo.
- No crea delivery paralelo.
- No crea economia paralela.
- Usa `MoneyManager.instance.Money`.
- Usa `UpgradeSystem` y `ExpansionScriptableObject` para activar expansiones fisicas existentes.
- Persiste su estado desde `SaveGameSystem`.

Reglas aplicadas:

- Venta inicial: 192 m2.
- Venta maxima: 576 m2.
- Unidad de venta: 16 m2.
- Costo venta: `$1,750.00`.
- Almacenamiento inicial: 32 m2.
- Unidad almacenamiento: 32 m2.
- Costo almacenamiento: `$2,500.00`.
- Cada 6 unidades de venta se intenta activar una expansion fisica real del asset.

Limitacion:

- El asset contiene expansiones fisicas tipo `Room 1..4`. No se crearon assets falsos para cada unidad de 16 m2. La granularidad documental se guarda y la expansion fisica se conecta por bloques.
- El almacenamiento se guarda como capacidad/logica de fase; falta confirmar si el asset tiene un espacio fisico de storage ampliable para conectarlo visualmente.

### Precios y probabilidad de compra

Se creo `ProductPricingCalculator` como fuente central para:

- Precio ideal.
- Precio maximo 300%.
- Precio minimo `$0.00`.
- Formato USD.
- Probabilidad de compra.
- Probabilidad de compra extra.
- Mensajes de advertencia.

Se conecto en:

- `Customer.Collect()`: decide si el cliente compra/rechaza por precio.
- `UIPriceLabelWindow`: muestra probabilidad y limita precio.
- `UIManagementPanel`: permite revisar/ajustar precios desde computadora.

Mensajes/efectos:

- Precio cero queda permitido.
- Sobreprecio reduce probabilidad.
- Precio excesivo se limita a 300%.
- Rechazo por precio registra queja en logros.

### Reporte diario

Se extendio el reporte existente:

- Renta diaria.
- Electricidad diaria.
- Productos agotados.
- Clientes perdidos.

Calculos integrados:

- Renta diaria con base en capital + area de venta.
- Electricidad diaria con base en gasto acumulado y expansion.
- Deduccion en cierre de dia desde `StatsDatabase`.
- Evaluacion de bancarrota despues de gastos diarios.

### Caja y clientes perdidos

`CashDesk` ahora:

- Lanza aviso si un cliente espera demasiado en caja.
- Registra cliente perdido si la espera continua.
- Limpia la caja y avanza la fila para evitar bloqueo.

Limitacion:

- La devolucion fisica del producto a estanteria queda pendiente. En el punto de caja ya no existe referencia confiable al lugar original del item para reponerlo sin inventar un sistema paralelo.

### Ladrones y expansion

`SecurityManager` ahora obtiene la probabilidad desde `ShopExpansionManager`:

- Base: 2%.
- Maxima: 6.5%.
- Escala segun progreso de expansion de venta.
- Mantiene reduccion por seguridad y logica previa de tipos de ladron.

### Finales

Se agrego `GameEndingService`:

- Final Monopoly queda disponible al completar arbol y todas las expansiones.
- Bancarrota queda marcada si el dinero cae bajo cero.
- Se notifican estados, pero no se fuerza pantalla final ni cierre de partida.

Esto deja hooks claros sin romper flujo actual.

## Gantt tecnico de esta fase

| Actividad | Tiempo estimado | Estado |
| --- | ---: | --- |
| Auditoria documental/reportes | 1.0 h | Completado |
| Auditoria sistemas existentes | 1.5 h | Completado |
| Integracion espacios/expansiones | 2.0 h | Completado tecnico |
| Integracion precios/probabilidades | 2.0 h | Completado tecnico |
| Reportes diarios/estadisticas | 1.0 h | Completado tecnico |
| Alerta caja/finales/ayuda | 1.5 h | Completado tecnico |
| Validacion build/batchmode | 1.0 h | Completado |
| Play Mode manual | 1.0 h | Pendiente local |

## No duplicacion

Confirmado:

- No se creo inventario paralelo.
- No se creo delivery paralelo.
- No se creo economia paralela.
- No se creo Canvas principal paralelo.
- No se rehizo el Arbol del Emprendedor.
- No se crearon productos falsos ni expansiones falsas.
- No se reintrodujo `Animo/`.

## `Animo/`

Busqueda funcional ejecutada en:

```text
rg -n "Animo" .\Assets .\Packages .\ProjectSettings .\Documentos
```

Resultado: sin referencias funcionales dentro de esas rutas.

Nota: si existe una carpeta `Animo/` fuera de `SHOP_MASTER_FINAL`, no fue tocada porque esta fase debia trabajar solo dentro del proyecto valido.

## Assets TMP no incluidos

Se detectaron cambios preexistentes no relacionados y no se deben stagear:

- `Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Bangers SDF.asset`.
- `Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset`.

Estos cambios no forman parte de Fase 5.

## Archivos modificados

- `Assembly-CSharp.csproj`.
- `Assets/StoreSimulator/Scripts/CashDesk.cs`.
- `Assets/StoreSimulator/Scripts/Customer.cs`.
- `Assets/StoreSimulator/Scripts/CustomerSystem.cs`.
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurAchievementDefinition.cs`.
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurAchievementManager.cs`.
- `Assets/StoreSimulator/Scripts/SaveGameSystem.cs`.
- `Assets/StoreSimulator/Scripts/Security/SecurityManager.cs`.
- `Assets/StoreSimulator/Scripts/StatsDatabase.cs`.
- `Assets/StoreSimulator/Scripts/UIPriceLabelWindow.cs`.
- `Assets/StoreSimulator/Scripts/UIShopDesktop.cs`.
- `Assets/StoreSimulator/Scripts/UIStats.cs`.
- `Assets/StoreSimulator/Scripts/UpgradeSystem.cs`.

## Archivos nuevos

- `Assets/StoreSimulator/Scripts/Endings/GameEndingService.cs`.
- `Assets/StoreSimulator/Scripts/Expansion/ShopExpansionManager.cs`.
- `Assets/StoreSimulator/Scripts/Management/UIManagementPanel.cs`.
- `Assets/StoreSimulator/Scripts/Management/UIManagementUIBootstrap.cs`.
- `Assets/StoreSimulator/Scripts/Pricing/ProductPricingCalculator.cs`.
- `Assets/StoreSimulator/Scripts/Endings.meta`.
- `Assets/StoreSimulator/Scripts/Endings/GameEndingService.cs.meta`.
- `Assets/StoreSimulator/Scripts/Expansion.meta`.
- `Assets/StoreSimulator/Scripts/Expansion/ShopExpansionManager.cs.meta`.
- `Assets/StoreSimulator/Scripts/Management.meta`.
- `Assets/StoreSimulator/Scripts/Management/UIManagementPanel.cs.meta`.
- `Assets/StoreSimulator/Scripts/Management/UIManagementUIBootstrap.cs.meta`.
- `Assets/StoreSimulator/Scripts/Pricing.meta`.
- `Assets/StoreSimulator/Scripts/Pricing/ProductPricingCalculator.cs.meta`.
- `Reportes/Codex_Fase5_UnityBatchmode.log`.
- `Reportes/Codex_Fase5_Cierre_Espacios_Precios_Reportes_Finales_QA.md`.

## Inscripcion POMPIC

Archivos de codigo modificados/nuevos incluyen:

```text
//Adaptado por POMPIC 20100333
```

El reporte incluye:

```text
<!-- Adaptado por POMPIC 20100333 -->
```

Excepciones:

- Archivos `.meta`: no se agrego comentario porque Unity serializa un formato estricto y meter comentarios puede romper importacion.
- Assets TMP modificados preexistentes: no se tocaron ni se stagearon.

## Validaciones ejecutadas

### Git

- Rama confirmada: `codex/fase5-cierre-espacios-precios-reportes-finales-qa`.
- Remoto confirmado: `https://github.com/DiosDeJuan/GG_Planet.git`.
- `git diff --check -- .`: sin errores de whitespace; solo warnings CRLF/LF.

### Compilacion .NET

Comando:

```text
dotnet build .\SHOP_MASTER_FINAL.sln
```

Resultado:

```text
Compilacion correcta.
0 Advertencia(s)
0 Errores
```

### Unity batchmode

Comando:

```text
"C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe" -batchmode -projectPath "C:\Users\ijuan\SHOP_MASTER_FINAL" -quit -logFile "C:\Users\ijuan\SHOP_MASTER_FINAL\Reportes\Codex_Fase5_UnityBatchmode.log"
```

Resultado:

- Exit code: 0.
- Log: `Reportes/Codex_Fase5_UnityBatchmode.log`.
- Evidencia del log: `*** Tundra build success (20.64 seconds), 10 items updated, 808 evaluated`.
- No se encontraron errores C# (`error CS`) ni `Compilation failed`.
- El log contiene mensajes no fatales de Licensing Client/access token en el arranque de Unity, pero la compilacion de scripts termino correctamente.

### Busqueda `Animo`

Comando:

```text
rg -n "Animo" .\Assets .\Packages .\ProjectSettings .\Documentos
```

Resultado: sin coincidencias funcionales.

## Validaciones no posibles desde nube

No se ejecuto Play Mode interactivo. Debe validarse localmente en Unity:

- Apertura de escena principal.
- Apertura de computadora/laptop.
- Visual de la app `GESTION`.
- Botones de compra de espacios.
- Ajuste de precios.
- Comportamiento real de clientes.
- Espera en caja.
- Reporte diario al cierre de dia.
- Persistencia visual despues de guardar/cargar.

## QA manual para Isaac

1. Abrir Unity.
2. Abrir escena principal del juego.
3. Entrar en Play Mode.
4. Abrir computadora/laptop.
5. Confirmar que aparece la app `GESTION`.
6. Entrar a `ESPACIOS`.
7. Comprar area de venta con fondos suficientes.
8. Confirmar descuento de dinero.
9. Confirmar aumento de m2 y progreso.
10. Comprar hasta activar una expansion fisica si hay fondos.
11. Confirmar que la expansion real del asset aparece/desbloquea correctamente.
12. Comprar almacenamiento y confirmar persistencia del valor.
13. Entrar a `PRECIOS`.
14. Cambiar precio de un producto a `$0.00`.
15. Confirmar que el juego no rompe y el cliente puede comprar sin ingreso.
16. Subir precio por encima de 300% y confirmar que se limita.
17. Confirmar que la UI muestra probabilidad de compra y advertencia.
18. Dejar un producto caro y observar rechazos/quejas si aparecen clientes.
19. Dejar un cliente esperando mas de 7 segundos en caja.
20. Confirmar alerta de espera.
21. Confirmar cliente perdido si sigue sin atenderse.
22. Cerrar dia y revisar reporte diario.
23. Confirmar renta/luz/productos agotados/clientes perdidos.
24. Guardar y cargar.
25. Confirmar persistencia de espacios, precios, reporte y final hooks.
26. Revisar consola sin errores rojos.

## Pendientes reales

- Play Mode manual para validar UX, layout y gameplay.
- Conectar expansion de almacenamiento a una representacion fisica si existe en escenas/prefabs y el asset lo permite.
- Devolver productos fisicamente a estanteria cuando cliente abandona caja; requiere una referencia al origen del item que el flujo actual no conserva.
- Crear UI final dedicada para Monopoly/Bancarrota si el proyecto quiere una pantalla de cierre real.
- Ajustar balance de spawn de clientes por expansion, porque +15% por unidad de 16 m2 puede crecer mucho en maxima expansion.
- Validar si el equipo quiere que la vieja tienda de `UPGRADES` permita comprar expansiones directamente o si se debe redirigir toda compra de espacios a `GESTION`.

## Estado final

**AMARILLO**

Motivo:

- Codigo compila por `dotnet build`.
- Unity batchmode compila scripts correctamente.
- Se integro con sistemas existentes.
- No hay Play Mode manual desde este entorno.
- Hay hooks documentados para almacenamiento fisico, devolucion de productos y pantallas finales.
