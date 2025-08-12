using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Ch2_SewerLever : BasePossessable
{
    [SerializeField] private GameObject mazeGroupToDisable;
    [SerializeField] private GameObject q_Key;
    [SerializeField] private GameObject turnOn;
    [SerializeField] private Ch2_SewerLightingTrigger lightingTrigger;
    [SerializeField] private Light2D leverLight;

    protected override void Update()
    {
        base.Update();

        if (!isPossessed || !hasActivated)
        {
            q_Key.SetActive(false);
            return;
        }
        
        if (leverLight != null)
        {
            leverLight.enabled = true;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            turnOn.SetActive(true);
            Ch2_SewerPuzzleManager.Instance.SetPuzzleSolved();
            
            if (lightingTrigger != null)
            {
                lightingTrigger.ForceRestoreLighting();
            }
            
            if (leverLight != null)
            {
                leverLight.enabled = false;
            }

            mazeGroupToDisable.SetActive(false); // 미로 제거
            Unpossess();
            q_Key.SetActive(false);

            hasActivated = false;
            MarkActivatedChanged();
        }
        
        q_Key.SetActive(true);
    }
}
