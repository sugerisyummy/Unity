// EventFlagStorage.cs
// 說明：管理旗標、一回性紀錄、冷卻時間戳。
// ★ 你可以換成存檔系統對接（例如 SaveData 內序列化）。

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EventFlagStorage
{
    private HashSet<string> flags = new();
    private HashSet<string> consumed = new();
    private Dictionary<string, float> lastTriggerTime = new();

    public bool HasFlag(string key) => !string.IsNullOrEmpty(key) && flags.Contains(key);

    public void SetFlag(string key)
    {
        if (!string.IsNullOrEmpty(key)) flags.Add(key);
    }

    public void ClearFlag(string key)
    {
        if (!string.IsNullOrEmpty(key)) flags.Remove(key);
    }

    public void MarkConsumed(DolEventAsset ev)
    {
        if (ev == null) return;
        consumed.Add(ev.name);
        lastTriggerTime[ev.name] = Time.unscaledTime;
    }

    public bool IsConsumed(DolEventAsset ev)
    {
        if (ev == null) return false;
        return consumed.Contains(ev.name);
    }

    public bool IsOnCooldown(DolEventAsset ev, float cooldownSeconds)
    {
        if (ev == null || cooldownSeconds <= 0) return false;
        if (!lastTriggerTime.TryGetValue(ev.name, out var t)) return false;
        return (Time.unscaledTime - t) < cooldownSeconds;
    }
}
