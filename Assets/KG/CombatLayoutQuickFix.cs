#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CyberLife.EditorTools
{
    public static class CombatLayoutQuickFix
    {
        [MenuItem("KG/Combat/Quick Fix Enemy Layout")]
        public static void FixLayout()
        {
            var er = GameObject.Find("Canvas/CombatPanel/EnemiesRoot");
            if (!er)
            {
                EditorUtility.DisplayDialog("KG", "找不到 Canvas/CombatPanel/EnemiesRoot", "OK");
                return;
            }
            var rt = er.GetComponent<RectTransform>();
            if (!rt) rt = er.AddComponent<RectTransform>();

            var grid = er.GetComponent<GridLayoutGroup>();
            if (!grid) grid = er.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(160, 160);
            grid.spacing = new Vector2(12, 12);
            grid.childAlignment = TextAnchor.LowerCenter;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.2f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(900, 420);
            rt.anchoredPosition = new Vector2(0, 40);

            EditorGUIUtility.PingObject(er);
            Debug.Log("[KG] Enemy 版面已調整：GridLayoutGroup 三列下緣對齊。");
        }
    }
}
#endif
