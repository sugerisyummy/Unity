using System;

[Serializable]
public class SaveData
{
    public int hp, money, sanity;
    public string currentCase;
    public string saveTime; // "yyyy/MM/dd HH:mm"
    public int difficulty;  // 0..3

    public int hunger, thirst, fatigue, hope, obedience, reputation;
    public int techParts, information, credits, augmentationLoad, radiation, infection, trust, control;

    public string[] flagsTrue, onceConsumedKeys, cooldownKeys;
    public long[]   cooldownUntilUnix;

    public AbilityStats abilities = new AbilityStats();
}
