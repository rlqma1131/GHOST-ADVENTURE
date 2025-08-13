using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryManager : MonoBehaviour
{
    public static MemoryManager Instance { get; private set; }

    // 저장과 동일하게 "ID" 리스트로 관리
    public List<string> collectedMemoryIDs = new();

    // id → data, title → data (호환용)
    private readonly Dictionary<string, MemoryData> byId = new();
    private readonly Dictionary<string, MemoryData> byTitle = new();

    private readonly List<MemoryData> scannedMemoryList = new();
    public IReadOnlyList<MemoryData> ScannedMemories => scannedMemoryList;

    public event Action<MemoryData> OnMemoryCollected;
    public Button closeMemoryStorage;

    // SaveManager.Loaded 구독/해제용 델리게이트(람다 대신 메서드)
    private void OnSaveLoaded(SaveData _) => WarmStartFromSave();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        LoadAllMemoryData();

        // 저장 로드 직후 복원
        SaveManager.Loaded += OnSaveLoaded;
        WarmStartFromSave(); // 이미 로드된 상태일 수 있어 한 번 더 시도

        closeMemoryStorage?.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        SaveManager.Loaded -= OnSaveLoaded;
    }

#if UNITY_EDITOR
    // 에디터 디버그: T 키로 모든 MemoryData 수집
    [SerializeField] private bool debugSaveAndRefresh = true;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[디버그] 모든 MemoryData 수집 시도");

            foreach (var m in byId.Values)
                TryCollect(m);

            Debug.Log($"[디버그] 총 수집된 기억 개수: {collectedMemoryIDs.Count}");

            if (debugSaveAndRefresh)
            {
                var data = SaveManager.CurrentData;
                if (data != null)
                {
                    data.collectedMemoryIDs ??= new List<string>();

                    foreach (var id in collectedMemoryIDs)
                        if (!data.collectedMemoryIDs.Contains(id))
                            data.collectedMemoryIDs.Add(id);

                    SaveManager.SaveGame(data);
                }
                else
                {
                    Debug.LogWarning("[디버그] SaveManager.CurrentData 가 없음: 저장 반영 생략");
                }
            }
        }
    }
#endif

    private void LoadAllMemoryData()
    {
        var all = Resources.LoadAll<MemoryData>("MemoryData");
        byId.Clear();
        byTitle.Clear();

        foreach (var m in all)
        {
            if (!string.IsNullOrEmpty(m.memoryID) && !byId.ContainsKey(m.memoryID))
                byId.Add(m.memoryID, m);

            if (!string.IsNullOrEmpty(m.memoryTitle) && !byTitle.ContainsKey(m.memoryTitle))
                byTitle.Add(m.memoryTitle, m);
        }
    }

    // 수집은 ID 기준으로 중복 방지
    public void TryCollect(MemoryData m)
    {
        if (m == null || string.IsNullOrEmpty(m.memoryID)) return;
        if (collectedMemoryIDs.Contains(m.memoryID)) return;

        collectedMemoryIDs.Add(m.memoryID);
        if (!scannedMemoryList.Contains(m)) scannedMemoryList.Add(m);

        OnMemoryCollected?.Invoke(m);
    }

    // 저장값으로 런타임 상태 복원
    public void WarmStartFromSave()
    {
        var data = SaveManager.CurrentData;
        if (data == null) return;

        collectedMemoryIDs.Clear();
        scannedMemoryList.Clear();

        // 1) 저장된 ID들로 복원 (정상 경로)
        if (data.collectedMemoryIDs != null)
        {
            foreach (var id in data.collectedMemoryIDs)
                if (byId.TryGetValue(id, out var m)) TryCollect(m);
        }

        // 2) 예전 세이브(제목만 저장) 호환
        if (data.scannedMemoryTitles != null)
        {
            foreach (var title in data.scannedMemoryTitles)
                if (byTitle.TryGetValue(title, out var m)) TryCollect(m);
        }
    }

    public List<MemoryData> GetCollectedMemories()
    {
        var result = new List<MemoryData>();
        foreach (var id in collectedMemoryIDs)
            if (byId.TryGetValue(id, out var m)) result.Add(m);
        return result;
    }

    // === ChapterEndingManager가 챕터 추정을 위해 쓰는 헬퍼 ===
    public bool TryGetById(string id, out MemoryData data) => byId.TryGetValue(id, out data);

    public bool IsCanStore(MemoryData data) => scannedMemoryList.Contains(data);

    public void OpenMemoryStorage()
    {
        UIManager.Instance.MemoryStorageUI.gameObject.SetActive(true);
        if (closeMemoryStorage != null) closeMemoryStorage.gameObject.SetActive(true);
    }

    public void CloseMemoryStorage()
    {
        UIManager.Instance.MemoryStorageUI.gameObject.SetActive(false);
        if (closeMemoryStorage != null) closeMemoryStorage.gameObject.SetActive(false);
    }

    public void PrintScannedDebugLog()
    {
        Debug.Log("== [MemoryManager] 스캔된 기억 목록 ==");
        foreach (var m in scannedMemoryList)
            Debug.Log($"- {m.memoryID}: {m.memoryTitle}");
    }

    public void TryCollectAll(List<MemoryData> list)
    {
        foreach (var m in list) TryCollect(m);
    }

    public void ClearScannedDebug()
    {
        scannedMemoryList.Clear();
        collectedMemoryIDs.Clear();
        Debug.Log("[MemoryManager] 스캔 기록 초기화됨");
    }
}
