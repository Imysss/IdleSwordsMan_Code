using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using UnityEngine;
using static Define;

public class MageController : PartyController
{
    public AnimationEvents animationEvents;
    private IDamageable _target;
    public string projectileName;

    protected override void Initialize()
    {
        animationEvents.OnCustomEvent += OnAnimationEvent;
    }

    protected override void Attack(IDamageable target)
    {
        _target = target;
        _character.OnAttack(PartyType.Mage);
        //마법사 동료 사운드
        Managers.Sound.Play(Sound.Sfx, "Fireball");
    }

    private void OnAnimationEvent(string eventName)
    {
        if (eventName == "CastMagic")
        {
            ShootProjectile();
        }
        else if (eventName == "DealDamage")
        {
            base.Attack(_target);
        }
    }

    private void ShootProjectile()
    {
        if (_target == null || string.IsNullOrEmpty(projectileName)) return;
        
        PoolableProjectile projectile;
        Managers.Resource
            .Instantiate(projectileName, transform, true)
            .TryGetComponent<PoolableProjectile>(out projectile);
        {
            projectile.transform.position = firePos.position;
            projectile.Fire(_target.Transform.position, 15.0f);
        }
    }
}
