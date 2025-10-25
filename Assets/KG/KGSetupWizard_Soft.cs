// KGSetupWizard_Soft.cs
// - Anchor-free（自動反射讀取名為 KGAnchor 的元件，如有）
// - 支援 value：鍵名 / 絕對場景路徑 / $Key/relative 相對鍵路徑
// - 設定檔：Assets/KG/KGSetupProfile.json
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System;

public class KGSetupWizard_Soft : EditorWindow
{
    const string PROFILE_PATH = "Assets/KG/KGSetupProfile.json";

    [MenuItem("KG/Project/One-Click Setup (Soft)")]
    public static void Open() => GetWindow<KGSetupWizard_Soft>("KG Soft Setup");

    Vector2 _scroll;
    StringBuilder _log = new StringBuilder();
    KGSoftProfile _profile;

    void OnEnable() => _profile = KGSoftProfile.Load(PROFILE_PATH, (s)=>_log.AppendLine(s));

    void OnGUI()
    {
        GUILayout.Label("一鍵接線（Soft，不鎖死；支援 $Key/relative）", EditorStyles.boldLabel);
        if (GUILayout.Button("Run", GUILayout.Height(28))) { _log.Clear(); Run(); }
        if (GUILayout.Button("Open Profile")) { Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(PROFILE_PATH); }

        GUILayout.Space(6);
        _scroll = GUILayout.BeginScrollView(_scroll);
        GUILayout.TextArea(_log.ToString(), GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
    }

    void Run()
    {
        if (_profile == null) _profile = KGSoftProfile.Load(PROFILE_PATH, (s)=>_log.AppendLine(s));

        int changes = 0;
        _log.AppendLine("== Objects ==");
        var resolved = ResolveObjects(_profile);

        _log.AppendLine("\n== Field Mapping ==");
        foreach (var m in _profile.fieldMap) changes += ApplyFieldMap(m, resolved);

        _log.AppendLine("\n== UnityEvents ==");
        foreach (var e in _profile.events) changes += ApplyEventMap(e, resolved);

        var outPath = "Assets/KG_SetupReport.txt";
        File.WriteAllText(outPath, _log.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("KG Soft Setup", $"完成，變更 {changes} 項。\n請看 {outPath}", "OK");
    }

    // ---------- Resolve ----------
    Dictionary<string, UnityEngine.Object> ResolveObjects(KGSoftProfile p)
    {
        var dict = new Dictionary<string, UnityEngine.Object>();

        // 1) 反射尋找名為 KGAnchor 的組件（有就用，沒有也能跑）
        foreach (var mb in UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true))
        {
            if (mb == null) continue;
            var t = mb.GetType();
            if (t.Name != "KGAnchor") continue;
            var keyField = t.GetField("key", BindingFlags.Public | BindingFlags.Instance);
            var key = keyField != null ? (keyField.GetValue(mb) as string) : null;
            if (!string.IsNullOrEmpty(key) && !dict.ContainsKey(key))
            {
                dict[key] = mb.gameObject;
                _log.AppendLine($"[Anchor] {key} → {mb.gameObject.name}");
            }
        }

        // 2) Transform 路徑（objects 表）
        foreach (var kv in p.objects)
        {
            if (dict.ContainsKey(kv.Key)) continue;
            var t = FindByPath(kv.Value);
            if (t) { dict[kv.Key] = t.gameObject; _log.AppendLine($"[Path] {kv.Key} → {kv.Value}"); }
            else
            {
                var go = CreateByPath(kv.Value);
                if (go) { dict[kv.Key] = go; _log.AppendLine($"[Create] {kv.Key} → {kv.Value}"); }
            }
        }

        // 3) UI 快速解析（ui 表允許直接寫 $Key/relative 或 絕對路徑）
        foreach (var kv in p.ui)
        {
            if (dict.ContainsKey(kv.Key)) continue;
            var obj = ResolveValueFlex(kv.Value, dict);
            if (obj != null) dict[kv.Key] = obj;
        }

        return dict;
    }

    // 支援：鍵名 / 絕對路徑 / $Key/relative
    UnityEngine.Object ResolveValueFlex(string value, Dictionary<string, UnityEngine.Object> dict)
    {
        if (string.IsNullOrEmpty(value)) return null;

        // 0) 直接鍵
        if (dict.TryGetValue(value, out var direct)) return direct;

        // 1) $Key/relative
        if (value[0] == '$')
        {
            int slash = value.IndexOf('/');
            string key = (slash >= 0) ? value.Substring(1, slash - 1) : value.Substring(1);
            string rel = (slash >= 0) ? value.Substring(slash + 1) : string.Empty;
            if (dict.TryGetValue(key, out var baseObj))
            {
                Transform t = null;
                if (baseObj is GameObject g) t = g.transform;
                else if (baseObj is Component c) t = c.transform;
                if (t)
                {
                    var found = string.IsNullOrEmpty(rel) ? t : t.Find(rel);
                    if (found) return found.GetComponent<Slider>() ?? (UnityEngine.Object)found.gameObject;
                }
            }
        }

        // 2) 絕對路徑
        if (value.Contains("/"))
        {
            var t = FindByPath(value);
            if (t) return t.GetComponent<Slider>() ?? (UnityEngine.Object)t.gameObject;
        }

        return null;
    }

    Transform FindByPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        // 從所有 Canvas 起點找
        foreach (var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var t = canvas.transform.root.Find(path) ?? canvas.transform.Find(path);
            if (t) return t;
        }
        // 從場景根找
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var r in roots)
        {
            var t = r.transform.Find(path);
            if (t) return t;
        }
        // 最後直接名稱查找
        var go = GameObject.Find(path);
        return go ? go.transform : null;
    }

    GameObject CreateByPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        var parts = path.Split('/');
        Transform parent = null;
        GameObject last = null;
        foreach (var name in parts)
        {
            var exist = (parent ? parent.Find(name) : null);
            if (!exist)
            {
                last = new GameObject(name);
                if (parent) last.transform.SetParent(parent, false);
                parent = last.transform;
            }
            else
            {
                parent = exist;
                last = exist.gameObject;
            }
        }
        return last;
    }

    // ---------- Apply ----------
    int ApplyFieldMap(KGSoftProfile.FieldMap m, Dictionary<string, UnityEngine.Object> resolved)
    {
        int changes = 0;
        var host = FindByTypeName(m.host);
        if (!host) { _log.AppendLine($"✗ Host {m.host} 不在場景"); return 0; }

        var val = ResolveValueFlex(m.value, resolved);
        if (val == null) { _log.AppendLine($"✗ Value {m.value} 未解析"); return 0; }

        var comp = host;
        var f = comp.GetType().GetField(m.field, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
        if (f == null) { _log.AppendLine($"✗ {m.host}.{m.field} 不存在"); return 0; }

        var toAssign = ConvertForField(f.FieldType, val, m.typeHint);
        if (toAssign == null) { _log.AppendLine($"✗ 無法將 {m.value} 指派到 {m.host}.{m.field}（需要 {f.FieldType.Name}）"); return 0; }

        var cur = f.GetValue(comp) as UnityEngine.Object;
        if (cur == toAssign) return 0;
        f.SetValue(comp, toAssign);
        EditorUtility.SetDirty(comp);
        _log.AppendLine($"→ {m.host}.{m.field} = {Pretty(toAssign)}");
        return ++changes;
    }

    int ApplyEventMap(KGSoftProfile.EventMap e, Dictionary<string, UnityEngine.Object> resolved)
    {
        var host = FindByTypeName(e.host);
        var recv = FindByTypeName(e.receiver);
        if (!host || !recv) { _log.AppendLine($"✗ 事件映射失敗：{e.host} 或 {e.receiver} 不在場景"); return 0; }

        var f = host.GetType().GetField(e.unityEvent, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
        if (f == null) { _log.AppendLine($"✗ {e.host}.{e.unityEvent} 不存在"); return 0; }
        if (!(f.GetValue(host) is UnityEventBase ev)) { _log.AppendLine($"✗ {e.host}.{e.unityEvent} 不是 UnityEvent"); return 0; }

        var so = new SerializedObject(host);
        var prop = so.FindProperty(e.unityEvent);
        if (prop == null) { _log.AppendLine($"✗ 無法序列化 {e.host}.{e.unityEvent}"); return 0; }

        prop.arraySize++;
        var elem = prop.GetArrayElementAtIndex(prop.arraySize - 1);
        elem.FindPropertyRelative("m_Target").objectReferenceValue = recv.GetComponent<MonoBehaviour>();
        elem.FindPropertyRelative("m_MethodName").stringValue = e.method;
        elem.FindPropertyRelative("m_Mode").intValue = 1; // EventDefined
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(host);
        _log.AppendLine($"→ 綁定 {e.host}.{e.unityEvent} → {e.receiver}.{e.method}()");
        return 1;
    }

    UnityEngine.Object ConvertForField(System.Type fieldType, UnityEngine.Object value, string typeHint)
    {
        if (!value) return null;

        if (fieldType == typeof(GameObject)) return (value is GameObject go) ? go : (value is Component c ? c.gameObject : null);
        if (typeof(Component).IsAssignableFrom(fieldType))
        {
            var go = (value is GameObject g) ? g : (value is Component c ? c.gameObject : null);
            if (!go) return null;

            var need = go.GetComponent(fieldType);
            if (need) return need;

            string wantName = !string.IsNullOrEmpty(typeHint) ? typeHint : fieldType.Name;
            var any = go.GetComponents<Component>().FirstOrDefault(x => x && x.GetType().Name == wantName);
            if (any) return any;

            var sceneAny = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true).FirstOrDefault(x => x && x.GetType().Name == wantName);
            if (sceneAny) return sceneAny;
        }
        if (fieldType == typeof(Slider) && value is GameObject go2) return go2.GetComponent<Slider>();

        return value;
    }

    MonoBehaviour FindByTypeName(string typeName)
    {
        return UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true)
            .FirstOrDefault(c => c && c.GetType().Name == typeName);
    }

    string Pretty(UnityEngine.Object o) => o ? $"{o.name} ({o.GetType().Name})" : "(null)";
}

[Serializable]
public class KGSoftProfile
{
    public Dictionary<string, string> objects = new Dictionary<string, string>(){
        {"ControllerRoot","ControllerRoot"},
        {"CombatController","CombatController"},
        {"CombatPanel","Canvas/CombatPanel"},
        {"StoryPanel","Canvas/StoryPanel"},
        {"EnemiesRoot","Canvas/CombatPanel/EnemiesRoot"}
    };
    public Dictionary<string, string> ui = new Dictionary<string, string>(){
        {"EnemyBar","$CombatPanel/UI/Hp_Enemys/Bar"},
        {"PlayerBar","$CombatPanel/UI/Hp_player/Bar"}
    };

    [Serializable] public class FieldMap { public string host; public string field; public string typeHint; public string value; }
    [Serializable] public class EventMap { public string host; public string unityEvent; public string receiver; public string method; }

    public List<FieldMap> fieldMap = new List<FieldMap>(){
        new FieldMap{host="CombatEventBridge",   field="manager",      typeHint="CombatManager",         value="CombatController"},
        new FieldMap{host="CombatEventBridge",   field="page",         typeHint="CombatPageController",  value="ControllerRoot"},
        new FieldMap{host="CombatEventBridge",   field="router",       typeHint="CombatResultRouter",    value="CombatController"},

        new FieldMap{host="CombatPageController", field="manager",      typeHint="CombatManager", value="CombatController"},
        new FieldMap{host="CombatPageController", field="combatPanel",  typeHint="GameObject",    value="CombatPanel"},
        new FieldMap{host="CombatPageController", field="storyPanel",   typeHint="GameObject",    value="StoryPanel"},
        new FieldMap{host="CombatPageController", field="enemiesRoot",  typeHint="GameObject",    value="EnemiesRoot"},

        new FieldMap{host="CombatUIController",   field="enemyHpSlider",typeHint="Slider",        value="EnemyBar"},
        new FieldMap{host="CombatUIController",   field="playerHpSlider",typeHint="Slider",       value="PlayerBar"},
    };

    public List<EventMap> events = new List<EventMap>(){
        // 沒 BoardKit 就留空；有的話再加即可
    };

    public static KGSoftProfile Load(string path, System.Action<string> log = null)
    {
        var dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        if (!File.Exists(path))
        {
            var proto = new KGSoftProfile();
            File.WriteAllText(path, JsonUtility.ToJson(proto, true), Encoding.UTF8);
            AssetDatabase.Refresh();
            log?.Invoke($"建立預設設定檔：{path}");
            return proto;
        }
        var txt = File.ReadAllText(path, Encoding.UTF8);
        var obj = JsonUtility.FromJson<KGSoftProfile>(txt);
        log?.Invoke($"載入設定檔：{path}");
        return obj ?? new KGSoftProfile();
    }
}
#endif
