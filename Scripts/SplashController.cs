using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class SplashController : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup logoGroup;
    public RawImage videoDisplay;

    [Header("Video Settings")]
    public VideoPlayer introVideo;

    [Header("Timing")]
    public float fadeDuration = 1.5f;
    public float displayDuration = 2.0f;

    void Start()
    {
        // 1. Force the initial state
        logoGroup.alpha = 0f;
        videoDisplay.color = new Color(1, 1, 1, 0); // Make the video screen invisible
        
        // 2. Subscribe to the video's completion event
        introVideo.loopPointReached += OnVideoEnd;

        // 3. Start the timeline
        StartCoroutine(IntroSequence());
    }

    void Update()
    {
        // The Skip Mechanic
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadMainMenu();
        }
    }

    IEnumerator IntroSequence()
    {
        // Phase 1: Fade In
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            logoGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null; // Wait for the next frame
        }

        // Phase 2: Hold the logo on screen
        yield return new WaitForSeconds(displayDuration);

        // Phase 3: Fade Out
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            logoGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            yield return null;
        }

        // Phase 4: Trigger the Video
        videoDisplay.color = new Color(1, 1, 1, 1); // Reveal the video screen
        introVideo.Play();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // Automatically load when the video finishes its natural runtime
        LoadMainMenu();
    }

    void LoadMainMenu()
    {
        // Unsubscribe from the event to prevent memory leaks before destroying the scene
        introVideo.loopPointReached -= OnVideoEnd;
        SceneManager.LoadScene(1);
    }
}