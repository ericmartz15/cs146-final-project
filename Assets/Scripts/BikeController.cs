// BikeController.cs
// Smooth top-down movement for the player bike.
// Checks road tilemap before moving, same approach as MoveScript.

using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BikeController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Tilemap References")]
    [SerializeField] private Tilemap roadTilemap;
    [SerializeField] private Tilemap obstacleTilemap;

    [Header("Collision")]
    [SerializeField] private float edgeOffset = 0.4f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 movement;
    private PlayerHealth playerHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        if (movement.x > 0f)
            spriteRenderer.flipX = true;
        else if (movement.x < 0f)
            spriteRenderer.flipX = false;
    }

    void FixedUpdate()
    {
        if (roadTilemap == null)
        {
            rb.velocity = movement * moveSpeed;
            return;
        }

        Vector2 edgePos = rb.position + movement.normalized * edgeOffset;
        Vector2 nextPos = edgePos + movement * moveSpeed * Time.fixedDeltaTime;
        Vector3Int nextCell = roadTilemap.WorldToCell(nextPos);

        if (roadTilemap.HasTile(nextCell))
        {
            rb.velocity = movement * moveSpeed;
        }
        else
        {
            Vector2 moveX = new Vector2(movement.x, 0f);
            Vector2 moveY = new Vector2(0f, movement.y);

            Vector3Int cellX = roadTilemap.WorldToCell(edgePos + moveX * moveSpeed * Time.fixedDeltaTime);
            Vector3Int cellY = roadTilemap.WorldToCell(edgePos + moveY * moveSpeed * Time.fixedDeltaTime);

            Vector2 constrainedMovement = Vector2.zero;
            if (roadTilemap.HasTile(cellX)) constrainedMovement.x = movement.x;
            if (roadTilemap.HasTile(cellY)) constrainedMovement.y = movement.y;

            rb.velocity = constrainedMovement * moveSpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("BikeController collision with: " + collision.gameObject.name);
        if (collision.gameObject.GetComponent<MotorcycleBehavior>() != null)
        {
            Debug.Log("Hit by motorcycle - triggering death");
            if (playerHealth != null && !playerHealth.IsDead)
                playerHealth.TakeDamage(playerHealth.MaxLives);
        }
    }
}