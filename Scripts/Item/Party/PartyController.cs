using System;
using System.Data;
using System.Numerics;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using Data;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;

public class PartyController : BaseController
{
    public Transform firePos;
    private PartyState _partyState;
    protected Character _character;

    private void OnEnable()
    {
        EventBus.Subscribe<TransitionStartEvent>(OnTransitionStart);
        EventBus.Subscribe<TransitionEndEvent>(OnTransitionEnd);
        EventBus.Subscribe<PlayerDieEvent>(OnPlayerDie);
        EventBus.Subscribe<PlayerReviveEvent>(OnPlayerRevive);
        EventBus.Subscribe<DungeonClearedEvent>(OnDungeonCleared);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<TransitionStartEvent>(OnTransitionStart);
        EventBus.UnSubscribe<TransitionEndEvent>(OnTransitionEnd);
        EventBus.UnSubscribe<PlayerDieEvent>(OnPlayerDie);
        EventBus.UnSubscribe<PlayerReviveEvent>(OnPlayerRevive);
        EventBus.UnSubscribe<DungeonClearedEvent>(OnDungeonCleared);
    }

    public void Init(PartyState state)
    {
        _character = GetComponent<Character>();
        _partyState = state;
        UnitStatData data = new UnitStatData { attackPower = 10, attackRange = 5, attackSpeed = state.data.attackSpeed };
        StatManager.Initialize(data);
        _character.SetState(CharacterState.Ready);

        Initialize();
    }

    protected virtual void Initialize() { }

    // 플레이어와 같은 로직을 공유
    protected override IDamageable FindTarget()
    {
        Collider2D[] results = Physics2D.OverlapCircleAll(transform.position,
            StatManager.GetFloatValue(StatType.AttackRange),
            LayerMask.GetMask("Enemy"));

        foreach (var col in results)
        {
            if (col.TryGetComponent(out IDamageable target))
            {
                if (target is MonoBehaviour mb && mb.TryGetComponent<BaseController>(out var ctrl))
                {
                    if (ctrl.IsDead) continue;
                }
                return target;
            }
        }

        return null;
    }

    public override BigInteger GetAttackPower()
    {
        double attackPower = (double)Managers.Player.PlayerStat.GetBigIntValue(StatType.AttackPower)
                             * CalculateMultiplier();
        bool isCritical = Random.value < Managers.Player.PlayerStat.GetFloatValue(StatType.CriticalChance);
        attackPower = isCritical
            ? (attackPower * Managers.Player.PlayerStat.GetFloatValue(StatType.CriticalDamage))
            : attackPower;
        IsLastAttackCritical = isCritical;
        return (BigInteger)attackPower;
    }


    public override void TakeDamage(IAttackable attacker)
    {
        // 파티 캐릭터는 데미지를 받지 않음
    }

    private float CalculateMultiplier()
    {
        if (!Managers.Data.PartyLevelDataDic.TryGetValue(_partyState.dataId, out PartyLevelData levelData))
        {
            return 1f;
        }
        
        string formula = levelData.formula;
        float baseValue = levelData.baseValue;
        
        formula = formula.Replace("baseValue", baseValue.ToString());
        formula = formula.Replace("level", _partyState.level.ToString());

        try
        {
            var result = new DataTable().Compute(formula, null);
            return Convert.ToSingle(result);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"PartyLevelCalculator 계산 오류: {e.Message}");
            return 1f;
        }
    }
    
    // public void PlayVictoryAnimation()
    // {
    //     Animator animator = transform.Find("Animation")?.GetComponent<Animator>();
    //     if (animator == null)
    //     {
    //         Debug.LogError($"[{name}] Animator not found in child 'Animation'");
    //         return;
    //     }
    //
    //     Debug.Log($"[{name}] Playing Victory animation.");
    //     animator.Rebind(); // 상태 초기화 (혹시라도 꼬였을 경우 대비)
    //     animator.SetTrigger("Victory");
    //     StartCoroutine(ResetToIdleAfterVictory(animator));
    // }
    //
    // private IEnumerator ResetToIdleAfterVictory(Animator animator)
    // {
    //     yield return new WaitForSeconds(2f);
    //     animator.ResetTrigger("Victory");
    // }

    #region Event Handlers

    private void OnTransitionStart(TransitionStartEvent e)
    {
        _character.SetState(CharacterState.Walk);
    }

    private void OnTransitionEnd(TransitionEndEvent e)
    {
        _character.ResetAnimation();
        _character.SetState(CharacterState.Ready);
        _character.SetExpression("Default");
    }

    private void OnPlayerDie(PlayerDieEvent e)
    {
        IsDead = true;
        _character.OnDie();
    }

    private void OnPlayerRevive(PlayerReviveEvent e)
    {
        IsDead = false;
        _character.ResetAnimation();
        _character.SetState(CharacterState.Ready);
        _character.SetExpression("Default");
    }

    private void OnDungeonCleared(DungeonClearedEvent e)
    {
        _character.OnVictory();
        _character.SetExpression("Happy");
    }

    #endregion
}
