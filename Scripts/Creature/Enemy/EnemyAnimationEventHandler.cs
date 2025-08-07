using UnityEngine;

public class EnemyAnimationEventHandler : MonoBehaviour
{
    private EnemyController _enemy;

    private void Awake()
    {
        _enemy = GetComponentInParent<EnemyController>();
    }

    public void ApplyDamage()
    {
        _enemy?.ApplyDamage();
    }
}