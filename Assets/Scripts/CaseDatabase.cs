// CaseDatabase.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DOL/Case Database")]
public class CaseDatabase : ScriptableObject
{
    [System.Serializable]
    public class CaseEntry
    {
        public CaseId caseId;
        public List<EventEntry> events = new();
    }

[System.Serializable] public class EventEntry {
    public DolEventAsset evt;
    // 改掉 Min(0f)；讓 -1 成為「沿用事件 weight」
    [Tooltip("-1=沿用事件的 Weight；>0=覆蓋")] 
    public float weightOverride = -1f;
}

    public List<CaseEntry> cases = new();

    public bool TryGetPool(CaseId id, out List<EventEntry> pool)
    {
        foreach (var c in cases)
        {
            if (c.caseId == id) { pool = c.events; return pool != null; }
        }
        pool = null; return false;
    }
}
