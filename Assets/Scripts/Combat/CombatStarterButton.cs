using UnityEngine;

namespace CyberLife.Combat
{
    public class CombatStarterButton : MonoBehaviour
    {
        public CombatPageController controller;
        public void StartNow() => controller?.StartCombat();
    }
}
