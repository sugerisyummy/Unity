// Assets/Scripts/KG/Editor/KGSetupWizard.cs (patched)
// 修正：TrySetRef 會根據欄位型別自動從 GameObject 取對應 Component，避免
// "Object of type 'GameObject' cannot be converted to type 'CyberLife.Combat.CombatManager'".

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace CyberLife.KGTools
{
    public class KGSetupWizard : EditorWindow
    {
        [MenuItem("KG/Project/One-Click Setup (Patched)")]
        public static void Open() => GetWindow<KGSetupWizard>("KG Setup");

        bool connectCombat = true;
        bool connectUI = true;
        bool connectBoard = true;
        bool addHitStopPack = true;

        Vector2 _scroll;
        StringBuilder _log = new StringBuilder();

        void OnGUI()
        {
            GUILayout.Label("一鍵接線（Patched）", EditorStyles.boldLabel);
            connectCombat   = EditorGUILayout.Toggle("接線：Combat 管線", connectCombat);
            connectUI       = EditorGUILayout.Toggle("接線：Combat UI 血條", connectUI);
            connectBoard    = EditorGUILayout.Toggle("接線：BoardKit 橋接", connectBoard);
            addHitStopPack  = EditorGUILayout.Toggle("安裝：Impact HitStop 小套件", addHitStopPack);

            if (GUILayout.Button("Run One-Click Setup", GUILayout.Height(32)))
            {
                _log.Clear();
                Run();
            }

            GUILayout.Label("輸出紀錄：", EditorStyles.boldLabel);
            _scroll = GUILayout.BeginScrollView(_scroll);
            GUILayout.TextArea(_log.ToString(), GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
        }

        void Run()
        {
            int changes = 0;
            if (connectCombat) changes += SetupCombat();
            if (connectUI)     changes += SetupCombatUI();
            if (connectBoard)  changes += SetupBoardBridge();
            if (addHitStopPack) changes += EnsureHitStopPack();

            var outPath = "Assets/KG_SetupReport.txt";
            System.IO.File.WriteAllText(outPath, _log.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("KG Setup", $"完成，調整 {changes} 項。\n請看 {outPath}", "OK");
        }

        // ------------------ helpers ------------------
        static GameObject FindOrCreateGO(string name, Transform parent = null)
        {
            var go = GameObject.Find(name);
            if (!go)
            {
                go = new GameObject(name);
                if (parent) go.transform.SetParent(parent, false);
            }
            return go;
        }

        static Transform FindInCanvas(string path)
        {
            foreach (var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var t = canvas.transform.Find(path);
                if (t) return t;
            }
            return null;
        }

        // 依型別名稱尋找場景現有的組件（不綁命名空間）
        static Component FindByTypeName(string typeName)
        {
            return UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true)
                .FirstOrDefault(c => c && c.GetType().Name == typeName);
        }

        // 智慧指派：自動把 GameObject 轉成欄位宣告的組件型別
        int TrySetRef(GameObject target, string fieldName, UnityEngine.Object value)
        {
            var comp = target.GetComponents<MonoBehaviour>().FirstOrDefault(m => m && m.GetType().GetField(fieldName) != null);
            if (!comp) { _log.AppendLine($"  ✗ {target.name} 找不到可寫入 {fieldName} 的腳本"); return 0; }

            var f = comp.GetType().GetField(fieldName);
            var fieldType = f.FieldType;

            UnityEngine.Object assign = value;

            if (assign is GameObject go)
            {
                // 欄位若是 Component 子類，從 go 取對應組件
                if (typeof(Component).IsAssignableFrom(fieldType))
                {
                    var got = go.GetComponent(fieldType);
                    if (!got)
                    {
                        // 嘗試在場景尋找相同型別名稱的組件直接指派
                        var byName = FindByTypeName(fieldType.Name);
                        if (byName) assign = byName;
                        else { _log.AppendLine($"  ✗ {fieldType.Name} 不在 {go.name} 上，且場景找不到同名組件"); return 0; }
                    }
                    else assign = got;
                }
                else if (fieldType == typeof(GameObject))
                {
                    assign = go;
                }
            }
            else if (assign is Component cc)
            {
                if (!fieldType.IsAssignableFrom(cc.GetType()))
                {
                    // 若目標欄位是 GameObject，可以退而求其次塞 GameObject
                    if (fieldType == typeof(GameObject)) assign = cc.gameObject;
                    else { _log.AppendLine($"  ✗ {comp.GetType().Name}.{fieldName} 型別不相容：需要 {fieldType.Name}"); return 0; }
                }
            }

            var cur = f.GetValue(comp) as UnityEngine.Object;
            if (cur == assign) return 0;
            f.SetValue(comp, assign);
            EditorUtility.SetDirty(comp);
            _log.AppendLine($"  → 設定 {comp.GetType().Name}.{fieldName} = {Pretty(assign)}");
            return 1;
        }

        string Pretty(UnityEngine.Object o) => o ? $"{o.name}({o.GetType().Name})" : "(null)";

        // ------------------ setup sections ------------------
        int SetupCombat()
        {
            int changes = 0;
            _log.AppendLine("== Combat Wiring ==");

            var root = FindOrCreateGO("ControllerRoot");
            var mgrGo = FindOrCreateGO("CombatController", root.transform);

            // 嘗試找到場景現有的特定組件
            var mgrComp    = FindByTypeName("CombatManager");
            var bridgeComp = FindByTypeName("CombatEventBridge");
            var routerComp = FindByTypeName("CombatResultRouter");

            if (!mgrComp) _log.AppendLine("  ✗ 場景未找到 CombatManager（請手動掛到 CombatController）");
            if (!bridgeComp) _log.AppendLine("  ✗ 場景未找到 CombatEventBridge（請手動掛到 CombatController）");
            if (!routerComp) _log.AppendLine("  ✗ 場景未找到 CombatResultRouter（可與 Manager 同物件）");

            // CombatPageController 指派 manager / panels / enemiesRoot
            var pageComp = FindByTypeName("CombatPageController");
            GameObject pageGo = pageComp ? pageComp.gameObject : FindOrCreateGO("CombatPage");
            if (!pageComp) _log.AppendLine("  ✗ 找不到 CombatPageController（已建立空物件等待你掛腳本）");

            if (pageGo)
            {
                changes += TrySetRef(pageGo, "manager", mgrComp ? mgrComp : (UnityEngine.Object)mgrGo); // 智慧指派
                changes += TrySetRef(pageGo, "combatPanel", FindInCanvas("CombatPanel") ? FindInCanvas("CombatPanel").gameObject : FindOrCreateGO("CombatPanel"));
                changes += TrySetRef(pageGo, "storyPanel",  FindInCanvas("StoryPanel")  ? FindInCanvas("StoryPanel").gameObject  : FindOrCreateGO("StoryPanel"));

                var enemiesRootT = FindInCanvas("CombatPanel/EnemiesRoot");
                var enemiesRoot = enemiesRootT ? enemiesRootT.gameObject : FindOrCreateGO("EnemiesRoot", FindInCanvas("CombatPanel"));
                if (enemiesRoot && !enemiesRoot.GetComponent<UnityEngine.UI.GridLayoutGroup>())
                    enemiesRoot.AddComponent<UnityEngine.UI.GridLayoutGroup>();
                changes += TrySetRef(pageGo, "enemiesRoot", enemiesRoot);
            }

            return changes;
        }

        int SetupCombatUI()
        {
            int changes = 0;
            _log.AppendLine("== Combat UI ==");

            var uiComp = FindByTypeName("CombatUIController");
            if (!uiComp) { _log.AppendLine("  ✗ 找不到 CombatUIController"); return changes; }
            var uiGo = uiComp.gameObject;

            var enemyBar = FindInCanvas("CombatPanel/UI/Hp_Enemys/Bar")?.GetComponent<Slider>();
            var playerBar= FindInCanvas("CombatPanel/UI/Hp_player/Bar")?.GetComponent<Slider>();

            if (enemyBar) changes += TrySetRef(uiGo, "enemyHpSlider", enemyBar);
            else _log.AppendLine("  ✗ 找不到 UI/Hp_Enemys/Bar Slider");

            if (playerBar) changes += TrySetRef(uiGo, "playerHpSlider", playerBar);
            else _log.AppendLine("  ✗ 找不到 UI/Hp_player/Bar Slider");

            return changes;
        }

        int SetupBoardBridge()
        {
            int changes = 0;
            _log.AppendLine("== Board Bridge ==");

            var board = FindByTypeName("BoardController");
            var bridge= FindByTypeName("BoardEventsBridge");
            if (!board)  { _log.AppendLine("  ✗ 找不到 BoardController"); return changes; }
            if (!bridge) { _log.AppendLine("  ✗ 找不到 BoardEventsBridge"); return changes; }

            changes += TrySetUnityEventTarget(board.gameObject, "onRequestEvent", bridge.gameObject, "HandleStoryEvent");
            changes += TrySetUnityEventTarget(board.gameObject, "onRequestCombat", bridge.gameObject, "HandleCombat");

            return changes;
        }

        int EnsureHitStopPack()
        {
            int changes = 0;
            var path = "Assets/Scripts/VFX/HitStop.cs";
            if (!System.IO.File.Exists(path))
            {
                System.IO.Directory.CreateDirectory("Assets/Scripts/VFX");
                var src = @"using UnityEngine; public static class HitStop { public static void Do(float t){ var inst = Camera.main; if(inst){ inst.StartCoroutine(_Do(t)); } } static System.Collections.IEnumerator _Do(float t){ var ts = Time.timeScale; Time.timeScale = 0.01f; yield return new WaitForSecondsRealtime(t); Time.timeScale = ts; } }";
                System.IO.File.WriteAllText(path, src, Encoding.UTF8);
                changes++;
            }
            return changes;
        }

        // 嘗試將 UnityEvent<T> 以 SerializedObject 追加 listener（簡化版）
        int TrySetUnityEventTarget(GameObject target, string eventField, GameObject receiver, string method)
        {
            var comp = target.GetComponents<MonoBehaviour>().FirstOrDefault(m => m && m.GetType().GetField(eventField) != null);
            if (!comp) { _log.AppendLine($"  ✗ {target.name} 找不到 {eventField}"); return 0; }

            var so = new SerializedObject(comp);
            var prop = so.FindProperty(eventField);
            if (prop == null) { _log.AppendLine($"  ✗ 無法序列化 {eventField}"); return 0; }

            // 粗略檢查是否已存在同 receiver/method（省略深入比對）
            prop.arraySize++;
            var elem = prop.GetArrayElementAtIndex(prop.arraySize - 1);
            elem.FindPropertyRelative("m_Target").objectReferenceValue = receiver.GetComponent<MonoBehaviour>();
            elem.FindPropertyRelative("m_MethodName").stringValue = method;
            elem.FindPropertyRelative("m_Mode").intValue = 1; // EventDefined
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(comp);
            _log.AppendLine($"  → 綁定 {comp.GetType().Name}.{eventField} → {receiver.name}.{method}()");
            return 1;
        }
    }
}
#endif