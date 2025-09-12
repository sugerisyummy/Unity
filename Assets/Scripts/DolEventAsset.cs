// DolEventAsset.cs — 支援多數值變化 + 事件內難度倍率
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DOL/Random Event")]
public class DolEventAsset : ScriptableObject
{
    [Header("識別")]
    public string eventId;

    [Header("條件")]
    public int minHP = int.MinValue;
    public int maxHP = int.MaxValue;
    public List<string> requireFlags = new();
    public List<string> forbidFlags = new();

    [Header("觸發控制")]
    [Min(0f)] public float weight = 1f;
    public bool oncePerSave = false;
    public float cooldownSeconds = 0f;

    [Header("事件劇本（多頁）")]
    public List<EventStage> stages = new();

    [Serializable]
    public class EventStage
    {
        [TextArea(2, 6)] public string text;
        public List<EventChoice> choices = new();
    }

    // 供傳回縮放後變化量使用
    public struct StatDelta
    {
        public int hp, hunger, thirst, fatigue;
        public int hope, obedience, reputation;
        public int techParts, information, credits;
        public int augmentationLoad, radiation;
    }

    [Serializable]
    public class EventChoice
    {
        public string text;

        [Header("效果（原始值）")]
        public int hpChange = 0;

        public int hungerChange = 0;
        public int thirstChange = 0;
        public int fatigueChange = 0;

        public int hopeChange = 0;
        public int obedienceChange = 0;
        public int reputationChange = 0;

        public int techPartsChange = 0;
        public int informationChange = 0;
        public int creditsChange = 0;

        public int augmentationLoadChange = 0;
        public int radiationChange = 0;

        [Header("旗標")]
        public List<string> setFlagsTrue = new();
        public List<string> setFlagsFalse = new();

        [Header("跳轉")]
        public int nextStage = -1;             // -1 = 不跳 stage
        public bool endEvent = false;          // 結束事件
        public CaseId gotoCase = CaseId.None;  // 結束後切地點（可選）
        public bool gotoCaseAfterEnd = false;  // true：事件結束時切到 gotoCase

        // ====== 難度倍率（事件內可編輯）======
        [Header("Difficulty Scaling")]
        public bool enableDifficultyScaling = false;

        [Serializable]
        public class DifficultyMultiplier
        {
            [Min(0f)] public float global = 1f;

            [Min(0f)] public float hp = 1f;
            [Min(0f)] public float hunger = 1f;
            [Min(0f)] public float thirst = 1f;
            [Min(0f)] public float fatigue = 1f;

            [Min(0f)] public float hope = 1f;
            [Min(0f)] public float obedience = 1f;
            [Min(0f)] public float reputation = 1f;

            [Min(0f)] public float techParts = 1f;
            [Min(0f)] public float information = 1f;
            [Min(0f)] public float credits = 1f;

            [Min(0f)] public float augmentationLoad = 1f;
            [Min(0f)] public float radiation = 1f;
        }

        // 0=Easy,1=Normal,2=Hard,3=Master（預設可自行調整）
        public DifficultyMultiplier[] difficulty = new DifficultyMultiplier[]
        {
            new DifficultyMultiplier{ global=0.9f, hp=0.9f, hunger=0.9f, thirst=0.9f, fatigue=0.9f,
                                      hope=1.1f, obedience=1.0f, reputation=1.0f,
                                      techParts=1.1f, information=1.1f, credits=1.0f,
                                      augmentationLoad=1.0f, radiation=0.9f }, // Easy
            new DifficultyMultiplier{ }, // Normal 全 1
            new DifficultyMultiplier{ global=1.1f, hp=1.1f, hunger=1.1f, thirst=1.1f, fatigue=1.1f,
                                      hope=0.9f, obedience=1.1f, reputation=1.1f,
                                      techParts=0.9f, information=0.9f, credits=1.0f,
                                      augmentationLoad=1.1f, radiation=1.1f }, // Hard
            new DifficultyMultiplier{ global=1.25f, hp=1.25f, hunger=1.25f, thirst=1.25f, fatigue=1.25f,
                                      hope=0.8f, obedience=1.2f, reputation=1.2f,
                                      techParts=0.8f, information=0.8f, credits=0.9f,
                                      augmentationLoad=1.2f, radiation=1.3f },  // Master
        };

        // 取得縮放後變化量
        public void GetScaledDelta(int diffIndex, out StatDelta dlt)
        {
            if (!enableDifficultyScaling || difficulty == null || difficulty.Length == 0)
            {
                dlt = new StatDelta{
                    hp = hpChange,
                    hunger = hungerChange, thirst = thirstChange, fatigue = fatigueChange,
                    hope = hopeChange, obedience = obedienceChange, reputation = reputationChange,
                    techParts = techPartsChange, information = informationChange, credits = creditsChange,
                    augmentationLoad = augmentationLoadChange, radiation = radiationChange
                };
                return;
            }

            diffIndex = Mathf.Clamp(diffIndex, 0, difficulty.Length - 1);
            var m = difficulty[diffIndex];
            float g = Mathf.Max(0f, m.global);

            dlt = new StatDelta{
                hp = Mathf.RoundToInt(hpChange * g * m.hp),

                hunger = Mathf.RoundToInt(hungerChange * g * m.hunger),
                thirst = Mathf.RoundToInt(thirstChange * g * m.thirst),
                fatigue = Mathf.RoundToInt(fatigueChange * g * m.fatigue),

                hope = Mathf.RoundToInt(hopeChange * g * m.hope),
                obedience = Mathf.RoundToInt(obedienceChange * g * m.obedience),
                reputation = Mathf.RoundToInt(reputationChange * g * m.reputation),

                techParts = Mathf.RoundToInt(techPartsChange * g * m.techParts),
                information = Mathf.RoundToInt(informationChange * g * m.information),
                credits = Mathf.RoundToInt(creditsChange * g * m.credits),

                augmentationLoad = Mathf.RoundToInt(augmentationLoadChange * g * m.augmentationLoad),
                radiation = Mathf.RoundToInt(radiationChange * g * m.radiation)
            };
        }
    }

    public bool ConditionsMet(int hp, Func<string, bool> flagGetter)
    {
        if (hp < minHP || hp > maxHP) return false;
        foreach (var f in requireFlags) if (!string.IsNullOrEmpty(f) && !flagGetter(f)) return false;
        foreach (var f in forbidFlags) if (!string.IsNullOrEmpty(f) && flagGetter(f)) return false;
        return true;
    }
}
