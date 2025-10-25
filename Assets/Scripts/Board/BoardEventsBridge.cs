using UnityEngine; using UnityEngine.Events;
namespace CyberLife.Board{ public class BoardEventsBridge:MonoBehaviour{
  public BoardController board; public PawnController pawn;
  public UnityEvent<string> onRequestEvent; public UnityEvent<int> onRequestCombat; public UnityEvent<int> onMoneyDelta;
  void Awake(){ if(!pawn) pawn=FindObjectOfType<PawnController>(); if(pawn) pawn.onLanded.AddListener(HandleLanded); }
  void OnDestroy(){ if(pawn) pawn.onLanded.RemoveListener(HandleLanded); }
  void HandleLanded(int i){ if(i%10==0) onMoneyDelta?.Invoke(+100); else if(i%5==0) onRequestCombat?.Invoke(1); else if(i%2==0) onRequestEvent?.Invoke($"tile:{i}"); } } }