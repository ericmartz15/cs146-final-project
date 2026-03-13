// BusBehavior.cs
// Moves a bus back and forth between two waypoints.
// Set pointA and pointB by dragging empty GameObjects into the Inspector.
// If the bus hits the player, the player is instantly killed.

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BusBehavior : MonoBehaviour
{
    [Header("Waypoints")]
    [Tooltip("Drag an empty GameObject here to mark the first endpoint")]
    [SerializeField] private Transform pointA;
    [Tooltip("Drag an empty GameObject here to mark the second endpoint")]
    [SerializeField] private Transform pointB;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Pause at ends (optional)")]
    [SerializeField] private float pauseDuration = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform targetPoint;
    private bool isPausing = false;
    private float pauseTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (pointA == null || pointB == null)
        {
            Debug.LogError("BusBehavior: pointA and pointB must be assigned in the Inspector!");
            enabled = false;
            return;
        }

        targetPoint = pointB;
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

        Vector2 direction = ((Vector2)targetPoint.position - rb.position).normalized;
        rb.velocity = direction * moveSpeed;

        // Flip sprite to face direction of travel
        if (Mathf.Abs(direction.x) > 0.01f)
            spriteRenderer.flipX = direction.x < 0f;

        // Check if we have reached the target point
        if (Vector2.Distance(rb.position, targetPoint.position) < 0.1f)
        {
            rb.position = targetPoint.position;
            targetPoint = (targetPoint == pointA) ? pointB : pointA;

            if (pauseDuration > 0f)
            {
                isPausing = true;
                pauseTimer = pauseDuration;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Bus collided with: " + collision.gameObject.name);
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.TakeDamage();
    }

    private void OnDrawGizmosSelected()
    {
        if (pointA == null || pointB == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pointA.position, pointB.position);
        Gizmos.DrawWireSphere(pointA.position, 0.3f);
        Gizmos.DrawWireSphere(pointB.position, 0.3f);
    }
}