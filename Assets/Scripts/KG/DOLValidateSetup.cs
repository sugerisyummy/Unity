// Assets/Editor/DOLValidateSetup.cs
#if UNITY_EDITOR
using UnityEditor; using UnityEngine;
using System.Linq; using System.Reflection; using System.IO;

public static class DOLValidateSetup
{
    [MenuItem("Tools/DOL/Quick Validate (Patched)")]
    static void Validate()
    {
        // 1) 列出疑似壞掉的 DolEventAsset（若有）
        var paths = AssetDatabase.GetAllAssetPaths().Where(p => p.EndsWith(".asset"));
        int broken = 0;
        foreach (var p in paths)
        {
            var ok = AssetDatabase.LoadAssetAtPath<DolEventAsset>(p) != null;
            if (!ok)
            {
                var txt = File.Exists(p) ? File.ReadAllText(p) : "";
                if (txt.Contains("DolEventAsset") && txt.Contains("m_Script"))
                { Debug.LogWarning($"[Broken DolEventAsset] {p}"); broken++; }
            }
        }
        Debug.Log($"Broken DolEventAsset count: {broken}");

        // 2) 檢查場景裡的 GameManager 是否綁到 CaseDatabase
        var gm = Object.FindAnyObjectByType<GameManager>() ?? Object.FindObjectOfType<GameManager>();
        if (gm == null) { Debug.LogError("場景找不到 GameManager。"); return; }

        var t = gm.GetType(); // 用實際元件型別，避免命名空間不合
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var f = t.GetField("caseDatabase", flags) ?? t.GetField("caseDB", flags);

        Object dbObj = null;
        if (f != null) dbObj = f.GetValue(gm) as Object;
        else
        {
            // 走 SerializedObject（私有序列化欄位）
            var so = new SerializedObject(gm);
            var prop = so.FindProperty("caseDatabase") ?? so.FindProperty("caseDB");
            if (prop != null) dbObj = prop.objectReferenceValue;
            else Debug.LogWarning("找不到欄位：caseDatabase / caseDB（請確認欄位名稱）。");
        }

        if (dbObj == null) Debug.LogError("GameManager 的 CaseDatabase 未指派。");
        else Debug.Log($"GameManager 已指派 CaseDatabase：{AssetDatabase.GetAssetPath(dbObj)}");
    }
}
#endif
