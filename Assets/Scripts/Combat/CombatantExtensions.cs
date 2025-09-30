using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace CyberLife.Combat
{
    public static class CombatantExtensions
    {
        static void SetIfExists(object obj, string name, object val)
        {
            var t = obj.GetType();
            var f = t.GetField(name, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
            if (f != null && f.FieldType.IsAssignableFrom(val?.GetType() ?? typeof(object))) { f.SetValue(obj, val); return; }
            var p = t.GetProperty(name, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
            if (p != null && p.CanWrite) { p.SetValue(obj, val); }
        }

        public static void ApplyBodyPreset(this Combatant c, BodyPreset preset)
        {
            if (c == null || preset == null) return;
            if (c.parts == null) c.parts = new List<BodyPartState>();
            c.parts.Clear();

            foreach (var e in preset.parts)
            {
                if (e == null) continue;

                BodyPartState s = null;

                // 嘗試使用 (id, tag, maxHP) 建構
                try { s = new BodyPartState(e.id, e.tag, e.maxHP); }
                catch { s = new BodyPartState(); }

                // 反射填欄位（哪個存在就填哪個）
                SetIfExists(s, "id",     e.id);
                SetIfExists(s, "name",   e.id);
                SetIfExists(s, "tag",    e.tag);
                SetIfExists(s, "group",  e.tag);
                SetIfExists(s, "maxHP",  e.maxHP);
                SetIfExists(s, "max",    e.maxHP);
                SetIfExists(s, "maxHealth", e.maxHP);
                // current 不一定有，就不強制
                SetIfExists(s, "currentHP", e.maxHP);
                SetIfExists(s, "hp",        e.maxHP);

                c.parts.Add(s);
            }
        }
    }
}
