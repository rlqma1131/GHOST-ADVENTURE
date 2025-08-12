using DG.Tweening;
using UnityEngine;

public class Ch1_Pan : BasePossessable
{
    [SerializeField] private AudioClip isFall;

    [Header("위치 설정")]
    [SerializeField] private Vector3 startLocalPosition;
    [SerializeField] private Quaternion startLocalRotation = Quaternion.identity;
    [SerializeField] private float dropYPos = -1.5f;

    [SerializeField] private Ch1_Cat cat;
    [SerializeField] private Ch1_MemoryFake_02_Cake cake;
    [SerializeField] private Ch1_Mouse mouse;

    [SerializeField] private GameObject q_Key;
    private bool isFalling = false;
    
    [SerializeField] private SoundEventConfig soundConfig;

    protected override void Start()
    {
        base.Start();
        transform.localPosition = startLocalPosition;
        transform.localRotation = startLocalRotation;
    }

    protected override void Update()
    {
        base.Update();

        if (!isPossessed || !hasActivated || isFalling)
        {
            q_Key.SetActive(false);
            return;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            q_Key.SetActive(false);
            TriggerPanEvent();
        }
        q_Key.SetActive(true);
    }

    private void TriggerPanEvent()
    {
        isFalling = true;

        Sequence panSequence = DOTween.Sequence();

        panSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, -60f), 0.5f).SetEase(Ease.InQuad));
        panSequence.Join(transform.DOLocalMoveY(dropYPos, 0.5f).SetEase(Ease.InQuad));

        panSequence.AppendCallback(() =>
        {
            SoundManager.Instance.PlaySFX(isFall);
            SoundTrigger.TriggerSound(transform.position, soundConfig.soundRange, soundConfig.chaseDuration);
        });

        panSequence.Append(transform.DOLocalRotateQuaternion(startLocalRotation, 0.2f).SetEase(Ease.OutBounce));
        panSequence.AppendInterval(0.05f);

        panSequence.AppendCallback(() =>
        {
            mouse.ActivateMouse();
            cat.ActivateCat();
            cake.ActivateCake();

            hasActivated = false;
            MarkActivatedChanged();

            Unpossess();
            isFalling = false;
        });
        SaveManager.MarkPuzzleSolved("후라이팬");
        UIManager.Instance.PromptUI.ShowPrompt("으악! 소리가 너무 커");
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {   
        base.OnTriggerEnter2D(collision);
        if(collision.CompareTag("Player") && !SaveManager.IsPuzzleSolved("후라이팬"))
        {
            UIManager.Instance.PromptUI.ShowPrompt("후라이팬이네. 떨어뜨려 볼까?");
        }
    }
}
