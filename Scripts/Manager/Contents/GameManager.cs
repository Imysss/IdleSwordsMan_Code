using System.Numerics;
using Data;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class GameManager
{
    //세이브 데이터
    #region SaveData

    private SaveLoadManager _saveLoad;

    public void Init()
    {
        _saveLoad = Managers.SaveLoad;

        // TODO: 저장된 데이터 불러오기
        if (_saveLoad.hasSaveData)
        {
            userName = _saveLoad.SaveData.userName;
            Gold = _saveLoad.SaveData.currencyData.Gold;
            Gems = _saveLoad.SaveData.currencyData.gem;
            isAutoSkillOn = _saveLoad.SaveData.isAutoSkillOn;
        }
        else
        {
            userName = "Guest";
            //재화 개수 초기화 -> 추후 삭제 해야됨
            Gold = 1000;
            Gems = 1500;
            SetAutoSkill(false);
        }

        // 개인정보 수집 동의 했을 시
        if (_saveLoad.SaveData.hasUserConsented)
        {
            // 개인정보 수집 시작
            Managers.Analytics.GiveConsent();
        }
        

        // 티켓 개수 초기화
        _goldDungeonTickets = _saveLoad.SaveData.currencyData.goldDungeonTicket;
        _bossDungeonTickets = _saveLoad.SaveData.currencyData.bossDungeonTicket;
        
        // 해금 상태 불러오기
        isSkillUnlocked = _saveLoad.SaveData.isSkillUnlocked;
        isPartyUnlocked = _saveLoad.SaveData.isPartyUnlocked;
        isDungeonUnlocked = _saveLoad.SaveData.isDungeonUnlocked;

        // 사운드 불러오기
        BGMOn = _saveLoad.SaveData.soundToggle[Sound.Bgm];
        SFXOn = _saveLoad.SaveData.soundToggle[Sound.Sfx];
        BgmVolume = _saveLoad.SaveData.soundVolume[Sound.Bgm];
        SfxVolume = _saveLoad.SaveData.soundVolume[Sound.Sfx];
        
        // Display Stats 비활성화
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false; 
    }

    #endregion

    //재화
    #region Goods
    public BigInteger Gold { get; private set; }
    public int Gems { get; private set; } // 예시로 보석(Gems) 추가

    /// <summary>
    /// 골드 획득
    /// </summary>
    public void AddGold(BigInteger amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("0 이하의 골드를 획득할 수 없습니다.");
            return;
        }

        Gold += amount;

        // Null 체크
        if (_saveLoad == null)
        {
            Debug.LogError("[AddGold] SaveLoadManager(_saveLoad)가 null입니다.");
            return;
        }

        if (_saveLoad.SaveData == null)
        {
            Debug.LogError("[AddGold] SaveData가 null입니다.");
            return;
        }

        if (_saveLoad.SaveData.currencyData == null)
        {
            Debug.LogError("[AddGold] currencyData가 null입니다.");
            return;
        }

        _saveLoad.SaveData.currencyData.GoldStr = Gold.ToString();
        // 골드가 변경되었음을 구독자들에게 알림 
        EventBus.Raise(new GoldChangeEvent(Gold));
    }

    /// <summary>
    /// 골드 사용
    /// </summary>
    /// <returns>사용에 성공하면 true, 실패(잔액 부족)하면 false</returns>
    public bool UseGold(BigInteger amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("0 이하의 골드를 사용할 수 없습니다.");
            return false;
        }

        if (Gold < amount)
        {
            Debug.Log("골드가 부족합니다.");
            return false;
        }

        Gold -= amount;
        //Debug.Log($"골드 {amount} 사용. 남은 골드: {Gold}");

        _saveLoad.SaveData.currencyData.GoldStr = Gold.ToString();
        
        // 골드가 변경되었음을 구독자들에게 알림
        EventBus.Raise(new GoldChangeEvent(Gold));
        return true;
    }

    /// <summary>
    /// 골드 획득
    /// </summary>
    public void AddGem(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("0 이하의 보석을 획득할 수 없습니다.");
            return;
        }

        Gems += amount;
        //Debug.Log($"보석 {amount}개 획득! 현재 보석: {Gems}");
        
        _saveLoad.SaveData.currencyData.gem = Gems;

        EventBus.Raise(new GemChangeEvent(Gems));
    }

    /// <summary>
    /// 골드 사용
    /// </summary>
    /// <returns>사용에 성공하면 true, 실패(잔액 부족)하면 false</returns>
    public bool UseGem(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("0 이하의 보석을 사용할 수 없습니다.");
            return false;
        }

        if (Gems < amount)
        {
            Debug.Log("보석이 부족합니다.");
            return false;
        }

        Gems -= amount;
        //Debug.Log($"보석 {amount}개 사용. 남은 보석: {Gems}");
        
        _saveLoad.SaveData.currencyData.gem = Gems;

        EventBus.Raise(new GemChangeEvent(Gems));
        return true;
    }
    #endregion

    //던전 티켓
    #region Dungeon Tickets
    private int _goldDungeonTickets; // 골드 던전 티켓 수
    private int _bossDungeonTickets; // 보스 던전 티켓 수
    
    // 현재 던전 티켓 수 반환
    public int GetDungeonTicketCount(DungeonType type)
    {
        return type switch
        {
            DungeonType.Gold => _goldDungeonTickets,
            DungeonType.Boss => _bossDungeonTickets,
            _ => 0
        };
    }
    
    // 던전 입장 시 티켓 1개 사용
    // <returns>성공 시 true, 부족 시 false</returns>
    public bool TryUseDungeonTicket(DungeonType type)
    {
        switch (type)
        {
            case DungeonType.Gold:
                if (_goldDungeonTickets <= 0)
                {
                    Debug.Log("골드 던전 티켓이 부족합니다.");
                    return false;
                }
                _goldDungeonTickets--;
                Debug.Log($"골드 던전 티켓 사용: 남은 {_goldDungeonTickets}");
                SaveDungeonTicket();
                EventBus.Raise(new DungeonTicketChangedEvent());
                return true;

            case DungeonType.Boss:
                if (_bossDungeonTickets <= 0)
                {
                    Debug.Log("보스 던전 티켓이 부족합니다.");
                    return false;
                }
                _bossDungeonTickets--;
                Debug.Log($"보스 던전 티켓 사용: 남은 {_bossDungeonTickets}");
                SaveDungeonTicket();
                EventBus.Raise(new DungeonTicketChangedEvent());
                return true;

            default:
                return false;
        }
    }
    
    // 외부에서 주기적으로 티켓 충전 (최대 2개)
    public void AddDungeonTicket(DungeonType type, int amount)
    {
        switch (type)
        {
            case DungeonType.Gold:
                _goldDungeonTickets += amount;
                Debug.Log($"골드 던전 티켓 +{amount}, 현재 {_goldDungeonTickets}");
                break;

            case DungeonType.Boss:
                _bossDungeonTickets += amount;
                Debug.Log($"보스 던전 티켓 +{amount}, 현재 {_bossDungeonTickets}");
                break;
        }
        
        EventBus.Raise(new DungeonTicketChangedEvent());
        SaveDungeonTicket();
    }

    public void SetDungeonTicketCount(DungeonType type, int amount)
    {
        switch (type)
        {
            case DungeonType.Gold:
                _goldDungeonTickets = amount;
                break;
            case DungeonType.Boss:
                _bossDungeonTickets = amount;
                break;
        }
        EventBus.Raise(new DungeonTicketChangedEvent());
        SaveDungeonTicket();
    }

    private void SaveDungeonTicket()
    {
        _saveLoad.SaveData.currencyData.bossDungeonTicket = _bossDungeonTickets;
        _saveLoad.SaveData.currencyData.goldDungeonTicket = _goldDungeonTickets;
    }

    #endregion

    //사운드 옵션
    #region Sound Option
    private bool _bgmOn = true;
    private bool _sfxOn = true;
    private float _bgmVolume = 1.0f;
    private float _sfxVolume = 1.0f;

    //BGMOn 설정 변경 시 자동으로 재생/중단 처리 수정
    public bool BGMOn
    {
        get { return _bgmOn; }
        set
        {
            if (_bgmOn == value)
            {
                Debug.Log("BgmOn Same");
                return;
            }
            _bgmOn = value;
            if (_bgmOn == false)
            {
                Managers.Sound.Stop(Define.Sound.Bgm);
            }
            else
            {
                //씬에 따라 재생할 배경 이름 분기 
                string name = "BGM_Game";
                if (Managers.Scene.CurrentScene.SceneType == Define.Scene.TitleScene)
                    name = "BGM_Title";
                Debug.Log($"BGM ON: {name}");
                Managers.Sound.Play(Define.Sound.Bgm, name);
            }
            _saveLoad.SaveData.soundToggle[Sound.Bgm] = _bgmOn;
        }
    }

    //SFX
    public bool SFXOn
    {
        get { return _sfxOn; }
        set
        {
            _sfxOn = value;
            _saveLoad.SaveData.soundToggle[Sound.Sfx] = _sfxOn;
        }
    }

    public float BgmVolume
    {
        get
        {
            _bgmVolume = _saveLoad.SaveData.soundVolume[Sound.Bgm];
            return _bgmVolume;
        }
        set
        {
            _bgmVolume = value;
            Managers.Sound.SetVolume(Define.Sound.Bgm, value);
            _saveLoad.SaveData.soundVolume[Sound.Bgm] = _bgmVolume;
        }
    }

    public float SfxVolume
    {
        get
        {
            _sfxVolume = _saveLoad.SaveData.soundVolume[Sound.Sfx];
            return _sfxVolume;
        }
        set
        {
            _sfxVolume = value;
            Managers.Sound.SetVolume(Define.Sound.Sfx, value);
            _saveLoad.SaveData.soundVolume[Sound.Sfx] = _sfxVolume;
        }
    }
    #endregion
    
    //시스템 해금
    #region System Unlock

    public bool isSkillUnlocked { get; private set; } = false;
    public bool isPartyUnlocked { get; private set; } = false;
    public bool isDungeonUnlocked { get; private set; } = false;

    public void UnlockSystem(UnlockType unlockType)
    {
        switch (unlockType)
        {
            case UnlockType.Skill :
                isSkillUnlocked = true;
                _saveLoad.SaveData.isSkillUnlocked = isSkillUnlocked;
                Managers.Equipment.SkillEquipment.AddSlot(1);
                //이거 호출되면 해당 skill 해금된다는 뜻 -> UI 호출
                Managers.UI.GameSceneUI.UnlockSkillToggle(() =>
                {
                    Managers.UI.GameSceneUI.UnlockSkillSlot(() =>
                    {
                        Managers.Tutorial.StartTutorial(9902);
                    });
                });
                break;
            case UnlockType.Party :
                isPartyUnlocked = true;
                _saveLoad.SaveData.isPartyUnlocked = isPartyUnlocked;
                Managers.Equipment.PartyEquipment.AddSlot(1);
                Managers.UI.GameSceneUI.UnlockPartyToggle(() =>
                {
                    Managers.Tutorial.StartTutorial(9903);
                });
                break;
            case UnlockType.Dungeon :
                isDungeonUnlocked = true;
                _saveLoad.SaveData.isDungeonUnlocked = isDungeonUnlocked;
                Managers.UI.GameSceneUI.UnlockDungeonToggle(() =>
                {
                    Managers.Tutorial.StartTutorial(9904);
                });
                break;
            case UnlockType.SkillSlot : 
                Managers.Equipment.SkillEquipment.AddSlot(1);
                //슬롯 최대치 오픈 9908
                if (Managers.Equipment.SkillEquipment.currentSlots == Managers.Equipment.SkillEquipment.maxSlots)
                {
                    //스킬 슬롯 잠금 해제 애니메이션 수행 후 튜토리얼 실행
                    Managers.UI.GameSceneUI.UnlockSkillSlot(() =>
                    {
                        Managers.Tutorial.StartTutorial(9908);
                    });
                }
                else
                {
                    Managers.UI.GameSceneUI.UnlockSkillSlot(() =>
                    {
                        Managers.Tutorial.StartTutorial(9906);
                    });
                }
                break;
            case UnlockType.PartySlot :
                Managers.Equipment.PartyEquipment.AddSlot(1);
                //동료 최대치 오픈
                if (Managers.Equipment.PartyEquipment.currentSlots == Managers.Equipment.PartyEquipment.maxSlots)
                {
                    Managers.Tutorial.StartTutorial(9909);
                }
                else
                {
                    Managers.Tutorial.StartTutorial(9907);
                }
                break;
        }
        EventBus.Raise(new UnlockSystemEvent());
        
    }
    
    #endregion
    
    //Auto Skill
    private bool isAutoSkillOn;
    public bool IsAutoSkillOn { get { return isAutoSkillOn; } }
    public void SetAutoSkill(bool auto)
    {
        isAutoSkillOn = auto;
        _saveLoad.SaveData.isAutoSkillOn = isAutoSkillOn;
    }
    
    //Option
    public bool AlarmCheck;
    
    //User Info
    public string userName;
}
