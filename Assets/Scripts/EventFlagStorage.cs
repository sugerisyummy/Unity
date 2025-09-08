// EventFlagStorage.cs
using System;
using UnityEngine;

public static class EventFlagStorage
{
    // 你也可以日後改成存 SaveData；目前先用 PlayerPrefs + slot 前綴
    static string Slot() => PlayerPrefs.GetString("CURRENT_SAVE_SLOT", "Default");

    static string K(string kind, string name) => $"DOL_{Slot()}_{kind}_{name}";

    public static bool GetFlag(string name) => PlayerPrefs.GetInt(K("FLAG", name), 0) == 1;
    public static void SetFlag(string name, bool v)
    {
        PlayerPrefs.SetInt(K("FLAG", name), v ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void MarkFired(string eventId)
    {
        PlayerPrefs.SetInt(K("ONCE", eventId), 1);
        PlayerPrefs.SetString(K("CD", eventId), DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        PlayerPrefs.Save();
    }

    public static bool IsOnceConsumed(string eventId) => PlayerPrefs.GetInt(K("ONCE", eventId), 0) == 1;

    public static bool IsOffCooldown(string eventId, float cd)
    {
        if (cd <= 0f) return true;
        long last = long.Parse(PlayerPrefs.GetString(K("CD", eventId), "0"));
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return now - last >= cd;
    }
}
