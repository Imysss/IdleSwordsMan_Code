using static Define;

public struct DungeonStateChangedEvent { }

public struct DungeonClearedEvent
{
    public DungeonType type;
    public int level;

    public DungeonClearedEvent(DungeonType type, int level)
    {
        this.type = type;
        this.level = level;
    }
}
public struct DungeonFailedEvent
{

}

public struct DungeonBossWaveStartEvent
{
    public int currentBossIndex;

    public DungeonBossWaveStartEvent(int currentBossIndex)
    {
        this.currentBossIndex = currentBossIndex;
    }
}