// Namespace: Game.UI
// 把按鈕與 R 熱鍵綁到 DiceRollerUI.Roll()；找不到骰子就用後備隨機值直接推棋。
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    [RequireComponent(typeof(Button))]
    public class RollButtonBinder : MonoBehaviour
    {
        public KeyCode hotkey = KeyCode.R;
        public bool enableHotkey = true;
        public int fallbackMin = 1;
        public int fallbackMax = 6;

        Button _btn;
        DiceRollerUI _dice;
        Game.Board.PawnController _pawn;

        void Awake(){
            _btn = GetComponent<Button>();
            TryBindTargets();
            _btn.onClick.RemoveAllListeners();
            _btn.onClick.AddListener(CallRoll);
        }

        void OnEnable(){ TryBindTargets(); }

        void TryBindTargets(){
            if (!_dice){
                _dice = GetComponent<DiceRollerUI>();
                if (!_dice) _dice = FindObjectOfType<DiceRollerUI>(true);
            }
            if (!_pawn) _pawn = FindObjectOfType<Game.Board.PawnController>(true);
        }

        void Update(){
            if (!enableHotkey) return;
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy) return;
            if (Input.GetKeyDown(hotkey)) CallRoll();
        }

        void CallRoll(){
            if (_dice && !_dice.IsRolling){ _dice.Roll(); return; }
            if (_pawn){
                int v = Random.Range(fallbackMin, fallbackMax + 1);
                _pawn.MoveSteps(v);
            }
        }
    }
}
