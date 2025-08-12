using UnityEngine;

public class Ch1_Closet : BaseUnlockObject
{
    [SerializeField] private GameObject baseball;

    public override void Unlock()
    {
        anim.SetTrigger("Unlock");
        baseball.SetActive(true);
    }
}
