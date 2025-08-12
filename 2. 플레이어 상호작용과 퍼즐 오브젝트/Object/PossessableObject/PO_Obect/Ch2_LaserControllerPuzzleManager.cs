using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_LaserControllerPuzzleManager : MonoBehaviour
{
    public Ch2_LaserController[] laserControllers;

    private bool firstDeactivationOccurred = false;

    private void Start()
    {
        foreach (var controller in laserControllers)
        {
            controller.OnLaserDeactivated += HandleLaserDeactivation;
        }
    }

    private void HandleLaserDeactivation(Ch2_LaserController deactivated)
    {
        if (!firstDeactivationOccurred)
        {
            firstDeactivationOccurred = true;
            laserInActiveFirst();
        }

        // 전부 다 꺼졌는지 확인
        bool allInactive = true;
        foreach (var controller in laserControllers)
        {
            if (controller.IsLaserActive)
            {
                allInactive = false;
                break;
            }
        }

        if (allInactive)
        {
            laserInActiveAll();
        }
    }

    private void laserInActiveFirst()
    {
        Debug.Log("첫 번째 레이저가 꺼졌습니다.");
        UIManager.Instance.PromptUI.ShowPrompt("하나 껐어… 같은 장치들을 찾아야 해.", 2f);
    }

    private void laserInActiveAll()
    {
        UIManager.Instance.PromptUI.ShowPrompt("불빛이 모두 꺼졌어. 이제 길이 열렸을까…", 3f);
    }
}
