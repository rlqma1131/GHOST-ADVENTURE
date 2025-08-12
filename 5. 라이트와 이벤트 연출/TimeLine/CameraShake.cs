using UnityEngine;
using Cinemachine; // Cinemachine 네임스페이스 추가
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // 인스펙터에서 가상 카메라 연결
    private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;

    // 쉐이크 강도와 지속 시간 
    public float defaultShakeIntensity = 1f;
    public float defaultShakeTime = 0.5f;
    void Awake()
    {
        if (virtualCamera != null)
        {
            // 가상 카메라의 Perlin Noise 컴포넌트를 가져옴
            cinemachineBasicMultiChannelPerlin =
                virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }



    public void DoShake( float duration)
    {
        if (cinemachineBasicMultiChannelPerlin != null)
        {
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = defaultShakeIntensity;
            StartCoroutine(StopShakeAfterTime(duration)); // 전달받은 duration 사용
        }
        else
        {
            Debug.LogWarning("CinemachineBasicMultiChannelPerlin component not found. Cannot shake camera.");
        }
    }

    // 매개변수 없는 버전 (기존 호출 방식 유지 또는 기본값 사용 시)
    public void DoShake()
    {
        DoShake(defaultShakeTime);
    }

    private IEnumerator StopShakeAfterTime(float duration) // duration 매개변수 추가
    {
        yield return new WaitForSecondsRealtime(duration); // 전달받은 duration 사용

        if (cinemachineBasicMultiChannelPerlin != null) 
        {
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0f;
        }
    }
}



