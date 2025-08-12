using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch03_04BG_In : MonoBehaviour
{

    [SerializeField]private DissolveController dissolveController;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 플레이어가 트리거에 들어오면 디졸브 효과 시작
            dissolveController.In();
        }
    }
}
