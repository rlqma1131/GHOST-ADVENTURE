using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowPrompt : MonoBehaviour
{

   [SerializeField] private GameObject PromptPanel; // 프롬프트 패널 이미지
   [SerializeField] private TextMeshProUGUI PromptText; // 프롬프트 텍스트
    private bool isActive = false;

    //public void ShowPrompt_TimeLine(string dialog)
    //{


    //    UIManager.Instance.PromptUI2.ShowPrompt(dialog, 2f);

    //}

    public void ShowPrompt_Cutscene(string line)
    {

        PromptPanel.SetActive(true); // 패널 보이게하기
        PromptText.text = line;
        StartCoroutine(HideAfterDelay(2));
    }

    private IEnumerator HideAfterDelay(float dlaytime)
    {
        yield return new WaitForSeconds(dlaytime);
        PromptPanel.SetActive(false);
        isActive = false;
        // onD
    }
}
