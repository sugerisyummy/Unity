// DolEvent.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EventChoice
{
    public string text;
    public int hpChange;
    public CaseId nextCase;
}

[Serializable]
public class DolEvent
{
    [TextArea(2, 6)] public string text;
    public List<EventChoice> choices = new();

    [Header("條件")]
    public int minHP = int.MinValue;
    public int maxHP = int.MaxValue;

    [Header("加權")]
    [Min(0f)] public float weight = 1f;

    public bool ConditionsMet(int hp) => hp >= minHP && hp <= maxHP;
}
