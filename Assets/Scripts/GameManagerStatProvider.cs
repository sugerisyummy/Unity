// GameManagerStatProvider.cs (替換版)
// 作用：把 GameManager 的 stats 值提供給 HUD（StatBar/HUDStats）。
// 修正點：不再讀取不存在的 CurrentHP/MaxHP，改讀 gameManager.stats.*，上限暫定 100。

using UnityEngine;

public class GameManagerStatProvider : MonoBehaviour, IStatProvider
{
    [Tooltip("場景中的 GameManager；可不手動指定，Awake 時會自動尋找")]
    public GameManager gameManager;

    [Header("各數值上限（可依你的遊戲規則調整）")]
    public float hpMax = 100f;
    public float sanityMax = 100f;
    public float defaultMax = 100f; // 其餘 0~100 類型用這個

    void Awake()
    {
        if (!gameManager) gameManager = FindObjectOfType<GameManager>();
    }

    public bool TryGet(StatType type, out float current, out float max)
    {
        current = 0f; max = 1f;
        if (!gameManager) return false;
        var s = gameManager.stats;

        switch (type)
        {
            case StatType.HP:
                current = s.hp;
                max = hpMax;
                return true;

            case StatType.Sanity:
                current = s.sanity;
                max = sanityMax;
                return true;

            case StatType.Hunger:
                current = s.hunger;   // 0(飽)~100(餓)
                max = defaultMax;
                return true;

            case StatType.Thirst:
                current = s.thirst;   // 0(解渴)~100(口渴)
                max = defaultMax;
                return true;

            case StatType.Fatigue:
                current = s.fatigue;  // 0(精神)~100(疲勞)
                max = defaultMax;
                return true;

            case StatType.Hope:
                current = s.hope;     // 0~100
                max = defaultMax;
                return true;

            // 你已實作的其餘數值，想顯示成條就往下加：
            // case StatType.Obedience: current = s.obedience; max = defaultMax; return true;
            // case StatType.Reputation: current = s.reputation; max = defaultMax; return true;
            // case StatType.AugmentationLoad: current = s.augmentationLoad; max = defaultMax; return true;
            // case StatType.Radiation: current = s.radiation; max = defaultMax; return true;
            // case StatType.Infection: current = s.infection; max = defaultMax; return true;
            // case StatType.Trust: current = s.trust; max = defaultMax; return true;
            // case StatType.Control: current = s.control; max = defaultMax; return true;
        }

        return false;
    }
}
