using System.Collections.Generic;
using Data;
using UnityEngine;
using static Define;

public class PartyEquipment : EquipmentManager<PartyState>
{
    // 현재 씬에 소환될 동료 인스턴스를 관리하는 딕셔너리
    private Dictionary<int, GameObject> _equippedParty = new Dictionary<int, GameObject>();
    
    public override void Init()
    {
        maxSlots = MAX_PARTY_SLOTS;
        base.Init();
        currentSlots = SaveLoad.SaveData.partySlots;
        equippedSlots = new List<PartyState>(new PartyState[currentSlots]);
        
        if (SaveLoad.hasSaveData)
        {
            foreach (int key in SaveLoad.SaveData.equippedParty)
            {
                if (Managers.Inventory.GetItemState(key) is PartyState party)
                {
                    Equip(party);
                }
            }
        }
    }

    /// <summary>
    /// 동료 장착. 부모의 Equip 로직에 더해 '소환' 기능을 수행.
    /// </summary>
    public override int Equip(PartyState partyState)
    {
        // 1. 부모의 Equip 함수를 호출하여 장착 성공 여부 먼저 확인
        int slotIndex = base.Equip(partyState);

        // 2. 장착에 성공했다면 
        if (slotIndex != -1)
        {
            GameObject prefab = Managers.Resource.Instantiate(partyState.dataId.ToString());
            if (prefab == null) return -1;

            // 3. 부모가 알려준 slotIndex를 사용하여, 올바른 위치에 동료를 소환
            //Vector3 spawnPosition = Managers.Player.GetPartySpawnPoint(slotIndex).position;
            Vector3 spawnPosition = Managers.Spawn.SpawnPoint.GetPartySpawnPoint(slotIndex).position;
            prefab.transform.position = spawnPosition;
            _equippedParty.Add(partyState.dataId, prefab);

            // 4. 객체 초기화
            if (prefab.TryGetComponent<PartyController>(out PartyController companion))
            {
                companion.Init(partyState);
            }
        }
        
        SaveData();
        
        return slotIndex;
    }
      
    /// <summary>
    /// 동료 장착 해제. 부모의 Unequip 로직에 더해 '파괴' 기능을 수행.
    /// </summary>
    public override int Unequip(PartyState partyState)
    {
        // 1. 부모 클래스의 Unequip을 먼저 호출하여 장착 목록(_equippedItemStates)에서 제거
        int slotIndex = base.Unequip(partyState);

        // 2. 전달받은 partyState의 itemID를 이용해 소환된 인스턴스를 찾아 파괴
        if (_equippedParty.Remove(partyState.dataId, out GameObject instanceToDestroy))
        {
            Managers.Resource.Destroy(instanceToDestroy);
        }

        SaveData();

        return slotIndex;
    }

    protected override void SaveData()
    {
        SaveLoad.SaveData.partySlots = currentSlots;
        SaveLoad.SaveData.equippedParty = GetAllEquippedKeys();
    }
}
