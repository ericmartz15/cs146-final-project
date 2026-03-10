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

    private float currentSpeed;
    private Coroutine activeBoost;
    private bool isStopped = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();
        rb.freezeRotation = true;
        currentSpeed = moveSpeed;
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
            rb.velocity = movement * currentSpeed;
            return;
        }

        Vector2 edgePos = rb.position + movement.normalized * edgeOffset;
        Vector2 nextPos = edgePos + movement * currentSpeed * Time.fixedDeltaTime;
        Vector3Int nextCell = roadTilemap.WorldToCell(nextPos);

        if (roadTilemap.HasTile(nextCell))
        {
            rb.velocity = movement * currentSpeed;
        }
        else
        {
            Vector2 moveX = new Vector2(movement.x, 0f);
            Vector2 moveY = new Vector2(0f, movement.y);

            Vector3Int cellX = roadTilemap.WorldToCell(edgePos + moveX * currentSpeed * Time.fixedDeltaTime);
            Vector3Int cellY = roadTilemap.WorldToCell(edgePos + moveY * currentSpeed * Time.fixedDeltaTime);

            Vector2 constrainedMovement = Vector2.zero;
            if (roadTilemap.HasTile(cellX)) constrainedMovement.x = movement.x;
            if (roadTilemap.HasTile(cellY)) constrainedMovement.y = movement.y;

            rb.velocity = constrainedMovement * currentSpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<MotorcycleBehavior>() != null ||
            collision.gameObject.GetComponent<BusBehavior>() != null)
        {
            if (playerHealth != null && !playerHealth.IsDead)
                playerHealth.TakeDamage(playerHealth.MaxLives);
        }
    }

    /// <summary>
    /// Stops the bike for a given duration — called when hitting a pedestrian.
    /// </summary>
    public void StopBriefly(float duration)
    {
        if (activeBoost != null) StopCoroutine(activeBoost);
        StartCoroutine(StopCoroutine(duration));
    }

    private IEnumerator StopCoroutine(float duration)
    {
        isStopped = true;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(duration);
        isStopped = false;
    }

    public void StartSpeedBoost(float speedMultiplier, float boostDuration)
    {
        if (activeBoost != null)
            StopCoroutine(activeBoost);
        activeBoost = StartCoroutine(SpeedBoostCoroutine(speedMultiplier, boostDuration));
    }

    private IEnumerator SpeedBoostCoroutine(float speedMultiplier, float boostDuration)
    {
        currentSpeed = moveSpeed * speedMultiplier;
        yield return new WaitForSeconds(boostDuration);
        currentSpeed = moveSpeed;
        activeBoost = null;
    }
}