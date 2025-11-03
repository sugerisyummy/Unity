#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Reflection;

public static class BoardLayoutFix
{
    [MenuItem("KG/Board/Fix Anchors (Scene)")]
    public static void FixAnchors()
    {
        var canvases = Object.FindObjectsOfType<Canvas>(true);
        int panels = 0, pawnsSnapped = 0;

        foreach (var cv in canvases)
        {
            FixCanvasScaler(cv);
            var rtList = cv.GetComponentsInChildren<RectTransform>(true);

            foreach (var rt in rtList)
            {
                if (!rt.gameObject.name.ToLower().Contains("panel") &&
                    rt.gameObject.name != "Tiles" &&
                    rt.gameObject.name != "Pawns" &&
                    rt.gameObject.name != "UI") continue;

                SetStretch(rt);
                panels++;
            }

            // 盡量讓 Tiles 的格子跟著面板大小縮放
            var tiles = rtList.FirstOrDefault(r => r.name == "Tiles");
            if (tiles)
            {
                var gl = tiles.GetComponent<GridLayoutGroup>();
                if (gl)
                {
                    var parent = tiles as RectTransform;
                    var rect = parent.rect;
                    float size = Mathf.Min(rect.width, rect.height);

                    // 預留 2% 邊距
                    size *= 0.98f;
                    // 嘗試把 cellSize 調到不超出
                    int constraintCount = gl.constraintCount > 0 ? gl.constraintCount : 8;
                    float cell = size / (constraintCount + 1); // 粗略估算，避免超出
                    gl.cellSize = new Vector2(cell, cell);
                    gl.spacing = new Vector2(cell * 0.08f, cell * 0.08f);
                    EditorUtility.SetDirty(gl);
                }
            }
        }

        // 讓所有 PawnController 對齊
        var pawnType = typeof(MonoBehaviour).Assembly.GetType("PawnController");
        if (pawnType == null)
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                pawnType = asm.GetType("PawnController");
                if (pawnType != null) break;
            }
        }
        if (pawnType != null)
        {
            var method = pawnType.GetMethod("SnapToCurrentIndex", BindingFlags.Public | BindingFlags.Instance);
            var pawns = Object.FindObjectsOfType(pawnType, true);
            foreach (var p in pawns)
            {
                if (method != null) method.Invoke(p, null);
                pawnsSnapped++;
            }
        }

        Debug.Log($"[KG] Fix Anchors 完成：處理面板/容器 {panels} 個，棋子對齊 {pawnsSnapped} 個。");
    }

    static void SetStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void FixCanvasScaler(Canvas cv)
    {
        var scaler = cv.GetComponent<CanvasScaler>();
        if (!scaler) scaler = cv.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        EditorUtility.SetDirty(scaler);
    }
}
#endif