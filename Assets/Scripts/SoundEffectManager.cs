// SoundEffectManager.cs — 環境/天氣/事件音 效果音系統（與 BGM 分離）
using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance;

    [Header("Ambience Crossfade (Loop)")]
    [SerializeField, Min(0f)] private float ambienceFadeSeconds = 1f;
    private AudioSource ambA, ambB;   // 交叉淡入
    private AudioSource ambActive, ambIdle;
    private float ambTimer, ambDur;
    private bool ambFading;

    [Header("Weather/Env Loops (each its own channel)")]
    private AudioSource loopBirds;
    private AudioSource loopRain;
    private AudioSource loopWindLight;
    private AudioSource loopWindStrong;

    [Header("OneShot Bus")]
    private AudioSource oneShot;       // 播放短音效

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // 建立需要的聲道
        ambA = CreateLoopSource("AmbienceA");
        ambB = CreateLoopSource("AmbienceB");
        ambActive = ambA; ambIdle = ambB;

        loopBirds     = CreateLoopSource("Loop_Birds");
        loopRain      = CreateLoopSource("Loop_Rain");
        loopWindLight = CreateLoopSource("Loop_WindLight");
        loopWindStrong= CreateLoopSource("Loop_WindStrong");

        oneShot = gameObject.AddComponent<AudioSource>();
        oneShot.playOnAwake = false;
        oneShot.loop = false;

        DontDestroyOnLoad(gameObject);
    }

    private AudioSource CreateLoopSource(string name)
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.loop = true;
        s.spatialBlend = 0f; // 2D
        s.volume = 0f;
        s.outputAudioMixerGroup = null; // 若有 Mixer 可外掛
        s.name = name;
        return s;
    }

    void Update()
    {
        if (!ambFading) return;
        ambTimer += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(ambTimer / Mathf.Max(ambDur, 0.0001f));
        if (ambActive) ambActive.volume = 1f - t;
        if (ambIdle)   ambIdle.volume = t;
        if (t >= 1f)
        {
            var tmp = ambActive; ambActive = ambIdle; ambIdle = tmp;
            ambFading = false;
            if (ambIdle != null) { ambIdle.Stop(); ambIdle.clip = null; ambIdle.volume = 0f; }
            if (ambActive != null) ambActive.volume = 1f;
        }
    }

    // === Ambience（單一路徑循環，交叉淡入） ===
    public void PlayAmbience(AudioClip clip, float fadeSeconds = -1f)
    {
        if (!clip)
        {
            StopAmbience(fadeSeconds);
            return;
        }
        if (ambActive && ambActive.clip == clip && ambActive.isPlaying) return;

        if (ambIdle == null) return;
        ambIdle.clip = clip;
        ambIdle.volume = 0f;
        ambIdle.Play();

        ambDur = (fadeSeconds >= 0f) ? fadeSeconds : ambienceFadeSeconds;
        ambTimer = 0f;
        ambFading = true;

        if (ambActive && !ambActive.isPlaying) { ambActive.volume = 1f; ambActive.Play(); }
    }

    public void StopAmbience(float fadeSeconds = -1f)
    {
        ambDur = (fadeSeconds >= 0f) ? fadeSeconds : ambienceFadeSeconds;
        ambTimer = 0f;
        ambFading = true;
        if (ambIdle) { ambIdle.clip = null; ambIdle.Stop(); ambIdle.volume = 0f; }
        // 淡出完成後 ambActive 會降到 0 並交換
    }

    // === Weather/Env Loops（可同時多條） ===
    public void SetLoop(AudioSource src, AudioClip clip)
    {
        if (!src) return;
        if (!clip)
        {
            if (src.isPlaying) src.Stop();
            src.clip = null;
            src.volume = 0f;
            return;
        }
        if (src.clip == clip && src.isPlaying) return;
        src.clip = clip;
        src.volume = 1f;
        src.Play();
    }

    public void SetBirds(AudioClip clip)      => SetLoop(loopBirds, clip);
    public void SetRain(AudioClip clip)       => SetLoop(loopRain, clip);
    public void SetWindLight(AudioClip clip)  => SetLoop(loopWindLight, clip);
    public void SetWindStrong(AudioClip clip) => SetLoop(loopWindStrong, clip);

    // 一次性短音效（不影響任何循環）
    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (!clip || !oneShot) return;
        oneShot.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    // 給 GameManager 快速套用 Case 視覺/聲音
    public void ApplyCaseAmbience(CaseVisuals.Entry entry)
    {
        if (entry == null) { StopAmbience(); SetBirds(null); SetRain(null); SetWindLight(null); SetWindStrong(null); return; }
        PlayAmbience(entry.ambienceLoop, 1f);
        SetBirds(entry.birdsLoop);
        SetRain(entry.rainLoop);
        SetWindLight(entry.windLightLoop);
        SetWindStrong(entry.windStrongLoop);
    }
}
