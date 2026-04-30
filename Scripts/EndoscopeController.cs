using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class EndoscopeController : MonoBehaviour
{
    [Header("Navigation System")]
    public float baseSpeed = 1.0f;
    public float boostMultiplier = 3.0f;
    public float rotationSpeed = 2.0f;

    [Header("Illumination System")]
    public Light ledLight;
    public float minLightIntensity = 200f;
    public float maxLightIntensity = 10000f;
    public float scrollSensitivity = 500f;
    private float currentLightLevel;

    [Header("Imaging System")]
    public Image flashPanel;
    public AudioClip captureSound;
    private bool isCapturing = false;

    public Camera mainCamera;
    public float zoomSpeed = 80f;
    public float minFOV = 15f;
    public float maxFOV = 60f;

    [Header("Audio Feedback")]
    public AudioSource audioSource;
    public bool enableProximityAudio = true;
    public Transform targetAnomaly;
    public float proximityTriggerDistance = 8.0f;
    public AudioClip proximityVoiceover;
    private bool hasPlayedProximity = false;

    [Header("Mission Logic")]
    public Transform cameraTransform;
    public string targetTag = "Tumor";
    public float validPhotoDistance = 2.5f;
    public AudioClip successVoiceover;

    private Rigidbody rb;
    private float pitch = 0.0f;
    private float yaw = 0.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentLightLevel = minLightIntensity;
        if (ledLight != null)
        {
            ledLight.intensity = currentLightLevel;
        }

        if (mainCamera != null)
        {
            mainCamera.fieldOfView = maxFOV;
        }
    }

    void Update()
    {
        // Illumination Control
        if (ledLight != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                currentLightLevel += scroll * scrollSensitivity;
                currentLightLevel = Mathf.Clamp(currentLightLevel, minLightIntensity, maxLightIntensity);
                ledLight.intensity = currentLightLevel;
            }
        }

        // Optical Zoom Control
        if (mainCamera != null)
        {
            if (Input.GetKey(KeyCode.PageUp))
                mainCamera.fieldOfView -= zoomSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.PageDown))
                mainCamera.fieldOfView += zoomSpeed * Time.deltaTime;

            mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView, minFOV, maxFOV);
        }

        if (enableProximityAudio && !hasPlayedProximity && targetAnomaly != null)
        {
            float distance = Vector3.Distance(transform.position, targetAnomaly.position);

            Debug.Log($"[RADAR TELEMETRY] Current distance to tumor: {distance:F2} units.");

            if (distance <= proximityTriggerDistance)
            {
                if (audioSource != null && proximityVoiceover != null)
                {
                    audioSource.PlayOneShot(proximityVoiceover);
                    hasPlayedProximity = true;
                    Debug.Log("[RADAR TELEMETRY] Target reached. Audio triggered. Radar offline.");
                }
            }
        }

        // Spacebar shutter
        if (Input.GetKeyDown(KeyCode.Space) && !isCapturing)
        {
            StartCoroutine(CaptureAndValidateImage());
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? (baseSpeed * boostMultiplier) : baseSpeed;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        float moveY = 0f;

        if (Input.GetKey(KeyCode.E)) moveY = 1f;
        if (Input.GetKey(KeyCode.Q)) moveY = -1f;

        Vector3 movement = (transform.right * moveX) + (transform.up * moveY) + (transform.forward * moveZ);
        rb.linearVelocity = movement.normalized * currentSpeed;

        yaw += rotationSpeed * Input.GetAxis("Mouse X");
        pitch -= rotationSpeed * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        rb.MoveRotation(Quaternion.Euler(pitch, yaw, 0.0f));
    }

    System.Collections.IEnumerator CaptureAndValidateImage()
    {
        isCapturing = true;

        if (audioSource != null && captureSound != null)
        {
            audioSource.PlayOneShot(captureSound);
        }

        bool missionPassed = false;
        if (cameraTransform != null)
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, validPhotoDistance))
            {
                if (hit.collider.CompareTag(targetTag))
                {
                    missionPassed = true;
                }
            }
        }

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filePath = Application.dataPath + "/EndoscopicScan_" + timestamp + ".png";
        ScreenCapture.CaptureScreenshot(filePath);

        yield return null;

        if (flashPanel != null)
        {
            flashPanel.color = new Color(1, 1, 1, 1);
            float fadeDuration = 0.3f;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                flashPanel.color = new Color(1, 1, 1, 1f - (elapsed / fadeDuration));
                yield return null;
            }
        }

        if (missionPassed)
        {
            PlayerPrefs.SetInt("CompletedBeginner", 1);
            PlayerPrefs.Save();

            if (audioSource != null && successVoiceover != null)
            {
                audioSource.PlayOneShot(successVoiceover);
                yield return new WaitForSeconds(successVoiceover.length);
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(1);
        }

        isCapturing = false;
    }
}