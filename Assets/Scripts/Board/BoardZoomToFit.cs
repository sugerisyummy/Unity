using UnityEngine;

namespace CyberLife.Board
{
    /// <summary>
    /// 保留舊組件名稱，但內部改用 Game.Board.BoardAutoFitPerimeter 在 Tiles 上運作。
    /// </summary>
    [ExecuteAlways, DisallowMultipleComponent]
    [AddComponentMenu("Game/Board/BoardZoomToFit (Legacy)")]
    public class BoardZoomToFit : MonoBehaviour
    {
        [Header("Refs")]
        public RectTransform tilesRoot;   // Canvas/BoardPanel/Tiles

        [Header("Fit Options")]
        [Range(0.5f, 1f)] public float coverage = 0.92f; // 塞滿比例(留點邊)
        public float margin = 24f;                       // 邊界像素(額外保險)
        public bool autoOnStart = true;                  // Play 時自動套用

        Game.Board.BoardAutoFitPerimeter autoFit;

        void Reset()
        {
            if (!tilesRoot)
            {
                var t = transform as RectTransform;
                tilesRoot = t;
                var candidate = transform.Find("Tiles") as RectTransform;
                if (candidate) tilesRoot = candidate;
            }
        }

        void Awake()
        {
            EnsureAutoFit();
        }

        void OnEnable()
        {
            if (EnsureAutoFit() && autoOnStart && Application.isPlaying)
            {
                RequestFit();
            }
        }

        void OnValidate()
        {
            if (EnsureAutoFit() && !Application.isPlaying)
            {
                RequestFit();
            }
        }

        [ContextMenu("Fit Now")]
        public void FitNow()
        {
            if (EnsureAutoFit())
            {
                RequestFit(immediate: true);
            }
        }

        bool EnsureAutoFit()
        {
            if (!tilesRoot)
            {
                tilesRoot = transform as RectTransform;
            }

            if (!tilesRoot)
            {
                autoFit = null;
                return false;
            }

            autoFit = tilesRoot.GetComponent<Game.Board.BoardAutoFitPerimeter>();
            if (!autoFit)
            {
                autoFit = tilesRoot.gameObject.AddComponent<Game.Board.BoardAutoFitPerimeter>();
            }

            ApplyOptions();
            return autoFit != null;
        }

        void RequestFit(bool immediate = false)
        {
            if (!autoFit)
            {
                return;
            }

            ApplyOptions();

            if (immediate)
            {
                autoFit.FitNow();
            }
            else
            {
                autoFit.RequestFit();
            }
        }

        void ApplyOptions()
        {
            if (!autoFit)
            {
                return;
            }

            autoFit.Margin = Mathf.Max(0f, margin);
            autoFit.Coverage = Mathf.Clamp(coverage, 0.1f, 1f);
            autoFit.AllowUpscale = true;
        }
    }
}
