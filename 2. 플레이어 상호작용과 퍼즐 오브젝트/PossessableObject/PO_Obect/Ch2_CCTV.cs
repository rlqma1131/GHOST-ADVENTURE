using UnityEngine;

public class Ch2_CCTV : BasePossessable
{
    [Header("CCTV 번호")]
    [SerializeField] private int index; // 0 ~ 3

    [Header("CCTV 모니터")]
    [SerializeField] private Ch2_CCTVMonitor monitor;

    [Header("조작키")]
    [SerializeField] private GameObject aKey;
    [SerializeField] private GameObject dKey;

    [Header("하이라이트 애니메이터")]
    [SerializeField] private Animator highlightAnimator;

    private bool isRight = false;

    protected override void Start()
    {
        isPossessed = false;
        hasActivated = false;

        anim = GetComponentInChildren<Animator>();
    }

    protected override void Update()
    {
        if (monitor.isRevealed) // 기억조각 드러나면 빙의 불가
        {
            InactiveCCTV();
        }

        if (!isPossessed || !hasActivated)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Unpossess();
            aKey.SetActive(false);
            dKey.SetActive(false);
        }

        // 좌우 움직임에 따라 모니터화면도 움직이기
        else if (Input.GetKeyDown(KeyCode.D))
        {
            anim.SetBool("Right", true);
            isRight = true; 

            monitor?.SetMonitorAnimBool(index, "Right", true);
            CheckSolvedPrompt();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            anim.SetBool("Right", false);
            isRight = false;

            monitor?.SetMonitorAnimBool(index, "Right", false);
            CheckSolvedPrompt();
        }
    }
    
    void CheckSolvedPrompt()
    {
        if (monitor.SolvedCheck())
        {
            UIManager.Instance.PromptUI.ShowPrompt("CCTV 화면을 확인해야봐야겠어", 2f);
        }
    }

    public void ActivateCCTV()
    {
        hasActivated = true;
        MarkActivatedChanged();
    }

    public void InactiveCCTV()
    {
        hasActivated = false;
        MarkActivatedChanged();
    }

    public override void OnPossessionEnterComplete() 
    { 
        aKey.SetActive(true);
        dKey.SetActive(true);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasActivated)
            return;

        if (other.CompareTag("Player"))
        {
            PlayerInteractSystem.Instance.AddInteractable(gameObject);

            Invoke(nameof(PlayHighlightAnim), 0.01f); // 0.01초 후 실행
        }
    }

    private void PlayHighlightAnim()
    {
        if (highlightAnimator != null && highlightAnimator.gameObject.activeInHierarchy)
        {
            highlightAnimator.SetBool("Right", isRight);
            Debug.Log($"CCTV 하이라이트 bool Right : {isRight}");
        }
        else
        {
            Debug.Log($"현재 하이라이터 상태 : {highlightAnimator.gameObject.activeInHierarchy}");
        }
    }

    public override void CantPossess()
    {
        UIManager.Instance.PromptUI.ShowPrompt("전력이 끊겨있는 것 같아", 2f);
    }
}
