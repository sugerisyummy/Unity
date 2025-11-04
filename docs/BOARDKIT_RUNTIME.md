# BoardKit Runtime 快速手冊

## 安裝步驟
1. 將 `Assets/Scripts/Board`、`Assets/Scripts/UI` 目錄複製到專案，並一併匯入對應的 `.asmdef`（若已有專案 asmdef，請新增對 `KG.Board` 的參考）。
2. 在場景的 `Canvas/BoardPanel/Tiles` 物件上掛上 `Game.Board.BoardAutoFitPerimeter`，`Pawns` 掛 `Game.Board.PawnAutoSizeToTile`（並指向 Tiles）。
3. 將控制流程腳本掛好：
   - `Game.Board.TurnManager` 指向 `PawnController` 與 `DiceRoller`。
   - `Game.Board.BoardEventsBridge` 指向 `BoardController` 與 `PawnController`，並設定需要切換的 UI 物件。
   - `Game.UI.BoardEventRouter` 放在共用 Canvas 上處理 Board/Event/Combat 面板切換。
4. UI 的 Roll 按鈕掛上 `Game.UI.RollButtonBinder`（或直接透過 EventSystem 綁定 `TurnManager.RollAndMove()`）。
5. 在 `BoardPanel` 子物件 `Tiles/Pawns/UI` 的 Anchor 設為 Stretch、Offset 設為 0，確保自動縮放能正確運作。
6. 進入 Play 前按一次 `BoardController.Generate()` 產生棋盤，確認 Pawn 已指到 Tiles，並執行 `PawnController.SnapToCurrentIndex(true)` 讓棋子落在正確格子上。

## 元件對應表
| 功能 | 掛在哪裡 | 腳本 | 備註 |
| --- | --- | --- | --- |
| 棋盤格生成 | 任意空物件（通常為 `BoardPanel`） | `CyberLife.Board.BoardController` | 可手動/程式呼叫 `Generate()` 重新生成。
| Tiles 等比縮放 | `Canvas/BoardPanel/Tiles` | `Game.Board.BoardAutoFitPerimeter` | 只調整 `localScale` 與 `anchoredPosition`，支援解析度變更。
| 棋子尺寸 | `Canvas/BoardPanel/Pawns` 下的每個 Pawn | `Game.Board.PawnAutoSizeToTile` | `tilesRoot` 指向 Tiles；支援 Editor/Runtime。
| 棋子移動 | 每個 Pawn | `Game.Board.PawnController` | `onLanded` 事件可接 UI／事件系統。
| 擲骰流程 | 任意控制器物件 | `Game.Board.TurnManager` | 呼叫 `RollAndMove()` 觸發 `PawnController`。
| 棋盤事件橋接 | 任意控制器物件 | `Game.Board.BoardEventsBridge` | 監聽 `PawnController.onLanded`，切換事件或戰鬥面板。
| 面板切換 | Canvas 根節點 | `Game.UI.BoardEventRouter` | 呼叫 `ShowBoard/ShowEvent/ShowCombat` 切面板。
| Roll 按鈕快捷 | UI Roll Button | `Game.UI.RollButtonBinder` | 熱鍵預設為 `R`，可尋找 `TurnManager` 或 `PawnController`。

## 常見地雷
- **棋盤縮放錯位**：請確認 Tiles 只有 `BoardAutoFitPerimeter` 會調整。移除舊的 `RectAutoStretch`、`BoardLayoutFix` 類腳本，避免互搶座標。
- **Pawn 沒有跟著格子移動**：`PawnController.board` 必須指向 Tiles（`RectTransform`），並確保 `PawnAutoSizeToTile.tilesRoot` 也連接同一個 Tiles。
- **事件沒觸發**：新的 `PawnController` 在落地時會發 `onLanded(int index)`。請確認 `BoardEventsBridge` 已訂閱該事件，或自行掛上 listener。
- **編譯錯誤 `CS0246 PawnController`**：所有腳本需引用 `Game.Board.PawnController`。若使用自訂 asmdef，請為需要的 assembly 新增對 `KG.Board` 的參考。
- **UI 亂縮放**：確保 `BoardPanel` 下的 `Tiles/Pawns/UI` Anchor = Stretch、Offset = 0；若有額外 Layout Group，需在其更新後再呼叫 `BoardAutoFitPerimeter.RequestFit()`。

## 驗證流程
1. 進行無頭編譯或 `Ctrl+B`，確認無錯誤/警告。
2. 進入 Play 模式：
   - 觀察 Tiles 與 Pawn 是否置中、邊距一致、畫面尺寸改變仍維持比例。
   - 切換至其他 UI 頁面後再回到棋盤，Tiles 不得跑位。
3. 點擊 Roll 按鈕或按 `R`，確認 Pawn 正常移動、`BoardEventsBridge` 觸發事件或戰鬥請求時 UI 無抖動。

## 此次整併清單
- 新增 `Game.Board.BoardAutoFitPerimeter`，統一處理 Tiles 等比縮放與置中。
- 調整 `CyberLife.Board.BoardZoomToFit` 為舊版相容殼層，改為呼叫 `BoardAutoFitPerimeter`。
- `Game.Board.PawnController` 新增 `onLanded` 事件與啟用時 Snap，同時修正命名空間引用問題。
- 清整 UI：保留單一 `Game.UI.RollButtonBinder`、`Game.UI.BoardEventRouter`，其餘舊腳本不再修改 RectTransform。
- 文件化流程，新增《BOARDKIT_RUNTIME.md》協助整合與驗證。
