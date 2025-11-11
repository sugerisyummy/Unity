# 棋盤系統架構說明

## 系統總覽
```mermaid
graph TD
    BoardPanel([Board Panel]) -->|管理| BoardController
    BoardController -->|擲骰移動| PawnController
    PawnController -->|落地事件| BoardEventsBridge
    BoardEventsBridge -->|顯示頁面| BoardEventRouter
    BoardEventRouter -->|啟用| BoardPanel
    BoardEventRouter -->|啟用| EventPanel
    BoardEventRouter -->|啟用| CombatPanel
    CombatSubsystem([Combat System]) -->|結果| BoardEventRouter
```

```mermaid
sequenceDiagram
    participant Player
    participant UI as Game.UI.BoardEventRouter
    participant Bridge as Game.Board.BoardEventsBridge
    participant Pawn as Game.Board.PawnController
    participant Turn as Game.Board.TurnManager

    Player->>UI: 點擊 / 按下 R 擲骰
    UI->>Turn: RollAndMove()
    Turn->>Pawn: MoveSteps(result)
    Pawn-->>Bridge: OnPawnLanded(index)
    Bridge->>UI: ShowEvent()/ShowCombat()
    UI->>Player: 切換面板
    CombatSubsystem-->>UI: OnCombatWin/Lose/Escape()
    UI->>Player: 返回事件面板
```

## 模組責任
| 模組 | 職責 | 關鍵檔案 |
| --- | --- | --- |
| Game.Board | 棋盤生成、棋子移動與事件判斷 | `Assets/Scripts/Board/BoardController.cs`、`Assets/Scripts/Board/PawnController.cs`、`Assets/Scripts/Board/BoardEventsBridge.cs`、`Assets/Scripts/Board/TurnManager.cs`、`Assets/Scripts/Board/PlayerState.cs` |
| Game.UI | 介面切換、擲骰顯示與按鈕綁定 | `Assets/Scripts/UI/BoardEventRouter.cs`、`Assets/Scripts/UI/RollButtonBinder.cs`、`Assets/Scripts/UI/DiceRollerUI.cs`、`Assets/Scripts/Bridges/MenuBridge.cs` |

## 差異與建議
- 移除舊版自動縮放腳本 (`BoardAutoFitPerimeter`、`BoardTripletAutoStretch`、`PawnAutoSizeToTile`) 以避免 RectTransform 競態。
- `PawnController` 單純依 Tiles 走格順序逐格推進，`DiceRollerUI` 提供擲骰展示與結果回呼。
- `RollButtonBinder` 維持熱鍵與按鈕共用邏輯，若無骰子 UI 也會 fallback 直接推棋。

## 驗證步驟
1. Unity 進入 Play，確認 Console 無編譯錯誤。
2. 啟用 BoardPanel 後按下 `R` 或點擊擲骰按鈕，骰子數字應連續變化約 1 秒並定格。
3. 擲骰完成時棋子沿 Tiles 順序逐格前進至結果格並觸發對應事件/戰鬥。
4. 調整 `DiceRollerUI` 的 `minValue`、`maxValue`，確認結果區間會跟著變化。
