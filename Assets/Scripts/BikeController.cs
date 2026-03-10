// BikeController.cs
// Smooth top-down movement for the player bike.

using System.Collections;
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

    private bool isStopped = false;

    // Boost state tracked separately from stop state
    private bool isBoosted = false;
    private float boostMultiplier = 1f;
    private Coroutine activeBoost;
    private Coroutine activeStop;

    // The speed actually used for movement — accounts for both boost and stop
    private float CurrentSpeed => isStopped ? 0f : moveSpeed * (isBoosted ? boostMultiplier : 1f);

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (isStopped) return;

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
        if (isStopped)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (roadTilemap == null)
        {
            rb.velocity = movement * CurrentSpeed;
            return;
        }

        Vector2 edgePos = rb.position + movement.normalized * edgeOffset;
        Vector2 nextPos = edgePos + movement * CurrentSpeed * Time.fixedDeltaTime;
        Vector3Int nextCell = roadTilemap.WorldToCell(nextPos);

        if (roadTilemap.HasTile(nextCell))
        {
            rb.velocity = movement * CurrentSpeed;
        }
        else
        {
            Vector2 moveX = new Vector2(movement.x, 0f);
            Vector2 moveY = new Vector2(0f, movement.y);

            Vector3Int cellX = roadTilemap.WorldToCell(edgePos + moveX * CurrentSpeed * Time.fixedDeltaTime);
            Vector3Int cellY = roadTilemap.WorldToCell(edgePos + moveY * CurrentSpeed * Time.fixedDeltaTime);

            Vector2 constrainedMovement = Vector2.zero;
            if (roadTilemap.HasTile(cellX)) constrainedMovement.x = movement.x;
            if (roadTilemap.HasTile(cellY)) constrainedMovement.y = movement.y;

            rb.velocity = constrainedMovement * CurrentSpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<MotorcycleBehavior>() != null ||
            collision.gameObject.GetComponent<BusBehavior>() != null ||
            collision.gameObject.GetComponent<BusSquare>() != null)
        {
            if (playerHealth != null && !playerHealth.IsDead)
                playerHealth.TakeDamage(playerHealth.MaxLives);
        }
    }

    /// <summary>
    /// Stops the bike for a given duration.
    /// The boost state is preserved and resumes correctly afterwards.
    /// </summary>
    public void StopBriefly(float duration)
    {
        if (activeStop != null) StopCoroutine(activeStop);
        activeStop = StartCoroutine(StopCoroutine(duration));
    }

    private IEnumerator StopCoroutine(float duration)
    {
        isStopped = true;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(duration);
        isStopped = false;
        activeStop = null;
        // Speed automatically restores via CurrentSpeed property — boost is unaffected
    }

    /// <summary>
    /// Applies a speed boost for a given duration.
    /// If a boost is already active it is replaced cleanly.
    /// Stopping the bike does not cancel or corrupt the boost timer.
    /// </summary>
    public void StartSpeedBoost(float multiplier, float duration)
    {
        if (activeBoost != null) StopCoroutine(activeBoost);
        activeBoost = StartCoroutine(SpeedBoostCoroutine(multiplier, duration));
    }

    private IEnumerator SpeedBoostCoroutine(float multiplier, float duration)
    {
        isBoosted = true;
        boostMultiplier = multiplier;
        Debug.Log("Speed boost started: x" + multiplier);

        yield return new WaitForSeconds(duration);

        isBoosted = false;
        boostMultiplier = 1f;
        activeBoost = null;
        Debug.Log("Speed boost ended.");
    }
}