using System;
using UnityEngine;

public enum AbilityStatType
{
    Strength,   // 力量：硬闖/承受/搬運
    Agility,    // 敏捷：閃避/追擊/跑酷
    Intellect,  // 智力：解謎/研發/判讀資訊
    Charisma,   // 魅力：交涉/恐嚇/說服
    Stealth,    // 潛行：潛入/躲避監控
    Tech        // 科技：駭入/修復/改造
}

[Serializable]
public class AbilityStats
{
    [Range(0,100)] public int strength = 30;
    [Range(0,100)] public int agility  = 30;
    [Range(0,100)] public int intellect= 30;
    [Range(0,100)] public int charisma = 30;
    [Range(0,100)] public int stealth  = 30;
    [Range(0,100)] public int tech     = 30;

    public int Get(AbilityStatType t)
    {
        switch (t)
        {
            case AbilityStatType.Strength:  return strength;
            case AbilityStatType.Agility:   return agility;
            case AbilityStatType.Intellect: return intellect;
            case AbilityStatType.Charisma:  return charisma;
            case AbilityStatType.Stealth:   return stealth;
            case AbilityStatType.Tech:      return tech;
        }
        return 0;
    }

    public float Get01(AbilityStatType t) => Mathf.Clamp01(Get(t) / 100f);

    public void Set(AbilityStatType t, int v)
    {
        v = Mathf.Clamp(v, 0, 100);
        switch (t)
        {
            case AbilityStatType.Strength:  strength  = v; break;
            case AbilityStatType.Agility:   agility   = v; break;
            case AbilityStatType.Intellect: intellect = v; break;
            case AbilityStatType.Charisma:  charisma  = v; break;
            case AbilityStatType.Stealth:   stealth   = v; break;
            case AbilityStatType.Tech:      tech      = v; break;
        }
    }
}
