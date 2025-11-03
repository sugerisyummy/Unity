using UnityEngine;

namespace Game.Board
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class PawnAutoSizeToTile : MonoBehaviour
    {
        public RectTransform tilesRoot;
        [Range(0.4f,1.2f)] public float sizeRatio = 0.8f;
        public bool autoOnResize = true;
        public bool autoOnUpdate = false;

        RectTransform _pawn;

        void OnEnable(){ Init(); FitNow(); }
        void OnValidate(){ Init(); FitNow(); }
        void Update(){ if (autoOnUpdate) FitNow(); }
        void OnRectTransformDimensionsChange(){ if (autoOnResize) FitNow(); }

        void Init()
        {
            _pawn = GetComponent<RectTransform>();
            if (_pawn.anchorMin != new Vector2(0.5f,0.5f) || _pawn.anchorMax != new Vector2(0.5f,0.5f))
                _pawn.anchorMin = _pawn.anchorMax = new Vector2(0.5f,0.5f);
            if (_pawn.pivot != new Vector2(0.5f,0.5f)) _pawn.pivot = new Vector2(0.5f,0.5f);
        }

        public void FitNow()
        {
            if (!_pawn) _pawn = GetComponent<RectTransform>();
            if (!tilesRoot) return;
            var tile = tilesRoot.childCount > 0 ? (tilesRoot.GetChild(0) as RectTransform) : null;
            if (!tile) return;
            var tileSize = tile.rect.size;
            float s = Mathf.Min(tileSize.x, tileSize.y) * Mathf.Clamp(sizeRatio, 0.1f, 2f);
            _pawn.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, s);
            _pawn.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, s);
        }
    }
}
