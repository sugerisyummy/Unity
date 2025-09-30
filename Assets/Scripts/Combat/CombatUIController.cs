
// Copyright (c) 2025
// Patched by ChatGPT — groupsPanel hidden by default, shown on SelectTarget(),
// and hidden again after an attack. Also keeps simple HitGroupButton API.
// Now casts int -> HitGroup enum to match manager API signatures.

using UnityEngine;

namespace CyberLife.Combat
{
    public class CombatUIController : MonoBehaviour
    {
        public CombatManager manager;
        public GameObject groupsPanel;      // parent of 6 hit-group buttons
        public Combatant currentTarget;

        private void Start()
        {
            if (groupsPanel) groupsPanel.SetActive(false); // hide on start
        }

        public void SetTarget(Combatant t) => SelectTarget(t);

        public void SelectTarget(Combatant t)
        {
            currentTarget = t;
            if (groupsPanel) groupsPanel.SetActive(t != null);
            RefreshButtons();
        }

        public void HitGroupButton(int groupIndex)
        {
            if (!currentTarget || manager == null) return;
            manager.PlayerAttackTargetWithGroup(currentTarget, (HitGroup)groupIndex);

            // After attack: hide and clear selection
            currentTarget = null;
            if (groupsPanel) groupsPanel.SetActive(false);
            RefreshButtons();
        }

        // For compatibility with some existing calls
        public void AttackAuto()
        {
            if (!currentTarget || manager == null) return;
            manager.PlayerAttackTarget(currentTarget);
            currentTarget = null;
            if (groupsPanel) groupsPanel.SetActive(false);
            RefreshButtons();
        }

        public void AttackAuto(Combatant enemy)
        {
            if (enemy == null || manager == null) return;
            manager.PlayerAttackTarget(enemy);
            currentTarget = null;
            if (groupsPanel) groupsPanel.SetActive(false);
            RefreshButtons();
        }

        public void HitGroupOnEnemy(Combatant owner, int groupIndex)
        {
            SelectTarget(owner);
            HitGroupButton(groupIndex);
        }

        public void RefreshButtons()
        {
            // hook for enabling/disabling buttons if needed later
        }
    }
}
