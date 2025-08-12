using Cinemachine;
using UnityEngine;
using DG.Tweening;

public class Ch3_Console : BaseInteractable
{
    [SerializeField] private GameObject qKey;
    [SerializeField] private ItemData cardKey;
    [SerializeField] private AudioClip correctSFX; // 하이라이트 오브젝트

    [Header("줌 화면")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera; // 줌 카메라
    [SerializeField] private GameObject zoomPos; // 줌 효과음
    [SerializeField] private GameObject paper;

    [Header("퍼즐 버튼")]
    [SerializeField] private Ch3_ConsoleButton[] buttons;
    [SerializeField] private GameObject correctBtn; // 버튼 하이라이트

    [Header("기억 조각")]
    [SerializeField] private GameObject memoryFragment;

    [Header("간호사")]
    [SerializeField] private Ch3_Nurse nurse;

    Inventory_PossessableObject inventory; // 빙의 인벤토리(Item을 갖고 있는지 확인용)

    private bool canUse = false;
    private bool isZoomed = false;
    private bool isFirstZoom = true;

    private Ch3_ConsoleButton currentActiveButton;
    private Ch3_MemoryNegative_02_Paper memoryPaper;

    void Start()
    {
        memoryPaper = memoryFragment?.GetComponent<Ch3_MemoryNegative_02_Paper>();
        memoryFragment.SetActive(false);
        
        // 처음에는 종이를 위에 올려둠
        if (paper != null)
        {
            Vector3 startPos = paper.transform.localPosition;
            paper.transform.localPosition = new Vector3(startPos.x, 0.1f, startPos.z);
            paper.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!canUse)
                return;

            inventory = Inventory_PossessableObject.Instance;

            if (inventory == null || cardKey != inventory.selectedItem()
                || !PossessionStateManager.Instance.IsPossessing())
            {
                UIManager.Instance.PromptUI.ShowPrompt("조작하려면 카드키가 필요해보여.. 방법이 없을까?", 2f);
                return;
            }

            if (cardKey == inventory.selectedItem() && cardKey != null && !isZoomed)
            {
                EnemyAI.PauseAllEnemies();
                UIManager.Instance.PlayModeUI_CloseAll();
                qKey.SetActive(false);
                zoomCamera.Priority = 20;
                isZoomed = true;

                if (isFirstZoom)
                {
                    isFirstZoom = false;
                    UIManager.Instance.PromptUI.ShowPrompt("알맞은 정보를 입력해야 해");
                }
                return;
            }
            else if (cardKey == inventory.selectedItem() && cardKey != null && isZoomed)
            {
                EnemyAI.ResumeAllEnemies();
                UIManager.Instance.PlayModeUI_OpenAll();
                qKey.SetActive(true);
                zoomCamera.Priority = 5;
                isZoomed = false;
                return;
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.E) && isZoomed)
        {
            ExitZoom();
        }
    }

    private void ExitZoom()
    {
        EnemyAI.ResumeAllEnemies();
        UIManager.Instance.PlayModeUI_OpenAll();
        zoomCamera.Priority = 5;
        isZoomed = false;
        qKey.SetActive(true);

        foreach (var button in buttons)
        {
            button.HideQuestion();
        }
        currentActiveButton = null;
    }

    public void CheckAllAnswers()
    {
        foreach (var btn in buttons)
        {
            if (!btn.IsCorrectlyAnswered())
                return;
        }

        SoundManager.Instance.PlaySFX(correctSFX);

        // 버튼 비활성화
        foreach (var btn in buttons)
        {
            btn.canClick = false;
            btn.HideQuestion();
        }

        correctBtn.SetActive(true);
        memoryFragment.SetActive(true);
        memoryPaper.ActivatePaper();

        if (paper != null && zoomPos != null)
        {
            paper.SetActive(true);
            paper.transform.DOKill();

            // 종이 초기 위치
            paper.transform.localPosition = new Vector3(
                paper.transform.localPosition.x,
                0.1f,
                paper.transform.localPosition.z);

            Sequence seq = DOTween.Sequence();

            // 카메라 내려주기
            Vector3 pos = zoomPos.transform.localPosition;
            Vector3 target = new Vector3(pos.x, -0.16f, pos.z);
            seq.Append(zoomPos.transform.DOLocalMoveY(target.y, 0.5f).SetEase(Ease.InOutQuad));

            seq.AppendInterval(0.2f); // 멈칫

            // 종이 출력 애니메이션
            float[] steps = { -0.4f, -0.8f, -1.2f, -1.6f, -2.0f };
            float stepTime = 0.25f;

            foreach (float y in steps)
            {
                seq.Append(paper.transform.DOLocalMoveY(y, stepTime).SetEase(Ease.OutQuad));
                seq.AppendInterval(0.1f);
            }

            seq.AppendInterval(2f);

            seq.AppendCallback(() =>
            {
                ExitZoom();

                nurse.InactiveNurse();
                nurse.Unpossess();
                UIManager.Instance.PromptUI.ShowPrompt("관찰기록지? 스캔해봐야겠어", 3f);
            });
        }
    }

    public void OnButtonClicked(Ch3_ConsoleButton clickedButton)
    {
        if (currentActiveButton == clickedButton)
        {
            // 같은 버튼 다시 누름
            clickedButton.HideQuestion();
            currentActiveButton = null;
        }
        else
        {
            // 다른 버튼 누름
            if (currentActiveButton != null)
                currentActiveButton.HideQuestion();

            clickedButton.ShowQuestion();
            currentActiveButton = clickedButton;
        }
    }

    public void ResetCurrentButton(Ch3_ConsoleButton button)
    {
        if (currentActiveButton == button)
            currentActiveButton = null;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Person") || collision.CompareTag("Player"))
        {
            SetHighlight(true);
            qKey.SetActive(true);
            canUse = true;
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Person") || collision.CompareTag("Player"))
        {
            SetHighlight(false);
            qKey.SetActive(false);
            canUse = false;
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
        }
    }
}
