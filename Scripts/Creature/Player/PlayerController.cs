using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

public class PlayerController : BaseController
{
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject _hpBarObject;
    [SerializeField] private AnimationEvents animationEvents;

    private IDamageable _cachedTarget;
    private Character _character; 

    public bool CanAttack { get; set; } = true;
    public bool IsSkillCasting { get; private set; }
    private bool _isPlayingSkill = false;
    
    // 플레이어 자동 회복
    private float _recoveryTimer;
    private const float RecoveryInterval = PLAYER_RECOVERY_INTERVAL;

    // 현재 캐스팅 중인 스킬을 저장
    private Skill _currentCastingSkill;

    // 글로벌 스킬 딜레이용 플래그
    private bool _isGlobalSkillDelay = false;
    
    public Animator Animator => animator;

    protected override void Awake()
    {
        base.Awake();
        
        _character = GetComponent<Character>();
        var hpBar = _hpBarObject.GetComponent<PlayerHPBar>();
        hpBar.Init(transform, StatManager);
        
        CombatPowerCalculator.Calculate();
        Managers.UI.RefreshCombatPowerUI?.Invoke();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<TransitionStartEvent>(OnTransitionStart);
        EventBus.Subscribe<TransitionEndEvent>(OnTransitionEnd);
        EventBus.Subscribe<DungeonClearedEvent>(OnDungeonCleared);
        EventBus.Subscribe<GameStartEvent>(OnGameStartHandler);
        
        if (animationEvents != null)
        {
            animationEvents.OnCustomEvent += OnAnimationEvent;
        }
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<TransitionStartEvent>(OnTransitionStart);
        EventBus.UnSubscribe<TransitionEndEvent>(OnTransitionEnd);
        EventBus.UnSubscribe<DungeonClearedEvent>(OnDungeonCleared);
        EventBus.UnSubscribe<GameStartEvent>(OnGameStartHandler);
        
        if (animationEvents != null)
        {
            animationEvents.OnCustomEvent -= OnAnimationEvent;
        }
    }

    protected override void Update()
    {
        base.Update();
        
        // 체력 회복 로직
        if (_recoveryTimer > RecoveryInterval)
        {
            StatManager.Heal(StatManager.GetBigIntValue(StatType.HpRecovery));
            _recoveryTimer = 0;
        }
        else
        {
            _recoveryTimer += Time.deltaTime;
        }
    }

    // 공격할 대상을 탐색하는 함수
    protected override IDamageable FindTarget()
    {
        Collider2D[] results = Physics2D.OverlapCircleAll(transform.position,
            StatManager.GetFloatValue(StatType.AttackRange),
            LayerMask.GetMask("Enemy"));

        // 가장 가까운 타겟 찾기
        return results
            .Select(col => col.GetComponent<IDamageable>()) // 1. 감지된 모든 콜라이더를 IDamageable로 변환
            .Where(target => target != null && target.IsAlive()) // 2. 살아있는 유효한 타겟만 필터링
            .OrderBy(target => Vector2.Distance(transform.position, target.Transform.position)) // 3. 거리가 가까운 순서로 정렬
            .FirstOrDefault(); // 4. 가장 첫 번째(가장 가까운) 타겟을 반환 (없으면 null)
    }

    protected override void HandleAttack()
    {
        if (IsDead || IsSkillCasting) return;

        attackCooldown -= Time.deltaTime;

        var target = FindTarget();
        if (target == null || !IsInRange(target)) return;

        // 스킬이 사용 가능하고 캐스팅 중이 아니라면 스킬 우선
        if (!IsSkillCasting &&  Managers.Player.SkillExecutor.TryExecuteAvailableSkill(transform.position))
        {
            attackCooldown = 1f / StatManager.GetFloatValue(StatType.AttackSpeed); // 쿨타임 리셋
            return;
        }

        // 평타 처리
        if (attackCooldown <= 0f)
        {
            Attack(target);
            attackCooldown = 1f / StatManager.GetFloatValue(StatType.AttackSpeed);
        }
    }
    
    // 공격 애니메이션 실행 및 타겟 캐싱
    protected override void Attack(IDamageable target)
    {
        if (!CanAttack) return;
        
        if (animator != null)
        {
            float attackSpeed = StatManager.GetFloatValue(StatType.AttackSpeed);
            float clampedSpeed = Mathf.Clamp(attackSpeed, 0.5f, 2.0f);
            animator.SetFloat("UpperBodySpeed", clampedSpeed);
            animator.SetTrigger("Slash");

            //플레이어 공격 사운드
            Managers.Sound.Play(Sound.Sfx, "PlayerAttack");
        }

        _cachedTarget = target;
    }

    // 애니메이션 이벤트로부터 데미지를 적용하는 함수
    public void ApplyDamage()
    {
        if (_cachedTarget == null || _cachedTarget.IsDead) return;

        BigInteger baseDamage = StatManager.GetBigIntValue(StatType.AttackPower);
        bool isCritical = Random.value < StatManager.GetFloatValue(StatType.CriticalChance);

        BigInteger finalDamage = isCritical
            ? (BigInteger)((double)baseDamage * StatManager.GetFloatValue(StatType.CriticalDamage))
            : baseDamage;

        //IDamageable target = FindTarget();
        if (_cachedTarget != null && _cachedTarget.Transform.gameObject.activeInHierarchy)
        {
            var wrapper = new TempAttackWrapper(finalDamage, isCritical);
            _cachedTarget.TakeDamage(wrapper);
            StatManager.InvokeDealDamage(_cachedTarget, finalDamage, isCritical, this);
        }
    }

    // 스킬 애니메이션을 실행하고 현재 스킬 캐싱
    public void PlaySkillAnimation(Skill skill, string triggerName)
    {
        if (_isPlayingSkill) return;
        if (string.IsNullOrEmpty(triggerName)) return;

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        if (currentState.IsName(triggerName))
        {
            return;
        }

        IsSkillCasting = true;
        _isPlayingSkill = true;

        _currentCastingSkill = skill;
        animator.SetTrigger(triggerName);

        float duration = skill.GetTotalSkillDelay();
        StartCoroutine(ResetSkillFlag(duration));
    }

    private IEnumerator ResetSkillFlag(float time)
    {
        yield return new WaitForSeconds(time);
        _isPlayingSkill = false;
    }

    // 사망 처리
    protected override void Die()
    {
        IsDead = true;
        
        _character.OnDie();
        
        // if (_hpBarObject != null)
        //     Destroy(_hpBarObject);
        
        EventBus.Raise(new PlayerDieEvent());
        StartCoroutine(DieRoutine());
    }

    private void Revive()
    {
        IsDead = false;
        StatManager.ResetHP();
        _character.SetExpression("Default");
        _character.ResetAnimation();
        Managers.Equipment.SkillEquipment.ResetAllCooldowns();
        EventBus.Raise(new PlayerReviveEvent());
    }

    private IEnumerator DieRoutine()
    {
        // 플레이어 사망 시 1초 후 GameOverUI 팝업
        yield return new WaitForSeconds(1f);
        if (Managers.Dungeon.IsDungeonActive)
        {
            // 던전에서 사망 시 던전 중단
            yield return new WaitForSeconds(1f);
        }
        else
        {
            Managers.Sound.Play(Sound.Sfx, "PlayerDeath");
            if (!Managers.Tutorial.IsTutorialActive)
            {   // 튜토리얼 진행 상태 아닐떄만 게임오버 팝업
                 Managers.UI.ShowPopupUI<UIGameOver>();
            }
            yield return new WaitForSeconds(1f);
            Managers.Level.OnWaveFailed();
        }
        Revive();
    }

    // 글로벌 스킬 딜레이 여부 확인 함수
    public bool IsGlobalSkillDelay()
    {
        return _isGlobalSkillDelay;
    }
    
    public void ResetState()
    {
        // 죽은 상태 해제
        IsDead = false;

        // 체력 초기화
        StatManager.ResetHP();

        // 애니메이션 초기화
        _character.SetExpression("Default");
        _character.ResetAnimation();

        // 스킬 상태 초기화
        IsSkillCasting = false;
        _isPlayingSkill = false;
        _currentCastingSkill = null;

        // 상태 전이
        _character.SetState(CharacterState.Ready);

        // 이벤트 재발행
        EventBus.Raise(new PlayerReviveEvent());
    }
    

    // 딜레이 시작 함수
    public void StartGlobalSkillDelay(float delay)
    {
        StartCoroutine(GlobalSkillDelayRoutine(delay));
    }

    private IEnumerator GlobalSkillDelayRoutine(float delay)
    {
        _isGlobalSkillDelay = true;
        yield return new WaitForSeconds(delay);
        _isGlobalSkillDelay = false;
    }

    #region Event Handlers

    private void OnTransitionStart(TransitionStartEvent e)
    {
        _character.SetState(CharacterState.Walk);
    }

    private void OnTransitionEnd(TransitionEndEvent e)
    {
        _character.SetState(CharacterState.Idle);
        _character.SetExpression("Default");
    }

    private void OnAnimationEvent(string eventName)
    {
        if (eventName == "Hit")
        {
            ApplyDamage();
        }
        else if (eventName == "SkillFire")
        {
            _currentCastingSkill?.Fire();
        }
        else if (eventName == "SkillEnd")
        {
            IsSkillCasting = false;
            _currentCastingSkill = null;
        }
    }

    private void OnDungeonCleared(DungeonClearedEvent e)
    {
        _character.OnVictory();
        _character.SetExpression("Happy");
    }

    private void OnGameStartHandler(GameStartEvent evnt)
    {
        StatManager.ResetHP();
    }

    #endregion
}
