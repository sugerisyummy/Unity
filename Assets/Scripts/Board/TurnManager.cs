using UnityEngine; namespace CyberLife.Board{ public class TurnManager:MonoBehaviour{
  public PawnController playerPawn; public DiceRoller dice;
  public void RollAndMove(){ if(!playerPawn||!dice) return; if(playerPawn.IsMoving) return; playerPawn.RollAndMove(); } } }