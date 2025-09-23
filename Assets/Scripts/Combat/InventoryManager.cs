// Auto-generated replacement by ChatGPT (very small inventory for combat)
using UnityEngine;

namespace CyberLife.Combat
{
    public class InventoryManager : MonoBehaviour
    {
        public WeaponDef primaryWeapon;
        public ArmorDef armor;

        System.Random rng = new System.Random();

        public float RollAttackDamage()
        {
            if (primaryWeapon == null) return 1f;
            return primaryWeapon.RollDamage(rng);
        }

        public HitGroup PickGroup()
        {
            if (primaryWeapon == null) return HitGroup.Torso;
            return primaryWeapon.PickGroup(rng);
        }

        public DamageType DamageType => primaryWeapon != null ? primaryWeapon.damageType : DamageType.Blunt;
    }
}