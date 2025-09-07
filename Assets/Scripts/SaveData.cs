using System;

[Serializable]
public class SaveData
{
    public int node;
    public int hp;
    public int difficulty;      // 0:Easy 1:Normal 2:Hard 3:Master
    public string saveTime;     // yyyy/MM/dd HH:mm
}
