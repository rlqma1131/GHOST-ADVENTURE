using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_Bottle : BasePossessable
{
    [SerializeField] private AudioClip isFall;

    [Header("위치 설정")]
    [SerializeField] private Vector3 startLocalPosition;
    [SerializeField] private Quaternion startLocalRotation = Quaternion.identity;
    [SerializeField] private float dropYPos = -1.5f;
    [SerializeField] private GameObject q_Key;
    [SerializeField] private SoundEventConfig soundConfig;
    
    protected override void Start()
    {
        base.Start();
        // 시작 시 위치와 회전 적용
        transform.localPosition = startLocalPosition;
        transform.localRotation = startLocalRotation;

        anim = GetComponentInChildren<Animator>();
    }

    protected override void Update()
    {
        base.Update();

        if (!isPossessed)
        {
            q_Key.SetActive(false);
            return;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            q_Key.SetActive(false);
            TriggerBottleEvent();
        }
        q_Key.SetActive(true);
    }

    private void TriggerBottleEvent()
    {
        // 애니메이션 시퀀스 생성
        Sequence panSequence = DOTween.Sequence();

        // 1. 병이 기울이며 아래로 떨어짐 (0.3초)
        panSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, -30f), 0.5f).SetEase(Ease.InQuad));
        panSequence.Join(transform.DOLocalMoveY(dropYPos, 0.5f).SetEase(Ease.InQuad));

        // 2. 낙하 후 사운드 재생 및 AI 유인
        panSequence.AppendCallback(() =>
        {
            anim.SetTrigger("Fall"); // 깨짐
            SoundManager.Instance.PlaySFX(isFall);

            SoundTrigger.TriggerSound(transform.position, soundConfig.soundRange, soundConfig.chaseDuration);
        });

        // 3. 회전 원래대로 복귀 (0.2초)
        panSequence.Append(transform.DOLocalRotateQuaternion(startLocalRotation, 0.2f).SetEase(Ease.OutBounce));

        // 4. 0.05초 후 관련 이벤트 실행
        panSequence.AppendInterval(0.05f);
        panSequence.AppendCallback(() =>
        {
            hasActivated = false;
            MarkActivatedChanged();

            Unpossess();
        });
    }
}