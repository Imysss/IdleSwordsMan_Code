using System.Numerics;
using UnityEngine;

public interface IAttackable
{
    BigInteger GetAttackPower();
    bool IsCritical();
    Transform Transform { get; }
}