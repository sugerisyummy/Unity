// Auto-generated replacement by ChatGPT (Consumable definition)
using UnityEngine;

namespace CyberLife.Combat
{
    [CreateAssetMenu(fileName = "ConsumableDef", menuName = "CyberLife/Combat/Consumable")]
    public class ConsumableDef : ItemDef
    {
        [Header("Healing & Cleansing")]
        public float healAmount = 0f;
        public EffectTag[] cureTags;

        public void Apply(Combatant target)
        {
            if (target == null) return;
            if (healAmount > 0f) target.HealAll(healAmount);
            if (cureTags != null && cureTags.Length > 0)
                target.CureByTags(cureTags);
        }
    }
}