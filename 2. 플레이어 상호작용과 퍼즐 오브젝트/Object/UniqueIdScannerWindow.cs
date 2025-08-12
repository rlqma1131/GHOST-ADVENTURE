// Assets/Editor/UniqueIdScannerWindow.cs
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UniqueIdScannerWindow : EditorWindow
{
    [MenuItem("Tools/UniqueId Utility")]
    public static void Open() => GetWindow<UniqueIdScannerWindow>("UniqueId Utility");

    private Vector2 _scroll;
    private List<GameObject> _report = new();
    private bool _includeInactive = true;

    void OnGUI()
    {
        GUILayout.Label("Scan & Fix UniqueId", EditorStyles.boldLabel);
        _includeInactive = EditorGUILayout.ToggleLeft("Include Inactive Objects", _includeInactive);
        EditorGUILayout.Space();

        if (GUILayout.Button("Scan CURRENT Scene"))
            ScanCurrentScene();

        if (GUILayout.Button("Fix CURRENT Scene (Add UniqueId where missing)"))
            FixCurrentScene();

        EditorGUILayout.Space();
        if (GUILayout.Button("Scan ALL Scenes In Build"))
            ScanAllScenesInBuild(false);

        if (GUILayout.Button("Fix ALL Scenes In Build"))
            ScanAllScenesInBuild(true);

        EditorGUILayout.Space();
        if (GUILayout.Button("Scan ALL Prefabs In Project"))
            ScanAllPrefabs(false);

        if (GUILayout.Button("Fix ALL Prefabs In Project"))
            ScanAllPrefabs(true);

        EditorGUILayout.Space();
        GUILayout.Label($"Found (no UniqueId): {_report.Count}", EditorStyles.miniBoldLabel);
        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(140));
        foreach (var go in _report.Where(x => x != null))
        {
            EditorGUILayout.ObjectField(go, typeof(GameObject), true);
        }
        EditorGUILayout.EndScrollView();
    }

    // ---------- SCAN/FIX: CURRENT SCENE ----------
    void ScanCurrentScene() => _report = FindTargetsInScene(SceneManager.GetActiveScene(), onlyMissingUniqueId: true, includeInactive: _includeInactive);

    void FixCurrentScene()
    {
        var scene = SceneManager.GetActiveScene();
        var targets = FindTargetsInScene(scene, onlyMissingUniqueId: true, includeInactive: true);
        int fixedCount = 0;

        foreach (var go in targets)
        {
            if (EnsureUniqueId(go))
                fixedCount++;
        }

        if (fixedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log($"[UniqueId] Current Scene fixed: {fixedCount}");
        ScanCurrentScene();
    }

    // ---------- SCAN/FIX: ALL SCENES IN BUILD ----------
    void ScanAllScenesInBuild(bool autoFix)
    {
        _report.Clear();
        int fixedCount = 0;
        var sceneSetup = EditorSceneManager.GetSceneManagerSetup(); // remember open scenes

        try
        {
            foreach (var s in EditorBuildSettings.scenes.Where(x => x.enabled))
            {
                var scene = EditorSceneManager.OpenScene(s.path, OpenSceneMode.Single);
                var targets = FindTargetsInScene(scene, onlyMissingUniqueId: true, includeInactive: true);
                _report.AddRange(targets);

                if (autoFix && targets.Count > 0)
                {
                    foreach (var go in targets)
                        if (EnsureUniqueId(go)) fixedCount++;

                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
        }
        finally
        {
            // restore previous scene setup
            if (sceneSetup != null && sceneSetup.Length > 0)
                EditorSceneManager.RestoreSceneManagerSetup(sceneSetup);
        }

        if (autoFix)
            Debug.Log($"[UniqueId] All Scenes fixed: {fixedCount}");

        Repaint();
    }

    // ---------- SCAN/FIX: ALL PREFABS ----------
    void ScanAllPrefabs(bool autoFix)
    {
        _report.Clear();
        int fixedCount = 0;

        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // 안전하게 Prefab 편집: LoadPrefabContents → Save → Unload
            var root = PrefabUtility.LoadPrefabContents(path);
            if (root == null) continue;

            var targets = FindTargetsInHierarchy(root, onlyMissingUniqueId: true, includeInactive: true);
            _report.AddRange(targets.Select(t => t.gameObject));

            if (autoFix && targets.Count > 0)
            {
                foreach (var t in targets)
                    if (EnsureUniqueId(t.gameObject)) fixedCount++;

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        if (autoFix)
            Debug.Log($"[UniqueId] Prefabs fixed: {fixedCount}");

        Repaint();
    }

    // ---------- CORE FINDERS ----------
    static List<GameObject> FindTargetsInScene(Scene scene, bool onlyMissingUniqueId, bool includeInactive)
    {
        var list = new List<GameObject>();
        foreach (var root in scene.GetRootGameObjects())
        {
            var targets = FindTargetsInHierarchy(root, onlyMissingUniqueId, includeInactive)
                            .Select(c => c.gameObject);
            list.AddRange(targets);
        }
        return list.Distinct().ToList();
    }

    static List<Component> FindTargetsInHierarchy(GameObject root, bool onlyMissingUniqueId, bool includeInactive)
    {
        // 대상: BasePossessable / MemoryFragment / BaseDoor(= LockedDoor, OpenDoor 등 포함)
        var possessables = root.GetComponentsInChildren(typeof(BasePossessable), includeInactive);
        var fragments = root.GetComponentsInChildren(typeof(MemoryFragment), includeInactive);
        var doors = root.GetComponentsInChildren(typeof(BaseDoor), includeInactive); // ★ 추가

        var all = new List<Component>();
        all.AddRange(possessables);
        all.AddRange(fragments);
        all.AddRange(doors); // ★ 추가

        if (onlyMissingUniqueId)
            all = all.Where(c => c != null && c.GetComponent<UniqueId>() == null).ToList();

        return all;
    }

    // ---------- ADD UniqueId + Assign GUID ----------
    static bool EnsureUniqueId(GameObject go)
    {
        if (go == null) return false;

        var uid = go.GetComponent<UniqueId>();
        if (uid == null)
        {
            uid = Undo.AddComponent<UniqueId>(go); // allow undo
            // 직접 id 세팅 (OnValidate 의존 X)
            var so = new SerializedObject(uid);
            var sp = so.FindProperty("id");
            sp.stringValue = Guid.NewGuid().ToString("N");
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(go);
            return true;
        }

        // 이미 있는데 빈 값이면 채워줌
        {
            var so = new SerializedObject(uid);
            var sp = so.FindProperty("id");
            if (string.IsNullOrEmpty(sp.stringValue))
            {
                sp.stringValue = Guid.NewGuid().ToString("N");
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(go);
                return true;
            }
        }
        return false;
    }
}
#endif
