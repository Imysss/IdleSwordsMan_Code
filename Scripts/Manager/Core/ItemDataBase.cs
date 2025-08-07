using System.Collections.Generic;
using Data;
using UnityEngine;
using Newtonsoft.Json;
using static Define;

public class ItemDatabase
{
    // 모든 아이템의 원본 데이터를 저장할 딕셔너리
    private Dictionary<int, ItemData> _itemDatabase = new Dictionary<int, ItemData>();

    public void Init()
    {
        // string jsonData = jsonAsset.text;
        //
        // JsonSerializerSettings settings = new JsonSerializerSettings
        // {
        //     TypeNameHandling = TypeNameHandling.Auto
        // };
        //
        // List<ItemData> allItems = JsonConvert.DeserializeObject<List<ItemData>>(jsonData, settings);
        
        _itemDatabase.Clear();
        
        // 리스트를 딕셔너리로 변환하여 접근하기 쉽게 만듦
        foreach (var item in Managers.Data.GearDataDic.Values)
        {
            _itemDatabase.Add(item.dataId, item);
        }

        foreach (var item in Managers.Data.SkillDataDic.Values)
        {
            _itemDatabase.Add(item.dataId, item);
        }

        foreach (var item in Managers.Data.PartyDataDic.Values)
        {
            _itemDatabase.Add(item.dataId, item);
        }
    }

    public ItemData GetItemData(int itemID)
    {
        _itemDatabase.TryGetValue(itemID, out ItemData data);
        return data;
    }

    public List<ItemData> GetAllItemData()
    {
        return new List<ItemData>(_itemDatabase.Values);
    }
    
    /// <summary>
    /// 아이템의 레벨, 희귀도에 맞게 보유효과 수치 반환
    /// </summary>
    public float GetOwnedEffect(int level, RarityType rarity)
    {
        OwnedEffectLevelData data = Managers.Data.OwnedEffectLevelDataDic[level];
        return rarity switch
        {
            RarityType.Normal => data.Normal,
            RarityType.Rare => data.Rare,
            RarityType.Epic => data.Epic,
            RarityType.Unique => data.Unique,
            RarityType.Legendary => data.Legendary,
            _ => data.Normal
        };
    }
}