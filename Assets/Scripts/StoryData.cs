using UnityEngine;

[System.Serializable]
public class Choice
{
    public string choiceText;
    public int nextNode;
    public int hpChange;

    // 🔽 新增條件
    public int requireMinHP;   // 選項需要的最小 HP
    public int requireMaxHP;   // (可選) 選項需要的最大 HP
}


[System.Serializable]
public class StoryNode
{
    [TextArea(3, 10)]
    public string text;
    public Choice[] choices;
}
