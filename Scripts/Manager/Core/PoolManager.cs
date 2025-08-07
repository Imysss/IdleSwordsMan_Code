using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

//자주 생성되는 UI/몬스터/이펙트 등 오브젝트를 재사용하여 GC와 Instantiate 비용 최소화
//ObjectPool<T> 기반으로 안정성과 커스터마이징 확보
//Unity 특유의 Destroy된 부모 참조로 인한 예외까지 구조적으로 차단

//장점
//1. 커스터마이징 기능: IObjectPool<T> 사용으로 콜백 네 가지 제어 가능 (OnCreate, OnGet 등)
//2. 안정성: Push 시 SetParent(Root) 처리로 부모 참조 문제 해결
//3. 일관성 있는 트리 구성: @프리팹명Pool GAmeObject 하위에 정리
//4. 중앙 집중 관리: PoolManager를 통해 모든 풀을 관리
class Pool
{
    //원본 프리팹
    GameObject _prefab;             //풀 대상 프리팹
    IObjectPool<GameObject> _pool;  //Unity 내장 풀 인터페이스 사용

    //Monster들은 @MonstersPool 부모 산하에 전부 생성되게 만들어서 깔끔하게 정리
    Transform _root;
    Transform Root
    {
        get
        {
            if (_root == null)
            {
                //이름 명시된 GameObject를 새로 만들어 하이어라키 정리
                GameObject go = new GameObject() { name = $"@{_prefab.name}Pool" };
                _root = go.transform;
            }

            return _root;
        }
    }

    //생성자(원본 프리펩 저장, 유니티에서 지정하는 pool을 생성(함수 네 가지를 받아줌)
    public Pool(GameObject prefab)
    {
        _prefab = prefab;
        //네 가지 콜백 지정: 생성 / 꺼낼 때 / 반납할 때 / 삭제할 때
        _pool = new ObjectPool<GameObject>(OnCreate, OnGet, OnRelease, OnDestroy);
    }

    //풀에 오브젝트 반납
    public void Push(GameObject go)
    {
        if (go.activeSelf)
            _pool.Release(go);
    }

    //풀에서 오브젝트 꺼내기
    public GameObject Pop()
    {
        return _pool.Get();
    }

    #region Funcs
    //오브젝트 생성
    GameObject OnCreate()
    {
        GameObject go = GameObject.Instantiate(_prefab);
        go.transform.SetParent(Root);
        go.name = _prefab.name;
        return go;
    }

    //오브젝트 꺼내기
    void OnGet(GameObject go)
    {
        go.SetActive(true);
        go.transform.localScale = Vector3.one;
    }

    //오브젝트 반납
    void OnRelease(GameObject go)
    {
        go.SetActive(false);
        go.transform.SetParent(Root);

        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;

        // 컴포넌트 초기화
        if (go.TryGetComponent(out DamageText dt)) dt.ResetText();
        if (go.TryGetComponent(out GoldDrop gd)) gd.ResetGold();
        if (go.TryGetComponent(out EnemyHPBar bar)) bar.ResetHPBar();
        if (go.TryGetComponent(out EnemyController ec)) ec.ResetEnemy();
    }

    //오브젝트 삭제
    private void OnDestroy(GameObject go)
    {
        GameObject.Destroy(go);
    }
    #endregion
}

public class PoolManager
{
    //프리팹 이름 기반 풀 딕셔너리
    Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();

    //prefab을 건네면 해당 prefab이 들어있는 objectpool이 있는지 확인한 다음, 그 풀에서 prefab을 꺼내서 활성화
    public GameObject Pop(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("PoolManager.Pop - prefab is null!");
            return null;
        }

        string key = prefab.name;
        
        //해당 pool이 생성되지 않았다면 pool 생성하기
        if (_pools.ContainsKey(prefab.name) == false)
            CreatePool(prefab);

        return _pools[prefab.name].Pop();   //꺼내기
    }

    //해당 오브젝트 go를 pool에 반납
    public bool Push(GameObject go)
    {
        //이름으로 못 찾으면 반납 불가
        if (_pools.ContainsKey(go.name) == false)
            return false;

        _pools[go.name].Push(go);
        return true;
    }

    void CreatePool(GameObject prefab)
    {
        //새로운 풀 등록
        Pool pool = new Pool(prefab);
        _pools.Add(prefab.name, pool);
    }

    public void Clear()
    {
        //필요 시 전체 풀 제거
        _pools.Clear();
    }
    
    public void PushAllOfType<T>() where T : Component
    {
        T[] instances = GameObject.FindObjectsOfType<T>(true);
        foreach (T instance in instances)
        {
            GameObject go = instance.gameObject;
            if (Push(go) == false)
            {
                // 못 밀었으면 그냥 비활성화라도
                go.SetActive(false);
            }
        }
    }
}
