using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    public Animator Animator { get; private set; }
    public EnemyMovementController Movement { get; private set; }
    public EnemyDetection Detection { get; private set; }
    public EnemyQTEHandler QTEHandler { get; private set; }

    public IdleState IdleState { get; private set; }
    public PatrolState PatrolState { get; private set; }
    public ChaseState ChaseState { get; private set; }
    public QTEState QTEState { get; private set; }
    public InvestigateState InvestigateState { get; private set; }

    private protected EnemyState currentState;
    public Vector3 startPosition;
    public static bool IsAnyQTERunning = false;

    private Coroutine soundChaseCoroutine;
    public bool isTeleporting { get; private protected set; }
    
    // [SerializeField] private float normalDetectionRange = 4f;
    [SerializeField] private float soundDetectionRange = 8f;

    // [SerializeField] private float normalDetectionAngle = 60f;
    // [SerializeField] private float soundDetectionAngle = 360f;
    
    public static bool IsPaused { get; private set; } = false;
    public static void PauseAllEnemies()  => IsPaused = true;
    public static void ResumeAllEnemies() => IsPaused = false;
    public bool IsSoundChaseActive => soundOmniActive;
    
    private bool soundOmniActive = false;
    
    [SerializeField] public float reacquireCooldownAfterSearch = 1.2f; // 수색 끝난 뒤 잠깐 다시 못 쫓게
    [SerializeField] private int   framesToReacquire = 6;               // 연속 N프레임 보이면만 재추격(≈0.1s)
    
    private void Awake()
    {
        Animator = GetComponent<Animator>();
        Movement = GetComponent<EnemyMovementController>();
        Detection = GetComponent<EnemyDetection>();
        QTEHandler = GetComponent<EnemyQTEHandler>();

        IdleState = new IdleState(this);
        PatrolState = new PatrolState(this);
        ChaseState = new ChaseState(this);
        QTEState = new QTEState(this);
        InvestigateState = new InvestigateState(this);

        startPosition = transform.position;
        
        if (Detection != null)
        {
            Detection.frontRange = 5f;
            Detection.frontAngle = 80f;
            Detection.backRange  = 4f;
            Detection.backAngle  = 40f;
        }
    }

    protected virtual void Start()
    {
        ChangeState(IdleState);
    }

    protected virtual void Update()
    {
        if (IsPaused) return;
        currentState?.Update();
    }

    private void FixedUpdate()
    {
        if (IsPaused) return;
        if (IsAnyQTERunning || (QTEHandler != null && QTEHandler.IsRunning())) return;
        
        currentState?.FixedUpdate();
    }

    public void ChangeState(EnemyState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }
    
    public bool IsPlayerObjectDetectableNow()
    {
        var p = GameManager.Instance != null ? GameManager.Instance.Player : null;
        if (p == null) return false;

        // 최소 조건: 씬에서 활성화되어 있어야 함(숨기/빙의 시 비활성 처리 가정)
        if (!p.activeInHierarchy) return false;

        return true;
    }

    public bool CurrentStateIsPatrol() => currentState == PatrolState;

    public void StartSoundTeleport(Vector3 playerPos, float offsetDistance, float chaseDuration)
    {
        if (soundChaseCoroutine != null)
            return;
        
        soundChaseCoroutine = StartCoroutine(SoundTeleportRoutine(playerPos, offsetDistance, chaseDuration));
    }

    private IEnumerator SoundTeleportRoutine(Vector3 playerPos, float offsetDistance, float chaseDuration)
    {
        isTeleporting = true;
        
        float facing = GameManager.Instance.Player != null
            ? GameManager.Instance.Player.transform.localScale.x
            : 1f;
        Vector3 targetPos = playerPos + new Vector3(facing * offsetDistance, 2f, 0);
        transform.position = targetPos;

        // 텔레포트 연출
        Animator.SetTrigger("SoundTeleport");
        float animLength = Animator.runtimeAnimatorController.animationClips
                                   .FirstOrDefault(c => c.name == "Enemy_SoundTeleport")?.length ?? 1f;
        yield return new WaitForSecondsRealtime(animLength);

        Movement.MarkTeleported();
        isTeleporting = false;

        // 전방위 감지 ON — 이후 이동/수색은 ChaseState가 전부 처리
        soundOmniActive = true;
        ChangeState(ChaseState);

        // 전방위 감지를 chaseDuration 동안만 유지
        float elapsed = 0f;
        while (elapsed < chaseDuration)
        {
            // 여기선 아무것도 하지 않음(상태머신이 추격/수색/복귀를 처리)
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 전방위 감지 OFF 및 정리
        soundOmniActive = false;
        soundChaseCoroutine = null;

        // 복귀 연출(프로젝트 기존 로직 유지)
        isTeleporting = true;
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            yield return sr.DOFade(0f, 1f).WaitForCompletion();
            transform.position = startPosition;
            yield return sr.DOFade(1f, 0.5f).WaitForCompletion();
        }
        else
        {
            transform.position = startPosition;
        }
        isTeleporting = false;

        // 강제타겟 정리 후 Patrol
        Movement.SetForcedTarget(null);
        ChangeState(PatrolState);
    }

    // “감지 가능?” 통합 판정: 전방위 모드면 반경, 아니면 전/후 시야
    public bool IsPlayerDetectable()
    {
        var playerObj = GameManager.Instance != null ? GameManager.Instance.Player : null;
        if (playerObj == null || Detection == null) return false;

        if (!playerObj.activeInHierarchy) return false;
        
        if (soundOmniActive)
            return Detection.IsWithinRadius(playerObj.transform, soundDetectionRange);
        else
            return Detection.IsInVision(playerObj.transform);
    }
    
    /// <summary>오브젝트 활성화 시 호출</summary>
    public void StartInvestigate(Transform target)
    {
        transform.position = target.position;

        // InvestigateState에 목표 위치 전달 (필요 시)
        InvestigateState.Setup(target.position);

        // 상태 변경
        ChangeState(InvestigateState);
    }

    /// <summary>오브젝트 비활성화 시 호출</summary>
    public void StopInvestigate()
    {
        // 맵 경계나 텔레포트 막힌 상태에서 Patrol로 복귀
        ChangeState(PatrolState);
        // (필요하다면) 원위치 복귀 코루틴 호출
        StartCoroutine(ReturnToStart());
    }

    private IEnumerator ReturnToStart()
    {
        transform.position = startPosition;

        // 약간의 대기(선택사항) 후 Patrol 상태로 복귀
        yield return null;

        ChangeState(PatrolState);
    }
}