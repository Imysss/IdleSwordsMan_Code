using System;
using System.Collections;
using UnityEngine;
using Assets.FantasyMonsters.Common.Scripts;

public class GoldDummyController : BaseController
{
    [SerializeField] private Monster monster;

    private Transform _playerTransform;
    private EnemyHPBar _hpBar; // HP바 캐싱용
    private GoldDummyEffectController _effectController;

    public event System.Action<GameObject> OnDie; // 사망 이벤트 추가

    protected override void Awake()
    {
        base.Awake();
        _effectController = GetComponent<GoldDummyEffectController>();
        monster.SetState(MonsterState.Walk); // 시작 상태는 이동
        _playerTransform = GameObject.FindWithTag("Player")?.transform; // 플레이어 위치 참조
    }

    private void OnEnable()
    {
        // 오브젝트 풀에서 다시 꺼내졌을 때 필요한 초기화
        IsDead = false;
        monster.SetState(MonsterState.Walk);

        // HPBar 부착
        if (_hpBar != null)
        {
            HPBarManager.Instance.Detach(_hpBar);
            _hpBar = null;
        }

        if (HPBarManager.Instance != null)
        {
            _hpBar = HPBarManager.Instance.Attach(transform, StatManager, new Vector3(0, 2.7f, 0));
        }
    }
    protected virtual void OnDisable()
    {
        // HPBar 반환 처리
        if (_hpBar != null && HPBarManager.Instance != null)
        {
            HPBarManager.Instance.Detach(_hpBar);
            _hpBar = null;
        }
    }

    private void FixedUpdate()
    {
        if (IsDead) return;
        MoveToPlayer();
    }

    private void MoveToPlayer()
    {
        if (_playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, _playerTransform.position);
        float stopRange = 1.5f;

        if (distance > stopRange)
        {
            Vector2 dir = (_playerTransform.position - transform.position).normalized;
            float speed = 1f;
            transform.position += (Vector3)(dir * (speed * Time.fixedDeltaTime));
        }
        else
        {
            monster.SetState(MonsterState.Idle); // 도착 시 정지 애니메이션
        }
    }

    protected override void HandleAttack()
    {
        // 공격 안 함
    }

    protected override IDamageable FindTarget()
    {
        // 타겟 탐지 비활성
        return null;
    }

    protected override void Attack(IDamageable target)
    {
        // 공격 안 함
    }

    protected override void Die()
    {
        if (IsDead) return;
        IsDead = true;

        monster.Die();
        
        _effectController?.DropGoldOnHit(); // 골드 드랍 추가

        // HP바 제거
        if (_hpBar != null)
        {
            HPBarManager.Instance.Detach(_hpBar);
            _hpBar = null;
        }

        StartCoroutine(DelayedReturn(1f));
        OnDie?.Invoke(gameObject); // 사망 시 이벤트 호출
    }

    private IEnumerator DelayedReturn(float delay)
    {
        yield return new WaitForSeconds(delay);
        Managers.Pool.Push(gameObject);
    }
}
