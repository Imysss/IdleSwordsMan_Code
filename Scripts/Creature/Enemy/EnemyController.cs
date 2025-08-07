using System;
using System.Collections;
using System.Numerics;
using Assets.FantasyMonsters.Common.Scripts;
using Data;
using UnityEngine;
using static Define;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class EnemyController : BaseController
{
    [SerializeField] private GameObject goldDropPrefab;
    [SerializeField] private Monster monster;
    [SerializeField] private Transform hpBarAnchor;
    [SerializeField] private GameObject shadow;
    
    // AnchorStatus 포인트 연결

    private CreatureData _data;
    private Transform _playerTarget;
    private EnemyHPBar _hpBar;
    private bool _isBossDungeonEnemy;

    public Action<GameObject> OnDie;

    protected override void Awake()
    {
        base.Awake();
        Transform shadowTransform = transform.Find("Shadow");
        if (shadowTransform != null)
            shadow = shadowTransform.gameObject;
    }

    private void OnEnable()
    {
        monster.SetHead(0);
        IsDead = false;
        
        if (_playerTarget == null)
        {
            GameObject targetBox = GameObject.FindWithTag("PlayerTargetBox");
            if (targetBox != null)
                _playerTarget = targetBox.transform;
        }

        shadow.SetActive(true);
        monster.SetState(MonsterState.Walk);
    }

    private void OnDisable()
    {
        monster.Animator.Rebind();
        monster.Animator.Update(0f);
        OnDie = null;
        IsDead = true;

        // 조건 없이 HPBar 정리
        if (_hpBar != null)
        {
            HPBarManager.Instance.Detach(_hpBar);
            _hpBar = null;
        }
    }

    private Vector3 GetDynamicHPBarOffset()
    {
        // AnchorStatus 오브젝트를 기준으로 offset 계산
        if (hpBarAnchor != null)
        {
            return hpBarAnchor.position - transform.position;
        }

        // 없으면 기본값
        return new Vector3(0f, 1.5f, 0f);
    }

    public void Init(CreatureData creatureData, bool isBossDungeonEnemy = false)
    {
        _data = creatureData;
        _isBossDungeonEnemy = isBossDungeonEnemy;

        BigInteger maxHp = Managers.Level.GetCurrentWaveEnemyStat().hp;
        BigInteger attack = Managers.Level.GetCurrentWaveEnemyStat().atk;

        UnitStatData data = new UnitStatData
        {
            // 성장 체력, 공격력 적용
            maxHP = maxHp,
            attackPower = attack,
            attackRange = creatureData.attackRange,
            attackSpeed = creatureData.attackSpeed,
            moveSpeed = creatureData.moveSpeed
        };

        StatManager.Initialize(data);
    }

    public void Init(CreatureData creatureData, BigInteger hp, BigInteger attack, bool isBossDungeonEnemy = false)
    {
        _data = creatureData;
        _isBossDungeonEnemy = isBossDungeonEnemy;

        UnitStatData data = new UnitStatData
        {
            maxHP = hp,
            attackPower = attack,
            attackRange = creatureData.attackRange,
            attackSpeed = creatureData.attackSpeed,
            moveSpeed = creatureData.moveSpeed
        };

        StatManager.Initialize(data);
    }

    protected override void HandleAttack()
    {
        if (IsDead) return;

        float range = StatManager.GetFloatValue(StatType.AttackRange);
        float speed = StatManager.GetFloatValue(StatType.MoveSpeed);

        // 사거리 내에 타겟이 없으면 계속 직선 이동
        var target = FindTarget();
        if (target == null)
        {
            transform.position += Vector3.left * (speed * Time.deltaTime);
        }
        else
        {
            // 타겟 범위 내면 멈추고 공격 준비
            attackCooldown -= Time.deltaTime;
            if (attackCooldown <= 0f)
            {
                Attack(target);
                attackCooldown = StatManager.GetFloatValue(StatType.AttackSpeed);
            }
        }
    }

    protected override IDamageable FindTarget()
    {
        float range = StatManager.GetFloatValue(StatType.AttackRange);
        Vector2 position = transform.position;

        // 공격 범위 내의 모든 트리거 감지
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, range);

        foreach (var col in hits)
        {
            if (col.CompareTag("PlayerTargetBox"))
            {
                Transform playerRoot = col.transform.root;
                if (playerRoot.TryGetComponent(out IDamageable target))
                {
                    if (target is BaseController bc && bc.IsDead) return null;
                    return target;
                }
            }
        }

        return null;
    }

    protected override void Attack(IDamageable target)
    {
        if (IsDead) return;
        monster.OnAttack();
    }

    public void ApplyDamage()
    {
        if (IsDead) return;

        var target = FindTarget();
        if (target != null && target is BaseController bc && !bc.IsDead)
        {
            base.Attack(target);
        }
    }

    public override void TakeDamage(IAttackable attacker)
    {
        // 이미 죽었거나 죽을 예정이면 HPBar 생성 안함
        if (IsDead || StatManager.CurrentHp <= 0 || !gameObject.activeInHierarchy)
            return;

        if (_hpBar == null && HPBarManager.Instance != null)
        {
            Vector3 offset = GetDynamicHPBarOffset();
            _hpBar = HPBarManager.Instance.Attach(transform, StatManager, offset);
        }

        Managers.Sound.Play(Sound.Sfx, "EnemyHit");
        base.TakeDamage(attacker);
    }

    protected override void Die()
    {
        if (IsDead || !gameObject.activeSelf) return;
        IsDead = true;

        // HPBar 즉시 제거
        if (_hpBar != null)
        {
            HPBarManager.Instance.Detach(_hpBar);
            _hpBar = null;
        }

        monster.Die();
        shadow.SetActive(false);
        OnDie?.Invoke(gameObject);
        EventBus.Raise(new EnemyDiedEvent());

        SpawnGoldDrop();

        StartCoroutine(DelayedReturn(0.6f));
    }

    private IEnumerator DelayedReturn(float delay)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(delay);
        Managers.Pool.Push(gameObject);
    }
    
    // public void ForceDie()
    // {
    //     if (IsDead) return;
    //     OnDie = null;
    //     Die(); // 이미 내부에서 IsDead 체크하므로 중복 호출 안전
    // }

    private void SpawnGoldDrop()
    {
        if (_isBossDungeonEnemy) return;

        GameObject gold = Managers.Pool.Pop(goldDropPrefab);
        if (gold.TryGetComponent<GoldDrop>(out GoldDrop goldDrop))
        {
            Vector3 spawnPos = transform.position + new Vector3(0f, 0.8f, 0f);
            gold.transform.position = spawnPos;
            goldDrop.Init();
            // 적 골드 드랍량 설정
        }
    }

    public void SetHPBar(EnemyHPBar bar)
    {
        _hpBar = bar;
    }

    // Pool.OnRelease에서 호출될 초기화 함수
    public void ResetEnemy()
    {
        IsDead = false;
        monster.SetState(MonsterState.Walk);
    }
    
    public void CleanupBeforeDisable()
    {
        if (_hpBar != null)
        {
            // HPBarManager가 살아있을 때 Detach를 호출
            if (HPBarManager.Instance != null)
            {
                HPBarManager.Instance.Detach(_hpBar);
            }
            _hpBar = null;
        }
    }
}
