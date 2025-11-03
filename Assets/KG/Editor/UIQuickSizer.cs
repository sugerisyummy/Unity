// KG Editor: UI Quick Sizer (TMP-Optional)
// - 不直接 using TMPro；改用反射處理 TMP_Text，沒有 TMP 也能編譯。
// - Canvas Scaler 快捷 + 整體縮放 + TMP AutoSize 開/關（若有 TMP 才生效）
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Reflection;

namespace KG.EditorTools
{
    public static class UIQuickSizer
    {
        // ===== Canvas Scaler =====
        [MenuItem("KG/UI/CanvasScaler → 1920x1080 (Match 0.5)")]
        public static void SetScaler1920() => SetScaler(1920,1080,0.5f);
        [MenuItem("KG/UI/CanvasScaler → 1280x720 (Match 0.5)")]
        public static void SetScaler1280() => SetScaler(1280,720,0.5f);
        [MenuItem("KG/UI/CanvasScaler → 960x540 (大一點)")]
        public static void SetScaler960() => SetScaler(960,540,0.5f);

        private static void SetScaler(int w, int h, float match)
        {
            var canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            if (!canvas) { Debug.LogWarning("[KG] 沒找到 Canvas"); return; }
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (!scaler) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(w, h);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = match;
            if (!UnityEngine.Object.FindObjectOfType<EventSystem>())
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Debug.Log($"[KG] CanvasScaler 設為 {w}x{h}, Match={match}");
        }

        // ===== 整體縮放 =====
        [MenuItem("KG/UI/Scale ×1.25 (Canvas)")]
        public static void Scale125() => ScaleAll(1.25f);
        [MenuItem("KG/UI/Scale ×1.5 (Canvas)")]
        public static void Scale150() => ScaleAll(1.5f);
        [MenuItem("KG/UI/Scale ×2.0 (Canvas)")]
        public static void Scale200() => ScaleAll(2.0f);
        [MenuItem("KG/UI/Scale ×0.75 (Canvas)")]
        public static void Scale075() => ScaleAll(0.75f);

        private static void ScaleAll(float k)
        {
            var canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            if (!canvas) { Debug.LogWarning("[KG] 沒找到 Canvas"); return; }

            int changed = 0;
            foreach (var rt in canvas.GetComponentsInChildren<RectTransform>(true))
            {
                Undo.RecordObject(rt, "UI Scale");
                if (rt.GetComponent<Canvas>() != null) continue;

                rt.sizeDelta *= k;
                rt.anchoredPosition *= k;

                var le = rt.GetComponent<LayoutElement>();
                if (le)
                {
                    if (le.preferredWidth > 0f) le.preferredWidth *= k;
                    if (le.preferredHeight > 0f) le.preferredHeight *= k;
                    if (le.minWidth > 0f) le.minWidth *= k;
                    if (le.minHeight > 0f) le.minHeight *= k;
                }

                // UGUI Text
                var ugui = rt.GetComponent<Text>();
                if (ugui) ugui.fontSize = Mathf.RoundToInt(ugui.fontSize * k);

                // TMP by reflection (no hard reference)
                var tmp = rt.GetComponent("TMPro.TMP_Text");
                if (tmp)
                {
                    var type = tmp.GetType();
                    var propFS = type.GetProperty("fontSize");
                    var propAuto = type.GetProperty("enableAutoSizing");
                    if (propFS != null && (propAuto == null || !(bool)propAuto.GetValue(tmp)))
                    {
                        float fs = Convert.ToSingle(propFS.GetValue(tmp));
                        propFS.SetValue(tmp, fs * k);
                    }
                    EditorUtility.SetDirty(rt);
                }

                EditorUtility.SetDirty(rt);
                changed++;
            }
            Debug.Log($"[KG] 已縮放 {changed} 個 RectTransform（含字體/LayoutElement）。倍率 ×{k}");
        }

        // ===== TMP Auto Size =====
        [MenuItem("KG/UI/TMP AutoSize → 開 (min=18,max=64)")]
        public static void TMPAutosizeOn() => SetTMPAutosize(true, 18, 64);
        [MenuItem("KG/UI/TMP AutoSize → 關")]
        public static void TMPAutosizeOff() => SetTMPAutosize(false, 0, 0);

        private static void SetTMPAutosize(bool on, float min, float max)
        {
            var canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            if (!canvas) { Debug.LogWarning("[KG] 沒找到 Canvas"); return; }

            int n = 0;
            foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
            {
                var comp = t.GetComponent("TMPro.TMP_Text");
                if (comp == null) continue;
                var type = comp.GetType();
                var pAuto = type.GetProperty("enableAutoSizing");
                if (pAuto != null) pAuto.SetValue(comp, on);
                if (on)
                {
                    var pMin = type.GetProperty("fontSizeMin");
                    var pMax = type.GetProperty("fontSizeMax");
                    if (pMin != null) pMin.SetValue(comp, min);
                    if (pMax != null) pMax.SetValue(comp, max);
                }
                EditorUtility.SetDirty(t);
                n++;
            }
            Debug.Log($"[KG] TMP AutoSize {(on ? "開" : "關")}，影響 {n} 個文字（若專案無 TMP 則 0）。");
        }
    }
}
#endif
