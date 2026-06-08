<!-- Adaptado por POMPIC 20100333 -->
# Copilot Prompt 1/5 - Limpieza, estabilidad y base del Árbol

## Resumen
Se revisaron documentos oficiales en `SHOP_MASTER_FINAL/Documentos`, se auditó el estado de la fase previa de Codex, se eliminó completamente `Animo/`, se corrigió la causa probable del `NullReference` en `PlayerController.Awake`, y se dejó una API explícita en `EntrepreneurProgress` para consultas de empleados/seguridad/productos/mejoras para fases siguientes.

## Documentos revisados
- Documentos encontrados en `SHOP_MASTER_FINAL/Documentos/`:
  - `NEW_Requerimientos.docx`
  - `Protocolo prpuesta Juanito (1).pdf`
  - `propuestas juanito (2).docx`
  - `Guia Gantt (1).pdf`
- Documentos leídos desde nube:
  - `NEW_Requerimientos.docx` (lectura por extracción de texto)
  - `propuestas juanito (2).docx` (lectura por extracción de texto)
  - `Protocolo prpuesta Juanito (1).pdf` (lectura parcial por extracción de texto)
  - `Guia Gantt (1).pdf` (revisión de contenido; guía de planeación, sin reglas funcionales fuertes del árbol)
- Documentos no leídos: ninguno de los listados faltó. Limitación: al ser lectura automatizada de PDF/DOCX, el formato visual/tablas puede perderse parcialmente.
- Reglas extraídas para esta fase:
  - Árbol del Emprendedor: acceso desde computadora (`RQF3`), nodos con costo en puntos, prerequisitos obligatorios (`RQF4`, `RQF36`, `RQNF8`, `RQNF9`).
  - Computadora/laptop/oficina: la laptop de oficina es centro de control (compra, empleados, árbol, expansiones), UI consistente (`RQNF18`).
  - Productos desbloqueables: `Productos Básicos 1` al inicio; grupos siguientes por nodos.
  - Empleados: desbloqueo vía árbol y asociación por prerequisitos (`RQF8`, `RQF9`, documentos de propuesta).
  - Seguridad: niveles 1/2/3 desbloqueables y progresivos (33/66/99%), con prerequisitos.
  - Mejoras: `Cafeína` (+10% velocidad empleados) y `Carismático` (+5% ingresos de cajero).
  - Guardado/carga: conservar desbloqueos de árbol, seguridad activa y progreso (`RQF21`, `RQNF3`).
  - Reportes/control de avance: reporte diario del juego (`RQF20`, `RQF34`) y control por reportes en `Reportes/`.
- Dudas registradas:
  - Tablas exactas de nodos/productos en los documentos pueden requerir validación visual manual en Word/PDF para confirmar todos los IDs reales usados en Unity.

## Carpeta Reportes
- `Reportes/` sí existía dentro de `SHOP_MASTER_FINAL/`.
- No fue necesario crearla.
- Reportes anteriores detectados:
  - `Fase1_Arbol_Emprendedor_UI_Computadora.md`
  - `Dotnet_Fase1_Arbol_Build.log`
  - `Unity_Fase1_Arbol_Compile_Attempt.log`
- Reporte nuevo creado:
  - `SHOP_MASTER_FINAL/Reportes/Copilot_Prompt1_Limpieza_Estabilidad_Arbol.md`

## Carpeta Animo
- `Animo/` sí existía en raíz del repo.
- `Animo/` fue eliminada completamente.
- Referencias residuales fuera de `Animo/`: no encontradas (`rg` global excluyendo `Animo/**`).
- Limpieza aplicada: eliminación total del proyecto descartado para dejar foco en `SHOP_MASTER_FINAL/`.

## Proyecto activo
Se trabajó solo sobre `SHOP_MASTER_FINAL/` para análisis/correcciones funcionales. La única acción fuera de esa carpeta fue la eliminación obligatoria de `Animo/`.

## Revisión de lo hecho por Codex
- `EntrepreneurProgress`: funciona como núcleo de estados/desbloqueos y puntos; valida prerequisitos y normaliza carga.
- `EntrepreneurTreeNodeDefinition`: concentra definición de nodos, costos, tipos y prerequisitos en un solo lugar.
- `EntrepreneurTreeUI`: muestra estados Bloqueado/Disponible/Desbloqueado y opera sobre `EntrepreneurProgress`.
- `EntrepreneurTreeUIBootstrap`: integra panel del árbol en la computadora y evita duplicado cuando ya existe el panel.
- `UIShopDesktop`: llama bootstrap del árbol dentro del flujo real de la computadora.
- `SaveGameSystem`: guarda/carga `EntrepreneurProgress` sin crear segundo sistema de guardado.
- `UIShopItemProduct`: valida desbloqueo de producto antes de compra para productos mapeados al árbol.

## Reparación PlayerController
- Causa del NullReference: uso directo de `PlayerInput.GetPlayerByIndex(0)` sin validación en `Awake` (línea reportada), lo cual rompe si no hay `PlayerInput` disponible aún/en esa escena.
- Archivo modificado:
  - `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/PlayerController.cs`
- Solución aplicada:
  - Cache de `PlayerInput` en `Awake`.
  - Validación nula con desactivación controlada del componente + `Debug.LogWarning` cuando falta `PlayerInput`.
  - Validación del ActionMap `UI` antes de deshabilitarlo.
  - Suscripción/desuscripción segura de `onActionTriggered`.
- Pendiente de prueba manual:
  - Verificar en Unity local que `Intro` ya no muestra error rojo y `Game` mantiene control completo.

## Integración con computadora
- El árbol sigue integrado en el flujo existente de `UIShopDesktop` + `UIShopCategoryHelper`.
- Usa navegación/paneles de la UI de computadora existente, no un canvas externo nuevo.
- Se mantiene la prevención de duplicados por comprobación del panel `Entrepreneur Tree` antes de crear.
- Se conserva estilo base del asset al construirse dentro del panel de categorías de la laptop.

## Guardado y carga
- Datos del árbol guardados: puntos de progreso y `unlockedNodeIds`.
- Datos consultables derivados de nodos cargados: productos desbloqueados, empleados desbloqueados, nivel de seguridad, mejoras activas.
- Protección de partidas antiguas:
  - `LoadFromJSON` reinicia defaults seguros.
  - Si no hay bloque del árbol, mantiene estado por defecto.
  - `Productos Básicos 1` se fuerza como desbloqueado base en normalización.

## Productos y desbloqueos
- `Productos Básicos 1`: queda desbloqueado por defecto (`DefaultUnlockedNodeId`).
- Bloqueo/desbloqueo: productos mapeados al árbol se bloquean hasta cumplir nodo; desbloqueados sí permiten compra.
- Pendientes detectados para Prompt 3/5:
  - Mapeo completo de todos los grupos/productos al árbol todavía no está exhaustivo (actualmente `GetKnownProductNodeId` cubre subconjunto).

## Archivos modificados
- Eliminado completo:
  - `Animo/` (todo el proyecto y sus archivos)
- Código:
  - `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/PlayerController.cs`
  - `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurProgress.cs`
- Reporte nuevo:
  - `SHOP_MASTER_FINAL/Reportes/Copilot_Prompt1_Limpieza_Estabilidad_Arbol.md`

## Inscripción POMPIC
Archivos donde se agregó inscripción:
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/PlayerController.cs` → `//Adaptado por POMPIC 20100333`
- `SHOP_MASTER_FINAL/Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurProgress.cs` → `//Adaptado por POMPIC 20100333`
- `SHOP_MASTER_FINAL/Reportes/Copilot_Prompt1_Limpieza_Estabilidad_Arbol.md` → `<!-- Adaptado por POMPIC 20100333 -->`

Archivos modificados sin inscripción y justificación:
- `Animo/` eliminado completo: no aplica inscripción al tratarse de eliminación masiva de archivos.

## Validaciones posibles desde la nube
- Inspección de estructura del repo y rutas activas.
- Revisión de scripts clave del árbol, guardado, desktop y compra.
- Revisión de escenas configuradas en `EditorBuildSettings`.
- Búsqueda global de referencias residuales a `Animo` fuera de carpeta eliminada.
- Build .NET intentado en nube (falló por falta de .NET Framework 4.7.1 targeting pack en entorno Linux, no por error de código específico verificado aquí).

## Validaciones no posibles desde la nube
Debe probar Isaac en Unity local:
- Abrir escena Intro.
- Confirmar que no hay NullReference en PlayerController.
- Abrir escena Game.
- Abrir computadora.
- Ver sección Árbol del Emprendedor.
- Abrir/cerrar computadora varias veces.
- Confirmar que no se duplican botones.
- Confirmar Productos Básicos 1 desbloqueado.
- Confirmar estados Bloqueado/Disponible/Desbloqueado.
- Confirmar guardado/carga si es posible.

## Estado final
- Limpieza de repo (`Animo` eliminado): **VERDE**
- Revisión de documentos oficiales y extracción de reglas: **VERDE**
- Integración del Árbol en computadora existente (sin canvas paralelo): **VERDE**
- Corrección defensiva de `PlayerController.Awake`: **AMARILLO** (requiere verificación en Unity local)
- Guardado/carga del Árbol con compatibilidad de save viejo: **VERDE**
- Validación ejecutable completa en nube (compilación Unity real/play mode): **AMARILLO**

## Pendientes para Prompt 2/5
- Mejorar presentación visual/jerárquica del Árbol del Emprendedor (UX y legibilidad) manteniendo coherencia con la UI base del asset.
- Completar afinado visual de nodos/rutas/agrupaciones sin romper integración ya estable.
