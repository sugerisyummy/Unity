using UnityEngine;
using UnityEngine.UI;

namespace Game.Board
{
    /// v2：提供靜態工具法給其他腳本呼叫；Reset/右鍵可一鍵全拉伸。
    [ExecuteAlways]
    public class RectAutoStretch : MonoBehaviour
    {
        // 其它腳本可直接：RectAutoStretch.StretchToParent(rt);
        public static void StretchToParent(RectTransform rt)
        {
            if (!rt) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        [ContextMenu("Stretch to Parent (This)")]
        void StretchThis() => StretchToParent(transform as RectTransform);

        void Reset() => StretchThis();
    }
}
