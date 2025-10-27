using UnityEngine;
using UnityEngine.Events;

namespace CyberLife.Board
{
    // 取代/覆蓋原先的 BoardEventsBridge：新增「每次落地 100% 進事件頁」
    public class BoardEventsBridge : MonoBehaviour
    {
        [Header("Refs")]
        public BoardController board;
        public PawnController  pawn;

        [Header("Events")]
        public UnityEvent<string> onRequestEvent;
        public UnityEvent<int>    onRequestCombat;

        [Header("Force Special Event")]
        public bool forceSpecialEveryLanding = true;
        [Tooltip("落地後要顯示的事件頁(例如 Canvas/StoryPanel 或你自己的事件頁)")]
        public GameObject specialEventPage;
        [Tooltip("棋盤面板(例如 Canvas/BoardPanel)。會在顯示事件頁時關閉，避免疊在一起。")]
        public GameObject boardPanelToHide;

        void OnEnable() { if (pawn) pawn.onLanded.AddListener(HandleLanded); }
        void OnDisable(){ if (pawn) pawn.onLanded.RemoveListener(HandleLanded); }

        void HandleLanded(int tileIndex)
        {
            if (forceSpecialEveryLanding)
            {
                // 切到事件頁
                if (boardPanelToHide) boardPanelToHide.SetActive(false);
                if (specialEventPage) specialEventPage.SetActive(true);
                // 仍然發一個事件 key，讓你有需要時可以顯示內容
                onRequestEvent?.Invoke("SPECIAL_FORCED");
                return;
            }

            // 原先的邏輯(範例)：5 的倍數進戰鬥，其餘普通事件
            if (board && board.Perimeter > 0 && (tileIndex + 1) % 5 == 0)
                onRequestCombat?.Invoke(1);
            else
                onRequestEvent?.Invoke($"TILE_{tileIndex}");
        }
    }
}
