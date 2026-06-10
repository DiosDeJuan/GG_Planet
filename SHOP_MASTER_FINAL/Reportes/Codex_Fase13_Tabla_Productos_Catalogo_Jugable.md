<!-- Adaptado por POMPIC 20100333 -->
# Codex Fase 13 - Tabla definitiva de productos y catalogo jugable

## Encabezado
- Fase: Codex Fase 13 - Tabla definitiva de productos, catalogo jugable.
- Rama base: `origin/codex/fase12-arbol-reproducible-gameplay-rpg`.
- Commit base esperado: `d7d320f22eb2e4680c8a450063b655144706d9b4`.
- Rama nueva: `codex/fase13-tabla-productos-catalogo-jugable`.
- Responsable: Codex.
- Proyecto: `SHOP_MASTER_FINAL`.
- Repositorio: `DiosDeJuan/GG_Planet`.
- Fecha: 2026-06-10.

## Objetivo
Cerrar la tabla real de 47 productos documentados sin crear una economia paralela. La integracion se hizo sobre `DocumentedProductCatalog`, `ItemDatabase`, `UIShopDesktop`, `PRODUCTS`, `PRECIOS`, `EntrepreneurProgress` y el flujo real de compra/entrega del asset base Store Simulator.

## Documentos y continuidad revisada
- Reporte Fase 12: arbol reproducible, ramas de productos y ruta desktop real.
- Guia Fase 12: prueba manual del Arbol.
- Reportes previos de QA visual, precios, gestion, guardado y regresion.
- Referencias funcionales heredadas: productos, computadora, laptop, arbol, precios, guardado y flujo real de entrega.

## Resumen ejecutivo
- Estado tecnico: VERDE en regresion automatizada.
- PlayMode: `64/64` tests passed en `Reportes/Codex_Fase13_PlayModeResults.xml`.
- Catalogo: `47/47` productos definitivos expuestos en runtime.
- PRODUCTS: productos base, bloqueados y placeholders se renderizan con mensajes de desbloqueo.
- PRECIOS: los 47 productos aparecen como preciables, con guardado/carga de precio y migracion de IDs legacy.
- Compra/entrega: los productos habilitados usan el flujo real de `ItemDatabase`, dinero del jugador y `DeliverySystem`.
- Placeholders: seguros, documentados y marcados; no bloquean compra, precio ni aparicion.
- Arbol: cada producto tiene nodo canonico y mensaje `Falta desbloquear: <nodo>`.
- Pendiente humano: probar fisicamente venta/cliente para todos los productos dentro de una partida larga.

## Archivos modificados
| Archivo | Cambio |
|---|---|
| `Assets/StoreSimulator/Scripts/Products/DocumentedProductCatalog.cs` | Tabla definitiva de 47 productos, IDs canonicos, metadatos jugables, placeholders seguros y migracion de IDs antiguos. |
| `Assets/StoreSimulator/Scripts/ItemDatabase.cs` | Carga de precios migra IDs legacy a IDs definitivos antes de aplicar valores guardados. |
| `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeNodeDefinition.cs` | Mapeo de los 47 productos definitivos al Arbol del Emprendedor. |
| `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurProgress.cs` | Mensaje de bloqueo especifico para producto no desbloqueado. |
| `Assets/StoreSimulator/Scripts/PackageObject.cs` | Guardia contra `UIGame.Instance == null` durante limpieza de paquetes en pruebas PlayMode. |
| `Assets/StoreSimulator/Tests/PlayMode/StoreSimulatorPlayModeRegressionTests.cs` | Regresion ampliada Fase 13, compra real, placeholders, precios, migracion y capturas. |

## Tabla definitiva de productos
| ID definitivo | Nombre | Categoria | Nodo | Mueble | Precio ideal | Costo caja | Asset |
|---|---|---|---|---|---:|---:|---|
| `leche` | Leche | Productos Basicos 1 | `productos_basicos_1` | Gondolas | 100 | 1200 | Asset base |
| `sal` | Sal | Productos Basicos 1 | `productos_basicos_1` | Gondolas | 50 | 200 | Asset base |
| `agua` | Agua | Productos Basicos 1 | `productos_basicos_1` | Gondolas | 50 | 600 | Asset base |
| `pasta` | Pasta | Productos Basicos 1 | `productos_basicos_1` | Gondolas | 100 | 500 | Asset base |
| `azucar` | Azucar | Productos Basicos 1 | `productos_basicos_1` | Gondolas | 100 | 400 | Asset base |
| `harina` | Harina | Productos Basicos 2 | `productos_basicos_2` | Gondolas | 100 | 800 | Placeholder Product_A-E |
| `arroz` | Arroz | Productos Basicos 2 | `productos_basicos_2` | Gondolas | 150 | 1500 | Placeholder Product_A-E |
| `frijoles` | Frijoles | Productos Basicos 2 | `productos_basicos_2` | Gondolas | 200 | 1500 | Placeholder Product_A-E |
| `pan` | Pan | Productos Basicos 2 | `productos_basicos_2` | Gondolas | 200 | 700 | Placeholder Product_A-E |
| `aceite` | Aceite | Productos Basicos 2 | `productos_basicos_2` | Gondolas | 250 | 2500 | Placeholder Product_A-E |
| `cafe` | Cafe | Productos Basicos 3 | `productos_basicos_3` | Gondolas | 300 | 2500 | Placeholder Product_A-E |
| `huevo` | Huevo | Productos Basicos 3 | `productos_basicos_3` | Gondolas | 200 | 800 | Placeholder Product_A-E |
| `cheddar` | Cheddar | Lacteos 1 | `lacteos_1` | Refrigeradores | 300 | 1200 | Placeholder Product_A-E |
| `yogurt_natural` | Yogurt natural | Lacteos 1 | `lacteos_1` | Refrigeradores | 100 | 600 | Placeholder Product_A-E |
| `mantequilla` | Mantequilla | Lacteos 1 | `lacteos_1` | Refrigeradores | 200 | 800 | Placeholder Product_A-E |
| `queso_americano` | Queso americano | Lacteos 2 | `lacteos_2` | Refrigeradores | 250 | 1000 | Placeholder Product_A-E |
| `queso_crema` | Queso crema | Lacteos 2 | `lacteos_2` | Refrigeradores | 200 | 800 | Placeholder Product_A-E |
| `mozzarella` | Mozzarella | Lacteos 3 | `lacteos_3` | Refrigeradores | 300 | 1000 | Placeholder Product_A-E |
| `parmesano` | Parmesano | Lacteos 3 | `lacteos_3` | Refrigeradores | 500 | 2000 | Placeholder Product_A-E |
| `pimienta_negra` | Pimienta negra | Especias 1 | `especias_1` | Gondolas | 200 | 700 | Placeholder Product_A-E |
| `canela` | Canela | Especias 1 | `especias_1` | Gondolas | 100 | 500 | Placeholder Product_A-E |
| `manzana` | Manzana | Productos Frescos 1 | `productos_frescos_1` | Refrigeradores | 200 | 2000 | Placeholder Product_A-E |
| `platano` | Platano | Productos Frescos 1 | `productos_frescos_1` | Refrigeradores | 100 | 1000 | Placeholder Product_A-E |
| `jitomate` | Jitomate | Productos Frescos 1 | `productos_frescos_1` | Refrigeradores | 200 | 2000 | Placeholder Product_A-E |
| `cebolla` | Cebolla | Productos Frescos 1 | `productos_frescos_1` | Refrigeradores | 150 | 1500 | Placeholder Product_A-E |
| `uvas` | Uvas | Productos Frescos 2 | `productos_frescos_2` | Refrigeradores | 400 | 1000 | Placeholder Product_A-E |
| `zanahorias` | Zanahorias | Productos Frescos 2 | `productos_frescos_2` | Refrigeradores | 100 | 500 | Placeholder Product_A-E |
| `ajo` | Ajo | Productos Frescos 2 | `productos_frescos_2` | Refrigeradores | 150 | 600 | Placeholder Product_A-E |
| `jabon` | Jabon | Productos de Higiene | `productos_higiene` | Gondolas | 100 | 1500 | Placeholder Product_A-E |
| `papel_higienico` | Papel higienico | Productos de Higiene | `productos_higiene` | Gondolas | 500 | 2500 | Placeholder Product_A-E |
| `detergente` | Detergente | Productos de Higiene | `productos_higiene` | Gondolas | 200 | 2000 | Placeholder Product_A-E |
| `pasta_dientes` | Pasta de dientes | Productos de Higiene | `productos_higiene` | Gondolas | 100 | 800 | Placeholder Product_A-E |
| `res` | Res | Proteina 1 | `proteina_1` | Congeladores | 1000 | 4000 | Placeholder Product_A-E |
| `pollo` | Pollo | Proteina 1 | `proteina_1` | Congeladores | 500 | 2000 | Placeholder Product_A-E |
| `cerdo` | Cerdo | Proteina 1 | `proteina_1` | Congeladores | 700 | 3000 | Placeholder Product_A-E |
| `pescado` | Pescado | Proteina 1 | `proteina_1` | Congeladores | 800 | 3000 | Placeholder Product_A-E |
| `cola` | Cola | Sodas | `sodas` | Refrigeradores | 150 | 1200 | Placeholder Product_A-E |
| `cola_sin_azucar` | Cola sin azucar | Sodas | `sodas` | Refrigeradores | 150 | 1200 | Placeholder Product_A-E |
| `refresco_limon` | Refresco de limon | Sodas | `sodas` | Refrigeradores | 150 | 1200 | Placeholder Product_A-E |
| `trufa` | Trufa | Productos de Lujo 1 | `productos_lujo_1` | Congeladores | 10000 | 15000 | Placeholder Product_A-E |
| `chocolate_importado` | Chocolate importado | Productos de Lujo 1 | `productos_lujo_1` | Congeladores | 300 | 1500 | Placeholder Product_A-E |
| `caviar` | Caviar | Productos de Lujo 1 | `productos_lujo_1` | Congeladores | 5000 | 40000 | Placeholder Product_A-E |
| `refrigerador` | Refrigerador | Electrodomesticos 1 | `electrodomesticos_1` | Electrodomesticos | 40000 | 40000 | Placeholder Product_A-E |
| `microondas` | Microondas | Electrodomesticos 1 | `electrodomesticos_1` | Electrodomesticos | 6000 | 6000 | Placeholder Product_A-E |
| `horno` | Horno | Electrodomesticos 1 | `electrodomesticos_1` | Electrodomesticos | 20000 | 20000 | Placeholder Product_A-E |
| `mesa` | Mesa | Electrodomesticos 1 | `electrodomesticos_1` | Electrodomesticos | 10000 | 10000 | Placeholder Product_A-E |
| `licuadora` | Licuadora | Electrodomesticos 1 | `electrodomesticos_1` | Electrodomesticos | 8000 | 8000 | Placeholder Product_A-E |

## Comportamiento validado
| Area | Resultado |
|---|---|
| IDs definitivos | No quedan IDs nuevos con prefijo `doc_`; los IDs antiguos migran a canonicos. |
| Conteo | `DocumentedProductCatalog.TotalDocumentedProducts == 47`. |
| PRODUCTS | El catalogo completo muestra 47/47 productos, bloqueos y placeholders. |
| PRECIOS | Los 47 productos son ajustables y el precio guardado/cargado se aplica por ID canonico. |
| Arbol | Cada producto resuelve un nodo conocido del Arbol del Emprendedor. |
| Bloqueo | Un producto sin nodo desbloqueado no permite compra y muestra `Falta desbloquear: <nodo>`. |
| Fondos insuficientes | Compra sin dinero reporta faltante y no genera paquete. |
| Compra correcta | Producto base y placeholder compran mediante `TryBuy`, descuentan dinero y crean entrega. |
| Persistencia legacy | Guardados con `doc_mozzarella` migran a `mozzarella` y conservan precio. |
| Limpieza PlayMode | `PackageObject.OnDestroy` ya no rompe al limpiar paquetes sin `UIGame.Instance`. |

## Capturas generadas
Directorio: `Reportes/Capturas_Fase13/`.

| Captura | Evidencia |
|---|---|
| `Products_01_Todos.png` | Catalogo completo 47/47. |
| `Products_02_Basicos1_Disponibles.png` | Productos base desbloqueados desde inicio. |
| `Products_03_CategoriaBloqueada.png` | Categoria bloqueada con mensaje de Arbol. |
| `Products_04_PlaceholderSeco.png` | Placeholder seco visible. |
| `Products_05_PlaceholderRefrigerado.png` | Placeholder refrigerado visible. |
| `Products_06_PlaceholderLujo.png` | Placeholder lujo visible. |
| `Products_07_ElectrodomesticoPlaceholder.png` | Placeholder de electrodomestico visible. |
| `Products_08_FondosInsuficientes.png` | Compra rechazada por fondos insuficientes. |
| `Products_09_CompraCorrecta.png` | Compra aceptada con paquete/entrega. |
| `Precios_01_Todos.png` | Tabla de precios con 47 productos. |
| `Precios_02_ProductoPlaceholder.png` | Placeholder tambien editable en precios. |
| `Precios_03_PrecioCero.png` | Caso precio cero. |
| `Precios_04_Maximo300.png` | Caso limite de precio alto. |
| `Arbol_Productos_01_RamaProductoDesbloqueada.png` | Rama de producto desbloqueada. |

## Tests agregados o ampliados
| Test | Proposito |
|---|---|
| `ProductTable_AllIdsAreDefinitiveStableAndUnique` | Garantiza 47 IDs definitivos, unicos y sin `doc_`. |
| `ProductTable_AllProductsExposePlayableMetadata` | Garantiza metadata minima para compra, precio, mueble y placeholder. |
| `ProductCatalog_LoadMigratesLegacyProductIds` | Garantiza compatibilidad con guardados de Fase 12. |
| `ProductsShop_BlockedProductCannotBePurchased` | Verifica bloqueo real por Arbol. |
| `ProductsShop_NoFundsShowsMissingAmount` | Verifica rechazo por dinero insuficiente. |
| `ProductsShop_PurchaseBaseAndPlaceholderProductsUsesRealDeliveryFlow` | Verifica compra real de producto base y placeholder. |
| `ProductVisualEvidence_GeneratesFase13ProductCaptures` | Genera evidencia visual Fase 13. |

## Validacion ejecutada
| Comando | Resultado |
|---|---|
| `dotnet build .\SHOP_MASTER_FINAL.sln` | Correcto, 0 errores. |
| Unity batchmode PlayMode con `Codex_Fase13_UnityPlayMode.log` y `Codex_Fase13_PlayModeResults.xml` | Correcto, `64/64` passed. |
| Revision visual de `Products_01_Todos.png` | Catalogo completo visible con 47/47. |

## Advertencias observadas
- Unity mantiene dos warnings heredados de `CommandBuffer: temporary render texture not found` durante PlayMode.
- Unity mantiene el warning heredado `PlayerController did not find optional Input Action Map 'UI'. Continuing with gameplay input.`
- No se observaron `NullReferenceException`, `MissingReferenceException`, errores `CS` ni fallo de tests en la corrida final.

## Inscripcion y exclusiones
- Los `.cs` modificados conservan o reciben `//Adaptado por POMPIC 20100333`.
- Este reporte y la guia reciben `<!-- Adaptado por POMPIC 20100333 -->`.
- Los PNG, XML y log no reciben inscripcion porque romperia o ensuciaria evidencia generada.
- No se modificaron prefabs, escenas ni assets serializados delicados.
- `Animo/ no fue reintroducido.`
- Los assets TMP preexistentes `Bangers SDF.asset` y `Roboto-Bold SDF.asset` aparecieron modificados en el worktree, pero no pertenecen a esta fase y no deben stagearse.

## Criterio final
La fase queda como Release Candidate tecnico para tabla de productos: todos los productos documentados existen en runtime, son visibles en PRODUCTS/PRECIOS, tienen nodo de Arbol, migran guardados y compran por el flujo real cuando estan desbloqueados y hay fondos. El pendiente honesto es la pasada humana de gameplay largo con colocacion/cliente/venta para todos los productos, especialmente placeholders y electrodomesticos.
