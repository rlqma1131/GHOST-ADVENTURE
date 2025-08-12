using UnityEngine;

public class Ch3_ConsoleAnswerButton : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private SpriteRenderer button;
    [SerializeField] private GameObject push;
    [SerializeField] private AudioClip buttonSFX;

    [Header("부모 콘솔 버튼")]
    [SerializeField] private Ch3_ConsoleButton parentConsoleButton;

    private bool isMouseInRange = false;
    public bool isActive = false;
    private bool isSelected = false;
    public bool IsSelected => isSelected;

    void Start()
    {
        button.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isMouseInRange || !button.enabled)
                return;

            SoundManager.Instance.PlaySFX(buttonSFX);

            if (isSelected)
            {
                DeSelectAnswer();
                return;
            }
            else
            {
                SelectAnswer();
            }
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

    private void SelectAnswer()
    {
        Debug.Log("정답 버튼 선택됨: " + gameObject.name);
        isSelected = true;
        parentConsoleButton?.OnAnswerSelected(this); // 선택되었음을 부모에게 알림
        push.SetActive(false);
    }

    public void DeSelectAnswer()
    {
        Debug.Log("정답 버튼 선택 해제됨: " + gameObject.name);
        push.SetActive(true);
        isSelected = false;
    }

    public void ActivateButton()
    {
        Debug.Log("정답 버튼 활성화됨: " + gameObject.name);
        button.gameObject.SetActive(true);
        button.enabled = true;
        isActive = true;
    }

    public void DeActiveButton()
    {
        Debug.Log("정답 버튼 비활성화됨: " + gameObject.name);
        button.gameObject.SetActive(false);
        button.enabled = false;
        isActive = false;
    }
}
