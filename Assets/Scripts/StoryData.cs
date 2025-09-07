using UnityEngine;

[System.Serializable]
public class Choice
{
    public string choiceText;
    public int nextNode;
    public int hpChange;
}

[System.Serializable]
public class StoryNode
{
    [TextArea(3, 10)]
    public string text;
    public Choice[] choices;
}

