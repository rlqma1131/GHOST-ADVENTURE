using DG.Tweening;
using UnityEngine;

public class Ch1_Plate : BasePossessable
{
    [SerializeField] private AudioClip isShaking;
    [SerializeField] private GameObject q_Key;

    private Ch1_Cat cat;

    protected override void Start()
    {
        base.Start();
        cat = FindObjectOfType<Ch1_Cat>();
    }

    protected override void Update()
    {
        base.Update();

        if (!isPossessed)
        {
            q_Key.SetActive(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            q_Key.SetActive(false);
            TriggerPlateEvent();
        }
        q_Key.SetActive(true);
    }

    private void TriggerPlateEvent()
    {
        Sequence shakeSeq = DOTween.Sequence();
        int shakeCount = 3;
        float startAngle = 5f;
        float durationPerShake = 0.05f;

        SoundManager.Instance.PlaySFX(isShaking);

        for (int i = 0; i < shakeCount; i++)
        {
            float angle = Mathf.Lerp(startAngle, 0f, (float)i / shakeCount);
            shakeSeq.Append(transform.DOLocalRotate(new Vector3(0, 0, angle), durationPerShake))
                    .Append(transform.DOLocalRotate(new Vector3(0, 0, -angle), durationPerShake));
        }

        shakeSeq.Append(transform.DOLocalRotate(Vector3.zero, 0.03f));

        // 고양이는 눈 깜빡이기만
        cat.Blink();
    }
    

}