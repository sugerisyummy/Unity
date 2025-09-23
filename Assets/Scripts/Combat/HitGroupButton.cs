using UnityEngine;

namespace CyberLife.Combat
{
    public class HitGroupButton : MonoBehaviour
    {
        public CombatUIController ui;
        [Range(0,5)] public int groupIndex; // 0=Head,1=Torso,2=Arms,3=Legs,4=Vital,5=Misc
        public void Fire() { ui?.AttackWithGroup(groupIndex); }
    }
}
