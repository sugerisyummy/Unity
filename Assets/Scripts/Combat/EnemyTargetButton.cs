using UnityEngine;

namespace CyberLife.Combat
{
    public class EnemyTargetButton : MonoBehaviour
    {
        public CombatUIController ui;
        public Combatant enemy;

        // 給 Button 的 OnClick 用：只鎖定
        public void Focus() { ui?.SelectTarget(enemy); }

        // 如果你想點一下就直接打：把 Button 綁這個
        public void FocusAndAttack() { if (ui==null || enemy==null) return; ui.SelectTarget(enemy); ui.AttackAuto(); }
    }
}
