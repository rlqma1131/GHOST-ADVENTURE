using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class Ch2_LaserTrigger : MonoBehaviour
{


    private bool isTriggered = false; // 트리거 상태를 나타내는 변수
    [SerializeField] private PlayableDirector director; // 타임라인 디렉터를 연결할 변수

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && !isTriggered)
        {
            director.Play(); // 타임라인 재생
            PossessionSystem.Instance.CanMove = false; // 플레이어 이동 불가능하게 설정
            UIManager.Instance.PlayModeUI_CloseAll(); // 플레이모드 UI 일시정지
            director.stopped += OnTimelineFinished; 
            Debug.Log("레이저 트리거"); 
            isTriggered = true; 

        }
    }
    private void OnTimelineFinished(PlayableDirector obj)
    {
        director.stopped -= OnTimelineFinished; 
    }


}
