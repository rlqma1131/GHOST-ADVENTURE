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

    // using UnityEngine;
    // using UnityEngine.UI;
    // using UnityEngine.SceneManagement;
    // using System.Collections;

    // public class ImageZoomTransition : MonoBehaviour
    // {
    //     public RectTransform targetImage;
    //     public CanvasGroup canvasGroup; // ⬅ 페이드 아웃용
    //     public float zoomDuration = 0.5f;
    //     public float fadeDuration = 0.5f;
    //     public string nextSceneName = "NextScene";

    //     private bool isZooming = false;
    //     private Vector3 originalScale;
    //     private Vector3 targetScale;

    //     private void Start()
    //     {
    //         originalScale = targetImage.localScale;
    //         targetScale = Vector3.one * 10f; // 확대 크기 조정
    //         if (canvasGroup != null)
    //             canvasGroup.alpha = 1f;
    //     }

    //     public void OnClickZoomAndLoad()
    //     {
    //         if (!isZooming)
    //             StartCoroutine(ZoomAndFadeOut());
    //     }

    //     private IEnumerator ZoomAndFadeOut()
    //     {
    //         isZooming = true;
    //         float elapsed = 0f;

    //         while (elapsed < zoomDuration)
    //         {
    //             elapsed += Time.deltaTime;
    //             float t = elapsed / zoomDuration;

    //             // 확대
    //             targetImage.localScale = Vector3.Lerp(originalScale, targetScale, t);

    //             // 페이드 아웃 (0.5초 동안 동시에 진행)
    //             if (canvasGroup != null)
    //                 canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

    //             yield return null;
    //         }

    //         targetImage.localScale = targetScale;
    //         if (canvasGroup != null) canvasGroup.alpha = 0f;

    //         // 씬 전환
    //         SceneManager.LoadScene(nextSceneName);
    //     }
    // }
}
