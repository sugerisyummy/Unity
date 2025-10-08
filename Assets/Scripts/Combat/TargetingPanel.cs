// TargetingPanel.cs — 舊 UI 兼容：把 0..5 的按鈕轉給 CombatUIController
using UnityEngine;

namespace CyberLife.Combat
{
    public class TargetingPanel : MonoBehaviour
    {
        public CombatUIController ui;
        public CombatManager manager; // 可留空（新流程基本用不到）

        void Awake()
        {
            if (ui == null) ui = FindObjectOfType<CombatUIController>();
        }

        // UI 六顆按鈕 → OnClick 傳 0..5
        public void OnClickTargetIndex(int i)
        {
            if (ui != null)
            {
                ui.HitGroupButton(i);
                return;
            }

            // 沒有 UIController 就直接嘗試走舊式：
            if (manager == null) return;
            var target = ui ? ui.currentTarget : null;
            if (target == null) return;

            HitGroup group = (HitGroup)Mathf.Clamp(i, 0, 5);
            manager.PlayerAttackTargetWithGroup(target, group);
        }
    }
}
