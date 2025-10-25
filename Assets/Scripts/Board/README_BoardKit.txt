BoardKit v0.1 — 快速把「大富翁式棋盤」接到你現有的事件/戰鬥

放置：
1) 將 Scripts/Board/* 丟進 Assets/Scripts/Board/ 。
2) 在場景新增空物件 BoardRoot：
   - 加 BoardController、TurnManager、DiceRoller。
   - 把玩家棋子 (PlayerState) 拖到 BoardController.players 與 TurnManager.turnOrder。
   - 建立 TileDefinition 資料（Create → CyberLife → Board → TileDefinition），依序放進 BoardController.tiles。
   - 若你要令棋子跟著格子移動，設置 tileAnchors[] 對應每個格子的位置 Transform。
3) 在 BoardController 的 onRequestEvent/onRequestCombat 綁定 BoardEventsBridge 對應方法，先用 Log 驗證路徑。
4) 按下 TurnManager.StartGame() 測試回合/擲骰/移動/落地處理。

你可以後續：
- Property：把自動購買改成 UI 彈窗。
- Event/Combat：在 BoardEventsBridge 內直接呼叫你的 Game/Event/Combat API。
- Jail/Tax/Shop：TileDefinition 已留欄位，照需求延伸。