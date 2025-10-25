// Assets/Scripts/KG/Editor/CombatTools_KG.cs
// 工具：
//  1) KG/Combat/Dump Wiring → 輸出戰鬥配線 Assets/CombatWiringReport.txt
//  2) KG/Project/Dump Inventory → 輸出清單 Assets/ProjectInventory.json / _Short.txt
//  3) 型別清單可由 Assets/KG/WiringTypes.txt 覆寫（每行一個 Type 名，例如 CombatManager）
//
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace CyberLife.KGTools
{
    public static class CombatTools_KG
    {
        // 預設想看的型別（可被外部文字檔覆蓋）
        static readonly string[] kDefaultTypes = new []{
            "CombatManager",
            "CombatEventBridge",
            "CombatResultRouter",
            "CombatPageController",
            "CombatUIController",
        };

        static HashSet<string> LoadTypeNames()
        {
            var set = new HashSet<string>(kDefaultTypes);
            var path = "Assets/KG/WiringTypes.txt";
            try
            {
                if (File.Exists(path))
                {
                    foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
                    {
                        var t = line.Trim();
                        if (string.IsNullOrEmpty(t) || t.StartsWith("#")) continue;
                        set.Add(t);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[KG] LoadTypeNames failed: {e.Message}");
            }
            return set;
        }

        [MenuItem("KG/Combat/Dump Wiring")]
        public static void DumpWiring()
        {
            var kTypes = LoadTypeNames();

            var sb = new StringBuilder();
            sb.AppendLine("# Combat Wiring Report");
            sb.AppendLine("Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | Unity: " + Application.unityVersion);
            sb.AppendLine("Types: " + string.Join(", ", kTypes));
            sb.AppendLine();

            var comps = Resources.FindObjectsOfTypeAll<MonoBehaviour>()
                .Where(c => c != null
                         && c.gameObject != null
                         && c.gameObject.scene.IsValid()
                         && c.gameObject.scene.isLoaded
                         && !EditorUtility.IsPersistent(c))
                .ToArray();

            foreach (var grp in comps.GroupBy(c => c.GetType().Name).OrderBy(g => g.Key))
            {
                if (!kTypes.Contains(grp.Key)) continue;
                sb.AppendLine("== " + grp.Key + " (" + grp.Count() + ") ==");
                foreach (var c in grp)
                {
                    sb.AppendLine("- " + HierarchyPath(c.gameObject));
                    DumpUnityObjectFields(sb, c);
                    DumpUnityEvents(sb, c);
                    sb.AppendLine();
                }
                sb.AppendLine();
            }

            var outPath = "Assets/CombatWiringReport.txt";
            File.WriteAllText(outPath, sb.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log("[KG] CombatWiringReport saved: " + outPath);
            EditorUtility.RevealInFinder(Path.GetFullPath(outPath));
        }

        [MenuItem("KG/Project/Dump Inventory")]
        public static void DumpInventory()
        {
            var inv = new Inventory {
                time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                unity = Application.unityVersion,
                project = Application.productName,
                buildScenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray(),
            };

            var guids = AssetDatabase.FindAssets("", new[] { "Assets" }).Distinct().ToArray();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path) || path.EndsWith(".meta")) continue;

                var type = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (type == null) continue;

                string full = Path.GetFullPath(path);
                long size = File.Exists(full) ? new FileInfo(full).Length : 0;
                string md5  = File.Exists(full) ? MD5Hex(full) : "";

                string[] exts = {".cs",".unity",".prefab",".asset",".png",".jpg",".jpeg",".psd",".wav",".mp3",".ogg",".ttf",".shader"};
                string[] deps = exts.Any(e => path.EndsWith(e, StringComparison.OrdinalIgnoreCase))
                              ? AssetDatabase.GetDependencies(path, true).Where(p=>!p.EndsWith(".meta")).Distinct().ToArray()
                              : Array.Empty<string>();

                inv.assets.Add(new AssetEntry {
                    path = path.Replace("\\", "/"),
                    guid = guid,
                    type = type.Name,
                    size = size,
                    md5  = md5,
                    deps = deps
                });
            }

            var jsonPath = "Assets/ProjectInventory.json";
            var json = JsonUtility.ToJson(inv, true);
            File.WriteAllText(jsonPath, json, Encoding.UTF8);

            var shortPath = "Assets/ProjectInventory_Short.txt";
            var sb = new StringBuilder();
            sb.AppendLine("# Project Inventory (short)");
            sb.AppendLine("Time: " + inv.time + " | Unity: " + inv.unity + " | Project: " + inv.project);
            sb.AppendLine("Assets count: " + inv.assets.Count);
            sb.AppendLine();
            sb.AppendLine("- Build Scenes: " + string.Join(", ", inv.buildScenes));
            sb.AppendLine("- Critical Scripts (exists): " +
                string.Join(", ", inv.criticalScripts.Select(p => File.Exists(Path.GetFullPath(p)) ? "✓ " + p : "✗ " + p)));
            sb.AppendLine();
            sb.AppendLine("Top-level folders under Assets:");
            foreach (var dir in Directory.GetDirectories("Assets"))
            {
                var nice = dir.Replace("\\", "/");
                sb.AppendLine("  - " + nice);
            }
            File.WriteAllText(shortPath, sb.ToString(), Encoding.UTF8);

            AssetDatabase.Refresh();
            Debug.Log("[KG] ProjectInventory saved:\n  " + jsonPath + "\n  " + shortPath);
            EditorUtility.RevealInFinder(Path.GetFullPath(jsonPath));
        }

        // ---------- helpers ----------

        static string HierarchyPath(GameObject go)
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

        static void DumpUnityObjectFields(StringBuilder sb, Component c)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var fields = c.GetType().GetFields(flags);
            foreach (var f in fields)
            {
                if (!typeof(UnityEngine.Object).IsAssignableFrom(f.FieldType)) continue;
                var val = f.GetValue(c) as UnityEngine.Object;
                if (val == null) continue;

                string path = "(asset)";
                if (val is Component vc && vc.gameObject != null) path = HierarchyPath(vc.gameObject);
                else if (val is GameObject g && g != null) path = HierarchyPath(g);

                sb.AppendLine("    " + f.Name + " → " + val.name + " @ " + path);
            }
        }

        static void DumpUnityEvents(StringBuilder sb, Component c)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var fields = c.GetType().GetFields(flags);
            foreach (var f in fields)
            {
                if (!typeof(UnityEventBase).IsAssignableFrom(f.FieldType)) continue;
                var ev = f.GetValue(c) as UnityEventBase;
                if (ev == null) continue;
                int n = ev.GetPersistentEventCount();
                if (n <= 0) continue;

                sb.AppendLine("    " + f.Name + " (UnityEvent) targets:");
                for (int i = 0; i < n; i++)
                {
                    var tgt  = ev.GetPersistentTarget(i);
                    var name = ev.GetPersistentMethodName(i);
                    var targetName = (tgt != null) ? tgt.name : "(null)";
                    sb.AppendLine("      [" + i + "] " + targetName + " :: " + name);
                }
            }
        }

        [Serializable] class AssetEntry {
            public string path;
            public string guid;
            public string type;
            public long size;
            public string md5;
            public string[] deps;
        }
        [Serializable] class Inventory {
            public string time;
            public string unity;
            public string project;
            public string root = "Assets";
            public string[] criticalScripts = new []{
                "Assets/Scripts/Combat/CombatPageController.cs",
                "Assets/Scripts/Combat/CombatManager.cs",
                "Assets/Scripts/Combat/CombatEventBridge.cs",
                "Assets/Scripts/Combat/CombatResultRouter.cs",
                "Assets/Scripts/Combat/CombatUIController.cs",
            };
            public string[] buildScenes;
            public List<AssetEntry> assets = new List<AssetEntry>();
        }

        static string MD5Hex(string fullPath)
        {
            using (var md5 = MD5.Create())
            using (var fs = File.OpenRead(fullPath))
            {
                var hash = md5.ComputeHash(fs);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
#endif