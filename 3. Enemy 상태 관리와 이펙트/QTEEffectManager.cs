using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QTEEffectManager : MonoBehaviour
{
    public static QTEEffectManager Instance { get; private set; }

    [Header("어두운 오버레이")]
    public CanvasGroup darkOverlay;
    public float fadeDuration = 2.5f;

    [Header("카메라 연출")]
    public Camera mainCamera;
    public float zoomFOV = 30f;
    public float zoomDuration = 0.5f;
    public float moveDuration = 0.5f;

    private float initialFOV;
    private Vector3 initialCamPosition;

    [Header("QTE 대상")]
    public Transform playerTarget;
    public Transform enemyTarget;

    private Coroutine fadeCoroutine;
    private Coroutine zoomCoroutine;
    private Coroutine moveCoroutine;

    private void Awake()
    {
        Instance = this;

        if (darkOverlay != null)
        {
            darkOverlay.alpha = 0f;
            darkOverlay.blocksRaycasts = false;
            darkOverlay.interactable = false;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        initialFOV = mainCamera.fieldOfView;
        initialCamPosition = mainCamera.transform.position;
    }

    // private void Update()
    // {
    //     if(UIManager.Instance.QTE_UI_2.isdead)
    //     {
    //         gameObject.SetActive(false);
    //     }
    // }
    
    public void ResetImmediate()
    {
        StopRunningCoroutines();

        if (darkOverlay != null)
        {
            darkOverlay.alpha = 0f;
            darkOverlay.blocksRaycasts = false;
            darkOverlay.interactable = false;
        }
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = initialFOV;
            mainCamera.transform.position = initialCamPosition;
        }
    }

    public void StartQTEEffects()
    {
        StopRunningCoroutines();

        if (darkOverlay != null)
        {
            darkOverlay.blocksRaycasts = true;  // QTE 중 클릭 차단
            darkOverlay.interactable   = false;
        }
        
        fadeCoroutine = StartCoroutine(FadeToAlphaRange(darkOverlay.alpha, 0.9f, fadeDuration));
        zoomCoroutine = StartCoroutine(ZoomTo(zoomFOV, zoomDuration));

        if (playerTarget != null && enemyTarget != null)
        {
            Vector3 mid = (playerTarget.position + enemyTarget.position) / 2f;
            Vector3 camPos = new Vector3(mid.x, mid.y, mainCamera.transform.position.z);
            moveCoroutine = StartCoroutine(MoveCameraTo(camPos, moveDuration));
        }
    }

    public void EndQTEEffects(bool instant = false)
    {
        StopRunningCoroutines();

        if (instant)
        {
            ResetImmediate();        // ← 즉시 복귀
            return;
        }

        if (darkOverlay != null)
            darkOverlay.blocksRaycasts = false;
        
        fadeCoroutine = StartCoroutine(FadeToAlphaRange(darkOverlay.alpha, 0f, fadeDuration));
        zoomCoroutine = StartCoroutine(ZoomTo(initialFOV, zoomDuration));
        moveCoroutine = StartCoroutine(MoveCameraTo(initialCamPosition, moveDuration));
    }

    private void StopRunningCoroutines()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
    }

    private IEnumerator FadeToAlphaRange(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        darkOverlay.alpha = fromAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            darkOverlay.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            yield return null;
        }

        darkOverlay.alpha = toAlpha;
    }

    private IEnumerator ZoomTo(float targetFOV, float duration)
    {
        float startFOV = mainCamera.fieldOfView;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            yield return null;
        }

        mainCamera.fieldOfView = targetFOV;
    }

    private IEnumerator MoveCameraTo(Vector3 targetPos, float duration)
    {
        Vector3 start = mainCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            mainCamera.transform.position = Vector3.Lerp(start, targetPos, t);
            yield return null;
        }

        mainCamera.transform.position = targetPos;
    }
}
