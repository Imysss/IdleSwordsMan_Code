using UnityEngine;

public interface IPoolObject
{
    string PoolID { get; }
    int PoolSize { get; }
    GameObject GameObject { get; }

    void OnReturnToPool();
}

