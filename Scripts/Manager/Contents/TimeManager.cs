using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Data;
using UnityEngine;

// 게임 내 시간 관련 관리 클래스
// 출석 체크, 방치 보상, 광고 쿨타임/횟수 제한 등 모든 시간 기반 기능을 처리
public class TimeManager : MonoBehaviour
{
    //방치 동안 획득한 아이템 보상 목록(데이터 ID 저장)
    //key: itemId, value: count
    private int offlineRewardGachaLevel;
    private Dictionary<int, int> _offlineRewardItemIds = new();
    public IReadOnlyDictionary<int, int> OfflineRewardItemIds => _offlineRewardItemIds;
    
    //보너스 방치 보상 아이템 저장
    private int bonusRewardGachaLevel;
    private Dictionary<int, int> _bonusRewardCache;
    public IReadOnlyDictionary<int, int> BonusRewardCache => _bonusRewardCache;
    
    // 현재 접속 시간을 int 로 반환 (초)
    public int CurrentPlayTimeSec
    {
        get { return (DateTime.Now - _gameStartTime).Seconds;}
    }
    
    // 일일 접속 시간을 int 로 반환 (초)
    public int DailyPlayTimeSec
    {
        get
        {
            TimeSpan currentSessionDuration = DateTime.Now - _gameStartTime;
            return Managers.SaveLoad.SaveData.timeData.dailyPlayTimeSec + (int)currentSessionDuration.TotalSeconds;
        }
    }

    // 누적 플레이 타임 (초)
    public int TotalPlayTimeSec
    {
        get {return Managers.SaveLoad.SaveData.timeData.totalPlayTimeSec + DailyPlayTimeSec;}
    }
    
    // 접속 시간
    private DateTime _gameStartTime;
    
    //2배 보상 남은 시간
    private DateTime doubleSpeedEndTime = DateTime.MinValue;
    public bool IsDoubleSpeed => DateTime.UtcNow < doubleSpeedEndTime;
    public float GetRemainingDoubleSpeedTime() => Mathf.Max(0f, (float)(doubleSpeedEndTime - DateTime.UtcNow).TotalSeconds);
    
    //방치 보상: 앱이 종료되거나 백그라운드 상태일 때도 경과 시간을 기억해야 하기 때문에 시스템 시간을 사용해 신뢰성 있는 시간 계산
    //보상 지급 후 마지막 지급 시간을 저장하여 다음 방치 보상을 받을 때는 경과 시간을 기반으로 보상을 계산함
    //마지막 방치 보상 획득 시간
    public DateTime LastRewardTime
    {
        get
        {
            string saved = Managers.SaveLoad.SaveData.timeData.lastRewardTimeStr;
            if (string.IsNullOrEmpty(saved))
            {
                DateTime initialTime = _gameStartTime;
                LastRewardTime = initialTime;
                return initialTime;
            }
            return DateTime.Parse(saved);
        }
        set
        {
            Managers.SaveLoad.SaveData.timeData.lastRewardTimeStr = value.ToString("o");
        }
    }

    // 마지막 보상 이후 경과 시간 (최대 24시간)
    public TimeSpan TimeSinceLastReward
    {
        get
        {
            TimeSpan timeSpan = DateTime.Now - LastRewardTime;
            return timeSpan > TimeSpan.FromHours(24) ? TimeSpan.FromHours(24) : timeSpan;
        }
    }
    
    //하루 1회 보너스 보상 시 광고를 보고 받은 시간
    private DateTime _lastBonusRewardTime
    {
        get
        {
            string saved = Managers.SaveLoad.SaveData.timeData.lastBonusRewardTimeStr;
            if (string.IsNullOrEmpty(saved))
                return DateTime.MinValue;

            return DateTime.Parse(saved);
        }
        set
        {
            Managers.SaveLoad.SaveData.timeData.lastBonusRewardTimeStr = value.ToString("o");
        }
    }
    
    //오늘 보너스 광고 받았는지 확인
    public bool CanReceiveBonusOfflineReward()
    {
        return _lastBonusRewardTime.Date < DateTime.Now.Date;
    }

    // 출석 일수
    public int AttendanceDay
    {
        get => Managers.SaveLoad.SaveData.timeData.attendanceDay;
        set
        {
            Managers.SaveLoad.SaveData.timeData.attendanceDay = value;
        }
    }

    // 마지막 로그인 시각
    public DateTime LastLoginTime
    {
        get
        {
            string saved = Managers.SaveLoad.SaveData.timeData.lastLoginTimeStr;
            return string.IsNullOrEmpty(saved) ? DateTime.Now : DateTime.Parse(saved);
        }
        set
        {
            Managers.SaveLoad.SaveData.timeData.lastLoginTimeStr = value.ToString("o");
        }
    }

    // 출석 보상 수령 여부
    public bool IsAttendanceRewardClaimed
    {
        get => Managers.SaveLoad.SaveData.timeData.isAttendanceRewardClaimed;
        set => Managers.SaveLoad.SaveData.timeData.isAttendanceRewardClaimed = value;
    }

    private void Start()
    {
        _gameStartTime = DateTime.Now;  // 게임 시작 시간 기록
    }

    // 초기화: 출석체크 및 일일 광고 가챠 리셋
    public void Init()
    {
        offlineRewardGachaLevel = Managers.SaveLoad.SaveData.offlineRewardGachaLevel;
        bonusRewardGachaLevel = -1;
        CheckAttendance();
        CheckAdGachaReset(); // 날짜가 바뀌었으면 광고 가챠 카운트 초기화
        StartCoroutine(CoCheckDayChanged());
        _offlineRewardItemIds = Managers.SaveLoad.SaveData.offLineRewardItemIds;

        string saved = Managers.SaveLoad.SaveData.timeData.doubleSpeedEndTimeStr;
        if (!string.IsNullOrEmpty(saved))
        {
            doubleSpeedEndTime = DateTime.Parse(saved, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
        }
        // Debug.Log($"Init: double speed end time: {doubleSpeedEndTime}");
        // Debug.Log($"Init: double speed is double speed: {IsDoubleSpeed}");
        // Debug.Log($"Init: double speed remaining time: {GetRemainingDoubleSpeedTime()}");
        
        //Managers.Push.CancelAllNotifications();
    }

    // 출석 확인 및 출석일 갱신
    private void CheckAttendance()
    {
        if (string.IsNullOrEmpty(Managers.SaveLoad.SaveData.timeData.lastLoginTimeStr))
        {
            AttendanceDay = 1;
            LastLoginTime = DateTime.Now;
            IsAttendanceRewardClaimed = false;
            InitDailyData();
        }
        else if (!IsSameDay(LastLoginTime, DateTime.Now))
        {
            AttendanceDay++;
            LastLoginTime = DateTime.Now;
            IsAttendanceRewardClaimed = false;
            InitDailyData();

            StartCoroutine(CoShowAttendancePopup());
        }
        else
        {
            Debug.Log("Already Checked Today");
        }
    }

    private IEnumerator CoShowAttendancePopup()
    {
        yield return new WaitForSeconds(0.3f);
        Managers.UI.ShowPopupUI<UIAttendancePopup>().SetInfo();
    }

    // 날짜 비교 (년/월/일만)
    private bool IsSameDay(DateTime savedTime, DateTime currentTime)
    {
        return savedTime.Year == currentTime.Year && savedTime.Month == currentTime.Month && savedTime.Day == currentTime.Day;
    }

    // 매 분마다 날짜 변경 여부 확인 (앱을 종료하지 않아도 체크됨)
    IEnumerator CoCheckDayChanged()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f);
            if (!IsSameDay(LastLoginTime, DateTime.Now))
            {
                CheckAttendance();
            }
            Managers.Quest.UpdatePlayTimeQuests();  // 1분마다 접속시간 퀘스트 업데이트
        }
    }

    // 2배속 적용 (지속 시간 설정 가능)
    public void SetDoubleSpeed(float duration)
    {
        doubleSpeedEndTime = DateTime.UtcNow.AddSeconds(duration);
        
        //저장
        Managers.SaveLoad.SaveData.timeData.doubleSpeedEndTimeStr = doubleSpeedEndTime.ToString("o");
        Managers.SaveLoad.Save();
        
        StartCoroutine(CoDoubleSpeed(duration));
        Managers.UI.GameSceneUI.StartDoubleSpeedUI();
    }

    public IEnumerator CoDoubleSpeed(float duration)
    {
        Time.timeScale = 2f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    // 방치 보상 지급
    public void GiveOfflineReward(OfflineRewardData data)
    {
        //1. 골드 계산 및 지급
        BigInteger rewardGold = NumberFormatter.Parse(data.rewardGold); 
        BigInteger gold = CalculateGoldPerMinute(rewardGold);
        Managers.Game.AddGold(gold);    //골드 추가
        
        //2. 아이템 계산 및 지급
        foreach (var pair in _offlineRewardItemIds)
        {
            int itemId = pair.Key;
            int count = pair.Value;

            for (int i = 0; i < count; i++)
            {
                Managers.Inventory.AddItem(itemId);
            }
        }
        _offlineRewardItemIds.Clear(); //받을 아이템 삭제
        
        //3. 기준 시점 갱신
        LastRewardTime = DateTime.Now;  //현재 시점을 기준으로 다음 기준점 저장 (보상 획득했음)
    }

    // 분당 골드 계산
    public BigInteger CalculateGoldPerMinute(BigInteger goldPerHour)
    {
        BigInteger goldPerMinute = goldPerHour / 60 * (BigInteger)TimeSinceLastReward.TotalMinutes;
        return goldPerMinute;
    }
    
    //방치 보상 아이템 계산
    public void CalculateOfflineRewardItems()
    {
        //현재 맵에 따라 가챠 레벨 결정
        //전체 스테이지 수: 180, gacha level: 20 -> 각 그룹에 9개 스테이지가 들어감 
        int level = (Managers.Level.GetCurrentLevel() - 1) / 9 + 1;
        int gachaLevel = Mathf.Clamp(level, 1, Managers.Data.GachaLevelTableDataDic.Count);

        if (offlineRewardGachaLevel != gachaLevel)
        {
            _offlineRewardItemIds.Clear();
            offlineRewardGachaLevel = gachaLevel;
            Managers.SaveLoad.SaveData.offlineRewardGachaLevel = offlineRewardGachaLevel;
        }
        
        //Debug.Log($"gacha level: {gachaLevel}");
        
        //30분마다 1개 보상 -> 최대 48개의 아이템 획득 가능
        int totalCount = Mathf.FloorToInt((float)TimeSinceLastReward.TotalMinutes / 30f);   //지급받아야 하는 보상 개수
        int currentCount = _offlineRewardItemIds.Values.Sum(); //현재 뽑아진 보상 개수
        int newItemCount = totalCount - currentCount;   //뽑아야 할 아이템 개수
        
        if (newItemCount <= 0)
            return; //추가 계산할 필요 없음
        

        for (int i = 0; i < newItemCount; i++)
        {
            Define.GachaType randomType = Managers.Gacha.GetRandomGachaType(); //가챠 3종 중 하나 랜덤으로 결정
            int rewardedItemId = Managers.Gacha.DoOfflineGacha(randomType, gachaLevel);
            if (rewardedItemId != -1)
            {
                if (_offlineRewardItemIds.ContainsKey(rewardedItemId))
                    _offlineRewardItemIds[rewardedItemId]++;
                else
                    _offlineRewardItemIds[rewardedItemId] = 1;
            }
        }
        
        // 방치 아이템 정보 저장
        Managers.SaveLoad.SaveData.offLineRewardItemIds = _offlineRewardItemIds;
    }
    
    //광고 시청 후 1회 보너스 오프라인 보상 지급
    public void CalculateBonusOfflineRewardItems()
    {
        //현재 맵에 따라 가챠 레벨 결정
        //전체 스테이지 수: 180, gacha level: 20 -> 각 그룹에 9개 스테이지가 들어감 
        int level = (Managers.Level.GetCurrentLevel() - 1) / 9 + 1;
        int gachaLevel = Mathf.Clamp(level, 1, Managers.Data.GachaLevelTableDataDic.Count);
        
        if (_bonusRewardCache != null && gachaLevel == bonusRewardGachaLevel)
            return; //이미 계산된 상태

        _bonusRewardCache = new();
        bonusRewardGachaLevel = gachaLevel;
        Debug.Log($"gacha level: {gachaLevel}");
        
        int bonusItemCount = 48;    //총 보상 아이템 개수 (24시간 -> 48개)
        
        for (int i = 0; i < bonusItemCount; i++)
        {
            Define.GachaType randomType = Managers.Gacha.GetRandomGachaType();  //3종 중 랜덤
            int rewardedItemId = Managers.Gacha.DoOfflineGacha(randomType, gachaLevel);
            if (rewardedItemId != -1)
            {
                if (_bonusRewardCache.ContainsKey(rewardedItemId))
                    _bonusRewardCache[rewardedItemId]++;
                else
                    _bonusRewardCache[rewardedItemId] = 1;
            }
        }
    }

    public void GiveBonusOfflineReward(OfflineRewardData data)
    {
        if (!CanReceiveBonusOfflineReward() || _bonusRewardCache == null)
            return;
        
        //1. 골드 지급 (24시간 고정)
        BigInteger rewardGold = NumberFormatter.Parse(data.rewardGold);
        BigInteger gold = rewardGold * 24;
        Managers.Game.AddGold(gold);
        
        //2. 아이템 지급
        foreach (var pair in _bonusRewardCache)
        {
            for (int i = 0; i < pair.Value; i++)
                Managers.Inventory.AddItem(pair.Key);
        }
        
        //3. 날짜 기록
        _lastBonusRewardTime = DateTime.Now;
        
        //4. 캐시 제거
        _bonusRewardCache = null;
    }
    
    // 출석 또는 날짜 변경 시 하루 단위 데이터 초기화
    private void InitDailyData()
    {
        Debug.Log("Init Daily Data");

        if (Managers.Game.GetDungeonTicketCount(Define.DungeonType.Boss) < Define.DUNGEON_TICKET_MAX_VALUE) 
            Managers.Game.SetDungeonTicketCount(Define.DungeonType.Boss, Define.DUNGEON_TICKET_MAX_VALUE);

        if (Managers.Game.GetDungeonTicketCount(Define.DungeonType.Gold) < Define.DUNGEON_TICKET_MAX_VALUE)
            Managers.Game.SetDungeonTicketCount(Define.DungeonType.Gold, Define.DUNGEON_TICKET_MAX_VALUE);

        Managers.SaveLoad.SaveData.timeData.isBossDungeonAdClaimedToday = false;
        Managers.SaveLoad.SaveData.timeData.isGoldDungeonAdClaimedToday = false;

        Managers.Quest.ResetDailyQuest();
    }

    // 앱 종료 시 마지막 보상 시간 저장
    // OnApplicationPause에서 처리하므로 주석처리
    private void OnApplicationQuit()
    {
        Managers.SaveLoad.SaveData.timeData.lastRewardTimeStr = LastRewardTime.ToString("o");
        
        if (TimeSinceLastReward < TimeSpan.FromHours(24))
        {
            // 테스트용 (1분 후 푸시)
            //DateTime rewardTime = DateTime.Now.AddMinutes(1);
        
            // 실사용용 (24시간 후 푸시)
            DateTime rewardTime = LastRewardTime.AddHours(24);

            //Managers.Push.SendNotification("방치 보상 도착!", "다시 접속하고 보상을 받아보세요!", rewardTime);

        }
    }
    
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Managers.SaveLoad.SaveData.timeData.lastRewardTimeStr = LastRewardTime.ToString("o");
            
            //  현재까지의 누적 시간을 최종 저장
            int currentSessionSeconds = (int)(DateTime.Now - _gameStartTime).TotalSeconds;
            Managers.SaveLoad.SaveData.timeData.dailyPlayTimeSec += currentSessionSeconds;
            Managers.SaveLoad.SaveData.timeData.totalPlayTimeSec += currentSessionSeconds;
            
            if (TimeSinceLastReward < TimeSpan.FromHours(24))
            {
                // 테스트용 (1분 후 푸시)
                //DateTime rewardTime = DateTime.Now.AddMinutes(1);

                // 실사용용 (24시간 후 푸시)
                DateTime rewardTime = LastRewardTime.AddHours(24);

                //Managers.Push.SendNotification("방치 보상 도착!", "다시 접속하고 보상을 받아보세요!", rewardTime);
            }
        }
        else // 앱이 다시 활성화될 때
        {
            // 세션 시작 시간을 현재로 리셋하여 중복 계산 방지
            _gameStartTime = DateTime.Now;
        }
    }

    // 광고 가챠 쿨타임 체크 (15분)
    public bool IsAdGachaAvailable(Define.GachaType type)
    {
        if (!Managers.SaveLoad.SaveData.timeData.lastAdGachaTimeStr.TryGetValue(type, out string timeStr) || string.IsNullOrEmpty(timeStr))
            return true;

        DateTime lastTime = DateTime.Parse(timeStr);
        return DateTime.Now - lastTime >= TimeSpan.FromMinutes(15);
    }

    // 광고 가챠 쿨타임 남은 시간
    public TimeSpan GetAdGachaCooldown(Define.GachaType type)
    {
        if (!Managers.SaveLoad.SaveData.timeData.lastAdGachaTimeStr.TryGetValue(type, out string timeStr) || string.IsNullOrEmpty(timeStr))
            return TimeSpan.Zero;

        DateTime lastTime = DateTime.Parse(timeStr);
        TimeSpan elapsed = DateTime.Now - lastTime;
        return elapsed < TimeSpan.FromMinutes(15) ? TimeSpan.FromMinutes(15) - elapsed : TimeSpan.Zero;
    }

    // 광고 가챠 사용 처리 (쿨타임 저장 + 카운트 증가)
    public void SetAdGachaUsed(Define.GachaType type)
    {
        Managers.SaveLoad.SaveData.timeData.lastAdGachaTimeStr[type] = DateTime.Now.ToString("o");

        if (!Managers.SaveLoad.SaveData.timeData.adGachaDailyCount.ContainsKey(type))
            Managers.SaveLoad.SaveData.timeData.adGachaDailyCount[type] = 0;

        Managers.SaveLoad.SaveData.timeData.adGachaDailyCount[type]++;
    }

    // 광고 가챠 하루 3회 제한 검사
    public bool IsUnderDailyAdLimit(Define.GachaType type)
    {
        if (!Managers.SaveLoad.SaveData.timeData.adGachaDailyCount.TryGetValue(type, out int count))
            return true;

        return count < 3;
    }

    // 광고 가챠 사용 가능 여부 (쿨타임 + 횟수 모두 만족해야 true)
    public bool CanUseAdGacha(Define.GachaType type)
    {
        return IsAdGachaAvailable(type) && IsUnderDailyAdLimit(type);
    }

    // 날짜 변경 시 광고 가챠 사용 횟수 초기화
    private void CheckAdGachaReset()
    {
        if (!IsSameDay(LastLoginTime, DateTime.Now))
        {
            Managers.SaveLoad.SaveData.timeData.adGachaDailyCount.Clear();
            Managers.SaveLoad.SaveData.timeData.dailyPlayTimeSec = 0;
        }
    }
    
}
