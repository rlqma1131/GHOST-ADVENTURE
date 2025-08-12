using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraChange : MonoBehaviour
{

    public  CinemachineVirtualCamera Vcam; // 현재 카메라를 참조하기 위한 변수

    private int CurrentPriority = 10; // 현재 카메라 우선순위
    private int ActivePriority = 13; // 활성화된 카메라 우선순위

    //[SerializeField] private UITweenAnimator uITweenAnimator; // UI 애니메이션 컴포넌트
    //[SerializeField] private TextMeshProUGUI text; // 방 이름 텍스트 컴포넌트



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 플레이어가 카메라 변경 영역에 들어오면 현재 카메라의 우선순위를 낮추고
            // 새로운 카메라의 우선순위를 높입니다.
            Vcam.Priority = ActivePriority;

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 플레이어가 카메라 변경 영역을 벗어나면 현재 카메라의 우선순위를 원래대로 되돌립니다.
            Vcam.Priority = CurrentPriority;
        }
    }


}
