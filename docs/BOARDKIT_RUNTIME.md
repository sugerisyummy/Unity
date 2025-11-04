# BoardKit 運行環境指引

## 元件對應表
| 區塊 | 指派腳本 / 職責 |
|------|-----------------|
| **BoardPanel/Tiles** | `BoardAutoFitPerimeter`：偵測父層大小，僅調整 Tiles 容器的 `localScale`，確保棋盤不會超框。 |
| **BoardPanel/Pawns** | `PawnController`：根據棋盤索引定位棋子與移動。 |
| **BoardPanel/UI** | 無自動縮放腳本：維持既有版面，必要時手動調整。 |

## 安裝與設定步驟
1. **掛載腳本**
   - 在 `BoardPanel/Tiles`（或棋盤根節點）上掛 `BoardAutoFitPerimeter`，`Tiles` 欄位指向實際要縮放的容器。
   - `BoardPanel/Pawns` 下的每個棋子掛 `PawnController`，並設定 `board`、`pawn` 參考。
   - `RollButtonBinder` 掛在 Roll 按鈕物件上，配合 `TurnManager` 或 `PawnController`。
2. **錨點與 Offset 建議**
   - `BoardPanel` 仍以 Canvas 中心對齊：錨點 `0.5, 0.5`、Pivot `0.5, 0.5`、`offset` 為 `0`。
   - `Tiles`、`Pawns`、`UI` 子節點維持中心錨點，讓 `BoardAutoFitPerimeter` 專注在縮放。
3. **常見錯誤排查**
   - **同時修改 RectTransform**：避免再掛 `RectAutoStretch`、`BoardTripletAutoStretch`、`BoardLayoutFix` 等舊腳本，會與 `BoardAutoFitPerimeter` 搶寫尺寸。
   - **未啟用面板時啟動協程**：請確保棋盤物件在 `OnEnable` 後再初始化；若需要延遲，可呼叫 `Fit()` 或重新啟用物件讓腳本重算。

## 驗證步驟
1. 在編輯器進入 Play Mode。
2. 確認棋盤自動縮放後不會超出外框。
3. 切換至其他頁籤再返回，棋盤仍保持正確比例。
4. 按下 `R` 或 UI 上的 `Roll` 按鈕，可成功觸發擲骰與棋子移動。

## 變更清單
- 新版 `BoardAutoFitPerimeter` 僅縮放 Tiles 容器並在啟用後一幀重新計算，支援解析度變化。
- `RollButtonBinder` 統一保留於 `Assets/Scripts/UI/RollButtonBinder.cs`。
- 移除舊版 `RectAutoStretch`、`BoardTripletAutoStretch`、Scene-wide `BoardLayoutFix` 的執行腳本，避免與新版縮放流程重疊。
- `TurnManager`、`BoardEventsBridge` 明確引用 `Game.Board.PawnController`，避免編譯錯誤。
