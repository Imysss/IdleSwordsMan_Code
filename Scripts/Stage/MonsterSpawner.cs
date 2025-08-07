using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    [SerializeField] private Transform spawnPoint;      //몬스터 스폰 위치
    [SerializeField] [Range(0f, 1f)] private float spawnOffsetX;   
    [SerializeField] [Range(0f, 2f)] private float spawnOffsetY; 
    
    private List<GameObject> _liveEnemies = new List<GameObject>();  //현재 생성된 적 리스트 
    private int _enemyCount = 0;
    
    private bool _spawnEnabled = true;

    private StageData _currentStageData; // 이전 스테이지 데이터 저장용

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
            SpawnMonster(data.creatureID[rand]);
        }
    }

    private void SpawnMonster(int monsterId)
    {
        GameObject monster = Managers.Resource.Instantiate(monsterId.ToString(), this.transform, true);
        if (monster == null)
        {
            Debug.LogError($"{monsterId} 몬스터를 생성하지 못했습니다.");
            return;
        }

        _liveEnemies.Add(monster);
        _enemyCount++;

        if (monster.TryGetComponent<EnemyController>(out EnemyController enemyController))
        {
            enemyController.OnDie += OnEnemyDie;
            enemyController.Init(Managers.Data.CreatureDataDic[monsterId]);
        }
        
        // 약간 랜덤한 위치에 생성
        Vector2 randomSpawnPoint = spawnPoint.position + 
                                   new Vector3(Random.Range(-spawnOffsetX, spawnOffsetX), Random.Range(-spawnOffsetY, spawnOffsetY));
        monster.transform.position = randomSpawnPoint;
    }

    private void OnEnemyDie(GameObject enemy)
    {
        if (!_liveEnemies.Contains(enemy)) return;
        
        if (enemy != null && enemy.TryGetComponent<EnemyController>(out EnemyController enemyController))
        {
            enemyController.OnDie -= OnEnemyDie; // 이벤트 해제
        }

        _liveEnemies.Remove(enemy);
        
        if (--_enemyCount <= 0)
        {
            _liveEnemies.Clear();
            Managers.Level.OnWaveCleared();
        }
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
           //ObjectPoolManager.Instance.ReturnObject(obj);
            Managers.Pool.Push(obj);
        }

        _enemyCount = 0;
        _liveEnemies.Clear();
    }
    #endregion
}
