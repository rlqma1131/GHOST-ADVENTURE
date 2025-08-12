using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Ch1_Cat : MoveBasePossessable
{
    [SerializeField] private LockedDoor door;
    [SerializeField] private GameObject q_Key;
    [SerializeField] private Animator highlightAnim;
    [SerializeField] private AudioClip catMeow;

    private bool isNearDoor = false;
    private bool isActing = false;

    protected override void Start()
    {
        base.Start();
        hasActivated = false;
    }

    protected override void Update()
    {
        if (!hasActivated)
        {
            q_Key.SetActive(false);
            return;
        }
        if (isNearDoor)
        {
            Vector2 catPos = this.transform.position;
            catPos.y += 0.5f;
            q_Key.transform.position = catPos;
            q_Key.SetActive(true);
        }
        else if (!isNearDoor)
        {
            q_Key.SetActive(false);
        }

        if (!isPossessed || !PossessionSystem.Instance.CanMove)
            return;

        if (isActing)
            return;

        Move();

        if (Input.GetKeyDown(KeyCode.E))
        {
            zoomCamera.Priority = 5;
            Unpossess();
            anim.SetBool("Move", false);
        }

        if (Input.GetKeyDown(KeyCode.Q) && isNearDoor)
        {
            q_Key.SetActive(false);
            // 문 열기
            StartCoroutine(CatAct());
        }
    }

    void LateUpdate()
    {
        highlightSpriteRenderer.flipX = spriteRenderer.flipX;
    }

    public override void OnQTESuccess()
    {
        SoulEnergySystem.Instance.RestoreAll();

        PossessionStateManager.Instance.StartPossessionTransition();
    }

    // 문 근처에 있는지 확인
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        if (collision.GetComponent<LockedDoor>() == door)
        {
            isNearDoor = true;
        }

        if(collision.CompareTag("Player") && !SaveManager.IsPuzzleSolved("후라이팬"))
        {
            UIManager.Instance.PromptUI.ShowPrompt("잠들어 있어..깨워볼까?");
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);

        if (collision.GetComponent<LockedDoor>() == door)
        {
            isNearDoor = false;
        }
    }

    public void Blink()
    {
        anim.SetTrigger("Blink");
    }

    public void ActivateCat()
    {
        SoundManager.Instance.PlaySFX(catMeow);
        // 1. 점프 애니메이션
        float jumpHeight = 1.5f;
        float jumpDuration = 0.4f;

        // 현재 위치 저장
        Vector3 originalPos = transform.position;

        // 2. 점프 시퀀스
        Sequence jumpSequence = DOTween.Sequence();

        // 위로 점프
        jumpSequence.Append(transform.DOMoveY(originalPos.y + jumpHeight, jumpDuration * 0.5f).SetEase(Ease.OutQuad));

        // 아래로 착지
        jumpSequence.Append(transform.DOMoveY(originalPos.y, jumpDuration * 0.5f).SetEase(Ease.InQuad));

        // 3. 착지 후 Idle 애니메이션으로 전환
        jumpSequence.AppendCallback(() =>
        {
            hasActivated = true;
            MarkActivatedChanged();
            anim.SetBool("Idle", true);
        });
    }

    IEnumerator CatAct()
    {
        isActing = true;

        anim.SetTrigger("Open");
        door.SolvePuzzle();

        yield return new WaitForSecondsRealtime(2f); // 2초 기다림

        zoomCamera.Priority = 5;
        Unpossess();
        anim.SetTrigger("Sleep");
        hasActivated = false;
        MarkActivatedChanged();

        isActing = false;
    }
}