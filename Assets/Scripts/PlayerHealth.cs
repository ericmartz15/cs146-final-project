using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    private bool isDead = false;
    private bool isInvulnerable = false;

    // Fired once when the player dies (GameManager listens to trigger lose screen).
    public static Action PlayerDied;

    public bool IsDead => isDead;

    /// <summary>
    /// Kills the player. Ignored if already dead or invulnerable.
    /// Returns true if the player died as a result of this call.
    /// </summary>
    public bool TakeDamage()
    {
        if (isDead || isInvulnerable) return false;

        isDead = true;
        Debug.Log("Player died!");

        // Play death VFX if present.
        DeathEffect deathEffect = GetComponent<DeathEffect>();
        if (deathEffect != null)
            deathEffect.PlayDeathEffect();

        PlayerDied?.Invoke();
        return true;
    }

    /// <summary>
    /// Resets the player to the alive state (called on game restart).
    /// </summary>
    public void ResetHealth()
    {
        isDead = false;
        isInvulnerable = false;
    }

    /// <summary>
    /// Prevents the player from taking damage (used when the win condition is met).
    /// </summary>
    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }
}
