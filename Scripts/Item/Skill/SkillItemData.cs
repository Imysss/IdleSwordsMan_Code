using System;
using Data;

[System.Serializable]
public class SkillState : ItemState<SkillData>
{
    public bool isAuto;

    // 런타임 캐싱 용도의 Skill 인스턴스 (세이브되지 않음)
    [NonSerialized] public Skill skillInstance;

    public SkillState(bool isAuto = true)
    {
        this.isAuto = isAuto;
    }
}