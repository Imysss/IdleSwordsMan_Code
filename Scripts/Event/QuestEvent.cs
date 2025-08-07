using Data;
using static Define;

// 스테이지 클리어 이벤트
public struct StageClearedEvent
{
    public int stageKey;

    public StageClearedEvent(int stageKey)
    {
        this.stageKey = stageKey;
    }
}

// 적 처치 이벤트
public struct EnemyDiedEvent
{
    public int enemyKey;

    public EnemyDiedEvent(int enemyKey)
    {
        this.enemyKey = enemyKey;
    }
}

// 아이템 획득 이벤트
public struct GachaEvent
{
    public GachaType gachaType;
    public int gachaCount;

    public GachaEvent(GachaType type, int count)
    {
        gachaType = type;
        gachaCount = count;
    }
}

public struct StatUpgradeEvent
{
    public Define.StatType statType;
    public int currentLevel;

    public StatUpgradeEvent(Define.StatType statType, int level)
    {
        this.statType = statType;
        currentLevel = level;
    }
}

// 퀘스트 클리어 이벤트
public struct QuestClearedEvent
{
    public QuestData questData;

    public QuestClearedEvent(QuestData questData)
    {
        this.questData = questData;
    }
}

// 광고 시청 이벤트
public struct AdWatchedEvent{}

public struct QuestStateChangedEvent { }