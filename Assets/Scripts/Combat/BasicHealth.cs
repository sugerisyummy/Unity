// BasicHealth.cs — 最小生命值組件
using UnityEngine;

namespace CyberLife.Combat
{
    public class BasicHealth : MonoBehaviour
    {
        public float MaxHP = 20f;
        public float HP = 20f;
        public bool IsAlive => HP > 0f;
        public void ApplyDamage(HitGroup group, DamageType type, float amount)
        {
            HP = Mathf.Max(0f, HP - Mathf.Max(0f, amount));
        }
    }
}
