using UnityEngine;
using System;

/// <summary>
/// Place this on a trigger collider GameObject at the destination (e.g. CoDa building).
/// When the Player enters the trigger, fires the PlayerReachedGoal event.
/// GameManager subscribes to this event to trigger the win condition.
/// </summary>
public class GoalZone : MonoBehaviour
{
    public static event Action PlayerReachedGoal;

    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("GoalZone: Player reached the goal!");
            PlayerReachedGoal?.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        // Draw a green marker in the Scene view so you can see the zone
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Collider2D col = GetComponent<Collider2D>();
        if (col is CircleCollider2D circle)
            Gizmos.DrawSphere(transform.position, circle.radius);
        else
            Gizmos.DrawCube(transform.position, Vector3.one);
    }
}
