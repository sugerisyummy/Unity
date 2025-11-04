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
        private Vector2 _lastParentSize;
        private Vector2Int _lastScreenSize;

        private void Awake()
        {
            EnsureReferences();
            CacheState();
        }

        private void OnValidate()
        {
            if (padding < 0f) padding = 0f;
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

        private void OnTransformParentChanged()
        {
            EnsureReferences();
            CacheState();
            if (isActiveAndEnabled)
            {
                StartCoroutine(FitNextFrame());
            }
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
            if (!EnsureReferences())
            {
                return;
            }

            var parentSize = _parentRect.rect.size;
            var screenSize = new Vector2Int(Screen.width, Screen.height);

            if (applyEveryFrame ||
                (parentSize - _lastParentSize).sqrMagnitude > 0.25f ||
                screenSize != _lastScreenSize)
            {
                Fit();
            }
        }

        private bool EnsureReferences()
        {
            if (!_tilesRect)
            {
                _tilesRect = tiles ? tiles : (RectTransform)transform;
            }

            if (_tilesRect)
            {
                _parentRect = _tilesRect.parent as RectTransform;
            }

            return _tilesRect != null && _parentRect != null;
        }

        private void CacheState()
        {
            if (_parentRect)
            {
                _lastParentSize = _parentRect.rect.size;
            }
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }

        private void Fit()
        {
            if (!EnsureReferences())
            {
                return;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_parentRect);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_tilesRect);

            var parentRect = _parentRect.rect;
            var availableWidth = Mathf.Max(0f, parentRect.width - padding * 2f);
            var availableHeight = Mathf.Max(0f, parentRect.height - padding * 2f);
            if (availableWidth <= 0f || availableHeight <= 0f)
            {
                return;
            }

            var contentRect = _tilesRect.rect;
            if (contentRect.width <= 0f || contentRect.height <= 0f)
            {
                return;
            }

            var scale = Mathf.Min(availableWidth / contentRect.width, availableHeight / contentRect.height);
            scale = Mathf.Clamp(scale, 0.01f, 100f);

            var uniformScale = new Vector3(scale, scale, 1f);
            if (_tilesRect.localScale != uniformScale)
            {
                _tilesRect.localScale = uniformScale;
            }

            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(_parentRect, _tilesRect);
            var offset = (Vector2)bounds.center;
            if (offset.sqrMagnitude > 0.0001f)
            {
                _tilesRect.anchoredPosition -= offset;
            }

            CacheState();
        }
    }
}
