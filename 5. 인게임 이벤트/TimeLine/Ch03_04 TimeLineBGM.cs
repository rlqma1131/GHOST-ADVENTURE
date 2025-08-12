using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch03_04TimeLineBGM : MonoBehaviour
{


    AudioSource bgmAudioSource;

    void Start()
    {
        bgmAudioSource = GetComponent<AudioSource>();
        if (bgmAudioSource == null)
        {
            Debug.LogError("AudioSource component not found on this GameObject.");
            return;
        }
        // Play the BGM

    }

    public void PlayBGM()
    {
        if (bgmAudioSource != null && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Play();
        }
    }


}
