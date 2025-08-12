using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_MemoryPositive_01_TeddyBear : MemoryFragment
{
    public bool Completed_TeddyBear = false;
    public bool PlayerNearby = false;
    private Collider2D col;

    void Start()
    {
        isScannable = false;
    }

    public void ActivateTeddyBear()
    {
        isScannable = true;
        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetMemoryFragmentScannable(uid.Id, isScannable);
    }

    public override void AfterScan() 
    {
        Completed_TeddyBear = true;
        SaveManager.MarkPuzzleSolved("곰인형");

        base.AfterScan();
    }
    // protected override void PlusAction()
    // {
    //     UIManager.Instance.PromptUI.ShowPrompt_2("맞아 이건 내 기억이야", "여기서 볼 일은 끝난거 같아");    
    // }

    //protected override void OnTriggerEnter2D(Collider2D collision)
    //{

    //    if (collision.CompareTag("Player"))
    //    {
    //        PlayerNearby = true;

    //        if (!ChapterEndingManager.Instance.AllCh1CluesCollected())
    //        {
    //            UIManager.Instance.PromptUI.ShowPrompt("단서가 부족해...");
    //        }
    //        else if (ChapterEndingManager.Instance.AllCh1CluesCollected())
    //        {
    //            PlayerInteractSystem.Instance.AddInteractable(gameObject);
    //        }
    //    }
    //}

    //protected override void OnTriggerExit2D(Collider2D other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        PlayerNearby = false;
    //        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    //    }
    //}
}
