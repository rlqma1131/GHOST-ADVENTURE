using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_LeftSwing : BasePossessable
{
    [SerializeField] private AudioClip swingSFX;
    [SerializeField] private Ch2_Kiosk targetKiosk;
    // [SerializeField] private Ch2_Computer targetComputer;
    [SerializeField] private GameObject q_Key;
    private int qteSuccessCount = 0;
    private int totalQTECount = 3;
    private bool isQTESequenceRunning = false;
    //private QTEUI qteUI;
    private bool isShowPrompt = false;

    protected override void Start()
    {
        base.Start();
        // qteUI = FindObjectOfType<QTEUI>();
    }
    
    protected override void Update()
    {
        if (!isPossessed || isQTESequenceRunning)
        {
            q_Key.SetActive(false);
            return;
        }
        
        if(isPossessed)
            if(!isShowPrompt)
            {
                UIManager.Instance.PromptUI.ShowPrompt("흔들어볼까?",2f);
                isShowPrompt = true;
            }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            q_Key.SetActive(false);
            isQTESequenceRunning = true;
            qteSuccessCount = 0;
            StartNextQTE();
        }
        
        if (!isQTESequenceRunning && Input.GetKeyDown(KeyCode.E))
        {
            
            Unpossess();
        }
        
        q_Key.SetActive(true);
    }
    
    private void StartNextQTE()
    {
        Time.timeScale = 0.3f;
        UIManager.Instance.QTE_UI.ShowQTEUI(OnQTEResult);
    }

    private void OnQTEResult(bool success)
    {
        Time.timeScale = 1f;

        if (!success)
        {
            isQTESequenceRunning = false;
            isShowPrompt = false;
            Unpossess();
            return;
        }

        qteSuccessCount++;
        Debug.Log($"QTE 성공 {qteSuccessCount}/{totalQTECount}");

        if (qteSuccessCount >= totalQTECount)
        {
            anim.SetTrigger("LeftSwing");
            SoundManager.Instance.PlaySFX(swingSFX);
            // 컴퓨터, 키오스크 hasActivated = true 포함된 매서드 추가
            if (targetKiosk != null)
                targetKiosk.Activate();
            // if(targetComputer != null)
            //     targetComputer.Activate();
            
            Unpossess();
            UIManager.Instance.PromptUI.ShowPrompt_Random("이 그네가…발전기…?", "어디선가... 전원이 켜지는 소리가 났어");
            isQTESequenceRunning = false;

            hasActivated = false;
            MarkActivatedChanged();
        }
        else
        {
            StartCoroutine(StartNextQTEDelayed());
        }
    }
    
    private IEnumerator StartNextQTEDelayed()
    {
        yield return null; // 1프레임 대기 (또는 WaitForSecondsRealtime(0.1f))
        StartNextQTE();
    }
}
