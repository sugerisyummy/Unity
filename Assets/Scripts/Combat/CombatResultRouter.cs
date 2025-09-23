using UnityEngine;
using UnityEngine.Events;

namespace CyberLife.Combat
{
    /// <summary>
    /// 輕量輪詢：看到 CombatManager.finalOutcome 有值就丟出 UnityEvent。
    /// 不改你現在的 CombatManager。
    /// </summary>
    public class CombatResultRouter : MonoBehaviour
    {
        public CombatManager manager;

        [Header("Events")]
        public UnityEvent onWin;
        public UnityEvent onLose;
        public UnityEvent onEscape;

        void Update()
        {
            if (manager == null || !manager.finalOutcome.HasValue) return;

            switch (manager.finalOutcome.Value)
            {
                case CombatOutcome.Win:   onWin?.Invoke();   break;
                case CombatOutcome.Lose:  onLose?.Invoke();  break;
                case CombatOutcome.Escape:onEscape?.Invoke();break;
            }

            // 觸發一次就收
            enabled = false;
        }
    }
}
