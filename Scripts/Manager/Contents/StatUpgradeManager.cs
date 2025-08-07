using System.Collections.Generic;
using System.Numerics;
using Data;
using static Define;

//StatType 기반의 딕셔너리 관리 구조: 각 능력치의 현재 레벨 관리
//오직 스탯 업그레이드 로직에만 집중: 데이터 불러오기, 해금 조건 확인, 비용 계산, 업그레이드 등
//데이터 중심 계산 방식: baseValue, increasePerLevel, unlock 조건 등을 기반으로 동적으로 계산

//장점
//1. 확장성: StatType이 추가되더라도 StatUpgradeData와 딕셔너리만 추가하면 자동으로 동작
//2. 유지보수성: 스탯 관련 로직이 한 곳에 집중되어 있어서 조건이 수치 변경이 쉬움
//3. 데이터-로직 분리: 수치 관련 설정은 외부 데이터에 위임
//4. 테스트 용이성: CanUpgrade, GetUpgradeCost, GetValue 등 단위 테스트하기 좋은 구조
//5. 유저 경험 최적화: IsLocked, CanUpgrade 등의 상태 체크를 통해 UI/UX에 조건부로 정보 제공 가능
public class StatUpgradeManager
{
    //각 스탯의 현재 레벨 저장
    private Dictionary<StatType, int> _statLevel = new Dictionary<StatType, int>();
    public Dictionary<StatType, int> StatLevel { get { return _statLevel; } }
    private Dictionary<StatType, StatUpgradeData> _statDataDic;
    private SaveLoadManager _saveLoad;
    
    public void Init()
    {
        _statDataDic = Managers.Data.StatUpgradeDataDic;
        _saveLoad = Managers.SaveLoad;
        
        //스탯 레벨 초기화
        if (Managers.SaveLoad.hasSaveData)
        {
            Dictionary<StatType, int> statLevel = Managers.SaveLoad.SaveData.statLevel;
            foreach (var kvp in _statDataDic)
            {
                _statLevel[kvp.Key] = statLevel[kvp.Key];
            }
        }
        else
        {
            foreach (var kvp in _statDataDic)
            {
                _statLevel[kvp.Key] = 1;
            }
            Save();
        }
        
        Managers.Player.RecalculateAllStats();
    }
    
    //현재 레벨 가져오기
    public int GetLevel(StatType type)
    {
        return _statLevel.TryGetValue(type, out int level) ? level : 1;
    }
    
    //현재 수치 계산
    public BigInteger GetBigIntValue(StatType type)
    {
        if (!_statDataDic.TryGetValue(type, out var data))
            return 0;

        double level = GetLevel(type);
        //레벨에 따라 증가하는 수치 계산
        switch (type)
        {
            case StatType.AttackPower:
                return (BigInteger)(level * data.increasePerLevel);
                //return (BigInteger)(data.increasePerLevel * (level * (level + 1) / 2f));
            case StatType.MaxHp:
                return (BigInteger)(data.baseValue + data.increasePerLevel * level);
                //return (BigInteger)((5 * level * level) + (105 * level));
            case StatType.HpRecovery:
                return (BigInteger)(data.baseValue + data.increasePerLevel * level);
                //return (BigInteger)((0.35 * level * level) + (6.65 * level));
            default:
                return 0;
        }
    }
    public float GetFloatValue(StatType type)
    {
        if (!_statDataDic.TryGetValue(type, out var data))
            return 0;

        int level = GetLevel(type);
        //레벨에 따라 증가하는 수치 계산
        switch (type)
        {
            case StatType.AttackSpeed:
                return data.baseValue + data.increasePerLevel * level;
            case StatType.CriticalChance:
            case StatType.CriticalDamage:
                return data.baseValue + data.increasePerLevel * (level - 1);
            default:
                return 0;
        }
    }
    
    
    //업그레이드 비용 계산
    public BigInteger GetUpgradeCost(StatType type)
    {
        BigInteger level = GetLevel(type);
        BigInteger multiplier = 0;
        switch (type)
        {
            case StatType.AttackPower:
                multiplier = 20;
                break;
            case StatType.MaxHp:
                multiplier = 15;
                break;
            case StatType.HpRecovery:
                multiplier = 15;
                break;
            case StatType.AttackSpeed:
                multiplier = 600;
                break;
            case StatType.CriticalChance:
                multiplier = 300;
                break;
            case StatType.CriticalDamage:
                multiplier = 30;
                break;
        }
        //TODO: 타입별로 다른 계산 공식 적용 필요
        BigInteger result = multiplier * (level * (level + 1)) / 2;
        return result;
    }
    
    //최대치인지 확인
    public bool IsMax(StatType type)
    {
        if (!_statDataDic.TryGetValue(type, out var data))
            return false;
        
        //최대 레벨에 도달한 경우
        int currentLevel = GetLevel(type);
        if (currentLevel >= data.maxLevel)
            return true;
        
        return false;
    }
    
    //해금됐는지 확인
    public bool IsUnlocked(StatType type)
    {
        if (!_statDataDic.TryGetValue(type, out var data))
            return false;
        //해금 조건이 있는 경우
        if (data.unlockStatType != StatType.None)
        {
            StatType unlockType = data.unlockStatType;
            int unlockLevel = data.unlockValue;

            if (_statLevel[unlockType] < unlockLevel)
                return false;
        }
        return true;
    }
    
    //업그레이드 가능한지 확인
    public bool CanUpgrade(StatType type)
    {
        if (!_statDataDic.TryGetValue(type, out var data))
            return false;
        
        //최대 레벨에 도달한 경우
        int currentLevel = GetLevel(type);
        if (currentLevel >= data.maxLevel)
            return false;
        
        //해금 조건 확인
        if (!IsUnlocked(type))
            return false;

        //골드가 부족한 경우
        BigInteger cost = GetUpgradeCost(type);
        if (Managers.Game.Gold < cost)
            return false;

        return true;
    }

    public void UpgradeStat(StatType type)
    {
        Managers.Game.UseGold(GetUpgradeCost(type));
        _statLevel[type]++;
        
        //업그레이드 관련 이벤트 발생 -> UI 및 퀘스트 시스템이 감지
        EventBus.Raise(new StatChangedEvent());
        EventBus.Raise(new StatUpgradeEvent(type, _statLevel[type]));

        Save();
        //전체 스탯 재계산
        Managers.Player.RecalculateAllStats();
    }

    private void Save()
    {
        _saveLoad.SaveData.statLevel = StatLevel;
    }
}
