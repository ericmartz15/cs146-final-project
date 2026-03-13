using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("End Screen Overlay")]
    [SerializeField] private GameObject winScreenTilemapObject; // Enable this on win/death

    [Header("Text Display")]
    [SerializeField] private TextMeshProUGUI winTextUI;      // Shows "You Win" or "You Died"
    [SerializeField] private TextMeshProUGUI restartTextUI;  // "Press R to Restart"

    [Header("Settings")]
    [SerializeField] private string winMessage = "You Win!";
    [SerializeField] private string deathMessage = "You Died";
    [SerializeField] private float restartPromptDelay = 1.5f; // Delay before freeze + restart prompt

    private bool gameWon = false;
    private bool gameLost = false;
    private bool canRestart = false;

    void Start()
    {
        // Hide end-screen overlay during gameplay
        if (winScreenTilemapObject != null)
            winScreenTilemapObject.SetActive(false);

        // Hide text during gameplay
        if (winTextUI != null) winTextUI.gameObject.SetActive(false);
        if (restartTextUI != null) restartTextUI.gameObject.SetActive(false);

        // Subscribe to events
        PlayerHealth.PlayerDied += HandlePlayerDeath;
        GoalZone.PlayerReachedGoal += HandleGoalReached;
    }

    void Update()
    {
        if (canRestart && Input.GetKeyDown(KeyCode.R))
            ResetGame();
    }

    void OnDestroy()
    {
        PlayerHealth.PlayerDied -= HandlePlayerDeath;
        GoalZone.PlayerReachedGoal -= HandleGoalReached;
    }

    private void HandlePlayerDeath()
    {
        if (gameWon || gameLost) return;
        gameLost = true;
        Debug.Log("GameManager: Player died — game over.");
        StartCoroutine(ShowEndScreenAfterDelay(deathMessage));
    }

    private void HandleGoalReached()
    {
        if (gameWon || gameLost) return;
        gameWon = true;
        Debug.Log("GameManager: Player reached goal — you win!");

        // Make player invulnerable so they can't die after winning
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.SetInvulnerable(true);

        StartCoroutine(ShowEndScreenAfterDelay(winMessage));
    }

    public void ResetGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator ShowEndScreenAfterDelay(string message)
    {
        yield return new WaitForSecondsRealtime(restartPromptDelay);

        // Show overlay
        if (winScreenTilemapObject != null)
            winScreenTilemapObject.SetActive(true);

        // Show win/death message
        if (winTextUI != null)
        {
            winTextUI.text = message;
            winTextUI.gameObject.SetActive(true);
        }

        // Show restart prompt
        if (restartTextUI != null)
        {
            restartTextUI.text = "Press R to Restart";
            restartTextUI.gameObject.SetActive(true);
        }

        canRestart = true;
        Time.timeScale = 0f;
    }
}
