using UnityEngine;

namespace CL.Combat
{
    [CreateAssetMenu(menuName = "CL/Combat/Items/Weapon")]
    public class WeaponDef : ItemDef
    {
        public WeaponCategory category = WeaponCategory.Blade;

        [Header("命中/速度")]
        public int accuracy = 0;     // 命中修正
        public int speed = 0;        // 速度修正

        [Header("基礎傷害")]
        public int baseDamage = 8;
        public int armorPenetration = 0;

        [Header("傷害型別比例（0~1，加總<=1，剩餘視為鈍擊）")]
        [Range(0,1)] public float slash = 0.5f;     // 刀
        [Range(0,1)] public float pierce = 0.3f;    // 槍（彈道）可用 pierce 或 ballistic
        [Range(0,1)] public float thermal = 0.0f;   // 熱能
        [Range(0,1)] public float chemical = 0.0f;  // 化學
        [Range(0,1)] public float ballistic = 0.0f; // 彈道

        [Header("偏好攻擊部位（可空）")]
        public BodyPartId[] preferredParts;
    }
}
