using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Board
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class BoardAutoFitPerimeter : MonoBehaviour
    {
        [SerializeField] private RectTransform tiles;
        [SerializeField, Min(0f)] private float margin = 24f;
        [SerializeField, Range(0.5f, 1.1f)] private float coverage = 0.92f;
        [SerializeField] private bool allowUpscale = true;

        RectTransform target;
        RectTransform panel;
        Coroutine scheduled;
#if UNITY_EDITOR
        bool editorScheduled;
#endif

        public float Margin
        {
            get => margin;
            set
            {
                value = Mathf.Max(0f, value);
                if (!Mathf.Approximately(margin, value))
                {
                    margin = value;
                    RequestFit();
                }
            }
        }

        public float Coverage
        {
            get => coverage;
            set
            {
                value = Mathf.Clamp(value, 0.1f, 2f);
                if (!Mathf.Approximately(coverage, value))
                {
                    coverage = value;
                    RequestFit();
                }
            }
        }

        public bool AllowUpscale
        {
            get => allowUpscale;
            set
            {
                if (allowUpscale != value)
                {
                    allowUpscale = value;
                    RequestFit();
                }
            }
        }

        void Reset()
        {
            tiles = GetComponent<RectTransform>();
            Cache();
        }

        void Awake()
        {
            Cache();
        }

        void OnValidate()
        {
            margin = Mathf.Max(0f, margin);
            coverage = Mathf.Clamp(coverage, 0.1f, 2f);
            Cache();
            if (isActiveAndEnabled)
            {
                ScheduleFit();
            }
        }

        void OnEnable()
        {
            Cache();
            ScheduleFit();
        }

        void OnDisable()
        {
            if (scheduled != null)
            {
                StopCoroutine(scheduled);
                scheduled = null;
            }
#if UNITY_EDITOR
            if (editorScheduled)
            {
                EditorApplication.delayCall -= EditorDelayedFit;
                editorScheduled = false;
            }
#endif
        }

#if UNITY_EDITOR
        void OnDestroy()
        {
            if (editorScheduled)
            {
                EditorApplication.delayCall -= EditorDelayedFit;
                editorScheduled = false;
            }
        }
#endif

        void OnTransformChildrenChanged()
        {
            ScheduleFit();
        }

        void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            ScheduleFit();
        }

        public void RequestFit()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            ScheduleFit();
        }

        public void FitNow()
        {
            if (!Ensure())
            {
                return;
            }

            if (!TryGetBounds(out var bounds))
            {
                target.localScale = Vector3.one;
                target.anchoredPosition = Vector2.zero;
                return;
            }

            var parentSize = panel.rect.size;
            Vector2 available = new Vector2(
                Mathf.Max(1f, parentSize.x - margin * 2f),
                Mathf.Max(1f, parentSize.y - margin * 2f));

            float width = Mathf.Max(1f, bounds.width);
            float height = Mathf.Max(1f, bounds.height);

            float sx = available.x / width;
            float sy = available.y / height;
            float scale = Mathf.Min(sx, sy) * Mathf.Clamp01(coverage);
            if (!allowUpscale)
            {
                scale = Mathf.Min(scale, 1f);
            }

            if (!float.IsFinite(scale) || scale <= 0f)
            {
                scale = 1f;
            }

            var localScale = target.localScale;
            localScale.x = localScale.y = scale;
            target.localScale = localScale;

            Vector2 center = bounds.center;
            target.anchoredPosition = -center * scale;
        }

        void ScheduleFit()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (editorScheduled)
                {
                    EditorApplication.delayCall -= EditorDelayedFit;
                }

                EditorApplication.delayCall += EditorDelayedFit;
                editorScheduled = true;
                return;
            }
#endif

            if (scheduled != null)
            {
                StopCoroutine(scheduled);
            }

            scheduled = StartCoroutine(FitNextFrame());
        }

        IEnumerator FitNextFrame()
        {
            yield return null;
            scheduled = null;
            FitNow();
        }

        void Cache()
        {
            target = tiles ? tiles : GetComponent<RectTransform>();
            panel = target ? target.parent as RectTransform : null;
            if (!panel && target)
            {
                panel = target.GetComponentInParent<RectTransform>();
            }
        }

        bool Ensure()
        {
            if (!target)
            {
                Cache();
            }

            if (!target)
            {
                return false;
            }

            if (!panel)
            {
                panel = target.parent as RectTransform;
                if (!panel)
                {
                    panel = target.GetComponentInParent<RectTransform>();
                }
            }

            return panel != null;
        }

        bool TryGetBounds(out Rect bounds)
        {
            bounds = default;
            if (!target)
            {
                return false;
            }

            Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

            for (int i = 0; i < target.childCount; i++)
            {
                if (!(target.GetChild(i) is RectTransform child))
                {
                    continue;
                }

                var rect = child.rect;
                Vector2 half = rect.size * 0.5f;
                Vector2 pos = child.anchoredPosition;
                Vector2 childMin = pos - half;
                Vector2 childMax = pos + half;

                min = Vector2.Min(min, childMin);
                max = Vector2.Max(max, childMax);
            }

            if (!float.IsFinite(min.x) || !float.IsFinite(min.y) ||
                !float.IsFinite(max.x) || !float.IsFinite(max.y))
            {
                return false;
            }

            bounds = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            return bounds.width > 0f && bounds.height > 0f;
        }

#if UNITY_EDITOR
        void EditorDelayedFit()
        {
            editorScheduled = false;
            if (this == null)
            {
                return;
            }

            FitNow();
        }
#endif
    }
}
