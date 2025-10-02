
// Copyright (c) 2025
// CombatUIController — target-first flow (enum index cast)
using UnityEngine;
using UnityEngine.UI;

namespace CyberLife.Combat
{
    // 依 #57 規範：0=Head,1=Torso,2=LeftArm,3=RightArm,4=LeftLeg,5=RightLeg
    public class CombatUIController : MonoBehaviour
    {
        [Header("Refs")]
        public CombatManager manager;
        public GameObject groupsPanel;   // 6 個部位按鈕的父物件
        [HideInInspector] public Combatant currentTarget;

        void Awake()
        {
            if (groupsPanel) groupsPanel.SetActive(false);
        }

        public void SelectTarget(Combatant target)
        {
            currentTarget = target;
            if (groupsPanel) groupsPanel.SetActive(currentTarget != null);
            RefreshButtons();
        }

        // 6 顆部位按鈕 OnClick 綁這個：HitGroupButton(0~5)
        public void HitGroupButton(int groupIndex)
        {
            if (!manager || !currentTarget) return;

            // 將 int 轉對應的 HitGroup（避免列舉名稱不同造成 CS0117）
            var hg = IndexToHitGroup(groupIndex);
            manager.PlayerAttackTargetWithGroup(currentTarget, hg);

            // 攻擊後收起面板並清空選取
            currentTarget = null;
            if (groupsPanel) groupsPanel.SetActive(false);
            RefreshButtons();
        }

        // 相容舊腳本：自動攻擊目前選取的敵人（若未選取則不做事）
        public void AttackAuto()
        {
            if (!manager || !currentTarget) return;
            manager.PlayerAttackTarget(currentTarget);
            currentTarget = null;
            if (groupsPanel) groupsPanel.SetActive(false);
            RefreshButtons();
        }

        // 相容舊腳本：指定敵人自動攻擊
        public void AttackAuto(Combatant enemy)
        {
            if (!manager || !enemy) return;
            manager.PlayerAttackTarget(enemy);
            if (groupsPanel) groupsPanel.SetActive(false);
            if (currentTarget == enemy) currentTarget = null;
            RefreshButtons();
        }

        private HitGroup IndexToHitGroup(int i)
        {
            if (i < 0) i = 0;
            if (i > 5) i = 5;
            // 直接以索引轉列舉，避免依賴列舉成員名稱
            return (HitGroup)i;
        }

        public void RefreshButtons()
        {
            // 需要時可在此依 currentTarget 來設定 interactable 狀態
        }
    }
}
