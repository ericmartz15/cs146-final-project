// MotorcycleBehavior.cs
// Enemy motorcycle that chases the player by pathfinding along road tiles using BFS.
// Stays on road because it follows a road-tile path rather than moving in a straight line.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class MotorcycleBehavior : MonoBehaviour
{
    [Header("Tilemap")]
    [SerializeField] private Tilemap roadTilemap;           // Road Map — motorcycle paths along this

    [Header("Detection")]
    [SerializeField] private float chaseRange = 8f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float wanderSpeed = 1.5f;

    [Header("Pathfinding")]
    [SerializeField] private float pathUpdateRate = 0.5f;       // How often to recalculate path
    [SerializeField] private float waypointReachDistance = 0.3f; // How close = reached waypoint
    [SerializeField] private int maxBFSIterations = 1000;        // Prevent freeze on huge maps

    [Header("Wander")]
    [SerializeField] private float wanderRadius = 5f;

    [Header("Damage")]
    [SerializeField] private float contactDamageRadius = 0.6f;
    [SerializeField] private float damageCooldown = 1f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private GameObject player;

    private List<Vector3> currentPath = new List<Vector3>();
    private int currentWaypoint = 0;
    private float lastPathTime = -999f;
    private float lastDamageTime;
    private Vector3 startPosition;
    private Vector3 wanderTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        player = GameObject.FindGameObjectWithTag("Player");
        lastDamageTime = -damageCooldown;
        startPosition = transform.position;
        PickNewWanderTarget();
    }

    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
        }

        float distToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distToPlayer <= chaseRange)
        {
            // Player is in range — recalculate BFS path toward player periodically
            if (Time.time >= lastPathTime + pathUpdateRate)
            {
                lastPathTime = Time.time;
                currentPath = FindPathBFS(transform.position, player.transform.position);
                currentWaypoint = 0;
            }
        }
        else
        {
            // Player out of range — clear any chase path so wander takes over
            currentPath = null;
        }

        // Contact damage
        if (distToPlayer <= contactDamageRadius && Time.time >= lastDamageTime + damageCooldown)
        {
            lastDamageTime = Time.time;
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(1);
        }
    }

    void FixedUpdate()
    {
        float distToPlayer = player != null ? Vector3.Distance(transform.position, player.transform.position) : float.MaxValue;

        if (distToPlayer <= chaseRange && currentPath != null && currentPath.Count > 0 && currentWaypoint < currentPath.Count)
        {
            // Chase: follow BFS path
            Vector2 waypoint = currentPath[currentWaypoint];
            Vector2 direction = (waypoint - rb.position).normalized;
            rb.velocity = direction * chaseSpeed;

            if (Mathf.Abs(direction.x) > 0.01f)
                spriteRenderer.flipX = direction.x < 0f;

            if (Vector2.Distance(rb.position, waypoint) < waypointReachDistance)
                currentWaypoint++;
        }
        else
        {
            // Wander: simple movement toward a random road tile, no BFS needed
            Wander();
        }
    }

    private void Wander()
    {
        if (roadTilemap == null) return;

        Vector2 toTarget = (wanderTarget - transform.position);
        if (toTarget.magnitude < waypointReachDistance)
        {
            rb.velocity = Vector2.zero;
            PickNewWanderTarget();
            return;
        }

        Vector2 direction = toTarget.normalized;
        rb.velocity = direction * wanderSpeed;

        if (Mathf.Abs(direction.x) > 0.01f)
            spriteRenderer.flipX = direction.x < 0f;
    }

    // BFS: find a path from startWorld to goalWorld along road tiles
    private List<Vector3> FindPathBFS(Vector3 startWorld, Vector3 goalWorld)
    {
        if (roadTilemap == null) return new List<Vector3>();

        Vector3Int startCell = roadTilemap.WorldToCell(startWorld);
        Vector3Int goalCell  = roadTilemap.WorldToCell(goalWorld);

        if (startCell == goalCell) return new List<Vector3>();

        Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        frontier.Enqueue(startCell);
        cameFrom[startCell] = startCell;

        Vector3Int[] neighbors = {
            Vector3Int.up, Vector3Int.down,
            Vector3Int.left, Vector3Int.right
        };

        int iterations = 0;
        bool found = false;

        while (frontier.Count > 0 && iterations < maxBFSIterations)
        {
            iterations++;
            Vector3Int current = frontier.Dequeue();

            if (current == goalCell) { found = true; break; }

            foreach (Vector3Int dir in neighbors)
            {
                Vector3Int next = current + dir;
                if (cameFrom.ContainsKey(next)) continue;
                if (!roadTilemap.HasTile(next)) continue;   // Only travel on road tiles

                frontier.Enqueue(next);
                cameFrom[next] = current;
            }
        }

        if (!found) return new List<Vector3>();

        // Reconstruct path from goal back to start
        List<Vector3> path = new List<Vector3>();
        Vector3Int step = goalCell;
        while (step != startCell)
        {
            path.Add(roadTilemap.GetCellCenterWorld(step));
            step = cameFrom[step];
        }
        path.Reverse();
        return path;
    }

    private void PickNewWanderTarget()
    {
        if (roadTilemap == null) return;

        for (int i = 0; i < 10; i++)
        {
            Vector2 offset = Random.insideUnitCircle * wanderRadius;
            Vector3 candidate = startPosition + new Vector3(offset.x, offset.y, 0f);
            Vector3Int cell = roadTilemap.WorldToCell(candidate);

            if (roadTilemap.HasTile(cell))
            {
                wanderTarget = roadTilemap.GetCellCenterWorld(cell);
                return;
            }
        }
        wanderTarget = startPosition;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, contactDamageRadius);

        // Draw the current planned path in green
        if (currentPath != null && currentPath.Count > 1)
        {
            Gizmos.color = Color.green;
            for (int i = currentWaypoint; i < currentPath.Count - 1; i++)
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
        }
    }

    public void ResetMotorcycle()
    {
        transform.position = startPosition;
        if (rb != null) rb.velocity = Vector2.zero;
        currentPath = null;
        lastDamageTime = -damageCooldown;
        PickNewWanderTarget();
    }
}
