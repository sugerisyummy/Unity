// SaveData.cs  （移除 node，新增 currentCase）
using System;

[Serializable]
public class SaveData
{
    public string currentCase;  // 例如 "ForestEntrance"
    public int hp;
    public int difficulty;      // 0:Easy 1:Normal 2:Hard 3:Master
    public string saveTime;     // yyyy/MM/dd HH:mm
}
