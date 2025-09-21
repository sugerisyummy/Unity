using UnityEngine;

namespace CL.Combat
{
    // 物品基底（SO）
    public abstract class ItemDef : ScriptableObject
    {
        public string displayName;
        public float weight = 1f;
        [TextArea] public string description;
    }
}
