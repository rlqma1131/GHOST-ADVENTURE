using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Ch03_To_Ch04MainTimeLine : MonoBehaviour
{
    bool alreadyPlayed = false;
    [SerializeField] PlayableDirector playableDirector;
    void Start()
    {
        if (!alreadyPlayed)
        {
            playableDirector.Play();
            alreadyPlayed = true;
        }
    }

}
