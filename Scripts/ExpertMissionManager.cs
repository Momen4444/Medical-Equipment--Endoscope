using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ExpertMissionManager : MonoBehaviour
{
    [Header("System References")]
    public EndoscopeController controller;
    public Camera mainCamera;
    public Collider anomalyCollider;

    [Header("Mission Constraints")]
    public float maxAnalysisDistance = 3.5f;

    [Header("Drawing Tools")]
    public float minimumPixelDistance = 5f;

    [Header("Visual Guide")]
    public LineRenderer guideLineRenderer;

    [Header("Evaluation & Win State")]
    public float requiredAccuracy = 0.90f;
    public AudioClip successSound;
    public AudioSource audioSource;
    public int outroSceneIndex = 2;

    private bool isAnalysisMode = false;
    private bool isDrawing = false;
    private bool missionComplete = false;

    // We only need screen points now for the math
    private List<Vector2> screenPoints = new List<Vector2>();

    void Start()
    {
        if (guideLineRenderer != null)
        {
            guideLineRenderer.positionCount = 0;
            guideLineRenderer.sortingOrder = 1;
        }
    }

    void Update()
    {
        if (missionComplete) return;

        if (Input.GetKeyDown(KeyCode.R)) ToggleAnalysisMode();
        if (isAnalysisMode) HandleDrawing();
    }

    void ToggleAnalysisMode()
    {
        if (!isAnalysisMode)
        {
            float distanceToTumor = Vector3.Distance(mainCamera.transform.position, anomalyCollider.bounds.center);
            if (distanceToTumor > maxAnalysisDistance)
            {
                Debug.LogWarning($"ACCESS DENIED: Target too far ({distanceToTumor:F1}m). Move closer.");
                return;
            }

            Vector3 viewportPos = mainCamera.WorldToViewportPoint(anomalyCollider.bounds.center);
            if (viewportPos.z < 0 || viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
            {
                Debug.LogWarning("ACCESS DENIED: Look directly at the anomaly to begin analysis.");
                return;
            }
        }

        isAnalysisMode = !isAnalysisMode;
        controller.enabled = !isAnalysisMode;

        if (isAnalysisMode)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            DrawGuideOutline();
            Debug.Log("Analysis Mode Active. Trace the yellow guide box with your mouse.");
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (guideLineRenderer != null) guideLineRenderer.positionCount = 0;
        }
    }

    void HandleDrawing()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            screenPoints.Clear();
        }

        if (Input.GetMouseButton(0) && isDrawing)
        {
            Vector2 currentScreenPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            if (screenPoints.Count == 0 || Vector2.Distance(screenPoints[screenPoints.Count - 1], currentScreenPos) > minimumPixelDistance)
            {
                screenPoints.Add(currentScreenPos);
            }
        }

        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            isDrawing = false;
            EvaluateAccuracy();
        }
    }

    Rect GetTumorScreenRect()
    {
        Bounds bounds = anomalyCollider.bounds;
        Vector3[] corners = new Vector3[8];
        corners[0] = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
        corners[1] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
        corners[2] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
        corners[3] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
        corners[4] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
        corners[5] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
        corners[6] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
        corners[7] = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z);

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (Vector3 corner in corners)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(corner);
            if (screenPos.x < minX) minX = screenPos.x;
            if (screenPos.x > maxX) maxX = screenPos.x;
            if (screenPos.y < minY) minY = screenPos.y;
            if (screenPos.y > maxY) maxY = screenPos.y;
        }
        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

    void DrawGuideOutline()
    {
        if (guideLineRenderer == null) return;

        Rect targetRect = GetTumorScreenRect();
        float zDepth = mainCamera.nearClipPlane + 0.1f;

        Vector3[] guidePoints = new Vector3[4];
        guidePoints[0] = mainCamera.ScreenToWorldPoint(new Vector3(targetRect.xMin, targetRect.yMin, zDepth));
        guidePoints[1] = mainCamera.ScreenToWorldPoint(new Vector3(targetRect.xMin, targetRect.yMax, zDepth));
        guidePoints[2] = mainCamera.ScreenToWorldPoint(new Vector3(targetRect.xMax, targetRect.yMax, zDepth));
        guidePoints[3] = mainCamera.ScreenToWorldPoint(new Vector3(targetRect.xMax, targetRect.yMin, zDepth));

        guideLineRenderer.positionCount = 4;
        guideLineRenderer.loop = true;
        guideLineRenderer.SetPositions(guidePoints);
    }

    void EvaluateAccuracy()
    {
        if (screenPoints.Count < 10)
        {
            Debug.Log("Drawing rejected: Not enough data points.");
            return;
        }

        float drawMinX = float.MaxValue, drawMaxX = float.MinValue;
        float drawMinY = float.MaxValue, drawMaxY = float.MinValue;

        foreach (Vector2 p in screenPoints)
        {
            if (p.x < drawMinX) drawMinX = p.x;
            if (p.x > drawMaxX) drawMaxX = p.x;
            if (p.y < drawMinY) drawMinY = p.y;
            if (p.y > drawMaxY) drawMaxY = p.y;
        }
        Rect drawRect = Rect.MinMaxRect(drawMinX, drawMinY, drawMaxX, drawMaxY);
        Rect objRect = GetTumorScreenRect();

        float intersectionArea = Mathf.Max(0, Mathf.Min(drawRect.xMax, objRect.xMax) - Mathf.Max(drawRect.xMin, objRect.xMin)) *
                                 Mathf.Max(0, Mathf.Min(drawRect.yMax, objRect.yMax) - Mathf.Max(drawRect.yMin, objRect.yMin));

        float drawArea = drawRect.width * drawRect.height;
        float objArea = objRect.width * objRect.height;
        float unionArea = drawArea + objArea - intersectionArea;

        float accuracy = (unionArea == 0) ? 0 : intersectionArea / unionArea;

        Debug.Log($"Evaluation Complete. Shape Accuracy (IoU): {accuracy * 100f}%");

        if (accuracy >= requiredAccuracy)
        {
            Debug.Log("SUCCESS: 90% Accuracy Reached. Anomaly Resected.");
            StartCoroutine(ExecuteOutroSequence());
        }
        else
        {
            Debug.Log("FAILED: Accuracy too low. Redraw the boundary.");
        }
    }

    System.Collections.IEnumerator ExecuteOutroSequence()
    {
        missionComplete = true;

        anomalyCollider.gameObject.SetActive(false);
        if (guideLineRenderer != null) guideLineRenderer.positionCount = 0;

        if (audioSource != null && successSound != null)
        {
            audioSource.PlayOneShot(successSound);
            yield return new WaitForSeconds(successSound.length);
        }
        else
        {
            yield return new WaitForSeconds(2.0f);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(outroSceneIndex);
    }
}