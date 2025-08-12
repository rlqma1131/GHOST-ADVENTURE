using DG.Tweening;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Ch3_Lock : BasePossessable
{
    public enum ButtonType { None, Top, Bottom, Num }
    [SerializeField] private AudioClip open;
    [SerializeField] private GameObject mainSprite;

    [Header("확대 UI")]
    [SerializeField] private GameObject lockZoom;
    [SerializeField] private RectTransform lockPos;
    [SerializeField] private Image zoomPanel;

    [Header("버튼 & 이미지")]
    [SerializeField] private GameObject topBtn;
    [SerializeField] private GameObject bottomBtn;
    [SerializeField] private GameObject[] numBtns;

    [Header("도형 스프라이트")]
    [SerializeField] private Sprite[] tops;
    [SerializeField] private Sprite[] bottoms;

    [Header("숫자 스프라이트")]
    [SerializeField] private Sprite[] nums; // 0~9

    [Header("기억 조각 서랍장")]
    [SerializeField] private Ch3_Shelf shelf;

    [Header("Xray 모니터 && 환자 서류")]
    [SerializeField] private Ch3_Xray_Monitor xrayMonitor;
    [SerializeField] private Ch3_PatientDocumentIndex[] documentIndex;

    // 상태 관리
    [HideInInspector] public ButtonType selectedType = ButtonType.None;
    [HideInInspector] public int selectedNumIndex = -1;
    private Ch3_LockButtons buttons;

    private Image topImage;
    private Image bottomImage;
    private Image[] numImages;
    private int[] numValues = new int[3];

    private int topIndex = 0;
    private int bottomIndex = 0;

    private bool isPlayerInside = false;
    private bool isZoomActive = false;
    private bool isSolved = false;

    protected override void Start()
    {
        base.Start();

        topImage = topBtn.GetComponent<Image>();
        bottomImage = bottomBtn.GetComponent<Image>();

        if (tops.Length > 0) topImage.sprite = tops[topIndex];
        if (bottoms.Length > 0) bottomImage.sprite = bottoms[bottomIndex];

        numImages = new Image[numBtns.Length];
        numValues = new int[numBtns.Length];

        for (int i = 0; i < numBtns.Length; i++)
        {
            numImages[i] = numBtns[i].GetComponent<Image>();
            numValues[i] = 0;
            numImages[i].sprite = nums[0];

            var highlight = numBtns[i].transform.Find("highlight")?.gameObject;
            if (highlight != null) highlight.SetActive(false);
        }

        lockZoom.SetActive(false);
        lockPos.anchoredPosition = new Vector2(0, -Screen.height);
    }

    protected override void Update()
    {
        if (!isPossessed || isSolved) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isZoomActive && !isPlayerInside) return;

            HideLockZoom();
            Unpossess();
        }

        if (isZoomActive && selectedType != ButtonType.None)
        {
            switch (selectedType)
            {
                case ButtonType.Top:
                    if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
                        ToggleTop();
                    break;

                case ButtonType.Bottom:
                    if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
                        ToggleBottom();
                    break;

                case ButtonType.Num:
                    if (Input.GetKeyDown(KeyCode.W))
                        ChangeNumValue(+1);
                    else if (Input.GetKeyDown(KeyCode.S))
                        ChangeNumValue(-1);
                    break;
            }
        }
    }

    // 정답은 눈, 거미줄 / 412 일 때
    private void CheckAnswer()
    {
        Debug.Log($"현재 상태: {IsCorrectAnswer()} Top ={topIndex}, Bottom={bottomIndex}, Num1={numValues[0]}, Num2={numValues[1]}, Num3={numValues[2]}");
        if (IsCorrectAnswer())
        {
            isSolved = true;
            hasActivated = false;
            MarkActivatedChanged();

            StartCoroutine(RevealMemory());
        }
    }

    IEnumerator RevealMemory()
    {
        HideLockZoom();
        Unpossess();

        hasActivated = false;
        MarkActivatedChanged();

        yield return new WaitForSeconds(0.5f);

        mainSprite.SetActive(false);
        yield return new WaitForSeconds(0.3f);

        SoundManager.Instance.PlaySFX(open);
        shelf.OpenShelf();
        gameObject.SetActive(false);
    }

    private bool IsCorrectAnswer()
    {
        return topImage.sprite == tops[1] &&
       bottomImage.sprite == bottoms[2] &&
       numImages[0].sprite == nums[4] &&
       numImages[1].sprite == nums[1] &&
       numImages[2].sprite == nums[2];
    }

    public void OnButtonClicked(ButtonType type, int idx)
    {
        selectedType = type;

        if (type == ButtonType.Num)
        {
            ChangeNumSelectionByIndex(idx);
        }
        else
        {
            selectedNumIndex = -1;
        }
    }

    public void ClearAllHighlights()
    {
        // top 버튼
        var top = topBtn.GetComponent<Ch3_LockButtons>();
        if (top != null) top.DisableHighlight();

        // bottom 버튼
        var bottom = bottomBtn.GetComponent<Ch3_LockButtons>();
        if (bottom != null) bottom.DisableHighlight();

        // 숫자 버튼들
        foreach (var btn in numBtns)
        {
            var comp = btn.GetComponent<Ch3_LockButtons>();
            if (comp != null) comp.DisableHighlight();
        }
    }

    private void ChangeNumSelectionByIndex(int newIndex)
    {
        if (selectedNumIndex >= 0 && selectedNumIndex < numBtns.Length)
        {
            var oldHL = numBtns[selectedNumIndex].transform.Find("highlight")?.gameObject;
            if (oldHL != null) oldHL.SetActive(false);
        }

        selectedNumIndex = Mathf.Clamp(newIndex, 0, numBtns.Length - 1);

        var newHL = numBtns[selectedNumIndex].transform.Find("highlight")?.gameObject;
        if (newHL != null) newHL.SetActive(true);
    }

    private void ToggleTop()
    {
        if (tops.Length < 2) return;
        topIndex = (topIndex + 1) % tops.Length;
        topImage.sprite = tops[topIndex];
        CheckAnswer();
    }

    private void ToggleBottom()
    {
        if (bottoms.Length == 0) return;
        bottomIndex = (bottomIndex + 1) % bottoms.Length;
        bottomImage.sprite = bottoms[bottomIndex];
        CheckAnswer();
    }

    private void ChangeNumValue(int delta)
    {
        numValues[selectedNumIndex] = (numValues[selectedNumIndex] + delta + 10) % 10;
        numImages[selectedNumIndex].sprite = nums[numValues[selectedNumIndex]];
        CheckAnswer();
    }

    private void ShowLockZoom()
    {
        EnemyAI.PauseAllEnemies();

        isZoomActive = true;
        zoomPanel.DOFade(150f / 255f, 0.5f);
        lockZoom.SetActive(true);
        lockPos.anchoredPosition = new Vector2(0, -Screen.height);
        lockPos.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic);
        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }

    private void HideLockZoom()
    {
        EnemyAI.ResumeAllEnemies();

        isZoomActive = false;
        zoomPanel.DOFade(0f, 0.5f);
        lockPos.DOAnchorPos(new Vector2(0, -Screen.height), 0.5f)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                lockZoom.SetActive(false);
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
        if (isZoomActive) HideLockZoom();
        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }

    public override void OnPossessionEnterComplete() 
    {
        ShowLockZoom();
        if (!xrayMonitor.IsSecondFind || !documentIndex.Any(doc => doc.IsChecked))
        {
            UIManager.Instance.PromptUI.ShowPrompt("정보가 부족해...");
        }
    }
}
