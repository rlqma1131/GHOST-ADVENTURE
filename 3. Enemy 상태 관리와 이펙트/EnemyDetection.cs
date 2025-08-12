using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [Header("Vision (dual cone)")]
    public float frontRange = 5f;
    [Range(0,360)] public float frontAngle = 80f;
    public float backRange = 4f;
    [Range(0,360)] public float backAngle = 40f;

    Transform tr;

    void Awake() => tr = transform.root;

    /// <summary>전/후 콘으로 '시야' 판정</summary>
    public bool IsInVision(Transform target)
    {
        if (!target || tr == null) return false;

        Vector2 to = (target.position - tr.position);
        float dist = to.magnitude;
        if (dist < Mathf.Epsilon) return true;

        Vector2 dir = to / dist;
        Vector2 fwd = tr.right * Mathf.Sign(tr.localScale.x);

        // 전방
        float angF = Vector2.Angle(fwd, dir);
        if (dist <= frontRange && angF <= frontAngle * 0.5f) return true;

        // 후방
        float angB = Vector2.Angle(-fwd, dir);
        if (dist <= backRange && angB <= backAngle * 0.5f) return true;

        return false;
    }
    
    public bool CanSeePlayer()
    {
        var player = GameManager.Instance != null ? GameManager.Instance.Player : null;
        if (player == null) return false;
        return IsInVision(player.transform);
    }

    /// <summary>각도 무시, 반지름만(사운드 추격 등 전방위)</summary>
    public bool IsWithinRadius(Transform target, float radius)
    {
        if (!target || tr == null) return false;
        return Vector2.Distance(tr.position, target.position) <= radius;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!tr) tr = transform.root;
        Vector2 fwd = tr.right * Mathf.Sign(tr.localScale.x);

        DrawCone(tr.position, fwd,  frontRange, frontAngle, Color.yellow);          // 전방
        DrawCone(tr.position, -fwd, backRange,  backAngle,  new Color(1,0.5f,0));   // 후방
    }

    void DrawCone(Vector3 o, Vector2 dir, float range, float angle, Color c)
    {
        #if UNITY_EDITOR
        UnityEditor.Handles.color = c;
        UnityEditor.Handles.DrawWireDisc(o, Vector3.forward, range);
        float h = angle * 0.5f;
        Vector3 L = Quaternion.Euler(0,0, h) * dir;
        Vector3 R = Quaternion.Euler(0,0,-h) * dir;
        UnityEditor.Handles.DrawLine(o, o + L * range);
        UnityEditor.Handles.DrawLine(o, o + R * range);
        #endif
    }
#endif
}
