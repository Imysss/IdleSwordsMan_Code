using System;
using System.Collections;
using System.Numerics;
using Assets.HeroEditor.Common.Scripts.Common;
using Data;
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEditor.Analytics;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class UIGameScene : UIScene
{
    #region UI 기능 리스트
    //UserInfoGroup
    //ProfileButton: 프로필 팝업 버튼
    //GoldButton: 골드 버튼 (클릭 -> 골드 정보 나타내기)
    //GoldValueText: 골드 정보 텍스트
    //GemButton: 젬 버튼 (클릭 -> 젬 정보 나타내기)
    //GemValueText: 젬 정보 텍스트
    //CombatPowerButton: 전투력 버튼 (클릭 -> 전투력 정보 나타내기)
    //CombatPowerValueText: 전투력 정보 텍스트
    //DoubleSpeedButton: 2배속 버튼 (클릭 -> 전투 2배속)
    //DoubleSpeedTimeText: (눌리기 전-2x -> 눌린 후-시간(11:23))
    //GoldDescriptionObject: 골드 설명
    //GemDescriptionObject: 젬 설명
    
    //StageInfoGroup
    //MapNameText: 맵 이름 텍스트
    //StageText: 스테이지 텍스트 1-1~10-6
    //TimeText: 현재 타임 텍스트
    //StageSlider: 현재 웨이브 진행도
    //BossButton: 보스 웨이브로 진입하는 버튼
    
    //MenuGroup
    //HeroToggle: 스탯 업그레이드 토글
    //GearToggle: 장비 토글
    //SkillToggle: 스킬 토글
    //PartyToggle: 동료 토글
    //DungeonToggle: 던전 토글
    //ShopToggle: 상점 토글 버튼
    
    //GearRedDotObject
    //SkillRedDotObject
    //PartyRedDotObject
    
    //RightGroup
    //DailyQuestButton: 메일 버튼
    //OptionButton: 옵션 버튼
    //AttendanceButton: 출석체크 버튼
    //OfflineRewardButton: 방치 보상 버튼
    
    //SkillGroup
    //SkillGroupObject: 장착 중인 스킬 그룹
    //AutoSkillButton: 스킬 온/오프 버튼
    //AutoSkillButtonText: 스킬 온/오프 텍스트
    //AutoSkillOnEffectImage: 스킬 온 -> Z축 회전으로 돌아가기
    #endregion
    
    #region Enum
    enum GameObjects
    {
        CheckHeroImageObject,
        CheckGearImageObject,
        CheckSkillImageObject,
        CheckPartyImageObject,
        CheckDungeonImageObject,
        CheckShopImageObject,
        
        StageInfoGroup,
        StageSlider,
        
        SkillGroupObject,
        AutoSkillOnEffectImage,
        
        GoldDescriptionObject,
        GemDescriptionObject,
        
        HeroRedDotObject,
        GearRedDotObject,
        SkillRedDotObject,
        PartyRedDotObject,
        
        UserInfoGroup,
        RightMenuGroup,
        SkillContent,
        
        SkillLockObject,
        PartyLockObject,
        DungeonLockObject,
        
        OfflineRewardRedDotObject,
        DailyQuestRedDotObject,
        
        SkillSwapBlockPanel,
        
        ProfileRedDotObject,
    }
    
    enum Buttons
    {
        ProfileButton,
        GoldButton,
        GemButton,
        CombatPowerButton,
        DoubleSpeedButton,
        
        DailyQuestButton,
        OptionButton,
        AttendanceButton,
        OfflineRewardButton,
        
        BossButton,
        
        AutoSkillButton,
        
        MaskedScreen,
    }

    enum Texts
    {
        GoldValueText,
        GemValueText,
        CombatPowerValueText,
        DoubleSpeedTimeText,
        
        StageText,
        
        AutoSkillButtonText,
    }

    enum Images
    {
        ProfileImage,
        FrameImage,
        
        DoubleSpeedImage,
        
        SkillLockImage,
        SkillUnlockImage,
        PartyLockImage,
        PartyUnlockImage,
        DungeonLockImage,
        DungeonUnlockImage,
    }
    
    enum Toggles
    {
        HeroToggle,
        GearToggle,
        SkillToggle,
        PartyToggle,
        DungeonToggle,
        ShopToggle,
    }
    #endregion
    
    private float rotationSpeed = 180f;
    private int sortOrder = 100;
    
    private BigInteger _prevGold = 0;
    private BigInteger _prevGem = 0;

    private Coroutine _refreshSkillSlotRoutine;

    private UIHeroPopup _heroPopupUI;
    private bool _isSelectedHero;
    private UIGearPopup _gearPopupUI;
    private bool _isSelectedGear;
    private UISkillPopup _skillPopupUI;
    private bool _isSelectedSkill;
    private UIPartyPopup _partyPopupUI;
    private bool _isSelectedParty;
    private UIDungeonPopup _dungeonPopupUI;
    private bool _isSelectedDungeon;
    private UIShopPopup _shopPopupUI;
    private bool _isSelectedShop;

    private CanvasGroup heroRedDot;
    private CanvasGroup gearRedDot;
    private CanvasGroup skillRedDot;
    private CanvasGroup partyRedDot;

    private UISkillSlotItem _nextUnlockSlot = null;
    
    private Action _skillSwapEventAction;
    
    private Coroutine _doubleSpeedCoroutine;

    private void OnEnable()
    {
        EventBus.Subscribe<TransitionEndEvent>(TransitionEndHandler);
        EventBus.Subscribe<GemChangeEvent>(GemChangedEventHandler);
        EventBus.Subscribe<GoldChangeEvent>(GoldChangeEventHandler);
        EventBus.Subscribe<SkillChangedEvent>(SkillChangedEventHandler);
        EventBus.Subscribe<DungeonStateChangedEvent>(DungeonStateChangedEventHandler);
        EventBus.Subscribe<CombatPowerChangedEvent>(CombatPowerChangedEventHandler);
        
        EventBus.Subscribe<GearStateChangedEvent>(GearStateChangedEventHandler);
        EventBus.Subscribe<SkillStateChangedEvent>(SkillStateChangedEventHandler);
        EventBus.Subscribe<PartyStateChangedEvent>(PartyStateChangedEventHandler);
        
        EventBus.Subscribe<ProfileChangedEvent>(ProfileChangedEventHandler);
        EventBus.Subscribe<ProfileRedDotChangedEvent>(ProfileRedDotChangedEventHandler);
        
        EventBus.Subscribe<QuestStateChangedEvent>(QuestStateChangedEventHandler);
        
        EventBus.Subscribe<BossTransitionEvent>(BossTransitionEventHandler);
        EventBus.Subscribe<BossTransitionEndEvent>(BossTransitionEndEventHandler);
        
        EventBus.Subscribe<SkillSwapStartEvent>(SkillSwapStartHandler);
        EventBus.Subscribe<SkillSwapEndEvent>(SkillSwapEndHandler);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<TransitionEndEvent>(TransitionEndHandler);
        EventBus.UnSubscribe<GemChangeEvent>(GemChangedEventHandler);
        EventBus.UnSubscribe<GoldChangeEvent>(GoldChangeEventHandler);
        EventBus.UnSubscribe<SkillChangedEvent>(SkillChangedEventHandler);
        EventBus.UnSubscribe<DungeonStateChangedEvent>(DungeonStateChangedEventHandler);
        EventBus.UnSubscribe<CombatPowerChangedEvent>(CombatPowerChangedEventHandler);
        
        EventBus.UnSubscribe<GearStateChangedEvent>(GearStateChangedEventHandler);
        EventBus.UnSubscribe<SkillStateChangedEvent>(SkillStateChangedEventHandler);
        EventBus.UnSubscribe<PartyStateChangedEvent>(PartyStateChangedEventHandler);
        
        EventBus.UnSubscribe<ProfileChangedEvent>(ProfileChangedEventHandler);
        EventBus.UnSubscribe<ProfileRedDotChangedEvent>(ProfileRedDotChangedEventHandler);
        
        EventBus.UnSubscribe<QuestStateChangedEvent>(QuestStateChangedEventHandler);
        
        EventBus.UnSubscribe<BossTransitionEvent>(BossTransitionEventHandler);
        EventBus.UnSubscribe<BossTransitionEndEvent>(BossTransitionEndEventHandler);
                
        EventBus.UnSubscribe<SkillSwapStartEvent>(SkillSwapStartHandler);
        EventBus.UnSubscribe<SkillSwapEndEvent>(SkillSwapEndHandler);
    }
    
    private void Update()
    {
        if (Managers.Game.IsAutoSkillOn)
        {
            GetObject((int)GameObjects.AutoSkillOnEffectImage).transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }
    }

    public override bool Init()
    {
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        BindToggle(typeof(Toggles));
        
        ApplySafeArea();

        //튜토리얼 버튼 연결
        Managers.Tutorial.AddUIButton(Define.UIButtonType.Hero, GetToggle((int)Toggles.HeroToggle).gameObject);
        Managers.Tutorial.AddUIButton(Define.UIButtonType.Equipment, GetToggle((int)Toggles.GearToggle).gameObject);
        Managers.Tutorial.AddUIButton(Define.UIButtonType.Skill, GetToggle((int)Toggles.SkillToggle).gameObject);
        Managers.Tutorial.AddUIButton(Define.UIButtonType.Party, GetToggle((int)Toggles.PartyToggle).gameObject);
        Managers.Tutorial.AddUIButton(Define.UIButtonType.Dungeon, GetToggle((int)Toggles.DungeonToggle).gameObject);
        Managers.Tutorial.AddUIButton(Define.UIButtonType.Shop, GetToggle((int)Toggles.ShopToggle).gameObject);
        Managers.Tutorial.AddUIButton(Define.UIButtonType.AutoSkill, GetButton((int)Buttons.AutoSkillButton).gameObject);
        Managers.Tutorial.AddUIButton(Define.UIButtonType.BossEnter, GetButton((int)Buttons.BossButton).gameObject);
        
        GetToggle((int)Toggles.HeroToggle).gameObject.BindEvent(OnClickHeroToggle);
        GetToggle((int)Toggles.GearToggle).gameObject.BindEvent(OnClickGearToggle);
        GetToggle((int)Toggles.SkillToggle).gameObject.BindEvent(OnClickSkillToggle);
        GetToggle((int)Toggles.PartyToggle).gameObject.BindEvent(OnClickPartyToggle);
        GetToggle((int)Toggles.DungeonToggle).gameObject.BindEvent(OnClickDungeonToggle);
        GetToggle((int)Toggles.ShopToggle).gameObject.BindEvent(OnClickShopToggle);

        _heroPopupUI = Managers.UI.ShowPopupUI<UIHeroPopup>();
        _gearPopupUI = Managers.UI.ShowPopupUI<UIGearPopup>();
        _skillPopupUI = Managers.UI.ShowPopupUI<UISkillPopup>();
        _partyPopupUI = Managers.UI.ShowPopupUI<UIPartyPopup>();
        _dungeonPopupUI = Managers.UI.ShowPopupUI<UIDungeonPopup>();
        _shopPopupUI = Managers.UI.ShowPopupUI<UIShopPopup>();
        
        GetButton((int)Buttons.ProfileButton).gameObject.BindEvent(OnClickProfileButton);
        GetButton((int)Buttons.GoldButton).gameObject.BindEvent(OnClickGoldButton);
        GetButton((int)Buttons.GemButton).gameObject.BindEvent(OnClickGemButton);
        GetButton((int)Buttons.CombatPowerButton).gameObject.BindEvent(OnClickCombatPowerButton);
        GetButton((int)Buttons.DoubleSpeedButton).gameObject.BindEvent(OnClickDoubleSpeedButton);

        GetButton((int)Buttons.AttendanceButton).gameObject.BindEvent(OnClickAttendanceButton);
        GetButton((int)Buttons.OptionButton).gameObject.BindEvent(OnClickOptionButton);
        GetButton((int)Buttons.DailyQuestButton).gameObject.BindEvent(OnClickDailyButton);
        GetButton((int)Buttons.OfflineRewardButton).gameObject.BindEvent(OnClickOfflineRewardButton);
        //GetButton((int)Buttons.SleepModeButton).gameObject.BindEvent(OnClickSleepModeButton);
        
        GetButton((int)Buttons.AutoSkillButton).gameObject.BindEvent(OnClickAutoSkillButton);
        
        GetButton((int)Buttons.BossButton).gameObject.BindEvent(OnClickBossButton);
        
        GetObject((int)GameObjects.GoldDescriptionObject).SetActive(false);
        GetObject((int)GameObjects.GemDescriptionObject).SetActive(false);
        
        //unlock 이미지 숨김
        GetImage((int)Images.SkillUnlockImage).SetActive(false);
        GetImage((int)Images.DungeonUnlockImage).SetActive(false);
        GetImage((int)Images.PartyUnlockImage).SetActive(false);
        
        //SkillUnlockObject 활성화/비활성화
        GetObject((int)GameObjects.SkillLockObject).SetActive(!Managers.Game.isSkillUnlocked);
        GetObject((int)GameObjects.PartyLockObject).SetActive(!Managers.Game.isPartyUnlocked);
        GetObject((int)GameObjects.DungeonLockObject).SetActive(!Managers.Game.isDungeonUnlocked);
        
        //reddot canvas group 연결
        heroRedDot = GetObject((int)GameObjects.HeroRedDotObject).GetComponent<CanvasGroup>();
        gearRedDot = GetObject((int)GameObjects.GearRedDotObject).GetComponent<CanvasGroup>();
        skillRedDot = GetObject((int)GameObjects.SkillRedDotObject).GetComponent<CanvasGroup>();
        partyRedDot = GetObject((int)GameObjects.PartyRedDotObject).GetComponent<CanvasGroup>();
        
        GetObject((int)GameObjects.SkillSwapBlockPanel).SetActive(false);
        GetObject((int)GameObjects.ProfileRedDotObject).SetActive(Managers.Profile.HasNewlyUnlocked(Define.ProfileType.Profile) || Managers.Profile.HasNewlyUnlocked(Define.ProfileType.Frame));
        
        TogglesInit();
        GetToggle((int)Toggles.HeroToggle).isOn = true;
        OnClickHeroToggle();
        
        RefreshUI();
        RefreshSkillUI();
        UpdateAutoSkillButtonUI();
        
        return true;
    }

    public void SetInfo()
    {
        //남은 double speed 시간이 존재한다면? -> 적용
        if (Managers.Time.IsDoubleSpeed)
        {
            StartCoroutine(Managers.Time.CoDoubleSpeed(Managers.Time.GetRemainingDoubleSpeedTime()));
            StartDoubleSpeedUI();
        }
        
        RefreshUI();
    }

    public void ShowHeroToggle()
    {
        GetToggle((int)Toggles.HeroToggle).isOn = true;
        OnClickHeroToggle();
    }

    public void ShowDungeonToggle()
    {
        GetToggle((int)Toggles.DungeonToggle).isOn = true;
        OnClickDungeonToggle();
    }

    public void ShowShopToggle()
    {
        GetToggle((int)Toggles.ShopToggle).isOn = true;
        OnClickShopToggle();
    }
    
    private void RefreshUI()
    {
        //프로필 이미지
        string defaultProfileSpriteName = "emptyIcon";
        string defaultFrameSpriteName = "emptyIcon";

        string profileSpriteName = Managers.Profile.GetEquippedState(Define.ProfileType.Profile)?.data.name ??
                                   defaultProfileSpriteName;
        string frameSpriteName = Managers.Profile.GetEquippedState(Define.ProfileType.Frame)?.data.name ??
                                 defaultFrameSpriteName;
        Sprite image = Managers.Resource.Load<Sprite>(profileSpriteName + ".sprite");
        GetImage((int)Images.ProfileImage).sprite = image;
        Sprite frame = Managers.Resource.Load<Sprite>(frameSpriteName + ".sprite");
        GetImage((int)Images.FrameImage).sprite = frame;
        
        //재화, 전투력 
        GetText((int)Texts.GoldValueText).text = NumberFormatter.FormatNumber(Managers.Game.Gold);
        GetText((int)Texts.GemValueText).text = Managers.Game.Gems.ToString();
        GetText((int)Texts.CombatPowerValueText).text = $"{NumberFormatter.FormatNumber((BigInteger)CombatPowerCalculator.Get())}";
        
        //일일 퀘스트 보상 받을 거 있는지 확인하는 RedDot
        bool canDailyQuestRewarded = false;
        foreach (QuestData data in Managers.Data.DailyQuestDataDic.Values)
        {
            if (Managers.Quest.QuestStates[data.key].status == Define.QuestStatus.Completed)
            {
                canDailyQuestRewarded = true;
            }
        }
        GetObject((int)GameObjects.DailyQuestRedDotObject).SetActive(canDailyQuestRewarded);
        
        // 스테이지 정보 
        if (!Managers.Dungeon.IsDungeonActive)
        {
            GetText((int)Texts.StageText).text = $"{Managers.Level.GetCurrentMapName()} {Managers.Level.CurrentStageData.mapIdx}-{Managers.Level.CurrentStageData.stageIdx}";
            GetObject((int)GameObjects.StageSlider).GetComponent<Slider>().value = Managers.Level.CurrentStageData.waveIdx / 6f;
            GetButton((int)Buttons.BossButton).gameObject.SetActive(Managers.Level.isWaitingForBossChallenge);
        }
        
        //RedDot Object 조건 확인
        bool isGearUpgrade = false;
        bool isSkillsUpgrade = false;
        bool isPartyUpgrade = false;
        
        foreach (var gear in Managers.Inventory.GearStates.Values)
        {
            if (gear.canUpgrade)
            {
                isGearUpgrade = true;
                break;
            }
        }
        
        foreach (var skill in Managers.Inventory.SkillStates.Values)
        {
            if (skill.canUpgrade)
            {
                isSkillsUpgrade = true;
                break;
            }
        }

        foreach (var party in Managers.Inventory.PartyStates.Values)
        {
            if (party.canUpgrade)
            {
                isPartyUpgrade = true;
                break;
            }
        }
        
        //메인 퀘스트 보상 받을 거 있는지 확인
        bool canMainQuestReward = false;
        QuestData currentMainQuestData = Managers.Quest.CurrentMainQuest;
        canMainQuestReward = Managers.Quest.QuestStates[currentMainQuestData.key].status == Define.QuestStatus.Completed;
        
        heroRedDot.alpha = canMainQuestReward ? 1 : 0;
        gearRedDot.alpha = isGearUpgrade ? 1 : 0;
        skillRedDot.alpha = (isSkillsUpgrade && Managers.Game.isSkillUnlocked )? 1 : 0;
        partyRedDot.alpha = (isPartyUpgrade && Managers.Game.isPartyUnlocked) ? 1 : 0;
        
        //Toggle 잠금 확인
        GetToggle((int)Toggles.SkillToggle).interactable = Managers.Game.isSkillUnlocked;
        GetToggle((int)Toggles.PartyToggle).interactable = Managers.Game.isPartyUnlocked;
        GetToggle((int)Toggles.DungeonToggle).interactable = Managers.Game.isDungeonUnlocked;
        
        _prevGold = Managers.Game.Gold;
        _prevGem = Managers.Game.Gems;

        if (Managers.Time.TimeSinceLastReward.TotalMinutes > Define.OFFLINE_REWARD_MINIMUM_TIME)
        {
            Managers.Time.CalculateOfflineRewardItems();
            if (Managers.Time.OfflineRewardItemIds.Count > 0)
            {
                GetObject((int)GameObjects.OfflineRewardRedDotObject).SetActive(true);
            }
        }
        else
        {
            GetObject((int)GameObjects.OfflineRewardRedDotObject).SetActive(false);
        }
    }

    private void RefreshSkillUI()
    {
        //장착 중인 스킬 보여주기
        if (_refreshSkillSlotRoutine != null)
        {
            StopCoroutine(_refreshSkillSlotRoutine);
            _refreshSkillSlotRoutine = null;
        }
        _refreshSkillSlotRoutine = StartCoroutine(CoRefreshSkillSlots());
    }

    private IEnumerator CoRefreshSkillSlots()
    {
        GameObject container = GetObject((int)GameObjects.SkillGroupObject);
        container.DestroyChilds();

        yield return null;

        int index = 0;
        int currentSlots = Managers.Equipment.SkillEquipment.currentSlots;
        int maxSlots = Managers.Equipment.SkillEquipment.maxSlots;
        _nextUnlockSlot = null;
        
        //1. 장착 슬롯 표시
        foreach (SkillState skill in Managers.Equipment.SkillEquipment.EquippedSlots)
        {
            if (index >= currentSlots)
                break;

            if (skill == null)
            {
                continue;
            }

            UISkillSlotItem skillSlotItem = Managers.UI.MakeSubItem<UISkillSlotItem>(container.transform);
            skillSlotItem.SetInfo(skill, index);
            index++;
        }
        
        //2. 비어 있지만 열려 있는 슬롯 추가 (currentSlots은 5인데 장착된 스킬은 3개인 경우)
        while (index < currentSlots)
        {
            UISkillSlotItem emptySlotItem = Managers.UI.MakeSubItem<UISkillSlotItem>(container.transform);
            emptySlotItem.SetInfo(Define.SlotType.Empty, index);
            index++;
        }
        
        //3. 잠금 슬롯 추가 (currentSlots ~ maxSlots 사이)
        while (index < maxSlots)
        {
            UISkillSlotItem lockedSlotItem = Managers.UI.MakeSubItem<UISkillSlotItem>(container.transform);
            lockedSlotItem.SetInfo(Define.SlotType.Locked, index);
            
            //null이면 저장 
            if (_nextUnlockSlot == null && index == currentSlots)
                _nextUnlockSlot = lockedSlotItem;
            
            index++;
        }

        _refreshSkillSlotRoutine = null;
    }
    
    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        
        //safe area의 하단 마진만큼 Y축으로 내리기
        float safeAreaBottomPadding = Screen.height - (safeArea.y + safeArea.height);
        
        //Safe area 보정값을 localPosition에 적용
        RectTransform userInfoRect = GetObject((int)GameObjects.UserInfoGroup).GetComponent<RectTransform>();
        RectTransform stageInfoRect = GetObject((int)GameObjects.StageInfoGroup).GetComponent<RectTransform>();
        RectTransform rightMenuRect = GetObject((int)GameObjects.RightMenuGroup).GetComponent<RectTransform>();

        userInfoRect.anchoredPosition -= new Vector2(0, safeAreaBottomPadding);
        stageInfoRect.anchoredPosition -= new  Vector2(0, safeAreaBottomPadding);
        rightMenuRect.anchoredPosition -= new Vector2(0, safeAreaBottomPadding);
    }

    #region Toggles
    private void TogglesInit()
    {
        //팝업 초기화
        _heroPopupUI.gameObject.SetActive(false);
        _gearPopupUI.gameObject.SetActive(false);
        _skillPopupUI.gameObject.SetActive(false);
        _partyPopupUI.gameObject.SetActive(false);
        _dungeonPopupUI.gameObject.SetActive(false);
        _shopPopupUI.gameObject.SetActive(false);
        
        //재클릭 방지 트리거 초기화
        _isSelectedHero = false;
        _isSelectedGear = false;
        _isSelectedSkill = false;
        _isSelectedParty = false;
        _isSelectedDungeon = false;
        _isSelectedShop = false;
        
        //선택 토글 아이콘 초기화
        GetObject((int)GameObjects.CheckHeroImageObject).SetActive(false);
        GetObject((int)GameObjects.CheckGearImageObject).SetActive(false);
        GetObject((int)GameObjects.CheckSkillImageObject).SetActive(false);
        GetObject((int)GameObjects.CheckPartyImageObject).SetActive(false);
        GetObject((int)GameObjects.CheckDungeonImageObject).SetActive(false);
        GetObject((int)GameObjects.CheckShopImageObject).SetActive(false);
        
        GetObject((int)GameObjects.CheckHeroImageObject).GetComponent<RectTransform>().sizeDelta = new Vector2(200, 200);
        GetObject((int)GameObjects.CheckGearImageObject).GetComponent<RectTransform>().sizeDelta = new Vector2(200, 200);
        GetObject((int)GameObjects.CheckSkillImageObject).GetComponent<RectTransform>().sizeDelta = new Vector2(200, 200);
        GetObject((int)GameObjects.CheckPartyImageObject).GetComponent<RectTransform>().sizeDelta = new Vector2(200, 200);
        GetObject((int)GameObjects.CheckDungeonImageObject).GetComponent<RectTransform>().sizeDelta = new Vector2(200, 200);
        GetObject((int)GameObjects.CheckShopImageObject).GetComponent<RectTransform>().sizeDelta = new Vector2(200, 200);
        
        //토글 크기 초기화
        GetToggle((int)Toggles.HeroToggle).GetComponent<RectTransform>().sizeDelta = new Vector2(210, 210);
        GetToggle((int)Toggles.GearToggle).GetComponent<RectTransform>().sizeDelta = new Vector2(210, 210);
        GetToggle((int)Toggles.SkillToggle).GetComponent<RectTransform>().sizeDelta = new Vector2(210, 210);
        GetToggle((int)Toggles.PartyToggle).GetComponent<RectTransform>().sizeDelta = new Vector2(210, 210);
        GetToggle((int)Toggles.DungeonToggle).GetComponent<RectTransform>().sizeDelta = new Vector2(210, 210);
        GetToggle((int)Toggles.ShopToggle).GetComponent<RectTransform>().sizeDelta = new Vector2(210, 210);
    }
        
    private void ShowUI(GameObject contentPopup, Toggle toggle, GameObject obj, float duration = 0.1f)
    {
        TogglesInit();
        
        contentPopup.SetActive(true);
        toggle.GetComponent<RectTransform>().sizeDelta = new Vector2(210, 210);
        obj.SetActive(true);
        obj.GetComponent<RectTransform>().DOSizeDelta(new Vector2(260, 260), duration).SetEase(Ease.InOutQuad);
        
        RefreshUI();
    }

    private void OnClickHeroToggle()
    {
        if (_isSelectedHero)
            return;
        
        ShowUI(_heroPopupUI.gameObject, GetToggle((int)Toggles.HeroToggle), GetObject((int)GameObjects.CheckHeroImageObject));
        _isSelectedHero = true;
        
        _heroPopupUI.SetInfo();
    }

    private void OnClickGearToggle()
    { 
        if (_isSelectedGear)
            return;
        
        ShowUI(_gearPopupUI.gameObject, GetToggle((int)Toggles.GearToggle), GetObject((int)GameObjects.CheckGearImageObject));
        _isSelectedGear = true;
        
        _gearPopupUI.SetInfo();
    }

    private void OnClickSkillToggle()
    {
        if (_isSelectedSkill)
            return;
        
        ShowUI(_skillPopupUI.gameObject, GetToggle((int)Toggles.SkillToggle), GetObject((int)GameObjects.CheckSkillImageObject));
        _isSelectedSkill = true;
        
        _skillPopupUI.SetInfo();
    }
    
    private void OnClickPartyToggle()
    {
        if (_isSelectedParty)
            return;
        
        ShowUI(_partyPopupUI.gameObject, GetToggle((int)Toggles.PartyToggle), GetObject((int)GameObjects.CheckPartyImageObject));
        _isSelectedParty = true;
        
        _partyPopupUI.SetInfo();
    }
    
    private void OnClickDungeonToggle()
    {
        if (_isSelectedDungeon)
            return;
        
        ShowUI(_dungeonPopupUI.gameObject, GetToggle((int)Toggles.DungeonToggle), GetObject((int)GameObjects.CheckDungeonImageObject));
        _isSelectedDungeon = true;
        
        _dungeonPopupUI.SetInfo();
    }

    private void OnClickShopToggle()
    {
        if (_isSelectedShop)
            return;

        ShowUI(_shopPopupUI.gameObject, GetToggle((int)Toggles.ShopToggle), GetObject((int)GameObjects.CheckShopImageObject));
        _isSelectedShop = true;

        _shopPopupUI.SetInfo();
    }
    #endregion

    #region Buttons
    private void OnClickProfileButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.UI.ShowPopupUI<UIUserInfoPopup>().SetInfo();
    }

    private void OnClickGoldButton()
    {
        StartCoroutine(CoShowGoodsInfo(GetObject((int)GameObjects.GoldDescriptionObject)));
    }

    private void OnClickGemButton()
    {
        StartCoroutine(CoShowGoodsInfo(GetObject((int)GameObjects.GemDescriptionObject)));
    }

    private IEnumerator CoShowGoodsInfo(GameObject go)
    {
        go.SetActive(true);
        go.GetComponent<Canvas>().sortingOrder = sortOrder;
        sortOrder++;
        yield return new WaitForSecondsRealtime(2f);
        go.SetActive(false);
    }

    private void OnClickCombatPowerButton()
    {

    }

    private void OnClickDoubleSpeedButton()
    {
        Managers.Sound.PlayButtonClick();
        
        // 광고 시청 버튼 클릭 시 보상형 광고 실행
        Managers.Ad.OnClickRewardSpeedBoostAd();
    }

    public void StartDoubleSpeedUI()
    {
        Managers.Sound.PlayButtonClick();

        // 기존 코루틴 중복 방지
        if (_doubleSpeedCoroutine != null)
        {
            StopCoroutine(_doubleSpeedCoroutine);
            _doubleSpeedCoroutine = null;
        }

        GetButton((int)Buttons.DoubleSpeedButton).interactable = false;
        _doubleSpeedCoroutine = StartCoroutine(CoDoubleTimeCheck());
    }
    
    IEnumerator CoDoubleTimeCheck()
    {
        GetImage((int)Images.DoubleSpeedImage).gameObject.SetActive(false);
        
        while (true)
        {
            float remainingTime = Managers.Time.GetRemainingDoubleSpeedTime();
            if (remainingTime <= 0f)
                break;
            
            TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTime);
            string formattedTime = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
            GetText((int)Texts.DoubleSpeedTimeText).text = formattedTime;
            yield return new WaitForSecondsRealtime(1);
        }
        
        GetText((int)Texts.DoubleSpeedTimeText).text = "     2x";
        GetButton((int)Buttons.DoubleSpeedButton).interactable = true;
        GetImage((int)Images.DoubleSpeedImage).gameObject.SetActive(true);
    }
    
    private void OnClickAutoSkillButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.Game.SetAutoSkill(!Managers.Game.IsAutoSkillOn);
        UpdateAutoSkillButtonUI();
    }
    
    private void UpdateAutoSkillButtonUI()
    {
        string text = Managers.Game.IsAutoSkillOn ? "Auto\nOn" : "Auto\nOff";
        GetText((int)Texts.AutoSkillButtonText).text = text;
        GetObject((int)GameObjects.AutoSkillOnEffectImage).SetActive(Managers.Game.IsAutoSkillOn);
    }
    
    private void PlayBounceEffect(GameObject target, float scaleUp = 1.1f, float duration = 0.2f)
    {
        if (target == null) return;

        Transform t = target.transform;
        t.DOKill(); // 중복 방지
        t.localScale = Vector3.one; // 원래 크기로 초기화

        Sequence seq = DOTween.Sequence();
        seq.Append(t.DOScale(Vector3.one * scaleUp, duration * 0.3f).SetEase(Ease.OutBack));
        seq.Append(t.DOScale(Vector3.one, duration * 0.3f).SetEase(Ease.InBack));
    }

    private void OnClickAttendanceButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.UI.ShowPopupUI<UIAttendancePopup>()?.SetInfo();
    }
    
    private void OnClickOptionButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.UI.ShowPopupUI<UIOptionPopup>().SetInfo();
    }
    
    private void OnClickDailyButton()
    {
        Managers.Sound.PlayButtonClick();
        Managers.UI.ShowPopupUI<UIDailyQuestPopup>().SetInfo();
    }

    private void OnClickOfflineRewardButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.UI.ShowPopupUI<UIOfflineRewardPopup>().SetInfo();
    }

    // private void OnClickSleepModeButton()
    // {
    //     Managers.SleepMode.ToggleSleepMode(true);
    // }

    private void OnClickBossButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.Level.StartBossWave();
    }
    #endregion

    #region Unlock

    public void UnlockSkillSlot(Action onComplete)
    {
        if (_nextUnlockSlot == null)
        {
            Debug.Log("해제할 슬롯이 없습니다.");
            return;
        }
        
        Managers.Sound.Play(Define.Sound.Sfx, "unlocksound1");
        
        //슬롯 애니메이션 실행 -> 끝나면 콜백 + 슬롯 비우기
        _nextUnlockSlot.PlayUnlockEffect(() =>
        {
            _nextUnlockSlot = null;
            onComplete?.Invoke();
            RefreshSkillUI();
        });
    }
    
    public void UnlockSkillToggle(Action onComplete)
    {
        Managers.Sound.Play(Define.Sound.Sfx, "unlocksound1");
        
        //skill slot unlock인 거 보여주기
        Sequence seq = DOTween.Sequence().SetUpdate(true);
        
        //LockImage 좌우 쉐이킹 2번
        seq.Append(GetImage((int)Images.SkillLockImage).transform
            .DOShakePosition(0.2f, new Vector3(20f, 0, 0), 20, 90, false, true));
        seq.AppendInterval(0.1f);
        seq.Append(GetImage((int)Images.SkillLockImage).transform
            .DOShakePosition(0.2f, new Vector3(20f, 0, 0), 20, 90, false, true));
        
        //이미지 전환
        seq.AppendCallback(() =>
        {
            GetImage((int)Images.SkillLockImage).gameObject.SetActive(false);
            GetImage((int)Images.SkillUnlockImage).gameObject.SetActive(true);
        });
        
        //Lock Object 비활성화
        seq.AppendInterval(0.4f);
        seq.AppendCallback(() =>
        {
            GetObject((int)GameObjects.SkillLockObject).SetActive(false);
            RefreshUI();
            onComplete?.Invoke();
        });
    }

    public void UnlockPartyToggle(Action onComplete)
    {
        Managers.Sound.Play(Define.Sound.Sfx, "unlocksound1");
        
        Sequence seq = DOTween.Sequence().SetUpdate(true);
        
        //LockImage 좌우 쉐이킹 2번
        seq.Append(GetImage((int)Images.PartyLockImage).transform
            .DOShakePosition(0.2f, new Vector3(20f, 0, 0), 20, 90, false, true));
        seq.AppendInterval(0.1f);
        seq.Append(GetImage((int)Images.PartyLockImage).transform
            .DOShakePosition(0.2f, new Vector3(20f, 0, 0), 20, 90, false, true));
        
        //이미지 전환
        seq.AppendCallback(() =>
        {
            GetImage((int)Images.PartyLockImage).gameObject.SetActive(false);
            GetImage((int)Images.PartyUnlockImage).gameObject.SetActive(true);
        });
        
        //Lock Object 비활성화
        seq.AppendInterval(0.4f);
        seq.AppendCallback(() =>
        {
            GetObject((int)GameObjects.PartyLockObject).SetActive(false);
            RefreshUI();
            onComplete?.Invoke();
        });
    }

    public void UnlockDungeonToggle(Action onComplete)
    {
        Managers.Sound.Play(Define.Sound.Sfx, "unlocksound1");
        
        Sequence seq = DOTween.Sequence().SetUpdate(true);
        
        //LockImage 좌우 쉐이킹 2번
        seq.Append(GetImage((int)Images.DungeonLockImage).transform
            .DOShakePosition(0.2f, new Vector3(20f, 0, 0), 20, 90, false, true));
        seq.AppendInterval(0.1f);
        seq.Append(GetImage((int)Images.DungeonLockImage).transform
            .DOShakePosition(0.2f, new Vector3(20f, 0, 0), 20, 90, false, true));
        
        //이미지 전환
        seq.AppendCallback(() =>
        {
            GetImage((int)Images.DungeonLockImage).gameObject.SetActive(false);
            GetImage((int)Images.DungeonUnlockImage).gameObject.SetActive(true);
        });
        
        //Lock Object 비활성화
        seq.AppendInterval(0.4f);
        seq.AppendCallback(() =>
        {
            GetObject((int)GameObjects.DungeonLockObject).SetActive(false);
            RefreshUI();
            onComplete?.Invoke();
        });
    }

    #endregion
    
    # region Event Handlers

    private void TransitionEndHandler(TransitionEndEvent evnt)
    {
        RefreshUI();
    }

    private void GemChangedEventHandler(GemChangeEvent evnt)
    {
        string currentGemText = GetText((int)Texts.GemValueText).text;
        BigInteger displayedGem = NumberFormatter.Parse(currentGemText);
        BigInteger actualGem = Managers.Game.Gems;

        if (actualGem > displayedGem)
        {
            PlayBounceEffect(GetButton((int)Buttons.GemButton).gameObject);
            PlayBounceEffect(GetText((int)Texts.GemValueText).gameObject);
        }

        RefreshUI();
    }

    private void GoldChangeEventHandler(GoldChangeEvent evnt)
    {
        string currentGoldText = GetText((int)Texts.GoldValueText).text;
        BigInteger displayedGold = NumberFormatter.Parse(currentGoldText);
        BigInteger actualGold = Managers.Game.Gold;

        if (actualGold > displayedGold)
        {
            PlayBounceEffect(GetButton((int)Buttons.GoldButton).gameObject);
            PlayBounceEffect(GetText((int)Texts.GoldValueText).gameObject);
        }

        RefreshUI();
    }

    private void DungeonStateChangedEventHandler(DungeonStateChangedEvent evnt)
    {
        GetObject((int)GameObjects.StageInfoGroup).SetActive(!Managers.Dungeon.IsDungeonActive);
        RefreshUI();
    }

    private void SkillChangedEventHandler(SkillChangedEvent evnt)
    {
        RefreshSkillUI();;
    }

    private void CombatPowerChangedEventHandler(CombatPowerChangedEvent evnt)
    {
        // 1. 현재 전투력 계산
        BigInteger newCombatPower = (BigInteger)CombatPowerCalculator.Get();

        // 2. UI 텍스트에서 전투력 숫자 파싱 (예: "1.5A" → BigInteger)
        string currentText = GetText((int)Texts.CombatPowerValueText).text;
        BigInteger displayedCombatPower = NumberFormatter.Parse(currentText);

        // 3. 실제 전투력이 더 높아졌을 경우 이펙트 실행
        if (newCombatPower > displayedCombatPower)
        {
            PlayBounceEffect(GetButton((int)Buttons.CombatPowerButton).gameObject);
            PlayBounceEffect(GetText((int)Texts.CombatPowerValueText).gameObject);
        }

        // 4. UI 갱신
        RefreshUI();
    }

    private void GearStateChangedEventHandler(GearStateChangedEvent evnt)
    {
        RefreshUI();
    }

    private void SkillStateChangedEventHandler(SkillStateChangedEvent evnt)
    {
        RefreshSkillUI();;
    }

    private void PartyStateChangedEventHandler(PartyStateChangedEvent evnt)
    {
        RefreshUI();
    }

    private void ProfileChangedEventHandler(ProfileChangedEvent evnt)
    {
        RefreshUI();
    }

    private void ProfileRedDotChangedEventHandler(ProfileRedDotChangedEvent evnt)
    {
        GetObject((int)GameObjects.ProfileRedDotObject).SetActive(Managers.Profile.HasNewlyUnlocked(Define.ProfileType.Profile) || Managers.Profile.HasNewlyUnlocked(Define.ProfileType.Frame));
    }

    private void QuestStateChangedEventHandler(QuestStateChangedEvent evnt)
    {
        RefreshUI();
    }

    private void BossTransitionEventHandler(BossTransitionEvent evnt)
    {
        // 여기서 게임 UI 비활성화
        GetObject((int)GameObjects.UserInfoGroup).SetActive(false);
        GetObject((int)GameObjects.StageInfoGroup).SetActive(false);
        GetObject((int)GameObjects.RightMenuGroup).SetActive(false);
        GetObject((int)GameObjects.SkillContent).SetActive(false);
    }

    private void BossTransitionEndEventHandler(BossTransitionEndEvent evnt)
    {
        // 여기서 다시 활성화
        GetObject((int)GameObjects.UserInfoGroup).SetActive(true);
        GetObject((int)GameObjects.StageInfoGroup).SetActive(true);
        GetObject((int)GameObjects.RightMenuGroup).SetActive(true);
        GetObject((int)GameObjects.SkillContent).SetActive(true);
    }

    private void SkillSwapStartHandler(SkillSwapStartEvent evnt)
    {
        GetObject((int)GameObjects.SkillSwapBlockPanel).SetActive(true);
        _skillSwapEventAction = () =>
        {
            EventBus.Raise(new SkillSwapEndEvent(evnt.Data));
        };
        GetButton((int)Buttons.MaskedScreen).gameObject.BindEvent(_skillSwapEventAction);
    }

    private void SkillSwapEndHandler(SkillSwapEndEvent evnt)
    {
        GetButton((int)Buttons.MaskedScreen).gameObject.UnbindEvent(_skillSwapEventAction);
        GetObject((int)GameObjects.SkillSwapBlockPanel).SetActive(false);
    }
    
    #endregion
}
