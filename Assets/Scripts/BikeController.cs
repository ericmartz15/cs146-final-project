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
    [SerializeField] private Tilemap roadTilemap;       // Road Map - where the bike can drive
    [SerializeField] private Tilemap obstacleTilemap;   // Obstacle Map - blocked tiles

    [Header("Collision")]
    [SerializeField] private float edgeOffset = 0.4f;  // Half-width of bike sprite — tune this in Inspector

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Read arrow keys or WASD
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Normalize so diagonal movement isn't faster
        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        // Flip sprite to face left/right
        if (movement.x > 0f)
            spriteRenderer.flipX = true;
        else if (movement.x < 0f)
            spriteRenderer.flipX = false;
    }

    void FixedUpdate()
    {
        if (roadTilemap == null)
        {
            // No tilemap assigned — move freely
            rb.velocity = movement * moveSpeed;
            return;
        }

        // Check the EDGE of the bike in the movement direction (not just the center)
        // edgeOffset pushes the check point to the leading edge of the sprite
        Vector2 edgePos = rb.position + movement.normalized * edgeOffset;
        Vector2 nextPos = edgePos + movement * moveSpeed * Time.fixedDeltaTime;
        Vector3Int nextCell = roadTilemap.WorldToCell(nextPos);

        if (roadTilemap.HasTile(nextCell))
        {
            rb.velocity = movement * moveSpeed;
        }
        else
        {
            // Try sliding along road edges: check X and Y axes separately
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
}
