using UnityEngine;

public static class SaveManager
{
    public const int AUTO_SLOT = 0;     // 自動存檔槽
    public const int MAX_SLOTS = 3;     // 手動槽 1..3

    static string Key(int slot) => $"SaveSlot_{slot}";

    public static void Save(int slot, SaveData data)
    {
        var json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(Key(slot), json);
        PlayerPrefs.Save();
    }

    public static SaveData Load(int slot)
    {
        if (!Has(slot)) return null;
        var json = PlayerPrefs.GetString(Key(slot));
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static bool Has(int slot) => PlayerPrefs.HasKey(Key(slot));

    public static void Delete(int slot)
    {
        PlayerPrefs.DeleteKey(Key(slot));
        PlayerPrefs.Save();
    }
}
