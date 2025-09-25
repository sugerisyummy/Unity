using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Reflection;

namespace CyberLife.Combat
{
    /// 戰鬥＝獨立面板（支援事件決定敵人數量/Prefab/Encounter 資產）
    public class CombatPageController : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject storyPanel;
        public GameObject combatPanel;
        [Tooltip("同層面板共同父物件（會把兄弟面板全關，只開目標）。可留空。")]
        public Transform panelsRoot;

        [Header("Combat (固定方)")]
        public CombatManager manager;     // CombatPanel/Manager
        public Combatant player;          // CombatPanel/Player

        [Header("Dynamic Enemies")]
        public Transform enemiesRoot;         // CombatPanel/Enemys（空物件）
        public GameObject defaultEnemyPrefab; // 需含 Combatant+InventoryManager+EnemyTargetButton，Tag=Enemy
        public CombatUIController ui;         // CombatUI（給 EnemyTargetButton 指回來）

        [Header("Result hooks")]
        public UnityEvent onWin;
        public UnityEvent onLose;
        public UnityEvent onSpecial;          // 之後接 SpecialCondition 再用

        bool running;
        readonly List<GameObject> spawned = new();

        // ====== 三個入口（事件可直接呼叫）======
        public void StartCombatWithCount(int count)
        {
            if (!CheckFixedRefs()) return;
            ClearSpawned();
            SpawnByCount(count);
            BeginWithCurrent();
        }

        public void StartCombatWithPrefabs(GameObject[] prefabs)
        {
            if (!CheckFixedRefs()) return;
            ClearSpawned();
            SpawnByPrefabs(prefabs);
            BeginWithCurrent();
        }

        public void StartCombatWithEncounter(ScriptableObject encounterAsset)
        {
            if (!CheckFixedRefs()) return;
            ClearSpawned();
            SpawnByEncounterAsset(encounterAsset);
            BeginWithCurrent();
        }

        // 舊版保留：若你先前已經綁這個
        public void StartCombat()
        {
            if (!CheckFixedRefs()) return;
            BeginWithCurrent();
        }

        void BeginWithCurrent()
        {
            ShowOnly(combatPanel);

            var allyList  = new List<Combatant> { player };
            var enemyList = CollectEnemies();

            if (enemyList.Count == 0)
            {
                Debug.LogError("[CombatPageController] 沒有敵人可打。");
                return;
            }

            manager.BeginCombat(allyList, enemyList);
            running = true;
        }

        void Update()
        {
            if (!running || manager == null) return;
            if (!manager.finalOutcome.HasValue) return;

            var outcome = manager.finalOutcome.Value;
            switch (outcome)
            {
                case CombatOutcome.Win:    onWin?.Invoke();  break;
                case CombatOutcome.Lose:   onLose?.Invoke(); break;
                case CombatOutcome.Escape: onLose?.Invoke(); break; // 先歸類 Lose
            }

            ShowOnly(storyPanel);
            running = false;
            manager.finalOutcome = null;
            manager.isActiveCombat = false;
        }

        // ---------- 產生器 ----------

        void SpawnByCount(int count)
        {
            count = Mathf.Max(1, count);
            for (int i = 0; i < count; i++)
                SpawnOne(defaultEnemyPrefab, $"Enemy_{i+1}");
        }

        void SpawnByPrefabs(GameObject[] prefabs)
        {
            if (prefabs == null || prefabs.Length == 0) return;
            for (int i = 0; i < prefabs.Length; i++)
            {
                var p = prefabs[i] ? prefabs[i] : defaultEnemyPrefab;
                SpawnOne(p, p ? p.name : $"Enemy_{i+1}");
            }
        }

        /// 嘗試讀取 Encounter（常見欄位：list/slots/enemies；每格含 def/definition/enemy 或 prefab + count）
        void SpawnByEncounterAsset(ScriptableObject enc)
        {
            if (enc == null) return;

            var t = enc.GetType();
            FieldInfo listF = null;
            foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(f.FieldType) && f.FieldType != typeof(string))
                { listF = f; break; }
            }
            if (listF == null) { Debug.LogWarning("[CombatPageController] Encounter 沒有可枚舉欄位。"); return; }

            var listObj = listF.GetValue(enc) as System.Collections.IEnumerable;
            if (listObj == null) return;

            foreach (var slot in listObj)
            {
                if (slot == null) continue;
                var st = slot.GetType();

                // count
                int count = 1;
                var cf = st.GetField("count", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (cf != null && cf.FieldType == typeof(int)) count = Mathf.Max(1, (int)cf.GetValue(slot));

                // prefab or def
                GameObject prefab = null;
                var pf = st.GetField("prefab", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (pf != null && typeof(GameObject).IsAssignableFrom(pf.FieldType))
                    prefab = (GameObject)pf.GetValue(slot);

                object def = null;
                if (prefab == null)
                {
                    var df = st.GetField("def", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                          ?? st.GetField("definition", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                          ?? st.GetField("enemy", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (df != null) def = df.GetValue(slot);
                }

                for (int i = 0; i < count; i++)
                {
                    var go = SpawnOne(prefab, null);
                    if (def != null) TryApplyEnemyDefinition(go, def);
                }
            }
        }

        GameObject SpawnOne(GameObject prefab, string overrideName)
        {
            if (enemiesRoot == null) enemiesRoot = combatPanel ? combatPanel.transform : null;
            if (prefab == null) prefab = defaultEnemyPrefab;

            var go = Instantiate(prefab, enemiesRoot);
            if (!string.IsNullOrEmpty(overrideName)) go.name = overrideName;

            var c = go.GetComponent<Combatant>();
            if (c != null && string.IsNullOrEmpty(c.displayName)) c.displayName = go.name;
            go.tag = "Enemy";

            var btn = go.GetComponent<EnemyTargetButton>();
            if (btn != null) { btn.ui = ui; if (btn.enemy == null) btn.enemy = c; }

            spawned.Add(go);
            return go;
        }

        void TryApplyEnemyDefinition(GameObject go, object def)
        {
            if (go == null || def == null) return;
            var c = go.GetComponent<Combatant>();
            var inv = go.GetComponent<InventoryManager>();
            if (inv == null) return;

            var dt = def.GetType();

            // displayName
            var nameF = dt.GetField("displayName") ?? dt.GetField("name");
            if (nameF != null && c != null)
            {
                var n = nameF.GetValue(def) as string;
                if (!string.IsNullOrEmpty(n)) { c.displayName = n; go.name = n; }
            }

            // weapon / armor
            var wf = dt.GetField("weapon") ?? dt.GetField("weaponDef");
            if (wf != null && wf.GetValue(def) is WeaponDef w) inv.primaryWeapon = w;

            var af = dt.GetField("armor") ?? dt.GetField("armorDef");
            if (af != null && af.GetValue(def) is ArmorDef a) inv.armor = a;
        }

        List<Combatant> CollectEnemies()
        {
            var list = new List<Combatant>();
            if (enemiesRoot == null) return list;
            foreach (Transform t in enemiesRoot)
            {
                var c = t.GetComponent<Combatant>();
                if (c != null) list.Add(c);
            }
            return list;
        }

        void ClearSpawned()
        {
            foreach (var go in spawned) if (go) Destroy(go);
            spawned.Clear();
        }

        bool CheckFixedRefs()
        {
            if (combatPanel == null || storyPanel == null)
            { Debug.LogError("[CombatPageController] storyPanel/combatPanel 未設定"); return false; }

            if (manager == null) manager = combatPanel ? combatPanel.GetComponentInChildren<CombatManager>(true) : null;
            if (player == null)  player  = combatPanel ? combatPanel.GetComponentInChildren<Combatant>(true) : null;

            if (enemiesRoot == null)
            { Debug.LogError("[CombatPageController] 請把 enemiesRoot 指到 CombatPanel/Enemys"); return false; }

            if (defaultEnemyPrefab == null)
            { Debug.LogError("[CombatPageController] 請指定 defaultEnemyPrefab"); return false; }

            return true;
        }

        void ShowOnly(GameObject panel)
        {
            if (panel == null) return;
            Transform root = panelsRoot != null ? panelsRoot : panel.transform.parent;
            if (root != null) foreach (Transform t in root) t.gameObject.SetActive(false);
            panel.SetActive(true);
        }
        public void BackToStory(){ if (storyPanel) storyPanel.SetActive(true); if (combatPanel) combatPanel.SetActive(false); }
    }
}
