using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ch03_EndMemory : MemoryFragment
{
    bool activeCutscene = false;

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !activeCutscene)
        {
            CutsceneManager.Instance.StartCoroutine(CutsceneManager.Instance.PlayCutscene());
            activeCutscene = true;
            PossessionSystem.Instance.CanMove = false;
            UIManager.Instance.PlayModeUI_CloseAll();
            EnemyAI.PauseAllEnemies();
            StartCoroutine(LoadNextSceneAfterDelay(3f));
        }
    }

    private IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Inventory_Player _inventory = GameManager.Instance.Player.GetComponent<Inventory_Player>();
        MemoryManager.Instance.TryCollect(data);
        SoundManager.Instance.FadeOutAndStopLoopingSFX();
        SceneManager.LoadScene("Ch03_End", LoadSceneMode.Additive);

        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetMemoryFragmentScannable(uid.Id, isScannable);

        var chapter = DetectChapterFromScene(SceneManager.GetActiveScene().name);
        ChapterEndingManager.Instance.RegisterScannedMemory(data.memoryID, chapter);

        SaveManager.SaveWhenScanAfter(data.memoryID, data.memoryTitle,
            SceneManager.GetActiveScene().name,
            GameManager.Instance.Player.transform.position,
            checkpointId: data.memoryID,
            autosave: true);

        Debug.Log($"[MemoryFragment] 진행도 저장됨 : {data.memoryID} / {data.memoryTitle}");
    }
}
