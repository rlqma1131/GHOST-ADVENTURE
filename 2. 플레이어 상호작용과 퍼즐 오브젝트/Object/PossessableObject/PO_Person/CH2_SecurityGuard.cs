using UnityEngine;
using DG.Tweening;
using System.Linq;


public enum GuardState { Idle, MovingToRadio, TurnOffRadio, MovingToBench, Resting, MovingToOffice, InOffice, Work, Roading }

public class CH2_SecurityGuard : MoveBasePossessable
{   
    // [SerializeField] private LockedDoor door; //도어락 있는 문 //없어도 될 것 같음
    [SerializeField] private Ch2_DoorLock doorLock; // 도어락
    [SerializeField] private SafeBox safeBox; // 금고
    [SerializeField] private Ch2_Radio radio; // 라디오
    public Transform Radio; // 라디오 위치
    public Transform bench; // 벤치 위치
    public Transform OfficeDoor_Outside; // 경비실 문(밖)
    public Transform OfficeDoor_Inside; // 경비실 문(안)
    public Transform chair; // 경비실 안 의자 위치
    private GuardState state; // 경비원의 상태
    private float turnOffRadioTimer = 0f;
    private float turnOffRadioDuration = 2f; // 라디오 끄는 시간
    private float restTimer = 0f; 
    public float restDuration = 3f; // 휴식시간
    private float roadingTimer = 0f;
    private float roadingDuration = 5f; // 로딩시간

    public PersonConditionUI targetPerson;
    public PersonConditionHandler conditionHandler;
    [SerializeField] private GameObject q_Key;
    // private bool isNearDoor = false;
    private HaveItem haveitem;
    private bool isInOffice;// 경비실 안에 있는지 확인
    private bool oneTimeShowClue = false; // 경비원 단서 - Clue:Missing 확대뷰어로 보여주기용(1번만)
    public bool isdoorLockOpen; // 도어락 스크립트에서 정보 넣어줌
    public bool doorPass = false;
    public bool UseAllItem = false;
    private BoxCollider2D[] cols;

    // 처음 시작시 빙의불가(경비실안에 있음)
    protected override void Start()
    {
        base.Start();
        moveSpeed = 2f;
        hasActivated = false;
        isInOffice = true;
        haveitem = GetComponent<HaveItem>();
        targetPerson = GetComponent<PersonConditionUI>();
        targetPerson.currentCondition = PersonCondition.Unknown;
        cols = GetComponentsInChildren<BoxCollider2D>();
    }

    protected override void Update()
    {
        if (radio != null && radio.IsPlaying)
        {
            // anim.Play("Idle");
            state = GuardState.MovingToRadio;
        }
        
        switch (state)
        {
            case GuardState.MovingToRadio:
                CheckInOut_GoToRadio();
                break;

            case GuardState.TurnOffRadio:
                turnOffRadioTimer += Time.deltaTime;
                if(turnOffRadioTimer >= turnOffRadioDuration) 
                {
                    targetPerson.currentCondition = PersonCondition.Normal;
                    state = GuardState.MovingToBench;
                }
                break;

            case GuardState.MovingToBench:
                MoveTo(bench.position);
                break;

            case GuardState.Resting:
                restTimer += Time.deltaTime;
                if (restTimer >= restDuration)
                {
                    targetPerson.currentCondition = PersonCondition.Vital;
                    TutorialManager.Instance.Show(TutorialStep.SecurityGuard_AfterRest);
                    state = GuardState.MovingToOffice;
                }
                break;

            case GuardState.MovingToOffice:
                MoveTo(OfficeDoor_Outside.position);
                break;
            
            case GuardState.InOffice:
                break;

            case GuardState.Work:
                MoveTo(chair.position);
                break;

            case GuardState.Roading:
                radio.triggerSound_Person.Stop();
                roadingTimer += Time.deltaTime;
                targetPerson.currentCondition = PersonCondition.Normal;
                if(roadingTimer >= roadingDuration) 
                {
                    if(isInOffice)
                        state = GuardState.Work;
                    else
                        state = GuardState.MovingToOffice;
                }
                break;
        }

        targetPerson.SetCondition(targetPerson.currentCondition);

        if (!isPossessed)
            return;

        
        if(radio.IsPlaying)
        {
            radio.triggerSound_Person.DOFade(0f, 1.5f)
            .OnComplete(() => radio.triggerSound_Person.Stop());
        }
        UIManager.Instance.tabkeyUI.SetActive(true);

        Move();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (doorPass)
            {
                OnDoorInteract();
                return;
            }

            if (haveitem.IsInventoryEmpty())
            {
                zoomCamera.Priority = 5;
                Unpossess();
                hasActivated = false;
                MarkActivatedChanged();

                UseAllItem = true;
            }
            else
                UIManager.Instance.PromptUI.ShowPrompt("뭔가 더 얻을 수 있는게 있을것 같아");
        }
                

            // zoomCamera.Priority = 5;
            // Unpossess();
            
        

        // 단서 관련 로직 (추후 수정예정)---------------------------
        if (isPossessed && Input.GetKeyDown(KeyCode.Alpha3) &&!oneTimeShowClue || 
            isPossessed && Input.GetKeyDown(KeyCode.Keypad3) && !oneTimeShowClue)
        {
            UIManager.Instance.InventoryExpandViewerUI.OnClueHidden += ShowText;
            oneTimeShowClue = true;
            SaveManager.MarkPuzzleSolved("메모3");
        }
        // if(oneTimeShowClue && !oneTimeActionDelete)
        // {
        //     if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4)||
        //     Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Keypad4))
        //     {
        //         UIManager.Instance.InventoryExpandViewerUI.OnClueHidden -= ShowText;
        //         oneTimeActionDelete = true;
        //     }
        // }

        // var found = haveitem.inventorySlots.Find(slot => slot?.item?.name == "MISSING");
        // if (isPossessed && found == null)
        // {
        //     var viewer = UIManager.Instance.InventoryExpandViewerUI;
        //     viewer.OnClueHidden += ShowText;
        // }
    }

    // 목적지까지 이동
    void MoveTo(Vector3 target)
    {   
        if(!isPossessed)
        {   
            anim.SetBool("Work", false);
            anim.SetBool("Rest", false);
            anim.SetBool("Move", true);
            Vector3 targetPos = transform.position;
            targetPos.x = target.x;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            if(transform.position.x - target.x >0)
                spriteRenderer.flipX = true;
            else
                spriteRenderer.flipX = false;
            if (Mathf.Abs(transform.position.x - target.x) < 0.1f)
            {
                OnDestinationReached(target);
            }
        }
    }

    // protected override void Move()
    // {
    //     float h = Input.GetAxis("Horizontal");

    //     Vector3 move = new Vector3(h, 0, 0);

    //     // 이동 여부 판단
    //     bool isMoving = move.sqrMagnitude > 0.01f;

    //     if (anim != null)
    //     {
   
    //             anim.SetBool("Move", isMoving);
    //     }

    //     if (isMoving)
    //     {
    //         transform.position += move * moveSpeed * Time.deltaTime;

    //         // 좌우 Flip
    //         if (spriteRenderer != null && Mathf.Abs(h) > 0.01f)
    //         {
    //             spriteRenderer.flipX = h < 0f;
    //         }
    //     }
    // }

    // 목적지 도착시 처리
    void OnDestinationReached(Vector3 destination)
    {
        // 라디오에 도착했을 때
        if (destination == Radio.position) 
        {
            state = GuardState.TurnOffRadio;
            anim.SetBool("Move", false);
        }

        // 벤치에 도착했을 때
        else if (destination == bench.position)
        {   
            state = GuardState.Resting;
            // anim.SetBool("Move", false);
            anim.SetBool("Rest", true);
            restTimer = 0f;
        }
        
        // 경비실 문(밖)에 도착했을 때
        else if (destination == OfficeDoor_Outside.position)
        {
            if(!isPossessed)
            {
                TutorialManager.Instance.Show(TutorialStep.SecurityGuard_InOffice);
                Vector3 targetPos = transform.position;
                targetPos.x = OfficeDoor_Inside.position.x;
                transform.position = targetPos;
                state = GuardState.Work;
            }
        }

        // 경비실 의자에 도착했을 때
        else if (destination == chair.position)
        {
            anim.SetBool("Move", false);
            anim.SetBool("Work", true);
            targetPerson.currentCondition = PersonCondition.Vital;
            if(UseAllItem)
            {
                foreach(BoxCollider2D col in cols)
                {
                    col.enabled = false;
                }
            }
        }
    }

    // 경비원이 있는 곳이 안인지 밖인지 확인 후 라디오로 가게만듬.
    private void CheckInOut_GoToRadio()
    {
        if(isInOffice)
        {
            MoveTo(OfficeDoor_Inside.position);
            if(transform.position.x == OfficeDoor_Inside.position.x)
            {   
                Vector3 guardPos = transform.position;
                guardPos.x = OfficeDoor_Outside.position.x + 0.5f;
                transform.position = guardPos;
            }

        }
        else
        {
            MoveTo(Radio.position);
            TutorialManager.Instance.Show(TutorialStep.SecurityGuard_GoToRadio);
        }
    }

    public override void OnQTESuccess()
    {
        SoulEnergySystem.Instance.RestoreAll();

        PossessionStateManager.Instance.StartPossessionTransition();
    }

    // 경비원이 있는 곳이 경비실 안인지 밖인지 확인 (트리거)
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // if (!hasActivated)
        //     return;

        base.OnTriggerEnter2D(collision);
        // if (collision.CompareTag("Player"))
        //     PlayerInteractSystem.Instance.AddInteractable(gameObject);

        if(collision.CompareTag("In"))
        {
            isInOffice = true;
            hasActivated = false;
            MarkActivatedChanged();

            targetPerson.currentCondition = PersonCondition.Unknown;
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if(other.CompareTag("In"))
        {
            targetPerson.currentCondition = PersonCondition.Unknown;
        }
    }
    
    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
        // if (collision.CompareTag("Player"))
        //     PlayerInteractSystem.Instance.RemoveInteractable(gameObject);

        if(collision.CompareTag("In"))
        {
            isInOffice = false;
            hasActivated = true;
            MarkActivatedChanged();
        }
    }

    // 문 앞에서 E키 눌렀을 때 빙의해제되는게 아니고 다른 문으로 이동
    protected override void OnDoorInteract()
    {
        if(isPossessed && doorPass && isdoorLockOpen && !isInOffice)
        {
            Vector3 newPos = transform.position;
            newPos.x = OfficeDoor_Inside.position.x;
            transform.position = newPos;
        }
        else if(isPossessed && doorPass && isdoorLockOpen && isInOffice)
        {
            Vector3 newPos = transform.position;
            newPos.x = OfficeDoor_Outside.position.x;
            transform.position = newPos;
        }
    }

    // 빙의 해제시
    public override void Unpossess()
    {
        radio.triggerSound_Person.Stop();
        base.Unpossess();
        UIManager.Instance.tabkeyUI.SetActive(false);
        state = GuardState.Roading;
        anim.SetBool("Move", false);
        roadingTimer = 0f;
    }
    public override void OnPossessionEnterComplete() 
    {   
        anim.SetBool("Rest", false);
        anim.SetBool("Move", false); 
        base.OnPossessionEnterComplete();
        radio.triggerSound_Person.DOFade(0f, 5f)
        .OnComplete(() => radio.triggerSound_Person.Stop());
        highlight.SetActive(false);
    }

    // 단서 획득시 대사 출력
    void ShowText()
    {
        UIManager.Instance.PromptUI.ShowPrompt("잃어버린 게 뭘까...? 사람일까, 기억일까.", 2f);
    }
}