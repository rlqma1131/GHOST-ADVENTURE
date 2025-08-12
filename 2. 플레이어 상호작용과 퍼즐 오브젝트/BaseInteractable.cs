using UnityEngine;

/// <summary>
/// 상호작용키 팝업 기능 구현하는 클래스
/// 어떤 오브젝트와 상호작용할지는 PlayerInteractSystem.cs 에서 관리
/// </summary>
public class BaseInteractable : MonoBehaviour
{
    public GameObject highlight;

    void Start()
    {
        if(highlight != null)
            highlight.SetActive(false);
    }
    
    public void SetHighlight(bool pop)
    {
        if(highlight != null)
            highlight?.SetActive(pop);
    }

    // 은신처일때만 적용 (외에는 각 스크립트에서 override 중)
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameObject.CompareTag("HideArea")) 
        {
            if (collision.CompareTag("Player"))
            {
                //SetHighlight(true);
                PlayerInteractSystem.Instance.AddInteractable(gameObject);
            }
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SetHighlight(false);
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
        }
    }
}
