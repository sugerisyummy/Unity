// Namespace: Game.UI
// 中間骰子 UI：跑數字 1 秒→定格。支援 Text 或 TMP_Text（用反射，不硬依賴 TMP）。
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Reflection;

namespace Game.UI
{
    [DisallowMultipleComponent]
    public class DiceRollerUI : MonoBehaviour
    {
        [Header("顯示文字 (自動尋找)")]
        public Component label; // Text 或 TMP_Text

        [Header("參數")]
        public int minValue = 1;
        public int maxValue = 6;
        public float rollDuration = 1.0f;
        public float tickInterval = 0.06f;

        [Header("結果事件")]
        public UnityEvent<int> onRollFinished;

        [Header("可選：自動推棋")]
        public Game.Board.PawnController pawnController;
        public bool autoMovePawn = true;

        public bool IsRolling { get; private set; }

        void Reset(){ AutoFind(); }
        void Awake(){ AutoFind(); }

        void AutoFind(){
            if (!label) label = FindLabelInChildren(transform);
            if (!pawnController) pawnController = FindObjectOfType<Game.Board.PawnController>();
        }

        Component FindLabelInChildren(Transform root){
            foreach (var c in root.GetComponentsInChildren<Component>(true)){
                if (c==null) continue;
                var n = c.GetType().Name;
                if (n=="TMP_Text" || n=="TextMeshProUGUI") return c;
            }
            return root.GetComponentInChildren<Text>(true);
        }

        public void Roll(){
            if (!gameObject.activeInHierarchy || IsRolling) return;
            StartCoroutine(CoRoll());
        }

        IEnumerator CoRoll(){
            IsRolling = true;
            float t=0f; int last=minValue;
            while (t < rollDuration){
                last = Random.Range(minValue, maxValue+1);
                SetLabel(last.ToString());
                yield return new WaitForSeconds(Mathf.Max(0.01f, tickInterval));
                t += Mathf.Max(0.01f, tickInterval);
            }
            int result = Random.Range(minValue, maxValue+1);
            SetLabel(result.ToString());

            onRollFinished?.Invoke(result);
            if (autoMovePawn && pawnController) pawnController.MoveSteps(result);
            IsRolling = false;
        }

        void SetLabel(string s){
            if (!label) return;
            var t = label.GetType();
            if (t == typeof(Text)){ ((Text)label).text = s; return; }
            var prop = t.GetProperty("text", BindingFlags.Instance|BindingFlags.Public);
            if (prop!=null && prop.CanWrite) prop.SetValue(label, s, null);
        }
    }
}
