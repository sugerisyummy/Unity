# ARCHITECTURE.md — 2025-10-01 快照

> 專案：cyberLife（Unity 2022.3 LTS）  
> 風格：像素；單場景多面板；事件驅動 + 戰鬥面板。

## 1. 高層結構
- UI 流：MainMenu → Prologue → Difficulty → StoryPanel →（事件選項）→ CombatPanel → 回 Story。
- 面板管理：`MenuManager`（堆疊返回 / ResetToPanel / ESC=Back）。
- 劇情事件：`CaseDatabase` + `DolEventAsset` + `EventFlagStorage`（一次性 / 冷卻 / 旗標 / 權重抽取）。
- 數值：`PlayerStats`（HP/Money/Sanity…）+ `AbilityStats`（Str/Agi/Int/Cha/Stealth/Tech）。
- HUD：`HUDStats` + `StatEventBus` + `StatBar` + `GameManagerStatProvider`。
- 音訊：`AudioManager`、`SoundEffectManager`、`CaseVisuals`（背景/BGM/環境音）。
- 存讀：`SaveData`、`SaveManager`、`SaveSlotButton`（摘要含 Case/時間）。
- 戰鬥：**不使用敵人 Prefab**；由 `CombatPageController` 依 `EnemyDefinition` 動態生成 UI + 元件；
  `CombatUIController` 管「選目標才顯示部位鈕」，`CombatManager` 負責結算。命名空間統一 `CyberLife.Combat`。

## 2. 核心模組
### 2.1 事件系統
- `CaseDatabase`: 每地點的事件池（可 weightOverride；>0 才覆蓋）。
- `DolEventAsset`: 多頁事件 / 選項（一次性/冷卻/旗標/數值變更/StartsCombat 等）。
- `EventFlagStorage`: 持久保存一次性/冷卻/旗標狀態（需完整序列化到存檔）。
- `GameManager`:
  - `RollAndStartEvent` 依能力與權重選取事件。
  - 與 `CombatPageController` 溝通：當 `choice.StartsCombat` → 呼叫 `StartCombatWithEncounter(choice.combatEncounter)`，把 `pendingCombatChoice` 存起來；
    由 `OnCombatWin/OnCombatLose/OnCombatEscape` 根據結果再結算與跳頁。

### 2.2 戰鬥系統（無 Prefab）
- 主要腳本：
  - `CombatManager`（攻擊/命中/部位/效果/結束結果）。
  - `CombatPageController`（**動態生成敵人 UI**：`RectTransform + Image + Button + Combatant + EnemyTargetButton`，自動套 `portrait` 與 `BodyPreset`）
  - `CombatUIController`（`groupsPanel`=TargetButtons：未選目標隱藏 → `SelectTarget()` 顯示 → `HitGroupButton()` 攻擊後隱藏）。
  - `Combatant`、`BodyPreset`、`CombatantExtensions`（22 部位模板注入）。
  - `EnemyDefinition`（Name/portrait/bodyPreset/基礎數值）。
  - `CombatEncounter`（一場戰鬥要生成的敵人清單）。
  - `EnemyTargetButton`（點擊敵人 → `ui.SelectTarget(owner)`）。
- HitGroup 固定索引：`0=Head, 1=Torso, 2=LeftArm, 3=RightArm, 4=LeftLeg, 5=RightLeg`。

### 2.3 UI 接線要點（戰鬥）
- `CombatPageController`
  - `manager` → 場上 `CombatManager`
  - `ui` → 場上 `CombatUIController`
  - `enemiesRoot` → `CombatPanel/Enemys`（建議掛 GridLayoutGroup）
  - `storyPanel` / `combatPanel` → 你的兩個面板
- `CombatUIController`
  - `manager` → `CombatManager`
  - `groupsPanel` → `CombatPanel/UI/TargetButtons`（開場自動 `SetActive(false)`）
- 六顆 `ChoiceButton(0~5)` → `HitGroupButton(int)`。

## 3. 檔案樹（Scripts 摘要）
```
Assets/Scripts
├─ AbilityStats.cs
├─ AudioManager.cs
├─ ButtonController.cs
├─ CaseDatabase.cs
├─ CaseId.cs
├─ CaseVisuals.cs
├─ EventFlagStorage.cs
├─ GameManager.cs
├─ GameManagerStatProvider.cs
├─ HUDStats.cs
├─ MenuManager.cs
├─ PlayerStats.cs
├─ ProloguePlayer.cs
├─ SampleAssetsGenerator.cs
├─ SaveData.cs
├─ SaveManager.cs
├─ SaveSlotButton.cs
├─ SoundEffectManager.cs
├─ StatBar.cs
├─ StatEventBus.cs
├─ StatType.cs
├─ StoryData.cs
├─ TMPFontReplacer.cs
└─ Combat/
   ├─ ArmorDef.cs
   ├─ BodyPreset.cs
   ├─ BodyTagDestroyedCondition.cs
   ├─ Combatant.cs
   ├─ CombatEncounter.cs
   ├─ CombatManager.cs
   ├─ CombatManager_InventoryHook.cs
   ├─ CombatPageController.cs
   ├─ CombatResultRouter.cs
   ├─ CombatSceneLoader.cs   (目前不使用)
   ├─ CombatSpecialCondition.cs
   ├─ CombatStarterButton.cs
   ├─ CombatantExtensions.cs
   ├─ ConsumableDef.cs
   ├─ Effects.cs
   ├─ EnemyDefinition.cs
   ├─ EnemyTargetButton.cs
   ├─ Enums.cs
   ├─ InventoryManager.cs
   ├─ ItemDef.cs
   ├─ ItemRegistry.cs
   ├─ TargetingPanel.cs
   └─ WeaponDef.cs
```

## 4. Git / LFS
- 已加入：`.gitignore`（Unity）與 `.gitattributes`（LFS：圖像/音訊/影片/3D/字型等）。
- 推薦分支：`main` 穩定、`feat/*` 功能、`fix/*` 修補。Commit 前先 Play 一次（Smoke test 30 秒）。

## 5. 未來待辦（精簡）
1) 旗標/冷卻/一次性完整序列化至存檔。  
2) 勝利 / 遊戲結束 UI 與條件。  
3) HUD 條列視覺（血條/狀態）與可及性。  
4) 事件內容量擴充（地點與事件）。  
5) BGM/環境音資產與混音。  
6) 平衡性回圈（飢渴疲勞→負面狀態→事件加權）。  
7) 地圖/移動或輕量戰鬥模組（可選）。