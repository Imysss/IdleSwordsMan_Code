using System.Numerics;

public struct GoldChangeEvent
{
    private BigInteger _currentGold;

    public GoldChangeEvent(BigInteger currentGold)
    {
        _currentGold = currentGold;
    }
}

public struct GemChangeEvent
{
    private int _currentGem;

    public GemChangeEvent(int currentGem)
    {
        _currentGem = currentGem;
    }
}

public struct NameChangedEvent { }

public struct DungeonTicketChangedEvent { }