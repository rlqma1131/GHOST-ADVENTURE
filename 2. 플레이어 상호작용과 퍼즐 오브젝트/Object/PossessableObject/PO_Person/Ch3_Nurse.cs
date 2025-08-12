using Cinemachine;
using UnityEngine;

enum NurseState
{
    Work,
    Rest
}

public class Ch3_Nurse : MoveBasePossessable
{
    [Header("위치")]
    [SerializeField] private Transform[] workPositions; // 일 하는 두 지점
    [SerializeField] private Transform restPosition;    // 쉴 위치

    [Header("시간")]
    [SerializeField] private float waitDuration = 3f;   // 각 지점 대기 시간
    [SerializeField] private float stateChangeInterval = 15f; // 상태 변경 주기

    [Header("하이라이트 셋업")]
    [SerializeField] private SpriteRenderer highlightSprite;
    [SerializeField] private Animator highlightAnimator;

    private PersonConditionUI condition;
    private PersonConditionHandler conditionHandler;

    private NurseState state = NurseState.Work;

    private int currentWorkIndex = 0;
    private float waitTimer = 0f;
    private float stateTimer = 0f;

    private bool isWaiting = false;
    private bool hasWorked = false;
    private bool isAnimatingWork = false;
    private bool isFirstPossessionIn = true;

    protected override void Start()
    {
        base.Start();
        conditionHandler = new VitalConditionHandler();
        condition = GetComponent<PersonConditionUI>();
    }

    protected override void Update()
    {
        // 빙의 상태
        if (isPossessed)
        {   
            UIManager.Instance.tabkeyUI.SetActive(true);
            if (!PossessionSystem.Instance.CanMove)
                return;
             
            Move();

            if (Input.GetKeyDown(KeyCode.E))
            {
                zoomCamera.Priority = 5;
                Unpossess();
            }
        }

        if (isPossessed) return;

        // 빙의 상태가 아닐 때
        stateTimer += Time.deltaTime;
        if (stateTimer >= stateChangeInterval)
        {
            ToggleState();
            stateTimer = 0f;
        }

        // 일정 주기마다 상태 변경
        switch (state)
        {
            case NurseState.Work:
                HandleWork();
                break;
            case NurseState.Rest:
                HandleRest();
                break;
        }

        // 컨디션 UI & QTE 업데이트
        SetCondition(condition.currentCondition);
    }

    void LateUpdate()
    {
        highlightSpriteRenderer.flipX = spriteRenderer.flipX;
    }

    private void HandleWork()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitDuration)
            {
                // 다음 일하는 지점으로 이동
                condition.currentCondition = PersonCondition.Normal;

                isWaiting = false;
                hasWorked = false;
                isAnimatingWork = false;
                currentWorkIndex = (currentWorkIndex + 1) % workPositions.Length;
            }
            else
            {
                // 일하기
                condition.currentCondition = PersonCondition.Tired;

                SetMoveAnimation(false);
                if (!hasWorked)
                {
                    anim.SetTrigger("Work");

                    if (highlightAnimator != null)
                        highlightAnimator.SetTrigger("Work");

                    isAnimatingWork = true;
                    hasWorked = true;
                }
                return;
            }
        }

        if (isAnimatingWork) return;

        Transform target = workPositions[currentWorkIndex];
        MoveTo(target.position);
    }

    private void HandleRest()
    {
        if (Vector2.Distance(transform.position, restPosition.position) > 0.1f)
        {
            // 쉬러 이동
            condition.currentCondition = PersonCondition.Normal;

            MoveTo(restPosition.position);
        }
        else
        {
            // 쉬는 중
            condition.currentCondition = PersonCondition.Vital;

            SetMoveAnimation(false);
        }
    }

    private void MoveTo(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        SetMoveAnimation(true);
        if (spriteRenderer != null)
            spriteRenderer.flipX = direction.x < 0;
        highlightSprite.flipX = direction.x < 0;

        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            isWaiting = true;
            waitTimer = 0f;
        }
    }

    private void SetMoveAnimation(bool isMoving)
    {
        if (anim != null)
            anim.SetBool("Move", isMoving);

        if (highlightAnimator != null && highlightAnimator.runtimeAnimatorController != null && highlightAnimator.isActiveAndEnabled)
            highlightAnimator.SetBool("Move", isMoving);
    }

    private void ToggleState()
    {
        if (state == NurseState.Work)
        {
            state = NurseState.Rest;
        }
        else
        {
            state = NurseState.Work;
            currentWorkIndex = 0;
            isWaiting = false;
        }
    }

    public void SetCondition(PersonCondition condition)
    {
        this.condition.currentCondition = condition;
        switch (condition)
        {
            case PersonCondition.Vital:
                conditionHandler = new VitalConditionHandler();
                break;
            case PersonCondition.Normal:
                conditionHandler = new NormalConditionHandler();
                break;
            case PersonCondition.Tired:
                conditionHandler = new TiredConditionHandler();
                break;
        }

        QTESettings qteSettings = conditionHandler.GetQTESettings();
        UIManager.Instance.QTE_UI_3.ApplySettings(qteSettings);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasActivated)
            return;

        if (other.CompareTag("Player"))
        {
            SyncHighlightAnimator();
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
        }

        SyncHighlightAnimator();
    }

    public void SyncHighlightAnimator()
    {
        if (highlightAnimator == null || anim == null) return;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        // 현재 상태 이름을 Hash로 가져오기
        int currentStateHash = stateInfo.shortNameHash;

        // 하이라이트 Animator에 동일한 상태 강제 재생
        highlightAnimator.Play(currentStateHash, 0, stateInfo.normalizedTime);
    }

    public void InactiveNurse()
    {
        hasActivated = false;
        MarkActivatedChanged();

        zoomCamera.Priority = 5;
    }

    public override void OnPossessionEnterComplete() 
    {
        zoomCamera.Priority = 20;
        anim.SetBool("Move", false);
        if (isFirstPossessionIn)
        {
            isFirstPossessionIn = false;
            UIManager.Instance.PromptUI.ShowPrompt("이 카드키로 콘솔을 조작할 수 있겠어");
        }
    }

    public override void Unpossess()
    {
        base.Unpossess();
        UIManager.Instance.tabkeyUI.SetActive(false);
    }
}
