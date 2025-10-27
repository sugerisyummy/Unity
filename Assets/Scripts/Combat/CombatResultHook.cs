using UnityEngine;
using UnityEngine.Events;
using CyberLife.UI;

namespace CyberLife.Combat
{
    // 把戰鬥結束事件轉給 BoardEventRouter 顯示結果
    public class CombatResultHook : MonoBehaviour
    {
        public BoardEventRouter router;
        public UnityEvent<bool> onCombatFinished; // true=win, false=lose

        public void CombatFinished(bool win)
        {
            if (!router) router = FindObjectOfType<BoardEventRouter>(true);
            if (router) router.ShowCombatResult(win);
            onCombatFinished?.Invoke(win);
        }
    }
}
