using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Ch3_PatientDocumentIndex : MonoBehaviour
{
    [Header("서류 인덱스")]
    [SerializeField] private GameObject highlight;

    [Header("줌 서류")]
    [SerializeField] private GameObject documentZoom; // 확대용 드로잉 UI (Canvas 하위)
    [SerializeField] private RectTransform documentPos; // 드로잉 UI의 시작 위치
    [SerializeField] private Image zoomPanel;        // 배경 패널 (알파 페이드용)
    [SerializeField] private AudioClip paperSFX;     // 줌 효과음

    private bool isMouseInRange = false;
    private bool isZoomActive = false;
    private bool isChecked = false;
    public bool IsChecked => isChecked;

    void Start()
    {
        // UI 초기화
        documentZoom.SetActive(false);
        documentPos.anchoredPosition = new Vector2(0, -Screen.height);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isZoomActive && !isMouseInRange)
                return;

            if (isZoomActive)
            {
                HideDocumentZoom();
            }
            else
            {
                SoundManager.Instance.PlaySFX(paperSFX);
                ShowDocumentZoom();
                if (!isChecked)
                {
                    isChecked = true;
                }
            }
        }

        if (isZoomActive && Input.GetKeyDown(KeyCode.Escape))
        {
            HideDocumentZoom();
        }
    }
    private void OnMouseEnter()
    {
        isMouseInRange = true;
        highlight.SetActive(true);
    }

    private void OnMouseExit()
    {
        isMouseInRange = false;
        highlight.SetActive(false);
    }

    private void ShowDocumentZoom()
    {
        isZoomActive = true;

        // 패널 페이드 인
        zoomPanel.color = new Color(zoomPanel.color.r, zoomPanel.color.g, zoomPanel.color.b, 0f);
        zoomPanel.DOFade(150f / 255f, 0.5f);

        // 슬라이드 인
        documentZoom.SetActive(true);
        documentPos.anchoredPosition = new Vector2(0, -Screen.height);
        documentPos.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic);
    }

    private void HideDocumentZoom()
    {
        isZoomActive = false;

        zoomPanel.DOFade(0f, 0.5f);

        documentPos.DOAnchorPos(new Vector2(0, -Screen.height), 0.5f)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                documentZoom.SetActive(false);
            });
    }
}

