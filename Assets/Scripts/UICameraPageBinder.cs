using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// 把 Main Camera 綁到指定的 UI Panel（RectTransform）。
    /// 前提：Canvas 使用 Screen Space - Camera，且 worldCamera 指向這台相機。
    /// 功能：FitTo(panel) 會把相機對準 panel 中心，並調整 Orthographic Size 讓整個 panel 填滿視窗（可加 padding）。
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class UICameraPageBinder : MonoBehaviour
    {
        public Canvas canvas;
        [Range(0f, 0.5f)] public float padding = 0.06f;  // 額外邊界比例
        public bool follow;                               // 面板在動畫/移動時，是否每幀跟隨
        public RectTransform current;                     // 目前綁定的面板

        Camera cam => _cam ?? (_cam = GetComponent<Camera>());
        Camera _cam;

        void Reset()
        {
            cam.orthographic = true;
            if (!canvas) canvas = FindObjectOfType<Canvas>();
        }

        void LateUpdate()
        {
            if (follow && current) FitTo(current);
        }

        /// <summary>綁定到指定面板並立即調整相機。</summary>
        public void BindTo(RectTransform panel)
        {
            current = panel;
            FitTo(panel);
        }

        /// <summary>抓目前啟用中的第一個 *Panel 物件來綁定。</summary>
        public void BindActivePanel()
        {
            if (!canvas) canvas = FindObjectOfType<Canvas>();
            if (!canvas) return;
            foreach (var rt in canvas.GetComponentsInChildren<RectTransform>(true))
            {
                if (!rt.gameObject.activeInHierarchy) continue;
                var n = rt.gameObject.name;
                if (n.Contains("Panel"))
                {
                    BindTo(rt);
                    return;
                }
            }
        }

        /// <summary>把相機對齊到 panel 並調整尺寸。</summary>
        public void FitTo(RectTransform panel)
        {
            if (!panel) return;
            if (!canvas) canvas = panel.GetComponentInParent<Canvas>();
            if (canvas)
            {
                if (canvas.renderMode != RenderMode.ScreenSpaceCamera)
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                if (canvas.worldCamera != cam) canvas.worldCamera = cam;
            }

            // 取四個世界座標角落
            var corners = new Vector3[4];
            panel.GetWorldCorners(corners);
            var min = corners[0];
            var max = corners[2];
            var center = (min + max) * 0.5f;
            var size = max - min;
            var halfW = Mathf.Abs(size.x) * 0.5f;
            var halfH = Mathf.Abs(size.y) * 0.5f;

            // 依螢幕比例決定 orthographic size
            float need = Mathf.Max(halfH, halfW / cam.aspect);
            cam.orthographic = true;
            cam.orthographicSize = need * (1f + padding);

            // 移到面板中心（保持原本 Z）
            var p = cam.transform.position;
            p.x = center.x;
            p.y = center.y;
            cam.transform.position = p;
        }
    }
}
