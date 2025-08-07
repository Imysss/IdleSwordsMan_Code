using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SkillEquipment : EquipmentManager<SkillState>
{
    private Dictionary<int, Skill> _activeSkills = new();
    private Dictionary<int, SkillState> _skillStates = new();

    public Dictionary<int, Skill> ActiveSkills => _activeSkills;
    public Dictionary<int, SkillState> SkillStates => _skillStates;

    public override void Init()
    {
        maxSlots = MAX_SKILL_SLOTS;
        
        base.Init();
        currentSlots = SaveLoad.SaveData.skillSlots;
        equippedSlots = new List<SkillState>(new SkillState[currentSlots]);
        
        if (SaveLoad.hasSaveData)
        {
            foreach (int key in SaveLoad.SaveData.equippedSkills)
            {
                if (Managers.Inventory.GetItemState(key) is SkillState skill)
                {
                    Equip(skill);
                }
            }
        }
    }

    public override int Equip(SkillState skillState)
    {
        int slotIndex = base.Equip(skillState);

        if (slotIndex != -1)
        {
            int id = skillState.dataId;

            // 기존 인스턴스 재사용. SkillState 내부에 Skill을 캐싱
            if (skillState.skillInstance == null)
                skillState.skillInstance = new Skill(skillState.data, Managers.Player.PlayerStat);

            _activeSkills[id] = skillState.skillInstance;
            _skillStates[id] = skillState;

            // 장착 즉시 쿨타임 적용
            skillState.skillInstance.StartCooldown();

            CombatPowerCalculator.Calculate();
            Managers.UI.RefreshCombatPowerUI?.Invoke();
            EventBus.Raise(new SkillChangedEvent());
        }

        SaveData();
        return slotIndex;
    }

    public override int Unequip(SkillState itemState)
    {
        int slotIndex = base.Unequip(itemState);

        if (slotIndex != -1)
        {
            // Skill 인스턴스 제거하지 않고 유지
            _skillStates.Remove(itemState.dataId);

            CombatPowerCalculator.Calculate();
            Managers.UI.RefreshCombatPowerUI?.Invoke();
            SaveData();
        }

        return slotIndex;
    }

    public Skill[] GetActiveSkills()
    {
        Skill[] skills = new Skill[_activeSkills.Count];
        _activeSkills.Values.CopyTo(skills, 0);
        return skills;
    }

    // 스킬 레벨 변경 후 다시 생성
    public void RefreshSkill(SkillState skillState)
    {
        int id = skillState.dataId;
        if (_activeSkills.ContainsKey(id))
        {
            // 캐시된 인스턴스 갱신
            skillState.skillInstance = new Skill(skillState.data, Managers.Player.PlayerStat);
            _activeSkills[id] = skillState.skillInstance;
        }
    }

    protected override void SaveData()
    {
        SaveLoad.SaveData.skillSlots = currentSlots;
        SaveLoad.SaveData.equippedSkills = GetAllEquippedKeys();
    }

    public void ResetAllCooldowns()
    {
        foreach (var skill in _activeSkills.Values)
            skill.ResetCooldown();
    }
}
