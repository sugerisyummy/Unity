// EventFlagStorage.cs
// 說明：管理旗標、一回性紀錄、冷卻時間戳，並與 SaveData 完整序列化/反序列化。
// 方案：存「最後觸發 Unix 秒」→ IsOnCooldown(now - last < cooldownSeconds)

using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EventFlagStorage
{
    private HashSet<string> flags = new();                     // 自訂旗標
    private HashSet<string> consumed = new();                  // 一次性事件已用過
    private Dictionary<string, long> lastTriggerUnix = new();  // 事件最後觸發時間（Unix 秒）

    // ==== 旗標 ====
    public bool HasFlag(string key) => !string.IsNullOrEmpty(key) && flags.Contains(key);
    public void SetFlag(string key){ if (!string.IsNullOrEmpty(key)) flags.Add(key); }
    public void ClearFlag(string key){ if (!string.IsNullOrEmpty(key)) flags.Remove(key); }

    // ==== 事件記錄 ====
    public void MarkConsumed(DolEventAsset ev)
    {
        if (ev == null) return;
        consumed.Add(ev.name);
        lastTriggerUnix[ev.name] = NowUnix();
    }

    public bool IsConsumed(DolEventAsset ev)
    {
        if (ev == null) return false;
        return consumed.Contains(ev.name);
    }

    public bool IsOnCooldown(DolEventAsset ev, float cooldownSeconds)
    {
        if (ev == null || cooldownSeconds <= 0f) return false;
        if (!lastTriggerUnix.TryGetValue(ev.name, out var last)) return false;
        long now = NowUnix();
        return (now - last) < (long)Mathf.Ceil(cooldownSeconds);
    }

    // ==== 存讀 ====
    public void WriteToSave(SaveData d)
    {
        if (d == null) return;

        d.flagsTrue = flags != null ? new List<string>(flags).ToArray() : Array.Empty<string>();
        d.onceConsumedKeys = consumed != null ? new List<string>(consumed).ToArray() : Array.Empty<string>();

        if (lastTriggerUnix != null && lastTriggerUnix.Count > 0)
        {
            var keys = new List<string>(lastTriggerUnix.Keys);
            var vals = new List<long>(keys.Count);
            foreach (var k in keys) vals.Add(lastTriggerUnix[k]);
            d.cooldownKeys = keys.ToArray();
            d.cooldownUntilUnix = vals.ToArray(); // 存最後觸發時間
        }
        else
        {
            d.cooldownKeys = Array.Empty<string>();
            d.cooldownUntilUnix = Array.Empty<long>();
        }
    }

    public void LoadFromSave(SaveData d)
    {
        flags = new HashSet<string>();
        consumed = new HashSet<string>();
        lastTriggerUnix = new Dictionary<string, long>();

        if (d == null) return;

        if (d.flagsTrue != null)
            foreach (var k in d.flagsTrue) if (!string.IsNullOrEmpty(k)) flags.Add(k);

        if (d.onceConsumedKeys != null)
            foreach (var k in d.onceConsumedKeys) if (!string.IsNullOrEmpty(k)) consumed.Add(k);

        if (d.cooldownKeys != null && d.cooldownUntilUnix != null)
        {
            int n = Mathf.Min(d.cooldownKeys.Length, d.cooldownUntilUnix.Length);
            for (int i = 0; i < n; i++)
            {
                var key = d.cooldownKeys[i];
                var t = d.cooldownUntilUnix[i];
                if (!string.IsNullOrEmpty(key)) lastTriggerUnix[key] = t;
            }
        }
    }

    private static long NowUnix() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}
