// GameManager.cs  （改成資料驅動抽事件 + 多頁執行）
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public enum Difficulty { Easy = 0, Normal = 1, Hard = 2, Master = 3 }

    [Header("UI 參考")]
    public TextMeshProUGUI storyText;
    public Transform choiceContainer;
    public Button choiceButtonPrefab;
    public Slider healthBar;
    public TextMeshProUGUI statusText;

    [Header("資料庫")]
    public CaseDatabase caseDB;

    [Header("起始設定")]
    public CaseId startCase = CaseId.ForestEntrance;
    public int defaultHP = 100;

    [Header("內部狀態")]
    public Difficulty difficulty = Difficulty.Normal;

    private int hp;
    private CaseId currentCase;

    // 事件執行狀態
    private DolEventAsset runningEvent;
    private int runningStage = -1;

    void Awake()
    {
        if (choiceContainer) choiceContainer.gameObject.SetActive(false);
    }

    public void SetDifficultyByIndex(int index)
    {
        difficulty = (Difficulty)Mathf.Clamp(index, 0, 3);
    }

    // ===== 入口 =====
    public void BeginNewGame()
    {
        hp = defaultHP;
        EnterCase(startCase);
    }

    public void BeginLoadGame()
    {
        if (HasSave()) LoadGame();
        else BeginNewGame();
    }

    // ===== 存讀檔 =====
    private bool HasSave()
    {
        return PlayerPrefs.HasKey("Save_CurrentCase") && PlayerPrefs.HasKey("Save_HP");
    }

    private void SaveGame()
    {
        PlayerPrefs.SetString("Save_CurrentCase", currentCase.ToString());
        PlayerPrefs.SetInt("Save_HP", hp);
        PlayerPrefs.SetInt("Save_Difficulty", (int)difficulty);
        PlayerPrefs.Save();
    }

    private void LoadGame()
    {
        hp = PlayerPrefs.GetInt("Save_HP", defaultHP);
        difficulty = (Difficulty)PlayerPrefs.GetInt("Save_Difficulty", (int)Difficulty.Normal);

        string c = PlayerPrefs.GetString("Save_CurrentCase", startCase.ToString());
        if (!Enum.TryParse<CaseId>(c, out currentCase)) currentCase = startCase;

        UpdateStatus();
        EnterCase(currentCase); // 重新 roll
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
    }

    public void LoadFromSlot(int slot)
    {
        var data = SaveManager.Load(slot);
        if (data == null) return;

        hp = data.hp;
        difficulty = (Difficulty)Mathf.Clamp(data.difficulty, 0, 3);
        if (!Enum.TryParse<CaseId>(data.currentCase, out currentCase))
            currentCase = startCase;

        UpdateStatus();
        EnterCase(currentCase);
    }

    // ===== UI =====
    private void UpdateStatus()
    {
        if (healthBar)
        {
            healthBar.value = hp;
            var hpText = healthBar.GetComponentInChildren<TextMeshProUGUI>();
            if (hpText) hpText.text = $"HP: {hp}";
        }
        if (statusText) statusText.text = $"HP: {hp}";
    }

    private void ClearChoices()
    {
        if (!choiceContainer) return;
        foreach (Transform child in choiceContainer) Destroy(child.gameObject);
        choiceContainer.gameObject.SetActive(false);
    }

    private void SpawnChoice(string text, Action action)
    {
        if (!choiceButtonPrefab || !choiceContainer) return;
        Button btn = Instantiate(choiceButtonPrefab, choiceContainer);
        var label = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (label) label.text = text;
        btn.onClick.AddListener(() => action());
        choiceContainer.gameObject.SetActive(true);
    }

    // ===================== 核心流程 =====================
    public void EnterCase(CaseId id)
    {
        currentCase = id;
        runningEvent = null;
        runningStage = -1;
        UpdateStatus();
        RollAndStartEvent();  // 進入地點即抽事件
        SaveGame();
    }

    private void RollAndStartEvent()
    {
        ClearChoices();
        if (!caseDB || !caseDB.TryGetPool(currentCase, out var pool) || pool == null || pool.Count == 0)
        {
            storyText.text = "(此地沒有事件)";
            return;
        }

        // 過濾條件＋一次性＋冷卻
        List<(DolEventAsset e, float w)> candidates = new();
        foreach (var entry in pool)
        {
            var e = entry.evt;
            if (!e) continue;

            // 一次性檢查
            if (e.oncePerSave && EventFlagStorage.IsOnceConsumed(e.eventId)) continue;

            // 冷卻檢查
            if (!EventFlagStorage.IsOffCooldown(e.eventId, e.cooldownSeconds)) continue;

            // 條件檢查
            if (!e.ConditionsMet(hp, EventFlagStorage.GetFlag)) continue;

            float w = entry.weightOverride >= 0f ? entry.weightOverride : e.weight;
            if (w <= 0f) continue;

            candidates.Add((e, w));
        }

        if (candidates.Count == 0)
        {
            storyText.text = "(目前沒有可觸發的事件)";
            return;
        }

        // 加權抽取
        float total = 0f; foreach (var c in candidates) total += c.w;
        float r = UnityEngine.Random.value * total, acc = 0f;
        DolEventAsset chosen = candidates[0].e;
        foreach (var c in candidates)
        {
            acc += c.w;
            if (r <= acc) { chosen = c.e; break; }
        }

        StartEvent(chosen);
    }

    private void StartEvent(DolEventAsset evt)
    {
        runningEvent = evt;
        runningStage = 0;
        ShowStage();
        // 觸發時就標記冷卻起始與一次性
        EventFlagStorage.MarkFired(evt.eventId);
    }

    private void ShowStage()
    {
        ClearChoices();
        if (runningEvent == null || runningStage < 0 || runningStage >= runningEvent.stages.Count)
        {
            storyText.text = "(事件錯誤)";
            return;
        }

        var stage = runningEvent.stages[runningStage];
        storyText.text = stage.text;

        if (stage.choices == null || stage.choices.Count == 0)
        {
            // 無選項視為事件結束
            EndEvent();
            return;
        }

        foreach (var ch in stage.choices)
        {
            SpawnChoice(ch.text, () =>
            {
                // 套用 HP 與旗標
                if (ch.hpChange != 0) { hp += ch.hpChange; UpdateStatus(); }
                foreach (var f in ch.setFlagsTrue)  if (!string.IsNullOrEmpty(f)) EventFlagStorage.SetFlag(f, true);
                foreach (var f in ch.setFlagsFalse) if (!string.IsNullOrEmpty(f)) EventFlagStorage.SetFlag(f, false);

                // 流程跳轉
                if (ch.nextStage >= 0) { runningStage = ch.nextStage; ShowStage(); return; }

                if (ch.endEvent)
                {
                    var gotoNext = ch.gotoCaseAfterEnd;
                    var nextCase = ch.gotoCase;
                    EndEvent();
                    if (gotoNext) EnterCase(nextCase);
                    return;
                }

                // 若沒有任何跳轉，就停留本頁（避免無限）
                ShowStage();
            });
        }
    }

    private void EndEvent()
    {
        runningEvent = null;
        runningStage = -1;
        // 結束後可自動再抽下一個事件（若想要連續遭遇）
        // RollAndStartEvent();
    }
}
