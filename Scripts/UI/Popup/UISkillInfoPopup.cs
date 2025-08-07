using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UISkillInfoPopup : UIPopup
{
    #region UI 기능 리스트
    //스킬 정보
    //SkillGradeBackgroundImage: 스킬 등급 배경
    //SkillImage: 스킬 이미지
    //SkillNameText: 스킬 이름 텍스트
    //SkillGradeText: 스킬 등급 텍스트
    //SkillLevelText: 스킬 레벨 텍스트
    //SkillLevelValueText
    //SkillExpSlider: 스킬 경험치 슬라이더
    //Fill
    //SkillExpValueText: 스킬 경헙치 값 텍스트 (0/13)
    
    //스킬 
    //SkillDescriptionText: 스킬 정보 텍스트
    //SkillCoolTimeValueText: 스킬 쿨타임 값
    //SkillPassiveValueText: 스킬 보유 스킬 값 텍스트
    
    //버튼
    //BackgroundButton
    //UnequipButton: 장착 해제 버튼
    //EquipButton: 장착 버튼
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
        SkillExpSlider,
    }

    enum Buttons
    {
        BackgroundButton,
        EquipButton,
        UnequipButton,
        UpgradeButton,
        ExitButton,
    }

    enum Texts
    {
        SkillNameText,
        SkillGradeText,
        SkillLevelText,
        SkillLevelValueText,
        SkillExpValueText,
        SkillDescriptionText,
        SkillCoolTimeValueText,
        SkillPassiveValueText,
    }

    enum Images
    {
        SkillGradeBackgroundImage,
        SkillImage,
        Fill,
    }
    #endregion

    private SkillState skillData;
    
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

    public void SetInfo(SkillState data)
    {
        //스킬 데이터 받아오기
        skillData = data;

        if (data.dataId == 5001)
        {
            Managers.Tutorial.AddUIButton(UIButtonType.EquipSkill, GetButton((int)Buttons.EquipButton).gameObject);
            Managers.Tutorial.AddUIButton(UIButtonType.SkillUpgrade, GetButton((int)Buttons.UpgradeButton).gameObject);
        }
        
        RefreshUI();
    }

    private void RefreshUI()
    {
        //스킬 등급에 따라 배경 색상 변경
        switch (skillData.data.rarity)
        {
            case RarityType.Normal:
                GetImage((int)Images.SkillGradeBackgroundImage).color = UIColors.Normal;
                GetImage((int)Images.Fill).color = UIColors.Normal;
                GetText((int)Texts.SkillGradeText).color = UIColors.Normal;
                break;
            case RarityType.Rare:
                GetImage((int)Images.SkillGradeBackgroundImage).color = UIColors.Rare;
                GetImage((int)Images.Fill).color = UIColors.Rare;
                GetText((int)Texts.SkillGradeText).color = UIColors.Rare;
                break;
            case RarityType.Epic:
                GetImage((int)Images.SkillGradeBackgroundImage).color = UIColors.Epic;
                GetImage((int)Images.Fill).color = UIColors.Epic;
                GetText((int)Texts.SkillGradeText).color = UIColors.Epic;
                break;
            case RarityType.Unique:
                GetImage((int)Images.SkillGradeBackgroundImage).color = UIColors.Unique;
                GetImage((int)Images.Fill).color = UIColors.Unique;
                GetText((int)Texts.SkillGradeText).color = UIColors.Unique;
                break;
            case RarityType.Legendary:
                GetImage((int)Images.SkillGradeBackgroundImage).color = UIColors.Legendary;
                GetImage((int)Images.Fill).color = UIColors.Legendary;
                GetText((int)Texts.SkillGradeText).color = UIColors.Legendary;
                break;
        }
        
        //현재 스킬의 이미지 적용
        Sprite spr = Managers.Resource.Load<Sprite>(skillData.dataId + ".sprite");
        GetImage((int)Images.SkillImage).sprite = spr;
        
        //스킬 이름, 등급, 레벨, 경험치, 스킬 정보 설정
        GetText((int)Texts.SkillNameText).text = skillData.data.name;
        GetText((int)Texts.SkillGradeText).text = Util.GetGradeStringEngToKor(skillData.data.rarity);
        string level = (Managers.Data.MergeCostDataDic.Count == skillData.level ? "MAX" : skillData.level.ToString());
        GetText((int)Texts.SkillLevelText).text = $"Lv {level}";
        GetObject((int)GameObjects.SkillExpSlider).GetComponent<Slider>().value = (float)skillData.experience /  Managers.Data.MergeCostDataDic[skillData.level].mergeCost;
        GetText((int)Texts.SkillExpValueText).text = $"{skillData.experience}/{Managers.Data.MergeCostDataDic[skillData.level].mergeCost}";
        GetText((int)Texts.SkillDescriptionText).text = skillData.data.description;
        
        //스킬 쿨타임
        GetText((int)Texts.SkillCoolTimeValueText).text = $"{skillData.data.cooldown}s";

        //스킬 보유 스킬
        GetText((int)Texts.SkillPassiveValueText).text = $"+{NumberFormatter.FormatNumber(Managers.Data.OwnedEffectLevelDataDic[skillData.level].Effects[skillData.data.rarity])}%";
        
        //스킬 이미지
        GetObject((int)GameObjects.RedDotObject).SetActive(skillData.canUpgrade);
        GetObject((int)GameObjects.EquippedObject).SetActive(Managers.Equipment.IsEquipped(skillData.dataId));
        GetObject((int)GameObjects.LockedObject).SetActive(!skillData.isUnlocked);
        GetText((int)Texts.SkillLevelValueText).gameObject.SetActive(skillData.isUnlocked);
        GetText((int)Texts.SkillLevelValueText).text = $"Lv {level}";

        //버튼 활성화
        GetButton((int)Buttons.EquipButton).interactable = (!Managers.Equipment.IsEquipped(skillData.dataId) && skillData.isUnlocked);
        GetButton((int)Buttons.UnequipButton).gameObject.SetActive(Managers.Equipment.IsEquipped(skillData.dataId) && skillData.isUnlocked);
        GetButton((int)Buttons.UpgradeButton).interactable = (skillData.canUpgrade);
    }

    private void OnClickEquipButton()
    {
        Managers.Sound.Play(Sound.Sfx, "EquipSound");
        
        int slotIdx = Managers.Equipment.Equip(skillData);
        if (slotIdx == -1)
        {
            EventBus.Raise(new SkillSwapStartEvent(skillData));
            //Managers.UI.ShowToast("장착할 슬롯이 부족합니다.");
        }
        else
        {
            RefreshUI();
            EventBus.Raise(new SkillChangedEvent());
        }
        
        PopupSlideOut(GetObject((int)GameObjects.ContentObject), -870f, 0.25f, () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }

    private void OnClickUnequipButton()
    {
        Managers.Sound.Play(Sound.Sfx, "EquipSound");
        
        Managers.Equipment.Unequip(skillData);
        
        RefreshUI();
        EventBus.Raise(new SkillChangedEvent());
        
        PopupSlideOut(GetObject((int)GameObjects.ContentObject), -870f, 0.25f, () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }

    private void OnClickUpgradeButton()
    {
        int itemId = skillData.dataId;
        int oldLevel = Managers.Inventory.SkillStates[itemId].level;
        
        //업그레이드 실행
        Managers.Inventory.UpgradeItem(itemId);
        
        int newLevel = Managers.Inventory.SkillStates[itemId].level;
        
        //업그레이드 결과 팝업 표시
        if (newLevel > oldLevel)
        {
            var result = new UpgradeResult(itemId, ItemType.Skill, oldLevel, newLevel);
            Managers.UI.ShowPopupUI<UIUpgradeResultPopup>().SetInfo(new List<UpgradeResult> { result });
        }
        
        RefreshUI();
        EventBus.Raise(new SkillChangedEvent());
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
