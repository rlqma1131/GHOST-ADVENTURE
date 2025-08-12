using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class IdleState : EnemyState
{
    private float waitTime;

    public IdleState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.Animator.SetBool("IsIdle", true);
        waitTime = Random.Range(1f, 3f);
    }

    public override void Update()
    {
        waitTime -= Time.deltaTime;
        if (waitTime <= 0)
        {
            enemy.ChangeState(enemy.PatrolState);
        }
    }

    public override void Exit()
    {
        enemy.Animator.SetBool("IsIdle", false);
    }
}
