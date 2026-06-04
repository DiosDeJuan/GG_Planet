using UnityEngine;
using UnityEngine.AI;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Runtime validator and fallback for the WarehouseZone.
    ///
    /// PRIMARY ROLE (Fase 3+):
    ///   • Detect whether WarehouseZone is already present in the scene (persisted
    ///     by ShopMasterWarehouseSceneSetupRunner).
    ///   • If found: validate critical children and assign EmployeeSpawnPoint to
    ///     EmployeeNPCSpawner.  Log "Using existing scene WarehouseZone".
    ///   • If missing: create a runtime fallback (YELLOW state) so gameplay still
    ///     boots, but log a [WARN] so the operator knows to run the scene setup.
    ///
    /// IMPORTANT — NavMesh:
    ///   A runtime-created WarehouseZone is NOT baked into the NavMesh because
    ///   Unity only bakes against geometry that exists at Editor bake time.
    ///   For GREEN state the zone must be in the scene as a persistent object
    ///   (run ShopMasterWarehouseSceneSetupRunner, then ShopMasterNavMeshRebuildRunner).
    ///
    /// Anti-duplication:
    ///   Uses FindWarehouseZone() before creating anything.  Safe to enter/exit
    ///   Play Mode multiple times without accumulating duplicate objects.
    /// </summary>
    [DisallowMultipleComponent]
    public class WarehouseZoneBootstrap : MonoBehaviour
    {
        private const string LogPrefix = "[WarehouseZone] ";

        // Half-extents — kept in sync with ShopMasterWarehouseSceneSetupRunner.
        private const float HalfWidth  = 4f;
        private const float HalfDepth  = 3f;
        private const float WallHeight = 3f;
        private const float WallThick  = 0.25f;

        // NavMesh snap search radius for EmployeeSpawnPoint placement.
        private const float NavSnapRadius = 10f;

        // Cached reference to the zone found or created by this bootstrap.
        private Transform _warehouseZone;

        // Whether the zone was created at runtime (not persisted = YELLOW state).
        private bool _isRuntimeFallback;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        void Awake()
        {
            _warehouseZone = EnsureWarehouseZone();
        }

        void Start()
        {
            if (_isRuntimeFallback)
            {
                Debug.LogWarning(LogPrefix + "[WARN] WarehouseZone es un fallback de runtime — "
                    + "los NPCs pueden no estar sobre el NavMesh. "
                    + "Ejecutar ShopMasterWarehouseSceneSetupRunner + ShopMasterNavMeshRebuildRunner "
                    + "para persistir la zona y rebakear el NavMesh. Estado: AMARILLO.");
            }

            AssignEmployeeSpawnPoint();
        }

        // ── Zone creation / validation ────────────────────────────────────────────

        private Transform EnsureWarehouseZone()
        {
            // Check if the zone already exists (persisted in scene by Editor script).
            Transform existing = FindWarehouseZone();
            if (existing != null)
            {
                Debug.Log(LogPrefix + "Using existing scene WarehouseZone: '"
                    + existing.name + "' at " + existing.position + ".");

                // Validate and repair critical children without creating duplicates.
                EnsureCriticalChildren(existing.gameObject);
                _isRuntimeFallback = false;
                return existing;
            }

            // ── No persistent zone found — create runtime fallback ────────────────
            Debug.LogWarning(LogPrefix + "[WARN] WarehouseZone not found in scene — "
                + "creating runtime fallback. Estado: AMARILLO. "
                + "Para corregir: ejecutar ShopMasterWarehouseSceneSetupRunner en Unity Editor.");

            _isRuntimeFallback = true;
            return CreateRuntimeFallback();
        }

        private void EnsureCriticalChildren(GameObject zone)
        {
            // EmployeeSpawnPoint — required by EmployeeNPCSpawner.
            if (zone.transform.Find("EmployeeSpawnPoint") == null)
            {
                GameObject spawnPt = new GameObject("EmployeeSpawnPoint");
                spawnPt.transform.SetParent(zone.transform, false);
                spawnPt.transform.localPosition = new Vector3(0f, 0.05f, -HalfDepth * 0.5f);
                spawnPt.transform.localRotation  = Quaternion.Euler(0f, 180f, 0f);
                Debug.Log(LogPrefix + "EmployeeSpawnPoint missing — created as runtime repair.");
            }

            // PackageDropArea — informational, not required for spawner.
            if (zone.transform.Find("PackageDropArea") == null)
                Debug.LogWarning(LogPrefix + "[WARN] PackageDropArea not found under WarehouseZone.");

            // DeliveryStartPoint — informational.
            if (zone.transform.Find("DeliveryStartPoint") == null)
                Debug.LogWarning(LogPrefix + "[WARN] DeliveryStartPoint not found under WarehouseZone.");
        }

        private Transform CreateRuntimeFallback()
        {
            // Determine world origin: center on DeliveryStart so the runner's
            // distance check (< 10 m) is satisfied even in fallback mode.
            Vector3 origin = ResolveZoneOrigin();

            GameObject zone = new GameObject("WarehouseZone");
            zone.transform.position = origin;

            // 3 walls.
            CreateWallBox(zone, "WarehouseWall_Back",
                localPos: new Vector3(0f, WallHeight * 0.5f, -HalfDepth),
                size:      new Vector3(HalfWidth * 2f + WallThick * 2f, WallHeight, WallThick));

            CreateWallBox(zone, "WarehouseWall_Left",
                localPos: new Vector3(-HalfWidth, WallHeight * 0.5f, 0f),
                size:      new Vector3(WallThick, WallHeight, HalfDepth * 2f));

            CreateWallBox(zone, "WarehouseWall_Right",
                localPos: new Vector3(HalfWidth, WallHeight * 0.5f, 0f),
                size:      new Vector3(WallThick, WallHeight, HalfDepth * 2f));

            // Floor.
            CreateWallBox(zone, "WarehouseFloor",
                localPos: new Vector3(0f, -0.05f, 0f),
                size:      new Vector3(HalfWidth * 2f, 0.1f, HalfDepth * 2f));

            // Wide door (open frame — two pillars).
            GameObject door = new GameObject("WarehouseWideDoor");
            door.transform.SetParent(zone.transform, false);
            door.transform.localPosition = new Vector3(0f, 0f, HalfDepth);

            CreateWallBox(door, "DoorPillar_Left",
                localPos: new Vector3(-HalfWidth + 0.6f, WallHeight * 0.5f, 0f),
                size:      new Vector3(WallThick, WallHeight, WallThick));

            CreateWallBox(door, "DoorPillar_Right",
                localPos: new Vector3(HalfWidth - 0.6f, WallHeight * 0.5f, 0f),
                size:      new Vector3(WallThick, WallHeight, WallThick));

            // EmployeeSpawnPoint (NavMesh-snapped in Start).
            GameObject spawnPt = new GameObject("EmployeeSpawnPoint");
            spawnPt.transform.SetParent(zone.transform, false);
            spawnPt.transform.localPosition = new Vector3(0f, 0.05f, -HalfDepth * 0.5f);
            spawnPt.transform.localRotation  = Quaternion.Euler(0f, 180f, 0f);

            // PackageDropArea.
            GameObject dropArea = new GameObject("PackageDropArea");
            dropArea.transform.SetParent(zone.transform, false);
            dropArea.transform.localPosition = new Vector3(0f, 0.05f, HalfDepth * 0.4f);

            Debug.Log(LogPrefix + "[FALLBACK] WarehouseZone created at " + origin
                + " (DeliveryStart=" + GetDeliveryStartPosition() + "). NavMesh coverage NOT guaranteed.");
            return zone.transform;
        }

        private static void CreateWallBox(GameObject parent, string name, Vector3 localPos, Vector3 size)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent.transform, false);
            wall.transform.localPosition = localPos;
            wall.transform.localScale    = size;

            // Renderer is intentionally left active so the zone is visible.
            // For a production build, replace with proper store-asset materials here.
        }

        // ── EmployeeSpawnPoint assignment ─────────────────────────────────────────

        private void AssignEmployeeSpawnPoint()
        {
            EmployeeNPCSpawner spawner = EmployeeNPCSpawner.Instance;
            if (spawner == null)
            {
                Debug.LogWarning(LogPrefix + "EmployeeNPCSpawner.Instance not found — cannot assign EmployeeSpawnPoint.");
                return;
            }

            if (spawner.employeeSpawnPoint != null)
            {
                Debug.Log(LogPrefix + "EmployeeNPCSpawner already has employeeSpawnPoint assigned: '"
                    + spawner.employeeSpawnPoint.name + "'.");
                return;
            }

            Transform spawnPt = FindEmployeeSpawnPoint();
            if (spawnPt == null)
            {
                Debug.LogWarning(LogPrefix + "[WARN] EmployeeSpawnPoint not found in scene — falling back to NPC spawner's own logic.");
                return;
            }

            // Snap to nearest NavMesh so the NPC lands on a walkable surface.
            Vector3 snapped = SnapToNavMesh(spawnPt.position, NavSnapRadius);
            if (snapped != spawnPt.position)
            {
                spawnPt.position = snapped;
                Debug.Log(LogPrefix + "EmployeeSpawnPoint snapped to NavMesh at " + snapped + ".");
            }

            spawner.employeeSpawnPoint = spawnPt;
            Debug.Log(LogPrefix + "Assigned EmployeeSpawnPoint ('" + spawnPt.name + "' at "
                + spawnPt.position + ") to EmployeeNPCSpawner.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        /// <summary>Returns the world position used as the zone's center.</summary>
        private static Vector3 ResolveZoneOrigin()
        {
            Vector3 ds = GetDeliveryStartPosition();
            if (ds != Vector3.zero)
                return ds;

            // Last resort: stay near world origin.
            Debug.LogWarning(LogPrefix + "[WARN] DeliveryStart not available — placing WarehouseZone at origin.");
            return Vector3.zero;
        }

        private static Vector3 GetDeliveryStartPosition()
        {
            DeliverySystem ds = DeliverySystem.Instance;
            if (ds != null && ds.deliveryStart != null)
                return ds.deliveryStart.position;
            return Vector3.zero;
        }

        private static Transform FindWarehouseZone()
        {
            GameObject exact = GameObject.Find("WarehouseZone");
            if (exact != null)
                return exact.transform;

#if UNITY_2022_2_OR_NEWER
            GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            GameObject[] all = Object.FindObjectsOfType<GameObject>(true);
#endif
            foreach (GameObject go in all)
                if (go != null && go.name.ToLower().Contains("warehouse") && go.transform.parent == null)
                    return go.transform;
            foreach (GameObject go in all)
                if (go != null && go.name.ToLower().Contains("warehousezone"))
                    return go.transform;
            return null;
        }

        private static Transform FindEmployeeSpawnPoint()
        {
            // Prefer the child inside the WarehouseZone.
            GameObject byPath = GameObject.Find("WarehouseZone/EmployeeSpawnPoint");
            if (byPath != null)
                return byPath.transform;

            // Fall back to any GameObject named EmployeeSpawnPoint.
#if UNITY_2022_2_OR_NEWER
            GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            GameObject[] all = Object.FindObjectsOfType<GameObject>(true);
#endif
            foreach (GameObject go in all)
                if (go != null && go.name == "EmployeeSpawnPoint")
                    return go.transform;

            return null;
        }

        /// <summary>
        /// Returns the nearest NavMesh position within <paramref name="radius"/> metres,
        /// or <paramref name="position"/> unchanged if no NavMesh is found.
        /// </summary>
        private static Vector3 SnapToNavMesh(Vector3 position, float radius)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(position, out hit, radius, NavMesh.AllAreas))
                return hit.position;

            Debug.LogWarning(LogPrefix + "[WARN] NavMesh.SamplePosition found no mesh within "
                + radius + "m of " + position + ". EmployeeSpawnPoint will not be on NavMesh — "
                + "rebuild NavMesh in Unity Editor (Window → AI → Navigation → Bake).");
            return position;
        }
    }
}
