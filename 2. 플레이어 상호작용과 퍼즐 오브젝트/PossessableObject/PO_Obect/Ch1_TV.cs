using Cinemachine;
using System.Collections;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class Ch1_TV : BasePossessable
{
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    [SerializeField] private Animator zoomAnim;
    [SerializeField] private GameObject memoryObject;
    [SerializeField] private GameObject show;
    [SerializeField] private TextMeshProUGUI channelTxt;
    [SerializeField] private LockedDoor Door;
    [SerializeField] private GameObject spaceBar;
    [SerializeField] private GameObject UI;
    [SerializeField] private AudioClip glitchSound;
    private Ch1_MemoryFake_01_BirthdayHat birthdayHat;

    private Collider2D col;
    private bool isControlMode = false;
    private int channel = 1;

    protected override void Start()
    {
        show.SetActive(false);
        birthdayHat = memoryObject.GetComponent<Ch1_MemoryFake_01_BirthdayHat>();
        memoryObject.SetActive(false);

        hasActivated = false;

        col = GetComponent<Collider2D>();
        spaceBar = UIManager.Instance.spacebar_Key;
        UI.SetActive(false);
    }

    public void ActivateTV()
    {
        if (hasActivated) return;

        hasActivated = true;
        MarkActivatedChanged();

        if (anim != null)
            anim.SetTrigger("On");
    }

    protected override void Update()
    {
        if (!isPossessed || !hasActivated)
        {
            UI.SetActive(false);
            spaceBar.SetActive(false);
            return;
        }
        UI.SetActive(true);
        spaceBar.SetActive(true);

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isControlMode)
            {
                // 조작 종료
                isControlMode = false;
                isPossessed = false;
                UIManager.Instance.PlayModeUI_OpenAll();
                //SoundManager.Instance.ChangeBGM(1챕터BGM);

                spaceBar.SetActive(false);
                UI.SetActive(false);

                EnemyAI.ResumeAllEnemies();
                zoomCamera.Priority = 5;
                Unpossess();

            }
        }
        
        // 채널 변경
        if (Input.GetKeyDown(KeyCode.W)) 
        { 
            channel = Mathf.Clamp(channel + 1, 1, 9); 
            UpdateChannelDisplay();
            // 채널 변경 효과음 추가
            zoomAnim.SetTrigger("Change");
        }
        if (Input.GetKeyDown(KeyCode.S)) 
        { 
            channel = Mathf.Clamp(channel - 1, 1, 9); 
            UpdateChannelDisplay();
            // 채널 변경 효과음 추가
            zoomAnim.SetTrigger("Change");
        }

        // 확정은 Space
        if (channel == 9 && Input.GetKeyDown(KeyCode.Space))
        {
            // 정답 효과음 추가
            spaceBar.SetActive(false);
            UI.SetActive(false);

            ShowMemoryandDoorOpen();

            hasActivated = false;
            MarkActivatedChanged();

            col.enabled = false;
            SaveManager.MarkPuzzleSolved("티비");

        }
    }

    private void UpdateChannelDisplay()
    {
        if(channelTxt != null)
            channelTxt.text = ($"0{channel}");
    }

    private void ShowMemoryandDoorOpen()
    {
        // 1. TV 줌 애니메이션 재생
        show.SetActive(true);
        anim.SetTrigger("Solved");
        zoomAnim.SetTrigger("Show");
        StartCoroutine(WaitZoomEnding(3f));
    }

    private IEnumerator WaitZoomEnding(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 2. 기억조각 보이기
        if (memoryObject != null)
        {
            memoryObject.SetActive(true);
            birthdayHat.ActivateHat();
            anim.SetTrigger("Solved");
        }

        // 3. 문 열기
        Door.SolvePuzzle();

        // 4. 빙의 해제
        isPossessed = false;
        isControlMode = false;

        UIManager.Instance.PlayModeUI_OpenAll();

        // BGM 복구
        SoundManager.Instance.FadeOutAndStopLoopingSFX(1f);
        SoundManager.Instance.RestoreLastBGM(1f);

        spaceBar.SetActive(false);
        UI.SetActive(false);

        EnemyAI.ResumeAllEnemies();
        zoomCamera.Priority = 5;
        Unpossess();

        UIManager.Instance.NoticePopupUI.FadeInAndOut("※ 노란 빛을 띄는 오브젝트는 E키를 3초간 눌러 스캔할 수 있습니다.");

    }

    public override void OnPossessionEnterComplete() 
    {
        EnemyAI.PauseAllEnemies();

        UIManager.Instance.PlayModeUI_CloseAll();

        zoomCamera.Priority = 20; // 빙의 시 카메라 우선순위 높이기
        isControlMode = true;
        isPossessed = true;

        channelTxt.text = "01"; // 초기 채널 표시
        UpdateChannelDisplay();

        SoundManager.Instance.FadeOutAndStopBGM(1f); // BGM 페이드 아웃
        SoundManager.Instance.FadeInLoopingSFX(glitchSound, 1f, 0.5f);

        UIManager.Instance.PromptUI.ShowPrompt("힌트를 살펴보자", 2.5f);
    }

}
