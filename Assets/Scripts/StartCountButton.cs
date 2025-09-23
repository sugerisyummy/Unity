using UnityEngine;
using System.Reflection;

namespace CyberLife.Combat
{
    public class StartCountButton : MonoBehaviour
    {
        public CombatPageController controller;  // 可留空（自動尋找）
        public int count = 1;

        public void StartNow()
        {
            if (controller == null) controller = FindObjectOfType<CombatPageController>(true);
            if (controller == null) { Debug.LogError("[StartCountButton] 找不到 CombatPageController"); return; }

            // 進階版：StartCombatWithCount(int)
            var m = typeof(CombatPageController).GetMethod("StartCombatWithCount", BindingFlags.Public|BindingFlags.Instance);
            if (m != null) { m.Invoke(controller, new object[]{ Mathf.Max(1, count) }); return; }

            // 舊版：只有 StartCombat()
            controller.StartCombat();
        }
    }
}
