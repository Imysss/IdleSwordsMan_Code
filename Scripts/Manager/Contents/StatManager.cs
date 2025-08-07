using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using static Define;

public class StatManager : MonoBehaviour
{
    // BigInteger 스탯과 float 스탯을 구분해서 관리
    private Dictionary<StatType, StatBase> _floatStats = new();
    public Dictionary<StatType, StatBase> FloatStats => _floatStats;
    
    private Dictionary<StatType, BigIntStat> _bigIntStats = new();
    public Dictionary<StatType, BigIntStat> BigIntStats => _bigIntStats;

    public event Action<StatType, BigInteger> OnBigIntStatChanged;
    public event Action<StatType, float> OnFloatStatChanged;
    public event Action<BigInteger> OnHpChanged;

    private BigInteger _currentHp;
    private BigInteger _currentMaxHp;
    public BigInteger CurrentHp => _currentHp;
    
    public event Action<IDamageable, BigInteger, bool, IAttackable> OnDealDamage;

    public float HpPercent
    {
        get
        {
            BigInteger maxHp = GetBigIntValue(StatType.MaxHp);
            if (maxHp == 0) return 0f;
            // 안전한 나눗셈을 위해 double로 변환 후 계산
            return Mathf.Clamp01((float)((double)_currentHp / (double)maxHp));
        }
    }

    public void Initialize(UnitStatData data)
    {
        _floatStats.Clear();

        AddStat(new BigIntStat { type = StatType.MaxHp, baseValue = data.maxHP });
        AddStat(new BigIntStat { type = StatType.AttackPower, baseValue = data.attackPower });
        AddStat(new BigIntStat {type = StatType.HpRecovery,baseValue = data.hpRecovery } );
        AddStat(new StatBase { type = StatType.AttackSpeed, baseValue = data.attackSpeed });
        AddStat(new StatBase { type = StatType.MoveSpeed, baseValue = data.moveSpeed });
        AddStat(new StatBase { type = StatType.AttackRange, baseValue = data.attackRange });
        AddStat(new StatBase { type = StatType.CriticalChance, baseValue = data.criticalChance });
        AddStat(new StatBase { type = StatType.CriticalDamage, baseValue = data.criticalDamage });

        BigInteger maxHp = GetBigIntValue(StatType.MaxHp);
        _currentHp = (BigInteger)data.curHP > 0 && (BigInteger)data.curHP <= maxHp 
            ? (BigInteger)data.curHP : maxHp;
    
        OnHpChanged?.Invoke(_currentHp);
        //CombatPowerCalculator.Calculate();
    }


    public BigInteger GetBigIntValue(StatType type)
    {
        return _bigIntStats.TryGetValue(type, out var stat) ? stat.Value : 0;
    }
    public float GetFloatValue(StatType type)
    {
        return _floatStats.TryGetValue(type, out var stat) ? stat.Value : 0f;
    }

    public void SetBase(StatType type, BigInteger value)
    {
        if (_bigIntStats.TryGetValue(type, out var stat))
        {
            stat.SetBase(value);
            OnBigIntStatChanged?.Invoke(type, stat.Value);
        }
    }
    public void SetBase(StatType type, float value)
    {
        if (_floatStats.TryGetValue(type, out var stat))
        {
            stat.SetBase(value);
            OnFloatStatChanged?.Invoke(type, stat.Value);
        }
    }

    public void SetUpgrade(StatType type, BigInteger value)
    {
        if (_bigIntStats.TryGetValue(type, out var stat))
        {
            stat.SetUpgrade(value);
            OnBigIntStatChanged?.Invoke(type, stat.Value);
        }
    }
    
    public void SetUpgrade(StatType type, float value)
    {
        if (_floatStats.TryGetValue(type, out var stat))
        {
            stat.SetUpgrade(value);
            OnFloatStatChanged?.Invoke(type, stat.Value);
        }
    }

    public void AddToAdditiveBonus(StatType type, float amount)
    {
        if (_bigIntStats.TryGetValue(type, out var bigIntStat))
        {
            bigIntStat.AddToAdditiveBonus(amount);
            OnBigIntStatChanged?.Invoke(type, bigIntStat.Value);
        }
        else if (_floatStats.TryGetValue(type, out var floatStat))
        {
            floatStat.AddToAdditiveBonus(amount);
            OnFloatStatChanged?.Invoke(type, floatStat.Value);
        }
    }

    public void ApplyBuffMultiplier(StatType type, float multiplier)
    {
        if (_bigIntStats.TryGetValue(type, out var bigIntStat))
        {
            bigIntStat.MultiplyMultiplier(multiplier);
            OnBigIntStatChanged?.Invoke(type, bigIntStat.Value);

            if (type == StatType.MaxHp)
            {
                _currentHp = BigInteger.Min(_currentHp, bigIntStat.Value);
                OnHpChanged?.Invoke(_currentHp);
            }
        }
        else if (_floatStats.TryGetValue(type, out var floatStat))
        {
            floatStat.MultiplyMultiplier(multiplier);
            OnFloatStatChanged?.Invoke(type, floatStat.Value);
        }
    }

    public void RemoveBuffMultiplier(StatType type, float multiplier)
    {
        if (_bigIntStats.TryGetValue(type, out var bigIntStat))
        {
            bigIntStat.DivideMultiplier(multiplier);
            
            if (type == StatType.MaxHp)
            {
                _currentHp = BigInteger.Min(_currentHp, bigIntStat.Value);
                OnHpChanged?.Invoke(_currentHp);
            }
        }
        
        else if (_floatStats.TryGetValue(type, out var floatStat))
        {
            floatStat.DivideMultiplier(multiplier);
            OnFloatStatChanged?.Invoke(type, floatStat.Value);

        }
    }

    public void TakeDamage(BigInteger amount)
    {
        _currentHp -= amount;
        if(_currentHp < 0) _currentHp = 0;
        OnHpChanged?.Invoke(_currentHp);
    }

    public void Heal(BigInteger amount)
    {
        _currentHp += amount;
        if(_currentHp > GetBigIntValue(StatType.MaxHp)) _currentHp = GetBigIntValue(StatType.MaxHp);
        OnHpChanged?.Invoke(_currentHp);
    }

    public void ResetHP()
    {
        _currentHp = GetBigIntValue(StatType.MaxHp);
        OnHpChanged?.Invoke(_currentHp);
    }
    

    public void RestoreFullHp()
    {
        ResetHP();
    }

    public void UpdateCurrentHp()
    {
        BigInteger newMaxHp = GetBigIntValue(StatType.MaxHp);
        Heal(newMaxHp - _currentMaxHp);
        _currentMaxHp = newMaxHp;
    }

    public void Reset()
    {
        foreach (var stat in _bigIntStats.Values)
        {
            stat.Reset();
        }
        foreach (var stat in _floatStats.Values)
            stat.Reset();
        
        //_currentHp = GetValue(StatType.MaxHp);
        //OnHpChanged?.Invoke(_currentHp);
    }

    public void AddStat(BigIntStat stat)
    {
        if(!_bigIntStats.ContainsKey(stat.type))
            _bigIntStats.Add(stat.type, stat);
    }
    public void AddStat(StatBase stat)
    {
        if (!_floatStats.ContainsKey(stat.type))
            _floatStats.Add(stat.type, stat);
    }

    public void SetGoldDungeonMaxHp(BigInteger value)
    {
        if (_bigIntStats.TryGetValue(StatType.MaxHp, out var stat))
        {
            stat.SetBase(value);
            stat.Reset();
            _currentHp = stat.Value;
            OnBigIntStatChanged?.Invoke(StatType.MaxHp, stat.Value);
            OnHpChanged?.Invoke(_currentHp);
        }
        else
        {
            AddStat(new BigIntStat { type = StatType.MaxHp, baseValue = value });
            _currentHp = value;
            OnBigIntStatChanged?.Invoke(StatType.MaxHp, value);
            OnHpChanged?.Invoke(_currentHp);
        }
    }

    public void InvokeDealDamage(IDamageable target, BigInteger damage, bool isCritical, IAttackable attacker)
    {
        OnDealDamage?.Invoke(target, damage, isCritical, attacker);
    }
}
