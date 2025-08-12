using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : EnemyState
{
    public PatrolState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.Animator.SetBool("IsWalking", true);
        enemy.Movement.PickRandomDirection();
    }

    public override void FixedUpdate()
    {
        enemy.Movement.PatrolMove();
    }

    public override void Update()
    {
        if (!enemy.IsPlayerObjectDetectableNow())
            return;

        // 실제 시야(앞 5m·80°, 뒤 4m·40°)에 들어온 경우에만 추격으로 전환
        if (enemy.IsPlayerDetectable())
            enemy.ChangeState(enemy.ChaseState);
    }

    public override void Exit()
    {
        enemy.Animator.SetBool("IsWalking", false);
    }
}
