using UnityEngine;
using DG.Tweening;

public class ShakePlayer : MonoBehaviour
{

    // 흔들림 강도 (위치 기준)
    public float strength = 0.1f;
    // 흔들림 지속 시간
    public float duration = 0.5f;
    // 흔들림 횟수 (진동 횟수)
    public int vibrato = 20;
    // 흔들림 흔들림 세기 감소 비율 (감쇠)
    public float randomness = 90f;

    private Vector3 originalPosition;

    public float punchStrength = 0.3f;   // 커졌다 작아지는 크기
    public float punchDuration = 0.6f;   // 튀는 시간
    public float vanishDuration = 0.4f;  // 사라지는 시간
    private float originalZ;

    void Start()
    {

    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.X))
        {
            PopAndVanish();
        }
    }
    public void Shake()
    {

        transform.DOShakePosition(duration, strength, vibrato, randomness, false, true);


    }

    public void PopAndVanish()
    {
        // 현재 Z값 저장
        originalZ = transform.localScale.z;
        Debug.Log("Original Z Scale: " + originalZ);
        Sequence seq = DOTween.Sequence();

        // 1. X, Y만 튀듯이 커졌다 작아졌다
        Vector3 punchVec = new Vector3(punchStrength, punchStrength, 0f);
        seq.Append(transform.DOPunchScale(punchVec, punchDuration, 10, 1f));

        // 2. 이후 X, Y만 0으로 줄이고, Z는 유지
        Vector3 finalScale = new Vector3(0f, 0f, originalZ);
        seq.Append(transform.DOScale(finalScale, vanishDuration).SetEase(Ease.InBack));
    }

}
