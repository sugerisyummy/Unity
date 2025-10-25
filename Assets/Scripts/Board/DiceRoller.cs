using UnityEngine; using UnityEngine.Events;
namespace CyberLife.Board{ public class DiceRoller:MonoBehaviour{ [Range(2,20)] public int sides=6; public UnityEvent<int> onRolled;
  public int Roll(){ int v=Random.Range(1,sides+1); onRolled?.Invoke(v); return v; } } }