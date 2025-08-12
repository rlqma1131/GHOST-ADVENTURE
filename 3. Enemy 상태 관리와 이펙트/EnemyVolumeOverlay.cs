using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EnemyVolumeOverlay : MonoBehaviour
{
    public static EnemyVolumeOverlay Instance { get; private set; }

    [Header("Overlay Settings")]
    [SerializeField] private Color overlayColor = new Color(108f/255f, 0f, 0f); // 모든 적 공통 색
    [SerializeField] private float enableEpsilon = 0.001f;  // 이하면 끔
    [SerializeField] private float blendSpeed    = 8f;      // weight 스무딩 속도(선택)

    private Volume overlayVolume;
    private VolumeProfile overlayProfile;
    private ColorAdjustments overlayCA;

    private readonly Dictionary<int, float> intensities = new(); // id -> [0..1]
    private float currentWeight = 0f;
    
    private bool suspended = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        // 원하면 씬 전환마다 새로 만들게 하려면 아래 줄을 지워도 됨
        DontDestroyOnLoad(gameObject);

        overlayVolume = gameObject.AddComponent<Volume>();
        overlayVolume.isGlobal = true;
        overlayVolume.priority = 100f;
        overlayVolume.weight = 0f;
        overlayVolume.enabled = false;

        overlayProfile = ScriptableObject.CreateInstance<VolumeProfile>();
        overlayVolume.profile = overlayProfile;

        overlayCA = overlayProfile.Add<ColorAdjustments>(true);
        overlayCA.colorFilter.overrideState = true;
        overlayCA.colorFilter.value = overlayColor;
    }

    public void SetColor(Color c)
    {
        overlayColor = c;
        if (overlayCA != null) overlayCA.colorFilter.value = c;
    }

    public void Suspend(bool on)
    {
        suspended = on;
        if (on)
        {
            intensities.Clear();
            currentWeight = 0f;
            overlayVolume.weight = 0f;
            overlayVolume.enabled = false;
        }
    }
    
    public void Report(int id, float intensity01)
    {
        if (suspended) return;
        intensity01 = Mathf.Clamp01(intensity01);
        intensities[id] = intensity01;
    }

    public void Clear(int id)
    {
        intensities.Remove(id);
    }

    private void LateUpdate()
    {
        if (suspended)
        {
            if (overlayVolume.enabled) overlayVolume.enabled = false;
            if (overlayVolume.weight != 0f) overlayVolume.weight = 0f;
            currentWeight = 0f;
            return;
        }
        
        float target = 0f;
        foreach (var kv in intensities)
            if (kv.Value > target) target = kv.Value;

        // 스무딩(선택): 개별 적이 서서히 변하면 여기선 빠르게 따라가도 됨
        currentWeight = Mathf.MoveTowards(currentWeight, target, blendSpeed * Time.deltaTime);

        overlayVolume.weight = currentWeight;

        // weight≈0일 때는 꺼서 Volume 스캔 비용 절감
        bool on = currentWeight > enableEpsilon;
        if (overlayVolume.enabled != on) overlayVolume.enabled = on;
    }
}
