using UnityEngine; using CyberLife.Board; using CyberLife.Combat;
namespace CyberLife.Bridges{
  public class BoardCombatBridge:MonoBehaviour{
    public BoardEventsBridge board; public CombatPageController combatPage; public CombatEventBridge combatBridge;
    [Range(1,6)] public int enemiesPerLevel=2;
    void OnEnable(){ if(board) board.onRequestCombat.AddListener(StartCombatFromBoard); }
    void OnDisable(){ if(board) board.onRequestCombat.RemoveListener(StartCombatFromBoard); }
    public void StartCombatFromBoard(int level){ if(!combatPage){ Debug.LogError("[Board→Combat] combatPage is null."); return; }
      int count=Mathf.Clamp(level*enemiesPerLevel,1,6); combatPage.StartCombatWithCount(count); Debug.Log($"[Board→Combat] level {level} → {count} enemies"); }
  } }