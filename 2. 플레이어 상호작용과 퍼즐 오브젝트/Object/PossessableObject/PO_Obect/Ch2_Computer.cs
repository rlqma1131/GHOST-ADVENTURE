using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class Ch2_Computer : BasePossessable
{
    [Header("UI References")]
    [SerializeField] private RectTransform monitorPanel;
    [SerializeField] private GameObject fileIcon;
    [SerializeField] private GameObject passwordPanel;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button closePasswordButton;
    
    [SerializeField] private GameObject correctImage;
    [SerializeField] private GameObject wrongImage;
    [SerializeField] private float wrongShakeStrength = 0.2f;
    [SerializeField] private float wrongShakeDuration = 0.3f;
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private Image panelBackgroundImage;
    [SerializeField] private Color defaultColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color flashColor = Color.red;

    [Header("Puzzle Settings")]
    [SerializeField] private string correctPassword;
    [SerializeField] private LockedDoor doorToOpen;
    // [SerializeField] private GameObject monitorOn;

    private Vector2 hiddenPos = new(0, -800);
    private Vector2 visiblePos = new(0, 0);

    private bool isPanelOpen = false;
    private float lastClickTime;
    private const float doubleClickDelay = 0.3f;
    
    private Inventory_Player _inventory;

    protected override void Start()
    {
        hasActivated = true;
        monitorPanel.anchoredPosition = hiddenPos;
        monitorPanel.gameObject.SetActive(false);
        passwordPanel.SetActive(false);
        
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(SubmitPassword);
        }
        
        if (closePasswordButton != null)
        {
            closePasswordButton.onClick.AddListener(ClosePasswordPanel);
        }

        _inventory = UIManager.Instance.Inventory_PlayerUI;
    }

    protected override void Update()
    {
        if (!isPossessed || !hasActivated)
            return;

        if (!isPanelOpen)
            OpenPanel();

        if (Input.GetKeyDown(KeyCode.E))
        {
            Unpossess();
            ClosePanel();
        }

        // 엔터 키로 비밀번호 제출
        if (passwordPanel.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            SubmitPassword();
        }
    }

    public void OpenPanel()
    {
        EnemyAI.PauseAllEnemies();
        isPanelOpen = true;
        monitorPanel.gameObject.SetActive(true);
        monitorPanel.DOAnchorPos(visiblePos, 0.5f).SetEase(Ease.OutBack);
    }

    public void ClosePanel()
    {
        EnemyAI.ResumeAllEnemies();
        isPanelOpen = false;
        passwordPanel.SetActive(false);
        monitorPanel.DOAnchorPos(hiddenPos, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                monitorPanel.gameObject.SetActive(false);
            });
    }
    
    private void ClosePasswordPanel()
    {
        passwordPanel.SetActive(false);
        
        if (_inventory != null) _inventory.enabled = true;
    }
    
    public void OnFileIconClick()
    {
        float time = Time.time;
        if (time - lastClickTime < doubleClickDelay)
        {
            OpenPasswordPanel();
        }
        lastClickTime = time;
    }

    private void OpenPasswordPanel()
    {
        passwordPanel.SetActive(true);
        passwordInput.text = "";
        passwordInput.ActivateInputField();
        
        if (_inventory != null) _inventory.enabled = false;
    }

    private void SubmitPassword()
    {
        string input = passwordInput.text.Trim().ToUpper(); // 입력값을 대문자로 변환
        string correct = correctPassword.Trim().ToUpper();  // 정답도 대문자로 변환

        if (input == correctPassword)
        {
            if (correctImage != null)
            {
                correctImage.SetActive(true);
            }
            StartCoroutine(ShowCorrectImage());
            UIManager.Instance.PromptUI.ShowPrompt("어디 문이 열린지..?", 2f);
        }
        else
        {
            passwordInput.text = "";
            StartCoroutine(WrongFeedback());
            UIManager.Instance.PromptUI.ShowPrompt("흠.. 주변을 둘러보자.", 2f);
        }
    }
    
    private IEnumerator ShowCorrectImage()
    {
        yield return new WaitForSeconds(2f);
        doorToOpen.SolvePuzzle();
        ClosePanel();
        Unpossess();
    }
    
    private IEnumerator WrongFeedback()
    {
        wrongImage.SetActive(true);
        // 흔들림 (UI 패널 자체 흔들기)
        monitorPanel.DOShakeAnchorPos(wrongShakeDuration, wrongShakeStrength);

        // 빨간색 플래시
        if (panelBackgroundImage != null)
        {
            panelBackgroundImage.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            panelBackgroundImage.color = defaultColor;
        }
        
        yield return new WaitForSeconds(1f);
        wrongImage.SetActive(false);

        // 입력창 재활성화
        passwordInput.ActivateInputField();
    }

    // public void Activate()
    // {
    //     hasActivated = true;
    //     MarkActivatedChanged();
    //     monitorOn.SetActive(false);
    // }
}
