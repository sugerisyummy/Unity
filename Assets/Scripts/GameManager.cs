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

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private TextMeshProUGUI systemTipText;

    [Header("起始設定")]
    [SerializeField] private CaseId startCase = CaseId.None;
    [SerializeField] private int defaultHP = 100;

    // ====== 狀態 ======
    private CaseId currentCase = CaseId.None;

    // 基礎
    private int hp;

    // 反烏托邦擴充數值
    private int hunger, thirst, fatigue;
    private int hope, obedience, reputation;
    private int techParts, information, credits;
    private int augmentationLoad, radiation;

    private Difficulty difficulty = Difficulty.Normal;

    // 事件執行
    private DolEventAsset runningEvent;
    private int runningStage = -1;

    void Awake()
    {
        if (flagStorage == null) flagStorage = new EventFlagStorage();
        if (!caseDB) Tip("請在 GameManager 指定 CaseDatabase。");
    }

    public void BeginNewGame()
    {
        // 初始化
        hp = defaultHP;

        hunger = 50; thirst = 50; fatigue = 0;      // 0~100 建議
        hope = 50; obedience = 50; reputation = 0;  // 0~100 建議
        techParts = 0; information = 0; credits = 0;
        augmentationLoad = 0; radiation = 0;

        var resolved = ResolveStartCase();
        if (resolved == CaseId.None) { Tip("目前沒有任何可觸發的事件。請在 CaseDatabase 加入事件。"); return; }
        EnterCase(resolved);
    }

    public void BeginLoadGame()
    {
        var d = SaveManager.Load(SaveManager.AUTO_SLOT);
        if (d == null) { BeginNewGame(); return; }

        currentCase = CaseId.None;
        if (!string.IsNullOrEmpty(d.currentCase)) System.Enum.TryParse(d.currentCase, out currentCase);

        hp = d.hp;

        hunger = d.hunger; thirst = d.thirst; fatigue = d.fatigue;
        hope = d.hope; obedience = d.obedience; reputation = d.reputation;
        techParts = d.techParts; information = d.information; credits = d.credits;
        augmentationLoad = d.augmentationLoad; radiation = d.radiation;

        difficulty = (Difficulty)Mathf.Clamp(d.difficulty, 0, 3);

        if (currentCase == CaseId.None || !HasAvailableInCase(currentCase))
        {
            var resolved = ResolveStartCase();
            if (resolved == CaseId.None) { Tip("載入後沒有可觸發事件。"); return; }
            currentCase = resolved;
        }
        EnterCase(currentCase);
    }

    public void SaveToSlot(int slot)
    {
        var data = new SaveData
        {
            currentCase = currentCase.ToString(),
            hp = hp,

            hunger = hunger, thirst = thirst, fatigue = fatigue,
            hope = hope, obedience = obedience, reputation = reputation,
            techParts = techParts, information = information, credits = credits,
            augmentationLoad = augmentationLoad, radiation = radiation,

            difficulty = (int)difficulty,
            saveTime = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm")
        };
        SaveManager.Save(slot, data);
        SaveManager.Save(SaveManager.AUTO_SLOT, data);
        Tip($"已存到槽 {slot}");
    }

    public void LoadFromSlot(int slot)
    {
        var d = SaveManager.Load(slot);
        if (d == null) { Tip($"槽 {slot} 為空。"); return; }

        currentCase = CaseId.None;
        if (!string.IsNullOrEmpty(d.currentCase)) System.Enum.TryParse(d.currentCase, out currentCase);

        hp = d.hp;

        hunger = d.hunger; thirst = d.thirst; fatigue = d.fatigue;
        hope = d.hope; obedience = d.obedience; reputation = d.reputation;
        techParts = d.techParts; information = d.information; credits = d.credits;
        augmentationLoad = d.augmentationLoad; radiation = d.radiation;

        difficulty = (Difficulty)Mathf.Clamp(d.difficulty, 0, 3);

        if (currentCase == CaseId.None || !HasAvailableInCase(currentCase))
        {
            var resolved = ResolveStartCase();
            if (resolved == CaseId.None) { Tip("載入後沒有可觸發事件。"); return; }
            currentCase = resolved;
        }
        EnterCase(currentCase);
    }

    public void EnterCase(CaseId id)
    {
        currentCase = id;
        runningEvent = null;
        runningStage = -1;

        // 背景 / BGM
        if (caseVisuals && caseVisuals.TryGet(currentCase, out var entry))
        {
            if (backgroundImage) backgroundImage.sprite = entry.background;
            if (AudioManager.Instance) AudioManager.Instance.PlayBGM(entry.bgm, 1f);
        }

        RollAndStartEvent();
    }

    private void RollAndStartEvent()
    {
        ClearChoices();
        if (!caseDB || !caseDB.TryGetPool(currentCase, out var pool) || pool == null || pool.Count == 0)
        {
            Tip($"地點 {currentCase} 沒有任何事件。");
            return;
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
        if (candidates.Count == 0)
        {
            Tip($"地點 {currentCase} 目前沒有可觸發事件（可能都在冷卻或條件不符）。");
            return;
        }
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
        {
            Tip("事件階段錯誤。"); return;
        }
        var stage = runningEvent.stages[runningStage];
        if (storyText) storyText.text = stage.text;

        if (stage.choices == null || stage.choices.Count == 0) { EndEvent(); return; }

        foreach (var ch in stage.choices)
        {
            SpawnChoice(ch.text, () =>
            {
                // 1) 依難度取得縮放後變化量
                ch.GetScaledDelta((int)difficulty, out var d);

                // 2) 套用（含界線保護；需的話自行調整上限/下限）
                hp = Mathf.Clamp(hp + d.hp, 0, 999999);

                hunger = Mathf.Clamp(hunger + d.hunger, 0, 100);
                thirst = Mathf.Clamp(thirst + d.thirst, 0, 100);
                fatigue = Mathf.Clamp(fatigue + d.fatigue, 0, 100);

                hope = Mathf.Clamp(hope + d.hope, 0, 100);
                obedience = Mathf.Clamp(obedience + d.obedience, 0, 100);
                reputation = Mathf.Clamp(reputation + d.reputation, -100, 100);

                techParts = Mathf.Clamp(techParts + d.techParts, -999999, 999999);
                information = Mathf.Clamp(information + d.information, -999999, 999999);
                credits = Mathf.Clamp(credits + d.credits, -999999, 999999);

                augmentationLoad = Mathf.Clamp(augmentationLoad + d.augmentationLoad, 0, 100);
                radiation = Mathf.Clamp(radiation + d.radiation, 0, 100);

                // 3) 旗標
                foreach (var f in ch.setFlagsTrue)  if (!string.IsNullOrEmpty(f)) flagStorage.SetFlag(f);
                foreach (var f in ch.setFlagsFalse) if (!string.IsNullOrEmpty(f)) flagStorage.ClearFlag(f);

                // 4) 跳轉/結束
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
        runningEvent = null;
        runningStage = -1;
    }

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

    private CaseId ResolveStartCase()
    {
        if (startCase != CaseId.None && HasAvailableInCase(startCase)) return startCase;
        if (caseDB && caseDB.cases != null)
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
}
