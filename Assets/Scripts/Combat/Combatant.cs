
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
                // Alive if any torso HP > 0 and head未全部破壞
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

            // 22 部位粗分到 6 群（Left/Right Arm/Leg、Head、Torso，其餘 vital 歸 Torso）
            parts = new List<BodyPartState>{
                new BodyPartState("Head", BodyTag.Head, 15),
                new BodyPartState("Neck", BodyTag.Torso, 8),
                new BodyPartState("Chest", BodyTag.Torso, 30),
                new BodyPartState("Abdomen", BodyTag.Torso, 25),

                new BodyPartState("LeftArm", BodyTag.LeftArm, 15),
                new BodyPartState("RightArm", BodyTag.RightArm, 15),
                new BodyPartState("LeftForearm", BodyTag.LeftArm, 12),
                new BodyPartState("RightForearm", BodyTag.RightArm, 12),
                new BodyPartState("LeftHand", BodyTag.LeftArm, 8),
                new BodyPartState("RightHand", BodyTag.RightArm, 8),

                new BodyPartState("LeftThigh", BodyTag.LeftLeg, 18),
                new BodyPartState("RightThigh", BodyTag.RightLeg, 18),
                new BodyPartState("LeftCalf", BodyTag.LeftLeg, 14),
                new BodyPartState("RightCalf", BodyTag.RightLeg, 14),
                new BodyPartState("LeftFoot", BodyTag.LeftLeg, 8),
                new BodyPartState("RightFoot", BodyTag.RightLeg, 8),

                // 內臟 → 歸 Torso（命中以 Torso 群處理）
                new BodyPartState("Heart", BodyTag.Torso, 10),
                new BodyPartState("LungL", BodyTag.Torso, 10),
                new BodyPartState("LungR", BodyTag.Torso, 10),
                new BodyPartState("Liver", BodyTag.Torso, 9),
                new BodyPartState("Stomach", BodyTag.Torso, 9),
                new BodyPartState("Spine", BodyTag.Torso, 12),
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
