// CombatManager.cs — 開戰前補上 BasicHealth，避免秒結束
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;

namespace CyberLife.Combat
{
    public partial class CombatManager : MonoBehaviour
    {
        [Header("Teams")]   public List<Combatant> allies = new(); public List<Combatant> enemies = new();
        [Header("Runtime")] public bool isActiveCombat = false; public CombatOutcome finalOutcome = CombatOutcome.None;
        [Header("Debug")]   public bool debugVerbose = true;
        [Header("Death Visuals")] public bool fadeOutDead = true; public bool removeDeadGO = false; public float removeDelay = 1.2f;

        public event System.Action<CombatOutcome> OnCombatEnd;

        public void BeginCombat(IEnumerable<Combatant> allyTeam, IEnumerable<Combatant> enemyTeam)
        {
            allies = allyTeam?.Where(x=>x).ToList() ?? new();
            enemies = enemyTeam?.Where(x=>x).ToList() ?? new();

            // 開戰前：雙方都確保有 BasicHealth，從既有 HP/MaxHP 鏡像
            EnsureTeamHealth(allies, 80f);
            EnsureTeamHealth(enemies, 20f);

            isActiveCombat = true; finalOutcome = CombatOutcome.None;
            if (debugVerbose) Debug.Log($"[Combat] BeginCombat allies:{allies.Count} enemies:{enemies.Count}");
        }

        void EnsureTeamHealth(List<Combatant> team, float defaultHP)
        {
            foreach (var c in team)
            {
                if (!c) continue;
                var bh = c.GetComponent<BasicHealth>();
                if (bh) continue;

                float hp = TryReadFloat(c, "HP", defaultHP);
                float max = TryReadFloat(c, "MaxHP", Mathf.Max(hp, defaultHP));
                if (max < hp) max = hp;

                bh = c.gameObject.AddComponent<BasicHealth>();
                bh.MaxHP = max <= 0f ? defaultHP : max;
                bh.HP = hp <= 0f ? bh.MaxHP : hp;
                if (debugVerbose) Debug.Log($"[Combat] Auto-attach BasicHealth to {c.name} (HP {bh.HP}/{bh.MaxHP})");
            }
        }
        float TryReadFloat(object obj, string propName, float fallback)
        {
            if (obj == null) return fallback;
            var flags = BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
            var p = obj.GetType().GetProperty(propName, flags);
            if (p != null && p.CanRead)
            {
                try { return Convert.ToSingle(p.GetValue(obj)); }
                catch {}
            }
            return fallback;
        }

        public void EndCombat(CombatOutcome outcome)
        {
            isActiveCombat = false; finalOutcome = outcome;
            if (debugVerbose) Debug.Log($"[Combat] EndCombat → {outcome}");
            OnCombatEnd?.Invoke(outcome);
        }

        public void PlayerAttackTarget(Combatant target)
        {
            var attacker = FirstAlive(allies);
            if (!attacker || !target) { if (debugVerbose) Debug.Log("[Combat] PlayerAttackTarget skipped"); return; }
            var inv = attacker.inventory;
            var group  = inv ? inv.PickGroup()     : HitGroup.Torso;
            var dtype  = inv ? inv.DamageType      : DamageType.Blunt;
            var damage = inv ? inv.RollAttackDamage(): 5f;

            if (debugVerbose) Debug.Log($"[Combat] PlayerAttackTarget → {target.name} @{group} {dtype} {damage}");
            ResolveAttack(attacker, target, group, dtype, damage);
            if (CheckEnd()) return;
            EnemyTurn(); CheckEnd();
        }

        public void PlayerAttackTargetWithGroup(Combatant target, HitGroup group)
        {
            var attacker = FirstAlive(allies);
            if (!attacker || !target) { if (debugVerbose) Debug.Log("[Combat] PlayerAttackTargetWithGroup skipped"); return; }
            var inv = attacker.inventory;
            var dtype  = inv ? inv.DamageType      : DamageType.Blunt;
            var damage = inv ? inv.RollAttackDamage(): 5f;

            if (debugVerbose) Debug.Log($"[Combat] PlayerAttackTargetWithGroup → {target.name} @{group} {dtype} {damage}");
            ResolveAttack(attacker, target, group, dtype, damage);
            if (CheckEnd()) return;
            EnemyTurn(); CheckEnd();
        }

        public Combatant GetFirstAliveEnemy() => FirstAlive(enemies);
        public Combatant GetFirstAliveAlly()  => FirstAlive(allies);

        void ResolveAttack(Combatant attacker, Combatant target, HitGroup group, DamageType dtype, float damage)
        {
            if (!target) return;
            float before = ReadHP(target); bool applied = false;

            var bh = target.GetComponent<BasicHealth>();
            if (bh) { bh.ApplyDamage(group, dtype, damage); applied = true; }
            else
            {
                var t = target.GetType();
                var flags = BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
                var m1 = t.GetMethod("ApplyDamage", flags, null, new Type[]{ typeof(HitGroup), typeof(DamageType), typeof(float)}, null);
                if (m1!=null) { m1.Invoke(target,new object[]{group,dtype,damage}); applied=true; }
                else {
                    var m2 = t.GetMethod("TakeDamage", flags, null, new Type[]{ typeof(float)}, null);
                    if (m2!=null) { m2.Invoke(target,new object[]{damage}); applied=true; }
                    else {
                        var hpProp = t.GetProperty("HP", flags);
                        if (hpProp!=null && hpProp.CanRead && hpProp.CanWrite)
                        { float hp = Convert.ToSingle(hpProp.GetValue(target)); hp = Mathf.Max(0f, hp - damage); hpProp.SetValue(target, hp); applied=true; }
                    }
                }
            }

            if (!applied)
            {
                float cur = before;
                float max = Mathf.Max(cur, 1f);
                var flags = BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
                var t2 = target.GetType();
                var maxP = t2.GetProperty("MaxHP", flags);
                if (maxP != null && maxP.CanRead)
                { try { max = Mathf.Max(Convert.ToSingle(maxP.GetValue(target)), max); } catch {} }
                var newBH = target.gameObject.AddComponent<BasicHealth>();
                newBH.MaxHP = max;
                newBH.HP = cur;
                newBH.ApplyDamage(group, dtype, damage);
                applied = true;
            }

            float after = ReadHP(target);
            if (debugVerbose) Debug.Log($"[Combat] {attacker?.name} → {target?.name} @{group} {dtype} {damage} | HP {before:F1}->{after:F1} | applied:{applied}");
            if (!applied) Debug.LogWarning("[Combat] 無法對目標套用傷害（未找到 BasicHealth/ApplyDamage/TakeDamage/HP）。");

            if (!IsAlive(target))
            {
                HandleDeath(target);
                allies.RemoveAll(x => !IsAlive(x));
                enemies.RemoveAll(x => !IsAlive(x));
                var ui = FindObjectOfType<CombatUIController>();
                if (ui && ui.currentTarget == target) ui.ClearSelection();
            }
        }

        void HandleDeath(Combatant target){
            var go = target.gameObject;
            var cg = go.GetComponent<CanvasGroup>(); if (!cg) cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 0.35f;
            var btn = go.GetComponent<UnityEngine.UI.Button>(); if (btn) btn.interactable = false;
            foreach (var g in go.GetComponentsInChildren<UnityEngine.UI.Graphic>()) g.raycastTarget = false;
        }
        void EnemyTurn()
        {
            var a = FirstAlive(enemies); var t = FirstAlive(allies);
            if (!a || !t) { if (debugVerbose) Debug.Log("[Combat] EnemyTurn skipped"); return; }
            var inv = a.inventory;
            var g = inv ? inv.PickGroup() : HitGroup.Torso;
            var dt= inv ? inv.DamageType  : DamageType.Blunt;
            var dm= inv ? inv.RollAttackDamage() : 4f;

            if (debugVerbose) Debug.Log($"[Combat] EnemyTurn {a.name} → {t.name} @{g} {dt} {dm}");
            ResolveAttack(a, t, g, dt, dm);
        }

        bool CheckEnd()
        {
            bool alliesAlive  = allies.Any(IsAlive);
            bool enemiesAlive = enemies.Any(IsAlive);
            if (!enemiesAlive) { EndCombat(CombatOutcome.Win);  return true; }
            if (!alliesAlive)  { EndCombat(CombatOutcome.Lose); return true; }
            return false;
        }

        bool IsAlive(Combatant c)
        {
            if (!c) return false;
            var bh = c.GetComponent<BasicHealth>(); if (bh) return bh.IsAlive;
            var t = c.GetType(); var flags = BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
            var pi = t.GetProperty("IsAlive", flags); if (pi!=null && pi.CanRead) { try { return (bool)pi.GetValue(c);}catch{} }
            var mi = t.GetMethod("IsAlive", flags, null, Type.EmptyTypes, null); if (mi!=null){ try { return (bool)mi.Invoke(c,null);}catch{} }
            return ReadHP(c) > 0f;
        }
        float ReadHP(Combatant c)
        {
            if (!c) return 0f;
            var bh = c.GetComponent<BasicHealth>(); if (bh) return bh.HP;
            var t = c.GetType(); var flags = BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
            var hpProp = t.GetProperty("HP", flags);
            if (hpProp!=null && hpProp.CanRead) { try { return Convert.ToSingle(hpProp.GetValue(c)); } catch {} }
            return 0f;
        }
        Combatant FirstAlive(List<Combatant> list){ for(int i=0;i<list.Count;i++) if(IsAlive(list[i])) return list[i]; return null; }
    }
}
