// DolEventAsset.cs — 增加各數值變更欄位
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

    [Serializable]
    public class EventChoice
    {
        public string text;

        [Header("效果：角色數值變更（可正可負）")]
        public int hpChange = 0;
        public int moneyChange = 0;
        public int sanityChange = 0;
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
        public int trustChange = 0;
        public int controlChange = 0;

        [Header("旗標")]
        public List<string> setFlagsTrue = new();
        public List<string> setFlagsFalse = new();

        [Header("跳轉")]
        public int nextStage = -1;
        public bool endEvent = false;
        public CaseId gotoCase = CaseId.None;
        public bool gotoCaseAfterEnd = false;
    }

    public bool ConditionsMet(int hp, Func<string, bool> flagGetter)
    {
        if (hp < minHP || hp > maxHP) return false;
        foreach (var f in requireFlags) if (!string.IsNullOrEmpty(f) && !flagGetter(f)) return false;
        foreach (var f in forbidFlags)  if (!string.IsNullOrEmpty(f) && flagGetter(f)) return false;
        return true;
    }
}
