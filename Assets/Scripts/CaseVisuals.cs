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
    }

    public List<Entry> items = new();

    public bool TryGet(CaseId id, out Entry e)
    {
        foreach (var it in items) { if (it != null && it.id.Equals(id)) { e = it; return true; } }
        e = null; return false;
    }
}
