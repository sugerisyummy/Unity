// Auto-generated replacement by ChatGPT (CombatManager partial - armor hook & helpers)
using UnityEngine;

namespace CyberLife.Combat
{
    public partial class CombatManager
    {
        public float ResolveDamageWithArmor(Combatant defender, HitGroup group, DamageType type, float rawDamage)
        {
            if (defender == null) return rawDamage;
            var armor = defender.inventory != null ? defender.inventory.armor : null;
            if (armor == null) return rawDamage;
            return armor.Mitigate(group, type, rawDamage);
        }
    }
}