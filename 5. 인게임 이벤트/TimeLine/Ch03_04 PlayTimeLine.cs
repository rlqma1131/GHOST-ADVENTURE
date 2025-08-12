using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Ch03_04PlayTimeLine : MonoBehaviour
{

    [SerializeField]PlayableDirector _playableDirector;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 플레이어가 트리거에 들어오면 타임라인 재생
            _playableDirector.Play();
        }
    }
}
 