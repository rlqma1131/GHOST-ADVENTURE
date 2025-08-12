using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Ch02_SecurityCollider : MonoBehaviour
{
    [SerializeField] private CinemachineConfiner2D virtualCamera;


    private void Start()
    {
        virtualCamera = GetComponent<CinemachineConfiner2D>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Room"))
        {
            // 플레이어가 경비원 영역에 들어왔을 때 카메라를 변경
            virtualCamera.m_BoundingShape2D = collision.GetComponent<PolygonCollider2D>();

        }
    }



}
