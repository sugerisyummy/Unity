// Auto-generated replacement by ChatGPT (UI Targeting Panel stub)
using UnityEngine;

namespace CyberLife.Combat
{
    /// <summary>
    /// Hook this to six buttons (Head/Torso/Arms/Legs/Vital/Misc).
    /// In OnClick pass index [0..5].
    /// </summary>
    public class TargetingPanel : MonoBehaviour
    {
        public CombatManager manager;
        public int allyIndex = 0;

        public void OnClickTargetIndex(int i)
        {
            if (manager == null) return;
            HitGroup group = (HitGroup)Mathf.Clamp(i, 0, 5);
            // For simplicity we just force an ally attack now;
            // if you have a more complex input queue, enqueue the chosen group.
            manager.AllyAttack(allyIndex);
        }
    }
}