using UnityEngine;

namespace CyberLife.Combat
{
    public abstract class CombatSpecialCondition : ScriptableObject
    {
        // 回傳 true 就走 onSpecial
        public abstract bool Evaluate(CombatManager cm, Combatant player, Combatant enemy, CombatOutcome outcome);
    }
}
