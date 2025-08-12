using UnityEngine;

public class Dust : MonoBehaviour
{
    public GameObject dustEffectPrefab;  // 먼지 파티클 프리팹

    public void PlayDustEffect()
    {
        if (dustEffectPrefab != null)
        {
            Instantiate(dustEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}
