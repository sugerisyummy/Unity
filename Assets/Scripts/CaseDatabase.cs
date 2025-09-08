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

    [System.Serializable]
    public class EventEntry
    {
        public DolEventAsset evt;
        [Min(0f)] public float weightOverride = -1f; // <0 使用事件內 weight
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
