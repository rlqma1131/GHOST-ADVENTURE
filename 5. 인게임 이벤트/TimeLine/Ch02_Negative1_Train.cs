using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class Ch02_Negative1_Train : MonoBehaviour
{

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float bounceHeight = 0.2f;
    [SerializeField] private float bounceDuration = 0.3f;



    private bool jump = true;

    void Start()
    {

            Vector3 startPos = transform.position;
        // 들썩임 트윈
        if (jump)
        {
            transform.DOMoveY(startPos.y + bounceHeight, bounceDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

        }



    }

    public void SetJumpFalse()
    {

        jump = false;
    }


}
