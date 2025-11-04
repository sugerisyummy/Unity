using UnityEngine;
using UnityEngine.Events;
using PawnController = Game.Board.PawnController;

namespace Game.Board
{
    /// <summary>
    /// 橋接棋盤移動結果到 UI/事件系統。提供強制事件頁模式。
    /// </summary>
    public sealed class BoardEventsBridge : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private BoardController board;
        [SerializeField] private PawnController pawn;

        [Header("Events")]
        public UnityEvent<string> onRequestEvent;
        public UnityEvent<int> onRequestCombat;

        [Header("Force Special Event")]
        public bool forceSpecialEveryLanding = true;
        [Tooltip("落地後要顯示的事件頁(例如 Canvas/StoryPanel 或你自己的事件頁)")]
        public GameObject specialEventPage;
        [Tooltip("棋盤面板(例如 Canvas/BoardPanel)。會在顯示事件頁時關閉，避免疊在一起。")]
        public GameObject boardPanelToHide;

        private void OnEnable()
        {
            if (pawn)
            {
                pawn.onLanded.AddListener(HandleLanded);
            }
        }

        private void OnDisable()
        {
            if (pawn)
            {
                pawn.onLanded.RemoveListener(HandleLanded);
            }
        }

        private void HandleLanded(int tileIndex)
        {
            if (forceSpecialEveryLanding)
            {
                if (boardPanelToHide)
                {
                    boardPanelToHide.SetActive(false);
                }

                if (specialEventPage)
                {
                    specialEventPage.SetActive(true);
                }

                onRequestEvent?.Invoke("SPECIAL_FORCED");
                return;
            }

            if (board && board.Perimeter > 0 && (tileIndex + 1) % 5 == 0)
            {
                onRequestCombat?.Invoke(1);
            }
            else
            {
                onRequestEvent?.Invoke($"TILE_{tileIndex}");
            }
        }
    }
}
