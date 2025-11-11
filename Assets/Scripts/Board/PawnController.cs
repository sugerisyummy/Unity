// Namespace: Game.Board
using UnityEngine;
using System.Collections;

namespace Game.Board {
  [DisallowMultipleComponent]
  public class PawnController : MonoBehaviour {
    [Header("參照")]
    public RectTransform tilesRoot;   // BoardPanel/Tiles
    public RectTransform pawn;        // BoardPanel/Pawns/Pawn

    [Header("索引 (0-based；0=第一格)")]
    public int startIndex = 0;
    [SerializeField] int currentIndex = 0;

    [Header("移動參數")]
    public float perTileDuration = 0.15f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0,0,1,1);

    public bool IsMoving { get; private set; }
    void Reset(){ AutoFill(); }
    void Awake(){ AutoFill(); }
    void OnEnable(){ currentIndex = Mod(currentIndex, TileCount); SnapToCurrentIndex(); }

    void AutoFill(){
      if (!tilesRoot){ var t = GameObject.Find("Tiles"); if (t) tilesRoot = t.transform as RectTransform; }
      if (!pawn){ var p = GameObject.Find("Pawn"); if (p) pawn = p.transform as RectTransform; }
    }

    int TileCount => (tilesRoot ? Mathf.Max(tilesRoot.childCount,1) : 1);
    int LastIndex => TileCount - 1;
    static int Mod(int a,int m){ a%=m; return a<0?a+m:a; }

    RectTransform GetTile(int i){
      if (!tilesRoot || tilesRoot.childCount==0) return null;
      i = Mod(i, TileCount);
      return tilesRoot.GetChild(i) as RectTransform;
    }

    Vector2 TileToPawnLocal(RectTransform tile){
      if (!tile || !pawn) return Vector2.zero;
      var parent = pawn.parent as RectTransform;
      var world = tile.TransformPoint(tile.rect.center);
      return (Vector2)parent.InverseTransformPoint(world);
    }

    public void SnapToCurrentIndex(){
      var tile = GetTile(currentIndex); if (!tile || !pawn) return;
      pawn.anchoredPosition = TileToPawnLocal(tile);
    }

    public void MoveToIndex(int index){
      int steps = Mod(index, TileCount) - Mod(currentIndex, TileCount);
      if (steps <= 0) steps += TileCount; // 只往前走
      StartCoroutine(CoMoveSteps(steps));
    }

    public void MoveSteps(int steps){
      if (steps <= 0) return;
      StartCoroutine(CoMoveSteps(steps));
    }

    IEnumerator CoMoveSteps(int steps){
      if (!isActiveAndEnabled || !gameObject.activeInHierarchy) yield break;
      if (IsMoving) yield break;
      IsMoving = true;

      int remain = steps;
      while (remain-- > 0){
        int next = Mod(currentIndex + 1, TileCount);
        var from = GetTile(currentIndex);
        var to   = GetTile(next);
        if (!from || !to) break;

        Vector2 a = TileToPawnLocal(from);
        Vector2 b = TileToPawnLocal(to);
        float t = 0f;
        while (t < 1f){
          t += Time.deltaTime / Mathf.Max(0.01f, perTileDuration);
          float k = curve.Evaluate(Mathf.Clamp01(t));
          pawn.anchoredPosition = Vector2.LerpUnclamped(a, b, k);
          yield return null;
        }
        pawn.anchoredPosition = b;
        currentIndex = next;
      }
      IsMoving = false;
    }

    public int GetCurrentIndex()=>Mod(currentIndex,TileCount);
    public int GetMaxIndex()=>LastIndex;
  }
}
