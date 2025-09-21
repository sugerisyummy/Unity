# Combat Phase B Scaffold

你勾選的方向已套用：
- Additive `CombatScene` 切場景
- 肢體 + 器官（RimWorld 風）
- 物品庫（列表 + 重量，骨架）
- 裝備欄：Head/Torso/Legs/Utility + Weapon
- 傷害型別：斬/刺/鈍/熱能/化學/彈道
- Fallout 風**鎖定部位**：UI/TargetingPanel（按鈕代表部位）

## 場景
- 新增 `CombatScene`，放一個物件掛 `CL.Combat.CombatBootstrap`。
- 想看 log：加一個 TMP_Text，掛 `CL.Combat.CombatHUD`。

## 用法
- 事件選項照舊勾 `startsCombat` + 指定 `Encounter`。
- 戰鬥中按 TargetingPanel 的部位按鈕 → Attack → 記錄在 log。

## 後續要接的（我可繼續做）
- 從 PlayerStats 讀玩家數值與裝備（現在用預設），並寫回 SaveData（inventory/equipment）。
- 裝甲對應部位的實際減傷（目前使用簡化 max 值）。
- 化學/熱能等賦予/清除 Effect（用 `cureTag`）。
- 行動序與速度/防禦影響斷肢與命中（現為簡化版）。
