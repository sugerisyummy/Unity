// Auto-generated replacement by ChatGPT (Effects core)
using UnityEngine;
using System;
using System.Collections.Generic;

namespace CyberLife.Combat
{
    [CreateAssetMenu(fileName = "EffectDef", menuName = "CyberLife/Combat/Effect")]
    public class EffectDef : ScriptableObject
    {
        public string effectId;
        public EffectTag tag = EffectTag.None;
        [Min(0f)] public float duration = 5f;
        [Min(0f)] public float tickInterval = 1f;
        [Min(0f)] public float tickDamage = 0f;
        public DamageType damageType = DamageType.Chemical; // for poison by default
    }

    [Serializable]
    public class EffectInstance
    {
        public EffectDef def;
        public float timeLeft;
        private float nextTick;
        public EffectInstance(EffectDef def)
        {
            this.def = def;
            this.timeLeft = def != null ? def.duration : 0f;
            this.nextTick = def != null ? def.tickInterval : 9999f;
        }

        public void Tick(Combatant target, float deltaTime)
        {
            if (def == null || target == null) return;
            timeLeft -= deltaTime;
            nextTick -= deltaTime;
            if (nextTick <= 0f)
            {
                nextTick += Mathf.Max(0.01f, def.tickInterval);
                if (def.tickDamage > 0f)
                {
                    // Spread small damage to torso by default
                    target.ApplyDirectDamage(HitGroup.Torso, def.damageType, def.tickDamage);
                }
            }
        }

        public bool Expired => timeLeft <= 0f;
    }
}