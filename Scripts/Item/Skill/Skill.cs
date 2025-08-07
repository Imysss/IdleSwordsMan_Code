// 중략 생략 없이 전체 스크립트 제공
// Skill.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Data;
using UnityEngine;
using static Define;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using DG.Tweening;

public class Skill
{
    private SkillData _data;
    private float _cooldownTimer = 0f;
    private StatManager _casterStat;
    private bool _isBuffActive = false;

    // 캐시된 발동 정보
    private List<IDamageable> _cachedTargets;
    private Vector3 _cachedOrigin;
    private BigInteger _cachedDamage;

    public Skill(SkillData data, StatManager casterStat)
    {
        _data = data;
        _casterStat = casterStat;
    }

    // 스킬 사용 가능 여부 (쿨타임 + 버프 중복 방지)
    public bool IsReady => _cooldownTimer <= 0f && (!_isBuffActive || _data.type != SkillType.BuffSkill);
    public int ID => _data.dataId;
    public int Type => (int)_data.type;

    public void UpdateCooldown(float deltaTime)
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= deltaTime;
    }

    public bool TryExecute(Vector3 origin, List<IDamageable> enemiesInScene)
    {
        if (!IsReady)
            return false;

        if (_casterStat.TryGetComponent(out PlayerController player))
        {
            if (player.IsDead)
                return false;

            // 글로벌 딜레이가 걸려있으면 사용 금지
            if (player.IsGlobalSkillDelay())
                return false;

            // 글로벌 딜레이 시작
            player.StartGlobalSkillDelay(1f);
        }

        if (_data.executeType == SkillExecuteType.Buff)
        {
            // 적이 일정 거리 내에 있는 경우에만 발동하도록 검사
            List<IDamageable> validTargets = GetTargets(origin, enemiesInScene);
            if (validTargets.Count == 0)
                return false;

            ApplyBuff();
            PlaySkillSound();
            return true;
        }

        _cachedOrigin = origin;
        _cachedTargets = GetTargets(origin, enemiesInScene);

        if (_cachedTargets.Count == 0)
        {
            return false;
        }

        _cooldownTimer = _data.cooldown;

        double baseAttack = (double)_casterStat.GetBigIntValue(StatType.AttackPower);
        float multiplier = SkillLevelCalculator.CalculateMultiplier(_data.dataId, GetSkillLevel());
        double damage = baseAttack * multiplier;

        _cachedDamage = (BigInteger)damage;

        if (_casterStat.TryGetComponent(out player))
        {
            player.PlaySkillAnimation(this, _data.animTrigger);
            PlaySkillSound();
        }
        return true;
    }

    // 애니메이션 이벤트에서 호출될 실제 발동 함수
    public void Fire()
    {
        if (_casterStat.TryGetComponent(out PlayerController player))
        {
            if (player.IsDead)
                return;
        }

        switch (_data.executeType)
        {
            case SkillExecuteType.DoubleSlash:
                _casterStat.StartCoroutine(DoDoubleSlash(_cachedTargets[0], _cachedDamage));
                Camera.main.transform.DOShakePosition(0.1f, 0.1f, 10, 90f);
                break;

            case SkillExecuteType.Projectile:
                FireProjectile(_cachedOrigin, _cachedDamage);
                break;

            case SkillExecuteType.AreaRepeat:
                _casterStat.StartCoroutine(DoAreaDamageRepeated(_cachedOrigin, _cachedDamage));
                break;

            case SkillExecuteType.MultiHit:
                _casterStat.StartCoroutine(DoMultiHit(_cachedTargets[0], _cachedDamage));
                break;

            case SkillExecuteType.SingleHit:
            case SkillExecuteType.Default:
                int count = Mathf.Min(_data.targetCount, _cachedTargets.Count);
                for (int i = 0; i < count; i++)
                {
                    bool isCritical = UnityEngine.Random.value < _casterStat.GetFloatValue(StatType.CriticalChance);
                    BigInteger dmg = _cachedDamage;
                    if (isCritical)
                        dmg = (BigInteger)((double)dmg * _casterStat.GetFloatValue(StatType.CriticalDamage));

                    var attacker = new TempAttackWrapper(dmg, isCritical);
                    _cachedTargets[i].TakeDamage(attacker);
                }

                SkillEffectPlayer.Play(_data.effectName, _cachedTargets[0].Transform.position, 0.5f);

                if (Camera.main != null)
                {
                    Camera.main.transform.DOComplete(); // 기존 트윈 종료
                    Camera.main.transform.DOShakePosition(
                        duration: 0.15f,
                        strength: 0.2f,
                        vibrato: 10,
                        randomness: 90f,
                        snapping: false
                    );
                }
                break;
        }
    }

    private int GetSkillLevel()
    {
        var state = Managers.Inventory.GetItemState(_data.dataId);
        return state?.level ?? 1;
    }

    private List<IDamageable> GetTargets(Vector3 origin, List<IDamageable> enemiesInScene)
    {
        List<IDamageable> validTargets = new();
        foreach (var enemy in enemiesInScene)
        {
            if (enemy.IsDead) continue;
            var enemyTransform = (enemy as MonoBehaviour)?.transform;
            if (enemyTransform == null) continue;

            float dist = Vector3.Distance(origin, enemyTransform.position);
            if (dist <= _data.range)
                validTargets.Add(enemy);
        }
        return validTargets;
    }

    private IEnumerator DoDoubleSlash(IDamageable target, BigInteger damage)
    {
        var attacker1 = GetAttackerWithCrit(damage);
        target.TakeDamage(attacker1);
        SkillEffectPlayer.Play(_data.effectName, target.Transform.position);

        yield return new WaitForSeconds(_data.attackInterval);

        if (!target.IsDead)
        {
            var attacker2 = GetAttackerWithCrit(damage);
            target.TakeDamage(attacker2);
            SkillEffectPlayer.Play(_data.effectName, target.Transform.position);
        }
    }

    private IEnumerator DoMultiHit(IDamageable lockedTarget, BigInteger damage)
    {
        Transform targetTransform = (lockedTarget as MonoBehaviour)?.transform;
        if (targetTransform == null) yield break;

        for (int i = 0; i < _data.attackCount; i++)
        {
            if (lockedTarget.IsDead) break;

            var attacker = GetAttackerWithCrit(damage);
            lockedTarget.TakeDamage(attacker);

            float yOffset = i switch { 0 => 0f, 1 => 0.4f, 2 => -0.4f, _ => 0f };

            float totalEffectDuration = _data.attackInterval * (_data.attackCount - i) + 0.3f;
            SkillEffectPlayer.Play(_data.effectName, targetTransform.position + new Vector3(0, yOffset, 0), totalEffectDuration);

            yield return new WaitForSeconds(_data.attackInterval);
        }
    }

    private IEnumerator DoAreaDamageRepeated(Vector3 origin, BigInteger damage)
    {
        SkillEffectPlayer.Play(_data, _casterStat.transform.position);

        float timer = 0f;

        while (timer < _data.duration)
        {
            Collider[] cols = Physics.OverlapSphere(_casterStat.transform.position, _data.range);
            foreach (var col in cols)
            {
                if (col.TryGetComponent(out IDamageable enemy) && !enemy.IsDead)
                {
                    var attacker = GetAttackerWithCrit(damage);
                    enemy.TakeDamage(attacker);
                }
            }

            yield return new WaitForSeconds(_data.attackInterval);
            timer += _data.attackInterval;
        }
    }

    private void FireProjectile(Vector3 origin, BigInteger damage)
    {
        Vector2 direction = Vector2.right;
        Vector3 spawnOffset = new Vector3(0.5f, 0f, 0f);
        Vector3 spawnPos = origin + spawnOffset;

        GameObject prefab = Managers.Resource.Load<GameObject>($"{_data.effectName}");
        if (prefab != null)
        {
            GameObject go = Managers.Pool.Pop(prefab);
            go.transform.position = spawnPos;

            if (go.TryGetComponent(out SkillProjectile proj))
            {
                proj.Setup(damage, _casterStat.GetFloatValue(StatType.CriticalChance),
                    _casterStat.GetFloatValue(StatType.CriticalDamage),
                    _data.projSpeed, _data.projRange, _data.targetCount);
            }
        }
    }

    private void ApplyBuff()
    {
        _casterStat.ApplyBuffMultiplier(_data.stat, _data.buffValue);
        _isBuffActive = true;
        _cooldownTimer = _data.cooldown;
        _casterStat.StartCoroutine(RemoveBuffAfterDuration());

        // 버프 오브젝트 이펙트 재생
        SkillEffectPlayer.Play(_data, _casterStat.transform.position);

        // 그림자 분신 전용 이벤트 등록 (5012 스킬일 때만)
        if (_data.dataId == 5012)
            _casterStat.OnDealDamage += OnShadowCloneDamage;
    }

    private void OnShadowCloneDamage(IDamageable target, BigInteger damage, bool isCritical, IAttackable attacker)
    {
        // 본 공격자가 플레이어인지 확인
        if (attacker is not PlayerController)
        {
            return;
        }

        if (target == null || target.IsDead) return;

        // 추가 그림자 공격 딜레이로 실행
        _casterStat.StartCoroutine(DelayedShadowAttack(target, damage, isCritical));
    }

    private IEnumerator DelayedShadowAttack(IDamageable target, BigInteger damage, bool isCritical)
    {
        yield return new WaitForSeconds(0.2f); // 분신이 약간 늦게 공격

        if (target.IsDead) yield break;

        var attacker = new TempAttackWrapper(damage / 2, isCritical); // 0.5배 데미지
        target.TakeDamage(attacker);
    }

    private IEnumerator RemoveBuffAfterDuration()
    {
        yield return new WaitForSeconds(_data.duration);
        _casterStat.RemoveBuffMultiplier(_data.stat, _data.buffValue);
        _isBuffActive = false;

        if (_data.dataId == 5012)
            _casterStat.OnDealDamage -= OnShadowCloneDamage;
    }

    public float GetTotalSkillDelay()
    {
        return _data.executeType switch
        {
            SkillExecuteType.DoubleSlash => _data.attackInterval * 2f,
            SkillExecuteType.MultiHit => _data.attackInterval * _data.attackCount,
            SkillExecuteType.AreaRepeat => _data.duration,
            SkillExecuteType.Buff => _data.duration,
            SkillExecuteType.Projectile => 0.5f,
            _ => 1f
        };
    }

    public float GetCooldownRatio()
    {
        return Mathf.Clamp01(_cooldownTimer / _data.cooldown);
    }

    public void ResetCooldown()
    {
        _cooldownTimer = 0f;

        if (_isBuffActive)
        {
            _casterStat.RemoveBuffMultiplier(_data.stat, _data.buffValue);
            _isBuffActive = false;
        }
    }

    private void PlaySkillSound()
    {
        string clipKey = _data.clipName;

        if (string.IsNullOrEmpty(clipKey))
        {
            Debug.LogWarning($"SkillData {ID} has no clipName defined.");
            return;
        }

        Managers.Resource.LoadAsync<AudioClip>(clipKey, (clip) =>
        {
            if (clip != null)
            {
                Managers.Sound.Play(Define.Sound.Sfx, clip);
            }
            else
            {
                Debug.LogWarning($"Clip not found: {clipKey}");
            }
        });
    }
    
    public void StartCooldown()
    {
        _cooldownTimer = _data.cooldown;
    }
    

    private TempAttackWrapper GetAttackerWithCrit(BigInteger baseDamage)
    {
        bool isCrit = UnityEngine.Random.value < _casterStat.GetFloatValue(StatType.CriticalChance);
        if (isCrit)
            baseDamage = (BigInteger)((double)baseDamage * _casterStat.GetFloatValue(StatType.CriticalDamage));
        return new TempAttackWrapper(baseDamage, isCrit);
    }
}
