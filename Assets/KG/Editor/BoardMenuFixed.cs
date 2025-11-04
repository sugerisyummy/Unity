// KG Editor: BoardMenuFixed（全新類名 & 選單，繞開舊檔衝突）
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Board;

public static class BoardMenuFixed
{
    [MenuItem("KG/Board/Create Basic Board (UI) [Fixed]")]
    public static void CreateBasicBoardFixed()
    {
        // Canvas + EventSystem
        var canvas = Object.FindObjectOfType<Canvas>();
        if (!canvas)
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var sc = go.GetComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1920, 1080);
        }
        if (!Object.FindObjectOfType<EventSystem>())
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        // BoardPanel + roots
        var panel = new GameObject("BoardPanel", typeof(RectTransform));
        var prt = panel.GetComponent<RectTransform>();
        prt.SetParent(canvas.transform, false);
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one; prt.offsetMin = prt.offsetMax = Vector2.zero;

        var tiles = new GameObject("Tiles", typeof(RectTransform)).GetComponent<RectTransform>();
        tiles.SetParent(prt, false);
        var pawns = new GameObject("Pawns", typeof(RectTransform)).GetComponent<RectTransform>();
        pawns.SetParent(prt, false);
        var ui = new GameObject("UI", typeof(RectTransform)).GetComponent<RectTransform>();
        ui.SetParent(prt, false);

        // BoardController
        var bc = panel.AddComponent<BoardController>();
        bc.tilesRoot = tiles;
        bc.pawnsRoot = pawns;
        bc.Generate();

        // Pawn
        var pawnGO = new GameObject("Pawn", typeof(RectTransform), typeof(Image));
        var pawnRT = pawnGO.GetComponent<RectTransform>();
        pawnRT.SetParent(pawns, false);
        pawnRT.sizeDelta = new Vector2(bc.tileSize * .7f, bc.tileSize * .7f);
        pawnRT.anchoredPosition = bc.GetTilePosition(0);
        pawnGO.GetComponent<Image>().color = new Color(.95f, .6f, .1f, 1f);

        // PawnController（RectTransform 對應）
        var pawnCtrl = pawnGO.AddComponent<Game.Board.PawnController>();
        pawnCtrl.board = tiles;     // RectTransform
        pawnCtrl.pawn  = pawnRT;    // RectTransform

        // Roll 按鈕
        var btnGO = new GameObject("Roll", typeof(RectTransform), typeof(Image), typeof(Button));
        var btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.SetParent(ui, false);
        btnRT.anchorMin = btnRT.anchorMax = new Vector2(.5f, 0f);
        btnRT.pivot = new Vector2(.5f, 0f);
        btnRT.sizeDelta = new Vector2(140, 42);
        btnRT.anchoredPosition = new Vector2(0, 20);
        btnGO.GetComponent<Image>().color = new Color(.2f, .5f, .9f, 1f);
        btnGO.GetComponent<Button>().onClick.AddListener(pawnCtrl.RollAndMove);

        Selection.activeGameObject = panel;
        Debug.Log("[KG] Basic Board (Fixed) 建立完成。");
    }
}
#endif
