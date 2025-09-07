using UnityEngine;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("Panels")]
    public GameObject mainPanel;     // 主選單
    public GameObject storyPanel;    // 遊戲/故事頁
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
        // 進入場景只開主選單
        ShowOnly(mainPanel);
        current = mainPanel;
    }

    void Update()
    {
        if (enableEscBack && Input.GetKeyDown(KeyCode.Escape))
            Back();
    }

    // 對外：顯示指定面板並記錄返回堆疊
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

    // 對外：返回上一頁（若無上一頁則回主選單）
    public void Back()
    {
        if (history.Count == 0) { BackToMain(); return; }

        if (current) current.SetActive(false);
        var prev = history.Pop();
        prev.SetActive(true);
        current = prev;
    }

    // 對外：回主選單並清空歷史
    public void BackToMain()
    {
        // 關掉目前
        if (current) current.SetActive(false);

        // 關掉歷史裡仍開著的面板（保險）
        while (history.Count > 0)
        {
            var p = history.Pop();
            if (p) p.SetActive(false);
        }

        // 只開主選單
        ShowOnly(mainPanel);
        current = mainPanel;
    }

    // 只顯示 target，其他全部關閉
    private void ShowOnly(GameObject target)
    {
        var all = CollectAllPanels();
        foreach (var p in all)
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

    // 便利方法：開始新遊戲 / 讀檔後切到故事頁
    public void StartNewGame(GameManager gm)
    {
        if (gm) gm.BeginNewGame();
        ShowPanel(storyPanel);
    }

    public void StartLoadGame(GameManager gm)
    {
        if (gm) gm.BeginLoadGame();
        ShowPanel(storyPanel);
    }
}
