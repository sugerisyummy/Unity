using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;                 // 指到 Canvas 下的 PauseMenu 面板
    public GameObject defaultSelected;       // 面板打開時預選的按鈕

    bool isOpen;

    void Awake()
    {
        if (panel) panel.SetActive(false);
        isOpen = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Toggle();
    }

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (!panel) return;
        panel.SetActive(true);
        isOpen = true;
        Time.timeScale = 0f;

        // 聚焦到第一個按鈕
        if (defaultSelected)
            EventSystem.current.SetSelectedGameObject(defaultSelected);
    }

    public void Close()
    {
        if (!panel) return;
        panel.SetActive(false);
        isOpen = false;
        Time.timeScale = 1f;
        EventSystem.current.SetSelectedGameObject(null);
    }

    // ===== 按鈕事件 =====
    public void OnResume() => Close();

    public void OnOpenSaveMenu()
    {
        Close();
        if (MenuManager.Instance) MenuManager.Instance.ShowPanel(MenuManager.Instance.loadPanel); // 你也可改成 SaveMenu
    }

    public void OnOpenLoadMenu()
    {
        Close();
        if (MenuManager.Instance) MenuManager.Instance.ShowPanel(MenuManager.Instance.loadPanel);
    }

    public void OnOpenOptions()
    {
        Close();
        if (MenuManager.Instance) MenuManager.Instance.ShowPanel(MenuManager.Instance.optionsPanel);
    }

    public void OnBackToMain()
    {
        Close();
        if (MenuManager.Instance) MenuManager.Instance.BackToMain();
    }
}
