// GameManager.cs（以程式撰寫劇情版）
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public enum Difficulty { Easy = 0, Normal = 1, Hard = 2, Master = 3 }

    [Header("UI 參考")]
    public TextMeshProUGUI storyText;
    public Transform choiceContainer;
    public Button choiceButtonPrefab;
    public Slider healthBar;
    public TextMeshProUGUI statusText;   // 只顯示 HP

    [Header("遊戲參數")]
    public int startNode = 0;
    public int defaultHP = 100;

    [Header("目前難度（內部使用，不顯示）")]
    public Difficulty difficulty = Difficulty.Normal;

    private int currentNode;
    private int hp;
    private const string Death = "你死了。\n\n結束";

    void Awake()
    {
        if (choiceContainer) choiceContainer.gameObject.SetActive(false);
    }

    // ===== 難度設定（UI 按鈕呼叫）=====
    public void SetDifficultyByIndex(int index)
    {
        difficulty = (Difficulty)Mathf.Clamp(index, 0, 3);
    }

    // ===== 入口 =====
    public void BeginNewGame()
    {
        hp = defaultHP;
        currentNode = startNode;
        UpdateStatus();
        DisplayNode(currentNode);
    }

    public void BeginLoadGame()
    {
        if (HasSave()) LoadGame();
        else BeginNewGame();
    }

    // ===== 單存位（PlayerPrefs）=====
    private bool HasSave()
    {
        return PlayerPrefs.HasKey("Save_Node") && PlayerPrefs.HasKey("Save_HP");
    }

    private void SaveGame()
    {
        PlayerPrefs.SetInt("Save_Node", currentNode);
        PlayerPrefs.SetInt("Save_HP", hp);
        PlayerPrefs.SetInt("Save_Difficulty", (int)difficulty);
        PlayerPrefs.Save();
    }

    private void LoadGame()
    {
        currentNode = PlayerPrefs.GetInt("Save_Node", startNode);
        hp = PlayerPrefs.GetInt("Save_HP", defaultHP);
        difficulty = (Difficulty)PlayerPrefs.GetInt("Save_Difficulty", (int)Difficulty.Normal);
        UpdateStatus();
        DisplayNode(currentNode);
    }

    // ===== 多槽 API（Save/Load 菜單使用）=====
    public void SaveToSlot(int slot)
    {
        var data = new SaveData
        {
            node = currentNode,
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

        currentNode = data.node;
        hp = data.hp;
        difficulty = (Difficulty)Mathf.Clamp(data.difficulty, 0, 3);
        UpdateStatus();
        DisplayNode(currentNode);
    }

    // ===== 顯示與選項 =====
    private void ClearChoices()
    {
        if (!choiceContainer) return;
        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);
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

    private void DisplayNode(int index)
    {
        ClearChoices();

        switch (index)
        {
            // ===== 範例劇情開始 =====
            case 0:
                storyText.text = "你好。你站在森林入口。";
                SpawnChoice("往左走", () => GoTo(1));
                SpawnChoice("往右走", () => GoTo(2));
                break;

            case 1:
                if (hp >= 80)
                {
                    storyText.text = "你精神滿滿，嚇跑了野狼（不扣血）。";
                }
                else
                {
                    storyText.text = "你體力不足被野狼咬傷HP -10。";
                    hp -= 10; UpdateStatus();
                }
                SpawnChoice("繼續前進", () => GoTo(3));
                break;
            case 2:
                storyText.text = "你踩到陷阱HP -20。";
                hp -= 20; UpdateStatus();
                // 依 HP 分支
                if (hp >= 50)
                    SpawnChoice("強忍疼痛走出去", () => GoTo(3));
                else
                    SpawnChoice("爬到樹下休息", () => GoTo(4));
                break;

            case 3:
                storyText.text = "你走出了森林。\n\n結束";
                End(); // 無按鈕 = 結局
                break;

            case 4:
                storyText.text = "你暈倒了……\n\n結束";
                
                End();
                break;
            // ===== 範例劇情結束 =====

            default:
                storyText.text = Death;
                End();
                break;
        }

        SaveGame();
    }

    private void GoTo(int node)
    {
        currentNode = node;
        DisplayNode(currentNode);
    }

    private void End()
    {
        // 不產生選項即為結局；可視需求加上「回主選單」：
        // SpawnChoice("回主選單", () => { if (MenuManager.Instance) MenuManager.Instance.BackToMain(); });
    }

    // ===== UI 更新（只顯示 HP）=====
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
}
