# DEVLOG.md

> 時區：UTC+8｜LTS：2022.3｜這份日誌只記「決策與行為」，細節放到 PR。

## 2025-10-01
- 戰鬥「無敵人 Prefab」方案落地：
  - `CombatPageController` 依 `EnemyDefinition` 動態生成 `RectTransform + Image + Button + Combatant + EnemyTargetButton`；自動套 `portrait`、`BodyPreset`。
  - `CombatUIController`：`TargetButtons` 未選目標隱藏 → `SelectTarget()` 顯示 → `HitGroupButton()` 攻擊後隱藏。
  - 固定 HitGroup 索引：`0=Head,1=Torso,2=LeftArm,3=RightArm,4=LeftLeg,5=RightLeg`。
- 交付：`CombatPageController.cs`、`CombatUIController.cs` 修訂與 zip。
- Docs：建立 `ARCHITECTURE.md`（快照）與本檔 `DEVLOG.md`。
- Repo：根目錄加入 `.gitignore`（Unity）與 `.gitattributes`（LFS）。

### Smoke Test（30 秒）
1. 開 `SampleScene`，`CombatPanel/UI/TargetButtons` 預設不勾選（或讓程式在 Start 關閉）。  
2. `CombatPageController` 指定：`manager`、`ui`、`enemiesRoot(=Enemys)`、兩個面板。  
3. `CombatUIController` 指定：`manager`、`groupsPanel(=TargetButtons)`；六顆按鈕綁 `HitGroupButton(0~5)`。  
4. 進戰鬥：點任一敵人 → TargetButtons 顯示 → 按任一部位 → 攻擊、按鈕收回。

### 下一步
- 把 `EventFlagStorage` 序列化接到 `SaveData`；補 `GameManager` 戰後結算的旗標/冷卻更新。  
- Combat：把 `CombatManager.EndCombat` 的結果路由回 `GameManager.OnCombatWin/Lose/Escape`（若尚未接齊）。  
- UI：`Enemys` 根節點加 `GridLayoutGroup`，調整 cell-size 與間距以適配不同數量敵人。

## 2025-09-30
- `CombatPageController`：新增 `StartCombat()` / `StartCombatWithCount(int)` 與 `defaultEncounterForButtons`；Spawn 改為**不依賴敵人 Prefab**；自動綁 `EnemyTargetButton`。  
- `CombatUIController`：按鈕直接綁 `HitGroupButton(int)`；攻擊後隱藏。

## 2025-09-29
- 敵人建立流程確立：Sprite→`BodyPreset(22)`→`EnemyDefinition`→`CombatEncounter`→事件 `StartsCombat` 引用。  
- 修正 weightOverride 規則：`>0` 才覆蓋，`0/-1` 沿用事件權重。

## 2025-09-28 ~ 更早
- MenuManager 堆疊返回；Prologue 打字機；字體與 CJK Fallback；存檔摘要；BGM/環境音系統；AbilityStats 與事件加權。