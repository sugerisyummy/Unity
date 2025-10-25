using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace CyberLife.Board
{
    public class PawnController : MonoBehaviour
    {
        public BoardController board;
        public RectTransform pawn;
        public int index;
        public float stepDuration = 0.15f;
        public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0,0,1,1);

        [Header("Events")] public UnityEvent<int> onLanded;

        bool moving;
        public bool IsMoving => moving;

        void Start()
        {
            if (!board) board = FindObjectOfType<BoardController>();
            if (board && (board.tiles == null || board.tiles.Count == 0)) board.Generate();
            PlaceAt(index);
        }

        void Update(){ if (Input.GetKeyDown(KeyCode.R)) RollAndMove(); }

        public void RollAndMove()
        {
            if (moving || board == null || board.Perimeter <= 0) return;
            int steps = Random.Range(1, 7);
            StartCoroutine(MoveSteps(steps));
        }

        public void PlaceAt(int i)
        {
            if (!pawn || !board || board.Perimeter <= 0) return;
            index = BoardController.Mod(i, board.Perimeter);
            pawn.anchoredPosition = board.GetTilePosition(index);
        }

        IEnumerator MoveSteps(int steps)
        {
            moving = true;
            for (int s = 0; s < steps; s++)
            {
                int next = BoardController.Mod(index + 1, board.Perimeter);
                yield return TweenTo(board.GetTilePosition(next));
                index = next;
            }
            moving = false;
            onLanded?.Invoke(index);
        }

        IEnumerator TweenTo(Vector2 target)
        {
            if (!pawn) yield break;
            var start = pawn.anchoredPosition;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, stepDuration);
                float k = moveCurve.Evaluate(Mathf.Clamp01(t));
                pawn.anchoredPosition = Vector2.LerpUnclamped(start, target, k);
                yield return null;
            }
            pawn.anchoredPosition = target;
        }
    }
}
