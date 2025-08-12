using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Ch1_Drawing : BaseInteractable
{
    [SerializeField] private GameObject drawingZoom; // 확대용 드로잉 UI (Canvas 하위)
    [SerializeField] private RectTransform drawingPos; // 드로잉 UI의 시작 위치
    [SerializeField] private Image zoomPanel;        // 배경 패널 (알파 페이드용)

    [Header("은신 퍼즐 오브젝트들")]
    [SerializeField] private Ch2_HideAreaPuzzleObj[] PuzzleObj; // 은신처 오브젝트들

    private CluePickup cluePickup;

    private bool isPlayerInside = false;
    private bool isZoomActive = false;
    private bool zoomActivatedOnce = false;

    void Start()
    {
        cluePickup = GetComponent<CluePickup>();

        // UI 초기화
        drawingZoom.SetActive(false);
        drawingPos.anchoredPosition = new Vector2(0, -Screen.height);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isZoomActive && !isPlayerInside)
                return;

            if (isZoomActive)
            {
                HideDrawingZoom();
            }
            else
            {
                ShowDrawingZoom();
            }
        }
    }

    private void ShowDrawingZoom()
    {
        EnemyAI.PauseAllEnemies();
        isZoomActive = true;

        // 패널 페이드 인
        zoomPanel.color = new Color(zoomPanel.color.r, zoomPanel.color.g, zoomPanel.color.b, 0f);
        zoomPanel.DOFade(150f / 255f, 0.5f);

        // 슬라이드 인
        drawingZoom.SetActive(true);
        drawingPos.anchoredPosition = new Vector2(0, -Screen.height);
        drawingPos.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic);

        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }

    private void HideDrawingZoom()
    {
        EnemyAI.ResumeAllEnemies();
        isZoomActive = false;

        zoomPanel.DOFade(0f, 0.5f);

        drawingPos.DOAnchorPos(new Vector2(0, -Screen.height), 0.5f)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                drawingZoom.SetActive(false);

                if (!zoomActivatedOnce)
                {
                    Debug.Log("은신처 퍼즐 시작");
                    foreach(var hideArea in PuzzleObj)
                    {
                        hideArea.HideAreaPuzzleActivate();
                    }
                    zoomActivatedOnce = true;
                }

                if (isPlayerInside)
                    PlayerInteractSystem.Instance.AddInteractable(gameObject);
            });
        
        cluePickup.PickupClue();
        UIManager.Instance.PromptUI.ShowPrompt("숨바꼭질? 순서가 중요해보이는데", 3f);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = true;

        if (!isZoomActive)
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
    }

    protected override void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;

        if (isZoomActive)
            HideDrawingZoom(); // 플레이어가 나가면 자동 닫기

        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }
}
