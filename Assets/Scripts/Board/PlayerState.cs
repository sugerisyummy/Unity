using UnityEngine;
using System.Collections.Generic;

namespace CyberLife.Board
{
    public class PlayerState : MonoBehaviour
    {
        [Header("Runtime")]
        public string playerId = "P1";
        public int position;              // index on board
        public int money = 1500;
        public List<int> ownedProperties = new List<int>();
        public int skipTurns;             // jail/rest etc.

        public bool CanAct => skipTurns <= 0;

        public void Deposit(int amount) { money += amount; }
        public bool Withdraw(int amount)
        {
            if (money < amount) return false;
            money -= amount;
            return true;
        }
    }
}