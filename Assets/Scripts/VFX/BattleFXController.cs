using UnityEngine;

public class BattleFXController : MonoBehaviour
{
    [Header("Refs")] public ScreenVignette vignette;
    public AudioClip sfxWin; public AudioClip sfxLose; public AudioClip sfxEscape;
    public AudioClip bgmAfterWin; public AudioClip bgmAfterLose; public AudioClip bgmAfterEscape;
    [Range(0,2f)] public float sfxDelay = 0.0f; [Range(0,2f)] public float bgmDelay = 0.5f;

    AudioSource _sfx; // 當你的 AudioManager 沒有 PlaySFX 時，這個會當後備

    void Awake()
    {
        _sfx = GetComponent<AudioSource>();
        if(!_sfx){ _sfx = gameObject.AddComponent<AudioSource>(); _sfx.playOnAwake=false; _sfx.loop=false; }
    }

    public void OnWin()
    {
        if (vignette) vignette.PlayWin();
        if (sfxWin)   Invoke(nameof(_playWinS), sfxDelay);
        if (bgmAfterWin) Invoke(nameof(_playWinB), bgmDelay);
    }
    public void OnLose()
    {
        if (vignette) vignette.PlayLose();
        if (sfxLose)   Invoke(nameof(_playLoseS), sfxDelay);
        if (bgmAfterLose) Invoke(nameof(_playLoseB), bgmDelay);
    }
    public void OnEscape()
    {
        if (vignette) vignette.PlayEscape();
        if (sfxEscape)   Invoke(nameof(_playEscS), sfxDelay);
        if (bgmAfterEscape) Invoke(nameof(_playEscB), bgmDelay);
    }

    void _playWinS(){ PlaySfx(sfxWin); }  void _playLoseS(){ PlaySfx(sfxLose); }  void _playEscS(){ PlaySfx(sfxEscape); }
    void _playWinB(){ PlayBgm(bgmAfterWin); } void _playLoseB(){ PlayBgm(bgmAfterLose); } void _playEscB(){ PlayBgm(bgmAfterEscape); }

    void PlaySfx(AudioClip clip)
    {
        if (!clip) return;
        // 如果你的 AudioManager 有 PlaySFX，就優先使用；沒有就用內建 AudioSource
        var am = FindObjectOfType(typeof(object), false);
        try {
            var amType = typeof(AudioManager);
            var instProp = amType.GetProperty("Instance", System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static);
            var inst = instProp != null ? instProp.GetValue(null) : null;
            var method = amType.GetMethod("PlaySFX");
            if (inst != null && method != null) { method.Invoke(inst, new object[]{ clip, 0.03f }); return; }
        } catch {}
        _sfx.pitch = 1f + Random.Range(-0.03f, 0.03f);
        _sfx.PlayOneShot(clip, 1f);
    }

    void PlayBgm(AudioClip clip)
    {
        if (!clip) return;
        // 若有 AudioManager.PlayBGM 用它；沒有則做一次性播放
        try {
            var amType = typeof(AudioManager);
            var instProp = amType.GetProperty("Instance", System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static);
            var inst = instProp != null ? instProp.GetValue(null) : null;
            var method = amType.GetMethod("PlayBGM");
            if (inst != null && method != null) { method.Invoke(inst, new object[]{ clip, 0.6f }); return; }
        } catch {}
        // fallback：一次性 AudioSource
        var go = new GameObject("BGM_OneShot"); var a = go.AddComponent<AudioSource>();
        a.clip = clip; a.loop = false; a.playOnAwake = false; a.volume = 1f; a.Play();
        GameObject.Destroy(go, clip.length + 0.2f);
    }
}
