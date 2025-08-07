using System;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using TMPro;
using UnityEngine;
using static Define;

public class ArcherController : PartyController
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
        //base.Attack(target);
        _target = target;
        _character.OnAttack(PartyType.Archer);
        // 아처 공격 사운드
        Managers.Sound.Play(Sound.Sfx, "ArrowAttack");
    }

    private void OnAnimationEvent(string eventName)
    {
        if (eventName == "ReleaseArrow")
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

