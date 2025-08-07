using System.Collections.Generic;
using static Define;

[System.Serializable]
public class StatBase
{
    public StatType type;
    
    public float baseValue; //초기 기본값 (데이터 기반)
    public float upgradeValue = 0f; //골드 업그레이드로 증가한 값
    public float _additiveBonus = 0f; //보유 + 장착 효과 
    public float _multiplier = 1f; //스킬 버프 계수

    public float Value => (baseValue + upgradeValue) * (1f + _additiveBonus) * _multiplier;

    //기본값은 데이터로 설정만 하기
    public void SetBase(float value)
    {
        baseValue = value;
    }

    //골드 업그레이드
    public void SetUpgrade(float value)
    {
        upgradeValue = value;
    }
    public void AddUpgrade(float amount)
    {
        upgradeValue += amount;
    }
    
    //보유/장착 가산 효과
    public void SetAdditiveBonus(float value)
    {
        _additiveBonus = value;
    }
    public void AddToAdditiveBonus(float amount)
    {
        _additiveBonus += amount;
    }
    
    //버프 배율
    public void SetMultiplier(float value)
    {
        _multiplier = value;
    }
    public void MultiplyMultiplier(float amount)
    {
        _multiplier *= amount;
    }
    public void DivideMultiplier(float amount)
    {
        _multiplier /= amount;
    }

    public void Reset()
    {
        upgradeValue = 0f;
        _additiveBonus = 0f;
        _multiplier = 1f;
    }
}