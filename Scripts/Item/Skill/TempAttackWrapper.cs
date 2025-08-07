using System.Numerics;
using UnityEngine;

public class TempAttackWrapper : IAttackable
{
    private BigInteger _damage;
    private bool _isCritical;

    public TempAttackWrapper(BigInteger damage, bool isCritical)
    {
        _damage = damage;
        _isCritical = isCritical;
    }

    public bool IsCritical()
    {
        return _isCritical;
    }

    public BigInteger GetAttackPower() => _damage;
    public Transform Transform => null;
}