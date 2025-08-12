using Cinemachine;
using System.Collections;
using UnityEngine;

public class Ch2_Switchboard : BasePossessable
{
    [SerializeField] private AudioClip electricSFX;

    [Header("줌 카메라")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera;

    [Header("CCTV")]
    [SerializeField] private Ch2_CCTVMonitor cctvMonitor;
    [SerializeField] private Ch2_CCTV[] cams;

    private Ch2_SwitchboardPuzzleManager puzzleManager;
    private bool isSolved = false;

    protected override void Start()
    {
        base.Start();

        zoomCamera.Priority = 5;

        puzzleManager = GetComponent<Ch2_SwitchboardPuzzleManager>();
    }

    protected override void Update()
    {
        if (!isPossessed)
            return;

        if (isSolved)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            EnemyAI.ResumeAllEnemies();
            UIManager.Instance.PlayModeUI_OpenAll();
            zoomCamera.Priority = 5;
            Unpossess();
        }
    }

    public override void OnPossessionEnterComplete()
    {
        EnemyAI.PauseAllEnemies();
        zoomCamera.Priority = 20;

        UIManager.Instance.PlayModeUI_CloseAll();
        puzzleManager.EnablePuzzleControl();
    }

    public void SolvedPuzzle()
    {
        isSolved = true;
        SoundManager.Instance.PlaySFX(electricSFX);
        UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode("CCTV 전력이 연결됐어!", 1f);
        StartCoroutine(SolvedPuzzleRoutine());
    }

    private IEnumerator SolvedPuzzleRoutine()
    {
        yield return new WaitForSeconds(2f);
        zoomCamera.Priority = 5;
        Unpossess();

        UIManager.Instance.PlayModeUI_OpenAll();

        hasActivated = false;
        MarkActivatedChanged();

        // CCTV, 모니터 빙의 가능
        foreach (var cctv in cams)
        {
            cctv.ActivateCCTV();
        }
        cctvMonitor.ActivateCCTVMonitor();
    }
}
