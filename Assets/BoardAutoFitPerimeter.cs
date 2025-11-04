// v2.1 安定版：等待一幀後再 Fit；僅改 Tiles 容器 localScale 與 anchoredPosition
// 避免與其他腳本搶寫；解析度改變/切頁回來會自動重算
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Board
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class BoardAutoFitPerimeter : MonoBehaviour
    {
        [SerializeField] private RectTransform tiles;          // 若留空，預設為自身
        [SerializeField] [Min(0)] private float padding = 24f; // 邊距（px）
        [SerializeField] private bool applyEveryFrame = false; // 除非你要動畫，通常 false 就好

        RectTransform rt, parent;

        void Awake()
        {
            rt = tiles ? tiles : (RectTransform)transform;
            parent = rt.parent as RectTransform;
        }

        void OnEnable() => StartCoroutine(ApplyNextFrame());
        IEnumerator ApplyNextFrame() { yield return null; Fit(); }

        void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled) return;
            Fit();
        }

        void Update()
        {
            if (applyEveryFrame) Fit();
        }

        void Fit()
        {
            if (rt == null || parent == null) return;

            // 確保父佈局已完成（避免拿到 0 尺寸）
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent);

            var pRect = parent.rect;
            var availW = Mathf.Max(0f, pRect.width  - padding * 2f);
            var availH = Mathf.Max(0f, pRect.height - padding * 2f);

            var raw = rt.rect;
            if (raw.width <= 0f || raw.height <= 0f)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                raw = rt.rect;
                if (raw.width <= 0f || raw.height <= 0f) return;
            }

            var s = Mathf.Min(availW / raw.width, availH / raw.height);
            s = Mathf.Clamp(s, 0.01f, 100f);

            // 鎖定中心與置中；不強改錨點（避免與你的場景設定衝突）
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            var cur = rt.localScale;
            if (Mathf.Abs(cur.x - s) > 0.001f || Mathf.Abs(cur.y - s) > 0.001f)
                rt.localScale = new Vector3(s, s, 1f);
        }
    }
}
