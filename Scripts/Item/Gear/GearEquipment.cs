using System.Collections.Generic;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Define;

// 장비 장착 클래스
// 슬롯 관리, 외형 반영, addressable 스프라이트
public class GearEquipment : EquipmentManager<GearState>
{
    //장비 타입별 현재 착용 장비 상태 저장
    private Dictionary<GearType, GearState> _equippedGears = new Dictionary<GearType, GearState>();
    
    // 외형에 반영할 PlayerVisual 참조
    private PlayerVisual _playerVisual;

    // 초기화 시 슬롯 수 설정 및 타입별 장비 초기화
    public override void Init()
    {
        maxSlots = MAX_GEAR_SLOTS;
        currentSlots = maxSlots;
        equippedSlots = new List<GearState>(new GearState[currentSlots]);
        
        base.Init();

        if (!Managers.Player.PlayerTransform.TryGetComponent<PlayerVisual>(out _playerVisual))
        {
            Debug.LogWarning("PlayerVisual not found");
            return;
        }
        
        _equippedGears = new Dictionary<GearType, GearState>()
        {
            { GearType.Weapon , null},
            { GearType.Hat , null},
            { GearType.Armor , null},
            { GearType.Gloves , null},
            { GearType.Shoes , null},
        };
        
        if (SaveLoad.hasSaveData)
        {
            foreach (int key in SaveLoad.SaveData.equippedGears)
            {
                if (Managers.Inventory.GetItemState(key) is GearState gear)
                {
                    Equip(gear);
                }
            }
        }
    }
    
    public override int Equip(GearState gearState)
    {
        // 이미 같은 부위를 착용 중이면, 장착 해제
        if (_equippedGears.TryGetValue(gearState.data.type, out GearState currentEquippedState))
        {
            if (currentEquippedState != null)
            {
                Unequip(currentEquippedState);
            }
            //처음 착용할 경우
            else
            {
                //타입이 모자면?
                if (gearState.data.type == GearType.Hat)
                {
                    //Hat sprite를 character 스크립트 받아와서 거기다가 연결해 주면 될 듯?
                    Character character = _playerVisual.GetComponent<Character>();
                    character.ShowHelmet = true;
                    character.HelmetInit();
                }
            }
        }
        
        int slotIndex = base.Equip(gearState);
        
        if (slotIndex != -1)
        {
            _equippedGears[gearState.data.type] = gearState;

            /// <summary>
            /// 장비 장착 시, 해당 GearType에 따라 적용할 부위별 Addressables 키를 생성하고,
            /// 각 키에 대응하는 Sprite를 비동기 로드한 뒤,
            /// 부위명을 추출하여 PlayerVisual의 대응 SpriteRenderer에 Sprite를 적용
            /// 
            /// 예를 들어 장갑(Gloves)은 "ArmL", "ArmR" 두 부위에 해당하는 키
            /// (ex: "5001[ArmL]", "5001[ArmR]")를 만들고,
            /// 로드된 Sprite를 각 부위에 정확히 반영
            /// 
            /// 비동기 로딩 실패 시에는 경고 로그를 출력하여 디버깅에 도움
            /// </summary>

            // Weapon, Hat 은 단일 sprite 리소스로 처리
            if (gearState.data.type == GearType.Weapon)
            {
                Sprite weaponSprite = Managers.Resource.Load<Sprite>($"{gearState.dataId}.sprite");
                _playerVisual?.ApplySprite(gearState.data.type, "Weapon", weaponSprite);
            }
            else if (gearState.data.type == GearType.Hat)
            {
                Sprite hatSprite = Managers.Resource.Load<Sprite>($"{gearState.dataId}.sprite");
                _playerVisual.ApplySprite(gearState.data.type, "Hat", hatSprite);
            }
            else //Glove, Armor, Shoes > Atlas
            {
                // 장비 타입에 따라 필요한 부위명 리스트 생성 -> 4001[ArmL], 4001[ArmR]
                List<string> addressKeys = GetAddressKeys(gearState);

                // 각 부위별 Addressable 키 기반 비동기 로딩
                foreach (string key in addressKeys)
                {
                    // ArmL 문자열만 추출
                    string partName = ExtractPartNameFromKey(key);

                    // 해당 부위에 Sprite 적용
                    _playerVisual?.SetGearVisual(gearState.data.type, partName, key);
                }
            }
        }
        
        //Managers.Player.RecalculateAllStats();
        SaveData();
        
        return slotIndex;
    }

 

    // 장비 해제 처리
    public override int Unequip(GearState gearState)
    {
        int slotIndex = base.Unequip(gearState);

        if (_equippedGears[gearState.data.type] == gearState)
        {
            _equippedGears.Remove(gearState.data.type);
            // 외형 제거 (null 스프라이트로 대체)
            // if (_playerVisual != null)
            // {
            //     // 장비 타입에 해당하는 부위의 목록 불러오기
            //     string[] parts = GetPartsForGearType(gearState.data.type);
            //
            //     // 해당 부위에 SpriteRenderer에 null 적용시켜 외형 제거
            //     foreach (string part in parts)
            //     {
            //         _playerVisual.SetGearVisual(gearState.data.type, part, null);
            //     }
            // }
        }

        return slotIndex;
    }
    
    /// <summary>
    /// 지정된 부위에 장착된 장비의 상태를 반환합니다.
    /// </summary>
    public GearState GetEquippedGear(GearType type)
    {
        if (_equippedGears.TryGetValue(type, out var gear))
        {
            return gear;
        }

        // 없을 경우 null 반환 또는 기본값 반환
        return null;
    }

    // Addressable key 생성 ex> 5001[ArmL]
    private List<string> GetAddressKeys(GearState gearState)
    {
        List<string> keys = new();
        string id = gearState.data.dataId.ToString();
        string[] parts = GetPartsForGearType(gearState.data.type);

        foreach (string part in parts)
        {
            keys.Add($"{id}[{part}]");
        }

        return keys; 
    }

    // GearType에 따라 사용할 부위 이름 반환
    private string[] GetPartsForGearType(GearType type)
    {
        return type switch
        {
            // GearType.Hat => new[] { "Hat" },
            // GearType.Weapon => new[] { "Weapon" },
            GearType.Armor => new[] { "Torso", "ArmL", "ArmR", "Pelvis" },
            GearType.Gloves => new[] { "ForearmL", "ForearmR", "HandL", "HandR", "SleeveR" },
            GearType.Shoes => new[] { "Shin" },
        };
    }
    // addressable key에서 부위명만 추출
    private string ExtractPartNameFromKey(string key)
    {
        int start = key.IndexOf('[') + 1;
        int end = key.IndexOf(']');

        if (start >= 0 && end > start)
            return key.Substring(start, end - start);

        return "";
    }

    protected override void SaveData()
    {   
        SaveLoad.SaveData.equippedGears = GetAllEquippedKeys();
    }
}
