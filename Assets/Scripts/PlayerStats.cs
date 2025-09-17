// PlayerStats.cs
using System;
using UnityEngine;

[Serializable]
public class PlayerStats
{
    // 核心
    public int hp = 100;
    public int money = 0;
    public int sanity = 100;

    // 反烏托邦擴充
    public int hunger = 0;
    public int thirst = 0;
    public int fatigue = 0;
    public int hope = 50;
    public int obedience = 50;
    public int reputation = 0;
    public int techParts = 0;
    public int information = 0;
    public int credits = 0;
    public int augmentationLoad = 0; // 0..100
    public int radiation = 0;        // 0..100
    public int infection = 0;        // 0..100   ← ★ 補上
    public int trust = 0;            // 0..100
    public int control = 0;          // 0..100

    [NonSerialized] public Action OnChanged;

    static int Clamp(int v, int min, int max) => Mathf.Clamp(v, min, max);

    public void ApplyDeltas(
        int hpDelta, int moneyDelta, int sanityDelta,
        int hungerDelta, int thirstDelta, int fatigueDelta,
        int hopeDelta, int obedienceDelta, int reputationDelta,
        int techPartsDelta, int informationDelta, int creditsDelta,
        int augmentationLoadDelta, int radiationDelta, int infectionDelta, // ★ 增參數
        int trustDelta, int controlDelta)
    {
        // No-Death 的基礎保護會交給 GameManager 做收尾，但 hp 不要炸到極端負數
        hp       = Clamp(hp + hpDelta,           -999999, 999999);
        money    = Clamp(money + moneyDelta,     -999999, 999999);
        sanity   = Clamp(sanity + sanityDelta,   -999999, 999999);

        hunger   = Clamp(hunger + hungerDelta,     0, 100);
        thirst   = Clamp(thirst + thirstDelta,     0, 100);
        fatigue  = Clamp(fatigue + fatigueDelta,   0, 100);
        hope     = Clamp(hope + hopeDelta,         0, 100);
        obedience= Clamp(obedience + obedienceDelta, 0, 100);
        reputation= Clamp(reputation + reputationDelta, -999999, 999999);

        techParts    = Mathf.Max(0, techParts + techPartsDelta);
        information  = Mathf.Max(0, information + informationDelta);
        credits      = Mathf.Max(0, credits + creditsDelta);

        augmentationLoad = Clamp(augmentationLoad + augmentationLoadDelta, 0, 100);
        radiation        = Clamp(radiation + radiationDelta,               0, 100);
        infection        = Clamp(infection + infectionDelta,               0, 100); // ★
        trust            = Clamp(trust + trustDelta,                       0, 100);
        control          = Clamp(control + controlDelta,                   0, 100);

        OnChanged?.Invoke();
    }

    public void ApplyChoiceDeltas(DolEventAsset.EventChoice ch)
    {
        if (ch == null) return;
        ApplyDeltas(
            ch.hpChange, ch.moneyChange, ch.sanityChange,
            ch.hungerChange, ch.thirstChange, ch.fatigueChange,
            ch.hopeChange, ch.obedienceChange, ch.reputationChange,
            ch.techPartsChange, ch.informationChange, ch.creditsChange,
            ch.augmentationLoadChange, ch.radiationChange, ch.infectionChange, // ★
            ch.trustChange, ch.controlChange
        );
    }

    // 存讀對接
    public void WriteTo(SaveData d)
    {
        d.hp = hp; d.money = money; d.sanity = sanity;
        d.hunger = hunger; d.thirst = thirst; d.fatigue = fatigue;
        d.hope = hope; d.obedience = obedience; d.reputation = reputation;
        d.techParts = techParts; d.information = information; d.credits = credits;
        d.augmentationLoad = augmentationLoad; d.radiation = radiation;
        d.infection = infection; // ★
        d.trust = trust; d.control = control;
    }
    public void ReadFrom(SaveData d)
    {
        hp = d.hp; money = d.money; sanity = d.sanity;
        hunger = d.hunger; thirst = d.thirst; fatigue = d.fatigue;
        hope = d.hope; obedience = d.obedience; reputation = d.reputation;
        techParts = d.techParts; information = d.information; credits = d.credits;
        augmentationLoad = d.augmentationLoad; radiation = d.radiation;
        infection = d.infection; // ★
        trust = d.trust; control = d.control;
        OnChanged?.Invoke();
    }
}
