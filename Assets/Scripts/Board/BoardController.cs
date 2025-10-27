using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CyberLife.Board
{
    public class BoardController : MonoBehaviour
    {
        [Header("Roots")]
        public RectTransform tilesRoot;   // Canvas/BoardPanel/Tiles
        public RectTransform pawnsRoot;   // Canvas/BoardPanel/Pawns
        public RectTransform uiRoot;      // Canvas/BoardPanel/UI (可空，未設會自動尋找)

        [Header("Layout")]
        [Range(4, 32)] public int side = 8;     // 每邊格數（>=4）
        public float tileSize = 80f;
        public float gap = 6f;
        public Color tileColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        public Color tileAltColor = new Color(0.22f, 0.22f, 0.22f, 0.9f);

        [Header("Auto-Fit")]
        public bool autoFit = true;
        [Range(0f, 200f)] public float fitMargin = 24f;          // 內縮邊界
        [Range(0.3f, 1.2f)] public float fitCoverage = 0.92f;    // 佔最短邊比例（調大會放大棋盤）
        [Range(0.00f, 0.50f)] public float gapRatio = 0.12f;     // gap 相對 tileSize 的比例（autoFit 時使用）

        [Header("Auto-Size pawn & UI")]
        public bool autoSizePawn = true;
        [Range(0.3f, 1.0f)] public float pawnSizeRatio = 0.7f;   // pawn = tileSize * ratio
        public bool autoPlaceRoll = true;
        [Range(0.6f, 3.0f)] public float rollWidthInTiles = 2.0f; // RollButton 寬 = tileSize * N
        [Range(0.3f, 1.5f)] public float rollHeightInTiles = 0.6f;

        [Header("Runtime")]
        public List<RectTransform> tiles = new List<RectTransform>();

        public int Perimeter => Mathf.Max(0, side * 4 - 4);

        [ContextMenu("Generate Board")]
        public void Generate()
        {
            if (!tilesRoot) { Debug.LogError("[Board] tilesRoot is null"); return; }
            if (!pawnsRoot) { Debug.LogWarning("[Board] pawnsRoot is null"); }

            if (!uiRoot)
            {
                // 嘗試自動找 BoardPanel 下的 "UI"
                var p = tilesRoot.parent;
                if (p) { var t = p.Find("UI"); if (t) uiRoot = t as RectTransform; }
            }

            if (autoFit) FitToPanel();

            // 清除舊 tiles
            for (int i = tilesRoot.childCount - 1; i >= 0; i--)
            {
                var child = tilesRoot.GetChild(i).gameObject;
                if (Application.isEditor) GameObject.DestroyImmediate(child);
                else GameObject.Destroy(child);
            }
            tiles.Clear();

            if (side < 4) side = 4;

            int idx = 0;
            for (int x = 0; x < side; x++) CreateTile(idx++, Pos(x, 0));
            for (int y = 1; y < side - 1; y++) CreateTile(idx++, Pos(side - 1, y));
            for (int x = side - 1; x >= 0; x--) CreateTile(idx++, Pos(x, side - 1));
            for (int y = side - 2; y >= 1; y--) CreateTile(idx++, Pos(0, y));

            // 調整 pawn 與 UI
            if (autoSizePawn) ResizeAllPawns();
            if (autoPlaceRoll) ResizeAndPlaceRoll();
        }

        Vector2 Pos(int gx, int gy)
        {
            float step = tileSize + gap;
            float half = ((side - 1) * step + 0f) * 0.5f;
            return new Vector2((gx * step) - half, (gy * step) - half);
        }

        void CreateTile(int index, Vector2 anchoredPos)
        {
            var go = new GameObject($"Tile_{index}", typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(tilesRoot, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(tileSize, tileSize);
            rt.anchoredPosition = anchoredPos;

            var img = go.GetComponent<Image>();
            img.color = (index % 2 == 0) ? tileColor : tileAltColor;

            tiles.Add(rt);
        }

        public Vector2 GetTilePosition(int index)
        {
            if (tiles == null || tiles.Count == 0) return Vector2.zero;
            index = Mod(index, tiles.Count);
            return tiles[index].anchoredPosition;
        }

        public static int Mod(int a, int n) => (a % n + n) % n;

        [ContextMenu("Fit To Panel")]
        public void FitToPanel()
        {
            if (!tilesRoot) return;
            var rootPanel = tilesRoot.parent as RectTransform;
            if (!rootPanel) rootPanel = tilesRoot;

            var size = rootPanel.rect.size;
            float shortest = Mathf.Min(size.x, size.y) * fitCoverage - fitMargin * 2f;
            if (shortest < 10f) shortest = Mathf.Min(size.x, size.y) - fitMargin * 2f;

            // 以「整體寬度 = side * tileSize + (side-1) * gap」為基準解 tileSize
            float localGap = Mathf.Max(0f, gap);
            if (autoFit)
            {
                // 以比例推 gap
                localGap = gapRatio * Mathf.Max(8f, shortest / side);
            }
            tileSize = (shortest - (side - 1) * localGap) / side;
            if (tileSize < 8f) tileSize = 8f;
            gap = localGap;
        }

        void ResizeAllPawns()
        {
            if (!pawnsRoot) return;
            float s = Mathf.Max(8f, tileSize * pawnSizeRatio);
            for (int i = 0; i < pawnsRoot.childCount; i++)
            {
                var t = pawnsRoot.GetChild(i) as RectTransform;
                if (!t) continue;
                t.sizeDelta = new Vector2(s, s);
            }
        }

        void ResizeAndPlaceRoll()
        {
            if (!uiRoot) return;
            var t = uiRoot.Find("Roll") as RectTransform;
            if (!t) return;
            float w = Mathf.Max(60f, tileSize * rollWidthInTiles);
            float h = Mathf.Max(28f, tileSize * rollHeightInTiles);
            t.sizeDelta = new Vector2(w, h);
            // 置底中
            t.anchorMin = t.anchorMax = new Vector2(0.5f, 0f);
            t.pivot = new Vector2(0.5f, 0f);
            t.anchoredPosition = new Vector2(0f, 20f);
        }
    }
}
