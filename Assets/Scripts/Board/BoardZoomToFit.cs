using UnityEngine;

namespace CyberLife.Board
{
    /// <summary>
    /// 讓棋盤(Tiles)等比縮放到 BoardPanel 內，Pawns/UI 跟著同倍率縮放與置中。
    /// 不入侵你的 BoardController，只做「量外框 → 等比縮放 → 置中」。
    /// </summary>
    [ExecuteAlways, DisallowMultipleComponent]
    public class BoardZoomToFit : MonoBehaviour
    {
        [Header("Refs")]
        public RectTransform tilesRoot;   // Canvas/BoardPanel/Tiles
        public RectTransform pawnsRoot;   // Canvas/BoardPanel/Pawns
        public RectTransform uiRoot;      // Canvas/BoardPanel/UI

        [Header("Fit Options")]
        [Range(0.5f, 1f)] public float coverage = 0.92f; // 塞滿比例(留點邊)
        public float margin = 24f;                       // 邊界像素(額外保險)
        public bool autoOnStart = true;                  // Play 時自動套用
        public bool normalizeBefore = true;              // 縮放前把 scale/pos 歸 1/0

        RectTransform panel;

        void Reset()
        {
            panel = (RectTransform)transform;
            if (!tilesRoot)  tilesRoot  = (RectTransform)transform.Find("Tiles");
            if (!pawnsRoot)  pawnsRoot  = (RectTransform)transform.Find("Pawns");
            if (!uiRoot)     uiRoot     = (RectTransform)transform.Find("UI");
        }

        void OnEnable()
        {
            if (!panel) panel = (RectTransform)transform;
            if (autoOnStart && Application.isPlaying)
                FitNow();
        }

        [ContextMenu("Fit Now")]
        public void FitNow()
        {
            if (!panel || !tilesRoot) return;

            Canvas.ForceUpdateCanvases();

            if (normalizeBefore)
            {
                Normalize(tilesRoot);
                if (pawnsRoot) Normalize(pawnsRoot);
                if (uiRoot)    Normalize(uiRoot);
            }

            // 取 Tiles 與 Panel 的世界外框
            var tilesRect  = GetWorldRect(tilesRoot);
            var panelRect  = GetWorldRect(panel);

            // 扣掉 margin
            panelRect.xMin += margin; panelRect.yMin += margin;
            panelRect.xMax -= margin; panelRect.yMax -= margin;

            if (tilesRect.width <= 0 || tilesRect.height <= 0) return;

            // 算倍率（等比）
            float sx = panelRect.width  / tilesRect.width;
            float sy = panelRect.height / tilesRect.height;
            float scale = Mathf.Min(sx, sy) * Mathf.Clamp01(coverage);

            // 套用倍率（Tiles/Pawns/UI 一起縮放）
            ApplyScale(tilesRoot, scale);
            if (pawnsRoot) ApplyScale(pawnsRoot, scale);
            if (uiRoot)    ApplyScale(uiRoot,    scale);

            Canvas.ForceUpdateCanvases();

            // 重新計算外框後置中
            tilesRect = GetWorldRect(tilesRoot);
            Vector2 panelCenter = panelRect.center;
            Vector2 tilesCenter = tilesRect.center;
            Vector2 delta = panelCenter - tilesCenter;

            ShiftWorld(tilesRoot, delta);
            if (pawnsRoot) ShiftWorld(pawnsRoot, delta);
            if (uiRoot)    ShiftWorld(uiRoot,    delta);
        }

        static void Normalize(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
        }

        static Rect GetWorldRect(RectTransform rt)
        {
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            return new Rect(corners[0], corners[2] - corners[0]); // min, size
        }

        static void ApplyScale(RectTransform rt, float scale)
        {
            var s = rt.localScale;
            s.x *= scale; s.y *= scale;
            rt.localScale = s;
        }

        static void ShiftWorld(RectTransform rt, Vector2 worldDelta)
        {
            // 把世界位移轉成本地位移
            var cam = rt.GetComponentInParent<Canvas>()?.worldCamera;
            Vector3 a = rt.position;
            Vector3 b = a + (Vector3)worldDelta;
            if (cam) { a = cam.WorldToScreenPoint(a); b = cam.WorldToScreenPoint(b); }
            Vector2 localDelta = b - a;
            rt.anchoredPosition += localDelta;
        }
    }
}
