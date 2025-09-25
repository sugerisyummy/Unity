using UnityEngine.SceneManagement;

namespace CyberLife.Combat
{
    public struct CombatReturn
    {
        public bool win, escaped;
        public int nextWin, nextLose, nextEscape;
        public string onWinFlag, onLoseFlag;
    }

    public static class CombatSceneLoader
    {
        public static CombatEncounter encounter;
        public static CombatReturn pending;
        public static System.Action<CombatReturn> onReturn;

        public static void Load(CombatEncounter enc, CombatReturn ret, System.Action<CombatReturn> cb)
        {
            encounter = enc;
            pending   = ret;
            onReturn  = cb;
            SceneManager.LoadSceneAsync("CombatScene", LoadSceneMode.Additive);
        }

        public static void EndAndReturn(CombatReturn r)
        {
            onReturn?.Invoke(r);
            SceneManager.UnloadSceneAsync("CombatScene");
            encounter = default;
            onReturn  = null;
        }
    }
}
