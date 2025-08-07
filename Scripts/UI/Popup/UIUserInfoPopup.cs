using System;
using System.Collections;
using System.Numerics;
using Data;
using UnityEngine;
using static Define;

public class UIUserInfoPopup : UIPopup
{
    #region UI 기능 리스트
    //ProfileBackground: 프로필 변경 버튼
    //ProfileImage
    //ProfileFrameImage
    
    //UserName: 이름 변경 버튼
    //UserNameText
    //StageInfoText
    
    //GearPassiveEffectValueText
    //GearSlot[Weapon]
    //GearImage[Weapon]
    //GearSlot[Hat]
    //GearImage[Hat]
    //GearSlot[Armor]
    //GearImage[Armor]
    //GearSlot[Gloves]
    //GearImage[Gloves]
    //GearSlot[Shoes]
    //GearImage[Shoes]
    
    //WeaponEquipEffect: 무기 장착 효과
    //WeaponEquipValueText: 무기 장착 효과 텍스트
    //HatEquipEffect
    //HatEquipValueText
    //ArmorEquipEffect
    //ArmorEquipValueText
    //GlovesEquipEffect
    //GlovesEquipValueText
    //ShoesEquipEffect
    //ShoesEquipValueText
    
    //SkillPassiveEffectValueText
    //SkillPreset: 스킬 프리셋 콘테이너
    
    //PartyPassiveEffectValueText
    //PartyPreset: 파티 프리셋 콘테이너
    
    //CombatPowerValueText
    
    //ExitButton
    #endregion

    #region Enum

    enum GameObjects
    {
        ContentObject,
        SkillPreset,
        PartyPreset,
        
        WeaponEquipEffect,
        HatEquipEffect,
        ArmorEquipEffect,
        GlovesEquipEffect,
        ShoesEquipEffect,
        
        ProfileRedDotObject,
    }
    enum Buttons
    {
        ProfileBackground,
        UserName,
        ExitButton,
    }

    enum Texts
    {
        UserNameText,
        StageInfoText,
        GearPassiveEffectValueText,
        SkillPassiveEffectValueText,
        PartyPassiveEffectValueText,
        CombatPowerValueText,
        
        WeaponEquipValueText,
        HatEquipValueText,
        ArmorEquipValueText,
        GlovesEquipValueText,
        ShoesEquipValueText,
    }

    enum Images
    {
        ProfileImage,
        ProfileFrameImage,
    
        GearSlotWeapon,
        GearImageWeapon,
        GearSlotHat,
        GearImageHat,
        GearSlotArmor,
        GearImageArmor,
        GearSlotGloves,
        GearImageGloves,
        GearSlotShoes,
        GearImageShoes,
    }
    #endregion
    
    private GearState weapon;
    private GearState hat;
    private GearState armor;
    private GearState gloves;
    private GearState shoes;
    private int sortOrder = 100;

    private void OnEnable()
    {
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));

        sortOrder = 100;
        
        EventBus.Subscribe<NameChangedEvent>(NameChangedEventHandler);
        EventBus.Subscribe<ProfileChangedEvent>(ProfileChangedEventHandler);
        EventBus.Subscribe<ProfileRedDotChangedEvent>(ProfileRedDotChangedEventHandler);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<NameChangedEvent>(NameChangedEventHandler);
        EventBus.UnSubscribe<ProfileChangedEvent>(ProfileChangedEventHandler);
        EventBus.UnSubscribe<ProfileRedDotChangedEvent>(ProfileRedDotChangedEventHandler);
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        
        GetButton((int)Buttons.ProfileBackground).gameObject.BindEvent(OnClickProfileButton);
        GetButton((int)Buttons.UserName).gameObject.BindEvent(OnClickUserNameButton);
        
        GetImage((int)Images.GearSlotWeapon).gameObject.BindEvent(OnClickSlotWeapon);
        GetImage((int)Images.GearSlotHat).gameObject.BindEvent(OnClickSlotHat);
        GetImage((int)Images.GearSlotArmor).gameObject.BindEvent(OnClickSlotArmor);
        GetImage((int)Images.GearSlotGloves).gameObject.BindEvent(OnClickSlotGloves);
        GetImage((int)Images.GearSlotShoes).gameObject.BindEvent(OnClickSlotShoes);
        
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);
        
        //정보 끄기
        GetObject((int)GameObjects.WeaponEquipEffect).gameObject.SetActive(false);
        GetObject((int)GameObjects.HatEquipEffect).gameObject.SetActive(false);
        GetObject((int)GameObjects.ArmorEquipEffect).gameObject.SetActive(false);
        GetObject((int)GameObjects.GlovesEquipEffect).gameObject.SetActive(false);
        GetObject((int)GameObjects.ShoesEquipEffect).gameObject.SetActive(false);
        
        GetObject((int)GameObjects.ProfileRedDotObject).SetActive(Managers.Profile.HasNewlyUnlocked(ProfileType.Profile) || Managers.Profile.HasNewlyUnlocked(ProfileType.Frame));

        return true;
    }

    public void SetInfo()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        //프로필 이미지 설정
        //프로필 이미지
        string defaultProfileSpriteName = "emptyIcon";
        string defaultFrameSpriteName = "emptyIcon";

        string profileSpriteName = Managers.Profile.GetEquippedState(ProfileType.Profile)?.data.name ??
                                   defaultProfileSpriteName;
        string frameSpriteName = Managers.Profile.GetEquippedState(ProfileType.Frame)?.data.name ??
                                 defaultFrameSpriteName;
        Sprite image = Managers.Resource.Load<Sprite>(profileSpriteName + ".sprite");
        GetImage((int)Images.ProfileImage).sprite = image;
        Sprite frame = Managers.Resource.Load<Sprite>(frameSpriteName + ".sprite");
        GetImage((int)Images.ProfileFrameImage).sprite = frame;

        //이름 설정
        GetText((int)Texts.UserNameText).text = Managers.Game.userName;

        //최고 스테이지 설정
        StageData stageData = Managers.Level.CurrentStageData;
        GetText((int)Texts.StageInfoText).text = $"{Managers.Level.GetCurrentMapName()} {Managers.Level.CurrentStageData.mapIdx}-{Managers.Level.CurrentStageData.stageIdx}";

        //장비
        GetText((int)Texts.GearPassiveEffectValueText).text = $"보유 효과: 공격력 +{NumberFormatter.FormatNumber((float)Managers.Inventory.TotalGearOwnedEffect)}%";

        //장착 중인 장비
        foreach (var gear in Managers.Equipment.GearEquipment.EquippedSlots)
        {
            if (gear == null)
                continue;
            Sprite spr = Managers.Resource.Load<Sprite>(gear.dataId + ".sprite");
            switch (gear.data.type)
            {
                case GearType.Weapon:
                    weapon = gear;
                    GetImage((int)Images.GearSlotWeapon).color = Util.GetBackgroundColor(gear.data.rarity);
                    GetImage((int)Images.GearImageWeapon).sprite = spr;
                    break;
                case GearType.Hat:
                    hat = gear;
                    GetImage((int)Images.GearSlotHat).color = Util.GetBackgroundColor(gear.data.rarity);
                    GetImage((int)Images.GearImageHat).sprite = spr;
                    break;
                case GearType.Armor:
                    armor = gear;
                    GetImage((int)Images.GearSlotArmor).color = Util.GetBackgroundColor(gear.data.rarity);
                    GetImage((int)Images.GearImageArmor).sprite = spr;
                    break;
                case GearType.Gloves:
                    gloves = gear;
                    GetImage((int)Images.GearSlotGloves).color = Util.GetBackgroundColor(gear.data.rarity);
                    GetImage((int)Images.GearImageGloves).sprite = spr;
                    break;
                case GearType.Shoes:
                    shoes = gear;
                    GetImage((int)Images.GearSlotShoes).color = Util.GetBackgroundColor(gear.data.rarity);
                    GetImage((int)Images.GearImageShoes).sprite = spr;
                    break;
            }
        }

        //스킬
        GetText((int)Texts.SkillPassiveEffectValueText).text = $"보유 효과: 공격력 +{NumberFormatter.FormatNumber((float)Managers.Inventory.TotalSkillOwnedEffect)}%";
        GameObject skillContainer = GetObject((int)GameObjects.SkillPreset);
        skillContainer.DestroyChilds();
        for (int i = 0; i < Managers.Equipment.SkillEquipment.currentSlots; i++)
        {
            if (i < Managers.Equipment.SkillEquipment.EquippedSlots.Count)
            {
                SkillState skill = Managers.Equipment.SkillEquipment.EquippedSlots[i];
                
                if (skill != null)
                {
                    Managers.UI.MakeSubItem<UIPresetSlot>(skillContainer.transform).SetInfo(skill);
                }
                else
                {
                    Managers.UI.MakeSubItem<UIPresetSlot>(skillContainer.transform).SetInfo();
                }
            }
            else
            {
                Managers.UI.MakeSubItem<UIPresetSlot>(skillContainer.transform).SetInfo();
            }
        }

        for (int i = 0; i < Managers.Equipment.SkillEquipment.maxSlots - Managers.Equipment.SkillEquipment.currentSlots; i++)
        {
            Managers.UI.MakeSubItem<UIPresetSlot>(skillContainer.transform).SetInfo("lockIcon1");
        }
        
        //동료 
        GetText((int)Texts.PartyPassiveEffectValueText).text = $"보유 효과: 공격력 +{NumberFormatter.FormatNumber((float)Managers.Inventory.TotalPartyOwnedEffect)}%";
        GameObject partyContainer = GetObject((int)GameObjects.PartyPreset);
        partyContainer.DestroyChilds();
        for (int i = 0; i < Managers.Equipment.PartyEquipment.currentSlots; i++)
        {
            if (i < Managers.Equipment.PartyEquipment.EquippedSlots.Count)
            {
                var party = Managers.Equipment.PartyEquipment.EquippedSlots[i];
                if (party != null)
                {
                    Managers.UI.MakeSubItem<UIPresetSlot>(partyContainer.transform).SetInfo(party);
                }
                else
                {
                    Managers.UI.MakeSubItem<UIPresetSlot>(partyContainer.transform).SetInfo();
                }
            }
            else
            {
                Managers.UI.MakeSubItem<UIPresetSlot>(partyContainer.transform).SetInfo();
            }
        }
        for (int i = 0; i < Managers.Equipment.PartyEquipment.maxSlots - Managers.Equipment.PartyEquipment.currentSlots; i++)
        {
            Managers.UI.MakeSubItem<UIPresetSlot>(partyContainer.transform).SetInfo("lockIcon1");
        }
        
        //전투력
        GetText((int)Texts.CombatPowerValueText).text = $"{NumberFormatter.FormatNumber((BigInteger)CombatPowerCalculator.Get())}";
    }

    private void OnClickProfileButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.UI.ShowPopupUI<UIProfileChangePopup>().SetInfo();
    }

    private void OnClickUserNameButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.UI.ShowPopupUI<UIChangeNamePopup>();
    }

    private void OnClickSlotWeapon()
    {
        if (weapon == null)
            return;
        
        GearLevelData selectedGearLevelData = Managers.Data.GearLevelDataDic[weapon.dataId];
        GetText((int)Texts.WeaponEquipValueText).text = $"공격력 +{NumberFormatter.FormatNumber((BigInteger)(selectedGearLevelData.baseValue * (1 + 0.14f * (weapon.level - 1))))}%";
        StartCoroutine(CoShowEquipPopupUI(GetObject((int)GameObjects.WeaponEquipEffect)));
    }

    private void OnClickSlotHat()
    {
        if (hat == null)
            return;
        
        GearLevelData selectedGearLevelData = Managers.Data.GearLevelDataDic[hat.dataId];
        GetText((int)Texts.HatEquipValueText).text = $"체력 회복 +{NumberFormatter.FormatNumber((BigInteger)(selectedGearLevelData.baseValue * (1 + 0.14f * (hat.level - 1))))}%";
        StartCoroutine(CoShowEquipPopupUI(GetObject((int)GameObjects.HatEquipEffect)));
    }

    private void OnClickSlotArmor()
    {
        if (armor == null)
            return;
        
        GearLevelData selectedGearLevelData = Managers.Data.GearLevelDataDic[armor.dataId];
        GetText((int)Texts.ArmorEquipValueText).text = $"체력 +{NumberFormatter.FormatNumber((BigInteger)(selectedGearLevelData.baseValue * (1 + 0.14f * (armor.level - 1))))}%";
        StartCoroutine(CoShowEquipPopupUI(GetObject((int)GameObjects.ArmorEquipEffect)));
    }

    private void OnClickSlotGloves()
    {
        if (gloves == null)
            return;
        
        GearLevelData selectedGearLevelData = Managers.Data.GearLevelDataDic[gloves.dataId];
        GetText((int)Texts.GlovesEquipValueText).text = $"크리티컬 데미지 +{NumberFormatter.FormatNumber((BigInteger)(selectedGearLevelData.baseValue * (1 + 0.14f * (gloves.level - 1))))}%";
        StartCoroutine(CoShowEquipPopupUI(GetObject((int)GameObjects.GlovesEquipEffect)));
    }

    private void OnClickSlotShoes()
    {
        if (shoes == null)
            return;
        
        GearLevelData selectedGearLevelData = Managers.Data.GearLevelDataDic[shoes.dataId];
        GetText((int)Texts.ShoesEquipValueText).text = $"공격 속도 +{NumberFormatter.FormatNumber((BigInteger)(selectedGearLevelData.baseValue * (1 + 0.14f * (shoes.level - 1))))}%";
        StartCoroutine(CoShowEquipPopupUI(GetObject((int)GameObjects.ShoesEquipEffect)));
    }

    private IEnumerator CoShowEquipPopupUI(GameObject go)
    {
        go.SetActive(true);
        go.GetComponent<Canvas>().sortingOrder = sortOrder;
        sortOrder++;
        yield return new WaitForSecondsRealtime(2f);
        go.SetActive(false);
    }
    
    private void OnClickExitButton()
    {
        Managers.Sound.PlayButtonClick();
        
        //닫기 애니메이션 후 ClosePopupUI
        PopupCloseAnimation(GetObject((int)GameObjects.ContentObject), () =>
        {
            StopAllCoroutines();
            Managers.UI.ClosePopupUI(this);
        });
    }

    private void NameChangedEventHandler(NameChangedEvent evnt)
    {
        RefreshUI();
    }

    private void ProfileChangedEventHandler(ProfileChangedEvent evnt)
    {
        RefreshUI();
    }
    
    private void ProfileRedDotChangedEventHandler(ProfileRedDotChangedEvent evnt)
    {
        GetObject((int)GameObjects.ProfileRedDotObject).SetActive(Managers.Profile.HasNewlyUnlocked(ProfileType.Profile) || Managers.Profile.HasNewlyUnlocked(ProfileType.Frame));
    }
}
