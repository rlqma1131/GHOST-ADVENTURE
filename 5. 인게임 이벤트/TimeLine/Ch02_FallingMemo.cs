using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch02_FallingMemo : MonoBehaviour
{
    [SerializeField] private float fallDistance = 1f; // 얼마나 아래로 떨어질지
    [SerializeField] private float fallDuration = 2.5f; // 떨어지는 시간
    [SerializeField] private float swayAmount = 1f; // 좌우 흔들리는 정도
    [SerializeField] private float swayDuration = 1f; // 흔들리는 속도

    private void Start()
    {



    }

    public void MemoDrop()
    {

        Sequence leafSequence = DOTween.Sequence();

        // 좌우로 살짝 흔들리는 트윈 (2번만 왔다갔다)
        leafSequence.Append(transform.DOLocalMoveX(transform.localPosition.x + swayAmount, swayDuration / 2f).SetEase(Ease.InOutSine));
        leafSequence.Append(transform.DOLocalMoveX(transform.localPosition.x - swayAmount, swayDuration).SetEase(Ease.InOutSine));
        leafSequence.Append(transform.DOLocalMoveX(transform.localPosition.x, swayDuration / 2f).SetEase(Ease.InOutSine));

        // 동시에 아래로 떨어지는 트윈
        transform.DOMoveY(transform.position.y - fallDistance, fallDuration).SetEase(Ease.Linear);
    }
}
