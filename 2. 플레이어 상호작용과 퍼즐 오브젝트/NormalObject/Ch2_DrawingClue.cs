using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Ch2_DrawingClue : MonoBehaviour
{
    [Header("프롬프트 메시지 설정")] [SerializeField]
    private string promptMessage;
    
    [SerializeField] private GameObject drawingZoom;             // 확대용 UI (Canvas 내)
    [SerializeField] private RectTransform drawingPos;           // 시작 위치
    [SerializeField] private Image zoomPanel;                    // 배경 패널 (페이드용)
    [SerializeField] private GameObject nextClueObject;

    private CluePickup cluePickup;
    private bool isPlayerInside = false;
    private bool isZoomActive = false;
    private bool zoomActivatedOnce = false;
    [SerializeField] private bool hasActivated = false;
    [SerializeField] private bool isLastClue = false;
    [SerializeField] private Ch2_BackStreetObj finalObjectToActivate; 
    [SerializeField] private CinemachineVirtualCamera vcam;

    void Start()
    {
        cluePickup = GetComponent<CluePickup>();

        // 초기화
        drawingZoom.SetActive(false);
        drawingPos.anchoredPosition = new Vector2(0, -Screen.height);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!hasActivated || (!isZoomActive && !isPlayerInside))
                return;

            if (isZoomActive)
                HideDrawingZoom();
            else
                ShowDrawingZoom();
        }
    }

    private void ShowDrawingZoom()
    {
        isZoomActive = true;
        EnemyAI.PauseAllEnemies();

        // 배경 패널 페이드 인
        zoomPanel.color = new Color(zoomPanel.color.r, zoomPanel.color.g, zoomPanel.color.b, 0f);
        zoomPanel.DOFade(150f / 255f, 0.5f);

        // UI 슬라이드 인
        drawingZoom.SetActive(true);
        drawingPos.anchoredPosition = new Vector2(0, -Screen.height);
        drawingPos.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic);

        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
        
        if (cluePickup != null)
        {
            cluePickup.PickupClue();
        }
        
        if (!string.IsNullOrEmpty(promptMessage))
        {
            UIManager.Instance.PromptUI.ShowPrompt(promptMessage, 2f);
        }
    }

    private void HideDrawingZoom()
    {
        isZoomActive = false;
        EnemyAI.ResumeAllEnemies();

        // 페이드 아웃
        zoomPanel.DOFade(0f, 0.5f);

        // UI 슬라이드 아웃
        drawingPos.DOAnchorPos(new Vector2(0, -Screen.height), 0.5f)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                drawingZoom.SetActive(false);

                if (!zoomActivatedOnce)
                {
                    // cluePickup?.PickupClue();
                    
                    if (isLastClue && finalObjectToActivate != null)
                    {
                        PlayFinalLightSequence();
                    }

                    if (nextClueObject != null)
                    {
                        var nextClue = nextClueObject.GetComponent<Ch2_DrawingClue>();
                        if (nextClue != null)
                            nextClue.Activate();
                    }
                    
                    zoomActivatedOnce = true;
                    GetComponent<Collider2D>().enabled = false;
                }

                if (isPlayerInside)
                    PlayerInteractSystem.Instance.AddInteractable(gameObject);
            });
    }
    
    private void PlayFinalLightSequence()
    {
        // 1) 플레이어 잠금 + VCam Follow 끊기
        PossessionSystem.Instance.CanMove = false;
        var oldFollow = vcam.Follow;
        vcam.Follow = null;

        // 2) VCam 트랜스폼 & 위치 계산
        var camTrans = vcam.transform;
        Vector3 orig = camTrans.position;
        Vector3 dest = finalObjectToActivate.transform.position;
        dest.z = orig.z;

        // 3) 시퀀스 구성
        float moveT   = 0.8f;
        float revealT = finalObjectToActivate.fadeInTime
                        + finalObjectToActivate.holdTime
                        + finalObjectToActivate.fadeOutTime
                        + finalObjectToActivate.moveDownTime;

        DOTween.Sequence()
               // 카메라(VCam) 이동
               .Append(camTrans.DOMove(dest, moveT).SetEase(Ease.InOutSine))
               // 단서 애니메이션 시작
               .AppendCallback(() => finalObjectToActivate.OnFinalClueActivated())
               // 애니메이션 전체 시간 대기
               .AppendInterval(revealT)
               // 카메라(VCam) 원위치 복귀
               .Append(camTrans.DOMove(orig, moveT).SetEase(Ease.InOutSine))
               // Follow 복구 & 플레이어 언락
               .AppendCallback(() =>
               {
                   vcam.Follow = oldFollow;
                   PossessionSystem.Instance.CanMove = true;
               });
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasActivated || !other.CompareTag("Player")) return;

        isPlayerInside = true;

        if (!isZoomActive)
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;

        if (isZoomActive)
            HideDrawingZoom(); // 범위 밖이면 자동 닫기

        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }
    
    public void Activate()
    {
        hasActivated = true;
    }
}
