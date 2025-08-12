using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC01Possessable : MoveBasePossessable
{
    protected override void Update()
    {
        base.Update();

        // if(상호작용 인풋)
        //     OnActivate();

    }
    public void OnActivate()
    {
        // 고유 기능 로직
    }
}
