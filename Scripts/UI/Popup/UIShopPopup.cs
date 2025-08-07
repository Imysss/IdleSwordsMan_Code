using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Define;

public class UIShopPopup : UIPopup
{
    #region UI Enum 정의
    enum GameObjects
    {
        SummonGroup,
        ShopGroup,
        ShopScrollContentObject,
        GearGachaExpSlider,
        SkillGachaExpSlider,
        PartyGachaExpSlider,
        SkillGachaLock,
        PartyGachaLock,
        
        AdRemovalPackageLockedImage,
        BeginnerPackageLockedImage,
        
        TipTextObject
    }

    enum Buttons
    {
        GearGachaProbabilityButton,
        SkillGachaProbabilityButton,
        PartyGachaProbabilityButton,
        GearSummon11AdButton,
        GearSummon11Button,
        GearSummon35Button,
        SkillSummon11AdButton,
        SkillSummon11Button,
        SkillSummon35Button,
        PartySummon11AdButton,
        PartySummon11Button,
        PartySummon35Button,
        
        AdRemovalPackageBuyButton,
        BeginnerPackageBuyButton,
        DungeonTicketPackageBuyButton,
    }

    enum Texts
    {
        GearGachaLevelText,
        SkillGachaLevelText,
        PartyGachaLevelText,
        GearGachaExpValueText,
        SkillGachaExpValueText,
        PartyGachaExpValueText,

        Gear11AdButtonText,
        Skill11AdButtonText,
        Party11AdButtonText,
        
        GearSummon35ButtonText,
        SkillSummon35ButtonText,
        PartySummon35ButtonText,

        GearAdCountText,
        SkillAdCountText,
        PartyAdCountText,
    }

    enum Toggles
    {
        SummonToggle,
        ShopToggle,
    }
    #endregion

    private float _adCooldownTimer = 0f;
    private GameManager _gameManager;
    private GachaManager _gachaManager;

    private void OnDestroy()
    {
        EventBus.UnSubscribe<GemChangeEvent>(GemChangedEventHandler);
        EventBus.UnSubscribe<UnlockSystemEvent>(UnlockSystemEventHandler);
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        
        _gameManager = Managers.Game;
        _gachaManager = Managers.Gacha;

        EventBus.Subscribe<GemChangeEvent>(GemChangedEventHandler);
        EventBus.Subscribe<UnlockSystemEvent>(UnlockSystemEventHandler);

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindToggle(typeof(Toggles));

        GetToggle((int)Toggles.SummonToggle).gameObject.BindEvent(OnClickSummonToggle);
        GetToggle((int)Toggles.ShopToggle).gameObject.BindEvent(OnClickShopToggle);

        GetButton((int)Buttons.GearGachaProbabilityButton).gameObject.BindEvent(OnClickGearGachaProbabilityButton);
        GetButton((int)Buttons.GearSummon11AdButton).gameObject.BindEvent(OnClickGearSummon11AdButton);
        GetButton((int)Buttons.GearSummon11Button).gameObject.BindEvent(OnClickGearSummon11Button);
        GetButton((int)Buttons.GearSummon35Button).gameObject.BindEvent(OnClickGearSummon35Button);

        GetButton((int)Buttons.SkillGachaProbabilityButton).gameObject.BindEvent(OnClickSkillGachaProbabilityButton);
        GetButton((int)Buttons.SkillSummon11AdButton).gameObject.BindEvent(OnClickSkillSummon11AdButton);
        GetButton((int)Buttons.SkillSummon11Button).gameObject.BindEvent(OnClickSkillSummon11Button);
        GetButton((int)Buttons.SkillSummon35Button).gameObject.BindEvent(OnClickSkillSummon35Button);

        GetButton((int)Buttons.PartyGachaProbabilityButton).gameObject.BindEvent(OnClickPartyGachaProbabilityButton);
        GetButton((int)Buttons.PartySummon11AdButton).gameObject.BindEvent(OnClickPartySummon11AdButton);
        GetButton((int)Buttons.PartySummon11Button).gameObject.BindEvent(OnClickPartySummon11Button);
        GetButton((int)Buttons.PartySummon35Button).gameObject.BindEvent(OnClickPartySummon35Button);
        
        GetButton((int)Buttons.AdRemovalPackageBuyButton).gameObject.BindEvent(OnClickAdRemovalPackageBuyButton);
        GetButton((int)Buttons.BeginnerPackageBuyButton).gameObject.BindEvent(OnClickBeginnerPackageBuyButton);
        GetButton((int)Buttons.DungeonTicketPackageBuyButton).gameObject.BindEvent(OnClickDungeonTicketPackageBuyButton);

        Managers.Tutorial.AddUIButton(UIButtonType.BuyGear, GetButton((int)Buttons.GearSummon35Button).gameObject);
        Managers.Tutorial.AddUIButton(UIButtonType.BuySkill, GetButton((int)Buttons.SkillSummon35Button).gameObject);
        Managers.Tutorial.AddUIButton(UIButtonType.BuyParty, GetButton((int)Buttons.PartySummon35Button).gameObject);

        return true;
    }

    public void SetInfo()
    {
        GetToggle((int)Toggles.SummonToggle).isOn = true;
        OnClickSummonToggle();
        RefreshShopUI();
    }

    private void Update()
    {
        _adCooldownTimer -= Time.unscaledDeltaTime;
        if (_adCooldownTimer <= 0f)
        {
            RefreshAdCooldownTexts();
            UpdateAdGachaRemainTexts(); // 남은 횟수 텍스트 갱신
            _adCooldownTimer = 1f;
        }
    }

    private void RefreshSummonUI()
    {
        Dictionary<GachaType, int> gachaExp = _gachaManager.GachaExp;
        Dictionary<GachaType, int> gachaLevel = _gachaManager.GachaLevel;

        GetText((int)Texts.GearGachaLevelText).text = $"Lv {gachaLevel[GachaType.Gear]}";
        if (gachaLevel[GachaType.Gear] >= Managers.Data.GachaLevelTableDataDic.Count)
        {
            GetText((int)Texts.GearGachaExpValueText).text = "MAX";
            GetObject((int)GameObjects.GearGachaExpSlider).GetComponent<Slider>().value = 1f;
        }
        else
        {
            GetText((int)Texts.GearGachaExpValueText).text =
                $"{gachaExp[GachaType.Gear]}/{Managers.Data.GachaLevelTableDataDic[gachaLevel[GachaType.Gear]].experience}";
            GetObject((int)GameObjects.GearGachaExpSlider).GetComponent<Slider>().value = (float)gachaExp[GachaType.Gear] /
                Managers.Data.GachaLevelTableDataDic[gachaLevel[GachaType.Gear]].experience;
        }

        GetText((int)Texts.SkillGachaLevelText).text = $"Lv {gachaLevel[GachaType.Skill]}";
        if (gachaLevel[GachaType.Skill] >= Managers.Data.GachaLevelTableDataDic.Count)
        {
            GetText((int)Texts.SkillGachaExpValueText).text = "MAX";
            GetObject((int)GameObjects.SkillGachaExpSlider).GetComponent<Slider>().value = 1f;
        }
        else
        {
            GetText((int)Texts.SkillGachaExpValueText).text =
                $"{gachaExp[GachaType.Skill]}/{Managers.Data.GachaLevelTableDataDic[gachaLevel[GachaType.Skill]].experience}";
            GetObject((int)GameObjects.SkillGachaExpSlider).GetComponent<Slider>().value = (float)gachaExp[GachaType.Skill] /
                Managers.Data.GachaLevelTableDataDic[gachaLevel[GachaType.Skill]].experience;
        }

        GetText((int)Texts.PartyGachaLevelText).text = $"Lv {gachaLevel[GachaType.Party]}";
        if (gachaLevel[GachaType.Party] >= Managers.Data.GachaLevelTableDataDic.Count)
        {
            GetText((int)Texts.PartyGachaExpValueText).text = "MAX";
            GetObject((int)GameObjects.PartyGachaExpSlider).GetComponent<Slider>().value = 1f;
        }
        else
        {
            GetText((int)Texts.PartyGachaExpValueText).text =
                $"{gachaExp[GachaType.Party]}/{Managers.Data.GachaLevelTableDataDic[gachaLevel[GachaType.Party]].experience}";
            GetObject((int)GameObjects.PartyGachaExpSlider).GetComponent<Slider>().value = (float)gachaExp[GachaType.Party] /
                Managers.Data.GachaLevelTableDataDic[gachaLevel[GachaType.Party]].experience;
        }

        
        bool canSummon11 = (_gameManager.Gems >= 500);
        bool canSummon35 = (_gameManager.Gems >= 1500);
        GetButton((int)Buttons.GearSummon11Button).interactable = canSummon11;
        GetButton((int)Buttons.SkillSummon11Button).interactable = canSummon11;
        GetButton((int)Buttons.PartySummon11Button).interactable = canSummon11;

        GetButton((int)Buttons.GearSummon35Button).interactable = canSummon35;
        GetButton((int)Buttons.SkillSummon35Button).interactable = canSummon35;
        GetButton((int)Buttons.PartySummon35Button).interactable = canSummon35;
        
        GetObject((int)GameObjects.SkillGachaLock).SetActive(!_gameManager.isSkillUnlocked);
        GetObject((int)GameObjects.PartyGachaLock).SetActive(!_gameManager.isPartyUnlocked);
        

        // 첫 번째 소환일 경우 무료로 설정
        GetText((int)Texts.GearSummon35ButtonText).text = _gachaManager.isFirstSummon(GachaType.Gear) ? "무료" : "1500";
        GetText((int)Texts.SkillSummon35ButtonText).text = _gachaManager.isFirstSummon(GachaType.Skill) ? "무료" : "1500";
        GetText((int)Texts.PartySummon35ButtonText).text = _gachaManager.isFirstSummon(GachaType.Party) ? "무료" : "1500";

        if (_gachaManager.isFirstSummon(GachaType.Gear))
        {
            GetButton((int)Buttons.GearSummon35Button).interactable = true;
        }

        if (_gachaManager.isFirstSummon(GachaType.Skill))
        {
            GetButton((int)Buttons.SkillSummon35Button).interactable = true;
        }

        if (_gachaManager.isFirstSummon(GachaType.Party))
        {
            GetButton((int)Buttons.PartySummon35Button).interactable = true;
        }
        

        RefreshAdCooldownTexts();
        UpdateAdGachaRemainTexts();
    }

    private void RefreshShopUI()
    {
        // 추후 상점 UI 업데이트
        //구매했다면 lock 이미지 띄우기
        GetObject((int)GameObjects.AdRemovalPackageLockedImage).SetActive(false);
        GetObject((int)GameObjects.BeginnerPackageLockedImage).SetActive(false);
    }

    private void RefreshAdCooldownTexts()
    {
        UpdateAdGachaCooldown(GachaType.Gear, Buttons.GearSummon11AdButton, Texts.Gear11AdButtonText);
        UpdateAdGachaCooldown(GachaType.Skill, Buttons.SkillSummon11AdButton, Texts.Skill11AdButtonText);
        UpdateAdGachaCooldown(GachaType.Party, Buttons.PartySummon11AdButton, Texts.Party11AdButtonText);
    }

    private void UpdateAdGachaCooldown(GachaType type, Buttons buttonEnum, Texts textEnum)
    {
        var button = GetButton((int)buttonEnum);
        var text = GetText((int)textEnum);

        if (Managers.Time.IsAdGachaAvailable(type))
        {
            button.interactable = true;
            text.text = "광고 보기";
        }
        else
        {
            button.interactable = false;
            var remain = Managers.Time.GetAdGachaCooldown(type);
            text.text = $" {remain.Minutes:D2}:{remain.Seconds:D2}";
        }
    }

    // 광고 남은 횟수 텍스트 갱신 함수
    private void UpdateAdGachaRemainTexts()
    {
        int maxCount = 3;

        int gearUsed = Managers.SaveLoad.SaveData.timeData.adGachaDailyCount.TryGetValue(GachaType.Gear, out var gUsed) ? gUsed : 0;
        int skillUsed = Managers.SaveLoad.SaveData.timeData.adGachaDailyCount.TryGetValue(GachaType.Skill, out var sUsed) ? sUsed : 0;
        int partyUsed = Managers.SaveLoad.SaveData.timeData.adGachaDailyCount.TryGetValue(GachaType.Party, out var pUsed) ? pUsed : 0;

        GetText((int)Texts.GearAdCountText).text = $"{maxCount - gearUsed}회 시청 가능";
        GetText((int)Texts.SkillAdCountText).text = $"{maxCount - skillUsed}회 시청 가능";
        GetText((int)Texts.PartyAdCountText).text = $"{maxCount - partyUsed}회 시청 가능";
    }

    private void OnClickSummonToggle()
    {
        GetObject((int)GameObjects.SummonGroup).SetActive(true);
        GetObject((int)GameObjects.ShopGroup).SetActive(false);
        GetObject((int)GameObjects.TipTextObject).SetActive(true);
        
        RefreshSummonUI();
    }

    private void OnClickShopToggle()
    {
        GetObject((int)GameObjects.SummonGroup).SetActive(false);
        GetObject((int)GameObjects.ShopGroup).SetActive(true);
        GetObject((int)GameObjects.TipTextObject).SetActive(false);
        
        RefreshShopUI();
    }

    private void OnClickGearGachaProbabilityButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.UI.ShowPopupUI<UIGachaListPopup>().SetInfo(GachaType.Gear);
    }

    private void OnClickGearSummon11AdButton()
    {
        Managers.Ad.ShowRewardedAd(RewardType.EquipmentGacha);
    }

    private void OnClickGearSummon11Button() => DoGacha(GachaType.Gear, 11);
    private void OnClickGearSummon35Button() => DoGacha(GachaType.Gear, 35);

    private void OnClickSkillGachaProbabilityButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.UI.ShowPopupUI<UIGachaListPopup>().SetInfo(GachaType.Skill);
    }

    private void OnClickSkillSummon11AdButton()
    {
        Managers.Ad.ShowRewardedAd(RewardType.SkillGacha);
    }

    private void OnClickSkillSummon11Button() => DoGacha(GachaType.Skill, 11);
    private void OnClickSkillSummon35Button() => DoGacha(GachaType.Skill, 35);

    private void OnClickPartyGachaProbabilityButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.UI.ShowPopupUI<UIGachaListPopup>().SetInfo(GachaType.Party);
    }

    private void OnClickPartySummon11AdButton()
    {
        Managers.Ad.ShowRewardedAd(RewardType.PartyGacha);
    }

    private void OnClickPartySummon11Button() => DoGacha(GachaType.Party, 11);
    private void OnClickPartySummon35Button() => DoGacha(GachaType.Party, 35);

    private void OnClickAdRemovalPackageBuyButton()
    {
        
    }

    private void OnClickBeginnerPackageBuyButton()
    {
        
    }

    private void OnClickDungeonTicketPackageBuyButton()
    {
        
    }

    private void DoGacha(GachaType type, int count = 1)
    {
        int amount = count == 11 ? 500 : count == 35 ? 1500 : 0;
        if (_gachaManager.isFirstSummon(type))  // 첫 소환은 무료!
            amount = 0;
        _gameManager.UseGem(amount);
        
        int prevExp = Managers.Gacha.GachaExp[type];
        int prevLevel = Managers.Gacha.GachaLevel[type];
        
        var result = _gachaManager.DoGacha(type, count).ToList();
        Managers.UI.ShowPopupUI<UIGachaResultPopup>().SetInfo(type, result, prevExp, prevLevel);
        RefreshSummonUI();
    }

    public void DoAdGacha(GachaType type, int count)
    {
        int prevExp = Managers.Gacha.GachaExp[type];
        int prevLevel = Managers.Gacha.GachaLevel[type];
        
        var result = _gachaManager.DoGacha(type, count).ToList();
        Managers.UI.ShowPopupUI<UIGachaResultPopup>().SetInfo(type, result, prevExp, prevLevel);
        RefreshSummonUI();
    }

    private void GemChangedEventHandler(GemChangeEvent evnt)
    {
        RefreshSummonUI();
    }

    private void UnlockSystemEventHandler(UnlockSystemEvent evnt)
    {
        RefreshSummonUI();
    }
}
