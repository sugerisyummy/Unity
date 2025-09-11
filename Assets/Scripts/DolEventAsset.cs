// DolEventAsset.cs — 修正 List<EventStage>；gotoCase 預設 None；其餘沿用。
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DOL/Random Event")]
public class DolEventAsset : ScriptableObject
{
    [Header("識別")]
    public string eventId; // 全域唯一（供旗標/冷卻/一次性記錄）

    [Header("條件")]
    public int minHP = int.MinValue;
    public int maxHP = int.MaxValue;
    public List<string> requireFlags = new(); // 需要為 true 的旗標
    public List<string> forbidFlags = new();  // 需要為 false 的旗標

    [Header("觸發控制")]
    [Min(0f)] public float weight = 1f; // 加權
    public bool oncePerSave = false;    // 此存檔只觸發一次
    public float cooldownSeconds = 0f;  // 觸發後冷卻

    [Header("事件劇本（多頁）")]
    public List<EventStage> stages = new(); // ★ 修正語法：List<EventStage>

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

        [Header("效果")]
        public int hpChange = 0;
        public List<string> setFlagsTrue = new();
        public List<string> setFlagsFalse = new();

        [Header("跳轉")]
        public int nextStage = -1;             // -1 = 不跳 stage
        public bool endEvent = false;          // 結束事件
        public CaseId gotoCase = CaseId.None;  // ★ 預設改 None（避免刪 enum 後編譯炸裂）
        public bool gotoCaseAfterEnd = false;  // true：事件結束時切到 gotoCase
    }

    public bool ConditionsMet(int hp, Func<string, bool> flagGetter)
    {
        if (hp < minHP || hp > maxHP) return false;
        foreach (var f in requireFlags) if (!string.IsNullOrEmpty(f) && !flagGetter(f)) return false;
        foreach (var f in forbidFlags) if (!string.IsNullOrEmpty(f) && flagGetter(f)) return false;
        return true;
    }
}
