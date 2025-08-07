using Data;

public struct PartyChangedEvent
{
    
}

public struct GearSelectedEvent
{
    public GearState data;

    public GearSelectedEvent(GearState data)
    {
        this.data = data;
    }
}
public struct GearChangedEvent
{
    
}

public struct SkillChangedEvent
{
    
}

public struct GearStateChangedEvent { }

public struct SkillStateChangedEvent { }

public struct PartyStateChangedEvent { }

public struct ProfileSelectedEvent
{
    public ProfileState data;

    public ProfileSelectedEvent(ProfileState data)
    {
        this.data = data;
    }
}

public struct ProfileChangedEvent
{
    
}

public struct ProfileRedDotChangedEvent { }