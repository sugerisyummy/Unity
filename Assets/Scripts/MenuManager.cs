// MenuManager.cs — 穩定做法（先 DifficultyPanel 後 StoryPanel）
using UnityEngine;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("Panels")]
    public GameObject mainPanel;     // 進場顯示（請指向 DifficultyPanel）
    public GameObject storyPanel;    // 故事頁
    public GameObject loadPanel;     // 讀檔頁（可留空）
    public GameObject optionsPanel;  // 設定頁（可留空）
    public List<GameObject> extraPanels = new(); // 其他自訂面板（可留空）

    [Header("Behavior")]
    public bool enableEscBack = true;

    private readonly Stack<GameObject> history = new();
    private GameObject current;

    void Awake()
    {
        if (Instance == null) Instance = this; else { Destroy(gameObject); return; }
        ShowOnly(mainPanel);          // 進場只開 DifficultyPanel
        current = mainPanel;
    }

    void Update()
    {
        if (!enableEscBack) return;
        if (Input.GetKeyDown(KeyCode.Escape)) Back(); // ESC 一律返回
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
        if (history.Count == 0) { BackToMain(); return; }
        if (current) current.SetActive(false);
        var prev = history.Pop();
        if (prev) prev.SetActive(true);
        current = prev;
    }

    public void BackToMain()
    {
        if (current) current.SetActive(false);
        while (history.Count > 0)
        {
            var p = history.Pop();
            if (p) p.SetActive(false);
        }
        ShowOnly(mainPanel);
        current = mainPanel;
    }

    // 清空堆疊並切到指定面板（開始/讀檔用）
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
        foreach (var p in CollectAllPanels())
            if (p) p.SetActive(false);
        if (target) target.SetActive(true);
    }

    private IEnumerable<GameObject> CollectAllPanels()
    {
        if (mainPanel) yield return mainPanel;
        if (storyPanel) yield return storyPanel;
        if (loadPanel) yield return loadPanel;
        if (optionsPanel) yield return optionsPanel;
        if (extraPanels != null)
            foreach (var p in extraPanels) if (p) yield return p;
    }

    // 開始/讀檔：清堆疊後進入故事頁
    public void StartNewGame(GameManager gm)
    {
        if (gm) gm.BeginNewGame();
        ResetToPanel(storyPanel);
    }

    public void StartLoadGame(GameManager gm)
    {
        if (gm) gm.BeginLoadGame();
        ResetToPanel(storyPanel);
    }
}
