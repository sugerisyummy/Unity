using UnityEngine;
using UnityEngine.UI;
public class HitFlash2D : MonoBehaviour
{
    [Range(0,1f)] public float intensity = 0.9f; public float duration = 0.08f; public bool includeChildren = true;
    Color[] _orig; Graphic[] _targets; float _time; bool _flashing;
    public void Flash(){ _targets = includeChildren ? GetComponentsInChildren<Graphic>(true) : GetComponents<Graphic>(); if(_targets==null||_targets.Length==0) return; _orig=new Color[_targets.Length]; for (int i=0;i<_targets.Length;i++) _orig[i]=_targets[i].color; _time=duration; _flashing=true; }
    void LateUpdate(){ if(!_flashing) return; _time -= Time.unscaledDeltaTime; float u = Mathf.Clamp01(_time/duration); for(int i=0;i<_targets.Length;i++){ var c=_orig[i]; float t=Mathf.SmoothStep(0f,1f,1f-u); _targets[i].color = Color.Lerp(new Color(1,1,1,1), c, t);} if(_time<=0){ for(int i=0;i<_targets.Length;i++) _targets[i].color=_orig[i]; _flashing=false; } }
}
