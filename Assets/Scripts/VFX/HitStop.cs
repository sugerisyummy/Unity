// HitStop.cs — 全域抽幀（時間暫停/慢動作）
// 放在任何場景常駐物件上即可（例如 AudioManager 同級）。
using UnityEngine;
using System.Collections;

public class HitStop : MonoBehaviour
{
    public static HitStop Instance { get; private set; }

    [Header("Defaults")]
    [Range(0.0f, 1.0f)] public float slowScale = 0.05f;
    [Tooltip("未指定時使用的抽幀秒數（以實際時間計，不受 timeScale 影響）。")]
    public float defaultDuration = 0.08f;

    float _remain; bool _running;
    float _prevScale; float _prevFixed;
    float _useScale;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject);
    }

    /// <summary>發動抽幀（以 Unscaled 時間計算）。</summary>
    public void Stop(float duration = -1f, float scaleOverride = -1f)
    {
        if (duration <= 0f) duration = defaultDuration;
        if (scaleOverride > 0f) _useScale = Mathf.Clamp(scaleOverride, 0.0f, 1.0f);
        else _useScale = slowScale;

        // 若已在抽幀中，就延長持續時間 & 取更慢的比率
        _remain = Mathf.Max(_remain, duration);
        if (_running) { if (Time.timeScale > _useScale) ApplyScale(_useScale); return; }

        _prevScale = Time.timeScale;
        _prevFixed = Time.fixedDeltaTime;
        ApplyScale(_useScale);
        _running = true;
        StartCoroutine(CoRun());
    }

    IEnumerator CoRun()
    {
        while (_remain > 0f) { _remain -= Time.unscaledDeltaTime; yield return null; }
        Restore();
    }

    void ApplyScale(float s)
    {
        Time.timeScale = s;
        Time.fixedDeltaTime = _prevFixed * s;
    }

    void Restore()
    {
        Time.timeScale = _prevScale;
        Time.fixedDeltaTime = _prevFixed;
        _remain = 0f; _running = false;
    }
}
