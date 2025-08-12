using UnityEngine;

public class Ch3_ConsoleButton : MonoBehaviour
{
    //[SerializeField] private GameObject highlight;

    [Header("버튼")]
    [SerializeField] private GameObject pushBtn;
    [SerializeField] private AudioClip buttonSFX;

    [Header("정답 버튼")]
    [SerializeField] private Ch3_ConsoleAnswerButton[] answerButtons;
    [SerializeField] private int correctAnswerBtn;

    private Ch3_Console console;

    private bool isMouseInRange = false;
    public bool canClick = true;

    void Start()
    {
        console = GetComponentInParent<Ch3_Console>();
    }

    void Update()
    {
        if(!canClick)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (!isMouseInRange)
                return;

            SoundManager.Instance.PlaySFX(buttonSFX);

            console.OnButtonClicked(this);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideQuestion();

            console.ResetCurrentButton(this);
        }
    }

    private void OnMouseEnter()
    {
        isMouseInRange = true;
    }

    private void OnMouseExit()
    {
        isMouseInRange = false;
    }

    public void OnAnswerSelected(Ch3_ConsoleAnswerButton selectedBtn)
    {
        Debug.Log("선택된 버튼은 " + selectedBtn);
        foreach (var btn in answerButtons)
        {
            if (btn != selectedBtn)
                btn.DeSelectAnswer(); // 나머지 버튼들은 선택 해제
        }

        CheckAnswer(); // 선택 후 정답인지 확인
    }

    private void CheckAnswer()
    {
        bool isCorrect = answerButtons[correctAnswerBtn].IsSelected;
        
        Debug.Log("정답 확인: " + isCorrect + "정답버튼은" + answerButtons[correctAnswerBtn].name);
        if (isCorrect)
            console.CheckAllAnswers(); // Console에서 전체 정답 여부 확인
    }

    public bool IsCorrectlyAnswered()
    {
        return answerButtons[correctAnswerBtn].IsSelected;
    }

    public void ShowQuestion()
    {
        pushBtn.SetActive(true);
        //SelectActive = true;
        foreach (var button in answerButtons)
        {
            button.ActivateButton();
        }
    }

    public void HideQuestion()
    {
        pushBtn.SetActive(false);
        //SelectActive = false;
        foreach (var button in answerButtons)
        {
            button.DeActiveButton();
        }
    }
}
