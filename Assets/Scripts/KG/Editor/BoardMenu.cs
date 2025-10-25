#if UNITY_EDITOR
using UnityEditor; using UnityEngine; using UnityEngine.UI; using UnityEngine.EventSystems; using CyberLife.Board;
public static class BoardMenu {
 [MenuItem("KG/Board/Create Basic Board (UI)")] public static void CreateBasicBoard(){
   Canvas canvas=Object.FindObjectOfType<Canvas>(); if(!canvas){ var go=new GameObject("Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
     canvas=go.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; var sc=go.GetComponent<CanvasScaler>();
     sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution=new Vector2(1920,1080); }
   if(!Object.FindObjectOfType<EventSystem>()) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
   var panel=new GameObject("BoardPanel",typeof(RectTransform)); var prt=panel.GetComponent<RectTransform>(); prt.SetParent(canvas.transform,false);
   prt.anchorMin=Vector2.zero; prt.anchorMax=Vector2.one; prt.offsetMin=prt.offsetMax=Vector2.zero;
   var tiles=new GameObject("Tiles",typeof(RectTransform)).GetComponent<RectTransform>(); tiles.SetParent(prt,false);
   var pawns=new GameObject("Pawns",typeof(RectTransform)).GetComponent<RectTransform>(); pawns.SetParent(prt,false);
   var board=panel.AddComponent<BoardController>(); board.tilesRoot=tiles; board.pawnsRoot=pawns; board.Generate();
   var pawnGO=new GameObject("Pawn",typeof(RectTransform),typeof(Image)); var pawnRT=pawnGO.GetComponent<RectTransform>();
   pawnRT.SetParent(pawns,false); pawnRT.sizeDelta=new Vector2(board.tileSize*.7f,board.tileSize*.7f); pawnGO.GetComponent<Image>().color=new Color(.95f,.6f,.1f,1f);
   pawnRT.anchoredPosition=board.GetTilePosition(0);
   var pawnCtrl=panel.AddComponent<CyberLife.Board.PawnController>(); pawnCtrl.board=board; pawnCtrl.pawn=pawnRT;
   var btnGO=new GameObject("Roll",typeof(RectTransform),typeof(Image),typeof(Button)); var btnRT=btnGO.GetComponent<RectTransform>(); btnRT.SetParent(prt,false);
   btnRT.anchorMin=btnRT.anchorMax=new Vector2(.5f,0f); btnRT.pivot=new Vector2(.5f,0f); btnRT.sizeDelta=new Vector2(140,42); btnRT.anchoredPosition=new Vector2(0,20);
   btnGO.GetComponent<Image>().color=new Color(.2f,.5f,.9f,1f); btnGO.GetComponent<Button>().onClick.AddListener(pawnCtrl.RollAndMove);
   Selection.activeGameObject=panel; Debug.Log("Basic Board created: Canvas/BoardPanel");
 } }
#endif