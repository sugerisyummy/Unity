// KG Editor: BoardMenu 修正版（RectTransform 對齊 + PawnController 掛在 Pawn 上）
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Board;

public static class BoardMenu
{
    [MenuItem("KG/Board/Create Basic Board (UI)")]
    public static void CreateBasicBoard()
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

        // BoardPanel
        var panel = new GameObject("BoardPanel", typeof(RectTransform));
        var prt = panel.GetComponent<RectTransform>();
        prt.SetParent(canvas.transform, false);
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one; prt.offsetMin = prt.offsetMax = Vector2.zero;

        // Roots
        var tiles = new GameObject("Tiles", typeof(RectTransform)).GetComponent<RectTransform>();
        tiles.SetParent(prt, false);
        var pawns = new GameObject("Pawns", typeof(RectTransform)).GetComponent<RectTransform>();
        pawns.SetParent(prt, false);
        var ui = new GameObject("UI", typeof(RectTransform)).GetComponent<RectTransform>();
        ui.SetParent(prt, false);

        // BoardController
        var board = panel.AddComponent<BoardController>();
        board.tilesRoot = tiles;
        board.pawnsRoot = pawns;
        board.Generate();

        // Pawn（Image）
        var pawnGO = new GameObject("Pawn", typeof(RectTransform), typeof(Image));
        var pawnRT = pawnGO.GetComponent<RectTransform>();
        pawnRT.SetParent(pawns, false);
        pawnRT.sizeDelta = new Vector2(board.tileSize * .7f, board.tileSize * .7f);
        pawnGO.GetComponent<Image>().color = new Color(.95f, .6f, .1f, 1f);
        pawnRT.anchoredPosition = board.GetTilePosition(0);

        // PawnController 掛在 Pawn 上；指向 Tiles / 自身 RectTransform
        var pawnCtrl = pawnGO.AddComponent<Game.Board.PawnController>();
        pawnCtrl.tilesRoot = tiles;
        pawnCtrl.pawn = pawnRT;

        // Roll 按鈕 + 文字 + Dice UI + Binder
        var btnGO = new GameObject("Roll", typeof(RectTransform), typeof(Image), typeof(Button));
        var btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.SetParent(ui, false);
        btnRT.anchorMin = btnRT.anchorMax = new Vector2(.5f, 0f);
        btnRT.pivot = new Vector2(.5f, 0f);
        btnRT.sizeDelta = new Vector2(140, 42);
        btnRT.anchoredPosition = new Vector2(0, 20);
        btnGO.GetComponent<Image>().color = new Color(.2f, .5f, .9f, 1f);

        // Label（顯示骰子結果）
        var labelGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
        var labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.SetParent(btnRT, false);
        labelRT.anchorMin = labelRT.anchorMax = new Vector2(.5f, .5f);
        labelRT.pivot = new Vector2(.5f, .5f);
        labelRT.sizeDelta = Vector2.zero;
        var text = labelGO.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = "Roll";
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        // DiceRoller UI + Binder
        var diceUI = btnGO.AddComponent<Game.UI.DiceRollerUI>();
        diceUI.pawnController = pawnCtrl;
        diceUI.autoMovePawn = true;

        var binder = btnGO.AddComponent<Game.UI.RollButtonBinder>();
        binder.enableHotkey = true;
        binder.hotkey = KeyCode.R;

        Selection.activeGameObject = panel;
        Debug.Log("Basic Board created: Canvas/BoardPanel");
    }
}
#endif
