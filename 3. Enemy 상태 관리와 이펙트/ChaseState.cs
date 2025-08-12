using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : EnemyState
{
    public ChaseState(EnemyAI enemy) : base(enemy) { }

    private const float SearchDuration  = 2.0f;  // 두리번 시간
    private const float ReachThreshold  = 0.2f;  // 마지막 지점 도달 판정

    private Vector3 lastSeenPos;
    private bool    searching = false;
    private float   searchTimer = 0f;

    public override void Enter()
    {
        enemy.Animator.SetBool("IsWalking", true);
        searching   = false;
        searchTimer = 0f;
        enemy.Movement.SetForcedTarget(null); // 실시간 플레이어 추격
    }

    public override void Update()
    {
        if (enemy.IsPlayerDetectable())
        {
            var p = GameManager.Instance.Player;
            if (p != null) lastSeenPos = p.transform.position;

            searching   = false;
            searchTimer = 0f;
            enemy.Movement.SetForcedTarget(null); // 실시간 추격 유지
            return;
        }

        // 여기 오면 "놓침"
        if (!searching)
        {
            searching   = true;
            searchTimer = SearchDuration;

            // 방금 놓친 위치로 이동해 잠깐 수색
            var p = GameManager.Instance.Player;
            lastSeenPos = p != null ? p.transform.position : enemy.transform.position;
            enemy.Movement.SetForcedTarget(lastSeenPos);
            return;
        }

        // 수색 타이머
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f)
        {
            // 수색 종료 → Patrol 복귀
            enemy.Movement.SetForcedTarget(null);
            enemy.ChangeState(enemy.PatrolState);
        }
    }

    public override void FixedUpdate()
    {
        // 강제 타겟이 있으면 그쪽, 없으면 플레이어 방향으로 이동
        enemy.Movement.ChasePlayer();
    }

    public override void Exit()
    {
        enemy.Animator.SetBool("IsWalking", false);
        enemy.Movement.SetForcedTarget(null);
    }
}
