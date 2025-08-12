using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class Ch02_Negative1_Car : MonoBehaviour
{
    [SerializeField] private float moveDistance = 5f;
    [SerializeField] private float moveDuration = 2f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float bounceHeight = 0.2f;
    [SerializeField] private float bounceDuration = 0.3f;

    [SerializeField] private Light2D light1; 
    [SerializeField] private Light2D light2;

    private bool facingRight = true;

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // 라이트 초기 상태 설정
        if (light1 != null) light1.enabled = true;
        if (light2 != null) light2.enabled = false;

        // 위치 저장
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x + moveDistance, startPos.y, startPos.z);

        // 좌우 이동 트윈
        transform.DOMoveX(endPos.x, moveDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .OnStepComplete(FlipRotation);

        // 들썩임 트윈
        transform.DOMoveY(startPos.y + bounceHeight, bounceDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        // 처음 바라보는 방향 설정
        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        facingRight = true;

        // 라이트 코루틴 시작
        StartCoroutine(LightToggleRoutine());
    }

    void FlipRotation()
    {
        facingRight = !facingRight;
        float yRotation = facingRight ? 180f : 0;
        transform.rotation = Quaternion.Euler(0f, yRotation, transform.rotation.eulerAngles.z);
    }

    IEnumerator LightToggleRoutine()
    {
        while (true)
        {
            if (light1 != null && light2 != null)
            {
                bool isLight1On = light1.enabled;
                light1.enabled = !isLight1On;
                light2.enabled = isLight1On;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
