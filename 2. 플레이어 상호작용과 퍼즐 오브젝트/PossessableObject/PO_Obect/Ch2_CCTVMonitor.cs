using Cinemachine;
using System.Collections;
using UnityEngine;

public class Ch2_CCTVMonitor : BasePossessable
{
    [Header("줌 카메라")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera;

    [Header("CCTV 화면 순서대로")]
    [SerializeField] private GameObject[] cctvScreens; // 기본 모니터 화면
    [SerializeField] private GameObject[] laserScreens; // 레이저 모니터 화면

    [Header("가짜 기억 02")]
    [SerializeField] private GameObject memoryH;
    [SerializeField] private GameObject clueH;
    [SerializeField] private AudioClip reveal;

    private SpriteRenderer[] cctvScreenSpriteRenderer;
    private SpriteRenderer[] laserScreenSpriteRenderer;

    private Animator[] cctvScreenAnimators;

    private bool isActivatedFirst = true;
    private bool firstZoom = true; // 처음 줌 카메라 활성화 여부
    public bool isRevealed { get; private set; } = false; // 기억조각 처음 한번만 나타내기
    private bool isRevealStarted = false;

    protected override void Start()
    {
        isPossessed = false;
        hasActivated = false;
        zoomCamera.Priority = 5;

        cctvScreenSpriteRenderer = new SpriteRenderer[cctvScreens.Length];
        laserScreenSpriteRenderer = new SpriteRenderer[laserScreens.Length];
        cctvScreenAnimators = new Animator[cctvScreens.Length];

        for (int i = 0; i < cctvScreens.Length; i++)
        {
            if (cctvScreens[i] != null)
            {
                cctvScreenSpriteRenderer[i] = cctvScreens[i].GetComponent<SpriteRenderer>();
                cctvScreenAnimators[i] = cctvScreens[i].GetComponent<Animator>();
            }

            if (laserScreens[i] != null)
            {
                laserScreenSpriteRenderer[i] = laserScreens[i].GetComponent<SpriteRenderer>();
            }
        }

        for (int i = 0; i < laserScreens.Length; i++)
        {
            if (laserScreenSpriteRenderer[i] != null && cctvScreenSpriteRenderer[i] != null)
            {
                cctvScreenSpriteRenderer[i].enabled = true;
                laserScreenSpriteRenderer[i].enabled = false;
            }
        }

        Debug.Log(
        $"CCTVMonitor : " +
        $"{cctvScreenAnimators[0].GetBool("Right")}, " +
        $"{cctvScreenAnimators[1].GetBool("Right")}, " +
        $"{cctvScreenAnimators[2].GetBool("Right")}, " +
        $"{cctvScreenAnimators[3].GetBool("Right")}"
        );
    }

    protected override void Update()
    {
        if (!isPossessed || isRevealStarted)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            EnemyAI.ResumeAllEnemies();
            zoomCamera.Priority = 5;

            UIManager.Instance.PlayModeUI_OpenAll();

            Unpossess();

            // 기본 모니터 화면으로 전환
            for (int i = 0; i < laserScreens.Length; i++)
            {
                if (laserScreenSpriteRenderer[i] != null && cctvScreenSpriteRenderer[i] != null)
                {
                    cctvScreenSpriteRenderer[i].enabled = true;
                    laserScreenSpriteRenderer[i].enabled = false;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            // 레이저 모니터 화면으로 전환
            for (int i = 0; i < laserScreens.Length; i++)
            {
                if (laserScreenSpriteRenderer[i] != null && cctvScreenSpriteRenderer[i] != null)
                {
                    cctvScreenSpriteRenderer[i].enabled = false;
                    laserScreenSpriteRenderer[i].enabled = true;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            // 기본 모니터 화면으로 전환
            for (int i = 0; i < laserScreens.Length; i++)
            {
                if (laserScreens[i] != null)
                {
                    laserScreenSpriteRenderer[i].enabled = false;
                    cctvScreenSpriteRenderer[i].enabled = true;
                }
            }
        }
    }

    // index번 모니터 화면과 연결된 CCTV 설정
    public void SetMonitorAnimBool(int idx, string param, bool value)
    {
        if (idx >= 0 && idx < cctvScreens.Length && cctvScreens[idx] != null)
        {
            cctvScreenAnimators[idx]?.SetBool(param, value);
        }
    }

    public void CheckMemoryUnlockCondition()
    {
        bool[] expected = { true, false, false, true };

        for (int i = 0; i < cctvScreens.Length && i < expected.Length; i++)
        {
            if (cctvScreens[i] == null)
                return;

            bool current = cctvScreenAnimators[i].GetBool("Right");
            if (current != expected[i])
                return; // 하나라도 다르면 조기 종료
        }

        // 전부 조건 만족 시 기억조각 나타남
        // 추후 효과 수정
        isRevealed = true;
        isRevealStarted = true; // 기억 조각 나타나는 동안 조작 불가능
        StartCoroutine(MemoryReveal());
    }

    private IEnumerator MemoryReveal()
    {
        yield return new WaitForSeconds(3f);

        // 효과 재생
        foreach (var animator in cctvScreenAnimators)
        {
            if (animator != null)
            {
                animator.SetTrigger("Reveal");
            }
        }

        yield return new WaitForSeconds(4f);

        memoryH.SetActive(true);
        clueH.SetActive(true);
        SoundManager.Instance.PlaySFX(reveal);
        ChapterEndingManager.Instance.CollectCh2Clue("H");

        hasActivated = false; // 기억 스캔 전까지 빙의 불가
        MarkActivatedChanged();

        EnemyAI.ResumeAllEnemies();
        zoomCamera.Priority = 5;

        UIManager.Instance.PlayModeUI_OpenAll();

        isRevealStarted = false; // 조작 가능 상태로 복귀
        Unpossess();

        Invoke(nameof(RevealPrompt), 2.5f); // 문양 스캔 프롬프트 표시
    }

    public override void OnPossessionEnterComplete()
    {
        EnemyAI.PauseAllEnemies();
        zoomCamera.Priority = 20;

        UIManager.Instance.PlayModeUI_CloseAll();

        if (firstZoom && !isRevealed)
        {
            firstZoom = false;
            UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode("CCTV 화면이다. 카메라를 조작할 수 있을까?", 2f);
        }

        if (!isRevealed)
            CheckMemoryUnlockCondition();
    }

    public void ActivateCCTVMonitor()
    {
        hasActivated = true;
        MarkActivatedChanged();

        // 처음 활성화 됐을 때 프롬프트
        if(isActivatedFirst)
        {
            isActivatedFirst = false;
            Invoke(nameof(ActivateFirst), 2.3f);
        }
    }

    public bool SolvedCheck()
    {
        bool[] expected = { true, false, false, true };
        bool solved = true;

        for (int i = 0; i < cctvScreenAnimators.Length && i < expected.Length; i++)
        {
            bool cur = cctvScreenAnimators[i].GetBool("Right");
            Debug.Log($"CCTVMonitor Screen[{i}] Right = {cur}");
            if (cur != expected[i]) solved = false;
        }

        return solved;
    }

    void ActivateFirst()
    {
        UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode("관리실 안 CCTV 화면도 켜진 것 같아. 카메라를 조작할 수 있을까?", 2f);
    }

    void RevealPrompt()
    {
        UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode("이 문양, 뭔가 의미가 있는 것 같아. 스캔해보자.", 2f);
    }
}
