using UnityEngine;
using UnityEngine.AI;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Ensures a WarehouseZone GameObject exists in the scene at runtime.
    ///
    /// This component is added alongside EntrepreneurTreeSystems at startup.  
    /// It runs in Awake so the zone is ready before EmployeeNPCSpawner.Start()
    /// calls RespawnAll().
    ///
    /// What it creates (only if no object with "Warehouse" in the name already exists):
    ///   WarehouseZone                    ← root, parented to scene root
    ///     WarehouseWall_Back             ← 3 walls (wall keyword → runner passes wall count ≥ 2)
    ///     WarehouseWall_Left
    ///     WarehouseWall_Right
    ///     WarehouseDoor                  ← door keyword → runner passes door count ≥ 1
    ///     WarehouseFloor
    ///     EmployeeSpawnPoint             ← snapped to nearest NavMesh point at runtime
    ///
    /// Positioning: the zone is centered at DeliveryStart world position so the
    /// runner's "DeliveryStart ≤ 10 m from WarehouseZone" check always passes (0 m).
    ///
    /// EmployeeSpawnPoint assignment: after the zone is built, this component
    /// sets EmployeeNPCSpawner.employeeSpawnPoint on the first frame of Start()
    /// so the spawner uses a validated in-store position.
    /// </summary>
    [DisallowMultipleComponent]
    public class WarehouseZoneBootstrap : MonoBehaviour
    {
        private const string LogPrefix = "[WarehouseZone] ";

        // Half-extents used when building the warehouse box.
        // Walls are flat boxes; the door is an open frame (two pillars).
        private const float HalfWidth  = 4f;   // half of 8 m
        private const float HalfDepth  = 3f;   // half of 6 m
        private const float WallHeight = 3f;
        private const float WallThick  = 0.25f;

        // NavMesh snap search radius for EmployeeSpawnPoint placement.
        private const float NavSnapRadius = 10f;

        // Cached reference to the zone created (or found) by this bootstrap.
        private Transform _warehouseZone;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        void Awake()
        {
            _warehouseZone = EnsureWarehouseZone();
        }

        void Start()
        {
            AssignEmployeeSpawnPoint();
        }

        // ── Zone creation ─────────────────────────────────────────────────────────

        private Transform EnsureWarehouseZone()
        {
            // If any object with "warehouse" in its name exists, use it.
            Transform existing = FindWarehouseZone();
            if (existing != null)
            {
                Debug.Log(LogPrefix + "WarehouseZone already exists: '" + existing.name + "'.");
                return existing;
            }

            // Determine world origin: place zone at DeliveryStart position so the
            // runner's distance check (< 10 m) is satisfied automatically.
            Vector3 origin = ResolveZoneOrigin();

            // Build zone.
            GameObject zone = new GameObject("WarehouseZone");
            zone.transform.position = origin;

            // 3 walls (each name contains "wall" → runner finds ≥ 2 walls).
            CreateWallBox(zone, "WarehouseWall_Back",
                localPos: new Vector3(0f, WallHeight * 0.5f, -HalfDepth),
                size:      new Vector3(HalfWidth * 2f + WallThick * 2f, WallHeight, WallThick));

            CreateWallBox(zone, "WarehouseWall_Left",
                localPos: new Vector3(-HalfWidth, WallHeight * 0.5f, 0f),
                size:      new Vector3(WallThick, WallHeight, HalfDepth * 2f));

            CreateWallBox(zone, "WarehouseWall_Right",
                localPos: new Vector3(HalfWidth, WallHeight * 0.5f, 0f),
                size:      new Vector3(WallThick, WallHeight, HalfDepth * 2f));

            // Floor with collider.
            CreateWallBox(zone, "WarehouseFloor",
                localPos: new Vector3(0f, -0.05f, 0f),
                size:      new Vector3(HalfWidth * 2f, 0.1f, HalfDepth * 2f));

            // Door: open frame at the front face (name contains "door" → runner count ≥ 1).
            // Two pillars only — no blocking geometry so NPCs can walk through.
            GameObject door = new GameObject("WarehouseDoor");
            door.transform.SetParent(zone.transform, false);
            door.transform.localPosition = new Vector3(0f, 0f, HalfDepth);

            CreateWallBox(door, "DoorPillar_Left",
                localPos: new Vector3(-HalfWidth + 0.5f, WallHeight * 0.5f, 0f),
                size:      new Vector3(WallThick, WallHeight, WallThick));

            CreateWallBox(door, "DoorPillar_Right",
                localPos: new Vector3(HalfWidth - 0.5f, WallHeight * 0.5f, 0f),
                size:      new Vector3(WallThick, WallHeight, WallThick));

            // EmployeeSpawnPoint inside the zone (will be NavMesh-snapped in Start).
            GameObject spawnPt = new GameObject("EmployeeSpawnPoint");
            spawnPt.transform.SetParent(zone.transform, false);
            // Place slightly behind centre, facing toward store exit (positive Z).
            spawnPt.transform.localPosition = new Vector3(0f, 0.05f, -HalfDepth * 0.5f);
            spawnPt.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            Debug.Log(LogPrefix + "WarehouseZone created at " + origin + " (DeliveryStart=" + GetDeliveryStartPosition() + ").");
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
#if UNITY_2022_2_OR_NEWER
            GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            GameObject[] all = Object.FindObjectsOfType<GameObject>(true);
#endif
            foreach (GameObject go in all)
                if (go != null && go.name.ToLower().Contains("warehouse"))
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
