using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;

//효율적인 필터링을 위해 GearData, SkillData, PartyData를 RarityType으로 분리해 캐시화
//가챠 경험치/레벨 업 시스템을 GachaType 별로 관리해 성장 기반 보상 구조 설계 가능
//코드 확장성과 유지보수성을 고려한 명확한 분리 (초기화/선택/등급 처리/레벨업 등)

//장점
//1. 성능 최적화: GroupBy로 등급별 데이터 캐싱 -> 반복 필터링 없음
//2. 유연한 등급 확률 제어: GachaTableData로 레벨별 확률 조정 가능
//3. 확장성 확보: GachaType, RarityType 기반으로 동작 -> 타입 추가 쉬움
//4. 경험치/레벨업 관리 통합: 가챠 반복 시 경험치 누적 + 조건부 레벨업으로 플레이어 성장 피드백 제공
public class GachaManager
{
    //가챠 타입별 누적 뽑기 횟수, 경험치, 현재 레벨 관리
    private Dictionary<GachaType, int> gachaCount = new();
    private Dictionary<GachaType, int> gachaExp = new();
    private Dictionary<GachaType, int> gachaLevel = new();
    
    // 타입별 총 뽑기 횟수를 저장할 딕셔너리 (캐시)
    private Dictionary<GachaType, int> _totalGachaCountCache = new();
    
    public Dictionary<GachaType, int> GachaCount {get{ return gachaCount; } }
    public Dictionary<GachaType, int> GachaExp { get { return gachaExp; } }
    public Dictionary<GachaType, int> GachaLevel { get { return gachaLevel; } }
    
    //등급(Rarity)를 기준으로 데이터를 분리해 빠른 접근이 가능하도록 함
    public Dictionary<RarityType, List<Data.GearData>> gearsByGrade;
    public Dictionary<RarityType, List<Data.SkillData>> skillsByGrade;
    public Dictionary<RarityType, List<Data.PartyData>> partiesByGrade;

    private SaveLoadManager _saveLoad;
    // 뽑기 횟수가 변경되었는지 확인하는 플래그
    private bool _isGachaCountDirty = true;

    public void Init()
    {
        _saveLoad = Managers.SaveLoad;
        
        //초기화 시 모든 장비/스킬/파티 데이터를 등급별로 분류
        gearsByGrade = Managers.Data.GearDataDic.Values.GroupBy(g => g.rarity)
            .ToDictionary(g => g.Key, g => g.ToList());
        skillsByGrade = Managers.Data.SkillDataDic.Values.GroupBy(s => s.rarity)
            .ToDictionary(s => s.Key, s => s.ToList());
        partiesByGrade = Managers.Data.PartyDataDic.Values.GroupBy(p => p.rarity)
            .ToDictionary(p => p.Key, p => p.ToList());
        
       
        //레벨/경험치 받아오기 
        if (_saveLoad.hasSaveData)
        {
            if (_saveLoad.SaveData.gachaExp != null)
            {
                gachaExp[GachaType.Gear] = _saveLoad.SaveData.gachaExp[GachaType.Gear];
                gachaExp[GachaType.Skill] =  _saveLoad.SaveData.gachaExp[GachaType.Skill];
                gachaExp[GachaType.Party] = _saveLoad.SaveData.gachaExp[GachaType.Party];
            }
            if (_saveLoad.SaveData.gachaLevel != null)
            {
                gachaLevel[GachaType.Gear] = _saveLoad.SaveData.gachaLevel[GachaType.Gear];
                gachaLevel[GachaType.Skill] = _saveLoad.SaveData.gachaLevel[GachaType.Skill];
                gachaLevel[GachaType.Party] = _saveLoad.SaveData.gachaLevel[GachaType.Party];
            }
        }
        else
        {   //저장 데이터가 존재하지 않는 경우
            gachaExp[GachaType.Gear] = 0;
            gachaExp[GachaType.Skill] = 0;
            gachaExp[GachaType.Party] = 0;
        
            gachaLevel[GachaType.Gear] = 1;
            gachaLevel[GachaType.Skill] = 1;
            gachaLevel[GachaType.Party] = 1;
            
            _saveLoad.SaveData.gachaExp = gachaExp;
            _saveLoad.SaveData.gachaLevel = gachaLevel;
        }
    }
    
    //DoGacha에서 해야 할 일
    //1. 반환값 dataId를 반환하고 있음
    public List<(int dataId, bool isNew)> DoGacha(GachaType type, int count)
    {
        List<(int dataId, bool isNew)> resultList = new List<(int dataId, bool isNew)>();

        for (int i = 0; i < count; i++)
        {
            if (gachaLevel[type] < Managers.Data.GachaLevelTableDataDic.Count)
            {
                gachaExp[type]++;   //1회 가챠 시 1 경험치 획득
                TryLevelUp(type);   //경험치 누적으로 인한 레벨업 시도
            }

            //현재 레벨에 맞는 가챠 확률 테이블 참조
            int level = gachaLevel[type];
            GachaTableData rate = Managers.Data.GachaTableDataDic[level];

            //가챠 확률 테이블에 따라 랜덤 등급 획득
            RarityType rarity = GetRandomRarity(rate);

            int dataId = -1;
        
            //해당 등급 리스트에서 무작위 아이템 선택
            //Gear 타입만 등급 선택 후 GearData 반환
            if (type == GachaType.Gear)
            {
                List<GearData> candidates = gearsByGrade[rarity];
                if (candidates.Count > 0)
                {
                    int index = Random.Range(0, candidates.Count);  //해당 등급 내에 가지고 있는 모든 equipment들을 동일한 확률로 random 해서 뽑는 것
                    dataId = candidates[index].dataId; 
                }
            }
            else if (type == GachaType.Skill)
            {
                List<SkillData> candidates = skillsByGrade[rarity];
                if (candidates.Count > 0)
                {
                    int index = Random.Range(0, candidates.Count);
                    dataId = candidates[index].dataId;
                }
            }
            else if (type == GachaType.Party)
            {
                List<PartyData> candidates = partiesByGrade[rarity];
                if (candidates.Count > 0)
                {
                    int index = Random.Range(0, candidates.Count);
                    dataId = candidates[index].dataId;
                }
            }
            
            //보유 여부 체크
            bool isNew = !Managers.Inventory.IsUnlocked(dataId);
            
            //인벤토리에 해당 아이템 추가
            Managers.Inventory.AddItem(dataId);
            resultList.Add((dataId, isNew));
        }
        
        // 데이터 저장
        SaveGachaExp(type);
        SaveGachaLevel(type);

        // 가챠 누적 횟수 재계산
        RecalculateGachaCounts(type);
        
        EventBus.Raise(new GachaEvent(type, count));
        
        //가챠 결과 팝업창에 보여주기 위해 해당 dataId 리스트 반환
        return resultList;
    }

    public int DoOfflineGacha(GachaType type, int level)
    {
        GachaTableData rate = Managers.Data.GachaTableDataDic[level];
        RarityType rarity = GetRandomRarity(rate);

        int dataId = -1;

        if (type == GachaType.Gear)
        {
            var candidates = Managers.Gacha.gearsByGrade[rarity];
            if (candidates.Count > 0)
                dataId = candidates[Random.Range(0, candidates.Count)].dataId;
        }
        else if (type == GachaType.Skill)
        {
            var candidates = Managers.Gacha.skillsByGrade[rarity];
            if (candidates.Count > 0)
                dataId = candidates[Random.Range(0, candidates.Count)].dataId;
        }
        else if (type == GachaType.Party)
        {
            var candidates = Managers.Gacha.partiesByGrade[rarity];
            if (candidates.Count > 0)
                dataId = candidates[Random.Range(0, candidates.Count)].dataId;
        }

        return dataId;
    }

    public GachaType GetRandomGachaType()
    {
        Array values = Enum.GetValues(typeof(GachaType));
        return (GachaType)values.GetValue(Random.Range(0, values.Length));
    }

    // 첫 번째 소환일 경우(경험치가 0인 경우) true를 반환
    public bool isFirstSummon(GachaType type)
    {
        return GachaExp[type] == 0 && GachaLevel[type] == 1;
    }

    // 현재 총 뽑기 횟수를 계산하여 반환하는 메서드
    public int GetGachaCount(GachaType type)
    {
        return _totalGachaCountCache.GetValueOrDefault(type, 0);
    }
    
    // 모든 타입의 총 뽑기 횟수를 미리 계산하여 캐시에 저장
    private void RecalculateGachaCounts(GachaType type)
    {
        _totalGachaCountCache[type] = 0;

        int count = 0;
        for (int i = 1; i < gachaLevel[type]; i++)
        {
            count += Managers.Data.GachaLevelTableDataDic[i].experience;
        }
        count += gachaExp[type];
        
        _totalGachaCountCache[type] = count;
    }

    //현재 경험치가 현재 레벨에서 요구하는 경험치 이상이면 레벨업
    private void TryLevelUp(GachaType type)
    {
        int currentLevel = gachaLevel[type];
        
        //현재 레벨의 요구 경험치를 못 찾으면 중단
        if (!Managers.Data.GachaLevelTableDataDic.TryGetValue(currentLevel, out GachaLevelTableData levelData))
            return;

        int needExp = levelData.experience;
        if (gachaExp[type] >= needExp)
        {
            gachaExp[type] -= needExp;  //초과 경험치 보존
            gachaLevel[type]++;
        }
    }
    
    
    //레벨에 따라 등급 확률 기반으로 랜덤 등급 선택
    private RarityType GetRandomRarity(GachaTableData rate)
    {
        //등급별 확률 배열 구성
        float[] probArray = new float[] { rate.Normal, rate.Rare, rate.Epic, rate.Unique, rate.Legendary };

        float total = probArray.Sum();  //총합 기준으로 랜덤 선택 (총합 1이 되도록 시트에서 설정해 두었지만 안 된다면 원하는 확률대로 분포할 수 있도록 설정)
        float rand = Random.value * total;

        float cumulative = 0f;  //누적 값
        for (int i = 0; i < probArray.Length; i++)
        {
            cumulative += probArray[i];
            if (rand <= cumulative)
            {
                return (RarityType)i;   //누적 확률을 기준으로 등급 반환
            }
        }
        
        //확률이 모두 0일 경우
        return RarityType.Normal;
    }

    private void SaveGachaExp(GachaType type)
    {
        _saveLoad.SaveData.gachaExp[type] = gachaExp[type];
    }

    private void SaveGachaLevel(GachaType type)
    {
        _saveLoad.SaveData.gachaLevel[type] = gachaLevel[type];
    }
}
