using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Board
{
    [ExecuteAlways]
    public class BoardTripletAutoStretch : MonoBehaviour
    {
        public string tilesName = "Tiles";
        public string pawnsName = "Pawns";
        public string uiName    = "UI";

        [ContextMenu("Apply To Tiles/Pawns/UI")]
        public void ApplyAll(){ StretchChild(tilesName); StretchChild(pawnsName); StretchChild(uiName); }
        void OnEnable()   => ApplyAll();
        void OnValidate() => ApplyAll();
        void Reset()      => ApplyAll();

        void StretchChild(string childName)
        {
            var rt = transform.Find(childName) as RectTransform;
            if (!rt) return;
            StretchToParent(rt);
        }

        public static void StretchToParent(RectTransform rt)
        {
            if (!rt) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition3D = new Vector3(0, 0, rt.anchoredPosition3D.z);
            rt.localScale = Vector3.one;
        }
    }
}
