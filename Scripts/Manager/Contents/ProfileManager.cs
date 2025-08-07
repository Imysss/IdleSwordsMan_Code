using System.Collections.Generic;
using System.Linq;
using Data;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

//하나의 프로필(혹은 프레임)에 대한 현재 상태를 보관
//상태와 데이터를 분리하여 관리 (데이터는 변경되지 않고 상태만 런타임에 변경됨)
public class ProfileState
{
    public ProfileData data { get; private set; }   //정적 데이터 (이름, 조건, 타입 등)
    public bool IsUnlocked; //획득 여부
    public bool IsEquipped; //장착 여부

    public bool IsNewlyUnlocked;

    public ProfileState(ProfileData data)
    {
        this.data = data;
        IsUnlocked = (data.conditionType == ProfileUnlockType.None);
        IsEquipped = false;
        IsNewlyUnlocked = false;
    }
}

//전체 프로필 상태를 관리하고 장착/해금/조회 등의 기능을 제공함
public class ProfileManager
{
    //전체 데이터
    private Dictionary<int, ProfileState> _allStates = new();
    
    //장착 정보
    public int EquippedProfileKey { get; private set; }
    public int EquippedFrameKey { get; private set; }
    
    private SaveLoadManager _saveLoad;
    
    public void Init()
    {
        _allStates.Clear();
        
        _saveLoad = Managers.SaveLoad;

        foreach (var pair in Managers.Data.ProfileDataDic)
        {
            var state = new ProfileState(pair.Value);
            _allStates[pair.Key] = state;
        }

        if (_saveLoad.hasSaveData)
        {
            //1. 해금된 키 복원
            foreach (int key in _saveLoad.SaveData.unlockedProfiles)
            {
                if (_allStates.TryGetValue(key, out var state))
                    state.IsUnlocked = true;
            }
            
            //2. 최근 해금된 키 복원
            foreach (int key in _saveLoad.SaveData.newlyUnlockedProfiles)
            {
                if (_allStates.TryGetValue(key, out var state))
                    state.IsNewlyUnlocked = true;
            }
            
            //3. 장착된 키 복원
            EquippedProfileKey = _saveLoad.SaveData.equippedProfileKey;
            EquippedFrameKey = _saveLoad.SaveData.equippedFrameKey;

            Equip(EquippedProfileKey);
            Equip(EquippedFrameKey);
        }
        else
        {
            //기본 장착: 해금된 것 중 첫 번째
            EquippedProfileKey = _allStates.Values.FirstOrDefault(s => s.data.type == ProfileType.Profile && s.IsUnlocked)?.data.key ?? 0;
            EquippedFrameKey = _allStates.Values.FirstOrDefault(s => s.data.type == ProfileType.Frame && s.IsUnlocked)?.data.key ?? 0;
            
            Equip(EquippedProfileKey);
            Equip(EquippedFrameKey);

            Save();
        }
        
        //이벤트 구독
        EventBus.Subscribe<DungeonClearedEvent>(DungeonClearedEventHandler);
        EventBus.Subscribe<PartyStateChangedEvent>(PartyStateChangedEventHandler);
        EventBus.Subscribe<CombatPowerChangedEvent>(CombatPowerChangedEventHandler);
    }
    
    public bool Equip(int key)
    {
        if (!_allStates.TryGetValue(key, out var newEquipState) || !newEquipState.IsUnlocked)
            return false;
        
        //현재 타입과 같은 모든 장비의 장착 해제
        foreach (var state in _allStates.Values)
        {
            if (state.data.type == newEquipState.data.type)
                state.IsEquipped = false;
        }
        
        //새로 장착
        newEquipState.IsEquipped = true;

        switch (newEquipState.data.type)
        {
            case ProfileType.Profile:
                EquippedProfileKey = newEquipState.data.key;
                break;
            case ProfileType.Frame:
                EquippedFrameKey = newEquipState.data.key;
                break;
        }
        
        Save();
        EventBus.Raise(new ProfileChangedEvent());
        return true;
    }

    public bool HasNewlyUnlocked(ProfileType type)
    {
        return _allStates.Values.Any(s => s.data.type == type && s.IsUnlocked && s.IsNewlyUnlocked);
    }
    
    //현재 장착된 상태 가져오기
    public ProfileState GetEquippedState(ProfileType type)
    {
        return _allStates.Values.FirstOrDefault(x => x.data.type == type && x.IsEquipped);
    }

    //프로필 타입에 따라 리스트 불러오기
    public List<ProfileState> GetProfileList(ProfileType type)
    {
        return _allStates.Values.Where(x => x.data.type == type).ToList();
    }
    
    public bool TryGetState(int key, out ProfileState state)
    {
        return _allStates.TryGetValue(key, out state);
    }

    public void Save()
    {
        _saveLoad.SaveData.unlockedProfiles = _allStates.Where(pair => pair.Value.IsUnlocked).Select(pair => pair.Key).ToList();
        _saveLoad.SaveData.newlyUnlockedProfiles = _allStates.Where(pair => pair.Value.IsNewlyUnlocked).Select(pair => pair.Key).ToList();
        _saveLoad.SaveData.equippedProfileKey = EquippedProfileKey;
        _saveLoad.SaveData.equippedFrameKey = EquippedFrameKey;
    }

    private void DungeonClearedEventHandler(DungeonClearedEvent evnt)
    {
        foreach (var state in _allStates.Values)
        {
            if (state.IsUnlocked)
                continue;

            var data = state.data;
            if (data.conditionType == ProfileUnlockType.BossDungeon && evnt.type == DungeonType.Boss)
            {
                if (evnt.level >= data.conditionCount)
                {
                    state.IsUnlocked = true;
                    state.IsNewlyUnlocked = true;
                    Save();
                    EventBus.Raise(new ProfileRedDotChangedEvent());
                }
            }
            else if (data.conditionType == ProfileUnlockType.GoldDungeon && evnt.type == DungeonType.Gold)
            {
                if (evnt.level >= data.conditionCount)
                {
                    state.IsUnlocked = true;
                    state.IsNewlyUnlocked = true;
                    Save();
                    EventBus.Raise(new ProfileRedDotChangedEvent());
                }
            }
        }    
    }

    private void PartyStateChangedEventHandler(PartyStateChangedEvent evnt)
    {
        foreach (var state in _allStates.Values)
        {
            if (state.IsUnlocked)
                continue;

            var data = state.data;
            if (data.conditionType == ProfileUnlockType.Party)
            {
                if (Managers.Inventory.PartyStates[data.conditionId].level >= data.conditionCount)
                {
                    state.IsUnlocked = true;
                    state.IsNewlyUnlocked = true;
                    Save();
                    EventBus.Raise(new ProfileRedDotChangedEvent());
                }
            }
        }
    }

    private void CombatPowerChangedEventHandler(CombatPowerChangedEvent evnt)
    {
        foreach (var state in _allStates.Values)
        {
            if (state.IsUnlocked)
                continue;
            
            var data = state.data;
            if (data.conditionType == ProfileUnlockType.Power)
            {
                if (data.conditionCount >= (int)CombatPowerCalculator.Get())
                {
                    state.IsUnlocked = true;
                    Save();
                }
            }
        }
    }
}