using UnityEngine;

public class Ch1_FlashlightBeam : MonoBehaviour
{
    [SerializeField] private AudioClip on;
    [SerializeField] private AudioClip off;
    [SerializeField] private GameObject beamVisual;
    public bool isOn { get; private set; } = false;

    public void ToggleBeam()
    {
        isOn = !isOn;
        beamVisual.SetActive(isOn);
        if (isOn)
            SoundManager.Instance.PlaySFX(on);
        else
            SoundManager.Instance.PlaySFX(off);
    }
}
