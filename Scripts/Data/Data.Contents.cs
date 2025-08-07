using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using static Define;

//데이터 직렬화와 로딩의 분리
//CreatureData는 순수 데이터 클래스
//CreatureDataLoader는 데이터를 리스트로 받고 이를 Dictionary로 변환하는 로직을 담당
//직렬화(데이터 형태 유지)와 게임 내 활용 구조(Dict 변환)를 명확하게 분리

//장점
//1. 확장성: 새로운 데이터 타입이 추가될 때 DataLoader만 새로 만들고 ILoader 인터페이스만 구현하면 됨
//2. 유지보수 용이성: 데이터 파싱, 딕셔너리 변환, 접근 방식이 일관되어 있어 버그 추적이 쉽고 테스트도 단순화
namespace Data
{
    #region CurrencyData
    
    [Serializable]
    public class CurrencyData
    {
        [JsonIgnore] public BigInteger Gold { get; private set;}
    
        // 안전하게 저장하기 위해 문자열로 변환해서 저장
        public string GoldStr
        {
            get => Gold.ToString();
            set => Gold = !string.IsNullOrEmpty(value) ? BigInteger.Parse(value) : BigInteger.Zero;
        }
        public int gem;
        public int goldDungeonTicket;
        public int bossDungeonTicket;

        public CurrencyData()
        {
            GoldStr = "0";
            gem = 0;
            goldDungeonTicket = 3;
            bossDungeonTicket = 3;
        }
    }

    #endregion
    
    #region CreatureData

    //몬스터 개체의 스탯 정보를 담는 직렬화 가능한 클래스
    [Serializable]
    public class CreatureData
    {
        public int dataId;
        public string name;
        public float moveSpeed;
        public float attackSpeed;
        public float attackRange;
    }

    //JSON으로부터 로드한 리스트 데이터를 Dictionary 형태로 변환하는 클래스
    [Serializable]
    public class CreatureDataLoader : ILoader<int, CreatureData>
    {
        public List<CreatureData> Items = new List<CreatureData>();

        //dataId를 키로 한 Dictionary 생성
        public Dictionary<int, CreatureData> MakeDict()
        {
            Dictionary<int, CreatureData> dict = new Dictionary<int, CreatureData>();
            foreach (CreatureData creatureData in Items)
                dict.Add(creatureData.dataId, creatureData);
            return dict;
        }
    }

    #endregion

    #region BossData

    [Serializable]
    public class BossHpData
    {
        public int key;
        public double normal;
        public string normalFormatted;
        public double hard;
        public string hardFormatted;
        public double veryHard;
        public string veryHardFormatted;
    }

    [Serializable]
    public class BossHpDataLoader : ILoader<int, BossHpData>
    {
        public List<BossHpData> Items = new List<BossHpData>();

        public Dictionary<int, BossHpData> MakeDict()
        {
            Dictionary<int, BossHpData> dict = new Dictionary<int, BossHpData>();
            foreach (BossHpData bossHpData in Items)
                dict.Add(bossHpData.key, bossHpData);
            return dict;
        }
    }
    
    [Serializable]
    public class BossAtkData
    {
        public int key;
        public double normal;
        public string normalFormatted;
        public double hard;
        public string hardFormatted;
        public double veryHard;
        public string veryHardFormatted;
    }

    [Serializable]
    public class BossAtkDataLoader : ILoader<int, BossAtkData>
    {
        public List<BossAtkData> Items = new List<BossAtkData>();

        public Dictionary<int, BossAtkData> MakeDict()
        {
            Dictionary<int, BossAtkData> dict = new Dictionary<int, BossAtkData>();
            foreach (BossAtkData bossAtkData in Items)
                dict.Add(bossAtkData.key, bossAtkData);
            return dict;
        }
    }

    #endregion

    #region StatUpgradeData

    [Serializable]
    public class StatUpgradeData
    {
        public int dataId;
        public StatType type;
        public string name;
        public int maxLevel;
        public float baseValue;
        public float increasePerLevel;
        public StatType unlockStatType;
        public int unlockValue;
    }

    [Serializable]
    public class StatUpgradeDataLoader : ILoader<StatType, StatUpgradeData>
    {
        public List<StatUpgradeData> Items = new List<StatUpgradeData>();

        public Dictionary<StatType, StatUpgradeData> MakeDict()
        {
            Dictionary<StatType, StatUpgradeData> dict = new Dictionary<StatType, StatUpgradeData>();
            foreach (StatUpgradeData statUpgradeData in Items)
                dict.Add(statUpgradeData.type, statUpgradeData);
            return dict;
        }
    }

    #endregion

    #region StageData

    [Serializable]
    public class StageData
    {
        public int key;
        public string mapName;
        public int mapIdx;
        public int stageIdx;
        public int waveIdx;
        public bool isBossWave;
        public List<int> creatureID;
        public int monsterCount;
    }

    [Serializable]
    public class StageDataLoader : ILoader<int, StageData>
    {
        public List<StageData> Items = new List<StageData>();

        public Dictionary<int, StageData> MakeDict()
        {
            Dictionary<int, StageData> dict = new Dictionary<int, StageData>();
            foreach (StageData stageData in Items)
                dict.Add(stageData.key, stageData);
            return dict;
        }
    }

    #endregion

    #region MergeCostData

    [Serializable]
    public class MergeCostData
    {
        public int level;
        public int mergeCost;
    }

    [Serializable]
    public class MergeCostDataLoader : ILoader<int, MergeCostData>
    {
        public List<MergeCostData> Items = new List<MergeCostData>();

        public Dictionary<int, MergeCostData> MakeDict()
        {
            Dictionary<int, MergeCostData> dict = new Dictionary<int, MergeCostData>();
            foreach (MergeCostData mergeCostData in Items)
                dict.Add(mergeCostData.level, mergeCostData);
            return dict;
        }
    }

    #endregion

    #region OwnedEffectLevelData

    [Serializable]
    public class OwnedEffectLevelData
    {
        public int level;
        public float Normal;
        public float Rare;
        public float Epic;
        public float Unique;
        public float Legendary;

        [NonSerialized] public Dictionary<RarityType, float> Effects;

        public void Init()
        {
            Effects = new Dictionary<RarityType, float>
            {
                { RarityType.Normal, Normal },
                { RarityType.Rare, Rare },
                { RarityType.Epic, Epic },
                { RarityType.Unique, Unique },
                { RarityType.Legendary, Legendary }
            };
        }
    }

    [Serializable]
    public class OwnedEffectLevelDataLoader : ILoader<int, OwnedEffectLevelData>
    {
        public List<OwnedEffectLevelData> Items = new List<OwnedEffectLevelData>();

        public Dictionary<int, OwnedEffectLevelData> MakeDict()
        {
            Dictionary<int, OwnedEffectLevelData> dict = new Dictionary<int, OwnedEffectLevelData>();
            foreach (OwnedEffectLevelData ownedEffectLevelData in Items)
                dict.Add(ownedEffectLevelData.level, ownedEffectLevelData);
            return dict;
        }
    }

    #endregion

    #region QuestData

    [Serializable]
    public class QuestData
    {
        public int key;
        public string name;
        public QuestType type;
        public int conditionCount;
        public List<int> nextKeys;
    }

    [Serializable]
    public class QuestState
    {
        public int questKey;
        public QuestStatus status = QuestStatus.Inactive;
        public int currentCount = 0;
    }

    [Serializable]
    public class QuestDataLoader : ILoader<int, QuestData>
    {
        public List<QuestData> Items = new List<QuestData>();

        public Dictionary<int, QuestData> MakeDict()
        {
            Dictionary<int, QuestData> dict = new Dictionary<int, QuestData>();
            foreach (QuestData questData in Items)
                dict.Add(questData.key, questData);
            return dict;
        }
    }

    #endregion

    #region SkillData

    [Serializable]
    public class SkillData : ItemData
    {
        public SkillType type;
        public SkillExecuteType executeType;
        public float cooldown;
        public float range;
        public int targetCount;
        public float damageMultiplier;
        public int attackCount;
        public float attackInterval;
        public float recognitionRange;
        public float projSpeed;
        public float projRange;
        public float duration;
        public StatType stat;
        public float buffValue;
        public string effectName;
        public float effectDuration;
        public string animTrigger;
        public string clipName;
    }

    [Serializable]
    public class SkillDataLoader : ILoader<int, SkillData>
    {
        public List<SkillData> Items = new List<SkillData>();

        public Dictionary<int, SkillData> MakeDict()
        {
            Dictionary<int, SkillData> dict = new Dictionary<int, SkillData>();
            foreach (SkillData skillData in Items)
                dict.Add(skillData.dataId, skillData);
            
            // foreach (SkillData skillData in Items)
            // {
            //     Debug.Log($"[Check RAW] skillData: {JsonUtility.ToJson(skillData)}");
            //     if (string.IsNullOrEmpty(skillData.clipName))
            //         Debug.LogWarning($"[Check] Skill {skillData.dataId} ({skillData.name}) has NO clipName");
            //     else
            //         Debug.Log($"[Check] Skill {skillData.dataId} clipName = {skillData.clipName}");
            // }
            
            return dict;
        }
        
        
    }

    #endregion

    #region SkillLevelData

    [Serializable]
    public class SkillLevelData
    {
        public int dataId;
        public float baseValue;
        public string formula;
    }

    [Serializable]
    public class SkillLevelDataLoader : ILoader<int, SkillLevelData>
    {
        public List<SkillLevelData> Items = new List<SkillLevelData>();

        public Dictionary<int, SkillLevelData> MakeDict()
        {
            Dictionary<int, SkillLevelData> dict = new Dictionary<int, SkillLevelData>();
            foreach (SkillLevelData skillLevelData in Items)
                dict.Add(skillLevelData.dataId, skillLevelData);
            return dict;
        }
    }

    #endregion

    #region GearData

    [Serializable]
    public class GearData : ItemData
    {
        public GearType type;
    }

    [Serializable]
    public class GearState : ItemState<GearData>
    {
        public Sprite visualSprite { get; private set; }
        
        // Resources 폴더에서 장비 외형 스프라이트를 로드
        public void LoadVisual()
        {
            string path = $"Sprites/Gears/{data.dataId}";
            visualSprite = Resources.Load<Sprite>(path);

            if (visualSprite == null)
            {
                Debug.LogWarning($"GearState 찾을 수 없습니다");
            }
        }
    }

    [Serializable]
    public class GearDataLoader : ILoader<int, GearData>
    {
        public List<GearData> Items = new List<GearData>();

        public Dictionary<int, GearData> MakeDict()
        {
            Dictionary<int, GearData> dict = new Dictionary<int, GearData>();
            foreach (GearData gearData in Items)
                dict.Add(gearData.dataId, gearData);
            return dict;
        }
    }

    #endregion

    #region GearLevelData

    [Serializable]
    public class GearLevelData
    {
        public int dataId;
        public StatType statType;
        public float baseValue;
        public string formula;
    }

    [Serializable]
    public class GearLevelDataLoader : ILoader<int, GearLevelData>
    {
        public List<GearLevelData> Items = new List<GearLevelData>();

        public Dictionary<int, GearLevelData> MakeDict()
        {
            Dictionary<int, GearLevelData> dict = new Dictionary<int, GearLevelData>();
            foreach (GearLevelData gearLevelData in Items)
                dict.Add(gearLevelData.dataId, gearLevelData);
            return dict;
        }
    }

    #endregion

    #region PartyData

    [Serializable]
    public class PartyData : ItemData
    {
        public float attackSpeed;
        public PartyType type;
    }

    [Serializable]
    public class PartyState : ItemState<PartyData>
    {
        
    }


    [Serializable]
    public class PartyDataLoader : ILoader<int, PartyData>
    {
        public List<PartyData> Items = new List<PartyData>();

        public Dictionary<int, PartyData> MakeDict()
        {
            Dictionary<int, PartyData> dict = new Dictionary<int, PartyData>();
            foreach (PartyData partyData in Items)
                dict.Add(partyData.dataId, partyData);
            return dict;
        }
    }

    #endregion

    #region PartyLevelData

    [Serializable]
    public class PartyLevelData
    {
        public int dataId;
        public float baseValue;
        public string formula;
    }

    [Serializable]
    public class PartyLevelDataLoader : ILoader<int, PartyLevelData>
    {
        public List<PartyLevelData> Items = new List<PartyLevelData>();

        public Dictionary<int, PartyLevelData> MakeDict()
        {
            Dictionary<int, PartyLevelData> dict = new Dictionary<int, PartyLevelData>();
            foreach (PartyLevelData partyLevelData in Items)
                dict.Add(partyLevelData.dataId, partyLevelData);
            return dict;
        }
    }

    #endregion

    #region GachaTableData

    [Serializable]
    public class GachaTableData
    {
        public int level;
        public float Normal;
        public float Rare;
        public float Epic;
        public float Unique;
        public float Legendary;
    }

    [Serializable]
    public class GachaTableDataLoader : ILoader<int, GachaTableData>
    {
        public List<GachaTableData> Items = new List<GachaTableData>();

        public Dictionary<int, GachaTableData> MakeDict()
        {
            Dictionary<int, GachaTableData> dict = new Dictionary<int, GachaTableData>();
            foreach (GachaTableData gachaTableData in Items)
                dict.Add(gachaTableData.level, gachaTableData);
            return dict;
        }
    }

    #endregion

    #region GachaLevelTableData

    [Serializable]
    public class GachaLevelTableData
    {
        public int level;
        public int experience;
    }

    [Serializable]
    public class GachaLevelTableDataLoader : ILoader<int, GachaLevelTableData>
    {
        public List<GachaLevelTableData> Items = new List<GachaLevelTableData>();

        public Dictionary<int, GachaLevelTableData> MakeDict()
        {
            Dictionary<int, GachaLevelTableData> dict = new Dictionary<int, GachaLevelTableData>();
            foreach (GachaLevelTableData gachaLevelTableData in Items)
                dict.Add(gachaLevelTableData.level, gachaLevelTableData);
            return dict;
        }
    }

    #endregion
    
    # region

    [Serializable]
    public class RewardData
    {
        public int key;             // 더미 키 값
        public int questId;             // 퀘스트 ID
        public RewardType rewardType;     // 보상 종류
        public int id;              // 아이템 ID, 튜토리얼 ID 등
        public int amount;          // 아이템 및 재화 수량
        public UnlockType unlockType;
    }

    [Serializable]
    public class RewardDataLoader : ILoader<int, RewardData>
    {
        public List<RewardData> Items = new List<RewardData>();

        public Dictionary<int, RewardData> MakeDict()
        {
            Dictionary<int, RewardData> dict = new Dictionary<int, RewardData>();
            foreach (RewardData rewardData in Items)
                dict.Add(rewardData.key, rewardData);
            return dict;
        }
    }
    
    # endregion

    #region OfflineRewardData

    [Serializable]
    public class OfflineRewardData
    {
        public int key;
        public string rewardGold;
    }

    [Serializable]
    public class OfflineRewardDataLoader : ILoader<int, OfflineRewardData>
    {
        public List<OfflineRewardData> Items = new List<OfflineRewardData>();

        public Dictionary<int, OfflineRewardData> MakeDict()
        {
            Dictionary<int, OfflineRewardData> dict = new Dictionary<int, OfflineRewardData>();
            foreach (OfflineRewardData offlineRewardData in Items)
                dict.Add(offlineRewardData.key, offlineRewardData);
            return dict;
        }
    }

    #endregion

    #region DungeonData
    [Serializable]
    public class BossDungeonData
    {
        public int key;
        public string map;
        public int reward;
        public List<int> bossIds;
        public List<string> bossHp;
        public List<string> bossAtk;
    }

    [System.Serializable]
    public class BossDungeonDataLoader : ILoader<int, BossDungeonData>
    {
        public List<BossDungeonData> Items = new();

        public Dictionary<int, BossDungeonData> MakeDict()
        {
            Dictionary<int, BossDungeonData> dict = new();
            foreach (var data in Items)
            {
                dict[data.key] = data; // level을 key로 사용
            }

            return dict;
        }
    }

    [Serializable]
    public class GoldDungeonData
    {
        public int key;
        public string map;
        public string reward;
        public int dummyId;
        public string hp;
    }

    [Serializable]
    public class GoldDungeonDataLoader : ILoader<int, GoldDungeonData>
    {
        public List<GoldDungeonData> Items = new();

        public Dictionary<int, GoldDungeonData> MakeDict()
        {
            Dictionary<int, GoldDungeonData> dict = new();
            foreach (var data in Items)
                dict[data.key] = data;
            return dict;
        }
    }
    #endregion

    #region ProfileData
    [Serializable]
    public class ProfileData
    {
        public int key;
        public ProfileType type;
        public string name;
        public string condition;
        public ProfileUnlockType conditionType;
        public int conditionCount;
        public int conditionId;
    }

    [Serializable]
    public class ProfileDataLoader : ILoader<int, ProfileData>
    {
        public List<ProfileData> Items = new();

        public Dictionary<int, ProfileData> MakeDict()
        {
            Dictionary<int, ProfileData> dict = new();
            foreach (var data in Items)
                dict[data.key] = data;
            return dict;
        }
    }
    #endregion
    
    #region TutorialData
    
    [Serializable]
    public class TutorialStepData
    {
        public int key;
        public TutorialStepType type;
        public float startDelay;
        
        // Dialogue 전용 데이터
        public string npcName;
        public string dialogueText;
        
        // 왼쪽, 혹은 오른쪽에 표시될 npc 스프라이트 ID
        // 둘다 비워둘 시 표시 X, 둘다 할당할 시 왼쪽만 표시됨
        public string leftSpriteName;
        public string rightSpriteName;
        
        // Action 전용 데이터
        // 클릭을 유도할 버튼의 고유 이름
        public UIButtonType targetButtonType;

        public string actionLog;
        // 강조 화살표 위치 (0 : 아래, 1 : 위)
        public int indicatorPosition;
    }

    [Serializable]
    public class TutorialData
    {
        public int key;
        public List<int> steps;
    }

    [Serializable]
    public class TutorialStepDataLoader : ILoader<int, TutorialStepData>
    {
        public List<TutorialStepData> Items = new();
        public Dictionary<int, TutorialStepData> MakeDict()
        {
            Dictionary<int, TutorialStepData> dict = new();
            foreach (TutorialStepData data in Items)
                dict.Add(data.key, data);
            return dict;
        }
    }

    [Serializable]
    public class TutorialDataLoader : ILoader<int, TutorialData>
    {
        public List<TutorialData> Items = new();

        public Dictionary<int, TutorialData> MakeDict()
        {
            Dictionary<int, TutorialData> dict = new();
            foreach (var data in Items)
                dict.Add(data.key, data);
            return dict;
        }
    }
    #endregion

    #region AttendanceData
    public class AttendanceData
    {
        public int key;
        public RewardType rewardType;
        public int rewardCount;
    }

    [Serializable]
    public class AttendanceDataLoader : ILoader<int, AttendanceData>
    {
        public List<AttendanceData> Items = new();
        public Dictionary<int, AttendanceData> MakeDict()
        {
            Dictionary<int, AttendanceData> dict = new();
            foreach (var data in Items)
                dict[data.key] = data;
            return dict;
        }
    }
    #endregion

    #region  TimeData

    [Serializable]
    public class TimeData
    {
        public int dailyPlayTimeSec;
        public int totalPlayTimeSec;
        public string lastLoginTimeStr;
        public string lastRewardTimeStr;
        public string lastBonusRewardTimeStr;
        public int attendanceDay;
        public bool isAttendanceRewardClaimed;
        public bool isBossDungeonAdClaimedToday;
        public bool isGoldDungeonAdClaimedToday;
        public string doubleSpeedEndTimeStr;

        // 광고 가챠별 마지막 사용 시간
        public Dictionary<Define.GachaType, string> lastAdGachaTimeStr;
    
        // 광고 가챠별 일일 사용 횟수
        public Dictionary<Define.GachaType, int> adGachaDailyCount;

        // SaveData가 처음 생성될 때 기본값을 설정하기 위한 생성자
        public TimeData()
        {
            dailyPlayTimeSec = 0;
            totalPlayTimeSec = 0;
            lastLoginTimeStr = string.Empty;
            lastRewardTimeStr = string.Empty;
            attendanceDay = 1;
            isAttendanceRewardClaimed = false;
            isBossDungeonAdClaimedToday = false;
            isGoldDungeonAdClaimedToday = false;

            lastAdGachaTimeStr = new Dictionary<Define.GachaType, string>();
            adGachaDailyCount = new Dictionary<Define.GachaType, int>();
        }
    }

    #endregion

    #region SoundOptionData

    [Serializable]
    public class SoundOptionData
    {
        public bool bgmOn = true;
        public bool sfxOn = true;
        public float bgmVolume = 1.0f;
        public float sfxVolume = 1.0f;
    }

    #endregion

    #region  SaveData

    [Serializable]
    public class SaveData
    {
        // 계정 정보
        public string userId;
        public string userName;
        public bool hasUserConsented;   // 개인 정보 수집 동의 여부
        
        //재화 및 성장
        public CurrencyData currencyData;
        public Dictionary<StatType, int> statLevel;
        
        // 아이템
        public List<int> equippedGears;
        public List<int> equippedSkills;
        public List<int> equippedParty;
        public Dictionary<int, GearState> gearStates;
        public Dictionary<int, SkillState> skillStates;
        public Dictionary<int, PartyState> partyStates;
        
        //가챠
        public Dictionary<GachaType, int> gachaCount;
        public Dictionary<GachaType, int> gachaExp;
        public Dictionary<GachaType, int> gachaLevel;
        
        // 진행도
        public int stageLevel;
        public int currentLoop;
        public bool isWaitingForBossChallenge;
        public int bossDungeonLevel;
        public int goldDungeonLevel;
        public List<int> clearedTutorials;
        public Dictionary<int, QuestState> questStates;
        public bool isSkillUnlocked;
        public bool isPartyUnlocked;
        public bool isDungeonUnlocked;
        public int skillSlots;
        public int partySlots;
        
        //설정
        public Dictionary<Sound, bool> soundToggle;
        public Dictionary<Sound, float> soundVolume;
        
        //설정-자동 스킬
        public bool isAutoSkillOn;
        
        // 시간 관련 데이터
        public TimeData timeData;
        public int offlineRewardGachaLevel;
        public Dictionary<int, int> offLineRewardItemIds;
        
        //프로필
        public List<int> unlockedProfiles;
        public List<int> newlyUnlockedProfiles;
        public int equippedProfileKey;
        public int equippedFrameKey;
        
        // 광고 제거 여부
        public bool isAdRemoved;
        
        // 생성자 : 기본값 설정
        public SaveData()
        {
            userId = "Guest";
            userName = "NoobHero";
            hasUserConsented = false;
            statLevel = new Dictionary<StatType, int>();
            equippedGears = new List<int>();
            equippedSkills = new List<int>();
            equippedParty = new List<int>();
            currencyData = new CurrencyData();
            gearStates = new Dictionary<int, GearState>();
            skillStates = new Dictionary<int, SkillState>();
            partyStates = new Dictionary<int, PartyState>();
            gachaCount = new Dictionary<GachaType, int>();
            gachaExp = new Dictionary<GachaType, int>();
            gachaLevel = new Dictionary<GachaType, int>();
            stageLevel = 1000;
            currentLoop = 0;
            isWaitingForBossChallenge = false;
            bossDungeonLevel = 1;
            goldDungeonLevel = 1;
            clearedTutorials = new List<int>();
            questStates = new Dictionary<int, QuestState>();
            isSkillUnlocked = false;
            isPartyUnlocked = false;
            isDungeonUnlocked = false;
            skillSlots = 0;
            partySlots = 0;
            soundToggle = new Dictionary<Sound, bool>()
            {
                { Sound.Bgm , true},
                { Sound.Sfx , true},
                { Sound.Max , true}
            };
            soundVolume = new Dictionary<Sound, float>()
            {
                { Sound.Bgm, 1.0f },
                { Sound.Sfx, 1.0f }, 
                { Sound.Max, 1.0f }
            };
            timeData = new TimeData();
            offLineRewardItemIds = new Dictionary<int, int>();
            unlockedProfiles = new List<int>();
            equippedProfileKey = 0;
            equippedFrameKey = 0;
            
            isAdRemoved = false;
        }
    }

    #endregion
    
    #region  SoundData
    [Serializable]
    public class SkillSoundData
    {
        public int skillId;
        public string clipName;
    }
    
    public class SkillSoundDataLoader : ILoader<int, SkillSoundData>
    {
        public List<SkillSoundData> dataList;

        public Dictionary<int, SkillSoundData> MakeDict()
        {
            Dictionary<int, SkillSoundData> dict = new();
            foreach (SkillSoundData data in dataList)
                dict[data.skillId] = data;
            return dict;
        }
    }
    
    #endregion
    
    
    // 모든 아이템의 기본이 될 추상 클래스
    // 아이템의 고정된 정보만 담음
    [Serializable]
    public abstract class ItemData
    {
        public int dataId;           // 아이디
        public string name;         // 이름
        public string description;      // 설명
        public RarityType rarity;    //등급
    }

    // 아이템의 가변성 데이터만 담음
    [Serializable]
    public class ItemState
    {
        public int dataId;           // 아이디
        public int level = 1;               // 레벨
        public int experience = 0;          // 경험치
        public bool isUnlocked = false;         // 소유 여부
        public bool canUpgrade = false;         // 업그레이드 가능 여부
    }

    // ItemState 가 어떤 종류의 ItemData 를 참고하는지 알 수 있도록 제너릭으로 생성
    [System.Serializable]
    public class ItemState<TData> : ItemState where TData : ItemData
    {
        public TData data;
    }
}

   
