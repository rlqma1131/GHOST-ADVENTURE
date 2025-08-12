using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EnemyVolumeTrigger : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRadius = 15f;

    [Header("State")]
    public bool PlayerInTrigger = false;
    public bool PlayerFind = false;
    public bool Ondead = false;

    private int _id;
    private float t = 0f; // 0..1 보간(강도)

    private void Start()
    {
        _id = GetInstanceID();

        // 플레이어 캐시 1회
        var playerObj = GameManager.Instance != null ? GameManager.Instance.Player : null;
        if (playerObj != null) { player = playerObj.transform; PlayerFind = true; }

        // 매니저 없으면 자동 생성
        if (EnemyVolumeOverlay.Instance == null)
        {
            var go = new GameObject("EnemyVolumeOverlay");
            go.AddComponent<EnemyVolumeOverlay>();
        }
    }

    private void Update()
    {
        // player 참조 보강
        if (!PlayerFind)
        {
            var playerObj = GameManager.Instance != null ? GameManager.Instance.Player : null;
            if (playerObj != null) { player = playerObj.transform; PlayerFind = true; }
        }
        if (player == null || EnemyVolumeOverlay.Instance == null) return;

        // 빙의 중이면 빙의 대상, 아니면 player
        Transform target = null;
        if (PossessionStateManager.Instance != null
            && PossessionStateManager.Instance.IsPossessing()
            && PossessionSystem.Instance != null
            && PossessionSystem.Instance.CurrentTarget != null)
        {
            target = PossessionSystem.Instance.CurrentTarget.transform;
        }
        else
        {
            target = player;
        }
        if (target == null) return;

        bool isDead = (UIManager.Instance != null
                       && UIManager.Instance.QTE_UI_2 != null
                       && UIManager.Instance.QTE_UI_2.isdead);

        float distance = Vector3.Distance(transform.position, target.position);
        bool inRange = !Ondead && !isDead && distance <= detectionRadius;

        // 엣지 사운드 처리만 유지
        if (inRange && !PlayerInTrigger)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.FadeOutAndStopBGM(1f);
                if (SoundManager.Instance.EnemySource != null)
                    SoundManager.Instance.FadeInLoopingSFX(SoundManager.Instance.EnemySource.clip, 1f, 0.5f);
            }
        }
        else if (!inRange && PlayerInTrigger)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.FadeOutAndStopLoopingSFX(1f);
                SoundManager.Instance.RestoreLastBGM(1f);
            }
        }
        PlayerInTrigger = inRange;

        // 강도 계산(가까울수록 강함)
        float targetT = 0f;
        if (inRange)
        {
            float norm = Mathf.Clamp01(1f - (distance / detectionRadius));
            targetT = Mathf.Pow(norm, 0.5f);

            if (SoundManager.Instance != null && SoundManager.Instance.EnemySource != null)
                SoundManager.Instance.EnemySource.volume = Mathf.Lerp(0.01f, 0.3f, targetT);
        }

        // 부드럽게 따라감
        t = Mathf.Lerp(t, targetT, Time.deltaTime * 2f);

        // ✅ 여기서 오버레이 매니저에 “내 강도”만 보고
        EnemyVolumeOverlay.Instance.Report(_id, t);
    }

    private void OnDisable()
    {
        if (EnemyVolumeOverlay.Instance != null)
            EnemyVolumeOverlay.Instance.Clear(_id);
    }

    private void OnDestroy()
    {
        if (EnemyVolumeOverlay.Instance != null)
            EnemyVolumeOverlay.Instance.Clear(_id);
    }
}