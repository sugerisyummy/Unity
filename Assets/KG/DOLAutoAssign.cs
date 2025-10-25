// Assets/Editor/DOLAutoAssign.cs
#if UNITY_EDITOR
using UnityEditor; using UnityEngine; using System.Linq; using System.Reflection;

public static class DOLAutoAssign
{
    [MenuItem("Tools/DOL/Assign CaseDatabase To GameManager (Patched)")]
    static void Assign()
    {
        var db = AssetDatabase.FindAssets("t:CaseDatabase")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(p => AssetDatabase.LoadAssetAtPath<CaseDatabase>(p))
            .FirstOrDefault();
        if (db == null) { Debug.LogError("專案沒有任何 CaseDatabase 資產。"); return; }

        var gm = Object.FindAnyObjectByType<GameManager>() ?? Object.FindObjectOfType<GameManager>();
        if (gm == null) { Debug.LogError("場景找不到 GameManager。"); return; }

        var t = gm.GetType();
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var f = t.GetField("caseDatabase", flags) ?? t.GetField("caseDB", flags);

        if (f != null)
        {
            Undo.RecordObject(gm, "Assign CaseDatabase");
            f.SetValue(gm, db);
            EditorUtility.SetDirty(gm);
            Debug.Log($"已用反射指派到欄位 {f.Name} → {AssetDatabase.GetAssetPath(db)}");
            return;
        }

        // SerializedObject 回退
        var so = new SerializedObject(gm);
        var prop = so.FindProperty("caseDatabase") ?? so.FindProperty("caseDB");
        if (prop == null) { Debug.LogError("找不到 caseDatabase / caseDB 欄位。"); return; }
        Undo.RecordObject(gm, "Assign CaseDatabase");
        prop.objectReferenceValue = db;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(gm);
        Debug.Log($"已用 SerializedObject 指派到 {prop.propertyPath} → {AssetDatabase.GetAssetPath(db)}");
    }
}
#endif
