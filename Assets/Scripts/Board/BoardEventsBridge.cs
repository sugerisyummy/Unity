using UnityEngine;

namespace Game.Board
{
    // Bridge：從棋盤落地 → 決定走事件或戰鬥 → 呼叫 UI 路由
    public sealed class BoardEventsBridge : MonoBehaviour
    {
        [Header("Router (Game.UI.BoardEventRouter)")]
        [SerializeField] private Game.UI.BoardEventRouter router;

        [Header("Simple Rules")]
        [Tooltip("每隔幾格觸發戰鬥；0 表示永不自動戰鬥")]
        [SerializeField] private int combatEveryN = 5;
        [Tooltip("從第幾格開始計數（含）；可為 0")]
        [SerializeField] private int combatStartIndex = 0;

        // 由 PawnController 在落地時呼叫
        public void OnPawnLanded(int index)
        {
            if (router == null) return;

            if (combatEveryN > 0 && (index - combatStartIndex) >= 0 &&
                ((index - combatStartIndex) % combatEveryN) == 0)
            {
                router.ShowCombat();
            }
            else
            {
                router.ShowEvent();
            }
        }

        // UI 按鈕可直接綁這些
        public void ShowBoard()  => router?.ShowBoard();
        public void ShowEvent()  => router?.ShowEvent();
        public void ShowCombat() => router?.ShowCombat();
    }
}
