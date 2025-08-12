using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Ch3_VirusMemoClue : MonoBehaviour
{
    [SerializeField] private GameObject virusMemoZoom; // 확대용 드로잉 UI (Canvas 하위)
    [SerializeField] private RectTransform virusMemoPos; // 드로잉 UI의 시작 위치
    [SerializeField] private Image zoomPanel;        // 배경 패널 (알파 페이드용)
    private CluePickup cluePickup;

    private bool isPlayerInside = false;
    private bool isZoomActive = false;

    void Start()
    {
        cluePickup = GetComponent<CluePickup>();

        // UI 초기화
        virusMemoZoom.SetActive(false);
        virusMemoPos.anchoredPosition = new Vector2(0, -Screen.height);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isZoomActive && !isPlayerInside)
                return;

            if (isZoomActive)
            {
                HideVirusMemoZoom();
            }
            else
            {
                ShowVirusMemoZoom();
            }
        }
    }

    private void ShowVirusMemoZoom()
    {
        EnemyAI.PauseAllEnemies();
        isZoomActive = true;

        // 패널 페이드 인
        zoomPanel.color = new Color(zoomPanel.color.r, zoomPanel.color.g, zoomPanel.color.b, 0f);
        zoomPanel.DOFade(150f / 255f, 0.5f);

        // 슬라이드 인
        virusMemoZoom.SetActive(true);
        virusMemoPos.anchoredPosition = new Vector2(0, -Screen.height);
        virusMemoPos.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic);

        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }

    private void HideVirusMemoZoom()
    {
        EnemyAI.ResumeAllEnemies();
        isZoomActive = false;

        zoomPanel.DOFade(0f, 0.5f);

        virusMemoPos.DOAnchorPos(new Vector2(0, -Screen.height), 0.5f)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                virusMemoZoom.SetActive(false);

                if (isPlayerInside)
                    PlayerInteractSystem.Instance.AddInteractable(gameObject);
            });

        cluePickup.PickupClue();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = true;

        if (!isZoomActive)
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;

        if (isZoomActive)
            HideVirusMemoZoom(); // 플레이어가 나가면 자동 닫기

        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }
}
