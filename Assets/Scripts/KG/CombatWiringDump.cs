// Assets/Editor/CombatWiringDump.cs
// 用途：輸出目前場景中戰鬥系統的接線狀態，幫我遠端確認你掛了哪些腳本與引用。
// 使用：放到 Assets/Editor/ ；Unity 功能表：Tools/Combat/Dump Wiring
// 產出：Assets/CombatWiringReport.txt（請把內容貼回來或直接上傳檔案）

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace CyberLife.Tools
{
    public static class CombatWiringDump
    {
        [MenuItem("Tools/Combat/Dump Wiring")]
        public static void Dump()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Combat Wiring Report");
            sb.AppendLine($"Time: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            // Helpers
            string PathOf(GameObject go)
            {
                if (go == null) return "(null)";
                var stack = new Stack<string>();
                var t = go.transform;
                while (t != null)
                {
                    stack.Push(t.name);
                    t = t.parent;
                }
                return string.Join("/", stack.ToArray());
            }
            string Ref(Object o)
            {
                if (!o) return "—";
                if (o is Component c) return $"{c.GetType().Name}@{PathOf(c.gameObject)}";
                if (o is GameObject go) return $"GameObject@{PathOf(go)}";
                return o.name;
            }
            void Field<T>(StringBuilder s, string name, T obj)
            {
                s.AppendLine($"  - {name}: {Ref(obj as Object)}");
            }
            string Bool(bool v) => v ? "OK" : "MISSING";

            // 1) CombatPanel / CombatManager / Bridge / Router
            var managers = Resources.FindObjectsOfTypeAll<CyberLife.Combat.CombatManager>()
                .Where(m => m.gameObject.scene.IsValid()).ToList();
            if (managers.Count == 0) sb.AppendLine("No CombatManager found in open scenes.");
            foreach (var m in managers)
            {
                sb.AppendLine($"## CombatManager @ {PathOf(m.gameObject)}");
                var bridge = m.GetComponent<CyberLife.Combat.CombatEventBridge>();
                var router = m.GetComponent<CyberLife.Combat.CombatResultRouter>();
                Field(sb, "CombatManager (self)", m);
                Field(sb, "CombatEventBridge", bridge);
                Field(sb, "CombatResultRouter", router);
                if (bridge)
                {
                    sb.AppendLine("### Bridge refs");
                    Field(sb, "manager", bridge.manager);
                    Field(sb, "page", bridge.page);
                    Field(sb, "router", bridge.router);
                }
                if (router)
                {
                    sb.AppendLine("### Router refs");
                    Field(sb, "manager", router.manager);
                    // Dump UnityEvents
                    void DumpEvent(string title, UnityEvent ev)
                    {
                        if (ev == null) { sb.AppendLine($"  - {title}: —"); return; }
                        int n = ev.GetPersistentEventCount();
                        sb.AppendLine($"  - {title}: {n} listeners");
                        for (int i = 0; i < n; i++)
                        {
                            var tgt = ev.GetPersistentTarget(i);
                            var meth= ev.GetPersistentMethodName(i);
                            sb.AppendLine($"      [{i}] {Ref(tgt)} :: {meth}");
                        }
                    }
                    DumpEvent("onWin", router.onWin);
                    DumpEvent("onLose", router.onLose);
                    DumpEvent("onEscape", router.onEscape);
                }
                sb.AppendLine();
            }

            // 2) PageController
            var pages = Resources.FindObjectsOfTypeAll<CyberLife.Combat.CombatPageController>()
                .Where(p => p.gameObject.scene.IsValid()).ToList();
            foreach (var p in pages)
            {
                sb.AppendLine($"## CombatPageController @ {PathOf(p.gameObject)}");
                Field(sb, "manager", p.manager);
                Field(sb, "enemiesRoot", p.enemiesRoot);
                Field(sb, "ui", p.ui);
                Field(sb, "storyPanel", p.storyPanel);
                Field(sb, "combatPanel", p.combatPanel);
                Field(sb, "playerSlot", p.playerSlot);
                Field(sb, "player (auto)", p.player);
                sb.AppendLine();
            }

            // 3) UI Controller
            var uis = Resources.FindObjectsOfTypeAll<CyberLife.Combat.CombatUIController>()
                .Where(u => u.gameObject.scene.IsValid()).ToList();
            foreach (var u in uis)
            {
                sb.AppendLine($"## CombatUIController @ {PathOf(u.gameObject)}");
                Field(sb, "manager", u.manager);
                Field(sb, "groupsPanel", u.groupsPanel);
                Field(sb, "enemyHpSlider", u.enemyHpSlider);
                Field(sb, "enemyHpLabel", u.enemyHpLabel);
                Field(sb, "playerHpSlider", u.playerHpSlider);
                Field(sb, "playerHpLabel", u.playerHpLabel);

                // TargetButtons children
                var targetButtons = u.transform.Find("TargetButtons");
                if (targetButtons)
                {
                    sb.AppendLine("### TargetButtons");
                    for (int i = 0; i < targetButtons.childCount; i++)
                    {
                        var child = targetButtons.GetChild(i);
                        var btn = child.GetComponent<Button>();
                        if (!btn) { sb.AppendLine($"  - {child.name}: (no Button)"); continue; }
                        int n = btn.onClick.GetPersistentEventCount();
                        sb.AppendLine($"  - {child.name}: onClick listeners={n}");
                        for (int k = 0; k < n; k++)
                        {
                            var tgt = btn.onClick.GetPersistentTarget(k);
                            var meth= btn.onClick.GetPersistentMethodName(k);
                            sb.AppendLine($"      [{k}] {Ref(tgt)} :: {meth}");
                        }
                    }
                }
                else sb.AppendLine("### TargetButtons not found under this UI controller.");
                sb.AppendLine();
            }

            // 4) GameManager wiring
            var gms = Resources.FindObjectsOfTypeAll<GameManager>()
                .Where(g => g.gameObject.scene.IsValid()).ToList();
            foreach (var g in gms)
            {
                sb.AppendLine($"## GameManager @ {PathOf(g.gameObject)}");
                // 反射拿 combatController
                var fi = typeof(GameManager).GetField("combatController", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                var cc = fi != null ? fi.GetValue(g) as CyberLife.Combat.CombatPageController : null;
                Field(sb, "combatController", cc);
                sb.AppendLine();
            }

            // 5) Basic sanity checks
            sb.AppendLine("## Checks");
            bool hasMgr = managers.Count > 0;
            bool hasPage= pages.Count > 0;
            bool hasUI  = uis.Count > 0;
            sb.AppendLine($"- CombatManager: {Bool(hasMgr)}");
            sb.AppendLine($"- CombatPageController: {Bool(hasPage)}");
            sb.AppendLine($"- CombatUIController: {Bool(hasUI)}");
            if (hasMgr)
            {
                foreach (var m in managers)
                {
                    var b = m.GetComponent<CyberLife.Combat.CombatEventBridge>();
                    var r = m.GetComponent<CyberLife.Combat.CombatResultRouter>();
                    sb.AppendLine($"- Bridge wired on {PathOf(m.gameObject)}: {Bool(b && b.manager == m)}");
                    sb.AppendLine($"- Router wired manager on {PathOf(m.gameObject)}: {Bool(r && r.manager == m)}");
                }
            }

            // Save
            var outPath = "Assets/CombatWiringReport.txt";
            System.IO.File.WriteAllText(outPath, sb.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log($"[CombatWiringDump] Report saved: {outPath}");
            EditorUtility.RevealInFinder(outPath);
        }
    }
}
#endif
