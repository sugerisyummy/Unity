// Namespace: Game.Board
using UnityEngine;
using System.Collections;

namespace Game.Board
{
    [DisallowMultipleComponent]
    public class PawnController : MonoBehaviour
    {
        [Header("參照 (手動即可)")]
        public RectTransform tilesRoot;   // BoardPanel/Tiles
        public RectTransform pawn;        // BoardPanel/Pawns/Pawn

        [Header("索引 (0-based；0=第一格)")]
        public int startIndex = 0;
        [SerializeField] int currentIndex = 0;

        [Header("移動參數")]
        public float perTileDuration = 0.15f;
        public AnimationCurve curve = AnimationCurve.EaseInOut(0,0,1,1);

        public bool IsMoving { get; private set; }
        void Reset(){ AutoFill(); }
        void Awake(){ AutoFill(); }
        void OnEnable(){
            int max = MaxIndex;
            int clampedStart = Mathf.Clamp(startIndex, 0, max);
            int clampedCurrent = Mathf.Clamp(currentIndex, 0, max);
            currentIndex = (clampedCurrent == 0 && clampedStart != 0) ? clampedStart : clampedCurrent;
            SnapToCurrentIndex();
        }

        void AutoFill(){
            if (!tilesRoot){ var t = GameObject.Find("Tiles"); if (t) tilesRoot = t.transform as RectTransform; }
            if (!pawn){ var p = GameObject.Find("Pawn"); if (p) pawn = p.transform as RectTransform; }
        }

        int MaxIndex => (tilesRoot ? tilesRoot.childCount - 1 : 0);
        RectTransform GetTile(int i){ if (!tilesRoot || tilesRoot.childCount==0) return null; i=Mathf.Clamp(i,0,MaxIndex); return tilesRoot.GetChild(i) as RectTransform; }

        Vector2 TileToPawnLocal(RectTransform tile){
            if (!tile || !pawn) return Vector2.zero;
            var parent = pawn.parent as RectTransform;
            var world = tile.TransformPoint(tile.rect.center);
            return (Vector2)parent.InverseTransformPoint(world);
        }

        public void SnapToCurrentIndex(){
            var tile = GetTile(currentIndex); if (!tile || !pawn) return;
            pawn.anchoredPosition = TileToPawnLocal(tile);
        }

        public void MoveToIndex(int index){
            int clamped = Mathf.Clamp(index, 0, MaxIndex);
            int steps = clamped - currentIndex;
            if (steps != 0) StartCoroutine(CoMoveSteps(steps));
        }

        public void MoveSteps(int steps){
            if (steps <= 0) return;
            StartCoroutine(CoMoveSteps(steps));
        }

        IEnumerator CoMoveSteps(int steps){
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy) yield break;
            if (IsMoving) yield break;
            IsMoving = true;

            int target = Mathf.Clamp(currentIndex + steps, 0, MaxIndex);
            while (currentIndex < target){
                int next = currentIndex + 1;
                var from = GetTile(currentIndex);
                var to   = GetTile(next);
                if (!from || !to) break;

                Vector2 a = TileToPawnLocal(from);
                Vector2 b = TileToPawnLocal(to);
                float t = 0f;
                while (t < 1f){
                    t += Time.deltaTime / Mathf.Max(0.01f, perTileDuration);
                    float k = curve.Evaluate(Mathf.Clamp01(t));
                    pawn.anchoredPosition = Vector2.LerpUnclamped(a, b, k);
                    yield return null;
                }
                pawn.anchoredPosition = b;
                currentIndex = next;
            }
            IsMoving = false;
        }

        public int GetCurrentIndex()=>currentIndex;
        public int GetMaxIndex()=>MaxIndex;
    }
}
