using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.VisualScripting;

// 튜토리얼은 1회만 작동됩니다. 
public enum TutorialStep
{
    LivingRoomIntro_Start, // CH1 씬 시작시 대화
    ShowControlKey_And_HighLightBithdayBox, // 조직키 보여주고 깜짝상자에 행동강제
    Q_key_Interact, // Q_key 빙의후행동 안내
    HideArea_Interact, // 은신처 상호작용 안내
    HideArea_QTE, // 은신처 QTE 안내
    Mouse_Possesse, // 쥐 빙의 후 안내
    LaundryRoom,
    EnergyRestoreZone, // 에너지회복존 안내
    Cake_Prompt, //
    SecurityGuard_GoToRadio,
    SecurityGuard_AfterRest,
    SecurityGuard_InOffice,
    BlackShadow,
    CollectedAllMemoClue,
    Test,
    TouchBat,
    MemoryStorageGuide
}
public class TutorialManager : Singleton<TutorialManager>
{
    private HashSet<TutorialStep> completedSteps = new HashSet<TutorialStep>(); // 완료된 튜토리얼
    private Action OnAction;
    private UIManager uimanager;
    private NoticePopup notice; // 알림창 (가이드안내)
    private Prompt prompt; // 프롬프트 (대사출력)
    private bool canMove; // 플레이어 움직일 수 있는지

    // Ch1 Tutorial
    [SerializeField] private Ch1_CelebrityBox celebrityBox;

    private void Start()
    {
        uimanager = UIManager.Instance;
        notice = uimanager.NoticePopupUI;
        prompt = uimanager.PromptUI;
        
    }
    
    public void Show(TutorialStep step)
    {
        canMove = PossessionSystem.Instance.CanMove;
        if (completedSteps.Contains(step)) return;

        completedSteps.Add(step);

        switch (step)
        {
            case TutorialStep.LivingRoomIntro_Start:
                LivingRoom_StartTutorial();
                break;

            case TutorialStep.ShowControlKey_And_HighLightBithdayBox: // 튜토리얼 키 보여주기, 깜짝상자 하이라이트
                PossessionSystem.Instance.CanMove = true;
                uimanager.TutorialUI_OpenAll();
                celebrityBox = FindObjectOfType<Ch1_CelebrityBox>();
                celebrityBox.highlight.SetActive(true); 
                break;
            case TutorialStep.Test:
                uimanager.TutorialUI_OpenAll();
                break;

            case TutorialStep.Q_key_Interact:
                notice.FadeInAndOut("※ 빙의 후에는 Q를 눌러 특정 행동을 할 수 있습니다.");
                break;

            case TutorialStep.HideArea_Interact:
                notice.FadeInAndOut("※특정 오브젝트 빙의는 쉽지않을 수 있습니다.");
                break;

            case TutorialStep.HideArea_QTE:
                break;
            case TutorialStep.Mouse_Possesse:
                prompt.ShowPrompt("숨겨진 공간을 찾아볼까?");
                break;
            
            case TutorialStep.LaundryRoom:
                prompt.ShowPrompt("…여긴… 잠깐, 문이… 닫혔어?");
                notice.FadeInAndOut("※ 제한 시간 내에 퍼즐을 해결하지 못하면 나갈 수 없습니다.");
                break;

            case TutorialStep.EnergyRestoreZone:
                notice.FadeInAndOut("※ 빛이 나는 곳 근처에서 에너지가 회복됩니다.");
                break;

            case TutorialStep.Cake_Prompt:
                prompt.ShowPrompt("쥐는 어디로 갔을라나? 아무튼 이제 케잌을 살펴보자");
                break;
            case TutorialStep.SecurityGuard_GoToRadio:
                prompt.ShowPrompt("나왔다... 지금이 기회야");
                notice.FadeInAndOut("※ 사람은 컨디션이 좋을수록 빙의가 어려워집니다.");
                break;
            case TutorialStep.SecurityGuard_AfterRest:
                prompt.ShowPrompt("컨디션이 너무 좋아서 힘들겠어..");
                break;
            case TutorialStep.SecurityGuard_InOffice:
                prompt.ShowPrompt("관리실에 들어가버렸어. 다시 유도해야 해.");
                break;
            case TutorialStep.BlackShadow:
                prompt.ShowPrompt_2("방금... 그림자가...?", "여기… 무언가가 떨어져 있어. 살펴보자.");
                break;
            case TutorialStep.CollectedAllMemoClue:
                CollectedAllMemoClue();
                break;
            case TutorialStep.TouchBat:
                prompt.ShowPrompt("박쥐를 건드리면 안돼");
                break;
            case TutorialStep.MemoryStorageGuide:
                PossessionSystem.Instance.CanMove = false;
                uimanager.guidBlackPanel.SetActive(true);
                uimanager.memoryStorageButton.GetComponent<Button>().
                onClick.AddListener(() => {
                    uimanager.guidBlackPanel.SetActive(false);
                    PossessionSystem.Instance.CanMove = true;
                    UIManager.Instance.guidButton.SetActive(true);
                    }); // 클릭되면 실행
                break;

            // case TutorialStep.HideGuide:
            //     ToastUI.Instance.Show("※ 특정 오브젝트 빙의는 쉽지 않을 수 있습니다.\n숨을 수 있어!", 3f);
            //     break;

            // ...다른 케이스도 추가
        }
    }

    // 거실 진입시
    public async void LivingRoom_StartTutorial()
    {   
        PossessionSystem.Instance.CanMove = false;
        await Task.Delay(2000);
        prompt.ShowPrompt_2("나도 모르게 여기로 들어왔어..", "여기서 기억을 찾을 수 있을까?..");
        await Task.Delay(3000);
        notice.FadeInAndOut("※ 이 집 안에 흩어진 기억 조각을 찾아 수집하세요.");
        await Task.Delay(3000);
        WaitTimeAfterShowTutorial(0f, TutorialStep.ShowControlKey_And_HighLightBithdayBox);
    }

    public async void CollectedAllMemoClue()
    {
        await Task.Delay(2000);
        prompt.ShowPrompt("이 제목들, 뭔가 의미가 있어... \n책장을 찾으면 알 수 있을까.");
    }
    

    public void WaitTimeAfterShowTutorial(float time, TutorialStep tutorial)
    {
        DOTween.Sequence()
            .AppendInterval(time)
            .AppendCallback(() =>
            {
                if(OnAction != null) {OnAction?.Invoke();}
                Show(tutorial);
                OnAction = null;
            });
    }
    public bool HasCompleted(TutorialStep step) => completedSteps.Contains(step);
}
