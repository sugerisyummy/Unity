// GameManager.cs（No-Death 軟結局 + 旗標序列化 + infection HUD）
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public TextMeshProUGUI augmentationLoadText, radiationText, infectionText, trustText, controlText; // ★ infectionText

    // 事件
    private DolEventAsset runningEvent;
    private int runningStage = -1;

    void Awake()
    {
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

    // ===== 存讀槽 =====
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

        if (caseVisuals && caseVisuals.TryGet(currentCase, out var entry))
        {
            if (backgroundImage) backgroundImage.sprite = entry.background;
            if (AudioManager.Instance) AudioManager.Instance.PlayBGM(entry.bgm, 1f);
            // SoundEffectManager.Instance?.ApplyCaseAmbience(entry);
        }

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
            float w = (e.weightOverride >= 0f) ? e.weightOverride : Mathf.Max(0f, ev.weight);
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
        {
            SpawnChoice(ch.text, () =>
            {
                // 1) 數值
                stats.ApplyChoiceDeltas(ch);

                // 2) 旗標
                foreach (var f in ch.setFlagsTrue)  if (!string.IsNullOrEmpty(f)) flagStorage.SetFlag(f);
                foreach (var f in ch.setFlagsFalse) if (!string.IsNullOrEmpty(f)) flagStorage.ClearFlag(f);

                // 3) No-Death 檢查（可能轉入軟結局）
                if (noDeathMode && TrySoftEnding()) return;

                // 4) HUD
                UpdateAllStatUI();

                // 5) 跳轉
                if (ch.nextStage >= 0)
                {
                    runningStage = ch.nextStage;
                    ShowStage();
                    return;
                }

                if (ch.endEvent)
                {
                    var goOther = ch.gotoCaseAfterEnd && ch.gotoCase != CaseId.None;
                    EndEvent();
                    if (goOther) EnterCase(ch.gotoCase);
                    else RollAndStartEvent();
                    return;
                }
                ShowStage();
            });
        }
    }

    void EndEvent(){ runningEvent = null; runningStage = -1; }

    // ===== No-Death / 軟結局 =====
    bool TrySoftEnding()
    {
        // 低於門檻就觸發；HP ≤ 0 或 Sanity ≤ 0 為強制觸發
        if (stats.hp <= 0 || stats.hp < lowHpThreshold)
            return TriggerSoftEnd(softEndLowHP, "你眼前一黑，被拖回了陰影裡……");

        if (stats.sanity <= lowSanityThreshold)
            return TriggerSoftEnd(softEndLowSanity, "你抱住頭，喃喃自語；世界的邊緣開始溶解。");

        return false;
    }

    bool TriggerSoftEnd(DolEventAsset softAsset, string fallbackText)
    {
        // 1) 若有指定軟結局事件，直接播放該事件
        if (softAsset != null && softAsset.stages != null && softAsset.stages.Count > 0)
        {
            runningEvent = softAsset;
            runningStage = 0;
            ShowStage();

            // 在軟結局事件「最後一頁 endEvent」後，做恢復 → 安全地點/抽事件
            // 做法：在 EndEvent 之後立刻恢復與傳送
            SpawnChoice("……", () => {}); // 保底：不會顯示；僅確保場景穩定
        }
        else
        {
            // 2) 沒資產就直接顯示一句話 + 恢復 + 跳轉
            Tip(fallbackText);
            ApplySoftRecoveryAndRelocate();
            return true;
        }

        // 讓玩家點完軟結局事件後也能恢復與跳轉：
        // 簡單做法：在下一幀排程
        DelayedSoftRecoverAndRelocate();

        return true;
    }

    void ApplySoftRecoveryAndRelocate()
    {
        // 恢復
        stats.hp = Mathf.Max(stats.hp, recoverHP);
        stats.sanity = Mathf.Max(stats.sanity, recoverSanity);
        stats.hunger = Mathf.Clamp(stats.hunger + reduceHunger, 0, 100);
        stats.thirst = Mathf.Clamp(stats.thirst + reduceThirst, 0, 100);
        stats.fatigue = Mathf.Clamp(stats.fatigue + reduceFatigue, 0, 100);

        // 最低保護（No-Death）
        if (stats.hp < 1) stats.hp = 1;

        UpdateAllStatUI();

        // 傳送或抽下一個事件
        if (safeCaseAfterSoftEnd != CaseId.None)
            EnterCase(safeCaseAfterSoftEnd);
        else
            RollAndStartEvent();
    }

    async void DelayedSoftRecoverAndRelocate()
    {
        // 粗暴一點：等 0.1 秒（確保 UI/事件切換完）
        await System.Threading.Tasks.Task.Delay(100);
        ApplySoftRecoveryAndRelocate();
    }

    // ===== UI 產生 =====
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
            float w = (e.weightOverride >= 0f) ? e.weightOverride : Mathf.Max(0f, ev.weight);
            if (w > 0f) return true;
        }
        return false;
    }
    void Tip(string msg){ if (storyText) storyText.text = msg; if (systemTipText) systemTipText.text = msg; Debug.LogWarning(msg); }

    // ===== HUD =====
    public void UpdateAllStatUI()
    {
        Set(hpText, $"HP:{stats.hp}");
        Set(moneyText, $"{stats.money}G");
        Set(sanityText, $"Sanity:{stats.sanity}");
        Set(hungerText, $"Hunger:{stats.hunger}");
        Set(thirstText, $"Thirst:{stats.thirst}");
        Set(fatigueText, $"Fatigue:{stats.fatigue}");
        Set(hopeText, $"Hope:{stats.hope}");
        Set(obedienceText, $"Obedience:{stats.obedience}");
        Set(reputationText, $"Reputation:{stats.reputation}");
        Set(techPartsText, $"TechParts:{stats.techParts}");
        Set(informationText, $"Information:{stats.information}");
        Set(creditsText, $"Credits:{stats.credits}");
        Set(augmentationLoadText, $"AugLoad:{stats.augmentationLoad}");
        Set(radiationText, $"Radiation:{stats.radiation}");
        Set(infectionText, $"Infection:{stats.infection}"); // ★
        Set(trustText, $"Trust:{stats.trust}");
        Set(controlText, $"Control:{stats.control}");
    }
    void Set(TextMeshProUGUI t, string v){ if (t) t.text = v; }
}
