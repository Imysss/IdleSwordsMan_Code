using System.Numerics;
using static Define;

[System.Serializable]
public class BigIntStat 
{
    public StatType type;

    public BigInteger baseValue;
    public BigInteger upgradeValue = 0;
    public float additiveBonus = 0.0f; // 가산 보너스 
    public float multiplier = 1.0f;    // 곱연산 버프

    public BigInteger Value
    {
        get
        {
            // 1. BigInteger 끼리 먼저 계산
            BigInteger finalValue = baseValue + upgradeValue;

            // 2. 비율 보너스 적용
            // BigInteger와 float을 직접 연산할 수 없으므로, double로 변환 후 계산
            double calculatedValue = (double)finalValue * (1.0 + additiveBonus) * multiplier;
            
            // 3. 다시 BigInteger로 변환하여 반환
            return (BigInteger)calculatedValue;
        }
    }
    
    //기본값은 데이터로 설정만 하기
    public void SetBase(BigInteger value)
    {
        baseValue = value;
    }

    //골드 업그레이드
    public void SetUpgrade(BigInteger value)
    {
        upgradeValue = value;
    }
    public void AddUpgrade(BigInteger amount)
    {
        upgradeValue += amount;
    }
    
    //보유/장착 가산 효과
    public void SetAdditiveBonus(float value)
    {
        additiveBonus = value;
    }
    public void AddToAdditiveBonus(float amount)
    {
        additiveBonus += amount;
    }
    
    //버프 배율
    public void SetMultiplier(float value)
    {
        multiplier = value;
    }
    public void MultiplyMultiplier(float amount)
    {
        multiplier *= amount;
    }
    public void DivideMultiplier(float amount)
    {
        multiplier /= amount;
    }

    public void Reset()
    {
        upgradeValue = 0;
        additiveBonus = 0f;
        multiplier = 1f;
    }
}
