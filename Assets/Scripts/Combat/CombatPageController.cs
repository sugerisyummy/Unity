using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace CyberLife.Combat
{
    public class CombatPageController : MonoBehaviour
    {
        [Header("Refs")]
        public CombatManager manager;
        public Transform enemiesRoot;
        public CombatUIController ui;

        [Header("Panels (optional)")]
        public GameObject storyPanel;
        public GameObject combatPanel;

        [Header("Player")]
        public Combatant player;
        public Transform playerSlot;          // 可指到 CombatPanel/player
        public int defaultPlayerHP = 60;

        [Header("Defaults")]
        public CombatEncounter defaultEncounterForButtons;

        // === 入口 ===
        public void StartCombat()
        {
            if (!EnsureReady()) return;
            ui?.ClearSelection(); // ← 確保 UI 初始狀態乾淨

            var enemies = CollectEnemiesInRoot();
            if (enemies.Count == 0 && defaultEncounterForButtons != null)
            {
                ClearEnemies();
                SpawnByEncounterAsset(defaultEncounterForButtons);
                enemies = CollectEnemiesInRoot();
            }
            if (enemies.Count == 0) { Debug.LogWarning("[CombatPage] 沒有敵人可開戰"); return; }

            ResetEnemyVisuals();       // ← 關鍵：第二場戰鬥把上一場的灰階/不可點復原
            EnsureEnemiesHaveHealth();

            var ally = new List<Combatant>();
            var p = EnsurePlayerCombatant(); if (p) ally.Add(p);

            manager.BeginCombat(ally, enemies);
            ShowOnly(combatPanel);
            _LogCurrentTeams();
        }

        public void StartCombatWithEncounter(CombatEncounter enc)
        {
            if (!EnsureReady()) return;
            ui?.ClearSelection();

            if (!enc) { Debug.LogError("[CombatPage] Encounter 為空"); return; }

            ClearEnemies();
            SpawnByEncounterAsset(enc);
            ResetEnemyVisuals();
            EnsureEnemiesHaveHealth();

            var enemies = CollectEnemiesInRoot();
            var ally = new List<Combatant>();
            var p = EnsurePlayerCombatant(); if (p) ally.Add(p);

            manager.BeginCombat(ally, enemies);
            ShowOnly(combatPanel);
            _LogCurrentTeams();
        }

        public void StartCombatWithCount(int count)
        {
            if (!EnsureReady()) return;
            ui?.ClearSelection();

            ClearEnemies();
            count = Mathf.Max(1, count);
            for (int i = 0; i < count; i++) SpawnGenericCard($"Enemy_{i+1}");
            ResetEnemyVisuals();
            EnsureEnemiesHaveHealth();

            var enemies = CollectEnemiesInRoot();
            var ally = new List<Combatant>() { EnsurePlayerCombatant() }.Where(x=>x!=null).ToList();

            manager.BeginCombat(ally, enemies);
            ShowOnly(combatPanel);
            _LogCurrentTeams();
        }

        public void ShowOnly(GameObject target)
        {
            if (combatPanel) combatPanel.SetActive(false);
            if (storyPanel)  storyPanel.SetActive(false);
            if (target)      target.SetActive(true);
            Debug.Log($"[CombatPage] ShowOnly → {(target ? target.name : "null")}");
        }

        public void BackToStory()
        {
            Debug.Log("[CombatPage] BackToStory()");
            ui?.ClearSelection();
            ShowOnly(storyPanel);
        }

        // === 內部 ===
        bool EnsureReady()
        {
            if (!manager)     { Debug.LogError("[CombatPage] manager is null"); return false; }
            if (!enemiesRoot) { Debug.LogError("[CombatPage] enemiesRoot is null"); return false; }
            EnsureLayout();
            return true;
        }
        void EnsureLayout()
        {
            var grid = enemiesRoot.GetComponent<GridLayoutGroup>();
            if (!grid)
            {
                grid = enemiesRoot.gameObject.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(200, 240);
                grid.spacing = new Vector2(16, 8);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 4;
                grid.childAlignment = TextAnchor.UpperCenter;
            }
        }
        void ClearEnemies()
        {
            for (int i = enemiesRoot.childCount - 1; i >= 0; i--)
                Destroy(enemiesRoot.GetChild(i).gameObject);
        }
        List<Combatant> CollectEnemiesInRoot()
        {
            var list = enemiesRoot.GetComponentsInChildren<Combatant>(false).ToList();
            if (playerSlot) list.RemoveAll(c => c && c.transform.IsChildOf(playerSlot));
            return list.Where(c=>c!=null).ToList();
        }

        void ResetEnemyVisuals()
        {
            foreach (var c in enemiesRoot.GetComponentsInChildren<Combatant>(true))
            {
                if (!c) continue;
                var go = c.gameObject;
                var cg = go.GetComponent<CanvasGroup>(); if (cg) cg.alpha = 1f;
                var btn = go.GetComponent<Button>(); if (btn) btn.interactable = true;
                foreach (var g in go.GetComponentsInChildren<Graphic>(true)) g.raycastTarget = true;
            }
        }

        Combatant EnsurePlayerCombatant()
        {
            if (player) return player;

            var all = FindObjectsOfType<Combatant>(true);
            var p = all.FirstOrDefault(x => x && x.CompareTag("Player"));
            if (!p && playerSlot) p = playerSlot.GetComponentInChildren<Combatant>(true);
            if (!p && all.Length > 0) p = all.FirstOrDefault(x => x && (enemiesRoot==null || !x.transform.IsChildOf(enemiesRoot)));
            if (!p)
            {
                var parent = playerSlot ? playerSlot : transform;
                var go = new GameObject("PlayerCombatant");
                go.transform.SetParent(parent, false);
                go.AddComponent<RectTransform>();
                go.AddComponent<Image>().preserveAspect = true;
                go.AddComponent<Button>().interactable   = false;

                p = go.AddComponent<Combatant>();
                var bh = go.AddComponent<BasicHealth>(); bh.MaxHP = defaultPlayerHP; bh.HP = defaultPlayerHP;
                go.tag = "Player";
                Debug.Log("[CombatPage] Auto-created PlayerCombatant (BasicHealth)");
            }
            player = p; return player;
        }
        void EnsureEnemiesHaveHealth()
        {
            foreach (var c in enemiesRoot.GetComponentsInChildren<Combatant>(false))
                if (c && !c.GetComponent<BasicHealth>()) c.gameObject.AddComponent<BasicHealth>();
        }

        // === Encounter 生成 / 泛型生成 ===（與先前相同，省略到尾）
        void SpawnByEncounterAsset(CombatEncounter enc)
        {
            int spawned = 0;
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            System.Collections.IEnumerable list = null;
            foreach (var n in new[]{ "enemies","units","entries","slots"})
            {
                var f = enc.GetType().GetField(n, flags);
                if (f != null) { list = f.GetValue(enc) as System.Collections.IEnumerable; if (list != null) break; }
                var p = enc.GetType().GetProperty(n, flags);
                if (p != null) { list = p.GetValue(enc) as System.Collections.IEnumerable; if (list != null) break; }
            }
            if (list == null) { Debug.LogWarning("[CombatPage] SpawnByEncounterAsset → no enemies list found"); return; }

            foreach (var item in list)
            {
                if (item == null) continue;
                if (TrySpawnKeyValue(item, ref spawned)) continue;
                if (TrySpawnSlotObject(item, ref spawned)) continue;
                if (TrySpawnUnknown(item, ref spawned))   continue;
            }
            Debug.Log($"[CombatPage] SpawnByEncounterAsset → spawned:{spawned}");
        }
        bool TrySpawnKeyValue(object kv, ref int spawned)
        {
            var t = kv.GetType(); var flags = BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
            var key = t.GetProperty("Key", flags); var val = t.GetProperty("Value", flags);
            if (key==null || val==null) return false;
            object def = key.GetValue(kv); int cnt=1; try{ cnt=(int)System.Convert.ChangeType(val.GetValue(kv),typeof(int)); }catch{}
            for (int i=0;i<cnt;i++) TrySpawnUnknown(def, ref spawned);
            return true;
        }
        bool TrySpawnSlotObject(object slot, ref int spawned)
        {
            var t=slot.GetType(); var flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
            var defF=t.GetField("def",flags)??t.GetField("definition",flags); if(defF==null) return false;
            object def=defF.GetValue(slot); int cnt=1; var cntF=t.GetField("count",flags)??t.GetField("num",flags);
            if(cntF!=null){ try{ cnt=(int)cntF.GetValue(slot);}catch{} }
            for(int i=0;i<cnt;i++) TrySpawnUnknown(def, ref spawned);
            return true;
        }
        bool TrySpawnUnknown(object obj, ref int spawned)
        {
            if (obj==null) return true;
            var flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
            var spawnOne=GetType().GetMethod("SpawnOne",flags,null,new System.Type[]{ obj.GetType() },null);
            if (spawnOne!=null){ spawnOne.Invoke(this,new object[]{obj}); spawned++; return true; }
            SpawnGenericCard(GetNiceName(obj), FindSprite(obj)); spawned++; return true;
        }

        // === 卡片 ===
        void SpawnGenericCard(string displayName, Sprite sp=null)
        {
            var go=new GameObject(string.IsNullOrEmpty(displayName)?"Enemy":displayName);
            go.transform.SetParent(enemiesRoot,false);
            go.AddComponent<RectTransform>();
            var img=go.AddComponent<Image>(); img.preserveAspect=true;
            go.AddComponent<Button>();
            var cbt=go.AddComponent<Combatant>();
            var hp =go.AddComponent<BasicHealth>(); hp.MaxHP=20; hp.HP=20;
            var etb=go.AddComponent<EnemyTargetButton>(); if(ui) etb.ui=ui; etb.enemy=cbt;
            var le =go.AddComponent<LayoutElement>(); le.preferredWidth=200; le.preferredHeight=240;
            if (sp) img.sprite=sp;
        }
        string GetNiceName(object obj)
        {
            var t=obj.GetType(); var flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
            var nameP=t.GetProperty("name",flags); if(nameP!=null){ try{ var v=nameP.GetValue(obj) as string; if(!string.IsNullOrEmpty(v)) return v; }catch{} }
            var idP=t.GetProperty("id",flags); if(idP!=null){ try{ return $"Enemy_{idP.GetValue(obj)}"; }catch{} }
            return t.Name;
        }
        Sprite FindSprite(object obj)
        {
            var t=obj.GetType(); var flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
            foreach(var n in new[]{"portrait","sprite","icon","image","face"})
            {
                var pf=t.GetField(n,flags); if(pf!=null && pf.FieldType==typeof(Sprite)){ try{ return (Sprite)pf.GetValue(obj);}catch{} }
                var pp=t.GetProperty(n,flags); if(pp!=null && pp.PropertyType==typeof(Sprite)){ try{ return (Sprite)pp.GetValue(obj);}catch{} }
            }
            return null;
        }

        // === 偵錯 ===
        void _LogCurrentTeams()
        {
            int a = manager && manager.allies  != null ? manager.allies.Count  : -1;
            int e = manager && manager.enemies != null ? manager.enemies.Count : -1;
            Debug.Log($"[CombatPage] Teams allies:{a} enemies:{e}");
        }
    }
}
