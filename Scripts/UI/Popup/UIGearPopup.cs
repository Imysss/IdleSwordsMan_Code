using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEditor.Analytics;
using UnityEngine;
using static Define;

public class UIGearPopup : UIPopup
{
    #region UI 기능 리스트
    //WeaponToggle
    //HatToggle
    //ArmorToggle
    //GlovesToggle
    //ShoesToggle
    
    //UpgradeAllButton
    //GearPassiveEffectText
    //GearPassiveEffectValueText
    
    //GearScrollContentObject
    #endregion

    #region Enum
    enum GameObjects
    {
        RedDotObject,
        GearScrollContentObject,
    }

    enum Buttons
    {
        UpgradeAllButton,
    }

    enum Texts
    {
        GearPassiveEffectText,
        GearPassiveEffectValueText,
    }

    enum Toggles
    {
        WeaponToggle,
        HatToggle,
        ArmorToggle,
        GlovesToggle,
        ShoesToggle,
    }
    #endregion

    private GearType type;

    private void OnEnable()
    {
        EventBus.Subscribe<GearChangedEvent>(GearChangedEventHandler);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<GearChangedEvent>(GearChangedEventHandler);
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        //BindText(typeof(Texts));
        BindToggle(typeof(Toggles));

        GetToggle((int)Toggles.WeaponToggle).gameObject.BindEvent(OnClickWeaponToggle);
        GetToggle((int)Toggles.HatToggle).gameObject.BindEvent(OnClickHatToggle);
        GetToggle((int)Toggles.ArmorToggle).gameObject.BindEvent(OnClickArmorToggle);
        GetToggle((int)Toggles.GlovesToggle).gameObject.BindEvent(OnClickGlovesToggle);
        GetToggle((int)Toggles.ShoesToggle).gameObject.BindEvent(OnClickShoesToggle);
        
        GetButton((int)Buttons.UpgradeAllButton).gameObject.BindEvent(OnClickUpgradeAllButton);
        
        return true;
    }

    public void SetInfo()
    {
        GetToggle((int)Toggles.WeaponToggle).isOn = true;
        OnClickWeaponToggle();
    }

    private void RefreshUI(GearType type)
    {
        GameObject container = GetObject((int)GameObjects.GearScrollContentObject);
        container.DestroyChilds();
        List<ItemState> currentBestItem = Managers.Inventory.FindBestEquippableItem(ItemType.Gear, 1, type); 
        foreach (var data  in Managers.Inventory.GetGearStatesByType(type))
        {
            UIGearItem gearItem = Managers.UI.MakeSubItem<UIGearItem>(container.transform);
            gearItem.SetInfo(data, currentBestItem.Contains(data));
        }

        bool canAllUpgrade = false;
        foreach (var gear in Managers.Inventory.GearStates.Values)
        {
            if (gear.canUpgrade)
            {
                canAllUpgrade = true;
            }
        }
        GetButton((int)Buttons.UpgradeAllButton).interactable = canAllUpgrade;
        GetObject((int)GameObjects.RedDotObject).SetActive(canAllUpgrade);
    }

    private void OnClickWeaponToggle()
    {
        type = GearType.Weapon;
        RefreshUI(GearType.Weapon);
    }

    private void OnClickHatToggle()
    {
        type = GearType.Hat;
        RefreshUI(GearType.Hat);
    }

    private void OnClickArmorToggle()
    {
        type = GearType.Armor;
        RefreshUI(GearType.Armor);
    }

    private void OnClickGlovesToggle()
    {
        type = GearType.Gloves;
        RefreshUI(GearType.Gloves);
    }

    private void OnClickShoesToggle()
    {
        type = GearType.Shoes;
        RefreshUI(GearType.Shoes);   
    }

    private void OnClickUpgradeAllButton()
    {
        Managers.Sound.PlayButtonClick();
        
        //업그레이드 전 아이템 레벨 기억
        Dictionary<int, int> beforeLevels = Managers.Inventory.GearStates.ToDictionary(pair => pair.Key, pair => pair.Value.level);
        
        //모두 업그레이드
        while (Managers.Inventory.GearStates.Values.Any(g => g.canUpgrade))
        {
            foreach (var gear in Managers.Inventory.GearStates.Values)
            {
                if (gear.canUpgrade)
                {
                    Managers.Inventory.UpgradeItem(gear.dataId);
                }
            }
        }
        
        //레벨이 달라진 경우에만 결과값 넣기
        List<UpgradeResult> upgradeResults = new();
        foreach (var kvp in Managers.Inventory.GearStates)
        {
            int itemId = kvp.Key;
            int oldLevel = beforeLevels[itemId];
            int newLevel = kvp.Value.level;

            if (newLevel > oldLevel)
            {
                upgradeResults.Add(new UpgradeResult(itemId, ItemType.Gear, oldLevel, newLevel));
            }
        }
        
        Managers.UI.ShowPopupUI<UIUpgradeResultPopup>().SetInfo(upgradeResults);
        
        RefreshUI(type);
    }

    private void GearChangedEventHandler(GearChangedEvent evnt)
    {
        RefreshUI(type);
    }
}
