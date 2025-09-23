// Auto-generated replacement by ChatGPT (BodyPartState)
using System;
using UnityEngine;

namespace CyberLife.Combat
{
    [Serializable]
    public class BodyPartState
    {
        [Tooltip("Unique id like 'Head', 'LeftArm', etc.")]
        public string id;

        [Tooltip("Bucket used for group targeting.")]
        public BodyTag tag = BodyTag.Misc;

        [Min(1)] public float maxHP = 10f;
        public float hp = 10f;

        public bool Destroyed => hp <= 0f;

        public BodyPartState(){}

        public BodyPartState(string id, BodyTag tag, float max)
        {
            this.id = id;
            this.tag = tag;
            this.maxHP = Mathf.Max(1f, max);
            this.hp = this.maxHP;
        }

        /// <summary>Apply raw damage after armor mitigation has been computed.</summary>
        public float ApplyDamage(float dmg)
        {
            if (Destroyed) return 0f;
            var before = hp;
            hp = Mathf.Max(0f, hp - Mathf.Max(0f, dmg));
            return before - hp;
        }

        public void Heal(float value)
        {
            if (value <= 0f) return;
            hp = Mathf.Min(maxHP, hp + value);
        }

        public HitGroup ToHitGroup()
        {
            switch (tag)
            {
                case BodyTag.Head: return HitGroup.Head;
                case BodyTag.Torso: return HitGroup.Torso;
                case BodyTag.Arm: return HitGroup.Arms;
                case BodyTag.Leg: return HitGroup.Legs;
                case BodyTag.Vital: return HitGroup.Vital;
                default: return HitGroup.Misc;
            }
        }
    }
}