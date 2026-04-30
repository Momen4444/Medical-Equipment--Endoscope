using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public Button btnBeginner;
    public Button btnExpert;
    public RectTransform expertButtonTransform;
    public GameObject expertCardObject;
    public Button btnResetProgress;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sceneIntroSound;  // NEW: Plays the moment the scene opens
    public AudioClip screamSound;      // "Aaah... no!"
    public AudioClip laserSound;       // Gunshot
    public AudioClip instructionSound; // "Click beginner to start..."

    [Header("Logic State")]
    private bool isGagRunning = false;

    // Physics & Reset Variables
    private Vector2 currentVelocity;
    private float gravity = 3500f;
    private float rotationSpeed = 0f;

    private Vector2 originalExpertPosition;
    private Quaternion originalExpertRotation;

    void Start()
    {
        // 1. Play the scene entry sound immediately
        if (audioSource != null && sceneIntroSound != null)
        {
            audioSource.PlayOneShot(sceneIntroSound);
        }

        // 2. Save original transforms for the reset button
        originalExpertPosition = expertButtonTransform.anchoredPosition;
        originalExpertRotation = expertButtonTransform.rotation;

        // 3. Bind UI listeners
        btnBeginner.onClick.AddListener(LoadBeginnerSim);
        btnExpert.onClick.AddListener(AttemptExpertSim);

        if (btnResetProgress != null)
        {
            btnResetProgress.onClick.AddListener(ResetGameProgress);
        }
    }

    void LoadBeginnerSim()
    {
        SceneManager.LoadScene(2);
    }

    void AttemptExpertSim()
    {
        if (PlayerPrefs.GetInt("CompletedBeginner", 0) == 1)
        {
            SceneManager.LoadScene(3);
        }
        else
        {
            if (!isGagRunning)
            {
                Vector2 clickPosition = Input.mousePosition;
                StartCoroutine(ExpertGagSequence(clickPosition));
            }
        }
    }

    IEnumerator ExpertGagSequence(Vector2 clickPos)
    {
        isGagRunning = true;
        btnExpert.interactable = false;

        // T = 0.00s: Start the scream
        if (audioSource != null && screamSound != null)
        {
            audioSource.PlayOneShot(screamSound);
        }

        // Wait 1.0 seconds for the scream to finish
        yield return new WaitForSeconds(1.0f);

        // T = 1.00s: Play the gunshot
        if (audioSource != null && laserSound != null)
        {
            audioSource.PlayOneShot(laserSound);
        }

        // Wait 0.15 seconds for impact delay
        yield return new WaitForSeconds(0.15f);

        // T = 1.15s: Calculate physics and launch the card
        Vector2 buttonCenter = RectTransformUtility.WorldToScreenPoint(null, expertButtonTransform.position);
        Vector2 clickOffset = clickPos - buttonCenter;

        float horizontalForce = -clickOffset.x * 15f;
        currentVelocity = new Vector2(horizontalForce, 1200f);
        rotationSpeed = clickOffset.x * 6f;

        float fallDuration = 2.0f;
        float elapsedTime = 0f;

        while (elapsedTime < fallDuration)
        {
            currentVelocity.y -= gravity * Time.deltaTime;
            expertButtonTransform.anchoredPosition += currentVelocity * Time.deltaTime;
            expertButtonTransform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        expertCardObject.SetActive(false);

        // T = 3.15s: The card is gone. Play the final instruction audio.
        if (audioSource != null && instructionSound != null)
        {
            audioSource.PlayOneShot(instructionSound);
        }

        isGagRunning = false;
    }

    public void ResetGameProgress()
    {
        PlayerPrefs.SetInt("CompletedBeginner", 0);
        PlayerPrefs.Save();

        StopAllCoroutines();
        isGagRunning = false;

        expertCardObject.SetActive(true);
        btnExpert.interactable = true;
        expertButtonTransform.anchoredPosition = originalExpertPosition;
        expertButtonTransform.rotation = originalExpertRotation;
    }
}