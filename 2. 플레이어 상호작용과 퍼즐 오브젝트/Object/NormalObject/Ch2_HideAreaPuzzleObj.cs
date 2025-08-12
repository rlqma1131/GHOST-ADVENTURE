using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_HideAreaPuzzleObj : HideArea
{
    public string areaID;

    protected override void Start()
    {
        isPossessed = false;
        hasActivated = false;
    }

    public void HideAreaPuzzleActivate()
    {
        hasActivated = true;
        MarkActivatedChanged();
    }

    public void HideAreaPuzzleDeactivate() 
    {
        hasActivated = false;
        MarkActivatedChanged();
    }

    public override void OnPossessionEnterComplete() 
    {
        // 챕터1 아이방 퍼즐에 순서 등록
        Ch1_HideAreaEventManager.Instance.RegisterArea(areaID);
    }
}
