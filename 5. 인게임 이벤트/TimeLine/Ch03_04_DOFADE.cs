using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch03_04_DOFADE : MonoBehaviour
{
    // Start is called before the first frame update
    public SpriteRenderer targetSprite; // 알파값 조절할 대상
    public float fadeDuration = 1f;     // 페이드 시간

    private void Start()
    {
        // 시작할 때 투명하게
        if (targetSprite != null)
        {
            Color c = targetSprite.color;
            c.a = 0f;
            targetSprite.color = c;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && targetSprite != null)
        {
            // 알파값 0 → 1로 서서히
            targetSprite.DOFade(1f, fadeDuration);
        }
    }
}
