using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum Difficulty { Easy = 0, Normal = 1, Hard = 2, Master = 3 }

    [Header("UI 參考")]
    public TextMeshProUGUI storyText;
    public Transform choiceContainer;
    public Button choiceButtonPrefab;
    public Slider healthBar;
    public TextMeshProUGUI statusText;

    [Header("故事資料")]
    public StoryNode[] storyNodes;

    [Header("遊戲參數")]
    public int startNode = 0;
    public int defaultHP = 100;

    [Header("目前難度")]
    public Difficulty difficulty = Difficulty.Normal; // 無 Slider；由按鈕或程式設定

    private int currentNode;
    private int hp;

    void Awake()
    {
        if (choiceContainer) choiceContainer.gameObject.SetActive(false);
    }

    // === 供 UI 按鈕使用：0..3 對應 Easy..Master ===
    public void SetDifficultyByIndex(int index)
    {
        index = Mathf.Clamp(index, 0, 3);
        difficulty = (Difficulty)index;
        // 若要即時顯示在某處，可在這裡更新 UI
        // 例如：statusText.text = $"難度：{GetDifficultyName()}";
    }
    public string GetDifficultyName()
    {
        switch (difficulty)
        {
            case Difficulty.Easy:   return "Easy";
            case Difficulty.Normal: return "Normal";
            case Difficulty.Hard:   return "Hard";
            case Difficulty.Master: return "Master";
            default: return "Normal";
        }
    }

    // ── 單場景入口 ──
    public void BeginNewGame()
    {
        hp = defaultHP;                 // 難度影響事件機率；不限制 HP
        currentNode = startNode;
        UpdateStatus();
        DisplayNode(currentNode);
    }

    public void BeginLoadGame()
    {
        if (HasSave()) LoadGame();
        else BeginNewGame();
    }

    // ── 單存位（沿用） ──
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

    // ── 多槽 API（Save/Load 菜單用） ──
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

    private void ClearChoices()
    {
        if (!choiceContainer) return;
        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);
        choiceContainer.gameObject.SetActive(false);
    }

    private void DisplayNode(int index)
    {
        if (storyNodes == null || storyNodes.Length == 0)
        {
            if (storyText) storyText.text = "（沒有故事資料）";
            ClearChoices();
            return;
        }
        if (index < 0 || index >= storyNodes.Length)
        {
            if (storyText) storyText.text = "The End";
            ClearChoices();
            SpawnBackToMenuButton();
            return;
        }

        ClearChoices();
        if (storyText) storyText.text = storyNodes[index].text;

        var choices = storyNodes[index].choices;
        if (choices == null || choices.Length == 0)
        {
            SpawnBackToMenuButton();
            return;
        }

        choiceContainer.gameObject.SetActive(true);
        foreach (var choice in choices)
        {
            var c = choice; // 避免閉包
            var btn = Instantiate(choiceButtonPrefab, choiceContainer);
            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label) label.text = c.choiceText;
            btn.onClick.AddListener(() => ChooseOption(c));
        }
    }

    private void ChooseOption(Choice choice)
    {
        hp += choice.hpChange;
        currentNode = choice.nextNode;
        UpdateStatus();
        SaveGame(); // 如不想自動存可註解
        DisplayNode(currentNode);
    }

    private void SpawnBackToMenuButton()
    {
        if (!choiceContainer) return;
        choiceContainer.gameObject.SetActive(true);

        var btn = Instantiate(choiceButtonPrefab, choiceContainer);
        var label = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (label) label.text = "返回主選單";
        btn.onClick.AddListener(() =>
        {
            if (MenuManager.Instance) MenuManager.Instance.BackToMain();
        });
    }

    private void UpdateStatus()
    {
        if (healthBar)
        {
            healthBar.value = hp;
            var hpText = healthBar.GetComponentInChildren<TextMeshProUGUI>();
            if (hpText) hpText.text = $"Hp: {hp}";
        }
        if (statusText)
        {
            statusText.text = $"HP: {hp}　難度: {GetDifficultyName()}";
        }
    }
}
