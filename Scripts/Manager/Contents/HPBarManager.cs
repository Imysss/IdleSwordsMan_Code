using System.Collections.Generic;
using UnityEngine;

public class HPBarManager : MonoBehaviour
{
    public static HPBarManager Instance { get; private set; }

    [SerializeField] private GameObject enemyHPBarPrefab;

    private List<EnemyHPBar> activeBars = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    public EnemyHPBar Attach(Transform target, StatManager stat, Vector3 offset)
    {
        GameObject obj = Managers.Pool.Pop(enemyHPBarPrefab);
        
        //Debug.Log($"[HPBarManager.Attach] Pop된 HPBar: {obj.name}"); // 이거 꼭 찍어보기
        obj.transform.SetParent(WorldCanvas.Instance.UICanvas.transform, false);

        EnemyHPBar bar = obj.GetComponent<EnemyHPBar>();
        bar.ResetHPBar();
        bar.Initialize(target, stat, offset);
        activeBars.Add(bar);
        return bar;
    }

    public void Detach(EnemyHPBar bar)
    {
        if (bar == null) return;

        bar.ResetHPBar();

        if (activeBars.Contains(bar))
            activeBars.Remove(bar);

        Managers.Pool.Push(bar.gameObject);
    }
    
    public void ClearAll()
    {
        foreach (var bar in activeBars)
        {
            bar.ResetHPBar();
            bar.gameObject.name = "EnemyHPBar";
            Managers.Pool.Push(bar.gameObject);
        }
        activeBars.Clear();
    }
    
    
}