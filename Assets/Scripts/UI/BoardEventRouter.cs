using UnityEngine;

namespace Game.UI
{
    public sealed class BoardEventRouter : MonoBehaviour
    {
        [SerializeField] private GameObject boardPanel;
        [SerializeField] private GameObject eventPanel;
        [SerializeField] private GameObject combatPanel;

        public void ShowBoard()  => Set(true, false, false);
        public void ShowEvent()  => Set(false, true, false);
        public void ShowCombat() => Set(false, false, true);

        public void OnCombatWin()    => ShowEvent();
        public void OnCombatLose()   => ShowEvent();
        public void OnCombatEscape() => ShowEvent();

        private void Set(bool board, bool ev, bool combat)
        {
            if (boardPanel)  boardPanel.SetActive(board);
            if (eventPanel)  eventPanel.SetActive(ev);
            if (combatPanel) combatPanel.SetActive(combat);
        }
    }
}
