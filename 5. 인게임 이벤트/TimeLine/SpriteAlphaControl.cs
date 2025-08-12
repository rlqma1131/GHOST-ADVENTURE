using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
//using Unity.VisualScripting;

public class SpriteAlphaControl : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] Sprites;

    [SerializeField] private float Alpha;


    private Transform headSetTransform;
    //[SerializeField] private float Duration;

    private void Awake()
    {
        Sprites = transform.GetComponentsInChildren<SpriteRenderer>();
        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {
            if (child.name == "HeadSet")
            {
                headSetTransform = child;
                break;
            }
        }

        if (headSetTransform == null)
        {
            Debug.LogWarning("HeadSet 오브젝트를 찾을 수 없습니다.");
        }

    }

    public void SetAlpha(float alpha)
    {
        foreach (var spriteRenderer in Sprites)
        {
            spriteRenderer.DOFade(alpha, 4);
                
        }
    }

    public void AnimateHeadSet()
    {
        if (headSetTransform == null)
        {
            Debug.LogWarning("HeadSet 트랜스폼이 없습니다.");
            return;
        }

        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < 700; i++)
        {
            float randomZ = Random.Range(-80f, 120f);
            seq.Append(headSetTransform.DORotate(new Vector3(0, 0, randomZ), 0.02f).SetEase(Ease.Linear));
        }
    }

    public void SetBlue()
    {
        foreach (var spriteRenderer in Sprites)
        {
            if (spriteRenderer.name == "bloodtear") continue;  // bloodtear는 건너뜀
            spriteRenderer.color = Color.blue;
        }
    }


    public void OffBloodTear()
    {

        foreach (var spriteRenderer in Sprites)
        {
            if (spriteRenderer.name == "bloodtear")
            {

                spriteRenderer.enabled = false;
            }
            
        }

    }    
    public void OnBloodTear()
    {

        foreach (var spriteRenderer in Sprites)
        {
            if (spriteRenderer.name == "bloodtear")
            {

                spriteRenderer.enabled = true;
            }
            
        }

    }

    public void SetAlphaBloodTear(float alpha)
    {
        foreach (var spriteRenderer in Sprites)
        {

            if (spriteRenderer.name == "bloodtear")
            {

                
            spriteRenderer.DOFade(alpha, 2);
            }

        }
    }



}
