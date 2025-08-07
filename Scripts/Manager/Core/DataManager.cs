using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

//Generic + Interface 기반의 로더 추상화
//ILoader<Key, Value>를 통해서 다양한 데이터 타입에 대해 공통된 로딩 방식을 사용 가능
//LoadJson<Loader, Key, Value>는 어떤 타입의 데이터든 읽고 파싱해서 MakeDict()만 호출하면 끝
//-> 코드 재사용성 극대화, 유지보수 용이성 향상

//데이터 로더 인터페이스: 다양한 데이터 타입에 대응 가능
public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

//게임 전역에서 참조할 수 있는 데이터 관리자
public class DataManager
{
    public Dictionary<int, CreatureData> CreatureDataDic { get; private set; } = new Dictionary<int, CreatureData>();
    public Dictionary<Define.StatType, StatUpgradeData> StatUpgradeDataDic { get; private set; } = new Dictionary<Define.StatType, StatUpgradeData>();
    public Dictionary<int, StageData> StageDataDic { get; private set; } = new Dictionary<int, Data.StageData>();
    public Dictionary<int, BossHpData> BossHpDataDic { get; private set; } = new Dictionary<int, BossHpData>();
    public Dictionary<int, BossAtkData> BossAtkDataDic { get; private set;} = new Dictionary<int, BossAtkData>();
    public Dictionary<int, MergeCostData> MergeCostDataDic { get; private set; } = new Dictionary<int, MergeCostData>();
    public Dictionary<int, QuestData> MainQuestDataDic { get; private set; } = new Dictionary<int, QuestData>();
    public Dictionary<int, QuestData> DailyQuestDataDic { get; private set; } = new Dictionary<int, QuestData>();
    public Dictionary<int, QuestData> TutorialQuestDataDic { get; private set; } = new Dictionary<int, QuestData>();
    public Dictionary<int, RewardData> QuestRewardDataDic { get; private set; } = new Dictionary<int, RewardData>();
    
    public Dictionary<int, OwnedEffectLevelData> OwnedEffectLevelDataDic { get; private set; } = new Dictionary<int, OwnedEffectLevelData>();
    public Dictionary<int, SkillData> SkillDataDic { get; private set; } = new Dictionary<int, Data.SkillData>();
    public Dictionary<int, SkillLevelData> SkillLevelDataDic { get; private set; } = new Dictionary<int, SkillLevelData>();
    public Dictionary<int, GearData> GearDataDic { get; private set; } = new Dictionary<int, Data.GearData>();
    public Dictionary<int, GearLevelData> GearLevelDataDic { get; private set; } = new Dictionary<int, GearLevelData>();
    public Dictionary<int, PartyData> PartyDataDic { get; private set; } = new Dictionary<int, Data.PartyData>();
    public Dictionary<int, PartyLevelData> PartyLevelDataDic { get; private set; } = new Dictionary<int, PartyLevelData>();
    public Dictionary<int, GachaTableData> GachaTableDataDic { get; private set; } = new Dictionary<int, GachaTableData>();
    public Dictionary<int, GachaLevelTableData> GachaLevelTableDataDic { get; private set; } = new Dictionary<int, GachaLevelTableData>();
    public Dictionary<int, OfflineRewardData> OfflineRewardDataDic { get; private set; } = new Dictionary<int, OfflineRewardData>();
    public Dictionary<int, BossDungeonData> BossDungeonDataDic { get; private set; } = new();
    public Dictionary<int, GoldDungeonData> GoldDungeonDataDic { get; private set; } = new();
    public Dictionary<int, TutorialStepData> TutorialStepDataDic { get; private set; } = new Dictionary<int, TutorialStepData>();
    public Dictionary<int, TutorialData> TutorialDataDic { get; private set; } = new Dictionary<int, TutorialData>();
    public Dictionary<int, AttendanceData> AttendanceDataDic { get; private set; } = new Dictionary<int, AttendanceData>();
    public Dictionary<int, ProfileData> ProfileDataDic { get; private set; } = new Dictionary<int, ProfileData>();
    public Dictionary<int, SkillSoundData> SkillSoundDataDic { get; private set; } = new();

    //JSON 로딩 후 Dictionary로 변환
    public void Init()
    {
        //CreatureDataDic = LoadJson<CreatureDataLoader, int, CreatureData>("CreatureData").MakeDict();
        //StatUpgradeDataDic = LoadJson<StatUpgradeDataLoader, Define.StatType, StatUpgradeData>("StatUpgradeData").MakeDict();
        //StageDataDic = LoadJson<StageDataLoader, int, StageData>("StageData").MakeDict();
        //BossHpDataDic = LoadJson<BossHpDataLoader, int, BossHpData>("BossHpData").MakeDict();
        //BossAtkDataDic = LoadJson<BossAtkDataLoader, int, BossAtkData>("BossAtkData").MakeDict();
        //MergeCostDataDic = LoadJson<MergeCostDataLoader, int, MergeCostData>("MergeCostData").MakeDict();
        //MainQuestDataDic = LoadJson<QuestDataLoader, int, QuestData>("MainQuestData").MakeDict();
        //DailyQuestDataDic = LoadJson<QuestDataLoader, int, QuestData>("DailyQuestData").MakeDict();
        //TutorialQuestDataDic = LoadJson<QuestDataLoader, int, QuestData>("TutorialQuestData").MakeDict();
        //QuestRewardDataDic = LoadJson<RewardDataLoader, int, RewardData>("QuestRewardData").MakeDict();
        //OwnedEffectLevelDataDic = LoadJson<OwnedEffectLevelDataLoader, int, OwnedEffectLevelData>("OwnedEffectLevelData").MakeDict();
        // foreach (var data in OwnedEffectLevelDataDic.Values)
        // {
        //     data.Init();
        // }
        //SkillDataDic = LoadJson<Data.SkillDataLoader, int, Data.SkillData>("SkillData").MakeDict();
        //SkillLevelDataDic = LoadJson<SkillLevelDataLoader, int, SkillLevelData>("SkillLevelData").MakeDict();
        //GearDataDic = LoadJson<Data.GearDataLoader, int, GearData>("GearData").MakeDict();
        //GearLevelDataDic = LoadJson<GearLevelDataLoader, int, GearLevelData>("GearLevelData").MakeDict();
        //PartyDataDic = LoadJson<Data.PartyDataLoader, int, PartyData>("PartyData").MakeDict();
        //PartyLevelDataDic = LoadJson<PartyLevelDataLoader, int, PartyLevelData>("PartyLevelData").MakeDict();
        //GachaTableDataDic = LoadJson<GachaTableDataLoader, int, GachaTableData>("GachaTableData").MakeDict();
        //GachaLevelTableDataDic = LoadJson<GachaLevelTableDataLoader, int, GachaLevelTableData>("GachaLevelTableData").MakeDict();
        //OfflineRewardDataDic = LoadJson<OfflineRewardDataLoader, int, OfflineRewardData>("OfflineRewardData").MakeDict();
        //BossDungeonDataDic = LoadJson<BossDungeonDataLoader, int, BossDungeonData>("BossDungeonData").MakeDict();
        //GoldDungeonDataDic = LoadJson<GoldDungeonDataLoader, int, GoldDungeonData>("GoldDungeonData").MakeDict();
        //TutorialStepDataDic = LoadJson<TutorialStepDataLoader, int, TutorialStepData>("TutorialStepData").MakeDict();
        //TutorialDataDic = LoadJson<TutorialDataLoader, int, TutorialData>("TutorialData").MakeDict();
        //AttendanceDataDic = LoadJson<AttendanceDataLoader, int, AttendanceData>("AttendanceData").MakeDict();
        //ProfileDataDic = LoadJson<ProfileDataLoader, int, ProfileData>("ProfileData").MakeDict();
        
    }

    public int GetServerDataCount()
    {
        return 26;
    }

    public IEnumerator InitFromServer(Action onEachLoad, Action onComplete)
    {
        int totalCount = GetServerDataCount();
        int currentCount = 0;

        List<IEnumerator> coroutines = new();
        
        void StartWrap<TLoader, TKey, TValue>(string path, Action<Dictionary<TKey, TValue>> assign)
            where TLoader : ILoader<TKey, TValue>, new()
        {
            coroutines.Add(WrapLoadParallel<TLoader, TKey, TValue>(path, dict =>
            {
                assign(dict);
                currentCount++;
                onEachLoad?.Invoke();
            }));
        }
        
        StartWrap<AttendanceDataLoader, int, AttendanceData>("attendance/list", dict => AttendanceDataDic = dict);
        StartWrap<BossAtkDataLoader, int, BossAtkData>("boss-atk/list", dict => BossAtkDataDic = dict);
        StartWrap<BossHpDataLoader, int, BossHpData>("boss-hp/list", dict => BossHpDataDic = dict);
        StartWrap<CreatureDataLoader, int, CreatureData>("creatureData/list", dict => CreatureDataDic = dict);
        StartWrap<QuestDataLoader, int, QuestData>("daily-quest/list", dict => DailyQuestDataDic = dict);
        StartWrap<GachaLevelTableDataLoader, int, GachaLevelTableData>("gacha-level/list", dict => GachaLevelTableDataDic = dict);
        StartWrap<GachaTableDataLoader, int, GachaTableData>("gacha-table/list", dict => GachaTableDataDic = dict);
        StartWrap<GearDataLoader, int, GearData>("gear-data/list", dict => GearDataDic = dict);
        StartWrap<GearLevelDataLoader, int, GearLevelData>("gear-level/list", dict => GearLevelDataDic = dict);
        StartWrap<GoldDungeonDataLoader, int, GoldDungeonData>("gold-dungeon/list", dict => GoldDungeonDataDic = dict);
        StartWrap<QuestDataLoader, int, QuestData>("main-quest/list", dict => MainQuestDataDic = dict);
        StartWrap<MergeCostDataLoader, int, MergeCostData>("merge-cost-data/list", dict => MergeCostDataDic = dict);
        StartWrap<OfflineRewardDataLoader, int, OfflineRewardData>("offline-reward-data/list", dict  => OfflineRewardDataDic = dict);
        StartWrap<OwnedEffectLevelDataLoader, int, OwnedEffectLevelData>("owned-effect-level-data/list", dict => OwnedEffectLevelDataDic = dict);

        StartWrap<PartyDataLoader, int, PartyData>("partyData/list", dict => PartyDataDic = dict);
        StartWrap<PartyLevelDataLoader, int, PartyLevelData>("party-levelData/list", dict => PartyLevelDataDic = dict);
        StartWrap<ProfileDataLoader, int, ProfileData>("profile/list", dict => ProfileDataDic = dict);
        StartWrap<RewardDataLoader, int, RewardData>("quest-reward/list", dict => QuestRewardDataDic = dict);
        StartWrap<SkillDataLoader, int, SkillData>("skill/list", dict => SkillDataDic = dict);
        StartWrap<SkillLevelDataLoader, int, SkillLevelData>("skill-level/list", dict => SkillLevelDataDic = dict);
        StartWrap<StatUpgradeDataLoader, Define.StatType, StatUpgradeData>("statUpgradeData/list", dict => StatUpgradeDataDic = dict);
        StartWrap<TutorialDataLoader, int, TutorialData>("tutorial/list", dict => TutorialDataDic = dict);
        StartWrap<QuestDataLoader, int, QuestData>("tutorial-quest/list", dict => TutorialQuestDataDic  = dict);
        StartWrap<TutorialStepDataLoader, int, TutorialStepData>("tutorial-step/list", dict => TutorialStepDataDic = dict);
        StartWrap<BossDungeonDataLoader, int, BossDungeonData>("boss-dungeon/list", dict => BossDungeonDataDic = dict);
        StartWrap<StageDataLoader, int, StageData>("stageData/list", dict => StageDataDic = dict);

        //병렬 처리
        foreach (var co in coroutines)
        {
            CoroutineManager.StartCoroutine(co);
        }
        
        //완료 대기
        yield return new WaitUntil(() => currentCount >= totalCount);
        
        //후처리
        foreach (var data in OwnedEffectLevelDataDic.Values)
        {
            data.Init();
        }
        
        onComplete?.Invoke();
    }

    private IEnumerator WrapLoadParallel<Loader, Key, Value>(string endpoint, Action<Dictionary<Key, Value>> assign)
        where Loader : ILoader<Key, Value>
    {
        bool done = false;
        LoadFromServer<Loader, Key, Value>(endpoint, dict =>
        {
            assign(dict);
            done = true;
        });
        yield return new WaitUntil(() => done);
    }

    //Generic JSON Loader: 다양한 타입에 대응 가능
    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"{path}");
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }

    private void LoadFromServer<Loader, Key, Value>(string endpoint, Action<Dictionary<Key, Value>> onSuccess) where Loader : ILoader<Key, Value>
    {
        string url = $"https://us-central1-team-55ee2.cloudfunctions.net/api/{endpoint}";
        CoroutineManager.StartCoroutine(PostJsonRequest(url, (string json) =>
        {
            Loader loader = Managers.Data.ParseJsonText<Loader, Key, Value>(json);
            Dictionary<Key, Value> dict = loader.MakeDict();
            onSuccess?.Invoke(dict);
        }));
    }

    private IEnumerator PostJsonRequest(string url, Action<string> onSuccess)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{}"));
        
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            Debug.Log($"[API ERROR] {url} - {request.error}");
        }
    }

    private Loader ParseJsonText<Loader, Key, Value>(string json) where Loader : ILoader<Key, Value>
    {
        return JsonConvert.DeserializeObject<Loader>(json);
    }
    
}
