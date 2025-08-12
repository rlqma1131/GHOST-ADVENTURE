using System.Collections;
using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    [Header("SO 기반 사운드 설정")]
    [SerializeField] private SoundEventConfig soundConfig;
    
    public static void TriggerSound(Vector3 soundPos, float range, float duration, float offsetDistance = 3f)
    {
        EnemyAI[] enemies = GameObject.FindObjectsOfType<EnemyAI>();
        foreach (var enemy in enemies)
        {
            if (enemy != null && !enemy.QTEHandler.IsRunning())
            {
                float distance = Vector3.Distance(enemy.transform.position, soundPos);
                if (distance <= range)
                {
                    enemy.StartSoundTeleport(soundPos, offsetDistance, duration);
                }
            }
        }
        UIManager.Instance.PromptUI.ShowPrompt("으악!!!!!!!", 2f);
    }

    // SO 기반 인스펙터 호출용
    public void TriggerSound()
    {
        if (soundConfig == null)
        {
            Debug.LogWarning($"SoundTrigger: {name}에 SoundEventConfig가 할당되지 않았습니다.");
            return;
        }

        TriggerSound(transform.position, soundConfig.soundRange, soundConfig.chaseDuration);
    }

    // Gizmo로 범위 표시
    private void OnDrawGizmosSelected()
    {
        if (soundConfig == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, soundConfig.soundRange);
    }
}
