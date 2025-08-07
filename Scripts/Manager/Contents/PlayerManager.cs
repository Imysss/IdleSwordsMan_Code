using System.Collections.Generic;
using Data;
using static Define;
using UnityEngine;

public class PlayerManager
{
    private PlayerController _player;
    public PlayerController Controller => _player;
    private StatManager _playerStat;
    public StatManager PlayerStat => _playerStat;
    public Transform PlayerTransform => _player?.transform;
    public Vector3 PlayerPosition => _player?.transform.position ?? Vector3.zero;
    public SkillExecutor SkillExecutor { get; private set; }

    //플레이어 프리팹 생성 및 StatManager 저장
    public void Init()
    {
        GameObject playerGO = Managers.Resource.Instantiate("Human");
        //GameObject playerGO = Managers.Resource.Instantiate("New character");

        if (playerGO.TryGetComponent<PlayerController>(out _player))
        {
            _playerStat = _player.StatManager;
            SkillExecutor = playerGO.GetComponent<SkillExecutor>();

            UnitStatData playerStatData = new UnitStatData
            {
                maxHP = 0,
                curHP = 0,
                attackPower = 0,

                attackSpeed = 0,
                moveSpeed = 0,
                attackRange = 3.0f,
                criticalChance = 0f,
                criticalDamage = 0f
            };

            _playerStat.Initialize(playerStatData);

            var hpBar = playerGO.GetComponentInChildren<PlayerHPBar>(true);
            if (hpBar != null)
            {
                hpBar.gameObject.SetActive(true);
                hpBar.Init(playerGO.transform, _playerStat);
            }
        }
        else
        {
            Debug.LogError("[PlayerManager] 플레이어 프리팹에 PlayerController가 없습니다.");
        }
        Managers.Inventory.RecalculateAllOwnedEffects();
    }

    // 던전 종료 후 플레이어 상태 리셋용 함수
    public void ResetAfterDungeon()
    {
        if (_player == null || _playerStat == null)
            return;

        _player.ResetState();
    }

    // 플레이어의 스탯은 기본 스탯(골드로 업그레이드), 장착 효과, 보유 효과로 결정됨
    // 아이템을 장착/해제하거나 스탯을 강화할 때마다 모든 스탯을 재계산 하는 것은 비효율적이지 않을까?
    // 이론적으로만 보면 비효율적이지만, 매우 안전하다는 장점이 있음
    
    // 스탯 재계산 함수
    public void RecalculateAllStats()
    {
        // 1. 스탯 초기화
        _playerStat.Reset();
        
        // 2. 스탯 업그레이드 적용
        foreach (var kvp in Managers.StatUpgrade.StatLevel)
        {
            if (kvp.Key is StatType.AttackPower or StatType.MaxHp or StatType.HpRecovery)
            {
                _playerStat.SetUpgrade(kvp.Key, Managers.StatUpgrade.GetBigIntValue(kvp.Key));
            }
            else
            {
                _playerStat.SetUpgrade(kvp.Key, Managers.StatUpgrade.GetFloatValue(kvp.Key));
            }
        }
           
        // 3. '장착 효과' 적용(장비 아이템 한정)
        IReadOnlyDictionary<StatType, float> totalEquippedGearEffect = Managers.Equipment.TotalGearEquippedEffect;
        foreach (var effect in totalEquippedGearEffect)
        {
            _playerStat.AddToAdditiveBonus(effect.Key, effect.Value);
        }
        //Debug.Log($"totalEquippedGearEffect: {totalEquippedGearEffect.Count}");
        
        // 4. '보유 효과' 적용
        float totalOwnedEffect = Managers.Inventory.TotalGearOwnedEffect +
                                 Managers.Inventory.TotalSkillOwnedEffect +
                                 Managers.Inventory.TotalPartyOwnedEffect;
        _playerStat.AddToAdditiveBonus(StatType.AttackPower, totalOwnedEffect);
        
        // 5. 변경된 체력값 현재 체력에도 적용
        _playerStat.UpdateCurrentHp();
        

        
        // Debug.Log($"플레이어 기본 공격력: {_playerStat.Stats[StatType.AttackPower].baseValue}\n" +
        //           $"플레이어 골드 공격력: {_playerStat.Stats[StatType.AttackPower].upgradeValue}\n" +
        //           $"플레이어 추가 공격력: {_playerStat.Stats[StatType.AttackPower]._additiveBonus}\n" +
        //           $"플레이어 버프 공격력: {_playerStat.Stats[StatType.AttackPower]._multiplier}\n" +
        //           $"플레이어 최종 공격력: {_playerStat.Stats[StatType.AttackPower].Value}");
        //
        // Debug.Log($"플레이어 기본 크리티컬: {_playerStat.Stats[StatType.CriticalChance].baseValue}\n" +
        //           $"플레이어 골드 크리티컬: {_playerStat.Stats[StatType.CriticalChance].upgradeValue}\n" +
        //           $"플레이어 추가 크리티컬: {_playerStat.Stats[StatType.CriticalChance]._additiveBonus}\n" +
        //           $"플레이어 버프 크리티컬: {_playerStat.Stats[StatType.CriticalChance]._multiplier}\n" +
        //           $"플레이어 최종 크리티컬: {_playerStat.Stats[StatType.CriticalChance].Value}");
        
        CombatPowerCalculator.Calculate();
    }
}