using UnityEngine;
using UnityEngine.Events;

namespace CyberLife.Combat
{
    /// 戰鬥＝獨立面板。切到 CombatPanel→監看結果→回故事面板（Win/Lose/Special）。
    public class CombatPageController : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject storyPanel;
        public GameObject combatPanel;
        [Tooltip("同層面板的共同父物件（會把兄弟面板全關，只開目標）。可留空。")]
        public Transform panelsRoot;

        [Header("Combat")]
        public CombatManager manager;          // 放在 CombatPanel 裡
        public Combatant player;               // CombatPanel 裡玩家
        public Combatant enemy;                // CombatPanel 裡敵人
        public CombatSpecialCondition special; // 可選：特殊條件

        [Header("Result hooks")]
        public UnityEvent onWin;
        public UnityEvent onLose;
        public UnityEvent onSpecial;

        bool running;

        public void StartCombat()
        {
            if (manager == null) manager = combatPanel ? combatPanel.GetComponentInChildren<CombatManager>(true) : null;
            if (player == null)  player  = combatPanel ? combatPanel.GetComponentInChildren<Combatant>(true) : null;
            if (enemy == null)
            {
                var found = combatPanel ? combatPanel.GetComponentsInChildren<Combatant>(true) : null;
                if (found != null && found.Length > 0)
                    foreach (var c in found) if (c != null && c != player) { enemy = c; break; }
                if (enemy == null)
                {
                    var go = GameObject.FindGameObjectWithTag("Enemy");
                    if (go) enemy = go.GetComponent<Combatant>();
                }
            }

            if (manager == null || player == null || enemy == null)
            {
                Debug.LogError("[CombatPageController] 缺引用：manager/player/enemy。");
                return;
            }

            ShowOnly(combatPanel);
            manager.BeginCombat(new[] { player }, new[] { enemy });
            running = true;
        }

        void Update()
        {
            if (!running || manager == null) return;
            if (!manager.finalOutcome.HasValue) return;

            var outcome = manager.finalOutcome.Value;
            bool hitSpecial = (special != null && special.Evaluate(manager, player, enemy, outcome));

            if (hitSpecial) onSpecial?.Invoke();
            else
            {
                switch (outcome)
                {
                    case CombatOutcome.Win:    onWin?.Invoke();  break;
                    case CombatOutcome.Lose:   onLose?.Invoke(); break;
                    case CombatOutcome.Escape: onLose?.Invoke(); break; // 先當成 Lose，需要再分流就再加
                }
            }

            ShowOnly(storyPanel);
            running = false;
            manager.finalOutcome = null;
            manager.isActiveCombat = false;
        }

        void ShowOnly(GameObject panel)
        {
            if (panel == null) return;
            Transform root = panelsRoot != null ? panelsRoot : panel.transform.parent;
            if (root != null)
                foreach (Transform t in root) t.gameObject.SetActive(false);
            panel.SetActive(true);
        }
    }
}
