
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public static class BoardLayoutMutatorScan
{
    static readonly string[] NameHints = new[] {
        "Layout","ContentSizeFitter","AspectRatioFitter",
        "Auto","Fit","Stretch","Sizer","Scaler","Anchor","Resize"
    };

    [MenuItem("KG/Board/Scan Mutators (BoardPanel)")]
    public static void Scan()
    {
        var board = FindBoardPanel();
        if (!board)
        {
            EditorUtility.DisplayDialog("找不到 BoardPanel", "請確認路徑：Canvas/BoardPanel", "OK");
            return;
        }

        var suspects = FindSuspects(board);
        if (suspects.Count == 0)
        {
            EditorUtility.DisplayDialog("掃描完成", "BoardPanel 子樹沒有可疑的版面/自動縮放腳本。", "OK");
            return;
        }

        string msg = "找到可能會改 RectTransform 的元件（請逐一停用測試）：\n\n";
        foreach (var s in suspects)
        {
            msg += $"- {s.comp.GetType().Name} ＠ {GetPath(s.go)}\n";
        }
        Debug.Log(msg);
        Selection.objects = suspects.Select(s => s.go).ToArray();
        EditorUtility.DisplayDialog("掃描完成（已選取可疑物件）", msg, "OK");
    }

    [MenuItem("KG/Board/Disable Mutators (BoardPanel)")]
    public static void DisableAll()
    {
        var board = FindBoardPanel();
        if (!board)
        {
            EditorUtility.DisplayDialog("找不到 BoardPanel", "請確認路徑：Canvas/BoardPanel", "OK");
            return;
        }
        var suspects = FindSuspects(board);
        foreach (var s in suspects)
        {
            var mb = s.comp as Behaviour;
            if (mb && mb.enabled)
            {
                Undo.RecordObject(mb, "Disable Mutator");
                mb.enabled = false;
                EditorUtility.SetDirty(mb);
            }
        }
        EditorUtility.DisplayDialog("已停用", $"停用 {suspects.Count} 個可疑元件（可用 Ctrl+Z 還原）。", "OK");
    }

    struct Item { public GameObject go; public Component comp; }

    static Transform FindBoardPanel()
    {
        var canvas = GameObject.Find("Canvas");
        return canvas ? canvas.transform.Find("BoardPanel") : null;
    }

    static List<Item> FindSuspects(Transform root)
    {
        var list = new List<Item>();
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            // 1) Unity 內建會動 layout 的
            var gl = t.GetComponent<GridLayoutGroup>();   if (gl) list.Add(new Item{ go=t.gameObject, comp=gl });
            var hl = t.GetComponent<HorizontalLayoutGroup>(); if (hl) list.Add(new Item{ go=t.gameObject, comp=hl });
            var vl = t.GetComponent<VerticalLayoutGroup>();   if (vl) list.Add(new Item{ go=t.gameObject, comp=vl });
            var csf = t.GetComponent<ContentSizeFitter>(); if (csf) list.Add(new Item{ go=t.gameObject, comp=csf });
            var arf = t.GetComponent<AspectRatioFitter>(); if (arf) list.Add(new Item{ go=t.gameObject, comp=arf });

            // 2) 其它腳本名稱含關鍵字
            foreach (var c in t.GetComponents<Component>())
            {
                if (!c) continue;
                if (c is RectTransform) continue;
                if (c is Image || c is Text || c is Button || c is Mask || c is CanvasRenderer) continue;
#if TMP_PRESENT
                if (c is TMPro.TMP_Text) continue;
#endif
                string n = c.GetType().Name;
                if (NameHints.Any(h => n.IndexOf(h, System.StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    // 避免重覆加入已捕捉過的內建類型
                    if (c is GridLayoutGroup || c is HorizontalLayoutGroup || c is VerticalLayoutGroup
                        || c is ContentSizeFitter || c is AspectRatioFitter)
                        continue;
                    list.Add(new Item{ go=t.gameObject, comp=c });
                }
            }
        }
        return list;
    }

    static string GetPath(GameObject go)
    {
        var path = go.name;
        var p = go.transform.parent;
        while (p != null)
        {
            path = p.name + "/" + path;
            p = p.parent;
        }
        return path;
    }
}
#endif
