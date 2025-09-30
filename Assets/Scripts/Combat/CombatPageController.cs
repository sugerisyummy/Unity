
// Copyright (c) 2025
// CombatPageController — No-Prefab version
// Spawns enemy UI purely from EnemyDefinition (portrait + stats + body preset).

using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace CyberLife.Combat
{
    public class CombatPageController : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject storyPanel;
        public GameObject combatPanel;

        [Header("Refs")]
        public CombatManager manager;
        public CombatUIController ui;
        public RectTransform enemiesRoot;

        [Header("Optional")]
        [Tooltip("If set (and defined in Project Settings → Tags), it will be assigned to spawned enemies. Leave empty to skip.")]
        public string enemyTag = "";

        [Header("Legacy Buttons")]
        public CombatEncounter defaultEncounterForButtons;

        // Entry points -----------------------
        public void StartCombatWithEncounter(CombatEncounter encounterAsset)
        {
            Debug.Log($"[CPC] StartCombatWithEncounter {encounterAsset}");
            if (!encounterAsset) { Debug.LogWarning("[CPC] EncounterAsset is null."); return; }

            if (storyPanel) storyPanel.SetActive(false);
            if (combatPanel) combatPanel.SetActive(true);

            // clear previous
            if (enemiesRoot)
            {
                for (int i = enemiesRoot.childCount - 1; i >= 0; i--)
                    Destroy(enemiesRoot.GetChild(i).gameObject);
            }

            if (ui) ui.SelectTarget(null); // hide groups

            SpawnByEncounterAsset(encounterAsset);
        }

        public void StartCombat() => StartCombatWithEncounter(defaultEncounterForButtons);
        public void StartCombatWithCount(int count) => StartCombatWithEncounter(defaultEncounterForButtons);

        public void BackToStory()
        {
            if (combatPanel) combatPanel.SetActive(false);
            if (storyPanel) storyPanel.SetActive(true);
        }

        // Spawning ---------------------------
        private void SpawnByEncounterAsset(object enc)
        {
            if (enc == null) { Debug.LogWarning("[CPC] Encounter is null"); return; }

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            // 1) enemies: EnemyDefinition[] / List<EnemyDefinition>
            var enemiesField = enc.GetType().GetField("enemies", flags);
            if (enemiesField != null)
            {
                var val = enemiesField.GetValue(enc);
                if (val is EnemyDefinition[] arr)
                {
                    foreach (var def in arr) if (def) SpawnOne(def);
                    Debug.Log($"[CPC] Spawned {arr.Length} by enemies[]");
                    return;
                }
                if (val is System.Collections.IList list)
                {
                    int c = 0;
                    foreach (var it in list) { if (it is EnemyDefinition d && d) { SpawnOne(d); c++; } }
                    Debug.Log($"[CPC] Spawned {c} by enemies[List]");
                    return;
                }
            }

            // 2) slots/entries with {def,count}
            var slotsField = enc.GetType().GetField("slots", flags)
                           ?? enc.GetType().GetField("entries", flags)
                           ?? enc.GetType().GetField("list", flags);
            if (slotsField != null)
            {
                int total = 0;
                if (slotsField.GetValue(enc) is IEnumerable slots)
                {
                    foreach (var s in slots)
                    {
                        var t = s.GetType();
                        var defF = t.GetField("def", flags) ?? t.GetField("enemy", flags) ?? t.GetField("definition", flags);
                        var cntF = t.GetField("count", flags) ?? t.GetField("qty", flags) ?? t.GetField("amount", flags);
                        var def  = defF?.GetValue(s) as EnemyDefinition;
                        int count = 1;
                        if (cntF != null)
                        {
                            try { count = Convert.ToInt32(cntF.GetValue(s)); } catch { count = 1; }
                            if (count < 1) count = 1;
                        }
                        for (int i = 0; i < count; i++) if (def) { SpawnOne(def); total++; }
                    }
                }
                Debug.Log($"[CPC] Spawned {total} by slots/entries");
                return;
            }

            Debug.LogWarning("[CPC] Unsupported CombatEncounter layout.");
        }

        private void ApplyOptionalTag(GameObject go)
        {
            if (!string.IsNullOrEmpty(enemyTag))
            {
                try { go.tag = enemyTag; } catch { /* if tag missing, ignore */ }
            }
        }

        private void SetIntMember(object obj, string[] names, int value)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (var n in names)
            {
                var f = obj.GetType().GetField(n, flags);
                if (f != null && f.FieldType == typeof(int)) { f.SetValue(obj, value); return; }
                var p = obj.GetType().GetProperty(n, flags);
                if (p != null && p.CanWrite && p.PropertyType == typeof(int)) { p.SetValue(obj, value, null); return; }
            }
        }

        private void BindEnemyTargetButton(Component etb, Combatant c)
        {
            if (!etb) return;
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var t = etb.GetType();

            // ui
            var uiField = t.GetField("ui", flags) ?? t.GetField("controller", flags) ?? t.GetField("combatUI", flags);
            if (uiField != null) uiField.SetValue(etb, ui);
            else
            {
                var uiProp = t.GetProperty("ui", flags) ?? t.GetProperty("controller", flags) ?? t.GetProperty("combatUI", flags);
                if (uiProp != null && uiProp.CanWrite) uiProp.SetValue(etb, ui, null);
            }
            // owner/target
            var ownField = t.GetField("enemy", flags) ?? t.GetField("owner", flags) ?? t.GetField("target", flags) ?? t.GetField("combatant", flags);
            if (ownField != null) ownField.SetValue(etb, c);
            else
            {
                var ownProp = t.GetProperty("enemy", flags) ?? t.GetProperty("owner", flags) ?? t.GetProperty("target", flags) ?? t.GetProperty("combatant", flags);
                if (ownProp != null && ownProp.CanWrite) ownProp.SetValue(etb, c, null);
            }
        }

        private void SpawnOne(EnemyDefinition def)
        {
            if (!def || !enemiesRoot) return;

            // Create UI object
            var go = new GameObject($"Enemy_{(string.IsNullOrEmpty(def.enemyName) ? def.name : def.enemyName)}",
                                    typeof(RectTransform),
                                    typeof(CanvasRenderer),
                                    typeof(Image),
                                    typeof(Button),
                                    typeof(Combatant),
                                    typeof(EnemyTargetButton));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(enemiesRoot, false);

            // size: follow grid if any, else 128x128
            var grid = enemiesRoot.GetComponent<GridLayoutGroup>();
            if (grid) rt.sizeDelta = grid.cellSize;
            else rt.sizeDelta = new Vector2(128, 128);

            ApplyOptionalTag(go);

            // Portrait
            var img = go.GetComponent<Image>();
            img.sprite = def.portrait;
            img.preserveAspect = true;
            img.raycastTarget = true;

            // Button
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;

            // Combatant stats
            var c = go.GetComponent<Combatant>();
            if (c)
            {
                int m = Mathf.Max(1, def.maxHP);
                SetIntMember(c, new[] { "maxHP", "MaxHP" }, m);
                SetIntMember(c, new[] { "hp", "HP", "currentHP" }, m);
                SetIntMember(c, new[] { "attack", "Attack" }, def.attack);
                SetIntMember(c, new[] { "defense", "Defense" }, def.defense);
                SetIntMember(c, new[] { "speed", "Speed" }, def.speed);
                if (def.bodyPreset) c.ApplyBodyPreset(def.bodyPreset);
            }

            // EnemyTargetButton binding
            var etb = go.GetComponent<EnemyTargetButton>();
            BindEnemyTargetButton(etb, c);

            // Fallback click → select target
            if (ui) btn.onClick.AddListener(() => ui.SelectTarget(c));

            Debug.Log($"[CPC] SpawnOne(no-prefab) -> {go.name}");
        }
    }
}
