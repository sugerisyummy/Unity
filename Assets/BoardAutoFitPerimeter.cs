using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Board
{
    /// <summary>
    /// v2：新增 Bake/Fix 選項，避免編譯或進出 Play 造成再次重算。
    /// - freezeAfterFit：執行 FitNow 後自動凍結（關閉 auto 並停用元件）。
    /// - 右鍵功能：Bake Fit & Disable（把目前縮放/位置固定、停用腳本）。
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class BoardAutoFitPerimeter : MonoBehaviour
    {
        [Range(0f, 0.2f)] public float padding = 0.02f;
        public bool keepSquare = true;
        public bool autoOnAwake = true;
        public bool autoOnResize = true;
        public bool centerToParent = true;
        public bool resetScaleBeforeFit = true;

        [Header("v2")]
        public bool freezeAfterFit = false;   // Fit 之後自動停用自己（鎖定結果）
        public bool bakeOnAwake = false;      // Awake 就 Bake 一次並停用（一次到位）

        RectTransform rt;
        Vector3 initLocalScale = Vector3.one;

        void OnEnable()
        {
            rt = GetComponent<RectTransform>();
            if (resetScaleBeforeFit) { initLocalScale = Vector3.one; rt.localScale = initLocalScale; }
            if (autoOnAwake) FitNow();
            if (bakeOnAwake) { BakeAndDisable(); }
        }

        void Awake()
        {
            rt = GetComponent<RectTransform>();
            if (resetScaleBeforeFit) { initLocalScale = Vector3.one; rt.localScale = initLocalScale; }
            if (autoOnAwake) FitNow();
            if (bakeOnAwake) { BakeAndDisable(); }
        }

        void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled) return;
            if (autoOnResize) FitNow();
        }

        public void FitNow()
        {
            if (!rt) rt = GetComponent<RectTransform>();
            if (!rt || rt.parent == null) return;
            var parent = rt.parent as RectTransform;
            if (!parent) return;

            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parent, rt);
            var parentRect = parent.rect;
            var parentSize = parentRect.size;

            float pad = Mathf.Clamp01(padding) * Mathf.Min(parentSize.x, parentSize.y);
            Vector2 targetSize = new Vector2(Mathf.Max(0, parentSize.x - 2f * pad), Mathf.Max(0, parentSize.y - 2f * pad));

            Vector2 currentSize = bounds.size;
            if (currentSize.x <= 0.0001f || currentSize.y <= 0.0001f) return;

            float sx = targetSize.x / currentSize.x;
            float sy = targetSize.y / currentSize.y;
            float s  = keepSquare ? Mathf.Min(sx, sy) : 1f;

            if (resetScaleBeforeFit) rt.localScale = initLocalScale;
            if (keepSquare)
                rt.localScale = new Vector3(s, s, 1f);
            else
                rt.localScale = new Vector3(sx, sy, 1f);

            if (centerToParent)
            {
                bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parent, rt);
                Vector2 offset = - (Vector2)bounds.center;
                rt.anchoredPosition += offset;
            }

            if (freezeAfterFit)
            {
                autoOnAwake = false;
                autoOnResize = false;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
                enabled = false; // 停用自己，避免之後又重算
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Bake Fit & Disable")]
        public void BakeAndDisable()
        {
            FitNow();
            autoOnAwake = false;
            autoOnResize = false;
            EditorUtility.SetDirty(this);
            enabled = false;
        }
#endif
    }
}