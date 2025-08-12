#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

/// <summary>
/// 기억조각 생성 및 관리용 에디터 윈도우입니다.
/// </summary>

public class MemoryEditorWindow : EditorWindow
{
    private Vector2 scroll;
    private string searchKeyword = "";
    private MemoryData.MemoryType filterType = (MemoryData.MemoryType)(-1);

    [MenuItem("Tools/Memory Editor")]
    public static void OpenWindow()
    {
        GetWindow<MemoryEditorWindow>("Memory Editor");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("New Memory Data", GUILayout.Height(30)))
        {
            CreateNewMemoryData();
        }
        GUILayout.Space(10);
        searchKeyword = EditorGUILayout.TextField("Search", searchKeyword);
        filterType = (MemoryData.MemoryType)EditorGUILayout.EnumPopup("Filter", filterType);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        scroll = EditorGUILayout.BeginScrollView(scroll);

        var allMemoryData = AssetDatabase.FindAssets("t:MemoryData")
            .Select(guid => AssetDatabase.LoadAssetAtPath<MemoryData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(data =>
                (string.IsNullOrEmpty(searchKeyword) || data.memoryTitle.ToLower().Contains(searchKeyword.ToLower())) &&
                (filterType < 0 || data.type == filterType)
            );

        foreach (var data in allMemoryData)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"ID: {data.memoryID} | Type: {data.type}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Title: {data.memoryTitle}");

            if (GUILayout.Button("Select"))
            {
                Selection.activeObject = data;
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    private void CreateNewMemoryData()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Memory Data", "NewMemory", "asset", "Create new memory data");
        if (string.IsNullOrEmpty(path)) return;

        var newMemory = ScriptableObject.CreateInstance<MemoryData>();
        newMemory.memoryID = System.Guid.NewGuid().ToString();
        AssetDatabase.CreateAsset(newMemory, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newMemory;
    }
}
#endif
