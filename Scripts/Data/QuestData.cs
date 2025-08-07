using System.Collections.Generic;
using System;

[Serializable]
public class QuestData
{
    public int key;
    public string name;
    public QuestType type;
    public int conditionCount;
    public List<int> nextKeys;
}
