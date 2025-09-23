// Auto-generated replacement by ChatGPT (CombatManager core, with target attack APIs)
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CyberLife.Combat
{
    public partial class CombatManager : MonoBehaviour
    {
        [Header("Teams")]
        public List<Combatant> allies = new List<Combatant>();
        public List<Combatant> enemies = new List<Combatant>();

        [Header("Runtime")]
        public CombatOutcome? finalOutcome;
        public bool isActiveCombat = false;

        System.Random rng = new System.Random();

        void Start()
        {
            // Auto-discover if placed under manager in scene
            if (allies.Count == 0) allies.AddRange(GetComponentsInChildren<Combatant>().Where(c => c.gameObject.tag == "Player"));
            if (enemies.Count == 0) enemies.AddRange(GetComponentsInChildren<Combatant>().Where(c => c.gameObject.tag == "Enemy"));
        }

        void Update()
        {
            if (!isActiveCombat) return;
            float dt = Time.deltaTime;
            foreach (var c in allies) c?.TickEffects(dt);
            foreach (var c in enemies) c?.TickEffects(dt);
        }

        public void BeginCombat(IEnumerable<Combatant> allyTeam, IEnumerable<Combatant> enemyTeam)
        {
            allies = allyTeam.Where(x=>x!=null).ToList();
            enemies = enemyTeam.Where(x=>x!=null).ToList();
            isActiveCombat = true;
            finalOutcome = null;
        }

        public void EndCombat(CombatOutcome outcome)
        {
            isActiveCombat = false;
            finalOutcome = outcome;
        }

        public bool CheckEnd()
        {
            bool alliesAlive = allies.Any(a => a != null && a.IsAlive);
            bool enemiesAlive = enemies.Any(e => e != null && e.IsAlive);
            if (!alliesAlive && !enemiesAlive)
            {
                EndCombat(CombatOutcome.Lose); // mutual K.O -> treat as lose for now
                return true;
            }
            if (!enemiesAlive)
            {
                EndCombat(CombatOutcome.Win);
                return true;
            }
            if (!alliesAlive)
            {
                EndCombat(CombatOutcome.Lose);
                return true;
            }
            return false;
        }

        public void AllyAttack(int allyIndex)
        {
            if (!isActiveCombat) return;
            var attacker = (allyIndex >= 0 && allyIndex < allies.Count) ? allies[allyIndex] : null;
            var defender = enemies.FirstOrDefault(e => e != null && e.IsAlive);
            if (attacker == null || defender == null) return;

            var inv = attacker.inventory;
            var group = inv != null ? inv.PickGroup() : HitGroup.Torso;
            var damage = inv != null ? inv.RollAttackDamage() : 5f;
            var dtype = inv != null ? inv.DamageType : DamageType.Blunt;

            ResolveAttack(attacker, defender, group, dtype, damage);
            if (CheckEnd()) return;

            // simple enemy retaliation
            EnemyTurn();
            CheckEnd();
        }

        public void ResolveAttack(Combatant attacker, Combatant defender, HitGroup group, DamageType type, float rawDamage)
        {
            if (defender == null || attacker == null) return;

            float finalDamage = ResolveDamageWithArmor(defender, group, type, rawDamage);
            var targetPart = defender.PickRandomPart(group, rng);
            if (targetPart == null) return;

            targetPart.ApplyDamage(finalDamage);
        }

        void EnemyTurn()
        {
            var attacker = enemies.FirstOrDefault(e => e != null && e.IsAlive);
            var defender = allies.FirstOrDefault(a => a != null && a.IsAlive);
            if (attacker == null || defender == null) return;
            var inv = attacker.inventory;
            var group = inv != null ? inv.PickGroup() : HitGroup.Torso;
            var damage = inv != null ? inv.RollAttackDamage() : 5f;
            var dtype = inv != null ? inv.DamageType : DamageType.Blunt;
            ResolveAttack(attacker, defender, group, dtype, damage);
        }

        // === 新增：點選鎖定敵人 → 直接攻擊（自動選群組） ===
        public void PlayerAttackTarget(Combatant target)
        {
            if (!isActiveCombat) return;
            var attacker = (allies.Count > 0) ? allies[0] : null;
            if (attacker == null || target == null || !target.IsAlive) return;

            var inv = attacker.inventory;
            var group  = inv != null ? inv.PickGroup() : HitGroup.Torso;
            var damage = inv != null ? inv.RollAttackDamage() : 5f;
            var dtype  = inv != null ? inv.DamageType : DamageType.Blunt;

            ResolveAttack(attacker, target, group, dtype, damage);
            if (CheckEnd()) return;
            EnemyTurn();
            CheckEnd();
        }

        // === 新增：指定群組（Head/Torso/Arms/Legs/Vital/Misc）攻擊 ===
        public void PlayerAttackTargetWithGroup(Combatant target, HitGroup group)
        {
            if (!isActiveCombat) return;
            var attacker = (allies.Count > 0) ? allies[0] : null;
            if (attacker == null || target == null || !target.IsAlive) return;

            var inv = attacker.inventory;
            var damage = inv != null ? inv.RollAttackDamage() : 5f;
            var dtype  = inv != null ? inv.DamageType : DamageType.Blunt;

            ResolveAttack(attacker, target, group, dtype, damage);
            if (CheckEnd()) return;
            EnemyTurn();
            CheckEnd();
        }
    }
}
