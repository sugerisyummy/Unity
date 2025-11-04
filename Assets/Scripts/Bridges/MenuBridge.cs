using UnityEngine;
using Game.Board;
using UnityEngine.Scripting.APIUpdating;

namespace Game.UI
{
    [MovedFrom(true, sourceNamespace: "CyberLife.UI")]
    public class MenuBridge : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainMenu;        // Canvas/MainMenu
        public GameObject difficultyPanel; // Canvas/DifficultyPanel
        public GameObject boardPanel;      // Canvas/BoardPanel
        public GameObject optionsPanel;    // Canvas/Options   (可空)
        public GameObject loadPanel;       // Canvas/LoadMenu  (可空)

        [Header("Board")]
        public BoardController board;

        void Awake()
        {
            // 預設打開主選單 (若場景已在棋盤就不動)
            if (mainMenu && boardPanel && !boardPanel.activeSelf) ShowMain();
        }

        // 主選單
        public void ShowMain()        => SetOnly(mainMenu);
        public void ShowOptions()     => SetOnly(optionsPanel);
        public void ShowLoad()        => SetOnly(loadPanel);
        public void QuitGame()        { Application.Quit(); Debug.Log("[Menu] Quit"); }

        // 流程：開始→難度→進棋盤
        public void ShowDifficulty()  => SetOnly(difficultyPanel);
        public void PickDifficulty(int level)
        {
            // TODO: 這裡可以依難度初始化 PlayerState
            EnterBoard();
        }

        public void EnterBoard()
        {
            SetOnly(boardPanel);
            if (board) board.Generate();
        }

        public void ReturnToBoard() => SetOnly(boardPanel);

        void SetOnly(GameObject target)
        {
            if (mainMenu)        mainMenu.SetActive(mainMenu == target);
            if (difficultyPanel) difficultyPanel.SetActive(difficultyPanel == target);
            if (optionsPanel)    optionsPanel.SetActive(optionsPanel == target);
            if (loadPanel)       loadPanel.SetActive(loadPanel == target);
            if (boardPanel)      boardPanel.SetActive(boardPanel == target);
        }
    }
}
