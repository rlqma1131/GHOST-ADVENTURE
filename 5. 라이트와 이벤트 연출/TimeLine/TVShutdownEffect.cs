using UnityEngine;
using DG.Tweening;
using System.Collections;

public class TVShutdownEffect : MonoBehaviour
{
    [Header("오브젝트 설정")]
    [SerializeField] private SpriteRenderer tvScreen;

    [Header("속도 설정 (값이 작을수록 빨라짐)")]
    [SerializeField] private float turnOnDuration = 1.1f;

    [SerializeField] private float waitTime;
    private Vector3 originalScale;

    public AudioSource CCTVSound;

    private void Awake()
    {
        if (tvScreen != null)
        {
            originalScale = tvScreen.transform.localScale;

            // 초기에는 완전히 꺼진 상태
            tvScreen.transform.localScale = Vector3.zero;
            tvScreen.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    private void Start()
    {
        StartCoroutine(RandomTurnOnCoroutine());
    }

    private IEnumerator RandomTurnOnCoroutine()
    {
        yield return new WaitForSecondsRealtime(waitTime); // ← 수정됨
        PlayTurnOn();
    }

    [ContextMenu("Start TurnOn")]
    public void PlayTurnOn()
    {
        DOTween.Kill(this);
        if (tvScreen == null) return;

        tvScreen.enabled = true;

        // 애니메이션 시작 전 초기 상태 설정
        tvScreen.color = new Color(1f, 1f, 1f, 0f);
        tvScreen.transform.localScale = Vector3.zero;

        // timeScale = 0에서도 작동
        Sequence turnOnSequence = DOTween.Sequence().SetId(this).SetUpdate(true);
        CCTVSound.PlayOneShot(CCTVSound.clip);

        turnOnSequence.Append(tvScreen.transform.DOScaleX(originalScale.x, turnOnDuration * 0.3f).SetEase(Ease.OutExpo));
        turnOnSequence.Join(tvScreen.DOFade(1f, turnOnDuration * 0.2f));
        turnOnSequence.Append(tvScreen.transform.DOScaleY(originalScale.y, turnOnDuration * 0.7f).SetEase(Ease.OutBounce));
    }
}
