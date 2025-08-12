using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Ch3_BedNametag : BaseInteractable
{
    [SerializeField] private GameObject nametagZoom; // 확대용 드로잉 UI (Canvas 하위)
    [SerializeField] private RectTransform nametagPos; // 드로잉 UI의 시작 위치
    [SerializeField] private Image zoomPanel;        // 배경 패널 (알파 페이드용)

    private bool isPlayerInside = false;
    private bool isZoomActive = false;

    void Start()
    {
        // UI 초기화
        nametagZoom.SetActive(false);
        nametagPos.anchoredPosition = new Vector2(0, -Screen.height);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isZoomActive && !isPlayerInside)
                return;

            if (isZoomActive)
            {
                HideNametagZoom();
            }
            else
            {
                ShowNametagZoom();
            }
        }
    }

    private void ShowNametagZoom()
    {
        EnemyAI.PauseAllEnemies();
        isZoomActive = true;

        // 패널 페이드 인
        zoomPanel.color = new Color(zoomPanel.color.r, zoomPanel.color.g, zoomPanel.color.b, 0f);
        zoomPanel.DOFade(150f / 255f, 0.5f);

        // 슬라이드 인
        nametagZoom.SetActive(true);
        nametagPos.anchoredPosition = new Vector2(0, -Screen.height);
        nametagPos.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic);

        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }

    private void HideNametagZoom()
    {
        EnemyAI.ResumeAllEnemies();
        isZoomActive = false;

        zoomPanel.DOFade(0f, 0.5f);

        nametagPos.DOAnchorPos(new Vector2(0, -Screen.height), 0.5f)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                nametagZoom.SetActive(false);

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
            HideNametagZoom(); // 플레이어가 나가면 자동 닫기

        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }
}
