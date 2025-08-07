using System;
using System.Collections.Generic;
using System.Numerics;
using Data;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIPartyInfoPopup : UIPopup
{
    #region UI 기능 리스트
    //동료 정보
    //PartyGradeBackgroundImage: 동료 등급 배경
    //PartyImage: 동료 이미지
    //PartyNameText: 동료 이름 텍스트
    //PartyGradeText: 동료 등급 텍스트
    //PartyLevelText: 동료 레벨 텍스트
    //PartyLevelValueText
    //PartyExpSlider: 동료 경험치 슬라이더
    //PartyExpValueText: 동료 경험치 값 텍스트 (0/13)
    //Fill
    
    //스킬
    //PartyActive1ValueText
    //PartyActive2ValueText
    //PartyPassiveValueText
    
    //버튼
    //BackgroundButton
    //EquipButton: 장착 버튼
    //UnequipButton: 장착 해제 버튼
    //UpgradeButton: 업그레이드 버튼
    //ExitButton: 팝업 닫기 버튼
    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        RedDotObject,
        EquippedObject,
        LockedObject,
        PartyExpSlider,
    }

    enum Buttons
    {
        BackgroundButton,
        UnequipButton,
        EquipButton,
        UpgradeButton,
        ExitButton,
    }

    enum Texts
    {
        PartyNameText,
        PartyGradeText,
        PartyLevelText,
        PartyLevelValueText,
        PartyExpValueText,
        PartyActive1ValueText,
        PartyActive2ValueText,
        PartyPassiveValueText,
    }

    enum Images
    {
        PartyGradeBackgroundImage,
        PartyImage,
        Fill,
    }
    #endregion

    private PartyState partyData;

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
        GetButton((int)Buttons.UnequipButton).gameObject.BindEvent(OnClickUnequipButton);
        GetButton((int)Buttons.UpgradeButton).gameObject.BindEvent(OnClickUpgradeButton);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);
        
        return true;
    }

    public void SetInfo(PartyState data)
    {
        //동료 데이터 받아오기
        partyData = data;

        if (data.dataId == 6001)
        {
            Managers.Tutorial.AddUIButton(UIButtonType.EquipParty, GetButton((int)Buttons.EquipButton).gameObject);
            Managers.Tutorial.AddUIButton(UIButtonType.PartyUpgrade, GetButton((int)Buttons.UpgradeButton).gameObject);
        }
        
        RefreshUI();
    }

    private void RefreshUI()
    {
         //등급에 따라 배경 색상 변경
        switch (partyData.data.rarity)
        {
            case RarityType.Normal:
                GetImage((int)Images.PartyGradeBackgroundImage).color = UIColors.Normal;
                GetImage((int)Images.Fill).color = UIColors.Normal;
                GetText((int)Texts.PartyGradeText).color = UIColors.Normal;
                break;
            case RarityType.Rare:
                GetImage((int)Images.PartyGradeBackgroundImage).color = UIColors.Rare;
                GetImage((int)Images.Fill).color = UIColors.Rare;
                GetText((int)Texts.PartyGradeText).color = UIColors.Rare;
                break;
            case RarityType.Epic:
                GetImage((int)Images.PartyGradeBackgroundImage).color = UIColors.Epic;
                GetImage((int)Images.Fill).color = UIColors.Epic;
                GetText((int)Texts.PartyGradeText).color = UIColors.Epic;
                break;
            case RarityType.Unique:
                GetImage((int)Images.PartyGradeBackgroundImage).color = UIColors.Unique;
                GetImage((int)Images.Fill).color = UIColors.Unique;
                GetText((int)Texts.PartyGradeText).color = UIColors.Unique;
                break;
            case RarityType.Legendary:
                GetImage((int)Images.PartyGradeBackgroundImage).color = UIColors.Legendary;
                GetImage((int)Images.Fill).color = UIColors.Legendary;
                GetText((int)Texts.PartyGradeText).color = UIColors.Legendary;
                break;
        }
        
        //현재 동료의 이미지 적용
        Sprite spr = Managers.Resource.Load<Sprite>(partyData.dataId + ".sprite");
        GetImage((int)Images.PartyImage).sprite = spr;
        
        //동료 이름, 등급, 레벨, 경험치 설정
        GetText((int)Texts.PartyNameText).text = partyData.data.name;
        GetText((int)Texts.PartyGradeText).text = Util.GetGradeStringEngToKor(partyData.data.rarity);
        string level = (Managers.Data.MergeCostDataDic.Count == partyData.level ? "MAX" : partyData.level.ToString());
        GetText((int)Texts.PartyLevelText).text = $"Lv {level}";
        GetObject((int)GameObjects.PartyExpSlider).GetComponent<Slider>().value = (float)partyData.experience / Managers.Data.MergeCostDataDic[partyData.level].mergeCost;
        GetText((int)Texts.PartyExpValueText).text = $"{partyData.experience}/{Managers.Data.MergeCostDataDic[partyData.level].mergeCost}";
        
        //동료 장착 스킬 설정
        double activeAtk = Managers.Data.PartyLevelDataDic[partyData.dataId].baseValue * (1 + 0.14f * (partyData.level - 1));
        activeAtk = (double)Managers.Player.PlayerStat.GetBigIntValue(StatType.AttackPower) * activeAtk;
        GetText((int)Texts.PartyActive1ValueText).text = $"{NumberFormatter.FormatNumber((BigInteger)activeAtk)}";
        GetText((int)Texts.PartyActive2ValueText).text = $"{partyData.data.attackSpeed}";
        
        //동료 보유 스킬 설정
        GetText((int)Texts.PartyPassiveValueText).text = $"+{NumberFormatter.FormatNumber((float)Managers.Data.OwnedEffectLevelDataDic[partyData.level].Effects[partyData.data.rarity])}%";
        
        //동료 이미지
        GetObject((int)GameObjects.RedDotObject).SetActive(partyData.canUpgrade);
        GetObject((int)GameObjects.EquippedObject).SetActive(Managers.Equipment.IsEquipped(partyData.dataId));
        GetObject((int)GameObjects.LockedObject).SetActive(!partyData.isUnlocked);
        GetText((int)Texts.PartyLevelValueText).gameObject.SetActive(partyData.isUnlocked);
        GetText((int)Texts.PartyLevelValueText).text = $"Lv {level}";
        
        //버튼 활성화
        GetButton((int)Buttons.EquipButton).interactable = (!Managers.Equipment.IsEquipped(partyData.dataId) && partyData.isUnlocked);
        GetButton((int)Buttons.UnequipButton).gameObject.SetActive(Managers.Equipment.IsEquipped(partyData.dataId) && partyData.isUnlocked);
        GetButton((int)Buttons.UpgradeButton).interactable = (partyData.canUpgrade);
    }
    
    private void OnClickEquipButton()
    {
        Managers.Sound.Play(Sound.Sfx, "EquipSound");
        
        //현재 동료 장착
        int slotIdx = Managers.Equipment.Equip(partyData);
        if (slotIdx == -1)
        {
            EventBus.Raise(new PartySwapStartEvent(partyData));
            //Managers.UI.ShowToast("장착할 슬롯이 부족합니다.");
        }
        else
        {
            RefreshUI();
            EventBus.Raise(new PartyChangedEvent());
        }

        
        PopupSlideOut(GetObject((int)GameObjects.ContentObject), -870f, 0.25f, () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }

    private void OnClickUnequipButton()
    {
        Managers.Sound.Play(Sound.Sfx, "EquipSound");
        
        //현재 동료 장착 해제
        Managers.Equipment.Unequip(partyData);
        RefreshUI();
        EventBus.Raise(new PartyChangedEvent());
        
        PopupSlideOut(GetObject((int)GameObjects.ContentObject), -870f, 0.25f, () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }

    private void OnClickUpgradeButton()
    {
        int itemId = partyData.dataId;
        int oldLevel = Managers.Inventory.PartyStates[itemId].level;
        
        //업그레이드 실행
        Managers.Inventory.UpgradeItem(itemId);
        
        int newLevel = Managers.Inventory.PartyStates[itemId].level;
        
        //업그레이드 결과 팝업 표시
        if (newLevel > oldLevel)
        {
            var result = new UpgradeResult(itemId, Define.ItemType.Party, oldLevel, newLevel);
            Managers.UI.ShowPopupUI<UIUpgradeResultPopup>().SetInfo(new List<UpgradeResult> { result });
        }

        RefreshUI();
        EventBus.Raise(new PartyChangedEvent());
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
