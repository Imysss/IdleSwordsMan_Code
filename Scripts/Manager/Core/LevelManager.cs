using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Data;
using static Define;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private GameObject _currentBackground;
    private SpawnManager _spawnManager;
    private Dictionary<int, BossHpData> _bossHpDataDic;
    private Dictionary<int, BossAtkData> _bossAtkDataDic;
    private Coroutine _waveTransitionCoroutine;
    private Coroutine _bossTransitionCoroutine;
    private UIBossEffect _uiBossEffect;
    
    [Header("연출 설정")]
    [SerializeField] private float transitionDuration = 1.5f;     //웨이브 클리어 시 이동 연출 시간
    [SerializeField] private float bossTransitionDuration = 3.0f;     //보스 웨이브 시작 시 이동 연출 시간

    private int _currentStageKey;
    private Dictionary<int, StageData> _stageData;
    private SaveLoadManager _saveLoad;
    
    private ParallaxController _currentParallaxController;
    public ParallaxController CurrentParallaxController => _currentParallaxController;
    
    // 보스 도전을 기다리는 중인지 확인하는 플래그
    public bool isWaitingForBossChallenge = false;
    
    public StageData CurrentStageData{get; private set;}
    
    // 현재 진행 상태 변수
    private int _currentLoop = 0;
    private int _currentMapIndex = 1;
    private int _currentStageIndex = 1;
    private int _currentWaveIndex = 1;
    
    public (int loop, int map, int stage, int wave) GetCurrentProgress()
    {
        return (_currentLoop, _currentMapIndex, _currentStageIndex, _currentWaveIndex);
    }

    // 코루틴 중복 방지를 위한 플래그
    private bool _isProcessingNextWave = false;

    // 던전 입장 전 진행 상태 저장용
    private struct LevelProgressSnapshot
    {
        public int mapIndex;
        public int stageIndex;
        public int waveIndex;
    }

    private LevelProgressSnapshot? _savedProgress;

    public void Init()
    {
        _saveLoad = Managers.SaveLoad;
        _stageData = Managers.Data.StageDataDic;
        _spawnManager = Managers.Spawn;
        _bossHpDataDic = Managers.Data.BossHpDataDic;
        _bossAtkDataDic = Managers.Data.BossAtkDataDic;

        // SaveData에서 저장 데이터 불러오기
        if (_saveLoad.hasSaveData)
        {
            CurrentStageData = _stageData[_saveLoad.SaveData.stageLevel];
            _currentLoop = _saveLoad.SaveData.currentLoop;
            _currentMapIndex = CurrentStageData.mapIdx;
            _currentStageIndex = CurrentStageData.stageIdx;
            _currentWaveIndex = CurrentStageData.waveIdx;
            isWaitingForBossChallenge = _saveLoad.SaveData.isWaitingForBossChallenge;
        }
        
        _currentStageKey = GetStageKey();
        CurrentStageData = _stageData[_currentStageKey];
        Managers.UI.ShowFirstPopupUI<UIParticleEffect>();
        EventBus.Subscribe<GameStartEvent>(OnGameStartHandler);
    }
    
    // 게임 시작 시 첫 웨이브를 스폰하는 함수 
    public void StartCurrentWave()
    {
        _currentStageKey = GetStageKey();
        if (_stageData == null || !_stageData.ContainsKey(_currentStageKey))
        {
            Debug.LogError($"StageDataDic에 키 {_currentStageKey}가 없습니다. StageData가 없거나 잘못된 진행 위치입니다.");
            return;
        }

        CurrentStageData = _stageData[_currentStageKey];
        ChangeMapTheme();

        if (CurrentStageData.isBossWave)
        {
            Managers.Player.Controller.CanAttack = true;
            _bossTransitionCoroutine = StartCoroutine(BossTransitionCoroutine());
        }
        else
        {
            Managers.Player.Controller.CanAttack = true;

            _spawnManager.SpawnWave(CurrentStageData);
        }
        EventBus.Raise(new TransitionEndEvent());
    }

    public void ChangeMap(string mapName)
    {    
        // 기존 배경 파괴
        if (_currentBackground != null)
        {
            Destroy(_currentBackground);
        }
        string mapKey = MapNameToAddressableKey[mapName];
        _currentBackground = Managers.Resource.Instantiate(mapKey);
        _currentParallaxController = _currentBackground.GetComponent<ParallaxController>();
    }

    // 맵 테마 변경
    private void ChangeMapTheme()
    {
        if (_currentMapIndex <= 0 || _currentMapIndex > 6)
        {
            Debug.LogError($"맵 테마 프리팹이 없거나, 맵 인덱스({_currentMapIndex})가 잘못되었습니다.");
            return;
        }
        
        // 기존 배경 파괴
        if (_currentBackground != null)
        {
            Destroy(_currentBackground);
        }

        string themeKey = MapNameToAddressableKey[CurrentStageData.mapName];
        _currentBackground = Managers.Resource.Instantiate(themeKey, this.transform , false);
    }
    
    // 웨이브 클리어 시 호출될 공용 함수
    public void OnWaveCleared()
    {
        if (_isProcessingNextWave) return;
        
        // '보스 재도전 대기' 상태인 경우, 현재 스테이지 반복
        if (isWaitingForBossChallenge)
        {
            Managers.Player.PlayerStat.ResetHP();
            _spawnManager.SpawnWave(CurrentStageData);
        }
        else
        {
            _waveTransitionCoroutine = StartCoroutine(ProcessNextWaveCoroutine());
        }

        if (CurrentStageData.isBossWave)
        {
            Managers.Sound.Play(Sound.Sfx, "Clear");
        }

        Save();
    }

    // 웨이브 실패 시 호출될 공용 함수
    public void OnWaveFailed()
    {
        _currentWaveIndex--;
        //보스전에서 패배한 경우, '재도전 대기' 상태로 전환
        if (CurrentStageData.isBossWave)
        {
            isWaitingForBossChallenge = true;
        }
        
        if (isWaitingForBossChallenge)
        {
            EventBus.Raise(new BossFailEvent());
        }
        if (_currentWaveIndex <= 0)
        {
            _currentWaveIndex = 1;
        }
        
        _spawnManager.ClearAllEnemies();
        StartCurrentWave();
    }
    
    // 웨이브 클리어 시 연출과 스폰을 순차적으로 처리하는 코루틴
    private IEnumerator ProcessNextWaveCoroutine()
    {
        // 적 사망 애니메이션 
        yield return new WaitForSeconds(1f);
        
        EventBus.Raise(new TransitionStartEvent());
        _isProcessingNextWave = true;

        Managers.Player.PlayerStat.ResetHP();
        
        bool mapChanged = false;

        _currentWaveIndex++;

        if (_currentWaveIndex > 6)      // 모든 웨이브 클리어
        {   
            EventBus.Raise(new StageClearedEvent(GetStageKey()));
            _currentWaveIndex = 6;
            Managers.Analytics.SendStageClearEvent(GetStageKey(), Managers.Time.TotalPlayTimeSec);
            _currentStageIndex++;
            _currentWaveIndex = 1;
           
            if (_currentStageIndex > 10)    // 모든 스테이지 클리어
            {
                mapChanged = true;
                _currentMapIndex++;
                _currentStageIndex = 1;
                
                if (_currentMapIndex > 6)   // 모든 맵 클리어
                {
                    _currentLoop++;
                    _currentMapIndex = 1;
                }
            }
        }

        _currentStageKey = GetStageKey();
        CurrentStageData = _stageData[_currentStageKey];
        
        // 맵 변경 시 연출
        if (mapChanged)
        {
            UIMapTransition ui = Managers.UI.ShowFirstPopupUI<UIMapTransition>();
            if (ui != null)
            {
                ui.SetInfo(CurrentStageData);
            }
            yield return new WaitForSeconds(transitionDuration * 2);
            ChangeMapTheme();
            if (ui != null)
            {
                Managers.UI.CloseFirstPopupUI(ui);
            }
        }
        else
        {
            if (CurrentStageData.isBossWave)
            {
                _bossTransitionCoroutine = StartCoroutine(BossTransitionCoroutine());
                yield break;
            }
            
            yield return new WaitForSeconds(transitionDuration);

        }
        
        _spawnManager.SpawnWave(CurrentStageData);

        _isProcessingNextWave = false;
        EventBus.Raise(new WaveClearEvent());
        EventBus.Raise(new TransitionEndEvent());
    }

    // 보스 웨이브 입장 연출
    private IEnumerator BossTransitionCoroutine()
    {
        Managers.Equipment.SkillEquipment.ResetAllCooldowns(); // 스킬 초기화
        
        Managers.Sound.Play(Sound.Sfx, "BossWave", 0.9f, true);
        EventBus.Raise(new BossTransitionEvent());
        CreatureData creature = Managers.Data.CreatureDataDic[CurrentStageData.creatureID[0]];
        
        //보스 전환 연출 UI 활성화
        _uiBossEffect = Managers.UI.ShowFirstPopupUI<UIBossEffect>();
        if (_uiBossEffect != null)
        {
            _uiBossEffect.SetInfo(creature);
        }
        
        yield return new WaitForSeconds(bossTransitionDuration);
        if (_uiBossEffect != null)
        {
            Managers.UI.CloseFirstPopupUI(_uiBossEffect);
        }
        _uiBossEffect = null;
        Managers.Pool.PushAllOfType<DamageText>();
        
        _spawnManager.SpawnWave(CurrentStageData);

        _isProcessingNextWave = false;
        Managers.Sound.Stop(Sound.Sfx);
        EventBus.Raise(new WaveClearEvent());
        EventBus.Raise(new TransitionEndEvent());
        EventBus.Raise(new BossTransitionEndEvent());
    }
    
    // UI의 '보스 재도전' 버튼이 호출할 메서드
    public void StartBossWave()
    {
        if (!isWaitingForBossChallenge) return;
        
        HPBarManager.Instance?.ClearAll();
        _spawnManager.ClearAllEnemies();
        
        Managers.Player.PlayerStat.ResetHP();
        
        isWaitingForBossChallenge = false;
        _currentWaveIndex = 5;
        _bossTransitionCoroutine = StartCoroutine(ProcessNextWaveCoroutine());
    }
    
    // 몬스터 스펙의 기준이 되는 현재 레벨 반환용 함수 => 1~180
    public int GetCurrentLevel()
    {
        // 현재까지 깬 스테이지 수를 기반으로 계산
        return (_currentLoop * 60) + ((_currentMapIndex - 1) * 10) + (_currentStageIndex);
    }

    public int GetStageKey()
    {
        return 1000 + (_currentMapIndex-1) * 100 + (_currentStageIndex-1) * 10 + (_currentWaveIndex-1);
    }

    // 현재 스테이지의 보스 웨이브 키값을 반환
    public int GetCurrentBossStageKey()
    {
        return 1000 + (_currentMapIndex - 1) * 100 + (_currentStageIndex - 1) * 10 + 5;
    }

    public (BigInteger hp, BigInteger atk, BigInteger gold) GetCurrentWaveEnemyStat()
    {
        int bossStageKey = GetCurrentBossStageKey();
        
        BigInteger maxHp, attack, gold;
        // 몬스터의 체력, 공격력을 현재 스테이지 기반으로 설정
        if (_currentLoop == 0)       // 일반 난이도
        {
            maxHp = NumberFormatter.Parse(_bossHpDataDic[bossStageKey].normalFormatted);
            attack = NumberFormatter.Parse(_bossAtkDataDic[bossStageKey].normalFormatted);
        }
        else if (_currentLoop == 1)      // 어려움 난이도
        {
            maxHp = NumberFormatter.Parse(_bossHpDataDic[bossStageKey].hardFormatted);
            attack = NumberFormatter.Parse(_bossAtkDataDic[bossStageKey].hardFormatted);
        }
        else       // 매우 어려움 난이도, 혹은 그 이상
        {
            maxHp = NumberFormatter.Parse(_bossHpDataDic[bossStageKey].veryHardFormatted);
            attack = NumberFormatter.Parse(_bossAtkDataDic[bossStageKey].veryHardFormatted);
        }

        gold = maxHp;
        
        // 보스 웨이브가 아닌 경우 스탯 세부조정
        if (!CurrentStageData.isBossWave)
        {
            maxHp /= 10;
            attack /= 10;
            gold /= 30;
        }
        else
        {
            gold /= 10;
        }

        return (maxHp, attack, gold);
    }

    // 현재 진행 상태 저장
    public void SaveCurrentProgress()
    {
        _savedProgress = new LevelProgressSnapshot
        {
            mapIndex = _currentMapIndex,
            stageIndex = _currentStageIndex,
            waveIndex = _currentWaveIndex
        };
    }

    // 저장된 진행 상태 복원
    public void RestoreSavedProgress()
    {
        if (_savedProgress.HasValue)
        {
            var p = _savedProgress.Value;
            _currentMapIndex = p.mapIndex;
            _currentStageIndex = p.stageIndex;
            _currentWaveIndex = p.waveIndex;
        }
        else
        {
            Debug.LogWarning("저장된 진행 상태가 없습니다.");
        }
    }

    public void StopTransitionCoroutines()
    {
        if (_waveTransitionCoroutine != null)
        {
            StopCoroutine(_waveTransitionCoroutine);
            _waveTransitionCoroutine = null;
        }
        if (_bossTransitionCoroutine != null)
        {
            StopCoroutine(_bossTransitionCoroutine);
            _bossTransitionCoroutine = null;
            if (_uiBossEffect != null)
            {
                Managers.UI.CloseFirstPopupUI(_uiBossEffect);
            }
        }

        _isProcessingNextWave = false;
    }

    public string GetCurrentMapName()
    {
        DifficultyType difficulty = (DifficultyType)(_currentLoop);
        
        return difficulty.ToDescription();
    }

    private void Save()
    {
        _saveLoad.SaveData.stageLevel = GetStageKey();
        _saveLoad.SaveData.currentLoop = _currentLoop;
        _saveLoad.SaveData.isWaitingForBossChallenge = isWaitingForBossChallenge;
    }

    private void OnGameStartHandler(GameStartEvent evnt)
    {
        StartCurrentWave();
    }

    #region 테스트용

    public void MoveToStageTest(int mapIndex, int stageIndex)
    {
        if (mapIndex <= 0 || mapIndex > 6 || stageIndex <= 0 || stageIndex > 10)
        {
            Debug.LogError("잘못된 맵/스테이지 인덱스입니다. Map: `1~6, Stage: 1~10");
            return;
        }

        Debug.Log($"{mapIndex}번 맵, {stageIndex}번 스테이지로 이동합니다.");
        
        _spawnManager.ReturnAllMonstersForTest();
        _isProcessingNextWave = false;
        StopAllCoroutines();

        _currentMapIndex = mapIndex;
        _currentStageIndex = stageIndex;
        _currentWaveIndex = 1;
        
        ChangeMapTheme();
        StartCurrentWave();
    }

    public void StartWaveTest(int waveIndex)
    {
        if (waveIndex <= 0 || waveIndex > 6)
        {
            Debug.LogError($"잘못된 웨이브 인덱스입니다. 현재 스테이지의 Wave: 1~6");
            return;
        }

        Debug.Log($"현재 스테이지에서 {waveIndex}번 웨이브를 시작합니다.");
        
        _spawnManager.ReturnAllMonstersForTest();
        _isProcessingNextWave = false;
        StopAllCoroutines();

        _currentWaveIndex = waveIndex;
        
        StartCurrentWave();
    }

    public void StartNextWaveTest()
    {
        _spawnManager.ReturnAllMonstersForTest();
        OnWaveCleared();
    }

    #endregion
}
