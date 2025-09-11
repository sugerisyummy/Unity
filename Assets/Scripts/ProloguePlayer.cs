using UnityEngine;

public class ProloguePlayer : MonoBehaviour
{
    [TextArea(3, 10)] public string prologueText; // 若用影片/動畫可替換
    public TMPro.TextMeshProUGUI textUI;

    void OnEnable()
    {
        if (textUI) textUI.text = prologueText;
        // 可在這裡啟動打字機/Timeline/Animator
    }

    public void OnContinue()
    {
        if (MenuManager.Instance) MenuManager.Instance.GoToDifficulty();
    }

    public void OnSkip()
    {
        OnContinue();
    }
}
