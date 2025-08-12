using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_DollPicture : BaseInteractable
{
    private bool interactAble = false;
    private bool CompleteCluePickup = false;
    CluePickup cluePickup;
    void Start()
    {
        cluePickup = GetComponent<CluePickup>();
    }
    void Update()
    {
        if(interactAble && !CompleteCluePickup)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                cluePickup.PickupClue();
                UIManager.Instance.PromptUI.ShowPrompt("모래성에서 본 그림과 다른데… 하나는 가짜야.");
                CompleteCluePickup = true;
            }
        }
        
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !CompleteCluePickup)
        {
            interactAble = true;
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
        interactAble = false;   
    }
}
