using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Game.Board
{
    [RequireComponent(typeof(RectTransform))]
    public class PawnController : MonoBehaviour
    {
        public RectTransform board;
        public RectTransform pawn;
        public int currentIndex = 0;
        public float moveTimePerTile = 0.15f;
        public bool IsMoving;
        public UnityEvent<int> onLanded = new UnityEvent<int>();

        RectTransform pawnRect => pawn ? pawn : (pawn = GetComponent<RectTransform>());

        void OnEnable()
        {
            SnapToCurrentIndex(notify: Application.isPlaying);
            IsMoving = false;
        }

        public void Roll() => RollAndMove();

        public void RollAndMove(){ int steps = Random.Range(1,7); MoveSteps(steps); }

        public void MoveSteps(int steps)
        {
            if (board == null || pawnRect == null) { Debug.LogWarning("[PawnController] Missing refs."); return; }
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy) return;
            if (IsMoving) return;
            StopAllCoroutines();
            StartCoroutine(CoMove(steps));
        }

        public void SnapToCurrentIndex(bool notify = false)
        {
            if (board == null || pawnRect == null) return;
            var target = GetTileByIndex(currentIndex);
            if (!target) return;
            pawnRect.anchoredPosition = TileToPawnLocal(target);
            if (notify)
            {
                NotifyLanded();
            }
        }

        IEnumerator CoMove(int steps)
        {
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy) yield break;
            IsMoving = true;
            int dir = steps >= 0 ? 1 : -1;
            steps = Mathf.Abs(steps);
            for (int i=0;i<steps;i++)
            {
                currentIndex = WrapIndex(currentIndex + dir);
                var target = GetTileByIndex(currentIndex);
                if (!target) break;
                Vector2 p = TileToPawnLocal(target);
                yield return StartCoroutine(CoLerpTo(p, moveTimePerTile));
            }
            IsMoving = false;
            NotifyLanded();
        }

        IEnumerator CoLerpTo(Vector2 target, float time)
        {
            Vector2 from = pawnRect.anchoredPosition;
            float t = 0f;
            while (t < 1f)
            {
                if (!isActiveAndEnabled || !gameObject.activeInHierarchy) yield break;
                t += Time.deltaTime / Mathf.Max(0.0001f, time);
                pawnRect.anchoredPosition = Vector2.Lerp(from, target, Mathf.SmoothStep(0f,1f,t));
                yield return null;
            }
            pawnRect.anchoredPosition = target;
        }

        int WrapIndex(int idx)
        {
            int count = board ? board.childCount : 0;
            if (count <= 0) return 0;
            if (idx < 0) idx = (idx % count + count) % count;
            return idx % count;
        }

        RectTransform GetTileByIndex(int index)
        {
            if (!board) return null;
            string[] names = new[] { $"Tile_{index}", $"Tile {index}", $"Tile{index}" };
            foreach (var n in names)
            {
                var t = board.Find(n) as RectTransform;
                if (t) return t;
            }
            if (index >= 0 && index < board.childCount) return board.GetChild(index) as RectTransform;
            if (index - 1 >= 0 && index - 1 < board.childCount) return board.GetChild(index - 1) as RectTransform;
            return null;
        }

        Vector2 TileToPawnLocal(RectTransform tile)
        {
            var pawnsRoot = pawnRect.parent as RectTransform;
            if (!pawnsRoot || !tile) return pawnRect.anchoredPosition;
            var canvas = pawnsRoot.GetComponentInParent<Canvas>();
            Camera cam = (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
            Vector3 world = tile.TransformPoint(tile.rect.center);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                pawnsRoot,
                RectTransformUtility.WorldToScreenPoint(cam, world),
                cam,
                out var local
            );
            return local;
        }

        void OnDisable(){ IsMoving = false; StopAllCoroutines(); }

        void NotifyLanded()
        {
            onLanded?.Invoke(currentIndex);
        }
    }
}
