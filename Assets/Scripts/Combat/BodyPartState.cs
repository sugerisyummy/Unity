using System;
using UnityEngine;

namespace CL.Combat
{
    [Serializable]
    public class BodyPartState
    {
        public BodyPartId id;
        [Min(1)] public int maxHP = 10;
        [Min(0)] public int hp = 10;

        [Range(0,100)] public int pain;          // 疼痛（%）
        [Min(0)] public float bleedRate;         // 每回合流血量
        public bool isBroken;                    // 骨折
        public bool isSevered;                   // 斷肢/器官致命破壞

        public bool Vital => id == BodyPartId.Brain || id == BodyPartId.Heart;
        public bool IsDestroyed => hp <= 0 || isSevered;
    }
}
