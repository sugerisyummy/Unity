using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private CaseVisuals caseVisuals;        // 指向 ScriptableObject
    [SerializeField] private Image backgroundImage;          // Story 背景 Image

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private TextMeshProUGUI systemTipText;

    [Header("起始設定")]
    [SerializeField] private CaseId startCase = CaseId.None;
    [SerializeField] private int defaultHP = 100;

    private CaseId currentCase = CaseId.None;
    private int hp;
    private Difficulty difficulty = Difficulty.Normal;

    private DolEventAsset runningEvent;
    private int runningStage = -1;

    void Awake()
    {
        if (flagStorage == null) flagStorage = new EventFlagStorage();
        if (!caseDB) Tip("請在 GameManager 指定 CaseDatabase。");
    }

    // ★ 新增：提供 UI 綁定的難度設定入口
    public void SetDifficulty(int idx)
    {
        idx = Mathf.Clamp(idx, 0, 3);
        difficulty = (Difficulty)idx;
        Debug.Log($"[GameManager] Difficulty set to {difficulty} ({idx})");
    }

    public void BeginNewGame()
    {
        hp = defaultHP;
        var resolved = ResolveStartCase();
        if (resolved == CaseId.None) { Tip("目前沒有任何可觸發的事件。請在 CaseDatabase 加入事件。"); return; }
        EnterCase(resolved);
    }

    public void BeginLoadGame()
    {
        var d = SaveManager.Load(SaveManager.AUTO_SLOT);
        if (d == null) { BeginNewGame(); return; }
        hp = d.hp;
        difficulty = (Difficulty)Mathf.Clamp(d.difficulty, 0, 3);
        if (!System.Enum.TryParse<CaseId>(d.currentCase, out currentCase)) currentCase = CaseId.None;

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
        hp = d.hp;
        difficulty = (Difficulty)Mathf.Clamp(d.difficulty, 0, 3);
        if (!System.Enum.TryParse<CaseId>(d.currentCase, out currentCase)) currentCase = CaseId.None;

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

        // 套用該地點的背景與 BGM
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
                if (ch.hpChange != 0) hp += ch.hpChange;
                foreach (var f in ch.setFlagsTrue)  if (!string.IsNullOrEmpty(f)) flagStorage.SetFlag(f);
                foreach (var f in ch.setFlagsFalse) if (!string.IsNullOrEmpty(f)) flagStorage.ClearFlag(f);

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
