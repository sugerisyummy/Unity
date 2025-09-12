// SaveSlotButton.cs — 顯示核心摘要：Case、HP、Hunger/Hope/Credits
using UnityEngine;
using TMPro;

public class SaveSlotButton : MonoBehaviour
{
    public enum Mode { Save, Load, Delete }
    public Mode mode = Mode.Load;

    [Range(0, 3)] public int slot = 1;    // 0=自動 1..3=手動
    public GameManager gameManager;       // 拖場景中的 GameManager
    public TextMeshProUGUI label;         // 顯示資訊的文字

    void OnEnable() { RefreshLabel(); }

    public void OnClick()
    {
        if (!gameManager) return;

        switch (mode)
        {
            case Mode.Save:   gameManager.SaveToSlot(slot); break;
            case Mode.Load:   gameManager.LoadFromSlot(slot); break;
            case Mode.Delete: SaveManager.Delete(slot); break;
        }
        RefreshLabel();
    }

    void RefreshLabel()
    {
        if (!label) return;

        if (!SaveManager.Has(slot))
        {
            label.text = "空槽";
            return;
        }

        var d = SaveManager.Load(slot);
        if (d == null) { label.text = "空槽"; return; }

        string caseName = string.IsNullOrEmpty(d.currentCase) ? "—" : d.currentCase;

        // 精簡摘要，避免過長：顯示 HP / Hunger / Hope / Credits
        label.text =
            $"{d.saveTime}\n" +
            $"{DiffName(d.difficulty)} · Case {caseName}\n" +
            $"HP {d.hp} | Hg {d.hunger} | Hope {d.hope} | Cr {d.credits}";
    }

    string DiffName(int idx)
    {
        switch (Mathf.Clamp(idx, 0, 3))
        {
            case 0: return "Easy";
            case 1: return "Normal";
            case 2: return "Hard";
            case 3: return "Master";
            default: return "Normal";
        }
    }
}
