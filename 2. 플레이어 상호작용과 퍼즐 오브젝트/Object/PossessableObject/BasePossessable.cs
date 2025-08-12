using UnityEngine;

public abstract class BasePossessable : BaseInteractable
{
    [SerializeField] protected Animator anim;
    [SerializeField] protected bool hasActivated; // 빙의가 가능한 상태인지 여부
    [SerializeField] private AudioClip possessionSFX;

    public bool isPossessed;
    public bool HasActivated => hasActivated;
    public bool IsPossessedState => isPossessed;

    protected virtual void Start()
    {
        isPossessed = false;
        hasActivated = true;
    }

    protected virtual void Update()
    {
        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.E))
            Unpossess();
    }

    // 상호작용 메시지 표시 대상 설정
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasActivated)
            return;

        if (other.CompareTag("Player"))
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
    }

    protected override void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }

    public virtual void Unpossess()
    {
        UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode("빙의 해제", 2f);
        isPossessed = false;
        PossessionStateManager.Instance.StartUnpossessTransition();
    }

    public virtual void OnQTESuccess()
    {
        // 빙의 효과음
        //SoundManager.Instance.PlaySFX(possessionSFX);

        PossessionStateManager.Instance.StartPossessionTransition();
    }

    public void OnQTEFailure()
    {
        isPossessed = false;
    }

    // 빙의 애니메이션이 끝나면 호출되는 메서드
    // 필요에 따라 빙의애니메이션 끝나면 구현되는 기능들을 넣어주세요.
    public virtual void OnPossessionEnterComplete() { }

    public virtual void CantPossess() { }

    // 로드 시 상태 셋업
    public void ApplyHasActivatedFromSave(bool value)
    {
        if (hasActivated == value) return;
        hasActivated = value;
        OnRestoredHasActivated(value);
    }

    // 추후 VFX/콜라이더/애니 갱신 등
    protected virtual void OnRestoredHasActivated(bool value) { }

    // 상태 기록
    protected void MarkActivatedChanged()
    {
        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetPossessableState(uid.Id, hasActivated);
    }
}
