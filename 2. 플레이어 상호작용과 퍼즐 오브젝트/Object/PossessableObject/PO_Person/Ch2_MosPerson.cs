using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ch2_MosPerson : BasePossessable
{
    [SerializeField] GameObject q_key;
    private PersonConditionUI targetPerson;
    private HaveItem haveitem;
    private bool UseAllItem = false;

    protected override void Start()
    {
        base.Start();
        haveitem = GetComponent<HaveItem>();
        targetPerson = GetComponent<PersonConditionUI>();
        targetPerson.currentCondition = PersonCondition.Tired;
    }

    protected override void Update()
    {
        targetPerson.currentCondition = PersonCondition.Tired;
        targetPerson.SetCondition(targetPerson.currentCondition);
        if (!isPossessed)
            return;

        UIManager.Instance.tabkeyUI.SetActive(true);
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (haveitem.IsInventoryEmpty())
            {
                Unpossess();
                hasActivated = false;
                MarkActivatedChanged();

                UseAllItem = true;
            }
            else
            {
                UIManager.Instance.PromptUI.ShowPrompt("뭔가 더 얻을 수 있는게 있을것 같아");
            }
            // if (haveitem.inventorySlots.All(s => s.item == null))
            // {
        
            //     Unpossess();
            //     hasActivated = false;
            // }

        //     bool allEmpty = haveitem.inventorySlots.All(slot => 
        //     slot.item == null || slot.quantity <= 0);

        //     if (allEmpty)
        //     {
        //         Unpossess();
        //  }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if(collision.CompareTag("Player") && !UseAllItem)
        {
            UIManager.Instance.PromptUI.ShowPrompt("저 사람, 메모를 들고 있어. 빙의해볼까?");
        } 
    }

    public override void Unpossess()
    {
        base.Unpossess();
        targetPerson.currentCondition = PersonCondition.Normal; 
        UIManager.Instance.tabkeyUI.SetActive(false);
    }
    
}
