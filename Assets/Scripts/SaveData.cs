// SaveData.cs
using System;

[Serializable]
public class SaveData
{
    public string currentCase;  // 例如 "ForestEntrance"

    // 基礎
    public int hp;

    // 反烏托邦擴充數值
    public int hunger;
    public int thirst;
    public int fatigue;

    public int hope;
    public int obedience;
    public int reputation;        // 通緝 / 名聲（依你定義）

    public int techParts;
    public int information;
    public int credits;

    public int augmentationLoad;  // 義體負荷
    public int radiation;         // 或改為 infection 皆可

    // 其他
    public int difficulty;        // 0:Easy 1:Normal 2:Hard 3:Master
    public string saveTime;       // yyyy/MM/dd HH:mm
}
