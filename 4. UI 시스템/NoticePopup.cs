using UnityEngine;
using TMPro;
using DG.Tweening;
//using Microsoft.Unity.VisualStudio.Editor;

public class NoticePopup : MonoBehaviour
{
    //public RectTransform uiElement;
    // public CanvasGroup canvasGroup; // CanvasGroup 컴포넌트 

    public TextMeshProUGUI text;
    [SerializeField] CanvasGroup canvasGroup;
    public float duration = 1f;
    
    //public float startX = -788f;
    //public float endX = 0f;


    public float waitTime = 2.0f; // UI가 화면에 머무는 시간 (1초)

    //public void SlideInAndOut()
    //{
    //    //초기 위치 설정

    //    Vector2 startPos = new Vector2(startX, uiElement.anchoredPosition.y);
    //    uiElement.anchoredPosition = startPos;


    //    Sequence sequence = DOTween.Sequence();

    //    //슬라이드 인 애니메이션
    //    sequence.Append(
    //        uiElement.DOAnchorPosX(endX, duration)
    //                 .SetEase(Ease.OutQuad)
    //    );

    //    //  대기 시간
    //    sequence.AppendInterval(waitTime);

    //    // 슬라이드 아웃 애니메이션
    //    sequence.Append(
    //        uiElement.DOAnchorPosX(startX, duration)
    //                 .SetEase(Ease.InQuad) 
    //    );


    //    sequence.Play();
    //}
    void Start()
    {
        gameObject.SetActive(false);
    }
    
    public void FadeInAndOut(string notice)
    {   
        if (canvasGroup == null || text == null)
        {
            Debug.LogError("CanvasGroup 또는 Text가 할당되지 않았습니다.");
            return;
        }
        text.text = notice;

        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(true);

        Sequence sequence = DOTween.Sequence();

        sequence.Append(canvasGroup.DOFade(1f, duration).SetEase(Ease.InOutSine));
        sequence.AppendInterval(waitTime);
        sequence.Append(canvasGroup.DOFade(0f, duration).SetEase(Ease.InOutSine));
        sequence.OnComplete(() => canvasGroup.gameObject.SetActive(false));

        sequence.Play();
    }

}