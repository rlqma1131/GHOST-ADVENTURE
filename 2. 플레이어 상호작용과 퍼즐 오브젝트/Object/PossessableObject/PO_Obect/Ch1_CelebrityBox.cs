using System.Collections;
using UnityEngine;

public class Ch1_CelebrityBox : BasePossessable
{
    // [SerializeField] private GameObject effect;
    [SerializeField] private GameObject birthdayLetter;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject q_Key;


    protected override void Update()
    {
        base.Update();
        
        if (!isPossessed || !hasActivated)
        {
            if(q_Key != null)
            q_Key.SetActive(false);
            return;
        }
        
        InteractTutorial();
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            q_Key.SetActive(false);
            TriggerBoxEvent();
        }
        q_Key.SetActive(true);
    }

    private void InteractTutorial()
    {
        TutorialManager.Instance.Show(TutorialStep.Q_key_Interact);
    }

    private void TriggerBoxEvent()
    {
        hasActivated = true;
        MarkActivatedChanged();

        UIManager.Instance.Hide_Q_Key();
        
        // 박스 애니메이션 트리거
        if(animator != null)
            animator.SetTrigger("Explode");

        // 폭발 이펙트 ( 넣는다면 )
        // if(effect != null)
        //     Instantiate(effect, transform.position, Quaternion.identity);

        // Letter 활성화
        StartCoroutine(ShowLetterWithDelay());

        hasActivated = false;
        MarkActivatedChanged();

        SaveManager.MarkPuzzleSolved("깜짝상자");
    }
    private IEnumerator ShowLetterWithDelay()
    {
        birthdayLetter.SetActive(true);
        yield return null; // 한 프레임 기다림
        Animator anim = birthdayLetter.GetComponent<Animator>();
        anim?.SetTrigger("Pop");

        yield return null;
        Unpossess(); // 빙의 해제
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasActivated)
            return;

        if (other.CompareTag("Player") && !SaveManager.IsPuzzleSolved("깜짝상자"))
        {
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
            // UIManager.Instance.TutorialUI_CloseAll();
            UIManager.Instance.PromptUI.ShowPrompt("…상자? 왜 여기에 이런 게…");
            UIManager.Instance.NoticePopupUI.FadeInAndOut("※ 파란 빛을 띄는 오브젝트는 E키로 빙의할 수 있습니다.");
        }
    }
}
