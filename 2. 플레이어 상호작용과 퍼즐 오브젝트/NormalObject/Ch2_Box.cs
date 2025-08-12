using UnityEngine;
using DG.Tweening;

public class Box : MonoBehaviour
{
    private SpriteRenderer[] renderers;

    private void Start() 
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    // 부딪히면 투명하게
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            foreach (SpriteRenderer sr in renderers)
            {
                sr.DOFade(0.1f, 0.5f);
            }
        }
    }

    // 안부딪히면 불투명하게(복원)
    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            foreach (var sr in renderers)
            {
                sr.DOFade(1f, 0.5f);  
            }
        }
    }
}
