using UnityEngine;
using System.Collections.Generic;
using Game.Board;   // ← 新增，拿 BoardController

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("Panels")]
    public GameObject startMenuPanel;
    public GameObject prologuePanel;
    public GameObject difficultyPanel;
    public GameObject storyPanel;
    public GameObject loadPanel;
    public GameObject optionsPanel;

    [Header("Board")]                 // ← 新增
    public GameObject boardPanel;     // Canvas/BoardPanel
    public BoardController board;     // BoardPanel 上的 BoardController

    [Header("Extra panels (optional)")]
    public List<GameObject> extraPanels = new(); // 戰鬥/事件/前導等，進棋盤時一併關掉

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

    // 主流程
    public void ShowPrologue(GameManager gm)   { ResetToPanel(prologuePanel); }
    public void GoToDifficulty()               { ResetToPanel(difficultyPanel); }

    // 難度 → 棋盤（這裡直接進棋盤）
    public void StartNewGame(GameManager gm)
    {
        if (gm) gm.BeginNewGame();
        EnterBoard();
    }
    public void StartLoadGame(GameManager gm)
    {
        if (gm) gm.BeginLoadGame();
        EnterBoard();
    }
    public void PickDifficulty(int level)      // 難度鈕可綁這個
    {
        // TODO: 依 level 設定起始資源
        EnterBoard();
    }

    public void EnterBoard()
    {
        // 關掉額外面板（如 CombatPanel/StoryPanel/Prologue 等）
        if (extraPanels != null) foreach (var p in extraPanels) if (p) p.SetActive(false);
        ResetToPanel(boardPanel);
        if (board) board.Generate();
    }
    public void ReturnToBoard() => EnterBoard(); // 戰鬥/事件結束回棋盤

    // 共用面板堆疊控制
    public void ShowPanel(GameObject panel)
    {
        if (!panel || panel == current) return;
        if (current != null) { current.SetActive(false); history.Push(current); }
        panel.SetActive(true); current = panel;
    }
    public void Back()
    {
        if (history.Count == 0) { BackToStartMenu(); return; }
        if (current) current.SetActive(false);
        var prev = history.Pop();
        if (prev) prev.SetActive(true);
        current = prev;
    }
    public void BackToStartMenu()
    {
        if (current) current.SetActive(false);
        while (history.Count > 0) { var p = history.Pop(); if (p) p.SetActive(false); }
        ShowOnly(startMenuPanel); current = startMenuPanel;
    }

    private void ResetToPanel(GameObject target)
    {
        if (current) current.SetActive(false);
        while (history.Count > 0) { var p = history.Pop(); if (p) p.SetActive(false); }
        ShowOnly(target); current = target;
    }
    private void ShowOnly(GameObject target)
    {
        foreach (var p in CollectAllPanels()) if (p) p.SetActive(false);
        if (target) target.SetActive(true);
    }
    private IEnumerable<GameObject> CollectAllPanels()
    {
        if (startMenuPanel) yield return startMenuPanel;
        if (prologuePanel)  yield return prologuePanel;
        if (difficultyPanel)yield return difficultyPanel;
        if (storyPanel)     yield return storyPanel;
        if (loadPanel)      yield return loadPanel;
        if (optionsPanel)   yield return optionsPanel;
        if (boardPanel)     yield return boardPanel;   // ← 新增，納入統一顯示管理
        if (extraPanels != null) foreach (var p in extraPanels) if (p) yield return p;
    }
}
