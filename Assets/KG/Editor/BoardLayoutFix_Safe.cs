#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public static class BoardLayoutFix_Safe
{
    [MenuItem("KG/Board/Fix Anchors (BoardPanel Only)")]
    public static void FixBoardPanelOnly()
    {
        var boardPanelGO = GameObject.Find("Canvas/BoardPanel") ?? GameObject.Find("BoardPanel");
        if (!boardPanelGO) { Debug.LogWarning("[KG] 找不到 BoardPanel。"); return; }

        int containers = 0, pawnsSnapped = 0;
        var boardRt = boardPanelGO.GetComponent<RectTransform>();
        Stretch(boardRt); containers++;

        FixChild("Tiles", boardRt, ref containers);
        FixChild("Pawns", boardRt, ref containers);
        FixChild("UI",    boardRt, ref containers);

        // 只在 BoardPanel 範圍內 Snap 棋子
        var pawnType = FindType("PawnController");
        if (pawnType != null)
        {
            var method = pawnType.GetMethod("SnapToCurrentIndex", BindingFlags.Public | BindingFlags.Instance);
            var pawns = boardPanelGO.GetComponentsInChildren(pawnType, true);
            foreach (var p in pawns) { method?.Invoke(p, null); pawnsSnapped++; }
        }

        Debug.Log($"[KG] BoardPanel-only anchors fixed. Containers {containers}, pawns snapped {pawnsSnapped}.（不改 CanvasScaler、不動其他頁面）");
    }

    [MenuItem("KG/Board/Fix Anchors (Selection Subtree)")]
    public static void FixSelectionSubtree()
    {
        var root = Selection.activeTransform as RectTransform;
        if (!root) { Debug.LogWarning("[KG] 請先選擇一個 UI 物件（RectTransform）。"); return; }

        int containers = 0, pawnsSnapped = 0;
        foreach (var rt in root.GetComponentsInChildren<RectTransform>(true))
        {
            Stretch(rt); containers++;
            MaybeAdjustGrid(rt);
        }

        var pawnType = FindType("PawnController");
        if (pawnType != null)
        {
            var method = pawnType.GetMethod("SnapToCurrentIndex", BindingFlags.Public | BindingFlags.Instance);
            var pawns = root.GetComponentsInChildren(pawnType, true);
            foreach (var p in pawns) { method?.Invoke(p, null); pawnsSnapped++; }
        }

        Debug.Log($"[KG] Selection subtree fixed. Containers {containers}, pawns snapped {pawnsSnapped}.（不改 CanvasScaler、不動其他頁面）");
    }

    static void FixChild(string name, RectTransform parent, ref int containers)
    {
        var child = parent.Find(name) as RectTransform;
        if (!child) return;
        Stretch(child); containers++;
        MaybeAdjustGrid(child);
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void MaybeAdjustGrid(RectTransform rt)
    {
        var gl = rt.GetComponent<GridLayoutGroup>();
        if (!gl) return;

        var rect = rt.rect;
        float size = Mathf.Min(rect.width, rect.height) * 0.98f;
        int n = gl.constraintCount > 0 ? gl.constraintCount : 8;
        float cell = size / (n + 1);
        gl.cellSize = new Vector2(cell, cell);
        gl.spacing  = new Vector2(cell * 0.08f, cell * 0.08f);
        EditorUtility.SetDirty(gl);
    }

    static System.Type FindType(string fullOrShortName)
    {
        var t = System.Type.GetType(fullOrShortName);
        if (t != null) return t;
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            t = asm.GetType(fullOrShortName);
            if (t != null) return t;
        }
        return null;
    }
}
#endif