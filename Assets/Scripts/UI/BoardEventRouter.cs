using UnityEngine;
using System;
using System.Reflection;

namespace Game.UI
{
    /// <summary>
    /// 單場景面板路由：棋盤/事件/戰鬥 切換 + 戰鬥結果回路。
    /// 不再硬依賴 MenuManager；若專案有，就用反射呼叫 ReturnToBoard()。
    /// </summary>
    [DisallowMultipleComponent]
    public class BoardEventRouter : MonoBehaviour
    {
        [Header("Panels (可留空，會自動尋找)")]
        public GameObject boardPanel;   // 建議指到 Canvas/BoardPanel
        public GameObject eventPanel;   // 建議指到 Canvas/StoryPanel
        public GameObject combatPanel;  // 建議指到 Canvas/CombatPanel

        public void OpenBoard()  => ShowBoard();
        public void OpenEvent()  => ShowEvent();
        public void OpenCombat() => ShowCombat();

        void Awake()
        {
            if (!boardPanel)  boardPanel  = GameObject.Find("Canvas/BoardPanel")  ?? GameObject.Find("BoardPanel");
            if (!eventPanel)  eventPanel  = GameObject.Find("Canvas/StoryPanel")  ?? GameObject.Find("StoryPanel");
            if (!combatPanel) combatPanel = GameObject.Find("Canvas/CombatPanel") ?? GameObject.Find("CombatPanel");
        }

        public void ShowBoard()
        {
            if (!TryCallMenuManagerReturn())
            {
                SetActive(boardPanel,  true);
                SetActive(eventPanel,  false);
                SetActive(combatPanel, false);
            }
        }

        public void ShowEvent()
        {
            SetActive(eventPanel,  true);
            SetActive(boardPanel,  false);
            SetActive(combatPanel, false);
        }

        public void ShowCombat()
        {
            SetActive(combatPanel, true);
            SetActive(boardPanel,  false);
            SetActive(eventPanel,  false);
        }

        // 兼容舊呼叫名
        public void ReturnToBoard() => ShowBoard();
        public void BackToEvent()   => ShowEvent();

        // 戰鬥結果回路
        public void OnCombatWin()    => ShowEvent();
        public void OnCombatLose()   => ShowEvent();
        public void OnCombatEscape() => ShowEvent();

        static void SetActive(GameObject go, bool v) { if (go) go.SetActive(v); }

        bool TryCallMenuManagerReturn()
        {
            var menuType = Type.GetType("MenuManager");
            if (menuType == null) return false;
            var mm = FindObjectOfType(menuType) as MonoBehaviour;
            if (!mm) return false;

            var mi = menuType.GetMethod("ReturnToBoard",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mi == null) return false;

            mi.Invoke(mm, null);
            return true;
        }
    }
}
