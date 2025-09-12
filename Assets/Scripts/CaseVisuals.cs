using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "DOL/Case Visuals")]
public class CaseVisuals : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public CaseId id;
        public Sprite background;
        public AudioClip bgm;

        [Header("Ambience / Weather (SFX loops)")]
        public AudioClip ambienceLoop;     // 一般環境底噪（室內/街道/森林）
        public AudioClip birdsLoop;        // 晴天鳥叫
        public AudioClip rainLoop;         // 雨天
        public AudioClip windLightLoop;    // 小風
        public AudioClip windStrongLoop;   // 大風
    }

    public List<Entry> items = new();

    public bool TryGet(CaseId id, out Entry e)
    {
        foreach (var it in items) { if (it != null && it.id.Equals(id)) { e = it; return true; } }
        e = null; return false;
    }
}
