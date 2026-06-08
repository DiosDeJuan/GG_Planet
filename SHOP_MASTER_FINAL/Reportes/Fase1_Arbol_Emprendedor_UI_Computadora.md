# Fase 1 - Árbol del Emprendedor en UI de Computadora

## Resumen
Se integró una sección "Árbol del Emprendedor" dentro del flujo existente de la computadora del asset Store Simulator. La sección se crea desde `UIShopDesktop`, usa la navegación/paneles existentes (`Navigation`, `Categories`, `UIShopCategoryHelper`), muestra nodos agrupados por Productos, Empleados, Seguridad y Mejoras, incluye panel de detalle y mantiene estados visibles: Desbloqueado, Disponible y Bloqueado.

El estado del árbol se centralizó en `EntrepreneurProgress`, con persistencia en `SaveGameSystem`, validación de prerequisitos en un único punto y normalización al cargar para evitar estados inválidos. Productos Básicos 1 inicia desbloqueado por defecto.

## Documentos revisados
- `Documentos/NEW_Requerimientos.docx`: RQF3, RQF4, tabla de empleados, niveles de seguridad, mejoras Cafeína/Carismático, guardado de desbloqueos.
- `Documentos/propuestas juanito (2).docx`: oficina/laptop, árbol del emprendedor, productos, puntos, empleados, seguridad, mejoras y reportes diarios.
- `Documentos/Protocolo prpuesta Juanito (1).pdf`: progresión por puntos, laptop/oficina, árbol, seguridad, empleados y reportes.
- `Documentos/Guia Gantt (1).pdf`: revisado; no aportó reglas funcionales específicas para esta fase.

## Requerimientos trabajados
- RQF3: Se agregó acceso al árbol desde la computadora.
- RQF4: Los prerequisitos se validan antes de desbloquear.
- RQF8: Existen nodos Empleado 1 a Empleado 18 con prerequisitos de la tabla del documento.
- RQF11: Existen nodos Seguridad Nivel 1, 2 y 3.
- RQF12: Seguridad respeta prerequisitos Empleado 7, 8 y 14.
- RQF18: Seguridad queda persistida y consultable por nivel.
- RQF36: `TryUnlock` valida prerequisitos/puntos y `LoadFromJSON` normaliza estados cargados.
- RQF28 mejoras: Cafeína y Carismático existen como nodos y persisten multiplicadores consultables.
- RQNF8: Estados del nodo calculados centralmente.
- RQNF9: Estados visibles en texto, no solo color.
- RQNF18: UI integrada dentro de la computadora existente.

## Archivos modificados
- `Assembly-CSharp.csproj`
- `Assets/StoreSimulator/Scripts/UIShopDesktop.cs`
- `Assets/StoreSimulator/Scripts/SaveGameSystem.cs`
- `Assets/StoreSimulator/Scripts/UIShopItemProduct.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeNodeDefinition.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurProgress.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeUI.cs`
- `Assets/StoreSimulator/Scripts/EntrepreneurTree/EntrepreneurTreeUIBootstrap.cs`
- Metas de la carpeta `Assets/StoreSimulator/Scripts/EntrepreneurTree/`
- `Reportes/Dotnet_Fase1_Arbol_Build.log`
- `Reportes/Unity_Fase1_Arbol_Compile_Attempt.log`

## Integración con asset base
Se reutilizó `UIShopDesktop` como punto de entrada de la computadora, `UIShopCategoryHelper` para cambiar paneles, `Button`, `Image`, `ScrollRect`, `TextMeshProUGUI` y layouts de Unity UI dentro del mismo Canvas. Las notificaciones usan `UIGame.AddNotification`.

La persistencia se agregó al flujo real de `SaveGameSystem` con `SimpleJSON`, junto al resto de sistemas del asset. Para productos existentes, `UIShopItemProduct` consulta `EntrepreneurProgress` antes de permitir compra si el producto tiene un nodo conocido.

## Estado funcional
- VERDE: Datos del árbol centralizados con productos, empleados, seguridad y mejoras.
- VERDE: Validación de prerequisitos y puntos desde `EntrepreneurProgress.TryUnlock`.
- VERDE: Guardado/carga del árbol con defaults seguros para partidas antiguas.
- VERDE: `dotnet build .\SHOP_MASTER_FINAL.sln` compila con 0 advertencias y 0 errores.
- AMARILLO: Unity batchmode no generó log dedicado desde este entorno; ver evidencia de intento.
- AMARILLO: Prueba manual de interacción en Play Mode pendiente en editor.
- AMARILLO: Empleados, seguridad y mejoras quedan persistidos/consultables, pero no hay sistemas finales de empleados/ladrones/ventas en esta copia para aplicar todos los efectos todavía.

## Validaciones ejecutadas
- `git status`: desde el workspace falló por raíz Git real en `C:\Users\ijuan`; con Git desde la raíz real mostró `?? SHOP_MASTER_FINAL/`.
- `git branch --show-current`: rama inicial `main`; rama usada `codex/fase1-arbol-computadora-ui`.
- `git remote -v`: origin inicial apuntaba a `https://github.com/DiosDeJuan/XINO.git`; se corrigió a `https://github.com/DiosDeJuan/GG_Planet.git`.
- `dotnet build .\SHOP_MASTER_FINAL.sln`: correcto, 0 advertencias, 0 errores. Log: `Reportes/Dotnet_Fase1_Arbol_Build.log`.
- Unity batchmode: intentado con Unity `6000.0.37f1`; no se obtuvo `Unity_Fase1_Arbol_Compile.log`. Evidencia: `Reportes/Unity_Fase1_Arbol_Compile_Attempt.log`.
- Play Mode/prueba manual: pendiente; no se pudo automatizar desde este entorno.

## Problemas encontrados y reparados
- Git resolvía la raíz real como `C:\Users\ijuan`, fuera del workspace. Se usaron comandos Git filtrados desde la raíz real.
- `origin` apuntaba a `DiosDeJuan/XINO.git`; se corrigió a `DiosDeJuan/GG_Planet.git`.
- El `.csproj` generado por Unity no incluía scripts nuevos; se agregaron los `Compile Include` necesarios para validar con `dotnet build`.
- Warnings de TextMeshPro por `enableWordWrapping` obsoleto; se reemplazó por `textWrappingMode`.
- Unity batchmode fue bloqueado inicialmente por una instancia interactiva abierta; se cerró la instancia y se documentaron los intentos posteriores.

## Pendientes
- Isaac debe abrir el editor y hacer prueba manual de Play Mode: computadora, botón Árbol, scroll, selección de nodos, mensajes de bloqueo, guardado/carga.
- Conectar multiplicadores de Cafeína y Carismático cuando existan sistemas finales de empleados/ventas en esta copia.
- Conectar `SecurityLevel` con el sistema final de ladrones cuando esté disponible.
- Conectar nodos de empleados con la app final de empleados/contratación cuando exista.
- Integrar recompensas reales de logros para sumar puntos de progreso; por ahora existe `AddPointsForInternalTesting` solo para editor.

## Evidencia
- `Reportes/Dotnet_Fase1_Arbol_Build.log`
- `Reportes/Unity_Fase1_Arbol_Compile_Attempt.log`
