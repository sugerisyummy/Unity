using System;
using System.Collections.Generic;
using UnityEngine;

namespace CL.Combat
{
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        public CombatEncounter currentEncounter;

        // 舊 API 仍保留（以免其他地方呼叫）
        public int playerMaxHP = 100;
        public int playerHP = 100;
        public int playerAttack = 12;
        public int playerDefense = 3;
        public int playerSpeed = 6;

        // 進階模型
        public Combatant player;
        public List<Combatant> enemies = new List<Combatant>();
        public BodyPartId selectedTarget = BodyPartId.Torso;

        public bool InCombat { get; private set; }
        public bool PlayerTurn { get; private set; }
        public bool LastWin { get; private set; }
        public bool LastEscaped { get; private set; }

        public event Action OnCombatStarted;
        public event Action OnCombatEnded;
        public event Action<string> OnLog;
        public event Action OnStateChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Begin(CombatEncounter encounter, int pMaxHP, int pHP, int pAtk, int pDef, int pSpd)
        {
            currentEncounter = encounter;
            // 舊參數仍存，但進階模型為主
            playerMaxHP = pMaxHP;
            playerHP = Mathf.Clamp(pHP, 0, pMaxHP);
            playerAttack = pAtk;
            playerDefense = pDef;
            playerSpeed = pSpd;

            // 建玩家/敵人
            player = new Combatant { displayName = "You", baseSpeed = pSpd, baseDefense = pDef, baseAccuracy = 0 };
            player.InitDefaultHuman();
            // default weapon (空手=鈍)
            player.mainHand = ScriptableObject.CreateInstance<WeaponDef>();
            player.mainHand.category = WeaponCategory.Blunt;
            player.mainHand.baseDamage = Math.Max(1, pAtk);

            enemies.Clear();
            if (encounter != null && encounter.enemies != null)
            {
                foreach (var e in encounter.enemies)
                {
                    if (!e) continue;
                    var c = new Combatant { displayName = e.enemyName, baseSpeed = e.speed, baseDefense = e.defense, baseAccuracy = 0 };
                    c.InitDefaultHuman(); // 先用人形；之後可依 EnemyDefinition 決定
                    var claw = ScriptableObject.CreateInstance<WeaponDef>();
                    claw.category = WeaponCategory.Blunt;
                    claw.baseDamage = Math.Max(1, e.attack);
                    c.mainHand = claw;
                    enemies.Add(c);
                }
            }

            InCombat = true;
            LastWin = false;
            LastEscaped = false;
            PlayerTurn = encounter == null ? true : encounter.playerActsFirst;
            OnCombatStarted?.Invoke();
            Log("戰鬥開始");
            OnStateChanged?.Invoke();

            if (!PlayerTurn) EnemyAct();
        }

        void Log(string s){ OnLog?.Invoke(s); }

        // === Player Actions ===
        public void SetTarget(BodyPartId id)
        {
            selectedTarget = id;
            Log($"目標：{id}");
            OnStateChanged?.Invoke();
        }

        public void PlayerAttackSelected()
        {
            if (!InCombat || !PlayerTurn) return;
            var target = FirstAlive(enemies);
            if (target == null) { EndCombat(true); return; }

            var part = target.parts.Find(p => p.id == selectedTarget) ?? target.parts.Find(p => p.id == BodyPartId.Torso);
            int raw = CalcBaseDamage(player, target);
            int afterArmor = ApplyArmorReduction(raw, target, part);
            int dmg = Mathf.Max(0, afterArmor);
            part.hp = Mathf.Max(0, part.hp - dmg);

            // 疼痛/流血簡化版
            part.pain = Mathf.Clamp(part.pain + Mathf.CeilToInt(dmg * 2f), 0, 100);
            part.bleedRate += dmg * 0.02f;

            // 斷肢/致命器官檢定（簡化）
            if (dmg > part.maxHP * 0.6f)
            {
                if (part.id == BodyPartId.LeftArm || part.id == BodyPartId.RightArm ||
                    part.id == BodyPartId.LeftLeg || part.id == BodyPartId.RightLeg ||
                    part.id == BodyPartId.LeftHand || part.id == BodyPartId.RightHand ||
                    part.id == BodyPartId.LeftFoot || part.id == BodyPartId.RightFoot)
                    part.isSevered = true;
                if (part.Vital) part.hp = 0;
            }

            Log($"你攻擊 {target.displayName} 的 {part.id}，造成 {dmg} 傷害");
            OnStateChanged?.Invoke();

            if (IsGroupDead(enemies)) { EndCombat(true); return; }
            PlayerTurn = false;
            EnemyAct();
        }

        public void TryEscape()
        {
            if (!InCombat || currentEncounter == null || !currentEncounter.allowEscape) return;
            if (UnityEngine.Random.value < 0.5f) { EndCombat(false, true); }
            else { PlayerTurn = false; EnemyAct(); }
        }

        // === Enemy Actions (簡化：第一個活著的攻玩家軀幹) ===
        void EnemyAct()
        {
            if (!InCombat) return;
            var attacker = FirstAlive(enemies);
            if (attacker == null) { EndCombat(true); return; }

            var torso = player.parts.Find(p => p.id == BodyPartId.Torso);
            int raw = CalcBaseDamage(attacker, player);
            int after = ApplyArmorReduction(raw, player, torso);
            int dmg = Mathf.Max(0, after);
            torso.hp = Mathf.Max(0, torso.hp - dmg);
            torso.pain = Mathf.Clamp(torso.pain + Mathf.CeilToInt(dmg * 2f), 0, 100);

            Log($"{attacker.displayName} 攻擊你的 Torso，造成 {dmg} 傷害");
            OnStateChanged?.Invoke();

            if (player.IsDead) { EndCombat(false); return; }
            PlayerTurn = true;
        }

        // === Calc/Util ===
        int CalcBaseDamage(Combatant atk, Combatant def)
        {
            int baseD = atk.mainHand ? atk.mainHand.baseDamage : 3;
            int acc = atk.baseAccuracy + (atk.mainHand ? atk.mainHand.accuracy : 0);
            int defv = def.Defense;
            int hit = Mathf.Clamp(70 + acc - defv, 5, 95);
            if (UnityEngine.Random.Range(0,100) > hit) { Log("Miss!"); return 0; }
            return baseD;
        }

        int ApplyArmorReduction(int raw, Combatant def, BodyPartState part)
        {
            int red = 0;
            ArmorDef a = ArmorCovering(def, part.id);
            if (a != null)
            {
                // 粗略：以鈍擊視剩餘比重，先用最簡版：固定減傷值取最大
                red = Math.Max(Math.Max(a.armorSlash, a.armorPierce), Math.Max(Math.Max(a.armorBlunt, a.armorThermal), Math.Max(a.armorChemical, a.armorBallistic)));
            }
            return raw - red;
        }

        ArmorDef ArmorCovering(Combatant c, BodyPartId part)
        {
            if (part == BodyPartId.Head || part == BodyPartId.Brain || part == BodyPartId.LeftEye || part == BodyPartId.RightEye || part == BodyPartId.Jaw)
                return c.head;
            if (part == BodyPartId.Torso || part == BodyPartId.Heart || part == BodyPartId.LeftLung || part == BodyPartId.RightLung ||
                part == BodyPartId.Liver || part == BodyPartId.Stomach || part == BodyPartId.LeftKidney || part == BodyPartId.RightKidney || part == BodyPartId.Neck)
                return c.torso;
            if (part.ToString().Contains("Leg") || part.ToString().Contains("Foot"))
                return c.legs;
            return null;
        }

        Combatant FirstAlive(List<Combatant> list) => list.Find(c => !c.IsDead);
        bool IsGroupDead(List<Combatant> list)
        {
            foreach (var c in list) if (!c.IsDead) return false;
            return true;
        }

        void EndCombat(bool win, bool escaped=false)
        {
            if (!InCombat) return;
            InCombat = false;
            LastWin = win;
            LastEscaped = escaped;
            OnCombatEnded?.Invoke();
            OnStateChanged?.Invoke();
            Log(win ? "勝利" : escaped ? "逃脫" : "戰敗");
        }
    }
}
