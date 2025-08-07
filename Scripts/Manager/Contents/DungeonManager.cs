using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using BigInteger = System.Numerics.BigInteger;
using static Define;

public class DungeonManager : MonoBehaviour
{
    public int maxGoldDungeonStage;
    public int maxBossDungeonStage;

    [SerializeField] private Transform spawnPoint;
    private GameManager _gameManager;
    private SpawnManager _spawnManager;
    private SaveLoadManager _saveLoad;
    private SoundManager _soundManager;

    private float transitionDuration = 1.5f; // 던전 전환 연출 시간

    private Coroutine dungeonCoroutine;

    private bool _goldDummyDestroyed;
    private int _currentBossIndex;
    private float _timer;
    public float RemainingTime => _timer;
    private bool _isDungeonActive;
    private bool _isTransitioning;
    public bool IsDungeonActive => _isDungeonActive;
    private bool _currentBossDead;

    private List<GameObject> _spawnedBoss = new List<GameObject>();

    private DungeonType _currentDungeonType;
    private int _currentDungeonLevel;

    private void Update()
    {
        if (!_isDungeonActive || _isTransitioning) return;

        _timer -= Time.deltaTime;

        // 플레이어가 죽으면 즉시 실패 처리
        if (Managers.Player?.Controller?.IsDead == true)
        {
            FailDungeon();
        }
    }

    public void Init()
    {
        _gameManager = Managers.Game;
        _saveLoad = Managers.SaveLoad;
        _spawnManager = Managers.Spawn;
        _soundManager = Managers.Sound;

        maxBossDungeonStage = _saveLoad.SaveData.bossDungeonLevel;
        maxGoldDungeonStage = _saveLoad.SaveData.goldDungeonLevel;

        spawnPoint = _spawnManager.SpawnPoint.MonsterSpawnPoint;
        if (_spawnManager == null)
            Debug.Log("[DungeonManager] MonsterSpawner is null");
        if (spawnPoint == null)
            Debug.Log("[DungeonManager] SpawnPoint is null");
    }

    public void StartDungeon(DungeonType type, int level)
    {
        if (dungeonCoroutine != null || _isDungeonActive)
            return;

        // 진행 상태 저장
        Managers.Level.SaveCurrentProgress();

        // 던전 상태 초기화
        _timer = DUNGEON_DURATION;
        _isDungeonActive = true;
        _goldDummyDestroyed = false;
        _currentBossDead = false;
        _currentBossIndex = 0;
        _spawnedBoss.Clear();
        _currentDungeonType = type;
        _currentDungeonLevel = level;

        // 적 제거 및 체력, 쿨타임 초기화
        _spawnManager.ClearAllEnemies();
        Managers.Level.StopTransitionCoroutines();
        if (Managers.Player != null && Managers.Player.PlayerStat != null)
        {
            Managers.Player.PlayerStat.ResetHP();
            Managers.Equipment.SkillEquipment.ResetAllCooldowns();
        }

        HPBarManager.Instance?.ClearAll();

        // 던전 타입에 따라 루틴 시작
        switch (type)
        {
            case DungeonType.Gold:
                if (!Managers.Data.GoldDungeonDataDic.TryGetValue(level, out var goldData))
                    return;
                dungeonCoroutine = StartCoroutine(GoldDungeonRoutine(goldData, level));
                break;
            case DungeonType.Boss:
                if (!Managers.Data.BossDungeonDataDic.TryGetValue(level, out var bossData))
                    return;
                dungeonCoroutine = StartCoroutine(BossDungeonRoutine(bossData, level));
                break;
        }

        // 던전 BGM 재생
        Managers.Sound.Play(Define.Sound.Bgm, "BGM_Dungeon");

        EventBus.Raise(new DungeonStateChangedEvent());
    }

    // 골드 던전 루틴
    private IEnumerator GoldDungeonRoutine(GoldDungeonData dungeonData, int level)
    {
        // 맵 변경 및 연출
        Managers.Level.ChangeMap(dungeonData.map);
        
        yield return null;
        
        yield return StartCoroutine(PlayTransition(transitionDuration));

        // 골드 더미 소환
        GameObject dummy = _spawnManager.SpawnMonster(dungeonData.dummyId, false);

        if (dummy == null)
            yield break;

        if (dummy.TryGetComponent<StatManager>(out var stat))
        {
            BigInteger goldBossHp = NumberFormatter.Parse(dungeonData.hp);
            stat.SetGoldDungeonMaxHp(goldBossHp);
        }

        if (dummy.TryGetComponent<GoldDummyController>(out var dummyCtrl))
        {
            dummyCtrl.OnDie += HandleGoldDummyDead;
        }

        _spawnedBoss.Add(dummy);

        // 시간 또는 더미 파괴 대기
        while (_timer > 0f && !_goldDummyDestroyed)
        {
            yield return null;
        }

        if (!_isDungeonActive) yield break;

        if (_goldDummyDestroyed)
        {
            //보상 지급
            BigInteger reward = NumberFormatter.Parse(dungeonData.reward);
            _gameManager.AddGold(reward);
            _spawnManager.ClearAllEnemies();

            yield return StartCoroutine(HandleDungeonClear(DungeonType.Gold, level));
        }
        else
        {
            FailDungeon();
        }
    }

    // 보스 던전 루틴
    private IEnumerator BossDungeonRoutine(BossDungeonData dungeonData, int level)
    {
        // 맵 전환 및 입장 연출
        Managers.Level.ChangeMap(dungeonData.map);
        
        yield return null;

        while (_currentBossIndex < dungeonData.bossIds.Count && _timer > 0f)
        {
            yield return StartCoroutine(PlayTransition(transitionDuration));

            EventBus.Raise(new DungeonBossWaveStartEvent(_currentBossIndex));
            
            _currentBossDead = false;

            int bossId = dungeonData.bossIds[_currentBossIndex];

            BigInteger bossHp = NumberFormatter.Parse(dungeonData.bossHp[_currentBossIndex]);
            BigInteger bossAtk = NumberFormatter.Parse(dungeonData.bossAtk[_currentBossIndex]);

            GameObject boss = _spawnManager.SpawnMonster(bossId, autoInit: false);

            if (boss.TryGetComponent<EnemyController>(out var ec))
            {
                ec.Init(Managers.Data.CreatureDataDic[bossId], bossHp, bossAtk, true);
                ec.OnDie += HandleBossDead;
            }

            _spawnedBoss.Add(boss);

            // 죽거나 타이머가 끝날 때까지 대기
            yield return new WaitUntil(() => _currentBossDead || _timer <= 0f);
            yield return new WaitForSeconds(1f);

            _currentBossIndex++;
        }

        if (!_isDungeonActive) yield break;

        if (_currentBossIndex >= dungeonData.bossIds.Count)
        {
            //보상 지급
            _gameManager.AddGem(dungeonData.reward);
            yield return StartCoroutine(HandleDungeonClear(DungeonType.Boss, level));
        }
        else
        {
            FailDungeon();
        }
    }
    private IEnumerator PlayTransition(float duration)
    {
        EventBus.Raise(new TransitionStartEvent());

        // ParallaxController가 준비될 때까지 대기
        float waitTimer = 0f;
        while (Managers.Level.CurrentParallaxController == null && waitTimer < 1f)
        {
            waitTimer += Time.deltaTime;
            yield return null;
        }

        Managers.Level.CurrentParallaxController?.StartMovement();

        _isTransitioning = true;
        yield return new WaitForSeconds(duration);
        _isTransitioning = false;

        Managers.Level.CurrentParallaxController?.StopMovement();

        EventBus.Raise(new TransitionEndEvent());
    }

    // 던전 클리어 처리
    private IEnumerator HandleDungeonClear(DungeonType type, int level)
    {
        if (type == DungeonType.Gold && level >= maxGoldDungeonStage)
        {
            maxGoldDungeonStage = level + 1;
            _saveLoad.SaveData.goldDungeonLevel = maxGoldDungeonStage;
        }
        else if (type == DungeonType.Boss && level >= maxBossDungeonStage)
        {
            maxBossDungeonStage = level + 1;
            _saveLoad.SaveData.bossDungeonLevel = maxBossDungeonStage;
        }

        _gameManager.TryUseDungeonTicket(type);

        EventBus.Raise(new DungeonClearedEvent(type, level));

        yield return new WaitForSeconds(2.0f);

        _soundManager.Play(Sound.Sfx, "Victory");
        Managers.UI.ShowPopupUI<UIDungeonClearPopup>().SetInfo(type, level);
    }

    // 골드 더미 사망 시 처리
    private void HandleGoldDummyDead(GameObject dummy)
    {
        _goldDummyDestroyed = true;
        _spawnedBoss.Remove(dummy);
    }

    // 보스 사망 시 처리
    private void HandleBossDead(GameObject boss)
    {
        _currentBossDead = true;
        _spawnedBoss.Remove(boss);
        //여기에 이벤트 하나 호출해 주기
    }

    // 던전 실패 처리
    private void FailDungeon()
    {
        if (!_isDungeonActive) return;

        _isDungeonActive = false;

        if (dungeonCoroutine != null)
        {
            StopCoroutine(dungeonCoroutine);
            dungeonCoroutine = null;
        }
        
        EventBus.Raise(new DungeonFailedEvent());

        Managers.UI.ShowPopupUI<UIDungeonFailPopup>().SetInfo(_currentDungeonLevel);
        _soundManager.Play(Sound.Sfx, "LoseTrumpet");
    }

    // 던전 종료 처리 (성공/실패 공통)
    public void EndDungeon()
    {
        if (!_isDungeonActive) return;
        
        _isDungeonActive = false;

        if (dungeonCoroutine != null)
        {
            StopCoroutine(dungeonCoroutine);
            dungeonCoroutine = null;
        }

        _spawnManager.ClearAllEnemies();
        _spawnedBoss.Clear();

        HPBarManager.Instance?.ClearAll();
        Managers.Pool.PushAllOfType<DamageText>();

        if (Managers.Player?.Controller != null)
        {
            Managers.Player.Controller.CanAttack = false;

            Animator animator = Managers.Player.Controller.Animator;
            if (animator != null)
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                AnimatorTransitionInfo transition = animator.GetAnimatorTransitionInfo(0);

                if (state.IsName("Victory") || transition.IsName("Victory"))
                {
                    animator.Play("IdleMelee");
                }
            }
        }

        EventBus.Raise(new DungeonStateChangedEvent());

        Managers.Level.RestoreSavedProgress();
        Managers.Level.StartCurrentWave();

        Managers.Sound.Play(Define.Sound.Bgm, "BGM_Game");
    }

    // 해당 던전이 잠금 해제되었는지 확인
    public bool IsDungeonUnlocked(DungeonType type, int level)
    {
        return type switch
        {
            DungeonType.Gold => maxGoldDungeonStage >= level,
            DungeonType.Boss => maxBossDungeonStage >= level,
            _ => false,
        };
    }
}
