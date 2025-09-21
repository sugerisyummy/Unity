using UnityEngine;

namespace CL.Combat
{
    [CreateAssetMenu(menuName = "CL/Combat/Items/Consumable")]
    public class ConsumableDef : ItemDef
    {
        [Header("治療/藥劑效果（單次）")]
        public int healBodyHP = 0;     // 單一部位回耐久（配合使用時指定）
        public int healAllHP = 0;      // 全身每部位回耐久
        public int painDown = 0;       // 疼痛下降
        public float stopBleedAmount = 0f; // 流血速率下降
        public string cureTag;         // 可解某效果（例："poison","burn"）
    }
}
