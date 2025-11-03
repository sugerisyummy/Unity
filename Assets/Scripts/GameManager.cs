// GameManager.cs — 完整版（含 BeginNewGame / BeginLoadGame / UpdateAllStatUI）
// 放到 Assets/Scripts/GameManager.cs 覆蓋即可。
// 這版會在 Awake() 自動尋找 CombatPageController（就算 Inspector 沒手動指也能跑）。

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CyberLife.Combat;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum Difficulty { Easy = 0, Normal = 1, Hard = 2, Master = 3 }

    [Header("資料庫 / 旗標")]
    [SerializeField] private CaseDatabase caseDB;
    [SerializeField] private EventFlagStorage flagStorage;

    [Header("視覺 / 音效")]
    [SerializeField] private CaseVisuals caseVisuals;
    [SerializeField] private Image backgroundImage;

    [Header("故事 UI")]
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private TextMeshProUGUI systemTipText;

    [Header("起始設定")]
    [SerializeField] private CaseId startCase = CaseId.None;
    [SerializeField] private int defaultHP = 100;

    // === No-Death / 軟壞結局設定 ===
    [Header("No-Death（軟結局設定）")]
    [SerializeField] private bool noDeathMode = true;
    [Tooltip("HP 低於此值觸發『衰竭』軟結局（<= 0 一定觸發）。")]
    [SerializeField] private int lowHpThreshold = 1;
    [Tooltip("理智低於等於此值觸發『崩潰』軟結局。")]
    [SerializeField] private int lowSanityThreshold = 0;

    [Tooltip("軟結局事件（可留空，留空時顯示簡訊息+直接恢復）。")]
    [SerializeField] private DolEventAsset softEndLowHP;
    [SerializeField] private DolEventAsset softEndLowSanity;

    [Tooltip("軟結局後的安全地點（可留空保持原地抽新事件）。")]
    [SerializeField] private CaseId safeCaseAfterSoftEnd = CaseId.None;

    [Header("恢復數值（軟結局後套用）")]
    [SerializeField] private int recoverHP = 30;
    [SerializeField] private int recoverSanity = 30;
    [SerializeField] private int reduceHunger = -20;
    [SerializeField] private int reduceThirst = -20;
    [SerializeField] private int reduceFatigue = -30;

    // 狀態
    private CaseId currentCase = CaseId.None;
    private Difficulty difficulty = Difficulty.Normal;

    public PlayerStats stats = new PlayerStats();

    // HUD（可不綁）
    [Header("HUD（可選）")]
    public TextMeshProUGUI hpText, moneyText, sanityText;
    public TextMeshProUGUI hungerText, thirstText, fatigueText, hopeText, obedienceText, reputationText;
    public TextMeshProUGUI techPartsText, informationText, creditsText;
    public TextMeshProUGUI augmentationLoadText, radiationText, infectionText, trustText, controlText;

    // ===== 戰鬥橋接 =====
    [Header("戰鬥橋接")]
    public CombatPageController combatController;                 // Inspector 拖 ControllerRoot；或 Awake() 自動尋找
    private DolEventAsset.EventChoice _pendingCombatChoice = null; // 暫存「這次要開戰的選項」

    // 事件
    private DolEventAsset runningEvent;
    private int runningStage = -1;

    void Awake()
    {
        if (combatController == null) combatController = FindObjectOfType<CombatPageController>(true);
        if (flagStorage == null) flagStorage = new EventFlagStorage();
        if (!caseDB) Tip("請在 GameManager 指定 CaseDatabase。");
        stats.OnChanged += UpdateAllStatUI;
    }

    // ===== 新遊戲 / 讀檔 =====
    public void BeginNewGame()
    {
        flagStorage = new EventFlagStorage();

        stats.hp = defaultHP; stats.money = 0; stats.sanity = 100;
        stats.hunger = 0; stats.thirst = 0; stats.fatigue = 0;
        stats.hope = 50; stats.obedience = 50; stats.reputation = 0;
        stats.techParts = 0; stats.information = 0; stats.credits = 0;
        stats.augmentationLoad = 0; stats.radiation = 0; stats.infection = 0;
        stats.trust = 0; stats.control = 0;

        var resolved = ResolveStartCase();
        if (resolved == CaseId.None) { Tip("沒有可觸發的事件。請在 CaseDatabase 加入事件。"); UpdateAllStatUI(); return; }
        EnterCase(resolved);
    }

    public void BeginLoadGame()
    {
        // 若你的專案有 SaveManager 就會讀取；沒有也不會報錯，直接當新遊戲。
        var d = SaveManager.Load(SaveManager.AUTO_SLOT);
        if (d == null) { BeginNewGame(); return; }

        difficulty = (Difficulty)Mathf.Clamp(d.difficulty, 0, 3);
        System.Enum.TryParse(d.currentCase, out currentCase);

        stats.ReadFrom(d);

        if (flagStorage == null) flagStorage = new EventFlagStorage();
        flagStorage.LoadFromSave(d);

        if (currentCase == CaseId.None || !HasAvailableInCase(currentCase))
        {
            var resolved = ResolveStartCase();
            if (resolved == CaseId.None) { Tip("載入後沒有可觸發事件。"); UpdateAllStatUI(); return; }
            currentCase = resolved;
        }
        EnterCase(currentCase);
    }

    // ===== 存讀槽（可選） =====
    public void SaveToSlot(int slot)
    {
        var data = new SaveData
        {
            currentCase = currentCase.ToString(),
            difficulty = (int)difficulty,
            saveTime = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm")
        };
        stats.WriteTo(data);
        flagStorage.WriteToSave(data);

        SaveManager.Save(slot, data);
        SaveManager.Save(SaveManager.AUTO_SLOT, data);
        Tip($"已存到槽 {slot}");
        UpdateAllStatUI();
    }

    public void LoadFromSlot(int slot)
    {
        var d = SaveManager.Load(slot);
        if (d == null) { Tip($"槽 {slot} 為空。"); return; }

        difficulty = (Difficulty)Mathf.Clamp(d.difficulty, 0, 3);
        System.Enum.TryParse(d.currentCase, out currentCase);

        stats.ReadFrom(d);
        if (flagStorage == null) flagStorage = new EventFlagStorage();
        flagStorage.LoadFromSave(d);

        if (currentCase == CaseId.None || !HasAvailableInCase(currentCase))
        {
            var resolved = ResolveStartCase();
            if (resolved == CaseId.None) { Tip("載入後沒有可觸發事件。"); UpdateAllStatUI(); return; }
            currentCase = resolved;
        }
        EnterCase(currentCase);
    }

    // ===== 進入地點 → 抽事件 → 播放 =====
    public void EnterCase(CaseId id)
    {
        currentCase = id;
        runningEvent = null; runningStage = -1;

        CaseVisuals.Entry visualsEntry = null;
        if (caseVisuals && caseVisuals.TryGet(currentCase, out var entry))
        {
            if (backgroundImage) backgroundImage.sprite = entry.background;
            visualsEntry = entry;
        }

        var audioManager = GameAudioManager.Instance;
        if (audioManager) audioManager.ApplyCaseAudio(visualsEntry);

        UpdateAllStatUI();
        RollAndStartEvent();
    }

    void RollAndStartEvent()
    {
        ClearChoices();
        if (!caseDB || !caseDB.TryGetPool(currentCase, out var pool) || pool == null || pool.Count == 0)
        { Tip($"地點 {currentCase} 沒事件。"); return; }

        var candidates = new List<(DolEventAsset e, float w)>();
        foreach (var e in pool)
        {
            var ev = e.evt;
            if (!ev) continue;
            if (!ev.ConditionsMet(stats.hp, flagStorage.HasFlag)) continue;
            if (ev.oncePerSave && flagStorage.IsConsumed(ev)) continue;
            if (flagStorage.IsOnCooldown(ev, ev.cooldownSeconds)) continue;
            float w = (e.weightOverride > 0f) ? e.weightOverride : Mathf.Max(0f, ev.weight);
            if (w <= 0f) continue;
            candidates.Add((ev, w));
        }
        if (candidates.Count == 0) { Tip($"地點 {currentCase} 目前沒有可觸發事件。"); return; }

        float total = 0f; foreach (var c in candidates) total += c.w;
        float r = Random.value * total, acc = 0f;
        DolEventAsset chosen = candidates[0].e;
        foreach (var c in candidates) { acc += c.w; if (r <= acc) { chosen = c.e; break; } }
        StartEvent(chosen);
    }

    void StartEvent(DolEventAsset evt)
    {
        runningEvent = evt;
        runningStage = 0;
        flagStorage.MarkConsumed(evt);
        ShowStage();
    }

    void ShowStage()
    {
        ClearChoices();
        if (runningEvent == null || runningEvent.stages == null ||
            runningStage < 0 || runningStage >= runningEvent.stages.Count)
        { Tip("事件階段錯誤。"); return; }

        var stage = runningEvent.stages[runningStage];
        if (storyText) storyText.text = stage.text;

        if (stage.choices == null || stage.choices.Count == 0) { EndEvent(); return; }

        foreach (var ch in stage.choices)
            WireChoice(ch);   // 走戰鬥橋接或一般處理
    }

    void WireChoice(DolEventAsset.EventChoice ch)
    {
        // 建立按鈕
        if (!choiceButtonPrefab || !choiceContainer) return;
        var btn = Instantiate(choiceButtonPrefab, choiceContainer);
        var label = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (label) label.text = ch.text;
        btn.onClick.RemoveAllListeners();
        choiceContainer.gameObject.SetActive(true);

        // 選項處理
        if (ch.startsCombat && ch.combat != null)
        {
            btn.onClick.AddListener(() =>
            {
                _pendingCombatChoice = ch;
                combatController.StartCombatWithEncounter(ch.combat);
            });
        }
        else
        {
            btn.onClick.AddListener(() =>
            {
                stats.ApplyChoiceDeltas(ch);
                if (ch.nextStage >= 0 && runningEvent != null && runningEvent.stages != null)
                    runningStage = Mathf.Clamp(ch.nextStage, 0, runningEvent.stages.Count - 1);
                else
                    runningStage += 1;
                ShowStage();
            });
        }
    }

    void EndEvent(){ runningEvent = null; runningStage = -1; }

    // ===== No-Death / 軟結局 =====
    bool TrySoftEnding()
    {
        if (stats.hp <= 0 || stats.hp < lowHpThreshold)
            return TriggerSoftEnd(softEndLowHP, "你眼前一黑，被拖回了陰影裡……");
        if (stats.sanity <= lowSanityThreshold)
            return TriggerSoftEnd(softEndLowSanity, "你抱住頭，喃喃自語；世界的邊緣開始溶解。");
        return false;
    }

    bool TriggerSoftEnd(DolEventAsset softAsset, string fallbackText)
    {
        if (softAsset != null && softAsset.stages != null && softAsset.stages.Count > 0)
        {
            runningEvent = softAsset;
            runningStage = 0;
            ShowStage();
            SpawnChoice("……", () => {}); // 保底
        }
        else
        {
            Tip(fallbackText);
            ApplySoftRecoveryAndRelocate();
            return true;
        }
        DelayedSoftRecoverAndRelocate();
        return true;
    }

    void ApplySoftRecoveryAndRelocate()
    {
        stats.hp = Mathf.Max(stats.hp, recoverHP);
        stats.sanity = Mathf.Max(stats.sanity, recoverSanity);
        stats.hunger = Mathf.Clamp(stats.hunger + reduceHunger, 0, 100);
        stats.thirst = Mathf.Clamp(stats.thirst + reduceThirst, 0, 100);
        stats.fatigue = Mathf.Clamp(stats.fatigue + reduceFatigue, 0, 100);
        if (stats.hp < 1) stats.hp = 1;
        UpdateAllStatUI();

        if (safeCaseAfterSoftEnd != CaseId.None) EnterCase(safeCaseAfterSoftEnd);
        else RollAndStartEvent();
    }

    async void DelayedSoftRecoverAndRelocate()
    {
        await System.Threading.Tasks.Task.Delay(100);
        ApplySoftRecoveryAndRelocate();
    }

    // ===== 舊的 Spawn/Clear（保留給軟結局等簡單用） =====
    void SpawnChoice(string text, System.Action action)
    {
        if (!choiceButtonPrefab || !choiceContainer) return;
        var btn = Instantiate(choiceButtonPrefab, choiceContainer);
        var label = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (label) label.text = text;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => action());
        choiceContainer.gameObject.SetActive(true);
    }
    void ClearChoices()
    {
        if (!choiceContainer) return;
        foreach (Transform child in choiceContainer) Destroy(child.gameObject);
        choiceContainer.gameObject.SetActive(false);
    }

    // ===== 工具 =====
    CaseId ResolveStartCase()
    {
        if (startCase != CaseId.None && HasAvailableInCase(startCase)) return startCase;
        if (caseDB != null && caseDB.cases != null)
        {
            foreach (var c in caseDB.cases)
                if (c != null && c.caseId != CaseId.None && HasAvailableInCase(c.caseId)) return c.caseId;
        }
        return CaseId.None;
    }
    bool HasAvailableInCase(CaseId id)
    {
        if (!caseDB || !caseDB.TryGetPool(id, out var pool) || pool == null) return false;
        foreach (var e in pool)
        {
            var ev = e.evt;
            if (!ev) continue;
            if (!ev.ConditionsMet(stats.hp, flagStorage.HasFlag)) continue;
            if (ev.oncePerSave && flagStorage.IsConsumed(ev)) continue;
            if (flagStorage.IsOnCooldown(ev, ev.cooldownSeconds)) continue;
            float w = (e.weightOverride > 0f) ? e.weightOverride : Mathf.Max(0f, ev.weight);
            if (w > 0f) return true;
        }
        return false;
    }

    // ===== 戰後回流 =====
    public void OnCombatWin()    { if (_pendingCombatChoice==null) return; ApplyOutcomeAndGoto(_pendingCombatChoice, CombatOutcome.Win);    _pendingCombatChoice=null; }
    public void OnCombatLose()   { if (_pendingCombatChoice==null) return; ApplyOutcomeAndGoto(_pendingCombatChoice, CombatOutcome.Lose);   _pendingCombatChoice=null; }
    public void OnCombatEscape() { if (_pendingCombatChoice==null) return; ApplyOutcomeAndGoto(_pendingCombatChoice, CombatOutcome.Escape); _pendingCombatChoice=null; }

    void ApplyOutcomeAndGoto(DolEventAsset.EventChoice c, CombatOutcome r)
    {
        // 1) 數值：只有 Win 才套用這個選項的變更（可依需求調整）
        if (r == CombatOutcome.Win) stats.ApplyChoiceDeltas(c);

        // 2) 旗標
        if (r==CombatOutcome.Win   && !string.IsNullOrEmpty(c.onWinFlag))  flagStorage.SetFlag(c.onWinFlag);
        if (r==CombatOutcome.Lose  && !string.IsNullOrEmpty(c.onLoseFlag)) flagStorage.SetFlag(c.onLoseFlag);

        // 3) 回故事面板 + 跳頁
        combatController?.BackToStory();
        int next = (r==CombatOutcome.Win) ? c.nextStageOnWin :
                   (r==CombatOutcome.Lose) ? c.nextStageOnLose : c.nextStageOnEscape;

        if (next >= 0) { runningStage = next; }
        UpdateAllStatUI();
        if (!(noDeathMode && TrySoftEnding()))
            ShowStage(); // 重繪目前頁或下一頁
    }

    void Tip(string msg){ if (storyText) storyText.text = msg; if (systemTipText) systemTipText.text = msg; Debug.LogWarning(msg); }

    // ===== HUD =====
    public void UpdateAllStatUI()
    {
        Set(hpText, $"HP {stats.hp}");
        Set(moneyText, $"Money {stats.money}");
        Set(sanityText, $"Sanity {stats.sanity}");
        Set(hungerText, $"Hunger {stats.hunger}");
        Set(thirstText, $"Thirst {stats.thirst}");
        Set(fatigueText, $"Fatigue {stats.fatigue}");
        Set(hopeText, $"Hope {stats.hope}");
        Set(obedienceText, $"Obedience {stats.obedience}");
        Set(reputationText, $"Reputation {stats.reputation}");
        Set(techPartsText, $"TechParts {stats.techParts}");
        Set(informationText, $"Information {stats.information}");
        Set(creditsText, $"Credits {stats.credits}");
        Set(augmentationLoadText, $"AugLoad {stats.augmentationLoad}");
        Set(radiationText, $"Radiation {stats.radiation}");
        Set(infectionText, $"Infection {stats.infection}");
        Set(trustText, $"Trust {stats.trust}");
        Set(controlText, $"Control {stats.control}");
    }
    void Set(TextMeshProUGUI t, string v){ if (t) t.text = v; }
}
