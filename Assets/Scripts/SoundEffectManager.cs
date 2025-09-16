using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance;

    [Header("Ambience Crossfade (Loop)")]
    [SerializeField, Min(0f)] private float ambienceFadeSeconds = 1f;
    private AudioSource ambA, ambB, ambActive, ambIdle;
    private float ambTimer, ambDur;  private bool ambFading;

    [Header("Weather/Env Loops")]
    private AudioSource loopBirds, loopRain, loopWindLight, loopWindStrong;

    [Header("OneShot Bus")]
    private AudioSource oneShot;

    void Awake()
    {
        if (Instance == null) Instance = this; else { Destroy(gameObject); return; }
        ambA = CreateLoop("AmbienceA"); ambB = CreateLoop("AmbienceB"); ambActive = ambA; ambIdle = ambB;
        loopBirds = CreateLoop("Loop_Birds"); loopRain = CreateLoop("Loop_Rain");
        loopWindLight = CreateLoop("Loop_WindLight"); loopWindStrong = CreateLoop("Loop_WindStrong");
        oneShot = gameObject.AddComponent<AudioSource>(); oneShot.playOnAwake = false; oneShot.loop = false;
        DontDestroyOnLoad(gameObject);
    }
    AudioSource CreateLoop(string n){ var s=gameObject.AddComponent<AudioSource>(); s.playOnAwake=false; s.loop=true; s.spatialBlend=0f; s.volume=0f; s.name=n; return s; }

    void Update(){
        if(!ambFading) return;
        ambTimer += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(ambTimer/Mathf.Max(ambDur,0.0001f));
        if (ambActive) ambActive.volume = 1f - t;
        if (ambIdle)   ambIdle.volume   = t;
        if (t>=1f){ var tmp=ambActive; ambActive=ambIdle; ambIdle=tmp; ambFading=false; if(ambIdle){ambIdle.Stop(); ambIdle.clip=null; ambIdle.volume=0f;} if(ambActive) ambActive.volume=1f; }
    }

    public void PlayAmbience(AudioClip clip, float fadeSeconds=-1f){
        if(!clip){ StopAmbience(fadeSeconds); return; }
        if(ambActive && ambActive.clip==clip && ambActive.isPlaying) return;
        if(!ambIdle) return;
        ambIdle.clip=clip; ambIdle.volume=0f; ambIdle.Play();
        ambDur = (fadeSeconds>=0f)?fadeSeconds:ambienceFadeSeconds; ambTimer=0f; ambFading=true;
        if(ambActive && !ambActive.isPlaying){ ambActive.volume=1f; ambActive.Play(); }
    }
    public void StopAmbience(float fadeSeconds=-1f){ ambDur=(fadeSeconds>=0f)?fadeSeconds:ambienceFadeSeconds; ambTimer=0f; ambFading=true; if(ambIdle){ambIdle.clip=null; ambIdle.Stop(); ambIdle.volume=0f;} }

    void SetLoop(AudioSource s, AudioClip c){
        if(!s) return;
        if(!c){ if(s.isPlaying) s.Stop(); s.clip=null; s.volume=0f; return; }
        if(s.clip==c && s.isPlaying) return;
        s.clip=c; s.volume=1f; s.Play();
    }
    public void SetBirds(AudioClip c)=>SetLoop(loopBirds,c);
    public void SetRain(AudioClip c)=>SetLoop(loopRain,c);
    public void SetWindLight(AudioClip c)=>SetLoop(loopWindLight,c);
    public void SetWindStrong(AudioClip c)=>SetLoop(loopWindStrong,c);
    public void PlayOneShot(AudioClip c,float v=1f){ if(c&&oneShot) oneShot.PlayOneShot(c,Mathf.Clamp01(v)); }

    // ★ 這裡改為接 CaseVisuals.Entry
    public void ApplyCaseAmbience(CaseVisuals.Entry entry){
        if(entry==null){ StopAmbience(); SetBirds(null); SetRain(null); SetWindLight(null); SetWindStrong(null); return; }
        PlayAmbience(entry.ambienceLoop, 1f);
        SetBirds(entry.birdsLoop);
        SetRain(entry.rainLoop);
        SetWindLight(entry.windLightLoop);
        SetWindStrong(entry.windStrongLoop);
    }
}
