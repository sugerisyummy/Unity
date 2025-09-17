// StatEventBus.cs
using System;
using UnityEngine;

public static class StatEventBus
{
    // 當單一數值改變（例如 HP -5）
    public static event Action<StatType, float, float> OnStatChanged; // (type, current, max)

    // 當你有一批數值要一起推（例如載入存檔時）
    public static event Action OnBulkRefresh;

    public static void Raise(StatType type, float current, float max)
    {
        OnStatChanged?.Invoke(type, current, max);
    }

    public static void RaiseBulkRefresh()
    {
        OnBulkRefresh?.Invoke();
    }
}
