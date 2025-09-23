using UnityEngine;
using TMPro;

namespace CyberLife.Combat
{
    public class CombatUIController : MonoBehaviour
    {
        public CombatManager manager;
        public int allyIndex = 0;              // 先用第0位玩家
        public Combatant currentTarget;
        public TMP_Text targetLabel;           // 可選：顯示鎖定對象名字

        public void SelectTarget(Combatant target)
        {
            currentTarget = target;
            if (targetLabel) targetLabel.text = target ? target.displayName : "-";
        }

        public void AttackAuto()
        {
            if (currentTarget == null || manager == null) return;
            manager.PlayerAttackTarget(currentTarget); // 用武器權重自選群組
        }

        public void AttackWithGroup(int groupIndex)
        {
            if (currentTarget == null || manager == null) return;
            groupIndex = Mathf.Clamp(groupIndex, 0, 5);
            manager.PlayerAttackTargetWithGroup(currentTarget, (HitGroup)groupIndex);
        }
    }
}
