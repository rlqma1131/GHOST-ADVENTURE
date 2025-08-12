using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_PasswordMemo : BasePossessable
{
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    protected override void Update()
    {
        if(!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            UIManager.Instance.PlayModeUI_OpenAll();
            zoomCamera.Priority = 5;
            Unpossess();
        }
    }

    public override void OnPossessionEnterComplete() 
    { 
        UIManager.Instance.PlayModeUI_CloseAll();
        zoomCamera.Priority = 20;
    }
}
