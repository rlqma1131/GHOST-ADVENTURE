using Cinemachine;
using System.Linq;
using UnityEngine;

public class Ch3_Drawers : BasePossessable
{
    [Header("줌 카메라")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera;

    [Header("효과음")]
    [SerializeField] private AudioClip openDrawerSFX;

    [Header("Xray 모니터 && 환자 서류")]
    [SerializeField] private Ch3_Xray_Monitor xrayMonitor;
    [SerializeField] private Ch3_PatientDocumentIndex[] documentIndex;

    private bool isFirstCheck = false;

    protected override void Start()
    {
        isPossessed = false;
        zoomCamera.Priority = 5;
    }

    protected override void Update()
    {
        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            EnemyAI.ResumeAllEnemies();
            zoomCamera.Priority = 5;

            UIManager.Instance.PlayModeUI_OpenAll();

            Unpossess();

            if(xrayMonitor.IsSecondFind && documentIndex.Any(doc => doc.IsChecked))
            {
                UIManager.Instance.PromptUI.ShowPrompt("환자 정보를 더 찾으러 가보자");
            }
        }
    }

    public override void OnPossessionEnterComplete()
    {
        EnemyAI.PauseAllEnemies();
        zoomCamera.Priority = 20;
        SoundManager.Instance.PlaySFX(openDrawerSFX, 1f);
        UIManager.Instance.PlayModeUI_CloseAll();
    }
}
