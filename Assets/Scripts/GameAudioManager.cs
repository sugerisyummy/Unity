using UnityEngine;

/// <summary>
/// Centralised audio controller that merges BGM crossfades, ambience loops and one-shot SFX.
/// Keeps the scene wiring simple by exposing a single singleton entry point.
/// </summary>
public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    [Header("BGM Crossfade (A/B)")]
    [SerializeField] private AudioSource bgmSourceA;
    [SerializeField] private AudioSource bgmSourceB;
    [SerializeField, Min(0f)] private float defaultBgmFadeSeconds = 1f;

    [Header("Ambience Crossfade")]
    [SerializeField, Min(0f)] private float ambienceFadeSeconds = 1f;

    private AudioSource _bgmActive, _bgmIdle;
    private AudioSource _ambActive, _ambIdle;
    private AudioSource _loopBirds, _loopRain, _loopWindLight, _loopWindStrong;
    private AudioSource _oneShot;

    private float _bgmTimer, _bgmDuration;
    private bool _bgmFading;

    private float _ambTimer, _ambDuration;
    private bool _ambFading;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        EnsureBgmSources();
        EnsureAmbienceSources();
        _oneShot = CreateOneShot("SFX_OneShot");

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (_bgmFading)
        {
            _bgmTimer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_bgmTimer / Mathf.Max(_bgmDuration, 0.0001f));
            if (_bgmActive) _bgmActive.volume = 1f - t;
            if (_bgmIdle) _bgmIdle.volume = t;
            if (t >= 1f)
            {
                Swap(ref _bgmActive, ref _bgmIdle);
                _bgmFading = false;
                ResetSource(_bgmIdle);
                if (_bgmActive) _bgmActive.volume = 1f;
            }
        }

        if (_ambFading)
        {
            _ambTimer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_ambTimer / Mathf.Max(_ambDuration, 0.0001f));
            if (_ambActive) _ambActive.volume = 1f - t;
            if (_ambIdle) _ambIdle.volume = t;
            if (t >= 1f)
            {
                Swap(ref _ambActive, ref _ambIdle);
                _ambFading = false;
                ResetSource(_ambIdle);
                if (_ambActive) _ambActive.volume = 1f;
            }
        }
    }

    public void PlayBGM(AudioClip clip, float fadeSeconds = -1f)
    {
        if (!clip)
        {
            StopBGM(fadeSeconds);
            return;
        }

        if (_bgmActive && _bgmActive.clip == clip && _bgmActive.isPlaying) return;
        if (_bgmIdle == null) return;

        _bgmIdle.clip = clip;
        _bgmIdle.volume = 0f;
        _bgmIdle.loop = true;
        _bgmIdle.Play();

        _bgmDuration = (fadeSeconds >= 0f) ? fadeSeconds : defaultBgmFadeSeconds;
        _bgmTimer = 0f;
        _bgmFading = true;

        if (_bgmActive && !_bgmActive.isPlaying)
        {
            _bgmActive.volume = 1f;
            _bgmActive.Play();
        }
    }

    public void StopBGM(float fadeSeconds = -1f)
    {
        if (_bgmActive == null) return;
        _bgmDuration = (fadeSeconds >= 0f) ? fadeSeconds : defaultBgmFadeSeconds;
        _bgmTimer = 0f;
        _bgmFading = true;
        ResetSource(_bgmIdle);
    }

    public void PlayAmbience(AudioClip clip, float fadeSeconds = -1f)
    {
        if (!clip)
        {
            StopAmbience(fadeSeconds);
            return;
        }

        if (_ambActive && _ambActive.clip == clip && _ambActive.isPlaying) return;
        if (_ambIdle == null) return;

        _ambIdle.clip = clip;
        _ambIdle.volume = 0f;
        _ambIdle.loop = true;
        _ambIdle.Play();

        _ambDuration = (fadeSeconds >= 0f) ? fadeSeconds : ambienceFadeSeconds;
        _ambTimer = 0f;
        _ambFading = true;

        if (_ambActive && !_ambActive.isPlaying)
        {
            _ambActive.volume = 1f;
            _ambActive.Play();
        }
    }

    public void StopAmbience(float fadeSeconds = -1f)
    {
        if (_ambActive == null) return;
        _ambDuration = (fadeSeconds >= 0f) ? fadeSeconds : ambienceFadeSeconds;
        _ambTimer = 0f;
        _ambFading = true;
        ResetSource(_ambIdle);
    }

    public void SetBirds(AudioClip clip) => SetLoop(_loopBirds, clip);
    public void SetRain(AudioClip clip) => SetLoop(_loopRain, clip);
    public void SetWindLight(AudioClip clip) => SetLoop(_loopWindLight, clip);
    public void SetWindStrong(AudioClip clip) => SetLoop(_loopWindStrong, clip);

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (!clip || _oneShot == null) return;
        _oneShot.pitch = 1f + Random.Range(-0.03f, 0.03f);
        _oneShot.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public void ApplyCaseAudio(CaseVisuals.Entry entry)
    {
        if (entry == null)
        {
            StopBGM();
            StopAmbience();
            SetBirds(null);
            SetRain(null);
            SetWindLight(null);
            SetWindStrong(null);
            return;
        }

        PlayBGM(entry.bgm);
        PlayAmbience(entry.ambienceLoop);
        SetBirds(entry.birdsLoop);
        SetRain(entry.rainLoop);
        SetWindLight(entry.windLightLoop);
        SetWindStrong(entry.windStrongLoop);
    }

    void EnsureBgmSources()
    {
        bgmSourceA = PrepareLoopSource(bgmSourceA, "BGM_A");
        bgmSourceB = PrepareLoopSource(bgmSourceB, "BGM_B");
        _bgmActive = bgmSourceA;
        _bgmIdle = bgmSourceB;
        _bgmActive.volume = 1f;
        _bgmIdle.volume = 0f;
    }

    void EnsureAmbienceSources()
    {
        _ambActive = PrepareLoopSource(_ambActive, "Ambience_A");
        _ambIdle = PrepareLoopSource(_ambIdle, "Ambience_B");
        _loopBirds = PrepareLoopSource(_loopBirds, "Loop_Birds");
        _loopRain = PrepareLoopSource(_loopRain, "Loop_Rain");
        _loopWindLight = PrepareLoopSource(_loopWindLight, "Loop_WindLight");
        _loopWindStrong = PrepareLoopSource(_loopWindStrong, "Loop_WindStrong");
        _ambActive.volume = 1f;
        _ambIdle.volume = 0f;
    }

    AudioSource PrepareLoopSource(AudioSource source, string name)
    {
        if (!source) source = gameObject.AddComponent<AudioSource>();
        source.name = name;
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = 0f;
        return source;
    }

    AudioSource CreateOneShot(string name)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.name = name;
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = 1f;
        return source;
    }

    void SetLoop(AudioSource source, AudioClip clip)
    {
        if (!source) return;
        if (!clip)
        {
            if (source.isPlaying) source.Stop();
            source.clip = null;
            source.volume = 0f;
            return;
        }

        if (source.clip == clip && source.isPlaying) return;
        source.clip = clip;
        source.volume = 1f;
        source.loop = true;
        source.Play();
    }

    void ResetSource(AudioSource source)
    {
        if (!source) return;
        source.Stop();
        source.clip = null;
        source.volume = 0f;
    }

    void Swap(ref AudioSource a, ref AudioSource b)
    {
        var temp = a;
        a = b;
        b = temp;
    }
}
