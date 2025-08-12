#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class ExampleSceneLink : MonoBehaviour
{
#if UNITY_EDITOR
    public SceneAsset memoryConnectScene;
#endif

    [HideInInspector] public string memoryConnectSceneName;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (memoryConnectScene != null)
        {
            memoryConnectSceneName = memoryConnectScene.name;
        }
    }
#endif
}
// using UnityEditor.SearchService;
// using UnityEngine;

[CreateAssetMenu(menuName = "Memory/MemoryData")]
public class MemoryData : ScriptableObject
{
    public enum MemoryType { Positive, Negative, Fake }
    public enum Chapter { Chapter1, Chapter2, Chapter3, Chapter4 }

    [Header("기억 타입")]
    public Chapter chapter;
    public MemoryType type;
    public bool isCorrectAnswer;

    [Header("기억 내용")]
    public string memoryID;

    // 스캔오브젝트 스프라이트
    public Sprite MemoryObjectSprite;
    
    // 기억저장소 컷씬이미지
    public Sprite MemoryCutSceneImage;

    // 씬 - 씬 인스펙터창에서 연결하면 memoryConnectSceneName에 씬 이름 저장됨
#if UNITY_EDITOR
    public SceneAsset memoryConnectScene;
#endif
    [HideInInspector] public string memoryConnectSceneName;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (memoryConnectScene != null)
        {
            memoryConnectSceneName = memoryConnectScene.name;
        }
    }
#endif

    // 스캔 후 드랍하는 조각 스프라이트
    public Sprite PositiveFragmentSprite;
    public Sprite NegativeFragmentSprite;
    public Sprite FakeFragmentSprite;
    public string CutSceneName;
    // 갖고 있는 기억은 나중에
    //public Sprite memoryImage;       
    public string memoryTitle;

    [TextArea(3, 10)]
    public string memoryDescription;

    public int soulRecovery = 0;
}
