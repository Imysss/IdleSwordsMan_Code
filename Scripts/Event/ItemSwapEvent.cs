using Data;

public struct PartySwapStartEvent
{
    public PartyState Data;

    public PartySwapStartEvent(PartyState data)
    { 
        Data = data;
    }
}

public struct PartySwapEndEvent
{
    public PartyState Data;

    public PartySwapEndEvent(PartyState data)
    { 
        Data = data;
    }
}

public struct SkillSwapStartEvent
{
    public SkillState Data;

    public SkillSwapStartEvent(SkillState data)
    {
        Data = data;
    }
}

public struct SkillSwapEndEvent
{
    public SkillState Data;

    public SkillSwapEndEvent(SkillState data)
    {
        Data = data;
    }
}
