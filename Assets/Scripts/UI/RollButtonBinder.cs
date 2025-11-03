using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

namespace Game.UI
{
    [RequireComponent(typeof(Button))]
    public class RollButtonBinder : MonoBehaviour
    {
        public KeyCode hotkey = KeyCode.R;
        public bool enableHotkey = true;

        Button btn;
        Object target;
        MethodInfo method;
        static readonly string[] METHOD_NAMES = { "Roll", "RequestRoll", "RollOnce", "OnRollButtonPressed" };

        void Awake()
        {
            btn = GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(TriggerRoll);
            FindTarget();
        }

        void Update(){ if (enableHotkey && Input.GetKeyDown(hotkey)) TriggerRoll(); }

        void FindTarget()
        {
            (target, method) = FindControllerAndMethod("TurnManager")
                               ?? FindControllerAndMethod("PawnController")
                               ?? (null, null);
        }

        static (Object, MethodInfo)? FindControllerAndMethod(string shortTypeName)
        {
            foreach (var mb in GameObject.FindObjectsOfType<MonoBehaviour>(true))
            {
                var t = mb.GetType();
                if (t.Name == shortTypeName || (t.FullName != null && t.FullName.EndsWith("." + shortTypeName)))
                {
                    foreach (var n in METHOD_NAMES)
                    {
                        var m = t.GetMethod(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (m != null && m.GetParameters().Length == 0)
                            return (mb, m);
                    }
                }
            }
            return null;
        }

        public void TriggerRoll()
        {
            if (target != null && method != null) { method.Invoke(target, null); return; }
            Debug.LogWarning("RollButtonBinder：找不到 TurnManager/PawnController 的擲骰方法（Roll/RequestRoll/RollOnce/OnRollButtonPressed）。");
        }
    }
}
