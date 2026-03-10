using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Shows a dismissable intro screen when the game first loads.
/// Attach to any persistent GameObject (e.g., the GameManager or a dedicated IntroManager).
/// Wire up IntroPanel in the Inspector.
/// </summary>
public class IntroScreen : MonoBehaviour
{
    [Header("Intro UI")]
    [SerializeField] private GameObject introPanel;       // The full-screen panel overlay
    [SerializeField] private TextMeshProUGUI introText;   // Main story text (optional, can be set in Inspector)
    [SerializeField] private TextMeshProUGUI promptText;  // "Press any key" blinking prompt

    [Header("Settings")]
    [TextArea(4, 10)]
    [SerializeField] private string storyText =
        "It\u2019s Week 10 of Winter Quarter and you\u2019re trying to get to your final presentation in CoDa from your dorm on East Campus.\n\n" +
        "But the e-bikes and buses have other plans\u2026\n\n" +
        "Try to get to CoDa in one piece!\n\n" +
        "Use arrow keys to navigate.";

    [SerializeField] private string promptMessage = "Press any key to start";
    [SerializeField] private float blinkRate = 0.6f;

    private bool introDismissed = false;

    void Start()
    {
        if (introPanel == null)
        {
            Debug.LogWarning("IntroScreen: introPanel not assigned — intro will not show.");
            return;
        }

        // Populate text fields if wired up
        if (introText != null)
            introText.text = storyText;

        if (promptText != null)
            promptText.text = promptMessage;

        // Show the panel and freeze gameplay
        introPanel.SetActive(true);
        Time.timeScale = 0f;

        // Start the blinking prompt coroutine
        if (promptText != null)
            StartCoroutine(BlinkPrompt());
    }

    void Update()
    {
        if (introDismissed) return;
        if (introPanel == null || !introPanel.activeSelf) return;

        // Any key press dismisses the intro
        if (Input.anyKeyDown)
        {
            DismissIntro();
        }
    }

    private void DismissIntro()
    {
        introDismissed = true;
        introPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private IEnumerator BlinkPrompt()
    {
        while (!introDismissed)
        {
            if (promptText != null)
                promptText.enabled = !promptText.enabled;

            // WaitForSecondsRealtime so blinking works even when timeScale = 0
            yield return new WaitForSecondsRealtime(blinkRate);
        }

        // Make sure prompt is visible when loop ends
        if (promptText != null)
            promptText.enabled = true;
    }
}
