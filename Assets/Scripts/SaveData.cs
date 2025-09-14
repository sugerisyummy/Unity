// SaveData.cs — 擴充多數值，完整序列化存檔
using System;

[Serializable]
public class SaveData
{
    public string currentCase;  // 例 "CityGate"

    // Core
    public int hp;
    public int difficulty;      // 0..3
    public string saveTime;     // yyyy/MM/dd HH:mm

    // Dystopia Stats
    public int money;
    public int sanity;
    public int hunger;
    public int thirst;
    public int fatigue;
    public int hope;
    public int obedience;
    public int reputation;         // or notoriety
    public int techParts;
    public int information;
    public int credits;
    public int augmentationLoad;
    public int radiation;          // or infection
    public int trust;
    public int control;            // surveillance
}
