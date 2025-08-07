using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using UnityEngine.InputSystem.DualShock;
using static Define;

// 새로운 이벤트 정의 (예시)


public class QuestManager
{
    //모든 퀘스트의 원본 데이터
    private Dictionary<int, QuestData> _questDatabase = new Dictionary<int, QuestData>();
    // 모든 퀘스트의 상태 데이터
    private Dictionary<int, QuestState> _questStates = new Dictionary<int, QuestState>();
    
    // 진행 상태인 모든 퀘스트를 담는 통합 리스트
    private Dictionary<QuestType, List<QuestState>> _inProgressQuests;
    
    // 카테고리별 퀘스트 데이터를 저장하는 딕셔너리들
    private Dictionary<int, QuestData> _mainQuestDatabase = new Dictionary<int, QuestData>();
    private Dictionary<int, QuestData> _dailyQuestDatabase = new Dictionary<int, QuestData>();
    private Dictionary<int, QuestData> _tutorialQuestDatabase = new Dictionary<int, QuestData>();
    
    //private Dictionary<int, QuestState> _eventQuestStates = new Dictionary<int, QuestState>();
    
    // 퀘스트 보상 
    private Dictionary<int, List<RewardData>> _rewardDatabase = new Dictionary<int, List<RewardData>>();

    public IReadOnlyDictionary<int, QuestState> QuestStates => _questStates;
    public IReadOnlyDictionary<int, QuestData> MainQuestDatabase => _mainQuestDatabase;
    public IReadOnlyDictionary<int, QuestData> DailyQuestDatabase => _dailyQuestDatabase;

    // 매핑 딕셔너리 모음
    private Dictionary<QuestType, StatType> _statQuestMap;
    private Dictionary<QuestType, DungeonType> _dungeonQuestMap;
    public QuestData CurrentMainQuest { get; private set; }

    private SaveLoadManager _saveLoad;
    private GameManager _gameManager;
    private GachaManager _gachaManager;

    public void Init()
    {
        _saveLoad = Managers.SaveLoad;
        _gameManager = Managers.Game;
        _gachaManager = Managers.Gacha;
        
        // 1. JSON 로드하여 QuestDatabase 채우기
        _mainQuestDatabase = Managers.Data.MainQuestDataDic;
        _dailyQuestDatabase = Managers.Data.DailyQuestDataDic;
        _tutorialQuestDatabase = Managers.Data.TutorialQuestDataDic;

        foreach (var reward in Managers.Data.QuestRewardDataDic.Values)
        {
            if (_rewardDatabase.ContainsKey(reward.questId))
            {
                _rewardDatabase[reward.questId].Add(reward);
            }
            else
            {
                _rewardDatabase[reward.questId] = new List<RewardData> { reward };
            }
        }
        
        //2. 모든 퀘스트 뎅이터를 하나의 마스터 데이터베이스에 통합
        _questDatabase = _mainQuestDatabase.Concat(_dailyQuestDatabase)
            .ToDictionary(x => x.Key, x => x.Value)
            .Concat(_tutorialQuestDatabase)
            .ToDictionary(x => x.Key, x => x.Value);

         // 3. 저장 데이터가 존재할 경우
         if (_saveLoad.hasSaveData)
         {
             // 저장된 플레이어의 퀘스트 진행도 불러와서 덮어쓰기
             _questStates = _saveLoad.SaveData.questStates;
         }
         // 저장 데이터가 존재하지 않을 경우
         else
         {
             // 모든 퀘스트에 대한 기본 QuestState 생성
             foreach (var questData in _questDatabase.Values)
             {
                 _questStates.Add(questData.key, new QuestState { questKey = questData.key });
             }
             _saveLoad.SaveData.questStates = _questStates;
         }
        // 4. 매핑 딕셔너리 초기화
        InitializeDict();
        
        // 5. 이벤트 구독
        SubscribeEvents();
        
        //6. 메인 퀘스트 시작
        ActivateQuest();
    }

    private void InitializeDict()
    {    
        _statQuestMap = new Dictionary<QuestType, Define.StatType>
        {
            { QuestType.UpMaxHp, StatType.MaxHp },
            { QuestType.UpHpGen, StatType.HpRecovery }, 
            { QuestType.UpAttackPower, StatType.AttackPower },
            { QuestType.UpAttackSpeed, StatType.AttackSpeed },
            { QuestType.UpCritChance, StatType.CriticalChance },
            { QuestType.UpCritDamage, StatType.CriticalDamage }
        };
        _dungeonQuestMap = new Dictionary<QuestType, DungeonType>()
        {
            { QuestType.BossDungeon , DungeonType.Boss},
            {QuestType.GoldDungeon, DungeonType.Gold}
        };
        
        _inProgressQuests = new Dictionary<QuestType, List<QuestState>>();
        foreach (QuestType type in Enum.GetValues(typeof(QuestType)))
        {
            _inProgressQuests.Add(type, new List<QuestState>());
        }

    }

    private void SubscribeEvents()
    {
        EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
        EventBus.Subscribe<GachaEvent>(OnGacha);
        EventBus.Subscribe<StatUpgradeEvent>(OnStatUpgrade);
        EventBus.Subscribe<StageClearedEvent>(OnStageCleared);
        EventBus.Subscribe<QuestClearedEvent>(OnQuestCleared);
        EventBus.Subscribe<DungeonClearedEvent>(OnDungeonCleared);
        EventBus.Subscribe<AdWatchedEvent>(OnAdWatched);
        EventBus.Subscribe<BossFailEvent>(OnBossFail);
    }
    
    // 상태 변경을 책임지는 단일 함수
    private void ChangeQuestStatus(int questKey, QuestStatus newStatus)
    {
        if (!_questStates.TryGetValue(questKey, out QuestState state)) return;
        
        QuestData data = _questDatabase[state.questKey];

        // 이미 같은 상태라면 변경하지 않음
        if (state.status == newStatus) return;

        state.status = newStatus;

        // 새로운 상태가 InProgress라면, 진행중 리스트에 추가하고 진행도 체크
        if (newStatus == QuestStatus.InProgress)
        {
            AddActivatedQuest(state);
            UpdateQuestProgress(state);
        }
        else if (newStatus == QuestStatus.Completed)
        {
            // 튜토리얼 퀘스트는 자동으로 클리어
            if (_tutorialQuestDatabase.ContainsKey(questKey))
            {
                CompleteQuest(questKey);
                return;
            }
        }
        else if (newStatus == QuestStatus.Rewarded)
        {
            _inProgressQuests[data.type].Remove(state);
        }
       
        UpdatePlayTimeQuests();
        EventBus.Raise(new QuestStateChangedEvent());
        // 데이터 저장
        Save(state);
    }

    // 퀘스트 완료 여부를 확인하고 상태를 변경하는 함수
    private void CheckQuestCompletion(QuestState state)
    {
        if (state.currentCount >= _questDatabase[state.questKey].conditionCount)
        {
            ChangeQuestStatus(state.questKey, QuestStatus.Completed);
        }
        EventBus.Raise(new QuestStateChangedEvent());
    }
    
    // 모든 일일퀘스트 시작
    public void ResetDailyQuest()
    {
        foreach (var data in _dailyQuestDatabase.Values)
        {
            QuestState state = _questStates[data.key];
            if (state.status != QuestStatus.Inactive) return;

            ChangeQuestStatus(data.key, QuestStatus.InProgress);
        }
    }
    
    private void ActivateQuest()
    {
        // 데일리 퀘스트 활성화
        foreach (var data in _dailyQuestDatabase.Values)
        {
            QuestState state = _questStates[data.key];
            if (state.status is QuestStatus.InProgress or QuestStatus.Completed)
            {
                AddActivatedQuest(state);
            }
        }
        
        // 튜토리얼 퀘스트 활성화
        foreach (var data in _tutorialQuestDatabase.Values)
        {
            QuestState state = _questStates[data.key];
            if (state.status is QuestStatus.Inactive)
            {
                state.status = QuestStatus.InProgress;
            }

            if (state.status is QuestStatus.InProgress)
            {
                AddActivatedQuest(state);
            }
        }
        
        // 메인 퀘스트 활성화
        foreach (var data in _mainQuestDatabase.Values)
        {
            QuestState state = _questStates[data.key];
            if (state.status is QuestStatus.InProgress or QuestStatus.Completed)
            {
                AddActivatedQuest(state);
                CurrentMainQuest = _mainQuestDatabase[data.key];
                return;
            }
        }
        //활성화 된 메인 퀘스트가 없을 경우, 첫 번째 퀘스트 활성화
        CurrentMainQuest = _mainQuestDatabase[_mainQuestDatabase.Keys.First()]; 
        ChangeQuestStatus(CurrentMainQuest.key, QuestStatus.InProgress);
    }
        
    /// <summary>
    /// 퀘스트 시작 시, 이미 달성했을 수 있는 진행도를 업데이트
    /// </summary>
    private void UpdateQuestProgress(QuestState state)
    {
        QuestData data = _questDatabase[state.questKey];
        
        switch (data.type)
        {
            case QuestType.StageClear:
                state.currentCount = Managers.Level.GetStageKey() + Managers.Level.GetCurrentProgress().loop * 1000;
                break;
            case QuestType.EnemyKill:
                return;
            case QuestType.Login :
                state.currentCount = 1;
                break;
            case QuestType.GearGet:
                state.currentCount = Managers.Gacha.GetGachaCount(GachaType.Gear);
                break;
            case QuestType.SkillGet:
                state.currentCount = Managers.Gacha.GetGachaCount(GachaType.Skill);
                break;
            case QuestType.PartyGet:
                state.currentCount = Managers.Gacha.GetGachaCount(GachaType.Party);
                break;
            case QuestType.UpMaxHp :
                state.currentCount = Managers.StatUpgrade.GetLevel(StatType.MaxHp);
                break;
            case QuestType.UpHpGen:
                state.currentCount = Managers.StatUpgrade.GetLevel(StatType.HpRecovery);
                break;
            case QuestType.UpAttackPower: 
                state.currentCount = Managers.StatUpgrade.GetLevel(StatType.AttackPower);
                break;
            case QuestType.UpAttackSpeed:
                state.currentCount = Managers.StatUpgrade.GetLevel(StatType.AttackSpeed);
                break;
            case QuestType.UpCritChance:
                state.currentCount = Managers.StatUpgrade.GetLevel(StatType.CriticalChance);
                break;
            case QuestType.UpCritDamage:
                state.currentCount = Managers.StatUpgrade.GetLevel(StatType.CriticalDamage);
                break;
            case QuestType.BossDungeon:
                state.currentCount = Managers.Dungeon.maxBossDungeonStage - 1;
                break;
            case QuestType.GoldDungeon:
                state.currentCount = Managers.Dungeon.maxGoldDungeonStage - 1;
                break;
            case QuestType.WatchAD:
                break;
            case QuestType.PlayTime:
                state.currentCount = (int)(Managers.Time.DailyPlayTimeSec / 60);
                break;
            default:
                break;
        }

        CheckQuestCompletion(state); // 진행도 업데이트 후, 바로 완료되었는지 체크
    }

    private void AddActivatedQuest(QuestState state)
    {
        QuestData data = _questDatabase[state.questKey];

        if (_inProgressQuests.TryGetValue(data.type, out var questList))
        {
            // 이미 리스트에 포함되어 있지 않은 경우에만 추가
            if (!questList.Contains(state))
            {
                questList.Add(state);
            }
        }
        else
        {
            _inProgressQuests.Add(data.type, new List<QuestState>{state});
        }
    }

    // '퀘스트 보상 지급 버튼' 클릭 시 호출
    public void CompleteQuest(int questKey)
    {
        // 퀘스트 완료 상태인지 확인
        if (_questStates[questKey].status != QuestStatus.Completed) return;
        
        QuestData data = _questDatabase[questKey];
        
        // 일단은 보석만 지급하기로...
        //Managers.Game.AddGem(data.rewardNum);
        
        // 퀘스트 보상 데이터가 있다면 지급, 없으면 스킵
        if (_rewardDatabase.TryGetValue(data.key, out List<RewardData> rewards))
        {
            //퀘스트에 연결된 모든 보상을 순차적으로 지급
            foreach (RewardData reward in rewards)
            {
                if (reward == null) continue;

                switch (reward.rewardType)
                {
                    case RewardType.Gold:
                        _gameManager.AddGold(reward.amount);
                        break;
                    case RewardType.Gem:
                        _gameManager.AddGem(reward.amount);
                        break;
                    case RewardType.BossDungeonTicket:
                        _gameManager.AddDungeonTicket(DungeonType.Boss, reward.amount);
                        break;
                    case RewardType.GoldDungeonTicket:
                        _gameManager.AddDungeonTicket(DungeonType.Gold, reward.amount);
                        break;
                    case RewardType.EquipmentGacha:
                        _gachaManager.DoGacha(GachaType.Gear, reward.amount);
                        break;
                    case RewardType.SkillGacha:
                        _gachaManager.DoGacha(GachaType.Skill, reward.amount);
                        break;
                    case RewardType.PartyGacha:
                        _gachaManager.DoGacha(GachaType.Party, reward.amount);
                        break;
                    case RewardType.SpeedBoost:
                        Managers.Time.SetDoubleSpeed(reward.amount);
                        break;
                    case RewardType.SystemUnlock:
                        _gameManager.UnlockSystem(reward.unlockType);
                        break;
                    case RewardType.Tutorial:
                        Managers.Tutorial.StartTutorial(reward.id);
                        break;
                }
            }
        }

        ChangeQuestStatus(questKey, QuestStatus.Rewarded);
        
        // 다음 퀘스트 실행
        if (data.nextKeys != null)
        {
            foreach (int key in data.nextKeys.Where(key => key > 0))
            {
                ChangeQuestStatus(key, QuestStatus.InProgress);
            }
        }
        // 메인 퀘스트인 경우
        if (_mainQuestDatabase.ContainsKey(questKey))
        {
            if (data.nextKeys != null && data.nextKeys[0] != -1)
            {
                int nextMainQuestKey = data.nextKeys[0];
                if (_mainQuestDatabase.TryGetValue(nextMainQuestKey, out QuestData value))
                {
                    CurrentMainQuest = value;
                }
            }
            else
            {
                CurrentMainQuest = null;
            }
            Managers.Analytics.SendQuestCompleteEvent(questKey, Managers.Time.TotalPlayTimeSec);
        }
        else if (_dailyQuestDatabase.ContainsKey(questKey)) // 일일 퀘스트
        {
            EventBus.Raise(new QuestClearedEvent(data));
            Managers.Analytics.SendQuestCompleteEvent(questKey, Managers.Time.CurrentPlayTimeSec);
        }
        
    }

    public RewardData GetQuestRewardData(int questId)
    {
        if (_rewardDatabase.ContainsKey(questId) && _rewardDatabase[questId].Count == 1)
        {
            return _rewardDatabase[questId].First();
        }

        return null;
    }

    // 접속 시간 확인
    // 퀘스트 창을 열 때 마다 호출하는 것이 좋을듯
    public void UpdatePlayTimeQuests()
    {
        List<QuestState> states  = _inProgressQuests[QuestType.PlayTime]; 
        for (int i = states.Count - 1; i >= 0; i--)
        {
            QuestState state = states[i];
            state.currentCount = (int)(Managers.Time.DailyPlayTimeSec / 60);
            CheckQuestCompletion(state); 
        }
    }

    private void Save(QuestState state)
    {
        _saveLoad.SaveData.questStates[state.questKey] = state;
    }

    #region Event Handlers
        
    // 몬스터 사망 이벤트가 발생했을 때 호출될 함수
    private void OnEnemyDied(EnemyDiedEvent e)
    {
        List<QuestState> states  = _inProgressQuests[QuestType.EnemyKill]; 
        // 진행중인 퀘스트 확인
        for (int i = states.Count - 1; i >= 0; i--)
        {
            QuestState state = states[i];
            state.currentCount++; // 진행도 증가
            CheckQuestCompletion(state); // 퀘스트 완료 여부 확인
        }
    }
    
    private void OnGacha(GachaEvent e)
    {
        List<QuestState> states;
        GachaType type;
        if (e.gachaType == GachaType.Gear) // 장비
        {
            states = _inProgressQuests[QuestType.GearGet];
            type = GachaType.Gear;
        }
        else if (e.gachaType  == GachaType.Skill) // 스킬
        {
            states = _inProgressQuests[QuestType.SkillGet];
            type = GachaType.Skill;
        }
        else if (e.gachaType == GachaType.Party) // 동료
        {
            states = _inProgressQuests[QuestType.PartyGet];
            type = GachaType.Party;
        }
        else return;
        
        for (int i = states.Count - 1; i >= 0; i--)
        {
            QuestState state = states[i];
            QuestData data = _questDatabase[state.questKey];
            
            // 일일 퀘스트인 경우, 누적 카운트가 아닌 개별 카운트로 적용
            if (_dailyQuestDatabase.ContainsKey(state.questKey))
            {
                state.currentCount += e.gachaCount;
            }
            else
            {
                state.currentCount = Managers.Gacha.GetGachaCount(type);
            }
            
            CheckQuestCompletion(state);
        }
    }

    private void OnStatUpgrade(StatUpgradeEvent e)
    {
        var pair = _statQuestMap.FirstOrDefault(x => x.Value == e.statType);
        List<QuestState> states = _inProgressQuests[pair.Key];
        for (int i = states.Count - 1; i >= 0; i--)
        {
            QuestState state = states[i];
            QuestData data = _questDatabase[state.questKey];
            state.currentCount = e.currentLevel;
            CheckQuestCompletion(state);
        }
    }
    
    private void OnStageCleared(StageClearedEvent e) 
    {
        List<QuestState> states = _inProgressQuests[QuestType.StageClear];
        
        for (int i = states.Count - 1; i >= 0; i--)
        {
            QuestState state = states[i];
            QuestData data = _questDatabase[state.questKey];
            int stageKey = e.stageKey + Managers.Level.GetCurrentProgress().loop * 1000;
            // 현재 클리어한 스테이지가 목표 스테이지보다 높거나 같으면 달성
            if (stageKey >= data.conditionCount)
            {
                state.currentCount = data.conditionCount; // 목표 달성으로 간주
                CheckQuestCompletion(state);
            }
        }
        
        states = _inProgressQuests[QuestType.StageClearCount];
        for (int i = states.Count - 1; i >= 0; i--)
        {
            QuestState state = states[i];
            state.currentCount++;
            CheckQuestCompletion(state);
        }
    }

    public void OnQuestCleared(QuestClearedEvent e)
    {
        List<QuestState> states = _inProgressQuests[QuestType.QuestComplete];
        for (int i = states.Count - 1; i >= 0; i--)
        {
            QuestState state = states[i];
            state.currentCount++;
            CheckQuestCompletion(state);
        }
    }

    private void OnAdWatched(AdWatchedEvent e)
    {
        List<QuestState> states = _inProgressQuests[QuestType.WatchAD];
        for (int i = states.Count - 1; i >= 0; i--)
        {
            QuestState state = states[i];
            state.currentCount++;
            CheckQuestCompletion(state);
        }
    }

    private void OnDungeonCleared(DungeonClearedEvent e)
    {
        var pair = _dungeonQuestMap.FirstOrDefault(x => x.Value == e.type);
        List<QuestState> states = _inProgressQuests[pair.Key];
        
        for (int i = states.Count - 1; i >= 0; i--)
        {
            QuestState state = states[i];
            state.currentCount = e.level;
            CheckQuestCompletion(state);
        }
    }

    private void OnBossFail(BossFailEvent e)
    {
        List<QuestState> states = _inProgressQuests[QuestType.PlayerDie];
        for (int i = states.Count - 1; i >= 0; i--)
        {
            QuestState state = states[i];
            state.currentCount++;
            CheckQuestCompletion(state);
        }
    }

    #endregion

}