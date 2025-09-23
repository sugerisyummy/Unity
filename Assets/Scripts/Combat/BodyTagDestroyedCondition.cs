using UnityEngine;
using System.Linq;

namespace CyberLife.Combat
{
    [CreateAssetMenu(fileName="BodyTagDestroyedCondition", menuName="CyberLife/Combat/Special/BodyTagDestroyed")]
    public class BodyTagDestroyedCondition : CombatSpecialCondition
    {
        public BodyTag[] requireDestroyed;      // 例：Head 或 Vital
        public bool requireWin = true;          // 只在勝利時判定

        public override bool Evaluate(CombatManager cm, Combatant player, Combatant enemy, CombatOutcome outcome)
        {
            if (requireWin && outcome != CombatOutcome.Win) return false;
            if (enemy == null || enemy.parts == null || requireDestroyed == null || requireDestroyed.Length == 0) return false;
            foreach (var tag in requireDestroyed)
            {
                bool anyDestroyed = enemy.parts.Any(p => p.tag == tag && p.Destroyed);
                if (!anyDestroyed) return false; // 有一個沒達成就否
            }
            return true;
        }
    }
}
