using UnityEngine;

namespace CyberLife.Combat
{
    [CreateAssetMenu(menuName = "CL/Combat/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        public string enemyName = "Drone";
        public Sprite portrait;
        public int maxHP = 30;
        public int attack = 6;
        public int defense = 2;
        public int speed = 5;

        public EnemyInstance CreateInstance()
        {
            return new EnemyInstance { def = this, currentHP = maxHP };
        }
    }

    [System.Serializable]
    public class EnemyInstance
    {
        public EnemyDefinition def;
        public int currentHP;
        public int MaxHP => def != null ? def.maxHP : 1;
        public bool IsDead => currentHP <= 0;
        public string DisplayName => def != null ? def.enemyName : "Enemy";
    }
}
