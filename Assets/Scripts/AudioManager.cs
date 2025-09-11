using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources (A/B Crossfade)")]
    [SerializeField] private AudioSource sourceA;
    [SerializeField] private AudioSource sourceB;
    [SerializeField, Min(0f)] private float defaultFadeSeconds = 1f;

    private AudioSource active, idle;
    private float fadeTimer, fadeDur;
    private bool isFading;

    void Awake()
    {
        if (Instance == null) Instance = this; else { Destroy(gameObject); return; }
        if (!sourceA) sourceA = gameObject.AddComponent<AudioSource>();
        if (!sourceB) sourceB = gameObject.AddComponent<AudioSource>();
        sourceA.loop = true; sourceB.loop = true;
        active = sourceA; idle = sourceB;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!isFading) return;
        fadeTimer += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(fadeTimer / Mathf.Max(0.0001f, fadeDur));
        if (active) active.volume = 1f - t;
        if (idle) idle.volume = t;
        if (t >= 1f)
        {
            // swap
            var tmp = active; active = idle; idle = tmp;
            isFading = false;
            if (idle) { idle.Stop(); idle.clip = null; idle.volume = 0f; }
            if (active) active.volume = 1f;
        }
    }

    public void PlayBGM(AudioClip clip, float fadeSeconds = -1f)
    {
        if (!clip) { StopBGM(); return; }
        if (active && active.clip == clip && active.isPlaying) return;

        if (!idle) return;
        idle.clip = clip;
        idle.volume = 0f;
        idle.Play();

        fadeDur = (fadeSeconds >= 0f) ? fadeSeconds : defaultFadeSeconds;
        fadeTimer = 0f;
        isFading = true;

        // 確保當前聲道有音量
        if (active && !active.isPlaying) { active.volume = 1f; active.Play(); }
    }

    public void StopBGM(float fadeSeconds = -1f)
    {
        fadeDur = (fadeSeconds >= 0f) ? fadeSeconds : defaultFadeSeconds;
        fadeTimer = 0f;
        isFading = true;
        if (idle) { idle.clip = null; idle.Stop(); idle.volume = 0f; }
        // idle 使用空白，完成後 active 會降到 0
    }
}
