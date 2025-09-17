// HUDStats.cs
using System;
using System.Collections.Generic;
using UnityEngine;

// 把多條 StatBar 管起來，也提供 RefreshAll 拉取當前值
public class HUDStats : MonoBehaviour
{
    [Serializable]
    public class Binding
    {
        public StatType type;
        public StatBar bar;
    }

    public List<Binding> bindings = new();

    IStatProvider _provider; // 由你的 GameManager 或存檔系統提供

    void Awake()
    {
        // 嘗試自動找 Provider（你也可以在 Inspector 手動指定）
        _provider = FindObjectOfType<GameManagerStatProvider>();
        // 若你的 GameManager 直接實作 IStatProvider，也能抓到
        if (_provider == null) _provider = FindObjectOfType<MonoBehaviour>() as IStatProvider;
    }

    void OnEnable()
    {
        StatEventBus.OnBulkRefresh += RefreshAll;
    }

    void OnDisable()
    {
        StatEventBus.OnBulkRefresh -= RefreshAll;
    }

    public void RefreshAll()
    {
        if (_provider == null) return;
        foreach (var b in bindings)
        {
            if (b.bar == null) continue;
            if (_provider.TryGet(b.type, out float current, out float max))
            {
                b.bar.SetImmediate(current, max);
            }
        }
    }

    // 若你在載入存檔或切換地點時想一次重繪
    [ContextMenu("Force Refresh All")]
    void EditorRefresh() => RefreshAll();
}

public interface IStatProvider
{
    bool TryGet(StatType type, out float current, out float max);
}
