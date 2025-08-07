using System;
using System.Numerics;
using Assets.HeroEditor.Common.Scripts.Common;
using Data;
using UnityEngine;
using static Define;

public class UIStatUpgradeItem : UIBase
{
    #region UI 기능 리스트
    //StatImage: 스탯 이미지
    //StatLevelText: 스탯 레벨 텍스트
    //StatNameText: 스탯 이름 텍스트
    //StatValueText: 스탯 능력치 텍스트
    //UpgradeButton: 업그레이드 버튼
    //UpgradeCostGoldImage: 골드 아이콘 이미지
    //UpgradeCostGoldText: 스탯 강화 시 필요한 골드 텍스트
    //LockObject: 잠금 오브젝트
    //LockObjectText: 잠금 텍스트 (해금 조건 텍스트)
    #endregion

    #region Enum
    enum GameObjects
    {
        LockObject,
        UpgradeMAXButton,
    }
    enum Buttons
    {
        UpgradeButton,
    }
    
    enum Texts
    {
        StatLevelText,
        StatNameText,
        StatValueText,
        UpgradeCostGoldText,
        LockObjectText,
    }

    enum Images
    {
        StatImage,
        UpgradeCostGoldImage,
    }
    #endregion

    private StatUpgradeData statUpgradeData;

    private void OnEnable()
    {
        EventBus.Subscribe<GoldChangeEvent>(GoldChangedEventHandler);
        EventBus.Subscribe<CombatPowerChangedEvent>(CombatPowerChangedEventHandler);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<GoldChangeEvent>(GoldChangedEventHandler);
        EventBus.UnSubscribe<CombatPowerChangedEvent>(CombatPowerChangedEventHandler);
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        
        GetButton((int)Buttons.UpgradeButton).gameObject.BindEvent(OnClickUpgradeButton);
        GetButton((int)Buttons.UpgradeButton).gameObject.BindEvent(OnClickUpgradeButton, type: Define.UIEvent.Pressed);

        return true;
    }

    public void SetInfo(StatUpgradeData data)
    {
        //업그레이드할 수 있는 스탯듣 정보 + 현재 플레이어가 강화한 수치? 정보 가져오기
        statUpgradeData = data;
        
        //AtkButton
        if (data.type == StatType.AttackPower)
        {
            Managers.Tutorial.AddUIButton(UIButtonType.UpgradeAttackPower, GetButton((int)Buttons.UpgradeButton).gameObject );
        }
        else if (data.type == StatType.MaxHp)
        {
            Managers.Tutorial.AddUIButton((UIButtonType.UpgradeMaxHp), GetButton((int)Buttons.UpgradeButton).gameObject);
        }
        
        RefreshUI();
    }

    private void RefreshUI()
    {
        //스탯 아이콘 이미지 설정
        Sprite spr = Managers.Resource.Load<Sprite>($"{statUpgradeData.type.ToString()}Icon.sprite");
        GetImage((int)Images.StatImage).sprite = spr;
        
        //골드 아이콘 이미지 설정
        Sprite gspr = Managers.Resource.Load<Sprite>(GOLD_SPRITE_NAME);
        GetImage((int)Images.UpgradeCostGoldImage).sprite = gspr;
        
        //스탯 이름, 능력치, 골드 설정
        GetText((int)Texts.StatLevelText).text = $"Lv {Managers.StatUpgrade.GetLevel(statUpgradeData.type)}";
        GetText((int)Texts.StatNameText).text = $"{statUpgradeData.name}";
        if (statUpgradeData.type == StatType.CriticalChance)
        {
            GetText((int)Texts.StatValueText).text = $"{Util.FormatTo2DecimalsNoRounding(Managers.Player.PlayerStat.GetFloatValue(statUpgradeData.type) * 100)}%";
        }
        else if (statUpgradeData.type is StatType.AttackPower or StatType.MaxHp or StatType.HpRecovery)
        {
            GetText((int)Texts.StatValueText).text = $"{NumberFormatter.FormatNumber(Managers.Player.PlayerStat.GetBigIntValue(statUpgradeData.type))}";
        }
        else if (statUpgradeData.type is StatType.CriticalDamage)
        {
            GetText((int)Texts.StatValueText).text = $"{NumberFormatter.FormatNumber(Managers.Player.PlayerStat.GetFloatValue(statUpgradeData.type))}%";
        }
        else 
        {
            GetText((int)Texts.StatValueText).text = $"{NumberFormatter.FormatNumber(Managers.Player.PlayerStat.GetFloatValue(statUpgradeData.type))}";
        }
        GetText((int)Texts.UpgradeCostGoldText).text = $"{NumberFormatter.FormatNumber(Managers.StatUpgrade.GetUpgradeCost(statUpgradeData.type))}";
        
        //잠금되었을 경우
        bool isUnlocked = Managers.StatUpgrade.IsUnlocked(statUpgradeData.type);
        GetObject((int)GameObjects.LockObject).SetActive(!isUnlocked);
        if (!isUnlocked)
        {
            var data = Managers.Data.StatUpgradeDataDic[statUpgradeData.type];
            GetText((int)Texts.LockObjectText).text = $"{Managers.Data.StatUpgradeDataDic[data.unlockStatType].name} LV {data.unlockValue} 달성 시 해제";
        }
        GetButton((int)Buttons.UpgradeButton).interactable = Managers.StatUpgrade.CanUpgrade(statUpgradeData.type);
        
        //최대 업그레이드 횟수 도달했을 경우
        bool isMax = Managers.StatUpgrade.IsMax(statUpgradeData.type);
        GetButton((int)Buttons.UpgradeButton).gameObject.SetActive(!isMax);
        GetObject((int)GameObjects.UpgradeMAXButton).SetActive(isMax);
        if (isMax)
            GetText((int)Texts.StatLevelText).text = "MAX";
    }

    private void OnClickUpgradeButton()
    {
        Managers.Sound.PlayUpgradeButtonClick();
        
        //스탯 업그레이드
        Managers.StatUpgrade.UpgradeStat(statUpgradeData.type);
        RefreshUI();
    }

    private void CombatPowerChangedEventHandler(CombatPowerChangedEvent evnt)
    {
        RefreshUI();
    }

    private void GoldChangedEventHandler(GoldChangeEvent evnt)
    {
        RefreshUI();
    }
}
