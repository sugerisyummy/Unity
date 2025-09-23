// Auto-generated replacement by ChatGPT (Combatant)
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CyberLife.Combat
{
    public class Combatant : MonoBehaviour
    {
        [Header("Identity")]
        public string displayName = "Unit";

        [Header("Body Model")]
        public List<BodyPartState> parts = new List<BodyPartState>();

        [Header("Systems")]
        public InventoryManager inventory;
        public List<EffectInstance> effects = new List<EffectInstance>();

        public bool IsAlive
        {
            get
            {
                // Alive if any torso HP > 0 and we still have any non-destroyed vital/torso/head
                bool torsoAlive = parts.Any(p => p.tag == BodyTag.Torso && !p.Destroyed);
                bool headDestroyed = parts.Where(p => p.tag == BodyTag.Head).All(p => p.Destroyed);
                return torsoAlive && !headDestroyed;
            }
        }

        public float TotalHP => parts.Sum(p => p.hp);
        public float TotalMaxHP => parts.Sum(p => p.maxHP);

        public void EnsureDefaultBody()
        {
            if (parts != null && parts.Count > 0) return;
            // Minimal 22-ish model approximated into buckets
            parts = new List<BodyPartState>{
                new BodyPartState("Head", BodyTag.Head, 15),
                new BodyPartState("Neck", BodyTag.Vital, 8),
                new BodyPartState("Chest", BodyTag.Torso, 30),
                new BodyPartState("Abdomen", BodyTag.Torso, 25),
                new BodyPartState("LeftArm", BodyTag.Arm, 15),
                new BodyPartState("RightArm", BodyTag.Arm, 15),
                new BodyPartState("LeftForearm", BodyTag.Arm, 12),
                new BodyPartState("RightForearm", BodyTag.Arm, 12),
                new BodyPartState("LeftHand", BodyTag.Arm, 8),
                new BodyPartState("RightHand", BodyTag.Arm, 8),
                new BodyPartState("LeftThigh", BodyTag.Leg, 18),
                new BodyPartState("RightThigh", BodyTag.Leg, 18),
                new BodyPartState("LeftCalf", BodyTag.Leg, 14),
                new BodyPartState("RightCalf", BodyTag.Leg, 14),
                new BodyPartState("LeftFoot", BodyTag.Leg, 8),
                new BodyPartState("RightFoot", BodyTag.Leg, 8),
                new BodyPartState("Heart", BodyTag.Vital, 10),
                new BodyPartState("LungL", BodyTag.Vital, 10),
                new BodyPartState("LungR", BodyTag.Vital, 10),
                new BodyPartState("Liver", BodyTag.Vital, 9),
                new BodyPartState("Stomach", BodyTag.Vital, 9),
                new BodyPartState("Spine", BodyTag.Vital, 12),
            };
        }

        public BodyPartState PickRandomPart(HitGroup group, System.Random rng)
        {
            var bucket = parts.Where(p => p.ToHitGroup() == group && !p.Destroyed).ToList();
            if (bucket.Count == 0) bucket = parts.Where(p => !p.Destroyed).ToList();
            if (bucket.Count == 0) return null;
            int idx = rng.Next(0, bucket.Count);
            return bucket[idx];
        }

        public void ApplyDirectDamage(HitGroup bucket, DamageType type, float rawDmg)
        {
            var rng = new System.Random();
            var part = PickRandomPart(bucket, rng);
            if (part == null) return;

            float mitigated = rawDmg;
            if (inventory != null && inventory.armor != null)
                mitigated = inventory.armor.Mitigate(bucket, type, rawDmg);

            part.ApplyDamage(mitigated);
        }

        public void CureByTags(EffectTag[] tags)
        {
            if (effects == null || tags == null) return;
            effects.RemoveAll(e => e != null && e.def != null && System.Array.IndexOf(tags, e.def.tag) >= 0);
        }

        public void HealAll(float value)
        {
            foreach (var p in parts) p.Heal(value);
        }

        void Awake()
        {
            EnsureDefaultBody();
            if (inventory == null) inventory = GetComponent<InventoryManager>();
        }

        public void TickEffects(float deltaTime)
        {
            if (effects == null) return;
            foreach (var e in effects) e?.Tick(this, deltaTime);
            effects.RemoveAll(e => e == null || e.Expired);
        }
    }
}