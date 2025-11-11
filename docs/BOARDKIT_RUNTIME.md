# BoardKit 運行環境指引

## 元件對應表
| 區塊 | 指派腳本 / 職責 |
|------|-----------------|
| **BoardPanel/Tiles** | 25 個 Tile 依走格順序排列，無自動縮放腳本。 |
| **BoardPanel/Pawns** | `PawnController`：根據棋盤索引定位棋子並逐格移動。 |
| **BoardPanel/UI** | `DiceRollerUI` 顯示骰子數字、`RollButtonBinder` 綁定按鈕與熱鍵。 |

## 安裝與設定步驟
1. **掛載腳本**
   - 將 `PawnController` 掛在棋子物件上，設定 `tilesRoot` 指向 `BoardPanel/Tiles`、`pawn` 指向自身 RectTransform。
   - 將 `DiceRollerUI` 掛在骰子文字或容器上，必要時手動指派 `label` 與 `pawnController`。
   - 將 `RollButtonBinder` 掛在中心 `Roll` 按鈕上，可與 `DiceRollerUI` 同物件使用。
2. **棋盤配置**
   - `Tiles` 子物件依實際走格順序排列，共 25 格。
   - `PawnController` 的 `startIndex` 與 `currentIndex` 可對應起始格，啟用時會自動貼齊。
3. **常見錯誤排查**
   - 確認 `DiceRollerUI` 的 `minValue` 小於或等於 `maxValue`，避免擲骰範圍錯誤。
   - 若骰子文字未更新，手動指派 `label` 或確認場景中含有 Text/TMP 元件。
   - 若棋子未移動，確認 `tilesRoot` 參照與 `PawnController` 是否處於啟用狀態。

## 驗證步驟
1. 在編輯器進入 Play Mode。
2. 啟用 BoardPanel 後按下 `R` 或中間 `Roll` 按鈕，骰子數字會在約 1 秒內快速跳動後定格。
3. 擲骰結束時，棋子會依結果前進對應格數，落點與 Tiles 順序一致。
4. 調整 `DiceRollerUI` 的 `minValue`、`maxValue` 可驗證擲骰上下限生效。

## 變更清單
- 移除 `BoardAutoFitPerimeter`、`BoardTripletAutoStretch`、`PawnAutoSizeToTile` 等自動縮放腳本與依賴。
- 新增 `DiceRollerUI` 協同 `RollButtonBinder` 提供擲骰展示與熱鍵綁定。
- `PawnController` 支援協程逐格前進並在啟用時自動貼齊棋盤格。 
