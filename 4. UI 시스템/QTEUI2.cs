using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Cinemachine;
using DG.Tweening;
public class QTEUI2 : MonoBehaviour
{
    [Header("UI References")]
    public GameObject qteUI;
    public Image gaugeBar;
    public TMP_Text timeText;
    public TextMeshProUGUI success;
    public TextMeshProUGUI fail;

    [Header("QTE Settings")]
    public AudioClip escape;
    public int requiredPresses = 15;
    public float timeLimit = 3f;
    private int currentPressCount = 0;
    private float currentTime = 0f;
    private bool isRunning = false;
    private bool isSuccess;
    public bool isdead = false;

    private CinemachineVirtualCamera camera;
    private float currentSize;
    private float targetSize;

    private CinemachineBasicMultiChannelPerlin noise;
    public void Start()
    {
        success.gameObject.SetActive(false);
        fail.gameObject.SetActive(false);
        qteUI.SetActive(false);
        isdead = false;
    }
    public void StartQTE()
    {
        if (isRunning) return;

        ResetState();
        if (qteUI) qteUI.SetActive(true);

        isRunning = true;

        // (카메라/참조 캐싱은 기존대로)
        camera = GameManager.Instance.Player.GetComponent<PlayerCamera>().currentCam;
        currentSize = camera.m_Lens.OrthographicSize;
        noise = camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        StartCoroutine(RunQTE());
    }

    private IEnumerator RunQTE()
    {
        
        while (currentTime < timeLimit)
        {

           
            if (currentPressCount >= requiredPresses)
            {
                success.gameObject.SetActive(true);
                isSuccess = true;
                break;
            }
            // currentTime += Time.deltaTime;
            currentTime += Time.unscaledDeltaTime;
            float remainingTime = Mathf.Max(0f, timeLimit - currentTime);
            timeText.text = remainingTime.ToString("F2");

            if (Input.GetKeyDown(KeyCode.Space))
            {
                 
                 
                 targetSize = camera.m_Lens.OrthographicSize - currentSize * 0.05f;
                DOTween.To(() => camera.m_Lens.OrthographicSize, x => camera.m_Lens.OrthographicSize = x, targetSize, 0.3f);
                StartCoroutine(ShakeCamera(0.3f, 3f, 10f));
                currentPressCount++;
                gaugeBar.fillAmount = Mathf.Clamp01((float)currentPressCount / requiredPresses);
            }

            yield return null;
        }
       

        // 탈출 성공시
        if (currentPressCount >= requiredPresses)
        {
            DOTween.To(() => camera.m_Lens.OrthographicSize, x => camera.m_Lens.OrthographicSize = x, currentSize, 0.3f);
            success.gameObject.SetActive(true);
            isSuccess = true;

            SoundManager.Instance.PlaySFX(escape);
        }
        
        // 탈출 실패시
        else
        {
            DOTween.To(() => camera.m_Lens.OrthographicSize, x => camera.m_Lens.OrthographicSize = x, currentSize, 0.3f);
            fail.gameObject.SetActive(true);
            isSuccess = false;
            isdead = true;
            UIManager.Instance.HideQTEEffectCanvas(); 
            UIManager.Instance.PlayModeUI_CloseAll();
            UIManager.Instance.QTE_UI_2.gameObject.SetActive(false);
        }
        
        isRunning = false;
        yield return new WaitForSecondsRealtime(1.5f);
        qteUI.SetActive(false);
        success.gameObject.SetActive(false);
        fail.gameObject.SetActive(false);

    }
    private IEnumerator ShakeCamera(float duration, float amplitude, float frequency)
    {
        noise.m_AmplitudeGain = amplitude;
        noise.m_FrequencyGain = frequency;

        yield return new WaitForSecondsRealtime(duration);

        noise.m_AmplitudeGain = 0f;
        noise.m_FrequencyGain = 0f;
    }
    
    public void ResetState()
    {
        StopAllCoroutines();

        // 순수 상태값 초기화
        currentPressCount = 0;
        currentTime = 0f;
        isRunning = false;
        isSuccess = false;
        isdead = false;

        // UI 초기화
        if (gaugeBar) gaugeBar.fillAmount = 0f;
        if (timeText) timeText.text = timeLimit.ToString("F2");
        if (success) success.gameObject.SetActive(false);
        if (fail)    fail.gameObject.SetActive(false);
        if (qteUI)   qteUI.SetActive(false);

        // 카메라 연출 원복(혹시 남았을 수 있으니 안전하게)
        if (camera != null)
        {
            camera.m_Lens.OrthographicSize = currentSize;
            if (noise == null) noise = camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise != null) { noise.m_AmplitudeGain = 0f; noise.m_FrequencyGain = 0f; }
        }
    }
    
    public bool IsQTERunning() => isRunning;
    public bool IsSuccess() => isSuccess;
}
