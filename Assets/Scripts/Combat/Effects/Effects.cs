using System;
using UnityEngine;

namespace CL.Combat
{
    [CreateAssetMenu(menuName = "CL/Combat/Effects/Effect")]
    public class EffectDef : ScriptableObject
    {
        public string effectId;
        public bool positive;
        public string[] tags;           // 例：burn, poison, stim
        public int durationTurns = 3;

        [Header("每回合影響")]
        public int painPerTurn = 0;
        public int hpLossPerTurn = 0;      // 對全身隨機部位
        public float bleedAddPerTurn = 0f; // 增加流血速率
        public int speedDelta = 0;         // 臨時速度改變
        public int defenseDelta = 0;       // 臨時防禦改變
    }

    [Serializable]
    public class StatusEffect
    {
        public EffectDef def;
        public int remaining;

        public bool MatchesTag(string tag)
        {
            if (def == null || def.tags == null) return false;
            foreach (var t in def.tags) if (t == tag) return true;
            return false;
        }
    }
}
