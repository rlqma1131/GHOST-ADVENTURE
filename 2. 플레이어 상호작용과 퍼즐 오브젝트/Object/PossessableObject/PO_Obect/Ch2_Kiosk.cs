using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class Ch2_Kiosk : BasePossessable
{
    [SerializeField] private RectTransform kioskPanel;
    [SerializeField] private GameObject hintNoteObject;
    [SerializeField] private GameObject hiddenDoorObject;
    [SerializeField] private GameObject kioskOn;
    
    [SerializeField] private Button[] allButtons;
    [SerializeField] private List<Button> correctSequence; // 인스펙터에서 정답 순서대로 등록
    [SerializeField] private List<Button> hiddenSequence;  // 히든 정답, 숨은 통로
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private TextMeshProUGUI inputTextUI;
    
    private List<Button> currentInput = new();
    private bool isPanelOpen = false;
    private bool correctSolved = false;
    private bool hiddenSolved = false;
    private Vector2 hiddenPos = new(0, -800);
    private Vector2 visiblePos = new(0, 0);
    
    [SerializeField] private SafeBox safeBox;

    protected override void Start()
    {
        hasActivated = false;
        kioskPanel.anchoredPosition = hiddenPos;
        kioskPanel.gameObject.SetActive(false);
    }

    protected override void Update()
    {
        // base.Update();

        if (!isPossessed || !hasActivated)
        {
            //q_Key.SetActive(false);
            return;
        }
        if (!isPanelOpen)
            OpenPanel();
        
        // if (Input.GetKeyDown(KeyCode.Q))
        // {
        //     if (!isPanelOpen)
        //     {
        //         q_Key.SetActive(false);
        //         
        //     }
        //     else
        //     {
        //         
        //     }
        // }

        if (Input.GetKeyDown(KeyCode.E))
        {
            ClosePanel();
            Unpossess();
        }
        
        //q_Key.SetActive(true);
    }

    public override void CantPossess()
    {
        UIManager.Instance.PromptUI.ShowPrompt("전력이 없어..",2f);
    }

    public void Activate()
    {
        hasActivated = true;
        MarkActivatedChanged();

        kioskOn.SetActive(false);
    }
    
    private void OpenPanel()
    {
        isPanelOpen = true;
        kioskPanel.gameObject.SetActive(true);
        EnemyAI.PauseAllEnemies();
        kioskPanel.DOAnchorPos(visiblePos, 0.5f).SetEase(Ease.OutBack);

        currentInput.Clear();
        
        if (safeBox != null)
        {
            if (safeBox.safeBoxOpen)
            {
                // UIManager.Instance.PromptUI.ShowPrompt("금고 안에서 본 이상한 기호들... 저 버튼과 닮았어.", 2f);
            }
            else
            {
                UIManager.Instance.PromptUI.ShowPrompt("어디선가 본 색깔들인데?", 2f);
            }
        }
        
        // 버튼 이벤트 연결
        foreach (var btn in allButtons)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnButtonPressed(btn));
        }
        
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(CheckAnswer);
        
        resetButton.onClick.RemoveAllListeners();
        resetButton.onClick.AddListener(ResetInput);
    }
    
    private void ClosePanel()
    {
        isPanelOpen = false;
        EnemyAI.ResumeAllEnemies();

        kioskPanel.DOAnchorPos(hiddenPos, 0.5f)
                  .SetEase(Ease.InBack)
                  .OnComplete(() =>
                  {
                      kioskPanel.gameObject.SetActive(false);
                  });

        currentInput.Clear();
        UpdateInputDisplay();
    }

    private void OnButtonPressed(Button btn)
    {
        if (currentInput.Count >= 5)
        {
            UIManager.Instance.PromptUI.ShowPrompt("최대 5개까지만 입력할 수 있어.", 1.5f);
            return;
        }
        
        currentInput.Add(btn);
        Debug.Log($"버튼 입력됨: {btn.name}");
        UpdateInputDisplay();
    }

    private void CheckAnswer()
    {
        bool isCorrect = CompareSequence(currentInput, correctSequence);
        bool isHidden = CompareSequence(currentInput, hiddenSequence);

        if (isCorrect && !correctSolved)
        {
            correctSolved = true;
            Unpossess();
            UIManager.Instance.PromptUI.ShowPrompt("뭔가 나온거 같은데?", 2f);
            hintNoteObject.SetActive(true);
            ClosePanel();
        }
        else if (isHidden && !hiddenSolved)
        {
            hiddenSolved = true;
            Unpossess();
            hiddenDoorObject.SetActive(true);
            UIManager.Instance.PromptUI.ShowPrompt("뭔가 열리는 소리가 들린거 같아.", 2f);
            ClosePanel();
        }
        else if (isCorrect && correctSolved)
        {
            UIManager.Instance.PromptUI.ShowPrompt("이미 본 힌트야.", 1.5f);
        }
        else if (isHidden && hiddenSolved)
        {
            UIManager.Instance.PromptUI.ShowPrompt("이미 열려 있어.", 1.5f);
        }
        else
        {
            UIManager.Instance.PromptUI.ShowPrompt("아무 일도 일어나지 않는다.", 1.5f);
        }

        currentInput.Clear();
        UpdateInputDisplay();
        
        if (correctSolved && hiddenSolved)
        {
            hasActivated = false;
            MarkActivatedChanged();
        }
    }
    
    private void ResetInput()
    {
        currentInput.Clear();
        UpdateInputDisplay();
    }

    private bool CompareSequence(List<Button> input, List<Button> target)
    {
        if (input.Count != target.Count) return false;

        for (int i = 0; i < target.Count; i++)
        {
            if (input[i] != target[i]) return false;
        }

        return true;
    }

    private void UpdateInputDisplay()
    {
        string sequence = string.Join(" > ", currentInput.Select(b => b.name));
        inputTextUI.text = sequence;
    }
}