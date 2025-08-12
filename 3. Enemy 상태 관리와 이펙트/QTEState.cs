using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QTEState : EnemyState
{
    public QTEState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.QTEHandler.StartQTE();
    }

    public override void Update()
    {
        
    }
}
