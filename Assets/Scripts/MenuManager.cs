using UnityEngine;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("Panels")]
    public GameObject startMenuPanel;   // 進場顯示（有 Start 按鈕）
    public GameObject prologuePanel;    // 前導劇情
    public GameObject difficultyPanel;  // 選難度
    public GameObject storyPanel;       // 故事頁
    public GameObject loadPanel;        // 讀檔頁（可留空）
    public GameObject optionsPanel;     // 設定頁（可留空）
    public List<GameObject> extraPanels = new();

    [Header("Behavior")]
    public bool enableEscBack = true;

    private readonly Stack<GameObject> history = new();
    private GameObject current;

    void Awake()
    {
        if (Instance == null) Instance = this; else { Destroy(gameObject); return; }
        ShowOnly(startMenuPanel);
        current = startMenuPanel;
    }

    void Update()
    {
        if (!enableEscBack) return;
        if (Input.GetKeyDown(KeyCode.Escape)) Back();
    }

    public void ShowPrologue(GameManager gm)
    {
        // Start 按後呼叫：清堆疊 → 進前導
        ResetToPanel(prologuePanel);
    }

    public void GoToDifficulty()
    {
        ResetToPanel(difficultyPanel);
    }

    public void StartNewGame(GameManager gm)
    {
        // 從「難度面板」開始遊戲（難度選完後的按鈕可呼叫此函式）
        if (gm) gm.BeginNewGame();
        ResetToPanel(storyPanel);
    }

    public void StartLoadGame(GameManager gm)
    {
        if (gm) gm.BeginLoadGame();
        ResetToPanel(storyPanel);
    }

    public void ShowPanel(GameObject panel)
    {
        if (!panel || panel == current) return;
        if (current != null)
        {
            current.SetActive(false);
            history.Push(current);
        }
        panel.SetActive(true);
        current = panel;
    }

    public void Back()
    {
        if (history.Count == 0)
        {
            BackToStartMenu();
            return;
        }
        if (current) current.SetActive(false);
        var prev = history.Pop();
        if (prev) prev.SetActive(true);
        current = prev;
    }

    public void BackToStartMenu()
    {
        if (current) current.SetActive(false);
        while (history.Count > 0)
        {
            var p = history.Pop();
            if (p) p.SetActive(false);
        }
        ShowOnly(startMenuPanel);
        current = startMenuPanel;
    }

    private void ResetToPanel(GameObject target)
    {
        if (current) current.SetActive(false);
        while (history.Count > 0)
        {
            var p = history.Pop();
            if (p) p.SetActive(false);
        }
        ShowOnly(target);
        current = target;
    }

    private void ShowOnly(GameObject target)
    {
        foreach (var p in CollectAllPanels()) if (p) p.SetActive(false);
        if (target) target.SetActive(true);
    }

    private IEnumerable<GameObject> CollectAllPanels()
    {
        if (startMenuPanel) yield return startMenuPanel;
        if (prologuePanel) yield return prologuePanel;
        if (difficultyPanel) yield return difficultyPanel;
        if (storyPanel) yield return storyPanel;
        if (loadPanel) yield return loadPanel;
        if (optionsPanel) yield return optionsPanel;
        if (extraPanels != null) foreach (var p in extraPanels) if (p) yield return p;
    }
}
