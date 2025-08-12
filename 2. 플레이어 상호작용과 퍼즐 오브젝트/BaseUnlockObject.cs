using UnityEngine;

public abstract class BaseUnlockObject : MonoBehaviour
{
    protected Animator anim;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    public abstract void Unlock();

    // 상호작용키 UI 표시
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerInteractSystem.Instance.AddInteractable(gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }
}
