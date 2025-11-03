// Copyright (c) 2025 KG Tools
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using CyberLife.Board;

namespace KG.EditorTools
{
    public static class BoardAutoWire
    {
        [MenuItem("KG/Board/Auto Wire (Scene)")]
        public static void AutoWire()
        {
            var tiles = FindRect("Canvas/BoardPanel/Tiles");
            int wired = 0;
            foreach (var pc in Object.FindObjectsOfType<PawnController>(true))
            {
                Undo.RecordObject(pc, "Auto Wire PawnController");
                if (pc.board == null) pc.board = tiles;
                if (pc.pawn  == null) pc.pawn  = pc.GetComponent<RectTransform>();
                EditorUtility.SetDirty(pc);
                wired++;
            }
            Debug.Log($"[KG] Auto Wire 完成：PawnController {wired} 個；Tiles={(tiles?tiles.name:"<null>")}");
        }

        private static RectTransform FindRect(string path)
        {
            var go = GameObject.Find(path);
            return go ? go.GetComponent<RectTransform>() : null;
        }
    }
}
#endif
