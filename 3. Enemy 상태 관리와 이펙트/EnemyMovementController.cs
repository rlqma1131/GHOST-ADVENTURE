using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    private Rigidbody2D rb;
    public float patrolSpeed = 4f;
    public float chaseSpeed = 4.2f;
    private Vector2 moveDir;
    private Transform player;
    private EnemyAI enemy;
    
    public float directionChangeInterval = 5f;
    private float directionChangeTimer = 0f;
    private float startTime;
    public float doorBlockDuration = 20f;
    public float teleportSafetyRadius   = 0.2f;
    
    private float teleportDoorIgnoreTime = 1f;   // 텔레포트 후 문 무시 시간
    private float lastTeleportTime = -10f;       // 초기값 멀리 설정
    
    private Vector2? forcedTarget = null;
    public LayerMask obstacleMask;
    
    [SerializeField] private float stuckMaxTime = 0.25f;      // 이 시간 이상 제자리면 '막힘'
    [SerializeField] private float idleMin = 0.35f;           // 짧은 Idle 최소
    [SerializeField] private float idleMax = 0.60f;           // 짧은 Idle 최대
    private float stuckTimer = 0f;
    private bool briefIdleRunning = false;
    private Vector2 lastPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<EnemyAI>();
        
        var qteTrigger = GetComponentInChildren<EnemyQTETrigger>();
        if (qteTrigger != null)
            qteTrigger.OnDoorEntered += HandleDoorEntered;
    }

    private void Start()
    {
        player = GameManager.Instance.Player.transform;
        // PickRandomDirection();
        startTime = Time.time;
        lastPos = rb.position;
    }

    public void PickRandomDirection()
    {
        float currentAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        float newAngle = Random.Range(currentAngle - 60f, currentAngle + 60f);

        Vector2 dir = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad)).normalized;

        if (Mathf.Abs(dir.x) < 0.3f) dir.x = Mathf.Sign(dir.x) * 0.3f;
        if (Mathf.Abs(dir.y) < 0.3f) dir.y = Mathf.Sign(dir.y) * 0.3f;

        moveDir = dir.normalized;
        directionChangeTimer = directionChangeInterval;
    }

    private void Update()
    {
        directionChangeTimer -= Time.deltaTime;
        if (directionChangeTimer <= 0f)
        {
            PickRandomDirection();
        }
    }
    
    public void SetForcedTarget(Vector2? target)
    {
        forcedTarget = target;
    }

    public void PatrolMove()
    {
        if (enemy.isTeleporting) return;

        Vector2 delta = moveDir * patrolSpeed * Time.fixedDeltaTime;
        Vector2 pos   = rb.position;
        Vector2 newPos = pos;

        bool movedY = false;
        bool movedX = false;

        // 1) 수직(Y)
        if (Mathf.Abs(delta.y) > 0f)
        {
            Vector2 dirY = Vector2.up * Mathf.Sign(delta.y);
            float distY  = Mathf.Abs(delta.y);
            if (!Physics2D.Raycast(pos, dirY, distY, obstacleMask))
            {
                newPos.y += delta.y;
                movedY = true;
            }
            else
            {
                moveDir.y = 0; // 막혔으면 Y 성분 제거
            }
        }

        // 2) 수평(X)
        if (Mathf.Abs(delta.x) > 0f)
        {
            Vector2 dirX = Vector2.right * Mathf.Sign(delta.x);
            float distX  = Mathf.Abs(delta.x);
            if (!Physics2D.Raycast(pos, dirX, distX, obstacleMask))
            {
                newPos.x += delta.x;
                movedX = true;
            }
            else
            {
                moveDir.x = 0; // 막혔으면 X 성분 제거
            }
        }

        // 실제 이동
        rb.MovePosition(newPos);
        UpdateFlip();

        // ── 막힘 감지: 이번 프레임에 거의 못 움직였으면 타이머 증가
        float movedDist = (newPos - pos).sqrMagnitude;
        if (movedDist < 0.0001f || (!movedX && !movedY))
            stuckTimer += Time.fixedDeltaTime;
        else
            stuckTimer = 0f;

        // ── 일정 시간 이상 막혔다 → 즉시 방향 재선택 + (가끔) 짧은 Idle
        if (stuckTimer >= stuckMaxTime)
        {
            stuckTimer = 0f;
            PickRandomDirection();

            // 너무 딱딱 바뀌는 느낌 방지: 짧게 Idle 후 재개(한 번에 하나만)
            if (!briefIdleRunning)
                StartCoroutine(BriefIdleThenPatrol());
        }
    }

    // 새 코루틴 추가: 아주 짧게 Idle 후 다시 Patrol
    private IEnumerator BriefIdleThenPatrol()
    {
        briefIdleRunning = true;
        enemy.ChangeState(enemy.IdleState);
        yield return new WaitForSecondsRealtime(Random.Range(idleMin, idleMax));
        enemy.ChangeState(enemy.PatrolState);
        PickRandomDirection();
        briefIdleRunning = false;
    }

    public void ChasePlayer()
    {
        if (enemy.isTeleporting) return;

        // 1) 강제 타겟 우선
        if (forcedTarget.HasValue)
        {
            Vector2 dir = (forcedTarget.Value - rb.position).normalized;
            rb.MovePosition(rb.position + dir * chaseSpeed * Time.fixedDeltaTime);
            moveDir = dir;
            UpdateFlip();
            return;
        }

        // 2) 플레이어 쫓기 (없으면 늦게 캐싱)
        if (player == null)
        {
            var pObj = GameManager.Instance != null ? GameManager.Instance.Player : null;
            if (pObj != null) player = pObj.transform;
        }
        if (player == null) return;

        Vector2 toPlayer = ((Vector2)player.position - rb.position).normalized;
        rb.MovePosition(rb.position + toPlayer * chaseSpeed * Time.fixedDeltaTime);
        moveDir = toPlayer;
        UpdateFlip();
    }

    private void UpdateFlip()
    {
        if (moveDir.x > 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveDir.x < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponentInChildren<EnemyVolumeTrigger>() != null)
            return;

        Vector2 normal = collision.contacts[0].normal;
        moveDir = Vector2.Reflect(moveDir, normal).normalized;
    }

    private void OnDestroy()
    {
        var qteTrigger = GetComponentInChildren<EnemyQTETrigger>();
        if (qteTrigger != null)
            qteTrigger.OnDoorEntered -= HandleDoorEntered;
    }

    private void HandleDoorEntered(BaseDoor door)
    {
        // 1) Patrol 중이 아니면 무시
        if (!enemy.CurrentStateIsPatrol())
            return;

        // 2) 순간이동 직후 무시
        if (Time.time - lastTeleportTime < teleportDoorIgnoreTime)
            return;

        // 3) 스폰 직후 짧게 막기
        if (Time.time - startTime < doorBlockDuration)
        {
            PickRandomDirection();
            return;
        }

        // 4) 잠긴 문은 건너뛸 수 있으니 패스
        if (door.IsLocked)
        {
            PickRandomDirection();
            return;
        }

        // 5) 순간이동 확률(테스트시 1f로 두세요)
        if (Random.value < 0.4f) 
        {
            // a) 도착 지점 장애물 검사
            Vector3 dest = door.GetTargetDoor()?.position ?? (Vector3)door.GetTargetPos();
            if (Physics2D.OverlapCircle(dest, teleportSafetyRadius, obstacleMask))
            {
                PickRandomDirection();
                return;
            }
            // b) 순간이동 실행
            TeleportThroughDoor(door);
        }
        else
        {
            PickRandomDirection();
        }
    }
    
    private IEnumerator RestoreCollision(int pLayer, int eLayer) {
        yield return new WaitForSecondsRealtime(teleportDoorIgnoreTime);
        Physics2D.IgnoreLayerCollision(pLayer, eLayer, false);
    }

    private void TeleportThroughDoor(BaseDoor door) {
        // 1) 충돌 무시 시작
        int pLayer = GameManager.Instance.Player.layer;
        int eLayer = gameObject.layer;
        Physics2D.IgnoreLayerCollision(pLayer, eLayer, true);

        // 2) 순간이동
        Transform td = door.GetTargetDoor();
        Vector3 dest = td != null ? td.position : (Vector3)door.GetTargetPos();
        transform.position = dest;

        // 3) 타이밍 재설정
        lastTeleportTime = Time.time;

        // 4) 1초 뒤 충돌 복원
        StartCoroutine(RestoreCollision(pLayer, eLayer));

        // 5) 상태 전환
        enemy.ChangeState(enemy.IdleState);
        StartCoroutine(ResumePatrolAfterDelay());
    }

    private IEnumerator ResumePatrolAfterDelay()
    {
        yield return new WaitForSecondsRealtime(1f);
        enemy.ChangeState(enemy.PatrolState);
    }
    
    public void MarkTeleported()
    {
        lastTeleportTime = Time.time;
    }
}
