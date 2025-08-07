using System.Collections.Generic;
using System.Numerics;
using DG.Tweening;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class SkillProjectile : MonoBehaviour
{
    private float _speed;
    private BigInteger _damage;
    private float _critChance;
    private float _critMultiplier;
    private float _maxDistance;
    private int _targetCount;

    private Vector3 _startPos;
    private HashSet<IDamageable> _hitTargets = new();
    private bool _isActive = false;

    private static readonly Vector2 Direction = Vector2.right;

    public void Setup(BigInteger damage, float critChance, float critMultiplier, float speed, float range, int targetCount)
    {
        _damage = damage;
        _critChance = critChance;
        _critMultiplier = critMultiplier;
        _speed = speed;
        _maxDistance = range;
        _targetCount = targetCount;

        _hitTargets.Clear();
        _startPos = transform.position;
        _isActive = true;
    }

    private void FixedUpdate()
    {
        if (!_isActive) return;

        float moveDelta = _speed * Time.deltaTime;
        transform.Translate(Direction * moveDelta, Space.World);

        float traveledDistance = Vector3.Distance(_startPos, transform.position);
        if (traveledDistance >= _maxDistance)
        {
            EndProjectile();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isActive) return;
        
        if (other.TryGetComponent(out IDamageable target))
        {
            if (_hitTargets.Contains(target)) return;

            _hitTargets.Add(target);

            // 각 대상별로 치명타 개별 판정
            bool isCrit = UnityEngine.Random.value < _critChance;
            BigInteger finalDamage = _damage;

            if (isCrit)
                finalDamage = (BigInteger)((double)finalDamage * _critMultiplier);

            target.TakeDamage(new TempAttackWrapper(finalDamage, isCrit));

            if (Camera.main != null)
            {
                Camera.main.transform.DOComplete();
                Camera.main.transform.DOShakePosition(0.1f, 0.05f, 10, 45f, false);
            }
            
            if (_hitTargets.Count >= _targetCount)
                EndProjectile();
        }
    }

    private void EndProjectile()
    {
        _isActive = false;
        Managers.Pool.Push(gameObject);
    }
}
