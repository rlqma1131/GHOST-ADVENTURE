using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_SandCastle : BaseInteractable
{
    private CluePickup cluepickup; // 단서획득
    [SerializeField] GameObject carToy; // 장난감자동차 - Ch2_부정기억1
    [SerializeField] GameObject SandCastle_intactly; // 모래성
    [SerializeField] GameObject SandCastle_crumble; // 무너진 모래성
    [SerializeField] GameObject q_key;
    [SerializeField] Ch2_Raven raven; // 까마귀
    public bool crumbleAble = false; // 모래성을 무너뜨릴 수 있는 범위에 있는지 확인
    private bool crumbled = false; // 모래성을 무너뜨렸는지 확인


    // 까마귀가 와서 Q키를 누르면
    // 모래성이 무너진다.
    // 인형그림 단서를 획득하고 장난감 자동차가 나타난다.

    void Start()
    {
        carToy.SetActive(false);
        SandCastle_crumble.SetActive(false);
        q_key.SetActive(false);
        cluepickup = GetComponent<CluePickup>();
    }

    void Update()
    {
        if(crumbleAble && !crumbled && raven.isPossessed)
        {
            if(Input.GetKeyDown(KeyCode.Q))
            {
                SandCastle_crumble.SetActive(true);
                carToy.SetActive(true);
                SandCastle_intactly.SetActive(false);
                q_key.SetActive(false);
                cluepickup.PickupClue();
                crumbled = true;
                UIManager.Instance.PromptUI.ShowPrompt("무너뜨릴까?");
            }

        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if(collision.CompareTag("Animal"))
        {
            raven = collision.GetComponent<Ch2_Raven>();
        }
        if(collision.CompareTag("Animal") && raven.isPossessed && !crumbled)
        {
            crumbleAble = true;
            q_key.SetActive(true);
        }
    }

    protected override void OnTriggerExit2D(Collider2D other) 
    {
        base.OnTriggerExit2D(other);
        if(other.CompareTag("Animal"))
        {
            raven = other.GetComponent<Ch2_Raven>();
        }

        if(other.CompareTag("Animal") && !crumbled)
        {
            crumbleAble = true;
            q_key.SetActive(false);
        }
    }

}
