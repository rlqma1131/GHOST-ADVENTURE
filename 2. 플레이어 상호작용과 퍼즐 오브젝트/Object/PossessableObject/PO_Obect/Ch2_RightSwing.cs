using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_RightSwing : BasePossessable
{
    [SerializeField] private AudioClip swingSFX;
    [SerializeField] private GameObject q_Key;
    
    protected override void Update()
    {
        base.Update();

        if (!isPossessed)
        {
            q_Key.SetActive(false);
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            q_Key.SetActive(false);
            anim.SetTrigger("RightSwing");
            SoundManager.Instance.PlaySFX(swingSFX);
            UIManager.Instance.PromptUI.ShowPrompt("아무일도 일어나지 않는군..",2f);
        }
        q_Key.SetActive(true);
    }
}
