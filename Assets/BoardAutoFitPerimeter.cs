using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Board
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class BoardAutoFitPerimeter : MonoBehaviour
    {
        [SerializeField] private RectTransform tiles;
        [SerializeField] [Min(0f)] private float padding = 24f;
        [SerializeField] private bool applyEveryFrame = false;

        private RectTransform _tilesRect;
        private RectTransform _parentRect;

        private void Awake()
        {
            _tilesRect = tiles ? tiles : (RectTransform)transform;
            _parentRect = _tilesRect.parent as RectTransform;
        }

        private void OnEnable()
        {
            StartCoroutine(FitNextFrame());
        }

        private IEnumerator FitNextFrame()
        {
            yield return null;
            Fit();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            Fit();
        }

        private void Update()
        {
            if (applyEveryFrame)
            {
                Fit();
            }
        }

        private void Fit()
        {
            if (_tilesRect == null || _parentRect == null)
            {
                return;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_parentRect);

            var parentRect = _parentRect.rect;
            var availableWidth = Mathf.Max(0f, parentRect.width - padding * 2f);
            var availableHeight = Mathf.Max(0f, parentRect.height - padding * 2f);

            var contentRect = _tilesRect.rect;
            if (contentRect.width <= 0f || contentRect.height <= 0f)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_tilesRect);
                contentRect = _tilesRect.rect;

                if (contentRect.width <= 0f || contentRect.height <= 0f)
                {
                    return;
                }
            }

            var scale = Mathf.Min(availableWidth / contentRect.width, availableHeight / contentRect.height);
            scale = Mathf.Clamp(scale, 0.01f, 100f);

            var current = _tilesRect.localScale;
            if (Mathf.Abs(current.x - scale) > 0.001f || Mathf.Abs(current.y - scale) > 0.001f)
            {
                _tilesRect.localScale = new Vector3(scale, scale, 1f);
            }
        }
    }
}
