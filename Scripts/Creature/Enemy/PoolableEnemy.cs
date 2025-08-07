using UnityEngine;
using static Define;

public class PoolableEnemy : MonoBehaviour, IPoolObject
{
    [SerializeField] private string poolID = "Enemy";
    [SerializeField] private int poolSize = 10;

    public string PoolID => poolID;
    public int PoolSize => poolSize;
    public GameObject GameObject => gameObject;

    public void OnReturnToPool()
    {
        var stat = GetComponent<StatManager>();
        stat.RestoreFullHp();
    }
}