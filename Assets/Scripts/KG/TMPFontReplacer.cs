// Assets/Editor/TMPFontReplacer.cs
using UnityEditor;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class TMPFontReplacer : EditorWindow
{
    TMP_FontAsset primary;
    Material materialPreset; // 可留空 → 用 primary 的 default material
    List<TMP_FontAsset> fallbacks = new();

    [MenuItem("Tools/TMP/Font Replacer")]
    static void Open() => GetWindow<TMPFontReplacer>("TMP Font Replacer");

    void OnGUI()
    {
        GUILayout.Label("Primary Font (必填)", EditorStyles.boldLabel);
        primary = (TMP_FontAsset)EditorGUILayout.ObjectField("Primary Font", primary, typeof(TMP_FontAsset), false);
        materialPreset = (Material)EditorGUILayout.ObjectField("Material Preset (可選)", materialPreset, typeof(Material), false);

        GUILayout.Space(6);
        GUILayout.Label("Fallback Fonts (可多個)", EditorStyles.boldLabel);
        DrawFallbackList();

        GUILayout.Space(10);
        using (new EditorGUI.DisabledScope(primary == null))
        {
            if (GUILayout.Button("Replace In Current Scene")) ReplaceInOpenScenes();
            if (GUILayout.Button("Replace In All Prefabs")) ReplaceInAllPrefabs();
        }
    }

    void DrawFallbackList()
    {
        int remove = -1;
        for (int i = 0; i < fallbacks.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            fallbacks[i] = (TMP_FontAsset)EditorGUILayout.ObjectField(fallbacks[i], typeof(TMP_FontAsset), false);
            if (GUILayout.Button("X", GUILayout.Width(22))) remove = i;
            EditorGUILayout.EndHorizontal();
        }
        if (remove >= 0) fallbacks.RemoveAt(remove);
        if (GUILayout.Button("+ Add Fallback")) fallbacks.Add(null);
    }

    void ApplyToText(TMP_Text t)
    {
        if (!t) return;
        Undo.RecordObject(t, "Replace TMP Font");

        t.font = primary;
        // 設材質：未指定就用字型預設材質
        t.fontSharedMaterial = materialPreset ? materialPreset : primary.material;

        // 設置後援鏈（物件層級）
        if (primary.fallbackFontAssetTable == null) primary.fallbackFontAssetTable = new List<TMP_FontAsset>();
        t.font.fallbackFontAssetTable = new List<TMP_FontAsset>(fallbacks);

        EditorUtility.SetDirty(t);
    }

    void ReplaceInOpenScenes()
    {
        var texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        int count = 0;
        foreach (var t in texts)
        {
            // 過濾：只處理場景物件（排除 Prefab 資源）
            if (t && t.gameObject.scene.IsValid())
            {
                ApplyToText(t);
                count++;
            }
        }
        Debug.Log($"[TMPFontReplacer] Scene objects replaced: {count}");
    }

    void ReplaceInAllPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int count = 0;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!root) continue;

            bool changed = false;
            foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
            {
                ApplyToText(t);
                changed = true;
            }
            if (changed)
            {
                PrefabUtility.SavePrefabAsset(root);
                count++;
            }
        }
        Debug.Log($"[TMPFontReplacer] Prefabs updated: {count}");
    }
}
