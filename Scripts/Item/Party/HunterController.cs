using UnityEngine;

public class HunterController : PartyController
{
    protected override void Attack(IDamageable target)
    {
        base.Attack(target);
        _character.Firearm.StartFire();
    }
}
