using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DOL/Event")]
public class DolEventAsset : ScriptableObject
{
    [Header("基本")]
    public string eventId;
    [TextArea] public string title;
    [TextArea] public string description;

    [Header("觸發條件 / 限制")]
    [Min(0f)] public float weight = 1f;     // 供 CaseDatabase 覆寫，或直接使用
    public bool oncePerSave = false;
    public float cooldownSeconds = 0f;
    public int  minHP = 0;                  // 低於此 HP 不觸發（給 ConditionsMet 用）
    public List<string> requireFlags = new();
    public List<string> forbidFlags  = new();

    [Header("依能力加權（抽事件機率）")]
    public List<AbilityWeightMod> weightByAbility = new();

    [Header("多頁劇情")]
    public List<EventStage> stages = new(); // 0 為起點

[Serializable]
public class EventStage
{
    [TextArea] public string text;
    public List<EventChoice> choices = new();
}

[Serializable]
public class EventChoice
{
    public string text;

    // === 事件→戰鬥 ===
    [Header("Combat (事件→戰鬥)")]
    public bool startsCombat = false;
    public CL.Combat.CombatEncounter combat;

    [Header("戰鬥結束後跳頁（-1 = 不指定）")]
    public int nextStageOnWin = -1;
    public int nextStageOnLose = -1;
    public int nextStageOnEscape = -1;

    [Header("旗標（可選）")]
    public string onWinFlag;
    public string onLoseFlag;

    // === 原有核心數值 ===
    public int hpChange;
    public int moneyChange;
    public int sanityChange;

    // === Dystopia 擴充所有數值（名稱需與 PlayerStats 對齊） ===
    public int hungerChange;
    public int thirstChange;
    public int fatigueChange;
    public int hopeChange;
    public int obedienceChange;
    public int reputationChange;
    public int techPartsChange;
    public int informationChange;
    public int creditsChange;
    public int augmentationLoadChange;
    public int radiationChange;
    public int infectionChange;
    public int trustChange;
    public int controlChange;

    // 技能檢定（可多個）
    public List<SkillCheck> skillChecks = new();

    // 跳轉
    public int  nextStage = -1;
    public bool endEvent = false;
    public bool gotoCaseAfterEnd = false;
    public CaseId gotoCase = CaseId.None;

    // 旗標
    public List<string> setFlagsTrue  = new();
    public List<string> setFlagsFalse = new();
}

    [Serializable]
    public class AbilityWeightMod
    {
        public AbilityStatType stat;
        [Tooltip("輸入=能力(0..1)；輸出=倍率增量（例：0→-0.5，1→+0.5）")]
        public AnimationCurve influence = AnimationCurve.Linear(0, 0, 1, 0);
        public float scale = 1f; // 結果 = weight * (1 + influence(a01)*scale)
    }

    [Serializable]
    public class SkillCheck
    {
        public AbilityStatType stat;
        [Range(0,100)] public int threshold = 40;
        [Range(0,50)]  public int diceBonus  = 0;

        public int successNextStage = -1;
        public int failNextStage    = -1;
        [TextArea] public string successAppendText;
        [TextArea] public string failAppendText;

        public bool hideIfBelowThreshold = false;

        // 成功/失敗附加變更
        public int hpOnSuccess, hpOnFail;
        public int moneyOnSuccess, moneyOnFail;
        public int sanityOnSuccess, sanityOnFail;

        public int hungerOnSuccess, hungerOnFail;
        public int thirstOnSuccess, thirstOnFail;
        public int fatigueOnSuccess, fatigueOnFail;
        public int hopeOnSuccess, hopeOnFail;
        public int obedienceOnSuccess, obedienceOnFail;
        public int reputationOnSuccess, reputationOnFail;
        public int techPartsOnSuccess, techPartsOnFail;
        public int informationOnSuccess, informationOnFail;
        public int creditsOnSuccess, creditsOnFail;
        public int augmentationLoadOnSuccess, augmentationLoadOnFail;
        public int radiationOnSuccess, radiationOnFail;
        public int infectionOnSuccess, infectionOnFail;
        public int trustOnSuccess, trustOnFail;
        public int controlOnSuccess, controlOnFail;
    }

    // === 計算抽取權重（若有能力加權，再乘到 weight 上） ===
    public float GetEffectiveWeight(AbilityStats abilities)
    {
        float w = Mathf.Max(0f, weight);
        if (w <= 0f) return 0f;

        if (weightByAbility != null)
        {
            foreach (var mod in weightByAbility)
            {
                if (mod == null) continue;
                float a01 = abilities != null ? abilities.Get01(mod.stat) : 0f;
                float delta = mod.influence.Evaluate(a01) * mod.scale;
                w *= Mathf.Max(0f, 1f + delta);
            }
        }
        return w;
    }

    // === 觸發條件（供 GameManager/HasAvailableInCase 使用） ===
    public bool ConditionsMet(int hp, Func<string, bool> hasFlag)
    {
        if (hp < minHP) return false;
        if (requireFlags != null)
            foreach (var k in requireFlags) if (!string.IsNullOrEmpty(k) && (hasFlag == null || !hasFlag(k))) return false;
        if (forbidFlags != null)
            foreach (var k in forbidFlags)  if (!string.IsNullOrEmpty(k) && (hasFlag != null && hasFlag(k))) return false;
        return true;
    }

    // （可選）能力不足隱藏選項
    public bool IsChoiceVisibleByAbility(EventChoice c, AbilityStats abilities)
    {
        if (c == null || c.skillChecks == null) return true;
        foreach (var sc in c.skillChecks)
        {
            if (sc.hideIfBelowThreshold && abilities != null && abilities.Get(sc.stat) < sc.threshold)
                return false;
        }
        return true;
    }
}
