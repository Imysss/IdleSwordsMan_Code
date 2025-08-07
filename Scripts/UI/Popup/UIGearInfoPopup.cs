using System;
using System.Collections.Generic;
using System.Numerics;
using Data;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using Vector2 = UnityEngine.Vector2;

public class UIGearInfoPopup : UIPopup
{
    #region UI 기능 리스트
    //BackgroundButton
    //GearGradeBackgroundImage: 현재 장비의 배경 (등급에 따라 색상 변경)
    //GearImage: 현재 장비의 아이콘
    //GearNameText: 현재 장비의 이름 텍스트
    //GearLevelText: 현재 장비의 레벨 텍스트
    //GearGradeText: 현재 장비의 등급 텍스트
    //RedDotObject
    //EquippedObject
    //LockedObject
    //GearExpSlider: 현재 장비의 경험치 슬라이더
    //Fill
    //GearLevelValueText
    //GearExpValueText: 현재 장비의 경험치 상태 텍스트 (0/13)
    //GearPassiveValueText: 장비의 보유 효과 값 텍스트
    //GearActiveValueText: 장비의 장착 효과 값 텍스트
    //EquipButton: 장착 버튼 
    //UnequipButton: 장착 해제 버튼
    //UpgradeButton: 업그레이드 버튼
    //ExitButton: 현재 팝업 닫기
    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        RedDotObject,
        EquippedObject,
        LockedObject,
        GearExpSlider,
    }
    
    enum Buttons
    {
        BackgroundButton,
        EquipButton,
        //UnequipButton,
        UpgradeButton,
        ExitButton,
    }

    enum Texts
    {
        GearNameText,
        GearLevelText,
        GearGradeText,
        GearLevelValueText,
        GearExpValueText,
        GearPassiveValueText,
        GearActiveValueText,
    }

    enum Images
    {
        GearGradeBackgroundImage,
        GearImage,
        Fill,
    }
    #endregion

    private GearState gearData;

    private void OnEnable()
    {
        PopupSlideIn(GetObject((int)GameObjects.ContentObject));
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        
        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickBackgroundButton);
        GetButton((int)Buttons.EquipButton).gameObject.BindEvent(OnClickEquipButton);
        //GetButton((int)Buttons.UnequipButton).gameObject.BindEvent(OnClickUnequipButton);
        GetButton((int)Buttons.UpgradeButton).gameObject.BindEvent(OnClickUpgradeButton);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);
        
        return true;
    }

    public void SetInfo(GearState data)
    {
        //현재 선택한 장비 데이터 받아오기
        gearData = data;
        
        if (gearData.data.type == GearType.Weapon)
        {
            SetImageSize(179, 358);
            GetImage((int)Images.GearImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 28);
        }
        else if (gearData.data.type == GearType.Hat)
        {
            SetImageSize(320, 320);
            GetImage((int)Images.GearImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -48);
        }
        else if (gearData.data.type == GearType.Armor)
        {
            SetImageSize(220, 220);
            GetImage((int)Images.GearImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        }
        else
        {
            SetImageSize(260, 260);
            GetImage((int)Images.GearImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        }

        if (data.dataId == 4001)
        {
            Managers.Tutorial.AddUIButton(UIButtonType.EquipGear, GetButton((int)Buttons.EquipButton).gameObject);
            Managers.Tutorial.AddUIButton(UIButtonType.GearUpgrade, GetButton((int)Buttons.UpgradeButton).gameObject);
        }
        
        RefreshUI();
    }
    
    private void RefreshUI()
    {
        GetText((int)Texts.GearNameText).text = gearData.data.name;
        
        //등급에 따라 색상 변경
        switch (gearData.data.rarity)
        {
            case RarityType.Normal:
                GetImage((int)Images.GearGradeBackgroundImage).color = UIColors.Normal;
                GetImage((int)Images.Fill).color = UIColors.Normal;
                GetText((int)Texts.GearGradeText).color = UIColors.Normal;
                break;
            case RarityType.Rare:
                GetImage((int)Images.GearGradeBackgroundImage).color = UIColors.Rare;
                GetImage((int)Images.Fill).color = UIColors.Rare;
                GetText((int)Texts.GearGradeText).color = UIColors.Rare;
                break;
            case RarityType.Epic:
                GetImage((int)Images.GearGradeBackgroundImage).color = UIColors.Epic;
                GetImage((int)Images.Fill).color = UIColors.Epic;
                GetText((int)Texts.GearGradeText).color = UIColors.Epic;
                break;
            case RarityType.Unique:
                GetImage((int)Images.GearGradeBackgroundImage).color = UIColors.Unique;
                GetImage((int)Images.Fill).color = UIColors.Unique;
                GetText((int)Texts.GearGradeText).color = UIColors.Unique;
                break;
            case RarityType.Legendary:
                GetImage((int)Images.GearGradeBackgroundImage).color = UIColors.Legendary;
                GetImage((int)Images.Fill).color = UIColors.Legendary;
                GetText((int)Texts.GearGradeText).color = UIColors.Legendary;
                break;
        }

        //현재 장비의 이미지와 타입 아이콘 적용
        Sprite spr = Managers.Resource.Load<Sprite>(gearData.dataId + ".sprite");
        GetImage((int)Images.GearImage).sprite = spr;
             
        //현재 장비의 이름, 레벨, 경험치, 장착/보유 효과
        GetText((int)Texts.GearNameText).text = gearData.data.name;
        GetText((int)Texts.GearGradeText).text = Util.GetGradeStringEngToKor(gearData.data.rarity);
        string level = (Managers.Data.MergeCostDataDic.Count == gearData.level ? "MAX" : gearData.level.ToString());
        GetText((int)Texts.GearLevelText).text = $"Lv {level}";
        GetText((int)Texts.GearExpValueText).text = $"{gearData.experience}/{Managers.Data.MergeCostDataDic[gearData.level].mergeCost}";
        GetObject((int)GameObjects.GearExpSlider).GetComponent<Slider>().value = (float)gearData.experience / Managers.Data.MergeCostDataDic[gearData.level].mergeCost;
             
        GearLevelData selectedGearLevelData = Managers.Data.GearLevelDataDic[gearData.dataId];
             
        //현재 선택한 장비의 보유 효과/장착 효과
        double active = selectedGearLevelData.baseValue * (1 + 0.14f * (gearData.level - 1));
        GetText((int)Texts.GearPassiveValueText).text = $"공격력 +{NumberFormatter.FormatNumber(Managers.Data.OwnedEffectLevelDataDic[gearData.level].Effects[gearData.data.rarity])}%";
        GetText((int)Texts.GearActiveValueText).text = $"{GetStatType(selectedGearLevelData.statType)} +{NumberFormatter.FormatNumber(active)}%";
        
        //장비 이미지
        GetObject((int)GameObjects.RedDotObject).SetActive(gearData.canUpgrade);
        GetObject((int)GameObjects.EquippedObject).SetActive(Managers.Equipment.IsEquipped(gearData.dataId));
        GetObject((int)GameObjects.LockedObject).SetActive(!gearData.isUnlocked);
        GetText((int)Texts.GearLevelValueText).gameObject.SetActive(gearData.isUnlocked);
        GetText((int)Texts.GearLevelValueText).text = $"Lv {level}";
             
        //버튼 활성화
        GetButton((int)Buttons.EquipButton).interactable = (!Managers.Equipment.IsEquipped(gearData.dataId) && gearData.isUnlocked);
        //GetButton((int)Buttons.UnequipButton).gameObject.SetActive(Managers.Equipment.IsEquipped(gearData.dataId) && gearData.isUnlocked);
        GetButton((int)Buttons.UpgradeButton).interactable = (gearData.canUpgrade);
    }

    private string GetStatType(StatType type)
    {
        switch (type)
        {
            case StatType.AttackPower:
                return "공격력";
            case StatType.HpRecovery:
                return "체력 회복";
            case StatType.MaxHp:
                return "체력";
            case StatType.CriticalDamage:
                return "크리티컬 데미지";
            case StatType.AttackSpeed:
                return "공격 속도";
            default:
                return "";
        }
    }
    
    private void SetImageSize(float width, float height)
    {
        RectTransform rect = GetImage((int)Images.GearImage).GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);
    }

    private void OnClickEquipButton()
    {
        Managers.Sound.Play(Sound.Sfx, "EquipSound");
        
        //현재 선택 중인 장비 장착
        Managers.Equipment.Equip(gearData);
        RefreshUI();
        EventBus.Raise(new GearChangedEvent());
        Managers.Equipment.RecalculateAllEquippedEffects();
        
        PopupSlideOut(GetObject((int)GameObjects.ContentObject), -870f, 0.25f, () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }

    // private void OnClickUnequipButton()
    // {
    //     Managers.Equipment.Unequip(gearData);
    //     RefreshUI();
    //     EventBus.Raise(new GearChangedEvent());
    // }

    private void OnClickUpgradeButton()
    {
        int itemId = gearData.dataId;
        int oldLevel = Managers.Inventory.GearStates[itemId].level;
        
        //업그레이드 실행
        Managers.Inventory.UpgradeItem(gearData.dataId);
        
        int newLevel = Managers.Inventory.GearStates[itemId].level;
        
        //업그레이드 결과 팝업 표시 
        if (newLevel > oldLevel)
        {
            var result = new UpgradeResult(itemId, ItemType.Gear, oldLevel, newLevel);
            Managers.UI.ShowPopupUI<UIUpgradeResultPopup>().SetInfo(new List<UpgradeResult> { result });
        }
        
        RefreshUI();
        EventBus.Raise(new GearChangedEvent());
        Managers.Equipment.RecalculateAllEquippedEffects();
    }

    private void OnClickBackgroundButton()
    {
        Managers.Sound.PlayButtonClick();
        
        PopupSlideOut(GetObject((int)GameObjects.ContentObject), -870f, 0.25f, () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }
    
    private void OnClickExitButton()
    {
        Managers.Sound.PlayButtonClick();
        
        PopupSlideOut(GetObject((int)GameObjects.ContentObject), -870f, 0.25f, () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }
}
