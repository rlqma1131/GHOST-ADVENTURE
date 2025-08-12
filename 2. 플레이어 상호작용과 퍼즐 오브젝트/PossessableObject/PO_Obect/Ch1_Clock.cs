using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Ch1_Clock : BasePossessable
{
    [SerializeField] private Button initializeBtn;
    [SerializeField] private AudioClip ticktock;
    [SerializeField] private Image zoomPanel;
    [SerializeField] private RectTransform clockPos; // 두트윈 시작 위치
    [SerializeField] private GameObject clockZoom; // 고해상도 시계 UI
    [SerializeField] private Transform hourHand;
    [SerializeField] private Transform minuteHand;
    [SerializeField] private GameObject UI;
    [SerializeField] private Ch1_TV  tvObject;
    
    private bool isControlMode = false;

    private int hour = 0;
    private int minute = 0;

    protected override void Start()
    {
        isPossessed = false;
        hasActivated = false;

        // 확대UI 초기화
        //zoomPanel = GameObject.Find("ZoomPanel").GetComponent<Image>();
        //clockZoom = GameObject.Find("Ch1_ClockZoom");
        //clockPos = clockZoom.GetComponent<RectTransform>();
        //hourHand = GameObject.Find("HourHand").transform;
        //minuteHand = GameObject.Find("MinuteHand").transform;

        // UI 초기화
        clockZoom.SetActive(false);
        UI.SetActive(false);

        // 시곗바늘 위치 초기화
        UpdateHands();

        initializeBtn.onClick.AddListener(OnClickInitialize);
    }

    protected override void Update()
    {
        if (!SaveManager.IsPuzzleSolved("편지")) return;
        if (!SaveManager.IsPuzzleSolved("시계")) hasActivated = true;
        if (!isPossessed) return;
        
        UI.SetActive(true); 
        // UIManager.Instance.Show_A_Key(hourHand.transform.position);
        // UIManager.Instance.Show_D_Key(minuteHand.transform.position);

        // 최초 상호작용
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 조작 종료
            isControlMode = false;
            UI.SetActive(false); 
            HideClockUI();
            Unpossess();
            // UIManager.Instance.Hide_A_Key();
            // UIManager.Instance.Hide_A_Key();
        }
        

        if (!isControlMode) return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            SoundManager.Instance.PlaySFX(ticktock);
            hour = (hour + 1) % 12;
            UpdateHands();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            SoundManager.Instance.PlaySFX(ticktock);
            minute = (minute + 1) % 60;
            UpdateHands();
        }

        if (hour == 8 && minute == 14)
        {
            Debug.Log("정답");
            SaveManager.MarkPuzzleSolved("시계");
            tvObject.ActivateTV();
            isControlMode = false;
            HideClockUI();
            UIManager.Instance.PromptUI.ShowPrompt("어? 저 TV… 무언가 보여줄지도 몰라.");

            hasActivated = false;
            MarkActivatedChanged();

            Unpossess();
            UI.SetActive(false); 
            
        }
    }

    private void UpdateHands()
    {
        if(hourHand != null)
            hourHand.localRotation = Quaternion.Euler(0, 0, -30f * hour);
        if (minuteHand != null)
            minuteHand.localRotation = Quaternion.Euler(0, 0, -6f * minute);
    }

    private void OnClickInitialize()
    {
        if (hourHand != null)
            hourHand.localRotation = Quaternion.Euler(0, 0, 0);
        if (minuteHand != null)
            minuteHand.localRotation = Quaternion.Euler(0, 0, 0);

        hour = 0;
        minute = 0;

        SoundManager.Instance.PlaySFX(ticktock);
    }

    private void ShowClockUI()
    {
        EnemyAI.PauseAllEnemies();
        //뒤에 어둡게 판넬 켜기
        zoomPanel.color = new Color(zoomPanel.color.r, zoomPanel.color.g, zoomPanel.color.b, 0f);
        zoomPanel.DOFade(150f / 255f, 0.5f);

        clockZoom.SetActive(true);
        clockPos.anchoredPosition = new Vector2(0, -Screen.height); // 아래에서 시작
        clockPos.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic);
    }

    private void HideClockUI()
    {
        EnemyAI.ResumeAllEnemies();
        // 판넬 끄기
        zoomPanel.DOFade(0f, 0.5f);
       
        clockPos.DOAnchorPos(new Vector2(0, -Screen.height), 0.5f)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                clockZoom.SetActive(false);
            });
        
        // UIManager.Instance.NoticePopupUI.FadeInAndOut("※목표 : TV 켜기 ");
    }

    public override void OnPossessionEnterComplete()
    {
        isControlMode = true;
        ShowClockUI();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if(collision.CompareTag("Player") && !SaveManager.IsPuzzleSolved("시계"))
        {
            if(SaveManager.IsPuzzleSolved("편지"))
            {
                UIManager.Instance.PromptUI.ShowPrompt("시간을 떠올릴만한 숫자를 본 거 같은데");
                if(hasActivated)
                {
                    UIManager.Instance.NoticePopupUI.FadeInAndOut("※ 파란 빛을 띄는 오브젝트는 E키로 빙의할 수 있습니다.");
                }
            }
            else
            {
                UIManager.Instance.PromptUI.ShowPrompt("시계가 멈춰 있네...");
            }
        }
    }
}
