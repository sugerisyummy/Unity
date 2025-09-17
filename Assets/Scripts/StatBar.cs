// StatBar.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatBar : MonoBehaviour
{
    [Header("Wiring")]
    public TextMeshProUGUI label;
    public Image fill;               // type = Filled, Method=Horizontal
    public TextMeshProUGUI valueText;

    [Header("Config")]
    public StatType statType;
    public bool smoothLerp = true;
    public float lerpSpeed = 8f;
    public Gradient colorByPct;      // 可在 Inspector 設一個從紅→黃→綠的 Gradient

    float _targetPct = 1f;
    float _currentPct = 1f;
    float _current = 0f, _max = 1f;

    void OnEnable()
    {
        StatEventBus.OnStatChanged += OnStatChanged;
        StatEventBus.OnBulkRefresh += ForceRefreshValueText;
    }

    void OnDisable()
    {
        StatEventBus.OnStatChanged -= OnStatChanged;
        StatEventBus.OnBulkRefresh -= ForceRefreshValueText;
    }

    void Update()
    {
        if (smoothLerp)
        {
            _currentPct = Mathf.Lerp(_currentPct, _targetPct, Time.deltaTime * lerpSpeed);
            ApplyFill(_currentPct);
        }
    }

    void OnStatChanged(StatType type, float current, float max)
    {
        if (type != statType) return;
        _current = Mathf.Max(0, current);
        _max = Mathf.Max(1, max);
        _targetPct = Mathf.Clamp01(_current / _max);
        if (!smoothLerp)
        {
            _currentPct = _targetPct;
            ApplyFill(_currentPct);
        }
        if (valueText) valueText.text = $"{Mathf.RoundToInt(_current)}/{Mathf.RoundToInt(_max)}";
    }

    void ForceRefreshValueText()
    {
        if (valueText) valueText.text = $"{Mathf.RoundToInt(_current)}/{Mathf.RoundToInt(_max)}";
        ApplyFill(_currentPct);
    }

    void ApplyFill(float pct)
    {
        if (fill)
        {
            fill.fillAmount = pct;
            if (colorByPct.colorKeys.Length > 0 || colorByPct.alphaKeys.Length > 0)
                fill.color = colorByPct.Evaluate(pct);
        }
    }

    // 允許外部強制設初值（例如第一次進場）
    public void SetImmediate(float current, float max)
    {
        _current = Mathf.Max(0, current);
        _max = Mathf.Max(1, max);
        _targetPct = _currentPct = Mathf.Clamp01(_current / _max);
        ApplyFill(_currentPct);
        if (valueText) valueText.text = $"{Mathf.RoundToInt(_current)}/{Mathf.RoundToInt(_max)}";
    }
}
