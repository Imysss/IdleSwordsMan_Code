using System;

[Serializable]
public class QuestState
{
    public int questKey;
    public QuestStatus status = QuestStatus.Inactive;
    public int currentCount = 0;
}


