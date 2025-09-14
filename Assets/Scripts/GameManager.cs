using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum Difficulty { Easy = 0, Normal = 1, Hard = 2, Master = 3 }

    [Header("資料參考")]
    [SerializeField] private CaseDatabase caseDB;
    [SerializeField] private EventFlagStorage flagStorage;

    [Header("視覺/音效")]
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

    // ====== 遊戲數值 ======
    private CaseId currentCase = CaseId.None;
    private Difficulty difficulty = Difficulty.Normal;

    // Core
    private int hp;

    // Dystopia Stats
    private int money, sanity, hunger, thirst, fatigue, hope, obedience, reputation;
    private int techParts, information, credits, augmentationLoad, radiation, trust, control;

    // UI 連結（可拖 UI Text）—— 不一定要全部填
    [Header("UI 連結（可選，拖對應 TMP Text）")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI sanityText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI thirstText;
    public TextMeshProUGUI fatigueText;
    public TextMeshProUGUI hopeText;
    public TextMeshProUGUI obedienceText;
    public TextMeshProUGUI reputationText;
    public TextMeshProUGUI techPartsText;
    public TextMeshProUGUI informationText;
    public TextMeshProUGUI creditsText;
    public TextMeshProUGUI augmentationLoadText;
    public TextMeshProUGUI radiationText;
    public TextMeshProUGUI trustText;
    public TextMeshProUGUI controlText;

    // 事件進行
    private DolEventAsset runningEvent;
    private int runningStage = -1;

    void Awake()
    {
        if (flagStorage == null) flagStorage = new EventFlagStorage();
        if (!caseDB) Tip("請在 GameManager 指定 CaseDatabase。");
    }

    // ====== 進入新遊戲 / 載入 ======
    public void BeginNewGame()
    {
        // 預設初始值（可改為 Inspector 參數化）
        hp = defaultHP;

        money = 0; sanity = 100;
        hunger = 0; thirst = 0; fatigue = 0;
        hope = 50; obedience = 50; reputation = 0;
        techParts = 0; information = 0; credits = 0;
        augmentationLoad = 0; radiation = 0; trust = 0; control = 0;

        var resolved = ResolveStartCase();
        if (resolved == CaseId.None) { Tip("目前沒有任何可觸發的事件。請在 CaseDatabase 加入事件。"); UpdateAllStatUI(); return; }
        EnterCase(resolved);
    }

    public void BeginLoadGame()
    {
        var d = SaveManager.Load(SaveManager.AUTO_SLOT);
        if (d == null) { BeginNewGame(); return; }

        // 邏輯：若新欄位不存在於舊檔案，皆可採用上面的 BeginNewGame 預設，但為簡化直接安全讀入（C# 預設 0）
        hp = d.hp;
        difficulty = (Difficulty)Mathf.Clamp(d.difficulty, 0, 3);
        System.Enum.TryParse(d.currentCase, out currentCase);

        money = d.money; sanity = d.sanity;
        hunger = d.hunger; thirst = d.thirst; fatigue = d.fatigue;
        hope = d.hope; obedience = d.obedience; reputation = d.reputation;
        techParts = d.techParts; information = d.information; credits = d.credits;
        augmentationLoad = d.augmentationLoad; radiation = d.radiation; trust = d.trust; control = d.control;

        if (currentCase == CaseId.None || !HasAvailableInCase(currentCase))
        {
            var resolved = ResolveStartCase();
            if (resolved == CaseId.None) { Tip("載入後沒有可觸發事件。"); UpdateAllStatUI(); return; }
            currentCase = resolved;
        }
        EnterCase(currentCase);
    }

    // ====== 存讀檔 ======
    public void SaveToSlot(int slot)
    {
        var data = new SaveData
        {
            currentCase = currentCase.ToString(),
            hp = hp,
            difficulty = (int)difficulty,
            saveTime = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm"),

            money = money, sanity = sanity,
            hunger = hunger, thirst = thirst, fatigue = fatigue,
            hope = hope, obedience = obedience, reputation = reputation,
            techParts = techParts, information = information, credits = credits,
            augmentationLoad = augmentationLoad, radiation = radiation, trust = trust, control = control
        };
        SaveManager.Save(slot, data);
        SaveManager.Save(SaveManager.AUTO_SLOT, data);
        Tip($"已存到槽 {slot}");
        UpdateAllStatUI();
    }

    public void LoadFromSlot(int slot)
    {
        var d = SaveManager.Load(slot);
        if (d == null) { Tip($"槽 {slot} 為空。"); return; }

        hp = d.hp;
        difficulty = (Difficulty)Mathf.Clamp(d.difficulty, 0, 3);
        System.Enum.TryParse(d.currentCase, out currentCase);

        money = d.money; sanity = d.sanity;
        hunger = d.hunger; thirst = d.thirst; fatigue = d.fatigue;
        hope = d.hope; obedience = d.obedience; reputation = d.reputation;
        techParts = d.techParts; information = d.information; credits = d.credits;
        augmentationLoad = d.augmentationLoad; radiation = d.radiation; trust = d.trust; control = d.control;

        if (currentCase == CaseId.None || !HasAvailableInCase(currentCase))
        {
            var resolved = ResolveStartCase();
            if (resolved == CaseId.None) { Tip("載入後沒有可觸發事件。"); UpdateAllStatUI(); return; }
            currentCase = resolved;
        }
        EnterCase(currentCase);
    }

    // ====== 進入地點 & 事件流程 ======
    public void EnterCase(CaseId id)
    {
        currentCase = id;
        runningEvent = null; runningStage = -1;

        if (caseVisuals && caseVisuals.TryGet(currentCase, out var entry))
        {
            if (backgroundImage) backgroundImage.sprite = entry.background;
            if (AudioManager.Instance) AudioManager.Instance.PlayBGM(entry.bgm, 1f);
        }

        UpdateAllStatUI();
        RollAndStartEvent();
    }

    private void RollAndStartEvent()
    {
        ClearChoices();
        if (!caseDB || !caseDB.TryGetPool(currentCase, out var pool) || pool == null || pool.Count == 0)
        {
            Tip($"地點 {currentCase} 沒有任何事件。"); return;
        }

        var candidates = new List<(DolEventAsset e, float w)>();
        foreach (var entry in pool)
        {
            var e = entry.evt;
            if (!e) continue;
            if (!e.ConditionsMet(hp, flagStorage.HasFlag)) continue;
            if (e.oncePerSave && flagStorage.IsConsumed(e)) continue;
            if (flagStorage.IsOnCooldown(e, e.cooldownSeconds)) continue;
            float w = (entry.weightOverride >= 0f) ? entry.weightOverride : Mathf.Max(0f, e.weight);
            if (w <= 0f) continue;
            candidates.Add((e, w));
        }
        if (candidates.Count == 0) { Tip($"地點 {currentCase} 目前沒有可觸發事件。"); return; }

        float total = 0f; foreach (var c in candidates) total += c.w;
        float r = Random.value * total, acc = 0f;
        DolEventAsset chosen = candidates[0].e;
        foreach (var c in candidates) { acc += c.w; if (r <= acc) { chosen = c.e; break; } }
        StartEvent(chosen);
    }

    private void StartEvent(DolEventAsset evt)
    {
        runningEvent = evt;
        runningStage = 0;
        flagStorage.MarkConsumed(evt);
        ShowStage();
    }

    private void ShowStage()
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
                // 數值套用
                ApplyDelta(ref hp, ch.hpChange);
                ApplyDelta(ref money, ch.moneyChange);
                ApplyDelta(ref sanity, ch.sanityChange);
                ApplyDelta(ref hunger, ch.hungerChange);
                ApplyDelta(ref thirst, ch.thirstChange);
                ApplyDelta(ref fatigue, ch.fatigueChange);
                ApplyDelta(ref hope, ch.hopeChange);
                ApplyDelta(ref obedience, ch.obedienceChange);
                ApplyDelta(ref reputation, ch.reputationChange);
                ApplyDelta(ref techParts, ch.techPartsChange);
                ApplyDelta(ref information, ch.informationChange);
                ApplyDelta(ref credits, ch.creditsChange);
                ApplyDelta(ref augmentationLoad, ch.augmentationLoadChange);
                ApplyDelta(ref radiation, ch.radiationChange);
                ApplyDelta(ref trust, ch.trustChange);
                ApplyDelta(ref control, ch.controlChange);

                foreach (var f in ch.setFlagsTrue)  if (!string.IsNullOrEmpty(f)) flagStorage.SetFlag(f);
                foreach (var f in ch.setFlagsFalse) if (!string.IsNullOrEmpty(f)) flagStorage.ClearFlag(f);

                UpdateAllStatUI();

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

    private void EndEvent()
    {
        runningEvent = null; runningStage = -1;
    }

    // ====== UI 生成功能 ======
    private void SpawnChoice(string text, System.Action action)
    {
        if (!choiceButtonPrefab || !choiceContainer) return;
        var btn = Instantiate(choiceButtonPrefab, choiceContainer);
        var label = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (label) label.text = text;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => action());
        choiceContainer.gameObject.SetActive(true);
    }

    private void ClearChoices()
    {
        if (!choiceContainer) return;
        foreach (Transform child in choiceContainer) Destroy(child.gameObject);
        choiceContainer.gameObject.SetActive(false);
    }

    // ====== 工具 ======
    private void ApplyDelta(ref int stat, int delta)
    {
        stat += delta;
        stat = Mathf.Clamp(stat, -999999, 999999); // 防護：避免溢出，可自行調整
    }

    private CaseId ResolveStartCase()
    {
        if (startCase != CaseId.None && HasAvailableInCase(startCase)) return startCase;
        if (caseDB != null && caseDB.cases != null)
        {
            foreach (var c in caseDB.cases)
            {
                if (c == null || c.caseId == CaseId.None) continue;
                if (HasAvailableInCase(c.caseId)) return c.caseId;
            }
        }
        return CaseId.None;
    }

    private bool HasAvailableInCase(CaseId id)
    {
        if (!caseDB || !caseDB.TryGetPool(id, out var pool) || pool == null) return false;
        foreach (var entry in pool)
        {
            var e = entry.evt;
            if (!e) continue;
            if (!e.ConditionsMet(hp, flagStorage.HasFlag)) continue;
            if (e.oncePerSave && flagStorage.IsConsumed(e)) continue;
            if (flagStorage.IsOnCooldown(e, e.cooldownSeconds)) continue;
            float w = (entry.weightOverride >= 0f) ? entry.weightOverride : Mathf.Max(0f, e.weight);
            if (w > 0f) return true;
        }
        return false;
    }

    private void Tip(string msg)
    {
        if (storyText) storyText.text = msg;
        if (systemTipText) systemTipText.text = msg;
        Debug.LogWarning(msg);
    }

    // ====== UI 同步 ======
    public void UpdateAllStatUI()
    {
        SetUI(hpText, $"HP {hp}");
        SetUI(moneyText, $"Money {money}");
        SetUI(sanityText, $"Sanity {sanity}");
        SetUI(hungerText, $"Hunger {hunger}");
        SetUI(thirstText, $"Thirst {thirst}");
        SetUI(fatigueText, $"Fatigue {fatigue}");
        SetUI(hopeText, $"Hope {hope}");
        SetUI(obedienceText, $"Obedience {obedience}");
        SetUI(reputationText, $"Reputation {reputation}");
        SetUI(techPartsText, $"TechParts {techParts}");
        SetUI(informationText, $"Information {information}");
        SetUI(creditsText, $"Credits {credits}");
        SetUI(augmentationLoadText, $"AugLoad {augmentationLoad}");
        SetUI(radiationText, $"Radiation {radiation}");
        SetUI(trustText, $"Trust {trust}");
        SetUI(controlText, $"Control {control}");
    }

    private void SetUI(TextMeshProUGUI t, string v){ if (t) t.text = v; }
}
