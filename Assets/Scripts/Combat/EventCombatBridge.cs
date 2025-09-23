using UnityEngine;
using UnityEngine.SceneManagement;

namespace CyberLife.Combat
{
    /// <summary>
    /// 事件→進戰鬥的單一入口。可選加載 Additive 場景，然後叫 CombatManager.BeginCombat。
    /// </summary>
    public class EventCombatBridge : MonoBehaviour
    {
        [Header("Scene")]
        public string combatSceneName = "CombatScene";
        public bool loadAdditive = true;

        [Header("Refs")]
        public CombatManager manager;           // 若沒拖，會自動 Find
        public CombatResultRouter resultRouter; // 可選：把結果丟回事件系統

        public void StartCombat(Combatant[] allies, Combatant[] enemies)
        {
            // 需要時載入戰鬥場景
            if (loadAdditive && !string.IsNullOrEmpty(combatSceneName))
            {
                var sc = SceneManager.GetSceneByName(combatSceneName);
                if (!sc.isLoaded) SceneManager.LoadScene(combatSceneName, LoadSceneMode.Additive);
            }

            if (manager == null) manager = FindObjectOfType<CombatManager>(true);
            if (manager == null)
            {
                Debug.LogError("[EventCombatBridge] 找不到 CombatManager。請把它放在戰鬥場景或同場景中。");
                return;
            }

            manager.BeginCombat(allies, enemies);

            // 若有 Router，指向同一個 manager
            if (resultRouter != null) resultRouter.manager = manager;
        }
    }
}
