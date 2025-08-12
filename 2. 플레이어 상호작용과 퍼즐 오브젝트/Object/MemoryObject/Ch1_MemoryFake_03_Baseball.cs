using UnityEngine;

public class Ch1_MemoryFake_03_Baseball : MemoryFragment
{
    [SerializeField] private GameObject alphabet_A;

    void Start()
    {
        isScannable = true;
        alphabet_A.SetActive(false);
    }

    public override void AfterScan()
    {
        alphabet_A.SetActive(true);
        ChapterEndingManager.Instance.CollectCh1Clue("A");
        SaveManager.MarkPuzzleSolved("A");
        UIManager.Instance.PromptUI.ShowPrompt("A");

        base.AfterScan();
    }

    protected override void PlusAction()
    {
        
    }
}
