#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CyberLife.Board;

namespace CyberLife.EditorTools
{
    public static class BoardAutoWireSimple
    {
        [MenuItem("KG/Board/Auto Wire (Simple)")]
        public static void AutoWire()
        {
            // Canvas
            Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            if (!canvas)
            {
                var canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasGO.AddComponent<GraphicRaycaster>();
            }
            // EventSystem
            if (!UnityEngine.Object.FindObjectOfType<EventSystem>())
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            // BoardPanel
            RectTransform boardPanel = FindOrCreateUnder(canvas.transform, "BoardPanel");
            RectTransform tiles      = FindOrCreateUnder(boardPanel, "Tiles");
            RectTransform pawns      = FindOrCreateUnder(boardPanel, "Pawns");
            RectTransform uiRoot     = FindOrCreateUnder(boardPanel, "UI");

            // BoardController
            var board = boardPanel.gameObject.GetComponent<BoardController>();
            if (!board) board = boardPanel.gameObject.AddComponent<BoardController>();
            board.tilesRoot = tiles;
            board.pawnsRoot = pawns;
            if (board.tiles == null || board.tiles.Count == 0) board.Generate();

            // Pawn
            RectTransform pawnRT = EnsurePawn(pawns, board);

            // PawnController
            var pawnCtrl = boardPanel.GetComponent<PawnController>();
            if (!pawnCtrl) pawnCtrl = boardPanel.gameObject.AddComponent<PawnController>();
            pawnCtrl.board = board;
            pawnCtrl.pawn  = pawnRT;

            // TurnManager
            var turn = boardPanel.GetComponent<TurnManager>();
            if (!turn) turn = boardPanel.gameObject.AddComponent<TurnManager>();
            turn.playerPawn = pawnCtrl;

            // BoardEventsBridge
            var bridge = boardPanel.GetComponent<BoardEventsBridge>();
            if (!bridge) bridge = boardPanel.gameObject.AddComponent<BoardEventsBridge>();
            bridge.board = board;
            bridge.pawn  = pawnCtrl;

            // Roll button
            var rollBtn = EnsureRollButton(uiRoot, out Button btn);
            if (btn != null)
            {
                btn.onClick = new Button.ButtonClickedEvent();
                btn.onClick.AddListener(turn.RollAndMove);
            }

            // Optional: Board → Combat bridge
            var combatBridgeType = System.Type.GetType("CyberLife.Bridges.BoardCombatBridge, Assembly-CSharp");
            if (combatBridgeType != null)
            {
                GameObject go = GameObject.Find("Bridges");
                if (!go) go = new GameObject("Bridges");
                var comp = go.GetComponent(combatBridgeType);
                if (!comp) comp = go.AddComponent(combatBridgeType);
                var fi = combatBridgeType.GetField("board");
                if (fi != null) fi.SetValue(comp, bridge);
            }

            Selection.activeObject = boardPanel.gameObject;
            Debug.Log("[KG] Auto Wire (Simple) 完成。");
        }

        static RectTransform FindOrCreateUnder(Transform parent, string name)
        {
            var t = parent.Find(name);
            if (t) return t as RectTransform;
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            if (name == "BoardPanel")
            {
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;
            }
            else
            {
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            }
            return rt;
        }

        static RectTransform EnsurePawn(RectTransform pawnsRoot, BoardController board)
        {
            var t = pawnsRoot.Find("Pawn") as RectTransform;
            if (!t)
            {
                var go = new GameObject("Pawn");
                t = go.AddComponent<RectTransform>();
                t.SetParent(pawnsRoot, false);
                t.sizeDelta = new Vector2(board ? board.tileSize * 0.7f : 48f, board ? board.tileSize * 0.7f : 48f);
                var img = go.AddComponent<Image>();
                img.color = new Color(0.95f, 0.6f, 0.1f, 1f);
            }
            if (board && board.tiles != null && board.tiles.Count > 0)
                t.anchoredPosition = board.GetTilePosition(0);
            return t;
        }

        static GameObject EnsureRollButton(RectTransform uiRoot, out Button button)
        {
            button = null;
            var t = uiRoot.Find("Roll");
            if (!t)
            {
                var go = new GameObject("Roll");
                var rt = go.AddComponent<RectTransform>();
                rt.SetParent(uiRoot, false);
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.sizeDelta = new Vector2(140, 42);
                rt.anchoredPosition = new Vector2(0, 20);
                var img = go.AddComponent<Image>();
                img.color = new Color(0.2f, 0.5f, 0.9f, 1f);
                button = go.AddComponent<Button>();
                return go;
            }
            button = t.GetComponent<Button>();
            return t.gameObject;
        }
    }
}
#endif
