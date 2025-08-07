using UnityEngine;

public interface IDamageable
{
    bool IsDead { get; }
    Transform Transform { get; }
    void TakeDamage(IAttackable attacker);  // 변경된 부분
    
    bool IsAlive();
}