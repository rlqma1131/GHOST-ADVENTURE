using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Ch2_Bat_Trigger : MonoBehaviour
{   
    Animator ani;
    SpriteRenderer sr;
    Collider2D col;
    [SerializeField] AudioClip TriggerSound_clip;
    [SerializeField] AudioClip ClearSound_clip;
    [SerializeField] YameScan_correct correctDoll; // 정답인형
    private bool Clear_Bat; // 박쥐들 다 없어졌는지

    [Header("Move")]
    [SerializeField] float flySpeed = 2f;          // 속도(유지)
    [SerializeField] int   curvePoints = 5;         // 경로 분할 수(많을수록 더 요철)
    [SerializeField] float jitterAmplitude = 1.0f;  // 흔들림 세기(월드 단위)
    [SerializeField] float pathSpan = 25f;          // 시작점에서 목표까지 대략 거리(카메라 밖까지 충분히)

    [Header("VFX")]
    [SerializeField] float fadeDuration = 0.25f;
    private Collider2D player;

    [Header("Etc")]
    [SerializeField] SoundEventConfig soundConfig;

    bool triggered, dying;
    Vector3 flyDir;

    

    void Start()
    {
        ani = GetComponent<Animator>();
        sr  = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        player = GameObject.FindWithTag("Player").GetComponent<Collider2D>();;
    }

    void Update()
    {
        if(correctDoll.clear_UnderGround && !Clear_Bat)
        {
            OnTriggerEnter2D(player);
            PlaySoundAndFadOut(ClearSound_clip);
            Clear_Bat = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;

        ani.SetBool("Move", true);
        if (soundConfig != null && !correctDoll.clear_UnderGround && !Clear_Bat)
        {
            SoundTrigger.TriggerSound(other.transform.position, soundConfig.soundRange, soundConfig.chaseDuration);
            TutorialManager.Instance.Show(TutorialStep.TouchBat);
        }

        if (col) col.enabled = false;

        // ★ 트리거 순간 플레이어를 향한 레이 방향 고정
        flyDir = (other.transform.position - transform.position).normalized;
        if (flyDir.sqrMagnitude < 1e-6f) flyDir = Vector3.right;

        RunPathTween();
    }

    void RunPathTween()
    {
        PlaySoundAndFadOut(TriggerSound_clip);
        // 수직 방향(2D용)
        Vector3 perp = new Vector3(-flyDir.y, flyDir.x, 0f).normalized;

        // 화면 밖까지 대략 가는 타깃(직선 기준)
        Vector3 end = transform.position + flyDir * pathSpan;

        // 경로 생성: 시작→끝 사이에 약간의 perp 오프셋을 가진 웨이포인트들
        var path = new List<Vector3>();
        path.Add(transform.position);
        for (int i = 1; i < curvePoints; i++)
        {
            float t = i / (float)curvePoints;
            // 직선 보간 + 수직 흔들림(좌우 랜덤)
            float side = Random.Range(-1f, 1f);
            Vector3 point = Vector3.Lerp(transform.position, end, t)
                          + perp * side * jitterAmplitude * (1f - Mathf.Abs(0.5f - t) * 2f); 
            // 중앙 구간에서 흔들림이 크고, 양 끝은 작게(자연스러움)
            path.Add(point);
        }
        path.Add(end);

        // 좌우 뒤집기(초기 방향 기준)
        if (sr) sr.flipX = flyDir.x < 0f;

        // 시퀀스 구성
        var seq = DOTween.Sequence();

        // 이동: 속도 기반, 2D에 맞춘 Path 옵션
       seq.Append(
        transform.DOPath(
            path.ToArray(),
            flySpeed,                      // duration이 아니라 SetSpeedBased 쓸 거라 '속도'처럼 동작
            PathType.Linear,
            PathMode.TopDown2D             // ⬅️ 여기로 이동
        )
        .SetSpeedBased(true)
        .SetEase(Ease.Linear)
    );
    

        // 화면 밖으로 충분히 나갔다고 가정 후 페이드
        if (sr && fadeDuration > 0f)
            seq.Append(sr.DOFade(0f, fadeDuration));

        // 완료 시 파괴
        seq.OnComplete(() =>
        {
            if (this) Destroy(gameObject);
        });

        // 안전: 파괴 시 트윈 정리
        seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    public void PlaySoundAndFadOut(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        // 임시 오브젝트 + AudioSource 생성
        GameObject tempObj = new GameObject("TempSFX_" + clip.name);
        AudioSource tempSource = tempObj.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = volume;
        tempSource.Play();

        // 페이드아웃 트윈
        tempSource.DOFade(0f, clip.length)
                  .SetEase(Ease.Linear)
                  .OnComplete(() => Destroy(tempObj, 0.05f)); // 완전 끝나면 제거
    }
}
    

// ================================================================================================
//  
// public class CarMover : MonoBehaviour
// {
//     [SerializeField] private float moveDistance = 5f;
//     [SerializeField] private float moveDuration = 2f;
//     [SerializeField] private SpriteRenderer spriteRenderer;
//     [SerializeField] private float bounceHeight = 0.2f;     // 들썩 높이
//     [SerializeField] private float bounceDuration = 0.3f;   // 들썩 속도

//     private bool facingRight = true;

//     void Start()
//     {
//         if (spriteRenderer == null)
//             spriteRenderer = GetComponent<SpriteRenderer>();

//         // 위치 저장
//         Vector3 startPos = transform.position;
//         Vector3 endPos = new Vector3(startPos.x + moveDistance, startPos.y, startPos.z);

//         // 좌우 이동 트윈
//         transform.DOMoveX(endPos.x, moveDuration)
//             .SetLoops(-1, LoopType.Yoyo)
//             .SetEase(Ease.InOutSine)
//             .OnStepComplete(FlipX);

//         // 들썩거리는 트윈 (Y축으로 위아래 반복)
//         transform.DOMoveY(startPos.y + bounceHeight, bounceDuration)
//             .SetLoops(-1, LoopType.Yoyo)
//             .SetEase(Ease.InOutSine);

//         flipX = true
//         spriteRenderer.flipX = true;
//         facingRight = false;
//     }

//     void FlipX()
//     {
//         facingRight = !facingRight;
//         spriteRenderer.flipX = !facingRight;
//     }
// }