using System;

[Serializable]
public class SkillData : ItemData
{
    public SkillType type;
    public SkillExecuteType executeType;
    public float cooldown;
    public float range;
    public int targetCount;
    public float damageMultiplier;
    public int attackCount;
    public float attackInterval;
    public float recognitionRange;
    public float projSpeed;
    public float projRange;
    public float duration;
    public StatType stat;
    public float buffValue;
    public string effectName;
    public float effectDuration;
    public string animTrigger;
    public string clipName;
}