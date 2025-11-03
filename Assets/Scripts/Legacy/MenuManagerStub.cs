using UnityEngine;

// 這是「改名後」的備援類，避免和你專案內的 MenuManager 類衝突。
// 若你有正式的 MenuManager，這支可以保留也可以刪除；
// BoardEventRouter（clean 版）不依賴它。

public class MenuManagerStub : MonoBehaviour
{
    [Tooltip("可直接指定棋盤面板；若留空則用路徑尋找 Canvas/BoardPanel")]
    public GameObject boardPanel;

    [ContextMenu("ReturnToBoard")]
    public void ReturnToBoard()
    {
        if (boardPanel != null)
        {
            boardPanel.SetActive(true);
            return;
        }
        var panel = GameObject.Find("Canvas/BoardPanel");
        if (panel != null) panel.SetActive(true);
    }
}
