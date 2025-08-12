
using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraTargetSetter : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;

    void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    void Start()
    {

        StartCoroutine(AssignPlayerWhenReady());
    }


    private IEnumerator AssignPlayerWhenReady()
    {

        yield return new WaitUntil(() => GameManager.Instance != null && GameManager.Instance.Player != null);


        Transform playerTransform = GameManager.Instance.Player.transform;
        virtualCamera.Follow = playerTransform;

        Debug.Log("카메라 타겟이 플레이어로 설정되었습니다!");
    }
}