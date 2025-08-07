using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using static Util;


//모든 enum, 상수들 여기에 작성
public static class Define
{
    public static string GOLD_SPRITE_NAME = "GoldIcon.sprite";
    public static string GEM_SPRITE_NAME = "GemIcon.sprite";
    public static string BOSS_DUNGEON_KEY_SPRITE_NAME = "BossDungeonKey.sprite";
    public static string GOLD_DUNGEON_KEY_SPRITE_NAME = "GoldDungeonKey.sprite";

    public const float DOUBLE_SPEED_TIME = 1200f;

    public const int MAX_GEAR_SLOTS = 5;
    public const int MAX_SKILL_SLOTS = 6;
    public const int MAX_PARTY_SLOTS = 4;
    
    public static int DUNGEON_TICKET_MAX_VALUE = 3;
    public static int OFFLINE_REWARD_MINIMUM_TIME = 30;

    public static float DUNGEON_DURATION = 30f;

    public const int ESSENTIAL_POPUP_COUNT = 6;

    public const float PLAYER_RECOVERY_INTERVAL = 1.0f;
    
    public const float SLEEP_MODE_DELAY = 120f; // 자동 절전모드 진입 시간
    
    // 맵의 한글 이름 -> 영어 키로 변환해주는 딕셔너리 
    public static readonly Dictionary<string, string> MapNameToAddressableKey = new Dictionary<string, string>
    {
        { "시작의 숲", "GreenForest" },
        { "황혼의 숲", "YellowForest" },
        { "메마른 사막", "Desert" },
        { "타락한 습지", "Swamp" },
        { "얼어붙은 숲", "SnowForest" },
        { "분노의 화산", "Volcano" }
    };


    public enum DifficultyType
    {
        [Description("보통")]
        Normal,
        [Description("어려움")]
        Hard,
        [Description("매우 어려움")]
        VeryHard,
    }

    public enum RewardType
    {
        None,
        Gold,
        Gem,
        BossDungeonTicket,
        GoldDungeonTicket, 
        EquipmentGacha, // 장비가챠
        SkillGacha, // 스킬가챠
        PartyGacha, // 파티가챠
        SpeedBoost,  // 배속
        SystemUnlock,  // 시스템 해금
        Tutorial,       // 튜토리얼 시작
        BonusOfflineReward,
    }

    // 게임 중 해금되는 시스템 종류
    public enum UnlockType
    {
        None,
        Skill,
        Party,
        Dungeon,
        SkillSlot,
        PartySlot,
    }
    public enum DungeonType
    {
        Boss, 
        Gold 
    }
    public enum UIEvent
    {
        Click,
        Pressed,
        PointerDown,
        PointerUp, 
        Drag,
        BeginDrag,
        EndDrag,
    }
    
    public enum Scene
    {
        Unknown,
        TitleScene,
        GameScene,
        DevGameScene,
    }

    public enum GearType
    {
        Weapon,
        Hat,
        Armor,
        Gloves,
        Shoes
    }
    public enum PartyType
    {
        Archer,
        Mage,
        Rogue,
        Hunter,
    }

    public enum RarityType
    {
        Normal,
        Rare,
        Epic,
        Unique,
        Legendary
    }

    public enum SkillType
    {
        ActiveSkill,
        BuffSkill,
    }
    
    public enum SkillExecuteType
    {
        None = -1,
        Buff = 0,
        SingleHit = 1,
        MultiHit = 2,
        DoubleSlash = 3,
        AreaRepeat = 4,
        Projectile = 5,
        Default = 6
    }

    public enum StatType
    {
        None,
        AttackPower,
        MaxHp,
        HpRecovery,
        AttackSpeed,
        CriticalChance,
        CriticalDamage,
        AttackRange,
        MoveSpeed,
    }
    public enum GachaType
    {
        Gear,
        Skill,
        Party,
    }

    public enum ItemType
    {
        Gear,
        Skill,
        Party,
    }

    public enum QuestType
    {
        QuestComplete,
        StageClear,
        StageClearCount,
        EnemyKill,
        Login,
        GearGet,
        SkillGet,
        PartyGet,
        UpMaxHp,
        UpHpGen,
        UpAttackPower,
        UpAttackSpeed,
        UpCritChance,
        UpCritDamage,
        BossDungeon,
        GoldDungeon,
        WatchAD,
        PlayTime,
        PlayerDie,
    }

    public enum QuestStatus
    {
        Inactive,
        InProgress,
        Completed,
        Rewarded
    }

    public enum TutorialStepType
    {
        None,
        Dialogue,     // 단순 대화
        Action,     // 특정 버튼 클릭 유도
    }
    
    public static class UIColors
    {
        //골드 이름 색상
        public static readonly Color GoldTextColor = HexToColor("FEDF00");
        //젬 이름 색상
        public static readonly Color GemTextColor = HexToColor("D596F0");
        
        //이름 색상
        public static readonly Color NormalNameColor = HexToColor("A2A2A2");
        public static readonly Color RareNameColor = HexToColor("57FF0B");
        public static readonly Color EpicNameColor = HexToColor("2471E0");
        public static readonly Color UniqueNameColor = HexToColor("9F37F2");
        public static readonly Color LegendaryNameColor = HexToColor("F67B09");

        //배경 색상
        public static readonly Color Normal = HexToColor("AC9B83");
        public static readonly Color Rare = HexToColor("73EC4E");
        public static readonly Color Epic = HexToColor("0F84FF");
        public static readonly Color Unique = HexToColor("B740EA");
        public static readonly Color Legendary = HexToColor("F19B02");
    }

    public enum Sound
    {
        Bgm,
        Sfx,
        Max, //사운드 최대치
    }

    public enum UIButtonType
    {
        None,
        Hero,
        Equipment,
        Skill,
        Party,
        Dungeon,
        Shop,
        UpgradeAttackPower,
        UpgradeMaxHp,
        GearItem,
        SkillItem,
        PartyItem,
        EquipGear,
        EquipSkill,
        EquipParty,
        BuyGear,
        BuySkill,
        BuyParty,
        GearUpgrade,
        SkillUpgrade,
        PartyUpgrade,
        MainQuest,
        BossDungeon,
        GoldDungeon,
        Exit,
        AutoSkill,
        BossEnter,
    }

    public enum ProfileType
    {
        Profile,
        Frame,
    }

    public enum ProfileUnlockType
    {
        None,
        BossDungeon,
        GoldDungeon,
        Party,
        Power,
    }

    public enum OptionCheckType
    {
        Logout,
        Delete,
        Exit,
    }

    public enum SlotType
    {
        None,
        Empty,
        Locked,
    }
}

public class UpgradeResult
{
    public int itemId;
    public Define.ItemType type;
    public int previousLevel;
    public int newLevel;

    public UpgradeResult(int itemId, Define.ItemType type, int previousLevel, int newLevel)
    {
        this.itemId = itemId;
        this.type = type;
        this.previousLevel = previousLevel;
        this.newLevel = newLevel;
    }
}
