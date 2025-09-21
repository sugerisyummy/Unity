using UnityEngine;

namespace CL.Combat
{
    [CreateAssetMenu(menuName = "CL/Combat/Encounter")]
    public class CombatEncounter : ScriptableObject
    {
        public EnemyDefinition[] enemies;
        public bool allowEscape = true;
        public bool playerActsFirst = true;
    }
}
