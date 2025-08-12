using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch02_Negative1_Duck : MonoBehaviour
{
    [SerializeField] private float tiltAngle = 15f;   
    [SerializeField] private float duration = 0.5f;   // 한쪽 방향으로 기울이는 시간

    void Start()
    {
        
        transform.rotation = Quaternion.Euler(0, 0, -tiltAngle);

        // Z축으로 15도,  -15도 사이를 무한 반복하며 왔다 갔다
        transform
            .DORotate(new Vector3(0, 0, tiltAngle), duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
}
