using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

public enum MemoryState
{
    None,
    Selected,
    Correct,
    Wrong
}

public class MemoryNode : MonoBehaviour
{
    [Header("퍼즐 선택시 켜짐")]
    [SerializeField] private Image overlay;

    public RectTransform targetImage;
    public CanvasGroup canvasGroup; // ⬅ 페이드 아웃용
    public float zoomDuration = 0.5f;
    public float fadeDuration = 0.5f;
    
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI memoryName;
    private string sceneName;

    public MemoryData memory;

    public object Data { get; internal set; }

    public void Initialize(MemoryData memoryData)
    {
        memory = memoryData;
        icon.sprite = memory.MemoryCutSceneImage;
        memoryName.text = memory.memoryTitle;
        sceneName = memory.CutSceneName;
    }

    public void GoToCutScene()
    {
        Debug.Log("씬 다시보기 버튼클릭");
        UIManager.Instance.PlayModeUI_CloseAll();
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        EnemyAI.PauseAllEnemies();
    }

    public void SetStateEffect(MemoryState state)
    {
        if (overlay == null) return;

        overlay.enabled = true;

        switch (state)
        {
            case MemoryState.Selected:
                overlay.color = new Color(1f, 1f, 1f, 0.2f); break;
            case MemoryState.Correct:
                overlay.color = new Color(0f, 1f, 0f, 0.4f); break;
            case MemoryState.Wrong:
                overlay.color = new Color(1f, 0f, 0f, 0.4f); break;
            case MemoryState.None:
                overlay.enabled = false; break;
        }
    }
}
