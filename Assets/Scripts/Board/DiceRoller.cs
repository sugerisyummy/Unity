using UnityEngine;
using UnityEngine.Events;

namespace Game.Board
{
    public sealed class DiceRoller : MonoBehaviour
    {
        [Range(2, 20)] public int sides = 6;
        public UnityEvent<int> onRolled;

        public int Roll()
        {
            int value = Random.Range(1, sides + 1);
            onRolled?.Invoke(value);
            return value;
        }
    }
}
