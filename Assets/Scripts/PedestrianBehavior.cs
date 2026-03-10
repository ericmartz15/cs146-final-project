// PedestrianBehavior.cs
// Pedestrian walks back and forth across the road.
// When hit by the bike, motorcycle, or bus, they spin and fly away.
// Only the bike stops on impact.

using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PedestrianBehavior : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 1f;
    [SerializeField] private float pauseDuration = 1f;

    [Header("On Hit")]
    [SerializeField] private float hitForce = 5f;
    [SerializeField] private float slideStopTime = 0.8f;
    [SerializeField] private float bikeStopTime = 1.5f;
    [SerializeField] private float spinForce = 50f;

    [Header("Tilemap")]
    [SerializeField] private Tilemap roadTilemap;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Vector3 pointA;
    private Vector3 pointB;
    private Vector3 target;

    private bool isPausing = false;
    private float pauseTimer = 0f;
    private bool initialized = false;
    private bool hasBeenHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Initialize(Tilemap road)
    {
        roadTilemap = road;
        pointA = transform.position;

        Vector3 foundB = FindOtherSide(Vector3Int.right);
        if (foundB == Vector3.zero)
            foundB = FindOtherSide(Vector3Int.up);
        if (foundB == Vector3.zero)
            foundB = transform.position + new Vector3(1.5f, 0f, 0f);

        pointB = foundB;
        target = pointB;
        initialized = true;
    }

    private Vector3 FindOtherSide(Vector3Int direction)
    {
        Vector3Int currentCell = roadTilemap.WorldToCell(transform.position);
        Vector3Int lastRoadCell = currentCell;

        for (int i = 1; i <= 20; i++)
        {
            Vector3Int next = currentCell + direction * i;
            if (roadTilemap.HasTile(next))
                lastRoadCell = next;
            else
                break;
        }

        if (lastRoadCell == currentCell) return Vector3.zero;
        return roadTilemap.GetCellCenterWorld(lastRoadCell);
    }

    void FixedUpdate()
    {
        if (!initialized || hasBeenHit) return;

        if (isPausing)
        {
            rb.velocity = Vector2.zero;
            pauseTimer -= Time.fixedDeltaTime;
            if (pauseTimer <= 0f)
                isPausing = false;
            return;
        }

        Vector2 direction = (target - transform.position).normalized;
        rb.velocity = direction * walkSpeed;

        if (Mathf.Abs(direction.x) > 0.01f)
            spriteRenderer.flipX = direction.x < 0f;

        if (Vector2.Distance(rb.position, target) < 0.15f)
        {
            rb.position = target;
            target = (target == pointA) ? pointB : pointA;
            isPausing = true;
            pauseTimer = pauseDuration;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject, collision.transform.position);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other.gameObject, other.transform.position);
    }

    private void HandleHit(GameObject other, Vector3 otherPosition)
    {
        if (hasBeenHit) return;

        bool hitByBike       = other.GetComponent<BikeController>()       != null;
        bool hitByMotorcycle = other.GetComponent<MotorcycleBehavior>()   != null;
        bool hitByBus        = other.GetComponent<BusBehavior>()          != null;

        if (!hitByBike && !hitByMotorcycle && !hitByBus) return;

        // Only the bike stops — motorcycles and buses keep moving
        if (hitByBike)
        {
            BikeController bike = other.GetComponent<BikeController>();
            bike.StopBriefly(bikeStopTime);
        }

        // Knock and spin the pedestrian
        hasBeenHit = true;
        rb.freezeRotation = false;

        Vector2 knockDirection = (transform.position - otherPosition).normalized;
        rb.velocity = Vector2.zero;
        rb.AddForce(knockDirection * hitForce, ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-spinForce, spinForce), ForceMode2D.Impulse);

        Invoke(nameof(StopSliding), slideStopTime);
    }

    private void StopSliding()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}