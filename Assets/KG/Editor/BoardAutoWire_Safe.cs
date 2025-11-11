#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using Game.Board;
using Game.UI;

public static class BoardAutoWire_Safe {
  [MenuItem("KG/Board/Auto Wire (Safe)")]
  public static void AutoWire(){
    var canvas = GameObject.Find("Canvas");
    var board  = canvas ? canvas.transform.Find("BoardPanel") : null;
    var tiles  = board ? board.Find("Tiles") : null;
    var pawnsR = board ? board.Find("Pawns") : null;
    var ui     = board ? board.Find("UI") : null;
    var pawnGo = pawnsR ? pawnsR.Find("Pawn")?.gameObject : null;
    var rollBtnGo = ui ? ui.Find("Roll")?.gameObject : null; // 你的中間按鈕名稱若不同，手動改
    var diceGo = ui ? ui.Find("Dice")?.gameObject : null;    // 若已存在 Dice 物件就用它

    string miss = "";
    if (!canvas) miss += "Canvas\n";
    if (!board)  miss += "BoardPanel\n";
    if (!tiles)  miss += "BoardPanel/Tiles\n";
    if (!pawnsR) miss += "BoardPanel/Pawns\n";
    if (!pawnGo) miss += "BoardPanel/Pawns/Pawn\n";
    if (!ui)     miss += "BoardPanel/UI\n";
    if (!rollBtnGo) miss += "BoardPanel/UI/Roll (按鈕)\n";

    if (miss != ""){
      EditorUtility.DisplayDialog("缺少必要物件（不自動新增）", "請先手動建立/命名以下路徑：\n\n"+miss, "OK");
      return;
    }

    // PawnController
    var pawn = pawnGo.GetComponent<RectTransform>();
    var pc = pawnGo.GetComponent<PawnController>() ?? Undo.AddComponent<PawnController>(pawnGo);
    Undo.RecordObject(pc, "Auto Wire PawnController");
    pc.tilesRoot = tiles.GetComponent<RectTransform>();
    pc.pawn      = pawn;

    // DiceRollerUI（若 Dice 物件不存在，嘗試從 Roll 按鈕抓顯示文字）
    GameObject diceHost = diceGo ? diceGo : rollBtnGo;
    var dice = diceHost.GetComponent<DiceRollerUI>() ?? Undo.AddComponent<DiceRollerUI>(diceHost);
    // 嘗試抓子階的 Text / TMP_Text
    if (!dice.label){
      var txt = diceHost.GetComponentInChildren<Text>(true);
      if (txt) dice.label = txt;
      else {
        // 搜尋 TMP_Text by name 以免硬依賴
        foreach (var c in diceHost.GetComponentsInChildren<Component>(true)){
          if (c && (c.GetType().Name=="TMP_Text" || c.GetType().Name=="TextMeshProUGUI")) { dice.label = c; break; }
        }
      }
    }
    dice.pawnController = pc;

    // Roll 按鈕 binder + R 熱鍵
    var btn = rollBtnGo.GetComponent<Button>();
    var binder = rollBtnGo.GetComponent<RollButtonBinder>() ?? Undo.AddComponent<RollButtonBinder>(rollBtnGo);
    binder.enableHotkey = true;
    binder.hotkey = KeyCode.R;

    // 事件：dice.onRollFinished -> pc.MoveSteps
    bool alreadyLinked = false;
    var ev = dice.onRollFinished;
    var count = ev.GetPersistentEventCount();
    for (int i=0;i<count;i++){
      if (ev.GetPersistentTarget(i) == pc && ev.GetPersistentMethodName(i)=="MoveSteps"){ alreadyLinked = true; break; }
    }
    if (!alreadyLinked){
      UnityAction<int> act = pc.MoveSteps;
      UnityEventTools.AddIntPersistentListener(ev, act);
      EditorUtility.SetDirty(dice);
    }

    EditorUtility.DisplayDialog("完成", "接線完成（未新增任何頁面/物件）。\n- PawnController 已指向 Tiles/Pawn\n- Roll 按鈕可用 R 觸發\n- Dice UI 已綁定並會推動棋子\n", "OK");
  }
}
#endif
