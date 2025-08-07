using DG.Tweening.Plugins.Core.PathCore;
using UnityEngine;

//게임의 전체 매니저 시스템을 중앙 집중화
//각종 게임 시스템 (UI, 자원 로딩, 오브젝트 풀, 씬 전환 등)을 정적인 방식으로 접근할 수 있게 함
//Managers.UI, Managers.Resource 등으로 모든 서브 시스템에 편리하게 접근할 수 있음

//장점
//1. 전역 접근 편의성: 각 매니저에 정적으로 접근할 수 있어 코드가 간결해지고 호출이 쉬움
//2. 초기화 일괄 관리: Init() 하나로 모든 매니저를 초기화하고 컴포넌트 기반 매니저는 AddComponent로 명확하게 구분

//주의해야 할 점
//1. 전역 상태 남용 금지: LevelManager에서 TImeManager를 직접 접근해서 수정하는 건 하지 않기
//2. 초기화 순서 의존성 문제: 다른 스크립트에서 해당 매니저를 먼저 접근하려고 하면 초기화되지 않은 상태일 수 있음
public class Managers : Singleton<Managers>
{
    #region Contents
    private DungeonManager _dungeon;
    private GachaManager _gacha = new GachaManager();
    private GameManager _game = new GameManager();
    private PlayerManager _player = new PlayerManager();
    private ProfileManager _profile = new ProfileManager();
    private QuestManager _quest = new QuestManager();
    private RewardedAdManager _ad;
    private StatUpgradeManager _statUpgrade = new StatUpgradeManager();
    private TimeManager _time;
    private TutorialManager _tutorial = new TutorialManager();
    //private GoogleLoginManager _googleLogin = new GoogleLoginManager();
    private UserDatabaseManager _userDatabase;
    private GuestManager _guest;
    private DataApiManager _dataApi = new DataApiManager();
    //private LocalPushManager _push;
    private PurchaseManager _purchase;

    
    public static DungeonManager Dungeon { get { return Instance?._dungeon; } }
    public static GachaManager Gacha { get { return Instance?._gacha; } }
    public static GameManager Game { get { return Instance?._game; } }
    public static PlayerManager Player { get { return Instance?._player; } }
    public static ProfileManager Profile { get { return Instance?._profile; } }
    public static QuestManager Quest { get { return Instance?._quest; } }
    public static RewardedAdManager Ad { get { return Instance?._ad; } }
    public static StatUpgradeManager StatUpgrade { get { return Instance?._statUpgrade; } }
    public static TimeManager Time { get { return Instance?._time; } }
    public static TutorialManager Tutorial { get { return Instance?._tutorial; } }
    //public static GoogleLoginManager GoogleLogin { get { return Instance?._googleLogin; } }
    public static UserDatabaseManager UserDatabase { get { return Instance?._userDatabase; } }
    public static GuestManager Guest { get { return Instance?._guest; } }
    public static DataApiManager DataApi { get { return Instance?._dataApi; } }

    //public static LocalPushManager Push => Instance?._push;
    public static PurchaseManager Purchase => Instance?._purchase;

    #endregion

    #region Core
    private DataManager _data = new DataManager();
    private PlayerEquipment _equipment = new PlayerEquipment();
    private InventoryManager _inventory = new InventoryManager();
    private ItemDatabase _itemDatabase = new ItemDatabase();
    private LevelManager _level;
    private PoolManager _pool = new PoolManager();
    private ResourceManager _resource = new ResourceManager();
    private SaveLoadManager _saveLoad;
    private SceneManagerEx _scene = new SceneManagerEx();
    private SoundManager _sound = new SoundManager();
    private SpawnManager _spawn = new SpawnManager();
    private UIManager _ui = new UIManager();
    //private SleepModeManager _sleepMode;
    private AnalyticsManager _analytics;
    
    public static DataManager  Data { get { return Instance?._data; } }
    public static PlayerEquipment Equipment { get { return Instance?._equipment; } }
    public static InventoryManager Inventory { get { return Instance?._inventory; } }
    public static ItemDatabase ItemDatabase { get { return Instance?._itemDatabase; } }
    public static LevelManager Level { get { return Instance?._level; } }
    public static PoolManager Pool { get { return Instance?._pool; } }
    public static ResourceManager Resource { get { return Instance?._resource; } }
    public static SaveLoadManager SaveLoad { get { return Instance?._saveLoad; } }
    public static SceneManagerEx Scene { get { return Instance?._scene; } }
    public static SoundManager Sound { get { return Instance?._sound; } }
    public static SpawnManager Spawn { get { return Instance?._spawn; } }
    public static UIManager UI { get { return Instance?._ui; } }
    //public static SleepModeManager SleepMode { get { return Instance?._sleepMode; } }
    public static AnalyticsManager Analytics {get {return Instance?._analytics;}}
    #endregion
    
    public static void Init()
    {
        GameObject go = GameObject.Find("(singleton) Managers");
        if (go == null)
        {
            go = new GameObject("(singleton) Managers");
            go.AddComponent<Managers>();
        }
        
        //MonoBehaviour 기반 매니저는 GameObject에 직접 컴포넌트로 붙임
        Instance._level = go.AddComponent<LevelManager>();
        Instance._time = go.AddComponent<TimeManager>();
        Instance._userDatabase = go.AddComponent<UserDatabaseManager>();
        Instance._saveLoad = go.AddComponent<SaveLoadManager>();
        Instance._guest = go.AddComponent<GuestManager>();
        Instance._dungeon = go.AddComponent<DungeonManager>();
        Instance._ad = go.AddComponent<RewardedAdManager>();
        //Instance._sleepMode = go.AddComponent<SleepModeManager>();
        Instance._purchase = go.AddComponent<PurchaseManager>();
        Instance._sound.Init();
        //Instance._push = go.AddComponent<LocalPushManager>();
        Instance._analytics = go.AddComponent<AnalyticsManager>();
        //Instance._push.Init();
        
        // 게임 테스트용 스크립트
#if UNITY_EDITOR
        go.AddComponent<GameTest>();
#endif
    }

    public static void Clear()
    {
        UI.Clear();
        Resource.Clear();
        Pool.Clear();
    }
}
