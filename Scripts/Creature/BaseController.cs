using System.Numerics;
using UnityEngine;
using static Define;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(StatManager))]
public abstract class BaseController : MonoBehaviour, IAttackable, IDamageable
{
    [SerializeField] private Transform body;
    public StatManager StatManager { get; protected set; }
    protected float attackCooldown;

    public bool IsDead { get; set; }
    
    public Transform Transform => body != null ? body : transform;
    
    [SerializeField] private GameObject damageTextPrefab;

    protected bool IsLastAttackCritical;

    protected virtual void Awake()
    {
        StatManager = GetComponent<StatManager>();
    }

    protected virtual void Update()
    {
        HandleAttack();
    }

    protected virtual void HandleAttack()
    {
        if (IsDead || (this is PlayerController pc && pc.IsSkillCasting)) return;

        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0f)
        {
            var target = FindTarget();
            if (target != null && IsInRange(target))
            {
                Attack(target);
                attackCooldown = 1f / StatManager.GetFloatValue(StatType.AttackSpeed);
            }
        }
    }

    protected virtual void Attack(IDamageable target)
    {
        if (target == null || target.IsDead) return;
        target.TakeDamage(this);
    }

    public virtual void TakeDamage(IAttackable attacker)
    {
        BigInteger damage = attacker.GetAttackPower();
        
        StatManager.TakeDamage(damage);

        if (StatManager.CurrentHp <= 0 && !IsDead)
        {
            Die();
        }

        if (damageTextPrefab != null)
        {
            GameObject dmgObj = Managers.Pool.Pop(damageTextPrefab);
            if (dmgObj != null)
            {
                dmgObj.transform.SetParent(WorldCanvas.Instance.transform, false);
                var damageText = dmgObj.GetComponent<DamageText>();

                Vector3 offset = new Vector3(0, 2.2f, 0);
                damageText.Show(transform, damage, offset, attacker.IsCritical());
            }
        }
    }
    
    public BigInteger GetCurrentHp() => StatManager.CurrentHp;
    public void SetCurrentHp(BigInteger hp)
    {
        StatManager.Heal(hp - StatManager.CurrentHp);
    }

    protected virtual void Die()
    {
        IsDead = true;
        Destroy(gameObject);
    }

    protected bool IsInRange(IDamageable target)
    {
        float range = StatManager.GetFloatValue(StatType.AttackRange);
        return Vector3.Distance(transform.position, ((MonoBehaviour)target).transform.position) <= range;
    }

    public virtual BigInteger GetAttackPower()
    {
        BigInteger power = StatManager.GetBigIntValue(StatType.AttackPower);
        bool isCritical = Random.value < StatManager.GetFloatValue(StatType.CriticalChance);
        IsLastAttackCritical = isCritical;
        return isCritical ? (BigInteger)((double)power * StatManager.GetFloatValue(StatType.CriticalDamage)) : power;
    }

    public bool IsAlive()
    {
        return !IsDead;
    }

    public bool IsCritical()
    {
        bool isCritical = IsLastAttackCritical;

        IsLastAttackCritical = false;
        
        return isCritical;
    }

    protected abstract IDamageable FindTarget();
}
