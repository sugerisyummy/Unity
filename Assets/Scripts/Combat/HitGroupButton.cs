using UnityEngine;

namespace CyberLife.Combat
{
    // 掛在六顆部位按鈕
    public class HitGroupButton : MonoBehaviour
    {
        public CombatUIController ui;     // 指向 CombatPanel/UI/CombatUI
        [Range(0,5)] public int groupIndex; // 0..5

        // Button OnClick 指到這個
        public void Fire()
        {
            if (ui == null) return;
            ui.HitGroupButton(groupIndex);   // 對齊你現有的 CombatUIController API
        }
    }
}
