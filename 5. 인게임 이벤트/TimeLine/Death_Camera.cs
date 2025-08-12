using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Death_Camera : MonoBehaviour
{


    [SerializeField] private CinemachineVirtualCamera vcam;

    public void Oncamera()
    {


        vcam.Priority = 50;
    }
    public void OffCamera()
    {

        vcam.Priority = 0;
    }

}
