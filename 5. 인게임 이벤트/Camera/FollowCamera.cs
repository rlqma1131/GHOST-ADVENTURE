using UnityEngine;
using Cinemachine;

public class FollowCamera : MonoBehaviour
{
    void Start()
    {
        // 씬에 있는 모든 VirtualCamera 찾기
        CinemachineVirtualCamera[] cameras = FindObjectsOfType<CinemachineVirtualCamera>();

        foreach (var cam in cameras)
        {

            if (cam.Follow == null)
            {
                cam.Follow = this.transform;
            }
           
            
        }

        
    }
}
