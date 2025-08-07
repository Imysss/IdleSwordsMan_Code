using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Data;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class InventoryManager
{
    // 아이템의 고유 ID(string)를 키로, 해당 아이템의 성장 상태(ItemState)를 값으로 갖는 딕셔너리
    private Dictionary<int, ItemState> _allItems = new Dictionary<int, ItemState>();
    private Dictionary<int, GearState> _gearStates = new Dictionary<int, GearState>();
    private Dictionary<int, PartyState> _partyStates = new Dictionary<int, PartyState>();
    private Dictionary<int, SkillState> _skillStates = new Dictionary<int, SkillState>();
    
    // 계산된 총합 보유 효과를 저장
    public float TotalGearOwnedEffect { get; private set; }
    public float TotalPartyOwnedEffect { get; private set; }
    public float TotalSkillOwnedEffect { get; private set; }
    
    public IReadOnlyDictionary<int, ItemState> AllItems => _allItems;
    public IReadOnlyDictionary<int, GearState> GearStates => _gearStates;
    public IReadOnlyDictionary<int, PartyState> PartyStates => _partyStates;
    public IReadOnlyDictionary<int, SkillState> SkillStates => _skillStates;

    private Dictionary<int, GearData> _gearDataDic;

    private ItemDatabase _itemDataBase;
    private SaveLoadManager _saveLoad;
    private Dictionary<int, MergeCostData> _mergeCostDataDic;
    

    public void Init()
    {
        _itemDataBase = Managers.ItemDatabase;
        _mergeCostDataDic = Managers.Data.MergeCostDataDic;
        _saveLoad = Managers.SaveLoad;

        // 저장 데이터가 존재할 경우 
        if (_saveLoad.hasSaveData)
        {
            // 저장 데이터 불러오기 및 덮어쓰기
            foreach (var state in _saveLoad.SaveData.gearStates)
            {
                if (_itemDataBase.GetItemData(state.Value.dataId) is GearData data)
                {
                    state.Value.data = data;
                }
                _gearStates.Add(state.Key, state.Value);
                _allItems.Add(state.Key, state.Value);
            }

            foreach (var state in _saveLoad.SaveData.skillStates)
            {
                if (_itemDataBase.GetItemData(state.Value.dataId) is SkillData data)
                {
                    state.Value.data = data;
                }
                _skillStates.Add(state.Key, state.Value);
                _allItems.Add(state.Key, state.Value);
            }

            foreach (var state in _saveLoad.SaveData.partyStates)
            {
                if (_itemDataBase.GetItemData(state.Value.dataId) is PartyData data)
                {
                    state.Value.data = data;
                }
                _partyStates.Add(state.Key, state.Value);
                _allItems.Add(state.Key, state.Value);
            }
        }

        //저장 데이터가 존재하지 않을 경우
        else
        {
            // ItemDatabase에서 모든 아이템의 기본 설계도를 가져옴
            List<ItemData> allItemData = _itemDataBase.GetAllItemData();
            
            //  각 설계도에 대한 플레이어의 상태(ItemState)를 생성
            foreach (var data in allItemData)
            {
                if (data is GearData gData)
                {
                    GearState newState = new GearState { data = gData };
                    newState.dataId = gData.dataId;
                    newState.LoadVisual();
                    _gearStates.Add(gData.dataId, newState);
                    _allItems.Add(gData.dataId, newState);
                }
                else if (data is PartyData pData)
                {
                    PartyState newState = new PartyState { data = pData };
                    newState.dataId = pData.dataId;
                    _partyStates.Add(pData.dataId, newState);
                    _allItems.Add(pData.dataId, newState);
                }
                else if (data is SkillData sData)
                {
                    SkillState newState = new SkillState { data = sData };
                    newState.dataId = sData.dataId;
                    _skillStates.Add(sData.dataId, newState);
                    _allItems.Add(sData.dataId, newState);
                }
            }
            //튜토리얼 위해 첫 번째 아이템만 잠금 해제
            _gearStates[_gearStates.Keys.First()].isUnlocked = true;
            _skillStates[_skillStates.Keys.First()].isUnlocked = true;
            _partyStates[_partyStates.Keys.First()].isUnlocked = true;
            
            Save();
        }
    }

    public bool IsUnlocked(int itemId)
    {
        var state = GetItemState(itemId);
        return state != null && state.isUnlocked;
    }

    // 뽑기 등을 통해 아이템 획득시 호출하는 함수
    public void AddItem(int itemID)
    {
        ItemData data = _itemDataBase.GetItemData(itemID);

        if (data == null)
        {
            Debug.LogWarning($"{itemID}에 해당하는 ItemData를 찾을 수 없습니다.");
            return;
        }
        
        ItemState stateToUpdate = GetItemState(itemID);
        
        // 가져온 상태 정보(stateToUpdate)를 업데이트
        if (stateToUpdate != null)
        {
            //장비를 처음 획득할때
            if (!stateToUpdate.isUnlocked)
            {
                stateToUpdate.isUnlocked = true;    // 해금 상태
                stateToUpdate.level = 1;            // 처음레벨 1
                stateToUpdate.experience = 0;
            }
            //장비를 다시 획득한 경우
            else
            {
                stateToUpdate.experience++;
                // 데이터 시트에서 경험치 상한선 가져오기
                if (stateToUpdate.experience >= _mergeCostDataDic[stateToUpdate.level].mergeCost && stateToUpdate.level < _mergeCostDataDic.Count)
                {
                    // 업그레이드 기능 활성화
                    stateToUpdate.canUpgrade = true;
                    
                    //해당 데이터 타입에 따라 이벤트 발생
                    switch (data)
                    {
                        case GearData gearData:
                            EventBus.Raise(new GearStateChangedEvent());
                            break;
                        case SkillData skillData:
                            EventBus.Raise(new SkillStateChangedEvent());
                            break;
                        case PartyData partyData:
                            EventBus.Raise(new PartyStateChangedEvent());
                            break;
                    }
                }
            }
        }
        
        //EventBus.Raise(new ItemAcquiredEvent(itemID));
        //보유 효과 재계산
        RecalculateAllOwnedEffects();
        
        // 데이터 저장
        Save();
    }
    
    // 아이템 업그레이드 시 호출하는 함수
    public void UpgradeItem(int itemID)
    {
        ItemData data = _itemDataBase.GetItemData(itemID);

        if (data == null)
        {
            Debug.LogWarning($"{itemID}에 해당하는 ItemData를 찾을 수 없습니다.");
            return;
        }
        
        ItemState stateToUpdate = GetItemState(itemID);

        if (stateToUpdate != null)
        {
            // 업그레이드 가능 상태인 경우
            if (stateToUpdate.canUpgrade)
            {
                stateToUpdate.experience -= _mergeCostDataDic[stateToUpdate.level].mergeCost;
                stateToUpdate.level++;
                // 업그레이드 후, 다음 경험치가 부족한 경우에만 업그레이드 비활성화
                if (stateToUpdate.experience < _mergeCostDataDic[stateToUpdate.level].mergeCost)
                {
                    stateToUpdate.canUpgrade = false;
                }
                
                //해당 데이터 타입에 따라 이벤트 발생
                switch (data)
                {
                    case GearData gearData:
                        EventBus.Raise(new GearStateChangedEvent());
                        break;
                    case SkillData skillData:
                        EventBus.Raise(new SkillStateChangedEvent());
                        break;
                    case PartyData partyData:
                        EventBus.Raise(new PartyStateChangedEvent());
                        break;
                }
            }
        }
        
        //보유 효과 재계산
        RecalculateAllOwnedEffects();
        
        // 데이터 저장
        Save();
    }
    
    // 아이템의 ItemState를 반환하는 함수
    public ItemState GetItemState(int dataId)
    {
        ItemData data = _itemDataBase.GetItemData(dataId);
        
        if (data == null)
        {
            Debug.LogWarning($"{dataId}에 해당하는 ItemData를 찾을 수 없습니다.");
            return null;
        }
        
        // 데이터의 실제 타입에 따라 올바른 딕셔너리를 검색
        if (data is GearData)
        {
            _gearStates.TryGetValue(dataId, out GearState gearState);
            return gearState;
        }
        else if (data is PartyData)
        {
            _partyStates.TryGetValue(dataId, out PartyState partyState);
            return partyState;
        }
        else if(data is SkillData)
        {
            _skillStates.TryGetValue(dataId, out SkillState skillState);
            return skillState;
        }
        Debug.LogWarning($"{dataId}의 타입을 처리할 수 있는 딕셔너리가 없습니다.");
        return null;
    }
    
    /// <summary>
    /// 지정된 장비 타입(GearType)에 해당하는 모든 장비의 상태(GearState) 목록을 반환
    /// </summary>
    public IEnumerable<GearState> GetGearStatesByType(GearType type)
    {
        return _gearStates.Values.Where(state =>
        {
            // 1. 해당 상태(state)의 원본 데이터(GearData)를 가져옴
            GearData data = Managers.ItemDatabase.GetItemData(state.dataId) as GearData;
            
            // 2. 데이터가 존재하고, 그 데이터의 gearType이 찾는 타입과 일치하는지 확인
            return data != null && data.type == type;
        });
    }
    
    /// <summary>
    /// 모든 카테고리의 아이템 상태를 하나의 시퀀스(IEnumerable)로 합쳐서 반환.
    /// </summary>
    public IEnumerable<ItemState> GetAllItemStates()
    {
        // 각 딕셔너리의 Value 컬렉션을 기본 ItemState로 캐스팅하고 Concat으로 연결.
        // 불필요한 리스트 생성이 없어 메모리 부분에서 효율적
        return _partyStates.Values.Cast<ItemState>()
            .Concat(_gearStates.Values.Cast<ItemState>())
            .Concat(_skillStates.Values.Cast<ItemState>());
    }

    /// <summary>
    /// '획득한(Unlocked)' 모든 아이템의 상태 목록만 필터링하여 반환.
    /// </summary>
    public IEnumerable<ItemState> GetAllUnlockedItemStates()
    {
        return GetAllItemStates().Where(state => state.isUnlocked);
    }
    

    /// <summary>
    /// 획득한 모든 아이템의 보유 효과를 처음부터 다시 계산
    /// </summary>
    public void RecalculateAllOwnedEffects()
    {
        // 1. 기존에 계산된 보너스 값을 모두 초기화
        TotalGearOwnedEffect = 0f;
        TotalPartyOwnedEffect = 0f;
        TotalSkillOwnedEffect = 0f;
    
        // 2. 획득한(Unlocked) 모든 아이템 목록을 가져와 순회
        foreach (ItemState state in GetAllUnlockedItemStates())
        {
            ItemData data = _itemDataBase.GetItemData(state.dataId);
            if (data == null) continue;
    
            // 3. 보유 효과 테이블에서 레벨에 맞는 보너스 값을 가져옴
            float effectValue = _itemDataBase.GetOwnedEffect(state.level, data.rarity);
    
            // 4. 아이템 카테고리에 따라 올바른 총합 딕셔너리에 보너스 값을 더함
            if (data is GearData)
            {
               TotalGearOwnedEffect += effectValue;
            }
            else if (data is SkillData)
            {
                TotalSkillOwnedEffect += effectValue;
            }
            else if (data is PartyData)
            {
                TotalPartyOwnedEffect += effectValue;
            }
        }
        Managers.Player.RecalculateAllStats();
    }

    public List<ItemState> FindBestEquippableItem(ItemType itemType, int slotCount, GearType gearType = GearType.Weapon)
    {
        // 아이템 타입에 따라 검색할 소스 딕셔너리를 결정
        IEnumerable<ItemState> source = null;
        
        switch (itemType)
        {
            case ItemType.Gear:
                source = _gearStates.Values;
                break;
            case ItemType.Skill:
                source = _skillStates.Values;
                break;
            case ItemType.Party:
                source = _partyStates.Values;
                break;
        }
        
        // 소스가 없다면 null 반환
        if (source == null) return null;

        var query = source.Where(item => item.isUnlocked);  // 획득한 아이템만 필터링
        
        // 장비 아이템은 부위별로 추가 필터링
        if (itemType == ItemType.Gear)
        {
            query = query.Where(item => (item as GearState)?.data.type == gearType);
        }

        return query
            .OrderByDescending(item =>          // 1순위 : 등급(Rarity) 내림차순
            {
                if (item is GearState gearState)
                    return gearState.data.rarity;
                if (item is SkillState skillState)
                    return skillState.data.rarity;
                if (item is PartyState partyState)
                    return partyState.data.rarity;
                return RarityType.Normal;
            })
            .ThenByDescending(item => item.level)   // 2순위 : 레벨 내림차순
            .ThenByDescending(item => item.dataId)  // 3순위 : ID 내림차순(뒤에 있는 아이템 우선)
            .Take(slotCount)    // 정렬된 결과에서 상위 slotCount 개수만큼 가져옴
            .ToList();      // 결과를 리스트로 
    }

    private void Save()
    {
        _saveLoad.SaveData.gearStates = new Dictionary<int, GearState>(GearStates);
        _saveLoad.SaveData.skillStates = new Dictionary<int, SkillState>(SkillStates);
        _saveLoad.SaveData.partyStates = new Dictionary<int, PartyState>(PartyStates);
    }
}
