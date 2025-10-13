using UnityEngine;
using UnityEngine.UI;

public class ScreenVignette : MonoBehaviour
{
    [Header("Refs")] public Image overlay;
    [Header("Timings")] public float fadeIn = 0.25f; public float hold = 0.6f; public float fadeOut = 0.45f;
    [Header("Colors")] public Sprite winSprite; public Sprite loseSprite; public Sprite escapeSprite;
    [Range(0,1)] public float maxAlpha = 1f;

    void Awake(){ if (!overlay) overlay = GetComponent<Image>(); if (overlay){ var c=overlay.color; c.a=0f; overlay.color=c; overlay.raycastTarget=false; } }
    public void PlayWin(){ Play(winSprite); } public void PlayLose(){ Play(loseSprite); } public void PlayEscape(){ Play(escapeSprite?escapeSprite:winSprite); }
    public void Play(Sprite sprite){ if(!overlay) return; overlay.sprite = sprite; StopAllCoroutines(); StartCoroutine(DoPlay()); }
    System.Collections.IEnumerator DoPlay(){ var c=overlay.color;
        for(float t=0;t<fadeIn;t+=Time.unscaledDeltaTime){ c.a=Mathf.Lerp(0f,maxAlpha,t/fadeIn); overlay.color=c; yield return null; }
        c.a=maxAlpha; overlay.color=c; yield return new WaitForSecondsRealtime(hold);
        for(float t=0;t<fadeOut;t+=Time.unscaledDeltaTime){ c.a=Mathf.Lerp(maxAlpha,0f,t/fadeOut); overlay.color=c; yield return null; }
        c.a=0f; overlay.color=c; }
}
