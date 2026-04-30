using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // CRITICAL: Required for TextMeshPro manipulation
using System.Collections;

[RequireComponent(typeof(VideoPlayer))]
public class OutroManager : MonoBehaviour
{
    [Header("System References")]
    public VideoPlayer videoPlayer;
    public Image fadePanel;
    public TMP_Text thankYouText; // NEW: The text we will fade out

    [Header("Transition Settings")]
    public float postVideoDelay = 2.0f;
    public int mainMenuSceneIndex = 0;

    private bool isTransitioning = false;

    void Start()
    {
        if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();

        // 1. Initialize black screen to invisible
        if (fadePanel != null)
        {
            Color startColor = fadePanel.color;
            startColor.a = 0f;
            fadePanel.color = startColor;
            fadePanel.raycastTarget = false;
        }

        // 2. Initialize text to fully visible
        if (thankYouText != null)
        {
            Color textStartColor = thankYouText.color;
            textStartColor.a = 1f;
            thankYouText.color = textStartColor;
        }

        videoPlayer.loopPointReached += OnVideoEnd;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionToMenu());
        }
    }

    IEnumerator TransitionToMenu()
    {
        isTransitioning = true;
        Debug.Log($"Executing Cross-Fade over {postVideoDelay} seconds.");

        float elapsedTime = 0f;

        // Cache colors to avoid heavy memory lookups in the while loop
        Color panelColor = fadePanel != null ? fadePanel.color : Color.black;
        Color textColor = thankYouText != null ? thankYouText.color : Color.white;

        // The Cross-Fade Loop
        while (elapsedTime < postVideoDelay)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / postVideoDelay);

            if (fadePanel != null)
            {
                panelColor.a = normalizedTime; // Fades IN (0 to 1)
                fadePanel.color = panelColor;
            }

            if (thankYouText != null)
            {
                textColor.a = 1f - normalizedTime; // Fades OUT (1 to 0)
                thankYouText.color = textColor;
            }

            yield return null;
        }

        // Execute the final scene load
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}