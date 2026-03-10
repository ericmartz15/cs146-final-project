// BusSquare.cs
// Moves a bus in a square loop between four waypoints.
// If the bus hits the player, the player is instantly killed.

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BusSquare : MonoBehaviour
{
    [Header("Waypoints (place in order around the square)")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private Transform pointC;
    [SerializeField] private Transform pointD;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Pause at corners (optional)")]
    [SerializeField] private float pauseDuration = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private bool isPausing = false;
    private float pauseTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (pointA == null || pointB == null || pointC == null || pointD == null)
        {
            Debug.LogError("BusSquare: All four waypoints must be assigned in the Inspector!");
            enabled = false;
            return;
        }

        waypoints = new Transform[] { pointA, pointB, pointC, pointD };
    }

    void FixedUpdate()
    {
        if (isPausing)
        {
            rb.velocity = Vector2.zero;
            pauseTimer -= Time.fixedDeltaTime;
            if (pauseTimer <= 0f)
                isPausing = false;
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        Vector2 direction = ((Vector2)target.position - rb.position).normalized;
        rb.velocity = direction * moveSpeed;

        if (Mathf.Abs(direction.x) > 0.01f)
            spriteRenderer.flipX = direction.x < 0f;

        if (Vector2.Distance(rb.position, target.position) < 0.1f)
        {
            rb.position = target.position;
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;

            if (pauseDuration > 0f)
            {
                isPausing = true;
                pauseTimer = pauseDuration;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("BusSquare collided with: " + collision.gameObject.name);
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null && !playerHealth.IsDead)
            playerHealth.TakeDamage(playerHealth.MaxLives);
    }

    private void OnDrawGizmosSelected()
    {
        if (pointA == null || pointB == null || pointC == null || pointD == null) return;

        Transform[] points = { pointA, pointB, pointC, pointD };

        Gizmos.color = Color.cyan;
        for (int i = 0; i < points.Length; i++)
        {
            Gizmos.DrawWireSphere(points[i].position, 0.3f);
            Gizmos.DrawLine(points[i].position, points[(i + 1) % points.Length].position);
        }
    }
}