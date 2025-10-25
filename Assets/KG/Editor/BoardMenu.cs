#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CyberLife.Board;

public static class BoardMenu
{
    [MenuItem("KG/Board/Create Basic Board (UI) - Single")]
    public static void CreateBasicBoard()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (!canvas)
        {
            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }
        if (!Object.FindObjectOfType<EventSystem>())
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var panel = new GameObject("BoardPanel", typeof(RectTransform));
        var prt = panel.GetComponent<RectTransform>();
        prt.SetParent(canvas.transform, false);
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
        prt.offsetMin = prt.offsetMax = Vector2.zero;

        var tiles = new GameObject("Tiles", typeof(RectTransform));
        var tilesRT = tiles.GetComponent<RectTransform>();
        tilesRT.SetParent(prt, false);

        var pawns = new GameObject("Pawns", typeof(RectTransform));
        var pawnsRT = pawns.GetComponent<RectTransform>();
        pawnsRT.SetParent(prt, false);

        var board = panel.AddComponent<BoardController>();
        board.tilesRoot = tilesRT;
        board.pawnsRoot = pawnsRT;
        board.Generate();

        var pawnGO = new GameObject("Pawn", typeof(RectTransform), typeof(Image));
        var pawnRT = pawnGO.GetComponent<RectTransform>();
        pawnRT.SetParent(pawnsRT, false);
        pawnRT.sizeDelta = new Vector2(board.tileSize * 0.7f, board.tileSize * 0.7f);
        pawnGO.GetComponent<Image>().color = new Color(0.95f, 0.6f, 0.1f, 1f);
        pawnRT.anchoredPosition = board.GetTilePosition(0);

        var pawnCtrl = panel.AddComponent<PawnController>();
        pawnCtrl.board = board;
        pawnCtrl.pawn = pawnRT;

        var btnGO = new GameObject("Roll", typeof(RectTransform), typeof(Image), typeof(Button));
        var btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.SetParent(prt, false);
        btnRT.anchorMin = btnRT.anchorMax = new Vector2(0.5f, 0f);
        btnRT.pivot = new Vector2(0.5f, 0f);
        btnRT.sizeDelta = new Vector2(140, 42);
        btnRT.anchoredPosition = new Vector2(0, 20);
        btnGO.GetComponent<Image>().color = new Color(0.2f, 0.5f, 0.9f, 1f);
        btnGO.GetComponent<Button>().onClick.AddListener(pawnCtrl.RollAndMove);

        Selection.activeGameObject = panel;
        Debug.Log("Basic Board created: Canvas/BoardPanel");
    }
}
#endif
