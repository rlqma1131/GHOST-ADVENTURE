using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Ch3_Xray_Monitor : BaseInteractable
{
    [SerializeField] private GameObject monitorZoom; // 확대용 드로잉 UI (Canvas 하위)
    [SerializeField] private RectTransform monitorPos; // 드로잉 UI의 시작 위치
    [SerializeField] private Image zoomPanel;        // 배경 패널 (알파 페이드용)

    [Header("X-ray 기계")]
    [SerializeField] private Ch3_Xray xray;

    private bool isPlayerInside = false;
    private bool isZoomActive = false;

    private bool isFirstFind = false;
    private bool isSecondFind = false;
    public bool IsSecondFind => isSecondFind;

    private int lastFoundIndex = -1;

    void Start()
    {
        // UI 초기화
        monitorZoom.SetActive(false);
        monitorPos.anchoredPosition = new Vector2(0, -Screen.height);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isZoomActive && !isPlayerInside)
                return;

            if (isZoomActive)
            {
                HidePhotoZoom();

                if (!isSecondFind) 
                    UIManager.Instance.PromptUI.ShowPrompt_Random("다른 곳도 촬영해볼까?", "더 촬영해보자");
            }
            else
            {
                ShowPhotoZoom();

                if (isFirstFind)
                {
                    if ((xray.currentPhotoIndex == 1 || xray.currentPhotoIndex == 3))
                        UIManager.Instance.PromptUI.ShowPrompt("어? 문양이 있네? 다른곳도 있을까?");
                }
                else if (isFirstFind && isSecondFind)
                {
                    if ((xray.currentPhotoIndex == 1 || xray.currentPhotoIndex == 3))
                        UIManager.Instance.PromptUI.ShowPrompt("이 문양들은 어떤 의미지?");
                }
            }
        }
    }

    private void ShowPhotoZoom()
    {
        EnemyAI.PauseAllEnemies();
        isZoomActive = true;

        // 패널 페이드 인
        zoomPanel.color = new Color(zoomPanel.color.r, zoomPanel.color.g, zoomPanel.color.b, 0f);
        zoomPanel.DOFade(150f / 255f, 0.5f);

        // 슬라이드 인
        monitorZoom.SetActive(true);
        monitorPos.anchoredPosition = new Vector2(0, -Screen.height);
        monitorPos.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic);

        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);

        // 첫 번째 문양 발견
        if ((xray.currentPhotoIndex == 1 || xray.currentPhotoIndex == 3) && !isFirstFind)
        {
            isFirstFind = true;
            lastFoundIndex = xray.currentPhotoIndex;
        }
        // 두 번째 발견 (첫 번째와 다른 index일 때)
        else if ((xray.currentPhotoIndex == 1 || xray.currentPhotoIndex == 3) && isFirstFind && xray.currentPhotoIndex != lastFoundIndex)
        {
            isSecondFind = true;
            lastFoundIndex = xray.currentPhotoIndex;
        }
    }

    private void HidePhotoZoom()
    {
        EnemyAI.ResumeAllEnemies();
        isZoomActive = false;

        zoomPanel.DOFade(0f, 0.5f);

        monitorPos.DOAnchorPos(new Vector2(0, -Screen.height), 0.5f)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                monitorZoom.SetActive(false);

                if (isPlayerInside)
                    PlayerInteractSystem.Instance.AddInteractable(gameObject);
            });
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
            HidePhotoZoom(); // 플레이어가 나가면 자동 닫기

        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }
}
