using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ch03_Memory01_Savedata : MemoryFragment
{
    void Start()
    {
        MemoryManager.Instance.TryCollect(data); // 기억 조각 수집
        //Inventory_Player _inventory = GameManager.Instance.Player.GetComponent<Inventory_Player>(); 
        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetMemoryFragmentScannable(uid.Id, isScannable);

        var chapter = DetectChapterFromScene(SceneManager.GetActiveScene().name);
        ChapterEndingManager.Instance.RegisterScannedMemory(data.memoryID, chapter);

        SaveManager.SaveWhenCutScene(data.memoryID, data.memoryTitle,
            SceneManager.GetActiveScene().name,
            //GameManager.Instance.Player.transform.position,
            //checkpointId: data.memoryID,
            autosave: true);

        Debug.Log($"[MemoryFragment] 진행도 저장됨 : {data.memoryID} / {data.memoryTitle}");
    }

}
 