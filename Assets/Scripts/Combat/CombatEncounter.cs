using UnityEngine;

namespace CyberLife.Combat
{
    [CreateAssetMenu(menuName = "CyberLife/Combat/Encounter")]
    public class CombatEncounter : ScriptableObject
    {
        public EnemyDefinition[] enemies;
        public bool allowEscape = true;
        public bool playerActsFirst = true;
    }
}
