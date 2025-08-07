using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

public abstract class EquipmentManager<TState>  where TState : ItemState
{
    protected SaveLoadManager SaveLoad;
    
    public int maxSlots { get; protected set; } // 장착 가능한 최대 슬롯 수
    public int currentSlots { get; protected set; } // 현재 슬롯 수
    
     // 고정된 슬롯으로 장착된 아이템 관리
    protected List<TState> equippedSlots;
    public IReadOnlyList<TState> EquippedSlots => equippedSlots;

    public virtual void Init()
    {
        SaveLoad = Managers.SaveLoad;
    }
    
    /// <summary>
    /// 아이템 장착
    /// </summary>
    public virtual int Equip(TState itemState)
    {
        // 이미 장착 중인지 확인
        if (IsEquipped(itemState.dataId))
        {
            //Debug.LogWarning($"{itemState.dataId}은(는) 이미 장착 중입니다.");
            return -1;
        }

        // 비어있는 첫 번째 슬롯의 인덱스 서치
        int emptySlotIndex = equippedSlots.FindIndex(slot => slot == null);

        // 빈 슬롯이 없다면 실패
        if (emptySlotIndex == -1)
        {
            return -1;
        }
        // 리스트에 아이템 추가
        equippedSlots[emptySlotIndex] = itemState;
        
        return emptySlotIndex;
    }
    
    /// <summary>
    /// 아이템 장착 해제
    /// </summary>
    public virtual int Unequip(TState itemState)
    {
        if (itemState == null) return -1;

        // 장착 해제할 아이템 인덱스 서치
        int slotIndex = equippedSlots.FindIndex(slot => slot != null && slot == itemState);
    
        // 유효한 인덱스를 찾았다면 해당 슬롯을 null로 설정
        if (slotIndex != -1)
        {
            equippedSlots[slotIndex] = null;
        }
        
        return slotIndex;
    }

    /// <summary>
    ///  아이템 교체
    /// </summary>
    public void ReplaceEquip(TState oldItem, TState newItem)
    {
        Unequip(oldItem);
        Equip(newItem);
    }
    
    /// <summary>
    /// 해당 ID의 아이템이 현재 장착 중인지 확인하여 알려줌.
    /// </summary>
    public bool IsEquipped(int itemID)
    {
        bool isEquipped = equippedSlots.Any(slot => slot != null && slot.dataId == itemID);
        return isEquipped;
    }

    public void AddSlot(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (currentSlots < maxSlots)
            {
                currentSlots++;
                equippedSlots.Add(null); // 늘어난 슬롯을 null로 채워줌
            }
        }
        SaveData();
    }

    public virtual List<int> GetAllEquippedKeys()
    {
        List<int> keys = new List<int>();
        foreach (TState itemState in equippedSlots)
        {
            if (itemState == null) continue;
            keys.Add(itemState.dataId);
        }

        return keys;
    }

    protected virtual void SaveData() { }

    public List<TState> GetEquippedItems()
    {
        return equippedSlots.Where(slot => slot != null).ToList();
    }
}