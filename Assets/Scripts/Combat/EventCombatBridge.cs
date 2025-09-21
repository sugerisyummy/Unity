using System;
using System.Reflection;
using UnityEngine;

namespace CL.Combat
{
    public class EventCombatBridge : MonoBehaviour
    {
        [Header("切場景")]
        public bool useAdditiveScene = true;
        public string combatSceneName = "CombatScene";

        public void StartCombatFromEventChoice(DolEventAsset.EventChoice choice)
        {
            if (choice == null || !choice.startsCombat || choice.combat == null)
            {
                Debug.LogWarning("[EventCombatBridge] choice 無效或未勾 startsCombat");
                return;
            }

            var ret = new CombatReturn {
                nextWin = choice.nextStageOnWin,
                nextLose = choice.nextStageOnLose,
                nextEscape = choice.nextStageOnEscape,
                onWinFlag = choice.onWinFlag,
                onLoseFlag = choice.onLoseFlag
            };

            if (useAdditiveScene)
            {
                CombatSceneLoader.StartCombat(choice.combat, ret, OnReturnToEvent);
            }
            else
            {
                if (CombatManager.Instance == null)
                    new GameObject("CombatManager").AddComponent<CombatManager>();
                CombatManager.Instance.Begin(choice.combat, 100, 100, 12, 3, 6);
                CombatManager.Instance.OnCombatEnded += () =>
                {
                    var r = new CombatReturn { win = CombatManager.Instance.LastWin, escaped = CombatManager.Instance.LastEscaped };
                    OnReturnToEvent(r);
                };
            }
        }

        void OnReturnToEvent(CombatReturn r)
        {
            var gm = GameObject.FindObjectOfType<GameManager>();
            if (gm == null) return;

            try
            {
                var gmType = gm.GetType();
                // 旗標
                var flagField = gmType.GetField("flagStorage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var flagObj = flagField != null ? flagField.GetValue(gm) : null;
                var setFlag = flagObj?.GetType().GetMethod("SetFlag", BindingFlags.Instance | BindingFlags.Public);

                if (setFlag != null)
                {
                    if (r.win && !string.IsNullOrEmpty(r.onWinFlag)) setFlag.Invoke(flagObj, new object[] { r.onWinFlag });
                    if (!r.win && !r.escaped && !string.IsNullOrEmpty(r.onLoseFlag)) setFlag.Invoke(flagObj, new object[] { r.onLoseFlag });
                }

                int next = -1;
                if (r.win && r.nextWin >= 0) next = r.nextWin;
                else if (r.escaped && r.nextEscape >= 0) next = r.nextEscape;
                else if (!r.win && !r.escaped && r.nextLose >= 0) next = r.nextLose;

                if (next >= 0)
                {
                    var runStageField = gmType.GetField("runningStage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (runStageField != null) runStageField.SetValue(gm, next);
                    var showStage = gmType.GetMethod("ShowStage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (showStage != null) showStage.Invoke(gm, null);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EventCombatBridge] 回返事件失敗：{ex.Message}");
            }
        }
    }
}
