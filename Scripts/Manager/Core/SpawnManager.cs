using System.Collections.Generic;
using System.Linq;
using Assets.FantasyMonsters.Common.Scripts;
using Data;
using UnityEngine;

public class SpawnManager
{
    private SpawnPoint _spawnPoint;
    private List<GameObject> _liveEnemies = new List<GameObject>();  //현재 생성된 적 리스트 
    private int _enemyCount = 0;
    private bool _spawnEnabled = true;
    private StageData _currentStageData; // 이전 스테이지 데이터 저장용
    private Dictionary<int, BossHpData> _bossHpDataDic = new Dictionary<int, BossHpData>();
    private Dictionary<int, BossAtkData> _bossAtkDataDic = new Dictionary<int, BossAtkData>();

    public SpawnPoint SpawnPoint => _spawnPoint;

    public void Init()
    {
        if (!Managers.Resource.Instantiate("SpawnPoints").TryGetComponent<SpawnPoint>(out _spawnPoint))
        {
            Debug.LogWarning("Cannot instantiate SpawnPoint");
            return;
        }
        
        _bossHpDataDic = Managers.Data.BossHpDataDic;
        _bossAtkDataDic = Managers.Data.BossAtkDataDic;

    }

    public void SetEnable(bool enabled)
    {
        _spawnEnabled = enabled;
    }

    // 웨이브 데이터를 받아와서 몬스터 웨이브 스폰
    public void SpawnWave(StageData data)
    {
        if (!_spawnEnabled) return;

        _currentStageData = data; // 현재 웨이브 정보 저장

        for (int i = 0; i < data.monsterCount; i++)
        {
            // 몬스터 리스트 중 랜덤으로 스폰
            int rand = Random.Range(0, data.creatureID.Count);
            GameObject monster = SpawnMonster(data.creatureID[rand], true);

             // 랜덤 위치 스폰 여부(보스는 랜덤스폰 X)
            if (!data.isBossWave)
            {
                float xOffset = _spawnPoint.MonsterSpawnPoint.position.x + Random.Range(-1.0f, 4f);
                float yOffset = _spawnPoint.MonsterSpawnPoint.position.y;
                if (i > 0)
                {
                    float magnitude = Mathf.Ceil((float)i / 2.0f) * 0.3f;
            
                    float sign = (i % 2 == 1) ? 1f : -1f;
            
                    yOffset += magnitude * sign;
                }
                monster.transform.position = new Vector2(xOffset, yOffset);
                
                if (monster.TryGetComponent<LayerManager>(out var layerManager))
                {
                    int sortingOrder = 300 - (int)(monster.transform.position.y * 100f);
                    layerManager.SetSortingGroupOrder(sortingOrder);
                }
            }
        }
    }

    // 일반 몬스터 스폰 (EnemyController가 붙은 객체)
    public GameObject SpawnMonster(int monsterId, bool autoInit = true)
    {
        GameObject monster = Managers.Resource.Instantiate(monsterId.ToString(), null, true);
        if (monster == null)
        {
            Debug.LogError($"{monsterId} 몬스터를 생성하지 못했습니다.");
            return null;
        }

        _liveEnemies.Add(monster);
        _enemyCount++;

        if (monster.TryGetComponent<EnemyController>(out EnemyController enemyController))
        {
            enemyController.OnDie += OnEnemyDie;

            if (autoInit)
                enemyController.Init(Managers.Data.CreatureDataDic[monsterId]);
        }
        
        monster.transform.position = _spawnPoint.MonsterSpawnPoint.position;
        return monster;
    }

    // 골드 던전에서 사용하는 골드 더미 스폰 (GoldDummyController 기반)
    // public GameObject SpawnGoldDummy(string dummyId)
    // {
    //     GameObject dummy = Managers.Resource.Instantiate(dummyId.ToString(), null, true);
    //     if (dummy == null)
    //     {
    //         Debug.LogError($"{dummyId} 골드 더미 생성 실패");
    //         return null;
    //     }
    //
    //     _liveEnemies.Add(dummy);
    //     
    //     dummy.transform.position = _spawnPoint.MonsterSpawnPoint.position;
    //     return dummy;
    // }

    private void OnEnemyDie(GameObject enemy)
    {
        
        if (!_liveEnemies.Contains(enemy)) return;
        
        _liveEnemies.Remove(enemy);

        if (--_enemyCount <= 0)
        {
            _liveEnemies.Clear();
            if (!Managers.Dungeon.IsDungeonActive)
            {
                Managers.Level.OnWaveCleared();
            }
        }
    }

    public void ClearAllEnemies()
    {
        foreach (GameObject obj in _liveEnemies.ToList())
        {
            if (obj == null) continue;

            if (obj.TryGetComponent<EnemyController>(out var controller))
            {
                controller.CleanupBeforeDisable();
            }
            Managers.Pool.Push(obj); // 기타 객체는 바로 반환
        }

        _enemyCount = 0;
        _liveEnemies.Clear();
    }

    // 이전 웨이브를 다시 시작
    public void RestartLastWave()
    {
        if (_currentStageData != null)
        {
            SpawnWave(_currentStageData);
        }
        else
        {
            Debug.LogWarning("이전 웨이브 정보가 없습니다.");
        }
    }

    #region 테스트용 함수
    public void SpawnMonsterForTest(int monsterId, int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnMonster(monsterId);
        }
    }

    public void ReturnAllMonstersForTest()
    {
        foreach (GameObject obj in _liveEnemies.ToList())
        {
            Managers.Pool.Push(obj);
        }

        _enemyCount = 0;
        _liveEnemies.Clear();
    }
    #endregion
}
