using UnityEngine;

public class CombatResultHook : MonoBehaviour
{
    [Header("（可留空，自動尋找 Game.UI.BoardEventRouter）")]
    [SerializeField] private GameObject routerGO;

    void Reset()  => AutoFind();
    void Awake()  => AutoFind();

    void AutoFind()
    {
        if (routerGO) return;

        var t = System.Type.GetType("Game.UI.BoardEventRouter");
        if (t == null)
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType("Game.UI.BoardEventRouter");
                if (t != null) break;
            }
        }

        if (t != null)
        {
            var mb = (MonoBehaviour)FindObjectOfType(t);
            if (mb) routerGO = mb.gameObject;
        }
        else
        {
            Debug.LogWarning("CombatResultHook：找不到型別 Game.UI.BoardEventRouter。");
        }
    }

    // 給 CombatResultRouter 或按鈕綁這三個
    public void OnCombatWin()    => Call("OnCombatWin");
    public void OnCombatLose()   => Call("OnCombatLose");
    public void OnCombatEscape() => Call("OnCombatEscape");

    // 也提供直接切頁
    public void ShowEvent()      => Call("ShowEvent");
    public void ShowBoard()      => Call("ShowBoard");
    public void ShowCombat()     => Call("ShowCombat");

    void Call(string method)
    {
        if (!routerGO)
        {
            Debug.LogWarning($"CombatResultHook：router 未找到，呼叫 {method} 失敗。");
            return;
        }
        routerGO.SendMessage(method, SendMessageOptions.DontRequireReceiver);
    }
}