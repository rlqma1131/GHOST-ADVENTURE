using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class BloomDirtEffect : MonoBehaviour
{
    public Volume globalVolume;
    public float duration = 1f; // 증가/감소 시간

    private Bloom bloom;

    void Start()
    {
        // Volume에서 Bloom 컴포넌트 가져오기
        if (globalVolume.profile.TryGet(out bloom))
        {
            bloom.dirtIntensity.overrideState = true;
            bloom.dirtIntensity.value = 0f;
        }
        else
        {
            Debug.LogError("Bloom 컴포넌트를 찾을 수 없습니다!");
        }
    }


    public void PlayBloomEffect(float intensity)
    {
        StartCoroutine(AnimateBloomDirtIntensity(intensity));
    }

    private IEnumerator AnimateBloomDirtIntensity(float targetIntensity)
    {
        // 점점 증가

        if (bloom == null)
        {
            Debug.LogError("Bloom 컴포넌트가 초기화되지 않았습니다!");
            yield break;
        }
        float timer = 0f;
        while (timer < duration)
        {
            float value = Mathf.Lerp(0f, targetIntensity, timer / duration);
            bloom.dirtIntensity.value = value;
            timer += Time.deltaTime;
            yield return null;
        }

        bloom.dirtIntensity.value = targetIntensity;

        // 점점 감소
        timer = 0f;
        while (timer < duration)
        {
            float value = Mathf.Lerp(targetIntensity, 0f, timer / duration);
            bloom.dirtIntensity.value = value;
            timer += Time.deltaTime;
            yield return null;
        }

        bloom.dirtIntensity.value = 0f;
    }
}
