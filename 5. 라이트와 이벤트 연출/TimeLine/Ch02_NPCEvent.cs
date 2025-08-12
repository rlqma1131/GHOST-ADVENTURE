using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
public class Ch02_NPCEvent : MonoBehaviour
{

    [SerializeField] private PlayableDirector director; // 타임라인 디렉터
    private bool isTimelinePlaying = false; // 타임라인 재생 여부

    SpriteRenderer spriteRenderer; // 스프라이트 렌더러
    [SerializeField] private RoomInfo roomInfo; // 방 정보
    [SerializeField] private GameObject kid; // Ch2_kid    d 

    private void Update()
    {
        if (roomInfo.roomCount >= 1)
        {
            kid.SetActive(false); // 방 방문 수가 1 이상이면 Ch2_kid 비활성화

        }

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isTimelinePlaying && roomInfo.roomCount ==0)
        {
            if (director != null)
            {
                spriteRenderer = collision.gameObject.GetComponent<SpriteRenderer>();
                GameObject player = collision.gameObject;
                Vector3 scale = player.transform.localScale;
                spriteRenderer.flipX = true; // 플레이어 스프라이트를 왼쪽으로 뒤집기
                //scale.x = -Mathf.Abs(scale.x); // 항상 왼쪽 보게
                player.transform.localScale = scale;
                PossessionSystem.Instance.CanMove = false; // 플레이어 이동 불가능하게 설정
                UIManager.Instance.PlayModeUI_CloseAll(); // 플레이모드 UI 닫기
                director.Play();
                EnemyAI.PauseAllEnemies();
                isTimelinePlaying = true; // 타임라인 재생 상태로 설정
                roomInfo.roomCount++; // 방 방문 수 증가
            SaveManager.SetRoomVisitCount(roomInfo.roomName, roomInfo.roomCount); 
            }
        }
    }
    private void OnEnable()
    {
        if (director != null)
        {
            director.stopped += OnTimelineStopped; // 타임라인이 중지될 때 이벤트 등록
        }
    }


    private void OnTimelineStopped(PlayableDirector playable)
    {
        PossessionSystem.Instance.CanMove = true; // 플레이어 이동 가능하게 설정
        UIManager.Instance.PlayModeUI_OpenAll(); // 플레이모드 UI 다시 열기
        UIManager.Instance.PromptUI.ShowPrompt("이 쪽지는 나를 말하는 건가?", 1.5f); // 프롬프트 UI 닫기
        EnemyAI.ResumeAllEnemies();
         // Ch2_kid 오브젝트 비활성화

    }

    private void OnDisable()
    {
        if (director != null && spriteRenderer != null)
        {
            if(spriteRenderer != null)
            {


                spriteRenderer.flipX = false; // 플레이어 스프라이트를 원래대로 돌리기
            director.stopped -= OnTimelineStopped;
            }
        }
    }
}

