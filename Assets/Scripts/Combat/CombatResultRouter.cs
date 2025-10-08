using UnityEngine;
using UnityEngine.Events;

namespace CyberLife.Combat
{
    public class CombatResultRouter : MonoBehaviour
    {
        public CombatManager manager;
        public UnityEvent onWin, onLose, onEscape;

        // 給 Bridge 呼叫
        public void Route(CombatOutcome outcome)
        {
            switch (outcome)
            {
                case CombatOutcome.Win:    onWin?.Invoke(); break;
                case CombatOutcome.Lose:   onLose?.Invoke(); break;
                case CombatOutcome.Escape: onEscape?.Invoke(); break;
            }
            enabled = false; // 觸發一次就關掉
        }

        // 可選：沒有 Bridge 時用輪詢
        void Update()
        {
            if (!enabled || manager == null) return;
            if (manager.finalOutcome == CombatOutcome.None) return;
            Route(manager.finalOutcome); // 注意：這裡用 enum，不是 HasValue/Value
        }
    }
}
