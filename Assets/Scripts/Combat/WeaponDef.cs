
using UnityEngine;

namespace CyberLife.Combat
{
    [CreateAssetMenu(fileName = "WeaponDef", menuName = "CyberLife/Combat/Weapon")]
    public class WeaponDef : ItemDef
    {
        [Header("Weapon")]
        public DamageType damageType = DamageType.Ballistic;
        [Min(0f)] public float baseDamage = 10f;
        [Range(0f,1f)] public float accuracy = 0.75f;
        [Range(0f,1f)] public float critChance = 0.1f;
        [Min(1f)] public float critMultiplier = 1.5f;

        [Header("Targeting Weights (6 groups, normalized at runtime)")]
        [Tooltip("Order: Head, Torso, LeftArm, RightArm, LeftLeg, RightLeg")]
        public float[] groupWeights = new float[] { 1.5f, 1.5f, 1.2f, 1.2f, 1.0f, 2.0f };

        public float RollDamage(System.Random rng)
        {
            var dmg = baseDamage * (0.85f + (float)rng.NextDouble() * 0.3f); // ±15%
            if ((float)rng.NextDouble() < critChance) dmg *= critMultiplier;
            return Mathf.Max(0f, dmg);
        }

        public HitGroup PickGroup(System.Random rng)
        {
            float[] w = groupWeights != null && groupWeights.Length == 6
                ? groupWeights
                : new float[] {1,4,2,2,1,1.2f};

            float sum = 0f;
            for (int i=0;i<6;i++) sum += Mathf.Max(0f, w[i]);
            if (sum <= 0f) return HitGroup.Torso;

            float r = (float)rng.NextDouble() * sum;
            for (int i=0;i<6;i++)
            {
                r -= Mathf.Max(0f, w[i]);
                if (r <= 0f) return (HitGroup)i;
            }
            return HitGroup.Torso;
        }
    }
}
