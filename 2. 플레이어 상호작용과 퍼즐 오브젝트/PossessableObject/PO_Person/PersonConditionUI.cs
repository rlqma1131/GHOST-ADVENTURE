using UnityEngine;


// 컨디션 * UI * 를 관리하는 스크립트입니다.

public enum PersonCondition
{
    Vital,   // 활력
    Normal,  // 보통
    Tired,    // 피곤함
    Unknown // 알 수 없음(빙의 불가)
}

public class PersonConditionUI : MonoBehaviour
{
    public PersonCondition currentCondition;
    private PersonCondition lastCondition;
    public PersonConditionHandler conditionHandler;
    [SerializeField] private float yPos_UI = 2.5f; // UI의 y포지션 (오브젝트마다 알맞게 설정해주세요)
    public GameObject UI;
    public GameObject vitalUI; // 활력UI
    public GameObject normalUI; // 보통UI
    public GameObject tiredUI; // 피곤UI
    [SerializeField] private BasePossessable targetPerson;

    void Start()
    {   
        // currentCondition = PersonCondition.Vital;
        conditionHandler = new VitalConditionHandler();
        targetPerson = GetComponent<BasePossessable>();
        ShowConditionUI();
    }
    
    void Update()
    {
        if (targetPerson.isPossessed)
        {
            if (UI != null) UI.SetActive(false);
            return;
        }
        // UI컨디션이 갱신될 때만 표시
        if (currentCondition != lastCondition)
            {
                ShowConditionUI();
                lastCondition = currentCondition;
            }

        // UI 위치는 매 프레임 갱신 (움직이는 캐릭터)
        if (UI != null)
            UI.transform.position = transform.position + Vector3.up * yPos_UI;
    }

    // 컨디션 UI 보여주는 함수
    public void ShowConditionUI()
    {
        if (targetPerson.isPossessed)
        {
            Debug.Log("컨디션UI 없애기");
            if (UI != null) UI.SetActive(false);
            return;
        }

        vitalUI.SetActive(false);
        normalUI.SetActive(false);
        tiredUI.SetActive(false);

        // 현재 상태에 맞는 UI를 선택하고 위치 설정 + 켜기
        if (currentCondition == PersonCondition.Vital)
            UI = vitalUI;
        else if (currentCondition == PersonCondition.Normal)
            UI = normalUI;
        else if (currentCondition == PersonCondition.Tired)
            UI = tiredUI;

        Vector3 uiPos = transform.position + Vector3.up * yPos_UI;
        UI.transform.position = uiPos;

        UI.SetActive(true);
    }

    // 컨디션벌 QTE 달라지게 만드는 함수.
    public void SetCondition(PersonCondition condition)
    {
        currentCondition = condition;
        switch (condition)
        {
            case PersonCondition.Vital:
                conditionHandler = new VitalConditionHandler();
                break;
            case PersonCondition.Normal:
                conditionHandler = new NormalConditionHandler();
                break;
            case PersonCondition.Tired:
                conditionHandler = new TiredConditionHandler();
                break;
        }
        QTESettings qteSettings = conditionHandler.GetQTESettings();
        UIManager.Instance.QTE_UI_3.ApplySettings(qteSettings);
    }
}



