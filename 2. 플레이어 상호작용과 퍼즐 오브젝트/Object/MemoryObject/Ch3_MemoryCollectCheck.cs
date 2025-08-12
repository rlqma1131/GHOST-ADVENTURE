using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_MemoryCollectCheck : MonoBehaviour
{
    public Ch3_MemoryNegative_02_Paper paper;
    public Ch3_MemoryNegative_03_Handbones handbones;

    private bool allMemoryCollected()
    {
        return paper.Colected && handbones.Colected;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (allMemoryCollected())
            {
                gameObject.SetActive(false);
            }
            else
            {
                UIManager.Instance.PromptUI.ShowPrompt("아직 못 나가는 것 같아. 해야 할 일을 찾아보자");
            }
        }
    }
}
