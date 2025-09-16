using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "DOL/Case Visuals")]
public class CaseVisuals : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public CaseId caseId = CaseId.None;

        [Header("Background")]
        public Sprite background;

        [Header("BGM (Location Default)")]
        public AudioClip bgm;

        [Header("Ambience / Weather Loops")]
        public AudioClip ambienceLoop;   // 城市底噪/室內嗡鳴
        public AudioClip birdsLoop;      // 晴天鳥叫
        public AudioClip rainLoop;       // 雨聲
        public AudioClip windLightLoop;  // 微風
        public AudioClip windStrongLoop; // 強風
    }

    public List<Entry> entries = new();

    /// <summary>依 CaseId 取得視覺/音效設定。</summary>
    public bool TryGet(CaseId id, out Entry e)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            var it = entries[i];
            if (it != null && it.caseId == id)
            {
                e = it; return true;
            }
        }
        e = null; return false;
    }
}
