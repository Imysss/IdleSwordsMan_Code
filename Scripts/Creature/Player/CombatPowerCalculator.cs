using UnityEngine;
using System.Collections.Generic;
using System.Numerics;

public static class CombatPowerCalculator
{
    // 계산된 전투력을 캐싱해서 UI 등에서 조회할 수 있게 함
    private static double _cachedPower = 0;

    /// <summary>
    /// 외부에서 전투력 값을 가져올 때 사용 (계산된 값을 반환)
    /// </summary>
    public static double Get()
    {
        return _cachedPower;
    }

    /// <summary>
    /// 전투력 재계산 및 저장
    /// </summary>
    public static double Calculate()
    {
        var stat = Managers.Player.PlayerStat;
        
        if (stat == null || stat.FloatStats == null || stat.FloatStats.Count == 0)
        {
            Debug.LogWarning("CombatPowerCalculator: StatManager가 초기화되지 않음");
            _cachedPower = 0;
            return _cachedPower;
        }

        double playerPower = CalculatePlayerStatPower(stat);

        // 최종 전투력 저장
        _cachedPower = playerPower;
        
        //이벤트 추가
        EventBus.Raise(new CombatPowerChangedEvent());
        
        return _cachedPower;
    }

    /// <summary>
    /// 캐릭터 기본 스탯에 따른 전투력 계산
    /// </summary>
    private static double CalculatePlayerStatPower(StatManager stat)
    {
        double atk = (double)stat.GetBigIntValue(Define.StatType.AttackPower);
        double hp = (double)stat.GetBigIntValue(Define.StatType.MaxHp);
        double regen = (double)stat.GetBigIntValue(Define.StatType.HpRecovery);
        float atkSpd = stat.GetFloatValue(Define.StatType.AttackSpeed);
        float critRate = stat.GetFloatValue(Define.StatType.CriticalChance);
        float critDmg = stat.GetFloatValue(Define.StatType.CriticalDamage);

        return atk * 1.0f
             + hp * 0.3f
             + regen * 0.5f
             + atk * atkSpd * 0.2f
             + atk * critRate * critDmg * 0.5f;
    }
}
