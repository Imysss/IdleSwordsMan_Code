using System;
using System.Collections.Generic;
using System.Data;
using Data;
using static Define;
using UnityEngine;


// 플레이어가 장비한 모든 Equipment 매니저를 관리하는 wrapper 클래스
public class PlayerEquipment
{
    // 각 전문 매니저들에 대한 참조
    private GearEquipment _gearEquipment;
    private PartyEquipment _partyEquipment;
    private SkillEquipment _skillEquipment;
    public GearEquipment GearEquipment { get { return _gearEquipment; } }
    public PartyEquipment PartyEquipment { get { return _partyEquipment; } }
    public SkillEquipment SkillEquipment { get { return _skillEquipment; } }
    
    // 장착중인 장비 아이템들의 총합 효과를 관리하는 딕셔너리
    private Dictionary<StatType, float> _totalGearEquippedEffect = new Dictionary<StatType, float>();
    public IReadOnlyDictionary<StatType, float> TotalGearEquippedEffect => _totalGearEquippedEffect;
    
    private ItemDatabase _itemDatabase;

    public void Init()
    {
        _gearEquipment = new GearEquipment();
        _partyEquipment = new PartyEquipment();
        _skillEquipment = new SkillEquipment();
        
        _gearEquipment.Init();
        _partyEquipment.Init();
        _skillEquipment.Init();

        _itemDatabase = Managers.ItemDatabase;
        
        RecalculateAllEquippedEffects();
    }

    /// <summary>
    /// 외부에서 아이템 장착을 요청하는 단일 함수
    /// </summary>
    public int Equip(ItemState state)
    {
        // 아이템의 원본 데이터 타입을 확인하여 올바른 전문 매니저에게 요청을 전달(라우팅)
        ItemData data = Managers.ItemDatabase.GetItemData(state.dataId);

        int slotIndex = -1;
        
        if (data is GearData)
        {
            slotIndex = _gearEquipment.Equip(state as GearState);
        }
        else if (data is PartyData)
        {
            // as를 사용한 형 변환으로 안전하게 캐스팅
            slotIndex = _partyEquipment.Equip(state as PartyState);
        }
        else if (data is SkillData)
        {
            slotIndex = _skillEquipment.Equip(state as SkillState);
        }

        return slotIndex;
    }

    /// <summary>
    /// 외부에서 아이템 장착 해제를 요청하는 단일 함수
    /// </summary>
    public void Unequip(ItemState state)
    {
        ItemData data = Managers.ItemDatabase.GetItemData(state.dataId);

        if (data is GearData)
        {
            _gearEquipment.Unequip(state as GearState);
        }
        else if (data is PartyData)
        {
            _partyEquipment.Unequip(state as PartyState);
        }
        else if (data is SkillData)
        {
            _skillEquipment.Unequip(state as SkillState);
        }
    }
    
    public bool IsEquipped(int itemID)
    {
        ItemData data = Managers.ItemDatabase.GetItemData(itemID);

        if(data is GearData) return _gearEquipment.IsEquipped(itemID);
        else if (data is PartyData) return _partyEquipment.IsEquipped(itemID);
        else if (data is SkillData) return _skillEquipment.IsEquipped(itemID);
        
        return false;
    }
    
    // 모든 매니저의 장착 목록을 하나로 합쳐서 반환
    public List<ItemState> GetAllEquippedStates()
    {
        List<ItemState> allEquipped = new List<ItemState>();
        if(_gearEquipment != null) allEquipped.AddRange(_gearEquipment.EquippedSlots);
        if(_partyEquipment != null) allEquipped.AddRange(_partyEquipment.EquippedSlots);
        if(_skillEquipment != null) allEquipped.AddRange(_skillEquipment.EquippedSlots);
        
        allEquipped.RemoveAll(item => item == null);
        return allEquipped;
    }
    

    public void RecalculateAllEquippedEffects()
    {
        _totalGearEquippedEffect.Clear();

        foreach (GearState gearState in _gearEquipment.EquippedSlots)
        {
            if (gearState == null ||  _itemDatabase.GetItemData(gearState.dataId) is not GearData data) continue;

            StatType statType;
            // GearType에 따라 어떤 스탯을 올려주는지는 여기서 정의! 
            switch (data.type)
            {
                case GearType.Armor:
                    statType = StatType.MaxHp;
                    break;
                case GearType.Gloves:
                    statType = StatType.CriticalDamage;
                    break;
                case GearType.Hat:
                    statType = StatType.HpRecovery;
                    break;
                case GearType.Shoes:
                case GearType.Weapon:
                    statType = StatType.AttackPower;
                    break;
                default:
                    continue;
            }
            // formula 기반으로 추가할 스탯 계산
            float bonusValue = CalculateMultiplier(gearState);
            _totalGearEquippedEffect.TryAdd(statType, bonusValue);
            //Debug.Log($"totalgearequppedeffect: {statType}: {_totalGearEquippedEffect[statType]}");
        }
        Managers.Player.RecalculateAllStats();
    }

    private float CalculateMultiplier(GearState gear)
    {
        if (!Managers.Data.GearLevelDataDic.TryGetValue(gear.dataId, out GearLevelData levelData))
        {
            return 1f;
        }
        
        string formula = levelData.formula;
        float baseValue = levelData.baseValue;
        
        formula = formula.Replace("baseValue", baseValue.ToString());
        formula = formula.Replace("level", gear.level.ToString());

        try
        {
            var result = new DataTable().Compute(formula, null);
            return Convert.ToSingle(result);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Calculation Failed : Gear");
            return 1f;
        }
    }
}