using UnityEngine;
using System.Collections.Generic;

namespace CyberLife.Board
{
    public class PlayerState : MonoBehaviour
    {
        [Header("Runtime")]
        public string playerId = "P1";
        public int position;
        public int money = 1500;
        public List<int> ownedProperties = new List<int>();
        public int skipTurns;

        // 新增：血量
        public int hpMax = 100;
        public int hp = 100;

        public bool CanAct => skipTurns <= 0;

        public void Deposit(int amount) { money += amount; }
        public bool Withdraw(int amount)
        {
            if (money < amount) return false;
            money -= amount; return true;
        }

        public void Heal(int amount)      { hp = Mathf.Clamp(hp + Mathf.Max(0, amount), 0, hpMax); }
        public void TakeDamage(int amount){ hp = Mathf.Clamp(hp - Mathf.Max(0, amount), 0, hpMax); }
        public void SetHP(int value)      { hp = Mathf.Clamp(value, 0, hpMax); }
    }
}
