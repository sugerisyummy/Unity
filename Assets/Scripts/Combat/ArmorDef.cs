// Auto-generated replacement by ChatGPT (Armor definition & mitigation)
using UnityEngine;

namespace CyberLife.Combat
{
    [CreateAssetMenu(fileName = "ArmorDef", menuName = "CyberLife/Combat/Armor")]
    public class ArmorDef : ItemDef
    {
        [Header("Flat DR per damage type")]
        public float ballisticDR = 2f;
        public float slashDR = 1f;
        public float bluntDR = 1f;
        public float thermalDR = 0f;
        public float chemicalDR = 0f;
        public float energyDR = 0f;

        [Header("Multipliers by body bucket")]
        [Tooltip("Order: Head, Torso, Arms, Legs, Vital, Misc")]
        public float[] bucketMultiplier = new float[] { 1.0f, 0.7f, 0.85f, 0.85f, 0.8f, 1.0f };

        public float Mitigate(HitGroup bucket, DamageType type, float incoming)
        {
            float flat = 0f;
            switch (type)
            {
                case DamageType.Ballistic: flat = ballisticDR; break;
                case DamageType.Slash: flat = slashDR; break;
                case DamageType.Blunt: flat = bluntDR; break;
                case DamageType.Thermal: flat = thermalDR; break;
                case DamageType.Chemical: flat = chemicalDR; break;
                case DamageType.Energy: flat = energyDR; break;
            }
            float afterFlat = Mathf.Max(0f, incoming - Mathf.Max(0f, flat));
            float mult = 1f;
            if (bucketMultiplier != null && bucketMultiplier.Length == 6)
                mult = bucketMultiplier[(int)bucket];
            return Mathf.Max(0f, afterFlat * mult);
        }
    }
}