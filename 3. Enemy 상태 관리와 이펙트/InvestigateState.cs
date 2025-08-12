using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigateState : EnemyState
{
    private Vector3 targetPosition;

    public InvestigateState(EnemyAI enemy) : base(enemy) { }

    public void Setup(Vector3 position)
    {
        targetPosition = position;
    }

    public override void Enter()
    {
        enemy.Animator.SetBool("IsWalking", true);
        enemy.Movement.SetForcedTarget(targetPosition);
    }

    public override void FixedUpdate()
    {
        enemy.Movement.PatrolMove();
    }

    public override void Exit()
    {
        enemy.Animator.SetBool("IsWalking", false);
        enemy.Movement.SetForcedTarget(null);
    }
}
