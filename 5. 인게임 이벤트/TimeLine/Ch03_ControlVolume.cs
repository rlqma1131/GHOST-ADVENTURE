using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class Ch03_ControlVolume : MonoBehaviour
{
    public Volume volume;
    private ColorAdjustments colorAdjustments;
    private ChromaticAberration chromaticAberration;
    private DepthOfField depthOfField;


    public Color targetColor = new Color(1f, 0f, 0f, 1f);
    public float duration = 3f;

    private void Start()
    {
        if (volume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            colorAdjustments.colorFilter.overrideState = true;
            
        }
        else
        {
            Debug.LogWarning("Color Adjustments not found.");
        }
        if(volume.profile.TryGet<ChromaticAberration>(out chromaticAberration))
        {
            chromaticAberration.active = true;
        }
        else
        {
            Debug.LogWarning("Chromatic Aberration not found.");
        }
        if(volume.profile.TryGet<DepthOfField>(out depthOfField))
        {
            depthOfField.active = true;
        }
        else
        {
            Debug.LogWarning("Depth of Field not found.");
        }
    }
    public void controlVolume()
    {
        StartCoroutine(FadeToRed());


    }
    public void controlScreen()
    {

        StartCoroutine(FadeToScreen());
    }

    IEnumerator FadeToRed()
    {
        Color startColor = colorAdjustments.colorFilter.value;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            colorAdjustments.colorFilter.value = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        
        colorAdjustments.colorFilter.value = targetColor;
    }

    IEnumerator FadeToScreen()
    {
        float time = 0f;

        while (time < 5)
        {
            time += Time.deltaTime;
            float t = time / 5;

            chromaticAberration.intensity.value = Mathf.Lerp(1f, 0.1f, t);
            depthOfField.focusDistance.value = Mathf.Lerp(0f, 10f, t);
            yield return null;
        }



    }
}
