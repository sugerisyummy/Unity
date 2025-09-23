// Auto-generated replacement by ChatGPT (Item base definition)
using UnityEngine;

namespace CyberLife.Combat
{
    public abstract class ItemDef : ScriptableObject
    {
        [Header("Common")]
        public string itemId;
        public string displayName;
        public float weight = 0f;
        [TextArea] public string description;
    }
}