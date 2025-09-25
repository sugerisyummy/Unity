using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CyberLife.Combat;   // ← 關鍵命名空間

public class CombatUIController : MonoBehaviour
{
    [Header("Refs")]
    public CombatManager manager;          // 拖到「combat manager」
    public int allyIndex = 0;

    [Header("Target UI")]
    public Combatant currentTarget;        // 目前鎖定的敵人
    public TMP_Text targetLabel;           // 顯示目標名(可留空)

    void Awake()
    {
        if (manager == null) manager = FindObjectOfType<CombatManager>();
        RefreshButtons();
    }

    // 舊按鈕相容：點敵人並自動攻擊（無指定部位）
    public void AttackAuto(Combatant enemy)
    {
        SetTarget(enemy);
        AttackAuto(); // 走無參數版本
    }

    // 舊按鈕相容：以 currentTarget 自動攻擊（無指定部位）
    public void AttackAuto()
    {
        if (manager == null || currentTarget == null) return;
        manager.PlayerAttackTarget(currentTarget);
    }

    // 讓敵人按鈕/點模型時切目標
    public void SelectTarget(Combatant enemy) => SetTarget(enemy);

    public void SetTarget(Combatant enemy)
    {
        currentTarget = enemy;
        if (targetLabel) targetLabel.text = enemy ? enemy.name : "-";
        RefreshButtons();
    }

    // 六顆群組按鈕綁這個（0~5）
    public void HitGroupButton(int groupIndex)
    {
        if (manager == null || currentTarget == null) return;

        HitGroup group = HitGroup.Torso;
        if (System.Enum.IsDefined(typeof(HitGroup), groupIndex))
            group = (HitGroup)groupIndex;

        manager.PlayerAttackTargetWithGroup(currentTarget, group);
    }

    // 沒選目標時鎖住六顆按鈕
    void RefreshButtons()
    {
        bool on = (manager != null && currentTarget != null);
        foreach (var b in GetComponentsInChildren<Button>(true))
            if (b.name.StartsWith("ChoiceButton")) b.interactable = on;
    }
}
