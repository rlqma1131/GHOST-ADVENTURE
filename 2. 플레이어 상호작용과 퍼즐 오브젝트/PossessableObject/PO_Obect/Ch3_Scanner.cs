using System.Collections.Generic;
using UnityEngine;

public class Ch3_Scanner : BasePossessable
{
    [SerializeField] private Ch3_MemoryPuzzleUI memoryPuzzleUI;

    private MemoryStorage memoryStorage;
    private List<MemoryData> memories;

    private bool isSolved = false;

    protected override void Start()
    {
        isPossessed = false;
        hasActivated = false;

        memoryStorage = UIManager.Instance.GetComponentInChildren<MemoryStorage>();
    }

    protected override void Update()
    {
        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            EnemyAI.ResumeAllEnemies();
            memoryPuzzleUI.Close();
            UIManager.Instance.PlayModeUI_OpenAll();
            Unpossess();
        }
    }

    public void ActiveScanner()
    {
        hasActivated = true;
        MarkActivatedChanged();
    }

    public void InactiveScanner()
    {
        hasActivated = false;
        MarkActivatedChanged();

        isSolved = true;
        Unpossess();
    }

    public override void OnPossessionEnterComplete() 
    {
        EnemyAI.PauseAllEnemies();
        UIManager.Instance.PlayModeUI_CloseAll();
        // UI 띄우기
        memoryPuzzleUI.StartFlow(MemoryManager.Instance.GetCollectedMemories());
    }

    public override void CantPossess() 
    {
        if (!isSolved)
        {
            UIManager.Instance.PromptUI.ShowPrompt("아직 위에서 해야할 일이 남은 것 같아");
        }
    }
}

