using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Ch2_BackStreetObj : MonoBehaviour
{
    [Header("연출용 오브젝트")]
    [SerializeField] private SpriteRenderer shadowObject;
    [SerializeField] private SpriteRenderer revealObject;
    [SerializeField] private Transform revealMoveTarget;
    [SerializeField] private Transform fallTargetPoint;
    
    [Header("연출 설정")]
    [SerializeField]
    public float fadeInTime = 0.5f;
    [SerializeField] public float fadeOutTime = 1.0f;
    [SerializeField] private float moveDownDistance = 100f;
    [SerializeField] public float moveDownTime = 0.7f;
    [SerializeField] public float holdTime;

    public void OnFinalClueActivated()
    {
        Sequence seq = DOTween.Sequence();

        // 1. 어두운 물체 빠르게 나타남
        shadowObject.color = new Color(1, 1, 1, 0);
        shadowObject.gameObject.SetActive(true);
        seq.Append(shadowObject.DOFade(1f, fadeInTime));

        // 2. 잠시 유지 후 서서히 사라짐
        seq.AppendInterval(0.5f);
        seq.Append(shadowObject.DOFade(0f, fadeOutTime));

        // 3. 새로운 물체 나타남
        revealObject.color = new Color(1, 1, 1, 0);
        revealObject.gameObject.SetActive(true);
        seq.Append(revealObject.DOFade(1f, 0.7f));

        // 4. 바닥으로 스르륵 떨어짐
        seq.Join(revealMoveTarget.DOMove(fallTargetPoint.position, moveDownTime).SetEase(Ease.OutCubic));

        // // 5. 최종 기억 조각 활성화
        // seq.AppendCallback(() =>
        // {
        //     if (memoryFragmentObject != null)
        //     {
        //         memoryFragmentObject.ActivateBackStreetObj();
        //     }
        // });
        TutorialManager.Instance.Show(TutorialStep.BlackShadow);
        ChapterEndingManager.Instance.CollectCh2Clue("L");
    }
}
