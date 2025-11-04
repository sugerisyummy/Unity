using UnityEngine;
using PawnController = Game.Board.PawnController;

namespace Game.Board
{
    public sealed class TurnManager : MonoBehaviour
    {
        [SerializeField] private PawnController playerPawn;
        [SerializeField] private DiceRoller dice;

        public void RollAndMove()
        {
            if (!playerPawn || !dice)
            {
                return;
            }

            if (playerPawn.IsMoving)
            {
                return;
            }

            playerPawn.RollAndMove();
        }
    }
}
